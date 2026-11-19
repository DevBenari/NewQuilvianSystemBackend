using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Service penyedia dan pengelola parameter kebijakan operasional lembur rumah sakit (BE-HRD-132, HRD-DEC-117, HRD-DEC-118, HRD-DEC-121).
    /// </summary>
    /// <remarks>
    /// Register masalah SDM <c>MSL-10</c> (18 November 2026): parameter disimpan di tabel <c>HrdOvertimeSetting</c>
    /// (satu baris aktif per rumah sakit) dan setiap perubahan dicatat ke log audit platform beserta nilai sebelum dan
    /// sesudahnya. Selama migration <c>20261118100000_AddHrdOvertimeSetting</c> belum dijalankan, pembacaan memakai nilai
    /// bawaan (<see cref="OvertimeSettingDto.IsPersisted"/> = false) dan penyimpanan ditolak dengan pesan yang jelas,
    /// sehingga layar dan guard kuota tetap berjalan.
    /// </remarks>
    public class OvertimeSettingService : IOvertimeSettingService
    {
        public const string DefaultHospitalCode = "RSMMC";
        private const string UndefinedTableSqlState = "42P01";

        private readonly ApplicationDbContext _context;
        private readonly LoggerService _auditLogger;
        private readonly ILogger<OvertimeSettingService> _logger;

        public OvertimeSettingService(ApplicationDbContext context, LoggerService auditLogger, ILogger<OvertimeSettingService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OvertimeSettingDto> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await FindActiveAsync(asNoTracking: true, cancellationToken);
                return entity == null ? Defaults(isPersisted: false) : ToDto(entity);
            }
            catch (Exception ex) when (IsMissingTable(ex))
            {
                _logger.LogWarning("Tabel HrdOvertimeSetting belum ada; pengaturan lembur memakai nilai bawaan sampai migration dijalankan.");
                return Defaults(isPersisted: false);
            }
        }

        public async Task<OvertimeSettingDto> UpdateSettingsAsync(
            UpdateOvertimeSettingCommand command,
            Guid? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            Validate(command);

            HrdOvertimeSetting? entity;
            try
            {
                entity = await FindActiveAsync(asNoTracking: false, cancellationToken);
            }
            catch (Exception ex) when (IsMissingTable(ex))
            {
                throw new InvalidOperationException("Pengaturan lembur belum dapat disimpan permanen karena tabel pengaturan belum dibuat di basis data. Minta admin sistem menjalankan pembaruan basis data lebih dulu.");
            }

            var before = entity == null ? Defaults(isPersisted: false) : ToDto(entity);
            var now = DateTime.UtcNow;
            var actor = actorUserId ?? Guid.Empty;

            if (entity == null)
            {
                entity = new HrdOvertimeSetting { HospitalCode = DefaultHospitalCode, CreateDateTime = now, CreateBy = actor };
                _context.Set<HrdOvertimeSetting>().Add(entity);
            }

            entity.DailyMaxHoursRegular = command.DailyMaxHoursRegular;
            entity.WeeklyMaxHoursRegular = command.WeeklyMaxHoursRegular;
            entity.RoundingIntervalMinutes = command.RoundingIntervalMinutes;
            entity.MinimumThresholdMinutes = command.MinimumThresholdMinutes;
            entity.CtoValidityDays = command.CtoValidityDays;
            entity.MonthlyCutoffDay = command.MonthlyCutoffDay;
            entity.BiometricDiscrepancyTolerance = command.BiometricDiscrepancyTolerance;
            entity.PostClaimCitoGraceDays = command.PostClaimCitoGraceDays;
            entity.IsActive = true;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _context.SaveChangesAsync(cancellationToken);

            var after = ToDto(entity);
            await _auditLogger.AuditAsync("OvertimeSetting", "Update",
                $"Pengaturan operasional lembur {entity.HospitalCode} diubah.", new { Before = before, After = after });
            return after;
        }

        /// <summary>Batas wajar setiap parameter; pesan Bahasa Indonesia ditampilkan apa adanya di layar.</summary>
        private static void Validate(UpdateOvertimeSettingCommand c)
        {
            if (c.DailyMaxHoursRegular <= 0 || c.DailyMaxHoursRegular > 24)
                throw new ArgumentException("Batas lembur per hari harus lebih dari 0 dan paling banyak 24 jam.");
            if (c.WeeklyMaxHoursRegular <= 0 || c.WeeklyMaxHoursRegular > 168)
                throw new ArgumentException("Batas lembur per minggu harus lebih dari 0 dan paling banyak 168 jam.");
            if (c.WeeklyMaxHoursRegular < c.DailyMaxHoursRegular)
                throw new ArgumentException("Batas lembur per minggu tidak boleh lebih kecil dari batas per hari.");
            if (c.RoundingIntervalMinutes <= 0 || c.RoundingIntervalMinutes > 60)
                throw new ArgumentException("Interval pembulatan harus 1 sampai 60 menit.");
            if (c.MinimumThresholdMinutes <= 0 || c.MinimumThresholdMinutes > 240)
                throw new ArgumentException("Durasi minimal lembur harus 1 sampai 240 menit.");
            if (c.CtoValidityDays <= 0 || c.CtoValidityDays > 365)
                throw new ArgumentException("Masa berlaku libur pengganti (CTO) harus 1 sampai 365 hari.");
            if (c.MonthlyCutoffDay < 1 || c.MonthlyCutoffDay > 28)
                throw new ArgumentException("Tanggal tutup buku lembur harus antara 1 dan 28.");
            if (c.BiometricDiscrepancyTolerance <= 0 || c.BiometricDiscrepancyTolerance > 240)
                throw new ArgumentException("Toleransi selisih mesin presensi harus 1 sampai 240 menit.");
            if (c.PostClaimCitoGraceDays <= 0 || c.PostClaimCitoGraceDays > 14)
                throw new ArgumentException("Tenggat klaim lembur darurat harus 1 sampai 14 hari.");
        }

        private Task<HrdOvertimeSetting?> FindActiveAsync(bool asNoTracking, CancellationToken cancellationToken)
        {
            var query = _context.Set<HrdOvertimeSetting>().Where(x => x.HospitalCode == DefaultHospitalCode && x.IsActive && !x.IsDelete);
            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync(cancellationToken);
        }

        private static bool IsMissingTable(Exception ex)
        {
            for (var current = ex; current != null; current = current.InnerException)
            {
                if (current is PostgresException { SqlState: UndefinedTableSqlState }) return true;
            }

            return false;
        }

        private static OvertimeSettingDto Defaults(bool isPersisted) => new()
        {
            DailyMaxHoursRegular = 4m,
            WeeklyMaxHoursRegular = 18m,
            RoundingIntervalMinutes = 30,
            MinimumThresholdMinutes = 30,
            CtoValidityDays = 90,
            MonthlyCutoffDay = 20,
            BiometricDiscrepancyTolerance = 30,
            PostClaimCitoGraceDays = 1,
            IsActive = true,
            IsPersisted = isPersisted
        };

        private static OvertimeSettingDto ToDto(HrdOvertimeSetting e) => new()
        {
            DailyMaxHoursRegular = e.DailyMaxHoursRegular,
            WeeklyMaxHoursRegular = e.WeeklyMaxHoursRegular,
            RoundingIntervalMinutes = e.RoundingIntervalMinutes,
            MinimumThresholdMinutes = e.MinimumThresholdMinutes,
            CtoValidityDays = e.CtoValidityDays,
            MonthlyCutoffDay = e.MonthlyCutoffDay,
            BiometricDiscrepancyTolerance = e.BiometricDiscrepancyTolerance,
            PostClaimCitoGraceDays = e.PostClaimCitoGraceDays,
            IsActive = e.IsActive,
            IsPersisted = true,
            LastUpdatedAt = e.UpdateDateTime ?? e.CreateDateTime,
            LastUpdatedByUserId = e.UpdateBy == Guid.Empty ? null : e.UpdateBy
        };
    }
}
