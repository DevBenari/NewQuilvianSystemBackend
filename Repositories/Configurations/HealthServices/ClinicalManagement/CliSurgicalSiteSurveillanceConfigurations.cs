using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement;

public class CliSurgicalSiteSurveillanceConfiguration : IEntityTypeConfiguration<CliSurgicalSiteSurveillance>
{
    public void Configure(EntityTypeBuilder<CliSurgicalSiteSurveillance> b)
    {
        b.ToTable("CliSurgicalSiteSurveillance", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.OprCaseId).IsRequired();
        b.Property(x => x.InpEpisodeId).IsRequired();
        b.Property(x => x.EncounterId).IsRequired();
        b.Property(x => x.PatientId).IsRequired();
        b.Property(x => x.InstrumentVersionId).IsRequired();
        b.Property(x => x.SurgeryCompletedAt).IsRequired();
        b.Property(x => x.DayOneDate).IsRequired();
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.SummaryResponsesJson).HasColumnType("text").IsRequired();
        b.Property(x => x.ReviewNote).HasMaxLength(1000);
        b.Property(x => x.CancelReason).HasMaxLength(500);
        b.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models.OprCase>().WithMany().HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models.InpEpisode>().WithMany().HasForeignKey(x => x.InpEpisodeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter>().WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models.MstPatient>().WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliClinicalInstrumentVersion>().WithMany().HasForeignKey(x => x.InstrumentVersionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.TrxNosocomialInfection>().WithMany().HasForeignKey(x => x.NosocomialInfectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.SuspectedFlaggedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.OprCaseId).IsUnique();
        b.HasIndex(x => new { x.InpEpisodeId, x.Status });
    }
}

public class CliSurgicalSiteSurveillanceEntryConfiguration : IEntityTypeConfiguration<CliSurgicalSiteSurveillanceEntry>
{
    public void Configure(EntityTypeBuilder<CliSurgicalSiteSurveillanceEntry> b)
    {
        b.ToTable("CliSurgicalSiteSurveillanceEntry", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.SurveillanceId).IsRequired();
        b.Property(x => x.DayNumber).IsRequired();
        b.Property(x => x.EntryDate).IsRequired();
        b.Property(x => x.ResponsesJson).HasColumnType("text").IsRequired();
        b.Property(x => x.TemperatureMaxCelsiusSnapshot).HasPrecision(4, 1);
        b.Property(x => x.RecordedByUserId).IsRequired();
        b.Property(x => x.RevisionNumber).IsConcurrencyToken().IsRequired();
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliSurgicalSiteSurveillance>().WithMany().HasForeignKey(x => x.SurveillanceId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.SurveillanceId, x.DayNumber }).IsUnique();
    }
}

public class CliSurgicalSiteSurveillanceEntryRevisionConfiguration : IEntityTypeConfiguration<CliSurgicalSiteSurveillanceEntryRevision>
{
    public void Configure(EntityTypeBuilder<CliSurgicalSiteSurveillanceEntryRevision> b)
    {
        b.ToTable("CliSurgicalSiteSurveillanceEntryRevision", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.EntryId).IsRequired();
        b.Property(x => x.RevisionNumber).IsConcurrencyToken().IsRequired();
        b.Property(x => x.PreviousResponsesJson).HasColumnType("text").IsRequired();
        b.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        b.Property(x => x.RevisedByUserId).IsRequired();
        b.Property(x => x.RevisedAt).IsRequired();
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliSurgicalSiteSurveillanceEntry>().WithMany().HasForeignKey(x => x.EntryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.RevisedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.EntryId, x.RevisionNumber }).IsUnique();
    }
}
