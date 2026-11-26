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
    // Nilai di luar enum (mis. 9) ditolak 400: tersimpan apa adanya, ia membuat permintaan bukan
    // rutin maupun CITO sehingga lolos dari aturan alasan CITO dan jalur otorisasinya.
    [Required, EnumDataType(typeof(StockRequestPriority))]
    public StockRequestPriority Priority { get; set; } = StockRequestPriority.Routine;

    public DateTime? NeededAt { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<StockRequestItemInput> Items { get; set; } = [];

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class UpdateStockRequestRequest
{
    [Required] public Guid StorageLocationId { get; set; }
    [Required, EnumDataType(typeof(StockRequestPriority))] public StockRequestPriority Priority { get; set; }

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

/// <summary>Persetujuan penyetuju atas permintaan yang menunggu persetujuan.</summary>
public class ApproveStockRequestRequest
{
    [MaxLength(1000)] public string? Note { get; set; }
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Penolakan oleh penyetuju; alasannya wajib.</summary>
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
    // Saringan enum divalidasi: nilai yang tidak dikenal dijawab 400, bukan ditafsirkan diam-diam.
    [EnumDataType(typeof(StockRequestStatus))] public StockRequestStatus? Status { get; set; }
    [EnumDataType(typeof(StockRequestPriority))] public StockRequestPriority? Priority { get; set; }
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
    /// CITO yang belum diverifikasi susulan.
    /// </summary>
    [EnumDataType(typeof(StockRequestEmergencyReviewState))]
    public StockRequestEmergencyReviewState? EmergencyReviewState { get; set; }

    /// <summary>
    /// Menyaring CITO yang menunggu verifikasi menurut keadaan tenggatnya (<c>NotDue</c>,
    /// <c>DueSoon</c>, <c>Overdue</c>). Selama tenggat belum dikonfigurasi hasilnya kosong.
    /// </summary>
    [EnumDataType(typeof(StockRequestEmergencyReviewDueState))]
    public StockRequestEmergencyReviewDueState? EmergencyReviewDueState { get; set; }

    /// <summary>
    /// Cakupan daftar. <c>MyAuthority</c> = "Dalam Kewenangan Saya": dihitung backend dari izin
    /// aktif pengguna yang login (Approve: seluruh unit, CITO: permintaan CITO; tanpa draf).
    /// </summary>
    [EnumDataType(typeof(StockRequestListScope))]
    public StockRequestListScope Scope { get; set; } = StockRequestListScope.All;

    /// <summary>Menyaring menurut petugas peminta (profil tenaga kerja <c>RequestedByWorkforceId</c>).</summary>
    public Guid? RequestedByWorkforceId { get; set; }

    /// <summary>Hanya permintaan yang sudah diajukan (bukan Draft) — tampilan Status Pemenuhan.</summary>
    public bool ExcludeDraft { get; set; }

    /// <summary>
    /// Hanya permintaan Selesai yang sekurang-kurangnya satu barisnya diserahkan kurang dari yang
    /// diminta (<see cref="StockRequestSummaryResponse.IsPartiallyFulfilled"/>).
    /// </summary>
    public bool PartiallyFulfilledOnly { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Saringan linimasa aktivitas permintaan ("Riwayat Permintaan"): setiap baris adalah satu
/// perubahan yang tercatat di riwayat permintaan, bukan satu permintaan.
/// </summary>
public class StockRequestHistoryPagedQuery
{
    /// <summary>Awal rentang waktu kejadian (tanggal, inklusif).</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Akhir rentang waktu kejadian (tanggal, inklusif sampai akhir hari).</summary>
    public DateTime? EndDate { get; set; }

    public Guid? RequestingServiceUnitId { get; set; }

    /// <summary>Petugas peminta permintaan (profil tenaga kerja).</summary>
    public Guid? RequestedByWorkforceId { get; set; }

    /// <summary>Status yang dituju oleh kejadian itu, mis. Ditolak atau Selesai.</summary>
    [EnumDataType(typeof(StockRequestStatus))]
    public StockRequestStatus? ToStatus { get; set; }

    /// <summary>Mencari pada nomor permintaan.</summary>
    [MaxLength(100)]
    public string? Search { get; set; }

    /// <summary>Cakupan yang sama dengan daftar permintaan (<see cref="StockRequestPagedQuery.Scope"/>).</summary>
    [EnumDataType(typeof(StockRequestListScope))]
    public StockRequestListScope Scope { get; set; } = StockRequestListScope.All;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ===================================================================== tanggapan

/// <summary>Satu kejadian pada linimasa aktivitas permintaan.</summary>
public class StockRequestHistoryEntryResponse
{
    public Guid Id { get; set; }
    public Guid StockRequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string RequestingServiceUnitName { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public StockRequestPriority Priority { get; set; }
    public string Action { get; set; } = string.Empty;
    public StockRequestStatus? FromStatus { get; set; }
    public StockRequestStatus ToStatus { get; set; }

    /// <summary>Alasan atau catatan yang ditulis pada kejadian itu (alasan CITO, penolakan, temuan, dsb.).</summary>
    public string? Reason { get; set; }

    public DateTime OccurredAt { get; set; }
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
}

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
    /// Gudang tetap tidak memutuskan permintaan. Persetujuan dilakukan pemegang izin persetujuan
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
    /// Waktu keputusan pertama — disetujui atasan, ditolak, atau diotorisasi CITO — dibaca dari
    /// riwayat. Bukan dari kolom <c>DecidedAt</c>, yang menurut perilaku lama ikut ditimpa saat
    /// penyerahan dan pembatalan.
    /// </summary>
    public DateTime? DecisionAt { get; set; }

    /// <summary>Waktu penyerahan gudang (riwayat penyerahan); null bila belum diserahkan.</summary>
    public DateTime? FulfilledAt { get; set; }

    /// <summary>Waktu pembatalan (riwayat pembatalan); null bila tidak dibatalkan.</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Jumlah baris yang diserahkan kurang dari yang diminta; dasar keterangan pemenuhan sebagian.</summary>
    public int ShortfallItemCount { get; set; }

    /// <summary>
    /// Permintaan berstatus Submitted yang diajukan sebelum batas
    /// <c>PharmacyManagement:StockRequest:ApprovalEnforcedFromUtc</c>. Aturan peralihan:
    /// permintaan seperti ini masih boleh diserahkan langsung seperti sebelum persetujuan atasan
    /// berlaku, dan tetap boleh diputuskan atasan. Selalu false bila batas itu tidak diatur.
    /// </summary>
    public bool IsLegacySubmission { get; set; }

    /// <summary>Keadaan verifikasi susulan CITO, dihitung dari riwayat (<c>LGS-DEC-005</c>).</summary>
    public StockRequestEmergencyReviewState EmergencyReviewState { get; set; }

    /// <summary>Waktu otorisasi CITO (UTC, dari riwayat); null bila tidak lewat jalur CITO.</summary>
    public DateTime? EmergencyAuthorizedAt { get; set; }

    /// <summary>Nama petugas yang mengotorisasi CITO.</summary>
    public string? EmergencyAuthorizedByName { get; set; }

    /// <summary>Saat jatuh tempo verifikasi susulan (UTC); null bila tenggat belum dikonfigurasi.</summary>
    public DateTime? EmergencyReviewDueAt { get; set; }

    /// <summary>Keadaan tenggat verifikasi susulan; hanya bermakna selama verifikasi masih menunggu.</summary>
    public StockRequestEmergencyReviewDueState EmergencyReviewDueState { get; set; }
}

/// <summary>
/// Cakupan kewenangan pengguna yang login atas permintaan stok, dihitung backend dari izin
/// aktifnya ("Dalam Kewenangan Saya", FIN-LGS-070). Tidak memuat data pegawai lain.
/// </summary>
public class StockRequestApprovalScopeResponse
{
    /// <summary>
    /// Izin <c>StockRequest:Approve</c> diberikan eksplisit lewat Akses Role: pengguna dapat
    /// menyetujui, menolak, dan memverifikasi susulan permintaan unit mana pun, kecuali miliknya sendiri.
    /// </summary>
    public bool HasApprovePermission { get; set; }

    /// <summary>Izin <c>StockRequest:AuthorizeEmergency</c> diberikan eksplisit lewat Akses Role.</summary>
    public bool HasEmergencyAuthority { get; set; }

    /// <summary>Penjelasan bila cakupan kosong atau tidak lengkap; null bila tidak ada catatan.</summary>
    public string? Message { get; set; }
}

/// <summary>
/// Kesiapan izin dan konfigurasi untuk persetujuan permintaan stok. Hanya membaca; berisi jumlah,
/// tanpa identitas pegawai. Data organisasi HRD tidak lagi dibaca (FIN-LGS-070).
/// </summary>
public class StockRequestApprovalReadinessResponse
{
    /// <summary>Kebijakan akses aktif yang memberi <c>StockRequest:Approve</c>.</summary>
    public int ApprovePermissionGrants { get; set; }

    /// <summary>Kebijakan akses aktif yang memberi <c>StockRequest:AuthorizeEmergency</c>.</summary>
    public int EmergencyPermissionGrants { get; set; }

    /// <summary>Akun aktif yang benar-benar memegang <c>StockRequest:Approve</c>.</summary>
    public int ApproveUsers { get; set; }

    /// <summary>Akun aktif yang benar-benar memegang <c>StockRequest:AuthorizeEmergency</c>.</summary>
    public int EmergencyUsers { get; set; }

    public string ApprovalCutoverState { get; set; } = string.Empty;
    public string EmergencyReviewDeadline { get; set; } = string.Empty;
    public bool IsReadyForApproval { get; set; }
    public bool IsReadyForEmergency { get; set; }
    public List<string> Blockers { get; set; } = [];
}

public class StockRequestDetailResponse : StockRequestSummaryResponse
{
    public Guid RequestedByWorkforceId { get; set; }
    public string? Notes { get; set; }
    /// <summary>
    /// Waktu keputusan penyetuju — disetujui, ditolak, atau diotorisasi CITO (LGS-DEC-002).
    /// Sejak FIN-LGS-015 tidak lagi ditimpa penyerahan maupun pembatalan. Baris lama yang dibuat
    /// sebelum perbaikan itu dapat masih berisi waktu penyerahan/pembatalan; untuk tampilan pakai
    /// <see cref="StockRequestSummaryResponse.DecisionAt"/> yang dibaca dari riwayat.
    /// </summary>
    public DateTime? DecidedAt { get; set; }

    /// <summary>Alasan atau catatan keputusan penyetuju; dengan catatan yang sama untuk baris lama.</summary>
    public string? DecisionReason { get; set; }
    public List<StockRequestItemResponse> Items { get; set; } = [];
    public List<StockRequestHistoryResponse> Histories { get; set; } = [];

    /// <summary>
    /// Pengguna yang sedang membuka adalah pemohon, pembuat, atau pengirim permintaan ini,
    /// sehingga tidak boleh menyetujui, menolak, maupun mengotorisasinya.
    /// </summary>
    public bool IsOwnRequest { get; set; }

    /// <summary>
    /// Pengguna memegang izin otorisasi CITO yang diberikan eksplisit lewat Akses Role.
    /// Hanya dihitung untuk permintaan CITO yang menunggu; selain itu bernilai null.
    /// </summary>
    public bool? HasEmergencyAuthority { get; set; }

    /// <summary>
    /// Pengguna memegang izin <c>StockRequest:Approve</c> yang diberikan eksplisit lewat Akses Role.
    /// Dihitung selama permintaan menunggu persetujuan atau menunggu verifikasi susulan. Bersama
    /// <see cref="IsOwnRequest"/>, inilah seluruh syarat memutus sejak FIN-LGS-070.
    /// </summary>
    public bool? HasApproveAuthority { get; set; }

    /// <summary>
    /// Pengguna boleh melakukan verifikasi susulan CITO atas permintaan ini: izin persetujuan
    /// eksplisit, bukan pemohon, dan bukan pengotorisasi CITO-nya.
    /// Null bila permintaan tidak sedang menunggu verifikasi susulan.
    /// </summary>
    public bool? CanVerifyEmergency { get; set; }

    /// <summary>Sebab pengguna tidak boleh melakukan verifikasi susulan, untuk ditampilkan apa adanya.</summary>
    public string? VerifyEmergencyMessage { get; set; }

    /// <summary>
    /// Alasan CITO yang ditulis pemohon saat mengajukan (wajib untuk prioritas darurat, PHM014),
    /// dibaca dari riwayat pengajuan terakhir. Dasar pertimbangan petugas yang mengotorisasi dan
    /// atasan yang memverifikasi; null untuk permintaan bukan CITO atau yang belum diajukan.
    /// </summary>
    public string? EmergencyRequestReason { get; set; }

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

    /// <summary>
    /// Sisa yang belum diserahkan (diminta − diserahkan), dihitung backend supaya angka diminta,
    /// diserahkan, dan sisa selalu konsisten. Null selama baris belum diserahkan.
    /// </summary>
    public decimal? RemainingQuantity { get; set; }

    public string? Note { get; set; }
    public int LineNumber { get; set; }

    /// <summary>
    /// Stok yang dapat diserahkan untuk obat baris ini di gudang pemenuhan (FIN-LGS-081), dengan
    /// aturan yang sama dengan penyerahan: status Siap Pakai, belum kedaluwarsa, saldo fisik dikurangi
    /// yang ditahan, dalam satuan stok obat. Baris lain dengan obat yang sama memakai stok yang sama.
    /// Hanya diisi selama permintaan dapat diserahkan; null selain itu. Ini informasi untuk petugas —
    /// kecukupan tetap diperiksa backend saat penyerahan (<c>PHM028</c>).
    /// </summary>
    public decimal? AvailableQuantity { get; set; }

    /// <summary>Satuan stok obat, satuan dari <see cref="AvailableQuantity"/>; null bila belum diatur.</summary>
    public string? StockUnitName { get; set; }
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
