using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.LeaveAndOvertime
{
    public class MstLeaveAdjustmentReasonConfiguration
        : IEntityTypeConfiguration<MstLeaveAdjustmentReason>
    {
        public void Configure(EntityTypeBuilder<MstLeaveAdjustmentReason> entity)
        {
            entity.ToTable("MstLeaveAdjustmentReason", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReasonCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReasonName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ReasonCategory)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.AdjustmentReasonCategory.ManualAdjustment)
                .IsRequired();
            entity.Property(x => x.AllowedDirection)
                .HasMaxLength(20)
                .HasDefaultValue(LeaveValueConstants.AdjustmentAllowedDirection.Both)
                .IsRequired();

            entity.Property(x => x.AllowOpeningBalance).HasDefaultValue(false);
            entity.Property(x => x.AllowManualAdjustment).HasDefaultValue(true);
            entity.Property(x => x.AllowCorrection).HasDefaultValue(true);
            entity.Property(x => x.AllowReversal).HasDefaultValue(true);
            entity.Property(x => x.MaximumAdjustmentDays).HasPrecision(18, 4);
            entity.Property(x => x.RequiresComment).HasDefaultValue(true);
            entity.Property(x => x.RequiresAttachment).HasDefaultValue(false);
            entity.Property(x => x.RequiresApproval).HasDefaultValue(true);
            entity.Property(x => x.ApprovalWorkflowCode).HasMaxLength(100);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date");
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LeaveType)
                .WithMany()
                .HasForeignKey(x => x.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ReasonCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.LeaveTypeId, x.ReasonName })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.ReasonCategory,
                x.AllowedDirection,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.LeaveTypeId,
                x.SortOrder,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });

            entity.HasCheckConstraint(
                "CK_MstLeaveAdjustmentReason_MaximumAdjustmentDays",
                "\"MaximumAdjustmentDays\" IS NULL OR \"MaximumAdjustmentDays\" > 0");

            entity.HasCheckConstraint(
                "CK_MstLeaveAdjustmentReason_EffectiveDate",
                "\"EffectiveEndDate\" IS NULL OR \"EffectiveStartDate\" IS NULL OR \"EffectiveEndDate\" >= \"EffectiveStartDate\"");
        }

        private static void ConfigureAuditFields(
            EntityTypeBuilder<MstLeaveAdjustmentReason> entity)
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
