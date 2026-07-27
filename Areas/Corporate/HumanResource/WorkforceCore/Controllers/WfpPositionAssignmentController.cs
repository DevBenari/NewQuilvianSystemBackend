using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using PositionAssignmentPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs.WfpPositionAssignmentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/position-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Position Assignment",
        AreaName = "Corporate",
        ControllerName = "WfpPositionAssignment",
        Description = "Corporate human resource workforce position assignment",
        SortOrder = 13)]
    [Tags("Corporate / Human Resource / Workforce Core / Position Assignment")]
    public class WfpPositionAssignmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";
        private static readonly string[] AssignmentTypes = { "Substantive", "Acting", "Functional", "Project", "Temporary" };
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpPositionAssignmentController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Position Assignment", Description = "Melihat metadata position assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpPositionAssignment", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpPositionAssignmentFilterMetadataResponse
            {
                DefaultFilter = new WfpPositionAssignmentDefaultFilterResponse(),
                CustomPeriods = BuildPeriods(),
                AssignmentTypeOptions = AssignmentTypes.Select(x => new WfpPositionAssignmentStringOptionResponse { Value = x, Label = BuildAssignmentTypeLabel(x) }).ToList(),
                SortOptions = new List<WfpPositionAssignmentStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal efektif" },
                    new() { Value = "positionName", Label = "Position" },
                    new() { Value = "departmentName", Label = "Department" },
                    new() { Value = "assignmentType", Label = "Tipe assignment" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "isActing", Label = "Acting" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            return Ok(ApiResponse<WfpPositionAssignmentFilterMetadataResponse>.Ok(result, "Metadata filter position assignment berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Position Assignment", Description = "Melihat ringkasan position assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpPositionAssignment", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken ct)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            var query = _dbContext.Set<WfpPositionAssignment>().AsNoTracking().Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
            var result = new WfpPositionAssignmentSummaryResponse
            {
                TotalData = await query.CountAsync(ct),
                ActiveData = await query.CountAsync(x => x.IsActive, ct),
                InactiveData = await query.CountAsync(x => !x.IsActive, ct),
                PrimaryData = await query.CountAsync(x => x.IsPrimary && x.IsActive, ct),
                ActingData = await query.CountAsync(x => x.IsActing && x.IsActive, ct)
            };
            return Ok(ApiResponse<WfpPositionAssignmentSummaryResponse>.Ok(result, "Ringkasan position assignment berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Workforce Position Assignment", Description = "Melihat pilihan position assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpPositionAssignment", "Read")]
        public async Task<IActionResult> GetOptions(Guid workforceProfileId, [FromQuery] bool onlyActive = true, [FromQuery] int take = 100, CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 200);
            var query = BuildBaseQuery(workforceProfileId);
            if (onlyActive) query = query.Where(x => x.IsActive);
            var result = await query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate).Take(take)
                .Select(x => new WfpPositionAssignmentOptionResponse
                {
                    Id = x.Id,
                    PositionId = x.PositionId,
                    PositionName = x.Position != null ? x.Position.PositionName : string.Empty,
                    AssignmentType = x.AssignmentType,
                    IsPrimary = x.IsPrimary,
                    IsActing = x.IsActing,
                    IsActive = x.IsActive
                }).ToListAsync(ct);
            return Ok(ApiResponse<List<WfpPositionAssignmentOptionResponse>>.Ok(result, "Pilihan position assignment berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Position Assignment", Description = "Melihat position assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpPositionAssignment", "Read")]
        public async Task<IActionResult> GetAll(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] string? assignmentType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isActing,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            NormalizePaging(ref pageNumber, ref pageSize);
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (!range.IsValid) return BadRequest(ApiResponse<object>.Fail(400, range.ErrorMessage!));

            var query = BuildBaseQuery(workforceProfileId);
            if (range.Start.HasValue) query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            if (departmentId.HasValue && departmentId.Value != Guid.Empty) query = query.Where(x => x.DepartmentId == departmentId.Value);
            if (positionId.HasValue && positionId.Value != Guid.Empty) query = query.Where(x => x.PositionId == positionId.Value);
            if (!string.IsNullOrWhiteSpace(assignmentType)) query = query.Where(x => x.AssignmentType == NormalizeAssignmentType(assignmentType));
            if (isPrimary.HasValue) query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isActing.HasValue) query = query.Where(x => x.IsActing == isActing.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.AssignmentType.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.PositionName.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var totalData = await query.CountAsync(ct);
            var entities = await ApplySorting(query, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var actorNames = await GetActorNamesAsync(entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy }), ct);
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<PositionAssignmentPagedResult>.Ok(new PositionAssignmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data position assignment berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Position Assignment", Description = "Melihat detail position assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpPositionAssignment", "Read")]
        public async Task<IActionResult> GetById(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var entity = await BuildBaseQuery(workforceProfileId).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Position assignment tidak ditemukan."));
            var actorNames = await GetActorNamesAsync(new[] { entity.CreateBy, entity.UpdateBy }, ct);
            return Ok(ApiResponse<WfpPositionAssignmentResponse>.Ok(MapResponse(entity, actorNames), "Detail position assignment berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Workforce Position Assignment", Description = "Membuat position assignment", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WfpPositionAssignment", "Create")]
        public async Task<IActionResult> Create(Guid workforceProfileId, [FromBody] CreateWfpPositionAssignmentRequest request, CancellationToken ct)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            var validation = await ValidateRequestAsync(workforceProfileId, null, request, ct);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimary && request.IsActive) await ClearPrimaryAsync(workforceProfileId, null, actor, now, ct);
                var entity = new WfpPositionAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    OrganizationAssignmentId = NormalizeGuid(request.OrganizationAssignmentId),
                    LegalEntityId = NormalizeGuid(request.LegalEntityId),
                    HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                    OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                    DepartmentId = NormalizeGuid(request.DepartmentId),
                    PositionId = request.PositionId,
                    JobFamilyId = NormalizeGuid(request.JobFamilyId),
                    JobLevelId = NormalizeGuid(request.JobLevelId),
                    EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId),
                    AssignmentType = NormalizeAssignmentType(request.AssignmentType),
                    EffectiveStartDate = request.EffectiveStartDate.Date,
                    EffectiveEndDate = request.EffectiveEndDate?.Date,
                    IsPrimary = request.IsPrimary,
                    IsActing = request.IsActing,
                    IsActive = request.IsActive,
                    Description = Normalize(request.Description),
                    CreateDateTime = now,
                    CreateBy = actor,
                    IsDelete = false,
                    IsCancel = false
                };
                _dbContext.Set<WfpPositionAssignment>().Add(entity);
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                await _loggerService.InfoAsync(LogCategory, "WfpPositionAssignment.Create", "Position assignment berhasil dibuat.", new { entity.Id, entity.WorkforceProfileId, entity.PositionId, entity.IsPrimary });
                return Ok(ApiResponse<object>.Ok(new { entity.Id }, "Position assignment berhasil dibuat."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Workforce Position Assignment", Description = "Mengubah position assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpPositionAssignment", "Update")]
        public async Task<IActionResult> Update(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpPositionAssignmentRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Position assignment tidak ditemukan."));
            var validation = await ValidateRequestAsync(workforceProfileId, id, request, ct);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimary && request.IsActive) await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                entity.OrganizationAssignmentId = NormalizeGuid(request.OrganizationAssignmentId);
                entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
                entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
                entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
                entity.DepartmentId = NormalizeGuid(request.DepartmentId);
                entity.PositionId = request.PositionId;
                entity.JobFamilyId = NormalizeGuid(request.JobFamilyId);
                entity.JobLevelId = NormalizeGuid(request.JobLevelId);
                entity.EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId);
                entity.AssignmentType = NormalizeAssignmentType(request.AssignmentType);
                entity.EffectiveStartDate = request.EffectiveStartDate.Date;
                entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
                entity.IsPrimary = request.IsPrimary;
                entity.IsActing = request.IsActing;
                entity.IsActive = request.IsActive;
                entity.Description = Normalize(request.Description);
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return Ok(ApiResponse<object>.Ok(null, "Position assignment berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Position Assignment", Description = "Mengubah status position assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpPositionAssignment", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpPositionAssignmentStatusRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Position assignment tidak ditemukan."));
            entity.IsActive = request.IsActive;
            if (!request.IsActive) entity.IsPrimary = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Status position assignment berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/primary")]
        [AccessAction("Update", "Update Workforce Position Assignment", Description = "Mengatur primary position assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpPositionAssignment", "Update")]
        public async Task<IActionResult> SetPrimary(Guid workforceProfileId, Guid id, [FromBody] SetWfpPositionAssignmentPrimaryRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Position assignment tidak ditemukan."));
            if (request.IsPrimary && !entity.IsActive) return BadRequest(ApiResponse<object>.Fail(400, "Position assignment tidak aktif tidak dapat dijadikan primary."));
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimary) await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                entity.IsPrimary = request.IsPrimary;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return Ok(ApiResponse<object>.Ok(null, "Primary position assignment berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Position Assignment", Description = "Menghapus position assignment", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WfpPositionAssignment", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Position assignment tidak ditemukan."));
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Position assignment berhasil dihapus."));
        }

        private IQueryable<WfpPositionAssignment> BuildBaseQuery(Guid workforceProfileId) =>
            _dbContext.Set<WfpPositionAssignment>().AsNoTracking().Include(x => x.WorkforceProfile).Include(x => x.Department).Include(x => x.Position)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

        private static IOrderedQueryable<WfpPositionAssignment> ApplySorting(IQueryable<WfpPositionAssignment> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "positionname" => desc ? query.OrderByDescending(x => x.Position != null ? x.Position.PositionName : string.Empty) : query.OrderBy(x => x.Position != null ? x.Position.PositionName : string.Empty),
                "departmentname" => desc ? query.OrderByDescending(x => x.Department != null ? x.Department.DepartmentName : string.Empty) : query.OrderBy(x => x.Department != null ? x.Department.DepartmentName : string.Empty),
                "assignmenttype" => desc ? query.OrderByDescending(x => x.AssignmentType) : query.OrderBy(x => x.AssignmentType),
                "isprimary" => desc ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate),
                "isacting" => desc ? query.OrderByDescending(x => x.IsActing) : query.OrderBy(x => x.IsActing),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid workforceProfileId, Guid? excludeId, CreateWfpPositionAssignmentRequest request, CancellationToken ct)
        {
            if (request.PositionId == Guid.Empty) return (false, "Position wajib dipilih.");
            if (!AssignmentTypes.Contains(NormalizeAssignmentType(request.AssignmentType), StringComparer.OrdinalIgnoreCase)) return (false, "Assignment type tidak valid.");
            if (request.EffectiveStartDate == default) return (false, "Effective start date wajib diisi.");
            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date) return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");
            if (!await ExistsActiveAsync<MstPosition>(request.PositionId, ct)) return (false, "Position tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstLegalEntity>(request.LegalEntityId, ct)) return (false, "Legal entity tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstHospitalSite>(request.HospitalSiteId, ct)) return (false, "Hospital site tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstOrganizationUnit>(request.OrganizationUnitId, ct)) return (false, "Organization unit tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstDepartment>(request.DepartmentId, ct)) return (false, "Department tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstJobFamily>(request.JobFamilyId, ct)) return (false, "Job family tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstJobLevel>(request.JobLevelId, ct)) return (false, "Job level tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstEmployeeGrade>(request.EmployeeGradeId, ct)) return (false, "Employee grade tidak ditemukan atau tidak aktif.");

            if (request.DepartmentId.HasValue)
            {
                var positionMatches = await _dbContext.Set<MstPosition>().AsNoTracking().AnyAsync(x => x.Id == request.PositionId && x.DepartmentId == request.DepartmentId.Value && x.IsActive && !x.IsDelete, ct);
                if (!positionMatches) return (false, "Position tidak sesuai dengan department.");
            }

            if (request.OrganizationAssignmentId.HasValue && request.OrganizationAssignmentId.Value != Guid.Empty)
            {
                var organization = await _dbContext.Set<WfpOrganizationAssignment>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.OrganizationAssignmentId.Value && x.WorkforceProfileId == workforceProfileId && x.IsActive && !x.IsDelete, ct);
                if (organization == null) return (false, "Organization assignment tidak ditemukan, bukan milik workforce, atau tidak aktif.");
                if (request.DepartmentId.HasValue && request.DepartmentId.Value != organization.DepartmentId) return (false, "Department tidak sesuai dengan organization assignment.");
                if (request.LegalEntityId.HasValue && organization.LegalEntityId.HasValue && request.LegalEntityId.Value != organization.LegalEntityId.Value) return (false, "Legal entity tidak sesuai dengan organization assignment.");
                if (request.HospitalSiteId.HasValue && organization.HospitalSiteId.HasValue && request.HospitalSiteId.Value != organization.HospitalSiteId.Value) return (false, "Hospital site tidak sesuai dengan organization assignment.");
                if (request.OrganizationUnitId.HasValue && organization.OrganizationUnitId.HasValue && request.OrganizationUnitId.Value != organization.OrganizationUnitId.Value) return (false, "Organization unit tidak sesuai dengan organization assignment.");
            }

            var duplicate = await _dbContext.Set<WfpPositionAssignment>().AsNoTracking().AnyAsync(x =>
                x.WorkforceProfileId == workforceProfileId && x.PositionId == request.PositionId && x.AssignmentType == NormalizeAssignmentType(request.AssignmentType) &&
                x.Id != excludeId && !x.IsDelete && x.EffectiveStartDate <= (request.EffectiveEndDate ?? DateTime.MaxValue).Date &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= request.EffectiveStartDate.Date), ct);
            if (duplicate) return (false, "Position assignment yang sama pada periode beririsan sudah tersedia.");
            return (true, null);
        }

        private async Task ClearPrimaryAsync(Guid workforceProfileId, Guid? excludeId, Guid actor, DateTime now, CancellationToken ct)
        {
            var rows = await _dbContext.Set<WfpPositionAssignment>().Where(x => x.WorkforceProfileId == workforceProfileId && x.IsPrimary && x.IsActive && !x.IsDelete && x.Id != excludeId).ToListAsync(ct);
            foreach (var row in rows) { row.IsPrimary = false; row.UpdateDateTime = now; row.UpdateBy = actor; }
        }

        private async Task<bool> WorkforceExistsAsync(Guid id, CancellationToken ct) => await _dbContext.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, ct);
        private IActionResult WorkforceNotFound() => NotFound(ApiResponse<object>.Fail(404, "Workforce profile tidak ditemukan atau sudah tidak aktif."));
        private async Task<WfpPositionAssignment?> FindEntityAsync(Guid workforceProfileId, Guid id, CancellationToken ct) => await _dbContext.Set<WfpPositionAssignment>().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);

        private static WfpPositionAssignmentResponse MapResponse(WfpPositionAssignment x, IReadOnlyDictionary<Guid, string?> actorNames) => new()
        {
            Id = x.Id,
            WorkforceProfileId = x.WorkforceProfileId,
            WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
            OrganizationAssignmentId = x.OrganizationAssignmentId,
            LegalEntityId = x.LegalEntityId,
            HospitalSiteId = x.HospitalSiteId,
            OrganizationUnitId = x.OrganizationUnitId,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department?.DepartmentName,
            PositionId = x.PositionId,
            PositionName = x.Position?.PositionName ?? string.Empty,
            JobFamilyId = x.JobFamilyId,
            JobLevelId = x.JobLevelId,
            EmployeeGradeId = x.EmployeeGradeId,
            AssignmentType = x.AssignmentType,
            EffectiveStartDate = x.EffectiveStartDate,
            EffectiveEndDate = x.EffectiveEndDate,
            IsPrimary = x.IsPrimary,
            IsActing = x.IsActing,
            IsActive = x.IsActive,
            Description = x.Description,
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actorNames, x.CreateBy),
            UpdateDateTime = x.UpdateDateTime,
            UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
            UpdateByName = GetActorName(actorNames, x.UpdateBy)
        };

        private async Task<Dictionary<Guid, string?>> GetActorNamesAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            var actorIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Users.AsNoTracking().Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> map, Guid id) => id == Guid.Empty ? null : map.TryGetValue(id, out var name) ? name : null;
        private Guid GetCurrentUserId() { var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"); return Guid.TryParse(value, out var id) ? id : Guid.Empty; }
        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string NormalizeAssignmentType(string value) => AssignmentTypes.FirstOrDefault(x => x.Equals(value?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? value?.Trim() ?? string.Empty;
        private static string BuildAssignmentTypeLabel(string value) => value switch { "Substantive" => "Substantif", "Acting" => "Pelaksana tugas", "Functional" => "Fungsional", "Project" => "Proyek", "Temporary" => "Sementara", _ => value };
        private static void NormalizePaging(ref int pageNumber, ref int pageSize) { pageNumber = pageNumber <= 0 ? 1 : pageNumber; pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100); }
        private async Task<bool> ExistsActiveAsync<TEntity>(Guid id, CancellationToken ct) where TEntity : IdentityModel => await _dbContext.Set<TEntity>().AsNoTracking().AnyAsync(x => EF.Property<Guid>(x, "Id") == id && EF.Property<bool>(x, "IsActive") && !EF.Property<bool>(x, "IsDelete"), ct);
        private async Task<bool> ExistsActiveIfProvidedAsync<TEntity>(Guid? id, CancellationToken ct) where TEntity : IdentityModel => !id.HasValue || id.Value == Guid.Empty || await ExistsActiveAsync<TEntity>(id.Value, ct);

        private static DateRangeResult ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) && !customPeriod.Equals("custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateTime.UtcNow.Date;
                return customPeriod.Trim().ToLowerInvariant() switch
                {
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }
            DateTime? start = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null;
            DateTime? endExclusive = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null;
            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value) return DateRangeResult.Invalid("StartDate tidak boleh lebih besar atau sama dengan EndDate.");
            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<WfpPositionAssignmentStringOptionResponse> BuildPeriods() => new()
        {
            new() { Value = "custom", Label = "Custom" }, new() { Value = "today", Label = "Hari ini" },
            new() { Value = "last7days", Label = "7 hari terakhir" }, new() { Value = "last30days", Label = "30 hari terakhir" },
            new() { Value = "thismonth", Label = "Bulan ini" }
        };

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private init; }
            public DateTime? Start { get; private init; }
            public DateTime? EndExclusive { get; private init; }
            public string? ErrorMessage { get; private init; }
            public static DateRangeResult Valid(DateTime? start, DateTime? end) => new() { IsValid = true, Start = start, EndExclusive = end };
            public static DateRangeResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
        }
    }
}
