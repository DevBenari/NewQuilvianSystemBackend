using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan sesi terapi — <c>data-dictionary.md</c> bagian 2.12.</summary>
    public class RhbSessionConfiguration : IEntityTypeConfiguration<RhbSession>
    {
        public void Configure(EntityTypeBuilder<RhbSession> builder)
        {
            builder.ToTable("RhbSession", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SessionNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.SessionStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.NotDoneReason).HasConversion<int?>();
            builder.Property(x => x.IdentityVerified).HasDefaultValue(false);
            builder.Property(x => x.ProgramVerified).HasDefaultValue(false);
            builder.Property(x => x.CurrentConditionText).HasMaxLength(1000);
            builder.Property(x => x.ContraindicationFound).HasDefaultValue(false);
            builder.Property(x => x.ContraindicationText).HasMaxLength(1000);
            builder.Property(x => x.ResponseText).HasMaxLength(2000);
            builder.Property(x => x.ImmediateOutcomeText).HasMaxLength(1000);
            builder.Property(x => x.DeviationType).HasConversion<int>().IsRequired();
            builder.Property(x => x.DeviationReasonText).HasMaxLength(1000);
            builder.Property(x => x.ImmediateActionText).HasMaxLength(1000);
            builder.Property(x => x.ContactedPartyText).HasMaxLength(200);
            builder.Property(x => x.RecordStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.BillingHandoffStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.BillingHandoffError).HasMaxLength(500);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.SessionNumber).IsUnique();
            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.ProgramId);
            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.TherapistUserId);
            builder.HasIndex(x => x.SessionStatus);
            builder.HasIndex(x => x.RecordStatus);
            builder.HasIndex(x => x.StartedAt);
            builder.HasIndex(x => x.BillingHandoffStatus);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Program)
                .WithMany()
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
