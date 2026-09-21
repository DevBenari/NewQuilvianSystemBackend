using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabPathologyParameterConfiguration : IEntityTypeConfiguration<LabPathologyParameter>
    {
        public void Configure(EntityTypeBuilder<LabPathologyParameter> builder)
        {
            builder.ToTable("LabPathologyParameter", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ParameterCode).HasMaxLength(32).IsRequired();
            builder.Property(x => x.ParameterName).HasMaxLength(200).IsRequired();

            // VAL-101, parsial dengan alasan yang sama seperti LabPathologyCategory.
            builder.HasIndex(x => x.ParameterCode)
                .IsUnique()
                .HasDatabaseName("IX_LabPathologyParameter_ParameterCode")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.ParameterName);

            builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        }
    }
}
