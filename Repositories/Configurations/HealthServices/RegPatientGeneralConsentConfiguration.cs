using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>General Consent Rawat Jalan per kunjungan (<c>RJ-REG-REV-BE-007</c>).</summary>
    public class RegPatientGeneralConsentConfiguration : IEntityTypeConfiguration<RegPatientGeneralConsent>
    {
        public void Configure(EntityTypeBuilder<RegPatientGeneralConsent> entity)
        {
            entity.ToTable("RegPatientGeneralConsent", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SignerType).IsRequired();
            entity.Property(x => x.SignerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SignerRelationshipText).HasMaxLength(100);
            entity.Property(x => x.SignerIdentityNumber).HasMaxLength(50);
            entity.Property(x => x.SignerPhoneNumber).HasMaxLength(13);
            entity.Property(x => x.SignerAddress).HasMaxLength(500);
            entity.Property(x => x.AgreedClauses).HasColumnType("integer[]").IsRequired();
            entity.Property(x => x.SignatureImageBase64).HasColumnType("text").IsRequired();
            entity.Property(x => x.SignedAt).IsRequired();

            // Satu kunjungan paling banyak punya satu consent yang hidup.
            entity.HasIndex(x => x.PatientEncounterId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PatientId);

            entity.HasOne(x => x.PatientEncounter)
                .WithMany()
                .HasForeignKey(x => x.PatientEncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
