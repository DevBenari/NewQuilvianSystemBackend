using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdCandidateConfiguration : IEntityTypeConfiguration<HrdCandidate>
    {
        public void Configure(EntityTypeBuilder<HrdCandidate> builder)
        {
            builder.ToTable("HrdCandidate", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CandidateCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.CandidateCode)
                .IsUnique();

            builder.Property(x => x.NationalIdNumber)
                .IsRequired()
                .HasMaxLength(16);

            builder.HasIndex(x => x.NationalIdNumber)
                .IsUnique();

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.Email);

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.PreviousWorkforceProfileId);

            builder.HasOne(x => x.PreviousWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.PreviousWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Applications)
                .WithOne(a => a.Candidate)
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Documents)
                .WithOne(d => d.Candidate)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
