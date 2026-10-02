using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabSusceptibilityBreakpointConfiguration : IEntityTypeConfiguration<LabSusceptibilityBreakpoint>
    {
        public void Configure(EntityTypeBuilder<LabSusceptibilityBreakpoint> builder)
        {
            builder.ToTable("LabSusceptibilityBreakpoint", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabOrganismId).IsRequired();
            builder.Property(x => x.LabAntibioticId).IsRequired();
            builder.Property(x => x.LowerMm).IsRequired();
            builder.Property(x => x.UpperMm).IsRequired();
            builder.Property(x => x.GuidelineVersion).HasMaxLength(100);
            builder.Property(x => x.IsActive).IsRequired();

            // Restrict pada keduanya. Rentang breakpoint adalah aturan keselamatan yang
            // menunjuk data induk; organisme maupun antibiotik yang masih dirujuk tidak boleh
            // hilang dari bawahnya. Keduanya dinonaktifkan, bukan dihapus (INV-31).
            builder.HasOne(x => x.LabOrganism)
                .WithMany()
                .HasForeignKey(x => x.LabOrganismId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LabAntibiotic)
                .WithMany()
                .HasForeignKey(x => x.LabAntibioticId)
                .OnDelete(DeleteBehavior.Restrict);

            // VAL-119. Satu pasangan organisme dan antibiotik hanya boleh punya SATU rentang
            // aktif — dua rentang aktif atas pasangan yang sama membuat hitungan interpretasi
            // bergantung baris mana yang kebetulan terbaca lebih dulu.
            //
            // PARSIAL atas IsDelete, mengikuti pola yang sama dengan kedua tabel isolat:
            // penghapusan berupa penandaan, sehingga index unik penuh membuat pasangan yang
            // sudah ditandai hapus tetap menempati kuncinya.
            builder.HasIndex(x => new { x.LabOrganismId, x.LabAntibioticId })
                .IsUnique()
                .HasDatabaseName("IX_LabSusceptibilityBreakpoint_OrganismId_AntibioticId")
                .HasFilter("\"IsDelete\" = false");

            // Dipakai penghitung interpretasi (BE-LAB-61) saat mencari rentang yang berlaku.
            builder.HasIndex(x => x.IsActive);
        }
    }
}
