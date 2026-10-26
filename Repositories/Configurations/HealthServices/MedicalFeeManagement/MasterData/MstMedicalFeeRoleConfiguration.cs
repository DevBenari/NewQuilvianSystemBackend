using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalFeeManagement.MasterData
{
    /// <summary>Pemetaan daftar peran jasa medis — <c>data-dictionary.md</c> bagian 1.</summary>
    public class MstMedicalFeeRoleConfiguration : IEntityTypeConfiguration<MstMedicalFeeRole>
    {
        public void Configure(EntityTypeBuilder<MstMedicalFeeRole> builder)
        {
            builder.ToTable("MstMedicalFeeRole", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsPrimaryRole).HasDefaultValue(false).ValueGeneratedNever();
            builder.Property(x => x.IsActive).HasDefaultValue(true).ValueGeneratedNever();

            // RoleCode unik di antara baris yang belum dihapus (validation-matrix 1.3).
            builder.HasIndex(x => x.RoleCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Satu nilai OprTeamRole dipetakan paling banyak satu peran (validation-matrix 1.5).
            // Baris tanpa pemetaan boleh banyak, jadi NULL dikecualikan dari keunikan.
            builder.HasIndex(x => x.OprTeamRoleMapping)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"OprTeamRoleMapping\" IS NOT NULL");

            builder.HasIndex(x => x.IsActive);
        }
    }
}
