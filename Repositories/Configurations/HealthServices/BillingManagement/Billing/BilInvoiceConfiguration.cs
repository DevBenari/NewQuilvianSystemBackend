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

        // Kontrak integrasi-billing 1.1.0 kamus data 6.6 — tanda "perlu diperiksa".
        entity.Property(x => x.RequiresReview).HasDefaultValue(false);
        entity.Property(x => x.ReviewReasonCode).HasMaxLength(50);
        entity.Property(x => x.ReviewFlaggedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ReviewResolvedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ReviewResolutionNote).HasMaxLength(500);
        entity.HasIndex(x => x.RequiresReview)
            .HasDatabaseName("IX_BilInvoice_RequiresReview")
            .HasFilter("\"RequiresReview\"");
        entity.HasOne<QuilvianSystemBackend.Models.ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.ReviewResolvedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Items).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.CalculationVersions).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
    }
}
