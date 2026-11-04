using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class JobRequisitionService : IJobRequisitionService
    {
        private readonly ApplicationDbContext _dbContext;

        public JobRequisitionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RecruitmentResult<PagedResult<JobRequisitionListItemResponse>>> GetPagedAsync(
            JobRequisitionQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdJobRequisitions
                .AsNoTracking()
                .Include(r => r.HeadcountRequest)
                .Include(r => r.HospitalSite)
                .Include(r => r.OrganizationUnit)
                .Include(r => r.Position)
                .Where(r => !r.IsDelete);

            if (query.HospitalSiteId.HasValue && query.HospitalSiteId.Value != Guid.Empty)
            {
                dbQuery = dbQuery.Where(r => r.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.OrganizationUnitId.HasValue && query.OrganizationUnitId.Value != Guid.Empty)
            {
                dbQuery = dbQuery.Where(r => r.OrganizationUnitId == query.OrganizationUnitId.Value);
            }

            if (query.PositionId.HasValue && query.PositionId.Value != Guid.Empty)
            {
                dbQuery = dbQuery.Where(r => r.PositionId == query.PositionId.Value);
            }

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(r => r.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(r => r.RequisitionNumber.ToLower().Contains(term) ||
                                            (r.Position != null && r.Position.PositionName.ToLower().Contains(term)));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);

            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(r => r.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new JobRequisitionListItemResponse
                {
                    Id = r.Id,
                    RequisitionNumber = r.RequisitionNumber,
                    HeadcountRequestId = r.HeadcountRequestId,
                    HeadcountRequestNumber = r.HeadcountRequest != null ? r.HeadcountRequest.RequestNumber : null,
                    HospitalSiteId = r.HospitalSiteId,
                    HospitalSiteName = r.HospitalSite != null ? r.HospitalSite.SiteName : null,
                    OrganizationUnitId = r.OrganizationUnitId,
                    OrganizationUnitName = r.OrganizationUnit != null ? r.OrganizationUnit.UnitName : null,
                    PositionId = r.PositionId,
                    PositionName = r.Position != null ? r.Position.PositionName : null,
                    RequestedSeats = r.RequestedSeats,
                    ApprovedSeats = r.ApprovedSeats,
                    FulfilledSeats = r.FulfilledSeats,
                    Status = r.Status,
                    CreateDateTime = r.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<JobRequisitionListItemResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<JobRequisitionListItemResponse>>.Ok(result, "Daftar job requisition berhasil diambil.");
        }

        public async Task<RecruitmentResult<JobRequisitionResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var r = await _dbContext.HrdJobRequisitions
                .AsNoTracking()
                .Include(x => x.HeadcountRequest)
                .Include(x => x.HospitalSite)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Position)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (r == null)
            {
                return RecruitmentResult<JobRequisitionResponse>.NotFound("Job requisition tidak ditemukan.");
            }

            return RecruitmentResult<JobRequisitionResponse>.Ok(MapToResponse(r), "Detail job requisition berhasil diambil.");
        }

        public async Task<RecruitmentResult<JobRequisitionResponse>> CreateAsync(
            CreateJobRequisitionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            // VR-REC-01: Validasi HeadcountRequestId wajib ada dan statusnya siap untuk rekrutmen (ReadyForRecruitment / PartiallyFulfilled)
            var headcount = await _dbContext.HrdHeadcountRequests
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(h => h.Id == request.HeadcountRequestId && !h.IsDelete, cancellationToken);

            if (headcount == null || (headcount.RequestStatus != HeadcountRequestStatus.ReadyForRecruitment && headcount.RequestStatus != HeadcountRequestStatus.PartiallyFulfilled))
            {
                return RecruitmentResult<JobRequisitionResponse>.Unprocessable(
                    "Headcount Request tidak valid atau belum berstatus siap untuk rekrutmen (ReadyForRecruitment). Pembuatan requisition ditolak (VR-REC-01).");
            }

            // VR-REC-02: Validasi kuota kursi eksternal yang tersedia
            var externalSeatsCount = headcount.Seats.Count(s => !s.IsDelete && s.FulfilmentSource == HeadcountFulfilmentSource.External);
            if (externalSeatsCount == 0)
            {
                return RecruitmentResult<JobRequisitionResponse>.Unprocessable(
                    "Headcount Request ini tidak memiliki alokasi kursi pemenuhan eksternal untuk rekrutmen.");
            }

            // Hitung kursi yang sudah dialokasikan ke requisition lain yang aktif
            var alreadyAllocatedSeats = await _dbContext.HrdJobRequisitions
                .Where(r => r.HeadcountRequestId == request.HeadcountRequestId && !r.IsDelete && r.Status != JobRequisitionStatus.Cancelled)
                .SumAsync(r => (int?)r.RequestedSeats, cancellationToken) ?? 0;

            var remainingExternalSeats = externalSeatsCount - alreadyAllocatedSeats;
            if (request.RequestedSeats > remainingExternalSeats)
            {
                return RecruitmentResult<JobRequisitionResponse>.Unprocessable(
                    $"Jumlah kursi yang diminta ({request.RequestedSeats}) melebihi sisa kursi eksternal yang tersedia ({remainingExternalSeats}) pada Headcount Request (VR-REC-02).");
            }

            var now = DateTime.UtcNow;
            var reqNumber = $"REQ-{now:yyyyMM}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            var entity = new HrdJobRequisition
            {
                Id = Guid.NewGuid(),
                RequisitionNumber = reqNumber,
                HeadcountRequestId = request.HeadcountRequestId,
                HospitalSiteId = request.HospitalSiteId,
                OrganizationUnitId = request.OrganizationUnitId,
                PositionId = request.PositionId,
                RequestedSeats = request.RequestedSeats,
                ApprovedSeats = 0,
                FulfilledSeats = 0,
                Status = JobRequisitionStatus.Draft,
                Notes = request.Notes,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now,
                CreateBy = userId
            };

            _dbContext.HrdJobRequisitions.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobRequisitionResponse>> UpdateAsync(
            Guid id,
            UpdateJobRequisitionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var entity = await _dbContext.HrdJobRequisitions
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RecruitmentResult<JobRequisitionResponse>.NotFound("Job requisition tidak ditemukan.");
            }

            if (entity.Status != JobRequisitionStatus.Draft)
            {
                return RecruitmentResult<JobRequisitionResponse>.Unprocessable("Hanya requisition berstatus Draft yang dapat diperbarui.");
            }

            if (entity.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<JobRequisitionResponse>.Conflict("Data requisition telah diubah oleh pengguna lain. Silakan muat ulang.");
            }

            // Validasi sisa kuota kursi pada headcount
            var headcount = await _dbContext.HrdHeadcountRequests
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(h => h.Id == entity.HeadcountRequestId && !h.IsDelete, cancellationToken);

            if (headcount != null)
            {
                var externalSeatsCount = headcount.Seats.Count(s => !s.IsDelete && s.FulfilmentSource == HeadcountFulfilmentSource.External);
                var otherAllocatedSeats = await _dbContext.HrdJobRequisitions
                    .Where(r => r.HeadcountRequestId == entity.HeadcountRequestId && r.Id != id && !r.IsDelete && r.Status != JobRequisitionStatus.Cancelled)
                    .SumAsync(r => (int?)r.RequestedSeats, cancellationToken) ?? 0;

                var remainingSeats = externalSeatsCount - otherAllocatedSeats;
                if (request.RequestedSeats > remainingSeats)
                {
                    return RecruitmentResult<JobRequisitionResponse>.Unprocessable(
                        $"Jumlah kursi yang diminta ({request.RequestedSeats}) melebihi sisa alokasi ({remainingSeats}) pada Headcount Request.");
                }
            }

            entity.RequestedSeats = request.RequestedSeats;
            entity.Notes = request.Notes;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobRequisitionResponse>> SubmitAsync(
            Guid id,
            SubmitJobRequisitionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var entity = await _dbContext.HrdJobRequisitions
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RecruitmentResult<JobRequisitionResponse>.NotFound("Job requisition tidak ditemukan.");
            }

            if (entity.Status != JobRequisitionStatus.Draft)
            {
                return RecruitmentResult<JobRequisitionResponse>.Unprocessable("Requisition sudah diajukan sebelumnya.");
            }

            if (entity.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<JobRequisitionResponse>.Conflict("Data requisition telah diubah oleh pihak lain.");
            }

            entity.Status = JobRequisitionStatus.Approved;
            entity.ApprovedSeats = entity.RequestedSeats;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobRequisitionResponse>> CancelAsync(
            Guid id,
            CancelJobRequisitionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var entity = await _dbContext.HrdJobRequisitions
                .Include(r => r.Vacancies)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RecruitmentResult<JobRequisitionResponse>.NotFound("Job requisition tidak ditemukan.");
            }

            // VR-REC-03: Pembatalan hanya diizinkan bila belum memiliki lowongan kerja aktif atau pelamar dalam seleksi
            var hasActiveVacancy = entity.Vacancies.Any(v => !v.IsDelete && v.Status == JobVacancyStatus.Published);
            if (hasActiveVacancy)
            {
                return RecruitmentResult<JobRequisitionResponse>.Unprocessable(
                    "Requisition tidak dapat dibatalkan karena masih memiliki lowongan kerja aktif yang tayang (VR-REC-03). Tutup lowongan terlebih dahulu.");
            }

            if (entity.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<JobRequisitionResponse>.Conflict("Data requisition telah diperbarui oleh pengguna lain.");
            }

            entity.Status = JobRequisitionStatus.Cancelled;
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes)
                ? $"Dibatalkan: {request.Reason}"
                : $"{entity.Notes} | Dibatalkan: {request.Reason}";

            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        private static JobRequisitionResponse MapToResponse(HrdJobRequisition r)
        {
            return new JobRequisitionResponse
            {
                Id = r.Id,
                RequisitionNumber = r.RequisitionNumber,
                HeadcountRequestId = r.HeadcountRequestId,
                HeadcountRequestNumber = r.HeadcountRequest?.RequestNumber,
                HospitalSiteId = r.HospitalSiteId,
                HospitalSiteName = r.HospitalSite?.SiteName,
                OrganizationUnitId = r.OrganizationUnitId,
                OrganizationUnitName = r.OrganizationUnit?.UnitName,
                PositionId = r.PositionId,
                PositionName = r.Position?.PositionName,
                RequestedSeats = r.RequestedSeats,
                ApprovedSeats = r.ApprovedSeats,
                FulfilledSeats = r.FulfilledSeats,
                Status = r.Status,
                Notes = r.Notes,
                RowVersion = r.RowVersion,
                CreateDateTime = r.CreateDateTime,
                UpdateDateTime = r.UpdateDateTime
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
