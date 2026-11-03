using FluentAssertions;
using HRMS.Application.DTOs;
using HRMS.Application.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Repositories;
using HRMS.Domain.ValueObjects;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace HRMS.UnitTests.Application;

[TestClass]
public class EmployeeServiceTests
{
    private readonly IEmployeeRepository _employeeRepositoryMock;
    private readonly HRMSDbContext _dbContext;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        // 1. Create NSubstitute mock for repository
        _employeeRepositoryMock = Substitute.For<IEmployeeRepository>();

        // 2. Set up InMemory DbContext for EF Core operations
        var options = new DbContextOptionsBuilder<HRMSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new HRMSDbContext(options);

        // 3. Initialize service with substitutes/context
        _employeeService = new EmployeeService(_employeeRepositoryMock, _dbContext);
    }

    [TestMethod]
    public async Task GetEmployeeByIdAsync_WhenEmployeeExists_ShouldReturnEmployeeDto()
    {
        // Given
        var employeeId = Guid.NewGuid();
        var email = Email.Create("alex.turner@company.com").Value!;
        var department = new Department(1, "Engineering", "ENG");

        var employee = new Employee("Alex", "Turner", email, "Senior Developer",1, DateTime.Today)
        {
            Department = department
        };

        _employeeRepositoryMock.GetByIdAsync(employeeId).Returns(Task.FromResult<Employee?>(employee));

        // When
        var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

        // Then
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Alex");
        result.Email.Should().Be("alex.turner@company.com");
        result.DepartmentName.Should().Be("Engineering");

        await _employeeRepositoryMock.Received(1).GetByIdAsync(employeeId);
    }

    [TestMethod]
    public async Task GetEmployeeByIdAsync_WhenEmployeeDoesNotExist_ShouldReturnNull()
    {
        // Given
        var employeeId = Guid.NewGuid();
        _employeeRepositoryMock.GetByIdAsync(employeeId).Returns(Task.FromResult<Employee?>((Employee?)null));

        // When
        var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

        // Then
        result.Should().BeNull();
    }
}