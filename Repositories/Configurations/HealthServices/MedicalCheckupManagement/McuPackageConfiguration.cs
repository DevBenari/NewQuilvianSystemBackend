using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan paket MCU — <c>data-dictionary.md</c> bagian 1.</summary>
    public class McuPackageConfiguration : IEntityTypeConfiguration<McuPackage>
    {
        public void Configure(EntityTypeBuilder<McuPackage> builder)
        {
            builder.ToTable("McuPackage", "public");
            builder.HasKey(x => x.Id);

            // Tanpa ValueGeneratedNever, false adalah nilai "belum diisi" bagi EF dan basis data
            // mengisinya true — paket yang sengaja dibuat nonaktif tersimpan aktif.
            builder.Property(x => x.IsActive).HasDefaultValue(true).ValueGeneratedNever();

            builder.HasIndex(x => x.PackageCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasOne(x => x.GeneralTariff)
                .WithMany()
                .HasForeignKey(x => x.GeneralTariffId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
