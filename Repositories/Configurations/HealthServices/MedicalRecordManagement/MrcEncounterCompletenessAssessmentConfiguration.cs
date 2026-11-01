using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalRecordManagement
{
    public class MrcEncounterCompletenessAssessmentConfiguration
        : IEntityTypeConfiguration<MrcEncounterCompletenessAssessment>
    {
        public void Configure(EntityTypeBuilder<MrcEncounterCompletenessAssessment> builder)
        {
            builder.ToTable("MrcEncounterCompletenessAssessment", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Deficiencies).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb").IsRequired();
            builder.Property(x => x.ProfileFingerprint).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Version).IsConcurrencyToken();

            builder.HasIndex(x => x.EncounterId).IsUnique();
            builder.HasIndex(x => x.ClinicalClosureId).IsUnique();
            builder.HasIndex(x => x.PatientId);

            // Pemeriksaan berkala mengambil giliran menurut NextCheckAt; daftar kerja menyaring
            // keadaan lalu mengurutkan menurut waktu penilaian.
            builder.HasIndex(x => new { x.State, x.NextCheckAt });
            builder.HasIndex(x => new { x.State, x.EvaluatedAt });

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ClinicalClosure)
                .WithOne()
                .HasForeignKey<MrcEncounterCompletenessAssessment>(x => x.ClinicalClosureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
