using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionItemConfiguration : IEntityTypeConfiguration<TrxPrescriptionItem>
    {
        public void Configure(EntityTypeBuilder<TrxPrescriptionItem> entity)
        {
            entity.ToTable("TrxPrescriptionItem", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionId).IsRequired();
            entity.Property(x => x.DrugId).IsRequired();
            entity.Property(x => x.TariffId).IsRequired(false);
            entity.Property(x => x.InsuranceTariffId).IsRequired(false);
            entity.Property(x => x.InsuranceCoverageRuleId).IsRequired(false);

            entity.Property(x => x.DrugCodeSnapshot).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DrugNameSnapshot).HasMaxLength(250).IsRequired();
            entity.Property(x => x.GenericNameSnapshot).HasMaxLength(250);
            entity.Property(x => x.DrugCategoryNameSnapshot).HasMaxLength(150);
            entity.Property(x => x.DrugFormSnapshot).HasMaxLength(100);
            entity.Property(x => x.StrengthSnapshot).HasMaxLength(100);
            entity.Property(x => x.RouteSnapshot).HasMaxLength(50);

            entity.Property(x => x.Dose).HasColumnType("numeric(18,4)").HasDefaultValue(1);
            entity.Property(x => x.DoseUnitNameSnapshot).HasMaxLength(100);
            entity.Property(x => x.DoseUnitSymbolSnapshot).HasMaxLength(30);
            entity.Property(x => x.FrequencyCode).HasMaxLength(50);
            entity.Property(x => x.FrequencyText).HasMaxLength(150);
            entity.Property(x => x.FrequencyPerDay).HasColumnType("numeric(18,4)");
            entity.Property(x => x.DurationValue).HasColumnType("numeric(18,4)");
            entity.Property(x => x.DurationUnit).HasMaxLength(30);
            entity.Property(x => x.AdministrationTime).HasMaxLength(250);
            entity.Property(x => x.Signa).HasMaxLength(500);
            entity.Property(x => x.AdministrationInstruction).HasMaxLength(500);
            entity.Property(x => x.DoctorNote).HasMaxLength(500);

            entity.Property(x => x.Quantity).HasColumnType("numeric(18,4)").HasDefaultValue(1);
            entity.Property(x => x.DispenseUnitNameSnapshot).HasMaxLength(100);
            entity.Property(x => x.DispenseUnitSymbolSnapshot).HasMaxLength(30);

            entity.Property(x => x.HospitalUnitPrice).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.ContractUnitPrice).HasColumnType("numeric(18,2)");
            entity.Property(x => x.UnitPrice).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.TotalPrice).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.PricingSource).HasMaxLength(50).HasDefaultValue("HospitalTariff");
            entity.Property(x => x.CoverageStatus).HasMaxLength(50).HasDefaultValue("NotApplicable");
            entity.Property(x => x.CoveragePercent).HasColumnType("numeric(7,2)").HasDefaultValue(0);
            entity.Property(x => x.CoveredAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.PatientPayAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.CoPaymentAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.ApprovalNote).HasMaxLength(250);
            entity.Property(x => x.CoverageNote).HasMaxLength(1000);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.Prescription)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Drug).WithMany().HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Tariff).WithMany().HasForeignKey(x => x.TariffId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InsuranceTariff).WithMany().HasForeignKey(x => x.InsuranceTariffId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InsuranceCoverageRule).WithMany().HasForeignKey(x => x.InsuranceCoverageRuleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DoseUnitMeasurement).WithMany().HasForeignKey(x => x.DoseUnitMeasurementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DispenseUnitMeasurement).WithMany().HasForeignKey(x => x.DispenseUnitMeasurementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PrescriptionId);
            entity.HasIndex(x => x.DrugId);
            entity.HasIndex(x => x.TariffId);
            entity.HasIndex(x => x.InsuranceTariffId);
            entity.HasIndex(x => x.InsuranceCoverageRuleId);
            entity.HasIndex(x => x.DoseUnitMeasurementId);
            entity.HasIndex(x => x.DispenseUnitMeasurementId);
            entity.HasIndex(x => x.ApprovedByUserId);
            entity.HasIndex(x => new { x.PrescriptionId, x.SortOrder, x.IsDelete });
            entity.HasIndex(x => new { x.PrescriptionId, x.IsNeedApproval, x.IsApproved, x.IsDelete });
        }
    }
}
