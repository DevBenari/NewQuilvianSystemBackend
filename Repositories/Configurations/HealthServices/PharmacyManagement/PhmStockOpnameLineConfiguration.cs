using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

/// <summary>Detail perhitungan Stock Opname (FIN-LGS-091). Tabel menunggu migrasi oleh pihak berwenang.</summary>
public class PhmStockOpnameLineConfiguration : IEntityTypeConfiguration<PhmStockOpnameLine>
{
    public void Configure(EntityTypeBuilder<PhmStockOpnameLine> builder)
    {
        builder.ToTable("PhmStockOpnameLine", "public", table =>
        {
            table.HasCheckConstraint("CK_PhmStockOpnameLine_PhysicalNotNegative",
                "\"PhysicalQuantity\" IS NULL OR \"PhysicalQuantity\" >= 0");

            // Baris saldo selalu menunjuk saldonya; temuan tidak pernah menunjuk saldo.
            table.HasCheckConstraint("CK_PhmStockOpnameLine_TypeMatchesBalance",
                "(\"LineType\" = 1 AND \"DrugStockBalanceId\" IS NOT NULL) OR " +
                "(\"LineType\" = 2 AND \"DrugStockBalanceId\" IS NULL)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DrugCodeSnapshot).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DrugNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BatchNumberSnapshot).HasMaxLength(100);
        builder.Property(x => x.StockUnitNameSnapshot).HasMaxLength(50);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.SnapshotQuantity).HasColumnType("numeric(18,3)");
        builder.Property(x => x.PhysicalQuantity).HasColumnType("numeric(18,3)");
        builder.Property(x => x.VarianceQuantity).HasColumnType("numeric(18,3)");

        builder.HasIndex(x => new { x.StockOpnameId, x.LineNumber })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        // Satu saldo satu baris per sesi.
        builder.HasIndex(x => new { x.StockOpnameId, x.DrugStockBalanceId })
            .IsUnique()
            .HasFilter("\"DrugStockBalanceId\" IS NOT NULL AND \"IsDelete\" = false");

        // Satu mutasi penyesuaian hanya milik satu baris.
        builder.HasIndex(x => x.AdjustmentMutationId)
            .IsUnique()
            .HasFilter("\"AdjustmentMutationId\" IS NOT NULL");

        builder.HasOne(x => x.DrugStockBalance).WithMany()
            .HasForeignKey(x => x.DrugStockBalanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Drug).WithMany()
            .HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DrugBatch).WithMany()
            .HasForeignKey(x => x.DrugBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AdjustmentMutation).WithMany()
            .HasForeignKey(x => x.AdjustmentMutationId).OnDelete(DeleteBehavior.Restrict);
    }
}
