using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseProcedurePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.ProcedureResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/procedures")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Procedure",
        AreaName = "HealthServices",
        ControllerName = "Procedure",
        Description = "Health service master data procedure",
        SortOrder = 9
    )]
    [Tags("Health Services / Master Data / Procedure")]
    public class ProcedureController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string CodePrefix = "PR-RSMMC-";
        private const int CodeSequenceLength = 5;

        private static readonly HashSet<string> AllowedProcedureTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "General",
            "Nursing",
            "DoctorAction",
            "Surgery",
            "Laboratory",
            "Radiology",
            "Therapy",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ProcedureController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Procedure", Description = "Melihat data procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ProcedureFilterMetadataResponse
            {
                DefaultFilter = new ProcedureDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<ProcedureSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "procedureCode", Label = "Kode procedure" },
                    new() { Value = "procedureName", Label = "Nama procedure" },
                    new() { Value = "procedureGroupName", Label = "Group procedure" },
                    new() { Value = "procedureCategoryName", Label = "Kategori procedure" },
                    new() { Value = "procedureType", Label = "Tipe procedure" },
                    new() { Value = "estimatedDurationMinutes", Label = "Estimasi durasi" },
                    new() { Value = "isNeedDoctor", Label = "Butuh dokter" },
                    new() { Value = "isNeedApproval", Label = "Butuh approval" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ProcedureTypeOptions = AllowedProcedureTypes.OrderBy(x => x).ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Procedure.GetFilterMetadata",
                "Mengambil metadata filter procedure.",
                result
            );

            return Ok(ApiResponse<ProcedureFilterMetadataResponse>.Ok(
                result,
                "Metadata filter procedure berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Procedure", Description = "Melihat data procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new ProcedureSummaryResponse
            {
                TotalProcedure = await query.CountAsync(),
                ActiveProcedure = await query.CountAsync(x => x.IsActive),
                InactiveProcedure = await query.CountAsync(x => !x.IsActive),
                DoctorActionProcedure = await query.CountAsync(x => x.IsDoctorAction),
                NursingActionProcedure = await query.CountAsync(x => x.IsNursingAction),
                SurgeryProcedure = await query.CountAsync(x => x.IsSurgery),
                LaboratoryProcedure = await query.CountAsync(x => x.IsLaboratory),
                RadiologyProcedure = await query.CountAsync(x => x.IsRadiology),
                TherapyProcedure = await query.CountAsync(x => x.IsTherapy),
                NeedDoctorProcedure = await query.CountAsync(x => x.IsNeedDoctor),
                NeedApprovalProcedure = await query.CountAsync(x => x.IsNeedApproval),
                CoveredByInsuranceDefaultProcedure = await query.CountAsync(x => x.IsCoveredByInsuranceDefault),
                AvailableForOutpatientProcedure = await query.CountAsync(x => x.IsAvailableForOutpatient),
                AvailableForInpatientProcedure = await query.CountAsync(x => x.IsAvailableForInpatient),
                AvailableForEmergencyProcedure = await query.CountAsync(x => x.IsAvailableForEmergency),
                HasDefaultTariffProcedure = await query.CountAsync(x => x.DefaultTariffId.HasValue)
            };

            return Ok(ApiResponse<ProcedureSummaryResponse>.Ok(
                result,
                "Ringkasan procedure berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseProcedurePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Procedure", Description = "Melihat data procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetProcedures(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? defaultTariffId,
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

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = BuildBaseQuery();

            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            if (defaultTariffId.HasValue && defaultTariffId.Value != Guid.Empty)
                query = query.Where(x => x.DefaultTariffId == defaultTariffId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProcedureCode.ToLower().Contains(keyword) ||
                    x.ProcedureName.ToLower().Contains(keyword) ||
                    x.ProcedureType.ToLower().Contains(keyword) ||
                    (x.ProcedureGroupName != null && x.ProcedureGroupName.ToLower().Contains(keyword)) ||
                    (x.ProcedureCategoryName != null && x.ProcedureCategoryName.ToLower().Contains(keyword)) ||
                    (x.ExternalProcedureCode != null && x.ExternalProcedureCode.ToLower().Contains(keyword)) ||
                    (x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.DefaultTariff != null && x.DefaultTariff.TariffCode.ToLower().Contains(keyword)) ||
                    (x.DefaultTariff != null && x.DefaultTariff.TariffName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProcedureResponse
                {
                    Id = x.Id,
                    ProcedureCode = x.ProcedureCode,
                    ProcedureName = x.ProcedureName,
                    ProcedureGroupName = x.ProcedureGroupName,
                    ProcedureCategoryName = x.ProcedureCategoryName,
                    ProcedureType = x.ProcedureType,
                    DefaultTariffId = x.DefaultTariffId,
                    DefaultTariffCode = x.DefaultTariff != null ? x.DefaultTariff.TariffCode : null,
                    DefaultTariffName = x.DefaultTariff != null ? x.DefaultTariff.TariffName : null,
                    DefaultTariffNormalPrice = x.DefaultTariff != null ? x.DefaultTariff.NormalPrice : null,
                    IsDoctorAction = x.IsDoctorAction,
                    IsNursingAction = x.IsNursingAction,
                    IsSurgery = x.IsSurgery,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsTherapy = x.IsTherapy,
                    IsNeedDoctor = x.IsNeedDoctor,
                    IsNeedApproval = x.IsNeedApproval,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    IsAvailableForOutpatient = x.IsAvailableForOutpatient,
                    IsAvailableForInpatient = x.IsAvailableForInpatient,
                    IsAvailableForEmergency = x.IsAvailableForEmergency,
                    EstimatedDurationMinutes = x.EstimatedDurationMinutes,
                    ExternalProcedureCode = x.ExternalProcedureCode,
                    IntegrationCode = x.IntegrationCode,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseProcedurePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseProcedurePagedResult>.Ok(
                result,
                "Data procedure berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Procedure", Description = "Melihat data procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetProcedureOptions(
    [FromQuery] Guid? defaultTariffId,
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

            if (defaultTariffId.HasValue && defaultTariffId.Value != Guid.Empty)
                query = query.Where(x => x.DefaultTariffId == defaultTariffId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProcedureCode.ToLower().Contains(keyword) ||
                    x.ProcedureName.ToLower().Contains(keyword) ||
                    x.ProcedureType.ToLower().Contains(keyword) ||
                    (x.ProcedureGroupName != null && x.ProcedureGroupName.ToLower().Contains(keyword)) ||
                    (x.ProcedureCategoryName != null && x.ProcedureCategoryName.ToLower().Contains(keyword)) ||
                    (x.ExternalProcedureCode != null && x.ExternalProcedureCode.ToLower().Contains(keyword)) ||
                    (x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword)) ||
                    (x.DefaultTariff != null && x.DefaultTariff.TariffName.ToLower().Contains(keyword)) ||
                    (x.DefaultTariff != null && x.DefaultTariff.TariffCode.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ProcedureName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProcedureOptionResponse
                {
                    Id = x.Id,
                    ProcedureCode = x.ProcedureCode,
                    ProcedureName = x.ProcedureName,
                    ProcedureGroupName = x.ProcedureGroupName,
                    ProcedureCategoryName = x.ProcedureCategoryName,
                    ProcedureType = x.ProcedureType,

                    DefaultTariffId = x.DefaultTariffId,
                    DefaultTariffName = x.DefaultTariff != null
                        ? x.DefaultTariff.TariffName
                        : null,
                    DefaultTariffNormalPrice = x.DefaultTariff != null
                        ? x.DefaultTariff.NormalPrice
                        : null,

                    IsDoctorAction = x.IsDoctorAction,
                    IsNursingAction = x.IsNursingAction,
                    IsSurgery = x.IsSurgery,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsTherapy = x.IsTherapy,
                    IsNeedDoctor = x.IsNeedDoctor,
                    IsNeedApproval = x.IsNeedApproval,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    IsAvailableForOutpatient = x.IsAvailableForOutpatient,
                    IsAvailableForInpatient = x.IsAvailableForInpatient,
                    IsAvailableForEmergency = x.IsAvailableForEmergency
                })
                .ToListAsync();

            var result = new ProcedureOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ProcedureOptionPagedResponse>.Ok(
                result,
                "Data pilihan procedure berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Procedure", Description = "Melihat data procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetProcedureById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new ProcedureDetailResponse
                {
                    Id = x.Id,
                    ProcedureCode = x.ProcedureCode,
                    ProcedureName = x.ProcedureName,
                    ProcedureGroupName = x.ProcedureGroupName,
                    ProcedureCategoryName = x.ProcedureCategoryName,
                    ProcedureType = x.ProcedureType,
                    DefaultTariffId = x.DefaultTariffId,
                    DefaultTariffCode = x.DefaultTariff != null ? x.DefaultTariff.TariffCode : null,
                    DefaultTariffName = x.DefaultTariff != null ? x.DefaultTariff.TariffName : null,
                    DefaultTariffNormalPrice = x.DefaultTariff != null ? x.DefaultTariff.NormalPrice : null,
                    IsDoctorAction = x.IsDoctorAction,
                    IsNursingAction = x.IsNursingAction,
                    IsSurgery = x.IsSurgery,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsTherapy = x.IsTherapy,
                    IsNeedDoctor = x.IsNeedDoctor,
                    IsNeedApproval = x.IsNeedApproval,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    IsAvailableForOutpatient = x.IsAvailableForOutpatient,
                    IsAvailableForInpatient = x.IsAvailableForInpatient,
                    IsAvailableForEmergency = x.IsAvailableForEmergency,
                    EstimatedDurationMinutes = x.EstimatedDurationMinutes,
                    ExternalProcedureCode = x.ExternalProcedureCode,
                    IntegrationCode = x.IntegrationCode,
                    SortOrder = x.SortOrder,
                    ClinicalNoteTemplate = x.ClinicalNoteTemplate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Procedure tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<ProcedureDetailResponse>.Ok(
                data,
                "Detail procedure berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProcedureCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Procedure", Description = "Membuat data procedure", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Procedure", "Create")]
        public async Task<IActionResult> CreateProcedure([FromBody] CreateProcedureRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                procedureName: request.ProcedureName,
                procedureType: request.ProcedureType,
                defaultTariffId: request.DefaultTariffId,
                estimatedDurationMinutes: request.EstimatedDurationMinutes
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data procedure tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstProcedure
            {
                Id = Guid.NewGuid(),
                ProcedureCode = await GenerateProcedureCodeAsync(),
                ProcedureName = request.ProcedureName.Trim(),
                ProcedureGroupName = NormalizeNullableText(request.ProcedureGroupName),
                ProcedureCategoryName = NormalizeNullableText(request.ProcedureCategoryName),
                ProcedureType = NormalizeProcedureType(request.ProcedureType),
                DefaultTariffId = NormalizeNullableGuid(request.DefaultTariffId),
                IsDoctorAction = request.IsDoctorAction,
                IsNursingAction = request.IsNursingAction,
                IsSurgery = request.IsSurgery,
                IsLaboratory = request.IsLaboratory,
                IsRadiology = request.IsRadiology,
                IsTherapy = request.IsTherapy,
                IsNeedDoctor = request.IsNeedDoctor,
                IsNeedApproval = request.IsNeedApproval,
                IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault,
                IsAvailableForOutpatient = request.IsAvailableForOutpatient,
                IsAvailableForInpatient = request.IsAvailableForInpatient,
                IsAvailableForEmergency = request.IsAvailableForEmergency,
                EstimatedDurationMinutes = request.EstimatedDurationMinutes,
                ExternalProcedureCode = NormalizeNullableText(request.ExternalProcedureCode),
                IntegrationCode = NormalizeNullableText(request.IntegrationCode),
                SortOrder = request.SortOrder,
                ClinicalNoteTemplate = NormalizeNullableText(request.ClinicalNoteTemplate),
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstProcedure>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "Procedure.CreateProcedure",
                "Membuat data procedure.",
                response
            );

            return Ok(ApiResponse<ProcedureCreateResponse>.Ok(
                response,
                "Procedure berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Procedure", Description = "Mengubah data procedure", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Procedure", "Update")]
        public async Task<IActionResult> UpdateProcedure(Guid id, [FromBody] UpdateProcedureRequest request)
        {
            var entity = await _dbContext.Set<MstProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Procedure tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                procedureName: request.ProcedureName,
                procedureType: request.ProcedureType,
                defaultTariffId: request.DefaultTariffId,
                estimatedDurationMinutes: request.EstimatedDurationMinutes
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data procedure tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ProcedureName = request.ProcedureName.Trim();
            entity.ProcedureGroupName = NormalizeNullableText(request.ProcedureGroupName);
            entity.ProcedureCategoryName = NormalizeNullableText(request.ProcedureCategoryName);
            entity.ProcedureType = NormalizeProcedureType(request.ProcedureType);
            entity.DefaultTariffId = NormalizeNullableGuid(request.DefaultTariffId);
            entity.IsDoctorAction = request.IsDoctorAction;
            entity.IsNursingAction = request.IsNursingAction;
            entity.IsSurgery = request.IsSurgery;
            entity.IsLaboratory = request.IsLaboratory;
            entity.IsRadiology = request.IsRadiology;
            entity.IsTherapy = request.IsTherapy;
            entity.IsNeedDoctor = request.IsNeedDoctor;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault;
            entity.IsAvailableForOutpatient = request.IsAvailableForOutpatient;
            entity.IsAvailableForInpatient = request.IsAvailableForInpatient;
            entity.IsAvailableForEmergency = request.IsAvailableForEmergency;
            entity.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
            entity.ExternalProcedureCode = NormalizeNullableText(request.ExternalProcedureCode);
            entity.IntegrationCode = NormalizeNullableText(request.IntegrationCode);
            entity.SortOrder = request.SortOrder;
            entity.ClinicalNoteTemplate = NormalizeNullableText(request.ClinicalNoteTemplate);
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "Procedure.UpdateProcedure",
                "Mengubah data procedure.",
                response
            );

            return Ok(ApiResponse<ProcedureUpdateResponse>.Ok(
                response,
                "Procedure berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Procedure", Description = "Menghapus data procedure", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Procedure", "Delete")]
        public async Task<IActionResult> DeleteProcedure(Guid id)
        {
            var entity = await _dbContext.Set<MstProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Procedure tidak ditemukan."
                ));
            }

            var isUsedByTariff = await _dbContext.Set<MstTariff>()
                .AnyAsync(x => x.ProcedureId == id && !x.IsDelete);

            if (isUsedByTariff)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Procedure tidak dapat dihapus karena sudah digunakan oleh tariff."
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

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Procedure.DeleteProcedure",
                "Menghapus data procedure.",
                new
                {
                    entity.Id,
                    entity.ProcedureCode,
                    entity.ProcedureName,
                    entity.DeleteDateTime
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Procedure berhasil dihapus."
            ));
        }

        private IQueryable<MstProcedure> BuildBaseQuery()
        {
            return _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string procedureName,
            string procedureType,
            Guid? defaultTariffId,
            int estimatedDurationMinutes)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
                return (false, "Nama procedure wajib diisi.");

            if (string.IsNullOrWhiteSpace(procedureType))
                return (false, "Tipe procedure wajib diisi.");

            if (!AllowedProcedureTypes.Contains(procedureType.Trim()))
            {
                return (false, "Tipe procedure tidak valid. Gunakan salah satu: General, Nursing, DoctorAction, Surgery, Laboratory, Radiology, Therapy, Other.");
            }

            if (estimatedDurationMinutes < 0)
                return (false, "Estimasi durasi procedure tidak boleh kurang dari 0 menit.");

            if (estimatedDurationMinutes > 1440)
                return (false, "Estimasi durasi procedure tidak boleh lebih dari 1440 menit.");

            var normalizedName = procedureName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ProcedureName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama procedure sudah digunakan.");

            var normalizedDefaultTariffId = NormalizeNullableGuid(defaultTariffId);

            if (normalizedDefaultTariffId.HasValue)
            {
                var tariffExists = await _dbContext.Set<MstTariff>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedDefaultTariffId.Value &&
                        !x.IsDelete &&
                        x.IsActive);

                if (!tariffExists)
                    return (false, "Default tariff tidak ditemukan atau tidak aktif.");
            }

            return (true, null);
        }

        private async Task<string> GenerateProcedureCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ProcedureCode.StartsWith(CodePrefix))
                .Select(x => x.ProcedureCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return $"{CodePrefix}{nextNumber.ToString().PadLeft(CodeSequenceLength, '0')}";
        }

        private static int? ExtractSequenceNumber(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            if (!code.StartsWith(CodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var sequenceText = code[CodePrefix.Length..];

            return int.TryParse(sequenceText, out var sequenceNumber)
                ? sequenceNumber
                : null;
        }

        private static IQueryable<MstProcedure> ApplySorting(
            IQueryable<MstProcedure> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "procedurecode" => isDesc
                    ? query.OrderByDescending(x => x.ProcedureCode)
                    : query.OrderBy(x => x.ProcedureCode),

                "procedurename" => isDesc
                    ? query.OrderByDescending(x => x.ProcedureName)
                    : query.OrderBy(x => x.ProcedureName),

                "proceduregroupname" => isDesc
                    ? query.OrderByDescending(x => x.ProcedureGroupName)
                    : query.OrderBy(x => x.ProcedureGroupName),

                "procedurecategoryname" => isDesc
                    ? query.OrderByDescending(x => x.ProcedureCategoryName)
                    : query.OrderBy(x => x.ProcedureCategoryName),

                "proceduretype" => isDesc
                    ? query.OrderByDescending(x => x.ProcedureType)
                    : query.OrderBy(x => x.ProcedureType),

                "estimateddurationminutes" => isDesc
                    ? query.OrderByDescending(x => x.EstimatedDurationMinutes)
                    : query.OrderBy(x => x.EstimatedDurationMinutes),

                "isneeddoctor" => isDesc
                    ? query.OrderByDescending(x => x.IsNeedDoctor)
                    : query.OrderBy(x => x.IsNeedDoctor),

                "isneedapproval" => isDesc
                    ? query.OrderByDescending(x => x.IsNeedApproval)
                    : query.OrderBy(x => x.IsNeedApproval),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ProcedureName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ProcedureName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<ProcedureCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<ProcedureCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisMonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static (bool IsValid, DateTime? Start, DateTime? EndExclusive, string? ErrorMessage) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;
            var period = customPeriod?.Trim();

            if (!string.IsNullOrWhiteSpace(period) && !string.Equals(period, "custom", StringComparison.OrdinalIgnoreCase))
            {
                return period.ToLowerInvariant() switch
                {
                    "today" => (true, today, today.AddDays(1), null),
                    "last7days" => (true, today.AddDays(-6), today.AddDays(1), null),
                    "last30days" => (true, today.AddDays(-29), today.AddDays(1), null),
                    "thismonth" => (true, new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1), null),
                    _ => (false, null, null, "Custom period tidak valid.")
                };
            }

            if (startDate.HasValue && endDate.HasValue && endDate.Value.Date < startDate.Value.Date)
            {
                return (false, null, null, "EndDate tidak boleh lebih kecil dari StartDate.");
            }

            var start = startDate?.Date;
            var endExclusive = endDate?.Date.AddDays(1);

            return (true, start, endExclusive, null);
        }

        private static List<ProcedureQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<ProcedureQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal berdasarkan CreateDateTime.", Example = "2026-01-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir berdasarkan CreateDateTime.", Example = "2026-01-31" },
                new() { Name = "customPeriod", Type = "string", Description = "today, last7days, last30days, thisMonth, custom.", Example = "thisMonth" },
                new() { Name = "defaultTariffId", Type = "Guid?", Description = "Filter berdasarkan default tariff.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama, group, kategori, tipe, external code, integration code, default tariff, dan deskripsi.", Example = "surgery" },
                new() { Name = "sortBy", Type = "string", Description = "Field sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman maksimal 100.", Example = "25" }
            };
        }

        private static List<ProcedureFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return new List<ProcedureFormFieldMetadataResponse>
            {
                new() { Name = "procedureCode", Label = "Kode Procedure", DataType = "string", InputType = "text", IsReadonly = true, Description = "Auto generated oleh sistem dengan format PR-RSMMC-00001." },
                new() { Name = "procedureName", Label = "Nama Procedure", DataType = "string", InputType = "text", Required = true, Placeholder = "Masukkan nama procedure" },
                new() { Name = "procedureGroupName", Label = "Group Procedure", DataType = "string", InputType = "text" },
                new() { Name = "procedureCategoryName", Label = "Kategori Procedure", DataType = "string", InputType = "text" },
                new() { Name = "procedureType", Label = "Tipe Procedure", DataType = "string", InputType = "select", Required = true, Description = "General, Nursing, DoctorAction, Surgery, Laboratory, Radiology, Therapy, Other." },
                new() { Name = "defaultTariffId", Label = "Default Tariff", DataType = "Guid?", InputType = "select" },
                new() { Name = "isDoctorAction", Label = "Tindakan Dokter", DataType = "bool", InputType = "switch" },
                new() { Name = "isNursingAction", Label = "Tindakan Perawat", DataType = "bool", InputType = "switch" },
                new() { Name = "isSurgery", Label = "Surgery", DataType = "bool", InputType = "switch" },
                new() { Name = "isLaboratory", Label = "Laboratory", DataType = "bool", InputType = "switch" },
                new() { Name = "isRadiology", Label = "Radiology", DataType = "bool", InputType = "switch" },
                new() { Name = "isTherapy", Label = "Therapy", DataType = "bool", InputType = "switch" },
                new() { Name = "isNeedDoctor", Label = "Butuh Dokter", DataType = "bool", InputType = "switch" },
                new() { Name = "isNeedApproval", Label = "Butuh Approval", DataType = "bool", InputType = "switch" },
                new() { Name = "isCoveredByInsuranceDefault", Label = "Default Ditanggung Asuransi", DataType = "bool", InputType = "switch" },
                new() { Name = "isAvailableForOutpatient", Label = "Tersedia Rawat Jalan", DataType = "bool", InputType = "switch" },
                new() { Name = "isAvailableForInpatient", Label = "Tersedia Rawat Inap", DataType = "bool", InputType = "switch" },
                new() { Name = "isAvailableForEmergency", Label = "Tersedia IGD", DataType = "bool", InputType = "switch" },
                new() { Name = "estimatedDurationMinutes", Label = "Estimasi Durasi", DataType = "int", InputType = "number" },
                new() { Name = "externalProcedureCode", Label = "Kode External", DataType = "string", InputType = "text" },
                new() { Name = "integrationCode", Label = "Kode Integrasi", DataType = "string", InputType = "text" },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "int", InputType = "number" },
                new() { Name = "clinicalNoteTemplate", Label = "Template Catatan Klinis", DataType = "string", InputType = "textarea" },
                new() { Name = "description", Label = "Deskripsi", DataType = "string", InputType = "textarea" }
            };
        }

        private static List<ProcedureFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildCreateFieldMetadata();
            fields.Add(new ProcedureFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status Aktif",
                DataType = "bool",
                InputType = "switch"
            });

            return fields;
        }

        private static ProcedureCreateResponse ToCreateUpdateResponse(MstProcedure entity)
        {
            return new ProcedureCreateResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName,
                ProcedureType = entity.ProcedureType,
                DefaultTariffId = entity.DefaultTariffId,
                IsActive = entity.IsActive
            };
        }

        private static ProcedureUpdateResponse ToUpdateResponse(MstProcedure entity)
        {
            return new ProcedureUpdateResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName,
                ProcedureType = entity.ProcedureType,
                DefaultTariffId = entity.DefaultTariffId,
                IsActive = entity.IsActive
            };
        }

        private static string NormalizeProcedureType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedProcedureTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "General";
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

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
