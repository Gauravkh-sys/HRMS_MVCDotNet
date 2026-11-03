using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interface;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly HRMSDbContext _context;

    public AttendanceService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AttendanceRecordDto>> GetAllAttendanceRecordsAsync()
    {
        var records = await _context.AttendanceRecords
            .Include(a => a.Employee)
            .ToListAsync();

        return records.Select(a => new AttendanceRecordDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
            Date = a.Date,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            Status = a.Status,
            TotalHours = a.TotalHoursWorked.HasValue ? $"{a.TotalHoursWorked.Value.Hours} hrs {a.TotalHoursWorked.Value.Minutes} mins" : "-",
            ProductiveHours = a.TotalHoursWorked.HasValue ? $"{a.TotalHoursWorked.Value.Hours} hrs {a.TotalHoursWorked.Value.Minutes} mins" : "-" // Set equal to total hours for now
        });
    }

    public async Task<bool> CheckInAsync(CheckInDto dto)
    {
        var today = DateTime.Today;
        // Check if already checked in today
        var existing = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.Date == today);

        if (existing != null) return false; // Already checked in today

        var record = new AttendanceRecord(
            dto.EmployeeId,
            today,
            DateTime.Now.TimeOfDay,
            dto.Status
        );

        await _context.AttendanceRecords.AddAsync(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CheckOutAsync(Guid employeeId)
    {
        var today = DateTime.Today;
        var record = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

        if (record == null || record.CheckOutTime != null) return false;

        record.RecordCheckOut(DateTime.Now.TimeOfDay);
        await _context.SaveChangesAsync();
        return true;
    }
}