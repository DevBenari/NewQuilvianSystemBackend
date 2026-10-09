using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement;

public class CliEquipmentUsageConfiguration : IEntityTypeConfiguration<CliEquipmentUsage>
{
    public void Configure(EntityTypeBuilder<CliEquipmentUsage> b)
    {
        b.ToTable("CliEquipmentUsage", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.InpEpisodeId).IsRequired();
        b.Property(x => x.EncounterId).IsRequired();
        b.Property(x => x.PatientId).IsRequired();
        b.Property(x => x.MedicalEquipmentId).IsRequired();
        b.Property(x => x.ResponsibleDoctorId).IsRequired();
        b.Property(x => x.PerformedByUserId).IsRequired();
        b.Property(x => x.StartedAt).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(10, 2);
        b.Property(x => x.ChargeUnitSnapshot).IsRequired();
        b.Property(x => x.RoundingRuleSnapshot).IsRequired();
        b.Property(x => x.BilledUnits).HasPrecision(10, 2);
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.RequiresNurseReview).IsRequired();
        b.Property(x => x.CancelReason).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(500);
        b.Property(x => x.RevisionNumber).IsConcurrencyToken().IsRequired();
        b.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models.InpEpisode>().WithMany().HasForeignKey(x => x.InpEpisodeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter>().WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models.MstPatient>().WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstMedicalEquipment>().WithMany().HasForeignKey(x => x.MedicalEquipmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstDoctor>().WithMany().HasForeignKey(x => x.ResponsibleDoctorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.PerformedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.InpEpisodeId).HasFilter("\"Status\" = 1");
    }
}

public class CliEquipmentUsageRevisionConfiguration : IEntityTypeConfiguration<CliEquipmentUsageRevision>
{
    public void Configure(EntityTypeBuilder<CliEquipmentUsageRevision> b)
    {
        b.ToTable("CliEquipmentUsageRevision", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.EquipmentUsageId).IsRequired();
        b.Property(x => x.RevisionNumber).IsConcurrencyToken().IsRequired();
        b.Property(x => x.PreviousStartedAt).IsRequired();
        b.Property(x => x.PreviousQuantity).HasPrecision(10, 2);
        b.Property(x => x.PreviousBilledUnits).HasPrecision(10, 2);
        b.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        b.Property(x => x.RevisedByUserId).IsRequired();
        b.Property(x => x.RevisedAt).IsRequired();
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliEquipmentUsage>().WithMany().HasForeignKey(x => x.EquipmentUsageId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.RevisedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.EquipmentUsageId, x.RevisionNumber }).IsUnique();
    }
}
