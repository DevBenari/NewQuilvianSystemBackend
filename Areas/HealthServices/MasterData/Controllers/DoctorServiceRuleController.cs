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

using ResponseDoctorServiceRulePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DoctorServiceRuleResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/doctor-service-rules")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Doctor Service Rule",
        AreaName = "HealthServices",
        ControllerName = "DoctorServiceRule",
        Description = "Health service master data doctor service rule",
        SortOrder = 7
    )]
    [Tags("Health Services / Master Data / Doctor Service Rule")]
    public class DoctorServiceRuleController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DoctorServiceRuleController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DoctorServiceRuleFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Service Rule", Description = "Melihat data doctor service rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorServiceRule", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DoctorServiceRuleFilterMetadataResponse
            {
                DefaultFilter = new DoctorServiceRuleDefaultFilterResponse(),
                SortOptions = new List<DoctorServiceRuleSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "ruleCode", Label = "Kode rule" },
                    new() { Value = "ruleName", Label = "Nama rule" },
                    new() { Value = "doctorName", Label = "Nama dokter" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "clinicName", Label = "Nama clinic" },
                    new() { Value = "tariffCategoryName", Label = "Kategori tarif" },
                    new() { Value = "tariffName", Label = "Tarif" },
                    new() { Value = "procedureName", Label = "Tindakan" },
                    new() { Value = "patientClassName", Label = "Kelas pasien" },
                    new() { Value = "ruleType", Label = "Tipe rule" },
                    new() { Value = "ruleStatus", Label = "Status rule" },
                    new() { Value = "priorityLevel", Label = "Prioritas" },
                    new() { Value = "dailyQuotaLimit", Label = "Limit kuota harian" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RuleTypeOptions = BuildEnumOptions<DoctorServiceRuleType>(),
                RuleStatusOptions = BuildEnumOptions<DoctorServiceRuleStatus>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorServiceRule.GetFilterMetadata",
                "Mengambil metadata filter doctor service rule.",
                result
            );

            return Ok(ApiResponse<DoctorServiceRuleFilterMetadataResponse>.Ok(
                result,
                "Metadata filter doctor service rule berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DoctorServiceRuleSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Service Rule", Description = "Melihat data doctor service rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorServiceRule", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDoctorServiceRule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DoctorServiceRuleSummaryResponse
            {
                TotalRule = await query.CountAsync(),
                ActiveRule = await query.CountAsync(x => x.IsActive),
                InactiveRule = await query.CountAsync(x => !x.IsActive),
                WalkInAllowedRule = await query.CountAsync(x => x.IsAllowWalkIn),
                AppointmentAllowedRule = await query.CountAsync(x => x.IsAllowAppointment),
                KioskAllowedRule = await query.CountAsync(x => x.IsAllowKioskRegistration),
                TelemedicineAllowedRule = await query.CountAsync(x => x.IsAllowTelemedicine),
                NeedReferralRule = await query.CountAsync(x => x.IsNeedReferral),
                NeedApprovalRule = await query.CountAsync(x => x.IsNeedApproval),
                PrimaryClinicRule = await query.CountAsync(x => x.IsPrimaryForClinic),
                DefaultClinicRule = await query.CountAsync(x => x.IsDefaultForClinic),
                SuspendedRule = await query.CountAsync(x => x.RuleStatus == DoctorServiceRuleStatus.Suspended),
                ClosedRule = await query.CountAsync(x => x.RuleStatus == DoctorServiceRuleStatus.Closed)
            };

            return Ok(ApiResponse<DoctorServiceRuleSummaryResponse>.Ok(
                result,
                "Ringkasan doctor service rule berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDoctorServiceRulePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Service Rule", Description = "Melihat data doctor service rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorServiceRule", "Read")]
        public async Task<IActionResult> GetDoctorServiceRules(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] Guid? tariffId,
            [FromQuery] Guid? procedureId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] DoctorServiceRuleType? ruleType,
            [FromQuery] DoctorServiceRuleStatus? ruleStatus,
            [FromQuery] bool? isAllowWalkIn,
            [FromQuery] bool? isAllowAppointment,
            [FromQuery] bool? isAllowKioskRegistration,
            [FromQuery] bool? isAllowTelemedicine,
            [FromQuery] bool? isNeedReferral,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isPrimaryForClinic,
            [FromQuery] bool? isDefaultForClinic,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstDoctorServiceRule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.RuleCode.ToLower().Contains(keyword) ||
                    x.RuleName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.DoctorCode.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.DoctorNumber.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.SpecialistName != null && x.Doctor.SpecialistName.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicCode.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryCode.ToLower().Contains(keyword)) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryName.ToLower().Contains(keyword)) ||
                    (x.Tariff != null && x.Tariff.TariffCode.ToLower().Contains(keyword)) ||
                    (x.Tariff != null && x.Tariff.TariffName.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureCode.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureName.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassCode.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (tariffCategoryId.HasValue && tariffCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.TariffCategoryId == tariffCategoryId.Value);

            if (tariffId.HasValue && tariffId.Value != Guid.Empty)
                query = query.Where(x => x.TariffId == tariffId.Value);

            if (procedureId.HasValue && procedureId.Value != Guid.Empty)
                query = query.Where(x => x.ProcedureId == procedureId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.PatientClassId == patientClassId.Value);

            if (ruleType.HasValue)
                query = query.Where(x => x.RuleType == ruleType.Value);

            if (ruleStatus.HasValue)
                query = query.Where(x => x.RuleStatus == ruleStatus.Value);

            if (isAllowWalkIn.HasValue)
                query = query.Where(x => x.IsAllowWalkIn == isAllowWalkIn.Value);

            if (isAllowAppointment.HasValue)
                query = query.Where(x => x.IsAllowAppointment == isAllowAppointment.Value);

            if (isAllowKioskRegistration.HasValue)
                query = query.Where(x => x.IsAllowKioskRegistration == isAllowKioskRegistration.Value);

            if (isAllowTelemedicine.HasValue)
                query = query.Where(x => x.IsAllowTelemedicine == isAllowTelemedicine.Value);

            if (isNeedReferral.HasValue)
                query = query.Where(x => x.IsNeedReferral == isNeedReferral.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (isPrimaryForClinic.HasValue)
                query = query.Where(x => x.IsPrimaryForClinic == isPrimaryForClinic.Value);

            if (isDefaultForClinic.HasValue)
                query = query.Where(x => x.IsDefaultForClinic == isDefaultForClinic.Value);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseDoctorServiceRulePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDoctorServiceRulePagedResult>.Ok(
                result,
                "Data doctor service rule berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<DoctorServiceRuleOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Service Rule", Description = "Melihat data doctor service rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorServiceRule", "Read")]
        public async Task<IActionResult> GetDoctorServiceRuleOptions(
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] Guid? tariffId,
            [FromQuery] Guid? procedureId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] DoctorServiceRuleType? ruleType,
            [FromQuery] DoctorServiceRuleStatus? ruleStatus,
            [FromQuery] bool? isAllowWalkIn,
            [FromQuery] bool? isAllowAppointment,
            [FromQuery] bool? isAllowKioskRegistration,
            [FromQuery] bool? isAllowTelemedicine,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstDoctorServiceRule>()
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

            if (tariffCategoryId.HasValue && tariffCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.TariffCategoryId == tariffCategoryId.Value);

            if (tariffId.HasValue && tariffId.Value != Guid.Empty)
                query = query.Where(x => x.TariffId == tariffId.Value);

            if (procedureId.HasValue && procedureId.Value != Guid.Empty)
                query = query.Where(x => x.ProcedureId == procedureId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.PatientClassId == patientClassId.Value);

            if (ruleType.HasValue)
                query = query.Where(x => x.RuleType == ruleType.Value);

            if (ruleStatus.HasValue)
                query = query.Where(x => x.RuleStatus == ruleStatus.Value);

            if (isAllowWalkIn.HasValue)
                query = query.Where(x => x.IsAllowWalkIn == isAllowWalkIn.Value);

            if (isAllowAppointment.HasValue)
                query = query.Where(x => x.IsAllowAppointment == isAllowAppointment.Value);

            if (isAllowKioskRegistration.HasValue)
                query = query.Where(x => x.IsAllowKioskRegistration == isAllowKioskRegistration.Value);

            if (isAllowTelemedicine.HasValue)
                query = query.Where(x => x.IsAllowTelemedicine == isAllowTelemedicine.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.RuleCode.ToLower().Contains(keyword) ||
                    x.RuleName.ToLower().Contains(keyword) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryName.ToLower().Contains(keyword)) ||
                    (x.Tariff != null && x.Tariff.TariffName.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureName.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Doctor != null ? x.Doctor.FullName : string.Empty)
                .ThenBy(x => x.RuleName)
                .Select(x => new DoctorServiceRuleOptionResponse
                {
                    Id = x.Id,
                    RuleCode = x.RuleCode,
                    RuleName = x.RuleName,
                    RuleType = x.RuleType,
                    RuleStatus = x.RuleStatus,

                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                    SpecialistName = x.Doctor != null ? x.Doctor.SpecialistName : null,

                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,

                    ClinicId = x.ClinicId,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,

                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : null,

                    TariffId = x.TariffId,
                    TariffName = x.Tariff != null ? x.Tariff.TariffName : null,

                    ProcedureId = x.ProcedureId,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,

                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,

                    IsAllowWalkIn = x.IsAllowWalkIn,
                    IsAllowAppointment = x.IsAllowAppointment,
                    IsAllowKioskRegistration = x.IsAllowKioskRegistration,
                    IsAllowTelemedicine = x.IsAllowTelemedicine,

                    IsNeedReferral = x.IsNeedReferral,
                    IsNeedApproval = x.IsNeedApproval,

                    IsPrimaryForClinic = x.IsPrimaryForClinic,
                    IsDefaultForClinic = x.IsDefaultForClinic,

                    DailyQuotaLimit = x.DailyQuotaLimit,
                    PriorityLevel = x.PriorityLevel
                })
                .ToListAsync();

            return Ok(ApiResponse<List<DoctorServiceRuleOptionResponse>>.Ok(
                data,
                "Data pilihan doctor service rule berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorServiceRuleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Doctor Service Rule", Description = "Melihat detail doctor service rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorServiceRule", "Read")]
        public async Task<IActionResult> GetDoctorServiceRuleById(Guid id)
        {
            var data = await _dbContext.Set<MstDoctorServiceRule>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new DoctorServiceRuleDetailResponse
                {
                    Id = x.Id,
                    RuleCode = x.RuleCode,
                    RuleName = x.RuleName,
                    RuleType = x.RuleType,
                    RuleStatus = x.RuleStatus,

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
                    ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,

                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryCode = x.TariffCategory != null ? x.TariffCategory.TariffCategoryCode : null,
                    TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : null,

                    TariffId = x.TariffId,
                    TariffCode = x.Tariff != null ? x.Tariff.TariffCode : null,
                    TariffName = x.Tariff != null ? x.Tariff.TariffName : null,

                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,

                    PatientClassId = x.PatientClassId,
                    PatientClassCode = x.PatientClass != null ? x.PatientClass.PatientClassCode : null,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,

                    IsAllowWalkIn = x.IsAllowWalkIn,
                    IsAllowAppointment = x.IsAllowAppointment,
                    IsAllowKioskRegistration = x.IsAllowKioskRegistration,
                    IsAllowTelemedicine = x.IsAllowTelemedicine,

                    IsNeedReferral = x.IsNeedReferral,
                    IsNeedApproval = x.IsNeedApproval,

                    IsPrimaryForClinic = x.IsPrimaryForClinic,
                    IsDefaultForClinic = x.IsDefaultForClinic,

                    DailyQuotaLimit = x.DailyQuotaLimit,
                    PriorityLevel = x.PriorityLevel,

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
                    "Doctor service rule tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DoctorServiceRuleDetailResponse>.Ok(
                data,
                "Detail doctor service rule berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DoctorServiceRuleCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Doctor Service Rule", Description = "Membuat data doctor service rule", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DoctorServiceRule", "Create")]
        public async Task<IActionResult> CreateDoctorServiceRule([FromBody] CreateDoctorServiceRuleRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data doctor service rule tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstDoctorServiceRule
            {
                Id = Guid.NewGuid(),
                RuleCode = request.RuleCode.Trim().ToUpperInvariant(),
                RuleName = request.RuleName.Trim(),
                RuleType = request.RuleType,
                DoctorId = request.DoctorId,
                ServiceUnitId = request.ServiceUnitId,
                ClinicId = NormalizeNullableGuid(request.ClinicId),
                TariffCategoryId = NormalizeNullableGuid(request.TariffCategoryId),
                TariffId = NormalizeNullableGuid(request.TariffId),
                ProcedureId = NormalizeNullableGuid(request.ProcedureId),
                PatientClassId = NormalizeNullableGuid(request.PatientClassId),
                IsAllowWalkIn = request.IsAllowWalkIn,
                IsAllowAppointment = request.IsAllowAppointment,
                IsAllowKioskRegistration = request.IsAllowKioskRegistration,
                IsAllowTelemedicine = request.IsAllowTelemedicine,
                IsNeedReferral = request.IsNeedReferral,
                IsNeedApproval = request.IsNeedApproval,
                IsPrimaryForClinic = request.IsPrimaryForClinic,
                IsDefaultForClinic = request.IsDefaultForClinic,
                DailyQuotaLimit = request.DailyQuotaLimit,
                PriorityLevel = request.PriorityLevel,
                RuleStatus = request.RuleStatus,
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

            _dbContext.Set<MstDoctorServiceRule>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new DoctorServiceRuleCreateResponse
            {
                Id = entity.Id,
                RuleCode = entity.RuleCode,
                RuleName = entity.RuleName,
                RuleType = entity.RuleType,
                RuleStatus = entity.RuleStatus,
                DoctorId = entity.DoctorId,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                TariffCategoryId = entity.TariffCategoryId,
                TariffId = entity.TariffId,
                ProcedureId = entity.ProcedureId,
                PatientClassId = entity.PatientClassId,
                IsAllowWalkIn = entity.IsAllowWalkIn,
                IsAllowAppointment = entity.IsAllowAppointment,
                IsAllowKioskRegistration = entity.IsAllowKioskRegistration,
                IsAllowTelemedicine = entity.IsAllowTelemedicine,
                IsNeedReferral = entity.IsNeedReferral,
                IsNeedApproval = entity.IsNeedApproval,
                IsPrimaryForClinic = entity.IsPrimaryForClinic,
                IsDefaultForClinic = entity.IsDefaultForClinic,
                DailyQuotaLimit = entity.DailyQuotaLimit,
                PriorityLevel = entity.PriorityLevel,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<DoctorServiceRuleCreateResponse>.Ok(
                response,
                "Doctor service rule berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Doctor Service Rule", Description = "Mengubah data doctor service rule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorServiceRule", "Update")]
        public async Task<IActionResult> UpdateDoctorServiceRule(Guid id, [FromBody] UpdateDoctorServiceRuleRequest request)
        {
            var entity = await _dbContext.Set<MstDoctorServiceRule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor service rule tidak ditemukan."
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
                    validation.ErrorMessage ?? "Data doctor service rule tidak valid."
                ));
            }

            entity.RuleCode = request.RuleCode.Trim().ToUpperInvariant();
            entity.RuleName = request.RuleName.Trim();
            entity.RuleType = request.RuleType;
            entity.DoctorId = request.DoctorId;
            entity.ServiceUnitId = request.ServiceUnitId;
            entity.ClinicId = NormalizeNullableGuid(request.ClinicId);
            entity.TariffCategoryId = NormalizeNullableGuid(request.TariffCategoryId);
            entity.TariffId = NormalizeNullableGuid(request.TariffId);
            entity.ProcedureId = NormalizeNullableGuid(request.ProcedureId);
            entity.PatientClassId = NormalizeNullableGuid(request.PatientClassId);
            entity.IsAllowWalkIn = request.IsAllowWalkIn;
            entity.IsAllowAppointment = request.IsAllowAppointment;
            entity.IsAllowKioskRegistration = request.IsAllowKioskRegistration;
            entity.IsAllowTelemedicine = request.IsAllowTelemedicine;
            entity.IsNeedReferral = request.IsNeedReferral;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.IsPrimaryForClinic = request.IsPrimaryForClinic;
            entity.IsDefaultForClinic = request.IsDefaultForClinic;
            entity.DailyQuotaLimit = request.DailyQuotaLimit;
            entity.PriorityLevel = request.PriorityLevel;
            entity.RuleStatus = request.RuleStatus;
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
                "Doctor service rule berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Doctor Service Rule", Description = "Mengaktifkan data doctor service rule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorServiceRule", "Update")]
        public async Task<IActionResult> ActivateDoctorServiceRule(Guid id)
        {
            return await SetActiveStatusAsync(id, true, "Doctor service rule berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Doctor Service Rule", Description = "Menonaktifkan data doctor service rule", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorServiceRule", "Update")]
        public async Task<IActionResult> DeactivateDoctorServiceRule(Guid id)
        {
            return await SetActiveStatusAsync(id, false, "Doctor service rule berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Doctor Service Rule", Description = "Menghapus data doctor service rule", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DoctorServiceRule", "Delete")]
        public async Task<IActionResult> DeleteDoctorServiceRule(Guid id)
        {
            var entity = await _dbContext.Set<MstDoctorServiceRule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor service rule tidak ditemukan."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Doctor service rule berhasil dihapus."
            ));
        }

        private static DoctorServiceRuleResponse ToResponse(MstDoctorServiceRule x)
        {
            return new DoctorServiceRuleResponse
            {
                Id = x.Id,
                RuleCode = x.RuleCode,
                RuleName = x.RuleName,
                RuleType = x.RuleType,
                RuleStatus = x.RuleStatus,

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
                ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,

                TariffCategoryId = x.TariffCategoryId,
                TariffCategoryCode = x.TariffCategory != null ? x.TariffCategory.TariffCategoryCode : null,
                TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : null,

                TariffId = x.TariffId,
                TariffCode = x.Tariff != null ? x.Tariff.TariffCode : null,
                TariffName = x.Tariff != null ? x.Tariff.TariffName : null,

                ProcedureId = x.ProcedureId,
                ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,

                PatientClassId = x.PatientClassId,
                PatientClassCode = x.PatientClass != null ? x.PatientClass.PatientClassCode : null,
                PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,

                IsAllowWalkIn = x.IsAllowWalkIn,
                IsAllowAppointment = x.IsAllowAppointment,
                IsAllowKioskRegistration = x.IsAllowKioskRegistration,
                IsAllowTelemedicine = x.IsAllowTelemedicine,

                IsNeedReferral = x.IsNeedReferral,
                IsNeedApproval = x.IsNeedApproval,

                IsPrimaryForClinic = x.IsPrimaryForClinic,
                IsDefaultForClinic = x.IsDefaultForClinic,

                DailyQuotaLimit = x.DailyQuotaLimit,
                PriorityLevel = x.PriorityLevel,

                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,

                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private async Task<IActionResult> SetActiveStatusAsync(Guid id, bool isActive, string successMessage)
        {
            var entity = await _dbContext.Set<MstDoctorServiceRule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Doctor service rule tidak ditemukan."
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
            CreateDoctorServiceRuleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RuleCode))
                return (false, "Kode doctor service rule wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.RuleName))
                return (false, "Nama doctor service rule wajib diisi.");

            if (request.DoctorId == Guid.Empty)
                return (false, "Dokter wajib dipilih.");

            if (request.ServiceUnitId == Guid.Empty)
                return (false, "Service unit wajib dipilih.");

            if (request.DailyQuotaLimit < 0)
                return (false, "Limit kuota harian tidak boleh kurang dari 0.");

            if (request.PriorityLevel < 0)
                return (false, "Level prioritas tidak boleh kurang dari 0.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "Tanggal akhir efektif tidak boleh lebih kecil dari tanggal mulai efektif.");
            }

            var clinicId = NormalizeNullableGuid(request.ClinicId);
            var tariffCategoryId = NormalizeNullableGuid(request.TariffCategoryId);
            var tariffId = NormalizeNullableGuid(request.TariffId);
            var procedureId = NormalizeNullableGuid(request.ProcedureId);
            var patientClassId = NormalizeNullableGuid(request.PatientClassId);

            if (request.RuleType == DoctorServiceRuleType.Tariff && !tariffId.HasValue)
                return (false, "Tariff wajib dipilih untuk rule type Tariff.");

            if (request.RuleType == DoctorServiceRuleType.TariffCategory && !tariffCategoryId.HasValue)
                return (false, "Tariff category wajib dipilih untuk rule type TariffCategory.");

            if (request.RuleType == DoctorServiceRuleType.Procedure && !procedureId.HasValue)
                return (false, "Procedure wajib dipilih untuk rule type Procedure.");

            var doctorExists = await _dbContext.Set<MstDoctor>()
                .AnyAsync(x => x.Id == request.DoctorId && x.IsActive && !x.IsDelete);

            if (!doctorExists)
                return (false, "Dokter tidak valid atau tidak aktif.");

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AnyAsync(x => x.Id == request.ServiceUnitId && x.IsActive && !x.IsDelete);

            if (!serviceUnitExists)
                return (false, "Service unit tidak valid atau tidak aktif.");

            if (clinicId.HasValue)
            {
                var clinicExists = await _dbContext.Set<MstClinic>()
                    .AnyAsync(x =>
                        x.Id == clinicId.Value &&
                        x.ServiceUnitId == request.ServiceUnitId &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!clinicExists)
                    return (false, "Clinic tidak valid, tidak aktif, atau tidak berada pada service unit yang dipilih.");
            }

            if (tariffCategoryId.HasValue)
            {
                var tariffCategoryExists = await _dbContext.Set<MstTariffCategory>()
                    .AnyAsync(x => x.Id == tariffCategoryId.Value && x.IsActive && !x.IsDelete);

                if (!tariffCategoryExists)
                    return (false, "Tariff category tidak valid atau tidak aktif.");
            }

            if (patientClassId.HasValue)
            {
                var patientClassExists = await _dbContext.Set<MstPatientClass>()
                    .AnyAsync(x => x.Id == patientClassId.Value && x.IsActive && !x.IsDelete);

                if (!patientClassExists)
                    return (false, "Patient class tidak valid atau tidak aktif.");
            }

            if (procedureId.HasValue)
            {
                var procedureExists = await _dbContext.Set<MstProcedure>()
                    .AnyAsync(x => x.Id == procedureId.Value && x.IsActive && !x.IsDelete);

                if (!procedureExists)
                    return (false, "Procedure tidak valid atau tidak aktif.");
            }

            if (tariffId.HasValue)
            {
                var tariff = await _dbContext.Set<MstTariff>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == tariffId.Value && x.IsActive && !x.IsDelete);

                if (tariff == null)
                    return (false, "Tariff tidak valid atau tidak aktif.");

                if (tariffCategoryId.HasValue && tariff.TariffCategoryId != tariffCategoryId.Value)
                    return (false, "Tariff tidak sesuai dengan tariff category yang dipilih.");

                if (tariff.ServiceUnitId.HasValue && tariff.ServiceUnitId.Value != request.ServiceUnitId)
                    return (false, "Tariff tidak sesuai dengan service unit yang dipilih.");

                if (clinicId.HasValue && tariff.ClinicId.HasValue && tariff.ClinicId.Value != clinicId.Value)
                    return (false, "Tariff tidak sesuai dengan clinic yang dipilih.");

                if (patientClassId.HasValue && tariff.PatientClassId.HasValue && tariff.PatientClassId.Value != patientClassId.Value)
                    return (false, "Tariff tidak sesuai dengan patient class yang dipilih.");

                if (procedureId.HasValue && tariff.ProcedureId.HasValue && tariff.ProcedureId.Value != procedureId.Value)
                    return (false, "Tariff tidak sesuai dengan procedure yang dipilih.");
            }

            var normalizedCode = request.RuleCode.Trim().ToUpperInvariant();
            var normalizedName = request.RuleName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstDoctorServiceRule>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.RuleCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode doctor service rule sudah digunakan.");

            var duplicateName = await _dbContext.Set<MstDoctorServiceRule>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DoctorId == request.DoctorId &&
                    x.ServiceUnitId == request.ServiceUnitId &&
                    x.ClinicId == clinicId &&
                    x.RuleName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama doctor service rule pada dokter dan unit tersebut sudah digunakan.");

            var duplicateCombination = await _dbContext.Set<MstDoctorServiceRule>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DoctorId == request.DoctorId &&
                    x.ServiceUnitId == request.ServiceUnitId &&
                    x.ClinicId == clinicId &&
                    x.TariffCategoryId == tariffCategoryId &&
                    x.TariffId == tariffId &&
                    x.ProcedureId == procedureId &&
                    x.PatientClassId == patientClassId &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCombination)
                return (false, "Rule dengan kombinasi dokter, unit, clinic, tariff, procedure, dan patient class tersebut sudah ada.");

            return (true, null);
        }

        private static IQueryable<MstDoctorServiceRule> ApplySorting(
            IQueryable<MstDoctorServiceRule> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "rulecode" => isDesc
                    ? query.OrderByDescending(x => x.RuleCode)
                    : query.OrderBy(x => x.RuleCode),

                "rulename" => isDesc
                    ? query.OrderByDescending(x => x.RuleName)
                    : query.OrderBy(x => x.RuleName),

                "doctorname" => isDesc
                    ? query.OrderByDescending(x => x.Doctor != null ? x.Doctor.FullName : "")
                    : query.OrderBy(x => x.Doctor != null ? x.Doctor.FullName : ""),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : "")
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : ""),

                "clinicname" => isDesc
                    ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : "")
                    : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : ""),

                "tariffcategoryname" => isDesc
                    ? query.OrderByDescending(x => x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : "")
                    : query.OrderBy(x => x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : ""),

                "tariffname" => isDesc
                    ? query.OrderByDescending(x => x.Tariff != null ? x.Tariff.TariffName : "")
                    : query.OrderBy(x => x.Tariff != null ? x.Tariff.TariffName : ""),

                "procedurename" => isDesc
                    ? query.OrderByDescending(x => x.Procedure != null ? x.Procedure.ProcedureName : "")
                    : query.OrderBy(x => x.Procedure != null ? x.Procedure.ProcedureName : ""),

                "patientclassname" => isDesc
                    ? query.OrderByDescending(x => x.PatientClass != null ? x.PatientClass.PatientClassName : "")
                    : query.OrderBy(x => x.PatientClass != null ? x.PatientClass.PatientClassName : ""),

                "ruletype" => isDesc
                    ? query.OrderByDescending(x => x.RuleType)
                    : query.OrderBy(x => x.RuleType),

                "rulestatus" => isDesc
                    ? query.OrderByDescending(x => x.RuleStatus)
                    : query.OrderBy(x => x.RuleStatus),

                "prioritylevel" => isDesc
                    ? query.OrderByDescending(x => x.PriorityLevel)
                    : query.OrderBy(x => x.PriorityLevel),

                "dailyquotalimit" => isDesc
                    ? query.OrderByDescending(x => x.DailyQuotaLimit)
                    : query.OrderBy(x => x.DailyQuotaLimit),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.PriorityLevel)
                        .ThenByDescending(x => x.Doctor != null ? x.Doctor.FullName : "")
                        .ThenByDescending(x => x.RuleName)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.PriorityLevel)
                        .ThenBy(x => x.Doctor != null ? x.Doctor.FullName : "")
                        .ThenBy(x => x.RuleName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<DoctorServiceRuleEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new DoctorServiceRuleEnumOptionResponse
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