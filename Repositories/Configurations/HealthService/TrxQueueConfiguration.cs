using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxQueueConfiguration : IEntityTypeConfiguration<TrxQueue>
    {
        public void Configure(EntityTypeBuilder<TrxQueue> entity)
        {
            entity.ToTable("TrxQueue", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.ServiceUnitId)
                .IsRequired();

            entity.Property(x => x.ClinicId)
                .IsRequired(false);

            entity.Property(x => x.DoctorId)
                .IsRequired(false);

            entity.Property(x => x.DoctorScheduleId)
                .IsRequired(false);

            entity.Property(x => x.QueueDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.QueueNumber)
                .IsRequired();

            entity.Property(x => x.QueueCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.QueueStatus)
                .HasConversion<int>()
                .HasDefaultValue(QueueStatus.WaitingForNurse)
                .IsRequired();

            entity.Property(x => x.NurseCallAttemptCount)
                .HasDefaultValue(0);

            entity.Property(x => x.LastNurseCalledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastNurseCalledByUserId)
                .IsRequired(false);

            entity.Property(x => x.NurseCallExpiresAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DoctorCallAttemptCount)
                .HasDefaultValue(0);

            entity.Property(x => x.LastDoctorCalledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastDoctorCalledByUserId)
                .IsRequired(false);

            entity.Property(x => x.DoctorCallExpiresAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ScreeningStartedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ScreeningCompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ConsultationStartedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ConsultationCompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.SkipCount)
                .HasDefaultValue(0);

            entity.Property(x => x.LastSkippedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastSkippedByUserId)
                .IsRequired(false);

            entity.Property(x => x.SkipReason)
                .HasMaxLength(250);

            entity.Property(x => x.RequeueCount)
                .HasDefaultValue(0);

            entity.Property(x => x.LastRequeuedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastRequeuedByUserId)
                .IsRequired(false);

            entity.Property(x => x.RequeueReason)
                .HasMaxLength(250);

            entity.Property(x => x.NoShowAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.NoShowByUserId)
                .IsRequired(false);

            entity.Property(x => x.NoShowReason)
                .HasMaxLength(250);

            entity.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelledByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelReason)
                .HasMaxLength(250);

            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CompletedByUserId)
                .IsRequired(false);

            entity.Property(x => x.IsPriorityQueue)
                .HasDefaultValue(false);

            entity.Property(x => x.IsFromKiosk)
                .HasDefaultValue(false);

            entity.Property(x => x.IsWalkIn)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAppointment)
                .HasDefaultValue(false);

            entity.Property(x => x.IsScreeningRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsDoctorRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

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

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DoctorSchedule)
                .WithMany()
                .HasForeignKey(x => x.DoctorScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LastNurseCalledByUser)
                .WithMany()
                .HasForeignKey(x => x.LastNurseCalledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LastDoctorCalledByUser)
                .WithMany()
                .HasForeignKey(x => x.LastDoctorCalledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LastSkippedByUser)
                .WithMany()
                .HasForeignKey(x => x.LastSkippedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LastRequeuedByUser)
                .WithMany()
                .HasForeignKey(x => x.LastRequeuedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.NoShowByUser)
                .WithMany()
                .HasForeignKey(x => x.NoShowByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CompletedByUser)
                .WithMany()
                .HasForeignKey(x => x.CompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EncounterId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => x.DoctorScheduleId);

            entity.HasIndex(x => x.QueueCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.QueueDate,
                x.ServiceUnitId,
                x.ClinicId,
                x.DoctorId,
                x.QueueNumber
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.QueueDate,
                x.ServiceUnitId,
                x.ClinicId,
                x.DoctorId,
                x.QueueStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.QueueDate,
                x.QueueStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.QueueStatus,
                x.IsScreeningRequired,
                x.IsDoctorRequired,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.QueueDate,
                x.QueueStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.QueueDate,
                x.IsDelete
            });

            entity.HasIndex(x => x.LastNurseCalledByUserId);

            entity.HasIndex(x => x.LastDoctorCalledByUserId);

            entity.HasIndex(x => x.LastSkippedByUserId);

            entity.HasIndex(x => x.LastRequeuedByUserId);

            entity.HasIndex(x => x.NoShowByUserId);

            entity.HasIndex(x => x.CancelledByUserId);

            entity.HasIndex(x => x.CompletedByUserId);
        }
    }
}
