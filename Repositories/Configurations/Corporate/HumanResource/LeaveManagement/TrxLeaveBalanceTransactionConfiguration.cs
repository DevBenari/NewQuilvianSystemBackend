using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveBalanceTransactionConfiguration
        : IEntityTypeConfiguration<TrxLeaveBalanceTransaction>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveBalanceTransaction> entity)
        {
            entity.ToTable("TrxLeaveBalanceTransaction", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TransactionNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TransactionDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.EffectiveDate).HasColumnType("date");
            entity.Property(x => x.TransactionSequence).HasDefaultValue(0L);
            entity.Property(x => x.TransactionType)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.TransactionType.ManualAdjustment)
                .IsRequired();
            entity.Property(x => x.Direction)
                .HasMaxLength(10)
                .HasDefaultValue(LeaveValueConstants.TransactionDirection.Credit)
                .IsRequired();

            ConfigureDecimal(entity, x => x.TransactionDays);
            ConfigureDecimal(entity, x => x.OpeningBalanceDelta);
            ConfigureDecimal(entity, x => x.EntitlementDelta);
            ConfigureDecimal(entity, x => x.AccruedDelta);
            ConfigureDecimal(entity, x => x.CarryForwardDelta);
            ConfigureDecimal(entity, x => x.AdjustmentDelta);
            ConfigureDecimal(entity, x => x.CompensatoryDelta);
            ConfigureDecimal(entity, x => x.PendingDelta);
            ConfigureDecimal(entity, x => x.ReservedDelta);
            ConfigureDecimal(entity, x => x.UsedDelta);
            ConfigureDecimal(entity, x => x.RecalledDelta);
            ConfigureDecimal(entity, x => x.ExpiredDelta);
            ConfigureDecimal(entity, x => x.EncashmentDelta);
            ConfigureDecimal(entity, x => x.AvailableDelta);
            ConfigureDecimal(entity, x => x.PreviousOpeningBalanceDays);
            ConfigureDecimal(entity, x => x.PreviousAvailableDays);
            ConfigureDecimal(entity, x => x.PreviousReservedDays);
            ConfigureDecimal(entity, x => x.NewAvailableDays);
            ConfigureDecimal(entity, x => x.NewReservedDays);
            ConfigureDecimal(entity, x => x.NewUsedDays);

            entity.Property(x => x.IdempotencyKey).HasMaxLength(150);
            entity.Property(x => x.PostingBatchType).HasMaxLength(50);
            entity.Property(x => x.SourceType).HasMaxLength(50).HasDefaultValue("System");
            entity.Property(x => x.SourceReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.TransactionStatus)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.TransactionStatus.Posted)
                .IsRequired();
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Remarks).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LeaveBalance).WithMany(x => x.Transactions).HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlementPeriod).WithMany(x => x.BalanceTransactions).HasForeignKey(x => x.LeaveEntitlementPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveRequest).WithMany(x => x.BalanceTransactions).HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlement).WithMany(x => x.BalanceTransactions).HasForeignKey(x => x.LeaveEntitlementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveAccrual).WithMany(x => x.BalanceTransactions).HasForeignKey(x => x.LeaveAccrualId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveCarryForward).WithMany(x => x.BalanceTransactions).HasForeignKey(x => x.LeaveCarryForwardId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveAdjustment).WithMany(x => x.BalanceTransactions).HasForeignKey(x => x.LeaveAdjustmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedTransaction).WithMany().HasForeignKey(x => x.ReversedTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OriginalTransaction).WithMany().HasForeignKey(x => x.OriginalTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TransactionNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            entity.HasIndex(x => new { x.LeaveBalanceId, x.TransactionSequence })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"TransactionSequence\" > 0");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.EffectiveDate });
            entity.HasIndex(x => new { x.LeaveEntitlementPeriodId, x.TransactionStatus, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.PostingBatchType, x.PostingBatchId });
            entity.HasIndex(x => new { x.SourceType, x.SourceReferenceId });
            entity.HasIndex(x => x.OriginalTransactionId);
            entity.HasIndex(x => x.LeaveCarryForwardId);
            entity.HasIndex(x => x.LeaveAdjustmentId);
            entity.HasIndex(x => x.ReversedTransactionId);
        }

        private static void ConfigureDecimal(
            EntityTypeBuilder<TrxLeaveBalanceTransaction> entity,
            System.Linq.Expressions.Expression<Func<TrxLeaveBalanceTransaction, decimal>> property)
        {
            entity.Property(property).HasPrecision(18, 4).HasDefaultValue(0);
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<TrxLeaveBalanceTransaction> entity)
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
