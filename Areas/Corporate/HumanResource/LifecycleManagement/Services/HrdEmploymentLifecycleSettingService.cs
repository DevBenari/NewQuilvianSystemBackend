using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdEmploymentLifecycleSettingService : IHrdEmploymentLifecycleSettingService
    {
        private readonly ApplicationDbContext _dbContext;

        public HrdEmploymentLifecycleSettingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RecruitmentResult<HrdEmploymentLifecycleSettingResponse>> GetByHospitalSiteIdAsync(
            Guid hospitalSiteId,
            CancellationToken cancellationToken = default)
        {
            var setting = await _dbContext.HrdEmploymentLifecycleSettings
                .Include(s => s.HospitalSite)
                .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSiteId && !s.IsDelete, cancellationToken);

            if (setting == null)
            {
                var site = await _dbContext.Set<MstHospitalSite>()
                    .FirstOrDefaultAsync(s => s.Id == hospitalSiteId && !s.IsDelete, cancellationToken);

                return RecruitmentResult<HrdEmploymentLifecycleSettingResponse>.Ok(new HrdEmploymentLifecycleSettingResponse
                {
                    Id = Guid.Empty,
                    HospitalSiteId = hospitalSiteId,
                    HospitalSiteName = site?.SiteName ?? string.Empty,
                    ContractReminderDays = 60,
                    ContractEscalationDays = 14,
                    ProbationReminderDays = 14,
                    LateStartFlagDays = 3,
                    RowVersion = Guid.Empty
                }, "Pengaturan default siklus kepegawaian berhasil dimuat.");
            }

            return RecruitmentResult<HrdEmploymentLifecycleSettingResponse>.Ok(new HrdEmploymentLifecycleSettingResponse
            {
                Id = setting.Id,
                HospitalSiteId = setting.HospitalSiteId,
                HospitalSiteName = setting.HospitalSite?.SiteName ?? string.Empty,
                ContractReminderDays = setting.ContractReminderDays,
                ContractEscalationDays = setting.ContractEscalationDays,
                ProbationReminderDays = setting.ProbationReminderDays,
                LateStartFlagDays = setting.LateStartFlagDays,
                RowVersion = setting.RowVersion
            }, "Pengaturan siklus kepegawaian berhasil diambil.");
        }

        public async Task<RecruitmentResult<HrdEmploymentLifecycleSettingResponse>> UpdateAsync(
            Guid hospitalSiteId,
            UpdateEmploymentLifecycleSettingRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            // Validasi VR-ONB-80
            if (request.ContractEscalationDays >= request.ContractReminderDays)
            {
                return RecruitmentResult<HrdEmploymentLifecycleSettingResponse>.Unprocessable(
                    "ContractEscalationDays harus lebih kecil daripada ContractReminderDays (VR-ONB-80).");
            }

            var setting = await _dbContext.HrdEmploymentLifecycleSettings
                .Include(s => s.HospitalSite)
                .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSiteId && !s.IsDelete, cancellationToken);

            if (setting == null)
            {
                setting = new HrdEmploymentLifecycleSetting
                {
                    Id = Guid.NewGuid(),
                    HospitalSiteId = hospitalSiteId,
                    ContractReminderDays = request.ContractReminderDays,
                    ContractEscalationDays = request.ContractEscalationDays,
                    ProbationReminderDays = request.ProbationReminderDays,
                    LateStartFlagDays = request.LateStartFlagDays,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.HrdEmploymentLifecycleSettings.AddAsync(setting, cancellationToken);
            }
            else
            {
                if (request.RowVersion.HasValue && request.RowVersion.Value != Guid.Empty && setting.RowVersion != request.RowVersion.Value)
                {
                    return RecruitmentResult<HrdEmploymentLifecycleSettingResponse>.Conflict("Pengaturan telah diubah oleh pengguna lain.");
                }

                setting.ContractReminderDays = request.ContractReminderDays;
                setting.ContractEscalationDays = request.ContractEscalationDays;
                setting.ProbationReminderDays = request.ProbationReminderDays;
                setting.LateStartFlagDays = request.LateStartFlagDays;
                setting.RowVersion = Guid.NewGuid();
                setting.UpdateDateTime = DateTime.UtcNow;
                setting.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByHospitalSiteIdAsync(hospitalSiteId, cancellationToken);
        }
    }
}
