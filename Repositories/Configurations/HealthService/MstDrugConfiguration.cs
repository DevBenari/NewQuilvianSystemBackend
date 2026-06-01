using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDrugConfiguration : IEntityTypeConfiguration<MstDrug>
    {
        public void Configure(EntityTypeBuilder<MstDrug> entity)
        {
            entity.ToTable("MstDrug", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DrugCategoryId)
                .IsRequired();

            entity.Property(x => x.DrugCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DrugName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.GenericName)
                .HasMaxLength(200);

            entity.Property(x => x.BrandName)
                .HasMaxLength(200);

            entity.Property(x => x.ManufacturerName)
                .HasMaxLength(100);

            entity.Property(x => x.DrugForm)
                .HasMaxLength(100);

            entity.Property(x => x.Strength)
                .HasMaxLength(100);

            entity.Property(x => x.StrengthValue)
                .HasColumnType("numeric(18,6)")
                .IsRequired(false);

            entity.Property(x => x.StrengthMeasurementId)
                .IsRequired(false);

            entity.Property(x => x.BaseUnit)
                .HasMaxLength(50);

            entity.Property(x => x.DispenseUnit)
                .HasMaxLength(50);

            entity.Property(x => x.BaseUnitMeasurementId)
                .IsRequired(false);

            entity.Property(x => x.DispenseUnitMeasurementId)
                .IsRequired(false);

            entity.Property(x => x.PurchaseUnitMeasurementId)
                .IsRequired(false);

            entity.Property(x => x.StockUnitMeasurementId)
                .IsRequired(false);

            entity.Property(x => x.DefaultDoseUnitMeasurementId)
                .IsRequired(false);

            entity.Property(x => x.Route)
                .HasMaxLength(50);

            entity.Property(x => x.IsFormulary)
                .HasDefaultValue(true);

            entity.Property(x => x.IsGeneric)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAntibiotic)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNarcotic)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPsychotropic)
                .HasDefaultValue(false);

            entity.Property(x => x.IsHighAlert)
                .HasDefaultValue(false);

            entity.Property(x => x.IsChronicDiseaseDrug)
                .HasDefaultValue(false);

            entity.Property(x => x.IsVaccine)
                .HasDefaultValue(false);

            entity.Property(x => x.IsConsumable)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCompoundIngredientAllowed)
                .HasDefaultValue(true);

            entity.Property(x => x.IsStockManaged)
                .HasDefaultValue(true);

            entity.Property(x => x.IsBatchTracked)
                .HasDefaultValue(true);

            entity.Property(x => x.IsExpiryDateTracked)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowFractionalDispense)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedPrescription)
                .HasDefaultValue(true);

            entity.Property(x => x.IsCoveredByInsuranceDefault)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedApproval)
                .HasDefaultValue(false);

            entity.Property(x => x.DefaultPrice)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.InsurancePrice)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.MemberPrice)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.CompanyPrice)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.Indication)
                .HasMaxLength(1000);

            entity.Property(x => x.Contraindication)
                .HasMaxLength(1000);

            entity.Property(x => x.SideEffect)
                .HasMaxLength(1000);

            entity.Property(x => x.WarningPrecaution)
                .HasMaxLength(1000);

            entity.Property(x => x.DosageInformation)
                .HasMaxLength(1000);

            entity.Property(x => x.DrugInteraction)
                .HasMaxLength(1000);

            entity.Property(x => x.AdministrationInstruction)
                .HasMaxLength(500);

            entity.Property(x => x.StorageInstruction)
                .HasMaxLength(500);

            entity.Property(x => x.PregnancyCategory)
                .HasMaxLength(100);

            entity.Property(x => x.LactationNote)
                .HasMaxLength(250);

            entity.Property(x => x.PediatricNote)
                .HasMaxLength(250);

            entity.Property(x => x.GeriatricNote)
                .HasMaxLength(250);

            entity.Property(x => x.ExternalDrugCode)
                .HasMaxLength(50);

            entity.Property(x => x.IntegrationCode)
                .HasMaxLength(50);

            entity.Property(x => x.BpomRegistrationNumber)
                .HasMaxLength(50);

            entity.Property(x => x.NationalDrugCode)
                .HasMaxLength(50);

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

            entity.HasOne(x => x.DrugCategory)
                .WithMany()
                .HasForeignKey(x => x.DrugCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StrengthMeasurement)
                .WithMany()
                .HasForeignKey(x => x.StrengthMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.BaseUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.BaseUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DispenseUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.DispenseUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PurchaseUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.PurchaseUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StockUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.StockUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DefaultDoseUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.DefaultDoseUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DrugCategoryId);

            entity.HasIndex(x => x.StrengthMeasurementId);

            entity.HasIndex(x => x.BaseUnitMeasurementId);

            entity.HasIndex(x => x.DispenseUnitMeasurementId);

            entity.HasIndex(x => x.PurchaseUnitMeasurementId);

            entity.HasIndex(x => x.StockUnitMeasurementId);

            entity.HasIndex(x => x.DefaultDoseUnitMeasurementId);

            entity.HasIndex(x => x.DrugCode)
                .IsUnique();

            entity.HasIndex(x => x.DrugName);

            entity.HasIndex(x => x.GenericName);

            entity.HasIndex(x => x.BrandName);

            entity.HasIndex(x => x.DrugForm);

            entity.HasIndex(x => x.Route);

            entity.HasIndex(x => x.ExternalDrugCode)
                .HasFilter("\"ExternalDrugCode\" IS NOT NULL");

            entity.HasIndex(x => x.IntegrationCode)
                .HasFilter("\"IntegrationCode\" IS NOT NULL");

            entity.HasIndex(x => x.BpomRegistrationNumber)
                .HasFilter("\"BpomRegistrationNumber\" IS NOT NULL");

            entity.HasIndex(x => x.NationalDrugCode)
                .HasFilter("\"NationalDrugCode\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.DrugCategoryId,
                x.IsFormulary,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DrugCategoryId,
                x.DrugForm,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAntibiotic,
                x.IsNarcotic,
                x.IsPsychotropic,
                x.IsHighAlert,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsStockManaged,
                x.IsBatchTracked,
                x.IsExpiryDateTracked,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsCompoundIngredientAllowed,
                x.IsAllowFractionalDispense,
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

            entity.HasIndex(x => new
            {
                x.IsNeedPrescription,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.BaseUnitMeasurementId,
                x.DispenseUnitMeasurementId,
                x.StockUnitMeasurementId
            });
        }
    }
}