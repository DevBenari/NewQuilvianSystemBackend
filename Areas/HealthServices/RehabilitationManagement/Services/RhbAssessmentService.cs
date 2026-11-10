using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;
using R = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services.RhbResult<QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs.RehabAssessmentDetailResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Asesmen medis awal (Sp.KFR) dan asesmen awal profesi — <c>BE-RHB-009</c>, FR-RHB-011..016,
    /// <c>state-transition-matrix.md</c> §2–3, <c>validation-matrix.md</c> V-ASM-01..05 dan V-MEA-01.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Keutuhan dokumen</b> dipegang Rekam Medis lewat <see cref="RhbDocumentIntegrityGateway"/>
    /// (jenis <c>RehabAssessment</c> = 47): draft didaftarkan saat dibuat, setiap ubah diperiksa
    /// <c>EnsureMutableAsync</c>, tanda tangan mengunci dokumen, dan koreksi sesudah final hanya lewat
    /// addendum RM. Dokumen final dijawab 409 <c>RHB-VAL-021</c>.
    /// </para>
    /// <para>
    /// <b>Resep disiplin.</b> Isian <c>prescribedDisciplines</c> draft disimpan sebagai baris
    /// <see cref="RhbEpisodeDiscipline"/> yang tertaut ke asesmen draft itu. Baris hanya berlaku
    /// sebagai resep setelah asesmennya final; pembaca lain menyaringnya lewat
    /// <c>PrescribedByAssessment.RecordStatus == Final</c>. Episode hanya boleh punya satu asesmen
    /// medis yang belum dibatalkan, sehingga indeks unik <c>(EpisodeId, DisciplineId)</c> aman.
    /// Draft yang dibatalkan menghapus lunak resepnya.
    /// </para>
    /// <para>
    /// <b>Kewenangan atas data.</b> Ubah, tanda tangan, dan batal hanya oleh penulis
    /// (<c>RHB-VAL-022</c>). Asesmen profesi hanya oleh pemegang profesi disiplinnya
    /// (<c>RHB-VAL-031</c>, RHB-INV-08). Hak akses fitur tetap dari butir Akses Role.
    /// </para>
    /// </remarks>
    public class RhbAssessmentService
    {
        private static readonly TimeSpan ClockSkewTolerance = TimeSpan.FromMinutes(2);

        private readonly ApplicationDbContext _dbContext;
        private readonly RhbDocumentIntegrityGateway _integrity;
        private readonly RhbProfessionAuthority _professionAuthority;
        private readonly RhbMeasurementRules _measurementRules;

        public RhbAssessmentService(
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

        public static string TypeName(RehabAssessmentType type) => type switch
        {
            RehabAssessmentType.MedicalInitial => "Asesmen medis",
            RehabAssessmentType.DisciplineInitial => "Asesmen profesi",
            _ => type.ToString()
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

        public async Task<RhbResult<List<RehabAssessmentListItem>>> ListByEpisodeAsync(
            Guid episodeId,
            RehabAssessmentType type,
            CancellationToken cancellationToken)
        {
            if (!await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken))
                return RhbResult<List<RehabAssessmentListItem>>.NotFound("Episode rehab tidak ditemukan.");

            var rows = await Project(_dbContext.RhbAssessments.AsNoTracking()
                    .Where(x => x.EpisodeId == episodeId && x.AssessmentType == type && !x.IsDelete))
                .OrderByDescending(x => x.AssessedAt)
                .ToListAsync(cancellationToken);

            await FillLabelsAsync(rows, cancellationToken);
            return RhbResult<List<RehabAssessmentListItem>>.Ok(rows);
        }

        public async Task<R> GetDetailAsync(Guid id, RehabAssessmentType type, Guid actorUserId, CancellationToken cancellationToken)
        {
            var detail = await LoadDetailAsync(id, type, actorUserId, cancellationToken);
            return detail == null ? NotFound(type) : R.Ok(detail);
        }

        // =====================================================================
        // Draft
        // =====================================================================

        public async Task<R> CreateAsync(
            Guid episodeId,
            RehabAssessmentType type,
            SaveRehabAssessmentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var episode = await _dbContext.RhbEpisodes.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
            if (episode == null)
                return R.NotFound("Episode rehab tidak ditemukan.");
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return EpisodeClosed();

            Guid? disciplineId = null;
            if (type == RehabAssessmentType.DisciplineInitial)
            {
                if (!request.DisciplineId.HasValue || request.DisciplineId.Value == Guid.Empty)
                    return R.Invalid(RhbErrorCodes.DisciplineRequired, "Pilih layanan rehab.").WithFields("disciplineId");

                disciplineId = request.DisciplineId.Value;
                if (!await IsPrescribedAsync(episodeId, disciplineId.Value, cancellationToken))
                    return R.Rule(RhbErrorCodes.DisciplineNotPrescribed, "Layanan ini belum diresepkan dokter rehab untuk episode ini.").WithFields("disciplineId");

                var profession = await CheckProfessionAsync(actorUserId, disciplineId.Value, cancellationToken);
                if (profession != null)
                    return profession;
            }

            if (!request.EncounterId.HasValue)
                return EncounterInvalid("Pilih kunjungan tempat asesmen dilakukan.");

            var encounter = await RhbServiceSupport.CheckEncounterAsync(_dbContext, request.EncounterId.Value, episode.PatientId, cancellationToken);
            if (!encounter.IsValid)
                return EncounterInvalid(encounter.Exists && !encounter.BelongsToPatient
                    ? "Kunjungan bukan milik pasien episode ini."
                    : "Kunjungan tidak ditemukan, sudah berakhir, atau pasien sudah pulang.");

            var now = DateTime.UtcNow;
            var values = await ValidateValuesAsync(request, type, disciplineId, episode.PatientId, now, cancellationToken);
            if (values.Error != null)
                return values.Error;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (type == RehabAssessmentType.MedicalInitial)
            {
                // Dua dokter yang membuat asesmen medis bersamaan untuk episode yang sama mengantre di
                // sini, sehingga yang kedua melihat draft yang pertama.
                await RhbServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, $"RHB_MEDICAL_ASSESSMENT:{episodeId}");

                var existing = await _dbContext.RhbAssessments.AsNoTracking()
                    .Where(x => x.EpisodeId == episodeId && x.AssessmentType == RehabAssessmentType.MedicalInitial &&
                                !x.IsDelete && x.RecordStatus != RehabRecordStatus.Cancelled)
                    .Select(x => x.RecordStatus)
                    .ToListAsync(cancellationToken);

                if (existing.Contains(RehabRecordStatus.Final))
                    return R.Conflict(RhbErrorCodes.MedicalAssessmentAlreadyFinal, "Asesmen medis awal episode ini sudah final. Gunakan reasesmen.");
                if (existing.Contains(RehabRecordStatus.Draft))
                    return R.Conflict(RhbErrorCodes.InvalidState, "Episode ini sudah punya draft asesmen medis. Lanjutkan draft tersebut.");
            }

            var entity = new RhbAssessment
            {
                EpisodeId = episodeId,
                AssessmentType = type,
                DisciplineId = disciplineId,
                EncounterId = request.EncounterId.Value,
                InpEpisodeId = await InpatientEpisodeIdAsync(request.EncounterId.Value, encounter.EncounterType, cancellationToken),
                AuthorUserId = actorUserId,
                RecordStatus = RehabRecordStatus.Draft,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            ApplyValues(entity, values.Values!);
            _dbContext.RhbAssessments.Add(entity);
            AddMeasurements(entity, values.Values!.Measurements, actorUserId, now);
            AddPrescriptions(entity, values.Values!.Prescriptions, actorUserId, now);

            await _integrity.RegisterDraftAsync(
                ClinicalDocumentKind.RehabAssessment, entity.Id, episode.PatientId, entity.EncounterId, actorUserId, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            await transaction.CommitAsync(cancellationToken);
            return R.Created((await LoadDetailAsync(entity.Id, type, actorUserId, cancellationToken))!);
        }

        public async Task<R> UpdateAsync(
            Guid id,
            RehabAssessmentType type,
            SaveRehabAssessmentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var (entity, episode, guard) = await LoadForChangeAsync(id, type, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            if (request.EncounterId.HasValue && request.EncounterId.Value != entity!.EncounterId)
                return EncounterInvalid("Kunjungan asesmen tidak dapat diganti. Batalkan draft ini lalu buat yang baru.");
            if (type == RehabAssessmentType.DisciplineInitial && request.DisciplineId.HasValue && request.DisciplineId.Value != entity!.DisciplineId)
                return R.Invalid(RhbErrorCodes.DisciplineRequired, "Layanan asesmen profesi tidak dapat diganti.").WithFields("disciplineId");

            var now = DateTime.UtcNow;
            var values = await ValidateValuesAsync(request, type, entity!.DisciplineId, episode!.PatientId, now, cancellationToken);
            if (values.Error != null)
                return values.Error;

            ApplyValues(entity, values.Values!);
            Touch(entity, actorUserId, now);

            // Daftar pengukuran dan resep diganti utuh: baris lama dihapus lunak, baris baru ditambah.
            var oldMeasurements = await _dbContext.RhbMeasurements
                .Where(x => x.AssessmentId == entity.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var row in oldMeasurements)
                SoftDelete(row, actorUserId, now);
            AddMeasurements(entity, values.Values!.Measurements, actorUserId, now);

            if (type == RehabAssessmentType.MedicalInitial)
            {
                var oldPrescriptions = await _dbContext.RhbEpisodeDisciplines
                    .Where(x => x.PrescribedByAssessmentId == entity.Id && !x.IsDelete)
                    .ToListAsync(cancellationToken);
                foreach (var row in oldPrescriptions)
                    SoftDelete(row, actorUserId, now);

                // Baris lama harus terhapus dulu sebelum baris baru disisipkan, karena keduanya
                // berbagi indeks unik (EpisodeId, DisciplineId).
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                var first = await TrySaveAsync(cancellationToken);
                if (first != null)
                    return first;

                AddPrescriptions(entity, values.Values!.Prescriptions, actorUserId, now);
                var second = await TrySaveAsync(cancellationToken);
                if (second != null)
                    return second;

                await transaction.CommitAsync(cancellationToken);
            }
            else
            {
                var failure = await TrySaveAsync(cancellationToken);
                if (failure != null)
                    return failure;
            }

            return R.Ok((await LoadDetailAsync(entity.Id, type, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Tanda tangan dan batal
        // =====================================================================

        public async Task<R> SignAsync(
            Guid id,
            RehabAssessmentType type,
            RehabSignRequest request,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            var (entity, episode, guard) = await LoadForChangeAsync(id, type, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(entity!.HistoryText)) missing.Add("historyText");
            if (string.IsNullOrWhiteSpace(entity.PhysicalFunctionalFindingsText)) missing.Add("physicalFunctionalFindingsText");
            if (string.IsNullOrWhiteSpace(entity.ProblemOrDiagnosisText)) missing.Add("problemOrDiagnosisText");
            if (string.IsNullOrWhiteSpace(entity.ConclusionText)) missing.Add("conclusionText");
            if (missing.Count > 0)
                return R.Rule(RhbErrorCodes.AssessmentIncomplete, "Lengkapi riwayat, temuan, masalah/diagnosis, dan kesimpulan.")
                    .WithFields(missing.ToArray());

            List<RhbEpisodeDiscipline> prescriptions = new();
            if (type == RehabAssessmentType.MedicalInitial)
            {
                prescriptions = await _dbContext.RhbEpisodeDisciplines
                    .Where(x => x.PrescribedByAssessmentId == entity.Id && !x.IsDelete)
                    .ToListAsync(cancellationToken);
                if (prescriptions.Count == 0)
                    return R.Rule(RhbErrorCodes.NoDisciplinePrescribed, "Resepkan minimal satu layanan terapi sebelum menandatangani.")
                        .WithFields("prescribedDisciplines");

                var activeIds = await _dbContext.RhbDisciplines.AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);
                if (prescriptions.Any(x => !activeIds.Contains(x.DisciplineId)))
                    return R.Rule(RhbErrorCodes.DisciplineNotPrescribed, "Salah satu layanan yang diresepkan sudah tidak aktif. Perbarui resep.")
                        .WithFields("prescribedDisciplines");
            }

            var measurementError = await ValidateStoredInstrumentsAsync(entity.Id, cancellationToken);
            if (measurementError != null)
                return measurementError;

            var now = DateTime.UtcNow;

            // Tanda tangan RM memanggil SaveChanges sendiri. Transaksi ini memastikan status asesmen,
            // perpindahan episode, riwayatnya, dan baris keutuhan tersimpan bersama atau tidak sama
            // sekali (aturan transaksi gateway).
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            entity.RecordStatus = RehabRecordStatus.Final;
            entity.SignedAt = now;
            Touch(entity, actorUserId, now);

            if (type == RehabAssessmentType.MedicalInitial)
            {
                var trackedEpisode = await _dbContext.RhbEpisodes.FirstAsync(x => x.Id == entity.EpisodeId, cancellationToken);
                if (trackedEpisode.EpisodeStatus == RehabEpisodeStatus.AwaitingMedicalAssessment)
                {
                    trackedEpisode.EpisodeStatus = RehabEpisodeStatus.Planning;
                    trackedEpisode.UpdateBy = actorUserId;
                    trackedEpisode.UpdateDateTime = now;
                    trackedEpisode.Version++;
                    RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Episode, trackedEpisode.Id,
                        RehabEpisodeStatus.AwaitingMedicalAssessment.ToString(), RehabEpisodeStatus.Planning.ToString(),
                        "sign-medical-assessment", null, actorUserId, now);
                }
            }

            try
            {
                var (result, _) = await _integrity.SignAsync(
                    ClinicalDocumentKind.RehabAssessment, entity.Id, actorUserId, deviceInfo, ipAddress, now, cancellationToken);
                if (!result.IsAllowed)
                {
                    DetachPending();
                    return result.StatusCode == StatusCodes.Status403Forbidden
                        ? R.Forbidden(RhbErrorCodes.NotAuthor, result.ErrorMessage ?? "Hanya penulis yang dapat menandatangani.")
                        : R.Conflict(RhbErrorCodes.DocumentLocked, result.ErrorMessage ?? "Catatan sudah final, gunakan addendum.");
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
                return R.Conflict(RhbErrorCodes.MedicalAssessmentAlreadyFinal, "Asesmen medis awal episode ini sudah final. Gunakan reasesmen.");
            }

            await transaction.CommitAsync(cancellationToken);
            return R.Ok((await LoadDetailAsync(entity.Id, type, actorUserId, cancellationToken))!);
        }

        public async Task<R> CancelAsync(
            Guid id,
            RehabAssessmentType type,
            RehabReasonRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var (entity, _, guard) = await LoadForChangeAsync(id, type, request.ExpectedVersion, actorUserId, cancellationToken);
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

            var prescriptions = await _dbContext.RhbEpisodeDisciplines
                .Where(x => x.PrescribedByAssessmentId == entity.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var row in prescriptions)
                SoftDelete(row, actorUserId, now);

            await _integrity.MarkCancelledAsync(ClinicalDocumentKind.RehabAssessment, entity.Id, actorUserId, reason, now, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            return R.Ok((await LoadDetailAsync(entity.Id, type, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Validasi isian
        // =====================================================================

        private sealed record AssessmentValues(
            DateTime AssessedAt,
            string? HistoryText,
            string? PhysicalFunctionalFindingsText,
            string? RiskAndContraindicationText,
            string? ProblemOrDiagnosisText,
            string? PrognosisText,
            string? ConclusionText,
            string? PatientGoalText,
            List<RhbMeasurementValue> Measurements,
            List<(Guid DisciplineId, string? Precaution)> Prescriptions);

        private async Task<(R? Error, AssessmentValues? Values)> ValidateValuesAsync(
            SaveRehabAssessmentRequest request,
            RehabAssessmentType type,
            Guid? disciplineId,
            Guid patientId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var assessedAt = request.AssessedAt.HasValue ? RhbServiceSupport.ToUtc(request.AssessedAt.Value) : nowUtc;
            if (assessedAt > nowUtc + ClockSkewTolerance)
                return (R.Invalid(RhbErrorCodes.AssessmentIncomplete, "Waktu asesmen tidak boleh di masa depan.").WithFields("assessedAt"), null);

            var texts = new (string Field, string? Value, int Max)[]
            {
                ("historyText", request.HistoryText, 4000),
                ("physicalFunctionalFindingsText", request.PhysicalFunctionalFindingsText, 4000),
                ("riskAndContraindicationText", request.RiskAndContraindicationText, 2000),
                ("problemOrDiagnosisText", request.ProblemOrDiagnosisText, 2000),
                ("prognosisText", request.PrognosisText, 1000),
                ("conclusionText", request.ConclusionText, 2000),
                ("patientGoalText", request.PatientGoalText, 1000)
            };
            var normalized = new Dictionary<string, string?>();
            foreach (var (field, value, max) in texts)
            {
                var text = RhbServiceSupport.Normalize(value);
                if (text?.Length > max)
                    return (TooLong(field, max), null);
                normalized[field] = text;
            }

            var measurementInput = request.Measurements ?? new();
            if (measurementInput.Count > RhbMeasurementRules.MaxMeasurements)
                return (R.Invalid(RhbErrorCodes.MeasurementInvalid, $"Pengukuran maksimal {RhbMeasurementRules.MaxMeasurements} baris.").WithFields("measurements"), null);

            var measurements = new List<RhbMeasurementValue>();
            for (var index = 0; index < measurementInput.Count; index++)
            {
                var (error, value) = await ValidateMeasurementAsync(measurementInput[index], index, disciplineId, patientId, nowUtc, cancellationToken);
                if (error != null)
                    return (error, null);
                measurements.Add(value!);
            }

            var prescriptions = new List<(Guid, string?)>();
            if (type == RehabAssessmentType.MedicalInitial)
            {
                var input = request.PrescribedDisciplines ?? new();
                if (input.Select(x => x.DisciplineId).Distinct().Count() != input.Count)
                    return (R.Invalid(RhbErrorCodes.DisciplineRequired, "Layanan yang diresepkan tidak boleh berulang.").WithFields("prescribedDisciplines"), null);

                var ids = input.Select(x => x.DisciplineId).ToList();
                var activeCount = await _dbContext.RhbDisciplines.AsNoTracking()
                    .CountAsync(x => ids.Contains(x.Id) && !x.IsDelete && x.IsActive, cancellationToken);
                if (activeCount != ids.Count)
                    return (R.Invalid(RhbErrorCodes.DisciplineRequired, "Layanan yang diresepkan tidak ditemukan atau tidak aktif.").WithFields("prescribedDisciplines"), null);

                foreach (var item in input)
                {
                    var precaution = RhbServiceSupport.Normalize(item.MedicalPrecautionText);
                    if (precaution?.Length > 1000)
                        return (TooLong("prescribedDisciplines.medicalPrecautionText", 1000), null);
                    prescriptions.Add((item.DisciplineId, precaution));
                }
            }

            return (null, new AssessmentValues(
                assessedAt,
                normalized["historyText"],
                normalized["physicalFunctionalFindingsText"],
                normalized["riskAndContraindicationText"],
                normalized["problemOrDiagnosisText"],
                normalized["prognosisText"],
                normalized["conclusionText"],
                normalized["patientGoalText"],
                measurements,
                prescriptions));
        }

        /// <summary>Satu pengukuran — aturan bersama <see cref="RhbMeasurementRules"/> (V-MEA-01).</summary>
        private async Task<(R? Error, RhbMeasurementValue? Value)> ValidateMeasurementAsync(
            RehabMeasurementRequest input,
            int index,
            Guid? disciplineId,
            Guid patientId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var (failure, value) = await _measurementRules.ValidateAsync(input, index, disciplineId, nowUtc, cancellationToken);
            return failure != null ? (failure.ToResult<RehabAssessmentDetailResponse>(), null) : (null, value);
        }

        /// <summary>Saat tanda tangan, pengukuran berinstrumen harus lengkap dan versinya masih boleh dipakai (V-MEA-01).</summary>
        private async Task<R?> ValidateStoredInstrumentsAsync(Guid assessmentId, CancellationToken cancellationToken)
        {
            var failure = await _measurementRules.ValidateStoredInstrumentsAsync(
                _dbContext.RhbMeasurements.AsNoTracking().Where(m => m.AssessmentId == assessmentId), cancellationToken);
            return failure?.ToResult<RehabAssessmentDetailResponse>();
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Memuat asesmen untuk diubah dan menjalankan seluruh penjaga umum: jenis cocok, episode belum
        /// ditutup, versi, status draft, keutuhan RM, penulis, dan — untuk asesmen profesi — profesi.
        /// </summary>
        private async Task<(RhbAssessment? Entity, RhbEpisode? Episode, R? Error)> LoadForChangeAsync(
            Guid id,
            RehabAssessmentType type,
            int expectedVersion,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RhbAssessments
                .FirstOrDefaultAsync(x => x.Id == id && x.AssessmentType == type && !x.IsDelete, cancellationToken);
            if (entity == null)
                return (null, null, NotFound(type));

            var episode = await _dbContext.RhbEpisodes.AsNoTracking()
                .FirstAsync(x => x.Id == entity.EpisodeId, cancellationToken);
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return (null, null, EpisodeClosed());

            if (entity.RecordStatus == RehabRecordStatus.Final)
                return (null, null, R.Conflict(RhbErrorCodes.DocumentLocked, "Catatan sudah final, gunakan addendum."));
            if (entity.RecordStatus != RehabRecordStatus.Draft)
                return (null, null, R.Conflict(RhbErrorCodes.InvalidState, "Asesmen sudah dibatalkan."));

            if (entity.Version != expectedVersion)
                return (null, null, Stale().WithFields("expectedVersion"));

            if (entity.AuthorUserId != actorUserId)
                return (null, null, R.Forbidden(RhbErrorCodes.NotAuthor, "Hanya penulis asesmen yang dapat mengubah, menandatangani, atau membatalkannya."));

            // Draft yang terkunci karena kunjungannya ditutup (LockedUnsigned) tidak boleh berubah lagi.
            var mutable = await _integrity.EnsureMutableAsync(ClinicalDocumentKind.RehabAssessment, entity.Id, cancellationToken);
            if (!mutable.IsAllowed)
                return (null, null, R.Conflict(RhbErrorCodes.DocumentLocked, mutable.ErrorMessage ?? "Catatan sudah terkunci, gunakan addendum."));

            if (type == RehabAssessmentType.DisciplineInitial && entity.DisciplineId.HasValue)
            {
                var profession = await CheckProfessionAsync(actorUserId, entity.DisciplineId.Value, cancellationToken);
                if (profession != null)
                    return (null, null, profession);
            }

            return (entity, episode, null);
        }

        private async Task<R?> CheckProfessionAsync(Guid actorUserId, Guid disciplineId, CancellationToken cancellationToken) =>
            await _professionAuthority.CanActForDisciplineAsync(actorUserId, disciplineId, cancellationToken)
                ? null
                : R.Forbidden(RhbErrorCodes.ProfessionMismatch, "Profesi Anda tidak sesuai dengan layanan ini.");

        /// <summary>Diresepkan = baris resep yang asesmen medisnya sudah final.</summary>
        private Task<bool> IsPrescribedAsync(Guid episodeId, Guid disciplineId, CancellationToken cancellationToken) =>
            _dbContext.RhbEpisodeDisciplines.AsNoTracking().AnyAsync(x =>
                x.EpisodeId == episodeId && x.DisciplineId == disciplineId && !x.IsDelete &&
                x.PrescribedByAssessment!.RecordStatus == RehabRecordStatus.Final, cancellationToken);

        private async Task<Guid?> InpatientEpisodeIdAsync(Guid encounterId, EncounterType? encounterType, CancellationToken cancellationToken)
        {
            if (encounterType != EncounterType.Inpatient)
                return null;

            return await _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static void ApplyValues(RhbAssessment entity, AssessmentValues values)
        {
            entity.AssessedAt = values.AssessedAt;
            entity.HistoryText = values.HistoryText;
            entity.PhysicalFunctionalFindingsText = values.PhysicalFunctionalFindingsText;
            entity.RiskAndContraindicationText = values.RiskAndContraindicationText;
            entity.ProblemOrDiagnosisText = values.ProblemOrDiagnosisText;
            entity.PrognosisText = values.PrognosisText;
            entity.ConclusionText = values.ConclusionText;
            entity.PatientGoalText = values.PatientGoalText;
        }

        private void AddMeasurements(RhbAssessment entity, List<RhbMeasurementValue> measurements, Guid actorUserId, DateTime nowUtc)
        {
            foreach (var m in measurements)
                _dbContext.RhbMeasurements.Add(RhbMeasurementRules.ToEntity(m, entity.Id, null, actorUserId, nowUtc));
        }

        private void AddPrescriptions(RhbAssessment entity, List<(Guid DisciplineId, string? Precaution)> prescriptions, Guid actorUserId, DateTime nowUtc)
        {
            foreach (var (disciplineId, precaution) in prescriptions)
            {
                _dbContext.RhbEpisodeDisciplines.Add(new RhbEpisodeDiscipline
                {
                    EpisodeId = entity.EpisodeId,
                    DisciplineId = disciplineId,
                    PrescribedByAssessmentId = entity.Id,
                    MedicalPrecautionText = precaution,
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

        private static void Touch(RhbAssessment entity, Guid actorUserId, DateTime nowUtc)
        {
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = nowUtc;
            entity.Version++;
        }

        private static IQueryable<RehabAssessmentListItem> Project(IQueryable<RhbAssessment> rows) =>
            rows.Select(x => new RehabAssessmentListItem
            {
                Id = x.Id,
                EpisodeId = x.EpisodeId,
                AssessmentType = x.AssessmentType,
                DisciplineId = x.DisciplineId,
                DisciplineName = x.Discipline != null ? x.Discipline.DisciplineName : null,
                EncounterId = x.EncounterId,
                AssessedAt = x.AssessedAt,
                AuthorUserId = x.AuthorUserId,
                RecordStatus = x.RecordStatus,
                SignedAt = x.SignedAt,
                Version = x.Version
            });

        private async Task FillLabelsAsync(IReadOnlyCollection<RehabAssessmentListItem> rows, CancellationToken cancellationToken)
        {
            var authorIds = rows.Select(x => x.AuthorUserId).Distinct().ToList();
            var encounterIds = rows.Select(x => x.EncounterId).Distinct().ToList();
            var authors = await _dbContext.Users.AsNoTracking()
                .Where(u => authorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);
            var encounters = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Where(e => encounterIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => e.EncounterNumber, cancellationToken);

            foreach (var row in rows)
            {
                row.AssessmentTypeName = TypeName(row.AssessmentType);
                row.RecordStatusName = RhbEpisodeService.RecordStatusName(row.RecordStatus);
                row.AuthorName = authors.TryGetValue(row.AuthorUserId, out var name) ? name : null;
                row.EncounterNumber = encounters.TryGetValue(row.EncounterId, out var number) ? number : null;
            }
        }

        private async Task<RehabAssessmentDetailResponse?> LoadDetailAsync(Guid id, RehabAssessmentType type, Guid actorUserId, CancellationToken cancellationToken)
        {
            var row = await _dbContext.RhbAssessments.AsNoTracking()
                .Where(x => x.Id == id && x.AssessmentType == type && !x.IsDelete)
                .Select(x => new RehabAssessmentDetailResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    EpisodeNumber = x.Episode!.EpisodeNumber,
                    PatientId = x.Episode.PatientId,
                    AssessmentType = x.AssessmentType,
                    DisciplineId = x.DisciplineId,
                    DisciplineName = x.Discipline != null ? x.Discipline.DisciplineName : null,
                    EncounterId = x.EncounterId,
                    InpEpisodeId = x.InpEpisodeId,
                    AssessedAt = x.AssessedAt,
                    AuthorUserId = x.AuthorUserId,
                    RecordStatus = x.RecordStatus,
                    SignedAt = x.SignedAt,
                    Version = x.Version,
                    HistoryText = x.HistoryText,
                    PhysicalFunctionalFindingsText = x.PhysicalFunctionalFindingsText,
                    RiskAndContraindicationText = x.RiskAndContraindicationText,
                    ProblemOrDiagnosisText = x.ProblemOrDiagnosisText,
                    PrognosisText = x.PrognosisText,
                    ConclusionText = x.ConclusionText,
                    PatientGoalText = x.PatientGoalText
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (row == null)
                return null;

            await FillLabelsAsync([row], cancellationToken);

            var measurements = await _dbContext.RhbMeasurements.AsNoTracking()
                .Where(x => x.AssessmentId == id && !x.IsDelete)
                .OrderBy(x => x.MeasuredAt)
                .ToListAsync(cancellationToken);
            row.Measurements = measurements.Select(m => new RehabMeasurementItem
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

            row.PrescribedDisciplines = await _dbContext.RhbEpisodeDisciplines.AsNoTracking()
                .Where(x => x.PrescribedByAssessmentId == id && !x.IsDelete)
                .OrderBy(x => x.Discipline!.DisciplineName)
                .Select(x => new RehabPrescribedDisciplineItem
                {
                    DisciplineId = x.DisciplineId,
                    DisciplineCode = x.Discipline!.DisciplineCode,
                    DisciplineName = x.Discipline.DisciplineName,
                    MedicalPrecautionText = x.MedicalPrecautionText
                })
                .ToListAsync(cancellationToken);

            var integrity = await _integrity.FindAsync(ClinicalDocumentKind.RehabAssessment, id, cancellationToken);
            row.IntegrityStatus = integrity?.IntegrityStatus;
            row.IntegrityStatusName = integrity != null ? IntegrityStatusName(integrity.IntegrityStatus) : null;

            var mutableDraft = row.RecordStatus == RehabRecordStatus.Draft &&
                               integrity?.IntegrityStatus is null or ClinicalDocumentIntegrityStatus.Draft;
            row.AvailableActions = mutableDraft && row.AuthorUserId == actorUserId
                ? ["update", "sign", "cancel"]
                : row.RecordStatus == RehabRecordStatus.Final || integrity?.IntegrityStatus == ClinicalDocumentIntegrityStatus.LockedUnsigned
                    ? ["addendum"]
                    : [];

            return row;
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
                return R.Conflict(RhbErrorCodes.InvalidState, "Asesmen ini sudah diproses pengguna lain. Muat ulang data.");
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

        private static R NotFound(RehabAssessmentType type) => R.NotFound($"{TypeName(type)} tidak ditemukan.");

        private static R Stale() => R.Conflict(RhbErrorCodes.StaleVersion, "Data sudah diubah pengguna lain. Muat ulang sebelum menyimpan.");

        private static R EpisodeClosed() => R.Conflict(RhbErrorCodes.EpisodeClosed, "Episode rehab sudah ditutup.");

        private static R EncounterInvalid(string message) =>
            R.Rule(RhbErrorCodes.ClinicalEncounterInvalid, message).WithFields("encounterId");

        private static R TooLong(string field, int max) =>
            R.Invalid(RhbErrorCodes.FieldTooLong, $"Isian terlalu panjang (maksimal {max} karakter).").WithFields(field);
    }
}
