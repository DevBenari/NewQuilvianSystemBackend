using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdCandidateScreeningConfiguration : IEntityTypeConfiguration<HrdCandidateScreening>
    {
        public void Configure(EntityTypeBuilder<HrdCandidateScreening> builder)
        {
            builder.ToTable("HrdCandidateScreening", "public");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.ApplicationId)
                .IsUnique();

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.ClinicalScreeningResult);

            builder.HasOne(x => x.Application)
                .WithOne(a => a.Screening)
                .HasForeignKey<HrdCandidateScreening>(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
