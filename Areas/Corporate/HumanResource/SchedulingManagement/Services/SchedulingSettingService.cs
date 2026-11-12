using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class SchedulingSettingService : ISchedulingSettingService
    {
        private readonly ApplicationDbContext _dbContext;

        public SchedulingSettingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user?.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        public async Task<RecruitmentResult<SchedulingSettingResponse>> GetSettingAsync(
            Guid hospitalSiteId,
            CancellationToken cancellationToken = default)
        {
            if (hospitalSiteId == Guid.Empty)
            {
                var defaultSiteId = await _dbContext.MstHospitalSites
                    .Where(s => !s.IsDelete)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (defaultSiteId != Guid.Empty)
                {
                    hospitalSiteId = defaultSiteId;
                }
            }

            var setting = await _dbContext.HrdSchedulingSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSiteId && !s.IsDelete, cancellationToken);

            if (setting == null)
            {
                // Inisialisasi konfigurasi default bila belum pernah disimpan
                var defaultSetting = new HrdSchedulingSetting
                {
                    Id = Guid.NewGuid(),
                    HospitalSiteId = hospitalSiteId,
                    MinRestHoursBetweenShifts = 11,
                    MaxConsecutiveWorkDays = 6,
                    MaxConsecutiveNightShifts = 2,
                    RetroactiveCorrectionWindowDays = 3,
                    RestHoursEscalatedToBlock = false,
                    ConsecutiveDaysEscalatedToBlock = false,
                    NightShiftsEscalatedToBlock = false,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow
                };

                _dbContext.HrdSchedulingSettings.Add(defaultSetting);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return RecruitmentResult<SchedulingSettingResponse>.Ok(MapToResponse(defaultSetting), "Pengaturan penjadwalan berhasil diinisialisasi.");
            }

            return RecruitmentResult<SchedulingSettingResponse>.Ok(MapToResponse(setting), "Pengaturan penjadwalan berhasil diambil.");
        }

        public async Task<RecruitmentResult<SchedulingSettingResponse>> UpdateSettingAsync(
            Guid hospitalSiteId,
            UpdateSchedulingSettingRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actorId = GetUserId(user);

            if (hospitalSiteId == Guid.Empty && request.HospitalSiteId.HasValue && request.HospitalSiteId.Value != Guid.Empty)
            {
                hospitalSiteId = request.HospitalSiteId.Value;
            }

            if (hospitalSiteId == Guid.Empty)
            {
                var defaultSiteId = await _dbContext.MstHospitalSites
                    .Where(s => !s.IsDelete)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (defaultSiteId != Guid.Empty)
                {
                    hospitalSiteId = defaultSiteId;
                }
            }

            // Validasi rentang nilai aturan bisnis VR-SCH-80
            if (request.MinRestHoursBetweenShifts < 8 || request.MinRestHoursBetweenShifts > 24)
            {
                return RecruitmentResult<SchedulingSettingResponse>.BadRequest(
                    "VR-SCH-80: Jeda istirahat minimal antar shift harus berada dalam rentang 8 s.d. 24 jam.");
            }

            if (request.MaxConsecutiveWorkDays < 1 || request.MaxConsecutiveWorkDays > 14)
            {
                return RecruitmentResult<SchedulingSettingResponse>.BadRequest(
                    "VR-SCH-80: Batas maksimal hari kerja berturut-turut harus berada dalam rentang 1 s.d. 14 hari.");
            }

            if (request.MaxConsecutiveNightShifts < 1 || request.MaxConsecutiveNightShifts > 7)
            {
                return RecruitmentResult<SchedulingSettingResponse>.BadRequest(
                    "VR-SCH-80: Batas maksimal shift malam berurutan harus berada dalam rentang 1 s.d. 7 hari.");
            }

            if (request.RetroactiveCorrectionWindowDays < 0 || request.RetroactiveCorrectionWindowDays > 30)
            {
                return RecruitmentResult<SchedulingSettingResponse>.BadRequest(
                    "VR-SCH-80: Toleransi batas koreksi mundur mandiri harus berada dalam rentang 0 s.d. 30 hari.");
            }

            var setting = await _dbContext.HrdSchedulingSettings
                .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSiteId && !s.IsDelete, cancellationToken);

            if (setting == null)
            {
                setting = new HrdSchedulingSetting
                {
                    Id = Guid.NewGuid(),
                    HospitalSiteId = hospitalSiteId,
                    MinRestHoursBetweenShifts = request.MinRestHoursBetweenShifts,
                    MaxConsecutiveWorkDays = request.MaxConsecutiveWorkDays,
                    MaxConsecutiveNightShifts = request.MaxConsecutiveNightShifts,
                    RetroactiveCorrectionWindowDays = request.RetroactiveCorrectionWindowDays,
                    RestHoursEscalatedToBlock = request.RestHoursEscalatedToBlock,
                    ConsecutiveDaysEscalatedToBlock = request.ConsecutiveDaysEscalatedToBlock,
                    NightShiftsEscalatedToBlock = request.NightShiftsEscalatedToBlock,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorId
                };

                _dbContext.HrdSchedulingSettings.Add(setting);
            }
            else
            {
                // Pemeriksaan konkurensi optimistik
                if (setting.RowVersion != request.RowVersion)
                {
                    return RecruitmentResult<SchedulingSettingResponse>.Conflict(
                        "Pengaturan penjadwalan telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
                }

                setting.MinRestHoursBetweenShifts = request.MinRestHoursBetweenShifts;
                setting.MaxConsecutiveWorkDays = request.MaxConsecutiveWorkDays;
                setting.MaxConsecutiveNightShifts = request.MaxConsecutiveNightShifts;
                setting.RetroactiveCorrectionWindowDays = request.RetroactiveCorrectionWindowDays;
                setting.RestHoursEscalatedToBlock = request.RestHoursEscalatedToBlock;
                setting.ConsecutiveDaysEscalatedToBlock = request.ConsecutiveDaysEscalatedToBlock;
                setting.NightShiftsEscalatedToBlock = request.NightShiftsEscalatedToBlock;
                setting.RowVersion = Guid.NewGuid();
                setting.UpdateDateTime = DateTime.UtcNow;
                setting.UpdateBy = actorId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<SchedulingSettingResponse>.Ok(
                MapToResponse(setting),
                "Pengaturan kebijakan penjadwalan berhasil diperbarui.");
        }

        private static SchedulingSettingResponse MapToResponse(HrdSchedulingSetting s)
        {
            return new SchedulingSettingResponse
            {
                Id = s.Id,
                HospitalSiteId = s.HospitalSiteId,
                MinRestHoursBetweenShifts = s.MinRestHoursBetweenShifts,
                MaxConsecutiveWorkDays = s.MaxConsecutiveWorkDays,
                MaxConsecutiveNightShifts = s.MaxConsecutiveNightShifts,
                RetroactiveCorrectionWindowDays = s.RetroactiveCorrectionWindowDays,
                RestHoursEscalatedToBlock = s.RestHoursEscalatedToBlock,
                ConsecutiveDaysEscalatedToBlock = s.ConsecutiveDaysEscalatedToBlock,
                NightShiftsEscalatedToBlock = s.NightShiftsEscalatedToBlock,
                RowVersion = s.RowVersion,
                UpdatedAt = s.UpdateDateTime ?? s.CreateDateTime
            };
        }
    }
}
