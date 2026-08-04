using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.EmergencyInstallationManagement
{
    public class TrxEmergencyTriageConfiguration : IEntityTypeConfiguration<TrxEmergencyTriage>
    {
        public void Configure(EntityTypeBuilder<TrxEmergencyTriage> builder)
        {
            builder.ToTable("TrxEmergencyTriage", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TriageSystem).HasConversion<int>();
            builder.Property(x => x.TriageStatus).HasConversion<int>();

            builder.Property(x => x.TriageReason).HasMaxLength(1000);
            builder.Property(x => x.AirwaySummary).HasMaxLength(1000);
            builder.Property(x => x.BreathingSummary).HasMaxLength(1000);
            builder.Property(x => x.CirculationSummary).HasMaxLength(1000);
            builder.Property(x => x.DisabilitySummary).HasMaxLength(1000);
            builder.Property(x => x.ExposureSummary).HasMaxLength(1000);
            builder.Property(x => x.RedFlagSummary).HasMaxLength(1000);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => new { x.EmergencyVisitId, x.Sequence }).IsUnique();
            builder.HasIndex(x => new { x.EmergencyVisitId, x.TriageStatus, x.StartedAt });
            builder.HasIndex(x => x.PatientVitalSignId);
            builder.HasIndex(x => x.PreviousTriageId);
            builder.HasIndex(x => x.ResponseDueAt);

            builder.HasOne(x => x.EmergencyVisit)
                .WithMany(x => x.Triages)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TriageLevel)
                .WithMany(x => x.Triages)
                .HasForeignKey(x => x.TriageLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientVitalSign)
                .WithMany()
                .HasForeignKey(x => x.PatientVitalSignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreviousTriage)
                .WithMany(x => x.Retriages)
                .HasForeignKey(x => x.PreviousTriageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PerformedByUser)
                .WithMany()
                .HasForeignKey(x => x.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReviewedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Details)
                .WithOne(x => x.EmergencyTriage)
                .HasForeignKey(x => x.EmergencyTriageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
