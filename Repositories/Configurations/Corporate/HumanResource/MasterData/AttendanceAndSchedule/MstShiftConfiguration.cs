using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstShiftConfiguration : IEntityTypeConfiguration<MstShift>
    {
        public void Configure(EntityTypeBuilder<MstShift> entity)
        {
            entity.ToTable("MstShift", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ShiftCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ShiftName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.StartTime).HasColumnType("time without time zone").IsRequired();
            entity.Property(x => x.EndTime).HasColumnType("time without time zone").IsRequired();
            entity.Property(x => x.BreakDurationMinutes).HasDefaultValue(0);
            entity.Property(x => x.PaidWorkMinutes).HasDefaultValue(0);
            entity.Property(x => x.IsOvernight).HasDefaultValue(false);
            entity.Property(x => x.IsNightShift).HasDefaultValue(false);
            entity.Property(x => x.IsOnCall).HasDefaultValue(false);
            entity.Property(x => x.IsOffShift).HasDefaultValue(false);
            entity.Property(x => x.AllowOvertime).HasDefaultValue(true);
            entity.Property(x => x.ColorCode).HasMaxLength(20);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.WorkSchedule)
                .WithMany(x => x.Shifts)
                .HasForeignKey(x => x.WorkScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ShiftGroup)
                .WithMany(x => x.Shifts)
                .HasForeignKey(x => x.ShiftGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OnCallType)
                .WithMany(x => x.Shifts)
                .HasForeignKey(x => x.OnCallTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ShiftCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ShiftName);
            entity.HasIndex(x => x.WorkScheduleId);
            entity.HasIndex(x => x.ShiftGroupId);
            entity.HasIndex(x => x.OnCallTypeId);
            entity.HasIndex(x => new { x.ShiftGroupId, x.IsNightShift, x.IsOnCall, x.IsOffShift, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstShift> entity)
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
