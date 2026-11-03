using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Controllers;

public class DashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    public DashboardController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return RedirectToAction(nameof(AdminDashboard));
        }
        else if (await _userManager.IsInRoleAsync(user, "HRManager"))
        {
            return RedirectToAction(nameof(HrDashboard));
        }

        return RedirectToAction(nameof(EmployeeDashboard));
    }

    [Authorize(Roles = "Employee,HRManager,Admin")]
    public IActionResult EmployeeDashboard()
    {
        return View();
    }

    [Authorize(Roles = "HRManager,Admin")]
    public IActionResult HrDashboard()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminDashboard()
    {
        return View();
    }
}
