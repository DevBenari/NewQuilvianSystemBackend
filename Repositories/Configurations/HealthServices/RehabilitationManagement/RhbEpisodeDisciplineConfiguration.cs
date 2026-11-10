using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan disiplin yang diresepkan — <c>data-dictionary.md</c> bagian 2.4.</summary>
    public class RhbEpisodeDisciplineConfiguration : IEntityTypeConfiguration<RhbEpisodeDiscipline>
    {
        public void Configure(EntityTypeBuilder<RhbEpisodeDiscipline> builder)
        {
            builder.ToTable("RhbEpisodeDiscipline", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MedicalPrecautionText).HasMaxLength(1000);

            builder.HasIndex(x => new { x.EpisodeId, x.DisciplineId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.DisciplineId);
            builder.HasIndex(x => x.PrescribedByAssessmentId);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.Disciplines)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Discipline)
                .WithMany()
                .HasForeignKey(x => x.DisciplineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PrescribedByAssessment)
                .WithMany()
                .HasForeignKey(x => x.PrescribedByAssessmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
