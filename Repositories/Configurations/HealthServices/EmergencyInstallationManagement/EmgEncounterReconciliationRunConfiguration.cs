using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgEncounterReconciliationRunConfiguration : IEntityTypeConfiguration<EmgEncounterReconciliationRun>
    {
        public void Configure(EntityTypeBuilder<EmgEncounterReconciliationRun> builder)
        {
            builder.ToTable("EmgEncounterReconciliationRun", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RunNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ReverseReason).HasMaxLength(500);

            builder.Property(x => x.Status).HasConversion<int>();

            builder.HasIndex(x => x.RunNumber).IsUnique();
            builder.HasIndex(x => x.ExecutedAt);

            builder.HasOne(x => x.ExecutedByUser)
                .WithMany()
                .HasForeignKey(x => x.ExecutedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReversedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReversedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Run)
                .HasForeignKey(x => x.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
