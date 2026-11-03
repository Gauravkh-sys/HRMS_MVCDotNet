using FluentAssertions;
using HRMS.Application.DTOs;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.ValueObjects;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace HRMS.UnitTests.Application;

[TestClass]
public class LeaveServiceTests
{
    private readonly HRMSDbContext _dbContext;
    private readonly LeaveService _leaveService;

    public LeaveServiceTests()
    {
        // Given an in-memory SQL database context for Leave management testing
        var options = new DbContextOptionsBuilder<HRMSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new HRMSDbContext(options);

        _leaveService = new LeaveService(_dbContext);
    }

    [TestMethod]
    public async Task CreateLeaveRequestAsync_WhenValidRequest_ShouldSaveAndReturnTrue()
    {
        // Given
        var employee = new Employee("Sarah", "Connor", Email.Create("sarah.connor@company.com").Value!, "Operations Manager", 2, DateTime.Today);
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        var leaveDto = new CreateLeaveRequestDto
        {
            EmployeeId = employee.Id,
            LeaveType = "Casual Leave",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(2),
            Reason = "Personal work"
        };

        // When
        var result = await _leaveService.CreateLeaveRequestAsync(leaveDto);

        // Then
        result.Should().BeTrue();
        var savedLeave = await _dbContext.LeaveRequests.FirstOrDefaultAsync(l => l.EmployeeId == employee.Id);
        savedLeave.Should().NotBeNull();
        savedLeave!.LeaveType.Should().Be("Casual Leave");
        savedLeave.Status.Should().Be(LeaveStatus.Pending);
    }

    [TestMethod]
    public async Task UpdateLeaveStatusAsync_WhenApproved_ShouldChangeStatusToApproved()
    {
        // Given
        var employee = new Employee("John", "Connor", Email.Create("john.connor@company.com").Value!, "Engineer", 1, DateTime.Today);
        _dbContext.Employees.Add(employee);

        var leaveRequest = new LeaveRequest(employee.Id, "Sick Leave", DateTime.Today, DateTime.Today.AddDays(1), "Fever");
        _dbContext.LeaveRequests.Add(leaveRequest);
        await _dbContext.SaveChangesAsync();

        // When
        var result = await _leaveService.UpdateLeaveStatusAsync(leaveRequest.Id, approve: true);

        // Then
        result.Should().BeTrue();
        var updatedLeave = await _dbContext.LeaveRequests.FindAsync(leaveRequest.Id);
        updatedLeave!.Status.Should().Be(LeaveStatus.Approved);
    }

    [TestMethod]
    public async Task RevokeLeaveAsync_WhenApprovedLeave_ShouldChangeStatusToRevoked()
    {
        // Given
        var employee = new Employee("Kyle", "Reese", Email.Create("kyle.reese@company.com").Value!, "Security", 1, DateTime.Today);
        _dbContext.Employees.Add(employee);

        var leaveRequest = new LeaveRequest(employee.Id, "Annual Leave", DateTime.Today, DateTime.Today.AddDays(5), "Vacation");
        leaveRequest.Approve(); // Already approved
        _dbContext.LeaveRequests.Add(leaveRequest);
        await _dbContext.SaveChangesAsync();

        // When
        var result = await _leaveService.RevokeLeaveAsync(leaveRequest.Id);

        // Then
        result.Should().BeTrue();
        var revokedLeave = await _dbContext.LeaveRequests.FindAsync(leaveRequest.Id);
        revokedLeave!.Status.Should().Be(LeaveStatus.Revoked);
    }
}