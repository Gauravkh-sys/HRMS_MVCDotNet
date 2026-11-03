using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Infrastructure.Configurations;

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Hra).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Allowances).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Deductions).HasColumnType("decimal(18,2)");

        builder.HasOne(s => s.Employee)
            .WithMany()
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}