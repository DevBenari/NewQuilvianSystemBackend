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

using ManagerAssignmentPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs.WfpManagerAssignmentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/manager-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Manager Assignment",
        AreaName = "Corporate",
        ControllerName = "WfpManagerAssignment",
        Description = "Corporate human resource workforce manager assignment",
        SortOrder = 11)]
    [Tags("Corporate / Human Resource / Workforce Core / Manager Assignment")]
    public class WfpManagerAssignmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";
        private static readonly string[] ManagerTypes = { "Direct", "Functional", "Project", "Acting", "DottedLine" };
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpManagerAssignmentController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Manager Assignment", Description = "Melihat metadata manager assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpManagerAssignment", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpManagerAssignmentFilterMetadataResponse
            {
                DefaultFilter = new WfpManagerAssignmentDefaultFilterResponse(),
                CustomPeriods = BuildPeriods(),
                ManagerTypeOptions = ManagerTypes.Select(x => new WfpManagerAssignmentStringOptionResponse { Value = x, Label = BuildManagerTypeLabel(x) }).ToList(),
                SortOptions = new List<WfpManagerAssignmentStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal efektif" },
                    new() { Value = "managerDisplayName", Label = "Nama manager" },
                    new() { Value = "managerType", Label = "Tipe manager" },
                    new() { Value = "isPrimaryManager", Label = "Primary manager" },
                    new() { Value = "canApproveRequests", Label = "Hak approval" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            return Ok(ApiResponse<WfpManagerAssignmentFilterMetadataResponse>.Ok(result, "Metadata filter manager assignment berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Manager Assignment", Description = "Melihat ringkasan manager assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpManagerAssignment", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken ct)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            var query = _dbContext.Set<WfpManagerAssignment>().AsNoTracking().Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
            var result = new WfpManagerAssignmentSummaryResponse
            {
                TotalData = await query.CountAsync(ct),
                ActiveData = await query.CountAsync(x => x.IsActive, ct),
                InactiveData = await query.CountAsync(x => !x.IsActive, ct),
                PrimaryManagerData = await query.CountAsync(x => x.IsPrimaryManager && x.IsActive, ct),
                ApprovalEnabledData = await query.CountAsync(x => x.CanApproveRequests && x.IsActive, ct)
            };
            return Ok(ApiResponse<WfpManagerAssignmentSummaryResponse>.Ok(result, "Ringkasan manager assignment berhasil diambil."));
        }

        [HttpGet("manager-options")]
        [AccessAction("Read", "Read Workforce Manager Assignment", Description = "Melihat kandidat manager", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpManagerAssignment", "Read")]
        public async Task<IActionResult> GetManagerOptions(Guid workforceProfileId, [FromQuery] string? search = null, [FromQuery] int take = 100, CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 200);
            var query = _dbContext.Set<MstWorkforceProfile>().AsNoTracking().Where(x => x.Id != workforceProfileId && x.IsActive && !x.IsDelete);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.ProfileCode.ToLower().Contains(keyword) || x.DisplayName.ToLower().Contains(keyword));
            }
            var result = await query.OrderBy(x => x.DisplayName).Take(take).Select(x => new WfpManagerCandidateOptionResponse
            {
                WorkforceProfileId = x.Id,
                ProfileCode = x.ProfileCode,
                DisplayName = x.DisplayName
            }).ToListAsync(ct);
            return Ok(ApiResponse<List<WfpManagerCandidateOptionResponse>>.Ok(result, "Kandidat manager berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Manager Assignment", Description = "Melihat manager assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpManagerAssignment", "Read")]
        public async Task<IActionResult> GetAll(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? managerWorkforceProfileId,
            [FromQuery] Guid? departmentId,
            [FromQuery] string? managerType,
            [FromQuery] bool? isPrimaryManager,
            [FromQuery] bool? canApproveRequests,
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
            if (managerWorkforceProfileId.HasValue && managerWorkforceProfileId.Value != Guid.Empty) query = query.Where(x => x.ManagerWorkforceProfileId == managerWorkforceProfileId.Value);
            if (departmentId.HasValue && departmentId.Value != Guid.Empty) query = query.Where(x => x.DepartmentId == departmentId.Value);
            if (!string.IsNullOrWhiteSpace(managerType)) query = query.Where(x => x.ManagerType == NormalizeManagerType(managerType));
            if (isPrimaryManager.HasValue) query = query.Where(x => x.IsPrimaryManager == isPrimaryManager.Value);
            if (canApproveRequests.HasValue) query = query.Where(x => x.CanApproveRequests == canApproveRequests.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ManagerType.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.ManagerWorkforceProfile != null && (x.ManagerWorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.ManagerWorkforceProfile.DisplayName.ToLower().Contains(keyword))) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.ManagerPosition != null && x.ManagerPosition.PositionName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(ct);
            var entities = await ApplySorting(query, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var actorNames = await GetActorNamesAsync(entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy }), ct);
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<ManagerAssignmentPagedResult>.Ok(new ManagerAssignmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data manager assignment berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Manager Assignment", Description = "Melihat detail manager assignment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpManagerAssignment", "Read")]
        public async Task<IActionResult> GetById(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var entity = await BuildBaseQuery(workforceProfileId).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Manager assignment tidak ditemukan."));
            var actorNames = await GetActorNamesAsync(new[] { entity.CreateBy, entity.UpdateBy }, ct);
            return Ok(ApiResponse<WfpManagerAssignmentResponse>.Ok(MapResponse(entity, actorNames), "Detail manager assignment berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Workforce Manager Assignment", Description = "Membuat manager assignment", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WfpManagerAssignment", "Create")]
        public async Task<IActionResult> Create(Guid workforceProfileId, [FromBody] CreateWfpManagerAssignmentRequest request, CancellationToken ct)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, ct)) return WorkforceNotFound();
            var validation = await ValidateRequestAsync(workforceProfileId, null, request, ct);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimaryManager && request.IsActive) await ClearPrimaryAsync(workforceProfileId, null, actor, now, ct);
                var entity = new WfpManagerAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    ManagerWorkforceProfileId = request.ManagerWorkforceProfileId,
                    OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                    DepartmentId = NormalizeGuid(request.DepartmentId),
                    ManagerPositionId = NormalizeGuid(request.ManagerPositionId),
                    ManagerType = NormalizeManagerType(request.ManagerType),
                    EffectiveStartDate = request.EffectiveStartDate.Date,
                    EffectiveEndDate = request.EffectiveEndDate?.Date,
                    IsPrimaryManager = request.IsPrimaryManager,
                    CanApproveRequests = request.CanApproveRequests,
                    IsActive = request.IsActive,
                    Description = Normalize(request.Description),
                    CreateDateTime = now,
                    CreateBy = actor,
                    IsDelete = false,
                    IsCancel = false
                };
                _dbContext.Set<WfpManagerAssignment>().Add(entity);
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                await _loggerService.InfoAsync(LogCategory, "WfpManagerAssignment.Create", "Manager assignment berhasil dibuat.", new { entity.Id, entity.WorkforceProfileId, entity.ManagerWorkforceProfileId, entity.IsPrimaryManager });
                return Ok(ApiResponse<object>.Ok(new { entity.Id }, "Manager assignment berhasil dibuat."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Workforce Manager Assignment", Description = "Mengubah manager assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpManagerAssignment", "Update")]
        public async Task<IActionResult> Update(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpManagerAssignmentRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Manager assignment tidak ditemukan."));
            var validation = await ValidateRequestAsync(workforceProfileId, id, request, ct);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimaryManager && request.IsActive) await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                entity.ManagerWorkforceProfileId = request.ManagerWorkforceProfileId;
                entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
                entity.DepartmentId = NormalizeGuid(request.DepartmentId);
                entity.ManagerPositionId = NormalizeGuid(request.ManagerPositionId);
                entity.ManagerType = NormalizeManagerType(request.ManagerType);
                entity.EffectiveStartDate = request.EffectiveStartDate.Date;
                entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
                entity.IsPrimaryManager = request.IsPrimaryManager;
                entity.CanApproveRequests = request.CanApproveRequests;
                entity.IsActive = request.IsActive;
                entity.Description = Normalize(request.Description);
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return Ok(ApiResponse<object>.Ok(null, "Manager assignment berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Manager Assignment", Description = "Mengubah status manager assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpManagerAssignment", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpManagerAssignmentStatusRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Manager assignment tidak ditemukan."));
            entity.IsActive = request.IsActive;
            if (!request.IsActive) entity.IsPrimaryManager = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Status manager assignment berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/primary")]
        [AccessAction("Update", "Update Workforce Manager Assignment", Description = "Mengatur primary manager", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpManagerAssignment", "Update")]
        public async Task<IActionResult> SetPrimary(Guid workforceProfileId, Guid id, [FromBody] SetWfpManagerAssignmentPrimaryRequest request, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Manager assignment tidak ditemukan."));
            if (request.IsPrimaryManager && !entity.IsActive) return BadRequest(ApiResponse<object>.Fail(400, "Manager assignment tidak aktif tidak dapat dijadikan primary."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.IsPrimaryManager) await ClearPrimaryAsync(workforceProfileId, id, actor, now, ct);
                entity.IsPrimaryManager = request.IsPrimaryManager;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor;
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return Ok(ApiResponse<object>.Ok(null, "Primary manager berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Manager Assignment", Description = "Menghapus manager assignment", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WfpManagerAssignment", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, ct);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Manager assignment tidak ditemukan."));
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimaryManager = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Manager assignment berhasil dihapus."));
        }

        private IQueryable<WfpManagerAssignment> BuildBaseQuery(Guid workforceProfileId) =>
            _dbContext.Set<WfpManagerAssignment>().AsNoTracking()
                .Include(x => x.WorkforceProfile).Include(x => x.ManagerWorkforceProfile)
                .Include(x => x.Department).Include(x => x.ManagerPosition)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

        private static IOrderedQueryable<WfpManagerAssignment> ApplySorting(IQueryable<WfpManagerAssignment> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "managerdisplayname" => desc ? query.OrderByDescending(x => x.ManagerWorkforceProfile != null ? x.ManagerWorkforceProfile.DisplayName : string.Empty) : query.OrderBy(x => x.ManagerWorkforceProfile != null ? x.ManagerWorkforceProfile.DisplayName : string.Empty),
                "managertype" => desc ? query.OrderByDescending(x => x.ManagerType) : query.OrderBy(x => x.ManagerType),
                "isprimarymanager" => desc ? query.OrderByDescending(x => x.IsPrimaryManager).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsPrimaryManager).ThenByDescending(x => x.EffectiveStartDate),
                "canapproverequests" => desc ? query.OrderByDescending(x => x.CanApproveRequests) : query.OrderBy(x => x.CanApproveRequests),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid workforceProfileId, Guid? excludeId, CreateWfpManagerAssignmentRequest request, CancellationToken ct)
        {
            if (request.ManagerWorkforceProfileId == Guid.Empty) return (false, "Manager workforce profile wajib dipilih.");
            if (workforceProfileId == request.ManagerWorkforceProfileId) return (false, "Workforce profile tidak dapat menjadi manager untuk dirinya sendiri.");
            if (!ManagerTypes.Contains(NormalizeManagerType(request.ManagerType), StringComparer.OrdinalIgnoreCase)) return (false, "Manager type tidak valid.");
            if (request.EffectiveStartDate == default) return (false, "Effective start date wajib diisi.");
            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date) return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");
            if (!await WorkforceExistsAsync(request.ManagerWorkforceProfileId, ct)) return (false, "Manager workforce profile tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstOrganizationUnit>(request.OrganizationUnitId, ct)) return (false, "Organization unit tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstDepartment>(request.DepartmentId, ct)) return (false, "Department tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstPosition>(request.ManagerPositionId, ct)) return (false, "Manager position tidak ditemukan atau tidak aktif.");
            if (request.DepartmentId.HasValue && request.ManagerPositionId.HasValue)
            {
                var matches = await _dbContext.Set<MstPosition>().AsNoTracking().AnyAsync(x => x.Id == request.ManagerPositionId.Value && x.DepartmentId == request.DepartmentId.Value && x.IsActive && !x.IsDelete, ct);
                if (!matches) return (false, "Manager position tidak sesuai dengan department.");
            }
            if (await WouldCreateCycleAsync(workforceProfileId, request.ManagerWorkforceProfileId, excludeId, ct)) return (false, "Relasi manager akan membentuk siklus hierarki.");

            var duplicate = await _dbContext.Set<WfpManagerAssignment>().AsNoTracking().AnyAsync(x =>
                x.WorkforceProfileId == workforceProfileId && x.ManagerWorkforceProfileId == request.ManagerWorkforceProfileId &&
                x.ManagerType == NormalizeManagerType(request.ManagerType) && x.Id != excludeId && !x.IsDelete &&
                x.EffectiveStartDate <= (request.EffectiveEndDate ?? DateTime.MaxValue).Date &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= request.EffectiveStartDate.Date), ct);
            if (duplicate) return (false, "Manager assignment yang sama pada periode beririsan sudah tersedia.");
            return (true, null);
        }

        private async Task<bool> WouldCreateCycleAsync(Guid workforceProfileId, Guid managerWorkforceProfileId, Guid? excludeId, CancellationToken ct)
        {
            var current = managerWorkforceProfileId;
            var visited = new HashSet<Guid>();
            var activeDate = DateTime.UtcNow.Date;
            for (var depth = 0; depth < 100; depth++)
            {
                if (current == workforceProfileId) return true;
                if (!visited.Add(current)) return true;
                var next = await _dbContext.Set<WfpManagerAssignment>().AsNoTracking()
                    .Where(x => x.WorkforceProfileId == current && x.IsPrimaryManager && x.IsActive && !x.IsDelete && x.Id != excludeId &&
                                x.EffectiveStartDate <= activeDate && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= activeDate))
                    .Select(x => (Guid?)x.ManagerWorkforceProfileId).FirstOrDefaultAsync(ct);
                if (!next.HasValue || next.Value == Guid.Empty) return false;
                current = next.Value;
            }
            return true;
        }

        private async Task ClearPrimaryAsync(Guid workforceProfileId, Guid? excludeId, Guid actor, DateTime now, CancellationToken ct)
        {
            var rows = await _dbContext.Set<WfpManagerAssignment>().Where(x => x.WorkforceProfileId == workforceProfileId && x.IsPrimaryManager && x.IsActive && !x.IsDelete && x.Id != excludeId).ToListAsync(ct);
            foreach (var row in rows) { row.IsPrimaryManager = false; row.UpdateDateTime = now; row.UpdateBy = actor; }
        }

        private async Task<bool> WorkforceExistsAsync(Guid id, CancellationToken ct) =>
            await _dbContext.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, ct);

        private IActionResult WorkforceNotFound() => NotFound(ApiResponse<object>.Fail(404, "Workforce profile tidak ditemukan atau sudah tidak aktif."));
        private async Task<WfpManagerAssignment?> FindEntityAsync(Guid workforceProfileId, Guid id, CancellationToken ct) => await _dbContext.Set<WfpManagerAssignment>().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);

        private static WfpManagerAssignmentResponse MapResponse(WfpManagerAssignment x, IReadOnlyDictionary<Guid, string?> actorNames) => new()
        {
            Id = x.Id,
            WorkforceProfileId = x.WorkforceProfileId,
            WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
            ManagerWorkforceProfileId = x.ManagerWorkforceProfileId,
            ManagerProfileCode = x.ManagerWorkforceProfile?.ProfileCode ?? string.Empty,
            ManagerDisplayName = x.ManagerWorkforceProfile?.DisplayName ?? string.Empty,
            OrganizationUnitId = x.OrganizationUnitId,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department?.DepartmentName,
            ManagerPositionId = x.ManagerPositionId,
            ManagerPositionName = x.ManagerPosition?.PositionName,
            ManagerType = x.ManagerType,
            EffectiveStartDate = x.EffectiveStartDate,
            EffectiveEndDate = x.EffectiveEndDate,
            IsPrimaryManager = x.IsPrimaryManager,
            CanApproveRequests = x.CanApproveRequests,
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
        private static string NormalizeManagerType(string value) => ManagerTypes.FirstOrDefault(x => x.Equals(value?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? value?.Trim() ?? string.Empty;
        private static string BuildManagerTypeLabel(string value) => value switch { "Direct" => "Atasan langsung", "Functional" => "Fungsional", "Project" => "Proyek", "Acting" => "Pelaksana tugas", "DottedLine" => "Garis koordinasi", _ => value };
        private static void NormalizePaging(ref int pageNumber, ref int pageSize) { pageNumber = pageNumber <= 0 ? 1 : pageNumber; pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100); }
        private async Task<bool> ExistsActiveIfProvidedAsync<TEntity>(Guid? id, CancellationToken ct) where TEntity : IdentityModel => !id.HasValue || id.Value == Guid.Empty || await _dbContext.Set<TEntity>().AsNoTracking().AnyAsync(x => EF.Property<Guid>(x, "Id") == id.Value && EF.Property<bool>(x, "IsActive") && !EF.Property<bool>(x, "IsDelete"), ct);

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

        private static List<WfpManagerAssignmentStringOptionResponse> BuildPeriods() => new()
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
