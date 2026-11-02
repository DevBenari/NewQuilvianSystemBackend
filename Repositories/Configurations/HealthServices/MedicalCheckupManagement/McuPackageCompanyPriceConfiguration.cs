using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan harga paket per perusahaan — <c>data-dictionary.md</c> bagian 1.</summary>
    /// <remarks>
    /// Tumpang tindih rentang tanggal (<c>VAL-MCU-05</c>) dijaga service, bukan index: index unik tidak
    /// dapat mengungkapkan "rentang tidak beririsan".
    /// </remarks>
    public class McuPackageCompanyPriceConfiguration : IEntityTypeConfiguration<McuPackageCompanyPrice>
    {
        public void Configure(EntityTypeBuilder<McuPackageCompanyPrice> builder)
        {
            builder.ToTable("McuPackageCompanyPrice", "public");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.PackageId, x.CompanyGuarantorId });
            builder.HasIndex(x => x.TariffId);

            builder.HasOne(x => x.Package)
                .WithMany(x => x.CompanyPrices)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.CompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Tariff)
                .WithMany()
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
