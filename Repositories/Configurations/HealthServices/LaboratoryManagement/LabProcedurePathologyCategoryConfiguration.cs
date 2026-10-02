using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabProcedurePathologyCategoryConfiguration : IEntityTypeConfiguration<LabProcedurePathologyCategory>
    {
        public void Configure(EntityTypeBuilder<LabProcedurePathologyCategory> builder)
        {
            builder.ToTable("LabProcedurePathologyCategory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProcedureId).IsRequired();
            builder.Property(x => x.LabPathologyCategoryId).IsRequired();

            // Satu jenis pemeriksaan TEPAT SATU golongan. PARSIAL: penggolongan yang keliru
            // dicabut lalu dipasang ulang ke golongan lain, dan baris tertandai hapus tidak
            // boleh menghalangi pemasangan ulangnya.
            builder.HasIndex(x => x.ProcedureId)
                .IsUnique()
                .HasDatabaseName("IX_LabProcedurePathologyCategory_ProcedureId")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.LabPathologyCategoryId);

            // Restrict pada keduanya, dan pada MstProcedure itu penting: tabel ini hanya
            // MENUNJUK katalog milik modul lain, sehingga ia nol boleh ikut menghapusnya.
            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LabPathologyCategory)
                .WithMany()
                .HasForeignKey(x => x.LabPathologyCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
