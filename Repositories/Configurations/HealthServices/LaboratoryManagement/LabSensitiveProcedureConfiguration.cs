using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    // Data dictionary 25.6. Satu penanda aktif per jenis pemeriksaan; penandaan ulang = baris baru.
    public class LabSensitiveProcedureConfiguration : IEntityTypeConfiguration<LabSensitiveProcedure>
    {
        public void Configure(EntityTypeBuilder<LabSensitiveProcedure> builder)
        {
            builder.ToTable("LabSensitiveProcedure", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Note).HasMaxLength(500);
            builder.Property(x => x.RemovalReason).HasMaxLength(500);

            builder.HasIndex(x => x.ProcedureId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Restrict: Laboratorium hanya merujuk MstProcedure milik Master Data.
            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
