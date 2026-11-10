using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveBalanceLedgerConfiguration : IEntityTypeConfiguration<TrxLeaveBalanceLedger>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveBalanceLedger> entity)
        {
            entity.ToTable("TrxLeaveBalanceLedger", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PeriodYear).IsRequired();
            entity.Property(x => x.TransactionType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AmountDays).HasPrecision(4, 1).IsRequired();
            entity.Property(x => x.BalanceBefore).HasPrecision(4, 1).IsRequired();
            entity.Property(x => x.BalanceAfter).HasPrecision(4, 1).IsRequired();
            entity.Property(x => x.TransactionDate).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.ExpiryDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsForfeited).HasDefaultValue(false);
            entity.Property(x => x.Notes).HasMaxLength(500);

            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeaveType)
                .WithMany()
                .HasForeignKey(x => x.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReferenceRequest)
                .WithMany(x => x.BalanceLedgers)
                .HasForeignKey(x => x.ReferenceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReferenceRecall)
                .WithMany()
                .HasForeignKey(x => x.ReferenceRecallId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReferenceAdjustment)
                .WithMany(x => x.BalanceLedgers)
                .HasForeignKey(x => x.ReferenceAdjustmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.PeriodYear, x.IsDelete });
            entity.HasIndex(x => new { x.PeriodYear, x.TransactionType, x.IsDelete });
            entity.HasIndex(x => new { x.ExpiryDate, x.IsForfeited, x.IsDelete });
        }
    }
}
