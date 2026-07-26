using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingAttendanceConfiguration : IEntityTypeConfiguration<TrxTrainingAttendance>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingAttendance> entity)
        {
            entity.ToTable("TrxTrainingAttendance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AttendanceDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CheckInAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CheckOutAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ScheduledMinutes).HasDefaultValue(0);
            entity.Property(x => x.AttendedMinutes).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingSession)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.TrainingSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingParticipant)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.TrainingParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RecordedByUser)
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.TrainingParticipantId, x.AttendanceDate })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.TrainingSessionId, x.AttendanceStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingAttendance> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
