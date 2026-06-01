using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxDoctorConsultationConfiguration : IEntityTypeConfiguration<TrxDoctorConsultation>
    {
        public void Configure(EntityTypeBuilder<TrxDoctorConsultation> entity)
        {
            entity.ToTable("TrxDoctorConsultation", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ConsultationNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.QueueId)
                .IsRequired();

            entity.Property(x => x.AssessmentId)
                .IsRequired(false);

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.DoctorId)
                .IsRequired();

            entity.Property(x => x.ServiceUnitId)
                .IsRequired();

            entity.Property(x => x.ClinicId)
                .IsRequired(false);

            entity.Property(x => x.ConsultationDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.ConsultationStatus)
                .HasConversion<int>()
                .HasDefaultValue(DoctorConsultationStatus.Draft)
                .IsRequired();

            entity.Property(x => x.IsVitalSignCopiedFromAssessment)
                .HasDefaultValue(true);

            entity.Property(x => x.BloodPressureSystolic)
                .IsRequired(false);

            entity.Property(x => x.BloodPressureDiastolic)
                .IsRequired(false);

            entity.Property(x => x.PulseRate)
                .IsRequired(false);

            entity.Property(x => x.RespiratoryRate)
                .IsRequired(false);

            entity.Property(x => x.Temperature)
                .HasColumnType("numeric(5,2)")
                .IsRequired(false);

            entity.Property(x => x.OxygenSaturation)
                .HasColumnType("numeric(5,2)")
                .IsRequired(false);

            entity.Property(x => x.Weight)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.Height)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.BMI)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.ChiefComplaint)
                .HasMaxLength(1000);

            entity.Property(x => x.HistoryOfPresentIllness)
                .HasMaxLength(4000);

            entity.Property(x => x.PhysicalExamination)
                .HasMaxLength(4000);

            entity.Property(x => x.Subjective)
                .HasMaxLength(4000);

            entity.Property(x => x.Objective)
                .HasMaxLength(4000);

            entity.Property(x => x.Assessment)
                .HasMaxLength(4000);

            entity.Property(x => x.Plan)
                .HasMaxLength(4000);

            entity.Property(x => x.DiagnosisText)
                .HasMaxLength(2000);

            entity.Property(x => x.PrimaryDiagnosisText)
                .HasMaxLength(2000);

            entity.Property(x => x.SecondaryDiagnosisText)
                .HasMaxLength(2000);

            entity.Property(x => x.DiagnosisCount)
                .HasDefaultValue(0);

            entity.Property(x => x.HasPrimaryDiagnosis)
                .HasDefaultValue(false);

            entity.Property(x => x.ProcedureText)
                .HasMaxLength(2000);

            entity.Property(x => x.ProcedureCount)
                .HasDefaultValue(0);

            entity.Property(x => x.HasProcedure)
                .HasDefaultValue(false);

            entity.Property(x => x.PrescriptionText)
                .HasMaxLength(2000);

            entity.Property(x => x.PrescriptionCount)
                .HasDefaultValue(0);

            entity.Property(x => x.HasPrescription)
                .HasDefaultValue(false);

            entity.Property(x => x.SupportingOrderText)
                .HasMaxLength(2000);

            entity.Property(x => x.SupportingOrderCount)
                .HasDefaultValue(0);

            entity.Property(x => x.HasSupportingOrder)
                .HasDefaultValue(false);

            entity.Property(x => x.MedicalCertificateCount)
                .HasDefaultValue(0);

            entity.Property(x => x.ClinicalDocumentCount)
                .HasDefaultValue(0);

            entity.Property(x => x.ConsentCount)
                .HasDefaultValue(0);

            entity.Property(x => x.ProcedurePlan)
                .HasMaxLength(2000);

            entity.Property(x => x.PrescriptionPlan)
                .HasMaxLength(2000);

            entity.Property(x => x.SupportingExamPlan)
                .HasMaxLength(2000);

            entity.Property(x => x.ReferralPlan)
                .HasMaxLength(2000);

            entity.Property(x => x.EducationPlan)
                .HasMaxLength(2000);

            entity.Property(x => x.FollowUpDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.FollowUpNote)
                .HasMaxLength(500);

            entity.Property(x => x.StartedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.StartedByUserId)
                .IsRequired(false);

            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CompletedByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelledByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelReason)
                .HasMaxLength(250);

            entity.Property(x => x.DoctorNote)
                .HasMaxLength(1000);

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

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Queue)
                .WithMany()
                .HasForeignKey(x => x.QueueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PatientAssessment)
                .WithMany()
                .HasForeignKey(x => x.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StartedByUser)
                .WithMany()
                .HasForeignKey(x => x.StartedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CompletedByUser)
                .WithMany()
                .HasForeignKey(x => x.CompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ConsultationNumber)
                .IsUnique();

            entity.HasIndex(x => x.EncounterId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.QueueId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.AssessmentId);

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.ConsultationDateTime,
                x.ConsultationStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.ConsultationDateTime,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EncounterId,
                x.ConsultationStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicId,
                x.ConsultationDateTime,
                x.ConsultationStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ConsultationStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.HasPrimaryDiagnosis,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.HasProcedure,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.HasPrescription,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.HasSupportingOrder,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DiagnosisCount,
                x.ProcedureCount,
                x.PrescriptionCount,
                x.SupportingOrderCount,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.FollowUpDate,
                x.IsDelete
            });

            entity.HasIndex(x => x.StartedByUserId);

            entity.HasIndex(x => x.CompletedByUserId);

            entity.HasIndex(x => x.CancelledByUserId);
        }
    }
}