using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;
using R = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services.RhbResult<QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs.RehabProgressReviewDetailResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Reasesmen kemajuan dan keputusannya — <c>BE-RHB-014</c>, FR-RHB-040/041, FEAT-024/025,
    /// <c>state-transition-matrix.md</c> §7–8, V-MEA-01/02.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dua jenis.</b> Reasesmen profesi (<c>DisciplineId</c> + <c>ProgramId</c> aktif, profesi = layanan) dan
    /// reasesmen medis (tanpa layanan; kewenangannya dari butir <c>RehabProgressReview</c> yang admin berikan
    /// kepada Sp.KFR).
    /// </para>
    /// <para>
    /// <b>Baseline.</b> Pembanding diambil dari asesmen awal final (profesi untuk layanan itu, atau medis).
    /// Pengukuran sepadan dicocokkan menurut jenis instrumen (berinstrumen) atau nama + bagian tubuh
    /// (terstruktur). Versi instrumen atau satuan yang berbeda wajib dijelaskan di <c>InstrumentChangeNote</c>
    /// (V-MEA-02, 422 <c>RHB-VAL-036</c>).
    /// </para>
    /// <para>
    /// <b>Efek keputusan</b> terjadi saat tanda tangan, dalam satu transaksi dengan kunci RM jenis 50:
    /// <c>Modify</c> → program <c>NeedsReview</c>; <c>Stop</c>/<c>Refer</c> → program <c>Stopped</c>;
    /// <c>Complete</c> → program <c>Completed</c>; penilaian tujuan → status tujuan. Pada reasesmen medis,
    /// <c>Stop</c>/<c>Refer</c>/<c>Complete</c> hanya mencatat "episode siap ditutup" di riwayat; episode tetap
    /// ditutup lewat ringkasan akhir (<c>BE-RHB-015</c>).
    /// </para>
    /// </remarks>
    public class RhbProgressReviewService
    {
        private static readonly TimeSpan ClockSkewTolerance = TimeSpan.FromMinutes(2);
        private static readonly RehabEpisodeStatus[] PlanningOrActive = [RehabEpisodeStatus.Planning, RehabEpisodeStatus.Active];
        private static readonly RehabGoalStatus[] ReviewableGoalStatuses =
            [RehabGoalStatus.Active, RehabGoalStatus.Achieved, RehabGoalStatus.PartiallyAchieved, RehabGoalStatus.NotAchieved];

        private readonly ApplicationDbContext _dbContext;
        private readonly RhbDocumentIntegrityGateway _integrity;
        private readonly RhbProfessionAuthority _professionAuthority;
        private readonly RhbMeasurementRules _measurementRules;

        public RhbProgressReviewService(
            ApplicationDbContext dbContext,
            RhbDocumentIntegrityGateway integrity,
            RhbProfessionAuthority professionAuthority,
            RhbMeasurementRules measurementRules)
        {
            _dbContext = dbContext;
            _integrity = integrity;
            _professionAuthority = professionAuthority;
            _measurementRules = measurementRules;
        }

        public static string DecisionName(RehabProgressDecision decision) => decision switch
        {
            RehabProgressDecision.Continue => "Lanjutkan",
            RehabProgressDecision.Modify => "Ubah program",
            RehabProgressDecision.Stop => "Hentikan",
            RehabProgressDecision.Refer => "Rujuk",
            RehabProgressDecision.Complete => "Selesai",
            _ => decision.ToString()
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

        public async Task<RhbResult<List<RehabProgressReviewItem>>> ListByEpisodeAsync(Guid episodeId, CancellationToken cancellationToken)
        {
            if (!await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken))
                return RhbResult<List<RehabProgressReviewItem>>.NotFound("Episode rehab tidak ditemukan.");

            var items = await Project(_dbContext.RhbProgressReviews.AsNoTracking().Where(x => x.EpisodeId == episodeId && !x.IsDelete))
                .OrderByDescending(x => x.ReviewedAt)
                .ToListAsync(cancellationToken);
            await FillAsync(items, cancellationToken);
            return RhbResult<List<RehabProgressReviewItem>>.Ok(items);
        }

        public async Task<R> GetDetailAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var detail = await LoadAsync(id, actorUserId, cancellationToken);
            return detail == null ? NotFound() : R.Ok(detail);
        }

        // =====================================================================
        // Draft
        // =====================================================================

        public async Task<R> CreateAsync(Guid episodeId, SaveRehabProgressReviewRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var episode = await _dbContext.RhbEpisodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
            if (episode == null)
                return R.NotFound("Episode rehab tidak ditemukan.");
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return EpisodeClosed();
            if (!PlanningOrActive.Contains(episode.EpisodeStatus))
                return R.Conflict(RhbErrorCodes.InvalidState, "Reasesmen dilakukan setelah asesmen medis final.");

            Guid? disciplineId = null, programId = null;
            if (request.DisciplineId.HasValue)
            {
                disciplineId = request.DisciplineId.Value;
                if (!request.ProgramId.HasValue)
                    return R.Rule(RhbErrorCodes.ProgramNotActive, "Pilih program aktif layanan ini.").WithFields("programId");
                var program = await _dbContext.RhbPrograms.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ProgramId.Value && x.EpisodeId == episodeId && !x.IsDelete, cancellationToken);
                if (program == null || program.DisciplineId != disciplineId)
                    return R.Rule(RhbErrorCodes.ProgramNotInEpisode, "Program tidak ditemukan pada episode dan layanan ini.").WithFields("programId");
                if (program.ProgramStatus != RehabProgramStatus.Active)
                    return R.Conflict(RhbErrorCodes.ProgramNotActive, "Program terapi tidak aktif.");
                programId = program.Id;

                if (!await _professionAuthority.CanActForDisciplineAsync(actorUserId, disciplineId.Value, cancellationToken))
                    return R.Forbidden(RhbErrorCodes.ProfessionMismatch, "Profesi Anda tidak sesuai dengan layanan ini.");
            }
            else if (request.ProgramId.HasValue)
            {
                return R.Invalid(RhbErrorCodes.ProgramNotInEpisode, "Reasesmen medis tidak dikaitkan ke satu program. Kosongkan program, atau pilih layanannya.").WithFields("programId");
            }

            if (!request.EncounterId.HasValue)
                return EncounterInvalid("Pilih kunjungan tempat reasesmen dilakukan.");
            var encounter = await RhbServiceSupport.CheckEncounterAsync(_dbContext, request.EncounterId.Value, episode.PatientId, cancellationToken);
            if (!encounter.IsValid)
                return EncounterInvalid(encounter.Exists && !encounter.BelongsToPatient
                    ? "Kunjungan bukan milik pasien episode ini."
                    : "Kunjungan tidak ditemukan, sudah berakhir, atau pasien sudah pulang.");

            var now = DateTime.UtcNow;
            var (error, values) = await ValidateAsync(request, episodeId, disciplineId, now, cancellationToken);
            if (error != null)
                return error;

            var entity = new RhbProgressReview
            {
                EpisodeId = episodeId,
                DisciplineId = disciplineId,
                ProgramId = programId,
                EncounterId = request.EncounterId.Value,
                AuthorUserId = actorUserId,
                RecordStatus = RehabRecordStatus.Draft,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            Apply(entity, values!);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            _dbContext.RhbProgressReviews.Add(entity);
            AddChildren(entity, values!, actorUserId, now);
            await _integrity.RegisterDraftAsync(ClinicalDocumentKind.RehabProgressReview, entity.Id, episode.PatientId, entity.EncounterId, actorUserId, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            await transaction.CommitAsync(cancellationToken);
            return R.Created((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> UpdateAsync(Guid id, SaveRehabProgressReviewRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, guard) = await LoadForAuthorAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            if (request.DisciplineId != entity!.DisciplineId && request.DisciplineId.HasValue)
                return R.Invalid(RhbErrorCodes.DisciplineRequired, "Layanan reasesmen tidak dapat diganti.").WithFields("disciplineId");
            if (request.ProgramId.HasValue && request.ProgramId != entity.ProgramId)
                return R.Invalid(RhbErrorCodes.ProgramNotInEpisode, "Program reasesmen tidak dapat diganti.").WithFields("programId");
            if (request.EncounterId.HasValue && request.EncounterId.Value != entity.EncounterId)
                return EncounterInvalid("Kunjungan reasesmen tidak dapat diganti.");

            var now = DateTime.UtcNow;
            var (error, values) = await ValidateAsync(request, entity.EpisodeId, entity.DisciplineId, now, cancellationToken);
            if (error != null)
                return error;

            Apply(entity, values!);
            Touch(entity, actorUserId, now);

            var oldMeasurements = await _dbContext.RhbMeasurements.Where(x => x.ProgressReviewId == entity.Id && !x.IsDelete).ToListAsync(cancellationToken);
            var oldGoalReviews = await _dbContext.RhbGoalReviews.Where(x => x.ProgressReviewId == entity.Id && !x.IsDelete).ToListAsync(cancellationToken);
            foreach (var row in oldMeasurements) SoftDelete(row, actorUserId, now);
            foreach (var row in oldGoalReviews) SoftDelete(row, actorUserId, now);

            // Penilaian tujuan lama terhapus dulu (indeks unik ProgressReviewId+GoalId), baru yang baru disisipkan.
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

        // =====================================================================
        // Tanda tangan + efek keputusan
        // =====================================================================

        public async Task<R> SignAsync(
            Guid id,
            RehabSignRequest request,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            var (entity, guard) = await LoadForAuthorAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            // Program yang sudah tidak aktif adalah penghalang paling mendasar: dilaporkan sebelum kelengkapan isian.
            RhbProgram? program = null;
            if (entity!.ProgramId.HasValue)
            {
                program = await _dbContext.RhbPrograms.FirstAsync(x => x.Id == entity.ProgramId.Value, cancellationToken);
                if (program.ProgramStatus != RehabProgramStatus.Active)
                    return R.Conflict(RhbErrorCodes.ProgramNotActive, "Program terapi tidak lagi aktif. Batalkan draft ini lalu buat reasesmen untuk program yang berlaku.");
            }

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(entity.ProgressSummaryText)) missing.Add("progressSummaryText");
            if (!entity.Decision.HasValue) missing.Add("decision");
            if (string.IsNullOrWhiteSpace(entity.DecisionReasonText)) missing.Add("decisionReasonText");
            if (entity.Decision == RehabProgressDecision.Refer && string.IsNullOrWhiteSpace(entity.ReferralTargetText)) missing.Add("referralTargetText");
            if (missing.Count > 0)
                return R.Rule(RhbErrorCodes.ProgressReviewIncomplete, "Lengkapi ringkasan kemajuan, keputusan, alasannya, dan tujuan rujukan bila merujuk.")
                    .WithFields(missing.ToArray());

            var stored = await _measurementRules.ValidateStoredInstrumentsAsync(
                _dbContext.RhbMeasurements.AsNoTracking().Where(m => m.ProgressReviewId == entity.Id), cancellationToken);
            if (stored != null)
                return stored.ToResult<RehabProgressReviewDetailResponse>();

            var changeError = await InstrumentChangeRuleAsync(entity.Id, entity.EpisodeId, entity.DisciplineId, entity.InstrumentChangeNote, cancellationToken);
            if (changeError != null)
                return changeError;

            var goalReviews = await _dbContext.RhbGoalReviews.AsNoTracking()
                .Where(x => x.ProgressReviewId == entity.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var goalIds = goalReviews.Select(x => x.GoalId).ToList();
            var goals = await _dbContext.RhbGoals.Where(x => goalIds.Contains(x.Id)).ToListAsync(cancellationToken);
            if (goals.Any(g => g.GoalStatus != RehabGoalStatus.Active))
                return R.Conflict(RhbErrorCodes.InvalidState, "Salah satu tujuan yang dinilai sudah tidak aktif. Perbarui penilaian tujuan.");

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            entity.RecordStatus = RehabRecordStatus.Final;
            entity.SignedAt = now;
            Touch(entity, actorUserId, now);
            await ApplyDecisionAsync(entity, program, actorUserId, now, cancellationToken);

            foreach (var review in goalReviews.Where(x => x.AchievementStatus != RehabGoalStatus.Active))
            {
                var goal = goals.First(g => g.Id == review.GoalId);
                goal.GoalStatus = review.AchievementStatus;
                goal.UpdateBy = actorUserId;
                goal.UpdateDateTime = now;
                goal.Version++;
                RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Goal, goal.Id, RehabGoalStatus.Active.ToString(),
                    review.AchievementStatus.ToString(), "progress-review", null, actorUserId, now);
            }

            try
            {
                var (result, _) = await _integrity.SignAsync(ClinicalDocumentKind.RehabProgressReview, entity.Id, actorUserId, deviceInfo, ipAddress, now, cancellationToken);
                if (!result.IsAllowed)
                {
                    DetachPending();
                    return result.StatusCode == StatusCodes.Status403Forbidden
                        ? R.Forbidden(RhbErrorCodes.NotAuthor, result.ErrorMessage ?? "Hanya penulis reasesmen yang dapat menandatanganinya.")
                        : R.Conflict(RhbErrorCodes.DocumentLocked, result.ErrorMessage ?? "Catatan sudah final, gunakan addendum.");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                DetachPending();
                return Stale();
            }

            await transaction.CommitAsync(cancellationToken);
            return R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        /// <summary>
        /// Batalkan draft (delta kontrak BE-RHB-014): tanpa aksi ini, draft yang programnya sudah tidak aktif tidak
        /// dapat ditandatangani maupun disingkirkan, dan akan menahan penutupan episode (V-DIS-02).
        /// </summary>
        public async Task<R> CancelAsync(Guid id, RehabReasonRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, guard) = await LoadForAuthorAsync(id, request.ExpectedVersion, actorUserId, cancellationToken, requireProfession: false);
            if (guard != null)
                return guard;

            var reason = RhbServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return R.Rule(RhbErrorCodes.ReasonRequired, "Alasan wajib diisi.").WithFields("reason");
            if (reason.Length > 500)
                return TooLong("reason", 500);

            var now = DateTime.UtcNow;
            entity!.RecordStatus = RehabRecordStatus.Cancelled;
            Touch(entity, actorUserId, now);
            await _integrity.MarkCancelledAsync(ClinicalDocumentKind.RehabProgressReview, entity.Id, actorUserId, reason, now, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        /// <summary>Efek keputusan — <c>state-transition-matrix.md</c> §7.</summary>
        private async Task ApplyDecisionAsync(RhbProgressReview review, RhbProgram? program, Guid actorUserId, DateTime nowUtc, CancellationToken cancellationToken)
        {
            var decision = review.Decision!.Value;

            if (program == null)
            {
                // Reasesmen medis: hanya menandai episode siap ditutup; status episode tidak berubah.
                if (decision is RehabProgressDecision.Stop or RehabProgressDecision.Refer or RehabProgressDecision.Complete)
                {
                    var status = (await _dbContext.RhbEpisodes.AsNoTracking().Where(x => x.Id == review.EpisodeId).Select(x => x.EpisodeStatus).FirstAsync(cancellationToken)).ToString();
                    RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Episode, review.EpisodeId, status, status,
                        $"ready-to-close:{decision}", review.DecisionReasonText, actorUserId, nowUtc);
                }
                return;
            }

            void TouchProgram(RhbProgram p)
            {
                p.UpdateBy = actorUserId;
                p.UpdateDateTime = nowUtc;
                p.Version++;
            }

            switch (decision)
            {
                case RehabProgressDecision.Modify:
                    if (!program.NeedsReview)
                    {
                        program.NeedsReview = true;
                        TouchProgram(program);
                    }
                    break;
                case RehabProgressDecision.Stop:
                case RehabProgressDecision.Refer:
                    program.ProgramStatus = RehabProgramStatus.Stopped;
                    program.StoppedAt = nowUtc;
                    var stopReason = decision == RehabProgressDecision.Refer
                        ? $"Dirujuk ke {review.ReferralTargetText}: {review.DecisionReasonText}"
                        : review.DecisionReasonText;
                    program.StopReason = stopReason is { Length: > 500 } ? stopReason[..500] : stopReason;
                    TouchProgram(program);
                    RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Program, program.Id, RehabProgramStatus.Active.ToString(),
                        RehabProgramStatus.Stopped.ToString(), $"progress-review-{decision.ToString().ToLowerInvariant()}", review.DecisionReasonText, actorUserId, nowUtc);
                    break;
                case RehabProgressDecision.Complete:
                    program.ProgramStatus = RehabProgramStatus.Completed;
                    TouchProgram(program);
                    RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Program, program.Id, RehabProgramStatus.Active.ToString(),
                        RehabProgramStatus.Completed.ToString(), "progress-review-complete", review.DecisionReasonText, actorUserId, nowUtc);
                    break;
            }
        }

        // =====================================================================
        // Validasi
        // =====================================================================

        private sealed record Values(
            DateTime ReviewedAt,
            string? ProgressSummary,
            string? Barrier,
            RehabProgressDecision? Decision,
            string? DecisionReason,
            string? ReferralTarget,
            string? InstrumentChangeNote,
            List<RhbMeasurementValue> Measurements,
            List<(Guid GoalId, RehabGoalStatus Status, string? Note)> GoalReviews);

        private async Task<(R? Error, Values? Values)> ValidateAsync(
            SaveRehabProgressReviewRequest request,
            Guid episodeId,
            Guid? disciplineId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var reviewedAt = request.ReviewedAt.HasValue ? RhbServiceSupport.ToUtc(request.ReviewedAt.Value) : nowUtc;
            if (reviewedAt > nowUtc + ClockSkewTolerance)
                return (R.Invalid(RhbErrorCodes.ProgressReviewIncomplete, "Waktu reasesmen tidak boleh di masa depan.").WithFields("reviewedAt"), null);

            var summary = RhbServiceSupport.Normalize(request.ProgressSummaryText);
            var barrier = RhbServiceSupport.Normalize(request.BarrierText);
            var reason = RhbServiceSupport.Normalize(request.DecisionReasonText);
            var referral = RhbServiceSupport.Normalize(request.ReferralTargetText);
            var changeNote = RhbServiceSupport.Normalize(request.InstrumentChangeNote);
            foreach (var (field, value, max) in new (string, string?, int)[]
                     {
                         ("progressSummaryText", summary, 4000), ("barrierText", barrier, 1000), ("decisionReasonText", reason, 1000),
                         ("referralTargetText", referral, 200), ("instrumentChangeNote", changeNote, 500)
                     })
            {
                if (value?.Length > max)
                    return (TooLong(field, max), null);
            }

            if (request.Decision.HasValue && !Enum.IsDefined(request.Decision.Value))
                return (R.Invalid(RhbErrorCodes.ProgressReviewIncomplete, "Keputusan reasesmen tidak dikenal.").WithFields("decision"), null);

            var measurementInput = request.Measurements ?? new();
            if (measurementInput.Count > RhbMeasurementRules.MaxMeasurements)
                return (R.Invalid(RhbErrorCodes.MeasurementInvalid, $"Pengukuran maksimal {RhbMeasurementRules.MaxMeasurements} baris.").WithFields("measurements"), null);
            var measurements = new List<RhbMeasurementValue>();
            for (var i = 0; i < measurementInput.Count; i++)
            {
                var (failure, value) = await _measurementRules.ValidateAsync(measurementInput[i], i, disciplineId, nowUtc, cancellationToken);
                if (failure != null)
                    return (failure.ToResult<RehabProgressReviewDetailResponse>(), null);
                measurements.Add(value!);
            }

            // V-MEA-02 sudah diperiksa saat simpan, supaya draft tidak menyimpan pembanding yang tidak sepadan tanpa alasan.
            var baseline = await BaselineAsync(episodeId, disciplineId, cancellationToken);
            var changed = measurements.Where(m => IsChanged(m.Source, m.InstrumentKind, m.InstrumentName, m.BodyPartOrDomain, m.InstrumentVersionId, m.Unit, baseline)).Select(m => m.InstrumentName).ToList();
            if (changed.Count > 0 && changeNote == null)
                return (R.Rule(RhbErrorCodes.InstrumentChangeUnexplained, "Instrumen berbeda dari baseline. Jelaskan alasannya.", changed).WithFields("instrumentChangeNote"), null);

            var goalInput = request.GoalReviews ?? new();
            if (goalInput.Select(x => x.GoalId).Distinct().Count() != goalInput.Count)
                return (R.Invalid(RhbErrorCodes.GoalIncomplete, "Tujuan yang dinilai tidak boleh berulang.").WithFields("goalReviews"), null);
            var goalIds = goalInput.Select(x => x.GoalId).ToList();
            var validGoals = await _dbContext.RhbGoals.AsNoTracking().CountAsync(x =>
                goalIds.Contains(x.Id) && x.EpisodeId == episodeId && !x.IsDelete && x.GoalStatus == RehabGoalStatus.Active &&
                (disciplineId == null || x.DisciplineId == null || x.DisciplineId == disciplineId), cancellationToken);
            if (validGoals != goalIds.Count)
                return (R.Rule(RhbErrorCodes.GoalIncomplete, "Tujuan yang dinilai harus tujuan aktif episode ini untuk layanan yang sama.").WithFields("goalReviews"), null);

            var goalReviews = new List<(Guid, RehabGoalStatus, string?)>();
            for (var i = 0; i < goalInput.Count; i++)
            {
                if (!ReviewableGoalStatuses.Contains(goalInput[i].AchievementStatus))
                    return (R.Invalid(RhbErrorCodes.GoalIncomplete, "Hasil penilaian tujuan: masih berjalan, tercapai, tercapai sebagian, atau tidak tercapai.").WithFields($"goalReviews[{i}].achievementStatus"), null);
                var note = RhbServiceSupport.Normalize(goalInput[i].NoteText);
                if (note?.Length > 500)
                    return (TooLong($"goalReviews[{i}].noteText", 500), null);
                goalReviews.Add((goalInput[i].GoalId, goalInput[i].AchievementStatus, note));
            }

            return (null, new Values(reviewedAt, summary, barrier, request.Decision, reason, referral, changeNote, measurements, goalReviews));
        }

        private sealed record Baseline(Guid? AssessmentId, List<RhbMeasurement> Rows);

        /// <summary>Asesmen awal final terakhir: profesi untuk layanan itu, atau medis bila reasesmen medis.</summary>
        private async Task<Baseline> BaselineAsync(Guid episodeId, Guid? disciplineId, CancellationToken cancellationToken)
        {
            var type = disciplineId.HasValue ? RehabAssessmentType.DisciplineInitial : RehabAssessmentType.MedicalInitial;
            var assessmentId = await _dbContext.RhbAssessments.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && x.AssessmentType == type && x.DisciplineId == disciplineId &&
                            x.RecordStatus == RehabRecordStatus.Final)
                .OrderByDescending(x => x.SignedAt)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (!assessmentId.HasValue)
                return new Baseline(null, new());

            var rows = await _dbContext.RhbMeasurements.AsNoTracking()
                .Where(x => x.AssessmentId == assessmentId.Value && !x.IsDelete)
                .ToListAsync(cancellationToken);
            return new Baseline(assessmentId, rows);
        }

        private static string MatchKey(RehabMeasurementSource source, object? kind, string name, string? bodyPart) =>
            source == RehabMeasurementSource.Instrument
                ? $"I:{kind}"
                : $"S:{name.Trim().ToLowerInvariant()}|{(bodyPart ?? string.Empty).Trim().ToLowerInvariant()}";

        private static RhbMeasurement? FindBaseline(Baseline baseline, RehabMeasurementSource source, object? kind, string name, string? bodyPart)
        {
            var key = MatchKey(source, kind, name, bodyPart);
            return baseline.Rows.FirstOrDefault(b => MatchKey(b.Source, b.InstrumentKind, b.InstrumentName, b.BodyPartOrDomain) == key);
        }

        private static bool SameUnit(string? a, string? b) =>
            string.Equals((a ?? string.Empty).Trim(), (b ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);

        /// <summary>Punya baseline sepadan, tetapi versi instrumen atau satuannya berbeda.</summary>
        private static bool IsChanged(RehabMeasurementSource source, object? kind, string name, string? bodyPart, Guid? versionId, string? unit, Baseline baseline)
        {
            var match = FindBaseline(baseline, source, kind, name, bodyPart);
            if (match == null)
                return false;
            return source == RehabMeasurementSource.Instrument ? match.InstrumentVersionId != versionId : !SameUnit(match.Unit, unit);
        }

        private async Task<R?> InstrumentChangeRuleAsync(Guid reviewId, Guid episodeId, Guid? disciplineId, string? changeNote, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(changeNote))
                return null;
            var baseline = await BaselineAsync(episodeId, disciplineId, cancellationToken);
            var rows = await _dbContext.RhbMeasurements.AsNoTracking().Where(x => x.ProgressReviewId == reviewId && !x.IsDelete).ToListAsync(cancellationToken);
            var changed = rows.Where(m => IsChanged(m.Source, m.InstrumentKind, m.InstrumentName, m.BodyPartOrDomain, m.InstrumentVersionId, m.Unit, baseline))
                .Select(m => m.InstrumentName).ToList();
            return changed.Count == 0
                ? null
                : R.Rule(RhbErrorCodes.InstrumentChangeUnexplained, "Instrumen berbeda dari baseline. Jelaskan alasannya.", changed).WithFields("instrumentChangeNote");
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>Penjaga draft: ada, episode belum ditutup, status Draft, versi, penulis, profesi (bila layanan), keutuhan RM.</summary>
        private async Task<(RhbProgressReview? Entity, R? Error)> LoadForAuthorAsync(Guid id, int expectedVersion, Guid actorUserId, CancellationToken cancellationToken, bool requireProfession = true)
        {
            var entity = await _dbContext.RhbProgressReviews.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return (null, NotFound());

            if (await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == entity.EpisodeId && x.EpisodeStatus == RehabEpisodeStatus.Closed, cancellationToken))
                return (null, EpisodeClosed());

            if (entity.RecordStatus == RehabRecordStatus.Final)
                return (null, R.Conflict(RhbErrorCodes.DocumentLocked, "Catatan sudah final, gunakan addendum."));
            if (entity.RecordStatus != RehabRecordStatus.Draft)
                return (null, R.Conflict(RhbErrorCodes.InvalidState, "Reasesmen sudah dibatalkan."));

            if (entity.Version != expectedVersion)
                return (null, Stale().WithFields("expectedVersion"));

            if (entity.AuthorUserId != actorUserId)
                return (null, R.Forbidden(RhbErrorCodes.NotAuthor, "Hanya penulis reasesmen yang dapat mengubah atau menandatanganinya."));

            if (requireProfession && entity.DisciplineId.HasValue && !await _professionAuthority.CanActForDisciplineAsync(actorUserId, entity.DisciplineId.Value, cancellationToken))
                return (null, R.Forbidden(RhbErrorCodes.ProfessionMismatch, "Profesi Anda tidak sesuai dengan layanan ini."));

            var mutable = await _integrity.EnsureMutableAsync(ClinicalDocumentKind.RehabProgressReview, entity.Id, cancellationToken);
            if (!mutable.IsAllowed)
                return (null, R.Conflict(RhbErrorCodes.DocumentLocked, mutable.ErrorMessage ?? "Catatan sudah terkunci, gunakan addendum."));

            return (entity, null);
        }

        private static void Apply(RhbProgressReview entity, Values values)
        {
            entity.ReviewedAt = values.ReviewedAt;
            entity.ProgressSummaryText = values.ProgressSummary;
            entity.BarrierText = values.Barrier;
            entity.Decision = values.Decision;
            entity.DecisionReasonText = values.DecisionReason;
            entity.ReferralTargetText = values.ReferralTarget;
            entity.InstrumentChangeNote = values.InstrumentChangeNote;
        }

        private void AddChildren(RhbProgressReview entity, Values values, Guid actorUserId, DateTime nowUtc)
        {
            foreach (var m in values.Measurements)
                _dbContext.RhbMeasurements.Add(RhbMeasurementRules.ToEntity(m, null, entity.Id, actorUserId, nowUtc));

            foreach (var (goalId, status, note) in values.GoalReviews)
            {
                _dbContext.RhbGoalReviews.Add(new RhbGoalReview
                {
                    ProgressReviewId = entity.Id,
                    GoalId = goalId,
                    AchievementStatus = status,
                    NoteText = note,
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

        private static void Touch(RhbProgressReview entity, Guid actorUserId, DateTime nowUtc)
        {
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = nowUtc;
            entity.Version++;
        }

        private static IQueryable<RehabProgressReviewItem> Project(IQueryable<RhbProgressReview> rows) =>
            rows.Select(x => new RehabProgressReviewItem
            {
                Id = x.Id,
                EpisodeId = x.EpisodeId,
                DisciplineId = x.DisciplineId,
                DisciplineName = x.Discipline != null ? x.Discipline.DisciplineName : null,
                IsMedicalReview = x.DisciplineId == null,
                ProgramId = x.ProgramId,
                ProgramVersion = x.Program != null ? x.Program.ProgramVersion : null,
                EncounterId = x.EncounterId,
                ReviewedAt = x.ReviewedAt,
                Decision = x.Decision,
                AuthorUserId = x.AuthorUserId,
                RecordStatus = x.RecordStatus,
                SignedAt = x.SignedAt,
                Version = x.Version
            });

        private async Task FillAsync<T>(List<T> items, CancellationToken cancellationToken) where T : RehabProgressReviewItem
        {
            var authorIds = items.Select(x => x.AuthorUserId).Distinct().ToList();
            var encounterIds = items.Select(x => x.EncounterId).Distinct().ToList();
            var authors = await _dbContext.Users.AsNoTracking().Where(u => authorIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);
            var encounters = await _dbContext.Set<RegPatientEncounter>().AsNoTracking().Where(e => encounterIds.Contains(e.Id)).ToDictionaryAsync(e => e.Id, e => e.EncounterNumber, cancellationToken);
            foreach (var item in items)
            {
                item.DecisionName = item.Decision.HasValue ? DecisionName(item.Decision.Value) : null;
                item.RecordStatusName = RhbEpisodeService.RecordStatusName(item.RecordStatus);
                item.AuthorName = authors.TryGetValue(item.AuthorUserId, out var name) ? name : null;
                item.EncounterNumber = encounters.TryGetValue(item.EncounterId, out var number) ? number : null;
            }
        }

        private async Task<RehabProgressReviewDetailResponse?> LoadAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var row = await _dbContext.RhbProgressReviews.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (row == null)
                return null;

            var item = await Project(_dbContext.RhbProgressReviews.AsNoTracking().Where(x => x.Id == id)).FirstAsync(cancellationToken);
            var detail = new RehabProgressReviewDetailResponse
            {
                Id = item.Id, EpisodeId = item.EpisodeId, DisciplineId = item.DisciplineId, DisciplineName = item.DisciplineName,
                IsMedicalReview = item.IsMedicalReview, ProgramId = item.ProgramId, ProgramVersion = item.ProgramVersion,
                EncounterId = item.EncounterId, ReviewedAt = item.ReviewedAt, Decision = item.Decision, AuthorUserId = item.AuthorUserId,
                RecordStatus = item.RecordStatus, SignedAt = item.SignedAt, Version = item.Version,
                EpisodeNumber = await _dbContext.RhbEpisodes.AsNoTracking().Where(e => e.Id == row.EpisodeId).Select(e => e.EpisodeNumber).FirstAsync(cancellationToken),
                PatientId = await _dbContext.RhbEpisodes.AsNoTracking().Where(e => e.Id == row.EpisodeId).Select(e => e.PatientId).FirstAsync(cancellationToken),
                ProgressSummaryText = row.ProgressSummaryText,
                BarrierText = row.BarrierText,
                DecisionReasonText = row.DecisionReasonText,
                ReferralTargetText = row.ReferralTargetText,
                InstrumentChangeNote = row.InstrumentChangeNote
            };
            await FillAsync([detail], cancellationToken);

            if (row.ProgramId.HasValue)
            {
                detail.ProgramStatus = await _dbContext.RhbPrograms.AsNoTracking().Where(p => p.Id == row.ProgramId).Select(p => p.ProgramStatus).FirstAsync(cancellationToken);
                detail.ProgramStatusName = RhbProgramService.StatusName(detail.ProgramStatus.Value);
            }

            var measurements = await _dbContext.RhbMeasurements.AsNoTracking()
                .Where(x => x.ProgressReviewId == id && !x.IsDelete)
                .OrderBy(x => x.MeasuredAt)
                .ToListAsync(cancellationToken);
            detail.Measurements = measurements.Select(m => new RehabMeasurementItem
            {
                Id = m.Id,
                Source = m.Source,
                InstrumentVersionId = m.InstrumentVersionId,
                InstrumentKind = m.InstrumentKind,
                InstrumentName = m.InstrumentName,
                BodyPartOrDomain = m.BodyPartOrDomain,
                Answers = string.IsNullOrWhiteSpace(m.AnswersJson) ? null : JsonDocument.Parse(m.AnswersJson).RootElement.Clone(),
                NumericValue = m.NumericValue,
                ValueText = m.ValueText,
                Unit = m.Unit,
                ScoreBandCode = m.ScoreBandCode,
                MeasuredAt = m.MeasuredAt
            }).ToList();

            var baseline = await BaselineAsync(row.EpisodeId, row.DisciplineId, cancellationToken);
            detail.BaselineAssessmentId = baseline.AssessmentId;
            detail.Comparisons = measurements.Select(m =>
            {
                var match = FindBaseline(baseline, m.Source, m.InstrumentKind, m.InstrumentName, m.BodyPartOrDomain);
                var comparable = match != null && !IsChanged(m.Source, m.InstrumentKind, m.InstrumentName, m.BodyPartOrDomain, m.InstrumentVersionId, m.Unit, baseline);
                return new RehabMeasurementComparison
                {
                    MeasurementId = m.Id,
                    InstrumentName = m.InstrumentName,
                    BodyPartOrDomain = m.BodyPartOrDomain,
                    InstrumentKind = m.InstrumentKind,
                    CurrentValue = m.NumericValue,
                    CurrentValueText = m.ValueText,
                    CurrentUnit = m.Unit,
                    HasBaseline = match != null,
                    BaselineMeasurementId = match?.Id,
                    BaselineValue = match?.NumericValue,
                    BaselineValueText = match?.ValueText,
                    BaselineUnit = match?.Unit,
                    BaselineMeasuredAt = match?.MeasuredAt,
                    IsComparable = comparable,
                    Difference = comparable && m.NumericValue.HasValue && match!.NumericValue.HasValue ? m.NumericValue - match.NumericValue : null
                };
            }).ToList();

            var goalReviews = await _dbContext.RhbGoalReviews.AsNoTracking()
                .Where(x => x.ProgressReviewId == id && !x.IsDelete)
                .Select(x => new RehabGoalReviewItem
                {
                    GoalId = x.GoalId,
                    Term = x.Goal!.Term,
                    DescriptionText = x.Goal.DescriptionText,
                    TargetText = x.Goal.TargetText,
                    AchievementStatus = x.AchievementStatus,
                    NoteText = x.NoteText
                })
                .ToListAsync(cancellationToken);
            foreach (var goal in goalReviews)
                goal.AchievementStatusName = RhbGoalService.StatusName(goal.AchievementStatus);
            detail.GoalReviews = goalReviews.OrderBy(x => x.Term).ToList();

            var integrity = await _integrity.FindAsync(ClinicalDocumentKind.RehabProgressReview, id, cancellationToken);
            detail.IntegrityStatus = integrity?.IntegrityStatus;
            detail.IntegrityStatusName = integrity != null ? IntegrityStatusName(integrity.IntegrityStatus) : null;
            var mutableDraft = row.RecordStatus == RehabRecordStatus.Draft && integrity?.IntegrityStatus is null or ClinicalDocumentIntegrityStatus.Draft;
            detail.AvailableActions = mutableDraft && row.AuthorUserId == actorUserId
                ? ["update", "sign", "cancel"]
                : row.RecordStatus == RehabRecordStatus.Final || integrity?.IntegrityStatus == ClinicalDocumentIntegrityStatus.LockedUnsigned
                    ? ["addendum"]
                    : [];

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
                return R.Conflict(RhbErrorCodes.InvalidState, "Reasesmen ini sudah diproses pengguna lain. Muat ulang data.");
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

        private static R NotFound() => R.NotFound("Reasesmen tidak ditemukan.");

        private static R Stale() => R.Conflict(RhbErrorCodes.StaleVersion, "Data sudah diubah pengguna lain. Muat ulang sebelum menyimpan.");

        private static R EpisodeClosed() => R.Conflict(RhbErrorCodes.EpisodeClosed, "Episode rehab sudah ditutup.");

        private static R EncounterInvalid(string message) =>
            R.Rule(RhbErrorCodes.ClinicalEncounterInvalid, message).WithFields("encounterId");

        private static R TooLong(string field, int max) =>
            R.Invalid(RhbErrorCodes.FieldTooLong, $"Isian terlalu panjang (maksimal {max} karakter).").WithFields(field);
    }
}
