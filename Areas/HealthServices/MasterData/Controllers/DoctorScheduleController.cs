using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
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

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DoctorScheduleController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DoctorScheduleFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Schedule", Description = "Melihat data doctor schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorSchedule", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DoctorScheduleFilterMetadataResponse
            {
                DefaultFilter = new DoctorScheduleDefaultFilterResponse(),
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
                PracticeDayOptions = BuildEnumOptions<DayOfWeek>()
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
        [ProducesResponseType(typeof(ApiResponse<ResponseDoctorSchedulePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Schedule", Description = "Melihat data doctor schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorSchedule", "Read")]
        public async Task<IActionResult> GetDoctorSchedules(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? roomId,
            [FromQuery] DoctorScheduleType? scheduleType,
            [FromQuery] DoctorScheduleStatus? scheduleStatus,
            [FromQuery] DayOfWeek? practiceDay,
            [FromQuery] DateTime? practiceDate,
            [FromQuery] bool? isAllowWalkIn,
            [FromQuery] bool? isAllowAppointment,
            [FromQuery] bool? isAllowKioskRegistration,
            [FromQuery] bool? isTelemedicineAvailable,
            [FromQuery] bool? isSubstituteSchedule,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

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

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (roomId.HasValue && roomId.Value != Guid.Empty)
                query = query.Where(x => x.RoomId == roomId.Value);

            if (scheduleType.HasValue)
                query = query.Where(x => x.ScheduleType == scheduleType.Value);

            if (scheduleStatus.HasValue)
                query = query.Where(x => x.ScheduleStatus == scheduleStatus.Value);

            if (practiceDay.HasValue)
                query = query.Where(x => x.PracticeDay == practiceDay.Value);

            if (practiceDate.HasValue)
            {
                var date = practiceDate.Value.Date;
                query = query.Where(x => x.PracticeDate == date);
            }

            if (isAllowWalkIn.HasValue)
                query = query.Where(x => x.IsAllowWalkIn == isAllowWalkIn.Value);

            if (isAllowAppointment.HasValue)
                query = query.Where(x => x.IsAllowAppointment == isAllowAppointment.Value);

            if (isAllowKioskRegistration.HasValue)
                query = query.Where(x => x.IsAllowKioskRegistration == isAllowKioskRegistration.Value);

            if (isTelemedicineAvailable.HasValue)
                query = query.Where(x => x.IsTelemedicineAvailable == isTelemedicineAvailable.Value);

            if (isSubstituteSchedule.HasValue)
                query = query.Where(x => x.IsSubstituteSchedule == isSubstituteSchedule.Value);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DoctorScheduleResponse
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
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
        [ProducesResponseType(typeof(ApiResponse<List<DoctorScheduleOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Schedule", Description = "Melihat data doctor schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorSchedule", "Read")]
        public async Task<IActionResult> GetDoctorScheduleOptions(
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? roomId,
            [FromQuery] DoctorScheduleType? scheduleType,
            [FromQuery] DoctorScheduleStatus? scheduleStatus,
            [FromQuery] DayOfWeek? practiceDay,
            [FromQuery] DateTime? practiceDate,
            [FromQuery] bool? isAllowWalkIn,
            [FromQuery] bool? isAllowAppointment,
            [FromQuery] bool? isAllowKioskRegistration,
            [FromQuery] bool? isTelemedicineAvailable,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (roomId.HasValue && roomId.Value != Guid.Empty)
                query = query.Where(x => x.RoomId == roomId.Value);

            if (scheduleType.HasValue)
                query = query.Where(x => x.ScheduleType == scheduleType.Value);

            if (scheduleStatus.HasValue)
                query = query.Where(x => x.ScheduleStatus == scheduleStatus.Value);

            if (practiceDay.HasValue)
                query = query.Where(x => x.PracticeDay == practiceDay.Value);

            if (practiceDate.HasValue)
            {
                var date = practiceDate.Value.Date;
                query = query.Where(x => x.PracticeDate == date);
            }

            if (isAllowWalkIn.HasValue)
                query = query.Where(x => x.IsAllowWalkIn == isAllowWalkIn.Value);

            if (isAllowAppointment.HasValue)
                query = query.Where(x => x.IsAllowAppointment == isAllowAppointment.Value);

            if (isAllowKioskRegistration.HasValue)
                query = query.Where(x => x.IsAllowKioskRegistration == isAllowKioskRegistration.Value);

            if (isTelemedicineAvailable.HasValue)
                query = query.Where(x => x.IsTelemedicineAvailable == isTelemedicineAvailable.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ScheduleCode.ToLower().Contains(keyword) ||
                    x.ScheduleName.ToLower().Contains(keyword) ||
                    (x.SessionName != null && x.SessionName.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Doctor != null ? x.Doctor.FullName : string.Empty)
                .ThenBy(x => x.PracticeDay)
                .ThenBy(x => x.StartTime)
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

            return Ok(ApiResponse<List<DoctorScheduleOptionResponse>>.Ok(
                data,
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
            var data = await _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new DoctorScheduleDetailResponse
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
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor schedule tidak ditemukan."
                ));
            }

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

            var entity = new MstDoctorSchedule
            {
                Id = Guid.NewGuid(),
                ScheduleCode = request.ScheduleCode.Trim().ToUpperInvariant(),
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
                IsActive = true,
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
                IsAllowWalkIn = entity.IsAllowWalkIn,
                IsAllowAppointment = entity.IsAllowAppointment,
                IsAllowKioskRegistration = entity.IsAllowKioskRegistration,
                IsTelemedicineAvailable = entity.IsTelemedicineAvailable,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<DoctorScheduleCreateResponse>.Ok(
                response,
                "Doctor schedule berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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

            entity.ScheduleCode = request.ScheduleCode.Trim().ToUpperInvariant();
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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Doctor schedule berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Doctor Schedule", Description = "Mengaktifkan data doctor schedule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorSchedule", "Update")]
        public async Task<IActionResult> ActivateDoctorSchedule(Guid id)
        {
            return await SetActiveStatusAsync(id, true, "Doctor schedule berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Doctor Schedule", Description = "Menonaktifkan data doctor schedule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorSchedule", "Update")]
        public async Task<IActionResult> DeactivateDoctorSchedule(Guid id)
        {
            return await SetActiveStatusAsync(id, false, "Doctor schedule berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Doctor Schedule", Description = "Menghapus data doctor schedule", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DoctorSchedule", "Delete")]
        public async Task<IActionResult> DeleteDoctorSchedule(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
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

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                successMessage
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateDoctorScheduleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ScheduleCode))
                return (false, "Kode jadwal dokter wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.ScheduleName))
                return (false, "Nama jadwal dokter wajib diisi.");

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
                .AnyAsync(x => x.Id == request.DoctorId && x.IsActive && !x.IsDelete);

            if (!doctorExists)
                return (false, "Dokter tidak valid atau tidak aktif.");

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AnyAsync(x => x.Id == request.ServiceUnitId && x.IsActive && !x.IsDelete);

            if (!serviceUnitExists)
                return (false, "Service unit tidak valid atau tidak aktif.");

            var clinicExists = await _dbContext.Set<MstClinic>()
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
                    .AnyAsync(x => x.Id == substituteDoctorId.Value && x.IsActive && !x.IsDelete);

                if (!substituteDoctorExists)
                    return (false, "Dokter pengganti tidak valid atau tidak aktif.");
            }

            var normalizedCode = request.ScheduleCode.Trim().ToUpperInvariant();
            var normalizedName = request.ScheduleName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstDoctorSchedule>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ScheduleCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode jadwal dokter sudah digunakan.");

            var duplicateNameInDoctorClinic = await _dbContext.Set<MstDoctorSchedule>()
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

        private static IQueryable<MstDoctorSchedule> ApplySorting(
            IQueryable<MstDoctorSchedule> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
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
                    ? query.OrderByDescending(x => x.Doctor != null ? x.Doctor.FullName : "")
                    : query.OrderBy(x => x.Doctor != null ? x.Doctor.FullName : ""),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : "")
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : ""),

                "clinicname" => isDesc
                    ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : "")
                    : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : ""),

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
                        .ThenByDescending(x => x.Doctor != null ? x.Doctor.FullName : "")
                        .ThenByDescending(x => x.PracticeDay)
                        .ThenByDescending(x => x.StartTime)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Doctor != null ? x.Doctor.FullName : "")
                        .ThenBy(x => x.PracticeDay)
                        .ThenBy(x => x.StartTime)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

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
    }
}