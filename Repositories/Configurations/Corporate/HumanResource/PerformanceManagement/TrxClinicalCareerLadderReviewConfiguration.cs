using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxClinicalCareerLadderReviewConfiguration : IEntityTypeConfiguration<TrxClinicalCareerLadderReview>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalCareerLadderReview> entity)
        {
            entity.ToTable("trx_clinical_career_ladder_reviews", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReviewNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CurrentClinicalLevel).HasMaxLength(20).IsRequired();
            entity.Property(x => x.TargetClinicalLevel).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ClinicalExperienceYears).HasColumnType("decimal(4,1)").IsRequired();
            entity.Property(x => x.MeetsPerformanceThreshold).HasDefaultValue(false);
            entity.Property(x => x.MeetsClinicalCompetency).HasDefaultValue(false);
            entity.Property(x => x.FreeFromSentinelIncident).HasDefaultValue(true);
            entity.Property(x => x.CommitteeRecommendation).HasMaxLength(30);
            entity.Property(x => x.FinalStatus).HasMaxLength(30).IsRequired().HasDefaultValue("Submitted");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NewSkNumber).HasMaxLength(60);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformanceAppraisal)
                .WithMany()
                .HasForeignKey(x => x.PerformanceAppraisalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByProfile)
                .WithMany()
                .HasForeignKey(x => x.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ReviewNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.WorkforceProfileId);
            entity.HasIndex(x => x.PerformanceAppraisalId);
            entity.HasIndex(x => x.FinalStatus);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxClinicalCareerLadderReview> entity)
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
