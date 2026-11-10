using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Pemetaan supplier BLM ke MstSupplier (BE-FIN-143, Revisi 23, FIN-DES-133, data-dictionary 13.7).
/// GUID supplier BLM dan Quilvian tidak dianggap sama (FIN-DEC-263). Satu supplier BLM aktif hanya boleh
/// dipetakan ke satu supplier V2; beberapa supplier BLM boleh menunjuk supplier V2 yang sama. Pemetaan lama
/// dinonaktifkan, tidak ditimpa, supaya jejak tetap ada. Tabel dibiarkan kosong saat rilis (FIN-OQ-144).
/// Tidak ada kolom BLM pada MstSupplier karena tabel itu milik Administrator Master Data.
/// </summary>
[Table("FinPurchasingSupplierMap", Schema = "public")]
public sealed class FinPurchasingSupplierMap : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(20)] public string SourceSystem { get; set; } = FinPurchasingSourceSystems.Blm;

    /// <summary><c>SupplierId</c> BLM. Bukan FK.</summary>
    public Guid ExternalSupplierId { get; set; }

    /// <summary>Nama supplier di BLM saat dipetakan, untuk membantu petugas memeriksa.</summary>
    [MaxLength(300)] public string? ExternalSupplierName { get; set; }

    /// <summary>Milik Administrator (FIN-DEC-014) — Finance MUST NOT membuat master Supplier baru.</summary>
    public Guid SupplierId { get; set; }
    public MstSupplier? Supplier { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
