using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using R = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services.RhbResult<QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs.RehabEducationItem>;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Edukasi rencana terapi dan status persetujuan pasien/keluarga — <c>BE-RHB-010</c>, FR-RHB-020,
    /// FEAT-015, <c>data-dictionary.md</c> bagian 2.11, <c>state-transition-matrix.md</c> §3.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dokumen klinis jenis <c>RehabEducation</c> (52): didaftarkan ke Rekam Medis saat dicatat, setiap
    /// ubah lewat <c>EnsureMutableAsync</c>, dikunci saat ditandatangani. Koreksi sesudah final hanya lewat
    /// addendum RM (409 <c>RHB-VAL-021</c>).
    /// </para>
    /// <para>
    /// Persetujuan <c>Refused</c> pada edukasi final terakhir sebuah program memblokir aktivasi program itu
    /// (<c>RHB-VAL-045</c>, dibaca <c>BE-RHB-011</c>). Karena itu penolakan dan setuju sebagian wajib
    /// disertai catatan, dan tanda tangan menuntut tingkat pemahaman sudah dinilai.
    /// </para>
    /// <para>
    /// Kontrak v1 tidak memuat aksi batal untuk edukasi; draft yang keliru diperbaiki pemberinya sebelum
    /// ditandatangani.
    /// </para>
    /// </remarks>
    public class RhbEducationService
    {
        private static readonly TimeSpan ClockSkewTolerance = TimeSpan.FromMinutes(2);

        private readonly ApplicationDbContext _dbContext;
        private readonly RhbDocumentIntegrityGateway _integrity;

        public RhbEducationService(ApplicationDbContext dbContext, RhbDocumentIntegrityGateway integrity)
        {
            _dbContext = dbContext;
            _integrity = integrity;
        }

        public static string UnderstandingName(RehabUnderstandingLevel level) => level switch
        {
            RehabUnderstandingLevel.Understood => "Memahami",
            RehabUnderstandingLevel.PartiallyUnderstood => "Memahami sebagian",
            RehabUnderstandingLevel.NotUnderstood => "Belum memahami",
            RehabUnderstandingLevel.NotAssessed => "Belum dinilai",
            _ => level.ToString()
        };

        public static string ConsentName(RehabConsentStatus status) => status switch
        {
            RehabConsentStatus.Agreed => "Setuju",
            RehabConsentStatus.PartiallyAgreed => "Setuju sebagian",
            RehabConsentStatus.Refused => "Menolak",
            RehabConsentStatus.NotRequired => "Tidak memerlukan persetujuan",
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

        public async Task<RhbResult<List<RehabEducationItem>>> ListByEpisodeAsync(
            Guid episodeId,
            RehabEducationQuery query,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (!await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken))
                return RhbResult<List<RehabEducationItem>>.NotFound("Episode rehab tidak ditemukan.");

            var rows = _dbContext.RhbEducationRecords.AsNoTracking().Where(x => x.EpisodeId == episodeId && !x.IsDelete);
            if (query.ProgramId.HasValue)
                rows = rows.Where(x => x.ProgramId == query.ProgramId.Value);

            var items = await Project(rows).OrderByDescending(x => x.EducatedAt).ToListAsync(cancellationToken);
            await FillAsync(items, actorUserId, cancellationToken);
            return RhbResult<List<RehabEducationItem>>.Ok(items);
        }

        // =====================================================================
        // Draft
        // =====================================================================

        public async Task<R> CreateAsync(Guid episodeId, SaveRehabEducationRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var episode = await _dbContext.RhbEpisodes.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
            if (episode == null)
                return R.NotFound("Episode rehab tidak ditemukan.");
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return EpisodeClosed();

            if (!request.EncounterId.HasValue)
                return EncounterInvalid("Pilih kunjungan tempat edukasi diberikan.");

            var encounter = await RhbServiceSupport.CheckEncounterAsync(_dbContext, request.EncounterId.Value, episode.PatientId, cancellationToken);
            if (!encounter.IsValid)
                return EncounterInvalid(encounter.Exists && !encounter.BelongsToPatient
                    ? "Kunjungan bukan milik pasien episode ini."
                    : "Kunjungan tidak ditemukan, sudah berakhir, atau pasien sudah pulang.");

            var now = DateTime.UtcNow;
            var (error, values) = await ValidateAsync(request, episodeId, now, cancellationToken);
            if (error != null)
                return error;

            var entity = new RhbEducationRecord
            {
                EpisodeId = episodeId,
                EncounterId = request.EncounterId.Value,
                EducatorUserId = actorUserId,
                RecordStatus = RehabRecordStatus.Draft,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            Apply(entity, values!);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            _dbContext.RhbEducationRecords.Add(entity);
            await _integrity.RegisterDraftAsync(
                ClinicalDocumentKind.RehabEducation, entity.Id, episode.PatientId, entity.EncounterId, actorUserId, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            await transaction.CommitAsync(cancellationToken);
            return R.Created((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> UpdateAsync(Guid id, SaveRehabEducationRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, guard) = await LoadForChangeAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            if (request.EncounterId.HasValue && request.EncounterId.Value != entity!.EncounterId)
                return EncounterInvalid("Kunjungan edukasi tidak dapat diganti.");

            var now = DateTime.UtcNow;
            var (error, values) = await ValidateAsync(request, entity!.EpisodeId, now, cancellationToken);
            if (error != null)
                return error;

            Apply(entity, values!);
            Touch(entity, actorUserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            return R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Tanda tangan
        // =====================================================================

        public async Task<R> SignAsync(
            Guid id,
            RehabSignRequest request,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            var (entity, guard) = await LoadForChangeAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            if (entity!.UnderstandingLevel == RehabUnderstandingLevel.NotAssessed)
                return R.Rule(RhbErrorCodes.EducationIncomplete, "Nilai tingkat pemahaman penerima edukasi sebelum menandatangani.")
                    .WithFields("understandingLevel");

            var noteError = ConsentNoteRule(entity.ConsentStatus, entity.ConsentNote);
            if (noteError != null)
                return noteError;

            var now = DateTime.UtcNow;

            // Tanda tangan RM menyimpan sendiri; transaksi ini menyatukan status edukasi dan kuncinya.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            entity.RecordStatus = RehabRecordStatus.Final;
            Touch(entity, actorUserId, now);

            try
            {
                var (result, _) = await _integrity.SignAsync(
                    ClinicalDocumentKind.RehabEducation, entity.Id, actorUserId, deviceInfo, ipAddress, now, cancellationToken);
                if (!result.IsAllowed)
                {
                    DetachPending();
                    return result.StatusCode == StatusCodes.Status403Forbidden
                        ? R.Forbidden(RhbErrorCodes.NotAuthor, result.ErrorMessage ?? "Hanya pemberi edukasi yang dapat menandatangani.")
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

        // =====================================================================
        // Validasi
        // =====================================================================

        private sealed record Values(
            Guid? ProgramId,
            DateTime EducatedAt,
            string MaterialText,
            string RecipientName,
            string RecipientRelation,
            string MethodText,
            RehabUnderstandingLevel UnderstandingLevel,
            string? BarrierText,
            RehabConsentStatus ConsentStatus,
            string? ConsentNote);

        private async Task<(R? Error, Values? Values)> ValidateAsync(
            SaveRehabEducationRequest request,
            Guid episodeId,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var educatedAt = request.EducatedAt.HasValue ? RhbServiceSupport.ToUtc(request.EducatedAt.Value) : nowUtc;
            if (educatedAt > nowUtc + ClockSkewTolerance)
                return (R.Invalid(RhbErrorCodes.EducationIncomplete, "Waktu edukasi tidak boleh di masa depan.").WithFields("educatedAt"), null);

            var material = RhbServiceSupport.Normalize(request.MaterialText);
            var recipient = RhbServiceSupport.Normalize(request.RecipientName);
            var relation = RhbServiceSupport.Normalize(request.RecipientRelation);
            var method = RhbServiceSupport.Normalize(request.MethodText);
            var barrier = RhbServiceSupport.Normalize(request.BarrierText);
            var note = RhbServiceSupport.Normalize(request.ConsentNote);

            var missing = new List<string>();
            if (material == null) missing.Add("materialText");
            if (recipient == null) missing.Add("recipientName");
            if (relation == null) missing.Add("recipientRelation");
            if (method == null) missing.Add("methodText");
            if (!request.ConsentStatus.HasValue || !Enum.IsDefined(request.ConsentStatus.Value)) missing.Add("consentStatus");
            if (missing.Count > 0)
                return (R.Invalid(RhbErrorCodes.EducationIncomplete, "Lengkapi materi, penerima, hubungan, metode, dan status persetujuan.")
                    .WithFields(missing.ToArray()), null);

            var understanding = request.UnderstandingLevel ?? RehabUnderstandingLevel.NotAssessed;
            if (!Enum.IsDefined(understanding))
                return (R.Invalid(RhbErrorCodes.EducationIncomplete, "Tingkat pemahaman tidak dikenal.").WithFields("understandingLevel"), null);

            foreach (var (field, value, max) in new (string, string?, int)[]
                     {
                         ("materialText", material, 2000), ("recipientName", recipient, 150), ("recipientRelation", relation, 50),
                         ("methodText", method, 200), ("barrierText", barrier, 500), ("consentNote", note, 500)
                     })
            {
                if (value?.Length > max)
                    return (TooLong(field, max), null);
            }

            var noteError = ConsentNoteRule(request.ConsentStatus!.Value, note);
            if (noteError != null)
                return (noteError, null);

            if (request.ProgramId.HasValue)
            {
                var programInEpisode = await _dbContext.RhbPrograms.AsNoTracking()
                    .AnyAsync(x => x.Id == request.ProgramId.Value && x.EpisodeId == episodeId && !x.IsDelete, cancellationToken);
                if (!programInEpisode)
                    return (R.Rule(RhbErrorCodes.ProgramNotInEpisode, "Program tidak ditemukan pada episode ini.").WithFields("programId"), null);
            }

            return (null, new Values(request.ProgramId, educatedAt, material!, recipient!, relation!, method!, understanding, barrier,
                request.ConsentStatus.Value, note));
        }

        /// <summary>Menolak dan setuju sebagian wajib disertai catatan (data-dictionary 2.11).</summary>
        private static R? ConsentNoteRule(RehabConsentStatus status, string? note) =>
            status is RehabConsentStatus.Refused or RehabConsentStatus.PartiallyAgreed && string.IsNullOrWhiteSpace(note)
                ? R.Rule(RhbErrorCodes.EducationIncomplete, "Tuliskan catatan persetujuan: apa yang ditolak atau disetujui sebagian, dan alasannya.")
                    .WithFields("consentNote")
                : null;

        // =====================================================================
        // Pembantu
        // =====================================================================

        private async Task<(RhbEducationRecord? Entity, R? Error)> LoadForChangeAsync(
            Guid id,
            int expectedVersion,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RhbEducationRecords.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return (null, R.NotFound("Catatan edukasi tidak ditemukan."));

            var closed = await _dbContext.RhbEpisodes.AsNoTracking()
                .AnyAsync(x => x.Id == entity.EpisodeId && x.EpisodeStatus == RehabEpisodeStatus.Closed, cancellationToken);
            if (closed)
                return (null, EpisodeClosed());

            if (entity.RecordStatus == RehabRecordStatus.Final)
                return (null, R.Conflict(RhbErrorCodes.DocumentLocked, "Catatan sudah final, gunakan addendum."));
            if (entity.RecordStatus != RehabRecordStatus.Draft)
                return (null, R.Conflict(RhbErrorCodes.InvalidState, "Catatan edukasi sudah dibatalkan."));

            if (entity.Version != expectedVersion)
                return (null, Stale().WithFields("expectedVersion"));

            if (entity.EducatorUserId != actorUserId)
                return (null, R.Forbidden(RhbErrorCodes.NotAuthor, "Hanya pemberi edukasi yang dapat mengubah atau menandatanganinya."));

            var mutable = await _integrity.EnsureMutableAsync(ClinicalDocumentKind.RehabEducation, entity.Id, cancellationToken);
            if (!mutable.IsAllowed)
                return (null, R.Conflict(RhbErrorCodes.DocumentLocked, mutable.ErrorMessage ?? "Catatan sudah terkunci, gunakan addendum."));

            return (entity, null);
        }

        private static void Apply(RhbEducationRecord entity, Values values)
        {
            entity.ProgramId = values.ProgramId;
            entity.EducatedAt = values.EducatedAt;
            entity.MaterialText = values.MaterialText;
            entity.RecipientName = values.RecipientName;
            entity.RecipientRelation = values.RecipientRelation;
            entity.MethodText = values.MethodText;
            entity.UnderstandingLevel = values.UnderstandingLevel;
            entity.BarrierText = values.BarrierText;
            entity.ConsentStatus = values.ConsentStatus;
            entity.ConsentNote = values.ConsentNote;
        }

        private static void Touch(RhbEducationRecord entity, Guid actorUserId, DateTime nowUtc)
        {
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = nowUtc;
            entity.Version++;
        }

        private static IQueryable<RehabEducationItem> Project(IQueryable<RhbEducationRecord> rows) =>
            rows.Select(x => new RehabEducationItem
            {
                Id = x.Id,
                EpisodeId = x.EpisodeId,
                ProgramId = x.ProgramId,
                ProgramLabel = x.Program != null ? x.Program.Discipline!.DisciplineName + " v" + x.Program.ProgramVersion : null,
                EncounterId = x.EncounterId,
                EducatedAt = x.EducatedAt,
                MaterialText = x.MaterialText,
                RecipientName = x.RecipientName,
                RecipientRelation = x.RecipientRelation,
                MethodText = x.MethodText,
                UnderstandingLevel = x.UnderstandingLevel,
                BarrierText = x.BarrierText,
                ConsentStatus = x.ConsentStatus,
                ConsentNote = x.ConsentNote,
                EducatorUserId = x.EducatorUserId,
                RecordStatus = x.RecordStatus,
                Version = x.Version
            });

        private async Task<RehabEducationItem?> LoadAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var item = await Project(_dbContext.RhbEducationRecords.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken);
            if (item == null)
                return null;

            await FillAsync([item], actorUserId, cancellationToken);
            return item;
        }

        private async Task FillAsync(List<RehabEducationItem> items, Guid actorUserId, CancellationToken cancellationToken)
        {
            if (items.Count == 0)
                return;

            var ids = items.Select(x => x.Id).ToList();
            var educatorIds = items.Select(x => x.EducatorUserId).Distinct().ToList();
            var encounterIds = items.Select(x => x.EncounterId).Distinct().ToList();

            var educators = await _dbContext.Users.AsNoTracking()
                .Where(u => educatorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);
            var encounters = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Where(e => encounterIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => e.EncounterNumber, cancellationToken);
            var integrities = await _dbContext.Set<QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models.MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.DocumentKind == ClinicalDocumentKind.RehabEducation && ids.Contains(x.DocumentId) && !x.IsDelete)
                .Select(x => new { x.DocumentId, x.IntegrityStatus, x.SignedAt })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                var integrity = integrities.FirstOrDefault(x => x.DocumentId == item.Id);
                item.UnderstandingLevelName = UnderstandingName(item.UnderstandingLevel);
                item.ConsentStatusName = ConsentName(item.ConsentStatus);
                item.RecordStatusName = RhbEpisodeService.RecordStatusName(item.RecordStatus);
                item.EducatorName = educators.TryGetValue(item.EducatorUserId, out var name) ? name : null;
                item.EncounterNumber = encounters.TryGetValue(item.EncounterId, out var number) ? number : null;
                item.IntegrityStatus = integrity?.IntegrityStatus;
                item.IntegrityStatusName = integrity != null ? IntegrityStatusName(integrity.IntegrityStatus) : null;
                item.SignedAt = integrity?.SignedAt;

                var mutableDraft = item.RecordStatus == RehabRecordStatus.Draft &&
                                   integrity?.IntegrityStatus is null or ClinicalDocumentIntegrityStatus.Draft;
                item.AvailableActions = mutableDraft && item.EducatorUserId == actorUserId
                    ? ["update", "sign"]
                    : item.RecordStatus == RehabRecordStatus.Final || integrity?.IntegrityStatus == ClinicalDocumentIntegrityStatus.LockedUnsigned
                        ? ["addendum"]
                        : [];
            }
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

        private static R TooLong(string field, int max) =>
            R.Invalid(RhbErrorCodes.FieldTooLong, $"Isian terlalu panjang (maksimal {max} karakter).").WithFields(field);
    }
}
