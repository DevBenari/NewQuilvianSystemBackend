using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdCandidateHiringHandoffConfiguration : IEntityTypeConfiguration<HrdCandidateHiringHandoff>
    {
        public void Configure(EntityTypeBuilder<HrdCandidateHiringHandoff> builder)
        {
            builder.ToTable("HrdCandidateHiringHandoff", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.HandoffNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.HandoffNumber)
                .IsUnique();

            builder.HasIndex(x => x.OfferId)
                .IsUnique();

            builder.HasIndex(x => x.CandidateId);
            builder.HasIndex(x => x.WorkforceProfileId);
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.Offer)
                .WithOne(o => o.HiringHandoff)
                .HasForeignKey<HrdCandidateHiringHandoff>(x => x.OfferId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Candidate)
                .WithMany()
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HeadcountRequestSeat)
                .WithMany()
                .HasForeignKey(x => x.HeadcountRequestSeatId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
