using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientVitalSignConfiguration : IEntityTypeConfiguration<TrxPatientVitalSign>
    {
        public void Configure(EntityTypeBuilder<TrxPatientVitalSign> builder)
        {
            builder.ToTable("TrxPatientVitalSign", "public");

            builder.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            builder.Property(x => x.VitalSignRecordNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ObservationLocation)
                .HasMaxLength(100);

            builder.Property(x => x.MeasurementMethod)
                .HasMaxLength(100);

            builder.Property(x => x.DeviceName)
                .HasMaxLength(100);

            builder.Property(x => x.DeviceSerialNumber)
                .HasMaxLength(100);

            builder.Property(x => x.BloodPressureLocation)
                .HasMaxLength(50);

            builder.Property(x => x.PulseRhythmNote)
                .HasMaxLength(100);

            builder.Property(x => x.TemperatureRoute)
                .HasMaxLength(50);

            builder.Property(x => x.OxygenSupportNote)
                .HasMaxLength(100);

            builder.Property(x => x.WeightMeasurementNote)
                .HasMaxLength(100);

            builder.Property(x => x.NeurologicalNote)
                .HasMaxLength(250);

            builder.Property(x => x.PainLocation)
                .HasMaxLength(250);

            builder.Property(x => x.PainNote)
                .HasMaxLength(250);

            builder.Property(x => x.EwsMonitoringRecommendation)
                .HasMaxLength(250);

            builder.Property(x => x.DoctorNotificationNote)
                .HasMaxLength(250);

            builder.Property(x => x.ClinicalNote)
                .HasMaxLength(500);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.CancelReason)
                .HasMaxLength(250);

            // =========================
            // DECIMAL PRECISION
            // =========================

            builder.Property(x => x.MeanArterialPressure)
                .HasPrecision(6, 2);

            builder.Property(x => x.Temperature)
                .HasPrecision(5, 2);

            builder.Property(x => x.OxygenSaturation)
                .HasPrecision(5, 2);

            builder.Property(x => x.OxygenFlowRate)
                .HasPrecision(6, 2);

            builder.Property(x => x.Weight)
                .HasPrecision(8, 2);

            builder.Property(x => x.Height)
                .HasPrecision(8, 2);

            builder.Property(x => x.HeadCircumference)
                .HasPrecision(8, 2);

            builder.Property(x => x.BMI)
                .HasPrecision(6, 2);

            // =========================
            // DEFAULT VALUES
            // =========================

            builder.Property(x => x.IsPulseReadable)
                .HasDefaultValue(true);

            builder.Property(x => x.IsPulseRegular)
                .HasDefaultValue(true);

            builder.Property(x => x.IsUsingOxygen)
                .HasDefaultValue(false);

            builder.Property(x => x.HasPain)
                .HasDefaultValue(false);

            builder.Property(x => x.IsAbnormal)
                .HasDefaultValue(false);

            builder.Property(x => x.IsCritical)
                .HasDefaultValue(false);

            builder.Property(x => x.NeedDoctorNotification)
                .HasDefaultValue(false);

            builder.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            builder.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            // =========================
            // INDEXES
            // =========================

            builder.HasIndex(x => x.VitalSignRecordNumber)
                .IsUnique();

            builder.HasIndex(x => x.PatientId);

            builder.HasIndex(x => x.EncounterId);

            builder.HasIndex(x => x.QueueId);

            builder.HasIndex(x => x.AssessmentId);

            builder.HasIndex(x => x.ConsultationId);

            builder.HasIndex(x => x.DoctorId);

            builder.HasIndex(x => x.ServiceUnitId);

            builder.HasIndex(x => x.ClinicId);

            builder.HasIndex(x => x.ObservedByUserId);

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.ObservationDateTime
            });

            builder.HasIndex(x => new
            {
                x.EncounterId,
                x.ObservationDateTime
            });

            builder.HasIndex(x => new
            {
                x.ConsultationId,
                x.ObservationDateTime
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.VitalSignStatus,
                x.IsActive
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.IsAbnormal,
                x.IsCritical
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.EwsRiskLevel
            });

            builder.HasIndex(x => new
            {
                x.NeedDoctorNotification,
                x.DoctorNotifiedAt
            });

            // =========================
            // RELATIONS
            // =========================

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Queue)
                .WithMany()
                .HasForeignKey(x => x.QueueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Assessment)
                .WithMany()
                .HasForeignKey(x => x.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Consultation)
                .WithMany()
                .HasForeignKey(x => x.ConsultationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ObservedByUser)
                .WithMany()
                .HasForeignKey(x => x.ObservedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DoctorNotifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.DoctorNotifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}