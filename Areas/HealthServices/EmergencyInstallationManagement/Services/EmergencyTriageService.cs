using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Validasi triage, retriage, snapshot target waktu ATS/ESI, dan transisi status triage.
    /// </summary>
    public class EmergencyTriageService
    {
        private readonly ApplicationDbContext _dbContext;

        public EmergencyTriageService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyTriageRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (request.TriageLevelId == Guid.Empty)
                return "TriageLevelId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyTriageSystem), request.TriageSystem))
                return "Nilai TriageSystem tidak valid.";

            if (!Enum.IsDefined(typeof(EmergencyTriageStatus), request.TriageStatus))
                return "Nilai TriageStatus tidak valid.";

            var visitExists = await _dbContext.Set<TrxEmergencyVisit>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.EmergencyVisitId &&
                         !x.IsDelete &&
                         x.VisitStatus != EmergencyVisitStatus.Disposed &&
                         x.VisitStatus != EmergencyVisitStatus.Cancelled,
                    cancellationToken);

            if (!visitExists)
                return "EmergencyVisitId tidak ditemukan atau kunjungan sudah ditutup.";

            var level = await _dbContext.Set<MstEmergencyTriageLevel>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.TriageLevelId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (level == null)
                return "TriageLevelId tidak ditemukan atau tidak aktif.";

            if (request.PatientVitalSignId.HasValue &&
                request.PatientVitalSignId.Value != Guid.Empty &&
                !await _dbContext.Set<TrxPatientVitalSign>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.PatientVitalSignId.Value && !x.IsDelete, cancellationToken))
                return "PatientVitalSignId tidak ditemukan.";

            if (request.IsRetriage &&
                (!request.PreviousTriageId.HasValue || request.PreviousTriageId.Value == Guid.Empty))
                return "PreviousTriageId wajib diisi untuk proses retriage.";

            if (request.PreviousTriageId.HasValue && request.PreviousTriageId.Value != Guid.Empty)
            {
                var previous = await _dbContext.Set<TrxEmergencyTriage>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.PreviousTriageId.Value && !x.IsDelete,
                        cancellationToken);

                if (previous == null)
                    return "PreviousTriageId tidak ditemukan.";

                if (previous.EmergencyVisitId != request.EmergencyVisitId)
                    return "PreviousTriageId harus berasal dari kunjungan IGD yang sama.";
            }

            return null;
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencyTriageRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyTriageRequest)request, cancellationToken);

        public async Task<MstEmergencyTriageLevel> GetTriageLevelAsync(
            Guid triageLevelId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<MstEmergencyTriageLevel>()
                .AsNoTracking()
                .FirstAsync(
                    x => x.Id == triageLevelId && !x.IsDelete && x.IsActive,
                    cancellationToken);
        }

        /// <summary>
        /// Menilai ulang pasien. Penilaian lama tidak pernah ditimpa; ia hanya berubah status
        /// menjadi <c>Superseded</c>, dan penilaian baru dibuat sebagai baris tersendiri yang
        /// menunjuk baris lama. Seluruh aturan penolakan berada di sini, bukan di controller,
        /// supaya pemanggil lain tidak dapat melewatinya.
        /// </summary>
        public async Task<RetriageOutcome> RetriageAsync(
            Guid triageId,
            RetriageEmergencyTriageRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var previous = await _dbContext.Set<TrxEmergencyTriage>()
                .FirstOrDefaultAsync(x => x.Id == triageId && !x.IsDelete, cancellationToken);

            if (previous == null)
                return RetriageOutcome.NotFound("Data triage IGD tidak ditemukan.");

            // Penilaian yang dibatalkan tidak pernah berlaku, sehingga tidak dapat digantikan.
            if (previous.TriageStatus == EmergencyTriageStatus.Cancelled)
                return RetriageOutcome.Conflict("Penilaian triage yang sudah dibatalkan tidak dapat dinilai ulang.");

            // Termasuk menutup penilaian yang sudah Superseded, Draft, dan InProgress.
            if (previous.TriageStatus != EmergencyTriageStatus.Completed)
                return RetriageOutcome.Conflict("Hanya penilaian yang sudah selesai yang dapat dinilai ulang.");

            if (!CanTransition(previous.TriageStatus, EmergencyTriageStatus.Superseded))
                return RetriageOutcome.Conflict("Hanya penilaian yang sudah selesai yang dapat dinilai ulang.");

            var visit = await _dbContext.Set<TrxEmergencyVisit>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == previous.EmergencyVisitId && !x.IsDelete, cancellationToken);

            if (visit == null)
                return RetriageOutcome.Conflict("Kunjungan IGD milik penilaian ini tidak ditemukan.");

            if (visit.VisitStatus == EmergencyVisitStatus.Disposed ||
                visit.VisitStatus == EmergencyVisitStatus.Cancelled)
                return RetriageOutcome.Conflict("Kunjungan IGD sudah ditutup, sehingga tidak dapat dinilai ulang.");

            if (request.TriageLevelId == Guid.Empty)
                return RetriageOutcome.BadRequest("TriageLevelId wajib diisi.");

            var level = await _dbContext.Set<MstEmergencyTriageLevel>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.TriageLevelId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (level == null)
                return RetriageOutcome.BadRequest("TriageLevelId tidak ditemukan atau tidak aktif.");

            if (request.PatientVitalSignId.HasValue &&
                request.PatientVitalSignId.Value != Guid.Empty &&
                !await _dbContext.Set<TrxPatientVitalSign>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.PatientVitalSignId.Value && !x.IsDelete, cancellationToken))
                return RetriageOutcome.BadRequest("PatientVitalSignId tidak ditemukan.");

            var now = DateTime.UtcNow;

            var nextSequence = (await _dbContext.Set<TrxEmergencyTriage>()
                .Where(x => x.EmergencyVisitId == previous.EmergencyVisitId && !x.IsDelete)
                .Select(x => (int?)x.Sequence)
                .MaxAsync(cancellationToken) ?? 0) + 1;

            var startedAt = request.StartedAt.HasValue && request.StartedAt.Value != default
                ? request.StartedAt.Value
                : now;

            var retriage = new TrxEmergencyTriage
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = previous.EmergencyVisitId,
                TriageLevelId = level.Id,
                PatientVitalSignId = request.PatientVitalSignId,
                Sequence = nextSequence,
                IsRetriage = true,
                PreviousTriageId = previous.Id,
                TriageSystem = level.TriageSystem,
                TriageStatus = EmergencyTriageStatus.Draft,
                StartedAt = startedAt,
                MaxWaitingMinutesSnapshot = level.MaxWaitingMinutes,

                // Aturan BE-IGD-002: target yang belum ditetapkan SOP dibiarkan kosong,
                // bukan dianggap 0 menit.
                ResponseDueAt = level.MaxWaitingMinutes.HasValue
                    ? startedAt.AddMinutes(level.MaxWaitingMinutes.Value)
                    : null,

                ImmediateCareAllowed = level.AllowsTreatmentBeforeRegistration,
                TriageReason = NormalizeText(request.TriageReason),
                AirwaySummary = NormalizeText(request.AirwaySummary),
                BreathingSummary = NormalizeText(request.BreathingSummary),
                CirculationSummary = NormalizeText(request.CirculationSummary),
                DisabilitySummary = NormalizeText(request.DisabilitySummary),
                ExposureSummary = NormalizeText(request.ExposureSummary),
                RedFlagSummary = NormalizeText(request.RedFlagSummary),
                PerformedByUserId = actorUserId,
                Notes = NormalizeText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            // Hanya status dan jejak audit baris lama yang berubah. Seluruh kolom klinisnya
            // sengaja tidak disentuh supaya riwayat tetap utuh.
            previous.TriageStatus = EmergencyTriageStatus.Superseded;
            previous.UpdateDateTime = now;
            previous.UpdateBy = actorUserId;

            _dbContext.Set<TrxEmergencyTriage>().Add(retriage);

            try
            {
                // Satu kali simpan berarti satu transaksi: mustahil baris lama menjadi
                // Superseded tanpa penilaian penggantinya ikut tersimpan.
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Termasuk dua permintaan bersamaan yang memperebutkan nomor urut yang sama;
                // index unik (EmergencyVisitId, Sequence) menolak yang kedua.
                return RetriageOutcome.Conflict(
                    "Penilaian ulang gagal disimpan karena data sedang diubah pihak lain. Muat ulang halaman lalu coba lagi.");
            }

            return RetriageOutcome.Success(retriage, previous);
        }

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public bool CanTransition(EmergencyTriageStatus current, EmergencyTriageStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyTriageStatus.Draft => target is EmergencyTriageStatus.InProgress
                    or EmergencyTriageStatus.Completed
                    or EmergencyTriageStatus.Cancelled,
                EmergencyTriageStatus.InProgress => target is EmergencyTriageStatus.Completed
                    or EmergencyTriageStatus.Cancelled,
                EmergencyTriageStatus.Completed => target is EmergencyTriageStatus.Superseded,
                _ => false
            };
        }
    }

    /// <summary>
    /// Hasil penilaian ulang. Dibuat agar aturan penolakan beserta kode statusnya ditetapkan
    /// service, bukan ditebak ulang controller.
    /// </summary>
    public class RetriageOutcome
    {
        private RetriageOutcome(
            int statusCode,
            string message,
            TrxEmergencyTriage? retriage,
            TrxEmergencyTriage? previous)
        {
            StatusCode = statusCode;
            Message = message;
            Retriage = retriage;
            Previous = previous;
        }

        public int StatusCode { get; }

        public string Message { get; }

        public TrxEmergencyTriage? Retriage { get; }

        public TrxEmergencyTriage? Previous { get; }

        public bool IsSuccess => Retriage != null;

        public static RetriageOutcome Success(TrxEmergencyTriage retriage, TrxEmergencyTriage previous)
            => new(200, "Penilaian ulang triage IGD berhasil dibuat.", retriage, previous);

        public static RetriageOutcome NotFound(string message)
            => new(404, message, null, null);

        public static RetriageOutcome BadRequest(string message)
            => new(400, message, null, null);

        public static RetriageOutcome Conflict(string message)
            => new(409, message, null, null);
    }
}
