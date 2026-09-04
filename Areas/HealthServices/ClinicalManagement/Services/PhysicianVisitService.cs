using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Permintaan mencatat satu kejadian visite dokter.
    /// </summary>
    public sealed class RecordPhysicianVisitCommand
    {
        public Guid EncounterId { get; init; }

        public Guid? InpEpisodeId { get; init; }

        public Guid PatientId { get; init; }

        public Guid DoctorId { get; init; }

        /// <summary>Waktu kedatangan dokter, bukan waktu pencatatan.</summary>
        public DateTime VisitDateTime { get; init; }

        public PhysicianVisitRole VisitRole { get; init; } = PhysicianVisitRole.Dpjp;

        public Guid? ConsultationId { get; init; }

        public Guid? ProgressNoteId { get; init; }

        public Guid? PatientProcedureId { get; init; }

        public string? Note { get; init; }

        /// <summary>Kunci permintaan; wajib terisi.</summary>
        public string IdempotencyKey { get; init; } = string.Empty;

        /// <summary>Kejadian yang digantikan, bila pencatatan ini adalah koreksi.</summary>
        public Guid? CorrectsVisitId { get; init; }
    }

    /// <summary>
    /// Hasil satu perintah pada kejadian visite.
    /// </summary>
    public sealed class PhysicianVisitResult
    {
        public bool IsSuccess { get; init; }

        public int StatusCode { get; init; }

        public string? ErrorMessage { get; init; }

        public CliPhysicianVisit? Visit { get; init; }

        /// <summary>
        /// Benar ketika kejadian yang dikembalikan sudah ada sebelumnya dan permintaan ini
        /// adalah kiriman ulang dengan kunci yang sama.
        /// </summary>
        public bool IsReplay { get; init; }

        internal static PhysicianVisitResult Ok(CliPhysicianVisit visit, bool isReplay = false) => new()
        {
            IsSuccess = true,
            StatusCode = isReplay ? StatusCodes.Status200OK : StatusCodes.Status201Created,
            Visit = visit,
            IsReplay = isReplay
        };

        internal static PhysicianVisitResult Fail(int statusCode, string message) => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = message
        };
    }

    /// <summary>
    /// Pemilik CRUD dan orkestrasi <see cref="CliPhysicianVisit"/> — <c>BE-RWI-041</c>,
    /// <c>CAP-025</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa lewat service, bukan langsung dari controller.</b> Dua controller klinis yang
    /// sudah ada memang menaruh logika bisnisnya di dalam controller dan mengakses
    /// <c>ApplicationDbContext</c> langsung. Itu utang teknis milik modul lain yang sengaja tidak
    /// dirapikan di sini, dan <b>bukan</b> wewenang untuk menirunya: kode baru tetap mengikuti
    /// <c>QBE-SVC-001</c>, sehingga controller visite kelak memanggil service ini.
    /// </para>
    /// <para>
    /// <b>Apa yang belum ada di sini.</b> Endpoint, DTO, dan butir hak aksesnya adalah pekerjaan
    /// <c>BE-RWI-048</c> dan <c>BE-RWI-049</c>. Service ini menyediakan perintah dan pembacaan
    /// yang menjadi dasarnya, tanpa memutuskan kebijakan yang belum disahkan — misalnya
    /// pencatatan atas nama dokter lain, yang kebijakannya memang belum ada.
    /// </para>
    /// <para>
    /// Tidak memakai interface, mengikuti pola service pada repository ini.
    /// </para>
    /// </remarks>
    public class PhysicianVisitService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PhysicianVisitNumberService _numberService;

        public PhysicianVisitService(
            ApplicationDbContext dbContext,
            PhysicianVisitNumberService numberService)
        {
            _dbContext = dbContext;
            _numberService = numberService;
        }

        /// <summary>
        /// Mencatat satu kejadian visite. Kiriman ulang dengan kunci yang sama mengembalikan
        /// kejadian yang sama, bukan kejadian kedua.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Pemeriksaan kunci di dalam aplikasi menangani kiriman ulang biasa; unique index penuh
        /// pada <c>IdempotencyKey</c> yang menangani dua permintaan yang tiba benar-benar
        /// bersamaan. Keduanya diperlukan — pemeriksaan aplikasi saja tidak dapat mencegah
        /// perlombaan.
        /// </para>
        /// <para>
        /// Dua visite nyata pada tanggal yang sama menghasilkan dua baris. Tidak ada penolakan
        /// berdasarkan pasangan perawatan, dokter, dan tanggal — <c>RWI-DEC-085</c>.
        /// </para>
        /// </remarks>
        /// <param name="command">Isi kejadian yang hendak dicatat.</param>
        /// <param name="actorUserId">Pengguna yang mencatat.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<PhysicianVisitResult> RecordAsync(
            RecordPhysicianVisitCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var key = command.IdempotencyKey?.Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                return PhysicianVisitResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kunci permintaan wajib diisi.");
            }

            if (command.EncounterId == Guid.Empty)
            {
                return PhysicianVisitResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kunjungan wajib diisi.");
            }

            if (command.PatientId == Guid.Empty || command.DoctorId == Guid.Empty)
            {
                return PhysicianVisitResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pasien dan dokter wajib diisi.");
            }

            var existing = await _dbContext.CliPhysicianVisits
                .FirstOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);

            if (existing != null)
                return PhysicianVisitResult.Ok(existing, isReplay: true);

            var now = DateTime.UtcNow;

            var visit = new CliPhysicianVisit
            {
                Id = Guid.NewGuid(),
                PhysicianVisitNumber = _numberService.Generate(),
                EncounterId = command.EncounterId,
                InpEpisodeId = command.InpEpisodeId,
                PatientId = command.PatientId,
                DoctorId = command.DoctorId,
                VisitDateTime = command.VisitDateTime == default ? now : command.VisitDateTime,
                VisitRole = command.VisitRole,
                VisitStatus = PhysicianVisitStatus.Recorded,
                ConsultationId = command.ConsultationId,
                ProgressNoteId = command.ProgressNoteId,
                PatientProcedureId = command.PatientProcedureId,
                Note = NormalizeNullableText(command.Note),
                RecordedByUserId = actorUserId,
                IdempotencyKey = key,
                CorrectsVisitId = command.CorrectsVisitId,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.CliPhysicianVisits.Add(visit);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return PhysicianVisitResult.Ok(visit);
        }

        /// <summary>
        /// Membatalkan satu kejadian beserta alasannya. Kejadian tetap tersimpan dan tetap
        /// terbaca pada riwayat — <c>INV-DOK-08</c>.
        /// </summary>
        /// <param name="visitId">Kejadian yang dibatalkan.</param>
        /// <param name="cancelReason">Alasan pembatalan; wajib diisi.</param>
        /// <param name="actorUserId">Pengguna yang membatalkan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<PhysicianVisitResult> CancelAsync(
            Guid visitId,
            string? cancelReason,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var reason = cancelReason?.Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                return PhysicianVisitResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan pembatalan wajib diisi.");
            }

            var visit = await _dbContext.CliPhysicianVisits
                .FirstOrDefaultAsync(x => x.Id == visitId && !x.IsDelete, cancellationToken);

            if (visit == null)
            {
                return PhysicianVisitResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Kejadian visite tidak ditemukan.");
            }

            if (visit.VisitStatus == PhysicianVisitStatus.Cancelled)
            {
                return PhysicianVisitResult.Fail(
                    StatusCodes.Status409Conflict,
                    "Kejadian visite ini sudah dibatalkan.");
            }

            var now = DateTime.UtcNow;

            visit.VisitStatus = PhysicianVisitStatus.Cancelled;
            visit.CancelledAt = now;
            visit.CancelledByUserId = actorUserId;
            visit.CancelReason = reason;
            visit.UpdateDateTime = now;
            visit.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return PhysicianVisitResult.Ok(visit, isReplay: true);
        }

        /// <summary>
        /// Riwayat kejadian visite satu perawatan, terurut waktu kedatangan menaik. Kejadian
        /// yang dibatalkan <b>ikut ditampilkan</b> beserta alasannya.
        /// </summary>
        /// <param name="episodeId">Perawatan yang riwayatnya dibaca.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<List<CliPhysicianVisit>> GetByEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.CliPhysicianVisits
                .AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.VisitDateTime)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Menghitung kejadian visite satu perawatan.
        /// </summary>
        /// <remarks>
        /// Hitungan diturunkan dari kejadian, <b>bukan</b> dari catatan yang ditulis dokter —
        /// <c>INV-DOK-07</c>. Kejadian yang dibatalkan tidak ikut dihitung, sedangkan dua visite
        /// nyata pada tanggal yang sama dihitung dua.
        /// </remarks>
        /// <param name="episodeId">Perawatan yang dihitung.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<int> CountRecordedByEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.CliPhysicianVisits
                .AsNoTracking()
                .CountAsync(x =>
                    x.InpEpisodeId == episodeId &&
                    !x.IsDelete &&
                    x.VisitStatus == PhysicianVisitStatus.Recorded,
                    cancellationToken);
        }

        private static string? NormalizeNullableText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
