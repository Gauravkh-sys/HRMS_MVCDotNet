using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interface;
using HRMS.Domain.Entities;
using HRMS.Domain.ValueObjects;
using HRMS.Domain.Repositories;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HRMS.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly HRMSDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        HRMSDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _employeeRepository = employeeRepository;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email.Value,
            Designation = e.Designation,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name ?? string.Empty,
            JoiningDate = e.JoiningDate,
            IsActive = e.IsActive
        });
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id)
    {
        var e = await _employeeRepository.GetByIdAsync(id);
        if (e == null) return null;

        return new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email.Value,
            Designation = e.Designation,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name ?? string.Empty,
            JoiningDate = e.JoiningDate,
            IsActive = e.IsActive
        };
    }

    public async Task<bool> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        var emailResult = Email.Create(dto.Email);
        if (emailResult.IsFailure) return false;

        var employee = new Employee(
            dto.FirstName,
            dto.LastName,
            emailResult.Value!,
            dto.Designation,
            dto.DepartmentId,
            dto.JoiningDate
        );

        await _employeeRepository.AddAsync(employee);
        await _context.SaveChangesAsync(); // Commit employee to SQL Server

        // Automatically provision AspNetUsers login entry
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser == null)
        {
            var appUser = new ApplicationUser
            {
                UserName = dto.Email,
                NormalizedUserName = dto.Email.ToUpper(),
                Email = dto.Email,
                NormalizedEmail = dto.Email.ToUpper(),
                FullName = $"{dto.FirstName} {dto.LastName}",
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(appUser, "Password@12");
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(appUser, "Employee");
            }
        }

        return true;
    }
}