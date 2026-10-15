using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>Riwayat reschedule appointment Rawat Jalan (RJ-REG-REV-BE-005).</summary>
    public class RegEncounterRescheduleConfiguration : IEntityTypeConfiguration<RegEncounterReschedule>
    {
        public void Configure(EntityTypeBuilder<RegEncounterReschedule> entity)
        {
            entity.ToTable("RegEncounterReschedule", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OldEncounterDate).IsRequired();
            entity.Property(x => x.NewEncounterDate).IsRequired();
            entity.Property(x => x.OldQueueCode).HasMaxLength(30);
            entity.Property(x => x.NewQueueCode).HasMaxLength(30);
            entity.Property(x => x.Reason).HasMaxLength(250).IsRequired();
            entity.Property(x => x.RescheduledAt).IsRequired();
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasIndex(x => new { x.EncounterId, x.RescheduledAt });

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
