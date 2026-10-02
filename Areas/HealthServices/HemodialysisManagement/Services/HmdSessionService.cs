using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Pelaksanaan sesi HD dari pasien datang sampai dokumentasi perawat diajukan
    /// (<c>BE-HMD-12</c> sampai <c>BE-HMD-16</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Penguncian ditegakkan di dua tempat</b> (Temuan Kritis 1). Setiap penulisan data klinis
    /// sesi melewati <see cref="GuardWritableAsync{T}"/>: sesi berstatus <c>Finalized</c> ditolak,
    /// <b>dan</b> <c>ClinicalDocumentIntegrityService.EnsureMutableAsync</c> untuk jenis
    /// <c>HemodialysisSession</c> ditanya. Keduanya menjawab <c>423 HMD-VAL-075</c>.
    /// </para>
    /// <para>
    /// <b>Mulai sesi idempoten.</b> Tombol Mulai membawa kunci idempotency dan dijalankan di dalam
    /// transaksi yang memegang kunci sesi. Permintaan kedua dengan kunci yang sama — misalnya
    /// dikirim ulang 500 ms kemudian karena jaringan lambat — menunggu permintaan pertama selesai,
    /// lalu menerima sesi yang sama tanpa membuat tindakan kedua. Unique index
    /// <c>IdempotencyKey</c> dan <c>PatientProcedureId</c> menjadi penjaga terakhirnya.
    /// </para>
    /// <para>
    /// <b>Pemeriksaan tepat waktu.</b> Saat Mulai ditekan, seluruh syarat siap diperiksa ulang
    /// detik itu juga. Contoh: sesi dinyatakan siap 06.55, teknisi memblokir mesin <c>M-01</c> pukul
    /// 06.58, perawat menekan Mulai pukul 07.02 — ditolak <c>422 HMD-VAL-051</c>, tidak ada
    /// tindakan terbentuk, dan sesi tetap <c>Ready</c>.
    /// </para>
    /// <para>
    /// <b>Catatan klinis dahulu, persediaan belakangan.</b> Pemberian obat selalu tersimpan lebih
    /// dulu. Penerusan ke Farmasi dijalankan sesudahnya; bila Farmasi gagal, catatan tetap ada dan
    /// status penerusannya <c>Pending</c> untuk diulang.
    /// </para>
    /// </remarks>
    public class HmdSessionService
    {
        private const string SessionNotFoundMessage = "Sesi hemodialisa tidak ditemukan atau sudah dihapus.";

        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalDocumentIntegrityService _integrityService;
        private readonly HmdUnitReadinessService _unitReadinessService;
        private readonly InpatientClinicalContextService _clinicalContextService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly DrugUsageService _drugUsageService;

        public HmdSessionService(
            ApplicationDbContext dbContext,
            ClinicalDocumentIntegrityService integrityService,
            HmdUnitReadinessService unitReadinessService,
            InpatientClinicalContextService clinicalContextService,
            NumberSeriesAllocator numberSeriesAllocator,
            DrugUsageService drugUsageService)
        {
            _dbContext = dbContext;
            _integrityService = integrityService;
            _unitReadinessService = unitReadinessService;
            _clinicalContextService = clinicalContextService;
            _numberSeriesAllocator = numberSeriesAllocator;
            _drugUsageService = drugUsageService;
        }

        // =================================================================
        // Konteks sesi
        // =================================================================

        public async Task<HmdSessionDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        {
            var baseRow = await HmdSessionProjection.Project(_dbContext.HmdSessions.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken);
            if (baseRow == null)
                return null;

            var extra = await _dbContext.HmdSessions.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.StopNote,
                    x.DeviationNote,
                    x.DispositionNote,
                    BirthDate = x.Episode != null && x.Episode.Patient != null ? x.Episode.Patient.BirthDate : null,
                    ObservationCount = x.Observations.Count(o => !o.IsDelete),
                    MedicationCount = x.Medications.Count(m => !m.IsDelete),
                    ComplicationCount = x.Complications.Count(c => !c.IsDelete)
                })
                .FirstAsync(cancellationToken);

            var detail = new HmdSessionDetailResponse();
            CopyBase(baseRow, detail);
            HmdSessionProjection.Label(detail);

            detail.Patient = new HmdPatientHeaderResponse
            {
                PatientId = baseRow.PatientId,
                PatientName = baseRow.PatientName,
                MedicalRecordNumber = baseRow.MedicalRecordNumber,
                BirthDate = extra.BirthDate
            };
            detail.StopNote = extra.StopNote;
            detail.DeviationNote = extra.DeviationNote;
            detail.DispositionNote = extra.DispositionNote;
            detail.ObservationCount = extra.ObservationCount;
            detail.MedicationCount = extra.MedicationCount;
            detail.ComplicationCount = extra.ComplicationCount;

            detail.Prescription = await _dbContext.HmdPrescriptions.AsNoTracking()
                .Where(x => x.Id == baseRow.PrescriptionId)
                .Select(x => new HmdPrescriptionSnapshotResponse
                {
                    Id = x.Id,
                    PrescribingDoctorName = x.PrescribingDoctor != null ? x.PrescribingDoctor.FullName : null,
                    FrequencyPerWeek = x.FrequencyPerWeek,
                    TargetDurationMinutes = x.TargetDurationMinutes,
                    TargetUltrafiltrationMl = x.TargetUltrafiltrationMl,
                    BloodFlowRate = x.BloodFlowRate,
                    DialysateFlowRate = x.DialysateFlowRate,
                    DialyzerType = x.DialyzerType,
                    DialysateComposition = x.DialysateComposition,
                    SodiumBicarbonateProfile = x.SodiumBicarbonateProfile,
                    DialysateTemperatureC = x.DialysateTemperatureC,
                    AnticoagulantPlan = x.AnticoagulantPlan,
                    VascularAccessSite = x.VascularAccess != null ? x.VascularAccess.AccessSite : null,
                    PrescriptionStatus = x.PrescriptionStatus
                })
                .FirstOrDefaultAsync(cancellationToken);

            detail.IsolationRequirement = await HmdServiceSupport.GetActiveIsolationAsync(_dbContext, baseRow.EpisodeId, baseRow.ScheduledDate, cancellationToken);
            detail.IsolationRequirementName = HmdLabels.Isolation(detail.IsolationRequirement);
            detail.PreAssessment = await ReadAssessmentAsync(id, HmdAssessmentPhase.Pre, cancellationToken);
            detail.PostAssessment = await ReadAssessmentAsync(id, HmdAssessmentPhase.Post, cancellationToken);

            var checklist = await ReadChecklistAsync(id, cancellationToken);
            detail.ChecklistMandatoryCount = checklist.Count(x => x.IsMandatory);
            detail.ChecklistSatisfiedCount = checklist.Count(x => x.IsMandatory && x.IsSatisfied);
            detail.StaffAssignments = await HmdScheduleService.ReadStaffAsync(_dbContext, id, cancellationToken);
            detail.AvailableActions = AvailableActionsFor(detail.SessionStatus, detail.BillingHandoffStatus);

            return detail;
        }

        /// <summary>Menandai pasien sudah datang. Kunjungan yang sah wajib ada (<c>HMD-VAL-036</c>).</summary>
        public async Task<HmdResult<HmdSessionResponse>> CheckInAsync(
            Guid id, CheckInHmdSessionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionResponse>();

            var guard = await GuardWritableAsync<HmdSessionResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus != HmdSessionStatus.Scheduled)
                return InvalidState<HmdSessionResponse>(session.SessionStatus);

            var encounterId = request.EncounterId ?? session.EncounterId;
            if (!encounterId.HasValue ||
                !(await HmdServiceSupport.CheckEncounterAsync(_dbContext, encounterId.Value, session.Episode!.PatientId, cancellationToken)).IsValid)
            {
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val036, HmdMessages.Val036);
            }

            var now = DateTime.UtcNow;
            session.EncounterId = encounterId.Value;
            session.CheckedInAt = now;
            session.CheckedInByUserId = actorUserId;
            session.SessionStatus = HmdSessionStatus.CheckedIn;
            Touch(session, actorUserId, now);

            await EnsureChecklistRowsAsync(session.Id, actorUserId, now, cancellationToken);

            return await SaveAndReturnAsync(id, cancellationToken);
        }

        // =================================================================
        // Checklist Pra-HD
        // =================================================================

        public async Task<HmdResult<List<HmdSessionChecklistResponse>>> GetChecklistAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.HmdSessions.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
                return NotFound<List<HmdSessionChecklistResponse>>();

            return HmdResult<List<HmdSessionChecklistResponse>>.Ok(await ReadChecklistAsync(id, cancellationToken));
        }

        /// <summary>
        /// Menyimpan hasil pemeriksaan butir checklist. Setiap butir menyimpan pemeriksa dan waktu
        /// server. Mengubah checklist sesi yang sudah siap mengembalikannya ke persiapan.
        /// </summary>
        public async Task<HmdResult<List<HmdSessionChecklistResponse>>> SaveChecklistAsync(
            Guid id, SaveHmdChecklistRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<List<HmdSessionChecklistResponse>>();

            var guard = await GuardWritableAsync<List<HmdSessionChecklistResponse>>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus is not (HmdSessionStatus.CheckedIn or HmdSessionStatus.PreCheck or HmdSessionStatus.Held or HmdSessionStatus.Ready))
                return InvalidState<List<HmdSessionChecklistResponse>>(session.SessionStatus);

            var now = DateTime.UtcNow;
            await EnsureChecklistRowsAsync(id, actorUserId, now, cancellationToken);

            var rows = await _dbContext.HmdSessionChecklists
                .Where(x => x.SessionId == id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            rows.AddRange(_dbContext.ChangeTracker.Entries<HmdSessionChecklist>()
                .Where(e => e.State == EntityState.Added && e.Entity.SessionId == id)
                .Select(e => e.Entity));

            foreach (var input in request.Items)
            {
                var row = rows.FirstOrDefault(x => x.ChecklistItemId == input.ChecklistItemId);
                if (row == null)
                {
                    return HmdResult<List<HmdSessionChecklistResponse>>.Invalid(
                        HmdErrorCodes.InvalidRequest, "Butir checklist yang dikirim tidak aktif atau bukan bagian dari checklist Pra-HD.");
                }

                row.Result = input.Result;
                row.Note = HmdServiceSupport.Normalize(input.Note);
                row.VerifiedByUserId = input.Result == HmdChecklistResult.NotChecked ? null : actorUserId;
                row.VerifiedAt = input.Result == HmdChecklistResult.NotChecked ? null : now;
                row.UpdateDateTime = now;
                row.UpdateBy = actorUserId;
            }

            MoveToPreCheck(session);
            Touch(session, actorUserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<List<HmdSessionChecklistResponse>>.From(failure);

            return HmdResult<List<HmdSessionChecklistResponse>>.Ok(await ReadChecklistAsync(id, cancellationToken));
        }

        /// <summary>
        /// Dokter melewati satu butir checklist dengan alasan tertulis. Ditolak <c>422 HMD-VAL-044</c>
        /// bila master butirnya bertanda tidak boleh dilewati — dan selama <c>HMD-ASM-001</c> berlaku,
        /// itu berarti seluruh butir.
        /// </summary>
        public async Task<HmdResult<HmdSessionChecklistResponse>> OverrideChecklistAsync(
            Guid id, Guid itemId, OverrideHmdChecklistRequest request, HmdActor actor, CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdSessionChecklistResponse>.Invalid(HmdErrorCodes.Val046, HmdMessages.Val046);

            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionChecklistResponse>();

            var guard = await GuardWritableAsync<HmdSessionChecklistResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus is not (HmdSessionStatus.CheckedIn or HmdSessionStatus.PreCheck or HmdSessionStatus.Held or HmdSessionStatus.Ready))
                return InvalidState<HmdSessionChecklistResponse>(session.SessionStatus);

            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(actor.Principal, actor.UserId, cancellationToken);
            if (!doctorId.HasValue)
                return HmdResult<HmdSessionChecklistResponse>.Forbidden(HmdErrorCodes.ActorNotDoctor, HmdMessages.ActorNotDoctor);

            var item = await _dbContext.HmdChecklistItems.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDelete && x.IsActive, cancellationToken);
            if (item == null)
                return HmdResult<HmdSessionChecklistResponse>.NotFound("Butir checklist tidak ditemukan atau tidak aktif.");

            // Fail-closed: nilai IsOverridable dibaca langsung dari master pada saat ini, sehingga
            // keputusan tata kelola klinis berlaku seketika tanpa restart aplikasi.
            if (!item.IsOverridable)
                return HmdResult<HmdSessionChecklistResponse>.Rule(HmdErrorCodes.Val044, HmdMessages.Val044);

            var now = DateTime.UtcNow;
            await EnsureChecklistRowsAsync(id, actor.UserId, now, cancellationToken);

            var row = await _dbContext.HmdSessionChecklists.FirstOrDefaultAsync(x => x.SessionId == id && x.ChecklistItemId == itemId && !x.IsDelete, cancellationToken)
                      ?? _dbContext.ChangeTracker.Entries<HmdSessionChecklist>()
                          .Where(e => e.State == EntityState.Added && e.Entity.SessionId == id && e.Entity.ChecklistItemId == itemId)
                          .Select(e => e.Entity)
                          .First();

            row.IsOverridden = true;
            row.OverrideReason = reason;
            row.OverriddenByUserId = actor.UserId;
            row.OverriddenAt = now;
            row.UpdateDateTime = now;
            row.UpdateBy = actor.UserId;

            MoveToPreCheck(session);
            Touch(session, actor.UserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdSessionChecklistResponse>.From(failure);

            var result = (await ReadChecklistAsync(id, cancellationToken)).First(x => x.ChecklistItemId == itemId);
            return HmdResult<HmdSessionChecklistResponse>.Ok(result);
        }

        // =================================================================
        // Penilaian Pra-HD dan Pasca-HD
        // =================================================================

        /// <summary>
        /// Menyimpan penilaian Pra-HD. Tanda vital klinisnya ditulis ke <c>TrxPatientVitalSign</c>
        /// milik Clinical Management; berat badan dan kondisi pasien tinggal pada penilaian sesi.
        /// </summary>
        public async Task<HmdResult<HmdSessionAssessmentResponse>> SavePreAssessmentAsync(
            Guid id, SaveHmdPreAssessmentRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionAssessmentResponse>();

            var guard = await GuardWritableAsync<HmdSessionAssessmentResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus is not (HmdSessionStatus.CheckedIn or HmdSessionStatus.PreCheck or HmdSessionStatus.Held or HmdSessionStatus.Ready))
                return InvalidState<HmdSessionAssessmentResponse>(session.SessionStatus);

            var saved = await UpsertAssessmentAsync(session, HmdAssessmentPhase.Pre, request, null, actorUserId, cancellationToken);
            if (!saved.IsSuccess)
                return saved;

            MoveToPreCheck(session);
            Touch(session, actorUserId, DateTime.UtcNow);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdSessionAssessmentResponse>.From(failure);

            return HmdResult<HmdSessionAssessmentResponse>.Ok((await ReadAssessmentAsync(id, HmdAssessmentPhase.Pre, cancellationToken))!);
        }

        /// <summary>
        /// Menyimpan penilaian Pasca-HD beserta disposisi pasien. Nilainya tidak pernah disalin dari
        /// penilaian Pra-HD.
        /// </summary>
        public async Task<HmdResult<HmdSessionAssessmentResponse>> SavePostAssessmentAsync(
            Guid id, SaveHmdPostAssessmentRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionAssessmentResponse>();

            var guard = await GuardWritableAsync<HmdSessionAssessmentResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus is not (HmdSessionStatus.InProgress or HmdSessionStatus.Completed or HmdSessionStatus.Stopped))
                return InvalidState<HmdSessionAssessmentResponse>(session.SessionStatus);

            var saved = await UpsertAssessmentAsync(session, HmdAssessmentPhase.Post, request, request.TargetAchieved, actorUserId, cancellationToken);
            if (!saved.IsSuccess)
                return saved;

            if (request.Disposition.HasValue)
                session.Disposition = request.Disposition.Value;
            if (request.DispositionNote != null)
                session.DispositionNote = HmdServiceSupport.Normalize(request.DispositionNote);
            if (request.ActualUltrafiltrationMl.HasValue)
                session.ActualUltrafiltrationMl = request.ActualUltrafiltrationMl.Value;

            Touch(session, actorUserId, DateTime.UtcNow);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdSessionAssessmentResponse>.From(failure);

            return HmdResult<HmdSessionAssessmentResponse>.Ok((await ReadAssessmentAsync(id, HmdAssessmentPhase.Post, cancellationToken))!);
        }

        // =================================================================
        // Siap, tahan, lanjutkan, mulai
        // =================================================================

        /// <summary>
        /// Menyatakan sesi siap dimulai (<c>HMD-VAL-040</c> sampai <c>HMD-VAL-043</c>).
        /// </summary>
        public async Task<HmdResult<HmdSessionResponse>> DeclareReadyAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionResponse>();

            var guard = await GuardWritableAsync<HmdSessionResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus != HmdSessionStatus.PreCheck)
                return InvalidState<HmdSessionResponse>(session.SessionStatus);

            var gate = await EvaluateReadinessGateAsync(session, cancellationToken);
            if (gate != null)
                return HmdResult<HmdSessionResponse>.From(gate);

            var now = DateTime.UtcNow;
            session.SessionStatus = HmdSessionStatus.Ready;
            session.ReadyAt = now;
            session.ReadyByUserId = actorUserId;
            session.HoldReason = null;
            Touch(session, actorUserId, now);

            return await SaveAndReturnAsync(id, cancellationToken);
        }

        public async Task<HmdResult<HmdSessionResponse>> HoldAsync(
            Guid id, HoldHmdSessionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdSessionResponse>.Invalid(HmdErrorCodes.Val045, HmdMessages.Val045);

            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionResponse>();

            var guard = await GuardWritableAsync<HmdSessionResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus is not (HmdSessionStatus.PreCheck or HmdSessionStatus.Ready))
                return InvalidState<HmdSessionResponse>(session.SessionStatus);

            var now = DateTime.UtcNow;
            session.SessionStatus = HmdSessionStatus.Held;
            session.HoldReason = reason;
            session.ReadyAt = null;
            session.ReadyByUserId = null;
            Touch(session, actorUserId, now);

            return await SaveAndReturnAsync(id, cancellationToken);
        }

        /// <summary>Melanjutkan sesi yang ditahan kembali ke persiapan Pra-HD (<c>Held → PreCheck</c>).</summary>
        public async Task<HmdResult<HmdSessionResponse>> ResumeAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionResponse>();

            var guard = await GuardWritableAsync<HmdSessionResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus != HmdSessionStatus.Held)
                return InvalidState<HmdSessionResponse>(session.SessionStatus);

            session.SessionStatus = HmdSessionStatus.PreCheck;
            Touch(session, actorUserId, DateTime.UtcNow);

            return await SaveAndReturnAsync(id, cancellationToken);
        }

        /// <summary>
        /// Memulai cuci darah: satu transaksi yang memindahkan sesi ke <c>InProgress</c> dengan waktu
        /// server dan membentuk satu <c>TrxPatientProcedure</c> berstatus <c>InProgress</c>.
        /// </summary>
        public async Task<HmdResult<HmdSessionResponse>> StartAsync(
            Guid id, StartHmdSessionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var key = HmdServiceSupport.Normalize(request.IdempotencyKey);
            if (key == null)
                return HmdResult<HmdSessionResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Kunci idempotency wajib dikirim ketika memulai sesi.");

            var keyOwner = await _dbContext.HmdSessions.AsNoTracking()
                .Where(x => x.IdempotencyKey == key)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (keyOwner.HasValue && keyOwner.Value != id)
                return HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.Val053, "Kunci idempotency ini sudah dipakai sesi lain.");

            if (keyOwner == id)
                return HmdResult<HmdSessionResponse>.Ok((await ReadSessionAsync(id, cancellationToken))!);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Permintaan Mulai yang datang bersamaan untuk sesi yang sama diantrekan di sini.
            await HmdServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, $"HMD_SESSION_{id:N}");

            var session = await LoadAsync(id, cancellationToken, fresh: true);
            if (session == null)
                return NotFound<HmdSessionResponse>();

            if (session.SessionStatus == HmdSessionStatus.InProgress)
            {
                if (string.Equals(session.IdempotencyKey, key, StringComparison.Ordinal))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return HmdResult<HmdSessionResponse>.Ok((await ReadSessionAsync(id, cancellationToken))!);
                }

                return HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.Val053, HmdMessages.Val053);
            }

            var guard = await GuardWritableAsync<HmdSessionResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus != HmdSessionStatus.Ready)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val050, HmdMessages.Val050);

            // Pemeriksaan tepat waktu: seluruh syarat siap diulang detik ini juga.
            var gate = await EvaluateReadinessGateAsync(session, cancellationToken);
            if (gate != null)
                return HmdResult<HmdSessionResponse>.From(gate);

            var setting = await HmdServiceSupport.FindSettingAsync(_dbContext, session.Episode!.ServiceUnitId, cancellationToken);
            if (setting == null)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.SettingMissing, HmdMessages.SettingMissing);

            var procedure = setting.ProcedureId.HasValue
                ? await _dbContext.Set<MstProcedure>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == setting.ProcedureId.Value && !x.IsDelete && x.IsActive, cancellationToken)
                : null;
            if (procedure == null)
            {
                return HmdResult<HmdSessionResponse>.Rule(
                    HmdErrorCodes.SettingMissing,
                    "Tindakan hemodialisa belum ditetapkan atau tidak aktif pada pengaturan unit. Hubungi admin unit.");
            }

            // Resep dikunci saat sesi dimulai: bila resep yang ditautkan saat penjadwalan sudah
            // digantikan, sesi memakai resep aktif episode saat ini — bukan instruksi yang usang.
            var prescription = await _dbContext.HmdPrescriptions.AsNoTracking()
                .Where(x => x.EpisodeId == session.EpisodeId && !x.IsDelete && x.PrescriptionStatus == HmdPrescriptionStatus.Active)
                .Select(x => new { x.Id, x.PrescribingDoctorId })
                .FirstOrDefaultAsync(cancellationToken);
            if (prescription == null)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val030, HmdMessages.Val030);

            var now = DateTime.UtcNow;
            var patientProcedure = new TrxPatientProcedure
            {
                EncounterId = session.EncounterId!.Value,
                PatientId = session.Episode.PatientId,
                DoctorId = session.ResponsibleDoctorId!.Value,
                InstructingDoctorId = prescription.PrescribingDoctorId,
                ServiceUnitId = session.Episode.ServiceUnitId,
                InpEpisodeId = session.InpEpisodeId,
                IdempotencyKey = $"HMD-SES-{session.Id:N}",
                ProcedureId = procedure.Id,
                ProcedureCodeSnapshot = procedure.ProcedureCode,
                ProcedureNameSnapshot = procedure.ProcedureName,
                ProcedureTypeSnapshot = procedure.ProcedureType,
                ProcedureCategoryNameSnapshot = procedure.ProcedureCategoryName,
                ProcedureMasterType = "Master",
                IsFromMasterProcedure = true,
                IsPrimaryProcedure = true,
                ProcedureSource = PatientProcedureSource.NursingAction,
                ProcedureStatus = PatientProcedureStatus.InProgress,
                ProcedureDateTime = now,
                StartedAt = now,
                Quantity = 1,
                UnitNameSnapshot = "Sesi",
                IsBillable = true,
                OrderedByUserId = actorUserId,
                PerformedByUserId = actorUserId,
                PerformedAt = now,
                ClinicalNote = $"Sesi hemodialisa {session.SessionNumber}.",
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<TrxPatientProcedure>().Add(patientProcedure);

            session.PrescriptionId = prescription.Id;
            session.SessionStatus = HmdSessionStatus.InProgress;
            session.StartedAt = now;
            session.StartedByUserId = actorUserId;
            session.IdempotencyKey = key;
            session.PatientProcedureId = patientProcedure.Id;
            Touch(session, actorUserId, now);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                HmdServiceSupport.DetachFailedEntries(_dbContext);

                var current = await ReadSessionAsync(id, cancellationToken);
                if (current != null && current.SessionStatus == HmdSessionStatus.InProgress)
                {
                    var sameKey = await _dbContext.HmdSessions.AsNoTracking().AnyAsync(x => x.Id == id && x.IdempotencyKey == key, cancellationToken);
                    return sameKey
                        ? HmdResult<HmdSessionResponse>.Ok(current)
                        : HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.Val053, HmdMessages.Val053);
                }

                return HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }

            return HmdResult<HmdSessionResponse>.Ok((await ReadSessionAsync(id, cancellationToken))!);
        }

        // =================================================================
        // Pemantauan berkala
        // =================================================================

        public async Task<HmdResult<PagedResult<HmdObservationResponse>>> GetObservationsAsync(
            Guid id, HmdObservationQuery query, CancellationToken cancellationToken)
        {
            if (!await _dbContext.HmdSessions.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
                return NotFound<PagedResult<HmdObservationResponse>>();

            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 100 : Math.Min(query.PageSize, 500);

            var rows = _dbContext.HmdSessionObservations.AsNoTracking()
                .Where(x => x.SessionId == id && !x.IsDelete)
                .OrderBy(x => x.ObservedAt).ThenBy(x => x.SequenceNumber);

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectObservation(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);

            return HmdResult<PagedResult<HmdObservationResponse>>.Ok(new PagedResult<HmdObservationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            });
        }

        /// <summary>
        /// Mencatat satu baris pemantauan. Setiap pengamatan adalah baris baru dengan nomor urut
        /// yang dialokasikan di bawah kunci sesi, sehingga riwayat tidak pernah saling menimpa.
        /// </summary>
        public async Task<HmdResult<HmdObservationResponse>> CreateObservationAsync(
            Guid id, CreateHmdObservationRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var hasValue =
                request.SystolicBp.HasValue || request.DiastolicBp.HasValue || request.PulseRate.HasValue ||
                request.RespiratoryRate.HasValue || request.TemperatureC.HasValue || request.OxygenSaturation.HasValue ||
                request.BloodFlowRate.HasValue || request.DialysateFlowRate.HasValue || request.TransmembranePressure.HasValue ||
                request.VenousPressure.HasValue || request.ArterialPressure.HasValue || request.UltrafiltrationVolumeMl.HasValue ||
                !string.IsNullOrWhiteSpace(request.Note);
            if (!hasValue)
                return HmdResult<HmdObservationResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Isi minimal satu nilai pemantauan atau catatan pengamatan.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await HmdServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, $"HMD_SESSION_{id:N}");

            var session = await LoadAsync(id, cancellationToken, fresh: true);
            if (session == null)
                return NotFound<HmdObservationResponse>();

            var guard = await GuardWritableAsync<HmdObservationResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus != HmdSessionStatus.InProgress)
                return HmdResult<HmdObservationResponse>.Rule(HmdErrorCodes.Val054, HmdMessages.Val054);

            var now = DateTime.UtcNow;
            var observedAt = request.ObservedAt.HasValue ? HmdServiceSupport.ToUtc(request.ObservedAt.Value) : now;

            if (session.StartedAt.HasValue && observedAt < session.StartedAt.Value)
                return HmdResult<HmdObservationResponse>.Invalid(HmdErrorCodes.Val055, HmdMessages.Val055);

            if (observedAt > now.AddMinutes(1))
                return HmdResult<HmdObservationResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Waktu pengamatan tidak boleh melewati waktu server saat ini.");

            var lastSequence = await _dbContext.HmdSessionObservations.AsNoTracking()
                .Where(x => x.SessionId == id)
                .MaxAsync(x => (int?)x.SequenceNumber, cancellationToken) ?? 0;

            var observation = new HmdSessionObservation
            {
                SessionId = id,
                SequenceNumber = lastSequence + 1,
                ObservedAt = observedAt,
                RecordedByUserId = actorUserId,
                SystolicBp = request.SystolicBp,
                DiastolicBp = request.DiastolicBp,
                PulseRate = request.PulseRate,
                RespiratoryRate = request.RespiratoryRate,
                TemperatureC = request.TemperatureC,
                OxygenSaturation = request.OxygenSaturation,
                BloodFlowRate = request.BloodFlowRate,
                DialysateFlowRate = request.DialysateFlowRate,
                TransmembranePressure = request.TransmembranePressure,
                VenousPressure = request.VenousPressure,
                ArterialPressure = request.ArterialPressure,
                UltrafiltrationVolumeMl = request.UltrafiltrationVolumeMl,
                Note = HmdServiceSupport.Normalize(request.Note),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdSessionObservations.Add(observation);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (HmdServiceSupport.IsUniqueViolation(exception))
            {
                await transaction.RollbackAsync(cancellationToken);
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<HmdObservationResponse>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }

            var result = await ProjectObservation(_dbContext.HmdSessionObservations.AsNoTracking().Where(x => x.Id == observation.Id))
                .FirstAsync(cancellationToken);
            return HmdResult<HmdObservationResponse>.Created(result);
        }

        // =================================================================
        // Pemberian obat
        // =================================================================

        public async Task<HmdResult<List<HmdMedicationResponse>>> GetMedicationsAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.HmdSessions.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
                return NotFound<List<HmdMedicationResponse>>();

            var items = await ProjectMedication(_dbContext.HmdSessionMedications.AsNoTracking()
                    .Where(x => x.SessionId == id && !x.IsDelete)
                    .OrderBy(x => x.AdministeredAt))
                .ToListAsync(cancellationToken);
            items.ForEach(LabelMedication);

            return HmdResult<List<HmdMedicationResponse>>.Ok(items);
        }

        /// <summary>
        /// Mencatat pemberian obat, lalu meneruskan faktanya ke Farmasi. Kegagalan Farmasi tidak
        /// pernah membatalkan catatan klinis: jawabannya tetap <c>201</c> dengan status penerusan
        /// <c>Pending</c>.
        /// </summary>
        /// <remarks>
        /// <b>Contoh.</b> Perawat memberikan Heparin 2.000 IU. Layanan Farmasi sedang gangguan.
        /// Catatan pemberian tersimpan, jawaban <c>201 Created</c>, dan <c>PharmacySyncStatus</c>
        /// bernilai <c>Pending</c> beserta sebab kegagalannya — siap diulang lewat endpoint
        /// pengulangan penerusan.
        /// </remarks>
        public async Task<HmdResult<HmdMedicationResponse>> CreateMedicationAsync(
            Guid id, CreateHmdMedicationRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            if (!request.DrugId.HasValue || request.DrugId.Value == Guid.Empty ||
                !request.Dose.HasValue || request.Dose.Value <= 0 ||
                string.IsNullOrWhiteSpace(request.DoseUnit) ||
                !request.Route.HasValue)
            {
                return HmdResult<HmdMedicationResponse>.Invalid(HmdErrorCodes.Val056, HmdMessages.Val056);
            }

            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdMedicationResponse>();

            var guard = await GuardWritableAsync<HmdMedicationResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus is not (HmdSessionStatus.InProgress or HmdSessionStatus.Completed or HmdSessionStatus.Stopped))
            {
                return HmdResult<HmdMedicationResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    "Pemberian obat hanya dapat dicatat selama sesi berlangsung atau sebelum dokumentasi diajukan.");
            }

            if (!await _dbContext.Set<MstDrug>().AsNoTracking().AnyAsync(x => x.Id == request.DrugId.Value && !x.IsDelete, cancellationToken))
                return HmdResult<HmdMedicationResponse>.Invalid(HmdErrorCodes.Val056, HmdMessages.Val056);

            var now = DateTime.UtcNow;
            var medication = new HmdSessionMedication
            {
                SessionId = id,
                DrugId = request.DrugId.Value,
                Dose = request.Dose.Value,
                DoseUnit = request.DoseUnit!.Trim(),
                Route = request.Route.Value,
                InstructedByDoctorId = request.InstructedByDoctorId,
                AdministeredByUserId = actorUserId,
                AdministeredAt = request.AdministeredAt.HasValue ? HmdServiceSupport.ToUtc(request.AdministeredAt.Value) : now,
                HandoffStatus = HmdPharmacyHandoffStatus.Pending,
                PharmacyStorageLocationId = request.PharmacyStorageLocationId,
                PharmacyMeasurementId = request.PharmacyMeasurementId,
                PharmacyQuantity = request.PharmacyQuantity,
                Note = HmdServiceSupport.Normalize(request.Note),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdSessionMedications.Add(medication);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Catatan klinis sudah tersimpan. Baru kemudian Farmasi dihubungi.
            await ForwardToPharmacyAsync(medication, session, actorUserId, cancellationToken);

            return HmdResult<HmdMedicationResponse>.Created(await ReadMedicationAsync(medication.Id, cancellationToken));
        }

        /// <summary>Mengulang penerusan pemberian obat yang tertunda atau ditolak Farmasi.</summary>
        public async Task<HmdResult<HmdMedicationResponse>> RetryPharmacyHandoffAsync(
            Guid id, Guid medicationId, RetryHmdPharmacyHandoffRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdMedicationResponse>();

            var medication = await _dbContext.HmdSessionMedications
                .FirstOrDefaultAsync(x => x.Id == medicationId && x.SessionId == id && !x.IsDelete, cancellationToken);
            if (medication == null)
                return HmdResult<HmdMedicationResponse>.NotFound("Catatan pemberian obat tidak ditemukan pada sesi ini.");

            if (medication.HandoffStatus == HmdPharmacyHandoffStatus.Succeeded)
                return HmdResult<HmdMedicationResponse>.Ok(await ReadMedicationAsync(medicationId, cancellationToken));

            // Yang boleh dilengkapi hanya data penerusan ke Farmasi, bukan isi catatan klinisnya.
            if (request.PharmacyStorageLocationId.HasValue)
                medication.PharmacyStorageLocationId = request.PharmacyStorageLocationId.Value;
            if (request.PharmacyMeasurementId.HasValue)
                medication.PharmacyMeasurementId = request.PharmacyMeasurementId.Value;
            if (request.PharmacyQuantity.HasValue)
                medication.PharmacyQuantity = request.PharmacyQuantity.Value;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await ForwardToPharmacyAsync(medication, session, actorUserId, cancellationToken);

            return HmdResult<HmdMedicationResponse>.Ok(await ReadMedicationAsync(medicationId, cancellationToken));
        }

        // =================================================================
        // Komplikasi
        // =================================================================

        public async Task<HmdResult<List<HmdComplicationResponse>>> GetComplicationsAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.HmdSessions.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
                return NotFound<List<HmdComplicationResponse>>();

            var items = await ProjectComplication(_dbContext.HmdSessionComplications.AsNoTracking()
                    .Where(x => x.SessionId == id && !x.IsDelete)
                    .OrderBy(x => x.DetectedAt))
                .ToListAsync(cancellationToken);
            items.ForEach(LabelComplication);

            return HmdResult<List<HmdComplicationResponse>>.Ok(items);
        }

        /// <summary>
        /// Mencatat komplikasi beserta penanganannya. Sistem tidak menyimpulkan komplikasi dari nilai
        /// tanda vital; perawat mencatatnya secara sadar.
        /// </summary>
        public async Task<HmdResult<HmdComplicationResponse>> CreateComplicationAsync(
            Guid id, CreateHmdComplicationRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var signs = HmdServiceSupport.Normalize(request.SignsAndSymptoms);
            var intervention = HmdServiceSupport.Normalize(request.Intervention);
            if (!request.ComplicationType.HasValue || signs == null || intervention == null)
                return HmdResult<HmdComplicationResponse>.Invalid(HmdErrorCodes.Val057, HmdMessages.Val057);

            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdComplicationResponse>();

            var guard = await GuardWritableAsync<HmdComplicationResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus is not (HmdSessionStatus.InProgress or HmdSessionStatus.Completed or HmdSessionStatus.Stopped))
            {
                return HmdResult<HmdComplicationResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    "Komplikasi hanya dapat dicatat selama sesi berlangsung atau sebelum dokumentasi diajukan.");
            }

            var now = DateTime.UtcNow;
            var complication = new HmdSessionComplication
            {
                SessionId = id,
                ComplicationType = request.ComplicationType.Value,
                DetectedAt = request.DetectedAt.HasValue ? HmdServiceSupport.ToUtc(request.DetectedAt.Value) : now,
                DetectedByUserId = actorUserId,
                Severity = HmdServiceSupport.Normalize(request.Severity),
                SignsAndSymptoms = signs,
                Intervention = intervention,
                ClinicianInstruction = HmdServiceSupport.Normalize(request.ClinicianInstruction),
                Outcome = request.Outcome,
                SessionImpact = request.SessionImpact,
                TransferDestination = HmdServiceSupport.Normalize(request.TransferDestination),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdSessionComplications.Add(complication);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = await ProjectComplication(_dbContext.HmdSessionComplications.AsNoTracking().Where(x => x.Id == complication.Id))
                .FirstAsync(cancellationToken);
            LabelComplication(result);
            return HmdResult<HmdComplicationResponse>.Created(result);
        }

        // =================================================================
        // Penghentian, penyelesaian, dokumentasi
        // =================================================================

        /// <summary>
        /// Menghentikan sesi sebelum selesai dengan alasan wajib. Tindakan pasien ditandai tidak
        /// dapat ditagih (<c>HMD-DEC-012</c>).
        /// </summary>
        public async Task<HmdResult<HmdSessionResponse>> StopAsync(
            Guid id, StopHmdSessionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            if (!request.StopReason.HasValue)
                return HmdResult<HmdSessionResponse>.Invalid(HmdErrorCodes.Val061, HmdMessages.Val061);

            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionResponse>();

            var guard = await GuardWritableAsync<HmdSessionResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus != HmdSessionStatus.InProgress)
                return InvalidState<HmdSessionResponse>(session.SessionStatus);

            var now = DateTime.UtcNow;
            session.SessionStatus = HmdSessionStatus.Stopped;
            session.StopReason = request.StopReason.Value;
            session.StopNote = HmdServiceSupport.Normalize(request.StopNote);
            session.EndedAt = now;
            session.EndedByUserId = actorUserId;
            session.ActualDurationMinutes = DurationMinutes(session.StartedAt, now);
            if (request.ActualUltrafiltrationMl.HasValue)
                session.ActualUltrafiltrationMl = request.ActualUltrafiltrationMl.Value;
            session.BillingHandoffStatus = HmdBillingHandoffStatus.NotRequired;
            Touch(session, actorUserId, now);

            if (session.PatientProcedureId.HasValue)
            {
                var procedure = await _dbContext.Set<TrxPatientProcedure>()
                    .FirstOrDefaultAsync(x => x.Id == session.PatientProcedureId.Value, cancellationToken);
                if (procedure != null)
                {
                    procedure.IsBillable = false;
                    procedure.ClinicalNote = $"Sesi hemodialisa {session.SessionNumber} dihentikan: {HmdLabels.StopReason(request.StopReason.Value)}.";
                    procedure.UpdateDateTime = now;
                    procedure.UpdateBy = actorUserId;
                }
            }

            return await SaveAndReturnAsync(id, cancellationToken);
        }

        /// <summary>
        /// Menyatakan cuci darah selesai secara fisik. Penilaian Pasca-HD dan disposisi wajib sudah
        /// terisi (<c>HMD-VAL-060</c>).
        /// </summary>
        public async Task<HmdResult<HmdSessionResponse>> CompleteAsync(
            Guid id, CompleteHmdSessionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionResponse>();

            var guard = await GuardWritableAsync<HmdSessionResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus != HmdSessionStatus.InProgress)
                return InvalidState<HmdSessionResponse>(session.SessionStatus);

            var post = await _dbContext.HmdSessionAssessments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.SessionId == id && x.Phase == HmdAssessmentPhase.Post && !x.IsDelete, cancellationToken);
            if (post == null || !post.BodyWeightKg.HasValue || !session.Disposition.HasValue)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val060, HmdMessages.Val060);

            var now = DateTime.UtcNow;
            session.SessionStatus = HmdSessionStatus.Completed;
            session.EndedAt = now;
            session.EndedByUserId = actorUserId;
            session.ActualDurationMinutes = DurationMinutes(session.StartedAt, now);
            if (request.ActualUltrafiltrationMl.HasValue)
                session.ActualUltrafiltrationMl = request.ActualUltrafiltrationMl.Value;
            if (request.DeviationNote != null)
                session.DeviationNote = HmdServiceSupport.Normalize(request.DeviationNote);
            Touch(session, actorUserId, now);

            return await SaveAndReturnAsync(id, cancellationToken);
        }

        /// <summary>
        /// Perawat menyatakan dokumentasi selesai. Sesi berpindah ke <c>AwaitingFinalization</c> dan
        /// penyelesainya tercatat terpisah dari pengesah (syarat 2).
        /// </summary>
        public async Task<HmdResult<HmdSessionResponse>> SubmitDocumentationAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await LoadAsync(id, cancellationToken);
            if (session == null)
                return NotFound<HmdSessionResponse>();

            var guard = await GuardWritableAsync<HmdSessionResponse>(session, cancellationToken);
            if (guard != null)
                return guard;

            if (session.SessionStatus is not (HmdSessionStatus.Completed or HmdSessionStatus.Stopped))
                return InvalidState<HmdSessionResponse>(session.SessionStatus);

            var missing = await FindMissingDocumentationAsync(session, _dbContext, cancellationToken);
            if (missing.Count > 0)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val070, HmdMessages.Val070, missing);

            var now = DateTime.UtcNow;
            session.SessionStatus = HmdSessionStatus.AwaitingFinalization;
            session.DocumentedByUserId = actorUserId;
            session.DocumentedAt = now;
            session.ReturnReason = null;
            Touch(session, actorUserId, now);

            return await SaveAndReturnAsync(id, cancellationToken);
        }

        /// <summary>
        /// Isian minimum dokumentasi final (<c>HMD-VAL-070</c>): waktu selesai, berat badan setelah
        /// tindakan, jumlah cairan yang ditarik, tanda vital akhir, dan tujuan pasien.
        /// </summary>
        public static async Task<List<string>> FindMissingDocumentationAsync(
            HmdSession session, ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            var missing = new List<string>();

            if (!session.EndedAt.HasValue)
                missing.Add("Waktu selesai sesi");
            if (!session.ActualUltrafiltrationMl.HasValue)
                missing.Add("Jumlah cairan yang ditarik");
            if (!session.Disposition.HasValue)
                missing.Add("Tujuan pasien setelah sesi");

            var post = await dbContext.HmdSessionAssessments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.SessionId == session.Id && x.Phase == HmdAssessmentPhase.Post && !x.IsDelete, cancellationToken);

            if (post?.BodyWeightKg == null)
                missing.Add("Berat badan setelah tindakan");

            var vital = post?.PatientVitalSignId == null
                ? null
                : await dbContext.Set<TrxPatientVitalSign>().AsNoTracking()
                    .Where(x => x.Id == post.PatientVitalSignId.Value)
                    .Select(x => new { x.BloodPressureSystolic, x.BloodPressureDiastolic, x.PulseRate })
                    .FirstOrDefaultAsync(cancellationToken);

            if (vital == null || !vital.BloodPressureSystolic.HasValue || !vital.BloodPressureDiastolic.HasValue || !vital.PulseRate.HasValue)
                missing.Add("Tanda vital akhir");

            return missing;
        }

        // =================================================================
        // Penolong
        // =================================================================

        private async Task<HmdSession?> LoadAsync(Guid id, CancellationToken cancellationToken, bool fresh = false)
        {
            var query = _dbContext.HmdSessions.Include(x => x.Episode).Where(x => x.Id == id && !x.IsDelete);
            if (fresh)
            {
                // Setelah kunci diambil, nilai wajib dibaca ulang dari database — bukan dari entity
                // yang mungkin sudah ter-tracking sebelum kunci dipegang.
                var tracked = _dbContext.ChangeTracker.Entries<HmdSession>().FirstOrDefault(e => e.Entity.Id == id);
                if (tracked != null)
                    await tracked.ReloadAsync(cancellationToken);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Penjaga penguncian dua tempat (Temuan Kritis 1): status sesi dan daftar keutuhan Rekam
        /// Medis. Keduanya menolak dengan <c>423 HMD-VAL-075</c>.
        /// </summary>
        private async Task<HmdResult<T>?> GuardWritableAsync<T>(HmdSession session, CancellationToken cancellationToken)
        {
            if (session.SessionStatus == HmdSessionStatus.Finalized)
                return HmdResult<T>.Locked(HmdErrorCodes.Val075, HmdMessages.Val075);

            var integrity = await _integrityService.EnsureMutableAsync(ClinicalDocumentKind.HemodialysisSession, session.Id, cancellationToken);
            if (!integrity.IsAllowed)
                return HmdResult<T>.Locked(HmdErrorCodes.Val075, HmdMessages.Val075);

            if (session.SessionStatus == HmdSessionStatus.Cancelled)
                return HmdResult<T>.Rule(HmdErrorCodes.InvalidTransition, "Sesi sudah dibatalkan dan tidak dapat diubah.");

            if (session.Episode == null)
                return HmdResult<T>.Rule(HmdErrorCodes.Val900, HmdMessages.Val900);

            return null;
        }

        /// <summary>
        /// Syarat sesi boleh siap dan boleh dimulai — dipakai saat pernyataan siap dan diulang
        /// seluruhnya saat Mulai ditekan.
        /// </summary>
        private async Task<HmdResult<bool>?> EvaluateReadinessGateAsync(HmdSession session, CancellationToken cancellationToken)
        {
            var checklist = await ReadChecklistAsync(session.Id, cancellationToken);
            var unsatisfied = checklist.Where(x => x.IsMandatory && !x.IsSatisfied)
                .Select(x => new { x.ItemCode, x.ItemName, Result = x.ResultName })
                .ToList();
            if (checklist.Count == 0 || unsatisfied.Count > 0)
                return HmdResult<bool>.Rule(HmdErrorCodes.Val040, HmdMessages.Val040, unsatisfied);

            var pre = await ReadAssessmentAsync(session.Id, HmdAssessmentPhase.Pre, cancellationToken);
            if (pre == null || !pre.BodyWeightKg.HasValue || !pre.SystolicBp.HasValue || !pre.DiastolicBp.HasValue || !pre.PulseRate.HasValue)
                return HmdResult<bool>.Rule(HmdErrorCodes.Val041, HmdMessages.Val041);

            if (!await _unitReadinessService.IsUnitReadyAsync(session.Episode!.ServiceUnitId, session.ScheduledDate, session.Shift, cancellationToken))
                return HmdResult<bool>.Rule(HmdErrorCodes.Val042, HmdMessages.Val042);

            if (!session.ResponsibleDoctorId.HasValue)
                return HmdResult<bool>.Rule(HmdErrorCodes.Val043, HmdMessages.Val043);

            var machineReady = await _dbContext.HmdMachines.AsNoTracking().AnyAsync(x =>
                x.Id == session.MachineId && !x.IsDelete && x.IsActive && x.MachineStatus == HmdMachineStatus.Ready, cancellationToken);
            var stationReady = await _dbContext.HmdStations.AsNoTracking().AnyAsync(x =>
                x.Id == session.StationId && !x.IsDelete && x.IsActive && x.StationStatus == HmdStationStatus.Available, cancellationToken);
            if (!machineReady || !stationReady)
                return HmdResult<bool>.Rule(HmdErrorCodes.Val051, HmdMessages.Val051);

            if (!session.EncounterId.HasValue ||
                !(await HmdServiceSupport.CheckEncounterAsync(_dbContext, session.EncounterId.Value, session.Episode.PatientId, cancellationToken)).IsValid)
            {
                return HmdResult<bool>.Rule(HmdErrorCodes.Val052, HmdMessages.Val052);
            }

            return null;
        }

        /// <summary>Membentuk baris checklist untuk setiap butir master aktif yang belum ada pada sesi.</summary>
        private async Task EnsureChecklistRowsAsync(Guid sessionId, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
        {
            var existing = await _dbContext.HmdSessionChecklists.AsNoTracking()
                .Where(x => x.SessionId == sessionId && !x.IsDelete)
                .Select(x => x.ChecklistItemId)
                .ToListAsync(cancellationToken);

            var pending = _dbContext.ChangeTracker.Entries<HmdSessionChecklist>()
                .Where(e => e.State == EntityState.Added && e.Entity.SessionId == sessionId)
                .Select(e => e.Entity.ChecklistItemId);

            var known = existing.Concat(pending).ToHashSet();

            var items = await _dbContext.HmdChecklistItems.AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var itemId in items.Where(x => !known.Contains(x)))
            {
                _dbContext.HmdSessionChecklists.Add(new HmdSessionChecklist
                {
                    SessionId = sessionId,
                    ChecklistItemId = itemId,
                    Result = HmdChecklistResult.NotChecked,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }
        }

        private async Task<List<HmdSessionChecklistResponse>> ReadChecklistAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var results = await _dbContext.HmdSessionChecklists.AsNoTracking()
                .Where(x => x.SessionId == sessionId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.ChecklistItemId,
                    x.Result,
                    x.VerifiedByUserId,
                    VerifiedByName = _dbContext.Users.Where(u => u.Id == x.VerifiedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                    x.VerifiedAt,
                    x.Note,
                    x.IsOverridden,
                    x.OverrideReason,
                    x.OverriddenByUserId,
                    x.OverriddenAt
                })
                .ToListAsync(cancellationToken);

            var resultByItem = results.ToDictionary(x => x.ChecklistItemId);
            var usedItemIds = results.Select(x => x.ChecklistItemId).ToList();

            var items = await _dbContext.HmdChecklistItems.AsNoTracking()
                .Where(x => !x.IsDelete && (x.IsActive || usedItemIds.Contains(x.Id)))
                .OrderBy(x => x.CheckSequence).ThenBy(x => x.ItemCode)
                .ToListAsync(cancellationToken);

            return items.Select(item =>
            {
                resultByItem.TryGetValue(item.Id, out var r);
                var result = r?.Result ?? HmdChecklistResult.NotChecked;
                var overridden = r?.IsOverridden ?? false;
                var isMandatory = item.IsMandatory && item.IsActive;

                return new HmdSessionChecklistResponse
                {
                    Id = r?.Id,
                    ChecklistItemId = item.Id,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    Category = item.Category,
                    CategoryName = HmdLabels.ChecklistCategory(item.Category),
                    IsMandatory = isMandatory,
                    IsOverridable = item.IsOverridable,
                    CheckSequence = item.CheckSequence,
                    Result = result,
                    ResultName = HmdLabels.ChecklistResult(result),
                    VerifiedByUserId = r?.VerifiedByUserId,
                    VerifiedByName = r?.VerifiedByName,
                    VerifiedAt = r?.VerifiedAt,
                    Note = r?.Note,
                    IsOverridden = overridden,
                    OverrideReason = r?.OverrideReason,
                    OverriddenByUserId = r?.OverriddenByUserId,
                    OverriddenAt = r?.OverriddenAt,
                    // HMD-VAL-040: butir wajib lolos hanya bila terpenuhi atau dilewati secara sah.
                    // "Tidak berlaku" bukan salah satunya; bila dihitung lolos, butir yang ditolak
                    // dilewati (HMD-VAL-044) cukup ditandai tidak berlaku untuk meloloskan sesi.
                    IsSatisfied = result == HmdChecklistResult.Met || overridden ||
                                  (!isMandatory && result == HmdChecklistResult.NotApplicable)
                };
            }).ToList();
        }

        private async Task<HmdResult<HmdSessionAssessmentResponse>> UpsertAssessmentAsync(
            HmdSession session,
            HmdAssessmentPhase phase,
            SaveHmdPreAssessmentRequest request,
            bool? targetAchieved,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var assessment = await _dbContext.HmdSessionAssessments
                .FirstOrDefaultAsync(x => x.SessionId == session.Id && x.Phase == phase && !x.IsDelete, cancellationToken);

            if (assessment == null)
            {
                assessment = new HmdSessionAssessment
                {
                    SessionId = session.Id,
                    Phase = phase,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                _dbContext.HmdSessionAssessments.Add(assessment);
            }
            else
            {
                assessment.UpdateDateTime = now;
                assessment.UpdateBy = actorUserId;
            }

            assessment.AssessedAt = now;
            assessment.AssessedByUserId = actorUserId;
            assessment.BodyWeightKg = request.BodyWeightKg;
            assessment.Complaint = HmdServiceSupport.Normalize(request.Complaint);
            assessment.AccessConditionNote = HmdServiceSupport.Normalize(request.AccessConditionNote);
            assessment.PatientCondition = HmdServiceSupport.Normalize(request.PatientCondition);
            if (phase == HmdAssessmentPhase.Post)
                assessment.TargetAchieved = targetAchieved;

            var hasVital = request.SystolicBp.HasValue || request.DiastolicBp.HasValue || request.PulseRate.HasValue ||
                           request.RespiratoryRate.HasValue || request.TemperatureC.HasValue || request.OxygenSaturation.HasValue ||
                           request.BodyWeightKg.HasValue;
            if (!hasVital)
                return HmdResult<HmdSessionAssessmentResponse>.Ok(new HmdSessionAssessmentResponse());

            TrxPatientVitalSign? vital = null;
            if (assessment.PatientVitalSignId.HasValue)
            {
                vital = await _dbContext.Set<TrxPatientVitalSign>()
                    .FirstOrDefaultAsync(x => x.Id == assessment.PatientVitalSignId.Value && !x.IsDelete, cancellationToken);
            }

            if (vital == null)
            {
                var number = await HmdServiceSupport.AllocateNumberAsync(
                    _numberSeriesAllocator, HmdServiceSupport.VitalSignSequenceKey, HmdServiceSupport.VitalSignNumberPrefix, actorUserId, cancellationToken);
                if (number == null)
                    return HmdResult<HmdSessionAssessmentResponse>.Rule(HmdErrorCodes.NumberAllocationFailed, HmdMessages.NumberAllocationFailed);

                vital = new TrxPatientVitalSign
                {
                    VitalSignRecordNumber = number,
                    PatientId = session.Episode!.PatientId,
                    EncounterId = session.EncounterId,
                    DoctorId = session.ResponsibleDoctorId,
                    ServiceUnitId = session.Episode.ServiceUnitId,
                    VitalSignSource = PatientVitalSignSource.ProcedureMonitoring,
                    VitalSignStatus = PatientVitalSignStatus.Recorded,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                _dbContext.Set<TrxPatientVitalSign>().Add(vital);
                assessment.PatientVitalSignId = vital.Id;
            }
            else
            {
                vital.UpdateDateTime = now;
                vital.UpdateBy = actorUserId;
            }

            vital.ObservationDateTime = now;
            vital.ObservedByUserId = actorUserId;
            vital.ObservationLocation = phase == HmdAssessmentPhase.Pre ? "Hemodialisa — Pra-HD" : "Hemodialisa — Pasca-HD";
            vital.BloodPressureSystolic = request.SystolicBp;
            vital.BloodPressureDiastolic = request.DiastolicBp;
            vital.PulseRate = request.PulseRate;
            vital.RespiratoryRate = request.RespiratoryRate;
            vital.Temperature = request.TemperatureC;
            vital.OxygenSaturation = request.OxygenSaturation;
            vital.Weight = request.BodyWeightKg;

            return HmdResult<HmdSessionAssessmentResponse>.Ok(new HmdSessionAssessmentResponse());
        }

        private async Task<HmdSessionAssessmentResponse?> ReadAssessmentAsync(Guid sessionId, HmdAssessmentPhase phase, CancellationToken cancellationToken)
        {
            var row = await _dbContext.HmdSessionAssessments.AsNoTracking()
                .Where(x => x.SessionId == sessionId && x.Phase == phase && !x.IsDelete)
                .Select(x => new HmdSessionAssessmentResponse
                {
                    Id = x.Id,
                    SessionId = x.SessionId,
                    Phase = x.Phase,
                    AssessedAt = x.AssessedAt,
                    AssessedByUserId = x.AssessedByUserId,
                    BodyWeightKg = x.BodyWeightKg,
                    PatientVitalSignId = x.PatientVitalSignId,
                    Complaint = x.Complaint,
                    AccessConditionNote = x.AccessConditionNote,
                    PatientCondition = x.PatientCondition,
                    TargetAchieved = x.TargetAchieved
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row?.PatientVitalSignId != null)
            {
                var vital = await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking()
                    .Where(x => x.Id == row.PatientVitalSignId.Value)
                    .Select(x => new
                    {
                        x.BloodPressureSystolic,
                        x.BloodPressureDiastolic,
                        x.PulseRate,
                        x.RespiratoryRate,
                        x.Temperature,
                        x.OxygenSaturation
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (vital != null)
                {
                    row.SystolicBp = vital.BloodPressureSystolic;
                    row.DiastolicBp = vital.BloodPressureDiastolic;
                    row.PulseRate = vital.PulseRate;
                    row.RespiratoryRate = vital.RespiratoryRate;
                    row.TemperatureC = vital.Temperature;
                    row.OxygenSaturation = vital.OxygenSaturation;
                }
            }

            return row;
        }

        /// <summary>
        /// Meneruskan fakta pemberian obat ke Farmasi sebagai pemakaian obat berstatus draf; Farmasi
        /// yang mencatat pengurangan stoknya. Kunci idempotency diturunkan dari id catatan, sehingga
        /// pengulangan tidak pernah membuat pemakaian kedua.
        /// </summary>
        private async Task ForwardToPharmacyAsync(
            HmdSessionMedication medication, HmdSession session, Guid actorUserId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            medication.HandoffAttemptedAt = now;

            string? blocker = null;
            Guid? measurementId = medication.PharmacyMeasurementId;
            Guid? workforceProfileId = null;

            if (!session.EncounterId.HasValue)
                blocker = "Sesi belum punya kunjungan pasien, sehingga pemakaian obat belum dapat diteruskan ke Farmasi.";
            else if (!medication.PharmacyStorageLocationId.HasValue)
                blocker = "Lokasi stok Farmasi belum dipilih.";
            else if (!medication.PharmacyQuantity.HasValue)
                blocker = "Jumlah dalam satuan stok Farmasi belum diisi.";

            if (blocker == null)
            {
                if (!measurementId.HasValue)
                {
                    measurementId = await _dbContext.Set<MstDrug>().AsNoTracking()
                        .Where(x => x.Id == medication.DrugId)
                        .Select(x => x.StockUnitMeasurementId ?? x.BaseUnitMeasurementId)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                workforceProfileId = await _dbContext.Users.AsNoTracking()
                    .Where(x => x.Id == actorUserId)
                    .Select(x => x.WorkforceProfileId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!measurementId.HasValue)
                    blocker = "Satuan stok obat belum ditetapkan pada data induk obat.";
                else if (!workforceProfileId.HasValue)
                    blocker = "Akun petugas tidak tertaut ke profil tenaga kerja.";
            }

            if (blocker != null)
            {
                medication.HandoffStatus = HmdPharmacyHandoffStatus.Pending;
                medication.HandoffError = blocker;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            try
            {
                var usage = await _drugUsageService.CreateAsync(new CreateDrugUsageRequest
                {
                    EncounterId = session.EncounterId!.Value,
                    StorageLocationId = medication.PharmacyStorageLocationId!.Value,
                    RecordedByWorkforceId = workforceProfileId!.Value,
                    UsedAt = medication.AdministeredAt,
                    Notes = $"Pemberian obat sesi hemodialisa {session.SessionNumber}.",
                    IdempotencyKey = $"HMD-MED-{medication.Id:N}",
                    Items =
                    [
                        new DrugUsageItemInput
                        {
                            DrugId = medication.DrugId,
                            MeasurementId = measurementId!.Value,
                            Quantity = medication.PharmacyQuantity!.Value
                        }
                    ]
                }, cancellationToken);

                medication.DrugUsageId = usage.Id;
                medication.HandoffStatus = HmdPharmacyHandoffStatus.Succeeded;
                medication.HandoffError = null;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Farmasi gagal: catatan klinis tidak disentuh. Sisa entity Farmasi yang sempat
                // ditambahkan dilepas supaya tidak ikut tersimpan pada penyimpanan berikutnya.
                foreach (var entry in _dbContext.ChangeTracker.Entries().ToList())
                {
                    if (entry.Entity is not HmdSessionMedication &&
                        entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                    {
                        entry.State = EntityState.Detached;
                    }
                }

                medication.HandoffStatus = HmdPharmacyHandoffStatus.Pending;
                medication.HandoffError = exception.Message.Length > 1000 ? exception.Message[..1000] : exception.Message;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<HmdMedicationResponse> ReadMedicationAsync(Guid medicationId, CancellationToken cancellationToken)
        {
            var row = await ProjectMedication(_dbContext.HmdSessionMedications.AsNoTracking().Where(x => x.Id == medicationId))
                .FirstAsync(cancellationToken);
            LabelMedication(row);
            return row;
        }

        private static IQueryable<HmdMedicationResponse> ProjectMedication(IQueryable<HmdSessionMedication> rows) =>
            rows.Select(x => new HmdMedicationResponse
            {
                Id = x.Id,
                SessionId = x.SessionId,
                DrugId = x.DrugId,
                DrugName = x.Drug != null ? x.Drug.DrugName : null,
                Dose = x.Dose,
                DoseUnit = x.DoseUnit,
                Route = x.Route,
                InstructedByDoctorId = x.InstructedByDoctorId,
                InstructedByDoctorName = x.InstructedByDoctor != null ? x.InstructedByDoctor.FullName : null,
                AdministeredByUserId = x.AdministeredByUserId,
                AdministeredAt = x.AdministeredAt,
                DrugUsageId = x.DrugUsageId,
                PharmacySyncStatus = x.HandoffStatus,
                HandoffAttemptedAt = x.HandoffAttemptedAt,
                HandoffError = x.HandoffError,
                Note = x.Note
            });

        private static void LabelMedication(HmdMedicationResponse row)
        {
            row.RouteName = HmdLabels.MedicationRoute(row.Route);
            row.PharmacySyncStatusName = HmdLabels.PharmacyHandoff(row.PharmacySyncStatus);
        }

        private static IQueryable<HmdObservationResponse> ProjectObservation(IQueryable<HmdSessionObservation> rows) =>
            rows.Select(x => new HmdObservationResponse
            {
                Id = x.Id,
                SessionId = x.SessionId,
                SequenceNumber = x.SequenceNumber,
                ObservedAt = x.ObservedAt,
                RecordedByUserId = x.RecordedByUserId,
                RecordedAt = x.CreateDateTime,
                SystolicBp = x.SystolicBp,
                DiastolicBp = x.DiastolicBp,
                PulseRate = x.PulseRate,
                RespiratoryRate = x.RespiratoryRate,
                TemperatureC = x.TemperatureC,
                OxygenSaturation = x.OxygenSaturation,
                BloodFlowRate = x.BloodFlowRate,
                DialysateFlowRate = x.DialysateFlowRate,
                TransmembranePressure = x.TransmembranePressure,
                VenousPressure = x.VenousPressure,
                ArterialPressure = x.ArterialPressure,
                UltrafiltrationVolumeMl = x.UltrafiltrationVolumeMl,
                Note = x.Note
            });

        private static IQueryable<HmdComplicationResponse> ProjectComplication(IQueryable<HmdSessionComplication> rows) =>
            rows.Select(x => new HmdComplicationResponse
            {
                Id = x.Id,
                SessionId = x.SessionId,
                ComplicationType = x.ComplicationType,
                DetectedAt = x.DetectedAt,
                DetectedByUserId = x.DetectedByUserId,
                Severity = x.Severity,
                SignsAndSymptoms = x.SignsAndSymptoms,
                Intervention = x.Intervention,
                ClinicianInstruction = x.ClinicianInstruction,
                Outcome = x.Outcome,
                SessionImpact = x.SessionImpact,
                TransferDestination = x.TransferDestination
            });

        private static void LabelComplication(HmdComplicationResponse row)
        {
            row.ComplicationTypeName = HmdLabels.ComplicationType(row.ComplicationType);
            row.OutcomeName = HmdLabels.ComplicationOutcome(row.Outcome);
            row.SessionImpactName = HmdLabels.ComplicationImpact(row.SessionImpact);
        }

        public static List<string> AvailableActionsFor(HmdSessionStatus status, HmdBillingHandoffStatus billing) => status switch
        {
            HmdSessionStatus.Planned or HmdSessionStatus.Scheduled => ["CheckIn", "Reschedule", "AssignStaff", "Cancel"],
            HmdSessionStatus.CheckedIn => ["SaveChecklist", "SavePreHd", "OverrideChecklist", "Reschedule", "AssignStaff", "Cancel"],
            HmdSessionStatus.PreCheck => ["SaveChecklist", "SavePreHd", "OverrideChecklist", "DeclareReady", "Hold", "Reschedule", "AssignStaff", "Cancel"],
            HmdSessionStatus.Held => ["Resume", "Reschedule", "AssignStaff", "Cancel"],
            HmdSessionStatus.Ready => ["Start", "Hold", "SaveChecklist", "SavePreHd", "Reschedule", "AssignStaff", "Cancel"],
            HmdSessionStatus.InProgress => ["RecordObservation", "AdministerMedication", "RecordComplication", "SavePostHd", "Complete", "Stop", "AssignStaff"],
            HmdSessionStatus.Completed or HmdSessionStatus.Stopped => ["SavePostHd", "AdministerMedication", "RecordComplication", "SubmitDocumentation", "AssignStaff"],
            HmdSessionStatus.AwaitingFinalization => ["Finalize", "ReturnForCompletion"],
            HmdSessionStatus.Finalized => billing is HmdBillingHandoffStatus.Failed or HmdBillingHandoffStatus.Pending
                ? ["RetryBillingHandoff", "Addendum"]
                : ["Addendum"],
            _ => []
        };

        private static void MoveToPreCheck(HmdSession session)
        {
            // Mulai mengisi Pra-HD memindahkan CheckedIn ke PreCheck. Mengubah isian Pra-HD pada sesi
            // yang sudah siap menariknya kembali ke PreCheck supaya kesiapannya dinyatakan ulang.
            if (session.SessionStatus is HmdSessionStatus.CheckedIn or HmdSessionStatus.Ready)
            {
                session.SessionStatus = HmdSessionStatus.PreCheck;
                session.ReadyAt = null;
                session.ReadyByUserId = null;
            }
        }

        private static int? DurationMinutes(DateTime? startedAt, DateTime endedAt) =>
            startedAt.HasValue ? (int)Math.Round((endedAt - startedAt.Value).TotalMinutes) : null;

        private static void Touch(HmdSession session, Guid actorUserId, DateTime now)
        {
            session.UpdateDateTime = now;
            session.UpdateBy = actorUserId;
            session.Version++;
        }

        private static void CopyBase(HmdSessionResponse source, HmdSessionResponse target)
        {
            foreach (var property in typeof(HmdSessionResponse).GetProperties().Where(p => p.CanRead && p.CanWrite))
                property.SetValue(target, property.GetValue(source));
        }

        private async Task<HmdSessionResponse?> ReadSessionAsync(Guid id, CancellationToken cancellationToken)
        {
            var row = await HmdSessionProjection.Project(_dbContext.HmdSessions.AsNoTracking().Where(x => x.Id == id))
                .FirstOrDefaultAsync(cancellationToken);
            return row == null ? null : HmdSessionProjection.Label(row);
        }

        private async Task<HmdResult<HmdSessionResponse>> SaveAndReturnAsync(Guid id, CancellationToken cancellationToken)
        {
            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdSessionResponse>.From(failure);

            return HmdResult<HmdSessionResponse>.Ok((await ReadSessionAsync(id, cancellationToken))!);
        }

        private static HmdResult<T> NotFound<T>() => HmdResult<T>.NotFound(SessionNotFoundMessage);

        private static HmdResult<T> InvalidState<T>(HmdSessionStatus current) =>
            HmdResult<T>.Rule(
                HmdErrorCodes.InvalidTransition,
                $"Tindakan ini tidak dapat dilakukan pada sesi berstatus {HmdLabels.SessionStatus(current).ToLowerInvariant()}.");

        private async Task<HmdResult<bool>?> TrySaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<bool>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }
            catch (DbUpdateException exception) when (HmdServiceSupport.IsUniqueViolation(exception))
            {
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<bool>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }
        }
    }
}
