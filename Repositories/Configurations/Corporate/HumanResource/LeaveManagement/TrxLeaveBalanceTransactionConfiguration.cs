using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveBalanceTransactionConfiguration : IEntityTypeConfiguration<TrxLeaveBalanceTransaction>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveBalanceTransaction> entity)
        {
            entity.ToTable("TrxLeaveBalanceTransaction", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TransactionNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TransactionDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.TransactionType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Direction).HasMaxLength(10).IsRequired();
            entity.Property(x => x.TransactionDays).HasPrecision(10, 2);
            entity.Property(x => x.PreviousOpeningBalanceDays).HasPrecision(10, 2);
            entity.Property(x => x.PreviousAvailableDays).HasPrecision(10, 2);
            entity.Property(x => x.PreviousReservedDays).HasPrecision(10, 2);
            entity.Property(x => x.NewAvailableDays).HasPrecision(10, 2);
            entity.Property(x => x.NewReservedDays).HasPrecision(10, 2);
            entity.Property(x => x.NewUsedDays).HasPrecision(10, 2);
            entity.Property(x => x.SourceType).HasMaxLength(50).HasDefaultValue("System");
            entity.Property(x => x.SourceReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.TransactionStatus).HasMaxLength(30).HasDefaultValue("Posted").IsRequired();
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Remarks).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.LeaveBalance).WithMany(x => x.Transactions).HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveRequest).WithMany(x => x.BalanceTransactions).HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlement).WithMany(x => x.BalanceTransactions).HasForeignKey(x => x.LeaveEntitlementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveAccrual).WithMany(x => x.BalanceTransactions).HasForeignKey(x => x.LeaveAccrualId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedTransaction).WithMany().HasForeignKey(x => x.ReversedTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.TransactionNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.LeaveBalanceId, x.TransactionDateTime, x.TransactionStatus, x.IsDelete });
            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.TransactionDateTime, x.IsDelete });
            entity.HasIndex(x => new { x.SourceType, x.SourceReferenceId, x.IsDelete });
        }
    }
}
