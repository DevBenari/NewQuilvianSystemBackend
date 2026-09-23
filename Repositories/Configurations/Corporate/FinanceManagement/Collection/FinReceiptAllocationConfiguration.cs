using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Collection;

public sealed class FinReceiptAllocationConfiguration : IEntityTypeConfiguration<FinReceiptAllocation>
{
    public void Configure(EntityTypeBuilder<FinReceiptAllocation> entity)
    {
        entity.ToTable("FinReceiptAllocation", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceiptAllocation_TargetType", "\"TargetType\" IN ('RECEIVABLE','INVOICE_DIRECT')");
            table.HasCheckConstraint("CK_FinReceiptAllocation_Amount", "\"Amount\" > 0");
            // Alokasi ke piutang wajib menyebut piutangnya.
            table.HasCheckConstraint("CK_FinReceiptAllocation_ReceivableRequired", "\"TargetType\" <> 'RECEIVABLE' OR \"ReceivableId\" IS NOT NULL");
            table.HasCheckConstraint("CK_FinReceiptAllocation_Reversal",
                "(\"IsReversal\" = true AND \"ReversalOfAllocationId\" IS NOT NULL) OR (\"IsReversal\" = false AND \"ReversalOfAllocationId\" IS NULL)");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.TargetType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.IsReversal).HasDefaultValue(false);
        entity.Property(x => x.AllocatedAt).HasColumnType("timestamp with time zone");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Receipt).WithMany(x => x.Allocations)
            .HasForeignKey(x => x.ReceiptId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Receivable).WithMany(x => x.ReceiptAllocations)
            .HasForeignKey(x => x.ReceivableId).OnDelete(DeleteBehavior.Restrict);
        // Baris pembalik sengaja tidak diberi nav — self-reference murni lewat Id (FIN-DES-012).
        entity.HasOne<FinReceiptAllocation>().WithMany().HasForeignKey(x => x.ReversalOfAllocationId).OnDelete(DeleteBehavior.Restrict);
    }
}
