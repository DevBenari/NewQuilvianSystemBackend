using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan reasesmen — <c>data-dictionary.md</c> bagian 2.14.</summary>
    public class RhbProgressReviewConfiguration : IEntityTypeConfiguration<RhbProgressReview>
    {
        public void Configure(EntityTypeBuilder<RhbProgressReview> builder)
        {
            builder.ToTable("RhbProgressReview", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProgressSummaryText).HasMaxLength(4000);
            builder.Property(x => x.BarrierText).HasMaxLength(1000);
            builder.Property(x => x.InstrumentChangeNote).HasMaxLength(500);
            builder.Property(x => x.Decision).HasConversion<int?>();
            builder.Property(x => x.DecisionReasonText).HasMaxLength(1000);
            builder.Property(x => x.ReferralTargetText).HasMaxLength(200);
            builder.Property(x => x.RecordStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => new { x.EpisodeId, x.DisciplineId });
            builder.HasIndex(x => x.ProgramId);
            builder.HasIndex(x => x.RecordStatus);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.ProgressReviews)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Discipline)
                .WithMany()
                .HasForeignKey(x => x.DisciplineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Program)
                .WithMany()
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
