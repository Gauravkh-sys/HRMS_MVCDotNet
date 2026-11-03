using ClosedXML.Excel;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interface;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace HRMS.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly HRMSDbContext _context;

    public PayrollService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task GenerateBulkPayslipsAsync(int month, int year, decimal defaultBasic, decimal defaultAllowances, decimal defaultDeductions)
    {
        var activeEmployees = await _context.Employees.Where(e => e.IsActive).ToListAsync();

        foreach (var emp in activeEmployees)
        {
            // Check if payslip already exists for this month/year
            bool exists = await _context.Set<PaySlip>().AnyAsync(p => p.EmployeeId == emp.Id && p.Month == month && p.Year == year);
            if (!exists)
            {
                var payslip = new PaySlip(emp.Id, month, year, defaultBasic, defaultAllowances, defaultDeductions);
                _context.Set<PaySlip>().Add(payslip);
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<byte[]> GenerateExcelPayslipAsync(Guid payslipId)
    {
        var payslip = await _context.Set<PaySlip>().Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == payslipId);
        if (payslip == null) throw new FileNotFoundException("Payslip not found");

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Salary Slip");

        // Company Header
        ws.Cell("B2").Value = "ENTERPRISE HRMS";
        ws.Cell("B2").Style.Font.Bold = true;
        ws.Cell("B2").Style.Font.FontSize = 14;
        ws.Cell("B3").Value = "Gorakhpur, India";

        ws.Cell("F2").Value = "Payslip For The Month";
        ws.Cell("F3").Value = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(payslip.Month)} {payslip.Year}";
        ws.Cell("F3").Style.Font.Bold = true;

        // Employee Summary
        ws.Cell("B5").Value = "Employee Name:"; ws.Cell("C5").Value = payslip.Employee != null ? $"{payslip.Employee.FirstName} {payslip.Employee.LastName}" : "N/A";
        ws.Cell("B6").Value = "Designation:"; ws.Cell("C6").Value = payslip.Employee?.Designation ?? "N/A";
        ws.Cell("B7").Value = "Pay Period:"; ws.Cell("C7").Value = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(payslip.Month)} {payslip.Year}";

        // Table Headers
        ws.Cell("B9").Value = "EARNINGS"; ws.Cell("C9").Value = "AMOUNT";
        ws.Cell("E9").Value = "DEDUCTIONS"; ws.Cell("F9").Value = "AMOUNT";
        ws.Range("B9:C9").Style.Fill.BackgroundColor = XLColor.DarkGray;
        ws.Range("E9:F9").Style.Fill.BackgroundColor = XLColor.DarkGray;

        // Table Rows
        ws.Cell("B10").Value = "Basic Salary"; ws.Cell("C10").Value = payslip.BasicSalary;
        ws.Cell("E10").Value = "Deductions"; ws.Cell("F10").Value = payslip.Deductions;

        ws.Cell("B11").Value = "Allowances"; ws.Cell("C11").Value = payslip.Allowances;

        // Totals
        ws.Cell("B13").Value = "Gross Earnings"; ws.Cell("C13").Value = payslip.BasicSalary + payslip.Allowances;
        ws.Cell("E13").Value = "Total Deductions"; ws.Cell("F13").Value = payslip.Deductions;

        // Net Pay Banner
        ws.Cell("B15").Value = "TOTAL NET PAYABLE";
        ws.Cell("F15").Value = payslip.NetSalary;
        ws.Range("B15:F15").Style.Fill.BackgroundColor = XLColor.LightGreen;
        ws.Range("B15:F15").Style.Font.Bold = true;

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GeneratePdfPayslipAsync(Guid payslipId)
    {
        var payslip = await _context.Set<PaySlip>().Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == payslipId);
        if (payslip == null) throw new FileNotFoundException("Payslip not found");

        // Constructing a clean printable HTML/Text statement representation for PDF export
        string htmlContent = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; padding: 20px; color: #333; }}
                .header {{ text-align: center; border-bottom: 2px solid #ddd; padding-bottom: 10px; margin-bottom: 20px; }}
                table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                th, td {{ border: 1px solid #ddd; padding: 10px; text-align: left; }}
                th {{ background-color: #f4f4f4; }}
                .text-end {{ text-align: right; }}
                .total-box {{ background-color: #e8f5e9; padding: 15px; margin-top: 20px; font-size: 18px; font-weight: bold; text-align: right; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <h2>ENTERPRISE HRMS</h2>
                <p>Official Salary Statement - {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(payslip.Month)} {payslip.Year}</p>
            </div>
            <p><strong>Employee Name:</strong> {(payslip.Employee != null ? $"{payslip.Employee.FirstName} {payslip.Employee.LastName}" : "N/A")}</p>
            <p><strong>Designation:</strong> {payslip.Employee?.Designation ?? "N/A"}</p>
            <table>
                <tr>
                    <th>Earnings & Deductions</th>
                    <th class='text-end'>Amount (INR)</th>
                </tr>
                <tr>
                    <td>Basic Salary</td>
                    <td class='text-end'>₹{payslip.BasicSalary:N2}</td>
                </tr>
                <tr>
                    <td>Allowances</td>
                    <td class='text-end'>₹{payslip.Allowances:N2}</td>
                </tr>
                <tr>
                    <td>Deductions</td>
                    <td class='text-end'>₹{payslip.Deductions:N2}</td>
                </tr>
            </table>
            <div class='total-box'>
                Total Net Payable: ₹{payslip.NetSalary:N2}
            </div>
        </body>
        </html>";

        // Return UTF-8 bytes designated for download
        return System.Text.Encoding.UTF8.GetBytes(htmlContent);
    }

    public async Task<byte[]> GenerateYearlyZipArchiveAsync(Guid employeeId, int year)
    {
        var payslips = await _context.Set<PaySlip>()
            .Include(p => p.Employee)
            .Where(p => p.EmployeeId == employeeId && p.Year == year && p.Month <= DateTime.Now.Month)
            .ToListAsync();

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var slip in payslips)
            {
                var excelBytes = await GenerateExcelPayslipAsync(slip.Id);
                string fileName = $"{slip.Employee.FirstName}_{slip.Year}_Month_{slip.Month}_Payslip.xlsx";

                var zipEntry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                using var entryStream = zipEntry.Open();
                await entryStream.WriteAsync(excelBytes, 0, excelBytes.Length);
            }
        }
        return memoryStream.ToArray();
    }
}