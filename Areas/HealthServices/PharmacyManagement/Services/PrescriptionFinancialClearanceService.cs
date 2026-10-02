using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Mengonsumsi surat clearance dari Billing dan melepaskan resep dari keadaan menunggu
/// pembayaran (PHA-BE-004, PHA-DEC-063, PHA-DEC-064, PHA-DEC-065).
/// </summary>
/// <remarks>
/// <para>
/// Sebelum ini tidak ada satu pun jalur kode yang memindahkan resep keluar dari
/// <c>WaitingForPayment</c>, sehingga seluruh resep rawat jalan macet permanen. Empat endpoint
/// yang dulu dapat menyatakan resep lunas dihapus permanen oleh keputusan pemilik, dan tidak
/// dihidupkan kembali di sini: satu-satunya masukan yang diterima service ini adalah surat dari
/// Billing.
/// </para>
/// <para>
/// Farmasi <strong>tidak</strong> menyimpulkan keadaan finansial sendiri. Tagihan, tender, dan
/// alokasi pembayaran tidak dibaca; yang dibaca hanya surat clearance, dan hasil finansialnya
/// disalin apa adanya. Penentuan lunas versus disetujui penjamin milik Billing (PHA-DEC-065).
/// </para>
/// <para>
/// Surat bernomor versi lebih rendah diabaikan diam-diam. Surat dapat tiba tidak berurutan, dan
/// mengabaikan yang lebih tua adalah perilaku yang benar — bukan kegagalan, tidak dicatat sebagai
/// galat, tidak memicu percobaan ulang (<c>PHA_CLR_STALE_VERSION</c>).
/// </para>
/// <para>
/// Pencabutan tidak menarik keadaan pemenuhan mundur. Resep ditahan di tempat, dan penahanannya
/// sendiri adalah pekerjaan <c>PHA-BE-005</c>; service ini hanya membuat keadaan yang benar
/// terbaca. Obat yang sudah diserahkan tidak berubah apa pun (PHA-DEC-069).
/// </para>
/// <para>
/// Service ini <strong>tidak menulis satu baris pun</strong> ke tabel Billing, termasuk tidak
/// menandai surat sebagai sudah diakui. Itu batas yang ditetapkan Definition of Done
/// <c>PHA-BE-004</c>. Konsumsinya dibuat idempoten lewat penjagaan nomor versi, sehingga surat
/// yang terbaca berulang tidak mengubah apa pun.
/// </para>
/// </remarks>
public sealed class PrescriptionFinancialClearanceService
{
    private readonly ApplicationDbContext _dbContext;

    public PrescriptionFinancialClearanceService(ApplicationDbContext dbContext)
        => _dbContext = dbContext;

    /// <summary>
    /// Menilai apakah sebuah gerbang pekerjaan Farmasi boleh dilewati (PHA-BE-005).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Penanda tahan finansial <strong>dihitung</strong> dari salinan, tidak disimpan sebagai
    /// kolom penanda pada resep. Menyimpannya berarti menambah keadaan kedua yang dapat
    /// menyimpang dari salinan yang otoritatif.
    /// </para>
    /// <para>
    /// Hanya <c>CLEARED</c> beserta hasil finansial yang dikenali yang meloloskan. <c>UNKNOWN</c>,
    /// <c>PENDING_VERIFICATION</c>, dan <c>STALE</c> menolak — ketiadaan surat bukan izin
    /// (PHA-DEC-067). Tidak ada override bagi peran mana pun, termasuk Kepala Farmasi dan
    /// Supervisor; ketiadaan permukaannya memang disengaja.
    /// </para>
    /// </remarks>
    public async Task<PrescriptionClearanceGateResult> EvaluateGateAsync(
        Guid prescriptionId,
        PrescriptionClearanceGate gate,
        CancellationToken ct = default)
    {
        var clearance = await DescribeAsync(prescriptionId, ct);

        if (clearance.IsCleared)
        {
            return new PrescriptionClearanceGateResult(true, null, null);
        }

        // Keadaan yang punya alasan sendiri — ditahan, belum diketahui, tertinggal — memakai
        // kalimatnya sendiri apa pun gerbangnya. Sisanya memakai kalimat gerbang yang dilewati.
        if (clearance.HoldReasonCode is not null
            and not ClearanceHoldReasonCodes.NotCleared)
        {
            return new PrescriptionClearanceGateResult(
                false, clearance.HoldReasonCode, clearance.HoldReason);
        }

        return gate == PrescriptionClearanceGate.Dispensing
            ? new PrescriptionClearanceGateResult(
                false,
                ClearanceHoldReasonCodes.NotSettled,
                "Obat belum dapat diserahkan karena pembayarannya belum beres.")
            : new PrescriptionClearanceGateResult(
                false,
                ClearanceHoldReasonCodes.NotCleared,
                "Resep ini belum dapat ditelaah karena pembayarannya belum dikonfirmasi kasir.");
    }

    /// <summary>
    /// Menolak pekerjaan yang tidak boleh dilanjutkan karena keadaan finansialnya.
    /// </summary>
    public async Task EnsureGateAllowedAsync(
        Guid prescriptionId,
        PrescriptionClearanceGate gate,
        CancellationToken ct = default)
    {
        var result = await EvaluateGateAsync(prescriptionId, gate, ct);

        if (!result.Allowed)
        {
            throw new InvalidOperationException(result.Message);
        }
    }

    /// <summary>
    /// Membaca keadaan finansial sebuah resep untuk ditampilkan di layar kerja (PHA-BE-006).
    /// </summary>
    /// <remarks>
    /// Baca murni. Resep yang belum pernah memiliki surat dijawab <c>UNKNOWN</c> beserta alasan
    /// penahanannya — bukan galat, bukan <c>404</c>, dan bukan izin.
    /// </remarks>
    public async Task<PrescriptionFinancialClearanceResponse> DescribeAsync(
        Guid prescriptionId,
        CancellationToken ct = default)
    {
        var projection = await _dbContext.PhmPrescriptionFinancialProjections.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PrescriptionId == prescriptionId && !x.IsDelete, ct);

        if (projection == null)
        {
            return new PrescriptionFinancialClearanceResponse
            {
                ClearanceStatus = PrescriptionClearanceProjectionStatuses.Unknown,
                IsCleared = false,
                IsKnown = false,
                HoldReasonCode = ClearanceHoldReasonCodes.Unknown,
                HoldReason = "Keadaan pembayaran resep ini belum diketahui. Obat belum boleh diserahkan.",
                SyncState = PrescriptionClearanceSyncStates.NeverSynced
            };
        }

        var isCleared = projection.ClearanceStatus == PrescriptionClearanceProjectionStatuses.Cleared
            && MapFinancialOutcome(projection.FinancialOutcome) != null;

        var (holdCode, holdReason) = DescribeHold(projection, isCleared);

        return new PrescriptionFinancialClearanceResponse
        {
            ClearanceStatus = projection.ClearanceStatus,
            FinancialOutcome = projection.FinancialOutcome,
            ReasonCode = projection.ReasonCode,
            IsCleared = isCleared,
            IsKnown = projection.ClearanceStatus != PrescriptionClearanceProjectionStatuses.Unknown,
            HoldReasonCode = holdCode,
            HoldReason = holdReason,
            SyncState = projection.SyncState,
            SyncedAt = projection.SyncedAt,
            FinancialVersion = projection.FinancialVersion,
            EffectiveAt = projection.EffectiveAt
        };
    }

    /// <summary>
    /// Alasan pekerjaan tertahan, mengikuti <c>PHA-VAL-CLEARANCE-v1</c> kata demi kata.
    /// </summary>
    private static (string? Code, string? Reason) DescribeHold(
        PhmPrescriptionFinancialProjection projection,
        bool isCleared)
    {
        if (isCleared)
        {
            return (null, null);
        }

        return projection.ClearanceStatus switch
        {
            PrescriptionClearanceProjectionStatuses.Revoked => (
                ClearanceHoldReasonCodes.OnHold,
                "Resep ini ditahan karena ada perubahan tagihan. Pekerjaan dilanjutkan setelah kasir menyelesaikannya."),
            PrescriptionClearanceProjectionStatuses.Stale => (
                ClearanceHoldReasonCodes.Stale,
                "Data pembayaran sedang tidak dapat dipastikan. Coba beberapa saat lagi; jangan menyerahkan obat dulu."),
            PrescriptionClearanceProjectionStatuses.PendingVerification => (
                ClearanceHoldReasonCodes.Stale,
                "Data pembayaran sedang tidak dapat dipastikan. Coba beberapa saat lagi; jangan menyerahkan obat dulu."),
            PrescriptionClearanceProjectionStatuses.Unknown => (
                ClearanceHoldReasonCodes.Unknown,
                "Keadaan pembayaran resep ini belum diketahui. Obat belum boleh diserahkan."),
            _ => (
                ClearanceHoldReasonCodes.NotCleared,
                "Resep ini belum dapat ditelaah karena pembayarannya belum dikonfirmasi kasir.")
        };
    }

    /// <summary>
    /// Mengonsumsi surat clearance untuk satu resep. Aman dipanggil berulang.
    /// </summary>
    public Task<PrescriptionClearanceConsumeResult> ConsumeForPrescriptionAsync(
        Guid prescriptionId,
        Guid actorUserId,
        CancellationToken ct = default)
        => ConsumeAsync(prescriptionId, actorUserId, maxLetters: null, ct);

    /// <summary>
    /// Mengonsumsi surat clearance yang belum tersalin, untuk seluruh resep.
    /// </summary>
    /// <remarks>
    /// Dipanggil di dalam proses saat dibutuhkan. Kontrak <c>PHA-API-CLEARANCE-v1</c> menolak
    /// tombol sinkronisasi manual, karena tombol semacam itu mengundang kebiasaan menekannya
    /// sampai hasilnya menyenangkan.
    /// </remarks>
    public Task<PrescriptionClearanceConsumeResult> ConsumePendingAsync(
        Guid actorUserId,
        int maxLetters = 200,
        CancellationToken ct = default)
        => ConsumeAsync(prescriptionId: null, actorUserId, maxLetters, ct);

    private async Task<PrescriptionClearanceConsumeResult> ConsumeAsync(
        Guid? prescriptionId,
        Guid actorUserId,
        int? maxLetters,
        CancellationToken ct)
    {
        var letterSource = _dbContext.BilPrescriptionClearanceHandoffs.AsNoTracking()
            .Where(x => !x.IsDelete);

        if (prescriptionId.HasValue)
        {
            letterSource = letterSource.Where(x => x.PrescriptionId == prescriptionId.Value);
        }

        // Hanya surat yang BELUM tersalin yang dibaca. Tanpa saringan ini sapuan bertahap
        // tidak pernah maju: ia akan mengambil 200 surat terlama berulang kali — seluruhnya
        // sudah tersalin dan langsung diabaikan — sementara surat yang benar-benar baru tidak
        // pernah terbaca. Penyaringannya memakai nomor versi yang sama yang dipakai penjagaan
        // di bawah, sehingga keduanya tidak mungkin berbeda pendapat.
        var query =
            from letter in letterSource
            join projection in _dbContext.PhmPrescriptionFinancialProjections.AsNoTracking()
                    .Where(p => !p.IsDelete)
                on letter.PrescriptionId equals projection.PrescriptionId into matches
            from projection in matches.DefaultIfEmpty()
            where projection == null
                || projection.SyncState == PrescriptionClearanceSyncStates.NeverSynced
                || letter.FinancialVersion > projection.FinancialVersion
            // Diurutkan menaik menurut nomor versi supaya urutan penerapannya benar ketika
            // beberapa surat untuk satu resep terbaca dalam sapuan yang sama.
            orderby letter.PrescriptionId, letter.FinancialVersion
            select letter;

        if (maxLetters.HasValue)
        {
            query = query.Take(maxLetters.Value);
        }

        var letters = await query.ToListAsync(ct);
        var result = new PrescriptionClearanceConsumeResult();

        foreach (var letter in letters)
        {
            await ApplyLetterWithRetryAsync(letter, actorUserId, result, ct);
        }

        return result;
    }

    /// <summary>
    /// Menerapkan satu surat, dengan satu kali pembacaan ulang bila salinannya berubah di
    /// tengah jalan.
    /// </summary>
    /// <remarks>
    /// Dua surat untuk resep yang sama yang diproses bersamaan berakhir pada tepat satu baris:
    /// yang kalah membuang perubahannya, membaca ulang salinan yang sudah dimenangkan pihak
    /// lain, lalu mengambil keputusan yang benar atas nomor versinya — diterapkan bila lebih
    /// tinggi, diabaikan bila lebih rendah (PHA-AT-CLR-11).
    /// </remarks>
    private async Task ApplyLetterWithRetryAsync(
        BilPrescriptionClearanceHandoff letter,
        Guid actorUserId,
        PrescriptionClearanceConsumeResult result,
        CancellationToken ct)
    {
        const int maxAttempts = 2;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await ApplyLetterAsync(letter, actorUserId, result, ct);
                return;
            }
            catch (DbUpdateException) when (attempt < maxAttempts)
            {
                result.ConcurrencyRetries++;
                _dbContext.ChangeTracker.Clear();
            }
        }
    }

    private async Task ApplyLetterAsync(
        BilPrescriptionClearanceHandoff letter,
        Guid actorUserId,
        PrescriptionClearanceConsumeResult result,
        CancellationToken ct)
    {
        var prescription = await _dbContext.Set<PhmPrescription>()
            .FirstOrDefaultAsync(x => x.Id == letter.PrescriptionId && !x.IsDelete, ct);

        if (prescription == null)
        {
            // Resep tidak dikenal Farmasi. Tidak ada salinan yang dapat dibuat — barisnya
            // menunjuk resep lewat foreign key. Suratnya tetap utuh di sisi Billing.
            result.SkippedUnknownPrescription++;
            return;
        }

        var projection = await _dbContext.PhmPrescriptionFinancialProjections
            .FirstOrDefaultAsync(x => x.PrescriptionId == letter.PrescriptionId, ct);

        var isFirstLetter = projection == null
            || projection.SyncState == PrescriptionClearanceSyncStates.NeverSynced;

        // Surat pertama diterima apa pun nomor versinya; sesudahnya hanya nomor versi yang
        // lebih tinggi yang mengubah salinan.
        if (!isFirstLetter && letter.FinancialVersion <= projection!.FinancialVersion)
        {
            result.IgnoredStaleVersion++;
            return;
        }

        var now = DateTimeOffset.UtcNow;

        if (projection == null)
        {
            projection = new PhmPrescriptionFinancialProjection
            {
                Id = Guid.NewGuid(),
                PrescriptionId = letter.PrescriptionId,
                CreateDateTime = now.UtcDateTime,
                CreateBy = actorUserId
            };
            _dbContext.PhmPrescriptionFinancialProjections.Add(projection);
        }

        projection.InvoiceId = letter.InvoiceId;
        projection.ClearanceStatus = letter.ClearanceStatus;
        projection.FinancialOutcome = letter.FinancialOutcome;
        projection.ReasonCode = letter.ReasonCode;
        projection.FinancialVersion = letter.FinancialVersion;
        projection.EffectiveAt = letter.EffectiveAt;
        projection.SourceHandoffId = letter.Id;
        projection.CorrelationId = letter.CorrelationId;
        projection.LastAttemptAt = now;
        projection.RowVersion = Guid.NewGuid();
        projection.UpdateDateTime = now.UtcDateTime;
        projection.UpdateBy = actorUserId;

        var isCleared = letter.ClearanceStatus == PrescriptionClearanceProjectionStatuses.Cleared;
        var settledPaymentStatus = MapFinancialOutcome(letter.FinancialOutcome);

        if (isCleared && settledPaymentStatus == null)
        {
            // Surat menyatakan boleh dikerjakan tetapi hasil finansialnya tidak dikenali.
            // Fail-closed: salinan dicatat apa adanya, tetapi tidak dijadikan izin dan resep
            // tidak dipindahkan (PHA-DEC-067). Farmasi tidak menebak hasil yang dimaksud.
            projection.SyncState = PrescriptionClearanceSyncStates.Failed;
            projection.RetryCount++;
            projection.ErrorMessage =
                "Hasil finansial pada surat clearance tidak dikenali: "
                + (string.IsNullOrWhiteSpace(letter.FinancialOutcome) ? "(kosong)" : letter.FinancialOutcome);

            await _dbContext.SaveChangesAsync(ct);
            result.FailedUnrecognizedOutcome++;
            return;
        }

        projection.SyncState = PrescriptionClearanceSyncStates.Synced;
        projection.SyncedAt = now;
        projection.RetryCount = 0;
        projection.ErrorMessage = null;

        if (isCleared)
        {
            // Salinan kenyamanan pada resep. Nilainya menyalin hasil yang ditentukan Billing —
            // hasil penjaminan tercatat sebagai disetujui penjamin, bukan lunas tunai.
            prescription.PaymentStatus = settledPaymentStatus!.Value;
            prescription.UpdateDateTime = DateTime.UtcNow;
            prescription.UpdateBy = actorUserId;

            // Inilah transisi yang selama ini tidak punya jalur kode. Resep yang sudah bergerak
            // lebih jauh dibiarkan pada keadaannya; surat pemulihan tidak mengulang antrean,
            // telaah, maupun penyiapan yang sudah selesai (PHA-DEC-069).
            if (prescription.FulfillmentStatus == PrescriptionFulfillmentStatus.WaitingForPayment)
            {
                prescription.FulfillmentStatus = PrescriptionFulfillmentStatus.QueuedAtPharmacy;
                result.ReleasedToQueue++;
            }

            result.Cleared++;
        }
        else
        {
            // Pencabutan tidak menurunkan keadaan pemenuhan dan tidak mengembalikan racikan
            // menjadi bahan. Penahanan pekerjaannya dipasang PHA-BE-005 sebagai gerbang yang
            // membaca salinan ini; keadaan pemenuhan sengaja dibiarkan apa adanya.
            result.Revoked++;
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Memetakan hasil finansial dari surat ke salinan kenyamanan pada resep.
    /// </summary>
    /// <remarks>
    /// Pemetaan satu-satu tanpa penafsiran. Nilai yang tidak dikenali menghasilkan
    /// <c>null</c>, dan penanganannya fail-closed di pemanggil.
    /// </remarks>
    private static PrescriptionPaymentStatus? MapFinancialOutcome(string? financialOutcome)
        => financialOutcome switch
        {
            PrescriptionFinancialOutcomes.Paid => PrescriptionPaymentStatus.Paid,
            PrescriptionFinancialOutcomes.InsuranceApproved => PrescriptionPaymentStatus.InsuranceApproved,
            PrescriptionFinancialOutcomes.PaymentWaived => PrescriptionPaymentStatus.PaymentWaived,
            _ => null
        };
}

/// <summary>
/// Empat titik pekerjaan Farmasi yang dijaga keadaan finansial (PHA-DES-005).
/// </summary>
/// <remarks>
/// Gerbangnya empat, bukan hanya penyerahan. Menjaga penyerahan saja berarti membiarkan apoteker
/// menelaah dan meracik obat yang izinnya sudah dicabut, lalu pekerjaannya terbuang.
/// </remarks>
public enum PrescriptionClearanceGate
{
    Review,
    Preparation,
    FinalCheck,
    Dispensing
}

/// <summary>Hasil penilaian satu gerbang.</summary>
public sealed record PrescriptionClearanceGateResult(bool Allowed, string? Code, string? Message);

/// <summary>
/// Kode alasan penahanan pekerjaan, mengikuti <c>PHA-VAL-CLEARANCE-v1</c>.
/// </summary>
/// <remarks>
/// <c>PHA_CLR_NO_OVERRIDE</c> sengaja tidak ada di sini: ia bukan pesan penolakan melainkan
/// ketiadaan permukaan. Tidak ada tombol yang dapat ditekan siapa pun, termasuk Kepala Farmasi
/// dan Supervisor (PHA-DEC-067).
/// </remarks>
public static class ClearanceHoldReasonCodes
{
    public const string NotCleared = "PHA_CLR_NOT_CLEARED";
    public const string OnHold = "PHA_CLR_ON_HOLD";
    public const string NotSettled = "PHA_CLR_NOT_SETTLED";
    public const string Unknown = "PHA_CLR_UNKNOWN";
    public const string Stale = "PHA_CLR_STALE";
}

/// <summary>
/// Hasil satu sapuan konsumsi surat clearance. Angka-angka ini bahan verifikasi, bukan bagian
/// kontrak API mana pun.
/// </summary>
public sealed class PrescriptionClearanceConsumeResult
{
    /// <summary>Surat yang menyatakan resep boleh dikerjakan dan berhasil disalin.</summary>
    public int Cleared { get; set; }

    /// <summary>Surat pencabutan yang berhasil disalin.</summary>
    public int Revoked { get; set; }

    /// <summary>Resep yang benar-benar terlepas dari keadaan menunggu pembayaran.</summary>
    public int ReleasedToQueue { get; set; }

    /// <summary>Surat bernomor versi lebih rendah yang diabaikan. Keadaan normal.</summary>
    public int IgnoredStaleVersion { get; set; }

    /// <summary>Surat yang menunjuk resep yang tidak dikenal Farmasi.</summary>
    public int SkippedUnknownPrescription { get; set; }

    /// <summary>Surat menyatakan boleh dikerjakan tetapi hasil finansialnya tidak dikenali.</summary>
    public int FailedUnrecognizedOutcome { get; set; }

    /// <summary>Penulisan yang kalah karena salinan berubah di tengah jalan.</summary>
    public int ConcurrencyRetries { get; set; }
}
