using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class SalaryStructure : Entity<Guid>
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public decimal BasicSalary { get; private set; }
    public decimal Hra { get; private set; } // House Rent Allowance
    public decimal Allowances { get; private set; }
    public decimal Deductions { get; private set; }

    public decimal NetSalary => BasicSalary + Hra + Allowances - Deductions;

    private SalaryStructure() { } // EF Core

    public SalaryStructure(Guid employeeId, decimal basicSalary, decimal hra, decimal allowances, decimal deductions)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        BasicSalary = basicSalary;
        Hra = hra;
        Allowances = allowances;
        Deductions = deductions;
    }

    public void UpdateStructure(decimal basicSalary, decimal hra, decimal allowances, decimal deductions)
    {
        BasicSalary = basicSalary;
        Hra = hra;
        Allowances = allowances;
        Deductions = deductions;
    }
}
