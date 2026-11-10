using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using R = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services.RhbResult<QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs.RehabProgramDetailResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Program terapi berversi per layanan — <c>BE-RHB-011</c>, FR-RHB-021/022,
    /// <c>state-transition-matrix.md</c> §4, V-PRG-01..06, RHB-INV-03/10.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Versi.</b> Nomor <c>ProgramVersion</c> per (episode, layanan) dihitung di bawah advisory lock
    /// <c>RHB_PROGRAM:{episode}:{layanan}</c> (pola HD) dan dijaga indeks unik <c>UX_RhbProgram_Version</c>.
    /// Satu program <c>Active</c> per layanan dijaga <c>UX_RhbProgram_ActivePerDiscipline</c>.
    /// </para>
    /// <para>
    /// <b>Aktivasi</b> = tanda tangan dokumen RM jenis 48 (<c>RehabProgram</c>). Dalam satu transaksi:
    /// versi aktif lama → <c>Superseded</c> (disimpan lebih dulu agar indeks "satu aktif" tidak bentrok),
    /// program → <c>Active</c>, episode <c>Planning</c> → <c>Active</c>, riwayat status, lalu tanda tangan RM
    /// (yang menyimpan sendiri). Seluruh prasyarat V-PRG-01..06 dikumpulkan dalam satu jawaban 422.
    /// </para>
    /// <para>
    /// <b>Kewenangan atas data.</b> Membuat, mengubah, merevisi, dan mengaktifkan hanya oleh pemegang profesi
    /// layanan (<c>RHB-VAL-031</c>); ubah, aktifkan, dan batal draft hanya oleh penulisnya (<c>RHB-VAL-022</c>;
    /// aktivasi = tanda tangan, dan RM hanya menerima tanda tangan penulis). Menghentikan program aktif
    /// dijaga butir <c>RehabProgram : Stop</c> saja (dipegang juga Sp.KFR).
    /// </para>
    /// </remarks>
    public class RhbProgramService
    {
        private static readonly RehabEpisodeStatus[] PlanningOrActive = [RehabEpisodeStatus.Planning, RehabEpisodeStatus.Active];

        private readonly ApplicationDbContext _dbContext;
        private readonly RhbDocumentIntegrityGateway _integrity;
        private readonly RhbProfessionAuthority _professionAuthority;

        public RhbProgramService(
            ApplicationDbContext dbContext,
            RhbDocumentIntegrityGateway integrity,
            RhbProfessionAuthority professionAuthority)
        {
            _dbContext = dbContext;
            _integrity = integrity;
            _professionAuthority = professionAuthority;
        }

        public static string StatusName(RehabProgramStatus status) => status switch
        {
            RehabProgramStatus.Draft => "Draf",
            RehabProgramStatus.Active => "Aktif",
            RehabProgramStatus.Superseded => "Digantikan versi baru",
            RehabProgramStatus.Stopped => "Dihentikan",
            RehabProgramStatus.Completed => "Selesai",
            RehabProgramStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        private static string IntegrityStatusName(ClinicalDocumentIntegrityStatus status) => status switch
        {
            ClinicalDocumentIntegrityStatus.Draft => "Draf",
            ClinicalDocumentIntegrityStatus.Signed => "Ditandatangani",
            ClinicalDocumentIntegrityStatus.LockedUnsigned => "Terkunci, tidak ditandatangani",
            ClinicalDocumentIntegrityStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<RhbResult<List<RehabProgramListItem>>> ListByEpisodeAsync(Guid episodeId, CancellationToken cancellationToken)
        {
            if (!await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken))
                return RhbResult<List<RehabProgramListItem>>.NotFound("Episode rehab tidak ditemukan.");

            var rows = await _dbContext.RhbPrograms.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.Discipline!.DisciplineName).ThenByDescending(x => x.ProgramVersion)
                .Select(x => new RehabProgramListItem
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    DisciplineId = x.DisciplineId,
                    DisciplineName = x.Discipline!.DisciplineName,
                    ProgramVersion = x.ProgramVersion,
                    SupersedesProgramId = x.SupersedesProgramId,
                    ProgramStatus = x.ProgramStatus,
                    NeedsReview = x.NeedsReview,
                    StartDate = x.StartDate,
                    EvaluationDueDate = x.EvaluationDueDate,
                    ActivatedAt = x.ActivatedAt,
                    AuthorUserId = x.AuthorUserId,
                    Version = x.Version
                })
                .ToListAsync(cancellationToken);

            var authorIds = rows.Select(x => x.AuthorUserId).Distinct().ToList();
            var authors = await _dbContext.Users.AsNoTracking()
                .Where(u => authorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);
            foreach (var row in rows)
            {
                row.ProgramStatusName = StatusName(row.ProgramStatus);
                row.AuthorName = authors.TryGetValue(row.AuthorUserId, out var name) ? name : null;
            }

            return RhbResult<List<RehabProgramListItem>>.Ok(rows);
        }

        public async Task<R> GetDetailAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var detail = await LoadAsync(id, actorUserId, cancellationToken);
            return detail == null ? NotFound() : R.Ok(detail);
        }

        // =====================================================================
        // Draft
        // =====================================================================

        public async Task<R> CreateAsync(Guid episodeId, SaveRehabProgramRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var episode = await _dbContext.RhbEpisodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
            if (episode == null)
                return R.NotFound("Episode rehab tidak ditemukan.");
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return EpisodeClosed();

            if (!request.DisciplineId.HasValue || request.DisciplineId.Value == Guid.Empty)
                return R.Invalid(RhbErrorCodes.DisciplineRequired, "Pilih layanan rehab.").WithFields("disciplineId");
            var disciplineId = request.DisciplineId.Value;

            if (!PlanningOrActive.Contains(episode.EpisodeStatus) || !await IsPrescribedAsync(episodeId, disciplineId, cancellationToken))
                return R.Rule(RhbErrorCodes.DisciplineNotPrescribed, "Layanan ini belum diresepkan dokter rehab untuk episode ini.").WithFields("disciplineId");

            var profession = await CheckProfessionAsync(actorUserId, disciplineId, cancellationToken);
            if (profession != null)
                return profession;

            if (!request.AuthoredEncounterId.HasValue)
                return EncounterInvalid("Pilih kunjungan tempat program disusun.");
            var encounterError = await CheckEncounterAsync(request.AuthoredEncounterId.Value, episode.PatientId, cancellationToken);
            if (encounterError != null)
                return encounterError;

            var (error, values) = await ValidateAsync(request, episodeId, disciplineId, cancellationToken);
            if (error != null)
                return error;

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await RhbServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, LockKey(episodeId, disciplineId));

            var existing = await _dbContext.RhbPrograms.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && x.DisciplineId == disciplineId && !x.IsDelete)
                .Select(x => new { x.ProgramStatus, x.ProgramVersion })
                .ToListAsync(cancellationToken);
            if (existing.Any(x => x.ProgramStatus is RehabProgramStatus.Draft or RehabProgramStatus.Active))
                return R.Conflict(RhbErrorCodes.InvalidState,
                    "Layanan ini sudah punya program draft atau aktif. Lanjutkan draft tersebut, atau revisi program aktif.");

            var entity = new RhbProgram
            {
                EpisodeId = episodeId,
                DisciplineId = disciplineId,
                ProgramVersion = existing.Count == 0 ? 1 : existing.Max(x => x.ProgramVersion) + 1,
                AuthoredEncounterId = request.AuthoredEncounterId.Value,
                ProgramStatus = RehabProgramStatus.Draft,
                AuthorUserId = actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            Apply(entity, values!);
            _dbContext.RhbPrograms.Add(entity);
            AddChildren(entity, values!, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Program, entity.Id, null,
                RehabProgramStatus.Draft.ToString(), "create", null, actorUserId, now);
            await _integrity.RegisterDraftAsync(ClinicalDocumentKind.RehabProgram, entity.Id, episode.PatientId,
                entity.AuthoredEncounterId, actorUserId, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            await transaction.CommitAsync(cancellationToken);
            return R.Created((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> UpdateAsync(Guid id, SaveRehabProgramRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, _, guard) = await LoadDraftForAuthorAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            if (request.DisciplineId.HasValue && request.DisciplineId.Value != entity!.DisciplineId)
                return R.Invalid(RhbErrorCodes.DisciplineRequired, "Layanan program tidak dapat diganti.").WithFields("disciplineId");
            if (request.AuthoredEncounterId.HasValue && request.AuthoredEncounterId.Value != entity!.AuthoredEncounterId)
                return EncounterInvalid("Kunjungan penyusunan program tidak dapat diganti.");

            var (error, values) = await ValidateAsync(request, entity!.EpisodeId, entity.DisciplineId, cancellationToken);
            if (error != null)
                return error;

            var now = DateTime.UtcNow;
            Apply(entity, values!);
            Touch(entity, actorUserId, now);

            var oldInterventions = await _dbContext.RhbProgramInterventions.Where(x => x.ProgramId == entity.Id && !x.IsDelete).ToListAsync(cancellationToken);
            var oldGoals = await _dbContext.RhbProgramGoals.Where(x => x.ProgramId == entity.Id && !x.IsDelete).ToListAsync(cancellationToken);
            foreach (var row in oldInterventions) SoftDelete(row, actorUserId, now);
            foreach (var row in oldGoals) SoftDelete(row, actorUserId, now);

            // Tautan tujuan lama harus terhapus sebelum yang baru disisipkan (indeks unik ProgramId+GoalId).
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var first = await TrySaveAsync(cancellationToken);
            if (first != null)
                return first;

            AddChildren(entity, values!, actorUserId, now);
            var second = await TrySaveAsync(cancellationToken);
            if (second != null)
                return second;

            await transaction.CommitAsync(cancellationToken);
            return R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> CancelAsync(Guid id, RehabReasonRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, _, guard) = await LoadDraftForAuthorAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            var reason = RhbServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return R.Rule(RhbErrorCodes.ReasonRequired, "Alasan wajib diisi.").WithFields("reason");
            if (reason.Length > 500)
                return TooLong("reason", 500);

            var now = DateTime.UtcNow;
            entity!.ProgramStatus = RehabProgramStatus.Cancelled;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Program, entity.Id,
                RehabProgramStatus.Draft.ToString(), RehabProgramStatus.Cancelled.ToString(), "cancel", reason, actorUserId, now);
            await _integrity.MarkCancelledAsync(ClinicalDocumentKind.RehabProgram, entity.Id, actorUserId, reason, now, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Revisi, aktivasi, penghentian
        // =====================================================================

        public async Task<R> ReviseAsync(Guid id, ReviseRehabProgramRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var source = await _dbContext.RhbPrograms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (source == null)
                return NotFound();

            var episode = await _dbContext.RhbEpisodes.AsNoTracking().FirstAsync(x => x.Id == source.EpisodeId, cancellationToken);
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return EpisodeClosed();
            if (source.ProgramStatus != RehabProgramStatus.Active)
                return R.Conflict(RhbErrorCodes.InvalidState, $"Hanya program aktif yang dapat direvisi (status saat ini {StatusName(source.ProgramStatus)}).");
            if (source.Version != request.ExpectedVersion)
                return Stale().WithFields("expectedVersion");

            var profession = await CheckProfessionAsync(actorUserId, source.DisciplineId, cancellationToken);
            if (profession != null)
                return profession;

            var reason = RhbServiceSupport.Normalize(request.ModificationReason);
            if (reason == null)
                return R.Rule(RhbErrorCodes.ReasonRequired, "Alasan modifikasi wajib diisi.").WithFields("modificationReason");
            if (reason.Length > 500)
                return TooLong("modificationReason", 500);

            var encounterId = request.AuthoredEncounterId ?? source.AuthoredEncounterId;
            if (request.AuthoredEncounterId.HasValue)
            {
                var encounterError = await CheckEncounterAsync(encounterId, episode.PatientId, cancellationToken);
                if (encounterError != null)
                    return encounterError;
            }

            var interventions = await _dbContext.RhbProgramInterventions.AsNoTracking()
                .Where(x => x.ProgramId == source.Id && !x.IsDelete)
                .Select(x => new { x.ProcedureId, x.ParametersText })
                .ToListAsync(cancellationToken);
            var goalIds = await _dbContext.RhbProgramGoals.AsNoTracking()
                .Where(x => x.ProgramId == source.Id && !x.IsDelete && x.Goal!.GoalStatus == RehabGoalStatus.Active)
                .Select(x => x.GoalId)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await RhbServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, LockKey(source.EpisodeId, source.DisciplineId));

            var versions = await _dbContext.RhbPrograms.AsNoTracking()
                .Where(x => x.EpisodeId == source.EpisodeId && x.DisciplineId == source.DisciplineId && !x.IsDelete)
                .Select(x => new { x.ProgramStatus, x.ProgramVersion })
                .ToListAsync(cancellationToken);
            if (versions.Any(x => x.ProgramStatus == RehabProgramStatus.Draft))
                return R.Conflict(RhbErrorCodes.InvalidState, "Sudah ada versi draft untuk layanan ini. Lanjutkan draft tersebut.");

            var revision = new RhbProgram
            {
                EpisodeId = source.EpisodeId,
                DisciplineId = source.DisciplineId,
                ProgramVersion = versions.Max(x => x.ProgramVersion) + 1,
                SupersedesProgramId = source.Id,
                AuthoredEncounterId = encounterId,
                FrequencyText = source.FrequencyText,
                PlannedSessionCount = source.PlannedSessionCount,
                PrecautionText = source.PrecautionText,
                StartDate = source.StartDate,
                EvaluationDueDate = source.EvaluationDueDate,
                ModificationReason = reason,
                ProgramStatus = RehabProgramStatus.Draft,
                AuthorUserId = actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            _dbContext.RhbPrograms.Add(revision);
            AddChildren(revision, new Values(revision.FrequencyText, revision.PlannedSessionCount, revision.PrecautionText,
                revision.StartDate, revision.EvaluationDueDate,
                interventions.Select(x => (x.ProcedureId, x.ParametersText)).ToList(), goalIds), actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Program, revision.Id, null,
                RehabProgramStatus.Draft.ToString(), "revise", reason, actorUserId, now);
            await _integrity.RegisterDraftAsync(ClinicalDocumentKind.RehabProgram, revision.Id, episode.PatientId,
                encounterId, actorUserId, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            await transaction.CommitAsync(cancellationToken);
            return R.Created((await LoadAsync(revision.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> ActivateAsync(
            Guid id,
            RehabSignRequest request,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            var (entity, episode, guard) = await LoadDraftForAuthorAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            var issues = await ActivationIssuesAsync(entity!, cancellationToken);
            if (issues.Count > 0)
                return R.Rule(issues[0].Code, "Program belum dapat diaktifkan. Lengkapi prasyarat berikut.", issues);

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await RhbServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, LockKey(entity!.EpisodeId, entity.DisciplineId));

            try
            {
                // 1) Versi aktif lama → Superseded, disimpan lebih dulu (indeks satu aktif per layanan).
                var previous = await _dbContext.RhbPrograms
                    .Where(x => x.EpisodeId == entity.EpisodeId && x.DisciplineId == entity.DisciplineId && !x.IsDelete &&
                                x.ProgramStatus == RehabProgramStatus.Active && x.Id != entity.Id)
                    .ToListAsync(cancellationToken);
                foreach (var old in previous)
                {
                    old.ProgramStatus = RehabProgramStatus.Superseded;
                    Touch(old, actorUserId, now);
                    RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Program, old.Id,
                        RehabProgramStatus.Active.ToString(), RehabProgramStatus.Superseded.ToString(), "superseded", null, actorUserId, now);
                }
                if (previous.Count > 0)
                    await _dbContext.SaveChangesAsync(cancellationToken);

                // 2) Program aktif + episode Active + riwayat; 3) tanda tangan RM (menyimpan semuanya).
                entity.ProgramStatus = RehabProgramStatus.Active;
                entity.ActivatedAt = now;
                Touch(entity, actorUserId, now);
                RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Program, entity.Id,
                    RehabProgramStatus.Draft.ToString(), RehabProgramStatus.Active.ToString(), "activate", null, actorUserId, now);

                var trackedEpisode = await _dbContext.RhbEpisodes.FirstAsync(x => x.Id == entity.EpisodeId, cancellationToken);
                if (trackedEpisode.EpisodeStatus == RehabEpisodeStatus.Planning)
                {
                    trackedEpisode.EpisodeStatus = RehabEpisodeStatus.Active;
                    trackedEpisode.UpdateBy = actorUserId;
                    trackedEpisode.UpdateDateTime = now;
                    trackedEpisode.Version++;
                    RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Episode, trackedEpisode.Id,
                        RehabEpisodeStatus.Planning.ToString(), RehabEpisodeStatus.Active.ToString(), "activate-program", null, actorUserId, now);
                }

                var (result, _) = await _integrity.SignAsync(ClinicalDocumentKind.RehabProgram, entity.Id, actorUserId,
                    deviceInfo, ipAddress, now, cancellationToken);
                if (!result.IsAllowed)
                {
                    DetachPending();
                    return result.StatusCode == StatusCodes.Status403Forbidden
                        ? R.Forbidden(RhbErrorCodes.NotAuthor, result.ErrorMessage ?? "Hanya penulis program yang dapat mengaktifkannya.")
                        : R.Conflict(RhbErrorCodes.DocumentLocked, result.ErrorMessage ?? "Program sudah terkunci.");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                DetachPending();
                return Stale();
            }
            catch (DbUpdateException exception) when (RhbServiceSupport.IsUniqueViolation(exception))
            {
                DetachPending();
                return R.Conflict(RhbErrorCodes.InvalidState, "Layanan ini sudah punya program aktif lain. Muat ulang data.");
            }

            await transaction.CommitAsync(cancellationToken);
            return R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> StopAsync(Guid id, RehabReasonRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RhbPrograms.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound();
            if (await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == entity.EpisodeId && x.EpisodeStatus == RehabEpisodeStatus.Closed, cancellationToken))
                return EpisodeClosed();
            if (entity.ProgramStatus != RehabProgramStatus.Active)
                return R.Conflict(RhbErrorCodes.InvalidState, $"Hanya program aktif yang dapat dihentikan (status saat ini {StatusName(entity.ProgramStatus)}).");
            if (entity.Version != request.ExpectedVersion)
                return Stale().WithFields("expectedVersion");

            var reason = RhbServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return R.Rule(RhbErrorCodes.ReasonRequired, "Alasan penghentian wajib diisi.").WithFields("reason");
            if (reason.Length > 500)
                return TooLong("reason", 500);

            var now = DateTime.UtcNow;
            entity.ProgramStatus = RehabProgramStatus.Stopped;
            entity.StoppedAt = now;
            entity.StopReason = reason;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Program, entity.Id,
                RehabProgramStatus.Active.ToString(), RehabProgramStatus.Stopped.ToString(), "stop", reason, actorUserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Prasyarat aktivasi (V-PRG-01..06)
        // =====================================================================

        private async Task<List<RehabActivationIssue>> ActivationIssuesAsync(RhbProgram program, CancellationToken cancellationToken)
        {
            var issues = new List<RehabActivationIssue>();
            void Add(string code, string message) => issues.Add(new RehabActivationIssue { Code = code, Message = message });

            var medicalFinal = await _dbContext.RhbAssessments.AsNoTracking().AnyAsync(x =>
                x.EpisodeId == program.EpisodeId && !x.IsDelete && x.AssessmentType == RehabAssessmentType.MedicalInitial &&
                x.RecordStatus == RehabRecordStatus.Final, cancellationToken);
            if (!medicalFinal)
                Add(RhbErrorCodes.MedicalAssessmentNotFinal, "Asesmen medis dokter rehab belum final.");

            var disciplineFinal = await _dbContext.RhbAssessments.AsNoTracking().AnyAsync(x =>
                x.EpisodeId == program.EpisodeId && !x.IsDelete && x.AssessmentType == RehabAssessmentType.DisciplineInitial &&
                x.DisciplineId == program.DisciplineId && x.RecordStatus == RehabRecordStatus.Final, cancellationToken);
            if (!disciplineFinal)
                Add(RhbErrorCodes.DisciplineAssessmentNotFinal, "Asesmen profesi untuk layanan ini belum final.");

            var procedureIds = await _dbContext.RhbProgramInterventions.AsNoTracking()
                .Where(x => x.ProgramId == program.Id && !x.IsDelete)
                .Select(x => x.ProcedureId)
                .ToListAsync(cancellationToken);
            var allowed = await AllowedProceduresAsync(program.DisciplineId, cancellationToken);
            if (procedureIds.Count == 0 || procedureIds.Any(x => !allowed.Contains(x)))
                Add(RhbErrorCodes.InterventionRequired, "Tambahkan intervensi dari daftar tindakan layanan ini.");

            var activeGoals = await _dbContext.RhbProgramGoals.AsNoTracking().CountAsync(x =>
                x.ProgramId == program.Id && !x.IsDelete && x.Goal!.GoalStatus == RehabGoalStatus.Active && !x.Goal.IsDelete, cancellationToken);
            if (activeGoals == 0)
                Add(RhbErrorCodes.GoalRequired, "Kaitkan minimal satu tujuan aktif.");

            if (string.IsNullOrWhiteSpace(program.FrequencyText) || !program.StartDate.HasValue || !program.EvaluationDueDate.HasValue ||
                program.EvaluationDueDate.Value < program.StartDate.Value)
                Add(RhbErrorCodes.ProgramScheduleIncomplete, "Lengkapi frekuensi, tanggal mulai, dan tanggal evaluasi.");

            if (await LatestConsentAsync(program.Id, cancellationToken) == RehabConsentStatus.Refused)
                Add(RhbErrorCodes.ConsentRefused, "Pasien/keluarga menolak rencana ini. Program tidak dapat diaktifkan.");

            return issues;
        }

        /// <summary>Persetujuan pada edukasi <b>final</b> terakhir untuk program ini (V-PRG-06).</summary>
        private Task<RehabConsentStatus?> LatestConsentAsync(Guid programId, CancellationToken cancellationToken) =>
            _dbContext.RhbEducationRecords.AsNoTracking()
                .Where(x => x.ProgramId == programId && !x.IsDelete && x.RecordStatus == RehabRecordStatus.Final)
                .OrderByDescending(x => x.EducatedAt).ThenByDescending(x => x.CreateDateTime)
                .Select(x => (RehabConsentStatus?)x.ConsentStatus)
                .FirstOrDefaultAsync(cancellationToken);

        // =====================================================================
        // Validasi isian
        // =====================================================================

        private sealed record Values(
            string? FrequencyText,
            int? PlannedSessionCount,
            string? PrecautionText,
            DateOnly? StartDate,
            DateOnly? EvaluationDueDate,
            List<(Guid ProcedureId, string ParametersText)> Interventions,
            List<Guid> GoalIds);

        /// <summary>
        /// Draft boleh belum lengkap; yang dicegah hanya data yang salah: tindakan di luar daftar layanan,
        /// tujuan di luar episode/layanan atau tidak aktif, urutan tanggal terbalik, dan panjang isian.
        /// </summary>
        private async Task<(R? Error, Values? Values)> ValidateAsync(
            SaveRehabProgramRequest request,
            Guid episodeId,
            Guid disciplineId,
            CancellationToken cancellationToken)
        {
            var frequency = RhbServiceSupport.Normalize(request.FrequencyText);
            var precaution = RhbServiceSupport.Normalize(request.PrecautionText);
            if (frequency?.Length > 200) return (TooLong("frequencyText", 200), null);
            if (precaution?.Length > 1000) return (TooLong("precautionText", 1000), null);
            if (request.PlannedSessionCount is <= 0 or > 500)
                return (R.Invalid(RhbErrorCodes.ProgramScheduleIncomplete, "Rencana jumlah sesi harus 1–500.").WithFields("plannedSessionCount"), null);
            if (request.StartDate.HasValue && request.EvaluationDueDate.HasValue && request.EvaluationDueDate.Value < request.StartDate.Value)
                return (R.Rule(RhbErrorCodes.ProgramScheduleIncomplete, "Tanggal evaluasi tidak boleh sebelum tanggal mulai.").WithFields("evaluationDueDate"), null);

            var interventionInput = request.Interventions ?? new();
            if (interventionInput.Select(x => x.ProcedureId).Distinct().Count() != interventionInput.Count)
                return (R.Rule(RhbErrorCodes.InterventionRequired, "Tindakan intervensi tidak boleh berulang.").WithFields("interventions"), null);
            var allowed = await AllowedProceduresAsync(disciplineId, cancellationToken);
            var interventions = new List<(Guid, string)>();
            for (var i = 0; i < interventionInput.Count; i++)
            {
                if (!allowed.Contains(interventionInput[i].ProcedureId))
                    return (R.Rule(RhbErrorCodes.InterventionRequired, "Tambahkan intervensi dari daftar tindakan layanan ini.")
                        .WithFields($"interventions[{i}].procedureId"), null);
                var parameters = RhbServiceSupport.Normalize(interventionInput[i].ParametersText) ?? string.Empty;
                if (parameters.Length > 1000)
                    return (TooLong($"interventions[{i}].parametersText", 1000), null);
                interventions.Add((interventionInput[i].ProcedureId, parameters));
            }

            var goalIds = (request.GoalIds ?? new()).Distinct().ToList();
            if (goalIds.Count > 0)
            {
                var valid = await _dbContext.RhbGoals.AsNoTracking().CountAsync(x =>
                    goalIds.Contains(x.Id) && x.EpisodeId == episodeId && !x.IsDelete && x.GoalStatus == RehabGoalStatus.Active &&
                    (x.DisciplineId == null || x.DisciplineId == disciplineId), cancellationToken);
                if (valid != goalIds.Count)
                    return (R.Rule(RhbErrorCodes.GoalRequired, "Tujuan harus tujuan aktif episode ini untuk layanan yang sama.").WithFields("goalIds"), null);
            }

            return (null, new Values(frequency, request.PlannedSessionCount, precaution, request.StartDate, request.EvaluationDueDate, interventions, goalIds));
        }

        private async Task<HashSet<Guid>> AllowedProceduresAsync(Guid disciplineId, CancellationToken cancellationToken) =>
            (await (from map in _dbContext.RhbDisciplineProcedures.AsNoTracking()
                    join procedure in _dbContext.Set<MstProcedure>().AsNoTracking() on map.ProcedureId equals procedure.Id
                    where map.DisciplineId == disciplineId && map.IsActive && !map.IsDelete && procedure.IsActive && !procedure.IsDelete
                    select map.ProcedureId)
                .ToListAsync(cancellationToken)).ToHashSet();

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>Penjaga draft: ada, episode belum ditutup, status Draft, versi cocok, penulis, profesi, dan RM belum mengunci.</summary>
        private async Task<(RhbProgram? Entity, RhbEpisode? Episode, R? Error)> LoadDraftForAuthorAsync(
            Guid id,
            int expectedVersion,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RhbPrograms.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return (null, null, NotFound());

            var episode = await _dbContext.RhbEpisodes.AsNoTracking().FirstAsync(x => x.Id == entity.EpisodeId, cancellationToken);
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return (null, null, EpisodeClosed());

            if (entity.ProgramStatus == RehabProgramStatus.Active)
                return (null, null, R.Conflict(RhbErrorCodes.DocumentLocked, "Program aktif sudah ditandatangani. Gunakan revisi."));
            if (entity.ProgramStatus != RehabProgramStatus.Draft)
                return (null, null, R.Conflict(RhbErrorCodes.InvalidState, $"Program berstatus {StatusName(entity.ProgramStatus)} tidak dapat diubah."));

            if (entity.Version != expectedVersion)
                return (null, null, Stale().WithFields("expectedVersion"));

            if (entity.AuthorUserId != actorUserId)
                return (null, null, R.Forbidden(RhbErrorCodes.NotAuthor, "Hanya penulis program yang dapat mengubah, mengaktifkan, atau membatalkannya."));

            var profession = await CheckProfessionAsync(actorUserId, entity.DisciplineId, cancellationToken);
            if (profession != null)
                return (null, null, profession);

            var mutable = await _integrity.EnsureMutableAsync(ClinicalDocumentKind.RehabProgram, entity.Id, cancellationToken);
            if (!mutable.IsAllowed)
                return (null, null, R.Conflict(RhbErrorCodes.DocumentLocked, mutable.ErrorMessage ?? "Program sudah terkunci."));

            return (entity, episode, null);
        }

        private async Task<R?> CheckProfessionAsync(Guid actorUserId, Guid disciplineId, CancellationToken cancellationToken) =>
            await _professionAuthority.CanActForDisciplineAsync(actorUserId, disciplineId, cancellationToken)
                ? null
                : R.Forbidden(RhbErrorCodes.ProfessionMismatch, "Profesi Anda tidak sesuai dengan layanan ini.");

        private async Task<R?> CheckEncounterAsync(Guid encounterId, Guid patientId, CancellationToken cancellationToken)
        {
            var check = await RhbServiceSupport.CheckEncounterAsync(_dbContext, encounterId, patientId, cancellationToken);
            return check.IsValid
                ? null
                : EncounterInvalid(check.Exists && !check.BelongsToPatient
                    ? "Kunjungan bukan milik pasien episode ini."
                    : "Kunjungan tidak ditemukan, sudah berakhir, atau pasien sudah pulang.");
        }

        private Task<bool> IsPrescribedAsync(Guid episodeId, Guid disciplineId, CancellationToken cancellationToken) =>
            _dbContext.RhbEpisodeDisciplines.AsNoTracking().AnyAsync(x =>
                x.EpisodeId == episodeId && x.DisciplineId == disciplineId && !x.IsDelete &&
                x.PrescribedByAssessment!.RecordStatus == RehabRecordStatus.Final, cancellationToken);

        private static string LockKey(Guid episodeId, Guid disciplineId) => $"RHB_PROGRAM:{episodeId}:{disciplineId}";

        private static void Apply(RhbProgram entity, Values values)
        {
            entity.FrequencyText = values.FrequencyText;
            entity.PlannedSessionCount = values.PlannedSessionCount;
            entity.PrecautionText = values.PrecautionText;
            entity.StartDate = values.StartDate;
            entity.EvaluationDueDate = values.EvaluationDueDate;
        }

        private void AddChildren(RhbProgram entity, Values values, Guid actorUserId, DateTime nowUtc)
        {
            foreach (var (procedureId, parameters) in values.Interventions)
            {
                _dbContext.RhbProgramInterventions.Add(new RhbProgramIntervention
                {
                    ProgramId = entity.Id,
                    ProcedureId = procedureId,
                    ParametersText = parameters,
                    CreateDateTime = nowUtc,
                    CreateBy = actorUserId
                });
            }

            foreach (var goalId in values.GoalIds)
            {
                _dbContext.RhbProgramGoals.Add(new RhbProgramGoal
                {
                    ProgramId = entity.Id,
                    GoalId = goalId,
                    CreateDateTime = nowUtc,
                    CreateBy = actorUserId
                });
            }
        }

        private static void SoftDelete(QuilvianSystemBackend.Models.IdentityModel row, Guid actorUserId, DateTime nowUtc)
        {
            row.IsDelete = true;
            row.DeleteBy = actorUserId;
            row.DeleteDateTime = nowUtc;
            row.UpdateBy = actorUserId;
            row.UpdateDateTime = nowUtc;
        }

        private static void Touch(RhbProgram entity, Guid actorUserId, DateTime nowUtc)
        {
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = nowUtc;
            entity.Version++;
        }

        private async Task<RehabProgramDetailResponse?> LoadAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var detail = await _dbContext.RhbPrograms.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new RehabProgramDetailResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    DisciplineId = x.DisciplineId,
                    DisciplineName = x.Discipline!.DisciplineName,
                    ProgramVersion = x.ProgramVersion,
                    SupersedesProgramId = x.SupersedesProgramId,
                    ProgramStatus = x.ProgramStatus,
                    NeedsReview = x.NeedsReview,
                    StartDate = x.StartDate,
                    EvaluationDueDate = x.EvaluationDueDate,
                    ActivatedAt = x.ActivatedAt,
                    AuthorUserId = x.AuthorUserId,
                    Version = x.Version,
                    AuthoredEncounterId = x.AuthoredEncounterId,
                    FrequencyText = x.FrequencyText,
                    PlannedSessionCount = x.PlannedSessionCount,
                    PrecautionText = x.PrecautionText,
                    ModificationReason = x.ModificationReason,
                    StoppedAt = x.StoppedAt,
                    StopReason = x.StopReason
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (detail == null)
                return null;

            detail.ProgramStatusName = StatusName(detail.ProgramStatus);
            detail.AuthorName = await _dbContext.Users.AsNoTracking().Where(u => u.Id == detail.AuthorUserId).Select(u => u.DisplayName).FirstOrDefaultAsync(cancellationToken);
            detail.AuthoredEncounterNumber = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Where(e => e.Id == detail.AuthoredEncounterId).Select(e => e.EncounterNumber).FirstOrDefaultAsync(cancellationToken);

            detail.Interventions = await (
                    from i in _dbContext.RhbProgramInterventions.AsNoTracking()
                    join p in _dbContext.Set<MstProcedure>().AsNoTracking() on i.ProcedureId equals p.Id into procedures
                    from p in procedures.DefaultIfEmpty()
                    where i.ProgramId == id && !i.IsDelete
                    orderby p.ProcedureName
                    select new RehabProgramInterventionItem
                    {
                        ProgramInterventionId = i.Id,
                        ProcedureId = i.ProcedureId,
                        ProcedureCode = p != null ? p.ProcedureCode : null,
                        ProcedureName = p != null ? p.ProcedureName : null,
                        ParametersText = i.ParametersText
                    })
                .ToListAsync(cancellationToken);

            var goals = await _dbContext.RhbProgramGoals.AsNoTracking()
                .Where(x => x.ProgramId == id && !x.IsDelete)
                .Select(x => new RehabProgramGoalItem
                {
                    GoalId = x.GoalId,
                    Term = x.Goal!.Term,
                    DescriptionText = x.Goal.DescriptionText,
                    GoalStatus = x.Goal.GoalStatus
                })
                .ToListAsync(cancellationToken);
            foreach (var goal in goals)
                goal.GoalStatusName = RhbGoalService.StatusName(goal.GoalStatus);
            detail.Goals = goals.OrderBy(x => x.Term).ToList();

            detail.LatestConsentStatus = await LatestConsentAsync(id, cancellationToken);
            detail.LatestConsentStatusName = detail.LatestConsentStatus.HasValue ? RhbEducationService.ConsentName(detail.LatestConsentStatus.Value) : null;

            var integrity = await _integrity.FindAsync(ClinicalDocumentKind.RehabProgram, id, cancellationToken);
            detail.IntegrityStatus = integrity?.IntegrityStatus;
            detail.IntegrityStatusName = integrity != null ? IntegrityStatusName(integrity.IntegrityStatus) : null;

            detail.AvailableActions = detail.ProgramStatus switch
            {
                RehabProgramStatus.Draft when detail.AuthorUserId == actorUserId &&
                                              integrity?.IntegrityStatus is null or ClinicalDocumentIntegrityStatus.Draft
                    => ["update", "activate", "cancel"],
                RehabProgramStatus.Active => ["revise", "stop", "addendum"],
                _ => []
            };

            return detail;
        }

        private async Task<R?> TrySaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                DetachPending();
                return Stale();
            }
            catch (DbUpdateException exception) when (RhbServiceSupport.IsUniqueViolation(exception))
            {
                DetachPending();
                return R.Conflict(RhbErrorCodes.InvalidState, "Program ini sudah diproses pengguna lain. Muat ulang data.");
            }
        }

        private void DetachPending()
        {
            foreach (var entry in _dbContext.ChangeTracker.Entries().ToList())
            {
                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                    entry.State = EntityState.Detached;
            }
        }

        private static R NotFound() => R.NotFound("Program terapi tidak ditemukan.");

        private static R Stale() => R.Conflict(RhbErrorCodes.StaleVersion, "Data sudah diubah pengguna lain. Muat ulang sebelum menyimpan.");

        private static R EpisodeClosed() => R.Conflict(RhbErrorCodes.EpisodeClosed, "Episode rehab sudah ditutup.");

        private static R EncounterInvalid(string message) =>
            R.Rule(RhbErrorCodes.ClinicalEncounterInvalid, message).WithFields("authoredEncounterId");

        private static R TooLong(string field, int max) =>
            R.Invalid(RhbErrorCodes.FieldTooLong, $"Isian terlalu panjang (maksimal {max} karakter).").WithFields(field);
    }
}
