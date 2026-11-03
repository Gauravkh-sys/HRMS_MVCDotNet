using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interface;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Web.Controllers;

public class AttendanceController : Controller
{
    private readonly IAttendanceService _attendanceService;
    private readonly HRMSDbContext _context;

    public AttendanceController(IAttendanceService attendanceService, HRMSDbContext context)
    {
        _attendanceService = attendanceService;
        _context = context;
    }

    // GET: /Attendance
    public async Task<IActionResult> Index()
    {
        var records = await _attendanceService.GetAllAttendanceRecordsAsync();
        return View(records);
    }

    // GET: /Attendance/CheckIn
    public async Task<IActionResult> CheckIn()
    {
        var employees = await _context.Employees
            .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
            .ToListAsync();

        ViewBag.Employees = new SelectList(employees, "Id", "FullName");
        return View();
    }

    // POST: /Attendance/CheckIn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInDto dto)
    {
        if (ModelState.IsValid)
        {
            var success = await _attendanceService.CheckInAsync(dto);
            if (success) return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, "Employee has already checked in today.");
        }

        var employees = await _context.Employees
            .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
            .ToListAsync();

        ViewBag.Employees = new SelectList(employees, "Id", "FullName", dto.EmployeeId);
        return View(dto);
    }

    // POST: /Attendance/CheckOut/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(Guid employeeId)
    {
        await _attendanceService.CheckOutAsync(employeeId);
        return RedirectToAction(nameof(Index));
    }
}
