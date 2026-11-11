using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdJobOfferApprovalConfiguration : IEntityTypeConfiguration<HrdJobOfferApproval>
    {
        public void Configure(EntityTypeBuilder<HrdJobOfferApproval> builder)
        {
            builder.ToTable("HrdJobOfferApproval", "public");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.OfferId);
            builder.HasIndex(x => x.ApproverUserId);
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.Offer)
                .WithMany(o => o.Approvals)
                .HasForeignKey(x => x.OfferId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
