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
                ProcedureTypeOptions = AllowedProcedureTypes.OrderBy(x => x).ToList()
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
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? procedureGroupName,
            [FromQuery] string? procedureCategoryName,
            [FromQuery] string? procedureType,
            [FromQuery] Guid? defaultTariffId,
            [FromQuery] bool? hasDefaultTariff,
            [FromQuery] bool? isDoctorAction,
            [FromQuery] bool? isNursingAction,
            [FromQuery] bool? isSurgery,
            [FromQuery] bool? isLaboratory,
            [FromQuery] bool? isRadiology,
            [FromQuery] bool? isTherapy,
            [FromQuery] bool? isNeedDoctor,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] bool? isAvailableForOutpatient,
            [FromQuery] bool? isAvailableForInpatient,
            [FromQuery] bool? isAvailableForEmergency,
            [FromQuery] int? minimumEstimatedDurationMinutes,
            [FromQuery] int? maximumEstimatedDurationMinutes,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilter(
                query,
                search,
                isActive,
                procedureGroupName,
                procedureCategoryName,
                procedureType,
                defaultTariffId,
                hasDefaultTariff,
                isDoctorAction,
                isNursingAction,
                isSurgery,
                isLaboratory,
                isRadiology,
                isTherapy,
                isNeedDoctor,
                isNeedApproval,
                isCoveredByInsuranceDefault,
                isAvailableForOutpatient,
                isAvailableForInpatient,
                isAvailableForEmergency,
                minimumEstimatedDurationMinutes,
                maximumEstimatedDurationMinutes
            );

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
        [ProducesResponseType(typeof(ApiResponse<List<ProcedureOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Procedure", Description = "Melihat data procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetProcedureOptions(
            [FromQuery] string? procedureGroupName,
            [FromQuery] string? procedureCategoryName,
            [FromQuery] string? procedureType,
            [FromQuery] Guid? defaultTariffId,
            [FromQuery] bool? hasDefaultTariff,
            [FromQuery] bool? isDoctorAction,
            [FromQuery] bool? isNursingAction,
            [FromQuery] bool? isSurgery,
            [FromQuery] bool? isLaboratory,
            [FromQuery] bool? isRadiology,
            [FromQuery] bool? isTherapy,
            [FromQuery] bool? isNeedDoctor,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] bool? isAvailableForOutpatient,
            [FromQuery] bool? isAvailableForInpatient,
            [FromQuery] bool? isAvailableForEmergency,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            query = ApplyFilter(
                query,
                search,
                onlyActive ? true : null,
                procedureGroupName,
                procedureCategoryName,
                procedureType,
                defaultTariffId,
                hasDefaultTariff,
                isDoctorAction,
                isNursingAction,
                isSurgery,
                isLaboratory,
                isRadiology,
                isTherapy,
                isNeedDoctor,
                isNeedApproval,
                isCoveredByInsuranceDefault,
                isAvailableForOutpatient,
                isAvailableForInpatient,
                isAvailableForEmergency,
                null,
                null
            );

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ProcedureName)
                .Select(x => new ProcedureOptionResponse
                {
                    Id = x.Id,
                    ProcedureCode = x.ProcedureCode,
                    ProcedureName = x.ProcedureName,
                    ProcedureGroupName = x.ProcedureGroupName,
                    ProcedureCategoryName = x.ProcedureCategoryName,
                    ProcedureType = x.ProcedureType,
                    DefaultTariffId = x.DefaultTariffId,
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
                    IsAvailableForEmergency = x.IsAvailableForEmergency
                })
                .ToListAsync();

            return Ok(ApiResponse<List<ProcedureOptionResponse>>.Ok(
                data,
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
        [AccessAction("Create", "Create Procedure", Description = "Membuat data procedure", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Procedure", "Create")]
        public async Task<IActionResult> CreateProcedure([FromBody] CreateProcedureRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                procedureCode: request.ProcedureCode,
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
                ProcedureCode = request.ProcedureCode.Trim().ToUpperInvariant(),
                ProcedureName = request.ProcedureName.Trim(),
                ProcedureGroupName = NormalizeNullableString(request.ProcedureGroupName),
                ProcedureCategoryName = NormalizeNullableString(request.ProcedureCategoryName),
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
                ExternalProcedureCode = NormalizeNullableString(request.ExternalProcedureCode),
                IntegrationCode = NormalizeNullableString(request.IntegrationCode),
                SortOrder = request.SortOrder,
                ClinicalNoteTemplate = NormalizeNullableString(request.ClinicalNoteTemplate),
                Description = NormalizeNullableString(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstProcedure>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new ProcedureCreateResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Procedure.CreateProcedure",
                "Membuat data procedure.",
                result
            );

            return Ok(ApiResponse<ProcedureCreateResponse>.Ok(
                result,
                "Procedure berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureUpdateResponse>), StatusCodes.Status200OK)]
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
                procedureCode: request.ProcedureCode,
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

            entity.ProcedureCode = request.ProcedureCode.Trim().ToUpperInvariant();
            entity.ProcedureName = request.ProcedureName.Trim();
            entity.ProcedureGroupName = NormalizeNullableString(request.ProcedureGroupName);
            entity.ProcedureCategoryName = NormalizeNullableString(request.ProcedureCategoryName);
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
            entity.ExternalProcedureCode = NormalizeNullableString(request.ExternalProcedureCode);
            entity.IntegrationCode = NormalizeNullableString(request.IntegrationCode);
            entity.SortOrder = request.SortOrder;
            entity.ClinicalNoteTemplate = NormalizeNullableString(request.ClinicalNoteTemplate);
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new ProcedureUpdateResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Procedure.UpdateProcedure",
                "Mengubah data procedure.",
                result
            );

            return Ok(ApiResponse<ProcedureUpdateResponse>.Ok(
                result,
                "Procedure berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Procedure", Description = "Mengaktifkan data procedure", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Procedure", "Update")]
        public async Task<IActionResult> ActivateProcedure(Guid id)
        {
            return await UpdateStatusAsync(id, true, "Procedure berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Procedure", Description = "Menonaktifkan data procedure", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Procedure", "Update")]
        public async Task<IActionResult> DeactivateProcedure(Guid id)
        {
            return await UpdateStatusAsync(id, false, "Procedure berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureDeleteResponse>), StatusCodes.Status200OK)]
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new ProcedureDeleteResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Procedure.DeleteProcedure",
                "Menghapus data procedure.",
                result
            );

            return Ok(ApiResponse<ProcedureDeleteResponse>.Ok(
                result,
                "Procedure berhasil dihapus."
            ));
        }

        private async Task<IActionResult> UpdateStatusAsync(Guid id, bool isActive, string successMessage)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = isActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new ProcedureStatusResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Procedure.UpdateStatus",
                successMessage,
                result
            );

            return Ok(ApiResponse<ProcedureStatusResponse>.Ok(
                result,
                successMessage
            ));
        }

        private IQueryable<MstProcedure> BuildBaseQuery()
        {
            return _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Include(x => x.DefaultTariff)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstProcedure> ApplyFilter(
            IQueryable<MstProcedure> query,
            string? search,
            bool? isActive,
            string? procedureGroupName,
            string? procedureCategoryName,
            string? procedureType,
            Guid? defaultTariffId,
            bool? hasDefaultTariff,
            bool? isDoctorAction,
            bool? isNursingAction,
            bool? isSurgery,
            bool? isLaboratory,
            bool? isRadiology,
            bool? isTherapy,
            bool? isNeedDoctor,
            bool? isNeedApproval,
            bool? isCoveredByInsuranceDefault,
            bool? isAvailableForOutpatient,
            bool? isAvailableForInpatient,
            bool? isAvailableForEmergency,
            int? minimumEstimatedDurationMinutes,
            int? maximumEstimatedDurationMinutes)
        {
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

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(procedureGroupName))
            {
                var keyword = procedureGroupName.Trim().ToLower();
                query = query.Where(x => x.ProcedureGroupName != null && x.ProcedureGroupName.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(procedureCategoryName))
            {
                var keyword = procedureCategoryName.Trim().ToLower();
                query = query.Where(x => x.ProcedureCategoryName != null && x.ProcedureCategoryName.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(procedureType))
            {
                var normalizedType = procedureType.Trim().ToLower();
                query = query.Where(x => x.ProcedureType.ToLower() == normalizedType);
            }

            if (defaultTariffId.HasValue && defaultTariffId.Value != Guid.Empty)
                query = query.Where(x => x.DefaultTariffId == defaultTariffId.Value);

            if (hasDefaultTariff.HasValue)
            {
                query = hasDefaultTariff.Value
                    ? query.Where(x => x.DefaultTariffId.HasValue)
                    : query.Where(x => !x.DefaultTariffId.HasValue);
            }

            if (isDoctorAction.HasValue)
                query = query.Where(x => x.IsDoctorAction == isDoctorAction.Value);

            if (isNursingAction.HasValue)
                query = query.Where(x => x.IsNursingAction == isNursingAction.Value);

            if (isSurgery.HasValue)
                query = query.Where(x => x.IsSurgery == isSurgery.Value);

            if (isLaboratory.HasValue)
                query = query.Where(x => x.IsLaboratory == isLaboratory.Value);

            if (isRadiology.HasValue)
                query = query.Where(x => x.IsRadiology == isRadiology.Value);

            if (isTherapy.HasValue)
                query = query.Where(x => x.IsTherapy == isTherapy.Value);

            if (isNeedDoctor.HasValue)
                query = query.Where(x => x.IsNeedDoctor == isNeedDoctor.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (isCoveredByInsuranceDefault.HasValue)
                query = query.Where(x => x.IsCoveredByInsuranceDefault == isCoveredByInsuranceDefault.Value);

            if (isAvailableForOutpatient.HasValue)
                query = query.Where(x => x.IsAvailableForOutpatient == isAvailableForOutpatient.Value);

            if (isAvailableForInpatient.HasValue)
                query = query.Where(x => x.IsAvailableForInpatient == isAvailableForInpatient.Value);

            if (isAvailableForEmergency.HasValue)
                query = query.Where(x => x.IsAvailableForEmergency == isAvailableForEmergency.Value);

            if (minimumEstimatedDurationMinutes.HasValue)
                query = query.Where(x => x.EstimatedDurationMinutes >= minimumEstimatedDurationMinutes.Value);

            if (maximumEstimatedDurationMinutes.HasValue)
                query = query.Where(x => x.EstimatedDurationMinutes <= maximumEstimatedDurationMinutes.Value);

            return query;
        }

        private static IOrderedQueryable<MstProcedure> ApplySorting(
            IQueryable<MstProcedure> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "procedurecode" => isDescending
                    ? query.OrderByDescending(x => x.ProcedureCode)
                    : query.OrderBy(x => x.ProcedureCode),

                "procedurename" => isDescending
                    ? query.OrderByDescending(x => x.ProcedureName)
                    : query.OrderBy(x => x.ProcedureName),

                "proceduregroupname" => isDescending
                    ? query.OrderByDescending(x => x.ProcedureGroupName)
                    : query.OrderBy(x => x.ProcedureGroupName),

                "procedurecategoryname" => isDescending
                    ? query.OrderByDescending(x => x.ProcedureCategoryName)
                    : query.OrderBy(x => x.ProcedureCategoryName),

                "proceduretype" => isDescending
                    ? query.OrderByDescending(x => x.ProcedureType)
                    : query.OrderBy(x => x.ProcedureType),

                "estimateddurationminutes" => isDescending
                    ? query.OrderByDescending(x => x.EstimatedDurationMinutes)
                    : query.OrderBy(x => x.EstimatedDurationMinutes),

                "isneeddoctor" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedDoctor)
                    : query.OrderBy(x => x.IsNeedDoctor),

                "isneedapproval" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedApproval)
                    : query.OrderBy(x => x.IsNeedApproval),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ProcedureName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ProcedureName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string procedureCode,
            string procedureName,
            string procedureType,
            Guid? defaultTariffId,
            int estimatedDurationMinutes)
        {
            if (string.IsNullOrWhiteSpace(procedureCode))
                return (false, "Kode procedure wajib diisi.");

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

            var normalizedCode = procedureCode.Trim().ToUpperInvariant();
            var normalizedName = procedureName.Trim().ToLower();

            var duplicateCodeQuery = _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ProcedureCode.ToUpper() == normalizedCode);

            if (excludeId.HasValue)
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateCodeQuery.AnyAsync())
                return (false, "Kode procedure sudah digunakan.");

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

        private static string? NormalizeNullableString(string? value)
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