using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstOnCallTypeConfiguration : IEntityTypeConfiguration<MstOnCallType>
    {
        public void Configure(EntityTypeBuilder<MstOnCallType> entity)
        {
            entity.ToTable("MstOnCallType", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OnCallTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OnCallTypeName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ResponseTimeMinutes).HasDefaultValue(0);
            entity.Property(x => x.MinimumCallHours).HasDefaultValue(0);
            entity.Property(x => x.MaximumCallHours).HasDefaultValue(24);
            entity.Property(x => x.IsRemoteAllowed).HasDefaultValue(false);
            entity.Property(x => x.RequiresOnSitePresence).HasDefaultValue(true);
            entity.Property(x => x.CountsAsWorkingTime).HasDefaultValue(true);
            entity.Property(x => x.IsAllowanceEligible).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasIndex(x => x.OnCallTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.OnCallTypeName);
            entity.HasIndex(x => new { x.IsRemoteAllowed, x.RequiresOnSitePresence, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstOnCallType> entity)
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
