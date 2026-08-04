using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.EmergencyInstallationManagement
{
    public class TrxEmergencyProcedureDetailConfiguration : IEntityTypeConfiguration<TrxEmergencyProcedureDetail>
    {
        public void Configure(EntityTypeBuilder<TrxEmergencyProcedureDetail> builder)
        {
            builder.ToTable("TrxEmergencyProcedureDetail", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DetailType).HasConversion<int>();
            builder.Property(x => x.SkinTestResult).HasMaxLength(250);
            builder.Property(x => x.TetanusToxoidResult).HasMaxLength(250);
            builder.Property(x => x.AntiTetanusSerumAmount).HasPrecision(18, 2);
            builder.Property(x => x.AntiTetanusSerumUnit).HasMaxLength(50);
            builder.Property(x => x.MedicationRoute).HasMaxLength(100);
            builder.Property(x => x.EmergencySpecificResult).HasMaxLength(1000);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => x.PatientProcedureId).IsUnique();
            builder.HasIndex(x => new { x.EmergencyVisitId, x.DetailType });
            builder.HasIndex(x => x.EmergencyResuscitationId);
            builder.HasIndex(x => x.EmergencyObservationId);

            builder.HasOne(x => x.EmergencyVisit)
                .WithMany(x => x.ProcedureDetails)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientProcedure)
                .WithMany()
                .HasForeignKey(x => x.PatientProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EmergencyResuscitation)
                .WithMany(x => x.ProcedureDetails)
                .HasForeignKey(x => x.EmergencyResuscitationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EmergencyObservation)
                .WithMany(x => x.ProcedureDetails)
                .HasForeignKey(x => x.EmergencyObservationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
