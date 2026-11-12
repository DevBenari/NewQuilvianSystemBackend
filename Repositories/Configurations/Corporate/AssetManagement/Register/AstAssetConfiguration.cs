using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstAsset</c> — data-dictionary.md bagian 3.1 dan DDL bagian 4 (BE-AST-003).</summary>
/// <remarks>
/// FK ke tabel modul lain (HR, Finance) memakai <c>WithMany()</c> tanpa koleksi balik agar model pemiliknya
/// tidak disentuh. Index tunggal <c>CurrentOrganizationUnitId</c> tidak dibuat terpisah karena sudah menjadi
/// kolom pertama <c>IX_AstAsset_Unit_Status</c>.
/// </remarks>
public sealed class AstAssetConfiguration : IEntityTypeConfiguration<AstAsset>
{
    public void Configure(EntityTypeBuilder<AstAsset> entity)
    {
        entity.ToTable("AstAsset", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.InventoryNumber).HasMaxLength(30);
        entity.Property(x => x.AssetName).HasMaxLength(250).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(1000);
        entity.Property(x => x.Brand).HasMaxLength(100);
        entity.Property(x => x.ModelType).HasMaxLength(100);
        entity.Property(x => x.SerialNumber).HasMaxLength(100);
        entity.Property(x => x.Manufacturer).HasMaxLength(150);
        entity.Property(x => x.DistributionPermitNumber).HasMaxLength(60);
        entity.Property(x => x.DistributorName).HasMaxLength(150);
        entity.Property(x => x.AcquisitionType).HasConversion<int>().IsRequired();
        entity.Property(x => x.AcquisitionDate).HasColumnType("date");
        entity.Property(x => x.ReadyForUseDate).HasColumnType("date");
        entity.Property(x => x.RegisteredAcquisitionCost).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.SupplierName).HasMaxLength(200);
        entity.Property(x => x.GoodsReceiptNumber).HasMaxLength(50);
        entity.Property(x => x.DonationReference).HasMaxLength(100);
        entity.Property(x => x.RecordStatus).HasConversion<int>().IsRequired();
        entity.Property(x => x.Condition).HasConversion<int>().IsRequired();
        entity.Property(x => x.PlacementState).HasConversion<int>().IsRequired();
        entity.Property(x => x.ActiveProcessKind).HasConversion<int>().IsRequired().HasDefaultValue(AssetActiveProcessKind.Tidak);
        entity.Property(x => x.DecisionReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.AssetGroup).WithMany().HasForeignKey(x => x.AssetGroupId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.OwnershipType).WithMany().HasForeignKey(x => x.OwnershipTypeId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.PurchaseOrder).WithMany().HasForeignKey(x => x.PurchaseOrderId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.GoodsReceipt).WithMany().HasForeignKey(x => x.GoodsReceiptId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CurrentOrganizationUnit).WithMany().HasForeignKey(x => x.CurrentOrganizationUnitId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CurrentLocation).WithMany().HasForeignKey(x => x.CurrentLocationId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CurrentCustodianEmployee).WithMany().HasForeignKey(x => x.CurrentCustodianEmployeeId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.SourceImportBatch).WithMany().HasForeignKey(x => x.SourceImportBatchId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        // Nomor inventaris permanen: unik bila terisi, tanpa pengecualian baris terhapus (DDL kamus bagian 4).
        entity.HasIndex(x => x.InventoryNumber)
            .IsUnique()
            .HasFilter("\"InventoryNumber\" IS NOT NULL")
            .HasDatabaseName("IX_AstAsset_InventoryNumber");

        entity.HasIndex(x => new { x.CurrentOrganizationUnitId, x.RecordStatus, x.Condition })
            .HasDatabaseName("IX_AstAsset_Unit_Status");

        entity.HasIndex(x => new { x.Brand, x.SerialNumber })
            .HasDatabaseName("IX_AstAsset_Brand_SerialNumber");

        entity.HasIndex(x => x.LegalEntityId);
        entity.HasIndex(x => x.AssetGroupId);
        entity.HasIndex(x => x.OwnershipTypeId);
        entity.HasIndex(x => x.AssetName);
        entity.HasIndex(x => x.AcquisitionType);
        entity.HasIndex(x => x.PurchaseOrderId);
        entity.HasIndex(x => x.GoodsReceiptId);
        entity.HasIndex(x => x.RecordStatus);
        entity.HasIndex(x => x.Condition);
        entity.HasIndex(x => x.PlacementState);
        entity.HasIndex(x => x.CurrentLocationId);
        entity.HasIndex(x => x.CurrentCustodianEmployeeId);
        entity.HasIndex(x => x.ActiveProcessKind);
        entity.HasIndex(x => x.SourceImportBatchId);
    }
}
