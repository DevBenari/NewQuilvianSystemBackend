using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.EmergencyInstallationManagement
{
    public class TrxEmergencyResuscitationConfiguration : IEntityTypeConfiguration<TrxEmergencyResuscitation>
    {
        public void Configure(EntityTypeBuilder<TrxEmergencyResuscitation> builder)
        {
            builder.ToTable("TrxEmergencyResuscitation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ResuscitationNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ResuscitationStatus).HasConversion<int>();
            builder.Property(x => x.Location).HasMaxLength(250);
            builder.Property(x => x.TriggerCondition).HasMaxLength(1000);
            builder.Property(x => x.AirwayManagementSummary).HasMaxLength(1000);
            builder.Property(x => x.BreathingManagementSummary).HasMaxLength(1000);
            builder.Property(x => x.CirculationManagementSummary).HasMaxLength(1000);
            builder.Property(x => x.NeurologicalManagementSummary).HasMaxLength(1000);
            builder.Property(x => x.OutcomeSummary).HasMaxLength(1000);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => x.ResuscitationNumber).IsUnique();
            builder.HasIndex(x => new { x.EmergencyVisitId, x.ResuscitationStatus, x.StartedAt });
            builder.HasIndex(x => x.TeamLeaderDoctorId);

            builder.HasOne(x => x.EmergencyVisit)
                .WithMany(x => x.Resuscitations)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TeamLeaderDoctor)
                .WithMany()
                .HasForeignKey(x => x.TeamLeaderDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RecordedByUser)
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ProcedureDetails)
                .WithOne(x => x.EmergencyResuscitation)
                .HasForeignKey(x => x.EmergencyResuscitationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
