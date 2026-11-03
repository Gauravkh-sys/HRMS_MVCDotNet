using HRMS.Infrastructure.Identity;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace HRMS.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager,ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            //return RedirectToAction("Index", "Home");
            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    [HttpPost]
    [ValidateAntiForgeryToken] // Optional for GET, but required for POST
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }


    // GET: /Account/ForgotPassword
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST: /Account/ForgotPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError(string.Empty, "Please enter your email address.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            // 1. Generate a secure random token
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            string rawToken = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-").Replace("/", "_").Replace("=", ""); // URL safe

            // 2. Save token and set expiration (e.g., 30 minutes from now)
            user.PasswordResetToken = rawToken;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await _userManager.UpdateAsync(user);

            // 3. Construct custom callback URL
            var callbackUrl = Url.Action(
                nameof(ResetPassword),
                "Account",
                values: new { email = user.Email, token = rawToken },
                protocol: Request.Scheme);

            // 4. Show the link directly in the UI Toast notification for local verification
            TempData["SuccessMessage"] = $"Reset link generated! Copy or click here: <a href='{callbackUrl}' class='text-white fw-bold text-decoration-underline' target='_blank'>Reset Password Link</a>";
        }
        else
        {
            // Still show success to prevent email enumeration attacks
            TempData["SuccessMessage"] = "If an account matches that email, reset instructions have been dispatched.";
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    // GET: /Account/ResetPassword
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return BadRequest("Invalid password reset parameters.");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.PasswordResetToken != token || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "The password reset link is invalid or has expired.";
            return RedirectToAction(nameof(Login));
        }

        // Pass email and token forward to the view model/form
        ViewBag.Email = email;
        ViewBag.Token = token;
        return View();
    }

    // POST: /Account/ResetPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string email, string token, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "The new password and confirmation password do not match.");
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.PasswordResetToken != token || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Password reset failed. Token is invalid or expired.";
            return RedirectToAction(nameof(Login));
        }

        // Manually remove old password and set the new one using Identity's secure hasher
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
        if (removePasswordResult.Succeeded)
        {
            var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (addPasswordResult.Succeeded)
            {
                // Clear out the token so it can never be reused
                user.PasswordResetToken = null;
                user.ResetTokenExpiry = null;
                await _userManager.UpdateAsync(user);

                TempData["SuccessMessage"] = "Your password has been reset successfully. You can now login.";
                return RedirectToAction(nameof(Login));
            }
            foreach (var error in addPasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewBag.Email = email;
        ViewBag.Token = token;
        return View();
    }
}
