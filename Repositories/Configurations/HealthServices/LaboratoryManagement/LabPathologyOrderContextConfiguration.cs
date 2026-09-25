using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabPathologyOrderContextConfiguration : IEntityTypeConfiguration<LabPathologyOrderContext>
    {
        public void Configure(EntityTypeBuilder<LabPathologyOrderContext> builder)
        {
            builder.ToTable("LabPathologyOrderContext", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabOrderId).IsRequired();

            // Ketiga ruas naratif bertipe text tanpa batas panjang, sebab yang sama dengan
            // LabPathologyReportValue.Value: riwayat penyakit yang terpotong berubah artinya.
            builder.Property(x => x.InitialDiagnosis).HasColumnType("text");
            builder.Property(x => x.RelevantHistory).HasColumnType("text");
            builder.Property(x => x.ClinicalNote).HasColumnType("text");

            // Tanggal tanpa jam. Masa terakhir haid dicatat per hari, dan menyimpannya sebagai
            // timestamp akan memunculkan jam 00:00 yang tidak pernah dinyatakan siapa pun.
            builder.Property(x => x.LastMenstrualPeriod).HasColumnType("date");

            // Satu pesanan tepat satu konteks. PARSIAL, sebab yang sama dengan index unik
            // lainnya pada modul ini.
            builder.HasIndex(x => x.LabOrderId)
                .IsUnique()
                .HasDatabaseName("IX_LabPathologyOrderContext_LabOrderId")
                .HasFilter("\"IsDelete\" = false");

            // Restrict. Konteks klinis adalah isi rekam medis yang melekat pada pesanan.
            builder.HasOne(x => x.LabOrder)
                .WithMany()
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
