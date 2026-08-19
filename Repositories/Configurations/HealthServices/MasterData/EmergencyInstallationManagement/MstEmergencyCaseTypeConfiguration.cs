using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData.EmergencyInstallationManagement
{
    public class MstEmergencyCaseTypeConfiguration : IEntityTypeConfiguration<MstEmergencyCaseType>
    {
        public void Configure(EntityTypeBuilder<MstEmergencyCaseType> builder)
        {
            builder.ToTable("MstEmergencyCaseType", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.IsActive, x.Sequence });
        }
    }
}
