using ClosedXML.Excel;
using HRMS.Application.Interface;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HRMS.Application.Services;

public class ExcelReportService : IExcelReportService
{
    private readonly HRMSDbContext _context;

    public ExcelReportService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<MemoryStream> ExportEmployeesToExcelAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Employees");
        worksheet.ShowGridLines = true;

        // Headers
        var headers = new[] { "ID", "First Name", "Last Name", "Email", "Position", "Department", "Joining Date" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#384166");
        headerRow.Style.Font.FontColor = XLColor.White;
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        int row = 2;
        foreach (var emp in employees)
        {
            worksheet.Cell(row, 1).Value = emp.Id.ToString();
            worksheet.Cell(row, 2).Value = emp.FirstName;
            worksheet.Cell(row, 3).Value = emp.LastName;
            worksheet.Cell(row, 4).Value = emp.Email.Value;
            worksheet.Cell(row, 5).Value = emp.Designation;
            worksheet.Cell(row, 6).Value = emp.Department?.Name ?? "-";

            // Store as a native DateTime with explicit Excel date formatting
            var dateCell = worksheet.Cell(row, 7);
            dateCell.Value = emp.JoiningDate;
            dateCell.Style.DateFormat.Format = "yyyy-MM-dd";
            dateCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Zebra striping (alternating row background colors)
            var currentRow = worksheet.Row(row);
            if (row % 2 == 0)
            {
                currentRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#F9FAFB");
            }

            // Subtle borders for a cleaner look
            currentRow.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            currentRow.Style.Border.BottomBorderColor = XLColor.FromHtml("#E5E7EB");

            row++;
        }

        // Auto-fit columns with safety padding
        worksheet.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    public async Task<MemoryStream> ExportAttendanceToExcelAsync()
    {
        var records = await _context.AttendanceRecords
            .Include(a => a.Employee)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance Summary");

        worksheet.Cell(1, 1).Value = "Employee Name";
        worksheet.Cell(1, 2).Value = "Date";
        worksheet.Cell(1, 3).Value = "Check-In";
        worksheet.Cell(1, 4).Value = "Check-Out";
        worksheet.Cell(1, 5).Value = "Status";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B735F");
        headerRow.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var rec in records)
        {
            worksheet.Cell(row, 1).Value = $"{rec.Employee.FirstName} {rec.Employee.LastName}";
            worksheet.Cell(row, 2).Value = rec.Date.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 3).Value = rec.CheckInTime?.ToString(@"hh\:mm\:ss") ?? "-";
            worksheet.Cell(row, 4).Value = rec.CheckOutTime?.ToString(@"hh\:mm\:ss") ?? "-";
            worksheet.Cell(row, 5).Value = rec.Status;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
