using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveEntitlementConfiguration : IEntityTypeConfiguration<TrxLeaveEntitlement>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveEntitlement> entity)
        {
            entity.ToTable("TrxLeaveEntitlement", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntitlementNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PeriodStartDate).HasColumnType("date");
            entity.Property(x => x.PeriodEndDate).HasColumnType("date");
            entity.Property(x => x.ExpiryDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.BaseEntitlementDays).HasPrecision(10, 2);
            entity.Property(x => x.ProratedEntitlementDays).HasPrecision(10, 2);
            entity.Property(x => x.AdditionalEntitlementDays).HasPrecision(10, 2);
            entity.Property(x => x.CarryForwardEntitlementDays).HasPrecision(10, 2);
            entity.Property(x => x.TotalEntitlementDays).HasPrecision(10, 2);
            entity.Property(x => x.EntitlementStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(50).HasDefaultValue("Policy");
            entity.Property(x => x.SourceReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.GeneratedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone").IsRequired(false);
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
            entity.HasOne(x => x.LeavePolicy).WithMany().HasForeignKey(x => x.LeavePolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveEntitlementPolicy).WithMany().HasForeignKey(x => x.LeaveEntitlementPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveBalance).WithMany(x => x.Entitlements).HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.GeneratedByUser).WithMany().HasForeignKey(x => x.GeneratedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.EntitlementNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.LeaveTypeId, x.EntitlementYear, x.IsDelete }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.EntitlementStatus, x.ExpiryDate, x.IsActive, x.IsDelete });
        }
    }
}
