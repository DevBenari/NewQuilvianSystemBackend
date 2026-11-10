using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan tujuan rehab — <c>data-dictionary.md</c> bagian 2.7.</summary>
    public class RhbGoalConfiguration : IEntityTypeConfiguration<RhbGoal>
    {
        public void Configure(EntityTypeBuilder<RhbGoal> builder)
        {
            builder.ToTable("RhbGoal", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Term).HasConversion<int>().IsRequired();
            builder.Property(x => x.GoalStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.DescriptionText).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.BaselineText).HasMaxLength(500).IsRequired();
            builder.Property(x => x.TargetText).HasMaxLength(500).IsRequired();
            builder.Property(x => x.AgreedWithPatient).HasDefaultValue(false);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => new { x.EpisodeId, x.DisciplineId, x.GoalStatus });
            builder.HasIndex(x => x.SupersedesGoalId);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.Goals)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Discipline)
                .WithMany()
                .HasForeignKey(x => x.DisciplineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SupersedesGoal)
                .WithMany()
                .HasForeignKey(x => x.SupersedesGoalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
