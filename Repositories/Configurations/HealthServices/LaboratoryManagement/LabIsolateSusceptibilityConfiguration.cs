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

            // AC-109. Satu antibiotik tidak boleh diuji dua kali pada isolat yang sama.
            //
            // FILTERNYA PARSIAL SEBAGAI JARING PENGAMAN, BUKAN SEBAGAI KEBUTUHAN — dan
            // pembedaan itu dikoreksi 2026-09-22 (LAB-CONFLICT-011, ditutup lewat BE-LAB-49).
            //
            // Komentar sebelumnya menyatakan "penghapusan berupa penandaan (IsDelete)".
            // ITU TIDAK BERLAKU BAGI TABEL INI. LabMicrobiologyResultService mengganti seluruh
            // koleksi anak dengan RemoveRange, dan nol tempat di codebase mengubah
            // EntityState.Deleted menjadi penandaan — nol interseptor SaveChanges, nol
            // HasQueryFilter global. Baris kepekaan yang dihapus LENYAP SECARA FISIK, dan
            // BE-LAB-49 membuktikannya terhadap database sungguhan: 0 baris bertanda terhapus.
            //
            // Artinya index unik PENUH akan berperilaku sama persis pada jalur yang ada hari
            // ini. Filter ini tetap dipertahankan justru karena itu murah: bila kelak gaya
            // hapus tabel ini berubah menjadi penandaan — seperti LabPathologyCategoryService
            // dan LabPathologyReportService yang memang memakai IsDelete=true — index penuh
            // akan mulai menolak antibiotik yang sama dipilih ulang, dan kegagalannya muncul
            // di tangan analis, bukan di pipeline.
            //
            // Mencabut filter ini karena "belum terpakai" adalah cara menukar jaring pengaman
            // gratis dengan cacat yang hanya terlihat saat sudah terjadi.
            builder.HasIndex(x => new { x.LabMicrobiologyIsolateId, x.LabAntibioticId })
                .IsUnique()
                .HasDatabaseName("IX_LabIsolateSusceptibility_IsolateId_AntibioticId")
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
