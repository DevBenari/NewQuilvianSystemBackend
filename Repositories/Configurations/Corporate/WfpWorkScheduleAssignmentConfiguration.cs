using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpWorkScheduleAssignmentConfiguration : IEntityTypeConfiguration<WfpWorkScheduleAssignment>
    {
        public void Configure(EntityTypeBuilder<WfpWorkScheduleAssignment> entity)
        {
            entity.ToTable("WfpWorkScheduleAssignment", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.WorkScheduleId)
                .IsRequired();

            entity.Property(x => x.ScheduleDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.IsOffDay)
                .HasDefaultValue(false);

            entity.Property(x => x.IsOvertimePlanned)
                .HasDefaultValue(false);

            entity.Property(x => x.IsOnCall)
                .HasDefaultValue(false);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.WorkScheduleAssignments)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkSchedule)
                .WithMany()
                .HasForeignKey(x => x.WorkScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.WorkScheduleId);

            entity.HasIndex(x => x.ScheduleDate);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.ScheduleDate,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkScheduleId,
                x.ScheduleDate,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.ScheduleDate
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");
        }
    }
}
