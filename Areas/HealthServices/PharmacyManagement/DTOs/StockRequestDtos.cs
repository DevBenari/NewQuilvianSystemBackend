using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

// ===================================================================== permintaan

public class StockRequestItemInput
{
    [Required] public Guid DrugId { get; set; }
    [Required] public Guid MeasurementId { get; set; }

    [Range(0.001, 1000000)]
    public decimal RequestedQuantity { get; set; }

    [MaxLength(500)] public string? Note { get; set; }
}

public class CreateStockRequestRequest
{
    [Required] public Guid RequestingServiceUnitId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }
    [Required] public Guid RequestedByWorkforceId { get; set; }
    [Required] public StockRequestPriority Priority { get; set; } = StockRequestPriority.Routine;

    public DateTime? NeededAt { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<StockRequestItemInput> Items { get; set; } = [];

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class UpdateStockRequestRequest
{
    [Required] public Guid StorageLocationId { get; set; }
    [Required] public StockRequestPriority Priority { get; set; }

    public DateTime? NeededAt { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    /// <summary>
    /// Daftar item pengganti. Yang dikirim menjadi isi permintaan seluruhnya; baris lama
    /// yang tidak lagi disebut akan dihapus.
    /// </summary>
    [Required, MinLength(1)]
    public List<StockRequestItemInput> Items { get; set; } = [];

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class SubmitStockRequestRequest
{
    /// <summary>
    /// Alasan darurat dari pemohon. Wajib bila prioritas permintaan <c>Emergency</c> (CITO);
    /// dicatat pada riwayat pengajuan sebagai jejak audit (<c>LGS-DEC-002</c>).
    /// </summary>
    [MaxLength(1000)] public string? EmergencyReason { get; set; }

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

// ------------------------------------------------------------- keputusan atasan

/// <summary>Persetujuan atasan unit atas permintaan yang menunggu persetujuan.</summary>
public class ApproveStockRequestRequest
{
    [MaxLength(1000)] public string? Note { get; set; }
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Penolakan atasan unit; alasannya wajib.</summary>
public class RejectStockRequestRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Otorisasi jalur CITO oleh petugas berwenang; alasannya wajib.</summary>
public class AuthorizeEmergencyStockRequestRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>
/// Verifikasi administratif susulan atas permintaan CITO yang sudah diotorisasi lewat jalur
/// darurat (<c>LGS-DEC-005</c>). Tidak mengubah status maupun stok.
/// </summary>
public class VerifyEmergencyStockRequestRequest
{
    [Required] public StockRequestEmergencyReviewOutcome Outcome { get; set; }

    /// <summary>Catatan pemeriksaan. Wajib bila hasilnya temuan penyimpangan.</summary>
    [MaxLength(1000)] public string? Note { get; set; }

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class CancelStockRequestRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

// ------------------------------------------------------------------ sisi gudang

public class FulfillStockRequestItemInput
{
    [Required] public Guid StockRequestItemId { get; set; }

    /// <summary>
    /// Jumlah yang benar-benar diserahkan. Nol sah dan berarti tidak ada yang diserahkan
    /// untuk baris ini — berbeda dari tidak menyebut barisnya sama sekali, yang ditolak.
    /// </summary>
    [Range(0, 1000000)]
    public decimal FulfilledQuantity { get; set; }
}

/// <summary>Penyerahan barang oleh gudang, per baris permintaan.</summary>
public class FulfillStockRequestRequest
{
    /// <summary>
    /// Harus memuat seluruh baris permintaan yang masih berlaku. Baris yang tidak disebut
    /// ditolak, bukan dianggap nol: gudang harus menyatakan setiap baris secara sadar.
    /// </summary>
    [Required, MinLength(1)]
    public List<FulfillStockRequestItemInput> Items { get; set; } = [];

    [MaxLength(1000)] public string? Note { get; set; }

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Saringan riwayat permintaan obat.</summary>
public class StockRequestPagedQuery
{
    public StockRequestStatus? Status { get; set; }
    public StockRequestPriority? Priority { get; set; }
    public Guid? RequestingServiceUnitId { get; set; }
    public Guid? StorageLocationId { get; set; }

    /// <summary>Menyaring menurut obat yang diminta, bukan hanya nomor permintaan.</summary>
    public Guid? DrugId { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>Mencari pada nomor permintaan, nama unit, atau nama obat di dalamnya.</summary>
    public string? Search { get; set; }

    /// <summary>
    /// Urutan antrean gudang: prioritas tertinggi (CITO) lebih dahulu, lalu yang paling lama
    /// menunggu. Tanpa ini, urutannya terbaru lebih dahulu seperti riwayat biasa.
    /// </summary>
    public bool OrderByPriority { get; set; }

    /// <summary>
    /// Antrean gudang: hanya permintaan yang boleh diserahkan sekarang — berstatus Disetujui,
    /// ditambah permintaan lama yang mengikuti aturan peralihan (lihat
    /// <see cref="StockRequestSummaryResponse.IsLegacySubmission"/>).
    /// </summary>
    public bool FulfillableOnly { get; set; }

    /// <summary>
    /// Menyaring menurut keadaan verifikasi susulan CITO, misalnya <c>Pending</c> untuk daftar
    /// CITO yang belum diverifikasi atasan unit.
    /// </summary>
    public StockRequestEmergencyReviewState? EmergencyReviewState { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ===================================================================== tanggapan

public class StockRequestSummaryResponse
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public Guid RequestingServiceUnitId { get; set; }
    public string RequestingServiceUnitName { get; set; } = string.Empty;
    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public StockRequestStatus Status { get; set; }
    public StockRequestPriority Priority { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? NeededAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int ItemCount { get; set; }
    public int Version { get; set; }

    /// <summary>
    /// Benar bila permintaan ini masih boleh diubah. Dihitung backend agar layar tidak
    /// perlu menyalin aturannya dan tidak dapat menyimpang darinya.
    /// </summary>
    public bool IsEditable { get; set; }

    /// <summary>
    /// Perintah yang sah pada keadaan sekarang. Sama alasannya dengan <see cref="IsEditable"/>:
    /// aturan transisi hanya ada di backend, layar cukup menuruti.
    /// </summary>
    /// <remarks>
    /// Gudang tetap tidak memutuskan permintaan. Persetujuan dilakukan atasan unit
    /// (<see cref="CanApprove"/>) atau petugas CITO berwenang (<see cref="CanAuthorizeEmergency"/>);
    /// gudang hanya menyerahkan permintaan yang sudah disetujui (<c>LGS-DEC-002</c>).
    /// </remarks>
    public bool CanFulfill { get; set; }
    public bool CanCancel { get; set; }

    /// <summary>Keadaan permintaan memungkinkan disetujui atau ditolak atasan. Izin tetap diperiksa backend.</summary>
    public bool CanApprove { get; set; }

    /// <summary>Permintaan CITO yang menunggu dan dapat diotorisasi lewat jalur darurat.</summary>
    public bool CanAuthorizeEmergency { get; set; }

    /// <summary>
    /// Selesai, tetapi sekurang-kurangnya satu baris diserahkan kurang dari yang diminta.
    /// Dihitung dari data, bukan status tersendiri, karena penyerahan sebagian tetap menutup permintaan.
    /// </summary>
    public bool IsPartiallyFulfilled { get; set; }

    /// <summary>
    /// Permintaan berstatus Submitted yang diajukan sebelum batas
    /// <c>PharmacyManagement:StockRequest:ApprovalEnforcedFromUtc</c>. Aturan peralihan:
    /// permintaan seperti ini masih boleh diserahkan langsung seperti sebelum persetujuan atasan
    /// berlaku, dan tetap boleh diputuskan atasan. Selalu false bila batas itu tidak diatur.
    /// </summary>
    public bool IsLegacySubmission { get; set; }

    /// <summary>Keadaan verifikasi susulan CITO, dihitung dari riwayat (<c>LGS-DEC-005</c>).</summary>
    public StockRequestEmergencyReviewState EmergencyReviewState { get; set; }
}

public class StockRequestDetailResponse : StockRequestSummaryResponse
{
    public Guid RequestedByWorkforceId { get; set; }
    public string? Notes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionReason { get; set; }
    public List<StockRequestItemResponse> Items { get; set; } = [];
    public List<StockRequestHistoryResponse> Histories { get; set; } = [];

    /// <summary>
    /// Pengguna yang sedang membuka adalah pemohon, pembuat, atau pengirim permintaan ini,
    /// sehingga tidak boleh menyetujui, menolak, maupun mengotorisasinya.
    /// </summary>
    public bool IsOwnRequest { get; set; }

    /// <summary>
    /// Pengguna yang sedang membuka adalah atasan unit dari unit peminta. Hanya dihitung
    /// selama permintaan menunggu persetujuan; selain itu bernilai null.
    /// </summary>
    public bool? HasUnitAuthority { get; set; }

    /// <summary>Sebab pengguna tidak berwenang sebagai atasan unit, untuk ditampilkan apa adanya.</summary>
    public string? UnitAuthorityMessage { get; set; }

    /// <summary>
    /// Pengguna memegang izin otorisasi CITO yang diberikan eksplisit lewat penempatan jabatan.
    /// Hanya dihitung untuk permintaan CITO yang menunggu; selain itu bernilai null.
    /// </summary>
    public bool? HasEmergencyAuthority { get; set; }

    /// <summary>
    /// Pengguna memegang izin <c>StockRequest:Approve</c> yang diberikan eksplisit lewat penempatan
    /// jabatan. Dihitung selama permintaan menunggu persetujuan atau menunggu verifikasi susulan.
    /// </summary>
    public bool? HasApproveAuthority { get; set; }

    /// <summary>
    /// Pengguna boleh melakukan verifikasi susulan CITO atas permintaan ini: izin persetujuan
    /// eksplisit, atasan unit peminta, bukan pemohon, dan bukan pengotorisasi CITO-nya.
    /// Null bila permintaan tidak sedang menunggu verifikasi susulan.
    /// </summary>
    public bool? CanVerifyEmergency { get; set; }

    /// <summary>Sebab pengguna tidak boleh melakukan verifikasi susulan, untuk ditampilkan apa adanya.</summary>
    public string? VerifyEmergencyMessage { get; set; }

    /// <summary>Otorisasi CITO awal (dari riwayat); null bila tidak lewat jalur CITO.</summary>
    public StockRequestDecisionRecordResponse? EmergencyAuthorization { get; set; }

    /// <summary>Hasil verifikasi susulan (dari riwayat); null bila belum diverifikasi.</summary>
    public StockRequestEmergencyReviewResponse? EmergencyReview { get; set; }
}

/// <summary>Satu keputusan yang tercatat di riwayat: siapa, kapan, dan alasannya.</summary>
public class StockRequestDecisionRecordResponse
{
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? Reason { get; set; }
}

/// <summary>Hasil verifikasi susulan CITO.</summary>
public class StockRequestEmergencyReviewResponse : StockRequestDecisionRecordResponse
{
    public StockRequestEmergencyReviewOutcome Outcome { get; set; }
}

public class StockRequestItemResponse
{
    public Guid Id { get; set; }
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public Guid MeasurementId { get; set; }
    public string? MeasurementName { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal? FulfilledQuantity { get; set; }
    public string? Note { get; set; }
    public int LineNumber { get; set; }
}

public class StockRequestHistoryResponse
{
    public Guid Id { get; set; }
    public StockRequestStatus? FromStatus { get; set; }
    public StockRequestStatus ToStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime OccurredAt { get; set; }

    /// <summary>Petugas yang melakukan perubahan ini; jejak audit keputusan.</summary>
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
}
