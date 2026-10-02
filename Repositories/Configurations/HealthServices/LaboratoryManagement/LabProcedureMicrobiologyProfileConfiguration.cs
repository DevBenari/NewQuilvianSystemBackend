using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabProcedureMicrobiologyProfileConfiguration : IEntityTypeConfiguration<LabProcedureMicrobiologyProfile>
    {
        public void Configure(EntityTypeBuilder<LabProcedureMicrobiologyProfile> builder)
        {
            builder.ToTable("LabProcedureMicrobiologyProfile", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProcedureId).IsRequired();
            builder.Property(x => x.UsesSusceptibilitySet).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();

            builder.Property(x => x.DefaultCultureType).HasConversion<int>();
            builder.Property(x => x.DefaultSusceptibilityMethod).HasConversion<int>();

            // Satu pemeriksaan katalog punya paling banyak SATU profil. Parsial atas IsDelete,
            // mengikuti LabProcedurePathologyCategory: penghapusan berupa penandaan, sehingga
            // index unik penuh membuat pemeriksaan yang profilnya pernah dihapus nol dapat
            // diprofilkan ulang.
            builder.HasIndex(x => x.ProcedureId)
                .IsUnique()
                .HasDatabaseName("IX_LabProcedureMicrobiologyProfile_ProcedureId")
                .HasFilter("\"IsDelete\" = false");

            // Restrict. Pemeriksaan katalog milik master-data; profil ini menunjuk kepadanya
            // dan nol boleh ikut menghapusnya.
            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
