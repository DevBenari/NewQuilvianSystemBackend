using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilInvoiceItemConfiguration : IEntityTypeConfiguration<BilInvoiceItem>
{
    public void Configure(EntityTypeBuilder<BilInvoiceItem> entity)
    {
        entity.ToTable("BilInvoiceItem", "public", table =>
        {
            table.HasCheckConstraint("CK_BilInvoiceItem_Status", "\"Status\" IN ('ACTIVE','VOIDED')");
            table.HasCheckConstraint("CK_BilInvoiceItem_SourceVersion", "\"SourceVersion\" > 0");
            table.HasCheckConstraint("CK_BilInvoiceItem_Quantity", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_BilInvoiceItem_Amounts", "\"UnitPrice\" >= 0 AND \"DoctorShare\" >= 0 AND \"DoctorShare\" <= (\"Quantity\" * \"UnitPrice\")");
            table.HasCheckConstraint("CK_BilInvoiceItem_PerformerRequired", "NOT \"IsFeeBearing\" OR (\"PerformerType\" IS NOT NULL AND \"PerformerReferenceId\" IS NOT NULL)");
            table.HasCheckConstraint("CK_BilInvoiceItem_PerformerOnlyWhenFeeBearing", "\"IsFeeBearing\" OR (\"PerformerType\" IS NULL AND \"PerformerReferenceId\" IS NULL AND \"PerformerRoleCode\" IS NULL)");
            table.HasCheckConstraint("CK_BilInvoiceItem_PerformerType", "\"PerformerType\" IS NULL OR \"PerformerType\" IN ('DOCTOR','WORKFORCE')");
            table.HasCheckConstraint("CK_BilInvoiceItem_DoctorShareResolution", "\"DoctorShareResolution\" IS NULL OR (\"DoctorShareResolution\" IN ('MEDICAL_FEE','NO_RULE','UNAVAILABLE','NOT_APPLICABLE') AND (\"DoctorShareResolution\" = 'MEDICAL_FEE') = (\"DoctorShareSharingRuleId\" IS NOT NULL))");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SourceDomain).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SourceDetailId).HasMaxLength(100).IsRequired();
        entity.Property(x => x.SourceContractVersion).HasMaxLength(30).IsRequired();
        entity.Property(x => x.SourceStatus).HasMaxLength(30).IsRequired();
        entity.Property(x => x.SourceOccurredAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DescriptionSnapshot).HasMaxLength(250).IsRequired();
        entity.Property(x => x.Quantity).HasPrecision(18, 4);
        entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        entity.Property(x => x.DoctorShare).HasPrecision(18, 2);
        entity.Property(x => x.IsFeeBearing).HasDefaultValue(false).IsRequired();
        entity.Property(x => x.PerformerType).HasMaxLength(20);
        entity.Property(x => x.PerformerReferenceId);
        entity.Property(x => x.PerformerRoleCode).HasMaxLength(30);
        entity.Property(x => x.DoctorShareResolution).HasMaxLength(20);
        entity.Property(x => x.DoctorShareSharingRuleId);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.VoidReason).HasMaxLength(500);
        entity.Property(x => x.SourcePayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => new { x.SourceDomain, x.SourceDetailId }).IsUnique()
            .HasFilter("\"Status\" <> 'VOIDED' AND \"IsDelete\" = false");
        entity.HasIndex(x => new { x.InvoiceId, x.Status });
        entity.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        // Restrict, bukan Cascade: tarif yang sudah pernah ditagihkan tidak boleh ikut terhapus
        // bersama master-nya, dan penghapusan MstTariff yang masih dirujuk item harus gagal
        // terang-terangan supaya riwayat tagihan tidak kehilangan asal-usulnya.
        entity.HasOne(x => x.Tariff).WithMany().HasForeignKey(x => x.TariffId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => x.TariffId);
    }
}
