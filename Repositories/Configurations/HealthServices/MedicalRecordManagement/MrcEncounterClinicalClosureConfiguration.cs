using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalRecordManagement
{
    public class MrcEncounterClinicalClosureConfiguration
        : IEntityTypeConfiguration<MrcEncounterClinicalClosure>
    {
        public void Configure(EntityTypeBuilder<MrcEncounterClinicalClosure> builder)
        {
            builder.ToTable("MrcEncounterClinicalClosure", "public");

            builder.HasKey(x => x.Id);

            // Satu kunjungan, satu penutupan klinis (`AGG-RM-02`). Tabrakan index ini pada dua
            // penutupan serentak diperlakukan pencatat sebagai "sudah tercatat" (`NFR-002`).
            builder.HasIndex(x => x.EncounterId).IsUnique();
            builder.HasIndex(x => x.PatientId);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
