using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class TrxBenefitClaimItemConfiguration : IEntityTypeConfiguration<TrxBenefitClaimItem>
    {
        public void Configure(EntityTypeBuilder<TrxBenefitClaimItem> entity)
        {
            entity.ToTable("TrxBenefitClaimItem", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ClaimedAmount).HasPrecision(18, 2);
            entity.Property(x => x.EligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.NonEligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.PolicyLimitAmount).HasPrecision(18, 2);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.BenefitClaim)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.BenefitClaimId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.BenefitType)
                .WithMany()
                .HasForeignKey(x => x.BenefitTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BenefitClaimId, x.ItemNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.EligibilityStatus, x.IsActive });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxBenefitClaimItem> entity)
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
