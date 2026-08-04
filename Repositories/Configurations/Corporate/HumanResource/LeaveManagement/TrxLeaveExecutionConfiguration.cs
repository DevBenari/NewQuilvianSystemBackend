using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveExecutionConfiguration : IEntityTypeConfiguration<TrxLeaveExecution>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveExecution> builder)
        {
            builder.ToTable("TrxLeaveExecution", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExecutionNumber).HasMaxLength(60).IsRequired();
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.RequestedDays).HasPrecision(18, 4);
            builder.Property(x => x.ExecutedDays).HasPrecision(18, 4);
            builder.Property(x => x.ExecutionStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.AttendanceIntegrationStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.BalanceExecutionStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.StartedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.LastAttemptAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CorrelationId).HasMaxLength(120);
            builder.Property(x => x.IdempotencyKey).HasMaxLength(160);
            builder.Property(x => x.ExecutionSnapshotJson).HasColumnType("jsonb");
            builder.Property(x => x.ResultSnapshotJson).HasColumnType("jsonb");
            builder.Property(x => x.ErrorSummary).HasMaxLength(4000);
            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.HasOne(x => x.LeaveRequest).WithMany().HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LeaveBalance).WithMany().HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.StartedByUser).WithMany().HasForeignKey(x => x.StartedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ExecutionNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.LeaveRequestId).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.ExecutionStatus, x.StartDate, x.EndDate });
            builder.HasIndex(x => new { x.WorkforceProfileId, x.StartDate, x.EndDate });
        }
    }
}
