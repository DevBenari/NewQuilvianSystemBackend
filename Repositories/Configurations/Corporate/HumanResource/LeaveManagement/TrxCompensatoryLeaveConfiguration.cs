using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxCompensatoryLeaveConfiguration : IEntityTypeConfiguration<TrxCompensatoryLeave>
    {
        public void Configure(EntityTypeBuilder<TrxCompensatoryLeave> entity)
        {
            entity.ToTable("TrxCompensatoryLeave", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CompensatoryLeaveNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(50).HasDefaultValue("Overtime").IsRequired();
            entity.Property(x => x.SourceReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.EarnedDate).HasColumnType("date");
            entity.Property(x => x.ExpiryDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.SourceHours).HasPrecision(10, 2);
            entity.Property(x => x.EarnedDays).HasPrecision(10, 2);
            entity.Property(x => x.UsedDays).HasPrecision(10, 2);
            entity.Property(x => x.RemainingDays).HasPrecision(10, 2);
            entity.Property(x => x.CompensatoryLeaveStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveBalance).WithMany().HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlement).WithMany().HasForeignKey(x => x.LeaveEntitlementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BalanceTransaction).WithMany().HasForeignKey(x => x.BalanceTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.CompensatoryLeaveNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.EarnedDate, x.IsDelete });
            entity.HasIndex(x => new { x.CompensatoryLeaveStatus, x.ExpiryDate, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.SourceType, x.SourceReferenceId, x.IsDelete });
        }
    }
}
