using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Program HD pasien beserta kelayakan, akses vaskular, tinjauan serologi, dan keputusan
    /// isolasi (<c>BE-HMD-08</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Satu episode aktif per pasien</b> dijaga dua lapis: service menolak aktivasi episode
    /// kedua dengan <c>409 HMD-VAL-011</c>, dan unique index bersyarat
    /// <c>IX_HmdEpisode_PatientId_Active</c> menolak dua aktivasi serentak yang lolos pemeriksaan
    /// service pada saat yang sama.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Ibu Sinta sudah punya episode aktif <c>HD-EP-00000001</c>. Petugas membuat
    /// episode baru untuk pasien yang sama lalu menekan Aktifkan: ditolak <c>409</c>, dan episode
    /// kedua tetap tersimpan sebagai draf. Ketika Ibu Sinta punya sesi kemarin yang masih
    /// <c>AwaitingFinalization</c>, penutupan episode ditolak <c>422 HMD-VAL-013</c> beserta daftar
    /// sesinya.
    /// </para>
    /// <para>
    /// <b>Serologi adalah data paling sensitif modul ini.</b> Ia hanya keluar lewat endpoint yang
    /// dijaga <c>HemodialysisSerology : Read</c>. Daftar episode dan daftar kerja hanya membawa
    /// penanda isolasi berupa <c>true</c>/<c>false</c>.
    /// </para>
    /// </remarks>
    public class HmdEpisodeService
    {
        private static readonly HmdSessionStatus[] SettledSessionStatuses =
        [
            HmdSessionStatus.Finalized,
            HmdSessionStatus.Cancelled
        ];

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly InpatientClinicalContextService _clinicalContextService;

        public HmdEpisodeService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            InpatientClinicalContextService clinicalContextService)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _clinicalContextService = clinicalContextService;
        }

        // =================================================================
        // Episode
        // =================================================================

        public static HmdEpisodeFilterMetadataResponse BuildFilterMetadata() => new()
        {
            EpisodeStatusOptions = HmdLabels.Options<HmdEpisodeStatus>(HmdLabels.EpisodeStatus),
            ClosureReasonOptions = HmdLabels.Options<HmdEpisodeClosureReason>(HmdLabels.ClosureReason),
            SortOptions =
            [
                new() { Value = "startDate", Label = "Tanggal mulai program" },
                new() { Value = "episodeNumber", Label = "Nomor episode" },
                new() { Value = "episodeStatus", Label = "Status" },
                new() { Value = "patientName", Label = "Nama pasien" }
            ],
            SortDirections = HmdLabels.SortDirections(),
            PageSizeOptions = HmdLabels.PageSizeOptions()
        };

        public async Task<HmdEpisodeSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
        {
            var rows = _dbContext.HmdEpisodes.AsNoTracking().Where(x => !x.IsDelete);

            return new HmdEpisodeSummaryResponse
            {
                TotalEpisode = await rows.CountAsync(cancellationToken),
                DraftEpisode = await rows.CountAsync(x => x.EpisodeStatus == HmdEpisodeStatus.Draft, cancellationToken),
                ActiveEpisode = await rows.CountAsync(x => x.EpisodeStatus == HmdEpisodeStatus.Active, cancellationToken),
                SuspendedEpisode = await rows.CountAsync(x => x.EpisodeStatus == HmdEpisodeStatus.Suspended, cancellationToken),
                ClosedEpisode = await rows.CountAsync(x => x.EpisodeStatus == HmdEpisodeStatus.Closed, cancellationToken)
            };
        }

        public async Task<PagedResult<HmdEpisodeListResponse>> GetListAsync(HmdEpisodePagedQuery query, CancellationToken cancellationToken)
        {
            var (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);
            var today = HmdServiceSupport.TodayLocal();

            var rows = _dbContext.HmdEpisodes.AsNoTracking().Where(x => !x.IsDelete);

            if (query.PatientId.HasValue)
                rows = rows.Where(x => x.PatientId == query.PatientId.Value);
            if (query.ServiceUnitId.HasValue)
                rows = rows.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            if (query.DpjpDoctorId.HasValue)
                rows = rows.Where(x => x.DpjpDoctorId == query.DpjpDoctorId.Value);
            if (query.EpisodeStatus.HasValue)
                rows = rows.Where(x => x.EpisodeStatus == query.EpisodeStatus.Value);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x =>
                    x.EpisodeNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            var ascending = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            rows = (query.SortBy ?? "startDate").ToLowerInvariant() switch
            {
                "episodenumber" => ascending ? rows.OrderBy(x => x.EpisodeNumber) : rows.OrderByDescending(x => x.EpisodeNumber),
                "episodestatus" => ascending ? rows.OrderBy(x => x.EpisodeStatus) : rows.OrderByDescending(x => x.EpisodeStatus),
                "patientname" => ascending ? rows.OrderBy(x => x.Patient!.FullName) : rows.OrderByDescending(x => x.Patient!.FullName),
                _ => ascending ? rows.OrderBy(x => x.StartDate) : rows.OrderByDescending(x => x.StartDate)
            };

            var total = await rows.CountAsync(cancellationToken);
            var items = await rows
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HmdEpisodeListResponse
                {
                    Id = x.Id,
                    EpisodeNumber = x.EpisodeNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    DpjpDoctorId = x.DpjpDoctorId,
                    DpjpDoctorName = x.DpjpDoctor != null ? x.DpjpDoctor.FullName : null,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    EpisodeStatus = x.EpisodeStatus,
                    ActivatedAt = x.ActivatedAt,
                    ClosedAt = x.ClosedAt,
                    IsIsolationRequired = x.IsolationDecisions.Any(d =>
                        d.IsActive && !d.IsDelete && d.Requirement != HmdIsolationRequirement.None &&
                        d.EffectiveFrom <= today && (d.EffectiveTo == null || d.EffectiveTo >= today)),
                    HasActivePrescription = x.Prescriptions.Any(p => !p.IsDelete && p.PrescriptionStatus == HmdPrescriptionStatus.Active)
                })
                .ToListAsync(cancellationToken);

            items.ForEach(x => x.EpisodeStatusName = HmdLabels.EpisodeStatus(x.EpisodeStatus));

            return new PagedResult<HmdEpisodeListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            };
        }

        public async Task<HmdEpisodeDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        {
            var today = HmdServiceSupport.TodayLocal();

            var row = await _dbContext.HmdEpisodes.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new HmdEpisodeDetailResponse
                {
                    Id = x.Id,
                    EpisodeNumber = x.EpisodeNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    Patient = new HmdPatientHeaderResponse
                    {
                        PatientId = x.PatientId,
                        PatientName = x.Patient != null ? x.Patient.FullName : null,
                        MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                        BirthDate = x.Patient != null ? x.Patient.BirthDate : null
                    },
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    DpjpDoctorId = x.DpjpDoctorId,
                    DpjpDoctorName = x.DpjpDoctor != null ? x.DpjpDoctor.FullName : null,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    EpisodeStatus = x.EpisodeStatus,
                    ActivatedAt = x.ActivatedAt,
                    ClosedAt = x.ClosedAt,
                    SuspendReason = x.SuspendReason,
                    ClosureReason = x.ClosureReason,
                    ClosureNote = x.ClosureNote,
                    HasActivePrescription = x.Prescriptions.Any(p => !p.IsDelete && p.PrescriptionStatus == HmdPrescriptionStatus.Active),
                    ActivePrescriptionId = x.Prescriptions
                        .Where(p => !p.IsDelete && p.PrescriptionStatus == HmdPrescriptionStatus.Active)
                        .Select(p => (Guid?)p.Id)
                        .FirstOrDefault(),
                    PrimaryVascularAccessId = x.VascularAccesses
                        .Where(a => !a.IsDelete && a.IsPrimary)
                        .Select(a => (Guid?)a.Id)
                        .FirstOrDefault(),
                    PrimaryVascularAccessName = x.VascularAccesses
                        .Where(a => !a.IsDelete && a.IsPrimary)
                        .Select(a => a.AccessSite)
                        .FirstOrDefault(),
                    LatestEligibilityOutcome = x.EligibilityAssessments
                        .Where(e => !e.IsDelete)
                        .OrderByDescending(e => e.AssessedAt)
                        .Select(e => (HmdEligibilityOutcome?)e.Outcome)
                        .FirstOrDefault(),
                    SessionCount = x.Sessions.Count(s => !s.IsDelete),
                    UnfinalizedSessionCount = x.Sessions.Count(s => !s.IsDelete &&
                        s.SessionStatus != HmdSessionStatus.Finalized && s.SessionStatus != HmdSessionStatus.Cancelled)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                return null;

            row.EpisodeStatusName = HmdLabels.EpisodeStatus(row.EpisodeStatus);
            row.ClosureReasonName = row.ClosureReason.HasValue ? HmdLabels.ClosureReason(row.ClosureReason.Value) : null;
            row.ActiveIsolationRequirement = await HmdServiceSupport.GetActiveIsolationAsync(_dbContext, id, today, cancellationToken);
            row.ActiveIsolationRequirementName = HmdLabels.Isolation(row.ActiveIsolationRequirement);
            row.IsIsolationRequired = row.ActiveIsolationRequirement != HmdIsolationRequirement.None;
            row.AvailableActions = row.EpisodeStatus switch
            {
                HmdEpisodeStatus.Draft => ["Update", "Activate", "Close"],
                HmdEpisodeStatus.Active => ["Update", "Suspend", "Close"],
                HmdEpisodeStatus.Suspended => ["Update", "Activate", "Close"],
                _ => []
            };

            return row;
        }

        public async Task<HmdResult<HmdEpisodeDetailResponse>> CreateAsync(
            CreateHmdEpisodeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (!request.DpjpDoctorId.HasValue ||
                !await _dbContext.Set<MstPatient>().AsNoTracking().AnyAsync(x => x.Id == request.PatientId && !x.IsDelete, cancellationToken) ||
                !await _dbContext.Set<MstDoctor>().AsNoTracking().AnyAsync(x => x.Id == request.DpjpDoctorId.Value && !x.IsDelete && x.IsActive, cancellationToken))
            {
                return HmdResult<HmdEpisodeDetailResponse>.Invalid(HmdErrorCodes.Val010, HmdMessages.Val010);
            }

            if (!await _dbContext.Set<MstServiceUnit>().AsNoTracking().AnyAsync(x => x.Id == request.ServiceUnitId && !x.IsDelete && x.IsActive, cancellationToken))
                return HmdResult<HmdEpisodeDetailResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Unit layanan tidak ditemukan atau tidak aktif.");

            var number = await HmdServiceSupport.AllocateNumberAsync(
                _numberSeriesAllocator, HmdServiceSupport.EpisodeSequenceKey, HmdServiceSupport.EpisodeNumberPrefix, actorUserId, cancellationToken);
            if (number == null)
                return HmdResult<HmdEpisodeDetailResponse>.Rule(HmdErrorCodes.NumberAllocationFailed, HmdMessages.NumberAllocationFailed);

            var now = DateTime.UtcNow;
            var episode = new HmdEpisode
            {
                EpisodeNumber = number,
                PatientId = request.PatientId,
                ServiceUnitId = request.ServiceUnitId,
                DpjpDoctorId = request.DpjpDoctorId.Value,
                StartDate = request.StartDate,
                EpisodeStatus = HmdEpisodeStatus.Draft,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdEpisodes.Add(episode);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdEpisodeDetailResponse>.From(failure);

            return HmdResult<HmdEpisodeDetailResponse>.Created((await GetDetailAsync(episode.Id, cancellationToken))!);
        }

        public async Task<HmdResult<HmdEpisodeDetailResponse>> UpdateAsync(
            Guid id,
            UpdateHmdEpisodeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var episode = await _dbContext.HmdEpisodes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (episode == null)
                return NotFound();

            if (episode.EpisodeStatus == HmdEpisodeStatus.Closed)
                return HmdResult<HmdEpisodeDetailResponse>.Rule(HmdErrorCodes.InvalidTransition, "Episode yang sudah ditutup tidak dapat diubah.");

            if (!await _dbContext.Set<MstDoctor>().AsNoTracking().AnyAsync(x => x.Id == request.DpjpDoctorId && !x.IsDelete && x.IsActive, cancellationToken))
                return HmdResult<HmdEpisodeDetailResponse>.Invalid(HmdErrorCodes.Val010, HmdMessages.Val010);

            episode.DpjpDoctorId = request.DpjpDoctorId;
            episode.StartDate = request.StartDate;
            Touch(episode, actorUserId);

            return await SaveAndReturnAsync(id, cancellationToken);
        }

        /// <summary>
        /// Mengaktifkan, menangguhkan, mengaktifkan kembali, atau menutup episode —
        /// <c>state-transition-matrix.md</c> bagian 2.
        /// </summary>
        public async Task<HmdResult<HmdEpisodeDetailResponse>> ChangeStatusAsync(
            Guid id,
            ChangeHmdEpisodeStatusRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var episode = await _dbContext.HmdEpisodes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (episode == null)
                return NotFound();

            var from = episode.EpisodeStatus;
            var to = request.TargetStatus;
            var now = DateTime.UtcNow;

            switch (from, to)
            {
                case (HmdEpisodeStatus.Draft, HmdEpisodeStatus.Active):
                case (HmdEpisodeStatus.Suspended, HmdEpisodeStatus.Active):
                {
                    var setting = await HmdServiceSupport.FindSettingAsync(_dbContext, episode.ServiceUnitId, cancellationToken);
                    var allowMultiple = setting?.AllowMultipleActiveEpisodePerPatient ?? false;

                    if (!allowMultiple &&
                        await _dbContext.HmdEpisodes.AsNoTracking().AnyAsync(x =>
                            x.Id != id && x.PatientId == episode.PatientId && !x.IsDelete && x.EpisodeStatus == HmdEpisodeStatus.Active,
                            cancellationToken))
                    {
                        return HmdResult<HmdEpisodeDetailResponse>.Conflict(HmdErrorCodes.Val011, HmdMessages.Val011);
                    }

                    episode.EpisodeStatus = HmdEpisodeStatus.Active;
                    episode.SuspendReason = null;
                    episode.ActivatedAt = now;
                    episode.ActivatedByUserId = actorUserId;
                    break;
                }

                case (HmdEpisodeStatus.Active, HmdEpisodeStatus.Suspended):
                {
                    var reason = HmdServiceSupport.Normalize(request.SuspendReason);
                    if (reason == null)
                        return HmdResult<HmdEpisodeDetailResponse>.Invalid(HmdErrorCodes.Val012, HmdMessages.Val012);

                    episode.EpisodeStatus = HmdEpisodeStatus.Suspended;
                    episode.SuspendReason = reason;
                    break;
                }

                case (HmdEpisodeStatus.Draft, HmdEpisodeStatus.Closed):
                case (HmdEpisodeStatus.Active, HmdEpisodeStatus.Closed):
                case (HmdEpisodeStatus.Suspended, HmdEpisodeStatus.Closed):
                {
                    if (!request.ClosureReason.HasValue)
                        return HmdResult<HmdEpisodeDetailResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Sebab penutupan episode wajib dipilih.");

                    // Draf belum pernah boleh punya sesi apa pun; episode berjalan hanya boleh
                    // ditutup bila setiap sesinya sudah disahkan atau dibatalkan (HMD-VAL-013).
                    var blocking = await _dbContext.HmdSessions.AsNoTracking()
                        .Where(x => x.EpisodeId == id && !x.IsDelete &&
                                    (from == HmdEpisodeStatus.Draft || !SettledSessionStatuses.Contains(x.SessionStatus)))
                        .OrderBy(x => x.ScheduledStartAt)
                        .Select(x => new HmdBlockingSessionItem
                        {
                            SessionId = x.Id,
                            SessionNumber = x.SessionNumber,
                            ScheduledDate = x.ScheduledDate,
                            SessionStatus = x.SessionStatus
                        })
                        .ToListAsync(cancellationToken);

                    if (blocking.Count > 0)
                    {
                        blocking.ForEach(x => x.SessionStatusName = HmdLabels.SessionStatus(x.SessionStatus));
                        return HmdResult<HmdEpisodeDetailResponse>.Rule(HmdErrorCodes.Val013, HmdMessages.Val013, blocking);
                    }

                    episode.EpisodeStatus = HmdEpisodeStatus.Closed;
                    episode.ClosureReason = request.ClosureReason.Value;
                    episode.ClosureNote = HmdServiceSupport.Normalize(request.ClosureNote);
                    episode.EndDate = request.EndDate ?? HmdServiceSupport.LocalDate(now);
                    episode.ClosedAt = now;
                    episode.ClosedByUserId = actorUserId;
                    break;
                }

                default:
                    return HmdResult<HmdEpisodeDetailResponse>.Rule(
                        HmdErrorCodes.InvalidTransition,
                        $"Status episode tidak dapat diubah dari {HmdLabels.EpisodeStatus(from)} menjadi {HmdLabels.EpisodeStatus(to)}.");
            }

            Touch(episode, actorUserId);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (HmdServiceSupport.IsUniqueViolation(exception))
            {
                // Dua aktivasi serentak lolos pemeriksaan service; index bersyarat menahan yang kedua.
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<HmdEpisodeDetailResponse>.Conflict(HmdErrorCodes.Val011, HmdMessages.Val011);
            }
            catch (DbUpdateConcurrencyException)
            {
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<HmdEpisodeDetailResponse>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }

            return HmdResult<HmdEpisodeDetailResponse>.Ok((await GetDetailAsync(id, cancellationToken))!);
        }

        // =================================================================
        // Kelayakan
        // =================================================================

        public async Task<HmdResult<PagedResult<HmdEligibilityResponse>>> GetEligibilityAsync(
            Guid episodeId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            if (!await EpisodeExistsAsync(episodeId, cancellationToken))
                return HmdResult<PagedResult<HmdEligibilityResponse>>.NotFound(EpisodeNotFoundMessage);

            (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(pageNumber, pageSize);
            var rows = _dbContext.HmdEligibilityAssessments.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderByDescending(x => x.AssessedAt);

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectEligibility(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            items.ForEach(x => x.OutcomeName = HmdLabels.EligibilityOutcome(x.Outcome));

            return HmdResult<PagedResult<HmdEligibilityResponse>>.Ok(Page(items, total, pageNumber, pageSize));
        }

        /// <summary>
        /// Dokter mencatat keputusan kelayakan. Dokter penilai diturunkan dari akun yang sedang
        /// masuk; akun tanpa tautan dokter aktif ditolak <c>403</c>.
        /// </summary>
        public async Task<HmdResult<HmdEligibilityResponse>> CreateEligibilityAsync(
            Guid episodeId,
            CreateHmdEligibilityRequest request,
            HmdActor actor,
            CancellationToken cancellationToken)
        {
            var episodeGuard = await GuardOpenEpisodeAsync<HmdEligibilityResponse>(episodeId, cancellationToken);
            if (episodeGuard != null)
                return episodeGuard;

            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(actor.Principal, actor.UserId, cancellationToken);
            if (!doctorId.HasValue)
                return HmdResult<HmdEligibilityResponse>.Forbidden(HmdErrorCodes.ActorNotDoctor, HmdMessages.ActorNotDoctor);

            var now = DateTime.UtcNow;
            var row = new HmdEligibilityAssessment
            {
                EpisodeId = episodeId,
                AssessedByDoctorId = doctorId.Value,
                AssessedAt = now,
                Outcome = request.Outcome,
                IndicationSummary = request.IndicationSummary.Trim(),
                DecisionReason = request.DecisionReason.Trim(),
                FollowUpInstruction = HmdServiceSupport.Normalize(request.FollowUpInstruction),
                CreateDateTime = now,
                CreateBy = actor.UserId
            };

            _dbContext.HmdEligibilityAssessments.Add(row);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = await ProjectEligibility(_dbContext.HmdEligibilityAssessments.AsNoTracking().Where(x => x.Id == row.Id))
                .FirstAsync(cancellationToken);
            result.OutcomeName = HmdLabels.EligibilityOutcome(result.Outcome);
            return HmdResult<HmdEligibilityResponse>.Created(result);
        }

        // =================================================================
        // Akses vaskular
        // =================================================================

        public async Task<HmdResult<PagedResult<HmdVascularAccessResponse>>> GetVascularAccessesAsync(
            Guid episodeId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            if (!await EpisodeExistsAsync(episodeId, cancellationToken))
                return HmdResult<PagedResult<HmdVascularAccessResponse>>.NotFound(EpisodeNotFoundMessage);

            (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(pageNumber, pageSize);
            var rows = _dbContext.HmdVascularAccesses.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.CreateDateTime);

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectAccess(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            items.ForEach(LabelAccess);

            return HmdResult<PagedResult<HmdVascularAccessResponse>>.Ok(Page(items, total, pageNumber, pageSize));
        }

        public async Task<HmdResult<HmdVascularAccessResponse>> CreateVascularAccessAsync(
            Guid episodeId,
            CreateHmdVascularAccessRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var episodeGuard = await GuardOpenEpisodeAsync<HmdVascularAccessResponse>(episodeId, cancellationToken);
            if (episodeGuard != null)
                return episodeGuard;

            var now = DateTime.UtcNow;

            if (request.IsPrimary)
                await ClearPrimaryAccessAsync(episodeId, null, actorUserId, now, cancellationToken);

            var access = new HmdVascularAccess
            {
                EpisodeId = episodeId,
                AccessType = request.AccessType,
                AccessSite = request.AccessSite.Trim(),
                AccessStatus = request.AccessStatus,
                IsPrimary = request.IsPrimary,
                EstablishedDate = request.EstablishedDate,
                LastAssessedAt = now,
                ConditionNote = HmdServiceSupport.Normalize(request.ConditionNote),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdVascularAccesses.Add(access);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return HmdResult<HmdVascularAccessResponse>.Created(await GetAccessAsync(access.Id, cancellationToken));
        }

        public async Task<HmdResult<HmdVascularAccessResponse>> ChangeVascularAccessStatusAsync(
            Guid accessId,
            ChangeHmdVascularAccessStatusRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var access = await _dbContext.HmdVascularAccesses.FirstOrDefaultAsync(x => x.Id == accessId && !x.IsDelete, cancellationToken);
            if (access == null)
                return HmdResult<HmdVascularAccessResponse>.NotFound("Akses vaskular tidak ditemukan atau sudah dihapus.");

            var episodeGuard = await GuardOpenEpisodeAsync<HmdVascularAccessResponse>(access.EpisodeId, cancellationToken);
            if (episodeGuard != null)
                return episodeGuard;

            var now = DateTime.UtcNow;

            if (request.IsPrimary == true && !access.IsPrimary)
                await ClearPrimaryAccessAsync(access.EpisodeId, access.Id, actorUserId, now, cancellationToken);

            access.AccessStatus = request.AccessStatus;
            if (request.IsPrimary.HasValue)
                access.IsPrimary = request.IsPrimary.Value;
            if (request.ConditionNote != null)
                access.ConditionNote = HmdServiceSupport.Normalize(request.ConditionNote);
            access.LastAssessedAt = now;
            access.UpdateDateTime = now;
            access.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return HmdResult<HmdVascularAccessResponse>.Ok(await GetAccessAsync(access.Id, cancellationToken));
        }

        // =================================================================
        // Serologi
        // =================================================================

        public async Task<HmdResult<PagedResult<HmdSerologyReviewResponse>>> GetSerologyReviewsAsync(
            Guid episodeId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            if (!await EpisodeExistsAsync(episodeId, cancellationToken))
                return HmdResult<PagedResult<HmdSerologyReviewResponse>>.NotFound(EpisodeNotFoundMessage);

            (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(pageNumber, pageSize);
            var rows = _dbContext.HmdSerologyReviews.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderByDescending(x => x.ResultDate).ThenBy(x => x.TestType);

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectSerology(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            items.ForEach(LabelSerology);

            return HmdResult<PagedResult<HmdSerologyReviewResponse>>.Ok(Page(items, total, pageNumber, pageSize));
        }

        /// <summary>
        /// Mencatat rujukan hasil serologi dan tinjauannya. Hasil laboratorium tidak pernah disalin
        /// sebagai sumber kebenaran baru; yang disimpan rujukan dan ringkasan yang dibaca petugas.
        /// </summary>
        public async Task<HmdResult<HmdSerologyReviewResponse>> CreateSerologyReviewAsync(
            Guid episodeId,
            CreateHmdSerologyReviewRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var episodeGuard = await GuardOpenEpisodeAsync<HmdSerologyReviewResponse>(episodeId, cancellationToken);
            if (episodeGuard != null)
                return episodeGuard;

            var now = DateTime.UtcNow;
            var review = new HmdSerologyReview
            {
                EpisodeId = episodeId,
                TestType = request.TestType,
                LabExaminationId = request.LabExaminationId,
                ResultDate = request.ResultDate,
                ResultFlag = request.ResultFlag,
                ResultSummary = HmdServiceSupport.Normalize(request.ResultSummary),
                ReviewStatus = request.IsReviewed ? HmdSerologyReviewStatus.Reviewed : HmdSerologyReviewStatus.PendingReview,
                ReviewedByUserId = request.IsReviewed ? actorUserId : null,
                ReviewedAt = request.IsReviewed ? now : null,
                ReviewNote = HmdServiceSupport.Normalize(request.ReviewNote),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdSerologyReviews.Add(review);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = await ProjectSerology(_dbContext.HmdSerologyReviews.AsNoTracking().Where(x => x.Id == review.Id))
                .FirstAsync(cancellationToken);
            LabelSerology(result);
            return HmdResult<HmdSerologyReviewResponse>.Created(result);
        }

        // =================================================================
        // Isolasi
        // =================================================================

        public async Task<HmdResult<PagedResult<HmdIsolationDecisionResponse>>> GetIsolationDecisionsAsync(
            Guid episodeId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            if (!await EpisodeExistsAsync(episodeId, cancellationToken))
                return HmdResult<PagedResult<HmdIsolationDecisionResponse>>.NotFound(EpisodeNotFoundMessage);

            (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(pageNumber, pageSize);
            var rows = _dbContext.HmdIsolationDecisions.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderByDescending(x => x.DecidedAt);

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectIsolation(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            items.ForEach(x => x.RequirementName = HmdLabels.Isolation(x.Requirement));

            return HmdResult<PagedResult<HmdIsolationDecisionResponse>>.Ok(Page(items, total, pageNumber, pageSize));
        }

        /// <summary>
        /// Menetapkan kebutuhan isolasi pasien. Keputusan baru menggantikan keputusan yang sedang
        /// berlaku; keputusan lama tetap tersimpan sebagai riwayat.
        /// </summary>
        /// <remarks>
        /// <b>Contoh.</b> Anti-HCV pasien reaktif dan tim PPI menetapkan pasien memerlukan mesin
        /// khusus. Keputusan tersimpan pada episode dan sejak itu dibaca gerbang penjadwalan: mesin
        /// umum ditolak <c>422 HMD-VAL-035</c>.
        /// </remarks>
        public async Task<HmdResult<HmdIsolationDecisionResponse>> CreateIsolationDecisionAsync(
            Guid episodeId,
            CreateHmdIsolationDecisionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var episodeGuard = await GuardOpenEpisodeAsync<HmdIsolationDecisionResponse>(episodeId, cancellationToken);
            if (episodeGuard != null)
                return episodeGuard;

            if (request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom)
                return HmdResult<HmdIsolationDecisionResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Tanggal akhir berlaku tidak boleh mendahului tanggal mulai berlaku.");

            if (request.SerologyReviewId.HasValue &&
                !await _dbContext.HmdSerologyReviews.AsNoTracking().AnyAsync(x =>
                    x.Id == request.SerologyReviewId.Value && x.EpisodeId == episodeId && !x.IsDelete, cancellationToken))
            {
                return HmdResult<HmdIsolationDecisionResponse>.Invalid(
                    HmdErrorCodes.InvalidRequest, "Tinjauan serologi yang dirujuk bukan milik episode ini.");
            }

            var now = DateTime.UtcNow;

            var previous = await _dbContext.HmdIsolationDecisions
                .Where(x => x.EpisodeId == episodeId && x.IsActive && !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var old in previous)
            {
                old.IsActive = false;
                if (!old.EffectiveTo.HasValue || old.EffectiveTo.Value >= request.EffectiveFrom)
                    old.EffectiveTo = request.EffectiveFrom.AddDays(-1) < old.EffectiveFrom ? old.EffectiveFrom : request.EffectiveFrom.AddDays(-1);
                old.UpdateDateTime = now;
                old.UpdateBy = actorUserId;
            }

            var decision = new HmdIsolationDecision
            {
                EpisodeId = episodeId,
                Requirement = request.Requirement,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                IsActive = true,
                DecidedByUserId = actorUserId,
                DecidedAt = now,
                Reason = request.Reason.Trim(),
                SerologyReviewId = request.SerologyReviewId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdIsolationDecisions.Add(decision);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = await ProjectIsolation(_dbContext.HmdIsolationDecisions.AsNoTracking().Where(x => x.Id == decision.Id))
                .FirstAsync(cancellationToken);
            result.RequirementName = HmdLabels.Isolation(result.Requirement);
            return HmdResult<HmdIsolationDecisionResponse>.Created(result);
        }

        // =================================================================
        // Penolong
        // =================================================================

        private const string EpisodeNotFoundMessage = "Episode hemodialisa tidak ditemukan atau sudah dihapus.";

        private static HmdResult<HmdEpisodeDetailResponse> NotFound() =>
            HmdResult<HmdEpisodeDetailResponse>.NotFound(EpisodeNotFoundMessage);

        private Task<bool> EpisodeExistsAsync(Guid episodeId, CancellationToken cancellationToken) =>
            _dbContext.HmdEpisodes.AsNoTracking().AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

        /// <summary>Episode wajib ada dan belum ditutup sebelum data klinisnya boleh ditambah.</summary>
        private async Task<HmdResult<T>?> GuardOpenEpisodeAsync<T>(Guid episodeId, CancellationToken cancellationToken)
        {
            var status = await _dbContext.HmdEpisodes.AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => (HmdEpisodeStatus?)x.EpisodeStatus)
                .FirstOrDefaultAsync(cancellationToken);

            if (!status.HasValue)
                return HmdResult<T>.NotFound(EpisodeNotFoundMessage);

            if (status.Value == HmdEpisodeStatus.Closed)
                return HmdResult<T>.Rule(HmdErrorCodes.InvalidTransition, "Episode sudah ditutup; data klinisnya tidak dapat ditambah atau diubah.");

            return null;
        }

        private async Task ClearPrimaryAccessAsync(Guid episodeId, Guid? exceptId, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
        {
            var primaries = await _dbContext.HmdVascularAccesses
                .Where(x => x.EpisodeId == episodeId && x.IsPrimary && !x.IsDelete && (!exceptId.HasValue || x.Id != exceptId.Value))
                .ToListAsync(cancellationToken);

            foreach (var item in primaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private static void Touch(HmdEpisode episode, Guid actorUserId)
        {
            episode.UpdateDateTime = DateTime.UtcNow;
            episode.UpdateBy = actorUserId;
            episode.Version++;
        }

        private async Task<HmdResult<HmdEpisodeDetailResponse>> SaveAndReturnAsync(Guid id, CancellationToken cancellationToken)
        {
            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdEpisodeDetailResponse>.From(failure);

            return HmdResult<HmdEpisodeDetailResponse>.Ok((await GetDetailAsync(id, cancellationToken))!);
        }

        private async Task<HmdVascularAccessResponse> GetAccessAsync(Guid id, CancellationToken cancellationToken)
        {
            var row = await ProjectAccess(_dbContext.HmdVascularAccesses.AsNoTracking().Where(x => x.Id == id)).FirstAsync(cancellationToken);
            LabelAccess(row);
            return row;
        }

        private static PagedResult<T> Page<T>(List<T> items, int total, int pageNumber, int pageSize) => new()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
            Items = items
        };

        private static IQueryable<HmdEligibilityResponse> ProjectEligibility(IQueryable<HmdEligibilityAssessment> rows) =>
            rows.Select(x => new HmdEligibilityResponse
            {
                Id = x.Id,
                EpisodeId = x.EpisodeId,
                AssessedByDoctorId = x.AssessedByDoctorId,
                AssessedByDoctorName = x.AssessedByDoctor != null ? x.AssessedByDoctor.FullName : null,
                AssessedAt = x.AssessedAt,
                Outcome = x.Outcome,
                IndicationSummary = x.IndicationSummary,
                DecisionReason = x.DecisionReason,
                FollowUpInstruction = x.FollowUpInstruction
            });

        private static IQueryable<HmdVascularAccessResponse> ProjectAccess(IQueryable<HmdVascularAccess> rows) =>
            rows.Select(x => new HmdVascularAccessResponse
            {
                Id = x.Id,
                EpisodeId = x.EpisodeId,
                AccessType = x.AccessType,
                AccessSite = x.AccessSite,
                AccessStatus = x.AccessStatus,
                IsPrimary = x.IsPrimary,
                EstablishedDate = x.EstablishedDate,
                LastAssessedAt = x.LastAssessedAt,
                ConditionNote = x.ConditionNote
            });

        private static void LabelAccess(HmdVascularAccessResponse row)
        {
            row.AccessTypeName = HmdLabels.AccessType(row.AccessType);
            row.AccessStatusName = HmdLabels.AccessStatus(row.AccessStatus);
        }

        private static IQueryable<HmdSerologyReviewResponse> ProjectSerology(IQueryable<HmdSerologyReview> rows) =>
            rows.Select(x => new HmdSerologyReviewResponse
            {
                Id = x.Id,
                EpisodeId = x.EpisodeId,
                TestType = x.TestType,
                LabExaminationId = x.LabExaminationId,
                ResultDate = x.ResultDate,
                ResultFlag = x.ResultFlag,
                ResultSummary = x.ResultSummary,
                ReviewStatus = x.ReviewStatus,
                ReviewedByUserId = x.ReviewedByUserId,
                ReviewedAt = x.ReviewedAt,
                ReviewNote = x.ReviewNote
            });

        private static void LabelSerology(HmdSerologyReviewResponse row)
        {
            row.TestTypeName = HmdLabels.SerologyTest(row.TestType);
            row.ResultFlagName = HmdLabels.SerologyResult(row.ResultFlag);
            row.ReviewStatusName = HmdLabels.SerologyReview(row.ReviewStatus);
        }

        private IQueryable<HmdIsolationDecisionResponse> ProjectIsolation(IQueryable<HmdIsolationDecision> rows) =>
            rows.Select(x => new HmdIsolationDecisionResponse
            {
                Id = x.Id,
                EpisodeId = x.EpisodeId,
                Requirement = x.Requirement,
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                IsActive = x.IsActive,
                DecidedByUserId = x.DecidedByUserId,
                DecidedByName = _dbContext.Users.Where(u => u.Id == x.DecidedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                DecidedAt = x.DecidedAt,
                Reason = x.Reason,
                SerologyReviewId = x.SerologyReviewId
            });

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
        }
    }
}
