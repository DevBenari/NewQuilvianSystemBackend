using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdEmploymentReadinessReadService : IHrdEmploymentReadinessReadService
    {
        private readonly ApplicationDbContext _dbContext;

        public HrdEmploymentReadinessReadService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<RecruitmentResult<SchedulingEligibilityResponse>> GetSchedulingEligibilityAsync(
            Guid workforceProfileId,
            DateOnly targetDate,
            CancellationToken cancellationToken = default)
        {
            var profile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == workforceProfileId && !p.IsDelete, cancellationToken);

            if (profile == null)
            {
                return RecruitmentResult<SchedulingEligibilityResponse>.NotFound("Profil pegawai tidak ditemukan.");
            }

            var targetDateTime = targetDate.ToDateTime(TimeOnly.MinValue);

            // 1. Cek kasus onboarding yang aktif
            var onboardingCase = await _dbContext.HrdOnboardingCases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.WorkforceProfileId == workforceProfileId && !c.IsDelete, cancellationToken);

            if (onboardingCase != null && onboardingCase.CaseStatus != OnboardingCaseStatus.Completed)
            {
                return RecruitmentResult<SchedulingEligibilityResponse>.Ok(new SchedulingEligibilityResponse
                {
                    WorkforceProfileId = profile.Id,
                    WorkforceCode = profile.ProfileCode,
                    FullName = profile.DisplayName,
                    TargetDate = targetDate,
                    EligibilityStatus = "NotReady",
                    Reason = "Pegawai baru belum menyelesaikan orientasi atau belum dinyatakan Siap Kerja (INT-HR-31)."
                });
            }

            // 2. Cek kontrak kerja aktif
            var currentContract = await _dbContext.WfpContractHistories
                .Include(c => c.ContractType)
                .AsNoTracking()
                .Where(c => c.WorkforceProfileId == workforceProfileId && !c.IsDelete)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentContract != null && currentContract.EndDate.HasValue)
            {
                if (targetDateTime > currentContract.EndDate.Value)
                {
                    // Cek apakah ada kontrak lanjutan yang sudah Signed
                    var hasSignedRenewal = await _dbContext.WfpContractHistories
                        .AnyAsync(r => r.PreviousContractHistoryId == currentContract.Id &&
                                       r.ContractStatus == "Signed" &&
                                       r.StartDate <= targetDateTime &&
                                       (!r.EndDate.HasValue || r.EndDate.Value >= targetDateTime) &&
                                       !r.IsDelete, cancellationToken);

                    if (!hasSignedRenewal)
                    {
                        return RecruitmentResult<SchedulingEligibilityResponse>.Ok(new SchedulingEligibilityResponse
                        {
                            WorkforceProfileId = profile.Id,
                            WorkforceCode = profile.ProfileCode,
                            FullName = profile.DisplayName,
                            TargetDate = targetDate,
                            EligibilityStatus = "ContractHold",
                            Reason = $"Tanggal penjadwalan {targetDate} melewati batas akhir masa kontrak kerja aktif ({DateOnly.FromDateTime(currentContract.EndDate.Value)})."
                        });
                    }
                }
            }

            // 3. Cek pendampingan klinis (IsAccompanied)
            var preceptorship = await _dbContext.HrdClinicalPreceptorships
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.WorkforceProfileId == workforceProfileId &&
                                          p.PreceptorshipStatus == PreceptorshipStatus.Accompanied &&
                                          !p.IsDelete, cancellationToken);

            bool isAccompanied = preceptorship != null;
            string? accompanyingNote = isAccompanied
                ? "Pegawai berada dalam masa pendampingan klinis (Accompanied) dan wajib didampingi preceptor saat berdinas."
                : null;

            return RecruitmentResult<SchedulingEligibilityResponse>.Ok(new SchedulingEligibilityResponse
            {
                WorkforceProfileId = profile.Id,
                WorkforceCode = profile.ProfileCode,
                FullName = profile.DisplayName,
                TargetDate = targetDate,
                EligibilityStatus = "Eligible",
                IsAccompanied = isAccompanied,
                AccompanyingNote = accompanyingNote,
                Reason = currentContract == null ? "Memenuhi syarat (kontrak belum tercatat dalam sistem)." : "Memenuhi seluruh kriteria penjadwalan dinas."
            });
        }
    }
}
