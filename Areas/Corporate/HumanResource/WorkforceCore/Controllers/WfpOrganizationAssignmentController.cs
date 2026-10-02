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
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

using OrganizationAssignmentPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs.WfpOrganizationAssignmentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/organization-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Organization Assignment",
        AreaName = "Corporate",
        ControllerName = "WfpOrganizationAssignment",
        Description = "Corporate human resource workforce organization assignment",
        SortOrder = 12)]
    [Tags("Corporate / Human Resource / Workforce Core / Organization Assignment")]
    public class WfpOrganizationAssignmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";
        private static readonly string[] AssignmentTypes = { "Primary", "Secondary", "Acting", "Temporary", "Project", "Functional" };
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly OrganizationAuthorizationProjectionService _authorizationProjection;

        public WfpOrganizationAssignmentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            OrganizationAuthorizationProjectionService authorizationProjection)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _authorizationProjection = authorizationProjection;
        }

        /// <summary>
        /// Menyelaraskan proyeksi otorisasi setelah penempatan berubah.
        ///
        /// Sebelum Phase A0, seluruh endpoint pada controller ini mengubah penempatan tanpa
        /// menyentuh <c>AspNetUserOrganization</c> sama sekali, sehingga menambah penempatan tidak
        /// pernah melahirkan izin dan menonaktifkannya tidak pernah mencabut izin.
        /// </summary>
        private Task SyncAuthorizationProjectionAsync(Guid workforceProfileId, CancellationToken cancellationToken) =>
            _authorizationProjection.ReconcileWorkforceProfileAsync(
                workforceProfileId,
                GetCurrentUserId(),
                dryRun: false,
                cancellationToken);

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Organization Assignment", Description = "Melihat metadata organization assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpOrganizationAssignment", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpOrganizationAssignmentFilterMetadataResponse
            {
                DefaultFilter = new WfpOrganizationAssignmentDefaultFilterResponse(),
                CustomPeriods = BuildPeriods(),
                AssignmentTypeOptions = AssignmentTypes.Select(x => new WfpOrganizationAssignmentStringOptionResponse { Value = x, Label = BuildAssignmentTypeLabel(x) }).ToList(),
                SortOptions = new List<WfpOrganizationAssignmentStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal efektif" },
                    new() { Value = "departmentName", Label = "Department" },
                    new() { Value = "positionName", Label = "Position" },
                    new() { Value = "assignmentType", Label = "Tipe assignment" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "isManagerialAssignment", Label = "Managerial" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            return Ok(ApiResponse<WfpOrganizationAssignmentFilterMetadataResponse>.Ok(result, "Metadata filter organization assignment berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Organization Assignment", Description = "Melihat ringkasan organization assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpOrganizationAssignment", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken ct)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            var query = _dbContext.Set<WfpOrganizationAssignment>().AsNoTracking().Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
            var result = new WfpOrganizationAssignmentSummaryResponse
            {
                TotalData = await query.CountAsync(ct),
                ActiveData = await query.CountAsync(x => x.IsActive, ct),
                InactiveData = await query.CountAsync(x => !x.IsActive, ct),
                PrimaryData = await query.CountAsync(x => x.IsPrimary && x.IsActive, ct),
                ManagerialData = await query.CountAsync(x => x.IsManagerialAssignment && x.IsActive, ct)
            };
            return Ok(ApiResponse<WfpOrganizationAssignmentSummaryResponse>.Ok(result, "Ringkasan organization assignment berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Workforce Organization Assignment", Description = "Melihat pilihan organization assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpOrganizationAssignment", "Read")]
        public async Task<IActionResult> GetOptions(Guid workforceProfileId, [FromQuery] bool onlyActive = true, [FromQuery] int take = 100, CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 200);
            var query = BuildBaseQuery(workforceProfileId);
            if (onlyActive) query = query.Where(x => x.IsActive);
            var result = await query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate).Take(take)
                .Select(x => new WfpOrganizationAssignmentOptionResponse
                {
                    Id = x.Id,
                    AssignmentType = x.AssignmentType,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
                    PositionName = x.Position != null ? x.Position.PositionName : string.Empty,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive
                }).ToListAsync(ct);
            return Ok(ApiResponse<List<WfpOrganizationAssignmentOptionResponse>>.Ok(result, "Pilihan organization assignment berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Organization Assignment", Description = "Melihat organization assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpOrganizationAssignment", "Read")]
        public async Task<IActionResult> GetAll(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] string? assignmentType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isManagerialAssignment,
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
            if (isManagerialAssignment.HasValue) query = query.Where(x => x.IsManagerialAssignment == isManagerialAssignment.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.AssignmentType.ToLower().Contains(keyword) ||
                    (x.AssignmentNumber != null && x.AssignmentNumber.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.PositionName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(ct);
            var entities = await ApplySorting(query, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var actorNames = await GetActorNamesAsync(entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy }), ct);
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<OrganizationAssignmentPagedResult>.Ok(new OrganizationAssignmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data organization assignment berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Organization Assignment", Description = "Melihat detail organization assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpOrganizationAssignment", "Read")]
        public async Task<IActionResult> GetById(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var entity = await BuildBaseQuery(workforceProfileId).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Organization assignment tidak ditemukan."));
            var actorNames = await GetActorNamesAsync(new[] { entity.CreateBy, entity.UpdateBy }, ct);
            return Ok(ApiResponse<WfpOrganizationAssignmentResponse>.Ok(MapResponse(entity, actorNames), "Detail organization assignment berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Workforce Organization Assignment", Description = "Membuat organization assignment", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WfpOrganizationAssignment", "Create")]
        public async Task<IActionResult> Create(Guid workforceProfileId, [FromBody] CreateWfpOrganizationAssignmentRequest request, CancellationToken ct)
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
                var entity = new WfpOrganizationAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    LegalEntityId = NormalizeGuid(request.LegalEntityId),
                    HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                    OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                    DepartmentId = request.DepartmentId,
                    PositionId = request.PositionId,
                    CostCenterId = NormalizeGuid(request.CostCenterId),
                    WorkLocationId = NormalizeGuid(request.WorkLocationId),
                    EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId),
                    AssignmentType = NormalizeAssignmentType(request.AssignmentType),
                    IsPrimary = request.IsPrimary,
                    IsManagerialAssignment = request.IsManagerialAssignment,
                    EffectiveStartDate = request.EffectiveStartDate.Date,
                    EffectiveEndDate = request.EffectiveEndDate?.Date,
                    AssignmentNumber = Normalize(request.AssignmentNumber),
                    Description = Normalize(request.Description),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actor,
                    IsDelete = false,
                    IsCancel = false
                };
                _dbContext.Set<WfpOrganizationAssignment>().Add(entity);
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                await _loggerService.InfoAsync(LogCategory, "WfpOrganizationAssignment.Create", "Organization assignment berhasil dibuat.", new { entity.Id, entity.WorkforceProfileId, entity.DepartmentId, entity.PositionId, entity.IsPrimary });
                await SyncAuthorizationProjectionAsync(workforceProfileId, ct);
                return Ok(ApiResponse<object>.Ok(new { entity.Id }, "Organization assignment berhasil dibuat."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Workforce Organization Assignment", Description = "Mengubah organization assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpOrganizationAssignment", "Update")]
        public async Task<IActionResult> Update(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpOrganizationAssignmentRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Organization assignment tidak ditemukan."));
            var validation = await ValidateRequestAsync(workforceProfileId, id, request, ct);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimary && request.IsActive) await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
                entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
                entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
                entity.DepartmentId = request.DepartmentId;
                entity.PositionId = request.PositionId;
                entity.CostCenterId = NormalizeGuid(request.CostCenterId);
                entity.WorkLocationId = NormalizeGuid(request.WorkLocationId);
                entity.EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId);
                entity.AssignmentType = NormalizeAssignmentType(request.AssignmentType);
                entity.IsPrimary = request.IsPrimary;
                entity.IsManagerialAssignment = request.IsManagerialAssignment;
                entity.EffectiveStartDate = request.EffectiveStartDate.Date;
                entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
                entity.AssignmentNumber = Normalize(request.AssignmentNumber);
                entity.Description = Normalize(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                await SyncAuthorizationProjectionAsync(workforceProfileId, ct);
                return Ok(ApiResponse<object>.Ok(null, "Organization assignment berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Organization Assignment", Description = "Mengubah status organization assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpOrganizationAssignment", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpOrganizationAssignmentStatusRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Organization assignment tidak ditemukan."));
            entity.IsActive = request.IsActive;
            if (!request.IsActive) entity.IsPrimary = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(ct);
            await SyncAuthorizationProjectionAsync(workforceProfileId, ct);
            return Ok(ApiResponse<object>.Ok(null, "Status organization assignment berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/primary")]
        [AccessAction("Update", "Update Workforce Organization Assignment", Description = "Mengatur primary organization assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpOrganizationAssignment", "Update")]
        public async Task<IActionResult> SetPrimary(Guid workforceProfileId, Guid id, [FromBody] SetWfpOrganizationAssignmentPrimaryRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Organization assignment tidak ditemukan."));
            if (request.IsPrimary && !entity.IsActive) return BadRequest(ApiResponse<object>.Fail(400, "Organization assignment tidak aktif tidak dapat dijadikan primary."));
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
                await SyncAuthorizationProjectionAsync(workforceProfileId, ct);
                return Ok(ApiResponse<object>.Ok(null, "Primary organization assignment berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Organization Assignment", Description = "Menghapus organization assignment", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WfpOrganizationAssignment", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Organization assignment tidak ditemukan."));
            var hasPositionAssignments = await _dbContext.Set<WfpPositionAssignment>().AsNoTracking().AnyAsync(x => x.OrganizationAssignmentId == id && !x.IsDelete, ct);
            if (hasPositionAssignments) return BadRequest(ApiResponse<object>.Fail(400, "Organization assignment masih digunakan oleh position assignment."));
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
            await SyncAuthorizationProjectionAsync(workforceProfileId, ct);
            return Ok(ApiResponse<object>.Ok(null, "Organization assignment berhasil dihapus."));
        }

        private IQueryable<WfpOrganizationAssignment> BuildBaseQuery(Guid workforceProfileId) =>
            _dbContext.Set<WfpOrganizationAssignment>().AsNoTracking().Include(x => x.WorkforceProfile).Include(x => x.Department).Include(x => x.Position)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

        private static IOrderedQueryable<WfpOrganizationAssignment> ApplySorting(IQueryable<WfpOrganizationAssignment> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "departmentname" => desc ? query.OrderByDescending(x => x.Department != null ? x.Department.DepartmentName : string.Empty) : query.OrderBy(x => x.Department != null ? x.Department.DepartmentName : string.Empty),
                "positionname" => desc ? query.OrderByDescending(x => x.Position != null ? x.Position.PositionName : string.Empty) : query.OrderBy(x => x.Position != null ? x.Position.PositionName : string.Empty),
                "assignmenttype" => desc ? query.OrderByDescending(x => x.AssignmentType) : query.OrderBy(x => x.AssignmentType),
                "isprimary" => desc ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate),
                "ismanagerialassignment" => desc ? query.OrderByDescending(x => x.IsManagerialAssignment) : query.OrderBy(x => x.IsManagerialAssignment),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid workforceProfileId, Guid? excludeId, CreateWfpOrganizationAssignmentRequest request, CancellationToken ct)
        {
            if (request.DepartmentId == Guid.Empty) return (false, "Department wajib dipilih.");
            if (request.PositionId == Guid.Empty) return (false, "Position wajib dipilih.");
            if (!AssignmentTypes.Contains(NormalizeAssignmentType(request.AssignmentType), StringComparer.OrdinalIgnoreCase)) return (false, "Assignment type tidak valid.");
            if (request.EffectiveStartDate == default) return (false, "Effective start date wajib diisi.");
            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date) return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");
            if (!await ExistsActiveAsync<MstDepartment>(request.DepartmentId, ct)) return (false, "Department tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveAsync<MstPosition>(request.PositionId, ct)) return (false, "Position tidak ditemukan atau tidak aktif.");
            var positionMatches = await _dbContext.Set<MstPosition>().AsNoTracking().AnyAsync(x => x.Id == request.PositionId && x.DepartmentId == request.DepartmentId && x.IsActive && !x.IsDelete, ct);
            if (!positionMatches) return (false, "Position tidak sesuai dengan department.");
            if (!await ExistsActiveIfProvidedAsync<MstLegalEntity>(request.LegalEntityId, ct)) return (false, "Legal entity tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstHospitalSite>(request.HospitalSiteId, ct)) return (false, "Hospital site tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstOrganizationUnit>(request.OrganizationUnitId, ct)) return (false, "Organization unit tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstCostCenter>(request.CostCenterId, ct)) return (false, "Cost center tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstWorkLocation>(request.WorkLocationId, ct)) return (false, "Work location tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstEmployeeGrade>(request.EmployeeGradeId, ct)) return (false, "Employee grade tidak ditemukan atau tidak aktif.");

            var normalizedNumber = Normalize(request.AssignmentNumber);
            if (!string.IsNullOrWhiteSpace(normalizedNumber))
            {
                var duplicateNumber = await _dbContext.Set<WfpOrganizationAssignment>().AsNoTracking().AnyAsync(x => x.WorkforceProfileId == workforceProfileId && x.AssignmentNumber == normalizedNumber && x.Id != excludeId && !x.IsDelete, ct);
                if (duplicateNumber) return (false, "Assignment number sudah digunakan.");
            }
            return (true, null);
        }

        private async Task ClearPrimaryAsync(Guid workforceProfileId, Guid? excludeId, Guid actor, DateTime now, CancellationToken ct)
        {
            var rows = await _dbContext.Set<WfpOrganizationAssignment>().Where(x => x.WorkforceProfileId == workforceProfileId && x.IsPrimary && x.IsActive && !x.IsDelete && x.Id != excludeId).ToListAsync(ct);
            foreach (var row in rows) { row.IsPrimary = false; row.UpdateDateTime = now; row.UpdateBy = actor; }
        }

        private async Task<bool> WorkforceExistsAsync(Guid id, CancellationToken ct) => await _dbContext.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, ct);
        private IActionResult WorkforceNotFound() => NotFound(ApiResponse<object>.Fail(404, "Workforce profile tidak ditemukan atau sudah tidak aktif."));
        private async Task<WfpOrganizationAssignment?> FindEntityAsync(Guid workforceProfileId, Guid id, CancellationToken ct) => await _dbContext.Set<WfpOrganizationAssignment>().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);

        private static WfpOrganizationAssignmentResponse MapResponse(WfpOrganizationAssignment x, IReadOnlyDictionary<Guid, string?> actorNames) => new()
        {
            Id = x.Id,
            WorkforceProfileId = x.WorkforceProfileId,
            WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
            LegalEntityId = x.LegalEntityId,
            HospitalSiteId = x.HospitalSiteId,
            OrganizationUnitId = x.OrganizationUnitId,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department?.DepartmentName ?? string.Empty,
            PositionId = x.PositionId,
            PositionName = x.Position?.PositionName ?? string.Empty,
            CostCenterId = x.CostCenterId,
            WorkLocationId = x.WorkLocationId,
            EmployeeGradeId = x.EmployeeGradeId,
            AssignmentType = x.AssignmentType,
            IsPrimary = x.IsPrimary,
            IsManagerialAssignment = x.IsManagerialAssignment,
            EffectiveStartDate = x.EffectiveStartDate,
            EffectiveEndDate = x.EffectiveEndDate,
            AssignmentNumber = x.AssignmentNumber,
            Description = x.Description,
            IsActive = x.IsActive,
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
        private static string BuildAssignmentTypeLabel(string value) => value switch { "Primary" => "Utama", "Secondary" => "Sekunder", "Acting" => "Pelaksana tugas", "Temporary" => "Sementara", "Project" => "Proyek", "Functional" => "Fungsional", _ => value };
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

        private static List<WfpOrganizationAssignmentStringOptionResponse> BuildPeriods() => new()
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
