using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

using OvertimeRatePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs.OvertimeRateResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/overtime-rates")]
    [AccessController(moduleCode: "HUMAN_RESOURCE_MASTER_DATA", moduleName: "Human Resource Master Data", displayName: "Overtime Rate", AreaName = "Corporate", ControllerName = "OvertimeRate", Description = "Corporate human resource master data overtime rate", SortOrder = 36)]
    [Tags("Corporate / Human Resource / Master Data / Leave and Overtime / Overtime Rate")]
    public class OvertimeRateController : ControllerBase
    {
        private static readonly HashSet<string> AllowedDayTypes =
            OvertimeValueConstants.DayType.All.ToHashSet(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> AllowedTimeBands =
            OvertimeValueConstants.TimeBand.All.ToHashSet(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> AllowedCalculationMethods =
            OvertimeValueConstants.CalculationMethod.All.ToHashSet(StringComparer.OrdinalIgnoreCase);
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "OTR-RSMMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly OvertimeRateResolverService _rateResolverService;

        public OvertimeRateController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            OvertimeRateResolverService rateResolverService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _rateResolverService = rateResolverService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Overtime Rate", Description = "Melihat metadata filter overtime rate", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRate", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new OvertimeRateFilterMetadataResponse
            {
                DefaultFilter = new OvertimeRateDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                DayTypeOptions = AllowedDayTypes.Select(x => new OvertimeRateStringOptionResponse { Value = x, Label = x }).ToList(),
                TimeBandOptions = AllowedTimeBands.Select(x => new OvertimeRateStringOptionResponse { Value = x, Label = x }).ToList(),
                CalculationMethodOptions = AllowedCalculationMethods.Select(x => new OvertimeRateStringOptionResponse { Value = x, Label = x }).ToList(),
                SortOptions = new List<OvertimeRateSortOptionResponse>
                {
                    new() { Value = "priority", Label = "Prioritas" },
                    new() { Value = "overtimeRateCode", Label = "Kode tarif lembur" },
                    new() { Value = "overtimeRateName", Label = "Nama tarif lembur" },
                    new() { Value = "overtimePolicyName", Label = "Kebijakan lembur" },
                    new() { Value = "dayType", Label = "Tipe hari" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
            await _loggerService.InfoAsync(LogCategory, "OvertimeRate.GetFilterMetadata", "Mengambil metadata filter overtime rate.", result);
            return Ok(ApiResponse<OvertimeRateFilterMetadataResponse>.Ok(result, "Metadata filter overtime rate berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Overtime Rate", Description = "Melihat ringkasan overtime rate", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRate", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var q = BaseQuery();
            var result = new OvertimeRateSummaryResponse
            {
                TotalData = await q.CountAsync(), ActiveData = await q.CountAsync(x => x.IsActive), InactiveData = await q.CountAsync(x => !x.IsActive),
                WorkdayData = await q.CountAsync(x => x.DayType == "Workday"), RestDayData = await q.CountAsync(x => x.DayType == "RestDay"),
                HolidayData = await q.CountAsync(x => x.DayType == "Holiday" || x.DayType == "SpecialHoliday")
            };
            return Ok(ApiResponse<OvertimeRateSummaryResponse>.Ok(result, "Ringkasan overtime rate berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Overtime Rate", Description = "Melihat data overtime rate", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRate", "Read")]
        public async Task<IActionResult> GetData(DateTime? startDate, DateTime? endDate, string? customPeriod, Guid? overtimePolicyId, string? dayType, string? timeBand, string? calculationMethod, bool? isActive, string? search, string? sortBy = "priority", string? sortDirection = "asc", int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), overtimePolicyId, dayType, timeBand, calculationMethod, isActive, search);
            q = WorkflowMasterDataSupport.ApplyDateFilter(q, startDate, endDate, customPeriod);
            var totalData = await q.CountAsync();
            var entities = await ApplySorting(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actors = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actors)).ToList();
            return Ok(ApiResponse<OvertimeRatePagedResult>.Ok(new OvertimeRatePagedResult
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Data overtime rate berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Overtime Rate", Description = "Melihat pilihan overtime rate", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRate", "Read")]
        public async Task<IActionResult> GetOptions(Guid? overtimePolicyId, string? dayType, bool onlyActive = true, string? search = null, int pageNumber = 1, int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var q = ApplyFilter(BaseQuery(), overtimePolicyId, dayType, null, null, onlyActive ? true : null, search);
            var totalData = await q.CountAsync();
            var items = await q.OrderBy(x => x.OvertimePolicyId).ThenByDescending(x => x.Priority).ThenBy(x => x.OvertimeRateName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new OvertimeRateOptionResponse
                {
                    Id = x.Id, OvertimePolicyId = x.OvertimePolicyId, OvertimeRateCode = x.OvertimeRateCode,
                    OvertimeRateName = x.OvertimeRateName, DayType = x.DayType, TimeBand = x.TimeBand,
                    CalculationMethod = x.CalculationMethod, Priority = x.Priority
                }).ToListAsync();
            return Ok(ApiResponse<OvertimeRateOptionPagedResponse>.Ok(new OvertimeRateOptionPagedResponse
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize), Items = items
            }, "Pilihan overtime rate berhasil diambil."));
        }

        [HttpPost("resolve-preview")]
        [AccessAction("Read", "Resolve Overtime Rate", Description = "Melakukan preview resolusi overtime rate", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRate", "Read")]
        public async Task<IActionResult> ResolvePreview(
            [FromBody] OvertimeRateResolveRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _rateResolverService.ResolveAsync(request, cancellationToken);
            await _loggerService.InfoAsync(
                LogCategory,
                "OvertimeRate.ResolvePreview",
                "Melakukan preview resolusi overtime rate.",
                result);
            return Ok(ApiResponse<OvertimeRateResolutionResponse>.Ok(
                result,
                result.Message));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Overtime Rate", Description = "Melihat detail overtime rate", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRate", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Overtime rate tidak ditemukan."));
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var b = MapResponse(entity, actors);
            var result = new OvertimeRateDetailResponse
            {
                Id = b.Id, OvertimePolicyId = b.OvertimePolicyId, OvertimePolicyCode = b.OvertimePolicyCode,
                OvertimePolicyName = b.OvertimePolicyName, OvertimeRateCode = b.OvertimeRateCode, OvertimeRateName = b.OvertimeRateName,
                DayType = b.DayType, TimeBand = b.TimeBand, CalculationMethod = b.CalculationMethod, RateMultiplier = b.RateMultiplier,
                FixedAmount = b.FixedAmount, StartMinute = b.StartMinute, EndMinute = b.EndMinute, StartTime = b.StartTime, EndTime = b.EndTime,
                MinimumEligibleMinutes = b.MinimumEligibleMinutes, MaximumEligibleMinutes = b.MaximumEligibleMinutes, Priority = b.Priority,
                EffectiveStartDate = b.EffectiveStartDate, EffectiveEndDate = b.EffectiveEndDate, Description = b.Description,
                IsActive = b.IsActive, CreateDateTime = b.CreateDateTime, CreateBy = b.CreateBy, CreateByName = b.CreateByName,
                UpdateDateTime = entity.UpdateDateTime, UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            return Ok(ApiResponse<OvertimeRateDetailResponse>.Ok(result, "Detail overtime rate berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Overtime Rate", Description = "Membuat overtime rate", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OvertimeRate", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateOvertimeRateRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            var entity = new MstOvertimeRate
            {
                Id = Guid.NewGuid(), OvertimePolicyId = request.OvertimePolicyId, OvertimeRateCode = await GenerateCodeAsync(),
                OvertimeRateName = request.OvertimeRateName.Trim(), DayType = NormalizeToken(request.DayType, AllowedDayTypes),
                TimeBand = NormalizeToken(request.TimeBand, AllowedTimeBands), CalculationMethod = NormalizeToken(request.CalculationMethod, AllowedCalculationMethods),
                RateMultiplier = request.RateMultiplier, FixedAmount = NormalizeFixedAmount(request),
                StartMinute = NormalizeStartMinute(request), EndMinute = NormalizeEndMinute(request),
                StartTime = NormalizeStartTime(request), EndTime = NormalizeEndTime(request),
                MinimumEligibleMinutes = request.MinimumEligibleMinutes, MaximumEligibleMinutes = request.MaximumEligibleMinutes,
                Priority = request.Priority, EffectiveStartDate = request.EffectiveStartDate?.Date, EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeText(request.Description), IsActive = true, CreateDateTime = now, CreateBy = actor,
                IsDelete = false, IsCancel = false
            };
            _dbContext.Set<MstOvertimeRate>().Add(entity); await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.CreateBy });
            var response = new OvertimeRateCreateResponse
            {
                Id = entity.Id, OvertimePolicyId = entity.OvertimePolicyId, OvertimeRateCode = entity.OvertimeRateCode,
                OvertimeRateName = entity.OvertimeRateName, IsActive = entity.IsActive, CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy, CreateByName = GetActorName(actors, entity.CreateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "OvertimeRate.Create", "Membuat data overtime rate.", response);
            return Ok(ApiResponse<OvertimeRateCreateResponse>.Ok(response, "Overtime rate berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Overtime Rate", Description = "Mengubah overtime rate", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimeRate", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOvertimeRateRequest request)
        {
            var entity = await _dbContext.Set<MstOvertimeRate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Overtime rate tidak ditemukan."));
            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));
            entity.OvertimePolicyId = request.OvertimePolicyId; entity.OvertimeRateName = request.OvertimeRateName.Trim();
            entity.DayType = NormalizeToken(request.DayType, AllowedDayTypes); entity.TimeBand = NormalizeToken(request.TimeBand, AllowedTimeBands);
            entity.CalculationMethod = NormalizeToken(request.CalculationMethod, AllowedCalculationMethods); entity.RateMultiplier = request.RateMultiplier;
            entity.FixedAmount = NormalizeFixedAmount(request); entity.StartMinute = NormalizeStartMinute(request); entity.EndMinute = NormalizeEndMinute(request);
            entity.StartTime = NormalizeStartTime(request); entity.EndTime = NormalizeEndTime(request); entity.MinimumEligibleMinutes = request.MinimumEligibleMinutes;
            entity.MaximumEligibleMinutes = request.MaximumEligibleMinutes; entity.Priority = request.Priority;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date; entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeText(request.Description); entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = GetCurrentUserId(); await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            var response = new OvertimeRateUpdateResponse
            {
                Id = entity.Id, OvertimePolicyId = entity.OvertimePolicyId, OvertimeRateCode = entity.OvertimeRateCode,
                OvertimeRateName = entity.OvertimeRateName, IsActive = entity.IsActive, UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy, UpdateByName = GetActorName(actors, entity.UpdateBy)
            };
            await _loggerService.InfoAsync(LogCategory, "OvertimeRate.Update", "Mengubah data overtime rate.", response);
            return Ok(ApiResponse<OvertimeRateUpdateResponse>.Ok(response, "Overtime rate berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Overtime Rate Status", Description = "Mengubah status overtime rate", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimeRate", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOvertimeRateStatusRequest request)
        {
            var entity = await _dbContext.Set<MstOvertimeRate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Overtime rate tidak ditemukan."));
            if (request.IsActive && !await _dbContext.Set<MstOvertimePolicy>().AsNoTracking().AnyAsync(x => x.Id == entity.OvertimePolicyId && x.IsActive && !x.IsDelete && !x.IsCancel))
                return BadRequest(ApiResponse<object>.Fail(400, "Overtime rate tidak dapat diaktifkan karena overtime policy tidak aktif atau tidak valid."));

            if (request.IsActive)
            {
                var overlap = await _rateResolverService.CheckAmbiguousOverlapAsync(
                    id,
                    BuildDefinitionInput(entity, true));

                if (overlap.HasAmbiguousOverlap)
                {
                    return BadRequest(ApiResponse<object>.Fail(400,
                        $"Overtime rate tidak dapat diaktifkan karena overlap ambigu dengan {overlap.ConflictingRateCode} - {overlap.ConflictingRateName}."));
                }
            }

            entity.IsActive = request.IsActive; entity.UpdateDateTime = DateTime.UtcNow; entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status overtime rate berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Overtime Rate", Description = "Menghapus overtime rate", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("OvertimeRate", "Delete")]
        public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteOvertimeRateRequest? request = null)
        {
            var entity = await _dbContext.Set<MstOvertimeRate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Overtime rate tidak ditemukan."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            entity.IsDelete = true; entity.IsActive = false; entity.DeleteDateTime = now; entity.DeleteBy = actor;
            entity.UpdateDateTime = now; entity.UpdateBy = actor; if (!string.IsNullOrWhiteSpace(request?.DeleteReason)) entity.Description = request.DeleteReason.Trim();
            await _dbContext.SaveChangesAsync();
            var actors = await GetActorNameMapAsync(new[] { entity.DeleteBy });
            var response = new OvertimeRateDeleteResponse
            {
                Id = entity.Id, OvertimeRateCode = entity.OvertimeRateCode, OvertimeRateName = entity.OvertimeRateName,
                DeleteDateTime = entity.DeleteDateTime, DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actors, entity.DeleteBy)
            };
            await _loggerService.InfoAsync(LogCategory, "OvertimeRate.Delete", "Menghapus data overtime rate.", response);
            return Ok(ApiResponse<OvertimeRateDeleteResponse>.Ok(response, "Overtime rate berhasil dihapus."));
        }

        private IQueryable<MstOvertimeRate> BaseQuery() => _dbContext.Set<MstOvertimeRate>().AsNoTracking().Include(x => x.OvertimePolicy).Where(x => !x.IsDelete);
        private static IQueryable<MstOvertimeRate> ApplyFilter(IQueryable<MstOvertimeRate> q, Guid? policyId, string? dayType, string? timeBand, string? method, bool? active, string? search)
        {
            if (policyId.HasValue && policyId != Guid.Empty) q = q.Where(x => x.OvertimePolicyId == policyId.Value);
            if (!string.IsNullOrWhiteSpace(dayType)) q = q.Where(x => x.DayType == dayType.Trim());
            if (!string.IsNullOrWhiteSpace(timeBand)) q = q.Where(x => x.TimeBand == timeBand.Trim());
            if (!string.IsNullOrWhiteSpace(method)) q = q.Where(x => x.CalculationMethod == method.Trim());
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
            if (!string.IsNullOrWhiteSpace(search)) { var k = search.Trim().ToLower(); q = q.Where(x => x.OvertimeRateCode.ToLower().Contains(k) || x.OvertimeRateName.ToLower().Contains(k) || x.DayType.ToLower().Contains(k) || (x.OvertimePolicy != null && x.OvertimePolicy.OvertimePolicyName.ToLower().Contains(k))); }
            return q;
        }
        private static IOrderedQueryable<MstOvertimeRate> ApplySorting(IQueryable<MstOvertimeRate> q, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "priority").Trim().ToLowerInvariant() switch
            {
                "overtimeratecode" => desc ? q.OrderByDescending(x => x.OvertimeRateCode) : q.OrderBy(x => x.OvertimeRateCode),
                "overtimeratename" => desc ? q.OrderByDescending(x => x.OvertimeRateName) : q.OrderBy(x => x.OvertimeRateName),
                "overtimepolicyname" => desc ? q.OrderByDescending(x => x.OvertimePolicy != null ? x.OvertimePolicy.OvertimePolicyName : string.Empty) : q.OrderBy(x => x.OvertimePolicy != null ? x.OvertimePolicy.OvertimePolicyName : string.Empty),
                "daytype" => desc ? q.OrderByDescending(x => x.DayType).ThenBy(x => x.Priority) : q.OrderBy(x => x.DayType).ThenBy(x => x.Priority),
                "createdatetime" => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? q.OrderByDescending(x => x.IsActive).ThenBy(x => x.Priority) : q.OrderBy(x => x.IsActive).ThenBy(x => x.Priority),
                _ => desc ? q.OrderByDescending(x => x.Priority).ThenByDescending(x => x.OvertimeRateName) : q.OrderBy(x => x.Priority).ThenBy(x => x.OvertimeRateName)
            };
        }
        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateOvertimeRateRequest request)
        {
            if (request.OvertimePolicyId == Guid.Empty) return (false, "Overtime policy wajib dipilih.");
            if (string.IsNullOrWhiteSpace(request.OvertimeRateName)) return (false, "Nama overtime rate wajib diisi.");
            if (!AllowedDayTypes.Contains(request.DayType.Trim())) return (false, "Day type tidak valid.");
            if (!AllowedTimeBands.Contains(request.TimeBand.Trim())) return (false, "Time band tidak valid.");
            if (!AllowedCalculationMethods.Contains(request.CalculationMethod.Trim())) return (false, "Calculation method tidak valid.");

            var normalizedDayType = NormalizeToken(request.DayType, AllowedDayTypes);
            var normalizedTimeBand = NormalizeToken(request.TimeBand, AllowedTimeBands);
            var method = NormalizeToken(request.CalculationMethod, AllowedCalculationMethods);
            var isActive = request is UpdateOvertimeRateRequest updateRequest
                ? updateRequest.IsActive
                : true;

            var policy = await _dbContext.Set<MstOvertimePolicy>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.OvertimePolicyId &&
                    (!isActive || x.IsActive) &&
                    !x.IsDelete &&
                    !x.IsCancel);

            if (policy == null) return (false, "Overtime policy tidak ditemukan atau tidak aktif.");

            if ((method == OvertimeValueConstants.CalculationMethod.FixedAmount ||
                 method == OvertimeValueConstants.CalculationMethod.HigherOfMultiplierOrFixed) &&
                (!request.FixedAmount.HasValue || request.FixedAmount <= 0))
                return (false, "Fixed amount wajib lebih besar dari nol untuk calculation method tersebut.");

            if ((method == OvertimeValueConstants.CalculationMethod.Multiplier ||
                 method == OvertimeValueConstants.CalculationMethod.HigherOfMultiplierOrFixed) &&
                request.RateMultiplier <= 0)
                return (false, "Rate multiplier wajib lebih besar dari nol.");

            if (normalizedTimeBand == OvertimeValueConstants.TimeBand.AllDay)
            {
                if (request.StartMinute != 0 || request.EndMinute.HasValue || request.StartTime.HasValue || request.EndTime.HasValue)
                    return (false, "Time band AllDay tidak boleh memiliki interval menit atau jam.");
            }
            else if (OvertimeValueConstants.TimeBand.UsesMinuteRange(normalizedTimeBand))
            {
                if (!request.EndMinute.HasValue || request.EndMinute.Value <= request.StartMinute)
                    return (false, "Time band FirstHour/NextHour wajib memiliki end minute yang lebih besar dari start minute.");
                if (request.StartTime.HasValue || request.EndTime.HasValue)
                    return (false, "Time band berbasis menit tidak boleh memiliki start time atau end time.");
            }
            else if (OvertimeValueConstants.TimeBand.UsesClockRange(normalizedTimeBand))
            {
                if (!request.StartTime.HasValue || !request.EndTime.HasValue)
                    return (false, "Time band Night/Custom wajib memiliki start time dan end time.");
                if (request.StartTime.Value == request.EndTime.Value)
                    return (false, "Start time dan end time tidak boleh sama.");
                if (request.StartMinute != 0 || request.EndMinute.HasValue)
                    return (false, "Time band berbasis jam tidak boleh memiliki interval menit.");
            }

            if (request.MaximumEligibleMinutes.HasValue && request.MaximumEligibleMinutes.Value < request.MinimumEligibleMinutes)
                return (false, "Maximum eligible minutes tidak boleh lebih kecil dari minimum eligible minutes.");

            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
                return (false, "Tanggal selesai efektif tidak boleh sebelum tanggal mulai efektif.");

            if (policy.EffectiveStartDate.HasValue && request.EffectiveStartDate.HasValue &&
                request.EffectiveStartDate.Value.Date < policy.EffectiveStartDate.Value.Date)
                return (false, "Tanggal mulai overtime rate tidak boleh sebelum tanggal mulai overtime policy.");

            if (policy.EffectiveEndDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date > policy.EffectiveEndDate.Value.Date)
                return (false, "Tanggal selesai overtime rate tidak boleh setelah tanggal selesai overtime policy.");

            var name = request.OvertimeRateName.Trim().ToLower();
            var duplicate = _dbContext.Set<MstOvertimeRate>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.OvertimePolicyId == request.OvertimePolicyId &&
                    x.DayType == normalizedDayType &&
                    x.TimeBand == normalizedTimeBand &&
                    x.OvertimeRateName.ToLower() == name);

            if (excludeId.HasValue) duplicate = duplicate.Where(x => x.Id != excludeId.Value);
            if (await duplicate.AnyAsync()) return (false, "Nama overtime rate sudah digunakan untuk overtime policy, day type, dan time band tersebut.");

            var overlap = await _rateResolverService.CheckAmbiguousOverlapAsync(
                excludeId,
                BuildDefinitionInput(request, isActive));

            if (overlap.HasAmbiguousOverlap)
            {
                return (false,
                    $"Overtime rate overlap ambigu dengan {overlap.ConflictingRateCode} - {overlap.ConflictingRateName}. Gunakan priority, periode efektif, atau interval yang berbeda.");
            }

            return (true, null);
        }

        private static OvertimeRateDefinitionInput BuildDefinitionInput(
            CreateOvertimeRateRequest request,
            bool isActive)
        {
            var timeBand = NormalizeToken(request.TimeBand, AllowedTimeBands);
            return new OvertimeRateDefinitionInput
            {
                OvertimePolicyId = request.OvertimePolicyId,
                DayType = NormalizeToken(request.DayType, AllowedDayTypes),
                TimeBand = timeBand,
                StartMinute = NormalizeStartMinute(request),
                EndMinute = NormalizeEndMinute(request),
                StartTime = NormalizeStartTime(request),
                EndTime = NormalizeEndTime(request),
                Priority = request.Priority,
                IsActive = isActive,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date
            };
        }

        private static OvertimeRateDefinitionInput BuildDefinitionInput(
            MstOvertimeRate entity,
            bool isActive) => new()
        {
            OvertimePolicyId = entity.OvertimePolicyId,
            DayType = entity.DayType,
            TimeBand = entity.TimeBand,
            StartMinute = entity.StartMinute,
            EndMinute = entity.EndMinute,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Priority = entity.Priority,
            IsActive = isActive,
            EffectiveStartDate = entity.EffectiveStartDate,
            EffectiveEndDate = entity.EffectiveEndDate
        };

        private static decimal? NormalizeFixedAmount(CreateOvertimeRateRequest request)
        {
            var method = NormalizeToken(request.CalculationMethod, AllowedCalculationMethods);
            return method == OvertimeValueConstants.CalculationMethod.Multiplier
                ? null
                : request.FixedAmount;
        }

        private static int NormalizeStartMinute(CreateOvertimeRateRequest request)
        {
            var timeBand = NormalizeToken(request.TimeBand, AllowedTimeBands);
            return OvertimeValueConstants.TimeBand.UsesMinuteRange(timeBand)
                ? request.StartMinute
                : 0;
        }

        private static int? NormalizeEndMinute(CreateOvertimeRateRequest request)
        {
            var timeBand = NormalizeToken(request.TimeBand, AllowedTimeBands);
            return OvertimeValueConstants.TimeBand.UsesMinuteRange(timeBand)
                ? request.EndMinute
                : null;
        }

        private static TimeOnly? NormalizeStartTime(CreateOvertimeRateRequest request)
        {
            var timeBand = NormalizeToken(request.TimeBand, AllowedTimeBands);
            return OvertimeValueConstants.TimeBand.UsesClockRange(timeBand)
                ? request.StartTime
                : null;
        }

        private static TimeOnly? NormalizeEndTime(CreateOvertimeRateRequest request)
        {
            var timeBand = NormalizeToken(request.TimeBand, AllowedTimeBands);
            return OvertimeValueConstants.TimeBand.UsesClockRange(timeBand)
                ? request.EndTime
                : null;
        }

        private OvertimeRateResponse MapResponse(MstOvertimeRate x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id, OvertimePolicyId = x.OvertimePolicyId, OvertimePolicyCode = x.OvertimePolicy?.OvertimePolicyCode ?? string.Empty,
            OvertimePolicyName = x.OvertimePolicy?.OvertimePolicyName ?? string.Empty, OvertimeRateCode = x.OvertimeRateCode,
            OvertimeRateName = x.OvertimeRateName, DayType = x.DayType, TimeBand = x.TimeBand, CalculationMethod = x.CalculationMethod,
            RateMultiplier = x.RateMultiplier, FixedAmount = x.FixedAmount, StartMinute = x.StartMinute, EndMinute = x.EndMinute,
            StartTime = x.StartTime, EndTime = x.EndTime, MinimumEligibleMinutes = x.MinimumEligibleMinutes,
            MaximumEligibleMinutes = x.MaximumEligibleMinutes, Priority = x.Priority, EffectiveStartDate = x.EffectiveStartDate,
            EffectiveEndDate = x.EffectiveEndDate, Description = x.Description, IsActive = x.IsActive, CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy, CreateByName = GetActorName(actors, x.CreateBy)
        };
        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstOvertimeRate>().AsNoTracking().Where(x => !x.IsDelete && x.OvertimeRateCode.StartsWith(CodePrefix)).Select(x => x.OvertimeRateCode).ToListAsync();
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
        private static string NormalizeToken(string value, HashSet<string> allowed) => allowed.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));

        private static List<OvertimeRateCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<OvertimeRateCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
