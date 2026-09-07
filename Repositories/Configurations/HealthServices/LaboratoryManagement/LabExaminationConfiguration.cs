using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabExaminationConfiguration : IEntityTypeConfiguration<LabExamination>
    {
        public void Configure(EntityTypeBuilder<LabExamination> builder)
        {
            builder.ToTable("LabExamination", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabOrderId).IsRequired();
            builder.Property(x => x.SpecimenId).IsRequired();
            builder.Property(x => x.ProcedureId).IsRequired();

            builder.Property(x => x.ProcedureCodeSnapshot).HasMaxLength(50);
            builder.Property(x => x.ProcedureNameSnapshot).HasMaxLength(200);
            builder.Property(x => x.TariffCodeSnapshot).HasMaxLength(50);
            builder.Property(x => x.UnitPriceSnapshot).HasPrecision(18, 2);

            builder.Property(x => x.ExaminationStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.Urgency).HasConversion<int>().IsRequired();

            builder.Property(x => x.IsDuplo).IsRequired();
            builder.Property(x => x.Version).IsConcurrencyToken();

            // BR-20 dan AC-35: satu wadah menopang banyak pemeriksaan, tetapi tidak boleh
            // menopang jenis pemeriksaan yang sama dua kali. Keunikan ditegakkan database supaya
            // dua permintaan bersamaan tidak dapat sama-sama berhasil; jalur penolakan beserta
            // pesannya adalah pekerjaan endpoint pada BE-LAB-16.
            //
            // Filter IsDelete mengikuti soft delete base model audit: baris yang sudah dihapus
            // tidak boleh menghalangi pemesanan ulang jenis pemeriksaan yang sama pada wadah itu.
            //
            // Pengerjaan ganda tidak melanggar keunikan ini karena duplo adalah penanda pada satu
            // baris — IsDuplo — bukan dua baris pemeriksaan yang sama.
            builder.HasIndex(x => new { x.SpecimenId, x.ProcedureId })
                .IsUnique()
                .HasDatabaseName("IX_LabExamination_SpecimenId_ProcedureId")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.LabOrderId);
            builder.HasIndex(x => x.ExaminationStatus);
            builder.HasIndex(x => x.ChargeEligibleAt);
            builder.HasIndex(x => x.Urgency);

            // Restrict di ketiga relasi. Pemeriksaan adalah satuan yang ditagihkan, sehingga
            // pesanan, wadah, maupun data induk jenis pemeriksaan yang masih dirujuk tidak boleh
            // hilang dari bawahnya dan memutus tautan tagihan.
            builder.HasOne(x => x.LabOrder)
                .WithMany(x => x.Examinations)
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Specimen)
                .WithMany(x => x.Examinations)
                .HasForeignKey(x => x.SpecimenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            // TariffId sengaja tanpa foreign key, mengikuti LabSpecimen. Tarif milik Master
            // Data dan disimpan di sini sebagai salinan bukti nilai saat kejadian; menautkannya
            // secara fisik akan membuat penataan ulang tarif di sana menyandera baris pemeriksaan
            // yang sudah terlanjur terbentuk.
        }
    }
}
