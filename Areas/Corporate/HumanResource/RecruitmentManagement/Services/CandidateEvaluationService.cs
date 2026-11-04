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
    public class CandidateEvaluationService : ICandidateEvaluationService
    {
        private readonly ApplicationDbContext _dbContext;

        public CandidateEvaluationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RecruitmentResult<PagedResult<CandidateEvaluationListItemResponse>>> GetPagedAsync(
            CandidateEvaluationQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdCandidateEvaluations
                .AsNoTracking()
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Candidate)
                .Where(e => !e.IsDelete);

            if (query.ApplicationId.HasValue && query.ApplicationId.Value != Guid.Empty)
            {
                dbQuery = dbQuery.Where(e => e.ApplicationId == query.ApplicationId.Value);
            }

            if (query.EvaluationType.HasValue)
            {
                dbQuery = dbQuery.Where(e => e.EvaluationType == query.EvaluationType.Value);
            }

            if (query.Recommendation.HasValue)
            {
                dbQuery = dbQuery.Where(e => e.Recommendation == query.Recommendation.Value);
            }

            if (query.IsClinicalVetoApplied.HasValue)
            {
                dbQuery = dbQuery.Where(e => e.IsClinicalVetoApplied == query.IsClinicalVetoApplied.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(e =>
                    (e.Application != null && e.Application.ApplicationNumber.ToLower().Contains(term)) ||
                    (e.Application != null && e.Application.Candidate != null && e.Application.Candidate.FullName.ToLower().Contains(term)) ||
                    e.EvaluatorName.ToLower().Contains(term));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(e => e.EvaluationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new CandidateEvaluationListItemResponse
                {
                    Id = e.Id,
                    ApplicationId = e.ApplicationId,
                    ApplicationNumber = e.Application != null ? e.Application.ApplicationNumber : string.Empty,
                    CandidateName = e.Application != null && e.Application.Candidate != null ? e.Application.Candidate.FullName : string.Empty,
                    EvaluationType = e.EvaluationType,
                    EvaluatorName = e.EvaluatorName,
                    EvaluationDate = e.EvaluationDate,
                    Score = e.Score,
                    Recommendation = e.Recommendation,
                    IsClinicalVetoApplied = e.IsClinicalVetoApplied
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<CandidateEvaluationListItemResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<CandidateEvaluationListItemResponse>>.Ok(result, "Daftar evaluasi kandidat berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateEvaluationResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var e = await _dbContext.HrdCandidateEvaluations
                .AsNoTracking()
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Candidate)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (e == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Data evaluasi tidak ditemukan.");
            }

            var response = MapToResponse(e);
            return RecruitmentResult<CandidateEvaluationResponse>.Ok(response, "Detail evaluasi kandidat berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateEvaluationResponse>> ScheduleAsync(
            ScheduleEvaluationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var application = await _dbContext.HrdCandidateApplications
                .Include(a => a.Vacancy)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && !a.IsDelete, cancellationToken);

            if (application == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Lamaran tidak ditemukan.");
            }

            // Validasi: tidak boleh menjadwalkan jika lamaran sudah gugur
            if (application.StageStatus == "ASSESSMENT_FAILED" || application.StageStatus == "REJECTED_ADMINISTRATION")
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable("Lamaran telah dinyatakan gugur dan tidak dapat dijadwalkan sesi evaluasi baru.");
            }

            var evaluation = new HrdCandidateEvaluation
            {
                Id = Guid.NewGuid(),
                ApplicationId = request.ApplicationId,
                EvaluationType = request.EvaluationType,
                EvaluatorUserId = request.EvaluatorUserId,
                EvaluatorName = request.EvaluatorName.Trim(),
                EvaluatorRole = request.EvaluatorRole.Trim(),
                EvaluationDate = request.EvaluationDate,
                Score = 0m,
                Recommendation = EvaluationRecommendation.RecommendedWithCondition,
                IsClinicalVetoApplied = false,
                Notes = request.Notes?.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdCandidateEvaluations.AddAsync(evaluation, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(evaluation.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateEvaluationResponse>> SubmitHrEvaluationAsync(
            Guid evaluationId,
            SubmitHrEvaluationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var evaluation = await _dbContext.HrdCandidateEvaluations
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Vacancy)
                .FirstOrDefaultAsync(e => e.Id == evaluationId && !e.IsDelete, cancellationToken);

            if (evaluation == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Data evaluasi tidak ditemukan.");
            }

            if (evaluation.EvaluationType != EvaluationType.HrInterview)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable("Sesi evaluasi ini bukan tipe Wawancara HR.");
            }

            var app = evaluation.Application;
            if (app == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Lamaran terkait tidak ditemukan.");
            }

            if (app.StageStatus == "ASSESSMENT_FAILED")
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Conflict("Lamaran ini telah dikunci karena gugur evaluasi sebelumnya.");
            }

            evaluation.Score = request.Score;
            evaluation.Recommendation = request.Recommendation;
            evaluation.EvaluationDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                evaluation.Notes = request.Notes.Trim();
            }
            evaluation.UpdateDateTime = DateTime.UtcNow;
            evaluation.UpdateBy = userId;

            // Evaluasi kesiapan melangkah ke offering
            await CheckAndAdvanceApplicationStageAsync(app, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(evaluation.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateEvaluationResponse>> SubmitClinicalEvaluationAsync(
            Guid evaluationId,
            SubmitClinicalEvaluationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var evaluation = await _dbContext.HrdCandidateEvaluations
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Vacancy)
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Candidate)
                .FirstOrDefaultAsync(e => e.Id == evaluationId && !e.IsDelete, cancellationToken);

            if (evaluation == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Data evaluasi tidak ditemukan.");
            }

            if (evaluation.EvaluationType != EvaluationType.ClinicalAssessment && evaluation.EvaluationType != EvaluationType.UserInterview)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable("Sesi evaluasi ini bukan tipe Uji Kompetensi Klinis atau Wawancara User.");
            }

            var app = evaluation.Application;
            if (app == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Lamaran terkait tidak ditemukan.");
            }

            evaluation.Score = request.Score;
            evaluation.Recommendation = request.Recommendation;
            evaluation.EvaluationDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                evaluation.Notes = request.Notes.Trim();
            }

            // Aturan Bisnis VR-REC-11, HRD-DEC-077: Hak Veto Klinis Mutlak jika NotRecommended
            if (request.Recommendation == EvaluationRecommendation.NotRecommended)
            {
                evaluation.IsClinicalVetoApplied = true;
                evaluation.VetoReason = string.IsNullOrWhiteSpace(request.VetoReason)
                    ? "VETO KLINIS MUTLAK: Kandidat tidak memenuhi standar kompetensi klinis rumah sakit (VR-REC-11)."
                    : request.VetoReason.Trim();

                // Kunci lamaran: status gagal permanen
                app.StageStatus = "ASSESSMENT_FAILED";
                app.Notes = $"Gugur evaluasi klinis. Alasan veto: {evaluation.VetoReason}";
                app.UpdateDateTime = DateTime.UtcNow;
                app.UpdateBy = userId;

                if (app.Candidate != null)
                {
                    app.Candidate.Status = CandidateStatus.Rejected;
                    app.Candidate.UpdateDateTime = DateTime.UtcNow;
                    app.Candidate.UpdateBy = userId;
                }
            }
            else
            {
                evaluation.IsClinicalVetoApplied = false;
                evaluation.VetoReason = null;

                // Evaluasi kesiapan melangkah ke offering jika kedua evaluasi selesai
                await CheckAndAdvanceApplicationStageAsync(app, cancellationToken);
            }

            evaluation.UpdateDateTime = DateTime.UtcNow;
            evaluation.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(evaluation.Id, cancellationToken);
        }

        private async Task CheckAndAdvanceApplicationStageAsync(HrdCandidateApplication app, CancellationToken cancellationToken)
        {
            var isClinical = app.Vacancy?.IsClinical ?? false;

            var allEvals = await _dbContext.HrdCandidateEvaluations
                .Where(e => e.ApplicationId == app.Id && !e.IsDelete)
                .ToListAsync(cancellationToken);

            var hrEval = allEvals.FirstOrDefault(e => e.EvaluationType == EvaluationType.HrInterview);
            var clinicalEval = allEvals.FirstOrDefault(e => e.EvaluationType == EvaluationType.ClinicalAssessment || e.EvaluationType == EvaluationType.UserInterview);

            if (isClinical)
            {
                // Aturan Bisnis VR-REC-12: Posisi klinis wajib menyelesaikan KEDUA evaluasi
                if (hrEval != null && hrEval.Recommendation != EvaluationRecommendation.NotRecommended &&
                    clinicalEval != null && clinicalEval.Recommendation != EvaluationRecommendation.NotRecommended &&
                    !clinicalEval.IsClinicalVetoApplied)
                {
                    app.CurrentStage = ApplicationStage.Offering;
                    app.StageStatus = "EvaluationPassedReadyForOffer";
                }
            }
            else
            {
                // Posisi non-klinis: cukup evaluasi HR (atau user) yang lulus
                if (hrEval != null && hrEval.Recommendation != EvaluationRecommendation.NotRecommended)
                {
                    app.CurrentStage = ApplicationStage.Offering;
                    app.StageStatus = "EvaluationPassedReadyForOffer";
                }
            }
        }

        private static CandidateEvaluationResponse MapToResponse(HrdCandidateEvaluation e)
        {
            return new CandidateEvaluationResponse
            {
                Id = e.Id,
                ApplicationId = e.ApplicationId,
                ApplicationNumber = e.Application?.ApplicationNumber ?? string.Empty,
                CandidateId = e.Application?.CandidateId ?? Guid.Empty,
                CandidateName = e.Application?.Candidate?.FullName ?? string.Empty,
                EvaluationType = e.EvaluationType,
                EvaluatorUserId = e.EvaluatorUserId,
                EvaluatorName = e.EvaluatorName,
                EvaluatorRole = e.EvaluatorRole,
                EvaluationDate = e.EvaluationDate,
                Score = e.Score,
                Recommendation = e.Recommendation,
                IsClinicalVetoApplied = e.IsClinicalVetoApplied,
                VetoReason = e.VetoReason,
                Notes = e.Notes,
                CreateDateTime = e.CreateDateTime
            };
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }
    }
}
