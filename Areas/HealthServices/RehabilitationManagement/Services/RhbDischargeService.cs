using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using R = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services.RhbResult<QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs.RehabDischargeSummaryResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Ringkasan akhir dan penutupan episode — <c>BE-RHB-015</c>, FR-RHB-050..052, FEAT-026..029,
    /// <c>state-transition-matrix.md</c> §2–4, V-DIS-01/02.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satu ringkasan per episode (indeks unik <c>EpisodeId</c>), dokumen RM jenis 51. Tanda tangan adalah satu-
    /// satunya jalan menutup episode: dalam satu transaksi ringkasan menjadi <c>Final</c>, episode <c>Closed</c>
    /// beserta alasannya, program yang masih <c>Active</c> menjadi <c>Completed</c>, riwayat dicatat, lalu RM
    /// mengunci dokumen.
    /// </para>
    /// <para>
    /// <b>V-DIS-02.</b> Selama masih ada catatan episode berstatus draft — asesmen, edukasi, program draft, sesi
    /// yang belum ditandatangani (termasuk yang berlangsung), atau reasesmen — tanda tangan dijawab 409
    /// <c>RHB-VAL-060</c> beserta daftarnya.
    /// </para>
    /// <para>
    /// Penutupan dari <c>AwaitingMedicalAssessment</c> hanya sah untuk alasan meninggal, menolak, pindah, atau
    /// putus layanan (<c>state-transition-matrix.md</c> §2).
    /// </para>
    /// </remarks>
    public class RhbDischargeService
    {
        private static readonly RehabEpisodeClosureReason[] ClosableBeforeAssessment =
        [
            RehabEpisodeClosureReason.Deceased,
            RehabEpisodeClosureReason.PatientRefused,
            RehabEpisodeClosureReason.Transferred,
            RehabEpisodeClosureReason.LostToFollowUp
        ];

        private readonly ApplicationDbContext _dbContext;
        private readonly RhbDocumentIntegrityGateway _integrity;

        public RhbDischargeService(ApplicationDbContext dbContext, RhbDocumentIntegrityGateway integrity)
        {
            _dbContext = dbContext;
            _integrity = integrity;
        }

        public static string ClosureReasonName(RehabEpisodeClosureReason reason) => reason switch
        {
            RehabEpisodeClosureReason.ClinicalCompletion => "Selesai secara klinis",
            RehabEpisodeClosureReason.Discontinued => "Dihentikan",
            RehabEpisodeClosureReason.Referred => "Dirujuk",
            RehabEpisodeClosureReason.Transferred => "Pindah fasilitas",
            RehabEpisodeClosureReason.LostToFollowUp => "Putus layanan",
            RehabEpisodeClosureReason.Deceased => "Meninggal",
            RehabEpisodeClosureReason.PatientRefused => "Pasien menolak melanjutkan",
            _ => reason.ToString()
        };

        public static string FollowUpName(RehabFollowUpType type) => type switch
        {
            RehabFollowUpType.None => "Tanpa tindak lanjut",
            RehabFollowUpType.Control => "Kontrol",
            RehabFollowUpType.HomeProgram => "Program rumah",
            RehabFollowUpType.ReferBack => "Rujuk balik",
            RehabFollowUpType.ReferOther => "Rujuk ke fasilitas lain",
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

        public async Task<R> GetAsync(Guid episodeId, Guid actorUserId, CancellationToken cancellationToken)
        {
            var response = await LoadAsync(episodeId, actorUserId, cancellationToken);
            return response == null ? R.NotFound("Episode rehab tidak ditemukan.") : R.Ok(response);
        }

        // =====================================================================
        // Simpan draft (buat bila belum ada)
        // =====================================================================

        public async Task<R> SaveAsync(Guid episodeId, SaveRehabDischargeSummaryRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var episode = await _dbContext.RhbEpisodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
            if (episode == null)
                return R.NotFound("Episode rehab tidak ditemukan.");
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return EpisodeClosed();

            var (error, values) = Validate(request);
            if (error != null)
                return error;

            var now = DateTime.UtcNow;
            var entity = await _dbContext.RhbDischargeSummaries.FirstOrDefaultAsync(x => x.EpisodeId == episodeId && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                if (!request.EncounterId.HasValue)
                    return EncounterInvalid("Pilih kunjungan tempat ringkasan akhir disusun.");
                var encounter = await RhbServiceSupport.CheckEncounterAsync(_dbContext, request.EncounterId.Value, episode.PatientId, cancellationToken);
                if (!encounter.IsValid)
                    return EncounterInvalid(encounter.Exists && !encounter.BelongsToPatient
                        ? "Kunjungan bukan milik pasien episode ini."
                        : "Kunjungan tidak ditemukan, sudah berakhir, atau pasien sudah pulang.");

                entity = new RhbDischargeSummary
                {
                    EpisodeId = episodeId,
                    EncounterId = request.EncounterId.Value,
                    AuthorUserId = actorUserId,
                    RecordStatus = RehabRecordStatus.Draft,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                Apply(entity, values!);

                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                _dbContext.RhbDischargeSummaries.Add(entity);
                await _integrity.RegisterDraftAsync(ClinicalDocumentKind.RehabDischargeSummary, entity.Id, episode.PatientId, entity.EncounterId, actorUserId, cancellationToken);
                var created = await TrySaveAsync(cancellationToken);
                if (created != null)
                    return created;
                await transaction.CommitAsync(cancellationToken);
                return R.Created((await LoadAsync(episodeId, actorUserId, cancellationToken))!);
            }

            var guard = await GuardDraftAsync(entity, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;
            if (request.EncounterId.HasValue && request.EncounterId.Value != entity.EncounterId)
                return EncounterInvalid("Kunjungan ringkasan akhir tidak dapat diganti.");

            Apply(entity, values!);
            Touch(entity, actorUserId, now);
            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(episodeId, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Tanda tangan → episode Closed
        // =====================================================================

        public async Task<R> SignAsync(
            Guid episodeId,
            RehabSignRequest request,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            var episode = await _dbContext.RhbEpisodes.FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
            if (episode == null)
                return R.NotFound("Episode rehab tidak ditemukan.");
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return EpisodeClosed();

            var entity = await _dbContext.RhbDischargeSummaries.FirstOrDefaultAsync(x => x.EpisodeId == episodeId && !x.IsDelete, cancellationToken);
            if (entity == null)
                return R.Rule(RhbErrorCodes.DischargeIncomplete, "Simpan ringkasan akhir lebih dulu.");

            var guard = await GuardDraftAsync(entity, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            // V-DIS-01
            var missing = new List<string>();
            if (!entity.ClosureReason.HasValue) missing.Add("closureReason");
            if (string.IsNullOrWhiteSpace(entity.MainServicesText)) missing.Add("mainServicesText");
            if (string.IsNullOrWhiteSpace(entity.GoalAchievementText)) missing.Add("goalAchievementText");
            if (string.IsNullOrWhiteSpace(entity.FinalConditionText)) missing.Add("finalConditionText");
            if (entity.FollowUpType != RehabFollowUpType.None && string.IsNullOrWhiteSpace(entity.FollowUpInstructionText)) missing.Add("followUpInstructionText");
            if (entity.FollowUpType is RehabFollowUpType.ReferBack or RehabFollowUpType.ReferOther && string.IsNullOrWhiteSpace(entity.FollowUpTargetText)) missing.Add("followUpTargetText");
            if (missing.Count > 0)
                return R.Rule(RhbErrorCodes.DischargeIncomplete, "Lengkapi ringkasan dan tindak lanjut.").WithFields(missing.ToArray());

            if (episode.EpisodeStatus == RehabEpisodeStatus.AwaitingMedicalAssessment && !ClosableBeforeAssessment.Contains(entity.ClosureReason!.Value))
                return R.Rule(RhbErrorCodes.DischargeIncomplete,
                    "Episode yang belum diasesmen hanya dapat ditutup karena pasien meninggal, menolak, pindah, atau putus layanan.").WithFields("closureReason");

            // V-DIS-02
            var pending = await PendingDocumentsAsync(episodeId, cancellationToken);
            if (pending.Count > 0)
                return R.Conflict(RhbErrorCodes.EpisodeNotReadyToClose, "Masih ada catatan yang belum final. Selesaikan atau batalkan dulu.", pending);

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            entity.RecordStatus = RehabRecordStatus.Final;
            entity.SignedAt = now;
            Touch(entity, actorUserId, now);

            var fromStatus = episode.EpisodeStatus;
            episode.EpisodeStatus = RehabEpisodeStatus.Closed;
            episode.ClosureReason = entity.ClosureReason;
            episode.ClosedAt = now;
            episode.ClosedByUserId = actorUserId;
            episode.UpdateBy = actorUserId;
            episode.UpdateDateTime = now;
            episode.Version++;
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Episode, episode.Id, fromStatus.ToString(),
                RehabEpisodeStatus.Closed.ToString(), "sign-discharge-summary", ClosureReasonName(entity.ClosureReason!.Value), actorUserId, now);

            var activePrograms = await _dbContext.RhbPrograms
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && x.ProgramStatus == RehabProgramStatus.Active)
                .ToListAsync(cancellationToken);
            foreach (var program in activePrograms)
            {
                program.ProgramStatus = RehabProgramStatus.Completed;
                program.UpdateBy = actorUserId;
                program.UpdateDateTime = now;
                program.Version++;
                RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Program, program.Id, RehabProgramStatus.Active.ToString(),
                    RehabProgramStatus.Completed.ToString(), "discharge", null, actorUserId, now);
            }

            try
            {
                var (result, _) = await _integrity.SignAsync(ClinicalDocumentKind.RehabDischargeSummary, entity.Id, actorUserId, deviceInfo, ipAddress, now, cancellationToken);
                if (!result.IsAllowed)
                {
                    DetachPending();
                    return result.StatusCode == StatusCodes.Status403Forbidden
                        ? R.Forbidden(RhbErrorCodes.NotAuthor, result.ErrorMessage ?? "Hanya penulis ringkasan akhir yang dapat menandatanganinya.")
                        : R.Conflict(RhbErrorCodes.DocumentLocked, result.ErrorMessage ?? "Catatan sudah final, gunakan addendum.");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                DetachPending();
                return Stale();
            }

            await transaction.CommitAsync(cancellationToken);
            return R.Ok((await LoadAsync(episodeId, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        private sealed record Values(
            RehabEpisodeClosureReason? ClosureReason,
            string? MainServices,
            string? GoalAchievement,
            string? FinalCondition,
            RehabFollowUpType FollowUpType,
            string? FollowUpInstruction,
            string? FollowUpTarget,
            DateOnly? FollowUpDueDate);

        private static (R? Error, Values? Values) Validate(SaveRehabDischargeSummaryRequest request)
        {
            if (request.ClosureReason.HasValue && !Enum.IsDefined(request.ClosureReason.Value))
                return (R.Invalid(RhbErrorCodes.DischargeIncomplete, "Alasan penutupan tidak dikenal.").WithFields("closureReason"), null);
            if (!Enum.IsDefined(request.FollowUpType))
                return (R.Invalid(RhbErrorCodes.DischargeIncomplete, "Jenis tindak lanjut tidak dikenal.").WithFields("followUpType"), null);

            var main = RhbServiceSupport.Normalize(request.MainServicesText);
            var goal = RhbServiceSupport.Normalize(request.GoalAchievementText);
            var condition = RhbServiceSupport.Normalize(request.FinalConditionText);
            var instruction = RhbServiceSupport.Normalize(request.FollowUpInstructionText);
            var target = RhbServiceSupport.Normalize(request.FollowUpTargetText);
            foreach (var (field, value, max) in new (string, string?, int)[]
                     {
                         ("mainServicesText", main, 2000), ("goalAchievementText", goal, 2000), ("finalConditionText", condition, 2000),
                         ("followUpInstructionText", instruction, 2000), ("followUpTargetText", target, 200)
                     })
            {
                if (value?.Length > max)
                    return (R.Invalid(RhbErrorCodes.FieldTooLong, $"Isian terlalu panjang (maksimal {max} karakter).").WithFields(field), null);
            }

            if (request.FollowUpDueDate.HasValue && request.FollowUpDueDate.Value < RhbServiceSupport.TodayLocal())
                return (R.Invalid(RhbErrorCodes.DischargeIncomplete, "Tanggal tindak lanjut tidak boleh lampau.").WithFields("followUpDueDate"), null);

            return (null, new Values(request.ClosureReason, main, goal, condition, request.FollowUpType, instruction, target, request.FollowUpDueDate));
        }

        private static void Apply(RhbDischargeSummary entity, Values values)
        {
            entity.ClosureReason = values.ClosureReason;
            entity.MainServicesText = values.MainServices;
            entity.GoalAchievementText = values.GoalAchievement;
            entity.FinalConditionText = values.FinalCondition;
            entity.FollowUpType = values.FollowUpType;
            entity.FollowUpInstructionText = values.FollowUpInstruction;
            entity.FollowUpTargetText = values.FollowUpTarget;
            entity.FollowUpDueDate = values.FollowUpDueDate;
        }

        /// <summary>Draft milik penulis: status, versi, penulis, keutuhan RM.</summary>
        private async Task<R?> GuardDraftAsync(RhbDischargeSummary entity, int expectedVersion, Guid actorUserId, CancellationToken cancellationToken)
        {
            if (entity.RecordStatus == RehabRecordStatus.Final)
                return R.Conflict(RhbErrorCodes.DocumentLocked, "Catatan sudah final, gunakan addendum.");
            if (entity.RecordStatus != RehabRecordStatus.Draft)
                return R.Conflict(RhbErrorCodes.InvalidState, "Ringkasan akhir sudah dibatalkan.");
            if (entity.Version != expectedVersion)
                return Stale().WithFields("expectedVersion");
            if (entity.AuthorUserId != actorUserId)
                return R.Forbidden(RhbErrorCodes.NotAuthor, "Hanya penulis ringkasan akhir yang dapat mengubah atau menandatanganinya.");

            var mutable = await _integrity.EnsureMutableAsync(ClinicalDocumentKind.RehabDischargeSummary, entity.Id, cancellationToken);
            return mutable.IsAllowed ? null : R.Conflict(RhbErrorCodes.DocumentLocked, mutable.ErrorMessage ?? "Catatan sudah terkunci, gunakan addendum.");
        }

        /// <summary>Catatan episode yang belum final (V-DIS-02).</summary>
        private async Task<List<RehabPendingDocument>> PendingDocumentsAsync(Guid episodeId, CancellationToken cancellationToken)
        {
            var result = new List<RehabPendingDocument>();

            var assessments = await _dbContext.RhbAssessments.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && x.RecordStatus == RehabRecordStatus.Draft)
                .Select(x => new { x.Id, x.AssessmentType, Discipline = x.Discipline != null ? x.Discipline.DisciplineName : null })
                .ToListAsync(cancellationToken);
            result.AddRange(assessments.Select(x => Pending(
                x.AssessmentType == RehabAssessmentType.MedicalInitial ? "MedicalAssessment" : "DisciplineAssessment",
                RhbAssessmentService.TypeName(x.AssessmentType), x.Id, x.Discipline, "Draf")));

            var educations = await _dbContext.RhbEducationRecords.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && x.RecordStatus == RehabRecordStatus.Draft)
                .Select(x => x.Id).ToListAsync(cancellationToken);
            result.AddRange(educations.Select(id => Pending("Education", "Edukasi dan persetujuan", id, null, "Draf")));

            var programs = await _dbContext.RhbPrograms.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && x.ProgramStatus == RehabProgramStatus.Draft)
                .Select(x => new { x.Id, x.ProgramVersion, x.Discipline!.DisciplineName })
                .ToListAsync(cancellationToken);
            result.AddRange(programs.Select(x => Pending("Program", "Program terapi", x.Id, $"{x.DisciplineName} v{x.ProgramVersion}", "Draf")));

            var sessions = await _dbContext.RhbSessions.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && x.RecordStatus == RehabRecordStatus.Draft)
                .Select(x => new { x.Id, x.SessionNumber, x.SessionStatus })
                .ToListAsync(cancellationToken);
            result.AddRange(sessions.Select(x => Pending("Session", "Sesi terapi", x.Id, x.SessionNumber,
                x.SessionStatus == RehabSessionStatus.InProgress ? "Berlangsung" : "Belum ditandatangani")));

            var reviews = await _dbContext.RhbProgressReviews.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && x.RecordStatus == RehabRecordStatus.Draft)
                .Select(x => new { x.Id, Discipline = x.Discipline != null ? x.Discipline.DisciplineName : null })
                .ToListAsync(cancellationToken);
            result.AddRange(reviews.Select(x => Pending("ProgressReview", "Reasesmen", x.Id, x.Discipline ?? "Medis", "Draf")));

            return result;
        }

        private static RehabPendingDocument Pending(string type, string typeName, Guid id, string? reference, string status) => new()
        {
            DocumentType = type,
            DocumentTypeName = typeName,
            DocumentId = id,
            Reference = reference,
            StatusName = status
        };

        private async Task<RehabDischargeSummaryResponse?> LoadAsync(Guid episodeId, Guid actorUserId, CancellationToken cancellationToken)
        {
            var episode = await _dbContext.RhbEpisodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
            if (episode == null)
                return null;

            var response = new RehabDischargeSummaryResponse
            {
                EpisodeId = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                EpisodeStatus = episode.EpisodeStatus,
                EpisodeStatusName = RhbRequestService.EpisodeStatusName(episode.EpisodeStatus),
                FollowUpTypeName = FollowUpName(RehabFollowUpType.None)
            };

            response.ReadyToCloseSignals = await _dbContext.RhbStatusHistories.AsNoTracking()
                .Where(x => x.AggregateType == RhbStatusHistoryWriter.Episode && x.AggregateId == episodeId && !x.IsDelete && x.Action.StartsWith("ready-to-close:"))
                .OrderByDescending(x => x.OccurredAt)
                .Select(x => x.Action.Substring("ready-to-close:".Length))
                .ToListAsync(cancellationToken);

            var entity = await _dbContext.RhbDischargeSummaries.AsNoTracking().FirstOrDefaultAsync(x => x.EpisodeId == episodeId && !x.IsDelete, cancellationToken);
            if (episode.EpisodeStatus != RehabEpisodeStatus.Closed)
                response.PendingDocuments = await PendingDocumentsAsync(episodeId, cancellationToken);

            if (entity == null)
            {
                response.AvailableActions = episode.EpisodeStatus == RehabEpisodeStatus.Closed ? [] : ["update"];
                return response;
            }

            response.Exists = true;
            response.Id = entity.Id;
            response.EncounterId = entity.EncounterId;
            response.EncounterNumber = await _dbContext.Set<RegPatientEncounter>().AsNoTracking().Where(e => e.Id == entity.EncounterId).Select(e => e.EncounterNumber).FirstOrDefaultAsync(cancellationToken);
            response.ClosureReason = entity.ClosureReason;
            response.ClosureReasonName = entity.ClosureReason.HasValue ? ClosureReasonName(entity.ClosureReason.Value) : null;
            response.MainServicesText = entity.MainServicesText;
            response.GoalAchievementText = entity.GoalAchievementText;
            response.FinalConditionText = entity.FinalConditionText;
            response.FollowUpType = entity.FollowUpType;
            response.FollowUpTypeName = FollowUpName(entity.FollowUpType);
            response.FollowUpInstructionText = entity.FollowUpInstructionText;
            response.FollowUpTargetText = entity.FollowUpTargetText;
            response.FollowUpDueDate = entity.FollowUpDueDate;
            response.AuthorUserId = entity.AuthorUserId;
            response.AuthorName = await _dbContext.Users.AsNoTracking().Where(u => u.Id == entity.AuthorUserId).Select(u => u.DisplayName).FirstOrDefaultAsync(cancellationToken);
            response.RecordStatus = entity.RecordStatus;
            response.RecordStatusName = RhbEpisodeService.RecordStatusName(entity.RecordStatus);
            response.SignedAt = entity.SignedAt;
            response.Version = entity.Version;

            var integrity = await _integrity.FindAsync(ClinicalDocumentKind.RehabDischargeSummary, entity.Id, cancellationToken);
            response.IntegrityStatus = integrity?.IntegrityStatus;
            response.IntegrityStatusName = integrity != null ? IntegrityStatusName(integrity.IntegrityStatus) : null;
            var mutableDraft = entity.RecordStatus == RehabRecordStatus.Draft && integrity?.IntegrityStatus is null or ClinicalDocumentIntegrityStatus.Draft;
            response.AvailableActions = mutableDraft && entity.AuthorUserId == actorUserId
                ? ["update", "sign"]
                : entity.RecordStatus == RehabRecordStatus.Final ? ["addendum"] : [];

            return response;
        }

        private static void Touch(RhbDischargeSummary entity, Guid actorUserId, DateTime nowUtc)
        {
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = nowUtc;
            entity.Version++;
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
                return R.Conflict(RhbErrorCodes.InvalidState, "Ringkasan akhir episode ini sudah dibuat pengguna lain. Muat ulang data.");
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

        private static R Stale() => R.Conflict(RhbErrorCodes.StaleVersion, "Data sudah diubah pengguna lain. Muat ulang sebelum menyimpan.");

        private static R EpisodeClosed() => R.Conflict(RhbErrorCodes.EpisodeClosed, "Episode rehab sudah ditutup.");

        private static R EncounterInvalid(string message) =>
            R.Rule(RhbErrorCodes.ClinicalEncounterInvalid, message).WithFields("encounterId");
    }
}
