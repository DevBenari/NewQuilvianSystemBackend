using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveRecallRequestConfiguration : IEntityTypeConfiguration<TrxLeaveRecallRequest>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveRecallRequest> entity)
        {
            entity.ToTable("TrxLeaveRecallRequest", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RecallNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EffectiveRecallDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.RestoredDays).HasPrecision(4, 1).IsRequired();
            entity.Property(x => x.EmergencyCategory).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RecallReason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("RequestedByHeadUnit").IsRequired();
            entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectionReason).HasMaxLength(500);

            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.LeaveRequest)
                .WithMany(x => x.RecallRequests)
                .HasForeignKey(x => x.LeaveRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RequestedByHeadUnit)
                .WithMany()
                .HasForeignKey(x => x.RequestedByHeadUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByHrManager)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByHrManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RecallNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.LeaveRequestId, x.Status, x.IsDelete });
            entity.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveRecallDate, x.IsDelete });
        }
    }
}
