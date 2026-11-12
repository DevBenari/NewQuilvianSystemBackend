using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>
/// Akar data satu aset fisik (data-dictionary.md bagian 3.1, BE-AST-003). Agregatnya memuat riwayat
/// penempatan dan dokumen kepemilikan. Nomor inventaris terbit hanya saat disetujui dan permanen;
/// kolom "saat ini" (unit, lokasi, penanggung jawab) hanya diubah <c>AssetPlacementService</c>;
/// kunci proses aktif hanya diubah <c>AssetProcessLockService</c> (AST-DES-005).
/// </summary>
[Table("AstAsset", Schema = "public")]
public sealed class AstAsset : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LegalEntityId { get; set; }
    public MstLegalEntity? LegalEntity { get; set; }

    public Guid HospitalSiteId { get; set; }
    public MstHospitalSite? HospitalSite { get; set; }

    /// <summary>Nomor <c>AST-&lt;KODE&gt;-######</c>; kosong sampai aset disetujui, unik bila terisi.</summary>
    [MaxLength(30)] public string? InventoryNumber { get; set; }

    public Guid AssetGroupId { get; set; }
    public MstAssetGroup? AssetGroup { get; set; }

    public Guid OwnershipTypeId { get; set; }
    public MstAssetOwnershipType? OwnershipType { get; set; }

    [Required, MaxLength(250)] public string AssetName { get; set; } = string.Empty;

    [MaxLength(1000)] public string? Description { get; set; }

    /// <summary>Wajib bila klasifikasi medis (AST-RULE-006) — ditegakkan service.</summary>
    [MaxLength(100)] public string? Brand { get; set; }

    /// <summary>Wajib bila klasifikasi medis.</summary>
    [MaxLength(100)] public string? ModelType { get; set; }

    /// <summary>Wajib bila klasifikasi medis.</summary>
    [MaxLength(100)] public string? SerialNumber { get; set; }

    /// <summary>Wajib bila klasifikasi medis.</summary>
    [MaxLength(150)] public string? Manufacturer { get; set; }

    /// <summary>Nomor izin edar AKL/AKD; wajib bila klasifikasi medis.</summary>
    [MaxLength(60)] public string? DistributionPermitNumber { get; set; }

    public int? ManufactureYear { get; set; }

    [MaxLength(150)] public string? DistributorName { get; set; }

    public AssetAcquisitionType AcquisitionType { get; set; }

    public DateOnly AcquisitionDate { get; set; }

    /// <summary>Tanggal siap pakai; dasar awal penyusutan.</summary>
    public DateOnly ReadyForUseDate { get; set; }

    /// <summary>
    /// Harga perolehan saat pencatatan, dibekukan setelah disetujui (AST-DES-002). Nilai berjalan milik
    /// sub-modul nilai. Sensitif: hanya keluar lewat <c>GET /assets/{id}/amounts</c>.
    /// </summary>
    public decimal RegisteredAcquisitionCost { get; set; } = 0m;

    /// <summary>Salinan nama pemasok saat pencatatan.</summary>
    [MaxLength(200)] public string? SupplierName { get; set; }

    public Guid? PurchaseOrderId { get; set; }
    public FinPurchaseOrder? PurchaseOrder { get; set; }

    public Guid? GoodsReceiptId { get; set; }
    public FinGoodsReceipt? GoodsReceipt { get; set; }

    /// <summary>Salinan nomor penerimaan barang.</summary>
    [MaxLength(50)] public string? GoodsReceiptNumber { get; set; }

    /// <summary>Nomor berita acara hibah; wajib bila perolehan <c>HibahMasuk</c>.</summary>
    [MaxLength(100)] public string? DonationReference { get; set; }

    public AssetRecordStatus RecordStatus { get; set; } = AssetRecordStatus.Draft;

    public AssetCondition Condition { get; set; } = AssetCondition.Aktif;

    /// <summary>Saat kondisi terakhir berubah; dasar hitung aset menganggur (AST-DES-017).</summary>
    public DateTime? ConditionChangedAt { get; set; }

    public AssetPlacementState PlacementState { get; set; } = AssetPlacementState.BelumDitempatkan;

    public Guid? CurrentOrganizationUnitId { get; set; }
    public MstOrganizationUnit? CurrentOrganizationUnit { get; set; }

    public Guid? CurrentLocationId { get; set; }
    public MstAssetLocation? CurrentLocation { get; set; }

    public Guid? CurrentCustodianEmployeeId { get; set; }
    public MstEmployee? CurrentCustodianEmployee { get; set; }

    public AssetActiveProcessKind ActiveProcessKind { get; set; } = AssetActiveProcessKind.Tidak;

    /// <summary>Dokumen yang sedang memegang kunci proses aset ini.</summary>
    public Guid? ActiveProcessId { get; set; }

    public Guid? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }

    /// <summary>Alasan tolak atau batal.</summary>
    [MaxLength(500)] public string? DecisionReason { get; set; }

    /// <summary>Batch impor asal aset ini, bila masuk lewat impor data awal.</summary>
    public Guid? SourceImportBatchId { get; set; }
    public AstImportBatch? SourceImportBatch { get; set; }

    /// <summary>Token konkurensi (pola <c>FinGoodsReceipt</c>).</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<AstAssetPlacement> Placements { get; set; } = new List<AstAssetPlacement>();

    public ICollection<AstAssetDocument> Documents { get; set; } = new List<AstAssetDocument>();
}
