using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

using LeavePolicyPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs.LeavePolicyResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/leave-policies")]
    [AccessController(moduleCode: "HUMAN_RESOURCE_MASTER_DATA", moduleName: "Human Resource Master Data", displayName: "Leave Policy", AreaName = "Corporate", ControllerName = "LeavePolicy", Description = "Corporate human resource master data leave policy", SortOrder = 31)]
    [Tags("Corporate / Human Resource / Master Data / Leave and Overtime / Leave Policy")]
    public class LeavePolicyController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "LVP-RSMMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public LeavePolicyController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Leave Policy", Description = "Melihat metadata filter leave policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePolicy", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new LeavePolicyFilterMetadataResponse
            {
                DefaultFilter = new LeavePolicyDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<LeavePolicySortOptionResponse>
                {
                    new() { Value = "priority", Label = "Prioritas" },
                    new() { Value = "leavePolicyCode", Label = "Kode kebijakan cuti" },
                    new() { Value = "leavePolicyName", Label = "Nama kebijakan cuti" },
                    new() { Value = "leaveTypeName", Label = "Jenis cuti" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isDefault", Label = "Kebijakan default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            await _loggerService.InfoAsync(LogCategory, "LeavePolicy.GetFilterMetadata", "Mengambil metadata filter leave policy.", result);
            return Ok(ApiResponse<LeavePolicyFilterMetadataResponse>.Ok(result, "Metadata filter leave policy berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Leave Policy", Description = "Melihat ringkasan leave policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePolicy", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var q = BaseQuery();
            var result = new LeavePolicySummaryResponse
            {
                TotalData = await q.CountAsync(), ActiveData = await q.CountAsync(x => x.IsActive),
                InactiveData = await q.CountAsync(x => !x.IsActive), DefaultData = await q.CountAsync(x => x.IsDefault && x.IsActive),
                FallbackData = await q.CountAsync(x => x.IsFallback && x.IsActive)
            };
            return Ok(ApiResponse<LeavePolicySummaryResponse>.Ok(result, "Ringkasan leave policy berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Leave Policy", Description = "Melihat data leave policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePolicy", "Read")]
        public async Task<IActionResult> GetData(DateTime? startDate, DateTime? endDate, string? customPeriod, Guid? leaveTypeId, Guid? legalEntityId, Guid? hospitalSiteId, Guid? organizationUnitId, Guid? departmentId, Guid? positionId, bool? isFallback, bool? isDefault, bool? isActive, string? search, string? sortBy = "priority", string? sortDirection = "desc", int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), leaveTypeId, legalEntityId, hospitalSiteId, organizationUnitId, departmentId, positionId, isFallback, isDefault, isActive, search);
            q = WorkflowMasterDataSupport.ApplyDateFilter(q, startDate, endDate, customPeriod);
            var totalData = await q.CountAsync();
            var entities = await ApplySorting(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actors = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actors)).ToList();
            return Ok(ApiResponse<LeavePolicyPagedResult>.Ok(new LeavePolicyPagedResult
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Data leave policy berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Leave Policy", Description = "Melihat pilihan leave policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePolicy", "Read")]
        public async Task<IActionResult> GetOptions(Guid? leaveTypeId, Guid? legalEntityId, Guid? hospitalSiteId, bool onlyActive = true, string? search = null, int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), leaveTypeId, legalEntityId, hospitalSiteId, null, null, null, null, null, onlyActive ? true : null, search);
            var totalData = await q.CountAsync();
            var items = await q.OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.IsFallback).ThenByDescending(x => x.Priority).ThenBy(x => x.LeavePolicyName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new LeavePolicyOptionResponse
                {
                    Id = x.Id, LeaveTypeId = x.LeaveTypeId, LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : string.Empty,
                    LeavePolicyCode = x.LeavePolicyCode, LeavePolicyName = x.LeavePolicyName, Priority = x.Priority,
                    IsFallback = x.IsFallback, IsDefault = x.IsDefault
                }).ToListAsync();
            return Ok(ApiResponse<LeavePolicyOptionPagedResponse>.Ok(new LeavePolicyOptionPagedResponse
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Pilihan leave policy berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Leave Policy", Description = "Melihat detail leave policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePolicy", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave policy tidak ditemukan."));
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var b = MapResponse(entity, actors);
            var result = new LeavePolicyDetailResponse
            {
                Id = b.Id, LeaveTypeId = b.LeaveTypeId, LeaveTypeCode = b.LeaveTypeCode, LeaveTypeName = b.LeaveTypeName,
                LegalEntityId = b.LegalEntityId, LegalEntityName = b.LegalEntityName, HospitalSiteId = b.HospitalSiteId,
                HospitalSiteName = b.HospitalSiteName, OrganizationUnitId = b.OrganizationUnitId, OrganizationUnitName = b.OrganizationUnitName,
                DepartmentId = b.DepartmentId, DepartmentName = b.DepartmentName, PositionId = b.PositionId, PositionName = b.PositionName,
                WorkLocationId = b.WorkLocationId, WorkLocationName = b.WorkLocationName, WorkforceTypeId = b.WorkforceTypeId,
                EmployeeCategoryId = b.EmployeeCategoryId, EmploymentTypeId = b.EmploymentTypeId, EmploymentStatusId = b.EmploymentStatusId,
                ContractTypeId = b.ContractTypeId, LeavePolicyCode = b.LeavePolicyCode, LeavePolicyName = b.LeavePolicyName,
                Priority = b.Priority, IsFallback = b.IsFallback, MinimumServiceMonths = b.MinimumServiceMonths,
                MinimumNoticeDays = b.MinimumNoticeDays, MaximumRequestDays = b.MaximumRequestDays,
                MinimumRequestMinutes = b.MinimumRequestMinutes, AllowDuringProbation = b.AllowDuringProbation,
                AllowNegativeBalance = b.AllowNegativeBalance, NegativeBalanceLimitDays = b.NegativeBalanceLimitDays,
                AllowBackdatedRequest = b.AllowBackdatedRequest, BackdatedLimitDays = b.BackdatedLimitDays,
                AllowFutureDatedRequest = b.AllowFutureDatedRequest, MaximumAdvanceRequestDays = b.MaximumAdvanceRequestDays,
                DayCalculationMethod = b.DayCalculationMethod, ExcludeHoliday = b.ExcludeHoliday, ExcludeWeeklyOff = b.ExcludeWeeklyOff,
                ReservationTiming = b.ReservationTiming, DeductionTiming = b.DeductionTiming, RequireAttachment = b.RequireAttachment,
                AttachmentRequiredAfterDays = b.AttachmentRequiredAfterDays, RequireReplacementEmployee = b.RequireReplacementEmployee,
                RequireManagerApproval = b.RequireManagerApproval, RequireHrVerification = b.RequireHrVerification,
                ApprovalWorkflowCode = b.ApprovalWorkflowCode, EffectiveStartDate = b.EffectiveStartDate,
                EffectiveEndDate = b.EffectiveEndDate, Description = b.Description, IsDefault = b.IsDefault, IsActive = b.IsActive,
                EntitlementPolicyCount = b.EntitlementPolicyCount, CreateDateTime = b.CreateDateTime, CreateBy = b.CreateBy,
                CreateByName = b.CreateByName, UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy, UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            return Ok(ApiResponse<LeavePolicyDetailResponse>.Ok(result, "Detail leave policy berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Leave Policy", Description = "Membuat leave policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeavePolicy", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeavePolicyRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault) await UnsetDefaultAsync(null, request.LeaveTypeId, now, actor);
            if (request.IsFallback) await UnsetFallbackAsync(null, request.LeaveTypeId, now, actor);
            var entity = new MstLeavePolicy
            {
                Id = Guid.NewGuid(), LeaveTypeId = request.LeaveTypeId, LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId), OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                DepartmentId = NormalizeGuid(request.DepartmentId), PositionId = NormalizeGuid(request.PositionId),
                WorkLocationId = NormalizeGuid(request.WorkLocationId), WorkforceTypeId = NormalizeGuid(request.WorkforceTypeId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId), EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                EmploymentStatusId = NormalizeGuid(request.EmploymentStatusId), ContractTypeId = NormalizeGuid(request.ContractTypeId),
                LeavePolicyCode = await GenerateCodeAsync(), LeavePolicyName = request.LeavePolicyName.Trim(), Priority = request.Priority,
                IsFallback = request.IsFallback, MinimumServiceMonths = request.MinimumServiceMonths, MinimumNoticeDays = request.MinimumNoticeDays,
                MaximumRequestDays = request.MaximumRequestDays, MinimumRequestMinutes = request.MinimumRequestMinutes,
                AllowDuringProbation = request.AllowDuringProbation, AllowNegativeBalance = request.AllowNegativeBalance,
                NegativeBalanceLimitDays = request.NegativeBalanceLimitDays, AllowBackdatedRequest = request.AllowBackdatedRequest,
                BackdatedLimitDays = request.BackdatedLimitDays, AllowFutureDatedRequest = request.AllowFutureDatedRequest,
                MaximumAdvanceRequestDays = request.MaximumAdvanceRequestDays, DayCalculationMethod = request.DayCalculationMethod.Trim(),
                ExcludeHoliday = request.ExcludeHoliday, ExcludeWeeklyOff = request.ExcludeWeeklyOff,
                ReservationTiming = request.ReservationTiming.Trim(), DeductionTiming = request.DeductionTiming.Trim(),
                RequireAttachment = request.RequireAttachment, AttachmentRequiredAfterDays = request.AttachmentRequiredAfterDays,
                RequireReplacementEmployee = request.RequireReplacementEmployee, RequireManagerApproval = request.RequireManagerApproval,
                RequireHrVerification = request.RequireHrVerification, ApprovalWorkflowCode = NormalizeText(request.ApprovalWorkflowCode),
                EffectiveStartDate = request.EffectiveStartDate?.Date, EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeText(request.Description), IsDefault = request.IsDefault, IsActive = true,
                CreateDateTime = now, CreateBy = actor, IsDelete = false, IsCancel = false
            };
            _dbContext.Set<MstLeavePolicy>().Add(entity); await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy });
            var response = new LeavePolicyCreateResponse
            {
                Id = entity.Id, LeaveTypeId = entity.LeaveTypeId, LeavePolicyCode = entity.LeavePolicyCode,
                LeavePolicyName = entity.LeavePolicyName, IsDefault = entity.IsDefault, IsFallback = entity.IsFallback,
                IsActive = entity.IsActive, CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy, CreateByName = GetActorName(actors, entity.CreateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeavePolicy.Create", "Membuat data leave policy.", response);
            return Ok(ApiResponse<LeavePolicyCreateResponse>.Ok(response, "Leave policy berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Leave Policy", Description = "Mengubah leave policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeavePolicy", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeavePolicyRequest request)
        {
            var entity = await _dbContext.Set<MstLeavePolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave policy tidak ditemukan."));
            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault && request.IsActive) await UnsetDefaultAsync(id, request.LeaveTypeId, now, actor);
            if (request.IsFallback && request.IsActive) await UnsetFallbackAsync(id, request.LeaveTypeId, now, actor);
            ApplyRequest(entity, request); entity.IsDefault = request.IsDefault && request.IsActive;
            entity.IsFallback = request.IsFallback && request.IsActive; entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now; entity.UpdateBy = actor; await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            var response = new LeavePolicyUpdateResponse
            {
                Id = entity.Id, LeaveTypeId = entity.LeaveTypeId, LeavePolicyCode = entity.LeavePolicyCode,
                LeavePolicyName = entity.LeavePolicyName, IsDefault = entity.IsDefault, IsFallback = entity.IsFallback,
                IsActive = entity.IsActive, UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy, UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeavePolicy.Update", "Mengubah data leave policy.", response);
            return Ok(ApiResponse<LeavePolicyUpdateResponse>.Ok(response, "Leave policy berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Leave Policy Status", Description = "Mengubah status leave policy", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeavePolicy", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLeavePolicyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstLeavePolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave policy tidak ditemukan."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault == true && request.IsActive) await UnsetDefaultAsync(id, entity.LeaveTypeId, now, actor);
            if (request.IsFallback == true && request.IsActive) await UnsetFallbackAsync(id, entity.LeaveTypeId, now, actor);
            entity.IsActive = request.IsActive;
            if (request.IsDefault.HasValue) entity.IsDefault = request.IsDefault.Value && request.IsActive; else if (!request.IsActive) entity.IsDefault = false;
            if (request.IsFallback.HasValue) entity.IsFallback = request.IsFallback.Value && request.IsActive; else if (!request.IsActive) entity.IsFallback = false;
            entity.UpdateDateTime = now; entity.UpdateBy = actor; await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status leave policy berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Leave Policy", Description = "Menghapus leave policy", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LeavePolicy", "Delete")]
        public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteLeavePolicyRequest? request = null)
        {
            var entity = await _dbContext.Set<MstLeavePolicy>().Include(x => x.EntitlementPolicies).FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave policy tidak ditemukan."));
            if (entity.EntitlementPolicies.Any(x => !x.IsDelete)) return BadRequest(ApiResponse<object>.Fail(400, "Leave policy tidak dapat dihapus karena masih digunakan oleh entitlement policy."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            entity.IsDelete = true; entity.IsActive = false; entity.IsDefault = false; entity.IsFallback = false;
            entity.DeleteDateTime = now; entity.DeleteBy = actor; entity.UpdateDateTime = now; entity.UpdateBy = actor;
            if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim();
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.DeleteBy });
            var response = new LeavePolicyDeleteResponse
            {
                Id = entity.Id, LeavePolicyCode = entity.LeavePolicyCode, LeavePolicyName = entity.LeavePolicyName,
                DeleteDateTime = entity.DeleteDateTime, DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actors, entity.DeleteBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeavePolicy.Delete", "Menghapus data leave policy.", response);
            return Ok(ApiResponse<LeavePolicyDeleteResponse>.Ok(response, "Leave policy berhasil dihapus."));
        }

        private IQueryable<MstLeavePolicy> BaseQuery() => _dbContext.Set<MstLeavePolicy>().AsNoTracking()
            .Include(x => x.LeaveType).Include(x => x.LegalEntity).Include(x => x.HospitalSite).Include(x => x.OrganizationUnit)
            .Include(x => x.Department).Include(x => x.Position).Include(x => x.WorkLocation).Include(x => x.EntitlementPolicies)
            .Where(x => !x.IsDelete);
        private static IQueryable<MstLeavePolicy> ApplyFilter(IQueryable<MstLeavePolicy> q, Guid? leaveTypeId, Guid? legalEntityId, Guid? hospitalSiteId, Guid? organizationUnitId, Guid? departmentId, Guid? positionId, bool? fallback, bool? isDefault, bool? active, string? search)
        {
            if (leaveTypeId.HasValue && leaveTypeId != Guid.Empty) q = q.Where(x => x.LeaveTypeId == leaveTypeId.Value);
            if (legalEntityId.HasValue && legalEntityId != Guid.Empty) q = q.Where(x => x.LegalEntityId == legalEntityId.Value);
            if (hospitalSiteId.HasValue && hospitalSiteId != Guid.Empty) q = q.Where(x => x.HospitalSiteId == hospitalSiteId.Value);
            if (organizationUnitId.HasValue && organizationUnitId != Guid.Empty) q = q.Where(x => x.OrganizationUnitId == organizationUnitId.Value);
            if (departmentId.HasValue && departmentId != Guid.Empty) q = q.Where(x => x.DepartmentId == departmentId.Value);
            if (positionId.HasValue && positionId != Guid.Empty) q = q.Where(x => x.PositionId == positionId.Value);
            if (fallback.HasValue) q = q.Where(x => x.IsFallback == fallback.Value);
            if (isDefault.HasValue) q = q.Where(x => x.IsDefault == isDefault.Value);
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
            if (!string.IsNullOrWhiteSpace(search)) { var k = search.Trim().ToLower(); q = q.Where(x => x.LeavePolicyCode.ToLower().Contains(k) || x.LeavePolicyName.ToLower().Contains(k) || (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(k)) || (x.Description != null && x.Description.ToLower().Contains(k))); }
            return q;
        }
        private static IOrderedQueryable<MstLeavePolicy> ApplySorting(IQueryable<MstLeavePolicy> q, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "priority").Trim().ToLowerInvariant() switch
            {
                "leavepolicycode" => desc ? q.OrderByDescending(x => x.LeavePolicyCode) : q.OrderBy(x => x.LeavePolicyCode),
                "leavepolicyname" => desc ? q.OrderByDescending(x => x.LeavePolicyName) : q.OrderBy(x => x.LeavePolicyName),
                "leavetypename" => desc ? q.OrderByDescending(x => x.LeaveType != null ? x.LeaveType.LeaveTypeName : string.Empty) : q.OrderBy(x => x.LeaveType != null ? x.LeaveType.LeaveTypeName : string.Empty),
                "createdatetime" => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                "isdefault" => desc ? q.OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.Priority) : q.OrderBy(x => x.IsDefault).ThenByDescending(x => x.Priority),
                "isactive" => desc ? q.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Priority) : q.OrderBy(x => x.IsActive).ThenByDescending(x => x.Priority),
                _ => desc ? q.OrderByDescending(x => x.Priority).ThenBy(x => x.LeavePolicyName) : q.OrderBy(x => x.Priority).ThenBy(x => x.LeavePolicyName)
            };
        }
        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateLeavePolicyRequest request)
        {
            if (request.LeaveTypeId == Guid.Empty) return (false, "Leave type wajib dipilih.");
            if (string.IsNullOrWhiteSpace(request.LeavePolicyName)) return (false, "Nama leave policy wajib diisi.");
            if (!await ExistsActiveAsync<MstLeaveType>(request.LeaveTypeId)) return (false, "Leave type tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstLegalEntity>(request.LegalEntityId)) return (false, "Legal entity tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstHospitalSite>(request.HospitalSiteId)) return (false, "Hospital site tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstOrganizationUnit>(request.OrganizationUnitId)) return (false, "Organization unit tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstDepartment>(request.DepartmentId)) return (false, "Department tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstPosition>(request.PositionId)) return (false, "Position tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstWorkLocation>(request.WorkLocationId)) return (false, "Work location tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstWorkforceType>(request.WorkforceTypeId)) return (false, "Workforce type tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstEmployeeCategory>(request.EmployeeCategoryId)) return (false, "Employee category tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstEmploymentType>(request.EmploymentTypeId)) return (false, "Employment type tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstEmploymentStatus>(request.EmploymentStatusId)) return (false, "Employment status tidak ditemukan atau tidak aktif.");
            if (!await ExistsActiveIfProvidedAsync<MstContractType>(request.ContractTypeId)) return (false, "Contract type tidak ditemukan atau tidak aktif.");
            if (request.AllowNegativeBalance && (!request.NegativeBalanceLimitDays.HasValue || request.NegativeBalanceLimitDays <= 0)) return (false, "Negative balance limit wajib lebih besar dari nol ketika saldo negatif diizinkan.");
            if (request.RequireAttachment && request.AttachmentRequiredAfterDays.HasValue && request.AttachmentRequiredAfterDays <= 0) return (false, "Attachment required after days harus lebih besar dari nol.");
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date) return (false, "Tanggal selesai efektif tidak boleh sebelum tanggal mulai efektif.");
            var name = request.LeavePolicyName.Trim().ToLower();
            var duplicate = _dbContext.Set<MstLeavePolicy>().AsNoTracking().Where(x => !x.IsDelete && x.LeaveTypeId == request.LeaveTypeId && x.LeavePolicyName.ToLower() == name);
            if (excludeId.HasValue) duplicate = duplicate.Where(x => x.Id != excludeId.Value);
            if (await duplicate.AnyAsync()) return (false, "Nama leave policy sudah digunakan pada leave type tersebut.");
            return (true, null);
        }
        private void ApplyRequest(MstLeavePolicy entity, CreateLeavePolicyRequest request)
        {
            entity.LeaveTypeId = request.LeaveTypeId; entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId); entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.DepartmentId = NormalizeGuid(request.DepartmentId); entity.PositionId = NormalizeGuid(request.PositionId);
            entity.WorkLocationId = NormalizeGuid(request.WorkLocationId); entity.WorkforceTypeId = NormalizeGuid(request.WorkforceTypeId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId); entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.EmploymentStatusId = NormalizeGuid(request.EmploymentStatusId); entity.ContractTypeId = NormalizeGuid(request.ContractTypeId);
            entity.LeavePolicyName = request.LeavePolicyName.Trim(); entity.Priority = request.Priority;
            entity.MinimumServiceMonths = request.MinimumServiceMonths; entity.MinimumNoticeDays = request.MinimumNoticeDays;
            entity.MaximumRequestDays = request.MaximumRequestDays; entity.MinimumRequestMinutes = request.MinimumRequestMinutes;
            entity.AllowDuringProbation = request.AllowDuringProbation; entity.AllowNegativeBalance = request.AllowNegativeBalance;
            entity.NegativeBalanceLimitDays = request.NegativeBalanceLimitDays; entity.AllowBackdatedRequest = request.AllowBackdatedRequest;
            entity.BackdatedLimitDays = request.BackdatedLimitDays; entity.AllowFutureDatedRequest = request.AllowFutureDatedRequest;
            entity.MaximumAdvanceRequestDays = request.MaximumAdvanceRequestDays; entity.DayCalculationMethod = request.DayCalculationMethod.Trim();
            entity.ExcludeHoliday = request.ExcludeHoliday; entity.ExcludeWeeklyOff = request.ExcludeWeeklyOff;
            entity.ReservationTiming = request.ReservationTiming.Trim(); entity.DeductionTiming = request.DeductionTiming.Trim();
            entity.RequireAttachment = request.RequireAttachment; entity.AttachmentRequiredAfterDays = request.AttachmentRequiredAfterDays;
            entity.RequireReplacementEmployee = request.RequireReplacementEmployee; entity.RequireManagerApproval = request.RequireManagerApproval;
            entity.RequireHrVerification = request.RequireHrVerification; entity.ApprovalWorkflowCode = NormalizeText(request.ApprovalWorkflowCode);
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date; entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeText(request.Description);
        }
        private async Task UnsetDefaultAsync(Guid? excludeId, Guid leaveTypeId, DateTime now, Guid actor)
        {
            var q = _dbContext.Set<MstLeavePolicy>().Where(x => !x.IsDelete && x.IsActive && x.IsDefault && x.LeaveTypeId == leaveTypeId);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
            foreach (var row in await q.ToListAsync()) { row.IsDefault = false; row.UpdateDateTime = now; row.UpdateBy = actor; }
        }
        private async Task UnsetFallbackAsync(Guid? excludeId, Guid leaveTypeId, DateTime now, Guid actor)
        {
            var q = _dbContext.Set<MstLeavePolicy>().Where(x => !x.IsDelete && x.IsActive && x.IsFallback && x.LeaveTypeId == leaveTypeId);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
            foreach (var row in await q.ToListAsync()) { row.IsFallback = false; row.UpdateDateTime = now; row.UpdateBy = actor; }
        }
        private LeavePolicyResponse MapResponse(MstLeavePolicy x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id, LeaveTypeId = x.LeaveTypeId, LeaveTypeCode = x.LeaveType?.LeaveTypeCode ?? string.Empty,
            LeaveTypeName = x.LeaveType?.LeaveTypeName ?? string.Empty, LegalEntityId = x.LegalEntityId,
            LegalEntityName = x.LegalEntity?.LegalEntityName, HospitalSiteId = x.HospitalSiteId, HospitalSiteName = x.HospitalSite?.SiteName,
            OrganizationUnitId = x.OrganizationUnitId, OrganizationUnitName = x.OrganizationUnit?.UnitName,
            DepartmentId = x.DepartmentId, DepartmentName = x.Department?.DepartmentName, PositionId = x.PositionId,
            PositionName = x.Position?.PositionName, WorkLocationId = x.WorkLocationId, WorkLocationName = x.WorkLocation?.LocationName,
            WorkforceTypeId = x.WorkforceTypeId, EmployeeCategoryId = x.EmployeeCategoryId, EmploymentTypeId = x.EmploymentTypeId,
            EmploymentStatusId = x.EmploymentStatusId, ContractTypeId = x.ContractTypeId, LeavePolicyCode = x.LeavePolicyCode,
            LeavePolicyName = x.LeavePolicyName, Priority = x.Priority, IsFallback = x.IsFallback,
            MinimumServiceMonths = x.MinimumServiceMonths, MinimumNoticeDays = x.MinimumNoticeDays,
            MaximumRequestDays = x.MaximumRequestDays, MinimumRequestMinutes = x.MinimumRequestMinutes,
            AllowDuringProbation = x.AllowDuringProbation, AllowNegativeBalance = x.AllowNegativeBalance,
            NegativeBalanceLimitDays = x.NegativeBalanceLimitDays, AllowBackdatedRequest = x.AllowBackdatedRequest,
            BackdatedLimitDays = x.BackdatedLimitDays, AllowFutureDatedRequest = x.AllowFutureDatedRequest,
            MaximumAdvanceRequestDays = x.MaximumAdvanceRequestDays, DayCalculationMethod = x.DayCalculationMethod,
            ExcludeHoliday = x.ExcludeHoliday, ExcludeWeeklyOff = x.ExcludeWeeklyOff, ReservationTiming = x.ReservationTiming,
            DeductionTiming = x.DeductionTiming, RequireAttachment = x.RequireAttachment,
            AttachmentRequiredAfterDays = x.AttachmentRequiredAfterDays, RequireReplacementEmployee = x.RequireReplacementEmployee,
            RequireManagerApproval = x.RequireManagerApproval, RequireHrVerification = x.RequireHrVerification,
            ApprovalWorkflowCode = x.ApprovalWorkflowCode, EffectiveStartDate = x.EffectiveStartDate, EffectiveEndDate = x.EffectiveEndDate,
            Description = x.Description, IsDefault = x.IsDefault, IsActive = x.IsActive,
            EntitlementPolicyCount = x.EntitlementPolicies.Count(y => !y.IsDelete), CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy, CreateByName = GetActorName(actors, x.CreateBy)
        };
        private async Task<bool> ExistsActiveAsync<T>(Guid id) where T : IdentityModel => await _dbContext.Set<T>().AsNoTracking().AnyAsync(x => EF.Property<Guid>(x, "Id") == id && EF.Property<bool>(x, "IsActive") && !EF.Property<bool>(x, "IsDelete"));
        private async Task<bool> ExistsActiveIfProvidedAsync<T>(Guid? id) where T : IdentityModel => !id.HasValue || id == Guid.Empty || await ExistsActiveAsync<T>(id.Value);
        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstLeavePolicy>().AsNoTracking().Where(x => !x.IsDelete && x.LeavePolicyCode.StartsWith(CodePrefix)).Select(x => x.LeavePolicyCode).ToListAsync();
            var used = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet(); var next = 1; while (used.Contains(next)) next++;
            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }
        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> ids)
        {
            var values = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Users.AsNoTracking().Where(x => values.Contains(x.Id)).Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode }).ToDictionaryAsync(x => x.Id, x => x.Name);
        }
        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> map, Guid id) => id == Guid.Empty ? null : map.TryGetValue(id, out var value) ? value : null;
        private Guid GetCurrentUserId() { var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"); return Guid.TryParse(value, out var id) ? id : Guid.Empty; }
        private static void NormalizePaging(ref int pageNumber, ref int pageSize) { pageNumber = pageNumber < 1 ? 1 : pageNumber; pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100); }
        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value == Guid.Empty ? null : value;
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static List<LeavePolicyCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<LeavePolicyCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
