using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Controllers;

public class RoleSelectionController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleSelectionController(UserManager<ApplicationUser> UserManager)
    {
        _userManager = UserManager;
    }

    [HttpGet]
    public async Task<IActionResult> ChooseRole(string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);

        // If the user only has 1 role, bypass the modal and go straight there
        if (roles.Count <= 1)
        {
            return RedirectToLocalRoleDashboard(roles.FirstOrDefault() ?? "Employee", returnUrl);
        }

        ViewBag.Roles = roles;
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetActiveRole(string selectedRole, string? returnUrl = null)
    {
        // Store the active persona in a temporary session or cookie
        HttpContext.Response.Cookies.Append("ActiveHRMSRole", selectedRole, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        return RedirectToLocalRoleDashboard(selectedRole, returnUrl);
    }

    private IActionResult RedirectToLocalRoleDashboard(string role, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return role switch
        {
            "Admin" => RedirectToAction("AdminDashboard", "Dashboard"),
            "HRManager" => RedirectToAction("HrDashboard", "Dashboard"),
            _ => RedirectToAction("EmployeeDashboard", "Dashboard")
        };
    }

    public IActionResult Index()
    {
        return View();
    }
}
