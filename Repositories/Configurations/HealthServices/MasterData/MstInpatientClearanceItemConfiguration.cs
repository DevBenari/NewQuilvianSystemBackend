using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    public class MstInpatientClearanceItemConfiguration : IEntityTypeConfiguration<MstInpatientClearanceItem>
    {
        public void Configure(EntityTypeBuilder<MstInpatientClearanceItem> builder)
        {
            builder.ToTable("MstInpatientClearanceItem", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => x.ItemCode).IsUnique();
            builder.HasIndex(x => x.IsMandatory);
            builder.HasIndex(x => x.IsActive);
        }
    }
}
