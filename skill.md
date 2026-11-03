---
name: hrms-ddd-engineering
description: Domain-Driven Design (DDD) and Clean Architecture standards for building enterprise HRMS applications using ASP.NET Core MVC/Razor Pages and SQL Server.
---

# HRMS DDD Engineering Skill

## Core Directives
1. **Strict DDD Boundaries**: Isolate business logic within the **Domain Layer**. Entities must encapsulate state and invariant validation. Use Value Objects for immutable attributes (e.g., `Email`, `EmployeeId`, `Money`, `DateRange`).
2. **Ubiquitous Language**: Use domain-specific terminology across code, database schemas, and UI (e.g., `Employee`, `LeaveRequest`, `AttendanceRecord`, `PaySlip`, `Department`).
3. **Aggregate Roots**: Enforce consistency boundaries through Aggregate Roots (`Employee` aggregate, `LeaveRequest` aggregate). Repositories should only be defined for Aggregate Roots.
4. **CQRS & Separation**: Separate read models from write models where applicable using Application Layer Handlers / Services.
5. **Presentation**: Utilize ASP.NET Core MVC for complex workflows and Razor Pages for dedicated form-heavy transactional screens (e.g., Leave Application, Attendance Punch-in).