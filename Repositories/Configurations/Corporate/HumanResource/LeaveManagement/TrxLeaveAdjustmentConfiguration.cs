using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveAdjustmentConfiguration
        : IEntityTypeConfiguration<TrxLeaveAdjustment>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveAdjustment> entity)
        {
            entity.ToTable("TrxLeaveAdjustment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AdjustmentNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AdjustmentType)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.AdjustmentType.ManualAdjustment)
                .IsRequired();
            entity.Property(x => x.Direction)
                .HasMaxLength(10)
                .HasDefaultValue(LeaveValueConstants.TransactionDirection.Credit)
                .IsRequired();
            entity.Property(x => x.RequestedDays).HasPrecision(18, 4);
            entity.Property(x => x.ApprovedDays).HasPrecision(18, 4);
            entity.Property(x => x.PostedDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.EffectiveDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.AdjustmentStatus)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.AdjustmentStatus.Draft)
                .IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(150);
            entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.RequestNote).HasMaxLength(1000);
            entity.Property(x => x.SourceType)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.AdjustmentSourceType.HrManual);
            entity.Property(x => x.SourceReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.RequestedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovalNote).HasMaxLength(1000);
            entity.Property(x => x.RejectionReason).HasMaxLength(1000);
            entity.Property(x => x.ReversalReason).HasMaxLength(1000);
            entity.Property(x => x.RequestSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ApprovalSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.PostingSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeaveBalance)
                .WithMany(x => x.Adjustments)
                .HasForeignKey(x => x.LeaveBalanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeaveType)
                .WithMany()
                .HasForeignKey(x => x.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeaveEntitlementPeriod)
                .WithMany(x => x.Adjustments)
                .HasForeignKey(x => x.LeaveEntitlementPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeaveAdjustmentReason)
                .WithMany()
                .HasForeignKey(x => x.LeaveAdjustmentReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowInstance)
                .WithMany()
                .HasForeignKey(x => x.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OriginalAdjustment)
                .WithOne(x => x.ReversalAdjustment)
                .HasForeignKey<TrxLeaveAdjustment>(x => x.OriginalAdjustmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RequestedByUser)
                .WithMany()
                .HasForeignKey(x => x.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RejectedByUser)
                .WithMany()
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PostedByUser)
                .WithMany()
                .HasForeignKey(x => x.PostedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReversedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReversedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AdjustmentNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            entity.HasIndex(x => x.WorkflowInstanceId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"WorkflowInstanceId\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.LeaveBalanceId,
                x.AdjustmentType,
                x.AdjustmentStatus
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.LeaveTypeId,
                x.EffectiveDate
            });

            entity.HasIndex(x => new
            {
                x.LeaveEntitlementPeriodId,
                x.AdjustmentStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.AdjustmentStatus,
                x.SubmittedAt,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => x.LeaveAdjustmentReasonId);
            entity.HasIndex(x => new { x.SourceType, x.SourceReferenceId });
            entity.HasIndex(x => x.OriginalAdjustmentId);

            entity.HasIndex(x => new { x.LeaveBalanceId, x.AdjustmentType })
                .IsUnique()
                .HasFilter(
                    "\"IsDelete\" = false " +
                    "AND \"AdjustmentType\" = 'OpeningBalance' " +
                    "AND \"AdjustmentStatus\" NOT IN ('Rejected', 'Cancelled', 'Reversed')");

            entity.HasCheckConstraint(
                "CK_TrxLeaveAdjustment_RequestedDays",
                "\"RequestedDays\" > 0");

            entity.HasCheckConstraint(
                "CK_TrxLeaveAdjustment_ApprovedDays",
                "\"ApprovedDays\" IS NULL OR \"ApprovedDays\" > 0");

            entity.HasCheckConstraint(
                "CK_TrxLeaveAdjustment_PostedDays",
                "\"PostedDays\" >= 0");
        }

        private static void ConfigureAuditFields(
            EntityTypeBuilder<TrxLeaveAdjustment> entity)
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
