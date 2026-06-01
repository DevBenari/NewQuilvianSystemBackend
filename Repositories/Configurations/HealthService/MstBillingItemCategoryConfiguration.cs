using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstBillingItemCategoryConfiguration : IEntityTypeConfiguration<MstBillingItemCategory>
    {
        public void Configure(EntityTypeBuilder<MstBillingItemCategory> entity)
        {
            entity.ToTable("MstBillingItemCategory", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.BillingItemCategoryCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.BillingItemCategoryName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.BillingGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.ItemSourceType)
                .HasMaxLength(50)
                .HasDefaultValue("Manual")
                .IsRequired();

            entity.Property(x => x.IsRegistrationFee)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAdministrationFee)
                .HasDefaultValue(false);

            entity.Property(x => x.IsConsultationFee)
                .HasDefaultValue(false);

            entity.Property(x => x.IsRoomCharge)
                .HasDefaultValue(false);

            entity.Property(x => x.IsProcedure)
                .HasDefaultValue(false);

            entity.Property(x => x.IsLaboratory)
                .HasDefaultValue(false);

            entity.Property(x => x.IsRadiology)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPharmacy)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDrug)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPackage)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDiscount)
                .HasDefaultValue(false);

            entity.Property(x => x.IsTax)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDeposit)
                .HasDefaultValue(false);

            entity.Property(x => x.IsRefund)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCoveredByInsuranceDefault)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedDoctor)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedApproval)
                .HasDefaultValue(false);

            entity.Property(x => x.IsEditableInBilling)
                .HasDefaultValue(true);

            entity.Property(x => x.IsSystemCategory)
                .HasDefaultValue(false);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

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

            entity.HasIndex(x => x.BillingItemCategoryCode)
                .IsUnique();

            entity.HasIndex(x => x.BillingItemCategoryName);

            entity.HasIndex(x => x.BillingGroupName);

            entity.HasIndex(x => x.ItemSourceType);

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ItemSourceType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsRegistrationFee,
                x.IsAdministrationFee,
                x.IsConsultationFee,
                x.IsRoomCharge,
                x.IsProcedure,
                x.IsLaboratory,
                x.IsRadiology,
                x.IsPharmacy,
                x.IsDrug,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsCoveredByInsuranceDefault,
                x.IsNeedApproval,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
