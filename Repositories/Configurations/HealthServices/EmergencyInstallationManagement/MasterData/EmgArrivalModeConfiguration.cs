using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement.MasterData
{
    public class EmgArrivalModeConfiguration : IEntityTypeConfiguration<EmgArrivalMode>
    {
        public void Configure(EntityTypeBuilder<EmgArrivalMode> builder)
        {
            builder.ToTable("EmgArrivalMode", "public");

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
            builder.HasIndex(x => new { x.IsAmbulance, x.IsReferral });
        }
    }
}
