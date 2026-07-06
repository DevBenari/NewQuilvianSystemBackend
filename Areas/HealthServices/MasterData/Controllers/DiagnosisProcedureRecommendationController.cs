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
using System.Security.Claims;

using ResponseDiagnosisProcedureRecommendationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DiagnosisProcedureRecommendationResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/diagnosis-procedure-recommendations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Diagnosis Procedure Recommendation",
        AreaName = "HealthServices",
        ControllerName = "DiagnosisProcedureRecommendation",
        Description = "Rekomendasi tindakan dan penunjang berdasarkan diagnosis ICD",
        SortOrder = 13
    )]
    [Tags("Health Services / Master Data / Diagnosis Procedure Recommendation")]
    public class DiagnosisProcedureRecommendationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private static readonly string[] RecommendationTypeOptions = { "Procedure", "Lab", "Radiology", "Monitoring", "Referral", "FollowUp" };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DiagnosisProcedureRecommendationController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisProcedureRecommendationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Procedure Recommendation", Description = "Melihat metadata filter rekomendasi tindakan diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisProcedureRecommendation", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DiagnosisProcedureRecommendationFilterMetadataResponse
            {
                DefaultFilter = new DiagnosisProcedureRecommendationDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = BuildSortOptions(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RecommendationTypeOptions = RecommendationTypeOptions.Select(x => new DiagnosisProcedureRecommendationStringOptionResponse { Value = x, Label = BuildLabel(x) }).ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(LogCategory, "DiagnosisProcedureRecommendation.GetFilterMetadata", "Mengambil metadata filter rekomendasi tindakan diagnosis.", result);
            return Ok(ApiResponse<DiagnosisProcedureRecommendationFilterMetadataResponse>.Ok(result, "Metadata filter rekomendasi tindakan diagnosis berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisProcedureRecommendationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Procedure Recommendation", Description = "Melihat ringkasan rekomendasi tindakan diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisProcedureRecommendation", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDiagnosisProcedureRecommendation>().AsNoTracking().Where(x => !x.IsDelete);
            var result = new DiagnosisProcedureRecommendationSummaryResponse
            {
                TotalRecommendation = await query.CountAsync(),
                ActiveRecommendation = await query.CountAsync(x => x.IsActive),
                InactiveRecommendation = await query.CountAsync(x => !x.IsActive),
                ProcedureRecommendation = await query.CountAsync(x => x.RecommendationType == "Procedure"),
                LabRecommendation = await query.CountAsync(x => x.RecommendationType == "Lab"),
                RadiologyRecommendation = await query.CountAsync(x => x.RecommendationType == "Radiology"),
                MonitoringRecommendation = await query.CountAsync(x => x.RecommendationType == "Monitoring"),
                ReferralRecommendation = await query.CountAsync(x => x.RecommendationType == "Referral"),
                FollowUpRecommendation = await query.CountAsync(x => x.RecommendationType == "FollowUp")
            };
            return Ok(ApiResponse<DiagnosisProcedureRecommendationSummaryResponse>.Ok(result, "Ringkasan rekomendasi tindakan diagnosis berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDiagnosisProcedureRecommendationPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Diagnosis Procedure Recommendation", Description = "Melihat data rekomendasi tindakan diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisProcedureRecommendation", "Read")]
        public async Task<IActionResult> GetRecommendations(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] Guid? diagnosisId,
            [FromQuery] Guid? procedureId,
            [FromQuery] string? recommendationType,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "diagnosisCode",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;
            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);
            if (!dateRange.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, dateRange.ErrorMessage ?? "Filter tanggal tidak valid."));

            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(query, search, diagnosisId, procedureId, recommendationType, isActive);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<ResponseDiagnosisProcedureRecommendationPagedResult>.Ok(new ResponseDiagnosisProcedureRecommendationPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data rekomendasi tindakan diagnosis berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<DiagnosisProcedureRecommendationOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Procedure Recommendation", Description = "Melihat pilihan rekomendasi tindakan diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisProcedureRecommendation", "Read")]
        public async Task<IActionResult> GetRecommendationOptions([FromQuery] Guid? diagnosisId, [FromQuery] string? recommendationType, [FromQuery] bool onlyActive = true, [FromQuery] string? search = null, [FromQuery] int take = 100)
        {
            if (take <= 0) take = 100;
            if (take > 200) take = 200;
            var query = BuildBaseQuery();
            query = ApplyStandardFilter(query, search, diagnosisId, null, recommendationType, onlyActive ? true : null);

            var optionEntities = await ApplySorting(query, "diagnosisCode", "asc").Take(take).ToListAsync();
            var data = optionEntities.Select(x => new DiagnosisProcedureRecommendationOptionResponse
            {
                Id = x.Id,
                DiagnosisId = x.DiagnosisId,
                DiagnosisCode = x.Diagnosis?.DiagnosisCode ?? string.Empty,
                DiagnosisName = x.Diagnosis?.DiagnosisName ?? string.Empty,
                ProcedureId = x.ProcedureId,
                RecommendationType = x.RecommendationType,
                RecommendationTypeName = BuildLabel(x.RecommendationType),
                RecommendationName = x.RecommendationName,
                InstructionText = x.InstructionText,
                IsActive = x.IsActive
            }).ToList();

            return Ok(ApiResponse<List<DiagnosisProcedureRecommendationOptionResponse>>.Ok(data, "Data pilihan rekomendasi tindakan diagnosis berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisProcedureRecommendationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Diagnosis Procedure Recommendation", Description = "Melihat detail rekomendasi tindakan diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisProcedureRecommendation", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi tindakan diagnosis tidak ditemukan."));
            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            return Ok(ApiResponse<DiagnosisProcedureRecommendationDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail rekomendasi tindakan diagnosis berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisProcedureRecommendationCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Diagnosis Procedure Recommendation", Description = "Membuat rekomendasi tindakan diagnosis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DiagnosisProcedureRecommendation", "Create")]
        public async Task<IActionResult> CreateRecommendation([FromBody] CreateDiagnosisProcedureRecommendationRequest request)
        {
            var normalized = NormalizeRequest(request);
            var validation = await ValidateRequestAsync(null, normalized);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data rekomendasi tindakan diagnosis tidak valid."));

            var actorUserId = GetCurrentUserId();
            var entity = new MstDiagnosisProcedureRecommendation
            {
                Id = Guid.NewGuid(),
                DiagnosisId = normalized.DiagnosisId,
                ProcedureId = NormalizeNullableGuid(normalized.ProcedureId),
                RecommendationType = normalized.RecommendationType,
                RecommendationName = normalized.RecommendationName,
                InstructionText = normalized.InstructionText,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.Set<MstDiagnosisProcedureRecommendation>().Add(entity);
            await _dbContext.SaveChangesAsync();
            var loaded = await BuildBaseQuery().FirstAsync(x => x.Id == entity.Id);
            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = ToCreateResponse(loaded, actorNames);
            await _loggerService.InfoAsync(LogCategory, "DiagnosisProcedureRecommendation.CreateRecommendation", "Membuat rekomendasi tindakan diagnosis.", result);
            return Ok(ApiResponse<DiagnosisProcedureRecommendationCreateResponse>.Ok(result, "Rekomendasi tindakan diagnosis berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisProcedureRecommendationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Procedure Recommendation", Description = "Mengubah rekomendasi tindakan diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisProcedureRecommendation", "Update")]
        public async Task<IActionResult> UpdateRecommendation(Guid id, [FromBody] UpdateDiagnosisProcedureRecommendationRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosisProcedureRecommendation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi tindakan diagnosis tidak ditemukan."));
            var normalized = NormalizeRequest(request);
            var validation = await ValidateRequestAsync(id, normalized);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data rekomendasi tindakan diagnosis tidak valid."));

            var actorUserId = GetCurrentUserId();
            entity.DiagnosisId = normalized.DiagnosisId;
            entity.ProcedureId = NormalizeNullableGuid(normalized.ProcedureId);
            entity.RecommendationType = normalized.RecommendationType;
            entity.RecommendationName = normalized.RecommendationName;
            entity.InstructionText = normalized.InstructionText;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
            var loaded = await BuildBaseQuery().FirstAsync(x => x.Id == entity.Id);
            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = ToUpdateResponse(loaded, actorNames);
            await _loggerService.InfoAsync(LogCategory, "DiagnosisProcedureRecommendation.UpdateRecommendation", "Mengubah rekomendasi tindakan diagnosis.", result);
            return Ok(ApiResponse<DiagnosisProcedureRecommendationUpdateResponse>.Ok(result, "Rekomendasi tindakan diagnosis berhasil diubah."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisProcedureRecommendationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Procedure Recommendation", Description = "Mengubah status rekomendasi tindakan diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisProcedureRecommendation", "Update")]
        public async Task<IActionResult> UpdateRecommendationStatus(Guid id, [FromBody] UpdateDiagnosisProcedureRecommendationStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosisProcedureRecommendation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi tindakan diagnosis tidak ditemukan."));
            var actorUserId = GetCurrentUserId();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
            var loaded = await BuildBaseQuery().FirstAsync(x => x.Id == entity.Id);
            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            return Ok(ApiResponse<DiagnosisProcedureRecommendationUpdateResponse>.Ok(ToUpdateResponse(loaded, actorNames), "Status rekomendasi tindakan diagnosis berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisProcedureRecommendationDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Diagnosis Procedure Recommendation", Description = "Menghapus rekomendasi tindakan diagnosis", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DiagnosisProcedureRecommendation", "Delete")]
        public async Task<IActionResult> DeleteRecommendation(Guid id, [FromBody] DeleteDiagnosisProcedureRecommendationRequest? request = null)
        {
            var entity = await _dbContext.Set<MstDiagnosisProcedureRecommendation>().Include(x => x.Diagnosis).FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi tindakan diagnosis tidak ditemukan."));
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = entity.DeleteDateTime;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new DiagnosisProcedureRecommendationDeleteResponse
            {
                Id = entity.Id,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.Diagnosis?.DiagnosisCode ?? string.Empty,
                DiagnosisName = entity.Diagnosis?.DiagnosisName ?? string.Empty,
                RecommendationName = entity.RecommendationName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };
            await _loggerService.InfoAsync(LogCategory, "DiagnosisProcedureRecommendation.DeleteRecommendation", "Menghapus rekomendasi tindakan diagnosis.", new { result, request?.DeleteReason });
            return Ok(ApiResponse<DiagnosisProcedureRecommendationDeleteResponse>.Ok(result, "Rekomendasi tindakan diagnosis berhasil dihapus."));
        }

        private IQueryable<MstDiagnosisProcedureRecommendation> BuildBaseQuery()
        {
            return _dbContext.Set<MstDiagnosisProcedureRecommendation>().AsNoTracking().Include(x => x.Diagnosis).Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDiagnosisProcedureRecommendation> ApplyDateFilter(IQueryable<MstDiagnosisProcedureRecommendation> query, DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstDiagnosisProcedureRecommendation> ApplyStandardFilter(IQueryable<MstDiagnosisProcedureRecommendation> query, string? search, Guid? diagnosisId, Guid? procedureId, string? recommendationType, bool? isActive)
        {
            var normalizedDiagnosisId = NormalizeNullableGuid(diagnosisId);
            if (normalizedDiagnosisId.HasValue) query = query.Where(x => x.DiagnosisId == normalizedDiagnosisId.Value);
            var normalizedProcedureId = NormalizeNullableGuid(procedureId);
            if (normalizedProcedureId.HasValue) query = query.Where(x => x.ProcedureId == normalizedProcedureId.Value);
            if (!string.IsNullOrWhiteSpace(recommendationType)) query = query.Where(x => x.RecommendationType == NormalizeRecommendationType(recommendationType));
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RecommendationType.ToLower().Contains(keyword) ||
                    x.RecommendationName.ToLower().Contains(keyword) ||
                    (x.InstructionText != null && x.InstructionText.ToLower().Contains(keyword)) ||
                    (x.Diagnosis != null && x.Diagnosis.DiagnosisCode.ToLower().Contains(keyword)) ||
                    (x.Diagnosis != null && x.Diagnosis.DiagnosisName.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstDiagnosisProcedureRecommendation> ApplySorting(IQueryable<MstDiagnosisProcedureRecommendation> query, string? sortBy, string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "diagnosisCode").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "diagnosiscode" => isDesc ? query.OrderByDescending(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty) : query.OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty),
                "diagnosisname" => isDesc ? query.OrderByDescending(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisName : string.Empty) : query.OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisName : string.Empty),
                "recommendationtype" => isDesc ? query.OrderByDescending(x => x.RecommendationType) : query.OrderBy(x => x.RecommendationType),
                "recommendationname" => isDesc ? query.OrderByDescending(x => x.RecommendationName) : query.OrderBy(x => x.RecommendationName),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc ? query.OrderByDescending(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty).ThenByDescending(x => x.RecommendationName) : query.OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty).ThenBy(x => x.RecommendationName)
            };
        }

        private static DiagnosisProcedureRecommendationResponse MapResponse(MstDiagnosisProcedureRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisProcedureRecommendationResponse
            {
                Id = entity.Id,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.Diagnosis?.DiagnosisCode ?? string.Empty,
                DiagnosisName = entity.Diagnosis?.DiagnosisName ?? string.Empty,
                ProcedureId = entity.ProcedureId,
                RecommendationType = entity.RecommendationType,
                RecommendationTypeName = BuildLabel(entity.RecommendationType),
                RecommendationName = entity.RecommendationName,
                InstructionText = entity.InstructionText,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DiagnosisProcedureRecommendationDetailResponse MapDetailResponse(MstDiagnosisProcedureRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, actorNames);
            return new DiagnosisProcedureRecommendationDetailResponse
            {
                Id = response.Id,
                DiagnosisId = response.DiagnosisId,
                DiagnosisCode = response.DiagnosisCode,
                DiagnosisName = response.DiagnosisName,
                ProcedureId = response.ProcedureId,
                RecommendationType = response.RecommendationType,
                RecommendationTypeName = response.RecommendationTypeName,
                RecommendationName = response.RecommendationName,
                InstructionText = response.InstructionText,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static DiagnosisProcedureRecommendationCreateResponse ToCreateResponse(MstDiagnosisProcedureRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, actorNames);
            return new DiagnosisProcedureRecommendationCreateResponse
            {
                Id = response.Id,
                DiagnosisId = response.DiagnosisId,
                DiagnosisCode = response.DiagnosisCode,
                DiagnosisName = response.DiagnosisName,
                ProcedureId = response.ProcedureId,
                RecommendationType = response.RecommendationType,
                RecommendationTypeName = response.RecommendationTypeName,
                RecommendationName = response.RecommendationName,
                InstructionText = response.InstructionText,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName
            };
        }

        private static DiagnosisProcedureRecommendationUpdateResponse ToUpdateResponse(MstDiagnosisProcedureRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapDetailResponse(entity, actorNames);
            return new DiagnosisProcedureRecommendationUpdateResponse
            {
                Id = response.Id,
                DiagnosisId = response.DiagnosisId,
                DiagnosisCode = response.DiagnosisCode,
                DiagnosisName = response.DiagnosisName,
                ProcedureId = response.ProcedureId,
                RecommendationType = response.RecommendationType,
                RecommendationTypeName = response.RecommendationTypeName,
                RecommendationName = response.RecommendationName,
                InstructionText = response.InstructionText,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = response.UpdateDateTime,
                UpdateBy = response.UpdateBy,
                UpdateByName = response.UpdateByName
            };
        }

        private static CreateDiagnosisProcedureRecommendationRequest NormalizeRequest(CreateDiagnosisProcedureRecommendationRequest request)
        {
            return new CreateDiagnosisProcedureRecommendationRequest
            {
                DiagnosisId = request.DiagnosisId,
                ProcedureId = NormalizeNullableGuid(request.ProcedureId),
                RecommendationType = NormalizeRecommendationType(request.RecommendationType),
                RecommendationName = NormalizeRequiredText(request.RecommendationName),
                InstructionText = NormalizeNullableText(request.InstructionText)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? currentId, CreateDiagnosisProcedureRecommendationRequest request)
        {
            if (request.DiagnosisId == Guid.Empty) return (false, "Diagnosis wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.RecommendationName)) return (false, "Nama rekomendasi wajib diisi.");
            if (!RecommendationTypeOptions.Contains(request.RecommendationType, StringComparer.OrdinalIgnoreCase)) return (false, "Tipe rekomendasi tidak valid.");
            var diagnosisExists = await _dbContext.Set<MstDiagnosis>().AnyAsync(x => !x.IsDelete && x.IsActive && x.Id == request.DiagnosisId);
            if (!diagnosisExists) return (false, "Diagnosis tidak valid atau tidak aktif.");
            if (request.ProcedureId.HasValue)
            {
                var procedureExists = await _dbContext.Set<MstProcedure>().AnyAsync(x => !x.IsDelete && x.IsActive && x.Id == request.ProcedureId.Value);
                if (!procedureExists) return (false, "Tindakan tidak valid atau tidak aktif.");
            }
            var duplicate = await _dbContext.Set<MstDiagnosisProcedureRecommendation>()
                .AnyAsync(x => !x.IsDelete && x.DiagnosisId == request.DiagnosisId && x.RecommendationType == request.RecommendationType && x.RecommendationName.ToLower() == request.RecommendationName.ToLower() && (!currentId.HasValue || x.Id != currentId.Value));
            if (duplicate) return (false, "Rekomendasi tindakan dengan diagnosis, tipe, dan nama yang sama sudah ada.");
            return (true, null);
        }

        private static string NormalizeRecommendationType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Procedure";
            var normalized = value.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
            return normalized switch
            {
                "PROCEDURE" => "Procedure",
                "LAB" => "Lab",
                "RADIOLOGY" => "Radiology",
                "MONITORING" => "Monitoring",
                "REFERRAL" => "Referral",
                "FOLLOWUP" => "FollowUp",
                _ => value.Trim()
            };
        }

        private static string BuildLabel(string value)
        {
            return value switch
            {
                "Procedure" => "Tindakan",
                "Lab" => "Laboratorium",
                "Radiology" => "Radiologi",
                "Monitoring" => "Monitoring",
                "Referral" => "Rujukan",
                "FollowUp" => "Follow Up",
                _ => value
            };
        }

        private static Guid? NormalizeNullableGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string NormalizeRequiredText(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        private static string? NormalizeNullableText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }
        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<Guid, string?>();
            return await _dbContext.Users.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode }).ToDictionaryAsync(x => x.Id, x => x.Name);
        }
        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId) => actorId == Guid.Empty ? null : actorNames.TryGetValue(actorId, out var actorName) ? actorName : null;
        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }
        private static DateRangeResult ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) && !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = AppDateTimeHelper.OperationalDate();
                return customPeriod.Trim().ToLowerInvariant() switch
                {
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1)),
                    "lastmonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }
            var start = startDate?.Date;
            var endExclusive = endDate?.Date.AddDays(1);
            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value) return DateRangeResult.Invalid("StartDate tidak boleh lebih besar atau sama dengan EndDate.");
            return DateRangeResult.Valid(start, endExclusive);
        }
        private static List<DiagnosisProcedureRecommendationCustomPeriodOptionResponse> BuildCustomPeriodOptions() => new()
        {
            new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
            new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
        };
        private static List<DiagnosisProcedureRecommendationSortOptionResponse> BuildSortOptions() => new()
        {
            new() { Value = "createDateTime", Label = "Tanggal dibuat" },
            new() { Value = "diagnosisCode", Label = "Kode diagnosis" },
            new() { Value = "diagnosisName", Label = "Nama diagnosis" },
            new() { Value = "recommendationType", Label = "Tipe rekomendasi" },
            new() { Value = "recommendationName", Label = "Nama rekomendasi" },
            new() { Value = "isActive", Label = "Status aktif" }
        };
        private static List<DiagnosisProcedureRecommendationQueryParameterInfoResponse> BuildQueryParameterInfo() => new()
        {
            new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
            new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
            new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
            new() { Name = "search", Type = "string", Description = "Cari diagnosis, tipe rekomendasi, nama rekomendasi, atau instruksi." },
            new() { Name = "diagnosisId", Type = "Guid?", Description = "Filter berdasarkan diagnosis." },
            new() { Name = "procedureId", Type = "Guid?", Description = "Filter berdasarkan master tindakan jika ada." },
            new() { Name = "recommendationType", Type = "string", Description = "Filter tipe rekomendasi.", Example = "Lab" },
            new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif.", Example = "true" }
        };
        private static List<DiagnosisProcedureRecommendationFormFieldMetadataResponse> BuildCreateFieldMetadata() => BuildFieldMetadata(false);
        private static List<DiagnosisProcedureRecommendationFormFieldMetadataResponse> BuildUpdateFieldMetadata() => BuildFieldMetadata(true);
        private static List<DiagnosisProcedureRecommendationFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<DiagnosisProcedureRecommendationFormFieldMetadataResponse>
            {
                new() { Name = "diagnosisId", Label = "Diagnosis", Section = "Relation", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", SortOrder = 1 },
                new() { Name = "procedureId", Label = "Master Tindakan", Section = "Relation", InputType = "select", Description = "Opsional. Dapat kosong untuk rekomendasi lab/radiologi/monitoring sederhana.", SortOrder = 2 },
                new() { Name = "recommendationType", Label = "Tipe Rekomendasi", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "recommendationTypeOptions", Example = "Lab", SortOrder = 3 },
                new() { Name = "recommendationName", Label = "Nama Rekomendasi", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 250, Example = "Pemeriksaan darah lengkap", SortOrder = 4 },
                new() { Name = "instructionText", Label = "Instruksi", Section = "Instruction", InputType = "textarea", MaxLength = 1000, SortOrder = 5 }
            };
            if (isUpdate) fields.Add(new DiagnosisProcedureRecommendationFormFieldMetadataResponse { Name = "isActive", Label = "Status Aktif", Section = "Status", InputType = "switch", SortOrder = 99 });
            return fields.OrderBy(x => x.SortOrder).ToList();
        }
        private sealed class DateRangeResult
        {
            public bool IsValid { get; private init; }
            public DateTime? Start { get; private init; }
            public DateTime? EndExclusive { get; private init; }
            public string? ErrorMessage { get; private init; }
            public static DateRangeResult Valid(DateTime? start, DateTime? endExclusive) => new() { IsValid = true, Start = start, EndExclusive = endExclusive };
            public static DateRangeResult Invalid(string errorMessage) => new() { IsValid = false, ErrorMessage = errorMessage };
        }
    }
}
