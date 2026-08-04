using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.EmergencyInstallationManagement
{
    public class TrxEmergencyObservationDetailConfiguration : IEntityTypeConfiguration<TrxEmergencyObservationDetail>
    {
        public void Configure(EntityTypeBuilder<TrxEmergencyObservationDetail> builder)
        {
            builder.ToTable("TrxEmergencyObservationDetail", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ClinicalConditionSummary).HasMaxLength(2000);
            builder.Property(x => x.InterventionSummary).HasMaxLength(2000);
            builder.Property(x => x.PatientResponseSummary).HasMaxLength(2000);
            builder.Property(x => x.FluidIntakeMl).HasPrecision(18, 2);
            builder.Property(x => x.UrineOutputMl).HasPrecision(18, 2);
            builder.Property(x => x.OtherOutputMl).HasPrecision(18, 2);
            builder.Property(x => x.BleedingEstimatedMl).HasPrecision(18, 2);
            builder.Property(x => x.VomitEstimatedMl).HasPrecision(18, 2);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => new { x.EmergencyObservationId, x.RecordedAt });
            builder.HasIndex(x => x.PatientVitalSignId);
            builder.HasIndex(x => x.ProgressNoteId);
            builder.HasIndex(x => x.RecordedByUserId);

            builder.HasOne(x => x.EmergencyObservation)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.EmergencyObservationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientVitalSign)
                .WithMany()
                .HasForeignKey(x => x.PatientVitalSignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProgressNote)
                .WithMany()
                .HasForeignKey(x => x.ProgressNoteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RecordedByUser)
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
