using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

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
                    // Completed ditambahkan BE-IGD-019. Sebelumnya hanya Disposed dan Cancelled
                    // yang dianggap tertutup, sehingga triase masih dapat dibuat pada kunjungan
                    // yang sudah benar-benar selesai — AT-IGD-088.
                    x => x.Id == request.EmergencyVisitId &&
                         !x.IsDelete &&
                         x.VisitStatus != EmergencyVisitStatus.Disposed &&
                         x.VisitStatus != EmergencyVisitStatus.Completed &&
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

            // Completed ditambahkan BE-IGD-020. Ini kembaran cacat yang ditutup BE-IGD-019 pada
            // ValidateRequestAsync: sebelumnya hanya Disposed dan Cancelled yang dianggap
            // tertutup, sehingga kunjungan yang sudah benar-benar selesai masih dapat dinilai
            // ulang — AT-IGD-088.
            if (visit.VisitStatus == EmergencyVisitStatus.Disposed ||
                visit.VisitStatus == EmergencyVisitStatus.Completed ||
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

        /// <summary>
        /// Menandai penilaian yang batas waktu responsnya sudah terlampaui sementara pasiennya
        /// belum ditangani. Mengembalikan jumlah penilaian yang baru ditandai.
        ///
        /// Idempotensi ditegakkan oleh saringan <c>!x.IsSlaBreached</c>: penilaian yang sudah
        /// bertanda tidak pernah masuk kandidat lagi, sehingga <c>SlaBreachedAt</c> yang sudah
        /// terisi tidak dapat bergeser walau pemindaian dijalankan berkali-kali.
        /// </summary>
        public async Task<int> MarkSlaBreachesAsync(
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var ukuranBatch = batchSize <= 0 ? 500 : batchSize;

            // "Belum ditangani" dibaca dari TreatmentStartedAt, bukan disimpulkan dari status
            // kunjungan. Kolom itu diisi sekali dengan ??= saat penanganan dimulai, sehingga
            // merupakan penanda yang paling langsung dan tidak pernah tertimpa.
            var kunjunganBelumDitangani = _dbContext.Set<TrxEmergencyVisit>()
                .Where(v => !v.IsDelete
                    && v.TreatmentStartedAt == null
                    && v.VisitStatus != EmergencyVisitStatus.Cancelled)
                .Select(v => v.Id);

            var kandidat = await _dbContext.Set<TrxEmergencyTriage>()
                .Where(x => !x.IsDelete
                    && !x.IsSlaBreached
                    // Target yang belum dikonfigurasi menghasilkan ResponseDueAt kosong
                    // (BE-IGD-002) dan karena itu tidak pernah boleh dianggap terlambat.
                    && x.ResponseDueAt != null
                    && x.ResponseDueAt <= now
                    // Penilaian yang dibatalkan tidak pernah berlaku.
                    && x.TriageStatus != EmergencyTriageStatus.Cancelled
                    && kunjunganBelumDitangani.Contains(x.EmergencyVisitId))
                .OrderBy(x => x.ResponseDueAt)
                .Take(ukuranBatch)
                .ToListAsync(cancellationToken);

            if (kandidat.Count == 0)
                return 0;

            foreach (var penilaian in kandidat)
            {
                // Hanya dua kolom penanda yang ditulis. Kolom klinis, kolom audit, dan
                // ResponseDueAt sengaja tidak disentuh: menimpa UpdateBy dengan pelaku sistem
                // akan menghapus jejak siapa terakhir mengubah penilaian klinisnya.
                penilaian.IsSlaBreached = true;
                penilaian.SlaBreachedAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return kandidat.Count;
        }

        /// <summary>
        /// Daftar pasien yang melampaui batas waktu respons dan <b>belum</b> ditangani.
        ///
        /// Penanda breach pada penilaian bersifat permanen sebagai riwayat, tetapi daftar ini
        /// menyaring ulang terhadap TreatmentStartedAt. Akibatnya pasien yang sudah ditangani
        /// hilang dari daftar tanpa penandanya dihapus.
        /// </summary>
        public async Task<PagedResult<EmergencyTriageSlaBreachResponse>> GetSlaBreachesAsync(
            Guid? serviceUnitId,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var query =
                from triage in _dbContext.Set<TrxEmergencyTriage>().AsNoTracking()
                join visit in _dbContext.Set<TrxEmergencyVisit>().AsNoTracking()
                    on triage.EmergencyVisitId equals visit.Id
                where !triage.IsDelete
                    && triage.IsSlaBreached
                    && triage.TriageStatus != EmergencyTriageStatus.Cancelled
                    && !visit.IsDelete
                    && visit.TreatmentStartedAt == null
                    && visit.VisitStatus != EmergencyVisitStatus.Cancelled
                select new { triage, visit };

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.visit.ServiceUnitId == serviceUnitId.Value);

            // Rentang waktu disaring pada saat pelampauan terjadi, bukan pada saat penilaian
            // dibuat, karena yang dicari adalah kejadian keterlambatannya.
            if (startDate.HasValue)
                query = query.Where(x => x.triage.SlaBreachedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.triage.SlaBreachedAt < endDate.Value.Date.AddDays(1));

            var totalData = await query.CountAsync(cancellationToken);

            var baris = await query
                .OrderBy(x => x.triage.ResponseDueAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.triage.Id,
                    x.triage.EmergencyVisitId,
                    x.visit.PatientId,
                    x.visit.IsUnknownPatient,
                    x.visit.TemporaryPatientAlias,
                    x.visit.ServiceUnitId,
                    ServiceUnitName = x.visit.ServiceUnit != null
                        ? x.visit.ServiceUnit.ServiceUnitName
                        : null,
                    PatientFullName = x.visit.Patient != null ? x.visit.Patient.FullName : null,
                    MedicalRecordNumber = x.visit.Patient != null
                        ? x.visit.Patient.MedicalRecordNumber
                        : null,
                    x.triage.TriageLevelId,
                    TriageLevelName = x.triage.TriageLevel != null
                        ? x.triage.TriageLevel.Name
                        : null,
                    TriageLevelColorName = x.triage.TriageLevel != null
                        ? x.triage.TriageLevel.ColorName
                        : null,
                    TriageLevelColorHex = x.triage.TriageLevel != null
                        ? x.triage.TriageLevel.ColorHex
                        : null,
                    TriageLevelNumber = x.triage.TriageLevel != null
                        ? (int?)x.triage.TriageLevel.Level
                        : null,
                    x.triage.Sequence,
                    x.triage.TriageStatus,
                    x.triage.StartedAt,
                    x.triage.MaxWaitingMinutesSnapshot,
                    x.triage.ResponseDueAt,
                    x.triage.SlaBreachedAt
                })
                .ToListAsync(cancellationToken);

            var items = baris.Select(x => new EmergencyTriageSlaBreachResponse
            {
                Id = x.Id,
                EmergencyVisitId = x.EmergencyVisitId,
                PatientId = x.PatientId,
                PatientName = !string.IsNullOrWhiteSpace(x.PatientFullName)
                    ? x.PatientFullName!
                    : (!string.IsNullOrWhiteSpace(x.TemporaryPatientAlias)
                        ? x.TemporaryPatientAlias!
                        : "Pasien belum teridentifikasi"),
                MedicalRecordNumber = x.MedicalRecordNumber,
                IsUnknownPatient = x.IsUnknownPatient,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnitName,
                TriageLevelId = x.TriageLevelId,
                TriageLevelName = x.TriageLevelName,
                TriageLevelColorName = x.TriageLevelColorName,
                TriageLevelColorHex = x.TriageLevelColorHex,
                TriageLevel = x.TriageLevelNumber,
                Sequence = x.Sequence,
                TriageStatus = x.TriageStatus,
                StartedAt = x.StartedAt,
                MaxWaitingMinutesSnapshot = x.MaxWaitingMinutesSnapshot,
                ResponseDueAt = x.ResponseDueAt,
                SlaBreachedAt = x.SlaBreachedAt,
                OverdueMinutes = x.ResponseDueAt.HasValue
                    ? (int)Math.Max(0, Math.Floor((now - x.ResponseDueAt.Value).TotalMinutes))
                    : 0
            }).ToList();

            return new PagedResult<EmergencyTriageSlaBreachResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
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
