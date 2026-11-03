using HRMS.Application.DTOs.Employee;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.Interface;

public interface ILeaveService
{
    Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync();
    Task<bool> CreateLeaveRequestAsync(CreateLeaveRequestDto dto);
    Task<bool> UpdateLeaveStatusAsync(Guid id, bool approve);
    Task<bool> RevokeLeaveAsync(Guid id);
}