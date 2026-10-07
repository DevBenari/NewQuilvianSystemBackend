using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement;

public class BbkTransfusionReactionNoticeConfiguration : IEntityTypeConfiguration<BbkTransfusionReactionNotice>
{
    public void Configure(EntityTypeBuilder<BbkTransfusionReactionNotice> b)
    {
        b.ToTable("BbkTransfusionReactionNotice", "public");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).IsRequired();
        b.Property(x => x.ClinicalReactionId).IsRequired();
        b.Property(x => x.BloodUnitId).IsRequired();
        b.Property(x => x.PatientId).IsRequired();
        b.Property(x => x.EncounterId).IsRequired();
        b.Property(x => x.ReactionSummarySnapshot).HasMaxLength(250).IsRequired();
        b.Property(x => x.OccurredAt).IsRequired();
        b.Property(x => x.ReceivedAt).IsRequired();
        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.AcknowledgeNote).HasMaxLength(500);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models.BbkBloodUnit>().WithMany().HasForeignKey(x => x.BloodUnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models.MstPatient>().WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter>().WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstServiceUnit>().WithMany().HasForeignKey(x => x.ServiceUnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<QuilvianSystemBackend.Models.ApplicationUser>().WithMany().HasForeignKey(x => x.AcknowledgedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.ClinicalReactionId).IsUnique();
        b.HasIndex(x => new { x.Status, x.ReceivedAt });
    }
}
