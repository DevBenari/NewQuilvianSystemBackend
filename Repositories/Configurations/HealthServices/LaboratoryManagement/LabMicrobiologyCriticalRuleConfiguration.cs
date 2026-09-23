using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabMicrobiologyCriticalRuleConfiguration : IEntityTypeConfiguration<LabMicrobiologyCriticalRule>
    {
        public void Configure(EntityTypeBuilder<LabMicrobiologyCriticalRule> builder)
        {
            builder.ToTable("LabMicrobiologyCriticalRule", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SusceptibilityResult).HasConversion<int>();
            builder.Property(x => x.RuleNote).HasMaxLength(500);
            builder.Property(x => x.IsActive).IsRequired();

            // Restrict pada keduanya, dan keduanya NULLABLE — aturan boleh berlaku bagi kuman
            // apa saja atau antibiotik apa saja. Organisme maupun antibiotik yang masih
            // dirujuk aturan tidak boleh hilang dari bawahnya (INV-31).
            builder.HasOne(x => x.LabOrganism)
                .WithMany()
                .HasForeignKey(x => x.LabOrganismId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LabAntibiotic)
                .WithMany()
                .HasForeignKey(x => x.LabAntibioticId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dipakai penilai saat membaca hasil: ia menarik seluruh aturan aktif sekali,
            // lalu mencocokkannya di memori terhadap setiap baris antibiogram.
            builder.HasIndex(x => x.IsActive);

            builder.HasIndex(x => new { x.LabOrganismId, x.LabAntibioticId, x.SusceptibilityResult })
                .HasDatabaseName("IX_LabMicrobiologyCriticalRule_Kombinasi");

            // SENGAJA NOL INDEX UNIK atas kombinasinya.
            //
            // Dua aturan yang tumpang tindih bukan kesalahan: "MRSA selalu kritis" dan
            // "resisten Meropenem selalu kritis" dapat berlaku bersamaan pada satu baris, dan
            // keduanya memang benar. Penilaian kritis bersifat "cukup satu cocok", sehingga
            // tumpang tindih nol menimbulkan keraguan — berbeda dari breakpoint, yang dua
            // barisnya akan membuat hitungan bergantung baris mana yang terbaca lebih dulu.
        }
    }
}
