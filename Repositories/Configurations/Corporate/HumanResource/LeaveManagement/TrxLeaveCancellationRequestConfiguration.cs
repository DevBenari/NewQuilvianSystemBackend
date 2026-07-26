using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveCancellationRequestConfiguration : IEntityTypeConfiguration<TrxLeaveCancellationRequest>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveCancellationRequest> entity)
        {
            entity.ToTable("TrxLeaveCancellationRequest", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CancellationNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveCancellationDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.RestoredDays).HasPrecision(10, 2);
            entity.Property(x => x.CancellationReason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.CancellationStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovalNotes).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.LeaveRequest).WithMany(x => x.CancellationRequests).HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BalanceTransaction).WithMany().HasForeignKey(x => x.BalanceTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByUser).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.CancellationNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.LeaveRequestId, x.CancellationStatus, x.IsDelete });
            entity.HasIndex(x => new { x.WorkforceProfileId, x.RequestedAt, x.CancellationStatus, x.IsDelete });
        }
    }
}
