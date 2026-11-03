using HRMS.Application.DTOs.Employee;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.Interface;

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceRecordDto>> GetAllAttendanceRecordsAsync();
    Task<bool> CheckInAsync(CheckInDto dto);
    Task<bool> CheckOutAsync(Guid employeeId);
}
