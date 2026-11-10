using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan permintaan rehab — <c>data-dictionary.md</c> bagian 2.1.</summary>
    /// <remarks>
    /// <b><c>CK_RhbRequest_Source</c></b>: rujukan eksternal (<c>SourceType = 4</c>) tidak punya
    /// kunjungan asal; sumber lain wajib punya. <c>InternalReferralLetterId</c> sengaja tanpa FK
    /// fisik (rujukan logis ke <c>CliDoctorCertificate</c>).
    /// </remarks>
    public class RhbRequestConfiguration : IEntityTypeConfiguration<RhbRequest>
    {
        public void Configure(EntityTypeBuilder<RhbRequest> builder)
        {
            builder.ToTable("RhbRequest", "public", t => t.HasCheckConstraint(
                "CK_RhbRequest_Source",
                "(\"SourceType\" = 4) = (\"SourceEncounterId\" IS NULL)"));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.SourceType).HasConversion<int>().IsRequired();
            builder.Property(x => x.RequestStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.ExternalInstitutionName).HasMaxLength(200);
            builder.Property(x => x.ExternalReferralNumber).HasMaxLength(100);
            builder.Property(x => x.RequestingPractitionerName).HasMaxLength(150);
            builder.Property(x => x.IndicationText).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.DuplicateOverrideReason).HasMaxLength(500);
            builder.Property(x => x.ReturnReason).HasMaxLength(500);
            builder.Property(x => x.DispositionReason).HasMaxLength(1000);
            builder.Property(x => x.RedirectTargetText).HasMaxLength(200);
            builder.Property(x => x.CancelReason).HasMaxLength(500);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.RequestNumber).IsUnique();
            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.SourceType);
            builder.HasIndex(x => x.RequestStatus);
            builder.HasIndex(x => x.RequestedAt);
            builder.HasIndex(x => x.SourceEncounterId);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.SourceEncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstDoctor>()
                .WithMany()
                .HasForeignKey(x => x.RequestingDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
