using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Ganti jadwal kunjungan dari form Perbarui Daftar Pasien Rawat Jalan: tanggal kunjungan
    /// dan sesi praktik <b>dokter yang sama</b> di poliklinik yang sama. Dokter tidak dapat diganti.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Hanya yang belum datang.</b> Belum batal/selesai/tidak hadir, belum check-in, status ≤
    /// Menunggu Dokter selain Screening Perawat, dan antreannya belum dipanggil maupun diskrining.
    /// </para>
    /// <para>
    /// <b>Konsistensi.</b> Dipanggil di dalam transaksi pemanggil yang sudah mengunci baris kunjungan
    /// <c>FOR UPDATE</c>; kunci advisory per pasien sama dengan pendaftaran. Antrean hidup kunjungan
    /// dipindah (ID tetap) dengan nomor baru dari <see cref="OutpatientQueueNumberAllocator"/>, kuota
    /// dihitung <see cref="DoctorScheduleQuotaService"/> — keduanya sama dengan pendaftaran.
    /// </para>
    /// </remarks>
    public class OutpatientEncounterScheduleService
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";

        /// <summary>Rentang jadwal tersedia: besok sampai 30 hari ke depan (sama dengan rentang Booking di daftar).</summary>
        public const int AvailableSlotDays = 30;

        private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");

        private readonly ApplicationDbContext _dbContext;
        private readonly OutpatientEncounterListService _listService;
        private readonly OutpatientQueueClassificationService _classificationService;
        private readonly OutpatientQueueNumberAllocator _queueNumberAllocator;
        private readonly DoctorScheduleQuotaService _quotaService;
        private readonly QueueRealtimeService _queueRealtimeService;
        private readonly LoggerService _loggerService;

        public OutpatientEncounterScheduleService(
            ApplicationDbContext dbContext,
            OutpatientEncounterListService listService,
            OutpatientQueueClassificationService classificationService,
            OutpatientQueueNumberAllocator queueNumberAllocator,
            DoctorScheduleQuotaService quotaService,
            QueueRealtimeService queueRealtimeService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _listService = listService;
            _classificationService = classificationService;
            _queueNumberAllocator = queueNumberAllocator;
            _quotaService = quotaService;
            _queueRealtimeService = queueRealtimeService;
            _loggerService = loggerService;
        }

        /// <summary>Hasil ganti jadwal yang notifikasinya dikirim pemanggil sesudah commit.</summary>
        public sealed class ScheduleChange
        {
            public List<RegQueue> CancelledQueues { get; } = new();
            public RegQueue? MovedQueue { get; set; }
            public DateTime OldEncounterDate { get; init; }
            public Guid? OldDoctorScheduleId { get; init; }
        }

        /// <summary>
        /// Alasan jadwal tidak dapat diganti, atau <c>null</c> bila boleh.
        /// </summary>
        public static string? GetBlockedReason(
            bool isCancelled,
            bool isClosed,
            bool isCheckedIn,
            EncounterStatus status,
            bool hasDoctor,
            bool queueProgressed)
        {
            if (isCancelled)
                return "Kunjungan sudah dibatalkan sehingga jadwalnya tidak dapat diubah.";

            if (isClosed || status > EncounterStatus.WaitingForDoctor)
                return $"Kunjungan dengan status {GetDisplayName(status)} tidak dapat diubah jadwalnya.";

            if (isCheckedIn || queueProgressed || status == EncounterStatus.InNurseScreening)
                return "Pasien sudah datang dan diproses, sehingga jadwalnya tidak dapat diubah.";

            if (!hasDoctor)
                return "Kunjungan belum memiliki dokter, sehingga jadwal praktik tidak dapat dipilih.";

            return null;
        }

        /// <summary>Alasan blokir untuk kunjungan yang sudah dimuat beserta status antreannya.</summary>
        public async Task<string?> GetBlockedReasonAsync(RegPatientEncounter encounter, CancellationToken cancellationToken)
        {
            var queueProgressed = await ActiveQueues(encounter.Id)
                .AnyAsync(q =>
                    q.LastNurseCalledAt != null ||
                    q.ScreeningStartedAt != null ||
                    q.LastDoctorCalledAt != null ||
                    (q.QueueStatus != QueueStatus.WaitingForNurse && q.QueueStatus != QueueStatus.WaitingForDoctor),
                    cancellationToken);

            return GetBlockedReason(
                encounter.IsCancel || encounter.CancelledAt.HasValue,
                encounter.CompletedAt.HasValue || encounter.NoShowAt.HasValue,
                encounter.CheckedInAt.HasValue,
                encounter.EncounterStatus,
                encounter.DoctorId.HasValue,
                queueProgressed);
        }

        /// <summary>
        /// Jadwal tersedia dokter kunjungan (dokter, poliklinik, dan unit tetap) mulai besok sampai
        /// <see cref="AvailableSlotDays"/> hari ke depan: setiap pasangan tanggal + sesi praktik yang
        /// kuotanya belum penuh, urut tanggal lalu jam. Jadwal kunjungan saat ini selalu ikut dan
        /// ditandai, supaya pilihan awal form tetap terisi.
        /// </summary>
        public async Task<List<OutpatientEncounterScheduleOptionResponse>> GetAvailableSlotsAsync(
            ClaimsPrincipal user,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var scope = await _listService.ResolveScopeOrThrowAsync(user, cancellationToken);

            var encounter = await _listService.BuildScopedQuery(scope)
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.DoctorId,
                    x.ClinicId,
                    x.ServiceUnitId,
                    x.DoctorScheduleId,
                    x.EncounterDate,
                    x.IsAppointment,
                    x.IsWalkIn,
                    ScheduleSessionName = x.DoctorSchedule != null ? x.DoctorSchedule.SessionName : null,
                    ScheduleStartTime = x.DoctorSchedule != null ? (TimeSpan?)x.DoctorSchedule.StartTime : null,
                    ScheduleEndTime = x.DoctorSchedule != null ? (TimeSpan?)x.DoctorSchedule.EndTime : null
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new OutpatientEncounterNotFoundException();

            var result = new List<OutpatientEncounterScheduleOptionResponse>();
            if (!encounter.DoctorId.HasValue)
                return result;

            var currentDate = ToUtcDate(encounter.EncounterDate);
            var schedules = await CandidateSchedules(encounter.DoctorId.Value, encounter.ClinicId!.Value, encounter.ServiceUnitId)
                .ToListAsync(cancellationToken);

            var today = Today();
            var currentIncluded = false;

            for (var offset = 1; offset <= AvailableSlotDays; offset++)
            {
                var date = today.AddDays(offset);
                var daySchedules = schedules
                    .Where(x => IsScheduleValidForDate(x, date))
                    .OrderBy(x => x.StartTime)
                    .ToList();

                if (daySchedules.Count == 0)
                    continue;

                var usages = await _quotaService.GetUsagesAsync(daySchedules.Select(x => x.Id).ToList(), date, cancellationToken);

                foreach (var schedule in daySchedules)
                {
                    usages.TryGetValue(schedule.Id, out var usage);
                    var isCurrent = date == currentDate && schedule.Id == encounter.DoctorScheduleId;
                    var fullReason = GetQuotaFullReason(schedule, usage, encounter.IsAppointment, encounter.IsWalkIn);

                    if (!isCurrent && (fullReason != null || (encounter.IsAppointment && !schedule.IsAllowAppointment)))
                        continue;

                    currentIncluded |= isCurrent;
                    result.Add(BuildSlot(date, schedule.Id, schedule.SessionName, schedule.StartTime, schedule.EndTime,
                        schedule.MaxPatientQuota, usage.Total, isCurrent));
                }
            }

            // Jadwal saat ini di luar rentang (hari ini, lampau, atau sesi tak aktif) tetap ditampilkan.
            if (!currentIncluded && encounter.DoctorScheduleId.HasValue)
            {
                result.Insert(0, BuildSlot(currentDate, encounter.DoctorScheduleId.Value, encounter.ScheduleSessionName,
                    encounter.ScheduleStartTime, encounter.ScheduleEndTime, 0, 0, true));
            }

            return result;
        }

        private static OutpatientEncounterScheduleOptionResponse BuildSlot(
            DateTime date,
            Guid scheduleId,
            string? sessionName,
            TimeSpan? start,
            TimeSpan? end,
            int maxQuota,
            int usedQuota,
            bool isCurrent) => new()
        {
            Value = $"{date:yyyy-MM-dd}|{scheduleId}",
            VisitDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DoctorScheduleId = scheduleId,
            Label = string.Join(" · ", new[]
            {
                date.ToString("ddd, dd MMM yyyy", IndonesianCulture),
                BuildScheduleLabel(sessionName, start, end)
            }.Where(x => !string.IsNullOrWhiteSpace(x))),
            MaxPatientQuota = maxQuota,
            UsedPatientQuota = usedQuota,
            IsCurrent = isCurrent
        };

        /// <summary>
        /// Memindah kunjungan yang sudah terkunci ke tanggal dan sesi praktik baru dokter yang sama.
        /// Mengembalikan <c>null</c> bila tanggal dan jadwal tidak berubah.
        /// </summary>
        public async Task<ScheduleChange?> ApplyAsync(
            RegPatientEncounter encounter,
            string? visitDate,
            Guid? doctorScheduleId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(visitDate) && !doctorScheduleId.HasValue)
                return null;

            if (string.IsNullOrWhiteSpace(visitDate) || !doctorScheduleId.HasValue || doctorScheduleId.Value == Guid.Empty)
                throw new OutpatientEncounterValidationException("Tanggal kunjungan dan jadwal praktik dokter wajib diisi bersamaan.");

            var newDate = ParseVisitDate(visitDate);
            var scheduleId = doctorScheduleId.Value;

            if (ToUtcDate(encounter.EncounterDate) == newDate && encounter.DoctorScheduleId == scheduleId)
                return null;

            if (newDate < Today())
                throw new OutpatientEncounterValidationException("Tanggal kunjungan baru tidak boleh sebelum hari ini.");

            // Kunci yang sama dengan pendaftaran (RJ-DOC-REV-BE-007).
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));",
                    [$"REG_ENCOUNTER_ACTIVE_{encounter.PatientId:N}"],
                    cancellationToken);
            }

            var activeQueues = await ActiveQueues(encounter.Id).ToListAsync(cancellationToken);

            var blockedReason = GetBlockedReason(
                encounter.IsCancel || encounter.CancelledAt.HasValue,
                encounter.CompletedAt.HasValue || encounter.NoShowAt.HasValue,
                encounter.CheckedInAt.HasValue,
                encounter.EncounterStatus,
                encounter.DoctorId.HasValue,
                activeQueues.Any(IsQueueProgressed));

            if (blockedReason != null)
                throw new OutpatientEncounterValidationException(blockedReason);

            var schedule = await CandidateSchedules(encounter.DoctorId!.Value, encounter.ClinicId!.Value, encounter.ServiceUnitId)
                .FirstOrDefaultAsync(x => x.Id == scheduleId, cancellationToken);

            if (schedule == null)
                throw new OutpatientEncounterValidationException("Jadwal praktik tidak valid, tidak aktif, atau bukan milik dokter dan poliklinik kunjungan ini. Dokter tidak dapat diganti.");

            if (!IsScheduleValidForDate(schedule, newDate))
                throw new OutpatientEncounterValidationException("Jadwal praktik dokter tidak sesuai dengan tanggal kunjungan baru.");

            if (encounter.IsAppointment && !schedule.IsAllowAppointment)
                throw new OutpatientEncounterValidationException("Jadwal praktik dokter tidak menerima appointment.");

            var usage = await _quotaService.GetUsageAsync(schedule.Id, newDate, cancellationToken);
            var quotaReason = GetQuotaFullReason(schedule, usage, encounter.IsAppointment, encounter.IsWalkIn);
            if (quotaReason != null)
                throw new OutpatientEncounterValidationException(quotaReason);

            var clash = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id != encounter.Id &&
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
                throw new OutpatientEncounterValidationException(
                    $"Pasien sudah memiliki kunjungan {clash} di poliklinik yang sama pada tanggal itu.");

            await EnsurePayerValidOnDateAsync(encounter.Id, newDate, cancellationToken);

            var change = new ScheduleChange
            {
                OldEncounterDate = encounter.EncounterDate,
                OldDoctorScheduleId = encounter.DoctorScheduleId
            };

            // Satu kunjungan hanya boleh punya satu antrean hidup (unique IX_RegQueue_EncounterId),
            // jadi antrean yang sama dipindah: ID tetap, tanggal/nomor/kode/jadwal baru. Salinan
            // keadaan lama dikirim sebagai notifikasi batal ke layar antrean tanggal lama.
            var queue = activeQueues.OrderByDescending(x => x.CreateDateTime).FirstOrDefault();

            foreach (var extra in activeQueues.Where(x => queue == null || x.Id != queue.Id))
            {
                extra.QueueStatus = QueueStatus.Cancelled;
                extra.CancelledAt = now;
                extra.CancelledByUserId = actorUserId;
                extra.CancelReason = "Jadwal kunjungan diubah.";
                extra.IsCancel = true;
                extra.CancelDateTime = now;
                extra.CancelBy = actorUserId;
                extra.IsActive = false;
                extra.UpdateDateTime = now;
                extra.UpdateBy = actorUserId;
                change.CancelledQueues.Add(extra);
            }

            if (queue != null)
            {
                change.CancelledQueues.Add(SnapshotAsCancelled(queue));

                // RJ-PRI-DEC-007: status member dinilai pada hari perubahan jadwal (WIB), sama
                // seperti pendaftaran baru, bukan pada tanggal kunjungan yang baru.
                var classification = await _classificationService.ClassifyAsync(
                    encounter.PatientId,
                    classificationDateWib: null,
                    cancellationToken);
                var allocation = await _queueNumberAllocator.AllocateAsync(
                    newDate,
                    encounter.ServiceUnitId,
                    encounter.ClinicId,
                    classification.IsPriorityQueue,
                    cancellationToken);

                queue.DoctorScheduleId = schedule.Id;
                queue.QueueDate = newDate;
                queue.QueueNumber = allocation.QueueNumber;
                queue.QueueCode = $"{QueueCodePrefix(queue.QueueCode)}{allocation.QueueNumber:D3}";
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
                change.MovedQueue = queue;

                encounter.EncounterStatus = encounter.IsScreeningRequired
                    ? EncounterStatus.WaitingForNurse
                    : EncounterStatus.WaitingForDoctor;
            }

            encounter.EncounterDate = newDate;
            encounter.DoctorScheduleId = schedule.Id;
            encounter.RoomId = schedule.RoomId ?? encounter.RoomId;
            encounter.UpdateDateTime = now;
            encounter.UpdateBy = actorUserId;

            return change;
        }

        /// <summary>Notifikasi antrean sesudah commit; kegagalannya tidak membatalkan perubahan.</summary>
        public async Task NotifyAsync(ScheduleChange change, Guid actorUserId)
        {
            foreach (var queue in change.CancelledQueues)
                await NotifySafeAsync(() => _queueRealtimeService.NotifyQueueCancelledAsync(queue, actorUserId, "Jadwal kunjungan diubah."), queue.Id);

            if (change.MovedQueue != null)
                await NotifySafeAsync(() => _queueRealtimeService.NotifyQueueCreatedAsync(change.MovedQueue, actorUserId, "Antrean hasil ubah jadwal kunjungan."), change.MovedQueue.Id);
        }

        private IQueryable<RegQueue> ActiveQueues(Guid encounterId) =>
            _dbContext.Set<RegQueue>()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete && !x.IsCancel && !x.CancelledAt.HasValue && !x.CompletedAt.HasValue);

        /// <summary>Jadwal aktif dokter kunjungan di poliklinik dan unit yang sama — dokter tetap.</summary>
        private IQueryable<MstDoctorSchedule> CandidateSchedules(Guid doctorId, Guid clinicId, Guid serviceUnitId) =>
            _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.IsActive &&
                            x.ScheduleStatus == DoctorScheduleStatus.Active &&
                            x.DoctorId == doctorId &&
                            x.ClinicId == clinicId &&
                            x.ServiceUnitId == serviceUnitId);

        private static string? GetQuotaFullReason(MstDoctorSchedule schedule, DoctorScheduleQuotaUsage usage, bool isAppointment, bool isWalkIn)
        {
            if (DoctorScheduleQuotaService.IsFull(schedule.MaxPatientQuota, usage.Total))
                return "Kuota pasien pada jadwal dokter sudah penuh.";

            if (isAppointment && DoctorScheduleQuotaService.IsFull(schedule.MaxAppointmentQuota, usage.Appointment))
                return "Kuota appointment pada jadwal dokter sudah penuh.";

            if (isWalkIn && DoctorScheduleQuotaService.IsFull(schedule.MaxWalkInQuota, usage.WalkIn))
                return "Kuota walk-in pada jadwal dokter sudah penuh.";

            return null;
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
                    throw new OutpatientEncounterValidationException("Asuransi pada kunjungan ini sudah tidak aktif. Batalkan dan daftarkan ulang dengan penjamin yang berlaku.");

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
                    throw new OutpatientEncounterValidationException("Penjamin perusahaan pada kunjungan ini sudah tidak aktif.");

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
                await _loggerService.WarningAsync(LogCategory, "ChangeOutpatientEncounterSchedule",
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

        private static DateTime ParseVisitDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                !DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw new OutpatientEncounterValidationException("Tanggal kunjungan wajib diisi dengan format yyyy-MM-dd.");

            return DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
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

        private static DateTime ToUtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

        private static DateTime Today() =>
            DateTime.SpecifyKind(AppDateTimeHelper.OperationalDate(), DateTimeKind.Utc);

        private static string GetDisplayName<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }
}
