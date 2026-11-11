using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class TalentPoolService : ITalentPoolService
    {
        private readonly ApplicationDbContext _dbContext;

        public TalentPoolService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        public async Task<RecruitmentResult<PagedResult<TalentPoolListItemResponse>>> GetPagedAsync(
            TalentPoolQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdTalentPools
                .Include(t => t.Candidate)
                .Include(t => t.OriginalVacancy)
                .Where(t => !t.IsDelete)
                .AsQueryable();

            if (query.VacancyId.HasValue)
            {
                dbQuery = dbQuery.Where(t => t.OriginalVacancyId == query.VacancyId.Value);
            }

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(t => t.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(t =>
                    t.QualificationSummary.ToLower().Contains(term) ||
                    (t.Candidate != null && (t.Candidate.FullName.ToLower().Contains(term) || t.Candidate.NationalIdNumber.Contains(term))) ||
                    (t.OriginalVacancy != null && t.OriginalVacancy.Title.ToLower().Contains(term)));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);
            var items = await dbQuery
                .OrderBy(t => t.RankingPosition)
                .ThenByDescending(t => t.RankingScore)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(t => new TalentPoolListItemResponse
                {
                    Id = t.Id,
                    CandidateId = t.CandidateId,
                    CandidateName = t.Candidate != null ? t.Candidate.FullName : "-",
                    CandidateNik = t.Candidate != null ? t.Candidate.NationalIdNumber : "-",
                    OriginalVacancyTitle = t.OriginalVacancy != null ? t.OriginalVacancy.Title : "-",
                    RankingScore = t.RankingScore,
                    RankingPosition = t.RankingPosition,
                    RetentionExpiryDate = t.RetentionExpiryDate,
                    Status = t.Status
                })
                .ToListAsync(cancellationToken);

            var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / query.PageSize) : 0;

            var result = new PagedResult<TalentPoolListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = totalPages,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return RecruitmentResult<PagedResult<TalentPoolListItemResponse>>.Ok(result, "Daftar kandidat cadangan talent pool berhasil diambil.");
        }

        public async Task<RecruitmentResult<TalentPoolResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var pool = await _dbContext.HrdTalentPools
                .Include(t => t.Candidate)
                .Include(t => t.OriginalVacancy)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDelete, cancellationToken);

            if (pool == null)
            {
                return RecruitmentResult<TalentPoolResponse>.NotFound("Data bank kandidat cadangan tidak ditemukan.");
            }

            return RecruitmentResult<TalentPoolResponse>.Ok(MapToResponse(pool), "Detail kandidat talent pool berhasil diambil.");
        }

        public async Task<RecruitmentResult<TalentPoolResponse>> EnrollAsync(
            EnrollTalentPoolRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var candidate = await _dbContext.HrdCandidates
                .FirstOrDefaultAsync(c => c.Id == request.CandidateId && !c.IsDelete, cancellationToken);

            if (candidate == null)
            {
                return RecruitmentResult<TalentPoolResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            var vacancy = await _dbContext.HrdJobVacancies
                .FirstOrDefaultAsync(v => v.Id == request.OriginalVacancyId && !v.IsDelete, cancellationToken);

            if (vacancy == null)
            {
                return RecruitmentResult<TalentPoolResponse>.NotFound("Lowongan asal tidak ditemukan.");
            }

            var existingActive = await _dbContext.HrdTalentPools
                .FirstOrDefaultAsync(t => t.CandidateId == request.CandidateId &&
                                          t.Status == TalentPoolStatus.Active &&
                                          !t.IsDelete, cancellationToken);

            if (existingActive != null)
            {
                return RecruitmentResult<TalentPoolResponse>.Conflict("Kandidat sudah aktif terdaftar di Talent Pool.");
            }

            var enrolledDate = DateTime.UtcNow;
            var retentionExpiry = enrolledDate.AddMonths(request.RetentionMonths > 0 ? request.RetentionMonths : 6);

            var talentPool = new HrdTalentPool
            {
                Id = Guid.NewGuid(),
                CandidateId = request.CandidateId,
                OriginalVacancyId = request.OriginalVacancyId,
                QualificationSummary = request.QualificationSummary.Trim(),
                RankingScore = request.RankingScore,
                RankingPosition = request.RankingPosition,
                EnrolledDate = enrolledDate,
                RetentionExpiryDate = retentionExpiry,
                Status = TalentPoolStatus.Active,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdTalentPools.AddAsync(talentPool, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(talentPool.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<TalentPoolResponse>> FastTrackPromoteAsync(
            Guid id,
            FastTrackPromoteRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var pool = await _dbContext.HrdTalentPools
                .Include(t => t.Candidate)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDelete, cancellationToken);

            if (pool == null)
            {
                return RecruitmentResult<TalentPoolResponse>.NotFound("Data bank kandidat cadangan tidak ditemukan.");
            }

            if (pool.Status != TalentPoolStatus.Active)
            {
                return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                    $"Kandidat tidak dapat dipromosikan karena status saat ini adalah {pool.Status}.");
            }

            // Aturan Bisnis VR-REC-18: Retensi talent pool maksimal 6 bulan
            if (DateTime.UtcNow > pool.RetentionExpiryDate)
            {
                pool.Status = TalentPoolStatus.Expired;
                pool.UpdateDateTime = DateTime.UtcNow;
                pool.UpdateBy = userId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                    "Masa retensi kandidat cadangan pada Talent Pool telah kedaluwarsa (> 6 bulan). Kandidat tidak dapat dipromosikan (VR-REC-18).");
            }

            var targetVacancy = await _dbContext.HrdJobVacancies
                .FirstOrDefaultAsync(v => v.Id == request.TargetVacancyId && !v.IsDelete, cancellationToken);

            if (targetVacancy == null)
            {
                return RecruitmentResult<TalentPoolResponse>.NotFound("Lowongan target tidak ditemukan.");
            }

            var salaryGrade = await _dbContext.MstSalaryGrades
                .FirstOrDefaultAsync(g => g.Id == request.SalaryGradeId && !g.IsDelete, cancellationToken);

            if (salaryGrade == null)
            {
                return RecruitmentResult<TalentPoolResponse>.NotFound("Master Skala Upah (Salary Grade) tidak ditemukan.");
            }

            // Evaluasi tier penawaran gaji (T13 standard vs T14 exceptional)
            var approvalTier = request.OfferedBasicSalary > salaryGrade.MaximumSalary
                ? JobOfferApprovalTier.Tier14Exceptional
                : JobOfferApprovalTier.Tier13Standard;

            // Aturan Bisnis VR-REC-19, AT-HRD-D2-10: Fast-Track Promotion
            // Buat berkas lamaran baru yang langsung melangkah ke tahap Offering tanpa tes ulang
            var newApp = new HrdCandidateApplication
            {
                Id = Guid.NewGuid(),
                CandidateId = pool.CandidateId,
                VacancyId = request.TargetVacancyId,
                ApplicationDate = DateTime.UtcNow,
                CurrentStage = ApplicationStage.Offering,
                StageStatus = "FastTrackPromotedFromTalentPool",
                Notes = $"Promosi cepat (Fast-Track) dari Talent Pool peringkat #{pool.RankingPosition} (Skor: {pool.RankingScore}).",
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };
            await _dbContext.HrdCandidateApplications.AddAsync(newApp, cancellationToken);

            // Auto-numbering offer OFR-yyyyMMdd-XXXX
            var today = DateTime.UtcNow;
            var todayStr = today.ToString("yyyyMMdd");
            var countToday = await _dbContext.HrdJobOffers
                .CountAsync(o => o.OfferNumber.StartsWith($"OFR-{todayStr}-"), cancellationToken);

            var offerNumber = $"OFR-{todayStr}-{(countToday + 1):D4}";

            var newOffer = new HrdJobOffer
            {
                Id = Guid.NewGuid(),
                ApplicationId = newApp.Id,
                OfferNumber = offerNumber,
                SalaryGradeId = request.SalaryGradeId,
                OfferedBasicSalary = request.OfferedBasicSalary,
                MaximumGradeSalary = salaryGrade.MaximumSalary,
                OfferedPositionTitle = targetVacancy.Title,
                ProposedJoinDate = DateOnly.FromDateTime(today.AddDays(14)),
                OfferDate = today,
                ExpiryDate = today.AddDays(7),
                ApprovalTier = approvalTier,
                Status = JobOfferStatus.Draft,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };
            await _dbContext.HrdJobOffers.AddAsync(newOffer, cancellationToken);

            // Perbarui status talent pool
            pool.Status = TalentPoolStatus.Promoted;
            pool.PromotedToOfferId = newOffer.Id;
            pool.RowVersion = Guid.NewGuid();
            pool.UpdateDateTime = DateTime.UtcNow;
            pool.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(pool.Id, cancellationToken);
        }

        private static TalentPoolResponse MapToResponse(HrdTalentPool model)
        {
            return new TalentPoolResponse
            {
                Id = model.Id,
                CandidateId = model.CandidateId,
                CandidateName = model.Candidate?.FullName ?? "-",
                CandidateNik = model.Candidate?.NationalIdNumber ?? "-",
                OriginalVacancyId = model.OriginalVacancyId,
                OriginalVacancyTitle = model.OriginalVacancy?.Title ?? "-",
                QualificationSummary = model.QualificationSummary,
                RankingScore = model.RankingScore,
                RankingPosition = model.RankingPosition,
                EnrolledDate = model.EnrolledDate,
                RetentionExpiryDate = model.RetentionExpiryDate,
                Status = model.Status,
                PromotedToOfferId = model.PromotedToOfferId,
                CreateDateTime = model.CreateDateTime
            };
        }
    }
}
