using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interface;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.Services;

public class LeaveService : ILeaveService
{
    private readonly HRMSDbContext _context;

    public LeaveService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync()
    {
        var leaves = await _context.LeaveRequests
            .Include(l => l.Employee)
            .ToListAsync();

        return leaves.Select(l => new LeaveRequestDto
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            EmployeeName = $"{l.Employee.FirstName} {l.Employee.LastName}",
            LeaveType = l.LeaveType,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Reason = l.Reason,
            Status = l.Status.ToString()
        });
    }

    public async Task<bool> CreateLeaveRequestAsync(CreateLeaveRequestDto dto)
    {
        var leave = new LeaveRequest(
            dto.EmployeeId,
            dto.LeaveType,
            dto.StartDate,
            dto.EndDate,
            dto.Reason
        );

        await _context.LeaveRequests.AddAsync(leave);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLeaveStatusAsync(Guid id, bool approve)
    {
        var leave = await _context.LeaveRequests.FindAsync(id);
        if (leave == null) return false;

        if (approve) leave.Approve();
        else leave.Reject();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeLeaveAsync(Guid id)
    {
        var leave = await _context.LeaveRequests.FindAsync(id);
        if (leave == null || leave.Status != LeaveStatus.Approved) return false;

        leave.Revoke();
        await _context.SaveChangesAsync();
        return true;
    }
}