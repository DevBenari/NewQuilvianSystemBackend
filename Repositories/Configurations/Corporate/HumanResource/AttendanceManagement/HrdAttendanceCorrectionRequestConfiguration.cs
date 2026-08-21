using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendanceCorrectionRequestConfiguration
        : IEntityTypeConfiguration<HrdAttendanceCorrectionRequest>
    {
        public void Configure(EntityTypeBuilder<HrdAttendanceCorrectionRequest> builder)
        {
            builder.ToTable("HrdAttendanceCorrectionRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.RequestNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.AttendanceDate)
                .HasColumnType("date");

            builder.Property(x => x.CorrectionType)
                .HasMaxLength(50)
                .HasDefaultValue(AttendanceValueConstants.CorrectionType.AttendanceTime)
                .IsRequired();

            builder.Property(x => x.RequestStatus)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.CorrectionRequestStatus.Draft)
                .IsRequired();

            builder.Property(x => x.Reason)
                .HasMaxLength(1500)
                .IsRequired();

            builder.Property(x => x.EvidenceFilePath).HasMaxLength(500);
            builder.Property(x => x.EvidenceFileName).HasMaxLength(255);
            builder.Property(x => x.EvidenceContentType).HasMaxLength(100);
            builder.Property(x => x.OriginalSummaryJson).HasColumnType("jsonb");
            builder.Property(x => x.RequestedSummaryJson).HasColumnType("jsonb");
            builder.Property(x => x.ApprovedSummaryJson).HasColumnType("jsonb");
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.FinalNote).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AttendanceDaily)
                .WithMany(x => x.CorrectionRequests)
                .HasForeignKey(x => x.AttendanceDailyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Attendance)
                .WithMany()
                .HasForeignKey(x => x.AttendanceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequestReason)
                .WithMany()
                .HasForeignKey(x => x.RequestReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RejectionReason)
                .WithMany()
                .HasForeignKey(x => x.RejectionReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkflowInstance)
                .WithMany()
                .HasForeignKey(x => x.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequestedByWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.RequestedByWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequestedByUser)
                .WithMany()
                .HasForeignKey(x => x.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AppliedByUser)
                .WithMany()
                .HasForeignKey(x => x.AppliedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RequestNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.WorkflowInstanceId)
                .IsUnique()
                .HasFilter("\"WorkflowInstanceId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => new { x.WorkforceProfileId, x.AttendanceDate });
            builder.HasIndex(x => new { x.RequestStatus, x.SubmittedAt });
            builder.HasIndex(x => new { x.WorkflowDefinitionId, x.RequestStatus });
        }
    }
}
