using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveEntitlementPeriodConfiguration
        : IEntityTypeConfiguration<TrxLeaveEntitlementPeriod>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveEntitlementPeriod> entity)
        {
            entity.ToTable("TrxLeaveEntitlementPeriod", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PeriodCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PeriodName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PeriodBasis)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.PeriodBasis.CalendarYear)
                .IsRequired();
            entity.Property(x => x.StartDate).HasColumnType("date");
            entity.Property(x => x.EndDate).HasColumnType("date");
            entity.Property(x => x.PeriodStatus)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.PeriodStatus.Open)
                .IsRequired();
            entity.Property(x => x.IsLocked).HasDefaultValue(false);
            entity.Property(x => x.ProcessingStartedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CloseReason).HasMaxLength(1000);
            entity.Property(x => x.ReopenedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReopenReason).HasMaxLength(1000);
            entity.Property(x => x.ReopenCount).HasDefaultValue(0);
            entity.Property(x => x.LastReconciledAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ValidationSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ProcessingStartedByUser).WithMany().HasForeignKey(x => x.ProcessingStartedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ClosedByUser).WithMany().HasForeignKey(x => x.ClosedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReopenedByUser).WithMany().HasForeignKey(x => x.ReopenedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PeriodCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.StartDate, x.EndDate, x.PeriodStatus });
            entity.HasIndex(x => new { x.LeaveTypeId, x.PeriodYear, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.StartDate, x.EndDate });
            entity.HasIndex(x => new { x.PeriodStatus, x.IsLocked, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<TrxLeaveEntitlementPeriod> entity)
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
