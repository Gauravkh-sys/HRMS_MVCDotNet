using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs.Payroll;

public class PaySlipDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string MonthYear { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal Hra { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetPay { get; set; }
    public DateTime GeneratedDate { get; set; }
}