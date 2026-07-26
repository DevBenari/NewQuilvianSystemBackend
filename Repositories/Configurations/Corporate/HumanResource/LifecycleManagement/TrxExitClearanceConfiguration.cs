using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxExitClearanceConfiguration : IEntityTypeConfiguration<TrxExitClearance>
    {
        public void Configure(EntityTypeBuilder<TrxExitClearance> builder)
        {
            builder.ToTable("TrxExitClearance", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.ClearanceNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.DueDate).HasColumnType("date");
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ClearanceStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ProgressPercentage).HasPrecision(7, 2);
            builder.Property(x => x.Notes).HasMaxLength(1500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.EmployeeSeparation).WithMany().HasForeignKey(x => x.EmployeeSeparationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OffboardingChecklist).WithMany().HasForeignKey(x => x.OffboardingChecklistId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.ClearanceNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.EmployeeSeparationId).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.ClearanceStatus, x.DueDate });
        }
    }
}
