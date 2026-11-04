using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdJobOfferConfiguration : IEntityTypeConfiguration<HrdJobOffer>
    {
        public void Configure(EntityTypeBuilder<HrdJobOffer> builder)
        {
            builder.ToTable("HrdJobOffer", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OfferNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.OfferNumber)
                .IsUnique();

            builder.HasIndex(x => x.ApplicationId)
                .IsUnique();

            builder.Property(x => x.OfferedBasicSalary)
                .HasPrecision(18, 2);

            builder.Property(x => x.MaximumGradeSalary)
                .HasPrecision(18, 2);

            builder.HasIndex(x => x.SalaryGradeId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.ApprovalTier);

            builder.HasOne(x => x.Application)
                .WithMany(a => a.Offers)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SalaryGrade)
                .WithMany()
                .HasForeignKey(x => x.SalaryGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ContractType)
                .WithMany()
                .HasForeignKey(x => x.ContractTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Approvals)
                .WithOne(a => a.Offer)
                .HasForeignKey(a => a.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MedicalCheck)
                .WithOne(m => m.Offer)
                .HasForeignKey<HrdPreEmploymentMedicalCheck>(m => m.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.HiringHandoff)
                .WithOne(h => h.Offer)
                .HasForeignKey<HrdCandidateHiringHandoff>(h => h.OfferId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
