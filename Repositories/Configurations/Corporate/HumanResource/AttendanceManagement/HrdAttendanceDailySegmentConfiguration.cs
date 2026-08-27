using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendanceDailySegmentConfiguration
        : IEntityTypeConfiguration<HrdAttendanceDailySegment>
    {
        public void Configure(EntityTypeBuilder<HrdAttendanceDailySegment> builder)
        {
            builder.ToTable("HrdAttendanceDailySegment", "public");
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

            builder.Property(x => x.SegmentType)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.AttendanceSegmentType.Work)
                .IsRequired();

            builder.Property(x => x.SegmentSource)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.AttendanceSegmentSource.Processor);

            builder.Property(x => x.ScheduledStartAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.ScheduledEndAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.ActualStartAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.ActualEndAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.SegmentStatus)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.AttendanceSegmentStatus.Calculated);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(x => x.AttendanceDaily)
                .WithMany(x => x.Segments)
                .HasForeignKey(x => x.AttendanceDailyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ShiftAssignment)
                .WithMany()
                .HasForeignKey(x => x.ShiftAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.StartRawLog)
                .WithMany()
                .HasForeignKey(x => x.StartRawLogId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EndRawLog)
                .WithMany()
                .HasForeignKey(x => x.EndRawLogId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AttendanceDailyId, x.SegmentOrder })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.ShiftAssignmentId, x.SegmentType })
                .HasFilter("\"ShiftAssignmentId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => new { x.SegmentType, x.SegmentStatus });
        }
    }
}
