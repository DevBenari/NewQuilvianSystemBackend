using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabAntibioticConfiguration : IEntityTypeConfiguration<LabAntibiotic>
    {
        public void Configure(EntityTypeBuilder<LabAntibiotic> builder)
        {
            builder.ToTable("LabAntibiotic", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AntibioticCode).HasMaxLength(32).IsRequired();
            builder.Property(x => x.AntibioticName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(256);

            // VAL-91, parsial dengan alasan yang sama seperti LabOrganism.
            builder.HasIndex(x => x.AntibioticCode)
                .IsUnique()
                .HasDatabaseName("IX_LabAntibiotic_AntibioticCode")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.AntibioticName);

            builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        }
    }
}
