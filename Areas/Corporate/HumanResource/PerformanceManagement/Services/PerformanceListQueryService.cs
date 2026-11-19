using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Daftar sanggahan nilai kinerja dan pengajuan jenjang karier klinis seluruh pegawai
    /// (register masalah SDM MSL-14, DEC-FER-05 opsi A, 18 November 2026).
    /// </summary>
    public interface IPerformanceListQueryService
    {
        Task<PagedResult<AppraisalDisputeListItemDto>> GetDisputesAsync(PerformanceListQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<CareerLadderApplicationListItemDto>> GetCareerLadderApplicationsAsync(PerformanceListQuery query, CancellationToken cancellationToken = default);
    }

    public class PerformanceListQueryService : IPerformanceListQueryService
    {
        private readonly ApplicationDbContext _context;

        public PerformanceListQueryService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedResult<AppraisalDisputeListItemDto>> GetDisputesAsync(PerformanceListQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from d in _context.HrdPerformanceAppraisalDisputes.AsNoTracking()
                where !d.IsDelete
                join p in _context.MstWorkforceProfiles.AsNoTracking() on d.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new { d, Name = p != null ? p.DisplayName : null, Code = p != null ? p.ProfileCode : null };

            if (!string.IsNullOrWhiteSpace(q.Status)) query = query.Where(x => x.d.Status == q.Status);
            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.d.DisputeNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.d.DisputeDate)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new AppraisalDisputeListItemDto
                {
                    Id = x.d.Id,
                    DisputeNumber = x.d.DisputeNumber,
                    PerformanceAppraisalId = x.d.PerformanceAppraisalId,
                    WorkforceProfileId = x.d.WorkforceProfileId,
                    EmployeeName = x.Name ?? "Pegawai tidak ditemukan",
                    ProfileCode = x.Code,
                    DisputeDate = x.d.DisputeDate,
                    DisputedComponent = x.d.DisputedComponent,
                    DisputeReason = x.d.GrievanceReason,
                    Status = x.d.Status,
                    FinalVerdict = x.d.FinalVerdict,
                    AdjustedCompositeScore = x.d.AdjustedCompositeScore,
                    VerdictDeliveredAt = x.d.VerdictDeliveredAt,
                    FinalResolutionScore = x.d.AdjustedCompositeScore,
                    InvestigationNotes = x.d.MediationNotes,
                    AdjudicatedDateTime = x.d.VerdictDeliveredAt
                })
                .ToListAsync(cancellationToken);

            return HrCrossEmployeeLookup.ToPaged(rows, total, page, size);
        }

        public async Task<PagedResult<CareerLadderApplicationListItemDto>> GetCareerLadderApplicationsAsync(PerformanceListQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from r in _context.HrdClinicalCareerLadderReviews.AsNoTracking()
                where !r.IsDelete
                join p in _context.MstWorkforceProfiles.AsNoTracking() on r.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new { r, Name = p != null ? p.DisplayName : null, Code = p != null ? p.ProfileCode : null };

            if (!string.IsNullOrWhiteSpace(q.Status)) query = query.Where(x => x.r.FinalStatus == q.Status);
            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.r.ReviewNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.r.CreateDateTime)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new CareerLadderApplicationListItemDto
                {
                    Id = x.r.Id,
                    ReviewNumber = x.r.ReviewNumber,
                    WorkforceProfileId = x.r.WorkforceProfileId,
                    EmployeeName = x.Name ?? "Pegawai tidak ditemukan",
                    ProfileCode = x.Code,
                    CurrentClinicalLevel = x.r.CurrentClinicalLevel,
                    TargetClinicalLevel = x.r.TargetClinicalLevel,
                    ClinicalExperienceYears = x.r.ClinicalExperienceYears,
                    MeetsPerformanceThreshold = x.r.MeetsPerformanceThreshold,
                    MeetsClinicalCompetency = x.r.MeetsClinicalCompetency,
                    FreeFromSentinelIncident = x.r.FreeFromSentinelIncident,
                    CommitteeRecommendation = x.r.CommitteeRecommendation,
                    FinalStatus = x.r.FinalStatus,
                    ApprovedAt = x.r.ApprovedAt,
                    NewSkNumber = x.r.NewSkNumber,
                    SubmittedAt = x.r.CreateDateTime,
                    CurrentLevel = x.r.CurrentClinicalLevel,
                    RequestedLevel = x.r.TargetClinicalLevel,
                    SubmissionDate = x.r.CreateDateTime,
                    KomiteRecommendation = x.r.CommitteeRecommendation,
                    DecreeNumber = x.r.NewSkNumber,
                    Status = x.r.FinalStatus
                })
                .ToListAsync(cancellationToken);

            return HrCrossEmployeeLookup.ToPaged(rows, total, page, size);
        }
    }
}
