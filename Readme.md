# Enterprise HRMS Solution

A modular, enterprise-grade Human Resource Management System built with modern .NET, ASP.NET Core MVC & Razor Pages, and Microsoft SQL Server, structured strictly following **Domain-Driven Design (DDD)** and **Clean Architecture**.

## Key Bounded Contexts
1. **Employee Management**: Personal records, department/designation assignments, employment status tracking.
2. **Leave Management**: Leave types, balance calculations, multi-tier approval workflows.
3. **Attendance & Time Tracking**: Shift management, check-in/check-out logs, overtime calculations.
4. **Payroll & Compensation**: Salary structures, allowances, deductions, and payslip generation.

## Technology Stack
- **Backend**: .NET 10 / ASP.NET Core MVC & Razor Pages
- **Database**: Microsoft SQL Server (via Entity Framework Core Code-First Migrations)
- **Validation & Mapping**: FluentValidation, AutoMapper / Mapster
- **Testing**: xUnit, Moq, FluentAssertions