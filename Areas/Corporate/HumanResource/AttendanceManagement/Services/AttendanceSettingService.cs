using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceSettingServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static AttendanceSettingServiceResult<T> Ok(
            T data,
            string message = "Berhasil.",
            int statusCode = StatusCodes.Status200OK)
        {
            return new AttendanceSettingServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static AttendanceSettingServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new AttendanceSettingServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }

    /// <summary>
    /// Service tata kelola parameter kebijakan presensi dan kehadiran rumah sakit (BE-HRD-105, HRD-DEC-105, HRD-DEC-106).
    /// </summary>
    public class AttendanceSettingService
    {
        public const string DefaultHospitalCode = "RSMMC";
        public const string SettingUnreadableMessage = "Setelan kehadiran tidak terbaca";

        // Batas nilai setelan (VR-PK-23).
        private const int MaxGracePeriodMinutes = 60;
        private const int MaxScanWindowBeforeMinutes = 720;
        private const int MaxScanWindowAfterMinutes = 960;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceSettingService> _logger;

        public AttendanceSettingService(
            ApplicationDbContext context,
            ILogger<AttendanceSettingService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AttendanceSettingServiceResult<AttendanceSettingResponse>> GetSettingAsync(
            string hospitalCode = "RSMMC",
            CancellationToken cancellationToken = default)
        {
            var code = string.IsNullOrWhiteSpace(hospitalCode) ? "RSMMC" : hospitalCode.Trim().ToUpperInvariant();

            var setting = await _context.HrdAttendanceSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.HospitalCode == code && s.IsActive && !s.IsDelete, cancellationToken);

            if (setting == null)
            {
                // Auto-provision default setting bila belum ada
                var defaultSetting = new HrdAttendanceSetting
                {
                    Id = Guid.NewGuid(),
                    HospitalCode = code,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.HrdAttendanceSettings.Add(defaultSetting);
                await _context.SaveChangesAsync(cancellationToken);
                setting = defaultSetting;
            }

            var response = new AttendanceSettingResponse
            {
                Id = setting.Id,
                HospitalCode = setting.HospitalCode,
                GracePeriodMinutes = setting.GracePeriodMinutes,
                MaxBackdatedCorrectionDays = setting.MaxBackdatedCorrectionDays,
                RequireAttachmentForCorrection = setting.RequireAttachmentForCorrection,
                ScanWindowBeforeMinutes = setting.ScanWindowBeforeMinutes,
                ScanWindowAfterMinutes = setting.ScanWindowAfterMinutes,
                IsActive = setting.IsActive,
                CreatedAt = setting.CreateDateTime,
                CreatedBy = setting.CreateBy,
                UpdatedAt = setting.UpdateDateTime,
                UpdatedBy = setting.UpdateBy
            };

            return AttendanceSettingServiceResult<AttendanceSettingResponse>.Ok(
                response,
                "Konfigurasi kebijakan presensi rumah sakit berhasil diambil.");
        }

        public async Task<AttendanceSettingServiceResult<AttendanceSettingResponse>> UpdateSettingAsync(
            string hospitalCode,
            UpdateAttendanceSettingRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var code = string.IsNullOrWhiteSpace(hospitalCode) ? "RSMMC" : hospitalCode.Trim().ToUpperInvariant();

            // VR-PK-23: toleransi dan jendela scan harus di dalam batas.
            if (!IsWithin(req.GracePeriodMinutes, MaxGracePeriodMinutes) ||
                (req.ScanWindowBeforeMinutes.HasValue && !IsWithin(req.ScanWindowBeforeMinutes.Value, MaxScanWindowBeforeMinutes)) ||
                (req.ScanWindowAfterMinutes.HasValue && !IsWithin(req.ScanWindowAfterMinutes.Value, MaxScanWindowAfterMinutes)))
            {
                return AttendanceSettingServiceResult<AttendanceSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nilai toleransi atau jendela scan di luar batas.");
            }

            if (req.MaxBackdatedCorrectionDays < 1 || req.MaxBackdatedCorrectionDays > 30)
            {
                return AttendanceSettingServiceResult<AttendanceSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Batas toleransi koreksi tanggal mundur (MaxBackdatedCorrectionDays) harus antara 1 hingga 30 hari (VR-ATT-18).");
            }

            var setting = await _context.HrdAttendanceSettings
                .FirstOrDefaultAsync(s => s.HospitalCode == code && s.IsActive && !s.IsDelete, cancellationToken);

            if (setting == null)
            {
                setting = new HrdAttendanceSetting
                {
                    Id = Guid.NewGuid(),
                    HospitalCode = code,
                    CreateBy = currentUserId,
                    CreateDateTime = DateTime.UtcNow
                };
                _context.HrdAttendanceSettings.Add(setting);
            }

            setting.GracePeriodMinutes = req.GracePeriodMinutes;
            setting.MaxBackdatedCorrectionDays = req.MaxBackdatedCorrectionDays;
            setting.RequireAttachmentForCorrection = req.RequireAttachmentForCorrection;
            // HRD-DEC-216: pilihan konversi otomatis tidak lagi diterima maupun dipakai. Jendela yang tidak dikirim
            // (layar lama) mempertahankan nilai tersimpan.
            if (req.ScanWindowBeforeMinutes.HasValue) setting.ScanWindowBeforeMinutes = req.ScanWindowBeforeMinutes.Value;
            if (req.ScanWindowAfterMinutes.HasValue) setting.ScanWindowAfterMinutes = req.ScanWindowAfterMinutes.Value;
            setting.UpdateDateTime = DateTime.UtcNow;
            setting.UpdateBy = currentUserId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Parameter presensi rumah sakit '{Code}' diperbarui oleh user {UserId}. GracePeriod: {Grace}m, ScanWindow: {Before}/{After}m, MaxBackdated: {Back}d",
                code, currentUserId, setting.GracePeriodMinutes, setting.ScanWindowBeforeMinutes, setting.ScanWindowAfterMinutes, req.MaxBackdatedCorrectionDays);

            var response = new AttendanceSettingResponse
            {
                Id = setting.Id,
                HospitalCode = setting.HospitalCode,
                GracePeriodMinutes = setting.GracePeriodMinutes,
                MaxBackdatedCorrectionDays = setting.MaxBackdatedCorrectionDays,
                RequireAttachmentForCorrection = setting.RequireAttachmentForCorrection,
                ScanWindowBeforeMinutes = setting.ScanWindowBeforeMinutes,
                ScanWindowAfterMinutes = setting.ScanWindowAfterMinutes,
                IsActive = setting.IsActive,
                CreatedAt = setting.CreateDateTime,
                CreatedBy = setting.CreateBy,
                UpdatedAt = setting.UpdateDateTime,
                UpdatedBy = setting.UpdateBy
            };

            return AttendanceSettingServiceResult<AttendanceSettingResponse>.Ok(
                response,
                "Konfigurasi kebijakan presensi rumah sakit berhasil diperbarui.");
        }

        /// <summary>
        /// Toleransi terlambat dan jendela scan yang berlaku (HRD-DEC-215, HRD-DEC-211). Satu-satunya sumber bagi
        /// pemrosesan kehadiran. Gagal bila setelan tidak ada, tidak dapat dibaca, atau nilainya di luar batas
        /// VR-PK-23 — tidak pernah memakai angka cadangan (HRD-ASM-83).
        /// </summary>
        public async Task<AttendanceSettingServiceResult<AttendanceRuleSetting>> GetRequiredSettingAsync(
            string hospitalCode = DefaultHospitalCode,
            CancellationToken cancellationToken = default)
        {
            var code = string.IsNullOrWhiteSpace(hospitalCode) ? DefaultHospitalCode : hospitalCode.Trim().ToUpperInvariant();
            try
            {
                var setting = await _context.HrdAttendanceSettings
                    .AsNoTracking()
                    .Where(s => s.HospitalCode == code && s.IsActive && !s.IsDelete)
                    .Select(s => new AttendanceRuleSetting(s.GracePeriodMinutes, s.ScanWindowBeforeMinutes, s.ScanWindowAfterMinutes))
                    .FirstOrDefaultAsync(cancellationToken);

                if (setting == null ||
                    !IsWithin(setting.GracePeriodMinutes, MaxGracePeriodMinutes) ||
                    !IsWithin(setting.ScanWindowBeforeMinutes, MaxScanWindowBeforeMinutes) ||
                    !IsWithin(setting.ScanWindowAfterMinutes, MaxScanWindowAfterMinutes))
                {
                    return AttendanceSettingServiceResult<AttendanceRuleSetting>.Fail(StatusCodes.Status422UnprocessableEntity, SettingUnreadableMessage);
                }

                return AttendanceSettingServiceResult<AttendanceRuleSetting>.Ok(setting);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Setelan kehadiran {Code} gagal dibaca.", code);
                return AttendanceSettingServiceResult<AttendanceRuleSetting>.Fail(StatusCodes.Status503ServiceUnavailable, SettingUnreadableMessage);
            }
        }

        private static bool IsWithin(int value, int max) => value >= 0 && value <= max;

        /// <summary>
        /// Mengambil batas maksimal hari toleransi koreksi tanggal mundur (default 3 hari per HRD-DEC-102).
        /// </summary>
        public async Task<int> GetEffectiveMaxBackdatedCorrectionDaysAsync(
            string hospitalCode = "RSMMC",
            CancellationToken cancellationToken = default)
        {
            var code = string.IsNullOrWhiteSpace(hospitalCode) ? "RSMMC" : hospitalCode.Trim().ToUpperInvariant();
            var setting = await _context.HrdAttendanceSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.HospitalCode == code && s.IsActive && !s.IsDelete, cancellationToken);

            return setting?.MaxBackdatedCorrectionDays ?? 3;
        }
    }

    /// <summary>
    /// Angka setelan kehadiran yang dipakai pemrosesan: toleransi terlambat dan jendela scan (menit).
    /// </summary>
    public sealed record AttendanceRuleSetting(int GracePeriodMinutes, int ScanWindowBeforeMinutes, int ScanWindowAfterMinutes);
}
