using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class Department : Entity<int>
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }

    private Department() { } // EF Core

    public Department( int id,string name, string code)
    {
        Id = id;
        Name = name;
        Code = code;
    }

    public void UpdateDetails(string name, string code)
    {
        Name = name;
        Code = code;
    }
}
