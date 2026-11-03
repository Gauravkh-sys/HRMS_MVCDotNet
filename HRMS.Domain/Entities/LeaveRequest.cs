using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Entities;

public enum LeaveStatus { Pending, Approved, Rejected, Revoked }

public class LeaveRequest : AggregateRoot<Guid>
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public string LeaveType { get; private set; } // e.g., Casual, Sick, Annual
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Reason { get; private set; }
    public LeaveStatus Status { get; private set; }

    private LeaveRequest() { } // EF Core

    public LeaveRequest(Guid employeeId, string leaveType, DateTime startDate, DateTime endDate, string reason)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        LeaveType = leaveType;
        StartDate = startDate;
        EndDate = endDate;
        Reason = reason;
        Status = LeaveStatus.Pending;
    }

    public void Approve() => Status = LeaveStatus.Approved;
    public void Reject() => Status = LeaveStatus.Rejected;
    public void Revoke() => Status = LeaveStatus.Revoked; // Added for revocation
}