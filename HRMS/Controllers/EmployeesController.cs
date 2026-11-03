using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interface;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Web.Controllers;

public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly HRMSDbContext _context;
    private readonly IExcelReportService _excelReportService; // inject in constructor

    public EmployeesController(IEmployeeService employeeService, HRMSDbContext context, IExcelReportService excelReportService)
    {
        _employeeService = employeeService;
        _context = context;
        _excelReportService = excelReportService;
    }

    // GET: /Employees
    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return View(employees);
    }




    // GET: /Employees/Create
    public async Task<IActionResult> Create()
    {
        var departments = await _context.Departments
            .Select(d => new { d.Id, d.Name })
            .ToListAsync();

        ViewBag.Departments = new SelectList(departments, "Id", "Name");
        return View();
    }

    // POST: /Employees/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        if (ModelState.IsValid)
        {
            var success = await _employeeService.CreateEmployeeAsync(dto);
            if (success)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, "Invalid email or failed to create employee.");
        }

        var departments = await _context.Departments
            .Select(d => new { d.Id, d.Name })
            .ToListAsync();

        ViewBag.Departments = new SelectList(departments, "Id", "Name", dto.DepartmentId);
        return View(dto);
    }

    [Authorize(Roles = "HRManager,Admin")]
    // GET: /Employees/ExportExcel
    public async Task<IActionResult> ExportExcel()
    {
        var stream = await _excelReportService.ExportEmployeesToExcelAsync();
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Employees_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
