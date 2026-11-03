using HRMS.Domain.Entities;

namespace HRMS.Domain.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id);
    Task<IEnumerable<Employee>> GetAllAsync();
    Task AddAsync(Employee employee);
    void Update(Employee employee);
}
