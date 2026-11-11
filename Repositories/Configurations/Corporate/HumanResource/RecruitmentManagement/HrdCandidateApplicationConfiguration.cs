using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdCandidateApplicationConfiguration : IEntityTypeConfiguration<HrdCandidateApplication>
    {
        public void Configure(EntityTypeBuilder<HrdCandidateApplication> builder)
        {
            builder.ToTable("HrdCandidateApplication", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ApplicationNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.ApplicationNumber)
                .IsUnique();

            builder.HasIndex(x => new { x.CandidateId, x.VacancyId })
                .IsUnique();

            builder.HasIndex(x => x.CandidateId);
            builder.HasIndex(x => x.VacancyId);
            builder.HasIndex(x => x.CurrentStage);

            builder.HasOne(x => x.Candidate)
                .WithMany(c => c.Applications)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Vacancy)
                .WithMany(v => v.Applications)
                .HasForeignKey(x => x.VacancyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Screening)
                .WithOne(s => s.Application)
                .HasForeignKey<HrdCandidateScreening>(s => s.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Evaluations)
                .WithOne(e => e.Application)
                .HasForeignKey(e => e.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Offers)
                .WithOne(o => o.Application)
                .HasForeignKey(o => o.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
