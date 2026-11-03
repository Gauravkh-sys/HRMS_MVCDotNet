using HRMS.Application.DTOs.Employee;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.Interface;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id);
    Task<bool> CreateEmployeeAsync(CreateEmployeeDto dto);
}