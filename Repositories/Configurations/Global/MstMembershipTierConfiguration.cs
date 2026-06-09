using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstMembershipTierConfiguration : IEntityTypeConfiguration<MstMembershipTier>
    {
        public void Configure(EntityTypeBuilder<MstMembershipTier> entity)
        {
            entity.ToTable("MstMembershipTier", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TierCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.TierName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.TierType)
                .HasConversion<int>()
                .HasDefaultValue(MembershipTierType.Regular)
                .IsRequired();

            entity.Property(x => x.CardTitle)
                .HasMaxLength(100);

            entity.Property(x => x.CardColor)
                .HasMaxLength(50);

            entity.Property(x => x.CardImagePath)
                .HasMaxLength(500);

            entity.Property(x => x.PriorityLevel)
                .HasDefaultValue(0);

            entity.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            entity.Property(x => x.IsSelectableInKiosk)
                .HasDefaultValue(false);

            entity.Property(x => x.IsSelectableInAdmission)
                .HasDefaultValue(false);

            entity.Property(x => x.IsManagedByMarketingOnly)
                .HasDefaultValue(true);

            entity.Property(x => x.RegistrationDiscountPercent)
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.ConsultationDiscountPercent)
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.ProcedureDiscountPercent)
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.LaboratoryDiscountPercent)
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.RadiologyDiscountPercent)
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.PharmacyDiscountPercent)
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.PriorityQueue)
                .HasDefaultValue(false);

            entity.Property(x => x.FreeAnnualCheckup)
                .HasDefaultValue(false);

            entity.Property(x => x.FreeParking)
                .HasDefaultValue(false);

            entity.Property(x => x.ValidityMonths)
                .HasDefaultValue(12);

            entity.Property(x => x.MinimumSpendAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.BenefitDescription)
                .HasMaxLength(500);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasIndex(x => x.TierCode)
                .IsUnique();

            entity.HasIndex(x => x.TierName);

            entity.HasIndex(x => x.TierType);

            entity.HasIndex(x => new
            {
                x.IsDefault,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsSelectableInKiosk,
                x.IsSelectableInAdmission,
                x.IsManagedByMarketingOnly,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
