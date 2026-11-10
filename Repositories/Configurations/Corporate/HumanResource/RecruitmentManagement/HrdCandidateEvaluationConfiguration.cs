using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdCandidateEvaluationConfiguration : IEntityTypeConfiguration<HrdCandidateEvaluation>
    {
        public void Configure(EntityTypeBuilder<HrdCandidateEvaluation> builder)
        {
            builder.ToTable("HrdCandidateEvaluation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Score)
                .HasPrecision(5, 2);

            builder.HasIndex(x => x.ApplicationId);
            builder.HasIndex(x => x.EvaluationType);
            builder.HasIndex(x => x.EvaluatorUserId);
            builder.HasIndex(x => x.Recommendation);
            builder.HasIndex(x => x.IsClinicalVetoApplied);

            builder.HasOne(x => x.Application)
                .WithMany(a => a.Evaluations)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
