using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseDoctorSchedulePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DoctorScheduleResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/doctor-schedules")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Doctor Schedule",
        AreaName = "HealthServices",
        ControllerName = "DoctorSchedule",
        Description = "Health service master data doctor schedule",
        SortOrder = 6
    )]
    [Tags("Health Services / Master Data / Doctor Schedule")]
    public class DoctorScheduleController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string KioskReadPolicy = "KioskRead";
        private const string ScheduleCodePrefix = "DSCH-RSMMC-";
        private const string DefaultDoctorProfilePhotoPathFallback = "/uploads/default-profile-photos/dokter.png";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IConfiguration _configuration;

        public DoctorScheduleController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _configuration = configuration;
        }

        [HttpGet("filters/metadata")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Schedule", Description = "Melihat data doctor schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DoctorScheduleFilterMetadataResponse
            {
                DefaultFilter = new DoctorScheduleDefaultFilterResponse(),
                CustomPeriods = new List<DoctorScheduleCustomPeriodResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "last30days", Label = "30 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "thisyear", Label = "Tahun ini" },
                    new() { Value = "all", Label = "Semua periode" }
                },
                SortOptions = new List<DoctorScheduleSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "scheduleCode", Label = "Kode jadwal" },
                    new() { Value = "scheduleName", Label = "Nama jadwal" },
                    new() { Value = "doctorName", Label = "Nama dokter" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "clinicName", Label = "Nama clinic" },
                    new() { Value = "practiceDay", Label = "Hari praktik" },
                    new() { Value = "practiceDate", Label = "Tanggal praktik" },
                    new() { Value = "startTime", Label = "Jam mulai" },
                    new() { Value = "endTime", Label = "Jam selesai" },
                    new() { Value = "scheduleType", Label = "Tipe jadwal" },
                    new() { Value = "scheduleStatus", Label = "Status jadwal" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ScheduleTypeOptions = BuildEnumOptions<DoctorScheduleType>(),
                ScheduleStatusOptions = BuildEnumOptions<DoctorScheduleStatus>(),
                PracticeDayOptions = BuildEnumOptions<DayOfWeek>(),
                QueryParameters = BuildQueryParameters(),
                CreateFields = BuildCreateFields(),
                UpdateFields = BuildUpdateFields()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorSchedule.GetFilterMetadata",
                "Mengambil metadata filter doctor schedule.",
                result
            );

            return Ok(ApiResponse<DoctorScheduleFilterMetadataResponse>.Ok(
                result,
                "Metadata filter doctor schedule berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Schedule", Description = "Melihat data doctor schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorSchedule", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DoctorScheduleSummaryResponse
            {
                TotalSchedule = await query.CountAsync(),
                ActiveSchedule = await query.CountAsync(x => x.IsActive),
                InactiveSchedule = await query.CountAsync(x => !x.IsActive),
                AppointmentAvailableSchedule = await query.CountAsync(x => x.IsAllowAppointment),
                WalkInAvailableSchedule = await query.CountAsync(x => x.IsAllowWalkIn),
                KioskAvailableSchedule = await query.CountAsync(x => x.IsAllowKioskRegistration),
                TelemedicineAvailableSchedule = await query.CountAsync(x => x.IsTelemedicineAvailable),
                SubstituteSchedule = await query.CountAsync(x => x.IsSubstituteSchedule),
                SuspendedSchedule = await query.CountAsync(x => x.ScheduleStatus == DoctorScheduleStatus.Suspended),
                ClosedSchedule = await query.CountAsync(x => x.ScheduleStatus == DoctorScheduleStatus.Closed)
            };

            return Ok(ApiResponse<DoctorScheduleSummaryResponse>.Ok(
                result,
                "Ringkasan doctor schedule berhasil diambil."
            ));
        }

        [HttpGet]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponseDoctorSchedulePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Schedule", Description = "Melihat data doctor schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetDoctorSchedules(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? clinicId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyFilters(query, doctorId, clinicId, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(ToResponse)
                .ToList();

            await EnrichDoctorSchedulePhotoFieldsAsync(items);

            var result = new ResponseDoctorSchedulePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDoctorSchedulePagedResult>.Ok(
                result,
                "Data doctor schedule berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Schedule", Description = "Melihat data pilihan doctor schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetDoctorScheduleOptions(
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? clinicId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            query = ApplyFilters(query, doctorId, clinicId, null, search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Doctor != null ? x.Doctor.FullName : string.Empty)
                .ThenBy(x => x.PracticeDay)
                .ThenBy(x => x.StartTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DoctorScheduleOptionResponse
                {
                    Id = x.Id,

                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                    SpecialistName = x.Doctor != null ? x.Doctor.SpecialistName : null,

                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,

                    ClinicId = x.ClinicId,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : string.Empty,

                    RoomId = x.RoomId,
                    RoomMasterName = x.Room != null ? x.Room.RoomName : null,

                    ScheduleCode = x.ScheduleCode,
                    ScheduleName = x.ScheduleName,
                    ScheduleType = x.ScheduleType,
                    ScheduleStatus = x.ScheduleStatus,

                    PracticeDay = x.PracticeDay,
                    PracticeDate = x.PracticeDate,

                    StartTime = x.StartTime,
                    EndTime = x.EndTime,

                    SessionName = x.SessionName,

                    MaxPatientQuota = x.MaxPatientQuota,
                    MaxAppointmentQuota = x.MaxAppointmentQuota,
                    MaxWalkInQuota = x.MaxWalkInQuota,
                    EstimatedServiceMinutes = x.EstimatedServiceMinutes,

                    IsAllowWalkIn = x.IsAllowWalkIn,
                    IsAllowAppointment = x.IsAllowAppointment,
                    IsAllowKioskRegistration = x.IsAllowKioskRegistration,
                    IsTelemedicineAvailable = x.IsTelemedicineAvailable
                })
                .ToListAsync();

            await EnrichDoctorScheduleOptionPhotoFieldsAsync(items);

            var result = new DoctorScheduleOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DoctorScheduleOptionPagedResponse>.Ok(
                result,
                "Data pilihan doctor schedule berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Doctor Schedule", Description = "Melihat detail doctor schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorSchedule", "Read")]
        public async Task<IActionResult> GetDoctorScheduleById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor schedule tidak ditemukan."
                ));
            }

            var data = ToDetailResponse(entity);
            await EnrichDoctorSchedulePhotoFieldAsync(data);

            return Ok(ApiResponse<DoctorScheduleDetailResponse>.Ok(
                data,
                "Detail doctor schedule berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Doctor Schedule", Description = "Membuat data doctor schedule", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DoctorSchedule", "Create")]
        public async Task<IActionResult> CreateDoctorSchedule([FromBody] CreateDoctorScheduleRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data doctor schedule tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var scheduleCode = await GenerateNextScheduleCodeAsync();

            var entity = new MstDoctorSchedule
            {
                Id = Guid.NewGuid(),
                ScheduleCode = scheduleCode,
                ScheduleName = request.ScheduleName.Trim(),
                ScheduleType = request.ScheduleType,
                DoctorId = request.DoctorId,
                ServiceUnitId = request.ServiceUnitId,
                ClinicId = request.ClinicId,
                RoomId = NormalizeNullableGuid(request.RoomId),
                PracticeDay = request.PracticeDay,
                PracticeDate = request.PracticeDate?.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsOvernight = request.IsOvernight,
                SessionName = NormalizeNullableText(request.SessionName),
                PracticeLocation = NormalizeNullableText(request.PracticeLocation),
                RoomName = NormalizeNullableText(request.RoomName),
                MaxPatientQuota = request.MaxPatientQuota,
                MaxAppointmentQuota = request.MaxAppointmentQuota,
                MaxWalkInQuota = request.MaxWalkInQuota,
                EstimatedServiceMinutes = request.EstimatedServiceMinutes,
                IsAllowWalkIn = request.IsAllowWalkIn,
                IsAllowAppointment = request.IsAllowAppointment,
                IsAllowKioskRegistration = request.IsAllowKioskRegistration,
                IsTelemedicineAvailable = request.IsTelemedicineAvailable,
                IsSubstituteSchedule = request.IsSubstituteSchedule,
                SubstituteDoctorId = NormalizeNullableGuid(request.SubstituteDoctorId),
                ScheduleStatus = request.ScheduleStatus,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDoctorSchedule>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new DoctorScheduleCreateResponse
            {
                Id = entity.Id,
                ScheduleCode = entity.ScheduleCode,
                ScheduleName = entity.ScheduleName,
                ScheduleType = entity.ScheduleType,
                ScheduleStatus = entity.ScheduleStatus,
                DoctorId = entity.DoctorId,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                RoomId = entity.RoomId,
                PracticeDay = entity.PracticeDay,
                PracticeDate = entity.PracticeDate,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorSchedule.CreateDoctorSchedule",
                "Membuat data doctor schedule.",
                response
            );

            return Ok(ApiResponse<DoctorScheduleCreateResponse>.Ok(
                response,
                "Doctor schedule berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Doctor Schedule", Description = "Mengubah data doctor schedule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorSchedule", "Update")]
        public async Task<IActionResult> UpdateDoctorSchedule(Guid id, [FromBody] UpdateDoctorScheduleRequest request)
        {
            var entity = await _dbContext.Set<MstDoctorSchedule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor schedule tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data doctor schedule tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ScheduleName = request.ScheduleName.Trim();
            entity.ScheduleType = request.ScheduleType;
            entity.DoctorId = request.DoctorId;
            entity.ServiceUnitId = request.ServiceUnitId;
            entity.ClinicId = request.ClinicId;
            entity.RoomId = NormalizeNullableGuid(request.RoomId);
            entity.PracticeDay = request.PracticeDay;
            entity.PracticeDate = request.PracticeDate?.Date;
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.IsOvernight = request.IsOvernight;
            entity.SessionName = NormalizeNullableText(request.SessionName);
            entity.PracticeLocation = NormalizeNullableText(request.PracticeLocation);
            entity.RoomName = NormalizeNullableText(request.RoomName);
            entity.MaxPatientQuota = request.MaxPatientQuota;
            entity.MaxAppointmentQuota = request.MaxAppointmentQuota;
            entity.MaxWalkInQuota = request.MaxWalkInQuota;
            entity.EstimatedServiceMinutes = request.EstimatedServiceMinutes;
            entity.IsAllowWalkIn = request.IsAllowWalkIn;
            entity.IsAllowAppointment = request.IsAllowAppointment;
            entity.IsAllowKioskRegistration = request.IsAllowKioskRegistration;
            entity.IsTelemedicineAvailable = request.IsTelemedicineAvailable;
            entity.IsSubstituteSchedule = request.IsSubstituteSchedule;
            entity.SubstituteDoctorId = NormalizeNullableGuid(request.SubstituteDoctorId);
            entity.ScheduleStatus = request.ScheduleStatus;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = new DoctorScheduleUpdateResponse
            {
                Id = entity.Id,
                ScheduleCode = entity.ScheduleCode,
                ScheduleName = entity.ScheduleName,
                ScheduleStatus = entity.ScheduleStatus,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorSchedule.UpdateDoctorSchedule",
                "Mengubah data doctor schedule.",
                response
            );

            return Ok(ApiResponse<DoctorScheduleUpdateResponse>.Ok(
                response,
                "Doctor schedule berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Doctor Schedule", Description = "Mengaktifkan data doctor schedule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorSchedule", "Update")]
        public async Task<IActionResult> ActivateDoctorSchedule(Guid id)
        {
            return await SetActiveStatusAsync(id, true, "Doctor schedule berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Doctor Schedule", Description = "Menonaktifkan data doctor schedule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorSchedule", "Update")]
        public async Task<IActionResult> DeactivateDoctorSchedule(Guid id)
        {
            return await SetActiveStatusAsync(id, false, "Doctor schedule berhasil dinonaktifkan.");
        }



        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Doctor Schedule", Description = "Mengubah status doctor schedule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorSchedule", "Update")]
        public async Task<IActionResult> UpdateDoctorScheduleStatus(Guid id, [FromBody] UpdateDoctorScheduleStatusRequest request)
        {
            return await SetActiveStatusAsync(
                id,
                request.IsActive,
                request.IsActive
                    ? "Doctor schedule berhasil diaktifkan."
                    : "Doctor schedule berhasil dinonaktifkan."
            );
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Doctor Schedule", Description = "Menghapus data doctor schedule", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DoctorSchedule", "Delete")]
        public async Task<IActionResult> DeleteDoctorSchedule(Guid id, [FromBody] DeleteDoctorScheduleRequest? deleteRequest = null)
        {
            var entity = await _dbContext.Set<MstDoctorSchedule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor schedule tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(deleteRequest?.DeleteReason))
                entity.Description = NormalizeNullableText(deleteRequest.DeleteReason);

            await _dbContext.SaveChangesAsync();

            var response = new DoctorScheduleDeleteResponse
            {
                Id = entity.Id,
                ScheduleCode = entity.ScheduleCode,
                ScheduleName = entity.ScheduleName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorSchedule.DeleteDoctorSchedule",
                "Menghapus data doctor schedule.",
                response
            );

            return Ok(ApiResponse<DoctorScheduleDeleteResponse>.Ok(
                response,
                "Doctor schedule berhasil dihapus."
            ));
        }

        private async Task<IActionResult> SetActiveStatusAsync(Guid id, bool isActive, string successMessage)
        {
            var entity = await _dbContext.Set<MstDoctorSchedule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor schedule tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = isActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = new DoctorScheduleStatusResponse
            {
                Id = entity.Id,
                ScheduleCode = entity.ScheduleCode,
                ScheduleName = entity.ScheduleName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            return Ok(ApiResponse<DoctorScheduleStatusResponse>.Ok(
                response,
                successMessage
            ));
        }

        private IQueryable<MstDoctorSchedule> BuildBaseQuery()
        {
            return _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Include(x => x.Doctor)
                .Include(x => x.SubstituteDoctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Room)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDoctorSchedule> ApplyDateFilter(
            IQueryable<MstDoctorSchedule> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = AppDateTimeHelper.OperationalDate();

            if (!string.IsNullOrWhiteSpace(customPeriod))
            {
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        startDate = today;
                        endDate = today;
                        break;
                    case "last7days":
                        startDate = today.AddDays(-6);
                        endDate = today;
                        break;
                    case "last30days":
                        startDate = today.AddDays(-29);
                        endDate = today;
                        break;
                    case "thismonth":
                        startDate = new DateTime(today.Year, today.Month, 1);
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                        break;
                    case "thisyear":
                        startDate = new DateTime(today.Year, 1, 1);
                        endDate = new DateTime(today.Year, 12, 31);
                        break;
                    case "all":
                        startDate = null;
                        endDate = null;
                        break;
                }
            }

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.CreateDateTime.Date >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date;
                query = query.Where(x => x.CreateDateTime.Date <= end);
            }

            return query;
        }

        private static IQueryable<MstDoctorSchedule> ApplyFilters(
            IQueryable<MstDoctorSchedule> query,
            Guid? doctorId,
            Guid? clinicId,
            bool? isActive,
            string? search)
        {
            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ScheduleCode.ToLower().Contains(keyword) ||
                    x.ScheduleName.ToLower().Contains(keyword) ||
                    (x.SessionName != null && x.SessionName.ToLower().Contains(keyword)) ||
                    (x.PracticeLocation != null && x.PracticeLocation.ToLower().Contains(keyword)) ||
                    (x.RoomName != null && x.RoomName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.DoctorCode.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.DoctorNumber.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.SpecialistName != null && x.Doctor.SpecialistName.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicCode.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomCode.ToLower().Contains(keyword)) ||
                    (x.Room != null && x.Room.RoomName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstDoctorSchedule> ApplySorting(
            IQueryable<MstDoctorSchedule> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "schedulecode" => isDesc
                    ? query.OrderByDescending(x => x.ScheduleCode)
                    : query.OrderBy(x => x.ScheduleCode),

                "schedulename" => isDesc
                    ? query.OrderByDescending(x => x.ScheduleName)
                    : query.OrderBy(x => x.ScheduleName),

                "doctorname" => isDesc
                    ? query.OrderByDescending(x => x.Doctor != null ? x.Doctor.FullName : string.Empty)
                    : query.OrderBy(x => x.Doctor != null ? x.Doctor.FullName : string.Empty),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty)
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty),

                "clinicname" => isDesc
                    ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty)
                    : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty),

                "practiceday" => isDesc
                    ? query.OrderByDescending(x => x.PracticeDay)
                    : query.OrderBy(x => x.PracticeDay),

                "practicedate" => isDesc
                    ? query.OrderByDescending(x => x.PracticeDate)
                    : query.OrderBy(x => x.PracticeDate),

                "starttime" => isDesc
                    ? query.OrderByDescending(x => x.StartTime)
                    : query.OrderBy(x => x.StartTime),

                "endtime" => isDesc
                    ? query.OrderByDescending(x => x.EndTime)
                    : query.OrderBy(x => x.EndTime),

                "scheduletype" => isDesc
                    ? query.OrderByDescending(x => x.ScheduleType)
                    : query.OrderBy(x => x.ScheduleType),

                "schedulestatus" => isDesc
                    ? query.OrderByDescending(x => x.ScheduleStatus)
                    : query.OrderBy(x => x.ScheduleStatus),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.Doctor != null ? x.Doctor.FullName : string.Empty)
                        .ThenByDescending(x => x.PracticeDay)
                        .ThenByDescending(x => x.StartTime)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Doctor != null ? x.Doctor.FullName : string.Empty)
                        .ThenBy(x => x.PracticeDay)
                        .ThenBy(x => x.StartTime)
            };
        }

        private static DoctorScheduleResponse ToResponse(MstDoctorSchedule x)
        {
            return new DoctorScheduleResponse
            {
                Id = x.Id,
                ScheduleCode = x.ScheduleCode,
                ScheduleName = x.ScheduleName,
                ScheduleType = x.ScheduleType,
                ScheduleStatus = x.ScheduleStatus,
                DoctorId = x.DoctorId,
                DoctorCode = x.Doctor != null ? x.Doctor.DoctorCode : string.Empty,
                DoctorNumber = x.Doctor != null ? x.Doctor.DoctorNumber : string.Empty,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                SpecialistName = x.Doctor != null ? x.Doctor.SpecialistName : null,
                SubSpecialistName = x.Doctor != null ? x.Doctor.SubSpecialistName : null,
                MedicalStaffGroup = x.Doctor != null ? x.Doctor.MedicalStaffGroup : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : string.Empty,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : string.Empty,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : string.Empty,
                RoomId = x.RoomId,
                RoomCode = x.Room != null ? x.Room.RoomCode : null,
                RoomMasterName = x.Room != null ? x.Room.RoomName : null,
                PracticeDay = x.PracticeDay,
                PracticeDate = x.PracticeDate,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                IsOvernight = x.IsOvernight,
                SessionName = x.SessionName,
                PracticeLocation = x.PracticeLocation,
                RoomName = x.RoomName,
                MaxPatientQuota = x.MaxPatientQuota,
                MaxAppointmentQuota = x.MaxAppointmentQuota,
                MaxWalkInQuota = x.MaxWalkInQuota,
                EstimatedServiceMinutes = x.EstimatedServiceMinutes,
                IsAllowWalkIn = x.IsAllowWalkIn,
                IsAllowAppointment = x.IsAllowAppointment,
                IsAllowKioskRegistration = x.IsAllowKioskRegistration,
                IsTelemedicineAvailable = x.IsTelemedicineAvailable,
                IsSubstituteSchedule = x.IsSubstituteSchedule,
                SubstituteDoctorId = x.SubstituteDoctorId,
                SubstituteDoctorCode = x.SubstituteDoctor != null ? x.SubstituteDoctor.DoctorCode : null,
                SubstituteDoctorName = x.SubstituteDoctor != null ? x.SubstituteDoctor.FullName : null,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = null
            };
        }

        private static DoctorScheduleDetailResponse ToDetailResponse(MstDoctorSchedule x)
        {
            var response = new DoctorScheduleDetailResponse
            {
                Description = x.Description
            };

            response.Id = x.Id;
            response.ScheduleCode = x.ScheduleCode;
            response.ScheduleName = x.ScheduleName;
            response.ScheduleType = x.ScheduleType;
            response.ScheduleStatus = x.ScheduleStatus;
            response.DoctorId = x.DoctorId;
            response.DoctorCode = x.Doctor != null ? x.Doctor.DoctorCode : string.Empty;
            response.DoctorNumber = x.Doctor != null ? x.Doctor.DoctorNumber : string.Empty;
            response.DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty;
            response.SpecialistName = x.Doctor != null ? x.Doctor.SpecialistName : null;
            response.SubSpecialistName = x.Doctor != null ? x.Doctor.SubSpecialistName : null;
            response.MedicalStaffGroup = x.Doctor != null ? x.Doctor.MedicalStaffGroup : null;
            response.ServiceUnitId = x.ServiceUnitId;
            response.ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : string.Empty;
            response.ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty;
            response.ClinicId = x.ClinicId;
            response.ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : string.Empty;
            response.ClinicName = x.Clinic != null ? x.Clinic.ClinicName : string.Empty;
            response.RoomId = x.RoomId;
            response.RoomCode = x.Room != null ? x.Room.RoomCode : null;
            response.RoomMasterName = x.Room != null ? x.Room.RoomName : null;
            response.PracticeDay = x.PracticeDay;
            response.PracticeDate = x.PracticeDate;
            response.StartTime = x.StartTime;
            response.EndTime = x.EndTime;
            response.IsOvernight = x.IsOvernight;
            response.SessionName = x.SessionName;
            response.PracticeLocation = x.PracticeLocation;
            response.RoomName = x.RoomName;
            response.MaxPatientQuota = x.MaxPatientQuota;
            response.MaxAppointmentQuota = x.MaxAppointmentQuota;
            response.MaxWalkInQuota = x.MaxWalkInQuota;
            response.EstimatedServiceMinutes = x.EstimatedServiceMinutes;
            response.IsAllowWalkIn = x.IsAllowWalkIn;
            response.IsAllowAppointment = x.IsAllowAppointment;
            response.IsAllowKioskRegistration = x.IsAllowKioskRegistration;
            response.IsTelemedicineAvailable = x.IsTelemedicineAvailable;
            response.IsSubstituteSchedule = x.IsSubstituteSchedule;
            response.SubstituteDoctorId = x.SubstituteDoctorId;
            response.SubstituteDoctorCode = x.SubstituteDoctor != null ? x.SubstituteDoctor.DoctorCode : null;
            response.SubstituteDoctorName = x.SubstituteDoctor != null ? x.SubstituteDoctor.FullName : null;
            response.EffectiveStartDate = x.EffectiveStartDate;
            response.EffectiveEndDate = x.EffectiveEndDate;
            response.SortOrder = x.SortOrder;
            response.IsActive = x.IsActive;
            response.CreateDateTime = x.CreateDateTime;
            response.CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy;
            response.CreateByName = null;
            response.UpdateDateTime = x.UpdateDateTime;
            response.UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy;
            response.UpdateByName = null;

            return response;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateDoctorScheduleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ScheduleName))
                return (false, "Nama jadwal dokter wajib diisi.");

            if (!Enum.IsDefined(typeof(DoctorScheduleType), request.ScheduleType))
                return (false, "Tipe jadwal dokter tidak valid.");

            if (!Enum.IsDefined(typeof(DoctorScheduleStatus), request.ScheduleStatus))
                return (false, "Status jadwal dokter tidak valid.");

            if (!Enum.IsDefined(typeof(DayOfWeek), request.PracticeDay))
                return (false, "Hari praktik tidak valid.");

            if (request.DoctorId == Guid.Empty)
                return (false, "Dokter wajib dipilih.");

            if (request.ServiceUnitId == Guid.Empty)
                return (false, "Service unit wajib dipilih.");

            if (request.ClinicId == Guid.Empty)
                return (false, "Clinic wajib dipilih.");

            if (request.StartTime == request.EndTime)
                return (false, "Jam mulai dan jam selesai tidak boleh sama.");

            if (!request.IsOvernight && request.EndTime <= request.StartTime)
                return (false, "Jam selesai harus lebih besar dari jam mulai untuk jadwal non-overnight.");

            if (request.MaxPatientQuota < 0)
                return (false, "Kuota pasien tidak boleh kurang dari 0.");

            if (request.MaxAppointmentQuota < 0)
                return (false, "Kuota appointment tidak boleh kurang dari 0.");

            if (request.MaxWalkInQuota < 0)
                return (false, "Kuota walk-in tidak boleh kurang dari 0.");

            if (request.MaxPatientQuota > 0 &&
                request.MaxAppointmentQuota + request.MaxWalkInQuota > request.MaxPatientQuota)
            {
                return (false, "Total kuota appointment dan walk-in tidak boleh melebihi kuota pasien.");
            }

            if (request.EstimatedServiceMinutes <= 0)
                return (false, "Estimasi menit layanan harus lebih besar dari 0.");

            if ((request.ScheduleType == DoctorScheduleType.SpecificDate ||
                 request.ScheduleType == DoctorScheduleType.Temporary) &&
                !request.PracticeDate.HasValue)
            {
                return (false, "Tanggal praktik wajib diisi untuk jadwal specific date atau temporary.");
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "Tanggal akhir efektif tidak boleh lebih kecil dari tanggal mulai efektif.");
            }

            var doctorExists = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.DoctorId && x.IsActive && !x.IsDelete);

            if (!doctorExists)
                return (false, "Dokter tidak valid atau tidak aktif.");

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.ServiceUnitId && x.IsActive && !x.IsDelete);

            if (!serviceUnitExists)
                return (false, "Service unit tidak valid atau tidak aktif.");

            var clinicExists = await _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.ClinicId &&
                    x.ServiceUnitId == request.ServiceUnitId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!clinicExists)
                return (false, "Clinic tidak valid, tidak aktif, atau tidak berada pada service unit yang dipilih.");

            var roomId = NormalizeNullableGuid(request.RoomId);

            if (roomId.HasValue)
            {
                var roomExists = await _dbContext.Set<MstRoom>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == roomId.Value &&
                        x.ServiceUnitId == request.ServiceUnitId &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!roomExists)
                    return (false, "Room tidak valid, tidak aktif, atau tidak berada pada service unit yang dipilih.");
            }

            var substituteDoctorId = NormalizeNullableGuid(request.SubstituteDoctorId);

            if (substituteDoctorId.HasValue)
            {
                if (substituteDoctorId.Value == request.DoctorId)
                    return (false, "Dokter pengganti tidak boleh sama dengan dokter utama.");

                var substituteDoctorExists = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == substituteDoctorId.Value && x.IsActive && !x.IsDelete);

                if (!substituteDoctorExists)
                    return (false, "Dokter pengganti tidak valid atau tidak aktif.");
            }

            var normalizedName = request.ScheduleName.Trim().ToLower();

            var duplicateNameInDoctorClinic = await _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DoctorId == request.DoctorId &&
                    x.ServiceUnitId == request.ServiceUnitId &&
                    x.ClinicId == request.ClinicId &&
                    x.ScheduleName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateNameInDoctorClinic)
                return (false, "Nama jadwal dokter pada clinic tersebut sudah digunakan.");

            var hasScheduleConflict = await HasScheduleConflictAsync(
                excludeId: excludeId,
                doctorId: request.DoctorId,
                serviceUnitId: request.ServiceUnitId,
                clinicId: request.ClinicId,
                practiceDay: request.PracticeDay,
                practiceDate: request.PracticeDate?.Date,
                startTime: request.StartTime,
                endTime: request.EndTime,
                isOvernight: request.IsOvernight
            );

            if (hasScheduleConflict)
                return (false, "Dokter sudah memiliki jadwal pada hari/tanggal dan rentang jam tersebut.");

            return (true, null);
        }

        private async Task<bool> HasScheduleConflictAsync(
            Guid? excludeId,
            Guid doctorId,
            Guid serviceUnitId,
            Guid clinicId,
            DayOfWeek practiceDay,
            DateTime? practiceDate,
            TimeSpan startTime,
            TimeSpan endTime,
            bool isOvernight)
        {
            var query = _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.DoctorId == doctorId &&
                    x.ServiceUnitId == serviceUnitId &&
                    x.ClinicId == clinicId &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (practiceDate.HasValue)
            {
                var date = practiceDate.Value.Date;
                query = query.Where(x => x.PracticeDate == date);
            }
            else
            {
                query = query.Where(x => x.PracticeDate == null && x.PracticeDay == practiceDay);
            }

            var existingSchedules = await query
                .Select(x => new
                {
                    x.StartTime,
                    x.EndTime,
                    x.IsOvernight
                })
                .ToListAsync();

            return existingSchedules.Any(x =>
                IsTimeRangeOverlap(
                    x.StartTime,
                    x.EndTime,
                    x.IsOvernight,
                    startTime,
                    endTime,
                    isOvernight
                ));
        }

        private static bool IsTimeRangeOverlap(
            TimeSpan firstStart,
            TimeSpan firstEnd,
            bool firstIsOvernight,
            TimeSpan secondStart,
            TimeSpan secondEnd,
            bool secondIsOvernight)
        {
            var firstRanges = BuildTimeRanges(firstStart, firstEnd, firstIsOvernight);
            var secondRanges = BuildTimeRanges(secondStart, secondEnd, secondIsOvernight);

            return firstRanges.Any(first =>
                secondRanges.Any(second =>
                    first.Start < second.End && second.Start < first.End));
        }

        private static List<(int Start, int End)> BuildTimeRanges(
            TimeSpan start,
            TimeSpan end,
            bool isOvernight)
        {
            var startMinute = (int)start.TotalMinutes;
            var endMinute = (int)end.TotalMinutes;

            if (!isOvernight)
                return new List<(int Start, int End)> { (startMinute, endMinute) };

            return new List<(int Start, int End)>
            {
                (startMinute, 24 * 60),
                (0, endMinute)
            };
        }

        private async Task<string> GenerateNextScheduleCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ScheduleCode.StartsWith(ScheduleCodePrefix))
                .Select(x => x.ScheduleCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(code =>
                {
                    var suffix = code.Replace(ScheduleCodePrefix, string.Empty);
                    return int.TryParse(suffix, out var number) ? number : 0;
                })
                .Where(number => number > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{ScheduleCodePrefix}{nextNumber:00000}";
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<DoctorScheduleEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new DoctorScheduleEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        private string GetDefaultDoctorProfilePhotoPath()
        {
            var configuredPath = _configuration["FileStorage:DefaultDoctorProfilePhotoPath"];

            return string.IsNullOrWhiteSpace(configuredPath)
                ? DefaultDoctorProfilePhotoPathFallback
                : configuredPath.Trim();
        }

        private string ResolveDoctorProfilePhotoPath(string? profilePhotoPath)
        {
            return string.IsNullOrWhiteSpace(profilePhotoPath)
                ? GetDefaultDoctorProfilePhotoPath()
                : profilePhotoPath.Trim();
        }

        private string? BuildPublicFileUrl(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            var normalizedPath = filePath.Trim();

            if (normalizedPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedPath;
            }

            if (!normalizedPath.StartsWith('/'))
            {
                normalizedPath = "/" + normalizedPath;
            }

            var configuredBaseUrl =
                _configuration["FileStorage:PublicBaseUrl"] ??
                _configuration["FileStorage:BaseUrl"] ??
                _configuration["App:PublicBaseUrl"] ??
                _configuration["AppSettings:PublicBaseUrl"];

            var requestBaseUrl = Request?.Host.HasValue == true
                ? $"{Request.Scheme}://{Request.Host.Value}"
                : string.Empty;

            var baseUrl = string.IsNullOrWhiteSpace(configuredBaseUrl)
                ? requestBaseUrl
                : configuredBaseUrl.Trim();

            return string.IsNullOrWhiteSpace(baseUrl)
                ? normalizedPath
                : baseUrl.TrimEnd('/') + normalizedPath;
        }

        private async Task<Dictionary<Guid, string>> GetDoctorProfilePhotoPathMapAsync(IEnumerable<Guid> doctorIds)
        {
            var normalizedDoctorIds = doctorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (normalizedDoctorIds.Count == 0)
            {
                return new Dictionary<Guid, string>();
            }

            var userPhotoRows = await _dbContext.Users
                .AsNoTracking()
                .Where(u =>
                    u.DoctorId.HasValue &&
                    normalizedDoctorIds.Contains(u.DoctorId.Value))
                .OrderByDescending(u => u.IsActive)
                .ThenByDescending(u => u.UpdateDateTime ?? u.CreateDateTime)
                .Select(u => new
                {
                    DoctorId = u.DoctorId!.Value,
                    u.ProfilePhotoPath
                })
                .ToListAsync();

            return userPhotoRows
                .GroupBy(x => x.DoctorId)
                .ToDictionary(
                    group => group.Key,
                    group => ResolveDoctorProfilePhotoPath(
                        group
                            .Select(x => x.ProfilePhotoPath)
                            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                    )
                );
        }

        private async Task EnrichDoctorSchedulePhotoFieldsAsync(List<DoctorScheduleResponse> responses)
        {
            var photoMap = await GetDoctorProfilePhotoPathMapAsync(responses.Select(x => x.DoctorId));

            foreach (var response in responses)
            {
                var photoPath = photoMap.TryGetValue(response.DoctorId, out var mappedPhotoPath)
                    ? mappedPhotoPath
                    : ResolveDoctorProfilePhotoPath(response.ProfilePhotoPath);

                ApplyDoctorPhotoFields(response, photoPath);
            }
        }

        private async Task EnrichDoctorSchedulePhotoFieldAsync(DoctorScheduleResponse response)
        {
            var photoMap = await GetDoctorProfilePhotoPathMapAsync(new[] { response.DoctorId });
            var photoPath = photoMap.TryGetValue(response.DoctorId, out var mappedPhotoPath)
                ? mappedPhotoPath
                : ResolveDoctorProfilePhotoPath(response.ProfilePhotoPath);

            ApplyDoctorPhotoFields(response, photoPath);
        }

        private async Task EnrichDoctorScheduleOptionPhotoFieldsAsync(List<DoctorScheduleOptionResponse> responses)
        {
            var photoMap = await GetDoctorProfilePhotoPathMapAsync(responses.Select(x => x.DoctorId));

            foreach (var response in responses)
            {
                var photoPath = photoMap.TryGetValue(response.DoctorId, out var mappedPhotoPath)
                    ? mappedPhotoPath
                    : ResolveDoctorProfilePhotoPath(response.ProfilePhotoPath);

                ApplyDoctorPhotoFields(response, photoPath);
            }
        }

        private void ApplyDoctorPhotoFields(DoctorScheduleResponse response, string? photoPath)
        {
            response.ProfilePhotoPath = ResolveDoctorProfilePhotoPath(photoPath);
            response.ProfilePhotoUrl = BuildPublicFileUrl(response.ProfilePhotoPath);
            response.DoctorPhotoPath = response.ProfilePhotoPath;
            response.DoctorPhotoUrl = response.ProfilePhotoUrl;
        }

        private void ApplyDoctorPhotoFields(DoctorScheduleOptionResponse response, string? photoPath)
        {
            response.ProfilePhotoPath = ResolveDoctorProfilePhotoPath(photoPath);
            response.ProfilePhotoUrl = BuildPublicFileUrl(response.ProfilePhotoPath);
            response.DoctorPhotoPath = response.ProfilePhotoPath;
            response.DoctorPhotoUrl = response.ProfilePhotoUrl;
        }

        private static List<DoctorScheduleQueryParameterResponse> BuildQueryParameters()
        {
            return new List<DoctorScheduleQueryParameterResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal berdasarkan tanggal dibuat." },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir berdasarkan tanggal dibuat." },
                new() { Name = "customPeriod", Type = "string", Description = "today, last7days, last30days, thismonth, thisyear, all." },
                new() { Name = "doctorId", Type = "Guid?", Description = "Filter relasi dokter." },
                new() { Name = "clinicId", Type = "Guid?", Description = "Filter relasi clinic." },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif." },
                new() { Name = "search", Type = "string", Description = "Pencarian teks." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting." },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc." },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman." },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman." }
            };
        }

        private static List<DoctorScheduleFieldMetadataResponse> BuildCreateFields()
        {
            return new List<DoctorScheduleFieldMetadataResponse>
            {
                new() { Name = "scheduleCode", Label = "Kode Jadwal", DataType = "text", Required = false, ReadOnly = true, DefaultValue = "Auto generated by system" },
                new() { Name = "scheduleName", Label = "Nama Jadwal", DataType = "text", Required = true, ReadOnly = false },
                new() { Name = "scheduleType", Label = "Tipe Jadwal", DataType = "enum", Required = true, ReadOnly = false, OptionsSource = "scheduleTypeOptions" },
                new() { Name = "doctorId", Label = "Dokter", DataType = "guid", Required = true, ReadOnly = false, OptionsSource = "/api/v1/corporate/human-resource/master-data/doctors/options" },
                new() { Name = "serviceUnitId", Label = "Service Unit", DataType = "guid", Required = true, ReadOnly = false, OptionsSource = "/api/v1/health-services/master-data/service-units/options" },
                new() { Name = "clinicId", Label = "Clinic", DataType = "guid", Required = true, ReadOnly = false, OptionsSource = "/api/v1/health-services/master-data/clinics/options" },
                new() { Name = "roomId", Label = "Room", DataType = "guid", Required = false, ReadOnly = false, OptionsSource = "/api/v1/health-services/master-data/rooms/options" },
                new() { Name = "practiceDay", Label = "Hari Praktik", DataType = "enum", Required = true, ReadOnly = false, OptionsSource = "practiceDayOptions" },
                new() { Name = "practiceDate", Label = "Tanggal Praktik", DataType = "date", Required = false, ReadOnly = false },
                new() { Name = "startTime", Label = "Jam Mulai", DataType = "time", Required = true, ReadOnly = false },
                new() { Name = "endTime", Label = "Jam Selesai", DataType = "time", Required = true, ReadOnly = false },
                new() { Name = "isOvernight", Label = "Overnight", DataType = "boolean", Required = false, ReadOnly = false },
                new() { Name = "sessionName", Label = "Nama Sesi", DataType = "text", Required = false, ReadOnly = false },
                new() { Name = "practiceLocation", Label = "Lokasi Praktik", DataType = "text", Required = false, ReadOnly = false },
                new() { Name = "roomName", Label = "Nama Ruangan Manual", DataType = "text", Required = false, ReadOnly = false },
                new() { Name = "maxPatientQuota", Label = "Kuota Pasien", DataType = "number", Required = false, ReadOnly = false },
                new() { Name = "maxAppointmentQuota", Label = "Kuota Appointment", DataType = "number", Required = false, ReadOnly = false },
                new() { Name = "maxWalkInQuota", Label = "Kuota Walk-In", DataType = "number", Required = false, ReadOnly = false },
                new() { Name = "estimatedServiceMinutes", Label = "Estimasi Menit Layanan", DataType = "number", Required = true, ReadOnly = false },
                new() { Name = "isAllowWalkIn", Label = "Allow Walk-In", DataType = "boolean", Required = false, ReadOnly = false },
                new() { Name = "isAllowAppointment", Label = "Allow Appointment", DataType = "boolean", Required = false, ReadOnly = false },
                new() { Name = "isAllowKioskRegistration", Label = "Allow Kiosk Registration", DataType = "boolean", Required = false, ReadOnly = false },
                new() { Name = "isTelemedicineAvailable", Label = "Telemedicine", DataType = "boolean", Required = false, ReadOnly = false },
                new() { Name = "isSubstituteSchedule", Label = "Jadwal Pengganti", DataType = "boolean", Required = false, ReadOnly = false },
                new() { Name = "substituteDoctorId", Label = "Dokter Pengganti", DataType = "guid", Required = false, ReadOnly = false, OptionsSource = "/api/v1/corporate/human-resource/master-data/doctors/options" },
                new() { Name = "scheduleStatus", Label = "Status Jadwal", DataType = "enum", Required = true, ReadOnly = false, OptionsSource = "scheduleStatusOptions" },
                new() { Name = "effectiveStartDate", Label = "Tanggal Mulai Efektif", DataType = "date", Required = false, ReadOnly = false },
                new() { Name = "effectiveEndDate", Label = "Tanggal Akhir Efektif", DataType = "date", Required = false, ReadOnly = false },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "number", Required = false, ReadOnly = false },
                new() { Name = "description", Label = "Deskripsi", DataType = "textarea", Required = false, ReadOnly = false },
                new() { Name = "isActive", Label = "Aktif", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = true }
            };
        }

        private static List<DoctorScheduleFieldMetadataResponse> BuildUpdateFields()
        {
            return BuildCreateFields();
        }
    }
}
