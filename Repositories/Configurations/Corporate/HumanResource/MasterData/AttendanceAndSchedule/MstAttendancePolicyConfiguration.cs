using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstAttendancePolicyConfiguration : IEntityTypeConfiguration<MstAttendancePolicy>
    {
        public void Configure(EntityTypeBuilder<MstAttendancePolicy> entity)
        {
            entity.ToTable("MstAttendancePolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AttendancePolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AttendancePolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.RequireCheckIn).HasDefaultValue(true);
            entity.Property(x => x.RequireCheckOut).HasDefaultValue(true);
            entity.Property(x => x.AllowMultipleCheckInOut).HasDefaultValue(false);
            entity.Property(x => x.AutoCloseOpenAttendance).HasDefaultValue(false);
            entity.Property(x => x.MinimumWorkMinutes).HasDefaultValue(0);
            entity.Property(x => x.MaximumWorkMinutes).HasDefaultValue(1440);
            entity.Property(x => x.IsOvertimeEnabled).HasDefaultValue(true);
            entity.Property(x => x.OvertimeThresholdMinutes).HasDefaultValue(0);
            entity.Property(x => x.IsAttendanceLocationRequired).HasDefaultValue(false);
            entity.Property(x => x.AllowManualCorrection).HasDefaultValue(true);
            entity.Property(x => x.CorrectionRequestLimitDays).HasDefaultValue(7);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.WorkSchedule)
                .WithMany(x => x.AttendancePolicies)
                .HasForeignKey(x => x.WorkScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.GracePeriodPolicy)
                .WithMany(x => x.AttendancePolicies)
                .HasForeignKey(x => x.GracePeriodPolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AttendancePolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.AttendancePolicyName);
            entity.HasIndex(x => new { x.WorkScheduleId, x.GracePeriodPolicyId });
            entity.HasIndex(x => new { x.IsDefault, x.IsOvertimeEnabled, x.AllowManualCorrection, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstAttendancePolicy> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
