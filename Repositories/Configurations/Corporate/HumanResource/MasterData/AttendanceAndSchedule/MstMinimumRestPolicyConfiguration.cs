using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstMinimumRestPolicyConfiguration : IEntityTypeConfiguration<MstMinimumRestPolicy>
    {
        public void Configure(EntityTypeBuilder<MstMinimumRestPolicy> entity)
        {
            entity.ToTable("MstMinimumRestPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.MinimumRestPolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.MinimumRestPolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.MinimumRestHours).HasPrecision(5, 2).HasDefaultValue(8m);
            entity.Property(x => x.MinimumRestHoursAfterNightShift).HasPrecision(5, 2).HasDefaultValue(10m);
            entity.Property(x => x.MinimumRestHoursAfterOvertime).HasPrecision(5, 2).HasDefaultValue(8m);
            entity.Property(x => x.MaximumDailyWorkHours).HasPrecision(5, 2).HasDefaultValue(12m);
            entity.Property(x => x.MaximumWeeklyWorkHours).HasPrecision(5, 2).HasDefaultValue(40m);
            entity.Property(x => x.MinimumWeeklyRestHours).HasPrecision(5, 2).HasDefaultValue(24m);
            entity.Property(x => x.ApplyToAllShifts).HasDefaultValue(true);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.ShiftGroup)
                .WithMany(x => x.MinimumRestPolicies)
                .HasForeignKey(x => x.ShiftGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.MinimumRestPolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.MinimumRestPolicyName);
            entity.HasIndex(x => new { x.ShiftGroupId, x.ApplyToAllShifts, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstMinimumRestPolicy> entity)
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
