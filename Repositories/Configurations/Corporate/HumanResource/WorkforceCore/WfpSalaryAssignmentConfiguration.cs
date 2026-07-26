using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpSalaryAssignmentConfiguration : IEntityTypeConfiguration<WfpSalaryAssignment>
    {
        public void Configure(EntityTypeBuilder<WfpSalaryAssignment> builder)
        {
            builder.ToTable("WfpSalaryAssignment", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.BaseSalary).HasPrecision(18, 2);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            builder.Property(x => x.PaymentFrequency).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SalaryStructure).WithMany().HasForeignKey(x => x.SalaryStructureId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SalaryGrade).WithMany().HasForeignKey(x => x.SalaryGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveStartDate, x.EffectiveEndDate });
            builder.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");
        }
    }
}
