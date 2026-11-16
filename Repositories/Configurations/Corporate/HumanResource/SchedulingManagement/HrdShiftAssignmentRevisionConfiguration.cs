using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class HrdShiftAssignmentRevisionConfiguration : IEntityTypeConfiguration<HrdShiftAssignmentRevision>
    {
        public void Configure(EntityTypeBuilder<HrdShiftAssignmentRevision> entity)
        {
            entity.ToTable("HrdShiftAssignmentRevision", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ShiftDate).HasColumnType("date");
            entity.Property(x => x.ChangeType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PreviousStartAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PreviousEndAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.NewStartAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.NewEndAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ReasonCategory).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReasonNote).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ChangedAt).HasColumnType("timestamp with time zone").IsRequired();

            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.ShiftAssignment)
                .WithMany()
                .HasForeignKey(x => x.ShiftAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RosterPeriod)
                .WithMany()
                .HasForeignKey(x => x.RosterPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PreviousWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.PreviousWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.NewWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.NewWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PreviousShift)
                .WithMany()
                .HasForeignKey(x => x.PreviousShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.NewShift)
                .WithMany()
                .HasForeignKey(x => x.NewShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ShiftReplacement)
                .WithMany()
                .HasForeignKey(x => x.ShiftReplacementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.RosterPeriodId, x.ShiftDate });
            entity.HasIndex(x => new { x.ShiftAssignmentId, x.ChangedAt });
        }
    }
}
