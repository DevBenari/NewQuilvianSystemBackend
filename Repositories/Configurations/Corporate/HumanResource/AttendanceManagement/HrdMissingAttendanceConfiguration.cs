using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdMissingAttendanceConfiguration : IEntityTypeConfiguration<HrdMissingAttendance>
    {
        public void Configure(EntityTypeBuilder<HrdMissingAttendance> builder)
        {
            builder.ToTable("HrdMissingAttendance", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.AttendanceDate).HasColumnType("date");
            builder.Property(x => x.MissingType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ExpectedCheckInAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ExpectedCheckOutAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ActualCheckInAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ActualCheckOutAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.MissingStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.DetectedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.NotifiedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ResolvedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ResolutionNote).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkSchedule).WithMany().HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceException).WithMany().HasForeignKey(x => x.AttendanceExceptionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceCorrectionRequest).WithMany().HasForeignKey(x => x.AttendanceCorrectionRequestId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ResolvedByUser).WithMany().HasForeignKey(x => x.ResolvedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.WorkforceProfileId, x.AttendanceDate, x.MissingType }).IsUnique().HasFilter("\"IsDelete\" = false AND \"MissingStatus\" <> 'Closed'");
            builder.HasIndex(x => new { x.MissingStatus, x.IsPayrollBlocking, x.AttendanceDate });
        }
    }
}
