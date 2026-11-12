using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan ringkasan akhir — <c>data-dictionary.md</c> bagian 2.16. Satu per episode.</summary>
    public class RhbDischargeSummaryConfiguration : IEntityTypeConfiguration<RhbDischargeSummary>
    {
        public void Configure(EntityTypeBuilder<RhbDischargeSummary> builder)
        {
            builder.ToTable("RhbDischargeSummary", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ClosureReason).HasConversion<int?>();
            builder.Property(x => x.MainServicesText).HasMaxLength(2000);
            builder.Property(x => x.GoalAchievementText).HasMaxLength(2000);
            builder.Property(x => x.FinalConditionText).HasMaxLength(2000);
            builder.Property(x => x.FollowUpType).HasConversion<int>().IsRequired();
            builder.Property(x => x.FollowUpInstructionText).HasMaxLength(2000);
            builder.Property(x => x.FollowUpTargetText).HasMaxLength(200);
            builder.Property(x => x.RecordStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.EpisodeId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.RecordStatus);

            builder.HasOne(x => x.Episode)
                .WithOne(x => x.DischargeSummary)
                .HasForeignKey<RhbDischargeSummary>(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
