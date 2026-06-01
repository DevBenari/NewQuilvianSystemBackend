using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpCompetencyAssessmentConfiguration : IEntityTypeConfiguration<WfpCompetencyAssessment>
    {
        public void Configure(EntityTypeBuilder<WfpCompetencyAssessment> entity)
        {
            entity.ToTable("WfpCompetencyAssessment", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.CompetencyId)
                .IsRequired();

            entity.Property(x => x.AssessmentDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.CompetencyLevel)
                .HasConversion<int>()
                .HasDefaultValue(CompetencyLevel.None)
                .IsRequired();

            entity.Property(x => x.ResultStatus)
                .HasConversion<int>()
                .HasDefaultValue(CompetencyAssessmentResultStatus.NotAssessed)
                .IsRequired();

            entity.Property(x => x.AssessedByUserId)
                .IsRequired(false);

            entity.Property(x => x.ExpiredDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.FilePath)
                .HasMaxLength(500);

            entity.Property(x => x.FileContentType)
                .HasMaxLength(100);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.CompetencyAssessments)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Competency)
                .WithMany(x => x.CompetencyAssessments)
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssessedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.CompetencyId);

            entity.HasIndex(x => x.AssessedByUserId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.CompetencyId,
                x.AssessmentDate,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.CompetencyId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.ResultStatus,
                x.IsVerified,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ExpiredDate,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
