using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement;

public class CliTransfusionMonitoringConfiguration : IEntityTypeConfiguration<CliTransfusionMonitoring>
{
    public void Configure(EntityTypeBuilder<CliTransfusionMonitoring> b)
    {
        b.ToTable("CliTransfusionMonitoring", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.InpEpisodeId).IsRequired();
        b.Property(x => x.EncounterId).IsRequired();
        b.Property(x => x.PatientId).IsRequired();
        b.Property(x => x.BloodUnitId).IsRequired();
        b.Property(x => x.ReceivedAtWardAt).IsRequired();
        b.Property(x => x.TransfusionStartedAt).IsRequired();
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.StopReason).HasMaxLength(500);
        b.Property(x => x.PerformedByUserId).IsRequired();
        b.Property(x => x.CancelReason).HasMaxLength(500);
        b.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models.InpEpisode>().WithMany().HasForeignKey(x => x.InpEpisodeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter>().WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models.MstPatient>().WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models.BbkBloodUnit>().WithMany().HasForeignKey(x => x.BloodUnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.PerformedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.BloodUnitId).IsUnique().HasFilter("\"Status\" <> 4");
        b.HasIndex(x => new { x.InpEpisodeId, x.Status });
    }
}

public class CliTransfusionMonitoringPointConfiguration : IEntityTypeConfiguration<CliTransfusionMonitoringPoint>
{
    public void Configure(EntityTypeBuilder<CliTransfusionMonitoringPoint> b)
    {
        b.ToTable("CliTransfusionMonitoringPoint", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.MonitoringId).IsRequired();
        b.Property(x => x.PointType).IsRequired();
        b.Property(x => x.DueAt).IsRequired();
        b.Property(x => x.TemperatureCelsius).HasPrecision(4, 1);
        b.Property(x => x.IsLate).IsRequired();
        b.Property(x => x.IsStopped).IsRequired();
        b.Property(x => x.LateNote).HasMaxLength(500);
        b.Property(x => x.RevisionNumber).IsConcurrencyToken().IsRequired();
        b.Property(x => x.CorrectionReason).HasMaxLength(500);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliTransfusionMonitoring>().WithMany().HasForeignKey(x => x.MonitoringId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.MonitoringId, x.PointType }).IsUnique();
    }
}

public class CliTransfusionReactionConfiguration : IEntityTypeConfiguration<CliTransfusionReaction>
{
    public void Configure(EntityTypeBuilder<CliTransfusionReaction> b)
    {
        b.ToTable("CliTransfusionReaction", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.MonitoringId).IsRequired();
        b.Property(x => x.OccurredAt).IsRequired();
        b.Property(x => x.ReactionSummary).HasMaxLength(250).IsRequired();
        b.Property(x => x.ReactionDetail).HasMaxLength(1000);
        b.Property(x => x.RecordedByUserId).IsRequired();
        b.Property(x => x.NoticeDelivery).IsRequired();
        b.Property(x => x.NoticeAttemptCount).IsRequired();
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliTransfusionMonitoring>().WithMany().HasForeignKey(x => x.MonitoringId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.NoticeDelivery);
    }
}
