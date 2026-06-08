using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;
using System.Security.Claims;

using ResponseWorkSchedulePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.WorkScheduleResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/work-schedules")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Work Schedule",
        AreaName = "Corporate",
        ControllerName = "WorkSchedule",
        Description = "Corporate human resource master data work schedule",
        SortOrder = 8
    )]
    [Tags("Corporate / Human Resource / Master Data / Work Schedule")]
    public class WorkScheduleController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "WS-RSMMC-";
        private const int CodeNumberLength = 5;

        private static readonly HashSet<string> AllowedScheduleTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Shift",
            "NonShift",
            "OnCall",
            "Off"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkScheduleController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkScheduleFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Work Schedule", Description = "Melihat metadata filter work schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkSchedule", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new WorkScheduleFilterMetadataResponse
            {
                DefaultFilter = new WorkScheduleDefaultFilterResponse(),
                CustomPeriods = new List<WorkScheduleCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkScheduleSortOptionResponse>
                {
                    new() { Value = "scheduleCode", Label = "Kode jadwal" },
                    new() { Value = "scheduleName", Label = "Nama jadwal" },
                    new() { Value = "scheduleType", Label = "Tipe jadwal" },
                    new() { Value = "workStartTime", Label = "Jam mulai" },
                    new() { Value = "workEndTime", Label = "Jam selesai" },
                    new() { Value = "checkInToleranceMinutes", Label = "Toleransi check-in" },
                    new() { Value = "checkOutToleranceMinutes", Label = "Toleransi check-out" },
                    new() { Value = "isOvernight", Label = "Overnight" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ScheduleTypes = AllowedScheduleTypes
                    .OrderBy(x => x)
                    .ToList(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkSchedule.GetFilterMetadata",
                "Mengambil metadata filter work schedule.",
                result
            );

            return Ok(ApiResponse<WorkScheduleFilterMetadataResponse>.Ok(
                result,
                "Metadata filter work schedule berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkScheduleSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Work Schedule Summary", Description = "Melihat ringkasan work schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkSchedule", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new WorkScheduleSummaryResponse
            {
                TotalWorkSchedule = await query.CountAsync(),
                ActiveWorkSchedule = await query.CountAsync(x => x.IsActive),
                InactiveWorkSchedule = await query.CountAsync(x => !x.IsActive),
                DefaultWorkSchedule = await query.CountAsync(x => x.IsDefault),
                ShiftSchedule = await query.CountAsync(x => x.ScheduleType == "Shift"),
                NonShiftSchedule = await query.CountAsync(x => x.ScheduleType == "NonShift"),
                OnCallSchedule = await query.CountAsync(x => x.ScheduleType == "OnCall"),
                OffSchedule = await query.CountAsync(x => x.ScheduleType == "Off"),
                OvernightSchedule = await query.CountAsync(x => x.IsOvernight),
                WithCheckInToleranceSchedule = await query.CountAsync(x => x.CheckInToleranceMinutes > 0),
                WithCheckOutToleranceSchedule = await query.CountAsync(x => x.CheckOutToleranceMinutes > 0)
            };

            return Ok(ApiResponse<WorkScheduleSummaryResponse>.Ok(
                result,
                "Ringkasan work schedule berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkSchedulePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Work Schedule", Description = "Melihat data work schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkSchedule", "Read")]
        public async Task<IActionResult> GetWorkSchedules(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? scheduleType,
            [FromQuery] bool? isDefault,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "scheduleCode",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, scheduleType, isDefault, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(entity => MapResponse(entity, actorNames))
                .ToList();

            var result = new ResponseWorkSchedulePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkSchedulePagedResult>.Ok(
                result,
                "Data work schedule berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkScheduleOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Work Schedule Options", Description = "Melihat pilihan work schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkSchedule", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? scheduleType,
            [FromQuery] bool? isDefault,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                scheduleType,
                isDefault,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.ScheduleType)
                .ThenBy(x => x.WorkStartTime)
                .ThenBy(x => x.ScheduleName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new WorkScheduleOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkScheduleOptionPagedResponse>.Ok(
                result,
                "Data pilihan work schedule berhasil diambil."
            ));
        }

        [HttpGet("{workScheduleId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkScheduleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Work Schedule Detail", Description = "Melihat detail work schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkSchedule", "Read")]
        public async Task<IActionResult> GetWorkScheduleById(Guid workScheduleId)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == workScheduleId);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work schedule tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            if (data.UpdateDateTime.HasValue &&
                data.UpdateDateTime.Value == DateTime.MinValue)
            {
                data.UpdateDateTime = null;
            }

            if (!data.CreateBy.HasValue || data.CreateBy.Value == Guid.Empty)
            {
                data.CreateBy = null;
                data.CreateByName = null;
            }

            if (!data.UpdateBy.HasValue || data.UpdateBy.Value == Guid.Empty)
            {
                data.UpdateBy = null;
                data.UpdateByName = null;
            }

            return Ok(ApiResponse<WorkScheduleDetailResponse>.Ok(
                data,
                "Detail work schedule berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkScheduleCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Work Schedule", Description = "Membuat work schedule", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkSchedule", "Create")]
        public async Task<IActionResult> CreateWorkSchedule([FromBody] CreateWorkScheduleRequest request)
        {
            var validation = await ValidateRequestAsync(
                request.ScheduleName,
                request.ScheduleType,
                request.WorkStartTime,
                request.WorkEndTime,
                request.IsOvernight,
                request.CheckInToleranceMinutes,
                request.CheckOutToleranceMinutes,
                existingId: null
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultSchedulesAsync(null);
                }

                var entity = new MstWorkSchedule
                {
                    Id = Guid.NewGuid(),
                    ScheduleCode = await GenerateWorkScheduleCodeAsync(),
                    ScheduleName = NormalizeRequiredText(request.ScheduleName),
                    ScheduleType = NormalizeScheduleType(request.ScheduleType),
                    WorkStartTime = validation.WorkStartTime!.Value,
                    WorkEndTime = validation.WorkEndTime!.Value,
                    IsOvernight = request.IsOvernight,
                    CheckInToleranceMinutes = request.CheckInToleranceMinutes,
                    CheckOutToleranceMinutes = request.CheckOutToleranceMinutes,
                    IsDefault = request.IsDefault,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.MstWorkSchedules.Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = new WorkScheduleCreateResponse
                {
                    Id = entity.Id,
                    ScheduleCode = entity.ScheduleCode,
                    ScheduleName = entity.ScheduleName,
                    ScheduleType = entity.ScheduleType,
                    IsActive = entity.IsActive
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkSchedule.CreateWorkSchedule",
                    "Work schedule berhasil dibuat.",
                    result
                );

                return Ok(ApiResponse<WorkScheduleCreateResponse>.Ok(
                    result,
                    "Work schedule berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkSchedule.CreateWorkSchedule",
                    "Gagal membuat work schedule.",
                    ex,
                    new { request.ScheduleName, request.ScheduleType }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal membuat work schedule: {ex.Message}"
                    )
                );
            }
        }

        [HttpPut("{workScheduleId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Work Schedule", Description = "Mengubah work schedule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkSchedule", "Update")]
        public async Task<IActionResult> UpdateWorkSchedule(
            Guid workScheduleId,
            [FromBody] UpdateWorkScheduleRequest request)
        {
            var entity = await _dbContext.MstWorkSchedules
                .FirstOrDefaultAsync(x => x.Id == workScheduleId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work schedule tidak ditemukan."
                ));
            }

            if (request.IsDefault && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Work schedule default harus aktif."
                ));
            }

            var validation = await ValidateRequestAsync(
                request.ScheduleName,
                request.ScheduleType,
                request.WorkStartTime,
                request.WorkEndTime,
                request.IsOvernight,
                request.CheckInToleranceMinutes,
                request.CheckOutToleranceMinutes,
                workScheduleId
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultSchedulesAsync(workScheduleId);
                }

                var now = DateTime.UtcNow;
                var actorUserId = GetCurrentUserId();

                entity.ScheduleName = NormalizeRequiredText(request.ScheduleName);
                entity.ScheduleType = NormalizeScheduleType(request.ScheduleType);
                entity.WorkStartTime = validation.WorkStartTime!.Value;
                entity.WorkEndTime = validation.WorkEndTime!.Value;
                entity.IsOvernight = request.IsOvernight;
                entity.CheckInToleranceMinutes = request.CheckInToleranceMinutes;
                entity.CheckOutToleranceMinutes = request.CheckOutToleranceMinutes;
                entity.IsDefault = request.IsActive ? request.IsDefault : false;
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkSchedule.UpdateWorkSchedule",
                    "Work schedule berhasil diubah.",
                    new { entity.Id, entity.ScheduleCode, entity.ScheduleName, entity.ScheduleType, entity.IsActive }
                );

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Work schedule berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkSchedule.UpdateWorkSchedule",
                    "Gagal mengubah work schedule.",
                    ex,
                    new { workScheduleId, request.ScheduleName, request.ScheduleType }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal mengubah work schedule: {ex.Message}"
                    )
                );
            }
        }

        [HttpPatch("{workScheduleId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkScheduleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Work Schedule Status", Description = "Mengubah status work schedule", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkSchedule", "Update")]
        public async Task<IActionResult> UpdateWorkScheduleStatus(
            Guid workScheduleId,
            [FromBody] UpdateWorkScheduleStatusRequest request)
        {
            var entity = await _dbContext.MstWorkSchedules
                .FirstOrDefaultAsync(x => x.Id == workScheduleId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work schedule tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;

            if (!request.IsActive)
            {
                entity.IsDefault = false;
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkSchedule.UpdateWorkScheduleStatus",
                "Status work schedule berhasil diubah.",
                new { entity.Id, entity.ScheduleCode, entity.IsActive }
            );

            return await GetWorkScheduleById(entity.Id);
        }

        [HttpPatch("{workScheduleId:guid}/set-default")]
        [ProducesResponseType(typeof(ApiResponse<WorkScheduleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Set Work Schedule Default", Description = "Mengubah default work schedule", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkSchedule", "Update")]
        public async Task<IActionResult> SetDefaultWorkSchedule(
            Guid workScheduleId,
            [FromBody] SetWorkScheduleDefaultRequest request)
        {
            var entity = await _dbContext.MstWorkSchedules
                .FirstOrDefaultAsync(x => x.Id == workScheduleId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work schedule tidak ditemukan."
                ));
            }

            if (request.IsDefault && !entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Work schedule tidak aktif tidak bisa dijadikan default."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultSchedulesAsync(workScheduleId);
                }

                entity.IsDefault = request.IsDefault;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = GetCurrentUserId();

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkSchedule.SetDefaultWorkSchedule",
                    "Default work schedule berhasil diubah.",
                    new { entity.Id, entity.ScheduleCode, entity.IsDefault }
                );

                return await GetWorkScheduleById(entity.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkSchedule.SetDefaultWorkSchedule",
                    "Gagal mengubah default work schedule.",
                    ex,
                    new { workScheduleId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal mengubah default work schedule: {ex.Message}"
                    )
                );
            }
        }

        [HttpDelete("{workScheduleId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Work Schedule", Description = "Menghapus work schedule", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("WorkSchedule", "Delete")]
        public async Task<IActionResult> DeleteWorkSchedule(
            Guid workScheduleId,
            [FromBody] DeleteWorkScheduleRequest? request = null)
        {
            var entity = await _dbContext.MstWorkSchedules
                .FirstOrDefaultAsync(x => x.Id == workScheduleId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work schedule tidak ditemukan."
                ));
            }

            var isUsed = await _dbContext.WfpWorkScheduleAssignments
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkScheduleId == workScheduleId &&
                    !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Work schedule tidak bisa dihapus karena sudah digunakan pada jadwal workforce."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkSchedule.DeleteWorkSchedule",
                "Work schedule berhasil dihapus.",
                new { entity.Id, entity.ScheduleCode, entity.ScheduleName, request?.DeleteReason }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Work schedule berhasil dihapus."
            ));
        }

        private IQueryable<MstWorkSchedule> BuildBaseQuery()
        {
            return _dbContext.MstWorkSchedules
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstWorkSchedule> ApplyDateFilter(
            IQueryable<MstWorkSchedule> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var now = DateTime.UtcNow;
                var today = now.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x => x.CreateDateTime >= today && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x => x.CreateDateTime >= today.AddDays(-6) && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x => x.CreateDateTime >= thisMonthStart && x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x => x.CreateDateTime >= lastMonthStart && x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstWorkSchedule> ApplyStandardFilter(
            IQueryable<MstWorkSchedule> query,
            string? scheduleType,
            bool? isDefault,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(scheduleType))
            {
                var selectedScheduleType = scheduleType.Trim().ToLower();
                query = query.Where(x => x.ScheduleType.ToLower() == selectedScheduleType);
            }

            if (isDefault.HasValue)
                query = query.Where(x => x.IsDefault == isDefault.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ScheduleCode.ToLower().Contains(keyword) ||
                    x.ScheduleName.ToLower().Contains(keyword) ||
                    x.ScheduleType.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IQueryable<MstWorkSchedule> ApplySorting(
            IQueryable<MstWorkSchedule> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "scheduleCode").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "schedulecode" => isDescending ? query.OrderByDescending(x => x.ScheduleCode) : query.OrderBy(x => x.ScheduleCode),
                "schedulename" => isDescending ? query.OrderByDescending(x => x.ScheduleName) : query.OrderBy(x => x.ScheduleName),
                "scheduletype" => isDescending ? query.OrderByDescending(x => x.ScheduleType).ThenBy(x => x.WorkStartTime) : query.OrderBy(x => x.ScheduleType).ThenBy(x => x.WorkStartTime),
                "workstarttime" => isDescending ? query.OrderByDescending(x => x.WorkStartTime) : query.OrderBy(x => x.WorkStartTime),
                "workendtime" => isDescending ? query.OrderByDescending(x => x.WorkEndTime) : query.OrderBy(x => x.WorkEndTime),
                "checkintoleranceminutes" => isDescending ? query.OrderByDescending(x => x.CheckInToleranceMinutes) : query.OrderBy(x => x.CheckInToleranceMinutes),
                "checkouttoleranceminutes" => isDescending ? query.OrderByDescending(x => x.CheckOutToleranceMinutes) : query.OrderBy(x => x.CheckOutToleranceMinutes),
                "isovernight" => isDescending ? query.OrderByDescending(x => x.IsOvernight).ThenBy(x => x.ScheduleCode) : query.OrderBy(x => x.IsOvernight).ThenBy(x => x.ScheduleCode),
                "isdefault" => isDescending ? query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.ScheduleCode) : query.OrderBy(x => x.IsDefault).ThenBy(x => x.ScheduleCode),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ScheduleCode) : query.OrderBy(x => x.IsActive).ThenBy(x => x.ScheduleCode),
                _ => isDescending ? query.OrderByDescending(x => x.ScheduleCode) : query.OrderBy(x => x.ScheduleCode)
            };
        }

        private async Task<(bool IsValid, string Message, TimeOnly? WorkStartTime, TimeOnly? WorkEndTime)> ValidateRequestAsync(
            string scheduleName,
            string scheduleType,
            string workStartTimeText,
            string workEndTimeText,
            bool isOvernight,
            int checkInToleranceMinutes,
            int checkOutToleranceMinutes,
            Guid? existingId)
        {
            if (string.IsNullOrWhiteSpace(scheduleName))
                return (false, "ScheduleName wajib diisi.", null, null);

            if (string.IsNullOrWhiteSpace(scheduleType))
                return (false, "ScheduleType wajib diisi.", null, null);

            var normalizedScheduleType = NormalizeScheduleType(scheduleType);

            if (!AllowedScheduleTypes.Contains(normalizedScheduleType))
                return (false, "ScheduleType hanya boleh Shift, NonShift, OnCall, atau Off.", null, null);

            if (!TryParseTime(workStartTimeText, out var workStartTime))
                return (false, "WorkStartTime harus menggunakan format HH:mm atau HH:mm:ss.", null, null);

            if (!TryParseTime(workEndTimeText, out var workEndTime))
                return (false, "WorkEndTime harus menggunakan format HH:mm atau HH:mm:ss.", null, null);

            if (!isOvernight && workEndTime <= workStartTime)
                return (false, "WorkEndTime harus lebih besar dari WorkStartTime jika IsOvernight false.", null, null);

            if (checkInToleranceMinutes < 0 || checkInToleranceMinutes > 1440)
                return (false, "CheckInToleranceMinutes harus antara 0 sampai 1440.", null, null);

            if (checkOutToleranceMinutes < 0 || checkOutToleranceMinutes > 1440)
                return (false, "CheckOutToleranceMinutes harus antara 0 sampai 1440.", null, null);

            var normalizedName = NormalizeRequiredText(scheduleName).ToLower();

            var duplicateExists = await _dbContext.MstWorkSchedules
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ScheduleName.ToLower() == normalizedName &&
                    x.ScheduleType.ToLower() == normalizedScheduleType.ToLower() &&
                    !x.IsDelete &&
                    (!existingId.HasValue || x.Id != existingId.Value));

            if (duplicateExists)
                return (false, "Work schedule dengan nama dan tipe jadwal tersebut sudah digunakan.", null, null);

            return (true, string.Empty, workStartTime, workEndTime);
        }

        private async Task<string> GenerateWorkScheduleCodeAsync()
        {
            var existingCodes = await _dbContext.MstWorkSchedules
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ScheduleCode.StartsWith(CodePrefix))
                .Select(x => x.ScheduleCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task UnsetOtherDefaultSchedulesAsync(Guid? exceptId)
        {
            var entities = await _dbContext.MstWorkSchedules
                .Where(x =>
                    x.IsDefault &&
                    !x.IsDelete &&
                    (!exceptId.HasValue || x.Id != exceptId.Value))
                .ToListAsync();

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            foreach (var entity in entities)
            {
                entity.IsDefault = false;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
            }
        }

        private static bool TryParseTime(string value, out TimeOnly result)
        {
            return TimeOnly.TryParseExact(
                       value,
                       new[] { "HH:mm:ss", "HH:mm" },
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out result
                   ) ||
                   TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static WorkScheduleResponse MapResponse(
            MstWorkSchedule entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new WorkScheduleResponse
            {
                Id = entity.Id,
                ScheduleCode = entity.ScheduleCode,
                ScheduleName = entity.ScheduleName,
                ScheduleType = entity.ScheduleType,
                WorkStartTime = entity.WorkStartTime.ToString("HH:mm:ss"),
                WorkEndTime = entity.WorkEndTime.ToString("HH:mm:ss"),
                IsOvernight = entity.IsOvernight,
                CheckInToleranceMinutes = entity.CheckInToleranceMinutes,
                CheckOutToleranceMinutes = entity.CheckOutToleranceMinutes,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = entity.CreateBy == Guid.Empty
                    ? null
                    : actorNames.GetValueOrDefault(entity.CreateBy)
            };
        }

        private static WorkScheduleDetailResponse MapDetailResponse(
            MstWorkSchedule entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new WorkScheduleDetailResponse
            {
                Id = entity.Id,
                ScheduleCode = entity.ScheduleCode,
                ScheduleName = entity.ScheduleName,
                ScheduleType = entity.ScheduleType,
                WorkStartTime = entity.WorkStartTime.ToString("HH:mm:ss"),
                WorkEndTime = entity.WorkEndTime.ToString("HH:mm:ss"),
                IsOvernight = entity.IsOvernight,
                CheckInToleranceMinutes = entity.CheckInToleranceMinutes,
                CheckOutToleranceMinutes = entity.CheckOutToleranceMinutes,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = entity.CreateBy == Guid.Empty
                    ? null
                    : actorNames.GetValueOrDefault(entity.CreateBy),

                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = entity.UpdateBy == Guid.Empty
                    ? null
                    : actorNames.GetValueOrDefault(entity.UpdateBy)
            };
        }

        private static WorkScheduleOptionResponse MapOptionResponse(MstWorkSchedule entity)
        {
            return new WorkScheduleOptionResponse
            {
                Id = entity.Id,
                ScheduleCode = entity.ScheduleCode,
                ScheduleName = entity.ScheduleName,
                ScheduleType = entity.ScheduleType,
                WorkStartTime = entity.WorkStartTime.ToString("HH:mm:ss"),
                WorkEndTime = entity.WorkEndTime.ToString("HH:mm:ss"),
                IsOvernight = entity.IsOvernight,
                CheckInToleranceMinutes = entity.CheckInToleranceMinutes,
                CheckOutToleranceMinutes = entity.CheckOutToleranceMinutes,
                IsDefault = entity.IsDefault
            };
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string NormalizeRequiredText(string value)
        {
            return value.Trim();
        }

        private static string NormalizeScheduleType(string value)
        {
            var trimmedValue = value.Trim();

            return AllowedScheduleTypes.FirstOrDefault(x =>
                    string.Equals(x, trimmedValue, StringComparison.OrdinalIgnoreCase))
                ?? trimmedValue;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
                pageNumber = 1;

            if (pageSize <= 0)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

            return (pageNumber, pageSize);
        }
    }
}
