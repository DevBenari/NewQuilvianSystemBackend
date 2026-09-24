using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgEncounterReconciliationItemConfiguration : IEntityTypeConfiguration<EmgEncounterReconciliationItem>
    {
        public void Configure(EntityTypeBuilder<EmgEncounterReconciliationItem> builder)
        {
            builder.ToTable("EmgEncounterReconciliationItem", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReverseSkipReason).HasMaxLength(200);

            builder.Property(x => x.Class).HasConversion<int>();
            builder.Property(x => x.StatusBefore).HasConversion<int>();
            builder.Property(x => x.StatusAfter).HasConversion<int>();

            builder.HasIndex(x => new { x.RunId, x.EncounterId }).IsUnique();
            builder.HasIndex(x => x.EncounterId);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EmergencyVisit)
                .WithMany()
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
