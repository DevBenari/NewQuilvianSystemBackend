using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement.MasterData
{
    public class MstEmergencySettingConfiguration : IEntityTypeConfiguration<MstEmergencySetting>
    {
        public void Configure(EntityTypeBuilder<MstEmergencySetting> builder)
        {
            builder.ToTable("MstEmergencySetting", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.TriageSystem).HasConversion<int>();

            builder.Property(x => x.TemporaryPatientNumberPrefix)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.EmergencyVisitNumberPrefix)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasCheckConstraint(
                "CK_MstEmergencySetting_ImmediateCareLevelThreshold",
                "\"ImmediateCareLevelThreshold\" >= 1 AND \"ImmediateCareLevelThreshold\" <= 5");

            builder.HasCheckConstraint(
                "CK_MstEmergencySetting_RequireRegistrationLevel",
                "\"RequireRegistrationBeforeTreatmentFromLevel\" >= 1 AND \"RequireRegistrationBeforeTreatmentFromLevel\" <= 5");

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.IsActive, x.IsDefault });
            builder.HasIndex(x => x.DefaultEmergencyServiceUnitId);

            builder.HasOne(x => x.DefaultEmergencyServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.DefaultEmergencyServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
