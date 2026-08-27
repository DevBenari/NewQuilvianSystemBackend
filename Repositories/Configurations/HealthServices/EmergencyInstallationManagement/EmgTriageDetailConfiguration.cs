using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgTriageDetailConfiguration : IEntityTypeConfiguration<EmgTriageDetail>
    {
        public void Configure(EntityTypeBuilder<EmgTriageDetail> builder)
        {
            builder.ToTable("EmgTriageDetail", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IndicatorCodeSnapshot)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.IndicatorNameSnapshot)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.IndicatorGroupSnapshot).HasMaxLength(100);
            builder.Property(x => x.ObservedValue).HasMaxLength(500);
            builder.Property(x => x.Score).HasPrecision(10, 2);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => new { x.EmergencyTriageId, x.Sequence }).IsUnique();
            builder.HasIndex(x => new { x.EmergencyTriageId, x.TriageIndicatorId });
            builder.HasIndex(x => x.IsMatched);

            builder.HasOne(x => x.EmergencyTriage)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.EmergencyTriageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TriageIndicator)
                .WithMany(x => x.TriageDetails)
                .HasForeignKey(x => x.TriageIndicatorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
