namespace HRMS.Application.DTOs.Employee;

public class CreateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public DateTime JoiningDate { get; set; } = DateTime.Now;
}