using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class MstWorkScheduleConfiguration : IEntityTypeConfiguration<MstWorkSchedule>
    {
        public void Configure(EntityTypeBuilder<MstWorkSchedule> entity)
        {
            entity.ToTable("MstWorkSchedule", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScheduleCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ScheduleName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ScheduleType)
                .HasMaxLength(50)
                .HasDefaultValue("Shift")
                .IsRequired();

            entity.Property(x => x.WorkStartTime)
                .HasColumnType("time without time zone")
                .IsRequired();

            entity.Property(x => x.WorkEndTime)
                .HasColumnType("time without time zone")
                .IsRequired();

            entity.Property(x => x.IsOvernight)
                .HasDefaultValue(false);

            entity.Property(x => x.CheckInToleranceMinutes)
                .HasDefaultValue(0);

            entity.Property(x => x.CheckOutToleranceMinutes)
                .HasDefaultValue(0);

            entity.Property(x => x.IsDefault)
                .HasDefaultValue(false);

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

            entity.HasIndex(x => x.ScheduleCode)
                .IsUnique();

            entity.HasIndex(x => x.ScheduleName);

            entity.HasIndex(x => x.ScheduleType);

            entity.HasIndex(x => new
            {
                x.ScheduleType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsDefault,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
