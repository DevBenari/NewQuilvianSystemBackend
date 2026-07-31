using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveCarryForwardConfiguration
        : IEntityTypeConfiguration<TrxLeaveCarryForward>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveCarryForward> entity)
        {
            entity.ToTable("TrxLeaveCarryForward", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CarryForwardNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CalculationDate).HasColumnType("date");
            entity.Property(x => x.CarryForwardExpiryDate).HasColumnType("date");
            ConfigureDecimal(entity, x => x.SourceAvailableDays);
            ConfigureDecimal(entity, x => x.EligibleDays);
            ConfigureDecimal(entity, x => x.CarryForwardDays);
            ConfigureDecimal(entity, x => x.ExpiredDays);
            ConfigureDecimal(entity, x => x.ExcessDays);
            ConfigureDecimal(entity, x => x.PayoutDays);
            ConfigureDecimal(entity, x => x.RoundingAdjustmentDays);
            entity.Property(x => x.CarryForwardStatus)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.CarryForwardStatus.Draft)
                .IsRequired();
            entity.Property(x => x.SkipReasonCode).HasMaxLength(100);
            entity.Property(x => x.SkipReason).HasMaxLength(1000);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(150);
            entity.Property(x => x.CalculatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SourceBalanceSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.CalculationDetailJson).HasColumnType("jsonb");
            entity.Property(x => x.ErrorMessage).HasMaxLength(4000);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LeaveCarryForwardRun)
                .WithMany(x => x.CarryForwards)
                .HasForeignKey(x => x.LeaveCarryForwardRunId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeaveCarryForwardPolicy).WithMany().HasForeignKey(x => x.LeaveCarryForwardPolicyId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SourceLeaveEntitlementPeriod)
                .WithMany(x => x.SourceCarryForwards)
                .HasForeignKey(x => x.SourceLeaveEntitlementPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DestinationLeaveEntitlementPeriod)
                .WithMany(x => x.DestinationCarryForwards)
                .HasForeignKey(x => x.DestinationLeaveEntitlementPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SourceLeaveType).WithMany().HasForeignKey(x => x.SourceLeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DestinationLeaveType).WithMany().HasForeignKey(x => x.DestinationLeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SourceLeaveBalance).WithMany().HasForeignKey(x => x.SourceLeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DestinationLeaveBalance).WithMany().HasForeignKey(x => x.DestinationLeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CalculatedByUser).WithMany().HasForeignKey(x => x.CalculatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CarryForwardNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            entity.HasIndex(x => new
                {
                    x.LeaveCarryForwardRunId,
                    x.WorkforceProfileId,
                    x.SourceLeaveTypeId,
                    x.SourceLeaveEntitlementPeriodId,
                    x.DestinationLeaveEntitlementPeriodId
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.SourceLeaveBalanceId, x.CarryForwardStatus, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.DestinationLeaveBalanceId, x.CarryForwardStatus, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.CarryForwardExpiryDate, x.CarryForwardStatus, x.IsActive, x.IsDelete });
        }

        private static void ConfigureDecimal(
            EntityTypeBuilder<TrxLeaveCarryForward> entity,
            System.Linq.Expressions.Expression<Func<TrxLeaveCarryForward, decimal>> property)
        {
            entity.Property(property).HasPrecision(18, 4).HasDefaultValue(0);
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<TrxLeaveCarryForward> entity)
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
