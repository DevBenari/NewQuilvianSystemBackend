using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientEncounterConfiguration : IEntityTypeConfiguration<TrxPatientEncounter>
    {
        public void Configure(EntityTypeBuilder<TrxPatientEncounter> entity)
        {
            entity.ToTable("TrxPatientEncounter", "public");

            entity.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            entity.Property(x => x.EncounterNumber)
                .HasMaxLength(50)
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

            entity.Property(x => x.DoctorServiceRuleId)
                .IsRequired(false);

            entity.Property(x => x.PatientClassId)
                .IsRequired(false);

            entity.Property(x => x.KioskScanSessionId)
                .IsRequired(false);

            // =========================
            // AGE CATEGORY SNAPSHOT
            // =========================

            entity.Property(x => x.AgeCategoryId)
                .IsRequired(false);

            entity.Property(x => x.AgeYearAtEncounter)
                .IsRequired(false);

            entity.Property(x => x.AgeMonthAtEncounter)
                .IsRequired(false);

            entity.Property(x => x.AgeDayAtEncounter)
                .IsRequired(false);

            entity.Property(x => x.TotalAgeDaysAtEncounter)
                .IsRequired(false);

            entity.Property(x => x.AgeTextAtEncounter)
                .HasMaxLength(100);

            entity.Property(x => x.AgeCategoryCodeSnapshot)
                .HasMaxLength(50);

            entity.Property(x => x.AgeCategoryNameSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.AgeReferenceDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.AgeCalculatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.EncounterDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.EncounterType)
                .HasConversion<int>()
                .HasDefaultValue(EncounterType.Outpatient)
                .IsRequired();

            entity.Property(x => x.VisitType)
                .HasConversion<int>()
                .HasDefaultValue(VisitType.NewVisit)
                .IsRequired();

            entity.Property(x => x.RegistrationSource)
                .HasConversion<int>()
                .HasDefaultValue(EncounterRegistrationSource.FrontDesk)
                .IsRequired();

            entity.Property(x => x.EncounterStatus)
                .HasConversion<int>()
                .HasDefaultValue(EncounterStatus.Registered)
                .IsRequired();

            entity.Property(x => x.ChiefComplaint)
                .HasMaxLength(500);

            // =========================
            // PAYMENT SUMMARY
            // =========================

            entity.Property(x => x.PaymentType)
                .HasConversion<int>()
                .HasDefaultValue(EncounterPaymentType.Cash)
                .IsRequired();

            entity.Property(x => x.PaymentMethodId)
                .IsRequired(false);

            entity.Property(x => x.IsInsurancePatient)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCompanyPatient)
                .HasDefaultValue(false);

            entity.Property(x => x.IsMembershipPatient)
                .HasDefaultValue(false);

            entity.Property(x => x.IsMixedPayment)
                .HasDefaultValue(false);

            entity.Property(x => x.PrimaryGuarantorNameSnapshot)
                .HasMaxLength(250);

            entity.Property(x => x.PrimaryGuarantorTypeSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.EligibilityReferenceNumber)
                .HasMaxLength(250);

            entity.Property(x => x.EligibilityCheckedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsEligibilityRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsEligibilityCompleted)
                .HasDefaultValue(false);

            // =========================
            // REFERRAL SUMMARY
            // =========================

            entity.Property(x => x.IsReferral)
                .HasDefaultValue(false);

            entity.Property(x => x.ReferralNumber)
                .HasMaxLength(250);

            entity.Property(x => x.IsReferralRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsReferralVerified)
                .HasDefaultValue(false);

            // =========================
            // REGISTRATION FLAGS
            // =========================

            entity.Property(x => x.IsNewPatient)
                .HasDefaultValue(false);

            entity.Property(x => x.IsFromKiosk)
                .HasDefaultValue(false);

            entity.Property(x => x.IsWalkIn)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAppointment)
                .HasDefaultValue(false);

            entity.Property(x => x.IsScreeningRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsQueueRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsDoctorRequired)
                .HasDefaultValue(true);

            // =========================
            // TIMELINE
            // =========================

            entity.Property(x => x.RegisteredAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.RegisteredByUserId)
                .IsRequired();

            entity.Property(x => x.CheckedInAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

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

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            // =========================
            // IDENTITY MODEL
            // =========================

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

            // =========================
            // RELATIONS
            // =========================

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

            entity.HasOne(x => x.DoctorServiceRule)
                .WithMany()
                .HasForeignKey(x => x.DoctorServiceRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PatientClass)
                .WithMany()
                .HasForeignKey(x => x.PatientClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AgeCategory)
                .WithMany()
                .HasForeignKey(x => x.AgeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PaymentMethod)
                .WithMany()
                .HasForeignKey(x => x.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.KioskScanSession)
                .WithMany()
                .HasForeignKey(x => x.KioskScanSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RegisteredByUser)
                .WithMany()
                .HasForeignKey(x => x.RegisteredByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.NoShowByUser)
                .WithMany()
                .HasForeignKey(x => x.NoShowByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.EncounterGuarantors)
                .WithOne(x => x.Encounter)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // INDEXES
            // =========================

            entity.HasIndex(x => x.EncounterNumber)
                .IsUnique();

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => x.DoctorScheduleId);

            entity.HasIndex(x => x.DoctorServiceRuleId);

            entity.HasIndex(x => x.PatientClassId);

            entity.HasIndex(x => x.AgeCategoryId);

            entity.HasIndex(x => x.PaymentMethodId);

            entity.HasIndex(x => x.KioskScanSessionId);

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.EncounterDate,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicId,
                x.DoctorId,
                x.EncounterDate,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EncounterStatus,
                x.EncounterType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PaymentType,
                x.PaymentMethodId,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsInsurancePatient,
                x.IsCompanyPatient,
                x.IsMixedPayment,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsEligibilityRequired,
                x.IsEligibilityCompleted,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.RegisteredAt,
                x.EncounterStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.AgeCategoryId,
                x.AgeCategoryCodeSnapshot,
                x.IsDelete
            });
        }
    }
}