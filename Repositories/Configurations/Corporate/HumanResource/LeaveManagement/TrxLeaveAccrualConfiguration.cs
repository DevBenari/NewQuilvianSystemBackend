using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveAccrualConfiguration : IEntityTypeConfiguration<TrxLeaveAccrual>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveAccrual> entity)
        {
            entity.ToTable("TrxLeaveAccrual", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AccrualNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AccrualDate).HasColumnType("date");
            entity.Property(x => x.ScheduledAccrualDate).HasColumnType("date");
            entity.Property(x => x.AccrualPeriodStartDate).HasColumnType("date");
            entity.Property(x => x.AccrualPeriodEndDate).HasColumnType("date");
            entity.Property(x => x.AccrualSequence).HasDefaultValue(1);
            entity.Property(x => x.AccrualAmountDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.BalanceBeforeAccrual).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.BalanceAfterAccrual).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.IsProrated).HasDefaultValue(false);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(150);
            entity.Property(x => x.AccrualStatus)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.AccrualStatus.Draft)
                .IsRequired();
            entity.Property(x => x.AccrualFrequency).HasMaxLength(50).HasDefaultValue("Monthly");
            entity.Property(x => x.SourceType).HasMaxLength(50).HasDefaultValue("ScheduledAccrual");
            entity.Property(x => x.CalculatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CalculationDetailJson).HasColumnType("jsonb");
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveBalance).WithMany(x => x.Accruals).HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlement).WithMany(x => x.Accruals).HasForeignKey(x => x.LeaveEntitlementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlementPolicy).WithMany().HasForeignKey(x => x.LeaveEntitlementPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveAccrualRun).WithMany(x => x.Accruals).HasForeignKey(x => x.LeaveAccrualRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BalanceTransaction).WithMany().HasForeignKey(x => x.BalanceTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CalculatedByUser).WithMany().HasForeignKey(x => x.CalculatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AccrualNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            entity.HasIndex(x => new
                {
                    x.LeaveEntitlementId,
                    x.AccrualPeriodStartDate,
                    x.AccrualPeriodEndDate,
                    x.AccrualSequence
                })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"LeaveEntitlementId\" IS NOT NULL");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.AccrualDate, x.AccrualStatus });
            entity.HasIndex(x => new { x.LeaveBalanceId, x.LeaveAccrualRunId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => x.BalanceTransactionId);
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<TrxLeaveAccrual> entity)
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
