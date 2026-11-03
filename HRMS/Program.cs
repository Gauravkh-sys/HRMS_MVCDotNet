using HRMS.Application.Interface;
using HRMS.Application.Services;
using HRMS.Domain.Repositories;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using HRMS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 1. Configure EF Core SQL Server connection
builder.Services.AddDbContext<HRMSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configure ASP.NET Core Identity with Roles & EF Stores
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<HRMSDbContext>()
.AddDefaultTokenProviders();

// Configure Application Cookie Settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register Repositories & Services
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IExcelReportService, ExcelReportService>();

var app = builder.Build();

// 3. Seed Roles, Admin Account, and Sync Employees on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<HRMSDbContext>();

        // Seed Roles
        string[] roleNames = { "Admin", "HRManager", "Employee" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed Super Admin Account
        var adminEmail = "admin@hrms.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                NormalizedUserName = adminEmail.ToUpper(),
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                FullName = "System Administrator",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin@123");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Sync Active Employees Table to AspNetUsers
        var activeEmployees = await dbContext.Employees.Where(e => e.IsActive).ToListAsync();
        foreach (var emp in activeEmployees)
        {
            string empEmail = emp.Email.Value;
            var existingUser = await userManager.FindByEmailAsync(empEmail);

            if (existingUser == null)
            {
                var newEmployeeUser = new ApplicationUser
                {
                    UserName = empEmail,
                    NormalizedUserName = empEmail.ToUpper(),
                    Email = empEmail,
                    NormalizedEmail = empEmail.ToUpper(),
                    FullName = $"{emp.FirstName} {emp.LastName}",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(newEmployeeUser, "Password@12");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newEmployeeUser, "Employee");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(existingUser, "Employee"))
                {
                    await userManager.AddToRoleAsync(existingUser, "Employee");
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding Identity roles and users.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();
app.MapRazorPages();

app.Run();