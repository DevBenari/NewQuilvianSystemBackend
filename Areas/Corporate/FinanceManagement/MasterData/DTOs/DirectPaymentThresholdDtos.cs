using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;

/// <summary>Mengubah (atau menetapkan untuk pertama kali) ambang pembayaran langsung (BE-FIN-076, FIN-DEC-134).</summary>
public sealed class UpdateDirectPaymentThresholdRequest
{
    public decimal Amount { get; set; }

    /// <summary>
    /// Wajib setiap kali diubah (FIN-DEC-134, FIN-VAL-205). SENGAJA TIDAK diberi <c>[Required]</c> —
    /// atribut itu akan membuat pipeline validasi otomatis [ApiController] memotong permintaan
    /// dengan 400 generik sebelum sempat dijawab 422 + pesan kontrak yang benar. Pemeriksaan
    /// kosong/spasi dilakukan manual di DirectPaymentThresholdService.
    /// </summary>
    [MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;

    /// <summary>
    /// Penanda versi yang dibaca klien dari <c>GET</c> terakhir (BE-FIN-086, FIN-VAL-228).
    ///
    /// WAJIB ketika baris ambang aktif sudah ada; versi basi atau tidak dikirim dijawab 409.
    /// SENGAJA nullable, dan itu bukan kelonggaran: pada penetapan ambang PERTAMA belum ada baris,
    /// sehingga tidak ada versi yang dapat dibaca klien. Tanpa pengecualian itu ambang TIDAK AKAN
    /// PERNAH dapat ditetapkan dan seluruh pembayaran langsung terkunci permanen (FIN-DES-086).
    ///
    /// <c>EffectiveFrom</c> DIBUANG dari kontrak ini (FIN-DEC-145/153). Klien lama yang masih
    /// mengirimnya tidak ditolak — ruas tak dikenal diabaikan — tetapi nilainya tidak disimpan.
    /// </summary>
    public Guid? ExpectedRowVersion { get; set; }
}

/// <summary>Ambang aktif beserta alasan, penanda versi, dan jejak perubahan terakhirnya (FIN-API-1.7 G.1).</summary>
public sealed class DirectPaymentThresholdResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string ChangeReason { get; set; } = string.Empty;

    /// <summary>Dikirim kembali klien pada penyimpanan berikutnya (FIN-VAL-228).</summary>
    public Guid RowVersion { get; set; }

    public Guid LastChangedBy { get; set; }

    /// <summary>
    /// Nama tampilan pengubah terakhir (BE-FIN-086, FIN-DEC-151, FIN-DES-095). Dibaca saat menyusun
    /// respons, TIDAK disalin ke tabel Finance — menyalinnya akan membuat nama membeku ketika nama
    /// aslinya berubah. <c>null</c> bila penggunanya tidak ditemukan; layar MUST NOT menampilkan ID mentah.
    /// </summary>
    public string? LastChangedByName { get; set; }

    public DateTime LastChangedAt { get; set; }
}

/// <summary>FIN-VAL-205: ChangeReason kosong saat mengubah ambang — MUST dijawab 422.</summary>
public sealed class DirectPaymentThresholdValidationException(string message) : Exception(message);

/// <summary>FIN-VAL-206: nilai ambang nol atau negatif — MUST dijawab 400.</summary>
public sealed class DirectPaymentThresholdBadRequestException(string message) : Exception(message);

/// <summary>
/// FIN-VAL-228: <c>ExpectedRowVersion</c> basi atau tidak dikirim sementara baris ambang aktif sudah
/// ada — MUST dijawab 409, BUKAN ditimpa (BE-FIN-086, FIN-DEC-146).
/// </summary>
public sealed class DirectPaymentThresholdConflictException(string message) : Exception(message);
