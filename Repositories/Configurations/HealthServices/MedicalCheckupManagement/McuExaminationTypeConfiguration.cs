using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan jenis Pemeriksaan MCU — <c>data-dictionary.md</c> bagian 1.</summary>
    public class McuExaminationTypeConfiguration : IEntityTypeConfiguration<McuExaminationType>
    {
        public void Configure(EntityTypeBuilder<McuExaminationType> builder)
        {
            builder.ToTable("McuExaminationType", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsActive).HasDefaultValue(true).ValueGeneratedNever();

            builder.HasIndex(x => x.ExaminationTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
