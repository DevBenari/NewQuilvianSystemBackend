using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveEntitlementConfiguration
        : IEntityTypeConfiguration<TrxLeaveEntitlement>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveEntitlement> entity)
        {
            entity.ToTable("TrxLeaveEntitlement", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EntitlementNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PeriodStartDate).HasColumnType("date");
            entity.Property(x => x.PeriodEndDate).HasColumnType("date");
            entity.Property(x => x.GrantDate).HasColumnType("date");
            entity.Property(x => x.AvailableFromDate).HasColumnType("date");
            entity.Property(x => x.ExpiryDate).HasColumnType("date");

            entity.Property(x => x.BaseEntitlementDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.ProratedEntitlementDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.AdditionalEntitlementDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.CarryForwardEntitlementDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.TotalEntitlementDays).HasPrecision(18, 4).HasDefaultValue(0);

            entity.Property(x => x.IsProrated).HasDefaultValue(false);
            entity.Property(x => x.ServiceMonthsAtGrant).HasDefaultValue(0);
            entity.Property(x => x.CalculationVersion).HasDefaultValue(1);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(150);
            entity.Property(x => x.EntitlementStatus)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.EntitlementStatus.Draft)
                .IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(50).HasDefaultValue("Policy");
            entity.Property(x => x.SourceReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.GeneratedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CalculationDetailJson).HasColumnType("jsonb");
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeavePolicy).WithMany().HasForeignKey(x => x.LeavePolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlementPolicy).WithMany().HasForeignKey(x => x.LeaveEntitlementPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlementPeriod).WithMany(x => x.Entitlements).HasForeignKey(x => x.LeaveEntitlementPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveBalance).WithMany(x => x.Entitlements).HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EntitlementTransaction).WithMany().HasForeignKey(x => x.EntitlementTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.GeneratedByUser).WithMany().HasForeignKey(x => x.GeneratedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EntitlementNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.LeaveEntitlementPeriodId, x.EntitlementStatus });
            entity.HasIndex(x => new { x.LeaveBalanceId, x.EntitlementYear, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.LeavePolicyId, x.LeaveEntitlementPolicyId });
            entity.HasIndex(x => x.EntitlementTransactionId);
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<TrxLeaveEntitlement> entity)
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
