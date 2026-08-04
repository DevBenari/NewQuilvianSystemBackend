using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class WfpLeaveBalanceConfiguration : IEntityTypeConfiguration<WfpLeaveBalance>
    {
        public void Configure(EntityTypeBuilder<WfpLeaveBalance> entity)
        {
            entity.ToTable("WfpLeaveBalance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PeriodStartDate).HasColumnType("date");
            entity.Property(x => x.PeriodEndDate).HasColumnType("date");
            entity.Property(x => x.CarryForwardExpiryDate).HasColumnType("date");

            ConfigureDecimal(entity, x => x.OpeningBalanceDays);
            ConfigureDecimal(entity, x => x.EntitlementDays);
            ConfigureDecimal(entity, x => x.AccruedDays);
            ConfigureDecimal(entity, x => x.CarriedForwardDays);
            ConfigureDecimal(entity, x => x.AdjustmentDays);
            ConfigureDecimal(entity, x => x.CompensatoryDays);
            ConfigureDecimal(entity, x => x.ReservedDays);
            ConfigureDecimal(entity, x => x.PendingDays);
            ConfigureDecimal(entity, x => x.UsedDays);
            ConfigureDecimal(entity, x => x.RecalledDays);
            ConfigureDecimal(entity, x => x.ExpiredDays);
            ConfigureDecimal(entity, x => x.EncashmentDays);
            ConfigureDecimal(entity, x => x.RemainingDays);
            ConfigureDecimal(entity, x => x.AvailableDays);

            entity.Property(x => x.BalanceStatus)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.BalanceStatus.Active)
                .IsRequired();
            entity.Property(x => x.LastTransactionSequence).HasDefaultValue(0L);
            entity.Property(x => x.BalanceVersion).HasDefaultValue(0L).IsConcurrencyToken();
            entity.Property(x => x.LastCalculatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.LastReconciledAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsLocked).HasDefaultValue(false);
            entity.Property(x => x.LockedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeavePolicy).WithMany().HasForeignKey(x => x.LeavePolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlementPolicy).WithMany().HasForeignKey(x => x.LeaveEntitlementPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlementPeriod).WithMany(x => x.LeaveBalances).HasForeignKey(x => x.LeaveEntitlementPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LastTransaction).WithMany().HasForeignKey(x => x.LastTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LockedByUser).WithMany().HasForeignKey(x => x.LockedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.LeaveEntitlementPeriodId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"LeaveEntitlementPeriodId\" IS NOT NULL");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.Year, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.LeaveEntitlementPeriodId, x.BalanceStatus, x.IsLocked, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.LeavePolicyId, x.LeaveEntitlementPolicyId });
            entity.HasIndex(x => x.LastTransactionId);
        }

        private static void ConfigureDecimal(
            EntityTypeBuilder<WfpLeaveBalance> entity,
            System.Linq.Expressions.Expression<Func<WfpLeaveBalance, decimal>> property)
        {
            entity.Property(property).HasPrecision(18, 4).HasDefaultValue(0);
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<WfpLeaveBalance> entity)
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
