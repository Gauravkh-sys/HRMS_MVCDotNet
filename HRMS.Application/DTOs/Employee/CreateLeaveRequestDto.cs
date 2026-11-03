using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs.Employee;

public class CreateLeaveRequestDto
{
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty; // Casual, Sick, Annual
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);
    public string Reason { get; set; } = string.Empty;
}
