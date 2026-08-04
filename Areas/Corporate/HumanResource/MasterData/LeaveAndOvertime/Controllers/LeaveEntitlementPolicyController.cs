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

using LeaveEntitlementPolicyPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs.LeaveEntitlementPolicyResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/leave-entitlement-policies")]
    [AccessController(moduleCode: "HUMAN_RESOURCE_MASTER_DATA", moduleName: "Human Resource Master Data", displayName: "Leave Entitlement Policy", AreaName = "Corporate", ControllerName = "LeaveEntitlementPolicy", Description = "Corporate human resource master data leave entitlement policy", SortOrder = 32)]
    [Tags("Corporate / Human Resource / Master Data / Leave and Overtime / Leave Entitlement Policy")]
    public class LeaveEntitlementPolicyController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "LEP-RSMMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public LeaveEntitlementPolicyController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Leave Entitlement Policy", Description = "Melihat metadata filter leave entitlement policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPolicy", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new LeaveEntitlementPolicyFilterMetadataResponse
            {
                DefaultFilter = new LeaveEntitlementPolicyDefaultFilterResponse(),
                SortOptions = new List<LeaveEntitlementPolicySortOptionResponse>
                {
                    new() { Value = "entitlementPolicyCode", Label = "Kode kebijakan hak cuti" },
                    new() { Value = "entitlementPolicyName", Label = "Nama kebijakan hak cuti" },
                    new() { Value = "leavePolicyName", Label = "Kebijakan cuti" },
                    new() { Value = "annualEntitlementDays", Label = "Hak cuti tahunan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isDefault", Label = "Kebijakan default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveEntitlementPolicy.GetFilterMetadata", "Mengambil metadata filter leave entitlement policy.", result);
            return Ok(ApiResponse<LeaveEntitlementPolicyFilterMetadataResponse>.Ok(result, "Metadata filter leave entitlement policy berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Leave Entitlement Policy", Description = "Melihat ringkasan leave entitlement policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPolicy", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var q = BaseQuery();
            var result = new LeaveEntitlementPolicySummaryResponse
            {
                TotalData = await q.CountAsync(), ActiveData = await q.CountAsync(x => x.IsActive),
                InactiveData = await q.CountAsync(x => !x.IsActive), DefaultData = await q.CountAsync(x => x.IsDefault && x.IsActive),
                WithCarryForwardData = await q.CountAsync(x => x.CarryForwardPolicies.Any(y => !y.IsDelete))
            };
            return Ok(ApiResponse<LeaveEntitlementPolicySummaryResponse>.Ok(result, "Ringkasan leave entitlement policy berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Leave Entitlement Policy", Description = "Melihat data leave entitlement policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPolicy", "Read")]
        public async Task<IActionResult> GetData(Guid? leavePolicyId, string? entitlementMethod, string? periodBasis, bool? isDefault, bool? isActive, string? search, string? sortBy = "entitlementPolicyName", string? sortDirection = "asc", int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), leavePolicyId, entitlementMethod, periodBasis, isDefault, isActive, search);
            var totalData = await q.CountAsync();
            var entities = await ApplySorting(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actors = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actors)).ToList();
            return Ok(ApiResponse<LeaveEntitlementPolicyPagedResult>.Ok(new LeaveEntitlementPolicyPagedResult
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Data leave entitlement policy berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Leave Entitlement Policy", Description = "Melihat pilihan leave entitlement policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPolicy", "Read")]
        public async Task<IActionResult> GetOptions(Guid? leavePolicyId, bool onlyActive = true, string? search = null, int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), leavePolicyId, null, null, null, onlyActive ? true : null, search);
            var totalData = await q.CountAsync();
            var items = await q.OrderByDescending(x => x.IsDefault).ThenBy(x => x.EntitlementPolicyName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new LeaveEntitlementPolicyOptionResponse
                {
                    Id = x.Id, LeavePolicyId = x.LeavePolicyId, LeavePolicyName = x.LeavePolicy != null ? x.LeavePolicy.LeavePolicyName : string.Empty,
                    EntitlementPolicyCode = x.EntitlementPolicyCode, EntitlementPolicyName = x.EntitlementPolicyName,
                    EntitlementMethod = x.EntitlementMethod, AnnualEntitlementDays = x.AnnualEntitlementDays, IsDefault = x.IsDefault
                }).ToListAsync();
            return Ok(ApiResponse<LeaveEntitlementPolicyOptionPagedResponse>.Ok(new LeaveEntitlementPolicyOptionPagedResponse
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Pilihan leave entitlement policy berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Leave Entitlement Policy", Description = "Melihat detail leave entitlement policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPolicy", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave entitlement policy tidak ditemukan."));
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var b = MapResponse(entity, actors);
            var result = new LeaveEntitlementPolicyDetailResponse
            {
                Id = b.Id, LeavePolicyId = b.LeavePolicyId, LeavePolicyCode = b.LeavePolicyCode, LeavePolicyName = b.LeavePolicyName,
                LeaveTypeId = b.LeaveTypeId, LeaveTypeName = b.LeaveTypeName, EntitlementPolicyCode = b.EntitlementPolicyCode,
                EntitlementPolicyName = b.EntitlementPolicyName, EntitlementMethod = b.EntitlementMethod, PeriodBasis = b.PeriodBasis,
                GrantTiming = b.GrantTiming, AnnualEntitlementDays = b.AnnualEntitlementDays, AccrualFrequency = b.AccrualFrequency,
                AccrualTiming = b.AccrualTiming, AccrualAmountDays = b.AccrualAmountDays, AccrualStartMonth = b.AccrualStartMonth,
                AccrualStartDay = b.AccrualStartDay, AccrualDayOfMonth = b.AccrualDayOfMonth, FirstAccrualRule = b.FirstAccrualRule,
                FinalAccrualRule = b.FinalAccrualRule, AccrualMaximumPerPeriodDays = b.AccrualMaximumPerPeriodDays,
                IsProratedOnJoin = b.IsProratedOnJoin, IsProratedOnSeparation = b.IsProratedOnSeparation,
                MinimumServiceMonths = b.MinimumServiceMonths, MaximumBalanceDays = b.MaximumBalanceDays,
                ResetMonth = b.ResetMonth, ResetDay = b.ResetDay, RoundingMethod = b.RoundingMethod,
                EffectiveStartDate = b.EffectiveStartDate, EffectiveEndDate = b.EffectiveEndDate, Description = b.Description,
                IsDefault = b.IsDefault, IsActive = b.IsActive, CarryForwardPolicyCount = b.CarryForwardPolicyCount,
                CreateDateTime = b.CreateDateTime, CreateBy = b.CreateBy, CreateByName = b.CreateByName,
                UpdateDateTime = entity.UpdateDateTime, UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            return Ok(ApiResponse<LeaveEntitlementPolicyDetailResponse>.Ok(result, "Detail leave entitlement policy berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Leave Entitlement Policy", Description = "Membuat leave entitlement policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeaveEntitlementPolicy", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeaveEntitlementPolicyRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault) await UnsetDefaultAsync(null, request.LeavePolicyId, now, actor);
            var entity = new MstLeaveEntitlementPolicy
            {
                Id = Guid.NewGuid(), LeavePolicyId = request.LeavePolicyId, EntitlementPolicyCode = await GenerateCodeAsync(),
                EntitlementPolicyName = request.EntitlementPolicyName.Trim(), EntitlementMethod = request.EntitlementMethod.Trim(),
                PeriodBasis = request.PeriodBasis.Trim(), GrantTiming = request.GrantTiming.Trim(), AnnualEntitlementDays = request.AnnualEntitlementDays,
                AccrualFrequency = request.AccrualFrequency.Trim(), AccrualTiming = request.AccrualTiming.Trim(), AccrualAmountDays = request.AccrualAmountDays,
                AccrualStartMonth = request.AccrualStartMonth, AccrualStartDay = request.AccrualStartDay, AccrualDayOfMonth = request.AccrualDayOfMonth,
                FirstAccrualRule = request.FirstAccrualRule.Trim(), FinalAccrualRule = request.FinalAccrualRule.Trim(),
                AccrualMaximumPerPeriodDays = request.AccrualMaximumPerPeriodDays, IsProratedOnJoin = request.IsProratedOnJoin,
                IsProratedOnSeparation = request.IsProratedOnSeparation, MinimumServiceMonths = request.MinimumServiceMonths,
                MaximumBalanceDays = request.MaximumBalanceDays, ResetMonth = request.ResetMonth, ResetDay = request.ResetDay,
                RoundingMethod = request.RoundingMethod.Trim(), EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date, Description = NormalizeText(request.Description),
                IsDefault = request.IsDefault, IsActive = true, CreateDateTime = now, CreateBy = actor, IsDelete = false, IsCancel = false
            };
            _dbContext.Set<MstLeaveEntitlementPolicy>().Add(entity); await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy });
            var response = new LeaveEntitlementPolicyCreateResponse
            {
                Id = entity.Id, LeavePolicyId = entity.LeavePolicyId, EntitlementPolicyCode = entity.EntitlementPolicyCode,
                EntitlementPolicyName = entity.EntitlementPolicyName, IsDefault = entity.IsDefault, IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime, CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actors, entity.CreateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveEntitlementPolicy.Create", "Membuat data leave entitlement policy.", response);
            return Ok(ApiResponse<LeaveEntitlementPolicyCreateResponse>.Ok(response, "Leave entitlement policy berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Leave Entitlement Policy", Description = "Mengubah leave entitlement policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveEntitlementPolicy", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveEntitlementPolicyRequest request)
        {
            var entity = await _dbContext.Set<MstLeaveEntitlementPolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave entitlement policy tidak ditemukan."));
            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault && request.IsActive) await UnsetDefaultAsync(id, request.LeavePolicyId, now, actor);
            entity.LeavePolicyId = request.LeavePolicyId; entity.EntitlementPolicyName = request.EntitlementPolicyName.Trim();
            entity.EntitlementMethod = request.EntitlementMethod.Trim(); entity.PeriodBasis = request.PeriodBasis.Trim();
            entity.GrantTiming = request.GrantTiming.Trim(); entity.AnnualEntitlementDays = request.AnnualEntitlementDays;
            entity.AccrualFrequency = request.AccrualFrequency.Trim(); entity.AccrualTiming = request.AccrualTiming.Trim();
            entity.AccrualAmountDays = request.AccrualAmountDays; entity.AccrualStartMonth = request.AccrualStartMonth;
            entity.AccrualStartDay = request.AccrualStartDay; entity.AccrualDayOfMonth = request.AccrualDayOfMonth;
            entity.FirstAccrualRule = request.FirstAccrualRule.Trim(); entity.FinalAccrualRule = request.FinalAccrualRule.Trim();
            entity.AccrualMaximumPerPeriodDays = request.AccrualMaximumPerPeriodDays; entity.IsProratedOnJoin = request.IsProratedOnJoin;
            entity.IsProratedOnSeparation = request.IsProratedOnSeparation; entity.MinimumServiceMonths = request.MinimumServiceMonths;
            entity.MaximumBalanceDays = request.MaximumBalanceDays; entity.ResetMonth = request.ResetMonth; entity.ResetDay = request.ResetDay;
            entity.RoundingMethod = request.RoundingMethod.Trim(); entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date; entity.Description = NormalizeText(request.Description);
            entity.IsDefault = request.IsDefault && request.IsActive; entity.IsActive = request.IsActive; entity.UpdateDateTime = now; entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            var response = new LeaveEntitlementPolicyUpdateResponse
            {
                Id = entity.Id, LeavePolicyId = entity.LeavePolicyId, EntitlementPolicyCode = entity.EntitlementPolicyCode,
                EntitlementPolicyName = entity.EntitlementPolicyName, IsDefault = entity.IsDefault, IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime, UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveEntitlementPolicy.Update", "Mengubah data leave entitlement policy.", response);
            return Ok(ApiResponse<LeaveEntitlementPolicyUpdateResponse>.Ok(response, "Leave entitlement policy berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Leave Entitlement Policy Status", Description = "Mengubah status leave entitlement policy", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveEntitlementPolicy", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLeaveEntitlementPolicyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstLeaveEntitlementPolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave entitlement policy tidak ditemukan."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            if (request.IsDefault == true && request.IsActive) await UnsetDefaultAsync(id, entity.LeavePolicyId, now, actor);
            entity.IsActive = request.IsActive;
            if (request.IsDefault.HasValue) entity.IsDefault = request.IsDefault.Value && request.IsActive;
            else if (!request.IsActive) entity.IsDefault = false;
            entity.UpdateDateTime = now; entity.UpdateBy = actor; await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status leave entitlement policy berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Leave Entitlement Policy", Description = "Menghapus leave entitlement policy", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LeaveEntitlementPolicy", "Delete")]
        public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteLeaveEntitlementPolicyRequest? request = null)
        {
            var entity = await _dbContext.Set<MstLeaveEntitlementPolicy>().Include(x => x.CarryForwardPolicies).FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Leave entitlement policy tidak ditemukan."));
            if (entity.CarryForwardPolicies.Any(x => !x.IsDelete)) return BadRequest(ApiResponse<object>.Fail(400, "Leave entitlement policy tidak dapat dihapus karena masih digunakan oleh carry forward policy."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            entity.IsDelete = true; entity.IsActive = false; entity.IsDefault = false; entity.DeleteDateTime = now; entity.DeleteBy = actor;
            entity.UpdateDateTime = now; entity.UpdateBy = actor; if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim();
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.DeleteBy });
            var response = new LeaveEntitlementPolicyDeleteResponse
            {
                Id = entity.Id, EntitlementPolicyCode = entity.EntitlementPolicyCode, EntitlementPolicyName = entity.EntitlementPolicyName,
                DeleteDateTime = entity.DeleteDateTime, DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actors, entity.DeleteBy)
            };
            await _loggerService.InfoAsync(LogCategory, "LeaveEntitlementPolicy.Delete", "Menghapus data leave entitlement policy.", response);
            return Ok(ApiResponse<LeaveEntitlementPolicyDeleteResponse>.Ok(response, "Leave entitlement policy berhasil dihapus."));
        }

        private IQueryable<MstLeaveEntitlementPolicy> BaseQuery() => _dbContext.Set<MstLeaveEntitlementPolicy>().AsNoTracking()
            .Include(x => x.LeavePolicy).ThenInclude(x => x!.LeaveType).Include(x => x.CarryForwardPolicies).Where(x => !x.IsDelete);
        private static IQueryable<MstLeaveEntitlementPolicy> ApplyFilter(IQueryable<MstLeaveEntitlementPolicy> q, Guid? leavePolicyId, string? method, string? basis, bool? isDefault, bool? active, string? search)
        {
            if (leavePolicyId.HasValue && leavePolicyId != Guid.Empty) q = q.Where(x => x.LeavePolicyId == leavePolicyId.Value);
            if (!string.IsNullOrWhiteSpace(method)) q = q.Where(x => x.EntitlementMethod == method.Trim());
            if (!string.IsNullOrWhiteSpace(basis)) q = q.Where(x => x.PeriodBasis == basis.Trim());
            if (isDefault.HasValue) q = q.Where(x => x.IsDefault == isDefault.Value);
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
            if (!string.IsNullOrWhiteSpace(search)) { var k = search.Trim().ToLower(); q = q.Where(x => x.EntitlementPolicyCode.ToLower().Contains(k) || x.EntitlementPolicyName.ToLower().Contains(k) || x.EntitlementMethod.ToLower().Contains(k) || (x.LeavePolicy != null && x.LeavePolicy.LeavePolicyName.ToLower().Contains(k))); }
            return q;
        }
        private static IOrderedQueryable<MstLeaveEntitlementPolicy> ApplySorting(IQueryable<MstLeaveEntitlementPolicy> q, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "entitlementPolicyName").Trim().ToLowerInvariant() switch
            {
                "entitlementpolicycode" => desc ? q.OrderByDescending(x => x.EntitlementPolicyCode) : q.OrderBy(x => x.EntitlementPolicyCode),
                "leavepolicyname" => desc ? q.OrderByDescending(x => x.LeavePolicy != null ? x.LeavePolicy.LeavePolicyName : string.Empty) : q.OrderBy(x => x.LeavePolicy != null ? x.LeavePolicy.LeavePolicyName : string.Empty),
                "annualentitlementdays" => desc ? q.OrderByDescending(x => x.AnnualEntitlementDays) : q.OrderBy(x => x.AnnualEntitlementDays),
                "createdatetime" => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                "isdefault" => desc ? q.OrderByDescending(x => x.IsDefault).ThenBy(x => x.EntitlementPolicyName) : q.OrderBy(x => x.IsDefault).ThenBy(x => x.EntitlementPolicyName),
                "isactive" => desc ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.EntitlementPolicyName) : q.OrderBy(x => x.IsActive).ThenBy(x => x.EntitlementPolicyName),
                _ => desc ? q.OrderByDescending(x => x.EntitlementPolicyName) : q.OrderBy(x => x.EntitlementPolicyName)
            };
        }
        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateLeaveEntitlementPolicyRequest request)
        {
            if (request.LeavePolicyId == Guid.Empty) return (false, "Leave policy wajib dipilih.");
            if (string.IsNullOrWhiteSpace(request.EntitlementPolicyName)) return (false, "Nama entitlement policy wajib diisi.");
            if (!await _dbContext.Set<MstLeavePolicy>().AsNoTracking().AnyAsync(x => x.Id == request.LeavePolicyId && x.IsActive && !x.IsDelete)) return (false, "Leave policy tidak ditemukan atau tidak aktif.");
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date) return (false, "Tanggal selesai efektif tidak boleh sebelum tanggal mulai efektif.");
            if (request.AccrualStartMonth.HasValue && request.AccrualStartDay.HasValue && !IsValidMonthDay(request.AccrualStartMonth.Value, request.AccrualStartDay.Value)) return (false, "Tanggal mulai akrual tidak valid.");
            if (request.ResetMonth.HasValue && request.ResetDay.HasValue && !IsValidMonthDay(request.ResetMonth.Value, request.ResetDay.Value)) return (false, "Tanggal reset tidak valid.");
            var name = request.EntitlementPolicyName.Trim().ToLower();
            var duplicate = _dbContext.Set<MstLeaveEntitlementPolicy>().AsNoTracking().Where(x => !x.IsDelete && x.LeavePolicyId == request.LeavePolicyId && x.EntitlementPolicyName.ToLower() == name);
            if (excludeId.HasValue) duplicate = duplicate.Where(x => x.Id != excludeId.Value);
            if (await duplicate.AnyAsync()) return (false, "Nama entitlement policy sudah digunakan pada leave policy tersebut.");
            return (true, null);
        }
        private async Task UnsetDefaultAsync(Guid? excludeId, Guid leavePolicyId, DateTime now, Guid actor)
        {
            var q = _dbContext.Set<MstLeaveEntitlementPolicy>().Where(x => !x.IsDelete && x.IsActive && x.IsDefault && x.LeavePolicyId == leavePolicyId);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
            foreach (var row in await q.ToListAsync()) { row.IsDefault = false; row.UpdateDateTime = now; row.UpdateBy = actor; }
        }
        private LeaveEntitlementPolicyResponse MapResponse(MstLeaveEntitlementPolicy x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id, LeavePolicyId = x.LeavePolicyId, LeavePolicyCode = x.LeavePolicy?.LeavePolicyCode ?? string.Empty,
            LeavePolicyName = x.LeavePolicy?.LeavePolicyName ?? string.Empty, LeaveTypeId = x.LeavePolicy?.LeaveTypeId ?? Guid.Empty,
            LeaveTypeName = x.LeavePolicy?.LeaveType?.LeaveTypeName ?? string.Empty, EntitlementPolicyCode = x.EntitlementPolicyCode,
            EntitlementPolicyName = x.EntitlementPolicyName, EntitlementMethod = x.EntitlementMethod, PeriodBasis = x.PeriodBasis,
            GrantTiming = x.GrantTiming, AnnualEntitlementDays = x.AnnualEntitlementDays, AccrualFrequency = x.AccrualFrequency,
            AccrualTiming = x.AccrualTiming, AccrualAmountDays = x.AccrualAmountDays, AccrualStartMonth = x.AccrualStartMonth,
            AccrualStartDay = x.AccrualStartDay, AccrualDayOfMonth = x.AccrualDayOfMonth, FirstAccrualRule = x.FirstAccrualRule,
            FinalAccrualRule = x.FinalAccrualRule, AccrualMaximumPerPeriodDays = x.AccrualMaximumPerPeriodDays,
            IsProratedOnJoin = x.IsProratedOnJoin, IsProratedOnSeparation = x.IsProratedOnSeparation,
            MinimumServiceMonths = x.MinimumServiceMonths, MaximumBalanceDays = x.MaximumBalanceDays, ResetMonth = x.ResetMonth,
            ResetDay = x.ResetDay, RoundingMethod = x.RoundingMethod, EffectiveStartDate = x.EffectiveStartDate,
            EffectiveEndDate = x.EffectiveEndDate, Description = x.Description, IsDefault = x.IsDefault, IsActive = x.IsActive,
            CarryForwardPolicyCount = x.CarryForwardPolicies.Count(y => !y.IsDelete), CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy, CreateByName = GetActorName(actors, x.CreateBy)
        };
        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstLeaveEntitlementPolicy>().AsNoTracking().Where(x => !x.IsDelete && x.EntitlementPolicyCode.StartsWith(CodePrefix)).Select(x => x.EntitlementPolicyCode).ToListAsync();
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
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static bool IsValidMonthDay(int month, int day) { if (month < 1 || month > 12 || day < 1) return false; return day <= DateTime.DaysInMonth(2024, month); }
    }
}
