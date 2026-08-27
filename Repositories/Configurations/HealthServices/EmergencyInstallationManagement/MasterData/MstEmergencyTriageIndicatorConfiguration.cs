using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement.MasterData
{
    public class MstEmergencyTriageIndicatorConfiguration : IEntityTypeConfiguration<MstEmergencyTriageIndicator>
    {
        public void Configure(EntityTypeBuilder<MstEmergencyTriageIndicator> builder)
        {
            builder.ToTable("MstEmergencyTriageIndicator", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.IndicatorGroup).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.TriageLevelId, x.Sequence }).IsUnique();
            builder.HasIndex(x => new { x.TriageLevelId, x.IsActive });

            builder.HasOne(x => x.TriageLevel)
                .WithMany(x => x.Indicators)
                .HasForeignKey(x => x.TriageLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
