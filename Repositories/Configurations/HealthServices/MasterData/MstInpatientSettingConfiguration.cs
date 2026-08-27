using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    public class MstInpatientSettingConfiguration : IEntityTypeConfiguration<MstInpatientSetting>
    {

        public void Configure(EntityTypeBuilder<MstInpatientSetting> builder)
        {
            builder.ToTable("MstInpatientSetting", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.EpisodeNumberPrefix)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.IsActive, x.IsDefault });
        }
    }
}
