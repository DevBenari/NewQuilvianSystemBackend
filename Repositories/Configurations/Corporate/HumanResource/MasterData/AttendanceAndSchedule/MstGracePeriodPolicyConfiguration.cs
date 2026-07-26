using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstGracePeriodPolicyConfiguration : IEntityTypeConfiguration<MstGracePeriodPolicy>
    {
        public void Configure(EntityTypeBuilder<MstGracePeriodPolicy> entity)
        {
            entity.ToTable("MstGracePeriodPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.GracePeriodPolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.GracePeriodPolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.EarlyCheckInMinutes).HasDefaultValue(0);
            entity.Property(x => x.LateCheckInGraceMinutes).HasDefaultValue(0);
            entity.Property(x => x.EarlyCheckOutGraceMinutes).HasDefaultValue(0);
            entity.Property(x => x.LateCheckOutMinutes).HasDefaultValue(0);
            entity.Property(x => x.CountLateAfterGrace).HasDefaultValue(true);
            entity.Property(x => x.CountEarlyLeaveAfterGrace).HasDefaultValue(true);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasIndex(x => x.GracePeriodPolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.GracePeriodPolicyName);
            entity.HasIndex(x => new { x.LateCheckInGraceMinutes, x.EarlyCheckOutGraceMinutes, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstGracePeriodPolicy> entity)
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
