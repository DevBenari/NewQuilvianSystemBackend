using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Penjadwalan sesi HD, penugasan petugas, dan daftar kerja unit (<c>BE-HMD-10</c>,
    /// <c>BE-HMD-11</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Tiga tabrakan diperiksa di dalam satu transaksi berkunci.</b> Sebelum memeriksa, service
    /// mengambil <c>pg_advisory_xact_lock</c> atas mesin, station, dan pasien yang diminta. Dua
    /// koordinator yang merebut mesin yang sama pada saat bersamaan karena itu diantrekan: yang
    /// pertama tersimpan, yang kedua baru memeriksa setelahnya dan melihat jadwal pertama — lalu
    /// ditolak <c>409</c>. Tidak pernah ada dua pasien terjadwal pada mesin yang sama.
    /// </para>
    /// <para>
    /// <b>Contoh berangka.</b> Pasien B dijadwalkan ke mesin <c>M-01</c> pukul 07.00–11.00.
    /// Pasien D diminta ke <c>M-01</c> pukul 09.00–13.00 pada tanggal yang sama. Rentang
    /// <c>[07.00, 11.00)</c> dan <c>[09.00, 13.00)</c> bertumpang tindih dua jam, sehingga permintaan
    /// kedua ditolak <c>409 HMD-VAL-032</c>. Jadwal 11.00–15.00 diterima, karena batas akhir
    /// tidak dihitung tumpang tindih.
    /// </para>
    /// <para>
    /// <b>Isolasi.</b> Pasien dengan keputusan isolasi aktif hanya boleh memakai mesin yang
    /// dikhususkan untuk kebutuhan yang sama dan station isolasi (<c>422 HMD-VAL-035</c>). Mesin
    /// yang dikhususkan sebaliknya tidak dipakai pasien tanpa kebutuhan yang sama, karena mesin
    /// khusus itu justru sumber risiko kontaminasi silang.
    /// </para>
    /// </remarks>
    public class HmdScheduleService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly HmdCompetencyGateService _competencyGateService;

        public HmdScheduleService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            HmdCompetencyGateService competencyGateService)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _competencyGateService = competencyGateService;
        }

        // =================================================================
        // Daftar kerja
        // =================================================================

        public static HmdWorklistFilterMetadataResponse BuildWorklistFilterMetadata() => new()
        {
            ShiftOptions = HmdLabels.Options<HmdShift>(HmdLabels.Shift),
            SessionStatusOptions = HmdLabels.Options<HmdSessionStatus>(HmdLabels.SessionStatus),
            StaffRoleOptions = HmdLabels.Options<HmdStaffRole>(HmdLabels.StaffRole),
            PageSizeOptions = HmdLabels.PageSizeOptions()
        };

        public async Task<HmdWorklistSummaryResponse> GetWorklistSummaryAsync(
            DateOnly? date, Guid? serviceUnitId, CancellationToken cancellationToken)
        {
            var day = date ?? HmdServiceSupport.TodayLocal();
            var rows = _dbContext.HmdSessions.AsNoTracking().Where(x => !x.IsDelete && x.ScheduledDate == day);
            if (serviceUnitId.HasValue)
                rows = rows.Where(x => x.Episode != null && x.Episode.ServiceUnitId == serviceUnitId.Value);

            return new HmdWorklistSummaryResponse
            {
                Date = day,
                TotalSession = await rows.CountAsync(cancellationToken),
                ScheduledSession = await rows.CountAsync(x => x.SessionStatus == HmdSessionStatus.Planned || x.SessionStatus == HmdSessionStatus.Scheduled, cancellationToken),
                PreparingSession = await rows.CountAsync(x =>
                    x.SessionStatus == HmdSessionStatus.CheckedIn || x.SessionStatus == HmdSessionStatus.PreCheck ||
                    x.SessionStatus == HmdSessionStatus.Held || x.SessionStatus == HmdSessionStatus.Ready, cancellationToken),
                InProgressSession = await rows.CountAsync(x => x.SessionStatus == HmdSessionStatus.InProgress, cancellationToken),
                AwaitingDocumentationSession = await rows.CountAsync(x => x.SessionStatus == HmdSessionStatus.Completed || x.SessionStatus == HmdSessionStatus.Stopped, cancellationToken),
                AwaitingFinalizationSession = await rows.CountAsync(x => x.SessionStatus == HmdSessionStatus.AwaitingFinalization, cancellationToken),
                FinalizedSession = await rows.CountAsync(x => x.SessionStatus == HmdSessionStatus.Finalized, cancellationToken),
                CancelledSession = await rows.CountAsync(x => x.SessionStatus == HmdSessionStatus.Cancelled, cancellationToken)
            };
        }

        /// <summary>
        /// Daftar kerja unit pada satu tanggal dan shift. Tidak memuat diagnosis maupun serologi;
        /// isolasi hanya berupa penanda <c>true</c>/<c>false</c>.
        /// </summary>
        public async Task<PagedResult<HmdWorklistItemResponse>> GetWorklistAsync(HmdWorklistQuery query, CancellationToken cancellationToken)
        {
            var (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);
            var day = query.Date ?? HmdServiceSupport.TodayLocal();

            var rows = _dbContext.HmdSessions.AsNoTracking().Where(x => !x.IsDelete && x.ScheduledDate == day);

            if (query.Shift.HasValue)
                rows = rows.Where(x => x.Shift == query.Shift.Value);
            if (query.SessionStatus.HasValue)
                rows = rows.Where(x => x.SessionStatus == query.SessionStatus.Value);
            if (query.ServiceUnitId.HasValue)
                rows = rows.Where(x => x.Episode != null && x.Episode.ServiceUnitId == query.ServiceUnitId.Value);
            if (query.NeedsDocumentation == true)
            {
                rows = rows.Where(x =>
                    x.SessionStatus == HmdSessionStatus.Completed ||
                    x.SessionStatus == HmdSessionStatus.Stopped ||
                    x.SessionStatus == HmdSessionStatus.AwaitingFinalization);
            }
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x =>
                    x.SessionNumber.ToLower().Contains(keyword) ||
                    (x.Episode != null && x.Episode.Patient != null && x.Episode.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Episode != null && x.Episode.Patient != null && x.Episode.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            rows = rows.OrderBy(x => x.ScheduledStartAt).ThenBy(x => x.Station!.StationCode);

            var total = await rows.CountAsync(cancellationToken);
            var items = await rows
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HmdWorklistItemResponse
                {
                    SessionId = x.Id,
                    SessionNumber = x.SessionNumber,
                    EpisodeId = x.EpisodeId,
                    PatientId = x.Episode != null ? x.Episode.PatientId : Guid.Empty,
                    PatientName = x.Episode != null && x.Episode.Patient != null ? x.Episode.Patient.FullName : null,
                    MedicalRecordNumber = x.Episode != null && x.Episode.Patient != null ? x.Episode.Patient.MedicalRecordNumber : null,
                    ScheduledDate = x.ScheduledDate,
                    Shift = x.Shift,
                    ScheduledStartAt = x.ScheduledStartAt,
                    ScheduledEndAt = x.ScheduledEndAt,
                    MachineId = x.MachineId,
                    MachineCode = x.Machine != null ? x.Machine.MachineCode : null,
                    StationId = x.StationId,
                    StationCode = x.Station != null ? x.Station.StationCode : null,
                    ResponsibleDoctorId = x.ResponsibleDoctorId,
                    ResponsibleDoctorName = x.ResponsibleDoctor != null ? x.ResponsibleDoctor.FullName : null,
                    PrimaryNurseName = x.StaffAssignments
                        .Where(s => !s.IsDelete && s.StaffRole == HmdStaffRole.PrimaryNurse)
                        .Select(s => s.WorkforceProfile != null ? s.WorkforceProfile.DisplayName : null)
                        .FirstOrDefault(),
                    SessionStatus = x.SessionStatus,
                    IsIsolationRequired = x.Episode != null && x.Episode.IsolationDecisions.Any(d =>
                        d.IsActive && !d.IsDelete && d.Requirement != HmdIsolationRequirement.None &&
                        d.EffectiveFrom <= day && (d.EffectiveTo == null || d.EffectiveTo >= day)),
                    HasUnverifiedCompetency = x.StaffAssignments.Any(s =>
                        !s.IsDelete && s.CompetencyVerificationStatus != HmdCompetencyVerificationStatus.Verified),
                    NeedsDocumentation =
                        x.SessionStatus == HmdSessionStatus.Completed ||
                        x.SessionStatus == HmdSessionStatus.Stopped ||
                        x.SessionStatus == HmdSessionStatus.AwaitingFinalization
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.ShiftName = HmdLabels.Shift(item.Shift);
                item.SessionStatusName = HmdLabels.SessionStatus(item.SessionStatus);
            }

            return new PagedResult<HmdWorklistItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            };
        }

        // =================================================================
        // Penjadwalan
        // =================================================================

        /// <summary>
        /// Menjadwalkan sesi baru. Sesi langsung berstatus <c>Scheduled</c> karena mesin dan station
        /// wajib ditetapkan pada permintaan yang sama, dan menautkan resep aktif pasien saat itu.
        /// </summary>
        public async Task<HmdResult<HmdSessionResponse>> CreateAsync(
            CreateHmdSessionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var startUtc = HmdServiceSupport.ToUtc(request.ScheduledStartAt);
            var endUtc = HmdServiceSupport.ToUtc(request.ScheduledEndAt);

            var windowError = ValidateWindow(request.ScheduledDate, startUtc, endUtc);
            if (windowError != null)
                return HmdResult<HmdSessionResponse>.From(windowError);

            var episode = await _dbContext.HmdEpisodes.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.EpisodeId && !x.IsDelete, cancellationToken);
            if (episode == null)
                return HmdResult<HmdSessionResponse>.NotFound("Episode hemodialisa tidak ditemukan atau sudah dihapus.");

            if (request.PatientId.HasValue && request.PatientId.Value != episode.PatientId)
                return HmdResult<HmdSessionResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Pasien yang dikirim bukan pemilik episode ini.");

            var prescriptionId = await _dbContext.HmdPrescriptions.AsNoTracking()
                .Where(x => x.EpisodeId == episode.Id && !x.IsDelete && x.PrescriptionStatus == HmdPrescriptionStatus.Active)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (episode.EpisodeStatus != HmdEpisodeStatus.Active || !prescriptionId.HasValue)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val030, HmdMessages.Val030);

            var setting = await HmdServiceSupport.FindSettingAsync(_dbContext, episode.ServiceUnitId, cancellationToken);
            if (setting == null)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.SettingMissing, HmdMessages.SettingMissing);

            var resourceError = await ValidateResourcesAsync(episode, request.MachineId, request.StationId, request.ScheduledDate, cancellationToken);
            if (resourceError != null)
                return HmdResult<HmdSessionResponse>.From(resourceError);

            var contextError = await ValidateContextAsync(
                episode, request.EncounterId, request.InpEpisodeId, request.ResponsibleDoctorId, request.OrderId, cancellationToken);
            if (contextError != null)
                return HmdResult<HmdSessionResponse>.From(contextError);

            var warnings = new List<string>();
            var staffPlan = await PlanStaffAsync(request.Staff ?? new List<HmdStaffAssignmentInput>(), setting, request.ScheduledDate, request.Shift, null, warnings, cancellationToken);
            if (!staffPlan.IsSuccess)
                return HmdResult<HmdSessionResponse>.From(staffPlan);

            var sessionNumber = await HmdServiceSupport.AllocateNumberAsync(
                _numberSeriesAllocator, HmdServiceSupport.SessionSequenceKey, HmdServiceSupport.SessionNumberPrefix, actorUserId, cancellationToken);
            if (sessionNumber == null)
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.NumberAllocationFailed, HmdMessages.NumberAllocationFailed);

            var now = DateTime.UtcNow;
            var session = new HmdSession
            {
                SessionNumber = sessionNumber,
                EpisodeId = episode.Id,
                PrescriptionId = prescriptionId.Value,
                EncounterId = request.EncounterId,
                InpEpisodeId = request.InpEpisodeId,
                OrderId = request.OrderId,
                MachineId = request.MachineId,
                StationId = request.StationId,
                ResponsibleDoctorId = request.ResponsibleDoctorId,
                ScheduledDate = request.ScheduledDate,
                Shift = request.Shift,
                ScheduledStartAt = startUtc,
                ScheduledEndAt = endUtc,
                SessionStatus = HmdSessionStatus.Scheduled,
                BillingHandoffStatus = HmdBillingHandoffStatus.NotRequired,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await HmdServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken,
                $"HMD_MACHINE_{request.MachineId:N}",
                $"HMD_STATION_{request.StationId:N}",
                $"HMD_PATIENT_{episode.PatientId:N}");

            var collision = await CheckCollisionsAsync(episode.PatientId, request.MachineId, request.StationId, startUtc, endUtc, null, cancellationToken);
            if (collision != null)
                return HmdResult<HmdSessionResponse>.From(collision);

            // Status mesin dibaca ulang di dalam kunci: mesin yang diblokir teknisi di antara
            // pemeriksaan awal dan penyimpanan ini tetap tertolak.
            if (!await _dbContext.HmdMachines.AsNoTracking().AnyAsync(x =>
                    x.Id == request.MachineId && !x.IsDelete && x.IsActive && x.IsSchedulable && x.MachineStatus == HmdMachineStatus.Ready,
                    cancellationToken))
            {
                return HmdResult<HmdSessionResponse>.Rule(HmdErrorCodes.Val034, HmdMessages.Val034);
            }

            _dbContext.HmdSessions.Add(session);

            foreach (var staff in staffPlan.Value!)
            {
                _dbContext.HmdSessionStaffAssignments.Add(new HmdSessionStaffAssignment
                {
                    SessionId = session.Id,
                    WorkforceProfileId = staff.WorkforceProfileId,
                    StaffRole = staff.StaffRole,
                    AssignedByUserId = actorUserId,
                    AssignedAt = now,
                    CompetencyVerificationStatus = staff.Competency.Status,
                    CompetencyCheckedAt = staff.Competency.CheckedAt,
                    CompetencySourceReference = staff.Competency.SourceReference,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            if (request.OrderId.HasValue)
            {
                var order = await _dbContext.HmdOrders.FirstAsync(x => x.Id == request.OrderId.Value, cancellationToken);
                if (order.OrderStatus != HmdOrderStatus.Accepted)
                    return HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.Val002, HmdMessages.Val002);

                order.OrderStatus = HmdOrderStatus.Fulfilled;
                order.EpisodeId ??= episode.Id;
                order.DecisionByUserId = actorUserId;
                order.DecisionAt = now;
                order.UpdateDateTime = now;
                order.UpdateBy = actorUserId;
                order.Version++;
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }

            var response = await GetSessionAsync(session.Id, cancellationToken);
            response!.Warnings = warnings;
            return HmdResult<HmdSessionResponse>.Created(response);
        }

        /// <summary>
        /// Mengubah tanggal, shift, mesin, station, atau dokter penanggung jawab sesi yang belum
        /// dimulai. Sesi yang sudah dinyatakan siap kembali ke persiapan Pra-HD bila jadwal atau
        /// sumber dayanya berubah, supaya kesiapannya dinyatakan ulang.
        /// </summary>
        public async Task<HmdResult<HmdSessionResponse>> RescheduleAsync(
            Guid id,
            RescheduleHmdSessionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var startUtc = HmdServiceSupport.ToUtc(request.ScheduledStartAt);
            var endUtc = HmdServiceSupport.ToUtc(request.ScheduledEndAt);

            var windowError = ValidateWindow(request.ScheduledDate, startUtc, endUtc);
            if (windowError != null)
                return HmdResult<HmdSessionResponse>.From(windowError);

            var session = await _dbContext.HmdSessions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (session == null)
                return SessionNotFound();

            var statusError = GuardReschedulable(session);
            if (statusError != null)
                return statusError;

            var episode = await _dbContext.HmdEpisodes.AsNoTracking().FirstAsync(x => x.Id == session.EpisodeId, cancellationToken);

            var resourceError = await ValidateResourcesAsync(episode, request.MachineId, request.StationId, request.ScheduledDate, cancellationToken);
            if (resourceError != null)
                return HmdResult<HmdSessionResponse>.From(resourceError);

            if (request.ResponsibleDoctorId.HasValue &&
                !await _dbContext.Set<MstDoctor>().AsNoTracking().AnyAsync(x => x.Id == request.ResponsibleDoctorId.Value && !x.IsDelete && x.IsActive, cancellationToken))
            {
                return HmdResult<HmdSessionResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Dokter penanggung jawab tidak ditemukan atau tidak aktif.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await HmdServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken,
                $"HMD_MACHINE_{request.MachineId:N}",
                $"HMD_STATION_{request.StationId:N}",
                $"HMD_PATIENT_{episode.PatientId:N}");

            var collision = await CheckCollisionsAsync(episode.PatientId, request.MachineId, request.StationId, startUtc, endUtc, id, cancellationToken);
            if (collision != null)
                return HmdResult<HmdSessionResponse>.From(collision);

            var resourceChanged =
                session.MachineId != request.MachineId ||
                session.StationId != request.StationId ||
                session.ScheduledStartAt != startUtc ||
                session.ScheduledEndAt != endUtc ||
                session.ScheduledDate != request.ScheduledDate ||
                session.Shift != request.Shift;

            var now = DateTime.UtcNow;
            session.ScheduledDate = request.ScheduledDate;
            session.Shift = request.Shift;
            session.ScheduledStartAt = startUtc;
            session.ScheduledEndAt = endUtc;
            session.MachineId = request.MachineId;
            session.StationId = request.StationId;
            if (request.ResponsibleDoctorId.HasValue)
                session.ResponsibleDoctorId = request.ResponsibleDoctorId.Value;

            if (resourceChanged && session.SessionStatus == HmdSessionStatus.Ready)
            {
                session.SessionStatus = HmdSessionStatus.PreCheck;
                session.ReadyAt = null;
                session.ReadyByUserId = null;
            }

            session.UpdateDateTime = now;
            session.UpdateBy = actorUserId;
            session.Version++;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<HmdSessionResponse>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }

            return HmdResult<HmdSessionResponse>.Ok((await GetSessionAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Membatalkan sesi sebelum dimulai. Sesi yang sudah berjalan tidak dapat dibatalkan — yang
        /// tersedia adalah penghentian.
        /// </summary>
        public async Task<HmdResult<HmdSessionResponse>> CancelAsync(
            Guid id,
            CancelHmdSessionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdSessionResponse>.Invalid(HmdErrorCodes.Val080, HmdMessages.Val080);

            var session = await _dbContext.HmdSessions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (session == null)
                return SessionNotFound();

            if (session.SessionStatus == HmdSessionStatus.InProgress)
            {
                return HmdResult<HmdSessionResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    "Cuci darah yang sudah berjalan tidak dapat dibatalkan. Gunakan penghentian sesi.");
            }

            var statusError = GuardReschedulable(session);
            if (statusError != null)
                return statusError;

            var now = DateTime.UtcNow;
            session.SessionStatus = HmdSessionStatus.Cancelled;
            session.CancelReason = reason;
            session.CancelDateTime = now;
            session.CancelBy = actorUserId;
            session.UpdateDateTime = now;
            session.UpdateBy = actorUserId;
            session.Version++;

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdSessionResponse>.From(failure);

            return HmdResult<HmdSessionResponse>.Ok((await GetSessionAsync(id, cancellationToken))!);
        }

        // =================================================================
        // Penugasan petugas
        // =================================================================

        public async Task<HmdResult<List<HmdStaffAssignmentResponse>>> GetStaffAssignmentsAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.HmdSessions.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
                return HmdResult<List<HmdStaffAssignmentResponse>>.NotFound(SessionNotFoundMessage);

            return HmdResult<List<HmdStaffAssignmentResponse>>.Ok(await ReadStaffAsync(_dbContext, id, cancellationToken));
        }

        /// <summary>
        /// Menetapkan dokter penanggung jawab dan petugas sesi. Setiap penugasan membawa hasil
        /// pemeriksaan kewenangan; selama HR belum tersedia nilainya <c>NotVerifiable</c> dan
        /// penugasan tetap diterima karena <c>EnforceCompetencyCheck</c> bawaannya mati.
        /// </summary>
        public async Task<HmdResult<List<HmdStaffAssignmentResponse>>> AssignStaffAsync(
            Guid id,
            AssignHmdStaffRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var session = await _dbContext.HmdSessions
                .Include(x => x.StaffAssignments.Where(s => !s.IsDelete))
                .Include(x => x.Episode)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (session == null)
                return HmdResult<List<HmdStaffAssignmentResponse>>.NotFound(SessionNotFoundMessage);

            if (session.SessionStatus == HmdSessionStatus.Finalized)
                return HmdResult<List<HmdStaffAssignmentResponse>>.Locked(HmdErrorCodes.Val075, HmdMessages.Val075);

            if (session.SessionStatus is HmdSessionStatus.AwaitingFinalization or HmdSessionStatus.Cancelled)
            {
                return HmdResult<List<HmdStaffAssignmentResponse>>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    $"Petugas sesi berstatus {HmdLabels.SessionStatus(session.SessionStatus).ToLowerInvariant()} tidak dapat diubah.");
            }

            if (request.ResponsibleDoctorId.HasValue &&
                !await _dbContext.Set<MstDoctor>().AsNoTracking().AnyAsync(x => x.Id == request.ResponsibleDoctorId.Value && !x.IsDelete && x.IsActive, cancellationToken))
            {
                return HmdResult<List<HmdStaffAssignmentResponse>>.Invalid(HmdErrorCodes.InvalidRequest, "Dokter penanggung jawab tidak ditemukan atau tidak aktif.");
            }

            var setting = await HmdServiceSupport.FindSettingAsync(_dbContext, session.Episode!.ServiceUnitId, cancellationToken);
            if (setting == null)
                return HmdResult<List<HmdStaffAssignmentResponse>>.Rule(HmdErrorCodes.SettingMissing, HmdMessages.SettingMissing);

            var warnings = new List<string>();
            var plan = await PlanStaffAsync(request.Staff ?? new List<HmdStaffAssignmentInput>(), setting, session.ScheduledDate, session.Shift, id, warnings, cancellationToken);
            if (!plan.IsSuccess)
                return HmdResult<List<HmdStaffAssignmentResponse>>.From(plan);

            var now = DateTime.UtcNow;
            var wanted = plan.Value!.ToDictionary(x => x.WorkforceProfileId);

            foreach (var existing in session.StaffAssignments.ToList())
            {
                if (wanted.TryGetValue(existing.WorkforceProfileId, out var keep))
                {
                    existing.StaffRole = keep.StaffRole;
                    existing.CompetencyVerificationStatus = keep.Competency.Status;
                    existing.CompetencyCheckedAt = keep.Competency.CheckedAt;
                    existing.CompetencySourceReference = keep.Competency.SourceReference;
                    existing.UpdateDateTime = now;
                    existing.UpdateBy = actorUserId;
                    wanted.Remove(existing.WorkforceProfileId);
                }
                else
                {
                    // Dilepas dengan penandaan, bukan dihapus fisik, supaya riwayat penugasan tetap terbaca.
                    existing.IsDelete = true;
                    existing.DeleteDateTime = now;
                    existing.DeleteBy = actorUserId;
                }
            }

            foreach (var staff in wanted.Values)
            {
                _dbContext.HmdSessionStaffAssignments.Add(new HmdSessionStaffAssignment
                {
                    SessionId = id,
                    WorkforceProfileId = staff.WorkforceProfileId,
                    StaffRole = staff.StaffRole,
                    AssignedByUserId = actorUserId,
                    AssignedAt = now,
                    CompetencyVerificationStatus = staff.Competency.Status,
                    CompetencyCheckedAt = staff.Competency.CheckedAt,
                    CompetencySourceReference = staff.Competency.SourceReference,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            if (request.ResponsibleDoctorId.HasValue)
                session.ResponsibleDoctorId = request.ResponsibleDoctorId.Value;

            session.UpdateDateTime = now;
            session.UpdateBy = actorUserId;
            session.Version++;

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<List<HmdStaffAssignmentResponse>>.From(failure);

            return HmdResult<List<HmdStaffAssignmentResponse>>.Ok(await ReadStaffAsync(_dbContext, id, cancellationToken));
        }

        public static async Task<List<HmdStaffAssignmentResponse>> ReadStaffAsync(
            ApplicationDbContext dbContext, Guid sessionId, CancellationToken cancellationToken)
        {
            var rows = await dbContext.HmdSessionStaffAssignments.AsNoTracking()
                .Where(x => x.SessionId == sessionId && !x.IsDelete)
                .OrderBy(x => x.StaffRole).ThenBy(x => x.AssignedAt)
                .Select(x => new HmdStaffAssignmentResponse
                {
                    Id = x.Id,
                    SessionId = x.SessionId,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                    StaffRole = x.StaffRole,
                    AssignedByUserId = x.AssignedByUserId,
                    AssignedAt = x.AssignedAt,
                    CompetencyVerificationStatus = x.CompetencyVerificationStatus,
                    CompetencyCheckedAt = x.CompetencyCheckedAt,
                    CompetencySourceReference = x.CompetencySourceReference
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                row.StaffRoleName = HmdLabels.StaffRole(row.StaffRole);
                row.CompetencyVerificationStatusName = HmdLabels.Competency(row.CompetencyVerificationStatus);
            }

            return rows;
        }

        // =================================================================
        // Penolong
        // =================================================================

        private const string SessionNotFoundMessage = "Sesi hemodialisa tidak ditemukan atau sudah dihapus.";

        private static HmdResult<HmdSessionResponse> SessionNotFound() =>
            HmdResult<HmdSessionResponse>.NotFound(SessionNotFoundMessage);

        private async Task<HmdSessionResponse?> GetSessionAsync(Guid id, CancellationToken cancellationToken)
        {
            var row = await HmdSessionProjection.Project(_dbContext.HmdSessions.AsNoTracking().Where(x => x.Id == id))
                .FirstOrDefaultAsync(cancellationToken);
            return row == null ? null : HmdSessionProjection.Label(row);
        }

        private static HmdResult<HmdSessionResponse>? GuardReschedulable(HmdSession session)
        {
            if (session.SessionStatus == HmdSessionStatus.Finalized)
                return HmdResult<HmdSessionResponse>.Locked(HmdErrorCodes.Val075, HmdMessages.Val075);

            if (!HmdSessionProjection.ReschedulableStatuses.Contains(session.SessionStatus))
            {
                return HmdResult<HmdSessionResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    $"Sesi berstatus {HmdLabels.SessionStatus(session.SessionStatus).ToLowerInvariant()} tidak dapat dijadwalkan ulang maupun dibatalkan.");
            }

            return null;
        }

        private static HmdResult<bool>? ValidateWindow(DateOnly scheduledDate, DateTime startUtc, DateTime endUtc)
        {
            if (endUtc <= startUtc)
                return HmdResult<bool>.Invalid(HmdErrorCodes.InvalidRequest, "Jam selesai terjadwal wajib setelah jam mulai terjadwal.");

            if (HmdServiceSupport.LocalDate(startUtc) != scheduledDate)
                return HmdResult<bool>.Invalid(HmdErrorCodes.InvalidRequest, "Tanggal jadwal tidak sama dengan tanggal jam mulai terjadwal.");

            if ((endUtc - startUtc).TotalHours > 24)
                return HmdResult<bool>.Invalid(HmdErrorCodes.InvalidRequest, "Rentang jadwal satu sesi tidak boleh lebih dari 24 jam.");

            return null;
        }

        /// <summary>
        /// Memeriksa mesin, station, dan kebutuhan isolasi — di luar kunci, sebagai penolakan cepat.
        /// Status mesin dibaca ulang di dalam kunci sebelum penyimpanan.
        /// </summary>
        private async Task<HmdResult<bool>?> ValidateResourcesAsync(
            HmdEpisode episode,
            Guid machineId,
            Guid stationId,
            DateOnly scheduledDate,
            CancellationToken cancellationToken)
        {
            var machine = await _dbContext.HmdMachines.AsNoTracking().FirstOrDefaultAsync(x => x.Id == machineId && !x.IsDelete, cancellationToken);
            if (machine == null || !machine.IsActive || !machine.IsSchedulable || machine.MachineStatus != HmdMachineStatus.Ready)
                return HmdResult<bool>.Rule(HmdErrorCodes.Val034, HmdMessages.Val034);

            var station = await _dbContext.HmdStations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == stationId && !x.IsDelete, cancellationToken);
            if (station == null || !station.IsActive || station.StationStatus != HmdStationStatus.Available)
                return HmdResult<bool>.Rule(HmdErrorCodes.Val034, "Station ini sedang tidak dapat digunakan. Pilih station lain.");

            if (machine.ServiceUnitId != episode.ServiceUnitId || station.ServiceUnitId != episode.ServiceUnitId)
                return HmdResult<bool>.Rule(HmdErrorCodes.InvalidRequest, "Mesin dan station wajib berada di unit hemodialisa yang melayani pasien.");

            var requirement = await HmdServiceSupport.GetActiveIsolationAsync(_dbContext, episode.Id, scheduledDate, cancellationToken);

            if (requirement != HmdIsolationRequirement.None &&
                (machine.DedicatedFor != requirement || !station.IsIsolationStation))
            {
                return HmdResult<bool>.Rule(HmdErrorCodes.Val035, HmdMessages.Val035);
            }

            if (requirement == HmdIsolationRequirement.None && machine.DedicatedFor != HmdIsolationRequirement.None)
            {
                return HmdResult<bool>.Rule(
                    HmdErrorCodes.Val035,
                    "Mesin ini dikhususkan untuk pasien dengan kebutuhan isolasi tertentu dan tidak dapat dipakai pasien ini.");
            }

            return null;
        }

        private async Task<HmdResult<bool>?> ValidateContextAsync(
            HmdEpisode episode,
            Guid? encounterId,
            Guid? inpEpisodeId,
            Guid? responsibleDoctorId,
            Guid? orderId,
            CancellationToken cancellationToken)
        {
            if (encounterId.HasValue)
            {
                var encounter = await HmdServiceSupport.CheckEncounterAsync(_dbContext, encounterId.Value, episode.PatientId, cancellationToken);
                if (!encounter.IsValid)
                    return HmdResult<bool>.Rule(HmdErrorCodes.Val036, HmdMessages.Val036);
            }

            if (inpEpisodeId.HasValue &&
                !await _dbContext.Set<InpEpisode>().AsNoTracking().AnyAsync(x => x.Id == inpEpisodeId.Value && x.PatientId == episode.PatientId && !x.IsDelete, cancellationToken))
            {
                return HmdResult<bool>.Invalid(HmdErrorCodes.InvalidRequest, "Episode rawat inap tidak ditemukan atau bukan milik pasien ini.");
            }

            if (responsibleDoctorId.HasValue &&
                !await _dbContext.Set<MstDoctor>().AsNoTracking().AnyAsync(x => x.Id == responsibleDoctorId.Value && !x.IsDelete && x.IsActive, cancellationToken))
            {
                return HmdResult<bool>.Invalid(HmdErrorCodes.InvalidRequest, "Dokter penanggung jawab tidak ditemukan atau tidak aktif.");
            }

            if (orderId.HasValue &&
                !await _dbContext.HmdOrders.AsNoTracking().AnyAsync(x =>
                    x.Id == orderId.Value && !x.IsDelete && x.PatientId == episode.PatientId && x.OrderStatus == HmdOrderStatus.Accepted,
                    cancellationToken))
            {
                return HmdResult<bool>.Rule(HmdErrorCodes.InvalidRequest, "Permintaan HD belum diterima unit atau bukan milik pasien ini.");
            }

            return null;
        }

        /// <summary>
        /// Tiga tabrakan: pasien, mesin, dan station pada rentang waktu yang bertumpang tindih.
        /// Tumpang tindih berarti <c>mulai_lain &lt; selesai_baru</c> dan <c>selesai_lain &gt; mulai_baru</c>;
        /// sesi yang dibatalkan tidak dihitung.
        /// </summary>
        private async Task<HmdResult<bool>?> CheckCollisionsAsync(
            Guid patientId,
            Guid machineId,
            Guid stationId,
            DateTime startUtc,
            DateTime endUtc,
            Guid? excludeSessionId,
            CancellationToken cancellationToken)
        {
            var overlapping = _dbContext.HmdSessions.AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.SessionStatus != HmdSessionStatus.Cancelled &&
                (!excludeSessionId.HasValue || x.Id != excludeSessionId.Value) &&
                x.ScheduledStartAt < endUtc &&
                x.ScheduledEndAt > startUtc);

            if (await overlapping.AnyAsync(x => x.Episode != null && x.Episode.PatientId == patientId, cancellationToken))
                return HmdResult<bool>.Conflict(HmdErrorCodes.Val031, HmdMessages.Val031);

            if (await overlapping.AnyAsync(x => x.MachineId == machineId, cancellationToken))
                return HmdResult<bool>.Conflict(HmdErrorCodes.Val032, HmdMessages.Val032);

            if (await overlapping.AnyAsync(x => x.StationId == stationId, cancellationToken))
                return HmdResult<bool>.Conflict(HmdErrorCodes.Val033, HmdMessages.Val033);

            return null;
        }

        private sealed record PlannedStaff(Guid WorkforceProfileId, HmdStaffRole StaffRole, HmdCompetencyResult Competency);

        /// <summary>
        /// Menyusun penugasan petugas beserta hasil pemeriksaan kewenangan dan rasio pasien per
        /// perawat. Penolakan hanya terjadi bila pengaturan penegakannya menyala; bila mati, keadaan
        /// yang sama menjadi peringatan.
        /// </summary>
        private async Task<HmdResult<List<PlannedStaff>>> PlanStaffAsync(
            List<HmdStaffAssignmentInput> input,
            HmdSetting setting,
            DateOnly scheduledDate,
            HmdShift shift,
            Guid? currentSessionId,
            List<string> warnings,
            CancellationToken cancellationToken)
        {
            var plan = new List<PlannedStaff>();

            if (input.Select(x => x.WorkforceProfileId).Distinct().Count() != input.Count)
                return HmdResult<List<PlannedStaff>>.Invalid(HmdErrorCodes.InvalidRequest, "Petugas yang sama tidak boleh ditugaskan dua kali pada satu sesi.");

            foreach (var staff in input)
            {
                var exists = await _dbContext.Set<MstWorkforceProfile>().AsNoTracking()
                    .AnyAsync(x => x.Id == staff.WorkforceProfileId && !x.IsDelete && x.IsActive, cancellationToken);
                if (!exists)
                    return HmdResult<List<PlannedStaff>>.Invalid(HmdErrorCodes.InvalidRequest, "Petugas yang dipilih tidak ditemukan atau tidak aktif.");

                var competency = await _competencyGateService.EvaluateAsync(staff.WorkforceProfileId, staff.StaffRole, cancellationToken);

                if (competency.Status == HmdCompetencyVerificationStatus.NotAuthorized)
                {
                    if (setting.EnforceCompetencyCheck)
                        return HmdResult<List<PlannedStaff>>.Rule(HmdErrorCodes.Val037, HmdMessages.Val037);

                    warnings.Add(HmdMessages.Val037);
                }
                else if (competency.Status == HmdCompetencyVerificationStatus.NotVerifiable)
                {
                    // Diterima dengan peringatan, apa pun nilai pengaturan penegakan
                    // (state-transition-matrix.md bagian 8).
                    if (!warnings.Contains(HmdMessages.WarningNotVerifiable))
                        warnings.Add(HmdMessages.WarningNotVerifiable);
                }

                if (staff.StaffRole is HmdStaffRole.PrimaryNurse or HmdStaffRole.AssistantNurse)
                {
                    var load = await _dbContext.HmdSessionStaffAssignments.AsNoTracking().CountAsync(x =>
                        x.WorkforceProfileId == staff.WorkforceProfileId &&
                        !x.IsDelete &&
                        (x.StaffRole == HmdStaffRole.PrimaryNurse || x.StaffRole == HmdStaffRole.AssistantNurse) &&
                        (!currentSessionId.HasValue || x.SessionId != currentSessionId.Value) &&
                        x.Session != null &&
                        !x.Session.IsDelete &&
                        x.Session.ScheduledDate == scheduledDate &&
                        x.Session.Shift == shift &&
                        x.Session.SessionStatus != HmdSessionStatus.Cancelled,
                        cancellationToken);

                    if (load + 1 > setting.MaxPatientsPerNurse)
                    {
                        if (setting.EnforceNurseRatio)
                            return HmdResult<List<PlannedStaff>>.Rule(HmdErrorCodes.Val038, HmdMessages.Val038);

                        if (!warnings.Contains(HmdMessages.WarningNurseRatio))
                            warnings.Add(HmdMessages.WarningNurseRatio);
                    }
                }

                plan.Add(new PlannedStaff(staff.WorkforceProfileId, staff.StaffRole, competency));
            }

            return HmdResult<List<PlannedStaff>>.Ok(plan);
        }

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
