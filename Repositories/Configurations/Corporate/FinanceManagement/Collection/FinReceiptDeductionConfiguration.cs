using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Collection;

/// <summary>
/// Bentuk REVISI 5 (erd/data-dictionary.md §D.3-D.4(c)) — menggantikan rancangan §C.15 yang
/// menunjuk hanya ReceiptId (belum pernah dibangun, digantikan sebelum ada baris data).
/// </summary>
public sealed class FinReceiptDeductionConfiguration : IEntityTypeConfiguration<FinReceiptDeduction>
{
    public void Configure(EntityTypeBuilder<FinReceiptDeduction> entity)
    {
        entity.ToTable("FinReceiptDeduction", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceiptDeduction_Type", "\"DeductionType\" IN ('PPH23','BANK_ADMIN_FEE','OTHER')");
            table.HasCheckConstraint("CK_FinReceiptDeduction_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_FinReceiptDeduction_OtherReason", "\"DeductionType\" <> 'OTHER' OR \"Reason\" IS NOT NULL");
            table.HasCheckConstraint("CK_FinReceiptDeduction_Reversal", "\"IsReversal\" = (\"ReversalOfDeductionId\" IS NOT NULL)");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.DeductionNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DeductionType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(500);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(100);
        entity.Property(x => x.IsReversal).HasDefaultValue(false);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Receipt)
            .WithMany()
            .HasForeignKey(x => x.ReceiptId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ReceiptAllocation)
            .WithMany()
            .HasForeignKey(x => x.ReceiptAllocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Baris pembalik sengaja tidak diberi nav — self-reference murni lewat Id, pola yang sama
        // dengan FinReceiptAllocation.ReversalOfAllocationId (FIN-DES-012).
        entity.HasOne<FinReceiptDeduction>().WithMany().HasForeignKey(x => x.ReversalOfDeductionId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.DeductionNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinReceiptDeduction_DeductionNumber");
        entity.HasIndex(x => x.ReceiptId).HasDatabaseName("IX_FinReceiptDeduction_ReceiptId");
        entity.HasIndex(x => x.DeductionType).HasDatabaseName("IX_FinReceiptDeduction_DeductionType");
        entity.HasIndex(x => x.ReceiptAllocationId).HasDatabaseName("IX_FinReceiptDeduction_Allocation");

        // Satu baris hanya boleh dibalik sekali.
        entity.HasIndex(x => x.ReversalOfDeductionId)
            .IsUnique()
            .HasFilter("\"ReversalOfDeductionId\" IS NOT NULL AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinReceiptDeduction_ReversalOnce");
    }
}
