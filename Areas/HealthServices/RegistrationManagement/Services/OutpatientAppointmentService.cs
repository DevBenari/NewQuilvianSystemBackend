using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>Reschedule bentrok dengan kunjungan lain pasien yang sama (RJ-REG-DEC-007).</summary>
    public sealed class OutpatientAppointmentConflictException : Exception
    {
        public OutpatientAppointmentConflictException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Tracking Jadwal Booking dan reschedule appointment Rawat Jalan
    /// (RJ-REG-REV-BE-005, RJ-REG-DEC-007).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Booking = kunjungan.</b> Source tidak punya entity booking terpisah: appointment adalah
    /// <see cref="RegPatientEncounter"/> ber-<c>IsAppointment</c> yang antreannya sudah
    /// dialokasikan pada tanggal kunjungan. Reschedule karena itu memindahkan kunjungan yang
    /// sama — ID kunjungan, nomor kunjungan, penjamin, dan rujukan tetap — lalu mengganti
    /// antreannya. Tidak ada kunjungan baru yang dibuat.
    /// </para>
    /// <para>
    /// <b>Hanya yang belum datang.</b> Status ≤ <c>Menunggu Dokter</c>, belum check-in, dan
    /// antreannya belum dipanggil maupun diskrining. Begitu pasien sudah diproses, jadwalnya
    /// adalah fakta klinis dan tidak dipindah dari menu ini.
    /// </para>
    /// <para>
    /// <b>Konsistensi.</b> Baris kunjungan dikunci <c>FOR UPDATE</c> dan kunci advisory per
    /// pasien yang sama dengan pendaftaran dipegang, sehingga reschedule tidak bersilangan dengan
    /// pendaftaran baru maupun reschedule lain. Nomor antrean baru diambil dari
    /// <see cref="OutpatientQueueNumberAllocator"/> yang sama dengan pendaftaran.
    /// </para>
    /// </remarks>
    public class OutpatientAppointmentService
    {
        public const string ResourceName = "OutpatientAppointment";
        public const string RescheduleAction = "Reschedule";
        private const int MaxRangeDays = 92;
        private const int MaxReasonLength = 250;
        private const string LogCategory = "HealthServices.RegistrationManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly OutpatientQueueClassificationService _classificationService;
        private readonly OutpatientQueueNumberAllocator _queueNumberAllocator;
        private readonly QueueRealtimeService _queueRealtimeService;
        private readonly LoggerService _loggerService;

        public OutpatientAppointmentService(
            ApplicationDbContext dbContext,
            AccessPermissionService accessPermissionService,
            OutpatientQueueClassificationService classificationService,
            OutpatientQueueNumberAllocator queueNumberAllocator,
            QueueRealtimeService queueRealtimeService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _accessPermissionService = accessPermissionService;
            _classificationService = classificationService;
            _queueNumberAllocator = queueNumberAllocator;
            _queueRealtimeService = queueRealtimeService;
            _loggerService = loggerService;
        }

        public async Task<PagedResult<OutpatientAppointmentListItemResponse>> GetPagedAsync(
            ClaimsPrincipal user,
            OutpatientAppointmentListQuery request,
            CancellationToken cancellationToken = default)
        {
            var today = Today();
            var from = request.DateFrom.HasValue ? ToUtcDate(request.DateFrom.Value) : today;
            var to = request.DateTo.HasValue ? ToUtcDate(request.DateTo.Value) : today.AddDays(30);

            if (from > to || (to - from).TotalDays > MaxRangeDays - 1)
                throw new OutpatientEncounterValidationException($"Rentang tanggal tidak valid. Maksimal {MaxRangeDays} hari.");

            var query = BaseQuery()
                .Where(x => x.EncounterDate >= from && x.EncounterDate < to.AddDays(1));

            if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
            {
                var clinicId = request.ClinicId.Value;
                query = query.Where(x => x.ClinicId == clinicId);
            }

            if (request.DoctorId.HasValue && request.DoctorId.Value != Guid.Empty)
            {
                var doctorId = request.DoctorId.Value;
                query = query.Where(x => x.DoctorId == doctorId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();

                if (keyword.Length > 100)
                    throw new OutpatientEncounterValidationException("Saringan tidak valid.");

                query = query.Where(x =>
                    x.EncounterNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)));
            }

            query = ApplyStatusFilter(query, request.Status);

            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var totalData = await query.CountAsync(cancellationToken);

            var rows = await ProjectRows(query
                    .OrderBy(x => x.EncounterDate)
                    .ThenBy(x => x.RegisteredAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize))
                .ToListAsync(cancellationToken);

            var canReschedule = await _accessPermissionService.HasAccessAsync(user, ResourceName, RescheduleAction);

            return new PagedResult<OutpatientAppointmentListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows.Select(x => MapItem(x, canReschedule, new OutpatientAppointmentListItemResponse())).ToList()
            };
        }

        public async Task<OutpatientAppointmentDetailResponse> GetDetailAsync(
            ClaimsPrincipal user,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var row = await ProjectRows(BaseQuery().Where(x => x.Id == id))
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                throw new OutpatientEncounterNotFoundException();

            var extra = await BaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    BirthDate = x.Patient != null ? x.Patient.BirthDate : null,
                    PhoneNumber = x.Patient != null ? x.Patient.PhoneNumber : null,
                    x.ServiceUnitId,
                    x.DoctorScheduleId,
                    DoctorSpecialistName = x.Doctor != null ? x.Doctor.SpecialistName : null,
                    PaymentSourceName = x.PaymentSource != null ? x.PaymentSource.PaymentSourceNameSnapshot : null,
                    x.PaymentType,
                    x.RegisteredAt,
                    x.CancelledAt,
                    x.CancelReason
                })
                .FirstAsync(cancellationToken);

            var history = await (
                    from h in _dbContext.Set<RegEncounterReschedule>().AsNoTracking()
                    where h.EncounterId == id && !h.IsDelete
                    join oldDoctor in _dbContext.Set<MstDoctor>() on h.OldDoctorId equals oldDoctor.Id into oldDoctors
                    from oldDoctor in oldDoctors.DefaultIfEmpty()
                    join newDoctor in _dbContext.Set<MstDoctor>() on h.NewDoctorId equals newDoctor.Id into newDoctors
                    from newDoctor in newDoctors.DefaultIfEmpty()
                    join actor in _dbContext.Users on h.RescheduledByUserId equals actor.Id into actors
                    from actor in actors.DefaultIfEmpty()
                    orderby h.RescheduledAt descending
                    select new OutpatientAppointmentRescheduleHistoryResponse
                    {
                        Id = h.Id,
                        OldEncounterDate = h.OldEncounterDate,
                        NewEncounterDate = h.NewEncounterDate,
                        OldDoctorName = oldDoctor != null ? oldDoctor.FullName : null,
                        NewDoctorName = newDoctor != null ? newDoctor.FullName : null,
                        OldQueueCode = h.OldQueueCode,
                        NewQueueCode = h.NewQueueCode,
                        Reason = h.Reason,
                        RescheduledAt = h.RescheduledAt,
                        RescheduledByName = actor != null ? actor.DisplayName : null
                    })
                .ToListAsync(cancellationToken);

            var canReschedule = await _accessPermissionService.HasAccessAsync(user, ResourceName, RescheduleAction);
            var detail = (OutpatientAppointmentDetailResponse)MapItem(row, canReschedule, new OutpatientAppointmentDetailResponse());

            detail.BirthDate = extra.BirthDate;
            detail.PhoneNumber = extra.PhoneNumber;
            detail.ServiceUnitId = extra.ServiceUnitId;
            detail.DoctorScheduleId = extra.DoctorScheduleId;
            detail.DoctorSpecialistName = extra.DoctorSpecialistName;
            detail.QueueStatusName = row.QueueStatus.HasValue ? GetDisplayName(row.QueueStatus.Value) : null;
            detail.QueueDate = row.QueueDate;
            detail.PaymentLabel = !string.IsNullOrWhiteSpace(extra.PaymentSourceName)
                ? extra.PaymentSourceName!
                : GetDisplayName(extra.PaymentType);
            detail.RegisteredAt = extra.RegisteredAt;
            detail.CancelledAt = extra.CancelledAt;
            detail.CancelReason = extra.CancelReason;
            detail.RescheduleHistory = history;

            return detail;
        }

        public async Task<OutpatientAppointmentFilterMetadataResponse> GetFilterMetadataAsync(
            CancellationToken cancellationToken = default)
        {
            return new OutpatientAppointmentFilterMetadataResponse
            {
                StatusOptions = new List<OutpatientEncounterOptionResponse>
                {
                    new() { Value = OutpatientAppointmentStatuses.Upcoming, Label = "Belum Datang" },
                    new() { Value = OutpatientAppointmentStatuses.Arrived, Label = "Sudah Diproses" },
                    new() { Value = OutpatientAppointmentStatuses.Cancelled, Label = "Batal / Tidak Hadir" }
                },
                ClinicOptions = await _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.ClinicName)
                    .Select(x => new OutpatientEncounterOptionResponse { Value = x.Id.ToString(), Label = x.ClinicName })
                    .ToListAsync(cancellationToken),
                DoctorOptions = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.FullName)
                    .Select(x => new OutpatientEncounterOptionResponse { Value = x.Id.ToString(), Label = x.FullName })
                    .ToListAsync(cancellationToken)
            };
        }

        /// <summary>Memindahkan appointment ke tanggal dan/atau jadwal dokter lain (RJ-REG-DEC-007).</summary>
        public async Task<OutpatientAppointmentRescheduleResponse> RescheduleAsync(
            ClaimsPrincipal user,
            Guid id,
            OutpatientAppointmentRescheduleRequest? request,
            CancellationToken cancellationToken = default)
        {
            var reason = request?.Reason?.Trim();
            if (string.IsNullOrEmpty(reason) || reason.Length > MaxReasonLength)
                throw new OutpatientEncounterValidationException("Alasan reschedule wajib diisi, maksimal 250 karakter.");

            if (string.IsNullOrWhiteSpace(request!.VisitDate) ||
                !DateOnly.TryParseExact(request.VisitDate.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var visitDateOnly))
                throw new OutpatientEncounterValidationException("Tanggal kunjungan baru wajib diisi dengan format yyyy-MM-dd.");

            if (!request.DoctorScheduleId.HasValue || request.DoctorScheduleId.Value == Guid.Empty)
                throw new OutpatientEncounterValidationException("Jadwal dokter baru wajib dipilih.");

            var newDate = DateTime.SpecifyKind(visitDateOnly.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var today = Today();

            if (newDate < today)
                throw new OutpatientEncounterValidationException("Tanggal kunjungan baru tidak boleh sebelum hari ini.");

            var scheduleId = request.DoctorScheduleId.Value;
            var actorUserId = GetUserId(user);
            var now = DateTime.UtcNow;

            List<RegQueue> cancelledQueues;
            RegQueue? newQueue = null;
            RegEncounterReschedule history;
            RegPatientEncounter encounter;
            string? doctorName;

            await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                encounter = await _dbContext.Set<RegPatientEncounter>()
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM public.""RegPatientEncounter""
                        WHERE ""Id"" = {id}
                          AND ""IsDelete"" = false
                        FOR UPDATE")
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new OutpatientEncounterNotFoundException();

                if (encounter.EncounterType != EncounterType.Outpatient || !encounter.ClinicId.HasValue || !encounter.IsAppointment)
                    throw new OutpatientEncounterNotFoundException();

                // Kunci yang sama dengan pendaftaran (RJ-DOC-REV-BE-007): pendaftaran baru pasien
                // ini menunggu sampai reschedule selesai, begitu pula sebaliknya.
                if (_dbContext.Database.IsNpgsql())
                {
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "SELECT pg_advisory_xact_lock(hashtext({0}));",
                        [$"REG_ENCOUNTER_ACTIVE_{encounter.PatientId:N}"],
                        cancellationToken);
                }

                var activeQueues = await _dbContext.Set<RegQueue>()
                    .Where(x => x.EncounterId == id && !x.IsDelete && !x.IsCancel && !x.CancelledAt.HasValue && !x.CompletedAt.HasValue)
                    .ToListAsync(cancellationToken);

                var blockedReason = GetRescheduleBlockedReason(
                    encounter.IsCancel || encounter.CancelledAt.HasValue,
                    encounter.CompletedAt.HasValue || encounter.NoShowAt.HasValue,
                    encounter.CheckedInAt.HasValue,
                    encounter.EncounterStatus,
                    activeQueues.Any(IsQueueProgressed));

                if (blockedReason != null)
                    throw new OutpatientEncounterValidationException(blockedReason);

                var schedule = await _dbContext.Set<MstDoctorSchedule>()
                    .AsNoTracking()
                    .Include(x => x.Doctor)
                    .FirstOrDefaultAsync(x => x.Id == scheduleId && !x.IsDelete, cancellationToken);

                if (schedule == null ||
                    !schedule.IsActive ||
                    schedule.ScheduleStatus != DoctorScheduleStatus.Active ||
                    schedule.ClinicId != encounter.ClinicId.Value ||
                    schedule.ServiceUnitId != encounter.ServiceUnitId)
                    throw new OutpatientEncounterValidationException("Jadwal dokter tidak valid, tidak aktif, atau bukan milik poliklinik kunjungan ini.");

                if (!schedule.IsAllowAppointment)
                    throw new OutpatientEncounterValidationException("Jadwal dokter tidak menerima appointment.");

                if (!IsScheduleValidForDate(schedule, newDate))
                    throw new OutpatientEncounterValidationException("Jadwal dokter tidak sesuai dengan tanggal kunjungan baru.");

                if (ToUtcDate(encounter.EncounterDate) == newDate && encounter.DoctorScheduleId == schedule.Id)
                    throw new OutpatientEncounterValidationException("Tanggal dan jadwal dokter sama dengan jadwal saat ini. Tidak ada yang diubah.");

                var queueIds = activeQueues.Select(x => x.Id).ToList();
                var scheduleQueues = _dbContext.Set<RegQueue>()
                    .AsNoTracking()
                    .Where(x => x.QueueDate == newDate &&
                                x.DoctorScheduleId == schedule.Id &&
                                !x.IsDelete &&
                                !x.IsCancel &&
                                !x.CancelledAt.HasValue &&
                                !queueIds.Contains(x.Id));

                if (schedule.MaxPatientQuota > 0 &&
                    await scheduleQueues.CountAsync(cancellationToken) >= schedule.MaxPatientQuota)
                    throw new OutpatientEncounterValidationException("Kuota pasien pada jadwal dokter tanggal itu sudah penuh.");

                if (schedule.MaxAppointmentQuota > 0 &&
                    await scheduleQueues.CountAsync(x => x.IsAppointment, cancellationToken) >= schedule.MaxAppointmentQuota)
                    throw new OutpatientEncounterValidationException("Kuota appointment pada jadwal dokter tanggal itu sudah penuh.");

                var clash = await _dbContext.Set<RegPatientEncounter>()
                    .AsNoTracking()
                    .Where(x => x.Id != id &&
                                x.PatientId == encounter.PatientId &&
                                x.ClinicId == encounter.ClinicId &&
                                x.EncounterDate == newDate &&
                                !x.IsDelete &&
                                !x.IsCancel &&
                                !x.CancelledAt.HasValue &&
                                !x.CompletedAt.HasValue &&
                                !x.NoShowAt.HasValue)
                    .Select(x => x.EncounterNumber)
                    .FirstOrDefaultAsync(cancellationToken);

                if (clash != null)
                    throw new OutpatientAppointmentConflictException(
                        $"Pasien sudah memiliki kunjungan {clash} di poliklinik yang sama pada tanggal itu.");

                await EnsurePayerValidOnDateAsync(encounter.Id, newDate, cancellationToken);

                // Satu kunjungan hanya boleh punya satu antrean hidup (unique IX_RegQueue_EncounterId),
                // jadi antrean yang sama dipindah: ID antrean tetap, tanggal/nomor/kode/jadwal baru.
                // Salinan keadaan lama dikirim sebagai notifikasi batal ke layar antrean tanggal lama.
                var queue = activeQueues.OrderByDescending(x => x.CreateDateTime).FirstOrDefault();
                cancelledQueues = new List<RegQueue>();
                var oldQueueId = queue?.Id;
                var oldQueueCode = queue?.QueueCode;

                foreach (var extra in activeQueues.Where(x => queue == null || x.Id != queue.Id))
                {
                    // Data lama yang menyimpang (lebih dari satu antrean hidup) dibatalkan, bukan dihapus.
                    extra.QueueStatus = QueueStatus.Cancelled;
                    extra.CancelledAt = now;
                    extra.CancelledByUserId = actorUserId;
                    extra.CancelReason = TrimTo($"Reschedule: {reason}", 250);
                    extra.IsCancel = true;
                    extra.CancelDateTime = now;
                    extra.CancelBy = actorUserId;
                    extra.IsActive = false;
                    extra.UpdateDateTime = now;
                    extra.UpdateBy = actorUserId;
                    cancelledQueues.Add(extra);
                }

                if (queue != null)
                {
                    cancelledQueues.Add(SnapshotAsCancelled(queue));

                    var classification = await _classificationService.ClassifyAsync(encounter.PatientId, newDate, cancellationToken);
                    var allocation = await _queueNumberAllocator.AllocateAsync(
                        newDate,
                        encounter.ServiceUnitId,
                        encounter.ClinicId,
                        classification.IsPriorityQueue,
                        cancellationToken);

                    queue.DoctorId = schedule.DoctorId;
                    queue.DoctorScheduleId = schedule.Id;
                    queue.QueueDate = newDate;
                    queue.QueueNumber = allocation.QueueNumber;
                    queue.QueueCode = $"{QueueCodePrefix(oldQueueCode)}{allocation.QueueNumber:D3}";
                    queue.QueueScopeKey = allocation.QueueScopeKey;
                    queue.QueueStatus = encounter.IsScreeningRequired ? QueueStatus.WaitingForNurse : QueueStatus.WaitingForDoctor;
                    queue.IsPriorityQueue = classification.IsPriorityQueue;
                    queue.QueuePriorityLevelSnapshot = classification.PriorityLevel;
                    queue.QueueAudienceSnapshot = classification.QueueAudience;
                    queue.PublicDisplayModeSnapshot = classification.PublicDisplayMode;
                    queue.PatientMembershipIdSnapshot = classification.PatientMembershipId;
                    queue.MembershipTierIdSnapshot = classification.MembershipTierId;
                    queue.MembershipTierCodeSnapshot = classification.MembershipTierCode;
                    queue.PriorityReasonCode = classification.PriorityReasonCode;
                    queue.UpdateDateTime = now;
                    queue.UpdateBy = actorUserId;
                    newQueue = queue;

                    encounter.EncounterStatus = encounter.IsScreeningRequired
                        ? EncounterStatus.WaitingForNurse
                        : EncounterStatus.WaitingForDoctor;
                }

                history = new RegEncounterReschedule
                {
                    Id = Guid.NewGuid(),
                    EncounterId = encounter.Id,
                    OldEncounterDate = encounter.EncounterDate,
                    NewEncounterDate = newDate,
                    OldDoctorScheduleId = encounter.DoctorScheduleId,
                    NewDoctorScheduleId = schedule.Id,
                    OldDoctorId = encounter.DoctorId,
                    NewDoctorId = schedule.DoctorId,
                    OldQueueId = oldQueueId,
                    NewQueueId = newQueue?.Id,
                    OldQueueCode = oldQueueCode,
                    NewQueueCode = newQueue?.QueueCode,
                    Reason = reason,
                    RescheduledAt = now,
                    RescheduledByUserId = actorUserId,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<RegEncounterReschedule>().Add(history);

                encounter.EncounterDate = newDate;
                encounter.DoctorScheduleId = schedule.Id;
                encounter.DoctorId = schedule.DoctorId;
                encounter.RoomId = schedule.RoomId ?? encounter.RoomId;
                encounter.UpdateDateTime = now;
                encounter.UpdateBy = actorUserId;
                doctorName = schedule.Doctor?.FullName;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            // Notifikasi sesudah commit; kegagalannya tidak membatalkan reschedule.
            foreach (var queue in cancelledQueues)
            {
                await NotifySafeAsync(() => _queueRealtimeService.NotifyQueueCancelledAsync(queue, actorUserId, "Appointment dijadwal ulang."), queue.Id);
            }

            if (newQueue != null)
            {
                await NotifySafeAsync(() => _queueRealtimeService.NotifyQueueCreatedAsync(newQueue, actorUserId, "Antrean appointment hasil jadwal ulang."), newQueue.Id);
            }

            // Tanpa alasan dan data pasien.
            await _loggerService.AuditAsync(LogCategory, "RescheduleOutpatientAppointment", "Appointment rawat jalan dijadwal ulang.",
                new
                {
                    EntityId = id,
                    RescheduleId = history.Id,
                    Controller = "OutpatientAppointment",
                    Action = RescheduleAction,
                    ActorUserId = actorUserId,
                    history.OldEncounterDate,
                    history.NewEncounterDate,
                    history.OldDoctorScheduleId,
                    history.NewDoctorScheduleId,
                    history.OldQueueId,
                    history.NewQueueId
                });

            return new OutpatientAppointmentRescheduleResponse
            {
                Id = encounter.Id,
                RescheduleId = history.Id,
                EncounterDate = encounter.EncounterDate,
                DoctorId = encounter.DoctorId,
                DoctorName = doctorName,
                QueueCode = newQueue?.QueueCode,
                CancelledQueueCode = history.OldQueueCode
            };
        }

        /// <summary>
        /// Alasan appointment tidak dapat dijadwal ulang, atau <c>null</c> bila boleh
        /// (RJ-REG-DEC-007).
        /// </summary>
        public static string? GetRescheduleBlockedReason(
            bool isCancelled,
            bool isClosed,
            bool isCheckedIn,
            EncounterStatus status,
            bool queueProgressed)
        {
            if (isCancelled)
                return "Appointment sudah dibatalkan.";

            if (isClosed || status > EncounterStatus.WaitingForDoctor)
                return $"Appointment dengan status {GetDisplayName(status)} tidak dapat dijadwal ulang.";

            if (isCheckedIn || queueProgressed || status == EncounterStatus.InNurseScreening)
                return "Pasien sudah datang dan diproses, sehingga jadwalnya tidak dapat diubah.";

            return null;
        }

        private sealed class AppointmentRow
        {
            public Guid Id { get; init; }
            public string EncounterNumber { get; init; } = string.Empty;
            public DateTime EncounterDate { get; init; }
            public Guid PatientId { get; init; }
            public string PatientName { get; init; } = string.Empty;
            public string MedicalRecordNumber { get; init; } = string.Empty;
            public Guid ClinicId { get; init; }
            public string ClinicName { get; init; } = string.Empty;
            public Guid? DoctorId { get; init; }
            public string? DoctorName { get; init; }
            public string? ScheduleSessionName { get; init; }
            public TimeSpan? ScheduleStartTime { get; init; }
            public TimeSpan? ScheduleEndTime { get; init; }
            public EncounterStatus EncounterStatus { get; init; }
            public bool IsCancelled { get; init; }
            public bool IsClosed { get; init; }
            public bool IsCheckedIn { get; init; }
            public string? QueueCode { get; init; }
            public QueueStatus? QueueStatus { get; init; }
            public DateTime? QueueDate { get; init; }
            public bool QueueProgressed { get; init; }
            public int RescheduleCount { get; init; }
        }

        private IQueryable<RegPatientEncounter> BaseQuery() =>
            _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.IsAppointment &&
                            x.EncounterType == EncounterType.Outpatient &&
                            x.ClinicId != null);

        private IQueryable<AppointmentRow> ProjectRows(IQueryable<RegPatientEncounter> query)
        {
            var queues = _dbContext.Set<RegQueue>().Where(q => !q.IsDelete && !q.IsCancel && !q.CancelledAt.HasValue);

            return query.Select(x => new AppointmentRow
            {
                Id = x.Id,
                EncounterNumber = x.EncounterNumber,
                EncounterDate = x.EncounterDate,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                ClinicId = x.ClinicId!.Value,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ScheduleSessionName = x.DoctorSchedule != null ? x.DoctorSchedule.SessionName : null,
                ScheduleStartTime = x.DoctorSchedule != null ? (TimeSpan?)x.DoctorSchedule.StartTime : null,
                ScheduleEndTime = x.DoctorSchedule != null ? (TimeSpan?)x.DoctorSchedule.EndTime : null,
                EncounterStatus = x.EncounterStatus,
                IsCancelled = x.IsCancel || x.CancelledAt != null,
                IsClosed = x.CompletedAt != null || x.NoShowAt != null,
                IsCheckedIn = x.CheckedInAt != null,
                QueueCode = queues.Where(q => q.EncounterId == x.Id).OrderByDescending(q => q.CreateDateTime).Select(q => q.QueueCode).FirstOrDefault(),
                QueueStatus = queues.Where(q => q.EncounterId == x.Id).OrderByDescending(q => q.CreateDateTime).Select(q => (QueueStatus?)q.QueueStatus).FirstOrDefault(),
                QueueDate = queues.Where(q => q.EncounterId == x.Id).OrderByDescending(q => q.CreateDateTime).Select(q => (DateTime?)q.QueueDate).FirstOrDefault(),
                QueueProgressed = queues.Any(q => q.EncounterId == x.Id &&
                    (q.LastNurseCalledAt != null || q.ScreeningStartedAt != null || q.LastDoctorCalledAt != null ||
                     (q.QueueStatus != QueueStatus.WaitingForNurse && q.QueueStatus != QueueStatus.WaitingForDoctor))),
                RescheduleCount = _dbContext.Set<RegEncounterReschedule>().Count(h => h.EncounterId == x.Id && !h.IsDelete)
            });
        }

        private IQueryable<RegPatientEncounter> ApplyStatusFilter(IQueryable<RegPatientEncounter> query, string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return query;

            var queues = _dbContext.Set<RegQueue>().Where(q => !q.IsDelete && !q.IsCancel && !q.CancelledAt.HasValue);
            Expression<Func<RegPatientEncounter, bool>> cancelled = x =>
                x.IsCancel || x.CancelledAt != null || x.NoShowAt != null ||
                x.EncounterStatus == EncounterStatus.Cancelled || x.EncounterStatus == EncounterStatus.NoShow;

            return status.Trim() switch
            {
                OutpatientAppointmentStatuses.Cancelled => query.Where(cancelled),
                OutpatientAppointmentStatuses.Upcoming => query.Where(x =>
                    !x.IsCancel && x.CancelledAt == null && x.NoShowAt == null && x.CompletedAt == null &&
                    x.CheckedInAt == null &&
                    x.EncounterStatus <= EncounterStatus.WaitingForDoctor &&
                    x.EncounterStatus != EncounterStatus.InNurseScreening &&
                    !queues.Any(q => q.EncounterId == x.Id &&
                        (q.LastNurseCalledAt != null || q.ScreeningStartedAt != null || q.LastDoctorCalledAt != null ||
                         (q.QueueStatus != QueueStatus.WaitingForNurse && q.QueueStatus != QueueStatus.WaitingForDoctor)))),
                OutpatientAppointmentStatuses.Arrived => query.Where(x =>
                    !x.IsCancel && x.CancelledAt == null && x.NoShowAt == null &&
                    x.EncounterStatus != EncounterStatus.Cancelled && x.EncounterStatus != EncounterStatus.NoShow &&
                    (x.CompletedAt != null ||
                     x.CheckedInAt != null ||
                     x.EncounterStatus > EncounterStatus.WaitingForDoctor ||
                     x.EncounterStatus == EncounterStatus.InNurseScreening ||
                     queues.Any(q => q.EncounterId == x.Id &&
                        (q.LastNurseCalledAt != null || q.ScreeningStartedAt != null || q.LastDoctorCalledAt != null ||
                         (q.QueueStatus != QueueStatus.WaitingForNurse && q.QueueStatus != QueueStatus.WaitingForDoctor))))),
                _ => throw new OutpatientEncounterValidationException(
                    "Saringan status booking tidak valid. Gunakan Upcoming, Arrived, atau Cancelled.")
            };
        }

        private static OutpatientAppointmentListItemResponse MapItem(
            AppointmentRow row,
            bool canReschedulePermission,
            OutpatientAppointmentListItemResponse target)
        {
            var blockedReason = GetRescheduleBlockedReason(row.IsCancelled, row.IsClosed, row.IsCheckedIn, row.EncounterStatus, row.QueueProgressed);
            var isCancelled = row.IsCancelled ||
                row.EncounterStatus is EncounterStatus.Cancelled or EncounterStatus.NoShow ||
                (row.IsClosed && row.EncounterStatus < EncounterStatus.ConsultationCompleted && !row.IsCheckedIn);
            var bookingStatus = isCancelled
                ? OutpatientAppointmentStatuses.Cancelled
                : blockedReason == null ? OutpatientAppointmentStatuses.Upcoming : OutpatientAppointmentStatuses.Arrived;

            target.Id = row.Id;
            target.EncounterNumber = row.EncounterNumber;
            target.EncounterDate = row.EncounterDate;
            target.PatientId = row.PatientId;
            target.PatientName = row.PatientName;
            target.MedicalRecordNumber = row.MedicalRecordNumber;
            target.ClinicId = row.ClinicId;
            target.ClinicName = row.ClinicName;
            target.DoctorId = row.DoctorId;
            target.DoctorName = row.DoctorName;
            target.ScheduleLabel = BuildScheduleLabel(row.ScheduleSessionName, row.ScheduleStartTime, row.ScheduleEndTime);
            target.QueueCode = row.QueueCode;
            target.EncounterStatus = (int)row.EncounterStatus;
            target.EncounterStatusName = GetDisplayName(row.EncounterStatus);
            target.BookingStatus = bookingStatus;
            target.BookingStatusName = bookingStatus switch
            {
                OutpatientAppointmentStatuses.Upcoming => "Belum Datang",
                OutpatientAppointmentStatuses.Arrived => "Sudah Diproses",
                _ => "Batal / Tidak Hadir"
            };
            target.RescheduleCount = row.RescheduleCount;
            target.CanReschedule = canReschedulePermission && blockedReason == null;
            target.RescheduleBlockedReason = blockedReason;

            return target;
        }

        private async Task EnsurePayerValidOnDateAsync(Guid encounterId, DateTime newDate, CancellationToken cancellationToken)
        {
            var source = await _dbContext.Set<RegPatientEncounterGuarantor>()
                .AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .Select(x => new { x.PaymentType, x.PatientInsuranceId, x.PatientCompanyGuarantorId })
                .FirstOrDefaultAsync(cancellationToken);

            if (source == null)
                return;

            if (source.PaymentType == EncounterPaymentType.Insurance && source.PatientInsuranceId.HasValue)
            {
                var insurance = await _dbContext.Set<MstPatientInsurance>()
                    .AsNoTracking()
                    .Where(x => x.Id == source.PatientInsuranceId.Value && !x.IsDelete)
                    .Select(x => new { x.IsActive, x.EffectiveStartDate, x.EffectiveEndDate })
                    .FirstOrDefaultAsync(cancellationToken);

                if (insurance == null || !insurance.IsActive)
                    throw new OutpatientEncounterValidationException("Asuransi pada appointment ini sudah tidak aktif. Batalkan dan daftarkan ulang dengan penjamin yang berlaku.");

                if ((insurance.EffectiveStartDate.HasValue && ToUtcDate(insurance.EffectiveStartDate.Value) > newDate) ||
                    (insurance.EffectiveEndDate.HasValue && ToUtcDate(insurance.EffectiveEndDate.Value) < newDate))
                    throw new OutpatientEncounterValidationException("Polis asuransi tidak berlaku pada tanggal kunjungan baru.");
            }
            else if (source.PaymentType == EncounterPaymentType.CompanyGuarantor && source.PatientCompanyGuarantorId.HasValue)
            {
                var guarantor = await _dbContext.Set<MstPatientCompanyGuarantor>()
                    .AsNoTracking()
                    .Where(x => x.Id == source.PatientCompanyGuarantorId.Value && !x.IsDelete)
                    .Select(x => new { x.IsActive, x.EffectiveStartDate, x.EffectiveEndDate })
                    .FirstOrDefaultAsync(cancellationToken);

                if (guarantor == null || !guarantor.IsActive)
                    throw new OutpatientEncounterValidationException("Penjamin perusahaan pada appointment ini sudah tidak aktif.");

                if ((guarantor.EffectiveStartDate.HasValue && ToUtcDate(guarantor.EffectiveStartDate.Value) > newDate) ||
                    (guarantor.EffectiveEndDate.HasValue && ToUtcDate(guarantor.EffectiveEndDate.Value) < newDate))
                    throw new OutpatientEncounterValidationException("Penjamin perusahaan tidak berlaku pada tanggal kunjungan baru.");
            }
        }

        private async Task NotifySafeAsync(Func<Task> notify, Guid queueId)
        {
            try
            {
                await notify();
            }
            catch (Exception exception)
            {
                await _loggerService.WarningAsync(LogCategory, "RescheduleOutpatientAppointment",
                    "Notifikasi antrean gagal dikirim.", new { EntityId = queueId, Error = exception.GetType().Name });
            }
        }

        /// <summary>Salinan keadaan antrean sebelum dipindah, untuk notifikasi batal ke tanggal lama.</summary>
        private static RegQueue SnapshotAsCancelled(RegQueue queue) => new()
        {
            Id = queue.Id,
            EncounterId = queue.EncounterId,
            PatientId = queue.PatientId,
            ServiceUnitId = queue.ServiceUnitId,
            ClinicId = queue.ClinicId,
            DoctorId = queue.DoctorId,
            DoctorScheduleId = queue.DoctorScheduleId,
            QueueDate = queue.QueueDate,
            QueueNumber = queue.QueueNumber,
            QueueCode = queue.QueueCode,
            QueueScopeKey = queue.QueueScopeKey,
            QueueStatus = QueueStatus.Cancelled,
            IsPriorityQueue = queue.IsPriorityQueue,
            QueuePriorityLevelSnapshot = queue.QueuePriorityLevelSnapshot,
            QueueAudienceSnapshot = queue.QueueAudienceSnapshot,
            PublicDisplayModeSnapshot = queue.PublicDisplayModeSnapshot,
            IsFromKiosk = queue.IsFromKiosk,
            IsWalkIn = queue.IsWalkIn,
            IsAppointment = queue.IsAppointment,
            IsScreeningRequired = queue.IsScreeningRequired,
            IsDoctorRequired = queue.IsDoctorRequired,
            CreateDateTime = queue.CreateDateTime
        };

        private static bool IsQueueProgressed(RegQueue queue) =>
            queue.LastNurseCalledAt.HasValue ||
            queue.ScreeningStartedAt.HasValue ||
            queue.LastDoctorCalledAt.HasValue ||
            (queue.QueueStatus != QueueStatus.WaitingForNurse && queue.QueueStatus != QueueStatus.WaitingForDoctor);

        /// <summary>Sama dengan aturan pendaftaran: berlaku efektif, lalu hari mingguan atau tanggal praktik.</summary>
        private static bool IsScheduleValidForDate(MstDoctorSchedule schedule, DateTime date)
        {
            if (schedule.EffectiveStartDate.HasValue && ToUtcDate(schedule.EffectiveStartDate.Value) > date)
                return false;

            if (schedule.EffectiveEndDate.HasValue && ToUtcDate(schedule.EffectiveEndDate.Value) < date)
                return false;

            if (schedule.ScheduleType == DoctorScheduleType.WeeklyRecurring)
                return schedule.PracticeDay == date.DayOfWeek;

            return schedule.PracticeDate.HasValue && ToUtcDate(schedule.PracticeDate.Value) == date;
        }

        /// <summary>Awalan kode antrean lama (poliklinik sama, jadi awalannya sama).</summary>
        private static string QueueCodePrefix(string? oldQueueCode)
        {
            var prefix = new string((oldQueueCode ?? string.Empty).TakeWhile(c => !char.IsDigit(c)).ToArray());
            return string.IsNullOrWhiteSpace(prefix) ? "Q" : prefix;
        }

        private static string? BuildScheduleLabel(string? sessionName, TimeSpan? start, TimeSpan? end)
        {
            var parts = new List<string?>
            {
                sessionName,
                start.HasValue && end.HasValue ? $"{start.Value:hh\\:mm}–{end.Value:hh\\:mm}" : null
            }.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

            return parts.Count == 0 ? null : string.Join(" · ", parts);
        }

        private static string TrimTo(string value, int max) => value.Length > max ? value[..max] : value;

        private static DateTime ToUtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

        /// <summary>Tanggal operasional WIB yang distempel tengah malam UTC, sama dengan kolom EncounterDate.</summary>
        private static DateTime Today() =>
            DateTime.SpecifyKind(AppDateTimeHelper.OperationalDate(), DateTimeKind.Utc);

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private static string GetDisplayName<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }
}
