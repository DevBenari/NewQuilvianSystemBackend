using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstRosterPolicyConfiguration : IEntityTypeConfiguration<MstRosterPolicy>
    {
        public void Configure(EntityTypeBuilder<MstRosterPolicy> entity)
        {
            entity.ToTable("MstRosterPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RosterPolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RosterPolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.MinimumStaffPerShift).HasDefaultValue(0);
            entity.Property(x => x.MinimumWeeklyHours).HasDefaultValue(0);
            entity.Property(x => x.MaximumWeeklyHours).HasDefaultValue(40);
            entity.Property(x => x.MaximumConsecutiveWorkDays).HasDefaultValue(6);
            entity.Property(x => x.MaximumConsecutiveNightShifts).HasDefaultValue(3);
            entity.Property(x => x.MinimumDaysOffPerMonth).HasDefaultValue(4);
            entity.Property(x => x.PublishLeadDays).HasDefaultValue(7);
            entity.Property(x => x.LockLeadDays).HasDefaultValue(1);
            entity.Property(x => x.RequireApproval).HasDefaultValue(true);
            entity.Property(x => x.RequireSkillMixValidation).HasDefaultValue(true);
            entity.Property(x => x.AllowShiftSwap).HasDefaultValue(true);
            entity.Property(x => x.AllowEmergencyOverride).HasDefaultValue(true);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ShiftGroup)
                .WithMany(x => x.RosterPolicies)
                .HasForeignKey(x => x.ShiftGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RosterPolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.RosterPolicyName);
            entity.HasIndex(x => new { x.HospitalSiteId, x.ShiftGroupId });
            entity.HasIndex(x => new { x.RequireApproval, x.RequireSkillMixValidation, x.AllowShiftSwap, x.IsDefault, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstRosterPolicy> entity)
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
