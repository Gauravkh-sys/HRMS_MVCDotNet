using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs.Payroll;

public class GeneratePaySlipDto
{
    public Guid EmployeeId { get; set; }
    public string MonthYear { get; set; } = string.Empty; // e.g., "November 2026"
    public decimal BasicSalary { get; set; }
    public decimal Hra { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
}