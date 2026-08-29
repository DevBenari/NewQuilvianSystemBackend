using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgDispositionConfiguration : IEntityTypeConfiguration<EmgDisposition>
    {
        public void Configure(EntityTypeBuilder<EmgDisposition> builder)
        {
            builder.ToTable("EmgDisposition", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DispositionStatus).HasConversion<int>();
            builder.Property(x => x.DestinationFacilityName).HasMaxLength(250);
            builder.Property(x => x.ReferralNumber).HasMaxLength(100);
            builder.Property(x => x.DispositionReason).HasMaxLength(2000);
            builder.Property(x => x.PatientConditionAtDisposition).HasMaxLength(2000);
            builder.Property(x => x.FollowUpInstruction).HasMaxLength(2000);
            builder.Property(x => x.RefusalReason).HasMaxLength(1000);
            builder.Property(x => x.DeathLocation).HasMaxLength(250);
            builder.Property(x => x.SuspectedCauseOfDeath).HasMaxLength(1000);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => new { x.EmergencyVisitId, x.DispositionStatus, x.DecidedAt });
            builder.HasIndex(x => x.DispositionTypeId);
            builder.HasIndex(x => x.DestinationServiceUnitId);
            builder.HasIndex(x => x.DecidedByDoctorId);

            builder.HasOne(x => x.EmergencyVisit)
                .WithMany(x => x.Dispositions)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DispositionType)
                .WithMany(x => x.EmergencyDispositions)
                .HasForeignKey(x => x.DispositionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DecidedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.DecidedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ConfirmedByUser)
                .WithMany()
                .HasForeignKey(x => x.ConfirmedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DestinationServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.DestinationServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
