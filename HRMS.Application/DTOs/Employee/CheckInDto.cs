using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs.Employee;

public class CheckInDto
{
    public Guid EmployeeId { get; set; }
    public string Status { get; set; } = "Present"; // Present, HalfDay, Late
}