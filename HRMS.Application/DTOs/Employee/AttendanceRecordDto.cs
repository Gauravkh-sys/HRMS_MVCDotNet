using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs.Employee;

public class AttendanceRecordDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string Status { get; set; } = string.Empty;

    // New fields for hours tracking
    public string TotalHours { get; set; } = "-";
    public string ProductiveHours { get; set; } = "-";
}
