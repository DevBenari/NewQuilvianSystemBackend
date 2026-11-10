using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan antrean rekrutmen untuk Recruiter (BE-HRD-042).
    /// Menyajikan kursi rekrut luar yang siap diproses tanpa kolom biaya.
    /// </summary>
    public sealed class HrdRecruitmentQueueService : IHrdRecruitmentQueueService
    {
        private readonly ApplicationDbContext _dbContext;

        public HrdRecruitmentQueueService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<RecruitmentQueueItemResponse>> GetQueueAsync(
            RecruitmentQueuePagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var validRequestStatuses = new[]
            {
                HeadcountRequestStatus.ReadyForRecruitment,
                HeadcountRequestStatus.PartiallyFulfilled
            };

            var dbQuery = _dbContext.HrdHeadcountRequestSeats
                .AsNoTracking()
                .Include(s => s.HeadcountRequest)
                    .ThenInclude(r => r!.HospitalSite)
                .Include(s => s.HeadcountRequest)
                    .ThenInclude(r => r!.OrganizationUnit)
                .Include(s => s.HeadcountRequest)
                    .ThenInclude(r => r!.Position)
                .Where(s => !s.IsDelete &&
                            s.FulfilmentSource == HeadcountFulfilmentSource.External &&
                            s.SeatStatus == HeadcountSeatStatus.Open &&
                            s.HeadcountRequest != null &&
                            !s.HeadcountRequest.IsDelete &&
                            validRequestStatuses.Contains(s.HeadcountRequest.RequestStatus));

            if (query.HospitalSiteId.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.HeadcountRequest!.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.OrganizationUnitId.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.HeadcountRequest!.OrganizationUnitId == query.OrganizationUnitId.Value);
            }

            if (query.PositionId.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.HeadcountRequest!.PositionId == query.PositionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(s => s.HeadcountRequest!.RequestNumber.ToLower().Contains(term) ||
                                             (s.HeadcountRequest.Position != null && s.HeadcountRequest.Position.PositionName.ToLower().Contains(term)));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var items = await dbQuery
                .OrderBy(s => s.HeadcountRequest!.RequiredStartDate)
                .ThenBy(s => s.HeadcountRequest!.RequestNumber)
                .ThenBy(s => s.SeatNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new RecruitmentQueueItemResponse
                {
                    SeatId = s.Id,
                    HeadcountRequestId = s.HeadcountRequestId,
                    RequestNumber = s.HeadcountRequest!.RequestNumber,
                    SeatNumber = s.SeatNumber,
                    SeatFte = s.SeatFte,
                    HospitalSiteId = s.HeadcountRequest.HospitalSiteId,
                    HospitalSiteName = s.HeadcountRequest.HospitalSite != null ? s.HeadcountRequest.HospitalSite.SiteName : null,
                    OrganizationUnitId = s.HeadcountRequest.OrganizationUnitId,
                    OrganizationUnitName = s.HeadcountRequest.OrganizationUnit != null ? s.HeadcountRequest.OrganizationUnit.UnitName : null,
                    PositionId = s.HeadcountRequest.PositionId,
                    PositionName = s.HeadcountRequest.Position != null ? s.HeadcountRequest.Position.PositionName : null,
                    RequiredStartDate = s.HeadcountRequest.RequiredStartDate,
                    RequestType = s.HeadcountRequest.RequestType,
                    ReadyForRecruitmentAt = s.HeadcountRequest.ReadyForRecruitmentAt ?? s.HeadcountRequest.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RecruitmentQueueItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
