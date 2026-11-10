using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan bisnis pencatatan dan pengelolaan angka beban kerja manual (BE-HRD-034).
    /// Memenuhi HRD-DEC-060 (MANUAL_WORKLOAD_INPUT), FR-HRD-096, VR-WFP-08, VR-WFP-09, dan VR-WFP-38.
    /// </summary>
    public sealed class HrdStaffingWorkloadService : IHrdStaffingWorkloadService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HrdStaffingWorkloadService> _logger;

        public HrdStaffingWorkloadService(
            ApplicationDbContext dbContext,
            ILogger<HrdStaffingWorkloadService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<WorkforcePlanningResult<PagedResult<StaffingWorkloadResponse>>> GetPagedListAsync(
            StaffingWorkloadPagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, 100);

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null)
            {
                if (query.OrganizationUnitId.HasValue && !scopedUnitIds.Contains(query.OrganizationUnitId.Value))
                {
                    return WorkforcePlanningResult<PagedResult<StaffingWorkloadResponse>>.Ok(new PagedResult<StaffingWorkloadResponse>
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalData = 0,
                        TotalPage = 0,
                        Items = new List<StaffingWorkloadResponse>()
                    }, "Daftar angka beban kerja berhasil diambil.");
                }
            }

            var baseQuery = _dbContext.HrdStaffingWorkloadInputs.AsNoTracking()
                .Include(w => w.OrganizationUnit)
                .Include(w => w.Position)
                .Where(w => !w.IsDelete);

            if (scopedUnitIds != null)
            {
                baseQuery = baseQuery.Where(w => scopedUnitIds.Contains(w.OrganizationUnitId));
            }

            if (query.OrganizationUnitId.HasValue && query.OrganizationUnitId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(w => w.OrganizationUnitId == query.OrganizationUnitId.Value);
            }

            if (query.PositionId.HasValue && query.PositionId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(w => w.PositionId == query.PositionId.Value);
            }

            if (query.PeriodDate.HasValue)
            {
                baseQuery = baseQuery.Where(w =>
                    w.PeriodStartDate <= query.PeriodDate.Value &&
                    w.PeriodEndDate >= query.PeriodDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                baseQuery = baseQuery.Where(w =>
                    (w.OrganizationUnit != null && w.OrganizationUnit.UnitName.ToLower().Contains(search)) ||
                    (w.Position != null && w.Position.PositionName.ToLower().Contains(search)) ||
                    w.SourceNote.ToLower().Contains(search));
            }

            var totalData = await baseQuery.CountAsync(cancellationToken);

            var items = await baseQuery
                .OrderByDescending(w => w.PeriodStartDate)
                .ThenByDescending(w => w.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new StaffingWorkloadResponse
                {
                    Id = w.Id,
                    OrganizationUnitId = w.OrganizationUnitId,
                    OrganizationUnitCode = w.OrganizationUnit != null ? w.OrganizationUnit.UnitCode : null,
                    OrganizationUnitName = w.OrganizationUnit != null ? w.OrganizationUnit.UnitName : null,
                    PositionId = w.PositionId,
                    PositionCode = w.Position != null ? w.Position.PositionCode : null,
                    PositionName = w.Position != null ? w.Position.PositionName : null,
                    PeriodStartDate = w.PeriodStartDate,
                    PeriodEndDate = w.PeriodEndDate,
                    WorkloadValue = w.WorkloadValue,
                    SourceNote = w.SourceNote,
                    CreatedAt = w.CreateDateTime,
                    UpdatedAt = w.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<StaffingWorkloadResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return WorkforcePlanningResult<PagedResult<StaffingWorkloadResponse>>.Ok(result, "Daftar angka beban kerja berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<StaffingWorkloadResponse>> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var workload = await _dbContext.HrdStaffingWorkloadInputs.AsNoTracking()
                .Include(w => w.OrganizationUnit)
                .Include(w => w.Position)
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDelete, cancellationToken);

            if (workload == null)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(workload.OrganizationUnitId))
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            var response = MapToResponse(workload);
            return WorkforcePlanningResult<StaffingWorkloadResponse>.Ok(response, "Detail angka beban kerja berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<StaffingWorkloadResponse>> CreateAsync(
            UpsertStaffingWorkloadRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan tidak boleh kosong.");
            }

            // Kriteria 2: Sumber kosong -> 400 VR-WFP-08
            if (string.IsNullOrWhiteSpace(request.SourceNote))
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tuliskan sumber angka beban, misalnya rekap sensus harian.");
            }

            if (request.PeriodEndDate < request.PeriodStartDate)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal akhir periode tidak boleh lebih awal dari tanggal mulai periode.");
            }

            if (request.WorkloadValue < 0)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Angka beban tidak boleh bernilai negatif.");
            }

            // Kriteria 4: Kepala Unit di luar cakupan unitnya: POST/PUT -> 404 VR-WFP-38
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(request.OrganizationUnitId))
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            var unit = await _dbContext.MstOrganizationUnits.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.OrganizationUnitId && !u.IsDelete, cancellationToken);

            if (unit == null)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Unit organisasi tidak ditemukan atau tidak aktif.");
            }

            var position = await _dbContext.MstPositions.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PositionId && !p.IsDelete, cancellationToken);

            if (position == null)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Jabatan tidak ditemukan atau tidak aktif.");
            }

            // Kriteria 3: Isian kedua untuk unit, jabatan, dan tanggal mulai periode yang sama -> 409 VR-WFP-09
            var existsDuplicate = await _dbContext.HrdStaffingWorkloadInputs.AnyAsync(w =>
                !w.IsDelete &&
                w.OrganizationUnitId == request.OrganizationUnitId &&
                w.PositionId == request.PositionId &&
                w.PeriodStartDate == request.PeriodStartDate,
                cancellationToken);

            if (existsDuplicate)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Angka beban periode ini sudah diisi. Ubah isian yang ada.");
            }

            // Kriteria 5: Isian tidak melewati persetujuan dan tidak menunjuk baris standar
            var workload = new HrdStaffingWorkloadInput
            {
                Id = Guid.NewGuid(),
                OrganizationUnitId = request.OrganizationUnitId,
                PositionId = request.PositionId,
                PeriodStartDate = request.PeriodStartDate,
                PeriodEndDate = request.PeriodEndDate,
                WorkloadValue = request.WorkloadValue,
                SourceNote = request.SourceNote.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = currentUserId,
                IsDelete = false
            };

            _dbContext.HrdStaffingWorkloadInputs.Add(workload);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Angka beban kerja berhasil dicatat. EntityId: {EntityId}, UnitId: {UnitId}, PositionId: {PositionId}, Value: {Value}, Start: {Start}, End: {End}",
                workload.Id,
                workload.OrganizationUnitId,
                workload.PositionId,
                workload.WorkloadValue,
                workload.PeriodStartDate,
                workload.PeriodEndDate);

            var createdResult = await GetByIdAsync(workload.Id, currentUserId, cancellationToken);
            return WorkforcePlanningResult<StaffingWorkloadResponse>.Ok(
                createdResult.Data!,
                "Angka beban kerja berhasil dicatat.",
                StatusCodes.Status201Created);
        }

        public async Task<WorkforcePlanningResult<StaffingWorkloadResponse>> UpdateAsync(
            Guid id,
            UpsertStaffingWorkloadRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan tidak boleh kosong.");
            }

            var workload = await _dbContext.HrdStaffingWorkloadInputs
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDelete, cancellationToken);

            if (workload == null)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Kriteria 4: Kepala Unit di luar cakupan unitnya: POST/PUT -> 404 VR-WFP-38
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null &&
                (!scopedUnitIds.Contains(workload.OrganizationUnitId) || !scopedUnitIds.Contains(request.OrganizationUnitId)))
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Kriteria 2: Sumber kosong -> 400 VR-WFP-08
            if (string.IsNullOrWhiteSpace(request.SourceNote))
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tuliskan sumber angka beban, misalnya rekap sensus harian.");
            }

            if (request.PeriodEndDate < request.PeriodStartDate)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal akhir periode tidak boleh lebih awal dari tanggal mulai periode.");
            }

            if (request.WorkloadValue < 0)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Angka beban tidak boleh bernilai negatif.");
            }

            // Kriteria 3: Isian kedua untuk unit, jabatan, dan tanggal mulai periode yang sama -> 409 VR-WFP-09
            var existsDuplicate = await _dbContext.HrdStaffingWorkloadInputs.AnyAsync(w =>
                !w.IsDelete &&
                w.Id != id &&
                w.OrganizationUnitId == request.OrganizationUnitId &&
                w.PositionId == request.PositionId &&
                w.PeriodStartDate == request.PeriodStartDate,
                cancellationToken);

            if (existsDuplicate)
            {
                return WorkforcePlanningResult<StaffingWorkloadResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Angka beban periode ini sudah diisi. Ubah isian yang ada.");
            }

            workload.OrganizationUnitId = request.OrganizationUnitId;
            workload.PositionId = request.PositionId;
            workload.PeriodStartDate = request.PeriodStartDate;
            workload.PeriodEndDate = request.PeriodEndDate;
            workload.WorkloadValue = request.WorkloadValue;
            workload.SourceNote = request.SourceNote.Trim();
            workload.UpdateDateTime = DateTime.UtcNow;
            workload.UpdateBy = currentUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Angka beban kerja berhasil diperbarui. EntityId: {EntityId}, UnitId: {UnitId}, PositionId: {PositionId}, Value: {Value}, Start: {Start}, End: {End}",
                workload.Id,
                workload.OrganizationUnitId,
                workload.PositionId,
                workload.WorkloadValue,
                workload.PeriodStartDate,
                workload.PeriodEndDate);

            var updatedResult = await GetByIdAsync(workload.Id, currentUserId, cancellationToken);
            return WorkforcePlanningResult<StaffingWorkloadResponse>.Ok(
                updatedResult.Data!,
                "Angka beban kerja berhasil diperbarui.");
        }

        private static StaffingWorkloadResponse MapToResponse(HrdStaffingWorkloadInput workload)
        {
            return new StaffingWorkloadResponse
            {
                Id = workload.Id,
                OrganizationUnitId = workload.OrganizationUnitId,
                OrganizationUnitCode = workload.OrganizationUnit?.UnitCode,
                OrganizationUnitName = workload.OrganizationUnit?.UnitName,
                PositionId = workload.PositionId,
                PositionCode = workload.Position?.PositionCode,
                PositionName = workload.Position?.PositionName,
                PeriodStartDate = workload.PeriodStartDate,
                PeriodEndDate = workload.PeriodEndDate,
                WorkloadValue = workload.WorkloadValue,
                SourceNote = workload.SourceNote,
                CreatedAt = workload.CreateDateTime,
                UpdatedAt = workload.UpdateDateTime
            };
        }

        private async Task<HashSet<Guid>?> GetScopedOrganizationUnitIdsAsync(Guid userId, CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
                return null;

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null || !user.WorkforceProfileId.HasValue)
                return null;

            var now = DateTime.UtcNow;

            var managerialAssignments = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(a => a.WorkforceProfileId == user.WorkforceProfileId.Value &&
                            a.IsActive && !a.IsDelete && !a.IsCancel &&
                            a.IsManagerialAssignment &&
                            a.EffectiveStartDate <= now &&
                            (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= now))
                .ToListAsync(cancellationToken);

            if (managerialAssignments.Count == 0)
            {
                return null;
            }

            var baseUnitIds = new HashSet<Guid>();
            foreach (var a in managerialAssignments)
            {
                if (a.OrganizationUnitId.HasValue && a.OrganizationUnitId.Value != Guid.Empty)
                {
                    baseUnitIds.Add(a.OrganizationUnitId.Value);
                }
                else if (a.DepartmentId != Guid.Empty)
                {
                    var mappedUnits = await _dbContext.MstOrganizationUnits
                        .AsNoTracking()
                        .Where(u => u.DepartmentId == a.DepartmentId && !u.IsDelete)
                        .Select(u => u.Id)
                        .ToListAsync(cancellationToken);

                    foreach (var mu in mappedUnits) baseUnitIds.Add(mu);
                }
            }

            if (baseUnitIds.Count == 0)
                return null;

            var allScopedUnits = new HashSet<Guid>(baseUnitIds);
            var currentLevel = baseUnitIds.ToList();

            while (currentLevel.Count > 0)
            {
                var childUnits = await _dbContext.MstOrganizationUnits
                    .AsNoTracking()
                    .Where(u => u.ParentOrganizationUnitId.HasValue &&
                                currentLevel.Contains(u.ParentOrganizationUnitId.Value) &&
                                !u.IsDelete)
                    .Select(u => u.Id)
                    .ToListAsync(cancellationToken);

                currentLevel = childUnits.Where(id => allScopedUnits.Add(id)).ToList();
            }

            return allScopedUnits;
        }
    }
}
