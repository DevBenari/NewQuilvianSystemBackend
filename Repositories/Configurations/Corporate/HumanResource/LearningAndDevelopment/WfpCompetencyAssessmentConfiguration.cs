using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class WfpCompetencyAssessmentConfiguration : IEntityTypeConfiguration<WfpCompetencyAssessment>
    {
        public void Configure(EntityTypeBuilder<WfpCompetencyAssessment> entity)
        {
            entity.ToTable("WfpCompetencyAssessment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssessmentDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpiredDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Score).HasPrecision(18, 4);
            entity.Property(x => x.MaximumScore).HasPrecision(18, 4);
            entity.Property(x => x.IsVerified).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.CompetencyAssessments)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Competency)
                .WithMany()
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SourceTrainingAssessment)
                .WithMany()
                .HasForeignKey(x => x.SourceTrainingAssessmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SourceTrainingResult)
                .WithMany()
                .HasForeignKey(x => x.SourceTrainingResultId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssessedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkforceProfileId, x.CompetencyId, x.AssessmentDate });

            entity.HasIndex(x => new { x.ExpiredDate, x.IsActive });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<WfpCompetencyAssessment> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
