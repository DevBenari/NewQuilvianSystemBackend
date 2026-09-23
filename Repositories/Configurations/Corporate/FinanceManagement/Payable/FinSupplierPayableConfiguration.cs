using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Payable;

public sealed class FinSupplierPayableConfiguration : IEntityTypeConfiguration<FinSupplierPayable>
{
    public void Configure(EntityTypeBuilder<FinSupplierPayable> entity)
    {
        entity.ToTable("FinSupplierPayable", "public", table =>
        {
            table.HasCheckConstraint("CK_FinSupplierPayable_Status", "\"Status\" IN ('OUTSTANDING','PARTIAL','PAID','CANCELLED')");
            table.HasCheckConstraint("CK_FinSupplierPayable_Outstanding", "\"OutstandingAmount\" >= 0");
            // Ekstrapolasi dari FIN-VAL-011 (FinReceivable) — lihat ringkasan kelas model.
            table.HasCheckConstraint("CK_FinSupplierPayable_Balance",
                "\"OriginalAmount\" = \"OutstandingAmount\" + \"PaidAmount\" + \"AdjustedAmount\"");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.PayableNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SupplierInvoiceNumber).HasMaxLength(100).IsRequired();
        entity.Property(x => x.SupplierInvoiceDate).HasColumnType("date");
        entity.Property(x => x.Description).HasMaxLength(300);
        entity.Property(x => x.OriginalAmount).HasPrecision(18, 2);
        entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
        entity.Property(x => x.PaidAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.AdjustedAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.DueDate).HasColumnType("date");
        entity.Property(x => x.PaymentTermDays).HasDefaultValue(0);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinSupplierPayableStatuses.Outstanding);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // FIN-DEC-014: SupplierId merujuk MstSupplier existing milik Administrator — pola identik
        // MstBankAccountConfiguration.HasOne(x => x.Bank), bukan master Supplier baru milik Finance.
        entity.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PayableNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinSupplierPayable_PayableNumber");
        // Penjaga tunggal terhadap invoice supplier dobel (FIN-DEC-015, state-transition-matrix.md §5).
        entity.HasIndex(x => new { x.SupplierId, x.SupplierInvoiceNumber })
            .IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinSupplierPayable_Supplier_InvoiceNumber");
        entity.HasIndex(x => new { x.Status, x.DueDate }).HasDatabaseName("IX_FinSupplierPayable_Status_DueDate");
    }
}
