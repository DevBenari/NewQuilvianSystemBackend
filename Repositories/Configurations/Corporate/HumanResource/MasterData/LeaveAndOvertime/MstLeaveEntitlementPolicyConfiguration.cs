using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.LeaveAndOvertime
{
    public class MstLeaveEntitlementPolicyConfiguration
        : IEntityTypeConfiguration<MstLeaveEntitlementPolicy>
    {
        public void Configure(EntityTypeBuilder<MstLeaveEntitlementPolicy> entity)
        {
            entity.ToTable("MstLeaveEntitlementPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EntitlementPolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EntitlementPolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.EntitlementMethod)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.EntitlementMethod.AnnualGrant)
                .IsRequired();
            entity.Property(x => x.PeriodBasis)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.PeriodBasis.CalendarYear)
                .IsRequired();
            entity.Property(x => x.GrantTiming)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.GrantTiming.StartOfPeriod)
                .IsRequired();
            entity.Property(x => x.AnnualEntitlementDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.AccrualFrequency).HasMaxLength(50).HasDefaultValue("Annual").IsRequired();
            entity.Property(x => x.AccrualTiming)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.AccrualTiming.EndOfPeriod)
                .IsRequired();
            entity.Property(x => x.AccrualAmountDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.FirstAccrualRule)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.FirstAccrualRule.Prorated)
                .IsRequired();
            entity.Property(x => x.FinalAccrualRule)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.FinalAccrualRule.Prorated)
                .IsRequired();
            entity.Property(x => x.AccrualMaximumPerPeriodDays).HasPrecision(18, 4);
            entity.Property(x => x.IsProratedOnJoin).HasDefaultValue(true);
            entity.Property(x => x.IsProratedOnSeparation).HasDefaultValue(true);
            entity.Property(x => x.MinimumServiceMonths).HasDefaultValue(0);
            entity.Property(x => x.MaximumBalanceDays).HasPrecision(18, 4);
            entity.Property(x => x.RoundingMethod)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.RoundingMethod.None)
                .IsRequired();
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date");
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LeavePolicy)
                .WithMany(x => x.EntitlementPolicies)
                .HasForeignKey(x => x.LeavePolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EntitlementPolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.EntitlementPolicyName);
            entity.HasIndex(x => new { x.LeavePolicyId, x.EntitlementMethod, x.AccrualFrequency, x.IsDefault, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstLeaveEntitlementPolicy> entity)
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
