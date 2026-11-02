using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

/// <summary>
/// P04 — NOT ACTIVATED (QBE-CFG-001). Konfigurasi entity yang belum punya migration, DbSet, maupun
/// pemakai. Lihat <see cref="FinInvoiceExchangeGoodsReceipt"/> untuk syarat aktivasi. Karena konfigurasi
/// dimuat <c>ApplyConfigurationsFromAssembly</c>, `dotnet ef migrations add` berikutnya akan memasukkan
/// tabel ini ke migration; itu disengaja dan dikendalikan pemilik, bukan efek samping yang boleh dibiarkan
/// tanpa tinjauan.
/// </summary>
public sealed class FinInvoiceExchangeGoodsReceiptConfiguration : IEntityTypeConfiguration<FinInvoiceExchangeGoodsReceipt>
{
    public void Configure(EntityTypeBuilder<FinInvoiceExchangeGoodsReceipt> entity)
    {
        entity.ToTable("FinInvoiceExchangeGoodsReceipt", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.InvoiceExchange)
            .WithMany()
            .HasForeignKey(x => x.InvoiceExchangeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.GoodsReceipt)
            .WithMany()
            .HasForeignKey(x => x.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Restrict);

        // Satu GR hanya sekali dalam bundel Tukar Faktur yang sama. Sebuah GR TIDAK dibatasi hanya pada satu
        // Tukar Faktur: GR boleh ditagihkan sebagian oleh beberapa faktur, yang dijaga alokasi kuantitas.
        entity.HasIndex(x => new { x.InvoiceExchangeId, x.GoodsReceiptId }).IsUnique().HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinInvoiceExchangeGoodsReceipt_ExchangeGoodsReceipt");
        entity.HasIndex(x => x.GoodsReceiptId).HasDatabaseName("IX_FinInvoiceExchangeGoodsReceipt_GoodsReceiptId");
    }
}
