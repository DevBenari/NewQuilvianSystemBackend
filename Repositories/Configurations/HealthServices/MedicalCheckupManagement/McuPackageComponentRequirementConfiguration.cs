using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan persyaratan komponen paket — <c>data-dictionary.md</c> bagian 1.</summary>
    public class McuPackageComponentRequirementConfiguration : IEntityTypeConfiguration<McuPackageComponentRequirement>
    {
        public void Configure(EntityTypeBuilder<McuPackageComponentRequirement> builder)
        {
            builder.ToTable("McuPackageComponentRequirement", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsConfirmationRequired).HasDefaultValue(true).ValueGeneratedNever();

            builder.HasIndex(x => x.PackageComponentId);

            builder.HasOne(x => x.PackageComponent)
                .WithMany(x => x.Requirements)
                .HasForeignKey(x => x.PackageComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
