using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class RadAcquisitionConsumptionConfiguration
        : IEntityTypeConfiguration<RadAcquisitionConsumption>
    {
        public void Configure(EntityTypeBuilder<RadAcquisitionConsumption> builder)
        {
            builder.ToTable("RadAcquisitionConsumption", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemType).HasConversion<int>().IsRequired();
            builder.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Quantity).HasPrecision(18, 4);
            builder.Property(x => x.Unit).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(1000);

            builder.HasIndex(x => new { x.RadStudyId, x.ItemType });
        }
    }
}
