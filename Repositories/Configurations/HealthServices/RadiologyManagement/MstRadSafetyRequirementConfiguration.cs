using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class MstRadSafetyRequirementConfiguration
        : IEntityTypeConfiguration<MstRadSafetyRequirement>
    {
        public void Configure(EntityTypeBuilder<MstRadSafetyRequirement> builder)
        {
            builder.ToTable("MstRadSafetyRequirement", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequirementCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequirementName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Category).HasMaxLength(50);
            builder.Property(x => x.SourceNote).HasMaxLength(500);

            builder.HasIndex(x => x.RequirementCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.Category, x.SortOrder });
        }
    }
}
