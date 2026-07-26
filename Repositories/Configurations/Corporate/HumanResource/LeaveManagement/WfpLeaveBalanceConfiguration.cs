using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
            entity.Property(x => x.OpeningBalanceDays).HasPrecision(10, 2);
            entity.Property(x => x.EntitlementDays).HasPrecision(10, 2);
            entity.Property(x => x.AccruedDays).HasPrecision(10, 2);
            entity.Property(x => x.CarriedForwardDays).HasPrecision(10, 2);
            entity.Property(x => x.AdjustmentDays).HasPrecision(10, 2);
            entity.Property(x => x.CompensatoryDays).HasPrecision(10, 2);
            entity.Property(x => x.ReservedDays).HasPrecision(10, 2);
            entity.Property(x => x.PendingDays).HasPrecision(10, 2);
            entity.Property(x => x.UsedDays).HasPrecision(10, 2);
            entity.Property(x => x.RecalledDays).HasPrecision(10, 2);
            entity.Property(x => x.ExpiredDays).HasPrecision(10, 2);
            entity.Property(x => x.EncashmentDays).HasPrecision(10, 2);
            entity.Property(x => x.RemainingDays).HasPrecision(10, 2);
            entity.Property(x => x.AvailableDays).HasPrecision(10, 2);
            entity.Property(x => x.LastCalculatedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.LockedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeavePolicy).WithMany().HasForeignKey(x => x.LeavePolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlementPolicy).WithMany().HasForeignKey(x => x.LeaveEntitlementPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LockedByUser).WithMany().HasForeignKey(x => x.LockedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.Year }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.PeriodStartDate, x.PeriodEndDate, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.LeaveTypeId, x.Year, x.IsActive, x.IsDelete });
        }
    }
}
