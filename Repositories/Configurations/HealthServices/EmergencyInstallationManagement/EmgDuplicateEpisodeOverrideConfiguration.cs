using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgDuplicateEpisodeOverrideConfiguration : IEntityTypeConfiguration<EmgDuplicateEpisodeOverride>
    {
        public void Configure(EntityTypeBuilder<EmgDuplicateEpisodeOverride> builder)
        {
            builder.ToTable("EmgDuplicateEpisodeOverride", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_EmgDuplicateEpisodeOverride_EpisodeYangDilangkahi",
                    "\"OverriddenEncounterId\" IS NOT NULL OR \"OverriddenVisitId\" IS NOT NULL");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(x => x.EncounterId).IsUnique();
            builder.HasIndex(x => x.PatientId);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OverriddenEncounter)
                .WithMany()
                .HasForeignKey(x => x.OverriddenEncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OverriddenVisit)
                .WithMany()
                .HasForeignKey(x => x.OverriddenVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OverriddenByUser)
                .WithMany()
                .HasForeignKey(x => x.OverriddenByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
