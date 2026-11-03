using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Entities;

public class AttendanceRecord : Entity<Guid>
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public DateTime Date { get; private set; }
    public TimeSpan? CheckInTime { get; private set; }
    public TimeSpan? CheckOutTime { get; private set; }
    public string Status { get; set; } // Present, Absent, HalfDay, Late

    // Computed property for total/productive hours
    public TimeSpan? TotalHoursWorked
    {
        get
        {
            if (CheckInTime.HasValue && CheckOutTime.HasValue)
            {
                return CheckOutTime.Value - CheckInTime.Value;
            }
            return null;
        }
    }

    private AttendanceRecord() { } // EF Core

    public AttendanceRecord(Guid employeeId, DateTime date, TimeSpan checkInTime, string status)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        Date = date.Date;
        CheckInTime = checkInTime;
        Status = status;
    }

    public void RecordCheckOut(TimeSpan checkOutTime)
    {
        CheckOutTime = checkOutTime;
    }
}
