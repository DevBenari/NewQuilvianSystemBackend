using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

/// <summary>
/// Depo farmasi yang menjadi sumber stok bagi satu kamar operasi.
/// </summary>
/// <remarks>
/// <para>
/// Kamar operasi bukan depo dengan saldo stok mandiri (`OPS-DEC-027`). Ia memakai stok milik
/// depo farmasi, dan pemetaan inilah yang menentukan depo mana. Pemetaan dibuat sebagai data,
/// bukan konstanta di dalam kode, supaya rumah sakit yang kelak membuka Depo OK tersendiri
/// cukup mengubah satu baris tanpa menyentuh desain transaksinya.
/// </para>
/// <para>
/// Satu kamar hanya boleh punya satu sumber aktif. Bila dua baris aktif dibiarkan ada, tidak
/// ada cara menentukan depo mana yang stoknya berkurang, dan selisih itu baru terlihat setelah
/// stok opname.
/// </para>
/// </remarks>
public class OprStockSource : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kamar operasi, dari master ruangan yang sudah ada.</summary>
    public Guid RoomId { get; set; }

    /// <summary>Depo farmasi yang stoknya dipakai kamar tersebut.</summary>
    public Guid StorageLocationId { get; set; }

    /// <summary>
    /// Pemetaan nonaktif tetap disimpan sebagai riwayat: pemakaian lama harus tetap dapat
    /// dijelaskan dengan pemetaan yang berlaku saat itu.
    /// </summary>
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Note { get; set; }

    public MstRoom? Room { get; set; }
    public MstDrugStorageLocation? StorageLocation { get; set; }
}
