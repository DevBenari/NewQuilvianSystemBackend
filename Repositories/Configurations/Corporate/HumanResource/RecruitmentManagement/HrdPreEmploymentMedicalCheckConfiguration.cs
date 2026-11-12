using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdPreEmploymentMedicalCheckConfiguration : IEntityTypeConfiguration<HrdPreEmploymentMedicalCheck>
    {
        public void Configure(EntityTypeBuilder<HrdPreEmploymentMedicalCheck> builder)
        {
            builder.ToTable("HrdPreEmploymentMedicalCheck", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReferralNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.ReferralNumber)
                .IsUnique();

            builder.HasIndex(x => x.OfferId)
                .IsUnique();

            builder.HasIndex(x => x.ClearanceResult);

            builder.HasOne(x => x.Offer)
                .WithOne(o => o.MedicalCheck)
                .HasForeignKey<HrdPreEmploymentMedicalCheck>(x => x.OfferId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
