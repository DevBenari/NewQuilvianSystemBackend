using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabOrderedProcedureConfiguration : IEntityTypeConfiguration<LabOrderedProcedure>
    {
        public void Configure(EntityTypeBuilder<LabOrderedProcedure> builder)
        {
            builder.ToTable("LabOrderedProcedure", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabOrderId).IsRequired();
            builder.Property(x => x.ProcedureId).IsRequired();

            builder.Property(x => x.ProcedureCodeSnapshot).HasMaxLength(50);
            builder.Property(x => x.ProcedureNameSnapshot).HasMaxLength(200);

            builder.Property(x => x.DisciplineSnapshot).HasConversion<int>();
            builder.Property(x => x.Urgency).HasConversion<int>().IsRequired();
            builder.Property(x => x.OrderedStatus).HasConversion<int>().IsRequired();

            // Satu jenis pemeriksaan diminta sekali per pesanan. Keunikan ditegakkan database,
            // bukan hanya service, supaya dua permintaan bersamaan tidak dapat sama-sama
            // berhasil — dan supaya seeder maupun skrip perbaikan data tidak dapat menembusnya
            // diam-diam. Polanya sama dengan VAL-62 pada jenis specimen.
            //
            // Filter IsDelete mengikuti soft delete base model: baris yang sudah dihapus tidak
            // boleh menghalangi pemesanan ulang jenis pemeriksaan yang sama pada pesanan itu.
            //
            // Pengerjaan ganda tidak melanggar keunikan ini. Duplo adalah penanda pada satu baris
            // LabExamination, bukan dua baris permintaan yang sama (LAB-DEC-026).
            builder.HasIndex(x => new { x.LabOrderId, x.ProcedureId })
                .IsUnique()
                .HasDatabaseName("IX_LabOrderedProcedure_LabOrderId_ProcedureId")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.OrderedStatus);

            // Restrict di ketiga relasi. Permintaan pemeriksaan adalah bagian riwayat pesanan;
            // pesanan, katalog, maupun baris pemeriksaan yang masih dirujuk tidak boleh hilang
            // dari bawahnya dan memutus penelusurannya.
            builder.HasOne(x => x.LabOrder)
                .WithMany()
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tautan ke baris pemeriksaan yang memenuhi permintaan ini. Kosong selama belum
            // masuk wadah — itulah yang membuat "masih menunggu wadah" dapat dijawab.
            builder.HasOne(x => x.FulfilledExamination)
                .WithMany()
                .HasForeignKey(x => x.FulfilledExaminationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
