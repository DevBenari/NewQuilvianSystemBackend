using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
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
        [AccessAction("Read", "Read Procedure", Description = "Melihat metadata filter procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ProcedureFilterMetadataResponse
            {
                DefaultFilter = new ProcedureDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = BuildSortOptions(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ProcedureTypeOptions = AllowedProcedureTypes
                    .OrderBy(x => x)
                    .Select(x => new ProcedureStringOptionResponse
                    {
                        Value = x,
                        Label = BuildProcedureTypeLabel(x)
                    })
                    .ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
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
        [AccessAction("Read", "Read Procedure", Description = "Melihat ringkasan procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

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
                AvailableForEmergencyProcedure = await query.CountAsync(x => x.IsAvailableForEmergency)
            };

            return Ok(ApiResponse<ProcedureSummaryResponse>.Ok(
                result,
                "Ringkasan procedure berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseProcedurePagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Procedure", Description = "Melihat data procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetProcedures(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isActive,
            [FromQuery] string? procedureType,
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
            query = ApplyDateFilter(query, dateRange.Start, dateRange.EndExclusive);
            query = ApplyStandardFilter(
                query,
                isActive,
                procedureType,
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
                search
            );

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
                .Select(x => MapResponse(x, actorNames))
                .ToList();

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
        [AccessAction("Read", "Read Procedure", Description = "Melihat data pilihan procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetProcedureOptions(
            [FromQuery] string? procedureType = null,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? isDoctorAction = null,
            [FromQuery] bool? isNursingAction = null,
            [FromQuery] bool? isSurgery = null,
            [FromQuery] bool? isLaboratory = null,
            [FromQuery] bool? isRadiology = null,
            [FromQuery] bool? isTherapy = null,
            [FromQuery] bool? isNeedDoctor = null,
            [FromQuery] bool? isNeedApproval = null,
            [FromQuery] bool? isAvailableForOutpatient = null,
            [FromQuery] bool? isAvailableForInpatient = null,
            [FromQuery] bool? isAvailableForEmergency = null,
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
                onlyActive ? true : null,
                procedureType,
                isDoctorAction,
                isNursingAction,
                isSurgery,
                isLaboratory,
                isRadiology,
                isTherapy,
                isNeedDoctor,
                isNeedApproval,
                null,
                isAvailableForOutpatient,
                isAvailableForInpatient,
                isAvailableForEmergency,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ProcedureName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(MapOptionResponse).ToList();

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
        [AccessAction("Read", "Read Procedure", Description = "Melihat detail procedure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Procedure", "Read")]
        public async Task<IActionResult> GetProcedureById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Procedure tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

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
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data procedure tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var entity = new MstProcedure
            {
                Id = Guid.NewGuid(),
                ProcedureCode = await GenerateProcedureCodeAsync(),
                ProcedureName = request.ProcedureName.Trim(),
                ProcedureGroupName = NormalizeNullableText(request.ProcedureGroupName),
                ProcedureCategoryName = NormalizeNullableText(request.ProcedureCategoryName),
                ProcedureType = NormalizeProcedureType(request.ProcedureType),
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
            await transaction.CommitAsync();

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

            var validation = await ValidateRequestAsync(id, request);

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

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Procedure Status", Description = "Mengubah status procedure", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Procedure", "Update")]
        public async Task<IActionResult> UpdateProcedureStatus(
            Guid id,
            [FromBody] UpdateProcedureStatusRequest request)
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

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<ProcedureUpdateResponse>.Ok(
                response,
                "Status procedure berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProcedureDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Procedure", Description = "Menghapus data procedure", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Procedure", "Delete")]
        public async Task<IActionResult> DeleteProcedure(
            Guid id,
            [FromBody] DeleteProcedureRequest? request = null)
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

            var isUsedByInsuranceTariff = await _dbContext.Set<MstInsuranceTariff>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.Tariff != null &&
                    x.Tariff.ProcedureId == id);

            var isUsedByCoverageRule = await _dbContext.Set<MstInsuranceCoverageRule>()
                .AnyAsync(x => x.ProcedureId == id && !x.IsDelete);

            var isUsedByDoctorServiceRule = await _dbContext.Set<MstDoctorServiceRule>()
                .AnyAsync(x => x.ProcedureId == id && !x.IsDelete);

            if (isUsedByTariff || isUsedByInsuranceTariff || isUsedByCoverageRule || isUsedByDoctorServiceRule)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Procedure tidak dapat dihapus karena sudah digunakan oleh tariff, insurance tariff, coverage rule, atau doctor service rule."
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

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var response = new ProcedureDeleteResponse
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
                response
            );

            return Ok(ApiResponse<ProcedureDeleteResponse>.Ok(
                response,
                "Procedure berhasil dihapus."
            ));
        }

        private IQueryable<MstProcedure> BuildBaseQuery()
        {
            return _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstProcedure> ApplyDateFilter(
            IQueryable<MstProcedure> query,
            DateTime? start,
            DateTime? endExclusive)
        {
            if (start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= start.Value);
            }

            if (endExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < endExclusive.Value);
            }

            return query;
        }

        private static IQueryable<MstProcedure> ApplyStandardFilter(
            IQueryable<MstProcedure> query,
            bool? isActive,
            string? procedureType,
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
            string? search)
        {
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(procedureType))
            {
                var normalizedType = NormalizeProcedureType(procedureType);
                query = query.Where(x => x.ProcedureType == normalizedType);
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
                    (x.ClinicalNoteTemplate != null && x.ClinicalNoteTemplate.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateProcedureRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProcedureName))
                return (false, "Nama procedure wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.ProcedureType))
                return (false, "Tipe procedure wajib diisi.");

            if (!AllowedProcedureTypes.Contains(request.ProcedureType.Trim()))
            {
                return (false, "Tipe procedure tidak valid. Gunakan salah satu: General, Nursing, DoctorAction, Surgery, Laboratory, Radiology, Therapy, Other.");
            }

            if (request.EstimatedDurationMinutes < 0)
                return (false, "Estimasi durasi procedure tidak boleh kurang dari 0 menit.");

            if (request.EstimatedDurationMinutes > 1440)
                return (false, "Estimasi durasi procedure tidak boleh lebih dari 1440 menit.");

            var normalizedName = request.ProcedureName.Trim().ToLower();
            var normalizedType = NormalizeProcedureType(request.ProcedureType);
            var normalizedGroup = NormalizeComparableText(request.ProcedureGroupName);
            var normalizedCategory = NormalizeComparableText(request.ProcedureCategoryName);

            var duplicateNameQuery = _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ProcedureName.ToLower() == normalizedName &&
                    x.ProcedureType == normalizedType &&
                    (x.ProcedureGroupName ?? string.Empty).Trim().ToLower() == normalizedGroup &&
                    (x.ProcedureCategoryName ?? string.Empty).Trim().ToLower() == normalizedCategory);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Procedure dengan nama, tipe, group, dan kategori tersebut sudah digunakan.");

            var externalCode = NormalizeNullableText(request.ExternalProcedureCode);
            if (!string.IsNullOrWhiteSpace(externalCode))
            {
                var normalizedExternalCode = externalCode.ToLower();

                var duplicateExternalCodeQuery = _dbContext.Set<MstProcedure>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ExternalProcedureCode != null &&
                        x.ExternalProcedureCode.ToLower() == normalizedExternalCode);

                if (excludeId.HasValue)
                    duplicateExternalCodeQuery = duplicateExternalCodeQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateExternalCodeQuery.AnyAsync())
                    return (false, "External procedure code sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateProcedureCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstProcedure>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ProcedureCode.StartsWith(CodePrefix))
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

        private static IOrderedQueryable<MstProcedure> ApplySorting(
            IQueryable<MstProcedure> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "procedurecode" => isDesc ? query.OrderByDescending(x => x.ProcedureCode) : query.OrderBy(x => x.ProcedureCode),
                "procedurename" => isDesc ? query.OrderByDescending(x => x.ProcedureName) : query.OrderBy(x => x.ProcedureName),
                "proceduregroupname" => isDesc ? query.OrderByDescending(x => x.ProcedureGroupName) : query.OrderBy(x => x.ProcedureGroupName),
                "procedurecategoryname" => isDesc ? query.OrderByDescending(x => x.ProcedureCategoryName) : query.OrderBy(x => x.ProcedureCategoryName),
                "proceduretype" => isDesc ? query.OrderByDescending(x => x.ProcedureType) : query.OrderBy(x => x.ProcedureType),
                "estimateddurationminutes" => isDesc ? query.OrderByDescending(x => x.EstimatedDurationMinutes) : query.OrderBy(x => x.EstimatedDurationMinutes),
                "isneeddoctor" => isDesc ? query.OrderByDescending(x => x.IsNeedDoctor) : query.OrderBy(x => x.IsNeedDoctor),
                "isneedapproval" => isDesc ? query.OrderByDescending(x => x.IsNeedApproval) : query.OrderBy(x => x.IsNeedApproval),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ProcedureName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ProcedureName)
            };
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
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static ProcedureResponse MapResponse(
            MstProcedure entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new ProcedureResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName,
                ProcedureGroupName = entity.ProcedureGroupName,
                ProcedureCategoryName = entity.ProcedureCategoryName,
                ProcedureType = entity.ProcedureType,
                ProcedureTypeName = BuildProcedureTypeLabel(entity.ProcedureType),
                IsDoctorAction = entity.IsDoctorAction,
                IsNursingAction = entity.IsNursingAction,
                IsSurgery = entity.IsSurgery,
                IsLaboratory = entity.IsLaboratory,
                IsRadiology = entity.IsRadiology,
                IsTherapy = entity.IsTherapy,
                IsNeedDoctor = entity.IsNeedDoctor,
                IsNeedApproval = entity.IsNeedApproval,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                IsAvailableForOutpatient = entity.IsAvailableForOutpatient,
                IsAvailableForInpatient = entity.IsAvailableForInpatient,
                IsAvailableForEmergency = entity.IsAvailableForEmergency,
                EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
                ExternalProcedureCode = entity.ExternalProcedureCode,
                IntegrationCode = entity.IntegrationCode,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static ProcedureDetailResponse MapDetailResponse(
            MstProcedure entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new ProcedureDetailResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName,
                ProcedureGroupName = entity.ProcedureGroupName,
                ProcedureCategoryName = entity.ProcedureCategoryName,
                ProcedureType = entity.ProcedureType,
                ProcedureTypeName = BuildProcedureTypeLabel(entity.ProcedureType),
                IsDoctorAction = entity.IsDoctorAction,
                IsNursingAction = entity.IsNursingAction,
                IsSurgery = entity.IsSurgery,
                IsLaboratory = entity.IsLaboratory,
                IsRadiology = entity.IsRadiology,
                IsTherapy = entity.IsTherapy,
                IsNeedDoctor = entity.IsNeedDoctor,
                IsNeedApproval = entity.IsNeedApproval,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                IsAvailableForOutpatient = entity.IsAvailableForOutpatient,
                IsAvailableForInpatient = entity.IsAvailableForInpatient,
                IsAvailableForEmergency = entity.IsAvailableForEmergency,
                EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
                ExternalProcedureCode = entity.ExternalProcedureCode,
                IntegrationCode = entity.IntegrationCode,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                ClinicalNoteTemplate = entity.ClinicalNoteTemplate,
                Description = entity.Description,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static ProcedureOptionResponse MapOptionResponse(MstProcedure entity)
        {
            return new ProcedureOptionResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName,
                ProcedureGroupName = entity.ProcedureGroupName,
                ProcedureCategoryName = entity.ProcedureCategoryName,
                ProcedureType = entity.ProcedureType,
                ProcedureTypeName = BuildProcedureTypeLabel(entity.ProcedureType),
                IsDoctorAction = entity.IsDoctorAction,
                IsNursingAction = entity.IsNursingAction,
                IsSurgery = entity.IsSurgery,
                IsLaboratory = entity.IsLaboratory,
                IsRadiology = entity.IsRadiology,
                IsTherapy = entity.IsTherapy,
                IsNeedDoctor = entity.IsNeedDoctor,
                IsNeedApproval = entity.IsNeedApproval,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                IsAvailableForOutpatient = entity.IsAvailableForOutpatient,
                IsAvailableForInpatient = entity.IsAvailableForInpatient,
                IsAvailableForEmergency = entity.IsAvailableForEmergency,
                EstimatedDurationMinutes = entity.EstimatedDurationMinutes
            };
        }

        private static ProcedureCreateResponse ToCreateUpdateResponse(MstProcedure entity)
        {
            return new ProcedureCreateResponse
            {
                Id = entity.Id,
                ProcedureCode = entity.ProcedureCode,
                ProcedureName = entity.ProcedureName,
                ProcedureType = entity.ProcedureType,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
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
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static string NormalizeProcedureType(string value)
        {
            var trimmed = string.IsNullOrWhiteSpace(value)
                ? "General"
                : value.Trim();

            var matched = AllowedProcedureTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "General";
        }

        private static string BuildProcedureTypeLabel(string value)
        {
            return value switch
            {
                "DoctorAction" => "Doctor Action",
                _ => SplitPascalCase(value)
            };
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
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

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = AppDateTimeHelper.OperationalDate();
                var period = customPeriod.Trim().ToLowerInvariant();

                return period switch
                {
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "yesterday" => DateRangeResult.Valid(today.AddDays(-1), today),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1)),
                    "lastmonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1)),
                    "thisyear" => DateRangeResult.Valid(new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }

            var start = startDate?.Date;
            var endExclusive = endDate?.Date.AddDays(1);

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar dari EndDate.");
            }

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<ProcedureCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<ProcedureCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "yesterday", Label = "Kemarin", Description = "Data yang dibuat kemarin.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisMonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastMonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisYear", Label = "Tahun ini", Description = "Data yang dibuat pada tahun berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static List<ProcedureSortOptionResponse> BuildSortOptions()
        {
            return new List<ProcedureSortOptionResponse>
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
            };
        }

        private static List<ProcedureQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<ProcedureQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Jika bukan custom, startDate dan endDate diabaikan.", Example = "thisMonth" },
                new() { Name = "procedureType", Type = "string", Description = "Filter tipe procedure.", Example = "DoctorAction" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama, group, kategori, tipe, external code, integration code, template, atau deskripsi.", Example = "surgery" },
                new() { Name = "sortBy", Type = "string", Description = "Field sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman maksimal 100.", Example = "25" }
            };
        }

        private static List<ProcedureFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(false);
        }

        private static List<ProcedureFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(true);
        }

        private static List<ProcedureFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<ProcedureFormFieldMetadataResponse>
            {
                new() { Name = "procedureCode", Label = "Kode Procedure", Section = "Basic", InputType = "readonly", RequiredType = "AutoGenerated", MaxLength = 50, Description = "Auto generated oleh sistem dengan format PR-RSMMC-00001.", Example = "PR-RSMMC-00001", SortOrder = 1 },
                new() { Name = "procedureName", Label = "Nama Procedure", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 200, Example = "Konsultasi Dokter Umum", SortOrder = 2 },
                new() { Name = "procedureGroupName", Label = "Group Procedure", Section = "Basic", InputType = "text", MaxLength = 100, SortOrder = 3 },
                new() { Name = "procedureCategoryName", Label = "Kategori Procedure", Section = "Basic", InputType = "text", MaxLength = 100, SortOrder = 4 },
                new() { Name = "procedureType", Label = "Tipe Procedure", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "procedureTypeOptions", SortOrder = 5 },
                new() { Name = "estimatedDurationMinutes", Label = "Estimasi Durasi", Section = "Rule", InputType = "number", Description = "Estimasi durasi tindakan dalam menit.", Example = "15", SortOrder = 6 },
                new() { Name = "isDoctorAction", Label = "Tindakan Dokter", Section = "Rule", InputType = "switch", SortOrder = 7 },
                new() { Name = "isNursingAction", Label = "Tindakan Perawat", Section = "Rule", InputType = "switch", SortOrder = 8 },
                new() { Name = "isSurgery", Label = "Surgery", Section = "Rule", InputType = "switch", SortOrder = 9 },
                new() { Name = "isLaboratory", Label = "Laboratory", Section = "Rule", InputType = "switch", SortOrder = 10 },
                new() { Name = "isRadiology", Label = "Radiology", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isTherapy", Label = "Therapy", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "isNeedDoctor", Label = "Butuh Dokter", Section = "Rule", InputType = "switch", SortOrder = 13 },
                new() { Name = "isNeedApproval", Label = "Butuh Approval", Section = "Rule", InputType = "switch", SortOrder = 14 },
                new() { Name = "isCoveredByInsuranceDefault", Label = "Default Ditanggung Asuransi", Section = "Coverage", InputType = "switch", SortOrder = 15 },
                new() { Name = "isAvailableForOutpatient", Label = "Tersedia Rawat Jalan", Section = "Availability", InputType = "switch", SortOrder = 16 },
                new() { Name = "isAvailableForInpatient", Label = "Tersedia Rawat Inap", Section = "Availability", InputType = "switch", SortOrder = 17 },
                new() { Name = "isAvailableForEmergency", Label = "Tersedia IGD", Section = "Availability", InputType = "switch", SortOrder = 18 },
                new() { Name = "externalProcedureCode", Label = "Kode External", Section = "Integration", InputType = "text", MaxLength = 50, SortOrder = 19 },
                new() { Name = "integrationCode", Label = "Kode Integrasi", Section = "Integration", InputType = "text", MaxLength = 50, SortOrder = 20 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 21 },
                new() { Name = "clinicalNoteTemplate", Label = "Template Catatan Klinis", Section = "Clinical", InputType = "textarea", MaxLength = 500, SortOrder = 22 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 23 }
            };

            if (isUpdate)
            {
                fields.Add(new ProcedureFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Status",
                    InputType = "switch",
                    SortOrder = 99
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string NormalizeComparableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLower();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private init; }
            public DateTime? Start { get; private init; }
            public DateTime? EndExclusive { get; private init; }
            public string? ErrorMessage { get; private init; }

            public static DateRangeResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResult Invalid(string errorMessage)
            {
                return new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
