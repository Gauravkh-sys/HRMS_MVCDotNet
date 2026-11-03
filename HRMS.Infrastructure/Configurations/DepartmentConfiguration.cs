using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(20);

        builder.HasData(
            new Department(1, "Information Technology", "IT"),
            new Department(2, "Human Resources", "HR"),
            new Department(3, "Finance", "FIN")
        );
    }
}