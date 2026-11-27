using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    // AGG-LAB-04 (data dictionary bagian 25.1–25.4). Seluruh FK Restrict: dokumen, versi, persetujuan,
    // dan catatan kirim adalah jejak permanen. Index unik wajib tersaring IsDelete (LAB-CONFLICT-005).
    public class LabPatientResultDocumentConfiguration : IEntityTypeConfiguration<LabPatientResultDocument>
    {
        public void Configure(EntityTypeBuilder<LabPatientResultDocument> builder)
        {
            builder.ToTable("LabPatientResultDocument", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Discipline).HasConversion<int>().IsRequired();
            builder.Property(x => x.CurrentVersionNumber).IsRequired().HasDefaultValue(1);
            builder.Property(x => x.Version).IsRequired().IsConcurrencyToken();

            // INV-70: satu dokumen per order — juga penjaga terbit ganda.
            builder.HasIndex(x => x.LabOrderId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasOne(x => x.LabOrder)
                .WithMany()
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
