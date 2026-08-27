using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilInvoiceConfiguration : IEntityTypeConfiguration<BilInvoice>
{
    public void Configure(EntityTypeBuilder<BilInvoice> entity)
    {
        entity.ToTable("BilInvoice", "public", table =>
            table.HasCheckConstraint("CK_BilInvoice_Status", "\"Status\" IN ('OPEN','FINAL','CLOSED','SETTLED_BY_WRITE_OFF')"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.KwitansiNumber).HasMaxLength(50);
        entity.Property(x => x.ServiceType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.InvoiceDate).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.EncounterId).IsUnique();
        entity.HasIndex(x => x.InvoiceNumber).IsUnique();
        entity.HasIndex(x => x.KwitansiNumber).IsUnique().HasFilter("\"KwitansiNumber\" IS NOT NULL");
        entity.HasMany(x => x.Items).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.CalculationVersions).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
    }
}
