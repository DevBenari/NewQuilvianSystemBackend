using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstBenefitTypeConfiguration : IEntityTypeConfiguration<MstBenefitType>
    {
        public void Configure(EntityTypeBuilder<MstBenefitType> entity)
        {
            entity.ToTable("MstBenefitType", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BenefitTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.BenefitTypeName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.BenefitCategory).HasMaxLength(50).HasDefaultValue("Other").IsRequired();
            entity.Property(x => x.FundingType).HasMaxLength(50).HasDefaultValue("Employer").IsRequired();
            entity.Property(x => x.IsTaxable).HasDefaultValue(false);
            entity.Property(x => x.RequiresEnrollment).HasDefaultValue(true);
            entity.Property(x => x.AllowsDependents).HasDefaultValue(false);
            entity.Property(x => x.MaximumDependents).HasDefaultValue(0);
            entity.Property(x => x.IsClaimBased).HasDefaultValue(false);
            entity.Property(x => x.RequiresEvidence).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasIndex(x => x.BenefitTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.BenefitTypeName);
            entity.HasIndex(x => new { x.BenefitCategory, x.FundingType, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.IsClaimBased, x.AllowsDependents, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);
        }
    }
}
