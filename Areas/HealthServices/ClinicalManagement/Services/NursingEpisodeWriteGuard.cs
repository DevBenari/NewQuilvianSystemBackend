using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Hasil satu perintah pada layanan keperawatan rawat inap revision 7 — <c>BE-RWI-107</c>
    /// s.d. <c>BE-RWI-123</c>.
    /// </summary>
    /// <remarks>
    /// Satu bentuk untuk seluruh service baru supaya controller membalas dengan cara yang sama:
    /// kode HTTP, kalimat untuk pengguna, dan kode alasan pada isian <c>errors.code</c>.
    /// </remarks>
    public sealed class NursingResult<T>
    {
        public bool IsSuccess { get; init; }

        public int StatusCode { get; init; }

        public string? ErrorMessage { get; init; }

        /// <summary>Kode alasan yang dapat dibaca mesin, misalnya <c>STALE_REVISION</c>.</summary>
        public string? ErrorCode { get; init; }

        public T? Value { get; init; }

        /// <summary><c>true</c> bila hasilnya kiriman ulang berkunci sama, bukan perubahan baru.</summary>
        public bool IsReplay { get; init; }

        public string Message { get; init; } = string.Empty;

        /// <summary>Isian <c>errors</c> pada <c>ApiResponse</c>.</summary>
        public object? Errors => ErrorCode == null ? null : new { code = ErrorCode };

        public static NursingResult<T> Ok(T value, string message, int statusCode = StatusCodes.Status200OK, bool isReplay = false) => new()
        {
            IsSuccess = true,
            StatusCode = isReplay ? StatusCodes.Status200OK : statusCode,
            Value = value,
            Message = message,
            IsReplay = isReplay
        };

        public static NursingResult<T> Fail(int statusCode, string message, string? errorCode = null) => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = message,
            ErrorCode = errorCode
        };

        public NursingResult<TOther> Cast<TOther>() => NursingResult<TOther>.Fail(StatusCode, ErrorMessage ?? string.Empty, ErrorCode);
    }

    /// <summary>
    /// Konteks penulisan yang sudah lolos penjaga: episode, pasien, unit saat ini, dan pelaku.
    /// </summary>
    public sealed class NursingWriteContext
    {
        public Guid EpisodeId { get; init; }

        public Guid EncounterId { get; init; }

        public Guid PatientId { get; init; }

        /// <summary>Unit episode <b>saat simpan</b>, bukan salinan pada dokumen.</summary>
        public Guid ServiceUnitId { get; init; }

        public DateTime? AdmittedAt { get; init; }

        public InpEpisodeStatus EpisodeStatus { get; init; }

        public Guid ActorEmployeeId { get; init; }

        public Guid ActorUserId { get; init; }
    }

    /// <summary>
    /// Penjaga bersama seluruh jalur tulis keperawatan rawat inap revision 7 — <c>VAL-KEP-01</c>
    /// s.d. <c>VAL-KEP-03</c>, <c>VAL-KEP-05</c> sebagaimana diubah <c>RWI-DEC-100</c>, dan
    /// <c>INT-KEP-07</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Urutan pemeriksaannya.</b> Episode ada (<c>404</c>) → episode berjalan: <c>Draft</c> dan
    /// episode tertutup dijawab <c>422</c> → pengguna tertaut pegawai (<c>403</c>) → pegawai
    /// ditempatkan di unit episode <b>saat ini</b> (<c>403</c>). Pasien yang dipindah ke Anggrek pukul
    /// 14.00 tidak lagi dapat ditulisi perawat Melati pukul 14.10.
    /// </para>
    /// <para>
    /// <b>Kegagalan membaca penempatan tidak pernah jatuh ke "izinkan"</b> — pengecualian dibiarkan
    /// naik dan tidak ditangkap di sini. Tidak ada nama peran yang dibaca; hak akses tetap ditentukan
    /// layar Akses Role, dan yang dijaga di sini hanya kewenangan yang melekat pada data.
    /// </para>
    /// </remarks>
    public class NursingEpisodeWriteGuard
    {
        public const string KodeEpisodeTidakBerjalan = "EPISODE_NOT_ACTIVE";
        public const string KodeTanpaPegawai = "NURSE_NOT_LINKED_TO_EMPLOYEE";
        public const string KodeUnitLain = "NURSE_UNIT_NOT_ASSIGNED";

        public const string PenolakanUnitLain = "Anda tidak ditempatkan di unit pasien ini.";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _contextService;
        private readonly NursingActorService _actorService;

        public NursingEpisodeWriteGuard(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService contextService,
            NursingActorService actorService)
        {
            _dbContext = dbContext;
            _contextService = contextService;
            _actorService = actorService;
        }

        /// <summary>Menegakkan penjaga tulis atas satu episode.</summary>
        public async Task<NursingResult<NursingWriteContext>> EnsureCanWriteAsync(
            Guid episodeId,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default,
            string unitRejectionMessage = PenolakanUnitLain)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.EncounterId, x.PatientId, x.ServiceUnitId, x.AdmittedAt, x.EpisodeStatus })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
            {
                return NursingResult<NursingWriteContext>.Fail(
                    StatusCodes.Status404NotFound,
                    "Perawatan rawat inap tidak ditemukan.");
            }

            var penolakanEpisode = EpisodeRejection(episode.EpisodeStatus);

            if (penolakanEpisode != null)
            {
                return NursingResult<NursingWriteContext>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    penolakanEpisode,
                    KodeEpisodeTidakBerjalan);
            }

            var employeeId = await _actorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

            if (!employeeId.HasValue)
            {
                return NursingResult<NursingWriteContext>.Fail(
                    StatusCodes.Status403Forbidden,
                    InpatientClinicalContextService.PenolakanPerawatTanpaPegawai,
                    KodeTanpaPegawai);
            }

            var ditempatkan = await _contextService.IsEmployeeAssignedToUnitAsync(
                episode.ServiceUnitId, employeeId.Value, DateTime.UtcNow, cancellationToken);

            if (!ditempatkan)
            {
                return NursingResult<NursingWriteContext>.Fail(
                    StatusCodes.Status403Forbidden,
                    unitRejectionMessage,
                    KodeUnitLain);
            }

            return NursingResult<NursingWriteContext>.Ok(new NursingWriteContext
            {
                EpisodeId = episode.Id,
                EncounterId = episode.EncounterId,
                PatientId = episode.PatientId,
                ServiceUnitId = episode.ServiceUnitId,
                AdmittedAt = episode.AdmittedAt,
                EpisodeStatus = episode.EpisodeStatus,
                ActorEmployeeId = employeeId.Value,
                ActorUserId = actorUserId
            }, string.Empty);
        }

        /// <summary>
        /// Kalimat penolakan <c>VAL-KEP-02</c>/<c>VAL-KEP-03</c>, atau <c>null</c> bila episode berjalan.
        /// <c>DischargePending</c> tetap berjalan: pasien masih di kamar sampai benar-benar pulang.
        /// </summary>
        public static string? EpisodeRejection(InpEpisodeStatus status) => status switch
        {
            InpEpisodeStatus.Admitted or InpEpisodeStatus.DischargePending => null,
            InpEpisodeStatus.Draft =>
                "Pasien belum dikonfirmasi tiba di kamar. Catatan keperawatan dapat dibuat setelah pasien benar-benar masuk.",
            _ => "Perawatan pasien ini sudah ditutup. Catatannya hanya dapat dibaca."
        };

        /// <summary>Kunci permintaan dari badan permintaan atau header <c>Idempotency-Key</c>, dirapikan.</summary>
        public static string? NormalizeIdempotencyKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var kunci = value.Trim();
            return kunci.Length > 100 ? kunci[..100] : kunci;
        }
    }
}
