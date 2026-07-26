using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
            entity.Property(x => x.AccrualPeriodStartDate).HasColumnType("date");
            entity.Property(x => x.AccrualPeriodEndDate).HasColumnType("date");
            entity.Property(x => x.AccrualAmountDays).HasPrecision(10, 2);
            entity.Property(x => x.BalanceBeforeAccrual).HasPrecision(10, 2);
            entity.Property(x => x.BalanceAfterAccrual).HasPrecision(10, 2);
            entity.Property(x => x.AccrualStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.AccrualFrequency).HasMaxLength(50).HasDefaultValue("Monthly");
            entity.Property(x => x.SourceType).HasMaxLength(50).HasDefaultValue("ScheduledAccrual");
            entity.Property(x => x.CalculatedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CalculationDetailJson).HasColumnType("jsonb");
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveBalance).WithMany(x => x.Accruals).HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlement).WithMany(x => x.Accruals).HasForeignKey(x => x.LeaveEntitlementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlementPolicy).WithMany().HasForeignKey(x => x.LeaveEntitlementPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CalculatedByUser).WithMany().HasForeignKey(x => x.CalculatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.AccrualNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.AccrualPeriodStartDate, x.AccrualPeriodEndDate }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.AccrualStatus, x.AccrualDate, x.IsActive, x.IsDelete });
        }
    }
}
