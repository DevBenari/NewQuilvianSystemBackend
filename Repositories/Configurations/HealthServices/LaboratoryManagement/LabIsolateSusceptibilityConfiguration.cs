using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabIsolateSusceptibilityConfiguration : IEntityTypeConfiguration<LabIsolateSusceptibility>
    {
        public void Configure(EntityTypeBuilder<LabIsolateSusceptibility> builder)
        {
            builder.ToTable("LabIsolateSusceptibility", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabMicrobiologyIsolateId).IsRequired();
            builder.Property(x => x.LabAntibioticId).IsRequired();

            builder.Property(x => x.AntibioticNameSnapshot).IsRequired().HasMaxLength(200);

            // Presisi ditetapkan eksplisit, mengikuti ResultNumeric pada LabExamination.
            // Bawaan Npgsql untuk decimal tanpa keterangan adalah numeric tanpa batas, dan
            // nilai MIC dibandingkan terhadap breakpoint — dua angka yang presisinya berbeda
            // akan berselisih pada pembulatan tanpa satu pun galat yang terlihat.
            builder.Property(x => x.Concentration).HasPrecision(18, 4);

            builder.Property(x => x.Result).HasConversion<int>().IsRequired();
            builder.Property(x => x.ComputedResult).HasConversion<int>();

            builder.Property(x => x.IsResultOverridden).IsRequired();
            builder.Property(x => x.ResultOverrideReason).HasMaxLength(500);
            builder.Property(x => x.Note).HasMaxLength(500);

            // Cascade dari isolatnya, dan HANYA di sini. Baris kepekaan nol punya umur sendiri:
            // ia hasil pengujian TERHADAP isolat itu, sehingga hilangnya isolat memang berarti
            // hilangnya seluruh baris kepekaannya (LAB-DA-001 A3.8).
            builder.HasOne(x => x.LabMicrobiologyIsolate)
                .WithMany(x => x.Susceptibilities)
                .HasForeignKey(x => x.LabMicrobiologyIsolateId)
                .OnDelete(DeleteBehavior.Cascade);

            // AC-108. Restrict pada data induk. Antibiotik yang masih dirujuk baris kepekaan
            // tidak boleh hilang dari bawahnya; ia dinonaktifkan, bukan dihapus (INV-31).
            builder.HasOne(x => x.LabAntibiotic)
                .WithMany()
                .HasForeignKey(x => x.LabAntibioticId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ConcentrationUnit)
                .WithMany()
                .HasForeignKey(x => x.ConcentrationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.LabMicrobiologyIsolateId);

            // AC-109 — BUTIR PALING MUDAH DILEWATKAN PADA GELOMBANG INI.
            //
            // Satu antibiotik tidak boleh diuji dua kali pada isolat yang sama. Index ini
            // WAJIB parsial: penghapusan berupa penandaan (IsDelete), sehingga index unik
            // penuh membuat baris yang sudah ditandai hapus TETAP MENEMPATI kuncinya —
            // analis yang salah memilih antibiotik, menghapusnya, lalu memilih antibiotik
            // yang benar akan ditolak database tanpa sebab yang dapat ia pahami.
            builder.HasIndex(x => new { x.LabMicrobiologyIsolateId, x.LabAntibioticId })
                .IsUnique()
                .HasDatabaseName("IX_LabIsolateSusceptibility_IsolateId_AntibioticId")
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
