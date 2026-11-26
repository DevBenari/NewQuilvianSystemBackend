using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class TalentPoolService : ITalentPoolService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator? _numberSeriesAllocator;

        public TalentPoolService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator? numberSeriesAllocator = null)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _numberSeriesAllocator = numberSeriesAllocator;
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

            foreach (var item in items)
            {
                item.CandidateNik = RecruitmentJourneyPolicy.MaskNationalIdNumber(item.CandidateNik);
            }

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

        public async Task<RecruitmentResult<List<EligibleTalentPoolApplicationResponse>>> GetEligibleApplicationsAsync(
            Guid? vacancyId,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdCandidateApplications
                .AsNoTracking()
                .Include(a => a.Candidate)
                .Include(a => a.Vacancy)
                .Include(a => a.Screening)
                .Include(a => a.Evaluations)
                .Where(a => !a.IsDelete
                            && a.Candidate != null && !a.Candidate.IsDelete
                            && a.Candidate.Status != CandidateStatus.Blacklisted
                            && a.Candidate.Status != CandidateStatus.Hired
                            && a.Vacancy != null && !a.Vacancy.IsDelete
                            && a.Screening != null && !a.Screening.IsDelete && a.Screening.Status == ScreeningStatus.Passed
                            && !_dbContext.HrdTalentPools.Any(t => t.CandidateId == a.CandidateId && t.Status == TalentPoolStatus.Active && !t.IsDelete));

            if (vacancyId.HasValue && vacancyId.Value != Guid.Empty)
            {
                dbQuery = dbQuery.Where(a => a.VacancyId == vacancyId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                dbQuery = dbQuery.Where(a =>
                    a.ApplicationNumber.ToLower().Contains(term) ||
                    (a.Candidate != null && (a.Candidate.FullName.ToLower().Contains(term) || a.Candidate.CandidateCode.ToLower().Contains(term))) ||
                    (a.Vacancy != null && a.Vacancy.Title.ToLower().Contains(term)));
            }

            var apps = await dbQuery.OrderByDescending(a => a.ApplicationDate).ToListAsync(cancellationToken);
            var vacancyIds = apps.Select(a => a.VacancyId).Distinct().ToList();
            var occupiedByVacancy = await CountOccupiedSeatsByVacancyAsync(vacancyIds, cancellationToken);

            var eligible = new List<EligibleTalentPoolApplicationResponse>();
            foreach (var app in apps)
            {
                var evals = app.Evaluations.Where(e => !e.IsDelete).ToList();
                var prereq = RecruitmentJourneyPolicy.EvaluateOfferPrerequisites(app, app.Vacancy, app.Screening, evals, app.Candidate);
                if (!prereq.CanCreateOffer || prereq.HasClinicalVeto)
                {
                    continue;
                }

                var quota = app.Vacancy?.QuotaSeats ?? 0;
                var occupied = occupiedByVacancy.TryGetValue(app.VacancyId, out var c) ? c : 0;
                if (quota > 0 && occupied < quota)
                {
                    continue;
                }

                var hrScore = evals
                    .Where(e => e.EvaluationType == EvaluationType.HrInterview && RecruitmentJourneyPolicy.IsEvaluationSubmitted(e))
                    .OrderByDescending(e => e.UpdateDateTime)
                    .Select(e => (decimal?)e.Score)
                    .FirstOrDefault();
                var clinScore = evals
                    .Where(e => (e.EvaluationType == EvaluationType.ClinicalAssessment || e.EvaluationType == EvaluationType.UserInterview)
                                && RecruitmentJourneyPolicy.IsEvaluationSubmitted(e))
                    .OrderByDescending(e => e.UpdateDateTime)
                    .Select(e => (decimal?)e.Score)
                    .FirstOrDefault();

                var scores = new[] { hrScore, clinScore }.Where(s => s.HasValue).Select(s => s!.Value).ToList();
                var suggestedScore = scores.Count > 0 ? Math.Round(scores.Average(), 2) : 0m;

                eligible.Add(new EligibleTalentPoolApplicationResponse
                {
                    ApplicationId = app.Id,
                    ApplicationNumber = app.ApplicationNumber,
                    CandidateId = app.CandidateId,
                    CandidateCode = app.Candidate?.CandidateCode ?? string.Empty,
                    CandidateName = app.Candidate?.FullName ?? string.Empty,
                    OriginalVacancyId = app.VacancyId,
                    OriginalVacancyTitle = app.Vacancy?.Title ?? string.Empty,
                    QuotaSeats = quota,
                    FilledOrOfferedCount = occupied,
                    HrScore = hrScore,
                    ClinicalScore = clinScore,
                    SuggestedRankingScore = suggestedScore
                });
            }

            return RecruitmentResult<List<EligibleTalentPoolApplicationResponse>>.Ok(eligible, "Daftar lamaran layak Talent Pool berhasil diambil.");
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
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<TalentPoolResponse>.Fail("Identitas user pada token tidak valid.", 401);
            }

            var candidate = await _dbContext.HrdCandidates
                .FirstOrDefaultAsync(c => c.Id == request.CandidateId && !c.IsDelete, cancellationToken);

            if (candidate == null)
            {
                return RecruitmentResult<TalentPoolResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            if (candidate.Status == CandidateStatus.Blacklisted || candidate.Status == CandidateStatus.Hired)
            {
                return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                    $"Kandidat berstatus '{candidate.Status}' tidak memenuhi syarat masuk Talent Pool.");
            }

            var vacancy = await _dbContext.HrdJobVacancies
                .FirstOrDefaultAsync(v => v.Id == request.OriginalVacancyId && !v.IsDelete, cancellationToken);

            if (vacancy == null)
            {
                return RecruitmentResult<TalentPoolResponse>.NotFound("Lowongan asal tidak ditemukan.");
            }

            // Wajib memiliki lamaran aktif pada lowongan asal dan lulus screening + evaluasi tanpa veto klinis (HRD-DEC-080)
            var originApplication = await _dbContext.HrdCandidateApplications
                .AsNoTracking()
                .Include(a => a.Screening)
                .Include(a => a.Evaluations)
                .FirstOrDefaultAsync(a => a.CandidateId == request.CandidateId
                                          && a.VacancyId == request.OriginalVacancyId
                                          && !a.IsDelete, cancellationToken);

            if (originApplication == null)
            {
                return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                    "Kandidat tidak memiliki lamaran yang sah pada lowongan asal untuk dimasukkan ke Talent Pool.");
            }

            var prereq = RecruitmentJourneyPolicy.EvaluateOfferPrerequisites(
                originApplication,
                vacancy,
                originApplication.Screening,
                originApplication.Evaluations,
                candidate);
            if (!prereq.CanCreateOffer || prereq.HasClinicalVeto)
            {
                return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                    prereq.BlockingMessage ?? "Kandidat belum memenuhi seluruh tahapan seleksi tanpa veto untuk masuk Talent Pool (HRD-DEC-080).");
            }

            if (vacancy.QuotaSeats > 0)
            {
                var occupiedByVacancy = await CountOccupiedSeatsByVacancyAsync(new[] { vacancy.Id }, cancellationToken);
                var occupied = occupiedByVacancy.TryGetValue(vacancy.Id, out var c) ? c : 0;
                if (occupied < vacancy.QuotaSeats)
                {
                    return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                        $"Kuota kursi lowongan '{vacancy.Title}' belum terisi penuh ({occupied}/{vacancy.QuotaSeats}); proses kandidat melalui Penawaran Kerja terlebih dahulu (HRD-DEC-080).");
                }
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
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<TalentPoolResponse>.Fail("Identitas user pada token tidak valid.", 401);
            }

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

            if (pool.Candidate == null || pool.Candidate.IsDelete
                || pool.Candidate.Status == CandidateStatus.Blacklisted
                || pool.Candidate.Status == CandidateStatus.Hired)
            {
                return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                    "Kandidat tidak valid atau berada dalam status terminal/daftar hitam/sudah direkrut.");
            }

            var hasClinicalVetoHistory = await _dbContext.HrdCandidateEvaluations
                .AsNoTracking()
                .AnyAsync(e => !e.IsDelete
                               && e.Application != null
                               && !e.Application.IsDelete
                               && e.Application.CandidateId == pool.CandidateId
                               && e.Application.VacancyId == pool.OriginalVacancyId
                               && e.IsClinicalVetoApplied, cancellationToken);
            if (hasClinicalVetoHistory)
            {
                return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                    "Kandidat memiliki riwayat veto klinis pada lamaran asal sehingga tidak dapat dipromosikan via Fast-Track.");
            }

            var targetVacancy = await _dbContext.HrdJobVacancies
                .FirstOrDefaultAsync(v => v.Id == request.TargetVacancyId && !v.IsDelete, cancellationToken);

            if (targetVacancy == null)
            {
                return RecruitmentResult<TalentPoolResponse>.NotFound("Lowongan target tidak ditemukan.");
            }

            if (targetVacancy.Status != JobVacancyStatus.Published)
            {
                return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                    "Promosi Fast-Track hanya dapat ditujukan ke lowongan berstatus Published (Aktif).");
            }

            if (targetVacancy.ClosingDate.HasValue && targetVacancy.ClosingDate.Value < DateTime.UtcNow)
            {
                return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                    "Lowongan target sudah melewati batas waktu penutupan.");
            }

            var duplicateTargetApp = await _dbContext.HrdCandidateApplications
                .AnyAsync(a => a.CandidateId == pool.CandidateId && a.VacancyId == request.TargetVacancyId && !a.IsDelete, cancellationToken);
            if (duplicateTargetApp)
            {
                return RecruitmentResult<TalentPoolResponse>.Conflict(
                    "Kandidat sudah memiliki lamaran aktif pada lowongan target.");
            }

            if (targetVacancy.QuotaSeats > 0)
            {
                var occupiedMap = await CountOccupiedSeatsByVacancyAsync(new[] { targetVacancy.Id }, cancellationToken);
                var occupied = occupiedMap.TryGetValue(targetVacancy.Id, out var c) ? c : 0;
                if (occupied >= targetVacancy.QuotaSeats)
                {
                    return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                        $"Kuota kursi lowongan target '{targetVacancy.Title}' sudah penuh ({occupied}/{targetVacancy.QuotaSeats}).");
                }
            }

            if (targetVacancy.IsClinical)
            {
                var candidateDocs = await _dbContext.HrdCandidateDocuments
                    .AsNoTracking()
                    .Where(d => d.CandidateId == pool.CandidateId && !d.IsDelete)
                    .ToListAsync(cancellationToken);
                var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
                var (hasStr, isExpiredStr, _) = RecruitmentJourneyPolicy.EvaluateClinicalStrDocument(candidateDocs, Guid.Empty, todayDate);
                if (!hasStr || isExpiredStr)
                {
                    return RecruitmentResult<TalentPoolResponse>.Unprocessable(
                        !hasStr
                            ? "Promosi Fast-Track ke lowongan klinis ditolak: kandidat belum memiliki dokumen STR aktif (HRD-DEC-076)."
                            : "Promosi Fast-Track ke lowongan klinis ditolak: STR kandidat telah kedaluwarsa (HRD-DEC-076).");
                }
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

            // Alokasi ApplicationNumber & OfferNumber sebelum membuka transaksi
            var dateCode = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, RecruitmentJourneyPolicy.JakartaTimeZone)
                .ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var appPrefix = $"APP-{dateCode}-";
            var existingAppNumbers = await _dbContext.HrdCandidateApplications
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(a => a.ApplicationNumber.StartsWith(appPrefix))
                .Select(a => a.ApplicationNumber)
                .ToListAsync(cancellationToken);

            var applicationNumber = await RecruitmentJourneyPolicy.AllocateSequenceAsync(
                _numberSeriesAllocator,
                existingAppNumbers,
                "HRD_CANDIDATE_APPLICATION",
                "APP",
                userId,
                cancellationToken);

            var offerPrefix = $"OFR-{dateCode}-";
            var existingOfferNumbers = await _dbContext.HrdJobOffers
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(o => o.OfferNumber.StartsWith(offerPrefix))
                .Select(o => o.OfferNumber)
                .ToListAsync(cancellationToken);

            var offerNumber = await RecruitmentJourneyPolicy.AllocateSequenceAsync(
                _numberSeriesAllocator,
                existingOfferNumbers,
                "HRD_JOB_OFFER",
                "OFR",
                userId,
                cancellationToken);

            await using var transaction = _dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction == null
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            // Aturan Bisnis VR-REC-19, AT-HRD-D2-10: Fast-Track Promotion
            var newApp = new HrdCandidateApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = applicationNumber,
                CandidateId = pool.CandidateId,
                VacancyId = request.TargetVacancyId,
                ApplicationDate = DateTime.UtcNow,
                CurrentStage = ApplicationStage.Offering,
                StageStatus = "FastTrackPromotedFromTalentPool",
                Notes = $"Promosi cepat (Fast-Track) dari Talent Pool peringkat #{pool.RankingPosition} (Skor: {pool.RankingScore}).",
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };
            await _dbContext.HrdCandidateApplications.AddAsync(newApp, cancellationToken);

            var today = DateTime.UtcNow;
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

            pool.Status = TalentPoolStatus.Promoted;
            pool.PromotedToOfferId = newOffer.Id;
            pool.RowVersion = Guid.NewGuid();
            pool.UpdateDateTime = DateTime.UtcNow;
            pool.UpdateBy = userId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
            }
            catch (DbUpdateException)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                return RecruitmentResult<TalentPoolResponse>.Conflict("Konflik data saat memproses Fast-Track Talent Pool.");
            }

            return await GetByIdAsync(pool.Id, cancellationToken);
        }

        private async Task<Dictionary<Guid, int>> CountOccupiedSeatsByVacancyAsync(
            IEnumerable<Guid> vacancyIds,
            CancellationToken cancellationToken)
        {
            var idList = vacancyIds.Where(v => v != Guid.Empty).Distinct().ToList();
            if (idList.Count == 0)
            {
                return new Dictionary<Guid, int>();
            }

            var activeOfferAppIds = await _dbContext.HrdJobOffers
                .AsNoTracking()
                .Include(o => o.Application)
                .Where(o => !o.IsDelete
                            && o.Application != null
                            && !o.Application.IsDelete
                            && idList.Contains(o.Application.VacancyId)
                            && o.Status != JobOfferStatus.RejectedByApprover
                            && o.Status != JobOfferStatus.ConditionalRejected
                            && o.Status != JobOfferStatus.FinalRejected
                            && o.Status != JobOfferStatus.Revoked
                            && o.Status != JobOfferStatus.Expired)
                .Select(o => new { o.Application!.VacancyId, o.ApplicationId })
                .ToListAsync(cancellationToken);

            var handoffAppIds = await _dbContext.HrdCandidateHiringHandoffs
                .AsNoTracking()
                .Include(h => h.Offer)
                    .ThenInclude(o => o!.Application)
                .Where(h => !h.IsDelete
                            && h.Status != HandoffStatus.Cancelled
                            && h.Offer != null
                            && h.Offer.Application != null
                            && idList.Contains(h.Offer.Application.VacancyId))
                .Select(h => new { h.Offer!.Application!.VacancyId, h.Offer.ApplicationId })
                .ToListAsync(cancellationToken);

            return activeOfferAppIds
                .Concat(handoffAppIds)
                .GroupBy(x => x.VacancyId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ApplicationId).Distinct().Count());
        }

        private static TalentPoolResponse MapToResponse(HrdTalentPool model)
        {
            return new TalentPoolResponse
            {
                Id = model.Id,
                CandidateId = model.CandidateId,
                CandidateName = model.Candidate?.FullName ?? "-",
                CandidateNik = RecruitmentJourneyPolicy.MaskNationalIdNumber(model.Candidate?.NationalIdNumber),
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
