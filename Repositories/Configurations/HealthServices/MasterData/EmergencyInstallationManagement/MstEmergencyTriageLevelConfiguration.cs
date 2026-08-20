using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData.EmergencyInstallationManagement
{
    public class MstEmergencyTriageLevelConfiguration : IEntityTypeConfiguration<MstEmergencyTriageLevel>
    {
        public void Configure(EntityTypeBuilder<MstEmergencyTriageLevel> builder)
        {
            builder.ToTable("MstEmergencyTriageLevel", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TriageSystem).HasConversion<int>();

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.ColorName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ColorHex).HasMaxLength(20);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasCheckConstraint(
                "CK_MstEmergencyTriageLevel_Level",
                "\"Level\" >= 1 AND \"Level\" <= 5");

            builder.HasCheckConstraint(
                "CK_MstEmergencyTriageLevel_MaxWaitingMinutes",
                "\"MaxWaitingMinutes\" >= 0");

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.TriageSystem, x.Level }).IsUnique();
            builder.HasIndex(x => new { x.IsActive, x.Sequence });
        }
    }
}
