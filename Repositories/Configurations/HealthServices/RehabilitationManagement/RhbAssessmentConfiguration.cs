using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan asesmen rehab — <c>data-dictionary.md</c> bagian 2.5.</summary>
    /// <remarks>
    /// <b><c>UX_RhbAssessment_MedicalFinal</c></b> menjaga satu asesmen medis awal
    /// (<c>AssessmentType = 1</c>) yang final (<c>RecordStatus = 2</c>) per episode.
    /// </remarks>
    public class RhbAssessmentConfiguration : IEntityTypeConfiguration<RhbAssessment>
    {
        public void Configure(EntityTypeBuilder<RhbAssessment> builder)
        {
            builder.ToTable("RhbAssessment", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AssessmentType).HasConversion<int>().IsRequired();
            builder.Property(x => x.RecordStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.HistoryText).HasMaxLength(4000);
            builder.Property(x => x.PhysicalFunctionalFindingsText).HasMaxLength(4000);
            builder.Property(x => x.RiskAndContraindicationText).HasMaxLength(2000);
            builder.Property(x => x.ProblemOrDiagnosisText).HasMaxLength(2000);
            builder.Property(x => x.PrognosisText).HasMaxLength(1000);
            builder.Property(x => x.ConclusionText).HasMaxLength(2000);
            builder.Property(x => x.PatientGoalText).HasMaxLength(1000);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.EpisodeId)
                .IsUnique()
                .HasFilter("\"AssessmentType\" = 1 AND \"RecordStatus\" = 2 AND \"IsDelete\" = false")
                .HasDatabaseName("UX_RhbAssessment_MedicalFinal");
            builder.HasIndex(x => new { x.EpisodeId, x.AssessmentType, x.DisciplineId });
            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.AuthorUserId);
            builder.HasIndex(x => x.RecordStatus);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.Assessments)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Discipline)
                .WithMany()
                .HasForeignKey(x => x.DisciplineId)
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
