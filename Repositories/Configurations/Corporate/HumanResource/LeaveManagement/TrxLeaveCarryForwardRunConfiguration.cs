using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveCarryForwardRunConfiguration
        : IEntityTypeConfiguration<TrxLeaveCarryForwardRun>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveCarryForwardRun> entity)
        {
            entity.ToTable("TrxLeaveCarryForwardRun", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RunNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RunMode)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.BatchRunMode.Manual)
                .IsRequired();
            entity.Property(x => x.RunStatus)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.BatchRunStatus.Draft)
                .IsRequired();
            entity.Property(x => x.ExecutionDate).HasColumnType("date");
            entity.Property(x => x.IsDryRun).HasDefaultValue(false);
            entity.Property(x => x.ForceReprocess).HasDefaultValue(false);
            entity.Property(x => x.RetryCount).HasDefaultValue(0);
            entity.Property(x => x.MaximumRetryCount).HasDefaultValue(3);
            entity.Property(x => x.TargetCount).HasDefaultValue(0);
            entity.Property(x => x.CalculatedCount).HasDefaultValue(0);
            entity.Property(x => x.PostedCount).HasDefaultValue(0);
            entity.Property(x => x.SkippedCount).HasDefaultValue(0);
            entity.Property(x => x.FailedCount).HasDefaultValue(0);
            entity.Property(x => x.TotalSourceAvailableDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.TotalEligibleDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.TotalCarryForwardDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.TotalExpiredDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.TotalExcessDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.TotalPayoutDays).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(150);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
            entity.Property(x => x.StartedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ParametersJson).HasColumnType("jsonb");
            entity.Property(x => x.ResultSummaryJson).HasColumnType("jsonb");
            entity.Property(x => x.ErrorSummary).HasMaxLength(4000);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.SourceLeaveEntitlementPeriod)
                .WithMany(x => x.SourceCarryForwardRuns)
                .HasForeignKey(x => x.SourceLeaveEntitlementPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DestinationLeaveEntitlementPeriod)
                .WithMany(x => x.DestinationCarryForwardRuns)
                .HasForeignKey(x => x.DestinationLeaveEntitlementPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LeaveCarryForwardPolicy).WithMany().HasForeignKey(x => x.LeaveCarryForwardPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TriggeredByUser).WithMany().HasForeignKey(x => x.TriggeredByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RunNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            entity.HasIndex(x => new
                {
                    x.SourceLeaveEntitlementPeriodId,
                    x.DestinationLeaveEntitlementPeriodId,
                    x.LeaveTypeId,
                    x.LeaveCarryForwardPolicyId,
                    x.ExecutionDate,
                    x.RunMode
                });

            entity.HasIndex(x => new { x.RunStatus, x.ExecutionDate, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId });
            entity.HasIndex(x => x.CorrelationId);
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<TrxLeaveCarryForwardRun> entity)
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
