using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using Status = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.DispensationStatus;
using DispensationTypes = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.DispensationType;
using PeriodStatus = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendancePeriodStatus;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    /// <summary>
    /// Dispensasi kehadiran (HRD-DEC-200, HRD-ASM-76, state 30.3, VR-PK-13 s.d. VR-PK-17): pegawai mengajukan untuk diri
    /// sendiri, Kepala Unit mencatat atas nama pegawai unitnya, dan HANYA atasan langsung yang ditetapkan server dari
    /// WfpManagerAssignment yang memutuskan. Tidak ada ID penyetuju dari klien; penyetuju tidak pernah sama dengan pegawai.
    /// Log mesin tidak disentuh; dispensasi yang disetujui memicu hitung ulang rekap harian (BE-HRD-354).
    /// </summary>
    public class AttendanceDispensationService
    {
        public const string RangeMessage = "Jam selesai harus sesudah jam mulai";
        public const string NoApproverSelfMessage = "Atasan langsung Anda belum ditetapkan. Hubungi HRD";
        public const string NoApproverOnBehalfMessage = "Atasan langsung pegawai ini belum ditetapkan. Hubungi HRD";
        public const string NotApproverMessage = "Anda tidak berwenang memutuskan dispensasi ini";
        public const string ForbiddenUnitMessage = AttendanceReportService.ForbiddenUnitMessage;
        public const string ConflictMessage = "Dispensasi sudah diubah pengguna lain. Muat ulang lalu coba lagi.";
        private static readonly TimeSpan WibOffset = TimeSpan.FromHours(7);
        private static readonly string[] MonthNames =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };

        public static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>
        {
            [Status.Submitted] = "Diajukan",
            [Status.Approved] = "Disetujui",
            [Status.Rejected] = "Ditolak",
            [Status.Cancelled] = "Dibatalkan"
        };

        public static readonly IReadOnlyDictionary<string, string> TypeLabels = new Dictionary<string, string>
        {
            [DispensationTypes.LateArrival] = "Datang terlambat",
            [DispensationTypes.EarlyDeparture] = "Pulang lebih awal"
        };

        private static readonly string[] ActiveStatuses = { Status.Submitted, Status.Approved };

        private readonly ApplicationDbContext _context;
        private readonly HrdUnitScopeService _unitScopeService;
        private readonly AttendanceSettingService _settingService;
        private readonly IUnifiedNotificationGatewayService _notificationGateway;
        private readonly AttendanceSchedulerService _schedulerService;
        private readonly ILogger<AttendanceDispensationService> _logger;

        public AttendanceDispensationService(
            ApplicationDbContext context,
            HrdUnitScopeService unitScopeService,
            AttendanceSettingService settingService,
            IUnifiedNotificationGatewayService notificationGateway,
            AttendanceSchedulerService schedulerService,
            ILogger<AttendanceDispensationService> logger)
        {
            _context = context;
            _unitScopeService = unitScopeService;
            _settingService = settingService;
            _notificationGateway = notificationGateway;
            _schedulerService = schedulerService;
            _logger = logger;
        }

        public async Task<DispensationFilterMetadataResponse> GetMetadataAsync(CancellationToken ct = default) => new()
        {
            StatusOptions = StatusLabels.Select(x => new DispensationOptionResponse { Value = x.Key, Label = x.Value }).ToList(),
            DispensationTypeOptions = TypeLabels.Select(x => new DispensationOptionResponse { Value = x.Key, Label = x.Value }).ToList(),
            PageSizeOptions = new List<int> { 10, 25, 50, 100 },
            MaxBackdatedDays = await _settingService.GetEffectiveMaxBackdatedCorrectionDaysAsync(cancellationToken: ct)
        };

        // ---------------------------------------------------------------- Pengajuan

        /// <summary>Pegawai mengajukan untuk dirinya sendiri; identitas dari sesi.</summary>
        public async Task<AttendanceDispensationServiceResult<DispensationResponse>> CreateMineAsync(
            Guid actorUserId, CreateMyDispensationRequest request, DateTime utcNow, CancellationToken ct = default)
        {
            var myProfileId = await ProfileOfAsync(actorUserId, ct);
            if (myProfileId == null) return Fail<DispensationResponse>(StatusCodes.Status403Forbidden, "Akun Anda belum terhubung dengan profil pegawai.");
            return await CreateAsync(actorUserId, myProfileId.Value, myProfileId.Value, request, utcNow, ct);
        }

        /// <summary>Kepala Unit mencatat atas nama pegawai pada unit yang ia kelola (cakupan kelola).</summary>
        public async Task<AttendanceDispensationServiceResult<DispensationResponse>> CreateOnBehalfAsync(
            Guid actorUserId, CreateDispensationRequest request, DateTime utcNow, CancellationToken ct = default)
        {
            if (request.WorkforceProfileId == Guid.Empty) return Fail<DispensationResponse>(StatusCodes.Status400BadRequest, "Pegawai wajib dipilih.");
            var myProfileId = await ProfileOfAsync(actorUserId, ct);
            return await CreateAsync(actorUserId, request.WorkforceProfileId, myProfileId, request, utcNow, ct);
        }

        private async Task<AttendanceDispensationServiceResult<DispensationResponse>> CreateAsync(
            Guid actorUserId, Guid employeeId, Guid? actorProfileId, CreateMyDispensationRequest request, DateTime utcNow, CancellationToken ct)
        {
            var type = (request.DispensationType ?? string.Empty).Trim();
            var canonicalType = TypeLabels.Keys.FirstOrDefault(x => string.Equals(x, type, StringComparison.OrdinalIgnoreCase));
            if (canonicalType == null) return Fail<DispensationResponse>(StatusCodes.Status400BadRequest, "Jenis dispensasi harus LateArrival atau EarlyDeparture.");

            // HRD-DEC-200 uji (d): dispensasi tanpa alasan ditolak.
            var reason = (request.Reason ?? string.Empty).Trim();
            if (reason.Length == 0) return Fail<DispensationResponse>(StatusCodes.Status400BadRequest, "Alasan dispensasi wajib diisi.");
            if (reason.Length > 1000) return Fail<DispensationResponse>(StatusCodes.Status400BadRequest, "Alasan dispensasi paling panjang 1000 karakter.");
            if (request.AttendanceDate == default) return Fail<DispensationResponse>(StatusCodes.Status400BadRequest, "Tanggal kerja wajib diisi.");

            var startAt = ToUtc(request.StartAt);
            var endAt = ToUtc(request.EndAt);
            if (endAt <= startAt) return Fail<DispensationResponse>(StatusCodes.Status400BadRequest, RangeMessage); // VR-PK-13

            // Rentang jam harus pada tanggal kerja itu (shift malam boleh berlanjut ke hari berikutnya).
            var startDateWib = DateOnly.FromDateTime(startAt.Add(WibOffset));
            if (startDateWib < request.AttendanceDate || startDateWib > request.AttendanceDate.AddDays(1) || endAt - startAt > TimeSpan.FromHours(24))
            {
                return Fail<DispensationResponse>(StatusCodes.Status400BadRequest, "Jam dispensasi harus berada pada tanggal kerja yang dipilih.");
            }

            // VR-PK-14: batas mundur = batas koreksi pada setelan kehadiran.
            var today = DateOnly.FromDateTime(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc).Add(WibOffset));
            var maxDays = await _settingService.GetEffectiveMaxBackdatedCorrectionDaysAsync(cancellationToken: ct);
            if (request.AttendanceDate < today.AddDays(-maxDays))
            {
                return Fail<DispensationResponse>(StatusCodes.Status422UnprocessableEntity, $"Dispensasi hanya bisa diajukan sampai {maxDays} hari ke belakang");
            }

            var employee = await _context.MstWorkforceProfiles.AsNoTracking()
                .Where(x => x.Id == employeeId && !x.IsDelete)
                .Select(x => new { x.Id, x.IsActive })
                .FirstOrDefaultAsync(ct);
            if (employee == null || !employee.IsActive) return Fail<DispensationResponse>(StatusCodes.Status404NotFound, "Pegawai tidak ditemukan atau tidak aktif.");

            var unitId = await UnitOnDateAsync(employeeId, request.AttendanceDate, ct);
            var isOnBehalf = actorProfileId != employeeId;
            if (isOnBehalf)
            {
                // Matriks 30.2: Kepala Unit mencatat atas nama pegawai UNITNYA (cakupan kelola, bukan cakupan baca).
                var manage = await _unitScopeService.GetManageScopeAsync(ct);
                if (!manage.Contains(unitId))
                {
                    return Fail<DispensationResponse>(StatusCodes.Status403Forbidden, "Anda tidak berwenang mencatat dispensasi untuk pegawai unit ini");
                }
            }

            var closed = await ClosedPeriodMessageAsync(request.AttendanceDate, unitId, ct);
            if (closed != null) return Fail<DispensationResponse>(StatusCodes.Status403Forbidden, closed); // VR-PK-15

            // HRD-ASM-76: penyetuju = atasan langsung dari WfpManagerAssignment pada tanggal itu, tidak pernah pegawai itu sendiri.
            var approverId = await ApproverOnDateAsync(employeeId, request.AttendanceDate, ct);
            if (approverId == null)
            {
                return Fail<DispensationResponse>(StatusCodes.Status422UnprocessableEntity, isOnBehalf ? NoApproverOnBehalfMessage : NoApproverSelfMessage); // VR-PK-16
            }

            var overlap = await _context.HrdAttendanceDispensations.AsNoTracking()
                .AnyAsync(x => x.WorkforceProfileId == employeeId && x.AttendanceDate == request.AttendanceDate && !x.IsDelete &&
                               x.DispensationType == canonicalType &&
                               ActiveStatuses.Contains(x.Status) && x.StartAt < endAt && x.EndAt > startAt, ct);
            if (overlap)
            {
                return Fail<DispensationResponse>(StatusCodes.Status409Conflict, "Sudah ada dispensasi sejenis yang diajukan atau disetujui pada rentang jam yang bertumpuk.");
            }

            var entity = new HrdAttendanceDispensation
            {
                WorkforceProfileId = employeeId,
                AttendanceDate = request.AttendanceDate,
                DispensationType = canonicalType,
                StartAt = startAt,
                EndAt = endAt,
                Reason = reason,
                Status = Status.Submitted,
                RequestedByUserId = actorUserId,
                IsOnBehalf = isOnBehalf,
                ApproverWorkforceProfileId = approverId,
                RowVersion = Guid.NewGuid(),
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };
            _context.HrdAttendanceDispensations.Add(entity);
            await _context.SaveChangesAsync(ct);

            var data = (await ProjectAsync(_context.HrdAttendanceDispensations.Where(x => x.Id == entity.Id), actorUserId, ct)).Single();
            return AttendanceDispensationServiceResult<DispensationResponse>.Ok(data,
                isOnBehalf ? "Dispensasi dicatat atas nama pegawai dan menunggu keputusan atasan langsung." : "Dispensasi diajukan dan menunggu keputusan atasan langsung.");
        }

        // ---------------------------------------------------------------- Keputusan dan pembatalan

        public Task<AttendanceDispensationServiceResult<DispensationResponse>> ApproveAsync(
            Guid actorUserId, Guid id, DispensationDecisionRequest request, CancellationToken ct = default) =>
            DecideAsync(actorUserId, id, request.RowVersion, Status.Approved, null, ct);

        public Task<AttendanceDispensationServiceResult<DispensationResponse>> RejectAsync(
            Guid actorUserId, Guid id, RejectDispensationRequest request, CancellationToken ct = default) =>
            DecideAsync(actorUserId, id, request.RowVersion, Status.Rejected, request.RejectionReason, ct);

        private async Task<AttendanceDispensationServiceResult<DispensationResponse>> DecideAsync(
            Guid actorUserId, Guid id, Guid rowVersion, string newStatus, string? rejectionReason, CancellationToken ct)
        {
            var entity = await _context.HrdAttendanceDispensations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null) return Fail<DispensationResponse>(StatusCodes.Status404NotFound, "Dispensasi tidak ditemukan.");

            // VR-PK-17: hanya penyetuju yang ditetapkan server, dan tidak pernah pegawai itu sendiri (AT-HRD-PK-33).
            var myProfileId = await ProfileOfAsync(actorUserId, ct);
            if (myProfileId == null || entity.ApproverWorkforceProfileId != myProfileId || myProfileId == entity.WorkforceProfileId)
            {
                return Fail<DispensationResponse>(StatusCodes.Status403Forbidden, NotApproverMessage);
            }

            var guard = await GuardTransitionAsync(entity, rowVersion, ct);
            if (guard != null) return Fail<DispensationResponse>(guard.Value.Status, guard.Value.Message);

            var reason = rejectionReason?.Trim();
            if (newStatus == Status.Rejected)
            {
                if (string.IsNullOrEmpty(reason)) return Fail<DispensationResponse>(StatusCodes.Status400BadRequest, "Alasan penolakan wajib diisi.");
                if (reason.Length > 500) return Fail<DispensationResponse>(StatusCodes.Status400BadRequest, "Alasan penolakan paling panjang 500 karakter.");
            }

            entity.Status = newStatus;
            entity.DecidedByUserId = actorUserId;
            entity.DecidedAt = DateTime.UtcNow;
            entity.RejectionReason = newStatus == Status.Rejected ? reason : null;
            var saved = await SaveTransitionAsync(entity, actorUserId, ct);
            if (saved != null) return saved;

            if (newStatus == Status.Approved) await ScheduleReprocessAsync(entity, actorUserId, ct);
            await NotifyRequesterAsync(entity, ct);
            var data = (await ProjectAsync(_context.HrdAttendanceDispensations.Where(x => x.Id == id), actorUserId, ct)).Single();
            return AttendanceDispensationServiceResult<DispensationResponse>.Ok(data, newStatus == Status.Approved ? "Dispensasi disetujui." : "Dispensasi ditolak.");
        }

        /// <summary>State 30.3: hanya pengaju yang membatalkan, selama masih Submitted.</summary>
        public async Task<AttendanceDispensationServiceResult<DispensationResponse>> CancelMineAsync(
            Guid actorUserId, Guid id, DispensationDecisionRequest request, CancellationToken ct = default)
        {
            var myProfileId = await ProfileOfAsync(actorUserId, ct);
            var entity = await _context.HrdAttendanceDispensations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null || myProfileId == null || entity.WorkforceProfileId != myProfileId)
            {
                return Fail<DispensationResponse>(StatusCodes.Status404NotFound, "Dispensasi tidak ditemukan.");
            }

            if (entity.RequestedByUserId != actorUserId)
            {
                return Fail<DispensationResponse>(StatusCodes.Status403Forbidden, "Dispensasi ini dicatat atas nama Anda oleh Kepala Unit; hanya pengajunya yang dapat membatalkan.");
            }

            var guard = await GuardTransitionAsync(entity, request.RowVersion, ct);
            if (guard != null) return Fail<DispensationResponse>(guard.Value.Status, guard.Value.Message);

            entity.Status = Status.Cancelled;
            var saved = await SaveTransitionAsync(entity, actorUserId, ct);
            if (saved != null) return saved;

            var data = (await ProjectAsync(_context.HrdAttendanceDispensations.Where(x => x.Id == id), actorUserId, ct)).Single();
            return AttendanceDispensationServiceResult<DispensationResponse>.Ok(data, "Dispensasi dibatalkan.");
        }

        /// <summary>Periode ditutup → 403 (VR-PK-15); token basi atau status akhir → 409.</summary>
        private async Task<(int Status, string Message)?> GuardTransitionAsync(HrdAttendanceDispensation entity, Guid rowVersion, CancellationToken ct)
        {
            var closed = await ClosedPeriodMessageAsync(entity.AttendanceDate, await UnitOnDateAsync(entity.WorkforceProfileId, entity.AttendanceDate, ct), ct);
            if (closed != null) return (StatusCodes.Status403Forbidden, closed);
            if (entity.RowVersion != rowVersion) return (StatusCodes.Status409Conflict, ConflictMessage);
            if (entity.Status != Status.Submitted)
            {
                return (StatusCodes.Status409Conflict, $"Dispensasi sudah berstatus {StatusLabels.GetValueOrDefault(entity.Status, entity.Status)}; tindakan ini hanya untuk dispensasi yang masih diajukan.");
            }

            return null;
        }

        private async Task<AttendanceDispensationServiceResult<DispensationResponse>?> SaveTransitionAsync(HrdAttendanceDispensation entity, Guid actorUserId, CancellationToken ct)
        {
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            try
            {
                await _context.SaveChangesAsync(ct);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Dua keputusan hampir bersamaan (klik ganda, dua tab): yang kalah ditolak tanpa efek.
                _context.ChangeTracker.Clear();
                return Fail<DispensationResponse>(StatusCodes.Status409Conflict, ConflictMessage);
            }
        }

        /// <summary>
        /// INT-HR-102 (BE-HRD-354): sesudah disetujui dan tersimpan, rekap harian tanggal itu dijadwalkan dihitung ulang agar
        /// menit terlambat/pulang cepat berkurang. Bila rekap belum ada, pemrosesan harian akan membaca dispensasi saat itu.
        /// Rekap terkunci (periode ditutup) atau sudah dikoreksi tidak dihitung ulang oleh pemroses. Kegagalan menjadwalkan
        /// tidak membatalkan keputusan; sapuan penjadwal dan job harian tetap memprosesnya.
        /// </summary>
        private async Task ScheduleReprocessAsync(HrdAttendanceDispensation entity, Guid actorUserId, CancellationToken ct)
        {
            try
            {
                var daily = await _context.HrdAttendanceDailies.AsNoTracking()
                    .Where(d => !d.IsDelete && !d.IsLocked && d.WorkforceProfileId == entity.WorkforceProfileId && d.AttendanceDate == entity.AttendanceDate)
                    .Select(d => new { d.Id })
                    .FirstOrDefaultAsync(ct);
                if (daily == null) return;

                var result = await _schedulerService.EnqueueAsync(new EnqueueAttendanceProcessingJobRequest
                {
                    StartDate = entity.AttendanceDate,
                    EndDate = entity.AttendanceDate,
                    WorkforceProfileId = entity.WorkforceProfileId,
                    ForceReprocess = true,
                    Priority = 100,
                    CorrelationId = $"ATT-DISP-{entity.Id:N}",
                    Notes = "Hitung ulang sesudah dispensasi disetujui (HRD-DEC-200, INT-HR-102)."
                }, actorUserId, ct);
                if (!result.Success)
                {
                    _logger.LogWarning("Hitung ulang sesudah dispensasi {DispensationId} disetujui tidak terjadwal: {Message}", entity.Id, result.Message);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Hitung ulang sesudah dispensasi {DispensationId} disetujui gagal dijadwalkan; keputusan tetap berlaku.", entity.Id);
            }
        }

        /// <summary>INT-HR-105: keputusan dikirim ke pengaju sebagai lonceng dalam aplikasi; kegagalan tidak membatalkan keputusan.</summary>
        private async Task NotifyRequesterAsync(HrdAttendanceDispensation entity, CancellationToken ct)
        {
            // Pengaju yang memutuskan sendiri (Kepala Unit mencatat atas nama lalu menolak/menyetujui) tidak perlu diberi tahu.
            if (entity.RequestedByUserId == entity.DecidedByUserId) return;
            try
            {
                var employeeName = await _context.MstWorkforceProfiles.AsNoTracking()
                    .Where(x => x.Id == entity.WorkforceProfileId).Select(x => x.DisplayName).FirstOrDefaultAsync(ct);
                var startWib = entity.StartAt.Add(WibOffset);
                var endWib = entity.EndAt.Add(WibOffset);
                var subject = entity.IsOnBehalf ? $"Dispensasi {employeeName}" : "Dispensasi Anda";
                var title = entity.Status == Status.Approved ? $"{subject} disetujui" : $"{subject} ditolak";
                var body = $"{TypeLabels[entity.DispensationType]} {entity.AttendanceDate:dd-MM-yyyy} {startWib:HH:mm}–{endWib:HH:mm}: " +
                           (entity.Status == Status.Approved ? "disetujui." : "ditolak. Lihat alasan penolakan pada detail dispensasi.");
                await _notificationGateway.SendInstantNotificationToUserAsync(entity.RequestedByUserId, title, body, dedupKey: $"dispensation:{entity.Id}:{entity.Status}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Pemberitahuan keputusan dispensasi {DispensationId} gagal; keputusan tetap berlaku.", entity.Id);
            }
        }

        // ---------------------------------------------------------------- Baca

        /// <summary>Dispensasi pada unit dalam cakupan baca, ditambah yang menunggu keputusan pengguna (atau hanya itu bila AwaitingMyDecision).</summary>
        public async Task<AttendanceDispensationServiceResult<PagedResult<DispensationResponse>>> ListAsync(
            Guid actorUserId, DispensationQueryRequest request, DateTime utcNow, CancellationToken ct = default)
        {
            var (from, to, rangeError) = Range(request.FromDate, request.ToDate, utcNow);
            if (rangeError != null) return Fail<PagedResult<DispensationResponse>>(StatusCodes.Status400BadRequest, rangeError);

            var myProfileId = await ProfileOfAsync(actorUserId, ct);
            var scope = await _unitScopeService.GetReadScopeAsync(ct);
            if (request.OrganizationUnitId.HasValue && !scope.Contains(request.OrganizationUnitId))
            {
                return Fail<PagedResult<DispensationResponse>>(StatusCodes.Status403Forbidden, ForbiddenUnitMessage);
            }

            var query = _context.HrdAttendanceDispensations.AsNoTracking().Where(x => !x.IsDelete && x.AttendanceDate >= from && x.AttendanceDate <= to);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status.Trim());

            var rows = await ProjectAsync(query, actorUserId, ct);
            bool AwaitsMe(DispensationResponse x) => myProfileId.HasValue && x.Status == Status.Submitted && x.ApproverWorkforceProfileId == myProfileId;
            var filtered = rows.Where(x =>
                    request.AwaitingMyDecision
                        ? AwaitsMe(x)
                        : (scope.Contains(x.OrganizationUnitId) || (myProfileId.HasValue && x.ApproverWorkforceProfileId == myProfileId)))
                .Where(x => !request.OrganizationUnitId.HasValue || x.OrganizationUnitId == request.OrganizationUnitId)
                .OrderBy(x => x.Status == Status.Submitted ? 0 : 1)
                .ThenByDescending(x => x.AttendanceDate)
                .ThenBy(x => x.EmployeeName)
                .ToList();

            return AttendanceDispensationServiceResult<PagedResult<DispensationResponse>>.Ok(Page(filtered, request.PageNumber, request.PageSize), "Daftar dispensasi berhasil diambil.");
        }

        public async Task<AttendanceDispensationServiceResult<DispensationResponse>> GetAsync(Guid actorUserId, Guid id, CancellationToken ct = default)
        {
            var data = (await ProjectAsync(_context.HrdAttendanceDispensations.AsNoTracking().Where(x => x.Id == id && !x.IsDelete), actorUserId, ct)).SingleOrDefault();
            if (data == null) return Fail<DispensationResponse>(StatusCodes.Status404NotFound, "Dispensasi tidak ditemukan.");

            var myProfileId = await ProfileOfAsync(actorUserId, ct);
            var scope = await _unitScopeService.GetReadScopeAsync(ct);
            if (!scope.Contains(data.OrganizationUnitId) && (myProfileId == null || data.ApproverWorkforceProfileId != myProfileId))
            {
                return Fail<DispensationResponse>(StatusCodes.Status403Forbidden, ForbiddenUnitMessage);
            }

            return AttendanceDispensationServiceResult<DispensationResponse>.Ok(data, "Detail dispensasi berhasil diambil.");
        }

        public async Task<AttendanceDispensationServiceResult<PagedResult<DispensationResponse>>> ListMineAsync(
            Guid actorUserId, MyDispensationQueryRequest request, DateTime utcNow, CancellationToken ct = default)
        {
            var (from, to, rangeError) = Range(request.FromDate, request.ToDate, utcNow);
            if (rangeError != null) return Fail<PagedResult<DispensationResponse>>(StatusCodes.Status400BadRequest, rangeError);

            var myProfileId = await ProfileOfAsync(actorUserId, ct);
            if (myProfileId == null) return Fail<PagedResult<DispensationResponse>>(StatusCodes.Status403Forbidden, "Akun Anda belum terhubung dengan profil pegawai.");

            var rows = await ProjectAsync(
                _context.HrdAttendanceDispensations.AsNoTracking()
                    .Where(x => !x.IsDelete && x.WorkforceProfileId == myProfileId && x.AttendanceDate >= from && x.AttendanceDate <= to),
                actorUserId, ct);
            var ordered = rows.OrderByDescending(x => x.AttendanceDate).ThenByDescending(x => x.CreatedAt).ToList();
            return AttendanceDispensationServiceResult<PagedResult<DispensationResponse>>.Ok(Page(ordered, request.PageNumber, request.PageSize), "Dispensasi Anda berhasil diambil.");
        }

        // ---------------------------------------------------------------- Pembantu

        private async Task<List<DispensationResponse>> ProjectAsync(IQueryable<HrdAttendanceDispensation> query, Guid actorUserId, CancellationToken ct)
        {
            var rows = await (
                    from d in query
                    join u in _context.Users.AsNoTracking() on d.RequestedByUserId equals u.Id into uj
                    from u in uj.DefaultIfEmpty()
                    select new DispensationResponse
                    {
                        Id = d.Id,
                        WorkforceProfileId = d.WorkforceProfileId,
                        ProfileCode = d.WorkforceProfile != null ? d.WorkforceProfile.ProfileCode : null,
                        EmployeeName = d.WorkforceProfile != null ? d.WorkforceProfile.DisplayName : null,
                        AttendanceDate = d.AttendanceDate,
                        DispensationType = d.DispensationType,
                        StartAt = d.StartAt,
                        EndAt = d.EndAt,
                        Reason = d.Reason,
                        Status = d.Status,
                        IsOnBehalf = d.IsOnBehalf,
                        RequestedByUserId = d.RequestedByUserId,
                        RequestedByName = u != null ? u.DisplayName : null,
                        ApproverWorkforceProfileId = d.ApproverWorkforceProfileId,
                        ApproverName = d.ApproverWorkforceProfile != null ? d.ApproverWorkforceProfile.DisplayName : null,
                        DecidedByUserId = d.DecidedByUserId,
                        DecidedAt = d.DecidedAt,
                        RejectionReason = d.RejectionReason,
                        CreatedAt = d.CreateDateTime,
                        RowVersion = d.RowVersion
                    })
                .ToListAsync(ct);
            if (rows.Count == 0) return rows;

            var myProfileId = await ProfileOfAsync(actorUserId, ct);
            var units = await UnitsOnDatesAsync(rows.Select(x => (x.WorkforceProfileId, x.AttendanceDate)).Distinct().ToList(), ct);
            var unitIds = units.Values.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var unitNames = await _context.MstOrganizationUnits.AsNoTracking().Where(x => unitIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.UnitName, ct);

            foreach (var row in rows)
            {
                row.StartAt = DateTime.SpecifyKind(row.StartAt, DateTimeKind.Utc);
                row.EndAt = DateTime.SpecifyKind(row.EndAt, DateTimeKind.Utc);
                row.OrganizationUnitId = units.GetValueOrDefault((row.WorkforceProfileId, row.AttendanceDate));
                row.OrganizationUnitName = row.OrganizationUnitId.HasValue ? unitNames.GetValueOrDefault(row.OrganizationUnitId.Value) : null;
                row.StatusLabel = StatusLabels.GetValueOrDefault(row.Status, row.Status);
                row.DispensationTypeLabel = TypeLabels.GetValueOrDefault(row.DispensationType, row.DispensationType);
                var submitted = row.Status == Status.Submitted;
                row.CanDecide = submitted && myProfileId.HasValue && row.ApproverWorkforceProfileId == myProfileId && myProfileId != row.WorkforceProfileId;
                row.CanCancel = submitted && row.RequestedByUserId == actorUserId && myProfileId == row.WorkforceProfileId;
            }

            return rows;
        }

        private async Task<Guid?> ProfileOfAsync(Guid userId, CancellationToken ct) =>
            userId == Guid.Empty
                ? null
                : await _context.Users.AsNoTracking().Where(x => x.Id == userId && x.IsActive).Select(x => x.WorkforceProfileId).FirstOrDefaultAsync(ct);

        private async Task<Guid?> UnitOnDateAsync(Guid profileId, DateOnly date, CancellationToken ct) =>
            (await UnitsOnDatesAsync(new List<(Guid, DateOnly)> { (profileId, date) }, ct)).GetValueOrDefault((profileId, date));

        /// <summary>
        /// Unit penempatan pegawai pada tanggal itu: penempatan utama yang berlaku, bila tidak ada penempatan aktif lain yang berlaku.
        /// Tanggal efektif HR disimpan sebagai tanggal kalender pada tengah malam UTC.
        /// </summary>
        private async Task<Dictionary<(Guid, DateOnly), Guid?>> UnitsOnDatesAsync(List<(Guid ProfileId, DateOnly Date)> keys, CancellationToken ct)
        {
            var profileIds = keys.Select(x => x.ProfileId).Distinct().ToList();
            var placements = await _context.WfpOrganizationAssignments.AsNoTracking()
                .Where(x => profileIds.Contains(x.WorkforceProfileId) && x.IsActive && !x.IsDelete && x.OrganizationUnitId != null)
                .Select(x => new { x.WorkforceProfileId, x.OrganizationUnitId, x.IsPrimary, x.EffectiveStartDate, x.EffectiveEndDate })
                .ToListAsync(ct);

            return keys.ToDictionary(k => (k.ProfileId, k.Date), k => placements
                .Where(p => p.WorkforceProfileId == k.ProfileId &&
                            DateOnly.FromDateTime(p.EffectiveStartDate) <= k.Date &&
                            (!p.EffectiveEndDate.HasValue || DateOnly.FromDateTime(p.EffectiveEndDate.Value) >= k.Date))
                .OrderByDescending(p => p.IsPrimary)
                .ThenByDescending(p => p.EffectiveStartDate)
                .Select(p => p.OrganizationUnitId)
                .FirstOrDefault());
        }

        private async Task<Guid?> ApproverOnDateAsync(Guid employeeId, DateOnly date, CancellationToken ct)
        {
            var candidates = await _context.WfpManagerAssignments.AsNoTracking()
                .Where(x => x.WorkforceProfileId == employeeId && x.ManagerWorkforceProfileId != employeeId &&
                            x.IsActive && !x.IsDelete && x.CanApproveRequests)
                .Select(x => new { x.ManagerWorkforceProfileId, x.IsPrimaryManager, x.ManagerType, x.EffectiveStartDate, x.EffectiveEndDate })
                .ToListAsync(ct);
            return candidates
                .Where(x => DateOnly.FromDateTime(x.EffectiveStartDate) <= date && (!x.EffectiveEndDate.HasValue || DateOnly.FromDateTime(x.EffectiveEndDate.Value) >= date))
                .OrderByDescending(x => x.IsPrimaryManager)
                .ThenByDescending(x => x.ManagerType == "Direct")
                .ThenByDescending(x => x.EffectiveStartDate)
                .Select(x => (Guid?)x.ManagerWorkforceProfileId)
                .FirstOrDefault();
        }

        /// <summary>VR-PK-15: periode kehadiran Closed yang mencakup tanggal itu (periode tanpa unit berlaku untuk semua unit).</summary>
        private async Task<string?> ClosedPeriodMessageAsync(DateOnly date, Guid? unitId, CancellationToken ct)
        {
            var closed = await _context.HrdAttendancePeriods.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.PeriodStatus == PeriodStatus.Closed && x.StartDate <= date && x.EndDate >= date &&
                               (x.OrganizationUnitId == null || x.OrganizationUnitId == unitId), ct);
            return closed ? $"Periode kehadiran {MonthNames[date.Month - 1]} {date.Year} sudah ditutup" : null;
        }

        private static (DateOnly From, DateOnly To, string? Error) Range(DateOnly? fromDate, DateOnly? toDate, DateTime utcNow)
        {
            var today = DateOnly.FromDateTime(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc).Add(WibOffset));
            var from = fromDate ?? new DateOnly(today.Year, today.Month, 1);
            var to = toDate ?? new DateOnly(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            return (from, to, to < from ? "Tanggal selesai tidak boleh sebelum tanggal mulai." : null);
        }

        private static PagedResult<DispensationResponse> Page(List<DispensationResponse> rows, int pageNumber, int pageSize)
        {
            var number = Math.Max(1, pageNumber);
            var size = Math.Clamp(pageSize, 1, 100);
            return new PagedResult<DispensationResponse>
            {
                PageNumber = number,
                PageSize = size,
                TotalData = rows.Count,
                TotalPage = (int)Math.Ceiling(rows.Count / (double)size),
                Items = rows.Skip((number - 1) * size).Take(size).ToList()
            };
        }

        /// <summary>Nilai tanpa zona dibaca sebagai jam WIB (sama dengan AppDateTimeHelper.ToUtc).</summary>
        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value - WibOffset, DateTimeKind.Utc)
        };

        private static AttendanceDispensationServiceResult<T> Fail<T>(int status, string message) => AttendanceDispensationServiceResult<T>.Fail(status, message);
    }

    public class AttendanceDispensationServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static AttendanceDispensationServiceResult<T> Ok(T data, string message) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Message = message, Data = data };

        public static AttendanceDispensationServiceResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
