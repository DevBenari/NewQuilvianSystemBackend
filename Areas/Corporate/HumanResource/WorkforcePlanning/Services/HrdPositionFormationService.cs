using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Implementasi layanan buku formasi jabatan berversi (BE-HRD-032).
    /// Memenuhi HRD-DEC-062, FR-HRD-098 s.d. FR-HRD-100, VR-WFP-10, VR-WFP-38, NFR-010, dan NFR-011.
    /// </summary>
    public sealed class HrdPositionFormationService : IHrdPositionFormationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HrdPositionFormationService> _logger;

        public HrdPositionFormationService(
            ApplicationDbContext dbContext,
            ILogger<HrdPositionFormationService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<WorkforcePlanningResult<PagedResult<PositionFormationResponse>>> GetEffectiveFormationsAsync(
            PositionFormationPagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var targetDate = query.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null)
            {
                // Jika filter unit diminta di luar cakupan Kepala Unit, kembalikan daftar kosong
                if (query.OrganizationUnitId.HasValue && !scopedUnitIds.Contains(query.OrganizationUnitId.Value))
                {
                    return WorkforcePlanningResult<PagedResult<PositionFormationResponse>>.Ok(new PagedResult<PositionFormationResponse>
                    {
                        PageNumber = query.PageNumber < 1 ? 1 : query.PageNumber,
                        PageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, 100),
                        TotalData = 0,
                        TotalPage = 0,
                        Items = new List<PositionFormationResponse>()
                    }, "Daftar formasi berhasil diambil.");
                }
            }

            var baseQuery = _dbContext.HrdPositionFormations
                .AsNoTracking()
                .Where(f => !f.IsDelete);

            if (scopedUnitIds != null)
            {
                baseQuery = baseQuery.Where(f => scopedUnitIds.Contains(f.OrganizationUnitId));
            }

            if (query.HospitalSiteId.HasValue && query.HospitalSiteId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(f => f.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.OrganizationUnitId.HasValue && query.OrganizationUnitId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(f => f.OrganizationUnitId == query.OrganizationUnitId.Value);
            }

            if (query.PositionId.HasValue && query.PositionId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(f => f.PositionId == query.PositionId.Value);
            }

            // Kriteria 1 (AT-HRD-D1-07 / FORMATION_CARRIES_FORWARD):
            // Formasi berlaku jika EffectiveStartDate <= targetDate dan (EffectiveEndDate == null || EffectiveEndDate >= targetDate).
            baseQuery = baseQuery.Where(f =>
                f.EffectiveStartDate <= targetDate &&
                (f.EffectiveEndDate == null || f.EffectiveEndDate >= targetDate));

            var totalData = await baseQuery.CountAsync(cancellationToken);

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, 100);

            var formations = await baseQuery
                .Include(f => f.HospitalSite)
                .Include(f => f.OrganizationUnit)
                .Include(f => f.Position)
                .OrderBy(f => f.OrganizationUnit != null ? f.OrganizationUnit.UnitName : string.Empty)
                .ThenBy(f => f.Position != null ? f.Position.PositionName : string.Empty)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var approverIds = formations.Select(f => f.ApprovedByUserId).Where(id => id != Guid.Empty).Distinct().ToList();
            var approvers = approverIds.Count > 0
                ? await _dbContext.Users.AsNoTracking()
                    .Where(u => approverIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? string.Empty, cancellationToken)
                : new Dictionary<Guid, string>();

            var items = formations.Select(f => MapToResponse(f, approvers)).ToList();

            var pagedResult = new PagedResult<PositionFormationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return WorkforcePlanningResult<PagedResult<PositionFormationResponse>>.Ok(pagedResult, "Daftar formasi berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<List<PositionFormationResponse>>> GetFormationHistoryAsync(
            Guid organizationUnitId,
            Guid positionId,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (organizationUnitId == Guid.Empty || positionId == Guid.Empty)
            {
                return WorkforcePlanningResult<List<PositionFormationResponse>>.Fail(
                    StatusCodes.Status400BadRequest,
                    "organizationUnitId dan positionId wajib diisi.");
            }

            // Kriteria 6 (VR-WFP-38): Detail unit di luar cakupan Kepala Unit menjawab 404 "Data tidak ditemukan."
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(organizationUnitId))
            {
                return WorkforcePlanningResult<List<PositionFormationResponse>>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data tidak ditemukan.");
            }

            var unit = await _dbContext.MstOrganizationUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == organizationUnitId && !u.IsDelete, cancellationToken);

            if (unit == null)
            {
                return WorkforcePlanningResult<List<PositionFormationResponse>>.Fail(
                    StatusCodes.Status404NotFound,
                    "Unit organisasi tidak ditemukan.");
            }

            var formations = await _dbContext.HrdPositionFormations
                .AsNoTracking()
                .Include(f => f.HospitalSite)
                .Include(f => f.OrganizationUnit)
                .Include(f => f.Position)
                .Where(f => f.OrganizationUnitId == organizationUnitId &&
                            f.PositionId == positionId &&
                            !f.IsDelete)
                .OrderBy(f => f.VersionNumber)
                .ToListAsync(cancellationToken);

            var approverIds = formations.Select(f => f.ApprovedByUserId).Where(id => id != Guid.Empty).Distinct().ToList();
            var approvers = approverIds.Count > 0
                ? await _dbContext.Users.AsNoTracking()
                    .Where(u => approverIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? string.Empty, cancellationToken)
                : new Dictionary<Guid, string>();

            var responses = formations.Select(f => MapToResponse(f, approvers)).ToList();

            return WorkforcePlanningResult<List<PositionFormationResponse>>.Ok(responses, "Riwayat formasi berhasil diambil.");
        }

        public async Task<HrdPositionFormation?> GetEffectiveFormationForPositionAsync(
            Guid organizationUnitId,
            Guid positionId,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.HrdPositionFormations
                .AsNoTracking()
                .Include(f => f.HospitalSite)
                .Include(f => f.OrganizationUnit)
                .Include(f => f.Position)
                .Where(f => f.OrganizationUnitId == organizationUnitId &&
                            f.PositionId == positionId &&
                            !f.IsDelete &&
                            f.EffectiveStartDate <= date &&
                            (f.EffectiveEndDate == null || f.EffectiveEndDate >= date))
                .OrderByDescending(f => f.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<HrdPositionFormation> CreateNewVersionAsync(
            Guid hospitalSiteId,
            Guid organizationUnitId,
            Guid positionId,
            decimal formationQuantity,
            DateOnly effectiveStartDate,
            PositionFormationSourceType sourceType,
            Guid sourceDocumentId,
            string sourceDocumentNumber,
            Guid approvedByUserId,
            DateTime approvedAt,
            CancellationToken cancellationToken = default)
        {
            // Kriteria 4 (NFR-010): Advisory lock PostgreSQL unit × jabatan
            await AcquireAdvisoryLockAsync(organizationUnitId, positionId, cancellationToken);

            var latestVersion = await _dbContext.HrdPositionFormations
                .Where(f => f.OrganizationUnitId == organizationUnitId &&
                            f.PositionId == positionId &&
                            !f.IsDelete)
                .OrderByDescending(f => f.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            int nextVersionNumber;
            Guid? previousFormationId;

            if (latestVersion != null)
            {
                nextVersionNumber = latestVersion.VersionNumber + 1;

                if (effectiveStartDate <= latestVersion.EffectiveStartDate)
                {
                    throw new InvalidOperationException(
                        $"Tanggal mulai versi baru ({effectiveStartDate:yyyy-MM-dd}) harus lebih besar dari tanggal mulai versi sebelumnya ({latestVersion.EffectiveStartDate:yyyy-MM-dd}).");
                }

                // Kriteria 4: Versi lama ditutup sehari sebelum tanggal mulai versi baru; kolom selain EffectiveEndDate tidak pernah diubah
                latestVersion.EffectiveEndDate = effectiveStartDate.AddDays(-1);
                latestVersion.UpdateDateTime = DateTime.UtcNow;
                latestVersion.UpdateBy = approvedByUserId;

                previousFormationId = latestVersion.Id;
            }
            else
            {
                nextVersionNumber = 1;
                previousFormationId = null;
            }

            // Kriteria 4: Melahirkan versi baru dengan EffectiveEndDate = null (terbuka/carries forward)
            var newFormation = new HrdPositionFormation
            {
                Id = Guid.NewGuid(),
                HospitalSiteId = hospitalSiteId,
                OrganizationUnitId = organizationUnitId,
                PositionId = positionId,
                VersionNumber = nextVersionNumber,
                FormationQuantity = formationQuantity,
                EffectiveStartDate = effectiveStartDate,
                EffectiveEndDate = null,
                SourceType = sourceType,
                SourceDocumentId = sourceDocumentId,
                SourceDocumentNumber = sourceDocumentNumber,
                ApprovedByUserId = approvedByUserId,
                ApprovedAt = approvedAt,
                PreviousFormationId = previousFormationId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = approvedByUserId,
                IsDelete = false
            };

            _dbContext.HrdPositionFormations.Add(newFormation);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PositionFormation version {VersionNumber} created. EntityId: {EntityId}, UnitId: {UnitId}, PositionId: {PositionId}, Quantity: {Quantity}, StartDate: {StartDate}",
                nextVersionNumber,
                newFormation.Id,
                organizationUnitId,
                positionId,
                formationQuantity,
                effectiveStartDate);

            return newFormation;
        }

        private async Task AcquireAdvisoryLockAsync(Guid organizationUnitId, Guid positionId, CancellationToken cancellationToken)
        {
            if (_dbContext.Database.IsRelational())
            {
                var lockKey = $"hrd_position_formation:{organizationUnitId}:{positionId}";
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));",
                    new object[] { lockKey },
                    cancellationToken);
            }
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

                var newChildren = childUnits.Where(c => allScopedUnits.Add(c)).ToList();
                currentLevel = newChildren;
            }

            return allScopedUnits;
        }

        private static PositionFormationResponse MapToResponse(
            HrdPositionFormation f,
            IReadOnlyDictionary<Guid, string> approvers)
        {
            return new PositionFormationResponse
            {
                Id = f.Id,
                HospitalSiteId = f.HospitalSiteId,
                HospitalSiteName = f.HospitalSite?.SiteName,
                OrganizationUnitId = f.OrganizationUnitId,
                OrganizationUnitCode = f.OrganizationUnit?.UnitCode,
                OrganizationUnitName = f.OrganizationUnit?.UnitName,
                PositionId = f.PositionId,
                PositionCode = f.Position?.PositionCode,
                PositionName = f.Position?.PositionName,
                VersionNumber = f.VersionNumber,
                FormationQuantity = f.FormationQuantity,
                EffectiveStartDate = f.EffectiveStartDate,
                EffectiveEndDate = f.EffectiveEndDate,
                SourceType = f.SourceType,
                SourceDocumentId = f.SourceDocumentId,
                SourceDocumentNumber = f.SourceDocumentNumber,
                ApprovedByUserId = f.ApprovedByUserId,
                ApprovedByUserName = approvers.GetValueOrDefault(f.ApprovedByUserId),
                ApprovedAt = f.ApprovedAt,
                PreviousFormationId = f.PreviousFormationId
            };
        }
    }
}
