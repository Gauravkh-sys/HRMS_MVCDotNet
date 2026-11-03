using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Infrastructure.Configurations;

public class PaySlipConfiguration : IEntityTypeConfiguration<PaySlip>
{
    public void Configure(EntityTypeBuilder<PaySlip> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Month).IsRequired();
        builder.Property(p => p.Year).IsRequired();
        builder.Property(p => p.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Allowances).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Deductions).HasColumnType("decimal(18,2)");
        builder.Property(p => p.NetSalary).HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}