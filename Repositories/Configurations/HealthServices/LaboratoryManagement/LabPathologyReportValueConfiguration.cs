using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabPathologyReportValueConfiguration : IEntityTypeConfiguration<LabPathologyReportValue>
    {
        public void Configure(EntityTypeBuilder<LabPathologyReportValue> builder)
        {
            builder.ToTable("LabPathologyReportValue", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabPathologyReportId).IsRequired();
            builder.Property(x => x.LabPathologyParameterId).IsRequired();
            builder.Property(x => x.ParameterNameSnapshot).HasMaxLength(200).IsRequired();

            // RULE-011 — nol batas panjang. Ditulis eksplisit sebagai "text", bukan dibiarkan
            // mengambil bawaan, supaya batas varchar tidak menyelinap masuk lewat konvensi.
            // Uraian makroskopik sebuah reseksi memang dapat panjang, dan batas yang dipilih
            // sembarangan akan memotong diagnosis pasien di tengah kalimat.
            builder.Property(x => x.Value).HasColumnType("text").IsRequired();

            // Satu parameter muncul sekali per laporan. PARSIAL: nilai yang dicabut lalu diisi
            // ulang adalah tindakan wajar saat laporan masih ditulis, dan baris tertandai hapus
            // nol boleh menghalanginya.
            builder.HasIndex(x => new { x.LabPathologyReportId, x.LabPathologyParameterId })
                .IsUnique()
                .HasDatabaseName("IX_LabPathologyReportValue_ReportId_ParameterId")
                .HasFilter("\"IsDelete\" = false");

            // Restrict pada KEDUANYA, dan pada parameter itu yang menegakkan INV-37: data induk
            // yang sudah dipakai laporan nol boleh hilang, sebab hilangnya membuat isi laporan
            // diagnostik pasien kehilangan nama.
            builder.HasOne(x => x.LabPathologyReport)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.LabPathologyReportId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LabPathologyParameter)
                .WithMany()
                .HasForeignKey(x => x.LabPathologyParameterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
