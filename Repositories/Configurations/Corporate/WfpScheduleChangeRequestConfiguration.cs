using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpScheduleChangeRequestConfiguration : IEntityTypeConfiguration<WfpScheduleChangeRequest>
    {
        public void Configure(EntityTypeBuilder<WfpScheduleChangeRequest> entity)
        {
            entity.ToTable("WfpScheduleChangeRequest", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.CurrentWorkScheduleAssignmentId)
                .IsRequired(false);

            entity.Property(x => x.RequestedScheduleDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.RequestedWorkScheduleId)
                .IsRequired(false);

            entity.Property(x => x.RequestType)
                .HasConversion<int>()
                .HasDefaultValue(ScheduleChangeRequestType.Unknown)
                .IsRequired();

            entity.Property(x => x.Reason)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.ApprovalStatus)
                .HasConversion<int>()
                .HasDefaultValue(ScheduleRequestApprovalStatus.Pending)
                .IsRequired();

            entity.Property(x => x.RequestedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.ApprovedByUserId)
                .IsRequired(false);

            entity.Property(x => x.ApprovedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RejectedByUserId)
                .IsRequired(false);

            entity.Property(x => x.RejectedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RejectedReason)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.ScheduleChangeRequests)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CurrentWorkScheduleAssignment)
                .WithMany()
                .HasForeignKey(x => x.CurrentWorkScheduleAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RequestedWorkSchedule)
                .WithMany()
                .HasForeignKey(x => x.RequestedWorkScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RejectedByUser)
                .WithMany()
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.CurrentWorkScheduleAssignmentId);

            entity.HasIndex(x => x.RequestedWorkScheduleId);

            entity.HasIndex(x => x.RequestType);

            entity.HasIndex(x => x.ApprovalStatus);

            entity.HasIndex(x => x.RequestedScheduleDate);

            entity.HasIndex(x => x.RequestedAt);

            entity.HasIndex(x => x.ApprovedByUserId);

            entity.HasIndex(x => x.RejectedByUserId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.RequestedScheduleDate,
                x.ApprovalStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.RequestType,
                x.ApprovalStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.CurrentWorkScheduleAssignmentId,
                x.ApprovalStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.RequestedScheduleDate,
                x.RequestType,
                x.ApprovalStatus
            })
            .HasFilter("\"ApprovalStatus\" = 1 AND \"IsDelete\" = false");
        }
    }
}
