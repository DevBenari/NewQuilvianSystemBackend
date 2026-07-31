using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class TrxAttendanceProcessingRunConfiguration
        : IEntityTypeConfiguration<TrxAttendanceProcessingRun>
    {
        public void Configure(EntityTypeBuilder<TrxAttendanceProcessingRun> builder)
        {
            builder.ToTable("TrxAttendanceProcessingRun", "public");
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

            builder.Property(x => x.RunNumber)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(x => x.ProcessingMode)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.ProcessingRunMode.Batch)
                .IsRequired();

            builder.Property(x => x.RunStatus)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.ProcessingRunStatus.Pending)
                .IsRequired();

            builder.Property(x => x.TriggerSource)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.ProcessingTriggerSource.System)
                .IsRequired();

            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.StartedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CorrelationId).HasMaxLength(100);
            builder.Property(x => x.ParametersJson).HasColumnType("jsonb");
            builder.Property(x => x.ErrorSummary).HasMaxLength(2000);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.TargetWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.TargetWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TriggeredByUser)
                .WithMany()
                .HasForeignKey(x => x.TriggeredByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RunNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.CorrelationId)
                .IsUnique()
                .HasFilter("\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => new { x.RunStatus, x.StartedAt });
            builder.HasIndex(x => new { x.StartDate, x.EndDate, x.ProcessingMode });
            builder.HasIndex(x => new { x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.StartDate });
            builder.HasIndex(x => new { x.TargetWorkforceProfileId, x.StartDate, x.EndDate });
        }
    }
}
