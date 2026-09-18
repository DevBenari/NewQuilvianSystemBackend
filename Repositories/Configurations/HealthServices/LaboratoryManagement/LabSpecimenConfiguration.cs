using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    // Konfigurasi LabOrder sendiri sudah ada sejak sebelum RJ-BIL-BE-003 pada
    // Repositories/Configurations/HealthServices/LabOrderConfiguration.cs dan diperluas di
    // sana, bukan diduplikasi di sini. ApplyConfigurationsFromAssembly menerapkan seluruh
    // IEntityTypeConfiguration yang ditemukannya, sehingga dua konfigurasi untuk entity yang
    // sama hanya akan menyulitkan penelusuran.

    public class LabSpecimenConfiguration : IEntityTypeConfiguration<LabSpecimen>
    {
        public void Configure(EntityTypeBuilder<LabSpecimen> builder)
        {
            builder.ToTable("LabSpecimen", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SpecimenBarcode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.SpecimenDescription).HasMaxLength(200);
            builder.Property(x => x.SpecimenStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusBeforeHold).HasConversion<int>();
            builder.Property(x => x.RejectionReasonCode).HasMaxLength(50);
            builder.Property(x => x.RejectionNote).HasMaxLength(1000);
            builder.Property(x => x.RecollectionCause).HasConversion<int>();
            builder.Property(x => x.RecollectionReason).HasMaxLength(1000);
            builder.Property(x => x.Version).IsConcurrencyToken();

            builder.Property(x => x.SpecimenTypeOtherNote).HasMaxLength(128);

            // numeric(12,3). Presisinya ditetapkan di sini, bukan dibiarkan pada nilai bawaan
            // provider, supaya volume 0,5 mL tersimpan apa adanya dan tidak dibulatkan diam-diam.
            builder.Property(x => x.VolumeAmount).HasPrecision(12, 3);

            // Barcode wajib unik. Keunikan ditegakkan database, bukan hanya pemeriksaan di
            // service, agar dua permintaan bersamaan tidak dapat menghasilkan barcode kembar.
            builder.HasIndex(x => x.SpecimenBarcode).IsUnique();

            builder.HasIndex(x => new { x.LabOrderId, x.SpecimenSequence });
            builder.HasIndex(x => x.SpecimenStatus);

            // Laporan penerimaan harian menyaring menurut waktu nyata, bukan waktu sistem
            // (AC-67), sehingga kolom inilah yang dipakai sebagai rentang tanggalnya.
            builder.HasIndex(x => x.PhysicallyReceivedAt);

            // Relasi ke LabOrder dideklarasikan dari sisi LabOrderConfiguration agar hanya ada
            // satu tempat yang mendefinisikannya.

            // Relasi ke MstProcedure dihapus BE-LAB-11 bersama keenam kolom salinan tarif.
            // Jenis pemeriksaan melekat pada LabExamination, dan relasinya dideklarasikan di
            // LabExaminationConfiguration.

            builder.HasOne(x => x.RejectionReason)
                .WithMany()
                .HasForeignKey(x => x.RejectionReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Jenis specimen dan satuan volume, keduanya Restrict — LAB-DEC-040 dan
            // LAB-DEC-041. Baris data induk yang sudah menempel pada wadah tidak boleh dapat
            // dihapus, karena wadah lama akan kehilangan keterangan jenis maupun satuannya dan
            // riwayat penerimaan menjadi tidak terbaca.
            //
            // Index atas kedua foreign key dibentuk konvensi EF dengan nama
            // IX_LabSpecimen_SpecimenTypeId dan IX_LabSpecimen_VolumeUnitId, sama seperti
            // RejectionReasonId di atas; keduanya tidak dideklarasikan ulang di sini.
            builder.HasOne(x => x.SpecimenType)
                .WithMany()
                .HasForeignKey(x => x.SpecimenTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // MstMeasurement milik master-data. Laboratorium hanya menunjuk barisnya; tidak ada
            // satu pun kolom yang ditambahkan ke tabel itu.
            builder.HasOne(x => x.VolumeUnit)
                .WithMany()
                .HasForeignKey(x => x.VolumeUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rantai pengambilan ulang. Restrict memastikan sampel yang ditolak tidak dapat
            // dihapus selama masih menjadi asal-usul sampel penggantinya.
            builder.HasOne(x => x.SupersededSpecimen)
                .WithMany()
                .HasForeignKey(x => x.SupersededSpecimenId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
