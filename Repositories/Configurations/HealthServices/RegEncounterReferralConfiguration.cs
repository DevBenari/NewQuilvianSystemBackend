using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>Rincian rujukan kunjungan (Amendment PM-B, <c>RJ-DOC-REFERRAL-001</c>).</summary>
    public class RegEncounterReferralConfiguration : IEntityTypeConfiguration<RegEncounterReferral>
    {
        public void Configure(EntityTypeBuilder<RegEncounterReferral> entity)
        {
            entity.ToTable("RegEncounterReferral", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReferralDateTime).IsRequired();
            entity.Property(x => x.TargetUnitType).IsRequired();
            entity.Property(x => x.DiagnosisNote).HasMaxLength(500);
            entity.Property(x => x.ReferralReason).HasMaxLength(1000);
            entity.Property(x => x.InstitutionIsPartnerSnapshot).HasDefaultValue(false);
            entity.Property(x => x.InstitutionCodeSnapshot).HasMaxLength(50);
            entity.Property(x => x.InstitutionNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.AgreementNumberSnapshot).HasMaxLength(100);
            entity.Property(x => x.IsComplete).HasDefaultValue(false);
            entity.Property(x => x.RowVersion).IsConcurrencyToken();

            // Satu kunjungan paling banyak punya satu rincian rujukan yang hidup.
            entity.HasIndex(x => x.PatientEncounterId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.IsComplete);

            entity.HasOne(x => x.PatientEncounter)
                .WithMany()
                .HasForeignKey(x => x.PatientEncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TargetServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.TargetServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TargetClinic)
                .WithMany()
                .HasForeignKey(x => x.TargetClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Diagnosis)
                .WithMany()
                .HasForeignKey(x => x.DiagnosisId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RegEncounterReferralDocumentConfiguration : IEntityTypeConfiguration<RegEncounterReferralDocument>
    {
        public void Configure(EntityTypeBuilder<RegEncounterReferralDocument> entity)
        {
            entity.ToTable("RegEncounterReferralDocument", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();

            entity.HasIndex(x => new { x.EncounterReferralId, x.IsDelete });

            entity.HasOne(x => x.EncounterReferral)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.EncounterReferralId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class RegEncounterReferralRevisionConfiguration : IEntityTypeConfiguration<RegEncounterReferralRevision>
    {
        public void Configure(EntityTypeBuilder<RegEncounterReferralRevision> entity)
        {
            entity.ToTable("RegEncounterReferralRevision", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OldValuesJson).HasColumnType("jsonb");
            entity.Property(x => x.NewValuesJson).HasColumnType("jsonb");
            entity.Property(x => x.ChangedAt).IsRequired();

            entity.HasIndex(x => new { x.EncounterReferralId, x.ChangedAt });

            entity.HasOne(x => x.EncounterReferral)
                .WithMany(x => x.Revisions)
                .HasForeignKey(x => x.EncounterReferralId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
