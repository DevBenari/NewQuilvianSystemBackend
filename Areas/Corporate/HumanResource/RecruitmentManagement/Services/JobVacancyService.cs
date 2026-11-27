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
    public class JobVacancyService : IJobVacancyService
    {
        private readonly ApplicationDbContext _dbContext;

        public JobVacancyService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RecruitmentResult<PagedResult<JobVacancyListItemResponse>>> GetPagedAsync(
            JobVacancyQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdJobVacancies
                .AsNoTracking()
                .Include(v => v.Requisition)
                    .ThenInclude(r => r!.Position)
                .Include(v => v.Requisition)
                    .ThenInclude(r => r!.OrganizationUnit)
                .Include(v => v.Requisition)
                    .ThenInclude(r => r!.HospitalSite)
                .Where(v => !v.IsDelete);

            if (query.RequisitionId.HasValue && query.RequisitionId.Value != Guid.Empty)
            {
                dbQuery = dbQuery.Where(v => v.RequisitionId == query.RequisitionId.Value);
            }

            if (query.IsClinical.HasValue)
            {
                dbQuery = dbQuery.Where(v => v.IsClinical == query.IsClinical.Value);
            }

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(v => v.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(v => v.VacancyNumber.ToLower().Contains(term) ||
                                            v.Title.ToLower().Contains(term));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);

            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(v => v.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new JobVacancyListItemResponse
                {
                    Id = v.Id,
                    VacancyNumber = v.VacancyNumber,
                    RequisitionId = v.RequisitionId,
                    RequisitionNumber = v.Requisition != null ? v.Requisition.RequisitionNumber : null,
                    PositionName = v.Requisition != null && v.Requisition.Position != null ? v.Requisition.Position.PositionName : null,
                    OrganizationUnitName = v.Requisition != null && v.Requisition.OrganizationUnit != null ? v.Requisition.OrganizationUnit.UnitName : null,
                    HospitalSiteName = v.Requisition != null && v.Requisition.HospitalSite != null ? v.Requisition.HospitalSite.SiteName : null,
                    Title = v.Title,
                    Description = v.Description,
                    QuotaSeats = v.QuotaSeats,
                    IsClinical = v.IsClinical,
                    PublishDate = v.PublishDate,
                    ClosingDate = v.ClosingDate,
                    Status = v.Status,
                    CreateDateTime = v.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            var vacancyIds = items.Select(i => i.Id).ToList();
            var occupiedMap = await RecruitmentJourneyPolicy.CountOccupiedSeatsByVacancyAsync(_dbContext, vacancyIds, cancellationToken);
            foreach (var item in items)
            {
                var occupied = occupiedMap.TryGetValue(item.Id, out var c) ? c : 0;
                item.OccupiedSeats = occupied;
                item.RemainingSeats = item.QuotaSeats > 0 ? Math.Max(0, item.QuotaSeats - occupied) : 0;
            }

            var result = new PagedResult<JobVacancyListItemResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<JobVacancyListItemResponse>>.Ok(result, "Daftar lowongan kerja berhasil diambil.");
        }

        public async Task<RecruitmentResult<JobVacancyResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var v = await _dbContext.HrdJobVacancies
                .AsNoTracking()
                .Include(x => x.Requisition)
                    .ThenInclude(r => r!.Position)
                .Include(x => x.Requisition)
                    .ThenInclude(r => r!.OrganizationUnit)
                .Include(x => x.Requisition)
                    .ThenInclude(r => r!.HospitalSite)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (v == null)
            {
                return RecruitmentResult<JobVacancyResponse>.NotFound("Lowongan kerja tidak ditemukan.");
            }

            var occupiedMap = await RecruitmentJourneyPolicy.CountOccupiedSeatsByVacancyAsync(_dbContext, new[] { v.Id }, cancellationToken);
            var occupied = occupiedMap.TryGetValue(v.Id, out var c) ? c : 0;
            return RecruitmentResult<JobVacancyResponse>.Ok(MapToResponse(v, occupied), "Detail lowongan kerja berhasil diambil.");
        }

        public async Task<RecruitmentResult<JobVacancyResponse>> CreateAsync(
            CreateJobVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            // VR-REC-04: Lowongan hanya dapat dibuat jika Requisition berstatus Approved
            var requisition = await _dbContext.HrdJobRequisitions
                .Include(r => r.Vacancies)
                .FirstOrDefaultAsync(r => r.Id == request.RequisitionId && !r.IsDelete, cancellationToken);

            if (requisition == null || requisition.Status != JobRequisitionStatus.Approved)
            {
                return RecruitmentResult<JobVacancyResponse>.Unprocessable(
                    "Job Requisition tidak ditemukan atau belum disetujui (Approved). Lowongan kerja tidak dapat dibuat (VR-REC-04).");
            }

            // Validasi kuota lowongan tidak boleh melampaui sisa kursi pada requisition
            var activeAllocatedSeats = requisition.Vacancies
                .Where(v => !v.IsDelete && v.Status != JobVacancyStatus.Cancelled && v.Status != JobVacancyStatus.Closed)
                .Sum(v => v.QuotaSeats);

            var availableSeats = requisition.ApprovedSeats - activeAllocatedSeats;
            if (request.QuotaSeats > availableSeats)
            {
                return RecruitmentResult<JobVacancyResponse>.Unprocessable(
                    $"Alokasi kuota lowongan ({request.QuotaSeats}) melebihi sisa kuota kursi yang disetujui pada requisition ({availableSeats}).");
            }

            var now = DateTime.UtcNow;
            var vacancyNumber = $"VAC-{now:yyyyMM}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            var entity = new HrdJobVacancy
            {
                Id = Guid.NewGuid(),
                VacancyNumber = vacancyNumber,
                RequisitionId = request.RequisitionId,
                Title = request.Title.Trim(),
                Description = request.Description,
                QuotaSeats = request.QuotaSeats,
                IsClinical = request.IsClinical,
                PublishDate = request.PublishDate,
                ClosingDate = request.ClosingDate,
                Status = JobVacancyStatus.Draft,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now,
                CreateBy = userId
            };

            _dbContext.HrdJobVacancies.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobVacancyResponse>> UpdateAsync(
            Guid id,
            UpdateJobVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var entity = await _dbContext.HrdJobVacancies
                .Include(v => v.Requisition)
                    .ThenInclude(r => r!.Vacancies)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RecruitmentResult<JobVacancyResponse>.NotFound("Lowongan kerja tidak ditemukan.");
            }

            if (entity.Status != JobVacancyStatus.Draft)
            {
                return RecruitmentResult<JobVacancyResponse>.Unprocessable("Hanya lowongan berstatus Draft yang dapat diperbarui informasinya.");
            }

            if (entity.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<JobVacancyResponse>.Conflict("Data lowongan kerja telah diubah oleh pihak lain.");
            }

            // Validasi sisa kuota kursi pada requisition induk
            var requisition = await _dbContext.HrdJobRequisitions
                .Include(r => r.Vacancies)
                .FirstOrDefaultAsync(r => r.Id == entity.RequisitionId && !r.IsDelete, cancellationToken);

            if (requisition != null)
            {
                var otherSeats = requisition.Vacancies
                    .Where(v => v.Id != id && !v.IsDelete && v.Status != JobVacancyStatus.Cancelled && v.Status != JobVacancyStatus.Closed)
                    .Sum(v => v.QuotaSeats);

                var availableSeats = requisition.ApprovedSeats - otherSeats;
                if (request.QuotaSeats > availableSeats)
                {
                    return RecruitmentResult<JobVacancyResponse>.Unprocessable(
                        $"Alokasi kuota lowongan ({request.QuotaSeats}) melebihi kuota kursi yang tersedia ({availableSeats}).");
                }
            }

            entity.Title = request.Title.Trim();
            entity.Description = request.Description;
            entity.QuotaSeats = request.QuotaSeats;
            entity.IsClinical = request.IsClinical;
            entity.ClosingDate = request.ClosingDate;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobVacancyResponse>> PublishAsync(
            Guid id,
            PublishJobVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var entity = await _dbContext.HrdJobVacancies
                .Include(v => v.Requisition)
                    .ThenInclude(r => r!.Vacancies)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RecruitmentResult<JobVacancyResponse>.NotFound("Lowongan kerja tidak ditemukan.");
            }

            if (entity.Requisition == null || entity.Requisition.IsDelete || entity.Requisition.Status != JobRequisitionStatus.Approved)
            {
                return RecruitmentResult<JobVacancyResponse>.Unprocessable(
                    "Lowongan kerja tidak dapat dipublikasikan karena Requisition induk belum disetujui (Approved).");
            }

            var otherSeats = entity.Requisition.Vacancies
                .Where(v => v.Id != entity.Id && !v.IsDelete && v.Status != JobVacancyStatus.Cancelled && v.Status != JobVacancyStatus.Closed)
                .Sum(v => v.QuotaSeats);
            var availableSeats = entity.Requisition.ApprovedSeats - otherSeats;
            if (entity.QuotaSeats > availableSeats)
            {
                return RecruitmentResult<JobVacancyResponse>.Unprocessable(
                    $"Lowongan tidak dapat dipublikasikan karena kuota ({entity.QuotaSeats}) melebihi sisa kursi Requisition ({availableSeats}).");
            }

            if (entity.Status != JobVacancyStatus.Draft)
            {
                return RecruitmentResult<JobVacancyResponse>.Unprocessable("Lowongan kerja sudah berstatus Published atau Closed.");
            }

            if (entity.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<JobVacancyResponse>.Conflict("Data lowongan kerja telah diperbarui oleh pengguna lain.");
            }

            var now = DateTime.UtcNow;
            entity.Status = JobVacancyStatus.Published;
            entity.PublishDate = now;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now;
            entity.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobVacancyResponse>> CloseAsync(
            Guid id,
            CloseJobVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var entity = await _dbContext.HrdJobVacancies
                .Include(v => v.Requisition)
                    .ThenInclude(r => r!.Vacancies)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RecruitmentResult<JobVacancyResponse>.NotFound("Lowongan kerja tidak ditemukan.");
            }

            if (entity.Status != JobVacancyStatus.Published)
            {
                return RecruitmentResult<JobVacancyResponse>.Unprocessable("Hanya lowongan kerja yang sedang Published yang dapat ditutup.");
            }

            if (entity.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<JobVacancyResponse>.Conflict("Data lowongan kerja telah diperbarui oleh pengguna lain.");
            }

            var now = DateTime.UtcNow;
            entity.Status = JobVacancyStatus.Closed;
            entity.ClosingDate = now;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now;
            entity.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        private static JobVacancyResponse MapToResponse(HrdJobVacancy v, int occupiedSeats = 0)
        {
            return new JobVacancyResponse
            {
                Id = v.Id,
                VacancyNumber = v.VacancyNumber,
                RequisitionId = v.RequisitionId,
                RequisitionNumber = v.Requisition?.RequisitionNumber,
                PositionName = v.Requisition?.Position?.PositionName,
                OrganizationUnitName = v.Requisition?.OrganizationUnit?.UnitName,
                HospitalSiteName = v.Requisition?.HospitalSite?.SiteName,
                Title = v.Title,
                Description = v.Description,
                QuotaSeats = v.QuotaSeats,
                OccupiedSeats = occupiedSeats,
                RemainingSeats = v.QuotaSeats > 0 ? Math.Max(0, v.QuotaSeats - occupiedSeats) : 0,
                IsClinical = v.IsClinical,
                PublishDate = v.PublishDate,
                ClosingDate = v.ClosingDate,
                Status = v.Status,
                RowVersion = v.RowVersion,
                CreateDateTime = v.CreateDateTime,
                UpdateDateTime = v.UpdateDateTime
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
