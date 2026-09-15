using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabSpecimenTypeConfiguration : IEntityTypeConfiguration<LabSpecimenType>
    {
        public void Configure(EntityTypeBuilder<LabSpecimenType> builder)
        {
            builder.ToTable("LabSpecimenType", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SpecimenTypeCode).HasMaxLength(32).IsRequired();
            builder.Property(x => x.SpecimenTypeName).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(256);

            builder.HasIndex(x => x.SpecimenTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // VAL-62 ditegakkan di basis data, bukan hanya di service. Aturan yang hanya dijaga
            // service akan bocor lewat seeder, skrip perbaikan data, atau migration berikutnya —
            // dan bocornya diam-diam.
            builder.HasIndex(x => x.IsOtherBucket)
                .IsUnique()
                .HasFilter("\"IsOtherBucket\" = true AND \"IsActive\" = true AND \"IsDelete\" = false")
                .HasDatabaseName("IX_LabSpecimenType_SingleOtherBucket");

            builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        }
    }
}
