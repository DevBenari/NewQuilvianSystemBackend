using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using OvertimePolicyPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs.OvertimePolicyResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/overtime-policies")]
    [AccessController(moduleCode: "HUMAN_RESOURCE_MASTER_DATA", moduleName: "Human Resource Master Data", displayName: "Overtime Policy", AreaName = "Corporate", ControllerName = "OvertimePolicy", Description = "Corporate human resource master data overtime policy", SortOrder = 35)]
    [Tags("Corporate / Human Resource / Master Data / Leave and Overtime / Overtime Policy")]
    public class OvertimePolicyController : ControllerBase
    {
        private static readonly HashSet<string> AllowedRoundingMethods =
            OvertimeValueConstants.RoundingMethod.All.ToHashSet(StringComparer.OrdinalIgnoreCase);
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "OTP-RSMMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly OvertimePolicyResolverService _policyResolverService;

        public OvertimePolicyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            OvertimePolicyResolverService policyResolverService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _policyResolverService = policyResolverService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Overtime Policy", Description = "Melihat metadata filter overtime policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePolicy", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new OvertimePolicyFilterMetadataResponse
            {
                DefaultFilter = new OvertimePolicyDefaultFilterResponse(),
                RoundingMethodOptions = AllowedRoundingMethods.Select(x => new OvertimePolicyStringOptionResponse { Value = x, Label = x }).ToList(),
                SortOptions = new List<OvertimePolicySortOptionResponse>
                {
                    new() { Value = "overtimePolicyCode", Label = "Kode kebijakan lembur" },
                    new() { Value = "overtimePolicyName", Label = "Nama kebijakan lembur" },
                    new() { Value = "priority", Label = "Prioritas resolver" },
                    new() { Value = "minimumOvertimeMinutes", Label = "Minimum menit lembur" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isDefault", Label = "Kebijakan default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            await _loggerService.InfoAsync(LogCategory, "OvertimePolicy.GetFilterMetadata", "Mengambil metadata filter overtime policy.", result);
            return Ok(ApiResponse<OvertimePolicyFilterMetadataResponse>.Ok(result, "Metadata filter overtime policy berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Overtime Policy", Description = "Melihat ringkasan overtime policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePolicy", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var q = BaseQuery();
            var result = new OvertimePolicySummaryResponse
            {
                TotalData = await q.CountAsync(), ActiveData = await q.CountAsync(x => x.IsActive),
                InactiveData = await q.CountAsync(x => !x.IsActive), DefaultData = await q.CountAsync(x => x.IsDefault && x.IsActive),
                PreApprovalRequiredData = await q.CountAsync(x => x.RequirePreApproval),
                AttendanceMatchRequiredData = await q.CountAsync(x => x.RequireAttendanceMatch)
            };
            return Ok(ApiResponse<OvertimePolicySummaryResponse>.Ok(result, "Ringkasan overtime policy berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Overtime Policy", Description = "Melihat data overtime policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePolicy", "Read")]
        public async Task<IActionResult> GetData(Guid? legalEntityId, Guid? hospitalSiteId, Guid? organizationUnitId, Guid? employeeCategoryId, Guid? employmentTypeId, bool? requirePreApproval, bool? requireAttendanceMatch, bool? isDefault, bool? isActive, string? search, string? sortBy = "overtimePolicyName", string? sortDirection = "asc", int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), legalEntityId, hospitalSiteId, organizationUnitId, employeeCategoryId, employmentTypeId, requirePreApproval, requireAttendanceMatch, isDefault, isActive, search);
            var totalData = await q.CountAsync();
            var entities = await ApplySorting(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actors = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actors)).ToList();
            return Ok(ApiResponse<OvertimePolicyPagedResult>.Ok(new OvertimePolicyPagedResult
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Data overtime policy berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Overtime Policy", Description = "Melihat pilihan overtime policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePolicy", "Read")]
        public async Task<IActionResult> GetOptions(Guid? legalEntityId, Guid? hospitalSiteId, Guid? organizationUnitId, bool onlyActive = true, string? search = null, int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), legalEntityId, hospitalSiteId, organizationUnitId, null, null, null, null, null, onlyActive ? true : null, search);
            var totalData = await q.CountAsync();
            var items = await q.OrderByDescending(x => x.IsDefault).ThenBy(x => x.IsFallback).ThenByDescending(x => x.Priority).ThenBy(x => x.OvertimePolicyName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new OvertimePolicyOptionResponse
                {
                    Id = x.Id, OvertimePolicyCode = x.OvertimePolicyCode, OvertimePolicyName = x.OvertimePolicyName,
                    LegalEntityId = x.LegalEntityId, HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId, IsDefault = x.IsDefault,
                    Priority = x.Priority, IsFallback = x.IsFallback
                }).ToListAsync();
            return Ok(ApiResponse<OvertimePolicyOptionPagedResponse>.Ok(new OvertimePolicyOptionPagedResponse
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Pilihan overtime policy berhasil diambil."));
        }

        [HttpPost("resolve-preview")]
        [AccessAction("Read", "Resolve Overtime Policy", Description = "Melakukan preview resolusi overtime policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePolicy", "Read")]
        public async Task<IActionResult> ResolvePreview(
            [FromBody] OvertimePolicyResolveRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _policyResolverService.ResolveAsync(request, cancellationToken);
            await _loggerService.InfoAsync(
                LogCategory,
                "OvertimePolicy.ResolvePreview",
                "Melakukan preview resolusi overtime policy.",
                result);
            return Ok(ApiResponse<OvertimePolicyResolutionResponse>.Ok(
                result,
                result.Message));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Overtime Policy", Description = "Melihat detail overtime policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePolicy", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Overtime policy tidak ditemukan."));
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var b = MapResponse(entity, actors);
            var result = new OvertimePolicyDetailResponse
            {
                Id = b.Id, LegalEntityId = b.LegalEntityId, LegalEntityName = b.LegalEntityName,
                HospitalSiteId = b.HospitalSiteId, HospitalSiteName = b.HospitalSiteName,
                OrganizationUnitId = b.OrganizationUnitId, OrganizationUnitName = b.OrganizationUnitName,
                EmployeeCategoryId = b.EmployeeCategoryId, EmploymentTypeId = b.EmploymentTypeId,
                OvertimePolicyCode = b.OvertimePolicyCode, OvertimePolicyName = b.OvertimePolicyName,
                Priority = b.Priority, IsFallback = b.IsFallback,
                RequirePreApproval = b.RequirePreApproval, RequirePostVerification = b.RequirePostVerification,
                RequireAttendanceMatch = b.RequireAttendanceMatch, MinimumOvertimeMinutes = b.MinimumOvertimeMinutes,
                MaximumOvertimeMinutesPerDay = b.MaximumOvertimeMinutesPerDay,
                MaximumOvertimeMinutesPerWeek = b.MaximumOvertimeMinutesPerWeek,
                MaximumOvertimeMinutesPerMonth = b.MaximumOvertimeMinutesPerMonth,
                OvertimeThresholdMinutes = b.OvertimeThresholdMinutes, RoundingIntervalMinutes = b.RoundingIntervalMinutes,
                RoundingMethod = b.RoundingMethod, DeductBreakMinutes = b.DeductBreakMinutes,
                BreakDeductionMinutes = b.BreakDeductionMinutes, AllowBeforeShift = b.AllowBeforeShift,
                AllowAfterShift = b.AllowAfterShift, AllowRestDay = b.AllowRestDay, AllowHoliday = b.AllowHoliday,
                AllowDuringLeave = b.AllowDuringLeave, AttendanceToleranceMinutes = b.AttendanceToleranceMinutes,
                ApprovalWorkflowCode = b.ApprovalWorkflowCode, EffectiveStartDate = b.EffectiveStartDate,
                EffectiveEndDate = b.EffectiveEndDate, Description = b.Description, IsDefault = b.IsDefault,
                IsActive = b.IsActive, OvertimeRateCount = b.OvertimeRateCount, CreateDateTime = b.CreateDateTime,
                CreateBy = b.CreateBy, CreateByName = b.CreateByName, UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            return Ok(ApiResponse<OvertimePolicyDetailResponse>.Ok(result, "Detail overtime policy berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Overtime Policy", Description = "Membuat overtime policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OvertimePolicy", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateOvertimePolicyRequest request)
        {
            var validation = await ValidateRequestAsync(null, request, true);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault) await UnsetDefaultAsync(null, request, now, actor);
            var entity = new MstOvertimePolicy
            {
                Id = Guid.NewGuid(), LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId), OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId), EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                OvertimePolicyCode = await GenerateCodeAsync(), OvertimePolicyName = request.OvertimePolicyName.Trim(),
                Priority = request.Priority, IsFallback = request.IsFallback,
                RequirePreApproval = request.RequirePreApproval, RequirePostVerification = request.RequirePostVerification,
                RequireAttendanceMatch = request.RequireAttendanceMatch, MinimumOvertimeMinutes = request.MinimumOvertimeMinutes,
                MaximumOvertimeMinutesPerDay = request.MaximumOvertimeMinutesPerDay,
                MaximumOvertimeMinutesPerWeek = request.MaximumOvertimeMinutesPerWeek,
                MaximumOvertimeMinutesPerMonth = request.MaximumOvertimeMinutesPerMonth,
                OvertimeThresholdMinutes = request.OvertimeThresholdMinutes, RoundingIntervalMinutes = request.RoundingIntervalMinutes,
                RoundingMethod = NormalizeRoundingMethod(request.RoundingMethod), DeductBreakMinutes = request.DeductBreakMinutes,
                BreakDeductionMinutes = request.DeductBreakMinutes ? request.BreakDeductionMinutes : 0, AllowBeforeShift = request.AllowBeforeShift,
                AllowAfterShift = request.AllowAfterShift, AllowRestDay = request.AllowRestDay, AllowHoliday = request.AllowHoliday,
                AllowDuringLeave = request.AllowDuringLeave, AttendanceToleranceMinutes = request.AttendanceToleranceMinutes,
                ApprovalWorkflowCode = NormalizeText(request.ApprovalWorkflowCode), EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date, Description = NormalizeText(request.Description),
                IsDefault = request.IsDefault, IsActive = true, CreateDateTime = now, CreateBy = actor,
                IsDelete = false, IsCancel = false
            };
            _dbContext.Set<MstOvertimePolicy>().Add(entity); await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy });
            var response = new OvertimePolicyCreateResponse
            {
                Id = entity.Id, OvertimePolicyCode = entity.OvertimePolicyCode,
                OvertimePolicyName = entity.OvertimePolicyName, Priority = entity.Priority, IsFallback = entity.IsFallback,
                IsDefault = entity.IsDefault, IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime, CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actors, entity.CreateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "OvertimePolicy.Create", "Membuat data overtime policy.", response);
            return Ok(ApiResponse<OvertimePolicyCreateResponse>.Ok(response, "Overtime policy berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Overtime Policy", Description = "Mengubah overtime policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimePolicy", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOvertimePolicyRequest request)
        {
            var entity = await _dbContext.Set<MstOvertimePolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Overtime policy tidak ditemukan."));
            var validation = await ValidateRequestAsync(id, request, request.IsActive);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault && request.IsActive) await UnsetDefaultAsync(id, request, now, actor);
            ApplyRequest(entity, request); entity.IsDefault = request.IsDefault && request.IsActive; entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now; entity.UpdateBy = actor; await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            var response = new OvertimePolicyUpdateResponse
            {
                Id = entity.Id, OvertimePolicyCode = entity.OvertimePolicyCode,
                OvertimePolicyName = entity.OvertimePolicyName, Priority = entity.Priority, IsFallback = entity.IsFallback,
                IsDefault = entity.IsDefault, IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime, UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "OvertimePolicy.Update", "Mengubah data overtime policy.", response);
            return Ok(ApiResponse<OvertimePolicyUpdateResponse>.Ok(response, "Overtime policy berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Overtime Policy Status", Description = "Mengubah status overtime policy", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimePolicy", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOvertimePolicyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstOvertimePolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Overtime policy tidak ditemukan."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsActive)
            {
                var workflowValidation = await ValidateWorkflowAsync(
                    entity.RequirePreApproval,
                    entity.ApprovalWorkflowCode);

                if (!workflowValidation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(400, workflowValidation.ErrorMessage!));
                }

                var overlap = await _policyResolverService.CheckAmbiguousOverlapAsync(
                    id,
                    BuildDefinitionInput(entity, true));

                if (overlap.HasAmbiguousOverlap)
                {
                    return BadRequest(ApiResponse<object>.Fail(400,
                        $"Overtime policy tidak dapat diaktifkan karena overlap ambigu dengan {overlap.ConflictingPolicyCode} - {overlap.ConflictingPolicyName}."));
                }
            }

            if (request.IsDefault == true && request.IsActive)
            {
                var scope = new CreateOvertimePolicyRequest
                {
                    LegalEntityId = entity.LegalEntityId,
                    HospitalSiteId = entity.HospitalSiteId,
                    OrganizationUnitId = entity.OrganizationUnitId,
                    EmployeeCategoryId = entity.EmployeeCategoryId,
                    EmploymentTypeId = entity.EmploymentTypeId
                };
                await UnsetDefaultAsync(id, scope, now, actor);
            }
            entity.IsActive = request.IsActive;
            if (request.IsDefault.HasValue) entity.IsDefault = request.IsDefault.Value && request.IsActive;
            else if (!request.IsActive) entity.IsDefault = false;
            entity.UpdateDateTime = now; entity.UpdateBy = actor; await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status overtime policy berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Overtime Policy", Description = "Menghapus overtime policy", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("OvertimePolicy", "Delete")]
        public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteOvertimePolicyRequest? request = null)
        {
            var entity = await _dbContext.Set<MstOvertimePolicy>().Include(x => x.OvertimeRates).FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Overtime policy tidak ditemukan."));
            if (entity.OvertimeRates.Any(x => !x.IsDelete)) return BadRequest(ApiResponse<object>.Fail(400, "Overtime policy tidak dapat dihapus karena masih digunakan oleh overtime rate."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            entity.IsDelete = true; entity.IsActive = false; entity.IsDefault = false; entity.DeleteDateTime = now;
            entity.DeleteBy = actor; entity.UpdateDateTime = now; entity.UpdateBy = actor;
            if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim();
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.DeleteBy });
            var response = new OvertimePolicyDeleteResponse
            {
                Id = entity.Id, OvertimePolicyCode = entity.OvertimePolicyCode,
                OvertimePolicyName = entity.OvertimePolicyName, DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actors, entity.DeleteBy)
            };
            await _loggerService.InfoAsync(LogCategory, "OvertimePolicy.Delete", "Menghapus data overtime policy.", response);
            return Ok(ApiResponse<OvertimePolicyDeleteResponse>.Ok(response, "Overtime policy berhasil dihapus."));
        }

        private IQueryable<MstOvertimePolicy> BaseQuery() => _dbContext.Set<MstOvertimePolicy>().AsNoTracking()
            .Include(x => x.LegalEntity).Include(x => x.HospitalSite).Include(x => x.OrganizationUnit)
            .Include(x => x.OvertimeRates).Where(x => !x.IsDelete);

        private static IQueryable<MstOvertimePolicy> ApplyFilter(IQueryable<MstOvertimePolicy> q, Guid? legalEntityId, Guid? hospitalSiteId, Guid? organizationUnitId, Guid? employeeCategoryId, Guid? employmentTypeId, bool? preApproval, bool? attendanceMatch, bool? isDefault, bool? active, string? search)
        {
            if (legalEntityId.HasValue && legalEntityId != Guid.Empty) q = q.Where(x => x.LegalEntityId == legalEntityId.Value);
            if (hospitalSiteId.HasValue && hospitalSiteId != Guid.Empty) q = q.Where(x => x.HospitalSiteId == hospitalSiteId.Value);
            if (organizationUnitId.HasValue && organizationUnitId != Guid.Empty) q = q.Where(x => x.OrganizationUnitId == organizationUnitId.Value);
            if (employeeCategoryId.HasValue && employeeCategoryId != Guid.Empty) q = q.Where(x => x.EmployeeCategoryId == employeeCategoryId.Value);
            if (employmentTypeId.HasValue && employmentTypeId != Guid.Empty) q = q.Where(x => x.EmploymentTypeId == employmentTypeId.Value);
            if (preApproval.HasValue) q = q.Where(x => x.RequirePreApproval == preApproval.Value);
            if (attendanceMatch.HasValue) q = q.Where(x => x.RequireAttendanceMatch == attendanceMatch.Value);
            if (isDefault.HasValue) q = q.Where(x => x.IsDefault == isDefault.Value);
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var k = search.Trim().ToLower();
                q = q.Where(x => x.OvertimePolicyCode.ToLower().Contains(k) || x.OvertimePolicyName.ToLower().Contains(k) ||
                    x.RoundingMethod.ToLower().Contains(k) || (x.Description != null && x.Description.ToLower().Contains(k)));
            }
            return q;
        }

        private static IOrderedQueryable<MstOvertimePolicy> ApplySorting(IQueryable<MstOvertimePolicy> q, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "overtimePolicyName").Trim().ToLowerInvariant() switch
            {
                "overtimepolicycode" => desc ? q.OrderByDescending(x => x.OvertimePolicyCode) : q.OrderBy(x => x.OvertimePolicyCode),
                "priority" => desc ? q.OrderByDescending(x => x.Priority).ThenBy(x => x.OvertimePolicyName) : q.OrderBy(x => x.Priority).ThenBy(x => x.OvertimePolicyName),
                "minimumovertimeminutes" => desc ? q.OrderByDescending(x => x.MinimumOvertimeMinutes) : q.OrderBy(x => x.MinimumOvertimeMinutes),
                "createdatetime" => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                "isdefault" => desc ? q.OrderByDescending(x => x.IsDefault).ThenBy(x => x.IsFallback).ThenByDescending(x => x.Priority).ThenBy(x => x.OvertimePolicyName) : q.OrderBy(x => x.IsDefault).ThenBy(x => x.OvertimePolicyName),
                "isactive" => desc ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.OvertimePolicyName) : q.OrderBy(x => x.IsActive).ThenBy(x => x.OvertimePolicyName),
                _ => desc ? q.OrderByDescending(x => x.OvertimePolicyName) : q.OrderBy(x => x.OvertimePolicyName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateOvertimePolicyRequest request, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(request.OvertimePolicyName)) return (false, "Nama overtime policy wajib diisi.");
            if (!AllowedRoundingMethods.Contains(request.RoundingMethod.Trim())) return (false, "Rounding method tidak valid.");
            if (!await ExistsActiveIfProvidedAsync<MstLegalEntity>(request.LegalEntityId)) return (false, "Legal entity tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstHospitalSite>(request.HospitalSiteId)) return (false, "Hospital site tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstOrganizationUnit>(request.OrganizationUnitId)) return (false, "Organization unit tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstEmployeeCategory>(request.EmployeeCategoryId)) return (false, "Employee category tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstEmploymentType>(request.EmploymentTypeId)) return (false, "Employment type tidak ditemukan atau tidak aktif.");
            if (request.MaximumOvertimeMinutesPerDay.HasValue && request.MaximumOvertimeMinutesPerDay.Value < request.MinimumOvertimeMinutes) return (false, "Maximum overtime minutes per day tidak boleh lebih kecil dari minimum overtime minutes.");
            if (request.MaximumOvertimeMinutesPerWeek.HasValue && request.MaximumOvertimeMinutesPerDay.HasValue && request.MaximumOvertimeMinutesPerWeek.Value < request.MaximumOvertimeMinutesPerDay.Value) return (false, "Maximum overtime minutes per week tidak boleh lebih kecil dari batas harian.");
            if (request.MaximumOvertimeMinutesPerMonth.HasValue && request.MaximumOvertimeMinutesPerWeek.HasValue && request.MaximumOvertimeMinutesPerMonth.Value < request.MaximumOvertimeMinutesPerWeek.Value) return (false, "Maximum overtime minutes per month tidak boleh lebih kecil dari batas mingguan.");
            if (request.DeductBreakMinutes && request.BreakDeductionMinutes <= 0) return (false, "Break deduction minutes wajib lebih besar dari nol ketika potongan istirahat diaktifkan.");
            if (!request.AllowBeforeShift && !request.AllowAfterShift && !request.AllowRestDay && !request.AllowHoliday) return (false, "Policy harus mengizinkan minimal satu jenis waktu lembur.");
            if (isActive)
            {
                var workflowValidation = await ValidateWorkflowAsync(
                    request.RequirePreApproval,
                    request.ApprovalWorkflowCode);

                if (!workflowValidation.IsValid)
                    return (false, workflowValidation.ErrorMessage);
            }
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date) return (false, "Tanggal selesai efektif tidak boleh sebelum tanggal mulai efektif.");
            var name = request.OvertimePolicyName.Trim().ToLower();
            var duplicate = _dbContext.Set<MstOvertimePolicy>().AsNoTracking().Where(x => !x.IsDelete && x.OvertimePolicyName.ToLower() == name);
            if (excludeId.HasValue) duplicate = duplicate.Where(x => x.Id != excludeId.Value);
            if (await duplicate.AnyAsync()) return (false, "Nama overtime policy sudah digunakan.");

            var overlap = await _policyResolverService.CheckAmbiguousOverlapAsync(
                excludeId,
                BuildDefinitionInput(request, isActive));

            if (overlap.HasAmbiguousOverlap)
            {
                return (false,
                    $"Overtime policy overlap ambigu dengan {overlap.ConflictingPolicyCode} - {overlap.ConflictingPolicyName}. Gunakan priority atau periode efektif yang berbeda.");
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateWorkflowAsync(
            bool requirePreApproval,
            string? approvalWorkflowCode)
        {
            if (requirePreApproval && string.IsNullOrWhiteSpace(approvalWorkflowCode))
            {
                return (false, "Approval workflow code wajib diisi ketika pre-approval diaktifkan.");
            }

            if (string.IsNullOrWhiteSpace(approvalWorkflowCode))
            {
                return (true, null);
            }

            var workflowCode = approvalWorkflowCode.Trim().ToLower();
            var requestType = OvertimeValueConstants.Workflow.RequestType.ToLower();
            var activeStatus = OvertimeValueConstants.Workflow.ActiveStatus.ToLower();

            var workflowValid = await _dbContext.MstWorkflowDefinitions
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.WorkflowCode.ToLower() == workflowCode &&
                    x.RequestType.ToLower() == requestType &&
                    x.WorkflowStatus.ToLower() == activeStatus);

            return workflowValid
                ? (true, null)
                : (false, "Approval workflow code tidak ditemukan, tidak aktif, atau bukan workflow OvertimeRequest.");
        }

        private void ApplyRequest(MstOvertimePolicy entity, CreateOvertimePolicyRequest request)
        {
            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.OvertimePolicyName = request.OvertimePolicyName.Trim();
            entity.Priority = request.Priority;
            entity.IsFallback = request.IsFallback;
            entity.RequirePreApproval = request.RequirePreApproval;
            entity.RequirePostVerification = request.RequirePostVerification;
            entity.RequireAttendanceMatch = request.RequireAttendanceMatch;
            entity.MinimumOvertimeMinutes = request.MinimumOvertimeMinutes;
            entity.MaximumOvertimeMinutesPerDay = request.MaximumOvertimeMinutesPerDay;
            entity.MaximumOvertimeMinutesPerWeek = request.MaximumOvertimeMinutesPerWeek;
            entity.MaximumOvertimeMinutesPerMonth = request.MaximumOvertimeMinutesPerMonth;
            entity.OvertimeThresholdMinutes = request.OvertimeThresholdMinutes;
            entity.RoundingIntervalMinutes = request.RoundingIntervalMinutes;
            entity.RoundingMethod = NormalizeRoundingMethod(request.RoundingMethod);
            entity.DeductBreakMinutes = request.DeductBreakMinutes;
            entity.BreakDeductionMinutes = request.DeductBreakMinutes ? request.BreakDeductionMinutes : 0;
            entity.AllowBeforeShift = request.AllowBeforeShift;
            entity.AllowAfterShift = request.AllowAfterShift;
            entity.AllowRestDay = request.AllowRestDay;
            entity.AllowHoliday = request.AllowHoliday;
            entity.AllowDuringLeave = request.AllowDuringLeave;
            entity.AttendanceToleranceMinutes = request.AttendanceToleranceMinutes;
            entity.ApprovalWorkflowCode = NormalizeText(request.ApprovalWorkflowCode);
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeText(request.Description);
        }

        private async Task UnsetDefaultAsync(Guid? excludeId, CreateOvertimePolicyRequest scope, DateTime now, Guid actor)
        {
            var legalEntityId = NormalizeGuid(scope.LegalEntityId);
            var hospitalSiteId = NormalizeGuid(scope.HospitalSiteId);
            var organizationUnitId = NormalizeGuid(scope.OrganizationUnitId);
            var employeeCategoryId = NormalizeGuid(scope.EmployeeCategoryId);
            var employmentTypeId = NormalizeGuid(scope.EmploymentTypeId);
            var q = _dbContext.Set<MstOvertimePolicy>().Where(x => !x.IsDelete && x.IsActive && x.IsDefault &&
                x.LegalEntityId == legalEntityId && x.HospitalSiteId == hospitalSiteId &&
                x.OrganizationUnitId == organizationUnitId && x.EmployeeCategoryId == employeeCategoryId &&
                x.EmploymentTypeId == employmentTypeId);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
            foreach (var row in await q.ToListAsync())
            {
                row.IsDefault = false;
                row.UpdateDateTime = now;
                row.UpdateBy = actor;
            }
        }

        private OvertimePolicyResponse MapResponse(MstOvertimePolicy x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id,
            LegalEntityId = x.LegalEntityId,
            LegalEntityName = x.LegalEntity?.LegalEntityName,
            HospitalSiteId = x.HospitalSiteId,
            HospitalSiteName = x.HospitalSite?.SiteName,
            OrganizationUnitId = x.OrganizationUnitId,
            OrganizationUnitName = x.OrganizationUnit?.UnitName,
            EmployeeCategoryId = x.EmployeeCategoryId,
            EmploymentTypeId = x.EmploymentTypeId,
            OvertimePolicyCode = x.OvertimePolicyCode,
            OvertimePolicyName = x.OvertimePolicyName,
            Priority = x.Priority,
            IsFallback = x.IsFallback,
            RequirePreApproval = x.RequirePreApproval,
            RequirePostVerification = x.RequirePostVerification,
            RequireAttendanceMatch = x.RequireAttendanceMatch,
            MinimumOvertimeMinutes = x.MinimumOvertimeMinutes,
            MaximumOvertimeMinutesPerDay = x.MaximumOvertimeMinutesPerDay,
            MaximumOvertimeMinutesPerWeek = x.MaximumOvertimeMinutesPerWeek,
            MaximumOvertimeMinutesPerMonth = x.MaximumOvertimeMinutesPerMonth,
            OvertimeThresholdMinutes = x.OvertimeThresholdMinutes,
            RoundingIntervalMinutes = x.RoundingIntervalMinutes,
            RoundingMethod = x.RoundingMethod,
            DeductBreakMinutes = x.DeductBreakMinutes,
            BreakDeductionMinutes = x.BreakDeductionMinutes,
            AllowBeforeShift = x.AllowBeforeShift,
            AllowAfterShift = x.AllowAfterShift,
            AllowRestDay = x.AllowRestDay,
            AllowHoliday = x.AllowHoliday,
            AllowDuringLeave = x.AllowDuringLeave,
            AttendanceToleranceMinutes = x.AttendanceToleranceMinutes,
            ApprovalWorkflowCode = x.ApprovalWorkflowCode,
            EffectiveStartDate = x.EffectiveStartDate,
            EffectiveEndDate = x.EffectiveEndDate,
            Description = x.Description,
            IsDefault = x.IsDefault,
            IsActive = x.IsActive,
            OvertimeRateCount = x.OvertimeRates.Count(y => !y.IsDelete),
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actors, x.CreateBy)
        };

        private static OvertimePolicyDefinitionInput BuildDefinitionInput(
            CreateOvertimePolicyRequest request,
            bool isActive) => new()
        {
            LegalEntityId = NormalizeGuid(request.LegalEntityId),
            HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
            OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
            EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
            EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
            Priority = request.Priority,
            IsFallback = request.IsFallback,
            IsActive = isActive,
            EffectiveStartDate = request.EffectiveStartDate?.Date,
            EffectiveEndDate = request.EffectiveEndDate?.Date
        };

        private static OvertimePolicyDefinitionInput BuildDefinitionInput(
            MstOvertimePolicy entity,
            bool isActive) => new()
        {
            LegalEntityId = entity.LegalEntityId,
            HospitalSiteId = entity.HospitalSiteId,
            OrganizationUnitId = entity.OrganizationUnitId,
            EmployeeCategoryId = entity.EmployeeCategoryId,
            EmploymentTypeId = entity.EmploymentTypeId,
            Priority = entity.Priority,
            IsFallback = entity.IsFallback,
            IsActive = isActive,
            EffectiveStartDate = entity.EffectiveStartDate,
            EffectiveEndDate = entity.EffectiveEndDate
        };

        private async Task<bool> ExistsActiveAsync<T>(Guid id) where T : IdentityModel =>
            await _dbContext.Set<T>().AsNoTracking().AnyAsync(x => EF.Property<Guid>(x, "Id") == id && EF.Property<bool>(x, "IsActive") && !EF.Property<bool>(x, "IsDelete"));

        private async Task<bool> ExistsActiveIfProvidedAsync<T>(Guid? id) where T : IdentityModel =>
            !id.HasValue || id == Guid.Empty || await ExistsActiveAsync<T>(id.Value);

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstOvertimePolicy>().AsNoTracking()
                .Where(x => !x.IsDelete && x.OvertimePolicyCode.StartsWith(CodePrefix))
                .Select(x => x.OvertimePolicyCode).ToListAsync();
            var used = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> ids)
        {
            var values = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Users.AsNoTracking().Where(x => values.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> map, Guid id) =>
            id == Guid.Empty ? null : map.TryGetValue(id, out var value) ? value : null;

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static void NormalizePaging(ref int pageNumber, ref int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
        }

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value == Guid.Empty ? null : value;
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string NormalizeRoundingMethod(string value) => AllowedRoundingMethods.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
