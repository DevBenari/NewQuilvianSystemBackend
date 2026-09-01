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
    }
}
