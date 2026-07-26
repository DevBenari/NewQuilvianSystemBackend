using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class TrxOnCallAssignmentConfiguration : IEntityTypeConfiguration<TrxOnCallAssignment>
    {
        public void Configure(EntityTypeBuilder<TrxOnCallAssignment> entity)
        {
            entity.ToTable("TrxOnCallAssignment", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StartAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EndAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.OnCallRole).HasMaxLength(30).HasDefaultValue("Primary").IsRequired();
            entity.Property(x => x.AssignmentStatus).HasMaxLength(30).HasDefaultValue("Scheduled").IsRequired();
            entity.Property(x => x.ConfirmedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ActivatedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.RosterPeriod).WithMany().HasForeignKey(x => x.RosterPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RosterAssignment).WithMany(x => x.OnCallAssignments).HasForeignKey(x => x.RosterAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OnCallType).WithMany().HasForeignKey(x => x.OnCallTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.WorkforceProfileId, x.StartAt, x.EndAt, x.IsDelete });
            entity.HasIndex(x => new { x.RosterPeriodId, x.AssignmentStatus, x.IsActive, x.IsDelete });
        }
    }
}
