using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using R = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services.RhbResult<QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs.RehabGoalItem>;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Tujuan terapi jangka pendek dan panjang — <c>BE-RHB-011</c>, FR-RHB-010,
    /// <c>state-transition-matrix.md</c> §8, V-GOL-01/02.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tujuan bukan dokumen klinis bertanda tangan; yang dikunci adalah program yang merujuknya. Karena
    /// itu tujuan yang dirujuk program <c>Active</c> tidak boleh diubah (409 <c>RHB-VAL-046</c>) dan
    /// koreksinya lewat <c>revise</c>: versi lama menjadi <c>Superseded</c>, versi baru <c>Active</c>.
    /// Program aktif tetap menunjuk versi lama sampai programnya direvisi.
    /// </para>
    /// <para>
    /// Alasan revisi dan penghentian disimpan di <c>RhbStatusHistory</c> (jenis agregat <c>Goal</c>),
    /// karena tabel tujuan tidak punya kolom alasan.
    /// </para>
    /// </remarks>
    public class RhbGoalService
    {
        private static readonly RehabEpisodeStatus[] PlanningOrActive = [RehabEpisodeStatus.Planning, RehabEpisodeStatus.Active];

        private readonly ApplicationDbContext _dbContext;

        public RhbGoalService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static string TermName(RehabGoalTerm term) => term switch
        {
            RehabGoalTerm.ShortTerm => "Jangka pendek",
            RehabGoalTerm.LongTerm => "Jangka panjang",
            _ => term.ToString()
        };

        public static string StatusName(RehabGoalStatus status) => status switch
        {
            RehabGoalStatus.Active => "Aktif",
            RehabGoalStatus.Achieved => "Tercapai",
            RehabGoalStatus.PartiallyAchieved => "Tercapai sebagian",
            RehabGoalStatus.NotAchieved => "Tidak tercapai",
            RehabGoalStatus.Discontinued => "Dihentikan",
            RehabGoalStatus.Superseded => "Digantikan versi baru",
            _ => status.ToString()
        };

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<RhbResult<List<RehabGoalItem>>> ListByEpisodeAsync(
            Guid episodeId,
            RehabGoalQuery query,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (!await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken))
                return RhbResult<List<RehabGoalItem>>.NotFound("Episode rehab tidak ditemukan.");

            var rows = _dbContext.RhbGoals.AsNoTracking().Where(x => x.EpisodeId == episodeId && !x.IsDelete);
            if (query.DisciplineId.HasValue)
                rows = rows.Where(x => x.DisciplineId == query.DisciplineId.Value);
            if (query.IncludeSuperseded != true)
                rows = rows.Where(x => x.GoalStatus != RehabGoalStatus.Superseded);

            var items = await Project(rows)
                .OrderBy(x => x.DisciplineName).ThenBy(x => x.Term).ThenBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);
            await FillAsync(items, actorUserId, cancellationToken);
            return RhbResult<List<RehabGoalItem>>.Ok(items);
        }

        // =====================================================================
        // Tulis
        // =====================================================================

        public async Task<R> CreateAsync(Guid episodeId, SaveRehabGoalRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var episode = await _dbContext.RhbEpisodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
            if (episode == null)
                return R.NotFound("Episode rehab tidak ditemukan.");
            var stateError = EpisodeState(episode.EpisodeStatus);
            if (stateError != null)
                return stateError;

            if (request.DisciplineId.HasValue && !await IsPrescribedAsync(episodeId, request.DisciplineId.Value, cancellationToken))
                return R.Rule(RhbErrorCodes.DisciplineNotPrescribed, "Layanan ini belum diresepkan dokter rehab untuk episode ini.").WithFields("disciplineId");

            var now = DateTime.UtcNow;
            var (error, values) = Validate(request);
            if (error != null)
                return error;

            var entity = new RhbGoal
            {
                EpisodeId = episodeId,
                DisciplineId = request.DisciplineId,
                GoalStatus = RehabGoalStatus.Active,
                AuthorUserId = actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            Apply(entity, values!);
            _dbContext.RhbGoals.Add(entity);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Created((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> UpdateAsync(Guid id, SaveRehabGoalRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, guard) = await LoadActiveAsync(id, request.ExpectedVersion, cancellationToken);
            if (guard != null)
                return guard;

            if (entity!.AuthorUserId != actorUserId)
                return R.Forbidden(RhbErrorCodes.NotAuthor, "Hanya penulis tujuan yang dapat mengubahnya. Gunakan revisi.");

            if (request.DisciplineId.HasValue && request.DisciplineId != entity.DisciplineId)
                return R.Invalid(RhbErrorCodes.GoalIncomplete, "Layanan tujuan tidak dapat diganti.").WithFields("disciplineId");

            if (await IsUsedByActiveProgramAsync(entity.Id, cancellationToken))
                return R.Conflict(RhbErrorCodes.GoalInUse, "Tujuan sedang dipakai program aktif. Gunakan revisi.");

            var (error, values) = Validate(request);
            if (error != null)
                return error;

            Apply(entity, values!);
            Touch(entity, actorUserId, DateTime.UtcNow);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> ReviseAsync(Guid id, ReviseRehabGoalRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, guard) = await LoadActiveAsync(id, request.ExpectedVersion, cancellationToken);
            if (guard != null)
                return guard;

            var reason = RhbServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return R.Rule(RhbErrorCodes.ReasonRequired, "Alasan revisi wajib diisi.").WithFields("reason");
            if (reason.Length > 1000)
                return TooLong("reason", 1000);

            var (error, values) = Validate(request);
            if (error != null)
                return error;

            var now = DateTime.UtcNow;
            entity!.GoalStatus = RehabGoalStatus.Superseded;
            Touch(entity, actorUserId, now);

            var revision = new RhbGoal
            {
                EpisodeId = entity.EpisodeId,
                DisciplineId = entity.DisciplineId,
                GoalStatus = RehabGoalStatus.Active,
                SupersedesGoalId = entity.Id,
                AuthorUserId = actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            Apply(revision, values!);
            _dbContext.RhbGoals.Add(revision);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Goal, entity.Id,
                RehabGoalStatus.Active.ToString(), RehabGoalStatus.Superseded.ToString(), "revise", reason, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Goal, revision.Id,
                null, RehabGoalStatus.Active.ToString(), "revise", reason, actorUserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Created((await LoadAsync(revision.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> DiscontinueAsync(Guid id, RehabReasonRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, guard) = await LoadActiveAsync(id, request.ExpectedVersion, cancellationToken);
            if (guard != null)
                return guard;

            var reason = RhbServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return R.Rule(RhbErrorCodes.ReasonRequired, "Alasan penghentian wajib diisi.").WithFields("reason");
            if (reason.Length > 1000)
                return TooLong("reason", 1000);

            var now = DateTime.UtcNow;
            entity!.GoalStatus = RehabGoalStatus.Discontinued;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Goal, entity.Id,
                RehabGoalStatus.Active.ToString(), RehabGoalStatus.Discontinued.ToString(), "discontinue", reason, actorUserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        private sealed record Values(RehabGoalTerm Term, string Description, string Baseline, string Target, DateOnly TargetDate, bool Agreed);

        /// <summary>V-GOL-02: deskripsi, baseline, target, jangka, dan tanggal target wajib; tanggal target tidak lampau.</summary>
        private static (R? Error, Values? Values) Validate(SaveRehabGoalRequest request)
        {
            var description = RhbServiceSupport.Normalize(request.DescriptionText);
            var baseline = RhbServiceSupport.Normalize(request.BaselineText);
            var target = RhbServiceSupport.Normalize(request.TargetText);

            var missing = new List<string>();
            if (!request.Term.HasValue || !Enum.IsDefined(request.Term.Value)) missing.Add("term");
            if (description == null) missing.Add("descriptionText");
            if (baseline == null) missing.Add("baselineText");
            if (target == null) missing.Add("targetText");
            if (!request.TargetDate.HasValue) missing.Add("targetDate");
            if (missing.Count > 0)
                return (R.Rule(RhbErrorCodes.GoalIncomplete, "Lengkapi tujuan, baseline, target, dan tanggal target.").WithFields(missing.ToArray()), null);

            if (request.TargetDate!.Value < RhbServiceSupport.TodayLocal())
                return (R.Rule(RhbErrorCodes.GoalIncomplete, "Tanggal target tidak boleh lampau.").WithFields("targetDate"), null);

            if (description!.Length > 1000) return (TooLong("descriptionText", 1000), null);
            if (baseline!.Length > 500) return (TooLong("baselineText", 500), null);
            if (target!.Length > 500) return (TooLong("targetText", 500), null);

            return (null, new Values(request.Term!.Value, description, baseline, target, request.TargetDate.Value, request.AgreedWithPatient));
        }

        private static void Apply(RhbGoal entity, Values values)
        {
            entity.Term = values.Term;
            entity.DescriptionText = values.Description;
            entity.BaselineText = values.Baseline;
            entity.TargetText = values.Target;
            entity.TargetDate = values.TargetDate;
            entity.AgreedWithPatient = values.Agreed;
        }

        /// <summary>Tujuan aktif, episode belum ditutup, versi cocok.</summary>
        private async Task<(RhbGoal? Entity, R? Error)> LoadActiveAsync(Guid id, int expectedVersion, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RhbGoals.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return (null, R.NotFound("Tujuan terapi tidak ditemukan."));

            var episodeStatus = await _dbContext.RhbEpisodes.AsNoTracking()
                .Where(x => x.Id == entity.EpisodeId).Select(x => x.EpisodeStatus).FirstAsync(cancellationToken);
            if (episodeStatus == RehabEpisodeStatus.Closed)
                return (null, R.Conflict(RhbErrorCodes.EpisodeClosed, "Episode rehab sudah ditutup."));

            if (entity.GoalStatus != RehabGoalStatus.Active)
                return (null, R.Conflict(RhbErrorCodes.InvalidState, $"Tujuan berstatus {StatusName(entity.GoalStatus)} tidak dapat diubah."));

            if (entity.Version != expectedVersion)
                return (null, Stale().WithFields("expectedVersion"));

            return (entity, null);
        }

        private static R? EpisodeState(RehabEpisodeStatus status) => status switch
        {
            RehabEpisodeStatus.Closed => R.Conflict(RhbErrorCodes.EpisodeClosed, "Episode rehab sudah ditutup."),
            _ when !PlanningOrActive.Contains(status) => R.Conflict(RhbErrorCodes.InvalidState, "Tujuan disusun setelah asesmen medis final."),
            _ => null
        };

        private Task<bool> IsPrescribedAsync(Guid episodeId, Guid disciplineId, CancellationToken cancellationToken) =>
            _dbContext.RhbEpisodeDisciplines.AsNoTracking().AnyAsync(x =>
                x.EpisodeId == episodeId && x.DisciplineId == disciplineId && !x.IsDelete &&
                x.PrescribedByAssessment!.RecordStatus == RehabRecordStatus.Final, cancellationToken);

        private Task<bool> IsUsedByActiveProgramAsync(Guid goalId, CancellationToken cancellationToken) =>
            _dbContext.RhbProgramGoals.AsNoTracking().AnyAsync(x =>
                x.GoalId == goalId && !x.IsDelete && !x.Program!.IsDelete && x.Program.ProgramStatus == RehabProgramStatus.Active, cancellationToken);

        private static void Touch(RhbGoal entity, Guid actorUserId, DateTime nowUtc)
        {
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = nowUtc;
            entity.Version++;
        }

        private static IQueryable<RehabGoalItem> Project(IQueryable<RhbGoal> rows) =>
            rows.Select(x => new RehabGoalItem
            {
                Id = x.Id,
                EpisodeId = x.EpisodeId,
                DisciplineId = x.DisciplineId,
                DisciplineName = x.Discipline != null ? x.Discipline.DisciplineName : null,
                Term = x.Term,
                DescriptionText = x.DescriptionText,
                BaselineText = x.BaselineText,
                TargetText = x.TargetText,
                TargetDate = x.TargetDate,
                GoalStatus = x.GoalStatus,
                SupersedesGoalId = x.SupersedesGoalId,
                AgreedWithPatient = x.AgreedWithPatient,
                AuthorUserId = x.AuthorUserId,
                CreateDateTime = x.CreateDateTime,
                Version = x.Version
            });

        private async Task<RehabGoalItem?> LoadAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var item = await Project(_dbContext.RhbGoals.AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync(cancellationToken);
            if (item != null)
                await FillAsync([item], actorUserId, cancellationToken);
            return item;
        }

        private async Task FillAsync(List<RehabGoalItem> items, Guid actorUserId, CancellationToken cancellationToken)
        {
            if (items.Count == 0)
                return;

            var ids = items.Select(x => x.Id).ToList();
            var authorIds = items.Select(x => x.AuthorUserId).Distinct().ToList();
            var authors = await _dbContext.Users.AsNoTracking()
                .Where(u => authorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);
            var inUse = await _dbContext.RhbProgramGoals.AsNoTracking()
                .Where(x => ids.Contains(x.GoalId) && !x.IsDelete && !x.Program!.IsDelete && x.Program.ProgramStatus == RehabProgramStatus.Active)
                .Select(x => x.GoalId)
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.TermName = TermName(item.Term);
                item.GoalStatusName = StatusName(item.GoalStatus);
                item.AuthorName = authors.TryGetValue(item.AuthorUserId, out var name) ? name : null;
                item.UsedByActiveProgram = inUse.Contains(item.Id);

                var actions = new List<string>();
                if (item.GoalStatus == RehabGoalStatus.Active)
                {
                    if (!item.UsedByActiveProgram && item.AuthorUserId == actorUserId)
                        actions.Add("update");
                    actions.Add("revise");
                    actions.Add("discontinue");
                }
                item.AvailableActions = actions;
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
                foreach (var entry in _dbContext.ChangeTracker.Entries().ToList())
                {
                    if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                        entry.State = EntityState.Detached;
                }
                return Stale();
            }
        }

        private static R Stale() => R.Conflict(RhbErrorCodes.StaleVersion, "Data sudah diubah pengguna lain. Muat ulang sebelum menyimpan.");

        private static R TooLong(string field, int max) =>
            R.Invalid(RhbErrorCodes.FieldTooLong, $"Isian terlalu panjang (maksimal {max} karakter).").WithFields(field);
    }
}
