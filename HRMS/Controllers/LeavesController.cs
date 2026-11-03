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
public class LeavesController : Controller
{
    private readonly HRMSDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeavesController(HRMSDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<string> GetActiveRoleAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count <= 1) return roles.FirstOrDefault() ?? "Employee";

        string activeRoleCookie = HttpContext.Request.Cookies["ActiveHRMSRole"] ?? "";
        if (!string.IsNullOrEmpty(activeRoleCookie) && roles.Contains(activeRoleCookie))
        {
            return activeRoleCookie;
        }

        if (roles.Contains("Admin")) return "Admin";
        if (roles.Contains("HRManager")) return "HRManager";
        return "Employee";
    }

    // GET: /Leaves
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var activeRole = await GetActiveRoleAsync(currentUser);

        // Fetch leave requests including the related Employee navigation property
        var query = _context.LeaveRequests.Include(l => l.Employee).AsQueryable();

        // If active persona is Employee, filter strictly to their own requests
        if (activeRole == "Employee")
        {
            query = query.Where(l => l.Employee.Email.Value == currentUser.Email);
        }
        else if (activeRole == "HRManager")
        {
            ViewBag.TotalHeadcount = await _context.Employees.CountAsync(e => e.IsActive);
        }

        var leaves = await query.ToListAsync();
        ViewBag.ActivePersona = activeRole;
        return View(leaves);
    }

    // GET: /Leaves/Apply
    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var activeRole = await GetActiveRoleAsync(currentUser);

        if (activeRole == "Employee")
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.Value == currentUser.Email);
            if (employee != null)
            {
                ViewBag.EmployeeName = employee.FirstName + " " + employee.LastName;
                ViewBag.EmployeeId = employee.Id;
                ViewBag.IsEmployee = true;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Employee profile not found for the current user account.");
                ViewBag.IsEmployee = false;
            }
        }
        else
        {
            var employees = await _context.Employees
                .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
                .ToListAsync();

            ViewBag.Employees = new SelectList(employees, "Id", "FullName");
            ViewBag.IsEmployee = false;
        }

        return View();
    }

    // POST: /Leaves/Apply
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(Guid employeeId, string leaveType, DateTime startDate, DateTime endDate, string reason)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var activeRole = currentUser != null ? await GetActiveRoleAsync(currentUser) : "Employee";

        if (activeRole == "Employee" && currentUser != null)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.Value == currentUser.Email);
            if (employee != null)
            {
                employeeId = employee.Id; // Enforce logged in user's ID
            }
        }

        if (ModelState.IsValid)
        {
            var leaveRequest = new LeaveRequest(employeeId, leaveType, startDate, endDate, reason);
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            // Set success toast message
            TempData["SuccessMessage"] = "Leave request submitted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Repopulate view data on validation error
        if (activeRole == "Employee" && currentUser != null)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.Value == currentUser.Email);
            if (employee != null)
            {
                ViewBag.EmployeeName = employee.FirstName + " " + employee.LastName;
                ViewBag.EmployeeId = employee.Id;
            }
            ViewBag.IsEmployee = true;
        }
        else
        {
            var employees = await _context.Employees
                .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
                .ToListAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "FullName", employeeId);
            ViewBag.IsEmployee = false;
        }

        TempData["ErrorMessage"] = "Please fix the validation errors and submit again.";
        return View();
    }

    // POST: /Leaves/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var leaveRequest = await _context.LeaveRequests.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
        if (leaveRequest == null) return NotFound();

        // 🛑 Segregation of Duties: Requester cannot approve their own leave
        if (leaveRequest.Employee.Email.Value == currentUser.Email)
        {
            TempData["ErrorMessage"] = "Self-Approval Violation: You cannot approve your own leave request.";
            return RedirectToAction(nameof(Index));
        }

        leaveRequest.Approve();
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Leave request approved successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Leaves/Reject/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var leaveRequest = await _context.LeaveRequests.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
        if (leaveRequest == null) return NotFound();

        if (leaveRequest.Employee.Email.Value == currentUser.Email)
        {
            TempData["ErrorMessage"] = "Action Denied: You cannot reject your own leave request.";
            return RedirectToAction(nameof(Index));
        }

        leaveRequest.Reject();
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Leave request rejected.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Leaves/Revoke/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var leaveRequest = await _context.LeaveRequests.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
        if (leaveRequest == null) return NotFound();

        leaveRequest.Revoke();
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Leave request cancelled/revoked.";
        return RedirectToAction(nameof(Index));
    }
}