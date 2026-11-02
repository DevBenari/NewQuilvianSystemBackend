using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan kategori kelayakan — <c>data-dictionary.md</c> bagian 1.</summary>
    public class McuFitnessCategoryConfiguration : IEntityTypeConfiguration<McuFitnessCategory>
    {
        public void Configure(EntityTypeBuilder<McuFitnessCategory> builder)
        {
            builder.ToTable("McuFitnessCategory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsActive).HasDefaultValue(true).ValueGeneratedNever();

            builder.HasIndex(x => x.FitnessCategoryCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_McuFitnessCategory_Code");
        }
    }
}
