using Microsoft.AspNetCore.Identity;

namespace HRMS.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    
    // Custom fields for manual password reset
    public string? PasswordResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
}
