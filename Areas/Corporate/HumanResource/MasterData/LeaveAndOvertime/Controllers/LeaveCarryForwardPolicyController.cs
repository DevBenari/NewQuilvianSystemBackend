using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

using LeaveCarryForwardPolicyPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs.LeaveCarryForwardPolicyResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/leave-carry-forward-policies")]
    [AccessController(moduleCode: "HUMAN_RESOURCE_MASTER_DATA", moduleName: "Human Resource Master Data", displayName: "Leave Carry Forward Policy", AreaName = "Corporate", ControllerName = "LeaveCarryForwardPolicy", Description = "Corporate human resource master data leave carry forward policy", SortOrder = 33)]
    [Tags("Corporate / Human Resource / Master Data / Leave and Overtime / Leave Carry Forward Policy")]
    public class LeaveCarryForwardPolicyController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "LCF-RSMMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public LeaveCarryForwardPolicyController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Leave Carry Forward Policy", Description = "Melihat metadata filter leave carry forward policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardPolicy", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new LeaveCarryForwardPolicyFilterMetadataResponse
            {
                DefaultFilter = new LeaveCarryForwardPolicyDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<LeaveCarryForwardPolicySortOptionResponse>
                {
                    new() { Value = "carryForwardPolicyCode", Label = "Kode carry forward" },
                    new() { Value = "carryForwardPolicyName", Label = "Nama carry forward" },
                    new() { Value = "entitlementPolicyName", Label = "Kebijakan entitlement" },
                    new() { Value = "carryForwardPercentage", Label = "Persentase carry forward" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isDefault", Label = "Kebijakan default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveCarryForwardPolicy.GetFilterMetadata", "Mengambil metadata filter leave carry forward policy.", result);
            return Ok(ApiResponse<LeaveCarryForwardPolicyFilterMetadataResponse>.Ok(result, "Metadata filter leave carry forward policy berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Leave Carry Forward Policy", Description = "Melihat ringkasan leave carry forward policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardPolicy", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var q = BaseQuery();
            var result = new LeaveCarryForwardPolicySummaryResponse
            {
                TotalData = await q.CountAsync(), ActiveData = await q.CountAsync(x => x.IsActive),
                InactiveData = await q.CountAsync(x => !x.IsActive), EnabledData = await q.CountAsync(x => x.IsCarryForwardEnabled),
                DefaultData = await q.CountAsync(x => x.IsDefault && x.IsActive), PayoutAllowedData = await q.CountAsync(x => x.IsPayoutAllowed)
            };
            return Ok(ApiResponse<LeaveCarryForwardPolicySummaryResponse>.Ok(result, "Ringkasan leave carry forward policy berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Leave Carry Forward Policy", Description = "Melihat data leave carry forward policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardPolicy", "Read")]
        public async Task<IActionResult> GetData(DateTime? startDate, DateTime? endDate, string? customPeriod, Guid? leaveEntitlementPolicyId, Guid? destinationLeaveTypeId, bool? isCarryForwardEnabled, bool? isPayoutAllowed, bool? isDefault, bool? isActive, string? search, string? sortBy = "carryForwardPolicyName", string? sortDirection = "asc", int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), leaveEntitlementPolicyId, destinationLeaveTypeId, isCarryForwardEnabled, isPayoutAllowed, isDefault, isActive, search);
            q = WorkflowMasterDataSupport.ApplyDateFilter(q, startDate, endDate, customPeriod);
            var totalData = await q.CountAsync();
            var entities = await ApplySorting(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actors = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actors)).ToList();
            return Ok(ApiResponse<LeaveCarryForwardPolicyPagedResult>.Ok(new LeaveCarryForwardPolicyPagedResult
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Data leave carry forward policy berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Leave Carry Forward Policy", Description = "Melihat pilihan leave carry forward policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardPolicy", "Read")]
        public async Task<IActionResult> GetOptions(Guid? leaveEntitlementPolicyId, bool onlyActive = true, string? search = null, int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), leaveEntitlementPolicyId, null, null, null, null, onlyActive ? true : null, search);
            var totalData = await q.CountAsync();
            var items = await q.OrderByDescending(x => x.IsDefault).ThenBy(x => x.CarryForwardPolicyName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new LeaveCarryForwardPolicyOptionResponse
                {
                    Id = x.Id, LeaveEntitlementPolicyId = x.LeaveEntitlementPolicyId,
                    LeaveEntitlementPolicyName = x.LeaveEntitlementPolicy != null ? x.LeaveEntitlementPolicy.EntitlementPolicyName : string.Empty,
                    CarryForwardPolicyCode = x.CarryForwardPolicyCode, CarryForwardPolicyName = x.CarryForwardPolicyName,
                    IsCarryForwardEnabled = x.IsCarryForwardEnabled, IsDefault = x.IsDefault
                }).ToListAsync();
            return Ok(ApiResponse<LeaveCarryForwardPolicyOptionPagedResponse>.Ok(new LeaveCarryForwardPolicyOptionPagedResponse
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Pilihan leave carry forward policy berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Leave Carry Forward Policy", Description = "Melihat detail leave carry forward policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardPolicy", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave carry forward policy tidak ditemukan."));
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var b = MapResponse(entity, actors);
            var result = new LeaveCarryForwardPolicyDetailResponse
            {
                Id = b.Id, LeaveEntitlementPolicyId = b.LeaveEntitlementPolicyId,
                LeaveEntitlementPolicyCode = b.LeaveEntitlementPolicyCode, LeaveEntitlementPolicyName = b.LeaveEntitlementPolicyName,
                LeavePolicyId = b.LeavePolicyId, LeavePolicyName = b.LeavePolicyName, DestinationLeaveTypeId = b.DestinationLeaveTypeId,
                DestinationLeaveTypeName = b.DestinationLeaveTypeName, CarryForwardPolicyCode = b.CarryForwardPolicyCode,
                CarryForwardPolicyName = b.CarryForwardPolicyName, IsCarryForwardEnabled = b.IsCarryForwardEnabled,
                MinimumCarryForwardDays = b.MinimumCarryForwardDays, MaximumCarryForwardDays = b.MaximumCarryForwardDays,
                MaximumCarryForwardPeriods = b.MaximumCarryForwardPeriods, CarryForwardPercentage = b.CarryForwardPercentage,
                CarryForwardExecutionTiming = b.CarryForwardExecutionTiming, RoundingMethod = b.RoundingMethod,
                ExpiryMethod = b.ExpiryMethod, ExpiryMonths = b.ExpiryMonths, ExpiryMonth = b.ExpiryMonth, ExpiryDay = b.ExpiryDay,
                IsPayoutAllowed = b.IsPayoutAllowed, PayoutMaximumDays = b.PayoutMaximumDays, ExcessBalanceAction = b.ExcessBalanceAction,
                EffectiveStartDate = b.EffectiveStartDate, EffectiveEndDate = b.EffectiveEndDate, Description = b.Description,
                IsDefault = b.IsDefault, IsActive = b.IsActive, CreateDateTime = b.CreateDateTime, CreateBy = b.CreateBy,
                CreateByName = b.CreateByName, UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy, UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            return Ok(ApiResponse<LeaveCarryForwardPolicyDetailResponse>.Ok(result, "Detail leave carry forward policy berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Leave Carry Forward Policy", Description = "Membuat leave carry forward policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeaveCarryForwardPolicy", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeaveCarryForwardPolicyRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault) await UnsetDefaultAsync(null, request.LeaveEntitlementPolicyId, now, actor);
            var entity = new MstLeaveCarryForwardPolicy
            {
                Id = Guid.NewGuid(), LeaveEntitlementPolicyId = request.LeaveEntitlementPolicyId,
                DestinationLeaveTypeId = NormalizeGuid(request.DestinationLeaveTypeId), CarryForwardPolicyCode = await GenerateCodeAsync(),
                CarryForwardPolicyName = request.CarryForwardPolicyName.Trim(), IsCarryForwardEnabled = request.IsCarryForwardEnabled,
                MinimumCarryForwardDays = request.MinimumCarryForwardDays, MaximumCarryForwardDays = request.MaximumCarryForwardDays,
                MaximumCarryForwardPeriods = request.MaximumCarryForwardPeriods, CarryForwardPercentage = request.CarryForwardPercentage,
                CarryForwardExecutionTiming = request.CarryForwardExecutionTiming.Trim(), RoundingMethod = request.RoundingMethod.Trim(),
                ExpiryMethod = request.ExpiryMethod.Trim(), ExpiryMonths = request.ExpiryMonths, ExpiryMonth = request.ExpiryMonth,
                ExpiryDay = request.ExpiryDay, IsPayoutAllowed = request.IsPayoutAllowed, PayoutMaximumDays = request.PayoutMaximumDays,
                ExcessBalanceAction = request.ExcessBalanceAction.Trim(), EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date, Description = NormalizeText(request.Description), IsDefault = request.IsDefault,
                IsActive = true, CreateDateTime = now, CreateBy = actor, IsDelete = false, IsCancel = false
            };
            _dbContext.Set<MstLeaveCarryForwardPolicy>().Add(entity); await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy });
            var response = new LeaveCarryForwardPolicyCreateResponse
            {
                Id = entity.Id, LeaveEntitlementPolicyId = entity.LeaveEntitlementPolicyId,
                CarryForwardPolicyCode = entity.CarryForwardPolicyCode, CarryForwardPolicyName = entity.CarryForwardPolicyName,
                IsCarryForwardEnabled = entity.IsCarryForwardEnabled, IsDefault = entity.IsDefault, IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime, CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actors, entity.CreateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveCarryForwardPolicy.Create", "Membuat data leave carry forward policy.", response);
            return Ok(ApiResponse<LeaveCarryForwardPolicyCreateResponse>.Ok(response, "Leave carry forward policy berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Leave Carry Forward Policy", Description = "Mengubah leave carry forward policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveCarryForwardPolicy", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveCarryForwardPolicyRequest request)
        {
            var entity = await _dbContext.Set<MstLeaveCarryForwardPolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave carry forward policy tidak ditemukan."));
            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault && request.IsActive) await UnsetDefaultAsync(id, request.LeaveEntitlementPolicyId, now, actor);
            entity.LeaveEntitlementPolicyId = request.LeaveEntitlementPolicyId; entity.DestinationLeaveTypeId = NormalizeGuid(request.DestinationLeaveTypeId);
            entity.CarryForwardPolicyName = request.CarryForwardPolicyName.Trim(); entity.IsCarryForwardEnabled = request.IsCarryForwardEnabled;
            entity.MinimumCarryForwardDays = request.MinimumCarryForwardDays; entity.MaximumCarryForwardDays = request.MaximumCarryForwardDays;
            entity.MaximumCarryForwardPeriods = request.MaximumCarryForwardPeriods; entity.CarryForwardPercentage = request.CarryForwardPercentage;
            entity.CarryForwardExecutionTiming = request.CarryForwardExecutionTiming.Trim(); entity.RoundingMethod = request.RoundingMethod.Trim();
            entity.ExpiryMethod = request.ExpiryMethod.Trim(); entity.ExpiryMonths = request.ExpiryMonths; entity.ExpiryMonth = request.ExpiryMonth;
            entity.ExpiryDay = request.ExpiryDay; entity.IsPayoutAllowed = request.IsPayoutAllowed; entity.PayoutMaximumDays = request.PayoutMaximumDays;
            entity.ExcessBalanceAction = request.ExcessBalanceAction.Trim(); entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date; entity.Description = NormalizeText(request.Description);
            entity.IsDefault = request.IsDefault && request.IsActive; entity.IsActive = request.IsActive; entity.UpdateDateTime = now; entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            var response = new LeaveCarryForwardPolicyUpdateResponse
            {
                Id = entity.Id, LeaveEntitlementPolicyId = entity.LeaveEntitlementPolicyId,
                CarryForwardPolicyCode = entity.CarryForwardPolicyCode, CarryForwardPolicyName = entity.CarryForwardPolicyName,
                IsCarryForwardEnabled = entity.IsCarryForwardEnabled, IsDefault = entity.IsDefault, IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime, UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveCarryForwardPolicy.Update", "Mengubah data leave carry forward policy.", response);
            return Ok(ApiResponse<LeaveCarryForwardPolicyUpdateResponse>.Ok(response, "Leave carry forward policy berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Leave Carry Forward Policy Status", Description = "Mengubah status leave carry forward policy", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveCarryForwardPolicy", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLeaveCarryForwardPolicyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstLeaveCarryForwardPolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave carry forward policy tidak ditemukan."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault == true && request.IsActive) await UnsetDefaultAsync(id, entity.LeaveEntitlementPolicyId, now, actor);
            entity.IsActive = request.IsActive;
            if (request.IsDefault.HasValue) entity.IsDefault = request.IsDefault.Value && request.IsActive;
            else if (!request.IsActive) entity.IsDefault = false;
            if (request.IsCarryForwardEnabled.HasValue) entity.IsCarryForwardEnabled = request.IsCarryForwardEnabled.Value;
            entity.UpdateDateTime = now; entity.UpdateBy = actor; await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status leave carry forward policy berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Leave Carry Forward Policy", Description = "Menghapus leave carry forward policy", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LeaveCarryForwardPolicy", "Delete")]
        public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteLeaveCarryForwardPolicyRequest? request = null)
        {
            var entity = await _dbContext.Set<MstLeaveCarryForwardPolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave carry forward policy tidak ditemukan."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            entity.IsDelete = true; entity.IsActive = false; entity.IsDefault = false; entity.IsCarryForwardEnabled = false;
            entity.DeleteDateTime = now; entity.DeleteBy = actor; entity.UpdateDateTime = now; entity.UpdateBy = actor;
            if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim();
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.DeleteBy });
            var response = new LeaveCarryForwardPolicyDeleteResponse
            {
                Id = entity.Id, CarryForwardPolicyCode = entity.CarryForwardPolicyCode,
                CarryForwardPolicyName = entity.CarryForwardPolicyName, DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy, DeleteByName = GetActorName(actors, entity.DeleteBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveCarryForwardPolicy.Delete", "Menghapus data leave carry forward policy.", response);
            return Ok(ApiResponse<LeaveCarryForwardPolicyDeleteResponse>.Ok(response, "Leave carry forward policy berhasil dihapus."));
        }

        private IQueryable<MstLeaveCarryForwardPolicy> BaseQuery() => _dbContext.Set<MstLeaveCarryForwardPolicy>().AsNoTracking()
            .Include(x => x.LeaveEntitlementPolicy).ThenInclude(x => x!.LeavePolicy).Include(x => x.DestinationLeaveType).Where(x => !x.IsDelete);
        private static IQueryable<MstLeaveCarryForwardPolicy> ApplyFilter(IQueryable<MstLeaveCarryForwardPolicy> q, Guid? entitlementId, Guid? destinationId, bool? enabled, bool? payout, bool? isDefault, bool? active, string? search)
        {
            if (entitlementId.HasValue && entitlementId != Guid.Empty) q = q.Where(x => x.LeaveEntitlementPolicyId == entitlementId.Value);
            if (destinationId.HasValue && destinationId != Guid.Empty) q = q.Where(x => x.DestinationLeaveTypeId == destinationId.Value);
            if (enabled.HasValue) q = q.Where(x => x.IsCarryForwardEnabled == enabled.Value);
            if (payout.HasValue) q = q.Where(x => x.IsPayoutAllowed == payout.Value);
            if (isDefault.HasValue) q = q.Where(x => x.IsDefault == isDefault.Value);
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
            if (!string.IsNullOrWhiteSpace(search)) { var k = search.Trim().ToLower(); q = q.Where(x => x.CarryForwardPolicyCode.ToLower().Contains(k) || x.CarryForwardPolicyName.ToLower().Contains(k) || x.ExpiryMethod.ToLower().Contains(k) || (x.LeaveEntitlementPolicy != null && x.LeaveEntitlementPolicy.EntitlementPolicyName.ToLower().Contains(k))); }
            return q;
        }
        private static IOrderedQueryable<MstLeaveCarryForwardPolicy> ApplySorting(IQueryable<MstLeaveCarryForwardPolicy> q, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "carryForwardPolicyName").Trim().ToLowerInvariant() switch
            {
                "carryforwardpolicycode" => desc ? q.OrderByDescending(x => x.CarryForwardPolicyCode) : q.OrderBy(x => x.CarryForwardPolicyCode),
                "entitlementpolicyname" => desc ? q.OrderByDescending(x => x.LeaveEntitlementPolicy != null ? x.LeaveEntitlementPolicy.EntitlementPolicyName : string.Empty) : q.OrderBy(x => x.LeaveEntitlementPolicy != null ? x.LeaveEntitlementPolicy.EntitlementPolicyName : string.Empty),
                "carryforwardpercentage" => desc ? q.OrderByDescending(x => x.CarryForwardPercentage) : q.OrderBy(x => x.CarryForwardPercentage),
                "createdatetime" => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                "isdefault" => desc ? q.OrderByDescending(x => x.IsDefault).ThenBy(x => x.CarryForwardPolicyName) : q.OrderBy(x => x.IsDefault).ThenBy(x => x.CarryForwardPolicyName),
                "isactive" => desc ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.CarryForwardPolicyName) : q.OrderBy(x => x.IsActive).ThenBy(x => x.CarryForwardPolicyName),
                _ => desc ? q.OrderByDescending(x => x.CarryForwardPolicyName) : q.OrderBy(x => x.CarryForwardPolicyName)
            };
        }
        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateLeaveCarryForwardPolicyRequest request)
        {
            if (request.LeaveEntitlementPolicyId == Guid.Empty) return (false, "Leave entitlement policy wajib dipilih.");
            if (string.IsNullOrWhiteSpace(request.CarryForwardPolicyName)) return (false, "Nama carry forward policy wajib diisi.");
            if (!await _dbContext.Set<MstLeaveEntitlementPolicy>().AsNoTracking().AnyAsync(x => x.Id == request.LeaveEntitlementPolicyId && x.IsActive && !x.IsDelete)) return (false, "Leave entitlement policy tidak ditemukan atau tidak aktif.");
            if (request.DestinationLeaveTypeId.HasValue && request.DestinationLeaveTypeId != Guid.Empty && !await _dbContext.Set<MstLeaveType>().AsNoTracking().AnyAsync(x => x.Id == request.DestinationLeaveTypeId && x.IsActive && !x.IsDelete)) return (false, "Destination leave type tidak ditemukan atau tidak aktif.");
            if (request.MinimumCarryForwardDays.HasValue && request.MaximumCarryForwardDays.HasValue && request.MaximumCarryForwardDays < request.MinimumCarryForwardDays) return (false, "Maximum carry forward days tidak boleh lebih kecil dari minimum carry forward days.");
            if (request.IsPayoutAllowed && !request.PayoutMaximumDays.HasValue) return (false, "Payout maximum days wajib diisi ketika payout diizinkan.");
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date) return (false, "Tanggal selesai efektif tidak boleh sebelum tanggal mulai efektif.");
            if (request.ExpiryMonth.HasValue && request.ExpiryDay.HasValue && !IsValidMonthDay(request.ExpiryMonth.Value, request.ExpiryDay.Value)) return (false, "Tanggal kedaluwarsa tidak valid.");
            var name = request.CarryForwardPolicyName.Trim().ToLower();
            var duplicate = _dbContext.Set<MstLeaveCarryForwardPolicy>().AsNoTracking().Where(x => !x.IsDelete && x.LeaveEntitlementPolicyId == request.LeaveEntitlementPolicyId && x.CarryForwardPolicyName.ToLower() == name);
            if (excludeId.HasValue) duplicate = duplicate.Where(x => x.Id != excludeId.Value);
            if (await duplicate.AnyAsync()) return (false, "Nama carry forward policy sudah digunakan pada entitlement policy tersebut.");
            return (true, null);
        }
        private async Task UnsetDefaultAsync(Guid? excludeId, Guid entitlementId, DateTime now, Guid actor)
        {
            var q = _dbContext.Set<MstLeaveCarryForwardPolicy>().Where(x => !x.IsDelete && x.IsActive && x.IsDefault && x.LeaveEntitlementPolicyId == entitlementId);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
            foreach (var row in await q.ToListAsync()) { row.IsDefault = false; row.UpdateDateTime = now; row.UpdateBy = actor; }
        }
        private LeaveCarryForwardPolicyResponse MapResponse(MstLeaveCarryForwardPolicy x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id, LeaveEntitlementPolicyId = x.LeaveEntitlementPolicyId,
            LeaveEntitlementPolicyCode = x.LeaveEntitlementPolicy?.EntitlementPolicyCode ?? string.Empty,
            LeaveEntitlementPolicyName = x.LeaveEntitlementPolicy?.EntitlementPolicyName ?? string.Empty,
            LeavePolicyId = x.LeaveEntitlementPolicy?.LeavePolicyId ?? Guid.Empty,
            LeavePolicyName = x.LeaveEntitlementPolicy?.LeavePolicy?.LeavePolicyName ?? string.Empty,
            DestinationLeaveTypeId = x.DestinationLeaveTypeId, DestinationLeaveTypeName = x.DestinationLeaveType?.LeaveTypeName,
            CarryForwardPolicyCode = x.CarryForwardPolicyCode, CarryForwardPolicyName = x.CarryForwardPolicyName,
            IsCarryForwardEnabled = x.IsCarryForwardEnabled, MinimumCarryForwardDays = x.MinimumCarryForwardDays,
            MaximumCarryForwardDays = x.MaximumCarryForwardDays, MaximumCarryForwardPeriods = x.MaximumCarryForwardPeriods,
            CarryForwardPercentage = x.CarryForwardPercentage, CarryForwardExecutionTiming = x.CarryForwardExecutionTiming,
            RoundingMethod = x.RoundingMethod, ExpiryMethod = x.ExpiryMethod, ExpiryMonths = x.ExpiryMonths,
            ExpiryMonth = x.ExpiryMonth, ExpiryDay = x.ExpiryDay, IsPayoutAllowed = x.IsPayoutAllowed,
            PayoutMaximumDays = x.PayoutMaximumDays, ExcessBalanceAction = x.ExcessBalanceAction,
            EffectiveStartDate = x.EffectiveStartDate, EffectiveEndDate = x.EffectiveEndDate, Description = x.Description,
            IsDefault = x.IsDefault, IsActive = x.IsActive, CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy, CreateByName = GetActorName(actors, x.CreateBy)
        };
        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstLeaveCarryForwardPolicy>().AsNoTracking().Where(x => !x.IsDelete && x.CarryForwardPolicyCode.StartsWith(CodePrefix)).Select(x => x.CarryForwardPolicyCode).ToListAsync();
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
        private static bool IsValidMonthDay(int month, int day) { if (month < 1 || month > 12 || day < 1) return false; return day <= DateTime.DaysInMonth(2024, month); }

        private static List<LeaveCarryForwardPolicyCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<LeaveCarryForwardPolicyCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
