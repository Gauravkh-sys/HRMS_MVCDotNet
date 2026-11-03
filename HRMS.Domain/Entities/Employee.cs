using HRMS.Domain.Common;
using HRMS.Domain.ValueObjects;

namespace HRMS.Domain.Entities;

public class Employee : AggregateRoot<Guid>
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; set; }
    public string Designation { get; private set; }
    public int DepartmentId { get; private set; }
    public Department Department { get; set; } = null!;
    public DateTime JoiningDate { get; private set; }
    public bool IsActive { get; private set; }

    private Employee() { } // EF Core

    public Employee(string firstName, string lastName, Email email, string designation, int departmentId, DateTime joiningDate)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Designation = designation;
        DepartmentId = departmentId;
        JoiningDate = joiningDate;
        IsActive = true;
    }

    public void UpdateProfile(string firstName, string lastName, string designation)
    {
        FirstName = firstName;
        LastName = lastName;
        Designation = designation;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
