using HRMS.Domain.Entities;
using HRMS.Domain.Repositories;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly HRMSDbContext _context;

    public EmployeeRepository(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();
    }

    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
    }

    public void Update(Employee employee)
    {
        _context.Employees.Update(employee);
    }
}