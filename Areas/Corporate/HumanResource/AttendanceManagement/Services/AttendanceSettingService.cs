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
                    GracePeriodMinutes = 10,
                    MaxBackdatedCorrectionDays = 3,
                    RequireAttachmentForCorrection = true,
                    AutoConvertIncompleteToAbsent = true,
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
                AutoConvertIncompleteToAbsent = setting.AutoConvertIncompleteToAbsent,
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

            // Validasi batas kewajaran nilai (VR-ATT-17, VR-ATT-18)
            if (req.GracePeriodMinutes < 0 || req.GracePeriodMinutes > 60)
            {
                return AttendanceSettingServiceResult<AttendanceSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Toleransi keterlambatan (GracePeriodMinutes) harus berada dalam rentang 0 hingga 60 menit (VR-ATT-17).");
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
            setting.AutoConvertIncompleteToAbsent = req.AutoConvertIncompleteToAbsent;
            setting.UpdateDateTime = DateTime.UtcNow;
            setting.UpdateBy = currentUserId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Parameter presensi rumah sakit '{Code}' diperbarui oleh user {UserId}. GracePeriod: {Grace}m, MaxBackdated: {Back}d",
                code, currentUserId, req.GracePeriodMinutes, req.MaxBackdatedCorrectionDays);

            var response = new AttendanceSettingResponse
            {
                Id = setting.Id,
                HospitalCode = setting.HospitalCode,
                GracePeriodMinutes = setting.GracePeriodMinutes,
                MaxBackdatedCorrectionDays = setting.MaxBackdatedCorrectionDays,
                RequireAttachmentForCorrection = setting.RequireAttachmentForCorrection,
                AutoConvertIncompleteToAbsent = setting.AutoConvertIncompleteToAbsent,
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
        /// Mengambil nilai toleransi keterlambatan aktif dalam menit (default 10 menit per HRD-DEC-105).
        /// </summary>
        public async Task<int> GetEffectiveGracePeriodMinutesAsync(
            string hospitalCode = "RSMMC",
            CancellationToken cancellationToken = default)
        {
            var code = string.IsNullOrWhiteSpace(hospitalCode) ? "RSMMC" : hospitalCode.Trim().ToUpperInvariant();
            var setting = await _context.HrdAttendanceSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.HospitalCode == code && s.IsActive && !s.IsDelete, cancellationToken);

            return setting?.GracePeriodMinutes ?? 10;
        }

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
}
