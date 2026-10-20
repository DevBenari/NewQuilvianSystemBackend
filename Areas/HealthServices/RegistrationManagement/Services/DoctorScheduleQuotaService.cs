using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
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

            return rows.ToDictionary(
                x => x.DoctorScheduleId,
                x => new DoctorScheduleQuotaUsage(x.Total, x.WalkIn, x.Appointment));
        }

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
