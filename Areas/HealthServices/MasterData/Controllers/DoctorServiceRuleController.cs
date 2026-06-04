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
        private const string RuleCodePrefix = "DSR-RSMMC-";

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
                CustomPeriods = BuildCustomPeriods(),
                SortOptions = new List<DoctorServiceRuleSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "ruleCode", Label = "Kode rule" },
                    new() { Value = "ruleName", Label = "Nama rule" },
                    new() { Value = "doctorName", Label = "Nama dokter" },
                    new() { Value = "serviceUnitName", Label = "Nama service unit" },
                    new() { Value = "clinicName", Label = "Nama clinic" },
                    new() { Value = "ruleType", Label = "Tipe rule" },
                    new() { Value = "ruleStatus", Label = "Status rule" },
                    new() { Value = "priorityLevel", Label = "Prioritas" },
                    new() { Value = "dailyQuotaLimit", Label = "Limit kuota harian" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RuleTypeOptions = BuildEnumOptions<DoctorServiceRuleType>(),
                RuleStatusOptions = BuildEnumOptions<DoctorServiceRuleStatus>(),
                QueryParameters = BuildQueryParameters(),
                CreateFields = BuildCreateFields(),
                UpdateFields = BuildUpdateFields()
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
        [ProducesResponseType(typeof(ApiResponse<DoctorServiceRuleOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Service Rule", Description = "Melihat data pilihan doctor service rule", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorServiceRule", "Read")]
        public async Task<IActionResult> GetDoctorServiceRuleOptions(
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
                .ThenBy(x => x.RuleName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            var result = new DoctorServiceRuleOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DoctorServiceRuleOptionPagedResponse>.Ok(
                result,
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
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => ToDetailResponse(x))
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
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data doctor service rule tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var ruleCode = await GenerateNextRuleCodeAsync();

            if (request.IsPrimaryForClinic || request.IsDefaultForClinic)
            {
                await ClearOtherClinicFlagsAsync(
                    excludeId: null,
                    doctorId: request.DoctorId,
                    serviceUnitId: request.ServiceUnitId,
                    clinicId: NormalizeNullableGuid(request.ClinicId)!.Value,
                    clearPrimary: request.IsPrimaryForClinic,
                    clearDefault: request.IsDefaultForClinic,
                    now: now,
                    actorUserId: actorUserId
                );
            }

            var entity = new MstDoctorServiceRule
            {
                Id = Guid.NewGuid(),
                RuleCode = ruleCode,
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
                IsActive = request.IsActive,
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
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DoctorServiceRule.Create",
                "Membuat data doctor service rule.",
                response
            );

            return Ok(ApiResponse<DoctorServiceRuleCreateResponse>.Ok(
                response,
                "Doctor service rule berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorServiceRuleUpdateResponse>), StatusCodes.Status200OK)]
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

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data doctor service rule tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimaryForClinic || request.IsDefaultForClinic)
            {
                await ClearOtherClinicFlagsAsync(
                    excludeId: id,
                    doctorId: request.DoctorId,
                    serviceUnitId: request.ServiceUnitId,
                    clinicId: NormalizeNullableGuid(request.ClinicId)!.Value,
                    clearPrimary: request.IsPrimaryForClinic,
                    clearDefault: request.IsDefaultForClinic,
                    now: now,
                    actorUserId: actorUserId
                );
            }

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
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = new DoctorServiceRuleUpdateResponse
            {
                Id = entity.Id,
                RuleCode = entity.RuleCode,
                RuleName = entity.RuleName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            return Ok(ApiResponse<DoctorServiceRuleUpdateResponse>.Ok(
                response,
                "Doctor service rule berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorServiceRuleDeleteResponse>), StatusCodes.Status200OK)]
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

            var response = new DoctorServiceRuleDeleteResponse
            {
                Id = entity.Id,
                RuleCode = entity.RuleCode,
                RuleName = entity.RuleName,
                IsDelete = entity.IsDelete,
                DeleteDateTime = entity.DeleteDateTime
            };

            return Ok(ApiResponse<DoctorServiceRuleDeleteResponse>.Ok(
                response,
                "Doctor service rule berhasil dihapus."
            ));
        }

        private IQueryable<MstDoctorServiceRule> BuildBaseQuery()
        {
            return _dbContext.Set<MstDoctorServiceRule>()
                .AsNoTracking()
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.TariffCategory)
                .Include(x => x.Tariff)
                .Include(x => x.Procedure)
                .Include(x => x.PatientClass)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDoctorServiceRule> ApplyDateFilter(
            IQueryable<MstDoctorServiceRule> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;

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

        private static IQueryable<MstDoctorServiceRule> ApplyFilters(
            IQueryable<MstDoctorServiceRule> query,
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

            return query;
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

        private static DoctorServiceRuleDetailResponse ToDetailResponse(MstDoctorServiceRule x)
        {
            var response = new DoctorServiceRuleDetailResponse
            {
                Description = x.Description
            };

            var baseResponse = ToResponse(x);
            response.Id = baseResponse.Id;
            response.RuleCode = baseResponse.RuleCode;
            response.RuleName = baseResponse.RuleName;
            response.RuleType = baseResponse.RuleType;
            response.RuleStatus = baseResponse.RuleStatus;
            response.DoctorId = baseResponse.DoctorId;
            response.DoctorCode = baseResponse.DoctorCode;
            response.DoctorNumber = baseResponse.DoctorNumber;
            response.DoctorName = baseResponse.DoctorName;
            response.SpecialistName = baseResponse.SpecialistName;
            response.SubSpecialistName = baseResponse.SubSpecialistName;
            response.MedicalStaffGroup = baseResponse.MedicalStaffGroup;
            response.ServiceUnitId = baseResponse.ServiceUnitId;
            response.ServiceUnitCode = baseResponse.ServiceUnitCode;
            response.ServiceUnitName = baseResponse.ServiceUnitName;
            response.ClinicId = baseResponse.ClinicId;
            response.ClinicCode = baseResponse.ClinicCode;
            response.ClinicName = baseResponse.ClinicName;
            response.TariffCategoryId = baseResponse.TariffCategoryId;
            response.TariffCategoryCode = baseResponse.TariffCategoryCode;
            response.TariffCategoryName = baseResponse.TariffCategoryName;
            response.TariffId = baseResponse.TariffId;
            response.TariffCode = baseResponse.TariffCode;
            response.TariffName = baseResponse.TariffName;
            response.ProcedureId = baseResponse.ProcedureId;
            response.ProcedureCode = baseResponse.ProcedureCode;
            response.ProcedureName = baseResponse.ProcedureName;
            response.PatientClassId = baseResponse.PatientClassId;
            response.PatientClassCode = baseResponse.PatientClassCode;
            response.PatientClassName = baseResponse.PatientClassName;
            response.IsAllowWalkIn = baseResponse.IsAllowWalkIn;
            response.IsAllowAppointment = baseResponse.IsAllowAppointment;
            response.IsAllowKioskRegistration = baseResponse.IsAllowKioskRegistration;
            response.IsAllowTelemedicine = baseResponse.IsAllowTelemedicine;
            response.IsNeedReferral = baseResponse.IsNeedReferral;
            response.IsNeedApproval = baseResponse.IsNeedApproval;
            response.IsPrimaryForClinic = baseResponse.IsPrimaryForClinic;
            response.IsDefaultForClinic = baseResponse.IsDefaultForClinic;
            response.DailyQuotaLimit = baseResponse.DailyQuotaLimit;
            response.PriorityLevel = baseResponse.PriorityLevel;
            response.EffectiveStartDate = baseResponse.EffectiveStartDate;
            response.EffectiveEndDate = baseResponse.EffectiveEndDate;
            response.SortOrder = baseResponse.SortOrder;
            response.IsActive = baseResponse.IsActive;
            response.CreateDateTime = baseResponse.CreateDateTime;

            return response;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateDoctorServiceRuleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RuleName))
                return (false, "Nama doctor service rule wajib diisi.");

            if (!Enum.IsDefined(typeof(DoctorServiceRuleType), request.RuleType))
                return (false, "Tipe doctor service rule tidak valid.");

            if (!Enum.IsDefined(typeof(DoctorServiceRuleStatus), request.RuleStatus))
                return (false, "Status doctor service rule tidak valid.");

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

            if ((request.IsPrimaryForClinic || request.IsDefaultForClinic) && !clinicId.HasValue)
                return (false, "Clinic wajib dipilih jika rule menjadi primary/default clinic.");

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

            var normalizedName = request.RuleName.Trim().ToLower();

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

        private async Task ClearOtherClinicFlagsAsync(
            Guid? excludeId,
            Guid doctorId,
            Guid serviceUnitId,
            Guid clinicId,
            bool clearPrimary,
            bool clearDefault,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstDoctorServiceRule>()
                .Where(x =>
                    !x.IsDelete &&
                    x.DoctorId == doctorId &&
                    x.ServiceUnitId == serviceUnitId &&
                    x.ClinicId == clinicId &&
                    ((clearPrimary && x.IsPrimaryForClinic) || (clearDefault && x.IsDefaultForClinic)));

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            var entities = await query.ToListAsync();

            foreach (var entity in entities)
            {
                if (clearPrimary)
                    entity.IsPrimaryForClinic = false;

                if (clearDefault)
                    entity.IsDefaultForClinic = false;

                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateNextRuleCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstDoctorServiceRule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.RuleCode.StartsWith(RuleCodePrefix))
                .Select(x => x.RuleCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(code =>
                {
                    var suffix = code.Replace(RuleCodePrefix, string.Empty);
                    return int.TryParse(suffix, out var number) ? number : 0;
                })
                .Where(number => number > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{RuleCodePrefix}{nextNumber:00000}";
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
                    ? query.OrderByDescending(x => x.Doctor != null ? x.Doctor.FullName : string.Empty)
                    : query.OrderBy(x => x.Doctor != null ? x.Doctor.FullName : string.Empty),
                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty)
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty),
                "clinicname" => isDesc
                    ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty)
                    : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty),
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
                        .ThenByDescending(x => x.Doctor != null ? x.Doctor.FullName : string.Empty)
                        .ThenByDescending(x => x.RuleName)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.PriorityLevel)
                        .ThenBy(x => x.Doctor != null ? x.Doctor.FullName : string.Empty)
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

        private static List<DoctorServiceRuleCustomPeriodResponse> BuildCustomPeriods()
        {
            return new List<DoctorServiceRuleCustomPeriodResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "last30days", Label = "30 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "thisyear", Label = "Tahun ini" },
                new() { Value = "all", Label = "Semua data" }
            };
        }

        private static List<DoctorServiceRuleQueryParameterResponse> BuildQueryParameters()
        {
            return new List<DoctorServiceRuleQueryParameterResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Filter tanggal awal berdasarkan tanggal dibuat." },
                new() { Name = "endDate", Type = "date", Description = "Filter tanggal akhir berdasarkan tanggal dibuat." },
                new() { Name = "customPeriod", Type = "string", Description = "today, last7days, last30days, thismonth, thisyear, all." },
                new() { Name = "doctorId", Type = "guid", Description = "Filter relasi dokter." },
                new() { Name = "clinicId", Type = "guid", Description = "Filter relasi clinic." },
                new() { Name = "isActive", Type = "boolean", Description = "Filter status aktif." },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama rule, dokter, unit, clinic, tarif, tindakan, kelas pasien." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting." },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc." },
                new() { Name = "pageNumber", Type = "number", Description = "Nomor halaman." },
                new() { Name = "pageSize", Type = "number", Description = "Jumlah data per halaman." }
            };
        }

        private static List<DoctorServiceRuleFieldMetadataResponse> BuildCreateFields()
        {
            return new List<DoctorServiceRuleFieldMetadataResponse>
            {
                new() { Name = "ruleCode", Label = "Kode Rule", DataType = "text", Required = false, ReadOnly = true, DefaultValue = "AutoGenerated" },
                new() { Name = "ruleName", Label = "Nama Rule", DataType = "text", Required = true, ReadOnly = false },
                new() { Name = "ruleType", Label = "Tipe Rule", DataType = "enum", Required = true, ReadOnly = false, OptionsSource = "ruleTypeOptions" },
                new() { Name = "doctorId", Label = "Dokter", DataType = "select", Required = true, ReadOnly = false, OptionsSource = "doctorOptions" },
                new() { Name = "serviceUnitId", Label = "Service Unit", DataType = "select", Required = true, ReadOnly = false, OptionsSource = "serviceUnitOptions" },
                new() { Name = "clinicId", Label = "Clinic", DataType = "select", Required = false, ReadOnly = false, OptionsSource = "clinicOptions" },
                new() { Name = "tariffCategoryId", Label = "Kategori Tarif", DataType = "select", Required = false, ReadOnly = false, OptionsSource = "tariffCategoryOptions" },
                new() { Name = "tariffId", Label = "Tarif", DataType = "select", Required = false, ReadOnly = false, OptionsSource = "tariffOptions" },
                new() { Name = "procedureId", Label = "Tindakan", DataType = "select", Required = false, ReadOnly = false, OptionsSource = "procedureOptions" },
                new() { Name = "patientClassId", Label = "Kelas Pasien", DataType = "select", Required = false, ReadOnly = false, OptionsSource = "patientClassOptions" },
                new() { Name = "isAllowWalkIn", Label = "Boleh Walk-In", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = true },
                new() { Name = "isAllowAppointment", Label = "Boleh Appointment", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = true },
                new() { Name = "isAllowKioskRegistration", Label = "Boleh Kiosk", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = true },
                new() { Name = "isAllowTelemedicine", Label = "Boleh Telemedicine", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = false },
                new() { Name = "isNeedReferral", Label = "Butuh Referral", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = false },
                new() { Name = "isNeedApproval", Label = "Butuh Approval", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = false },
                new() { Name = "isPrimaryForClinic", Label = "Primary untuk Clinic", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = false },
                new() { Name = "isDefaultForClinic", Label = "Default untuk Clinic", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = false },
                new() { Name = "dailyQuotaLimit", Label = "Limit Kuota Harian", DataType = "number", Required = false, ReadOnly = false, DefaultValue = 0 },
                new() { Name = "priorityLevel", Label = "Level Prioritas", DataType = "number", Required = false, ReadOnly = false, DefaultValue = 0 },
                new() { Name = "ruleStatus", Label = "Status Rule", DataType = "enum", Required = true, ReadOnly = false, OptionsSource = "ruleStatusOptions" },
                new() { Name = "effectiveStartDate", Label = "Tanggal Mulai Efektif", DataType = "date", Required = false, ReadOnly = false },
                new() { Name = "effectiveEndDate", Label = "Tanggal Akhir Efektif", DataType = "date", Required = false, ReadOnly = false },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "number", Required = false, ReadOnly = false, DefaultValue = 0 },
                new() { Name = "description", Label = "Deskripsi", DataType = "textarea", Required = false, ReadOnly = false },
                new() { Name = "isActive", Label = "Status Aktif", DataType = "boolean", Required = false, ReadOnly = false, DefaultValue = true }
            };
        }

        private static List<DoctorServiceRuleFieldMetadataResponse> BuildUpdateFields()
        {
            return BuildCreateFields();
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
