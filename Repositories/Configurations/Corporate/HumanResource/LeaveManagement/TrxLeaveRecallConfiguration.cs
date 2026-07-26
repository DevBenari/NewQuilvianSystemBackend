using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveRecallConfiguration : IEntityTypeConfiguration<TrxLeaveRecall>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveRecall> entity)
        {
            entity.ToTable("TrxLeaveRecall", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RecallNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OriginalLeaveEndDate).HasColumnType("date");
            entity.Property(x => x.RecallEffectiveDate).HasColumnType("date");
            entity.Property(x => x.ActualReturnToWorkDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.RecalledLeaveDays).HasPrecision(10, 2);
            entity.Property(x => x.RestoredBalanceDays).HasPrecision(10, 2);
            entity.Property(x => x.RecallReason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.RecallStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.AcknowledgedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.LeaveRequest).WithMany(x => x.Recalls).HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReplacementWorkforceProfile).WithMany().HasForeignKey(x => x.ReplacementWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BalanceTransaction).WithMany().HasForeignKey(x => x.BalanceTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InitiatedByUser).WithMany().HasForeignKey(x => x.InitiatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AcknowledgedByUser).WithMany().HasForeignKey(x => x.AcknowledgedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RecallNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.LeaveRequestId, x.RecallStatus, x.IsDelete });
            entity.HasIndex(x => new { x.RecallEffectiveDate, x.RecallStatus, x.IsDelete });
        }
    }
}
