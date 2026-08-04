using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.EmergencyInstallationManagement
{
    public class TrxEmergencyObservationConfiguration : IEntityTypeConfiguration<TrxEmergencyObservation>
    {
        public void Configure(EntityTypeBuilder<TrxEmergencyObservation> builder)
        {
            builder.ToTable("TrxEmergencyObservation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ObservationNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ObservationStatus).HasConversion<int>();
            builder.Property(x => x.ObservationLocation).HasMaxLength(250);
            builder.Property(x => x.Indication).HasMaxLength(1000);
            builder.Property(x => x.ObservationPlan).HasMaxLength(2000);
            builder.Property(x => x.CompletionSummary).HasMaxLength(1000);
            builder.Property(x => x.EscalationReason).HasMaxLength(1000);

            builder.HasIndex(x => x.ObservationNumber).IsUnique();
            builder.HasIndex(x => new { x.EmergencyVisitId, x.ObservationStatus, x.StartedAt });
            builder.HasIndex(x => x.ResponsibleDoctorId);

            builder.HasOne(x => x.EmergencyVisit)
                .WithMany(x => x.Observations)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ResponsibleDoctor)
                .WithMany()
                .HasForeignKey(x => x.ResponsibleDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ResponsibleNurseUser)
                .WithMany()
                .HasForeignKey(x => x.ResponsibleNurseUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Details)
                .WithOne(x => x.EmergencyObservation)
                .HasForeignKey(x => x.EmergencyObservationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ProcedureDetails)
                .WithOne(x => x.EmergencyObservation)
                .HasForeignKey(x => x.EmergencyObservationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
