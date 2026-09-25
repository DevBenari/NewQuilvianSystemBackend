using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan penilaian kelayakan — <c>data-dictionary.md</c> bagian 3.3.</summary>
    public class HmdEligibilityAssessmentConfiguration : IEntityTypeConfiguration<HmdEligibilityAssessment>
    {
        public void Configure(EntityTypeBuilder<HmdEligibilityAssessment> builder)
        {
            builder.ToTable("HmdEligibilityAssessment", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Outcome).HasConversion<int>().IsRequired();
            builder.Property(x => x.IndicationSummary).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.DecisionReason).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.FollowUpInstruction).HasMaxLength(1000);

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.AssessedByDoctorId);
            builder.HasIndex(x => x.AssessedAt);
            builder.HasIndex(x => x.Outcome);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.EligibilityAssessments)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssessedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.AssessedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
