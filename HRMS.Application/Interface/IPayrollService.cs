using HRMS.Application.DTOs.Payroll;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.Interface;

public interface IPayrollService
{
    Task GenerateBulkPayslipsAsync(int month, int year, decimal defaultBasic = 50000, decimal defaultAllowances = 10000, decimal defaultDeductions = 5000);
    Task<byte[]> GenerateExcelPayslipAsync(Guid payslipId);
    Task<byte[]> GeneratePdfPayslipAsync(Guid payslipId); // Or HTML-to-PDF stream
    Task<byte[]> GenerateYearlyZipArchiveAsync(Guid employeeId, int year);
}