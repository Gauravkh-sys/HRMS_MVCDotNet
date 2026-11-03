# AI Agent Operational Guidelines for HRMS Project

## 1. Architectural Layers & Folder Structure
Follow Clean Architecture with strict dependency rules pointing inward:
- `HRMS.Domain`: Enterprise entities, value objects, domain events, repository interfaces.
- `HRMS.Application`: CQRS/Command/Query handlers, DTOs, domain service interfaces, validation rules (FluentValidation).
- `HRMS.Infrastructure`: Entity Framework Core `DbContext`, SQL Server configurations, repository implementations, external integrations.
- `HRMS.Web`: ASP.NET Core MVC controllers, Razor Pages, view models, authentication/authorization setup.

## 2. Coding Standards
- **C#**: Enable nullable reference types (`<Nullable>enable</Nullable>`), use async/await for I/O operations, apply primary constructors where appropriate, and keep domain models anemic-free (encapsulate setters).
- **Entity Framework Core**: Use Fluent API for entity configurations, explicit migrations, and avoid lazy loading to prevent N+1 query issues.
- **Error Handling**: Use Result pattern or Domain Exceptions handled via global middleware.