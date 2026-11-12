using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdTalentPoolConfiguration : IEntityTypeConfiguration<HrdTalentPool>
    {
        public void Configure(EntityTypeBuilder<HrdTalentPool> builder)
        {
            builder.ToTable("HrdTalentPool", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RankingScore)
                .HasPrecision(5, 2);

            builder.HasIndex(x => x.CandidateId);
            builder.HasIndex(x => x.OriginalVacancyId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.RetentionExpiryDate);

            builder.HasOne(x => x.Candidate)
                .WithMany()
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OriginalVacancy)
                .WithMany()
                .HasForeignKey(x => x.OriginalVacancyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
