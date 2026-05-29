using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/work-schedules")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Work Schedule",
        AreaName = "Corporate",
        ControllerName = "WorkforceWorkSchedule",
        Description = "Corporate human resource workforce work schedule",
        SortOrder = 18
    )]
    [Tags("Corporate / Human Resource / Workforce / Work Schedule")]
    public class WorkforceWorkScheduleController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceWorkScheduleController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Work Schedule",
            Description = "Melihat data work schedule workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceWorkSchedule", "Read")]
        public async Task<IActionResult> GetFilterMetadata(Guid workforceProfileId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;

            var result = new WorkforceWorkScheduleFilterMetadataResponse
            {
                DefaultFilter = new WorkforceWorkScheduleDefaultFilterResponse
                {
                    WorkforceProfileId = workforceProfileId,
                    StartDate = new DateTime(today.Year, today.Month, 1),
                    EndDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1),
                    SortBy = "scheduleDate",
                    SortDirection = "asc",
                    PageNumber = 1,
                    PageSize = 31
                },
                SortOptions = new List<WorkforceWorkScheduleSortOptionResponse>
                {
                    new() { Value = "scheduleDate", Label = "Tanggal jadwal" },
                    new() { Value = "workScheduleName", Label = "Nama jadwal" },
                    new() { Value = "workScheduleCode", Label = "Kode jadwal" },
                    new() { Value = "scheduleType", Label = "Tipe jadwal" },
                    new() { Value = "workStartTime", Label = "Jam mulai" },
                    new() { Value = "workEndTime", Label = "Jam selesai" },
                    new() { Value = "isOffDay", Label = "Hari libur" },
                    new() { Value = "isOvertimePlanned", Label = "Rencana lembur" },
                    new() { Value = "isOnCall", Label = "On call" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 7, 14, 31, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceWorkSchedule.GetFilterMetadata",
                "Mengambil metadata filter work schedule workforce.",
                new { workforceProfileId }
            );

            return Ok(ApiResponse<WorkforceWorkScheduleFilterMetadataResponse>.Ok(
                result,
                "Metadata filter work schedule workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<WorkforceWorkScheduleOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Work Schedule Options",
            Description = "Melihat pilihan master work schedule",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceWorkSchedule", "Read")]
        public async Task<IActionResult> GetWorkScheduleOptions(
            Guid workforceProfileId,
            [FromQuery] string? search,
            [FromQuery] string? scheduleType,
            [FromQuery] bool onlyActive = true)
        {
            var profileExists = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = _dbContext.MstWorkSchedules
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(scheduleType))
            {
                var selectedScheduleType = scheduleType.Trim().ToLower();
                query = query.Where(x => x.ScheduleType.ToLower() == selectedScheduleType);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ScheduleCode.ToLower().Contains(keyword) ||
                    x.ScheduleName.ToLower().Contains(keyword) ||
                    x.ScheduleType.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.ScheduleType)
                .ThenBy(x => x.WorkStartTime)
                .ThenBy(x => x.ScheduleName)
                .Select(x => new WorkforceWorkScheduleOptionResponse
                {
                    Id = x.Id,
                    WorkScheduleCode = x.ScheduleCode,
                    WorkScheduleName = x.ScheduleName,
                    ScheduleType = x.ScheduleType,
                    WorkStartTime = x.WorkStartTime.ToString("HH:mm:ss"),
                    WorkEndTime = x.WorkEndTime.ToString("HH:mm:ss"),
                    IsOvernight = x.IsOvernight,
                    IsDefault = x.IsDefault,
                    CheckInToleranceMinutes = x.CheckInToleranceMinutes,
                    CheckOutToleranceMinutes = x.CheckOutToleranceMinutes
                })
                .ToListAsync();

            return Ok(ApiResponse<List<WorkforceWorkScheduleOptionResponse>>.Ok(
                data,
                "Data pilihan work schedule berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Work Schedule Summary",
            Description = "Melihat ringkasan work schedule workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceWorkSchedule", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] Guid? workScheduleId,
            [FromQuery] string? scheduleType,
            [FromQuery] bool? isActive)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = BuildBaseQuery(workforceProfileId);

            query = ApplyFilters(
                query,
                search: null,
                startDate,
                endDate,
                workScheduleId,
                scheduleType,
                isOffDay: null,
                isOvertimePlanned: null,
                isOnCall: null,
                isActive
            );

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var result = new WorkforceWorkScheduleSummaryResponse
            {
                TotalSchedule = await query.CountAsync(),
                ActiveSchedule = await query.CountAsync(x => x.IsActive),
                InactiveSchedule = await query.CountAsync(x => !x.IsActive),
                WorkingDaySchedule = await query.CountAsync(x => !x.IsOffDay),
                OffDaySchedule = await query.CountAsync(x => x.IsOffDay),
                OvertimePlannedSchedule = await query.CountAsync(x => x.IsOvertimePlanned),
                OnCallSchedule = await query.CountAsync(x => x.IsOnCall),
                OvernightSchedule = await query.CountAsync(x => x.WorkSchedule != null && x.WorkSchedule.IsOvernight),
                TodaySchedule = await query.CountAsync(x => x.ScheduleDate == today),
                UpcomingSchedule = await query.CountAsync(x => x.ScheduleDate > today),
                PastSchedule = await query.CountAsync(x => x.ScheduleDate < today)
            };

            return Ok(ApiResponse<WorkforceWorkScheduleSummaryResponse>.Ok(
                result,
                "Ringkasan work schedule workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Work Schedule",
            Description = "Melihat data work schedule workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceWorkSchedule", "Read")]
        public async Task<IActionResult> GetWorkSchedules(
            Guid workforceProfileId,
            [FromQuery] string? search,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] Guid? workScheduleId,
            [FromQuery] string? scheduleType,
            [FromQuery] bool? isOffDay,
            [FromQuery] bool? isOvertimePlanned,
            [FromQuery] bool? isOnCall,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "scheduleDate",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 31)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);

            query = ApplyFilters(
                query,
                search,
                startDate,
                endDate,
                workScheduleId,
                scheduleType,
                isOffDay,
                isOvertimePlanned,
                isOnCall,
                isActive
            );

            var totalData = await query.CountAsync();
            var activeData = await query.CountAsync(x => x.IsActive);
            var workingDayData = await query.CountAsync(x => !x.IsOffDay);
            var offDayData = await query.CountAsync(x => x.IsOffDay);
            var overtimePlannedData = await query.CountAsync(x => x.IsOvertimePlanned);
            var onCallData = await query.CountAsync(x => x.IsOnCall);

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(x => MapWorkScheduleResponse(x, profile))
                .ToList();

            var result = new WorkforceWorkScheduleListResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                StartDate = startDate?.Date,
                EndDate = endDate?.Date,
                TotalData = totalData,
                ActiveData = activeData,
                WorkingDayData = workingDayData,
                OffDayData = offDayData,
                OvertimePlannedData = overtimePlannedData,
                OnCallData = onCallData,
                Items = items
            };

            return Ok(ApiResponse<WorkforceWorkScheduleListResponse>.Ok(
                result,
                "Data work schedule workforce berhasil diambil."
            ));
        }

        [HttpGet("calendar")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleCalendarResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Work Schedule Calendar",
            Description = "Melihat calendar work schedule workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceWorkSchedule", "Read")]
        public async Task<IActionResult> GetCalendar(
            Guid workforceProfileId,
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;
            var selectedYear = year ?? today.Year;
            var selectedMonth = month ?? today.Month;

            if (selectedYear < 1900 || selectedYear > 9999)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Year tidak valid."
                ));
            }

            if (selectedMonth < 1 || selectedMonth > 12)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Month harus berada di antara 1 sampai 12."
                ));
            }

            var start = new DateTime(selectedYear, selectedMonth, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var startDate = DateOnly.FromDateTime(start);
            var endDate = DateOnly.FromDateTime(end);

            var entities = await BuildBaseQuery(workforceProfileId)
                .Where(x => x.ScheduleDate >= startDate && x.ScheduleDate <= endDate)
                .OrderBy(x => x.ScheduleDate)
                .ThenBy(x => x.WorkSchedule != null ? x.WorkSchedule.WorkStartTime : TimeOnly.MinValue)
                .ToListAsync();

            var items = entities
                .Select(x => MapWorkScheduleResponse(x, profile))
                .ToList();

            var result = new WorkforceWorkScheduleCalendarResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                Year = selectedYear,
                Month = selectedMonth,
                TotalData = items.Count,
                WorkingDayData = items.Count(x => !x.IsOffDay),
                OffDayData = items.Count(x => x.IsOffDay),
                OvertimePlannedData = items.Count(x => x.IsOvertimePlanned),
                OnCallData = items.Count(x => x.IsOnCall),
                Items = items
            };

            return Ok(ApiResponse<WorkforceWorkScheduleCalendarResponse>.Ok(
                result,
                "Data calendar work schedule workforce berhasil diambil."
            ));
        }

        [HttpGet("{workforceWorkScheduleId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Work Schedule Detail",
            Description = "Melihat detail work schedule workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceWorkSchedule", "Read")]
        public async Task<IActionResult> GetWorkScheduleById(
            Guid workforceProfileId,
            Guid workforceWorkScheduleId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceWorkScheduleId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work schedule workforce tidak ditemukan."
                ));
            }

            var data = MapWorkScheduleDetailResponse(entity, profile);

            return Ok(ApiResponse<WorkforceWorkScheduleDetailResponse>.Ok(
                data,
                "Detail work schedule workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Work Schedule",
            Description = "Membuat work schedule workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceWorkSchedule", "Create")]
        public async Task<IActionResult> CreateWorkSchedule(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceWorkScheduleRequest request)
        {
            var validation = await ValidateWorkScheduleRequestAsync(
                workforceProfileId,
                request.WorkScheduleId,
                request.ScheduleDate,
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
            var scheduleDate = DateOnly.FromDateTime(request.ScheduleDate.Date);

            var entity = new WfpWorkScheduleAssignment
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                WorkScheduleId = request.WorkScheduleId,
                ScheduleDate = scheduleDate,
                IsOffDay = request.IsOffDay,
                IsOvertimePlanned = request.IsOvertimePlanned,
                IsOnCall = request.IsOnCall,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.WfpWorkScheduleAssignments.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceWorkSchedule.CreateWorkSchedule",
                "Work schedule workforce berhasil dibuat.",
                new { entity.Id, entity.WorkforceProfileId, entity.WorkScheduleId, entity.ScheduleDate }
            );

            return await GetWorkScheduleById(workforceProfileId, entity.Id);
        }

        [HttpPost("bulk")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Bulk Create Workforce Work Schedule",
            Description = "Membuat work schedule workforce secara bulk",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceWorkSchedule", "Create")]
        public async Task<IActionResult> BulkCreateWorkSchedule(
            Guid workforceProfileId,
            [FromBody] BulkCreateWorkforceWorkScheduleRequest request)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Items wajib diisi minimal 1 data."
                ));
            }

            var duplicateDateInRequest = request.Items
                .GroupBy(x => DateOnly.FromDateTime(x.ScheduleDate.Date))
                .FirstOrDefault(x => x.Count() > 1);

            if (duplicateDateInRequest != null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Terdapat tanggal jadwal yang duplikat di request: {duplicateDateInRequest.Key:yyyy-MM-dd}."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                var actorUserId = GetCurrentUserId();

                foreach (var item in request.Items)
                {
                    var validation = await ValidateWorkScheduleMasterAsync(item.WorkScheduleId);

                    if (!validation.IsValid)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            validation.Message
                        ));
                    }

                    var processResult = await CreateOrReplaceScheduleAsync(
                        workforceProfileId,
                        item.WorkScheduleId,
                        DateOnly.FromDateTime(item.ScheduleDate.Date),
                        item.IsOffDay,
                        item.IsOvertimePlanned,
                        item.IsOnCall,
                        item.Description,
                        item.IsActive,
                        request.ReplaceExistingSameDate,
                        now,
                        actorUserId
                    );

                    if (!processResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            processResult.Message
                        ));
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceWorkSchedule.BulkCreateWorkSchedule",
                    "Bulk work schedule workforce berhasil dibuat.",
                    new { workforceProfileId, TotalData = request.Items.Count, request.ReplaceExistingSameDate }
                );

                var firstDate = request.Items.Min(x => x.ScheduleDate.Date);
                var lastDate = request.Items.Max(x => x.ScheduleDate.Date);

                return await GetWorkSchedules(
                    workforceProfileId,
                    search: null,
                    startDate: firstDate,
                    endDate: lastDate,
                    workScheduleId: null,
                    scheduleType: null,
                    isOffDay: null,
                    isOvertimePlanned: null,
                    isOnCall: null,
                    isActive: null,
                    sortBy: "scheduleDate",
                    sortDirection: "asc",
                    pageNumber: 1,
                    pageSize: Math.Max(request.Items.Count, 31)
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceWorkSchedule.BulkCreateWorkSchedule",
                    "Gagal membuat bulk work schedule workforce.",
                    ex,
                    new { workforceProfileId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal membuat bulk work schedule workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Generate Workforce Work Schedule",
            Description = "Generate work schedule workforce berdasarkan periode dan hari",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceWorkSchedule", "Create")]
        public async Task<IActionResult> GenerateWorkSchedule(
            Guid workforceProfileId,
            [FromBody] GenerateWorkforceWorkScheduleRequest request)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var dateRangeValidation = ValidateDateRange(request.StartDate, request.EndDate, maxDays: 366);

            if (!dateRangeValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRangeValidation.Message
                ));
            }

            var scheduleValidation = await ValidateWorkScheduleMasterAsync(request.WorkScheduleId);

            if (!scheduleValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    scheduleValidation.Message
                ));
            }

            var dates = BuildGenerateDates(request);

            if (!dates.Any())
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tidak ada tanggal yang sesuai dengan pilihan hari."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                var actorUserId = GetCurrentUserId();

                foreach (var date in dates)
                {
                    var processResult = await CreateOrReplaceScheduleAsync(
                        workforceProfileId,
                        request.WorkScheduleId,
                        date,
                        request.IsOffDay,
                        request.IsOvertimePlanned,
                        request.IsOnCall,
                        request.Description,
                        request.IsActive,
                        request.ReplaceExistingSameDate,
                        now,
                        actorUserId
                    );

                    if (!processResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            processResult.Message
                        ));
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceWorkSchedule.GenerateWorkSchedule",
                    "Generate work schedule workforce berhasil.",
                    new { workforceProfileId, TotalData = dates.Count, request.StartDate, request.EndDate }
                );

                return await GetWorkSchedules(
                    workforceProfileId,
                    search: null,
                    startDate: request.StartDate.Date,
                    endDate: request.EndDate.Date,
                    workScheduleId: null,
                    scheduleType: null,
                    isOffDay: null,
                    isOvertimePlanned: null,
                    isOnCall: null,
                    isActive: null,
                    sortBy: "scheduleDate",
                    sortDirection: "asc",
                    pageNumber: 1,
                    pageSize: Math.Max(dates.Count, 31)
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceWorkSchedule.GenerateWorkSchedule",
                    "Gagal generate work schedule workforce.",
                    ex,
                    new { workforceProfileId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal generate work schedule workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpPost("copy")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Copy Workforce Work Schedule",
            Description = "Copy work schedule workforce dari periode sumber ke periode target",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceWorkSchedule", "Create")]
        public async Task<IActionResult> CopyWorkSchedule(
            Guid workforceProfileId,
            [FromBody] CopyWorkforceWorkScheduleRequest request)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var sourceRangeValidation = ValidateDateRange(request.SourceStartDate, request.SourceEndDate, maxDays: 366);

            if (!sourceRangeValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    sourceRangeValidation.Message
                ));
            }

            var sourceStart = DateOnly.FromDateTime(request.SourceStartDate.Date);
            var sourceEnd = DateOnly.FromDateTime(request.SourceEndDate.Date);
            var targetStart = DateOnly.FromDateTime(request.TargetStartDate.Date);

            var sourceItems = await _dbContext.WfpWorkScheduleAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.ScheduleDate >= sourceStart &&
                    x.ScheduleDate <= sourceEnd &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderBy(x => x.ScheduleDate)
                .ToListAsync();

            if (!sourceItems.Any())
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Data jadwal sumber tidak ditemukan."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                var actorUserId = GetCurrentUserId();

                foreach (var source in sourceItems)
                {
                    var dayOffset = source.ScheduleDate.DayNumber - sourceStart.DayNumber;
                    var targetDate = targetStart.AddDays(dayOffset);
                    var description = NormalizeNullableText(request.Description) ?? source.Description;

                    var processResult = await CreateOrReplaceScheduleAsync(
                        workforceProfileId,
                        source.WorkScheduleId,
                        targetDate,
                        source.IsOffDay,
                        source.IsOvertimePlanned,
                        source.IsOnCall,
                        description,
                        source.IsActive,
                        request.ReplaceExistingSameDate,
                        now,
                        actorUserId
                    );

                    if (!processResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            processResult.Message
                        ));
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceWorkSchedule.CopyWorkSchedule",
                    "Copy work schedule workforce berhasil.",
                    new
                    {
                        workforceProfileId,
                        request.SourceStartDate,
                        request.SourceEndDate,
                        request.TargetStartDate,
                        TotalData = sourceItems.Count
                    }
                );

                var targetEnd = targetStart.AddDays(sourceEnd.DayNumber - sourceStart.DayNumber).ToDateTime(TimeOnly.MinValue);

                return await GetWorkSchedules(
                    workforceProfileId,
                    search: null,
                    startDate: request.TargetStartDate.Date,
                    endDate: targetEnd,
                    workScheduleId: null,
                    scheduleType: null,
                    isOffDay: null,
                    isOvertimePlanned: null,
                    isOnCall: null,
                    isActive: null,
                    sortBy: "scheduleDate",
                    sortDirection: "asc",
                    pageNumber: 1,
                    pageSize: Math.Max(sourceItems.Count, 31)
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceWorkSchedule.CopyWorkSchedule",
                    "Gagal copy work schedule workforce.",
                    ex,
                    new { workforceProfileId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal copy work schedule workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpPut("{workforceWorkScheduleId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Work Schedule",
            Description = "Mengubah work schedule workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceWorkSchedule", "Update")]
        public async Task<IActionResult> UpdateWorkSchedule(
            Guid workforceProfileId,
            Guid workforceWorkScheduleId,
            [FromBody] UpdateWorkforceWorkScheduleRequest request)
        {
            var validation = await ValidateWorkScheduleRequestAsync(
                workforceProfileId,
                request.WorkScheduleId,
                request.ScheduleDate,
                workforceWorkScheduleId
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var entity = await _dbContext.WfpWorkScheduleAssignments
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceWorkScheduleId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work schedule workforce tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;

            entity.WorkScheduleId = request.WorkScheduleId;
            entity.ScheduleDate = DateOnly.FromDateTime(request.ScheduleDate.Date);
            entity.IsOffDay = request.IsOffDay;
            entity.IsOvertimePlanned = request.IsOvertimePlanned;
            entity.IsOnCall = request.IsOnCall;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceWorkSchedule.UpdateWorkSchedule",
                "Work schedule workforce berhasil diubah.",
                new { entity.Id, entity.WorkforceProfileId, entity.WorkScheduleId, entity.ScheduleDate }
            );

            return await GetWorkScheduleById(workforceProfileId, entity.Id);
        }

        [HttpPut("bulk")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceWorkScheduleListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Bulk Update Workforce Work Schedule",
            Description = "Mengubah work schedule workforce secara bulk",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceWorkSchedule", "Update")]
        public async Task<IActionResult> BulkUpdateWorkSchedule(
            Guid workforceProfileId,
            [FromBody] BulkUpdateWorkforceWorkScheduleRequest request)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Items wajib diisi minimal 1 data."
                ));
            }

            var duplicateDateInRequest = request.Items
                .GroupBy(x => DateOnly.FromDateTime(x.ScheduleDate.Date))
                .FirstOrDefault(x => x.Count() > 1);

            if (duplicateDateInRequest != null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Terdapat tanggal jadwal yang duplikat di request: {duplicateDateInRequest.Key:yyyy-MM-dd}."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                var actorUserId = GetCurrentUserId();

                foreach (var item in request.Items)
                {
                    var validation = await ValidateWorkScheduleRequestAsync(
                        workforceProfileId,
                        item.WorkScheduleId,
                        item.ScheduleDate,
                        item.Id
                    );

                    if (!validation.IsValid)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            validation.Message
                        ));
                    }

                    var entity = await _dbContext.WfpWorkScheduleAssignments
                        .FirstOrDefaultAsync(x =>
                            x.Id == item.Id &&
                            x.WorkforceProfileId == workforceProfileId &&
                            !x.IsDelete);

                    if (entity == null)
                    {
                        await transaction.RollbackAsync();

                        return NotFound(ApiResponse<object>.Fail(
                            StatusCodes.Status404NotFound,
                            $"Work schedule workforce dengan Id {item.Id} tidak ditemukan."
                        ));
                    }

                    entity.WorkScheduleId = item.WorkScheduleId;
                    entity.ScheduleDate = DateOnly.FromDateTime(item.ScheduleDate.Date);
                    entity.IsOffDay = item.IsOffDay;
                    entity.IsOvertimePlanned = item.IsOvertimePlanned;
                    entity.IsOnCall = item.IsOnCall;
                    entity.Description = NormalizeNullableText(item.Description);
                    entity.IsActive = item.IsActive;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = actorUserId;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceWorkSchedule.BulkUpdateWorkSchedule",
                    "Bulk work schedule workforce berhasil diubah.",
                    new { workforceProfileId, TotalData = request.Items.Count }
                );

                var firstDate = request.Items.Min(x => x.ScheduleDate.Date);
                var lastDate = request.Items.Max(x => x.ScheduleDate.Date);

                return await GetWorkSchedules(
                    workforceProfileId,
                    search: null,
                    startDate: firstDate,
                    endDate: lastDate,
                    workScheduleId: null,
                    scheduleType: null,
                    isOffDay: null,
                    isOvertimePlanned: null,
                    isOnCall: null,
                    isActive: null,
                    sortBy: "scheduleDate",
                    sortDirection: "asc",
                    pageNumber: 1,
                    pageSize: Math.Max(request.Items.Count, 31)
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceWorkSchedule.BulkUpdateWorkSchedule",
                    "Gagal mengubah bulk work schedule workforce.",
                    ex,
                    new { workforceProfileId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal mengubah bulk work schedule workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpDelete("{workforceWorkScheduleId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Work Schedule",
            Description = "Menghapus work schedule workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceWorkSchedule", "Delete")]
        public async Task<IActionResult> DeleteWorkSchedule(
            Guid workforceProfileId,
            Guid workforceWorkScheduleId,
            [FromBody] DeleteWorkforceWorkScheduleRequest? request = null)
        {
            var entity = await _dbContext.WfpWorkScheduleAssignments
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceWorkScheduleId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work schedule workforce tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.Description = NormalizeNullableText(request?.DeleteReason) ?? entity.Description;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceWorkSchedule.DeleteWorkSchedule",
                "Work schedule workforce berhasil dihapus.",
                new { entity.Id, entity.WorkforceProfileId, entity.ScheduleDate }
            );

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Work schedule workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpWorkScheduleAssignment> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.WfpWorkScheduleAssignments
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.WorkSchedule)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpWorkScheduleAssignment> ApplyFilters(
            IQueryable<WfpWorkScheduleAssignment> query,
            string? search,
            DateTime? startDate,
            DateTime? endDate,
            Guid? workScheduleId,
            string? scheduleType,
            bool? isOffDay,
            bool? isOvertimePlanned,
            bool? isOnCall,
            bool? isActive)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.WorkSchedule != null && x.WorkSchedule.ScheduleCode.ToLower().Contains(keyword)) ||
                    (x.WorkSchedule != null && x.WorkSchedule.ScheduleName.ToLower().Contains(keyword)) ||
                    (x.WorkSchedule != null && x.WorkSchedule.ScheduleType.ToLower().Contains(keyword)));
            }

            if (startDate.HasValue)
            {
                var start = DateOnly.FromDateTime(startDate.Value.Date);
                query = query.Where(x => x.ScheduleDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateOnly.FromDateTime(endDate.Value.Date);
                query = query.Where(x => x.ScheduleDate <= end);
            }

            if (workScheduleId.HasValue && workScheduleId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkScheduleId == workScheduleId.Value);
            }

            if (!string.IsNullOrWhiteSpace(scheduleType))
            {
                var selectedScheduleType = scheduleType.Trim().ToLower();
                query = query.Where(x =>
                    x.WorkSchedule != null &&
                    x.WorkSchedule.ScheduleType.ToLower() == selectedScheduleType);
            }

            if (isOffDay.HasValue)
            {
                query = query.Where(x => x.IsOffDay == isOffDay.Value);
            }

            if (isOvertimePlanned.HasValue)
            {
                query = query.Where(x => x.IsOvertimePlanned == isOvertimePlanned.Value);
            }

            if (isOnCall.HasValue)
            {
                query = query.Where(x => x.IsOnCall == isOnCall.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            return query;
        }

        private static IQueryable<WfpWorkScheduleAssignment> ApplySorting(
            IQueryable<WfpWorkScheduleAssignment> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "scheduleDate").Trim().ToLower() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "workschedulecode" => isDescending
                    ? query.OrderByDescending(x => x.WorkSchedule != null ? x.WorkSchedule.ScheduleCode : string.Empty)
                    : query.OrderBy(x => x.WorkSchedule != null ? x.WorkSchedule.ScheduleCode : string.Empty),

                "workschedulename" => isDescending
                    ? query.OrderByDescending(x => x.WorkSchedule != null ? x.WorkSchedule.ScheduleName : string.Empty)
                    : query.OrderBy(x => x.WorkSchedule != null ? x.WorkSchedule.ScheduleName : string.Empty),

                "scheduletype" => isDescending
                    ? query.OrderByDescending(x => x.WorkSchedule != null ? x.WorkSchedule.ScheduleType : string.Empty)
                    : query.OrderBy(x => x.WorkSchedule != null ? x.WorkSchedule.ScheduleType : string.Empty),

                "workstarttime" => isDescending
                    ? query.OrderByDescending(x => x.WorkSchedule != null ? x.WorkSchedule.WorkStartTime : TimeOnly.MinValue)
                    : query.OrderBy(x => x.WorkSchedule != null ? x.WorkSchedule.WorkStartTime : TimeOnly.MinValue),

                "workendtime" => isDescending
                    ? query.OrderByDescending(x => x.WorkSchedule != null ? x.WorkSchedule.WorkEndTime : TimeOnly.MinValue)
                    : query.OrderBy(x => x.WorkSchedule != null ? x.WorkSchedule.WorkEndTime : TimeOnly.MinValue),

                "isoffday" => isDescending
                    ? query.OrderByDescending(x => x.IsOffDay).ThenBy(x => x.ScheduleDate)
                    : query.OrderBy(x => x.IsOffDay).ThenBy(x => x.ScheduleDate),

                "isovertimeplanned" => isDescending
                    ? query.OrderByDescending(x => x.IsOvertimePlanned).ThenBy(x => x.ScheduleDate)
                    : query.OrderBy(x => x.IsOvertimePlanned).ThenBy(x => x.ScheduleDate),

                "isoncall" => isDescending
                    ? query.OrderByDescending(x => x.IsOnCall).ThenBy(x => x.ScheduleDate)
                    : query.OrderBy(x => x.IsOnCall).ThenBy(x => x.ScheduleDate),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ScheduleDate)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.ScheduleDate),

                _ => isDescending
                    ? query.OrderByDescending(x => x.ScheduleDate)
                        .ThenByDescending(x => x.WorkSchedule != null ? x.WorkSchedule.WorkStartTime : TimeOnly.MinValue)
                    : query.OrderBy(x => x.ScheduleDate)
                        .ThenBy(x => x.WorkSchedule != null ? x.WorkSchedule.WorkStartTime : TimeOnly.MinValue)
            };
        }

        private async Task<(bool IsValid, string Message)> ValidateWorkScheduleRequestAsync(
            Guid workforceProfileId,
            Guid workScheduleId,
            DateTime scheduleDate,
            Guid? existingId)
        {
            var profileExists = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return (false, "Workforce profile tidak ditemukan.");
            }

            var scheduleValidation = await ValidateWorkScheduleMasterAsync(workScheduleId);

            if (!scheduleValidation.IsValid)
            {
                return scheduleValidation;
            }

            if (scheduleDate == default)
            {
                return (false, "ScheduleDate wajib diisi.");
            }

            var selectedDate = DateOnly.FromDateTime(scheduleDate.Date);

            var duplicateExists = await _dbContext.WfpWorkScheduleAssignments
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.ScheduleDate == selectedDate &&
                    !x.IsDelete &&
                    (!existingId.HasValue || x.Id != existingId.Value));

            if (duplicateExists)
            {
                return (false, $"Work schedule workforce untuk tanggal {selectedDate:yyyy-MM-dd} sudah ada.");
            }

            return (true, string.Empty);
        }

        private async Task<(bool IsValid, string Message)> ValidateWorkScheduleMasterAsync(Guid workScheduleId)
        {
            if (workScheduleId == Guid.Empty)
            {
                return (false, "WorkScheduleId wajib diisi.");
            }

            var scheduleExists = await _dbContext.MstWorkSchedules
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == workScheduleId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!scheduleExists)
            {
                return (false, "Work schedule master tidak ditemukan atau tidak aktif.");
            }

            return (true, string.Empty);
        }

        private static (bool IsValid, string Message) ValidateDateRange(
            DateTime startDate,
            DateTime endDate,
            int maxDays)
        {
            if (startDate == default)
            {
                return (false, "StartDate wajib diisi.");
            }

            if (endDate == default)
            {
                return (false, "EndDate wajib diisi.");
            }

            var start = startDate.Date;
            var end = endDate.Date;

            if (end < start)
            {
                return (false, "EndDate tidak boleh lebih kecil dari StartDate.");
            }

            var totalDays = (end - start).TotalDays + 1;

            if (totalDays > maxDays)
            {
                return (false, $"Rentang tanggal maksimal {maxDays} hari.");
            }

            return (true, string.Empty);
        }

        private async Task<(bool IsSuccess, string Message)> CreateOrReplaceScheduleAsync(
            Guid workforceProfileId,
            Guid workScheduleId,
            DateOnly scheduleDate,
            bool isOffDay,
            bool isOvertimePlanned,
            bool isOnCall,
            string? description,
            bool isActive,
            bool replaceExistingSameDate,
            DateTime now,
            Guid actorUserId)
        {
            var existing = await _dbContext.WfpWorkScheduleAssignments
                .FirstOrDefaultAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.ScheduleDate == scheduleDate &&
                    !x.IsDelete);

            if (existing != null && !replaceExistingSameDate)
            {
                return (false, $"Work schedule workforce untuk tanggal {scheduleDate:yyyy-MM-dd} sudah ada.");
            }

            if (existing != null)
            {
                existing.WorkScheduleId = workScheduleId;
                existing.IsOffDay = isOffDay;
                existing.IsOvertimePlanned = isOvertimePlanned;
                existing.IsOnCall = isOnCall;
                existing.Description = NormalizeNullableText(description);
                existing.IsActive = isActive;
                existing.UpdateDateTime = now;
                existing.UpdateBy = actorUserId;

                return (true, string.Empty);
            }

            var entity = new WfpWorkScheduleAssignment
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                WorkScheduleId = workScheduleId,
                ScheduleDate = scheduleDate,
                IsOffDay = isOffDay,
                IsOvertimePlanned = isOvertimePlanned,
                IsOnCall = isOnCall,
                Description = NormalizeNullableText(description),
                IsActive = isActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.WfpWorkScheduleAssignments.Add(entity);

            return (true, string.Empty);
        }

        private static List<DateOnly> BuildGenerateDates(GenerateWorkforceWorkScheduleRequest request)
        {
            var result = new List<DateOnly>();

            var start = DateOnly.FromDateTime(request.StartDate.Date);
            var end = DateOnly.FromDateTime(request.EndDate.Date);

            for (var current = start; current <= end; current = current.AddDays(1))
            {
                if (ShouldApplyDay(current, request))
                {
                    result.Add(current);
                }
            }

            return result;
        }

        private static bool ShouldApplyDay(
            DateOnly date,
            GenerateWorkforceWorkScheduleRequest request)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => request.ApplyMonday,
                DayOfWeek.Tuesday => request.ApplyTuesday,
                DayOfWeek.Wednesday => request.ApplyWednesday,
                DayOfWeek.Thursday => request.ApplyThursday,
                DayOfWeek.Friday => request.ApplyFriday,
                DayOfWeek.Saturday => request.ApplySaturday,
                DayOfWeek.Sunday => request.ApplySunday,
                _ => false
            };
        }

        private static WorkforceWorkScheduleResponse MapWorkScheduleResponse(
            WfpWorkScheduleAssignment x,
            WorkforceProfileHeader profile)
        {
            var scheduledTimes = BuildScheduledDateTimes(x);

            return new WorkforceWorkScheduleResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                WorkScheduleId = x.WorkScheduleId,
                WorkScheduleCode = x.WorkSchedule?.ScheduleCode ?? string.Empty,
                WorkScheduleName = x.WorkSchedule?.ScheduleName ?? string.Empty,
                ScheduleType = x.WorkSchedule?.ScheduleType ?? string.Empty,
                ScheduleDate = x.ScheduleDate.ToDateTime(TimeOnly.MinValue),
                WorkStartTime = x.WorkSchedule?.WorkStartTime.ToString("HH:mm:ss") ?? string.Empty,
                WorkEndTime = x.WorkSchedule?.WorkEndTime.ToString("HH:mm:ss") ?? string.Empty,
                IsOvernight = x.WorkSchedule?.IsOvernight ?? false,
                CheckInToleranceMinutes = x.WorkSchedule?.CheckInToleranceMinutes ?? 0,
                CheckOutToleranceMinutes = x.WorkSchedule?.CheckOutToleranceMinutes ?? 0,
                ScheduledCheckInAt = scheduledTimes.ScheduledCheckInAt,
                ScheduledCheckOutAt = scheduledTimes.ScheduledCheckOutAt,
                IsOffDay = x.IsOffDay,
                IsOvertimePlanned = x.IsOvertimePlanned,
                IsOnCall = x.IsOnCall,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static WorkforceWorkScheduleDetailResponse MapWorkScheduleDetailResponse(
            WfpWorkScheduleAssignment x,
            WorkforceProfileHeader profile)
        {
            var scheduledTimes = BuildScheduledDateTimes(x);

            return new WorkforceWorkScheduleDetailResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                WorkScheduleId = x.WorkScheduleId,
                WorkScheduleCode = x.WorkSchedule?.ScheduleCode ?? string.Empty,
                WorkScheduleName = x.WorkSchedule?.ScheduleName ?? string.Empty,
                ScheduleType = x.WorkSchedule?.ScheduleType ?? string.Empty,
                ScheduleDate = x.ScheduleDate.ToDateTime(TimeOnly.MinValue),
                WorkStartTime = x.WorkSchedule?.WorkStartTime.ToString("HH:mm:ss") ?? string.Empty,
                WorkEndTime = x.WorkSchedule?.WorkEndTime.ToString("HH:mm:ss") ?? string.Empty,
                IsOvernight = x.WorkSchedule?.IsOvernight ?? false,
                CheckInToleranceMinutes = x.WorkSchedule?.CheckInToleranceMinutes ?? 0,
                CheckOutToleranceMinutes = x.WorkSchedule?.CheckOutToleranceMinutes ?? 0,
                ScheduledCheckInAt = scheduledTimes.ScheduledCheckInAt,
                ScheduledCheckOutAt = scheduledTimes.ScheduledCheckOutAt,
                IsOffDay = x.IsOffDay,
                IsOvertimePlanned = x.IsOvertimePlanned,
                IsOnCall = x.IsOnCall,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                CreateBy = x.CreateBy,
                UpdateBy = x.UpdateBy
            };
        }

        private static (DateTime? ScheduledCheckInAt, DateTime? ScheduledCheckOutAt) BuildScheduledDateTimes(
            WfpWorkScheduleAssignment x)
        {
            if (x.WorkSchedule == null)
            {
                return (null, null);
            }

            var scheduledCheckInAt = x.ScheduleDate.ToDateTime(x.WorkSchedule.WorkStartTime);
            var scheduledCheckOutAt = x.ScheduleDate.ToDateTime(x.WorkSchedule.WorkEndTime);

            if (x.WorkSchedule.IsOvernight || x.WorkSchedule.WorkEndTime <= x.WorkSchedule.WorkStartTime)
            {
                scheduledCheckOutAt = scheduledCheckOutAt.AddDays(1);
            }

            return (scheduledCheckInAt, scheduledCheckOutAt);
        }

        private async Task<WorkforceProfileHeader?> GetWorkforceProfileHeaderAsync(Guid workforceProfileId)
        {
            return await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => new WorkforceProfileHeader
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName,
                    UserType = x.UserType
                })
                .FirstOrDefaultAsync();
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

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 31;
            }

            if (pageSize > 200)
            {
                pageSize = 200;
            }

            return (pageNumber, pageSize);
        }

        private class WorkforceProfileHeader
        {
            public Guid Id { get; set; }

            public string ProfileCode { get; set; } = string.Empty;

            public string DisplayName { get; set; } = string.Empty;

            public UserType UserType { get; set; }
        }
    }
}
