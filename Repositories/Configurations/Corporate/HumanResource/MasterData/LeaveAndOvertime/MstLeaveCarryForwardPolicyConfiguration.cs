using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.LeaveAndOvertime
{
    public class MstLeaveCarryForwardPolicyConfiguration
        : IEntityTypeConfiguration<MstLeaveCarryForwardPolicy>
    {
        public void Configure(EntityTypeBuilder<MstLeaveCarryForwardPolicy> entity)
        {
            entity.ToTable("MstLeaveCarryForwardPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CarryForwardPolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CarryForwardPolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.IsCarryForwardEnabled).HasDefaultValue(true);
            entity.Property(x => x.MinimumCarryForwardDays).HasPrecision(18, 4);
            entity.Property(x => x.MaximumCarryForwardDays).HasPrecision(18, 4);
            entity.Property(x => x.MaximumCarryForwardPeriods).HasDefaultValue(1);
            entity.Property(x => x.CarryForwardPercentage).HasPrecision(5, 2).HasDefaultValue(100);
            entity.Property(x => x.CarryForwardExecutionTiming)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.CarryForwardExecutionTiming.PeriodClose)
                .IsRequired();
            entity.Property(x => x.RoundingMethod)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.RoundingMethod.None)
                .IsRequired();
            entity.Property(x => x.ExpiryMethod).HasMaxLength(50).HasDefaultValue("MonthsAfterCarryForward").IsRequired();
            entity.Property(x => x.IsPayoutAllowed).HasDefaultValue(false);
            entity.Property(x => x.PayoutMaximumDays).HasPrecision(18, 4);
            entity.Property(x => x.ExcessBalanceAction).HasMaxLength(50).HasDefaultValue("Forfeit").IsRequired();
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date");
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LeaveEntitlementPolicy)
                .WithMany(x => x.CarryForwardPolicies)
                .HasForeignKey(x => x.LeaveEntitlementPolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DestinationLeaveType)
                .WithMany()
                .HasForeignKey(x => x.DestinationLeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CarryForwardPolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.CarryForwardPolicyName);
            entity.HasIndex(x => new { x.LeaveEntitlementPolicyId, x.IsCarryForwardEnabled, x.IsDefault, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.DestinationLeaveTypeId, x.ExpiryMethod, x.ExcessBalanceAction });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstLeaveCarryForwardPolicy> entity)
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
