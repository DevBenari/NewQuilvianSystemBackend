using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxRetirementConfiguration : IEntityTypeConfiguration<TrxRetirement>
    {
        public void Configure(EntityTypeBuilder<TrxRetirement> builder)
        {
            builder.ToTable("TrxRetirement", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.RetirementNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RetirementType).HasMaxLength(50);
            builder.Property(x => x.NormalRetirementDate).HasColumnType("date");
            builder.Property(x => x.ActualRetirementDate).HasColumnType("date");
            builder.Property(x => x.RetirementStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Notes).HasMaxLength(1500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeSeparation).WithMany().HasForeignKey(x => x.EmployeeSeparationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.BenefitPlan).WithMany().HasForeignKey(x => x.BenefitPlanId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.RetirementNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.NormalRetirementDate, x.RetirementStatus });
        }
    }
}
