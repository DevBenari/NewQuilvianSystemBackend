using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

/// <summary>Sesi Stock Opname (FIN-LGS-091). Tabel menunggu migrasi oleh pihak berwenang.</summary>
public class PhmStockOpnameConfiguration : IEntityTypeConfiguration<PhmStockOpname>
{
    public void Configure(EntityTypeBuilder<PhmStockOpname> builder)
    {
        builder.ToTable("PhmStockOpname", "public", table =>
        {
            // Penolakan tanpa alasan tidak dapat dipertanggungjawabkan di berita acara maupun audit.
            table.HasCheckConstraint("CK_PhmStockOpname_RejectedHasReason",
                "\"Status\" <> 4 OR (\"DecisionReason\" IS NOT NULL AND length(\"DecisionReason\") > 0)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OpnameNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.StorageLocationNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.DecisionReason).HasMaxLength(500);
        builder.Property(x => x.Version).IsConcurrencyToken();

        // Nomor bisnis unik (QBE-CODE-004).
        builder.HasIndex(x => x.OpnameNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        // Gudang tidak dibekukan selama opname; dua sesi aktif pada lokasi yang sama dapat
        // menyesuaikan saldo yang sama dua kali. Satu sesi aktif (dihitung atau diajukan) per lokasi.
        builder.HasIndex(x => x.StorageLocationId)
            .IsUnique()
            .HasFilter("\"Status\" IN (1, 2) AND \"IsDelete\" = false")
            .HasDatabaseName("UX_PhmStockOpname_ActiveLocation");

        builder.HasIndex(x => new { x.StorageLocationId, x.OpnameDate });
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.StorageLocation).WithMany()
            .HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PreviousStockOpname).WithMany()
            .HasForeignKey(x => x.PreviousStockOpnameId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines).WithOne(x => x.StockOpname)
            .HasForeignKey(x => x.StockOpnameId).OnDelete(DeleteBehavior.Restrict);
    }
}
