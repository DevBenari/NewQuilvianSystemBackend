using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class MstRadModalityConfiguration : IEntityTypeConfiguration<MstRadModality>
    {
        public void Configure(EntityTypeBuilder<MstRadModality> builder)
        {
            builder.ToTable("MstRadModality", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ModalityCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.ModalityName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => x.ModalityCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.IsActive);
        }
    }
}
