using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement;

public class CliWsdDrainConfiguration : IEntityTypeConfiguration<CliWsdDrain>
{
    public void Configure(EntityTypeBuilder<CliWsdDrain> b)
    {
        b.ToTable("CliWsdDrain", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.InpEpisodeId).IsRequired();
        b.Property(x => x.EncounterId).IsRequired();
        b.Property(x => x.PatientId).IsRequired();
        b.Property(x => x.DrainLabel).HasMaxLength(50).IsRequired();
        b.Property(x => x.InsertionSite).HasMaxLength(100);
        b.Property(x => x.InsertedAt).IsRequired();
        b.Property(x => x.InitialResidualMl).HasPrecision(8, 1).IsRequired();
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.RegisteredByUserId).IsRequired();
        b.Property(x => x.CorrectionReason).HasMaxLength(500);
        b.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models.InpEpisode>().WithMany().HasForeignKey(x => x.InpEpisodeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter>().WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models.MstPatient>().WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.RemovedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.RegisteredByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.InpEpisodeId, x.DrainLabel }).IsUnique().HasFilter("\"Status\" = 1 AND \"IsDelete\" = false");
        b.HasIndex(x => x.Status);
    }
}

public class CliWsdReadingConfiguration : IEntityTypeConfiguration<CliWsdReading>
{
    public void Configure(EntityTypeBuilder<CliWsdReading> b)
    {
        b.ToTable("CliWsdReading", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.WsdDrainId).IsRequired();
        b.Property(x => x.InpEpisodeId).IsRequired();
        b.Property(x => x.PeriodStartAt).IsRequired();
        b.Property(x => x.PeriodEndAt).IsRequired();
        b.Property(x => x.PreviousResidualMl).HasPrecision(8, 1).IsRequired();
        b.Property(x => x.CurrentResidualMl).HasPrecision(8, 1).IsRequired();
        b.Property(x => x.DiscardedVolumeMl).HasPrecision(8, 1).IsRequired();
        b.Property(x => x.IncreaseMl).HasPrecision(8, 1).IsRequired();
        b.Property(x => x.FluidBalanceEntryId).IsRequired();
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.RevisionNumber).IsConcurrencyToken().IsRequired();
        b.Property(x => x.RecordedByUserId).IsRequired();
        b.Property(x => x.CancelReason).HasMaxLength(500);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliWsdDrain>().WithMany().HasForeignKey(x => x.WsdDrainId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models.InpEpisode>().WithMany().HasForeignKey(x => x.InpEpisodeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliNursingShift>().WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliFluidBalanceEntry>().WithMany().HasForeignKey(x => x.FluidBalanceEntryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.WsdDrainId, x.PeriodEndAt });
        b.HasIndex(x => x.FluidBalanceEntryId).IsUnique();
    }
}
