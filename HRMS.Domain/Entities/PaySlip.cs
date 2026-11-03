using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class PaySlip : AggregateRoot<Guid>
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public int Month { get; private set; } // 1 to 12
    public int Year { get; private set; }
    public decimal BasicSalary { get; private set; }
    public decimal Allowances { get; private set; }
    public decimal Deductions { get; private set; }
    public decimal NetSalary { get; private set; }
    public DateTime GeneratedDate { get; private set; }

    // Constructor name must match the class name (PaySlip) exactly
    private PaySlip() { }

    public PaySlip(Guid employeeId, int month, int year, decimal basicSalary, decimal allowances, decimal deductions)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        Month = month;
        Year = year;
        BasicSalary = basicSalary;
        Allowances = allowances;
        Deductions = deductions;
        NetSalary = (basicSalary + allowances) - deductions; // Added missing minus sign
        GeneratedDate = DateTime.UtcNow;
    }
}