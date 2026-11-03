using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interface;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Web.Controllers;

[Authorize]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HRMSDbContext _context;

    public PayrollController(IPayrollService payrollService, UserManager<ApplicationUser> userManager, HRMSDbContext context)
    {
        _payrollService = payrollService;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        bool isAdminOrHr = await _userManager.IsInRoleAsync(currentUser, "Admin") || await _userManager.IsInRoleAsync(currentUser, "HRManager");

        if (isAdminOrHr)
        {
            var allPayslips = await _context.Set<PaySlip>().Include(p => p.Employee).OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToListAsync();
            return View("AdminIndex", allPayslips);
        }
        else
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.Value == currentUser.Email);
            var myPayslips = employee != null
                ? await _context.Set<PaySlip>().Where(p => p.EmployeeId == employee.Id).OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToListAsync()
                : new List<PaySlip>();

            ViewBag.EmployeeId = employee?.Id;
            return View("EmployeeIndex", myPayslips);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HRManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateBulk(int month, int year)
    {
        await _payrollService.GenerateBulkPayslipsAsync(month, year);
        TempData["SuccessMessage"] = $"Bulk payslips for {month}/{year} generated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> DownloadExcel(Guid id)
    {
        // Security check: Ensure regular employees can only download their own payslips
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var payslip = await _context.Set<PaySlip>().Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == id);
        if (payslip == null) return NotFound();

        bool isAdminOrHr = await _userManager.IsInRoleAsync(currentUser, "Admin") || await _userManager.IsInRoleAsync(currentUser, "HRManager");
        if (!isAdminOrHr && payslip.Employee.Email.Value != currentUser.Email)
        {
            TempData["ErrorMessage"] = "Access Denied: You can only download your own payslips.";
            return RedirectToAction(nameof(Index));
        }

        var bytes = await _payrollService.GenerateExcelPayslipAsync(id);
        string fileName = $"Payslip_{payslip.Employee.FirstName}_{payslip.Month}_{payslip.Year}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadYearlyZip(Guid employeeId, int year)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        bool isAdminOrHr = await _userManager.IsInRoleAsync(currentUser, "Admin") || await _userManager.IsInRoleAsync(currentUser, "HRManager");

        // If user is an Employee, force employeeId to match their own profile (prevent ID tampering)
        if (!isAdminOrHr)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.Value == currentUser.Email);
            if (employee == null) return NotFound();
            employeeId = employee.Id; // Override parameter for security
        }

        var zipBytes = await _payrollService.GenerateYearlyZipArchiveAsync(employeeId, year);
        return File(zipBytes, "application/zip", $"Payslips_{year}_Archive.zip");
    }


    // GET: /Payroll/Details/5 (View payslip online)
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var payslip = await _context.Set<PaySlip>().Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == id);
        if (payslip == null) return NotFound();

        bool isAdminOrHr = await _userManager.IsInRoleAsync(currentUser, "Admin") || await _userManager.IsInRoleAsync(currentUser, "HRManager");
        if (!isAdminOrHr && payslip.Employee.Email.Value != currentUser.Email)
        {
            TempData["ErrorMessage"] = "Access Denied: You can only view your own payslips.";
            return RedirectToAction(nameof(Index));
        }

        return View(payslip); // Renders a clean online view
    }


    // GET: /Payroll/DownloadPdf/5
    [HttpGet]
    public async Task<IActionResult> DownloadPdf(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var payslip = await _context.Set<PaySlip>().Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == id);
        if (payslip == null) return NotFound();

        bool isAdminOrHr = await _userManager.IsInRoleAsync(currentUser, "Admin") || await _userManager.IsInRoleAsync(currentUser, "HRManager");
        if (!isAdminOrHr && payslip.Employee.Email.Value != currentUser.Email)
        {
            TempData["ErrorMessage"] = "Access Denied.";
            return RedirectToAction(nameof(Index));
        }

        var pdfBytes = await _payrollService.GeneratePdfPayslipAsync(id);
        string fileName = $"Payslip_{payslip.Employee.FirstName}_{payslip.Month}_{payslip.Year}.html"; // or .pdf if using a native PDF renderer

        // Using text/html or application/pdf depending on format
        return File(pdfBytes, "text/html", fileName);
    }


    // GET: /Payroll/Print/5
    [HttpGet]
    public async Task<IActionResult> Print(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var payslip = await _context.Set<PaySlip>().Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == id);
        if (payslip == null) return NotFound();

        bool isAdminOrHr = await _userManager.IsInRoleAsync(currentUser, "Admin") || await _userManager.IsInRoleAsync(currentUser, "HRManager");
        if (!isAdminOrHr && payslip.Employee.Email.Value != currentUser.Email)
        {
            TempData["ErrorMessage"] = "Access Denied.";
            return RedirectToAction(nameof(Index));
        }

        return View("Print", payslip);
    }
}