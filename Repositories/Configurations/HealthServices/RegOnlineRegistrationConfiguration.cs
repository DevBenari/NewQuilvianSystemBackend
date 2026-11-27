using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>
    /// Pemetaan Pendaftaran Pasien Online — kiosk <c>data/data-dictionary.md</c> PO-D.2 dan DDL PO-D.6.
    /// </summary>
    public class RegOnlineRegistrationConfiguration : IEntityTypeConfiguration<RegOnlineRegistration>
    {
        public void Configure(EntityTypeBuilder<RegOnlineRegistration> builder)
        {
            builder.ToTable("RegOnlineRegistration", "public", table =>
            {
                // KSK-DEC-043: No. WhatsApp atau email wajib diisi salah satu.
                table.HasCheckConstraint(
                    "CK_RegOnlineRegistration_Contact",
                    "\"ContactWhatsAppNumber\" IS NOT NULL OR \"ContactEmail\" IS NOT NULL");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RegistrationNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CodeHash).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(OnlineRegistrationStatus.AwaitingCheckIn)
                .IsRequired();
            builder.Property(x => x.PatientMode).HasConversion<int>().IsRequired();
            builder.Property(x => x.IdentityNumber).HasMaxLength(16);
            builder.Property(x => x.FullName).HasMaxLength(150);
            builder.Property(x => x.BirthDate).HasColumnType("date");
            builder.Property(x => x.Gender).HasConversion<int?>();
            builder.Property(x => x.VisitDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.ClinicId).IsRequired();
            builder.Property(x => x.DoctorScheduleId).IsRequired();
            builder.Property(x => x.PatientType).HasConversion<int>().IsRequired();
            builder.Property(x => x.PaymentType).HasConversion<int>().IsRequired();
            builder.Property(x => x.NewInsurancePolicyNumber).HasMaxLength(50);
            builder.Property(x => x.NewInsuranceHolderName).HasMaxLength(150);
            builder.Property(x => x.NewInsuranceHolderRelationship).HasMaxLength(50);
            builder.Property(x => x.IsReferral).HasDefaultValue(false);
            builder.Property(x => x.ReferralNumber).HasMaxLength(50);
            builder.Property(x => x.ReferralDate).HasColumnType("date");
            builder.Property(x => x.ReferralDoctorName).HasMaxLength(150);
            builder.Property(x => x.ReferralDiagnosisNote).HasMaxLength(500);
            builder.Property(x => x.ReferralReason).HasMaxLength(500);
            builder.Property(x => x.HasConsent).HasDefaultValue(false);
            builder.Property(x => x.ConsentSignerType).HasConversion<int?>();
            builder.Property(x => x.ConsentSignerName).HasMaxLength(150);
            builder.Property(x => x.ConsentSignerRelationshipText).HasMaxLength(100);
            builder.Property(x => x.ConsentSignerIdentityNumber).HasMaxLength(50);
            builder.Property(x => x.ConsentSignerPhoneNumber).HasMaxLength(30);
            builder.Property(x => x.ConsentSignerAddress).HasMaxLength(500);
            builder.Property(x => x.ConsentAgreedClauses)
                .HasColumnType("integer[]")
                .HasDefaultValueSql("'{}'::integer[]")
                .IsRequired();
            builder.Property(x => x.ConsentSignatureImageBase64).HasColumnType("text");
            builder.Property(x => x.ConsentSignedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ContactWhatsAppNumber).HasMaxLength(20);
            builder.Property(x => x.ContactEmail).HasMaxLength(150);
            builder.Property(x => x.EmailDeliveryStatus)
                .HasConversion<int>()
                .HasDefaultValue(OnlineRegistrationEmailStatus.NotRequested)
                .IsRequired();
            builder.Property(x => x.EmailAttemptedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.CheckInTokenHash).HasMaxLength(64);
            builder.Property(x => x.CheckInTokenExpiresAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CheckedInAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelReason).HasMaxLength(500);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.RegistrationNumber).IsUnique();

            // KSK-DSN-PO-002: kode unik di antara pendaftaran Menunggu Check-in.
            builder.HasIndex(x => x.CodeHash)
                .IsUnique()
                .HasDatabaseName("IX_RegOnlineRegistration_CodeHash_Awaiting")
                .HasFilter("\"Status\" = 1");

            builder.HasIndex(x => x.EncounterId)
                .IsUnique()
                .HasFilter("\"EncounterId\" IS NOT NULL");

            // Hitungan kuota (INV-PO-02) dan aturan satu pendaftaran per pasien/NIK per tanggal (INV-PO-03).
            builder.HasIndex(x => new { x.DoctorScheduleId, x.VisitDate, x.Status });
            builder.HasIndex(x => new { x.PatientId, x.VisitDate, x.Status });
            builder.HasIndex(x => new { x.IdentityNumber, x.VisitDate, x.Status });
            builder.HasIndex(x => new { x.VisitDate, x.Status });

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DoctorSchedule)
                .WithMany()
                .HasForeignKey(x => x.DoctorScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientInsurance)
                .WithMany()
                .HasForeignKey(x => x.PatientInsuranceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientCompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.PatientCompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.NewInsuranceProvider)
                .WithMany()
                .HasForeignKey(x => x.NewInsuranceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReferralInstitution)
                .WithMany()
                .HasForeignKey(x => x.ReferralInstitutionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
