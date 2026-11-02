using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan impor booking rombongan — <c>data-dictionary.md</c> bagian 3.</summary>
    /// <remarks>Index unik <c>ImportNumber</c> tanpa filter: nomor impor tidak pernah dipakai ulang.</remarks>
    public class McuBookingImportConfiguration : IEntityTypeConfiguration<McuBookingImport>
    {
        public void Configure(EntityTypeBuilder<McuBookingImport> builder)
        {
            builder.ToTable("McuBookingImport", "public");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.ImportNumber).IsUnique();
            builder.HasIndex(x => x.CompanyGuarantorId);

            builder.HasOne(x => x.CompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.CompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Package)
                .WithMany()
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
