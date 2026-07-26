using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxClinicalPrivilegeAssessmentConfiguration : IEntityTypeConfiguration<TrxClinicalPrivilegeAssessment>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalPrivilegeAssessment> entity)
        {
            entity.ToTable("TrxClinicalPrivilegeAssessment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssessmentDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AssessmentScore).HasPrecision(18, 2);
            entity.Property(x => x.CompetencyResultJson).HasColumnType("jsonb");
            entity.Property(x => x.ValidUntil).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.ClinicalPrivilegeRequest)
                .WithMany(x => x.Assessments)
                .HasForeignKey(x => x.ClinicalPrivilegeRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Competency)
                .WithMany()
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssessorUser)
                .WithMany()
                .HasForeignKey(x => x.AssessorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ClinicalPrivilegeRequestId, x.AssessmentType, x.AssessorUserId });

            entity.HasIndex(x => new { x.AssessmentResult, x.ValidUntil });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxClinicalPrivilegeAssessment> entity)
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
