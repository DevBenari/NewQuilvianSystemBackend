using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement
{
    /// <summary>RJ-DOC-REV-BE-004 — surat dokter.</summary>
    public class CliDoctorCertificateConfiguration : IEntityTypeConfiguration<CliDoctorCertificate>
    {
        public void Configure(EntityTypeBuilder<CliDoctorCertificate> builder)
        {
            builder.ToTable("CliDoctorCertificate", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CertificateNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.CertificateType).HasConversion<int>().IsRequired();
            builder.Property(x => x.CertificateStatus).HasConversion<int>().IsRequired();

            builder.Property(x => x.Purpose).HasMaxLength(500);
            builder.Property(x => x.Diagnosis).HasMaxLength(2000);
            builder.Property(x => x.ClinicalSummary).HasMaxLength(4000);
            builder.Property(x => x.AdditionalNote).HasMaxLength(2000);

            builder.Property(x => x.PatientNameSnapshot).HasMaxLength(250).IsRequired();
            builder.Property(x => x.MedicalRecordNumberSnapshot).HasMaxLength(50);
            builder.Property(x => x.GenderSnapshot).HasMaxLength(50);
            builder.Property(x => x.AddressSnapshot).HasMaxLength(1000);
            builder.Property(x => x.OccupationSnapshot).HasMaxLength(200);
            builder.Property(x => x.DoctorNameSnapshot).HasMaxLength(250);
            builder.Property(x => x.DoctorSipSnapshot).HasMaxLength(100);
            builder.Property(x => x.ClinicNameSnapshot).HasMaxLength(250);
            // Gambar tanda tangan dalam bentuk data URL; ukuran dibatasi pada validasi request.
            builder.Property(x => x.DoctorSignatureDataUrl).HasColumnType("text");

            builder.Property(x => x.ActivityRestriction).HasMaxLength(1000);
            builder.Property(x => x.HealthConclusion).HasMaxLength(1000);
            builder.Property(x => x.BloodPressure).HasMaxLength(50);
            builder.Property(x => x.Pulse).HasMaxLength(50);
            builder.Property(x => x.Temperature).HasMaxLength(50);
            builder.Property(x => x.Weight).HasMaxLength(50);
            builder.Property(x => x.Height).HasMaxLength(50);
            builder.Property(x => x.ColorBlindResult).HasMaxLength(200);
            builder.Property(x => x.HealthRecommendation).HasMaxLength(2000);

            builder.Property(x => x.ReferralDiagnosis).HasMaxLength(2000);
            builder.Property(x => x.ReferralReason).HasMaxLength(2000);
            builder.Property(x => x.ReferralTargetNameSnapshot).HasMaxLength(250);
            builder.Property(x => x.RequestedRoomClass).HasMaxLength(100);
            builder.Property(x => x.SpecialInstruction).HasMaxLength(2000);
            builder.Property(x => x.CancelReason).HasMaxLength(500);

            // QBE-CODE-004 — nomor surat unik.
            builder.HasIndex(x => x.CertificateNumber).IsUnique();
            builder.HasIndex(x => new { x.PatientId, x.IssuedDate });
            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.QueueId);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
