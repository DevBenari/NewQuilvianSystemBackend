using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Pemakaian kuota satu jadwal dokter pada satu tanggal: jumlah antrean aktif total,
    /// walk-in, dan appointment.
    /// </summary>
    public readonly record struct DoctorScheduleQuotaUsage(int Total, int WalkIn, int Appointment);

    /// <summary>
    /// Satu-satunya penghitung kuota terpakai jadwal dokter (<c>RJ-REG-REV-BE-006</c>,
    /// <c>RJ-REG-DEC-010</c>). Dipakai validasi simpan kunjungan dan endpoint kuota di layar
    /// pendaftaran, sehingga keduanya tidak bisa berbeda. Antrean dihitung bila tanggal antrean dan
    /// jadwalnya cocok serta antrean tidak dihapus dan tidak dibatalkan.
    /// </summary>
    public class DoctorScheduleQuotaService
    {
        private readonly ApplicationDbContext _dbContext;

        public DoctorScheduleQuotaService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static bool IsFull(int max, int used) => max > 0 && used >= max;

        /// <summary>
        /// Apakah jadwal berlaku pada tanggal kunjungan: masa berlaku, lalu hari praktik (mingguan) atau
        /// tanggal praktik (sekali). Satu aturan untuk jalur kunjungan dan opsi jadwal halaman publik
        /// Pendaftaran Pasien Online (<c>BE-KSK-PO-004</c>), dipindah dari
        /// <c>PatientEncounterController.IsDoctorScheduleValidForVisitDate</c> tanpa perubahan.
        /// </summary>
        public static bool IsScheduleValidForVisitDate(MstDoctorSchedule schedule, DateTime visitDate)
        {
            var normalizedVisitDate = ToUtcDate(visitDate);

            if (schedule.EffectiveStartDate.HasValue &&
                ToUtcDate(schedule.EffectiveStartDate.Value) > normalizedVisitDate)
            {
                return false;
            }

            if (schedule.EffectiveEndDate.HasValue &&
                ToUtcDate(schedule.EffectiveEndDate.Value) < normalizedVisitDate)
            {
                return false;
            }

            if (schedule.ScheduleType == DoctorScheduleType.WeeklyRecurring)
            {
                return schedule.PracticeDay == normalizedVisitDate.DayOfWeek;
            }

            return schedule.PracticeDate.HasValue &&
                ToUtcDate(schedule.PracticeDate.Value) == normalizedVisitDate;
        }

        private static DateTime ToUtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

        public static bool IsFullForWalkIn(MstDoctorSchedule schedule, DoctorScheduleQuotaUsage usage) =>
            IsFull(schedule.MaxPatientQuota, usage.Total) || IsFull(schedule.MaxWalkInQuota, usage.WalkIn);

        public static bool IsFullForAppointment(MstDoctorSchedule schedule, DoctorScheduleQuotaUsage usage) =>
            IsFull(schedule.MaxPatientQuota, usage.Total) || IsFull(schedule.MaxAppointmentQuota, usage.Appointment);

        public async Task<DoctorScheduleQuotaUsage> GetUsageAsync(
            Guid doctorScheduleId,
            DateTime visitDate,
            CancellationToken cancellationToken = default)
        {
            var usages = await GetUsagesAsync(new[] { doctorScheduleId }, visitDate, cancellationToken);
            return usages.TryGetValue(doctorScheduleId, out var usage) ? usage : default;
        }

        public async Task<Dictionary<Guid, DoctorScheduleQuotaUsage>> GetUsagesAsync(
            IReadOnlyCollection<Guid> doctorScheduleIds,
            DateTime visitDate,
            CancellationToken cancellationToken = default)
        {
            if (doctorScheduleIds.Count == 0)
            {
                return new Dictionary<Guid, DoctorScheduleQuotaUsage>();
            }

            var normalizedVisitDate = DateTime.SpecifyKind(visitDate.Date, DateTimeKind.Utc);

            var rows = await _dbContext.Set<RegQueue>()
                .AsNoTracking()
                .Where(x =>
                    x.QueueDate == normalizedVisitDate &&
                    x.DoctorScheduleId.HasValue &&
                    doctorScheduleIds.Contains(x.DoctorScheduleId.Value) &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    !x.CancelledAt.HasValue)
                .GroupBy(x => x.DoctorScheduleId!.Value)
                .Select(g => new
                {
                    DoctorScheduleId = g.Key,
                    Total = g.Count(),
                    WalkIn = g.Count(x => x.IsWalkIn),
                    Appointment = g.Count(x => x.IsAppointment),
                })
                .ToListAsync(cancellationToken);

            var usages = rows.ToDictionary(
                x => x.DoctorScheduleId,
                x => new DoctorScheduleQuotaUsage(x.Total, x.WalkIn, x.Appointment));

            // kiosk Amendment PO (BE-KSK-PO-002, INV-PO-02, KSK-DEC-030): pendaftaran online yang
            // masih Menunggu Check-in sudah memakai satu kuota appointment dan satu kuota total.
            // Status lain tidak dihitung: Menjadi Kunjungan sudah diwakili antreannya sendiri,
            // sedangkan Jalur Lain, Tidak Datang, dan Dibatalkan melepas kuotanya.
            var calendarDate = DateTime.SpecifyKind(visitDate.Date, DateTimeKind.Unspecified);

            var onlineRows = await _dbContext.Set<RegOnlineRegistration>()
                .AsNoTracking()
                .Where(x =>
                    x.VisitDate == calendarDate &&
                    doctorScheduleIds.Contains(x.DoctorScheduleId) &&
                    x.Status == OnlineRegistrationStatus.AwaitingCheckIn &&
                    !x.IsDelete)
                .GroupBy(x => x.DoctorScheduleId)
                .Select(g => new { DoctorScheduleId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var online in onlineRows)
            {
                var current = usages.TryGetValue(online.DoctorScheduleId, out var found) ? found : default;
                usages[online.DoctorScheduleId] = current with
                {
                    Total = current.Total + online.Count,
                    Appointment = current.Appointment + online.Count
                };
            }

            return usages;
        }

        /// <summary>
        /// Kunci kuota satu jadwal dokter pada satu tanggal (kiosk Amendment PO, <c>BE-KSK-PO-002</c>,
        /// <c>KSK-DSN-PO-009</c>). Dipegang database sampai transaksi pemanggil selesai, sehingga dua
        /// permintaan yang berebut slot terakhir diproses bergantian. Wajib dipanggil di dalam
        /// transaksi, <b>sesudah</b> kunci pasien, lalu kuota dihitung ulang.
        /// </summary>
        public async Task AcquireQuotaLockAsync(
            Guid doctorScheduleId,
            DateTime visitDate,
            CancellationToken cancellationToken = default)
        {
            if (!_dbContext.Database.IsNpgsql())
            {
                return;
            }

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [BuildQuotaLockKey(doctorScheduleId, visitDate)],
                cancellationToken);
        }

        public static string BuildQuotaLockKey(Guid doctorScheduleId, DateTime visitDate) =>
            $"REG_SCHEDULE_QUOTA_{doctorScheduleId:N}_{visitDate:yyyyMMdd}";

        /// <summary>
        /// Kuota terpakai jadwal dokter aktif pada tanggal kunjungan, opsional per poliklinik.
        /// </summary>
        public async Task<List<DoctorScheduleQuotaResponse>> GetQuotasAsync(
            DateTime visitDate,
            Guid? clinicId,
            CancellationToken cancellationToken = default)
        {
            var scheduleQuery = _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            if (clinicId.HasValue)
            {
                scheduleQuery = scheduleQuery.Where(x => x.ClinicId == clinicId.Value);
            }

            var schedules = await scheduleQuery.ToListAsync(cancellationToken);
            var usages = await GetUsagesAsync(
                schedules.Select(x => x.Id).ToList(),
                visitDate,
                cancellationToken);

            return schedules
                .Select(schedule =>
                {
                    var usage = usages.TryGetValue(schedule.Id, out var found) ? found : default;
                    return new DoctorScheduleQuotaResponse
                    {
                        DoctorScheduleId = schedule.Id,
                        MaxPatientQuota = schedule.MaxPatientQuota,
                        MaxWalkInQuota = schedule.MaxWalkInQuota,
                        MaxAppointmentQuota = schedule.MaxAppointmentQuota,
                        UsedTotal = usage.Total,
                        UsedWalkIn = usage.WalkIn,
                        UsedAppointment = usage.Appointment,
                        IsFullForWalkIn = IsFullForWalkIn(schedule, usage),
                        IsFullForAppointment = IsFullForAppointment(schedule, usage),
                    };
                })
                .ToList();
        }
    }
}
