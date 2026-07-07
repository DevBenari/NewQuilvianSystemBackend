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

using ResponseDiagnosisDrugRecommendationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DiagnosisDrugRecommendationResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/diagnosis-drug-recommendations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Diagnosis Drug Recommendation",
        AreaName = "HealthServices",
        ControllerName = "DiagnosisDrugRecommendation",
        Description = "Rekomendasi obat berdasarkan diagnosis ICD",
        SortOrder = 12
    )]
    [Tags("Health Services / Master Data / Diagnosis Drug Recommendation")]
    public class DiagnosisDrugRecommendationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string ReviewStatusDraft = "DraftFromLiterature";
        private const string ReviewStatusDoctorReviewed = "DoctorReviewed";
        private const string ReviewStatusActiveForSoap = "ActiveForSoap";
        private const string ReviewStatusInactive = "Inactive";

        private static readonly string[] RecommendationTypeOptionsValues = { "FirstLine", "Alternative", "Symptomatic", "Supportive", "Conditional" };
        private static readonly string[] ReviewStatusOptions = { ReviewStatusDraft, ReviewStatusDoctorReviewed, ReviewStatusActiveForSoap, ReviewStatusInactive };
        private static readonly string[] SourceTypeOptions = { "PNPK", "Fornas", "PPK_RS", "ManualDoctorInput", "Other" };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DiagnosisDrugRecommendationController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDrugRecommendationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Drug Recommendation", Description = "Melihat metadata filter rekomendasi obat diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisDrugRecommendation", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DiagnosisDrugRecommendationFilterMetadataResponse
            {
                DefaultFilter = new DiagnosisDrugRecommendationDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = BuildSortOptions(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RecommendationTypeOptions = RecommendationTypeOptionsValues.Select(x => new DiagnosisDrugRecommendationStringOptionResponse { Value = x, Label = BuildLabel(x) }).ToList(),
                ReviewStatusOptions = ReviewStatusOptions.Select(x => new DiagnosisDrugRecommendationStringOptionResponse { Value = x, Label = BuildReviewStatusLabel(x) }).ToList(),
                SourceTypeOptions = SourceTypeOptions.Select(x => new DiagnosisDrugRecommendationStringOptionResponse { Value = x, Label = BuildSourceTypeLabel(x) }).ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(LogCategory, "DiagnosisDrugRecommendation.GetFilterMetadata", "Mengambil metadata filter rekomendasi obat diagnosis.", result);
            return Ok(ApiResponse<DiagnosisDrugRecommendationFilterMetadataResponse>.Ok(result, "Metadata filter rekomendasi obat diagnosis berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDrugRecommendationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Drug Recommendation", Description = "Melihat ringkasan rekomendasi obat diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisDrugRecommendation", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDiagnosisDrugRecommendation>().AsNoTracking().Where(x => !x.IsDelete);
            var result = new DiagnosisDrugRecommendationSummaryResponse
            {
                TotalRecommendation = await query.CountAsync(),
                ActiveRecommendation = await query.CountAsync(x => x.IsActive),
                InactiveRecommendation = await query.CountAsync(x => !x.IsActive),
                DraftFromLiteratureRecommendation = await query.CountAsync(x => x.ReviewStatus == ReviewStatusDraft),
                DoctorReviewedRecommendation = await query.CountAsync(x => x.ReviewStatus == ReviewStatusDoctorReviewed),
                ActiveForSoapRecommendation = await query.CountAsync(x => x.ReviewStatus == ReviewStatusActiveForSoap),
                FirstLineRecommendation = await query.CountAsync(x => x.RecommendationType == "FirstLine"),
                AlternativeRecommendation = await query.CountAsync(x => x.RecommendationType == "Alternative"),
                SymptomaticRecommendation = await query.CountAsync(x => x.RecommendationType == "Symptomatic"),
                SupportiveRecommendation = await query.CountAsync(x => x.RecommendationType == "Supportive"),
                ConditionalRecommendation = await query.CountAsync(x => x.RecommendationType == "Conditional")
            };
            return Ok(ApiResponse<DiagnosisDrugRecommendationSummaryResponse>.Ok(result, "Ringkasan rekomendasi obat diagnosis berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDiagnosisDrugRecommendationPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Diagnosis Drug Recommendation", Description = "Melihat data rekomendasi obat diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisDrugRecommendation", "Read")]
        public async Task<IActionResult> GetRecommendations(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] Guid? diagnosisId, [FromQuery] Guid? drugId, [FromQuery] string? recommendationType, [FromQuery] string? reviewStatus, [FromQuery] string? sourceType, [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "diagnosisCode",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);
            if (!dateRange.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, dateRange.ErrorMessage ?? "Filter tanggal tidak valid."));

            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(query, search, diagnosisId, drugId, recommendationType, reviewStatus, sourceType, isActive);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(BuildActorIds(entities));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<ResponseDiagnosisDrugRecommendationPagedResult>.Ok(new ResponseDiagnosisDrugRecommendationPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data rekomendasi obat diagnosis berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<DiagnosisDrugRecommendationOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Drug Recommendation", Description = "Melihat pilihan rekomendasi obat diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisDrugRecommendation", "Read")]
        public async Task<IActionResult> GetRecommendationOptions([FromQuery] Guid? diagnosisId, [FromQuery] Guid? drugId, [FromQuery] string? recommendationType, [FromQuery] string? reviewStatus, [FromQuery] bool onlyActive = true, [FromQuery] string? search = null, [FromQuery] int take = 100)
        {
            if (take <= 0) take = 100;
            if (take > 200) take = 200;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(query, search, diagnosisId, drugId, recommendationType, reviewStatus, null, onlyActive ? true : null);

            var optionEntities = await ApplySorting(query, "diagnosisCode", "asc")
                .Take(take)
                .ToListAsync();

            var data = optionEntities.Select(x => new DiagnosisDrugRecommendationOptionResponse
            {
                Id = x.Id,
                DiagnosisId = x.DiagnosisId,
                DiagnosisCode = x.Diagnosis?.DiagnosisCode ?? string.Empty,
                DiagnosisName = x.Diagnosis?.DiagnosisName ?? string.Empty,

                DrugId = x.DrugId,
                RecommendationType = x.RecommendationType,
                RecommendationTypeName = BuildLabel(x.RecommendationType),
                IndicationText = x.IndicationText,
                DoseText = x.DoseText,
                Route = x.Route,
                Frequency = x.Frequency,
                DurationText = x.DurationText,
                CautionNote = x.CautionNote,
                ReviewStatus = x.ReviewStatus,
                ReviewStatusName = BuildReviewStatusLabel(x.ReviewStatus),
                IsActive = x.IsActive
            }).ToList();

            return Ok(ApiResponse<List<DiagnosisDrugRecommendationOptionResponse>>.Ok(data, "Data pilihan rekomendasi obat diagnosis berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDrugRecommendationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Diagnosis Drug Recommendation", Description = "Melihat detail rekomendasi obat diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisDrugRecommendation", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi obat diagnosis tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(BuildActorIds(new[] { entity }));
            return Ok(ApiResponse<DiagnosisDrugRecommendationDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail rekomendasi obat diagnosis berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDrugRecommendationCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Diagnosis Drug Recommendation", Description = "Membuat rekomendasi obat diagnosis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DiagnosisDrugRecommendation", "Create")]
        public async Task<IActionResult> CreateRecommendation([FromBody] CreateDiagnosisDrugRecommendationRequest request)
        {
            var normalized = NormalizeRequest(request);
            var validation = await ValidateRequestAsync(null, normalized);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data rekomendasi obat diagnosis tidak valid."));

            var actorUserId = GetCurrentUserId();
            var entity = new MstDiagnosisDrugRecommendation
            {
                Id = Guid.NewGuid(),

                DiagnosisId = normalized.DiagnosisId,
                DrugId = normalized.DrugId,
                RecommendationType = normalized.RecommendationType,
                IndicationText = normalized.IndicationText,
                DoseText = normalized.DoseText,
                Route = normalized.Route,
                Frequency = normalized.Frequency,
                DurationText = normalized.DurationText,
                CautionNote = normalized.CautionNote,
                ReviewStatus = ReviewStatusDraft,
                SourceType = normalized.SourceType,
                SourceTitle = normalized.SourceTitle,
                SourceYear = normalized.SourceYear,
                SourceUrl = normalized.SourceUrl,
                SourceNote = normalized.SourceNote,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstDiagnosisDrugRecommendation>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var loaded = await BuildBaseQuery().FirstAsync(x => x.Id == entity.Id);
            var actorNames = await GetActorNameMapAsync(BuildActorIds(new[] { loaded }));
            var result = ToCreateResponse(loaded, actorNames);
            await _loggerService.InfoAsync(LogCategory, "DiagnosisDrugRecommendation.CreateRecommendation", "Membuat rekomendasi obat diagnosis.", result);

            return Ok(ApiResponse<DiagnosisDrugRecommendationCreateResponse>.Ok(result, "Rekomendasi obat diagnosis berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDrugRecommendationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Drug Recommendation", Description = "Mengubah rekomendasi obat diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisDrugRecommendation", "Update")]
        public async Task<IActionResult> UpdateRecommendation(Guid id, [FromBody] UpdateDiagnosisDrugRecommendationRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosisDrugRecommendation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi obat diagnosis tidak ditemukan."));

            var normalized = NormalizeRequest(request);
            var validation = await ValidateRequestAsync(id, normalized);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data rekomendasi obat diagnosis tidak valid."));

            var actorUserId = GetCurrentUserId();

            entity.DiagnosisId = normalized.DiagnosisId;
            entity.DrugId = normalized.DrugId;
            entity.RecommendationType = normalized.RecommendationType;
            entity.IndicationText = normalized.IndicationText;
            entity.DoseText = normalized.DoseText;
            entity.Route = normalized.Route;
            entity.Frequency = normalized.Frequency;
            entity.DurationText = normalized.DurationText;
            entity.CautionNote = normalized.CautionNote;
            entity.SourceType = normalized.SourceType;
            entity.SourceTitle = normalized.SourceTitle;
            entity.SourceYear = normalized.SourceYear;
            entity.SourceUrl = normalized.SourceUrl;
            entity.SourceNote = normalized.SourceNote;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var loaded = await BuildBaseQuery().FirstAsync(x => x.Id == entity.Id);
            var actorNames = await GetActorNameMapAsync(BuildActorIds(new[] { loaded }));
            var result = ToUpdateResponse(loaded, actorNames);
            await _loggerService.InfoAsync(LogCategory, "DiagnosisDrugRecommendation.UpdateRecommendation", "Mengubah rekomendasi obat diagnosis.", result);

            return Ok(ApiResponse<DiagnosisDrugRecommendationUpdateResponse>.Ok(result, "Rekomendasi obat diagnosis berhasil diubah."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDrugRecommendationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Drug Recommendation", Description = "Mengubah status aktif rekomendasi obat diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisDrugRecommendation", "Update")]
        public async Task<IActionResult> UpdateRecommendationStatus(Guid id, [FromBody] UpdateDiagnosisDrugRecommendationStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosisDrugRecommendation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi obat diagnosis tidak ditemukan."));

            var actorUserId = GetCurrentUserId();
            entity.IsActive = request.IsActive;
            if (!request.IsActive && entity.ReviewStatus == ReviewStatusActiveForSoap)
            {
                entity.ReviewStatus = ReviewStatusInactive;
            }
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            var loaded = await BuildBaseQuery().FirstAsync(x => x.Id == entity.Id);
            var actorNames = await GetActorNameMapAsync(BuildActorIds(new[] { loaded }));
            return Ok(ApiResponse<DiagnosisDrugRecommendationUpdateResponse>.Ok(ToUpdateResponse(loaded, actorNames), "Status aktif rekomendasi obat diagnosis berhasil diubah."));
        }

        [HttpPatch("{id:guid}/review-status")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDrugRecommendationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Drug Recommendation", Description = "Mengubah status review rekomendasi obat diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisDrugRecommendation", "Update")]
        public async Task<IActionResult> UpdateRecommendationReviewStatus(Guid id, [FromBody] UpdateDiagnosisDrugRecommendationReviewStatusRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data status review tidak valid."));

            var normalizedStatus = NormalizeReviewStatus(request.ReviewStatus);
            if (!ReviewStatusOptions.Contains(normalizedStatus, StringComparer.OrdinalIgnoreCase))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Status review tidak valid."));

            var entity = await _dbContext.Set<MstDiagnosisDrugRecommendation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi obat diagnosis tidak ditemukan."));

            if (normalizedStatus == ReviewStatusActiveForSoap && entity.ReviewStatus != ReviewStatusDoctorReviewed && entity.ReviewStatus != ReviewStatusActiveForSoap)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Rekomendasi harus berstatus DoctorReviewed sebelum diaktifkan untuk SOAP."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var note = NormalizeNullableText(request.Note);

            entity.ReviewStatus = normalizedStatus;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (normalizedStatus == ReviewStatusDraft)
            {
                entity.IsActive = true;
            }
            else if (normalizedStatus == ReviewStatusDoctorReviewed)
            {
                entity.IsActive = true;
                entity.ReviewedAt = now;
                entity.ReviewedByUserId = actorUserId;
                entity.ReviewNote = note;
            }
            else if (normalizedStatus == ReviewStatusActiveForSoap)
            {
                entity.IsActive = true;
                entity.ActivatedAt = now;
                entity.ActivatedByUserId = actorUserId;
                entity.ActivationNote = note;
            }
            else if (normalizedStatus == ReviewStatusInactive)
            {
                entity.IsActive = false;
            }

            await _dbContext.SaveChangesAsync();

            var loaded = await BuildBaseQuery().FirstAsync(x => x.Id == entity.Id);
            var actorNames = await GetActorNameMapAsync(BuildActorIds(new[] { loaded }));
            var result = ToUpdateResponse(loaded, actorNames);
            await _loggerService.InfoAsync(LogCategory, "DiagnosisDrugRecommendation.UpdateRecommendationReviewStatus", "Mengubah status review rekomendasi obat diagnosis.", result);

            return Ok(ApiResponse<DiagnosisDrugRecommendationUpdateResponse>.Ok(result, "Status review rekomendasi obat diagnosis berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDrugRecommendationDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Diagnosis Drug Recommendation", Description = "Menghapus rekomendasi obat diagnosis", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DiagnosisDrugRecommendation", "Delete")]
        public async Task<IActionResult> DeleteRecommendation(Guid id, [FromBody] DeleteDiagnosisDrugRecommendationRequest? request = null)
        {
            var entity = await _dbContext.Set<MstDiagnosisDrugRecommendation>().Include(x => x.Diagnosis).FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi obat diagnosis tidak ditemukan."));

            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.ReviewStatus = ReviewStatusInactive;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = entity.DeleteDateTime;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new DiagnosisDrugRecommendationDeleteResponse
            {
                Id = entity.Id,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.Diagnosis?.DiagnosisCode ?? string.Empty,
                DiagnosisName = entity.Diagnosis?.DiagnosisName ?? string.Empty,
                DrugId = entity.DrugId,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(LogCategory, "DiagnosisDrugRecommendation.DeleteRecommendation", "Menghapus rekomendasi obat diagnosis.", new { result, request?.DeleteReason });
            return Ok(ApiResponse<DiagnosisDrugRecommendationDeleteResponse>.Ok(result, "Rekomendasi obat diagnosis berhasil dihapus."));
        }

        private IQueryable<MstDiagnosisDrugRecommendation> BuildBaseQuery()
        {
            return _dbContext.Set<MstDiagnosisDrugRecommendation>()
                .AsNoTracking()
                .Include(x => x.Diagnosis)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDiagnosisDrugRecommendation> ApplyDateFilter(IQueryable<MstDiagnosisDrugRecommendation> query, DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstDiagnosisDrugRecommendation> ApplyStandardFilter(IQueryable<MstDiagnosisDrugRecommendation> query, string? search, Guid? diagnosisId, Guid? drugId, string? recommendationType, string? reviewStatus, string? sourceType, bool? isActive)
        {
            var normalizedDiagnosisId = NormalizeNullableGuid(diagnosisId);
            if (normalizedDiagnosisId.HasValue) query = query.Where(x => x.DiagnosisId == normalizedDiagnosisId.Value);


            var normalizedDrugId = NormalizeNullableGuid(drugId);
            if (normalizedDrugId.HasValue) query = query.Where(x => x.DrugId == normalizedDrugId.Value);

            if (!string.IsNullOrWhiteSpace(recommendationType)) query = query.Where(x => x.RecommendationType == NormalizeRecommendationType(recommendationType));

            if (!string.IsNullOrWhiteSpace(reviewStatus)) query = query.Where(x => x.ReviewStatus == NormalizeReviewStatus(reviewStatus));
            if (!string.IsNullOrWhiteSpace(sourceType)) query = query.Where(x => x.SourceType != null && x.SourceType == NormalizeSourceType(sourceType));
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>

                    x.RecommendationType.ToLower().Contains(keyword) ||
                    (x.IndicationText != null && x.IndicationText.ToLower().Contains(keyword)) ||
                    (x.DoseText != null && x.DoseText.ToLower().Contains(keyword)) ||
                    (x.Route != null && x.Route.ToLower().Contains(keyword)) ||
                    (x.Frequency != null && x.Frequency.ToLower().Contains(keyword)) ||
                    (x.DurationText != null && x.DurationText.ToLower().Contains(keyword)) ||
                    (x.CautionNote != null && x.CautionNote.ToLower().Contains(keyword)) ||
                    x.ReviewStatus.ToLower().Contains(keyword) ||
                    (x.SourceType != null && x.SourceType.ToLower().Contains(keyword)) ||
                    (x.SourceTitle != null && x.SourceTitle.ToLower().Contains(keyword)) ||
                    (x.SourceYear != null && x.SourceYear.ToLower().Contains(keyword)) ||
                    (x.SourceNote != null && x.SourceNote.ToLower().Contains(keyword)) ||
                    (x.Diagnosis != null && x.Diagnosis.DiagnosisCode.ToLower().Contains(keyword)) ||
                    (x.Diagnosis != null && x.Diagnosis.DiagnosisName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstDiagnosisDrugRecommendation> ApplySorting(IQueryable<MstDiagnosisDrugRecommendation> query, string? sortBy, string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "diagnosisCode").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "diagnosiscode" => isDesc ? query.OrderByDescending(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty) : query.OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty),
                "diagnosisname" => isDesc ? query.OrderByDescending(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisName : string.Empty) : query.OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisName : string.Empty),

                "recommendationtype" => isDesc ? query.OrderByDescending(x => x.RecommendationType) : query.OrderBy(x => x.RecommendationType),
                "reviewstatus" => isDesc ? query.OrderByDescending(x => x.ReviewStatus) : query.OrderBy(x => x.ReviewStatus),
                "sourcetype" => isDesc ? query.OrderByDescending(x => x.SourceType) : query.OrderBy(x => x.SourceType),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc ? query.OrderByDescending(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty) : query.OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty)
            };
        }

        private static DiagnosisDrugRecommendationResponse MapResponse(MstDiagnosisDrugRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisDrugRecommendationResponse
            {
                Id = entity.Id,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.Diagnosis?.DiagnosisCode ?? string.Empty,
                DiagnosisName = entity.Diagnosis?.DiagnosisName ?? string.Empty,

                DrugId = entity.DrugId,
                RecommendationType = entity.RecommendationType,
                RecommendationTypeName = BuildLabel(entity.RecommendationType),
                IndicationText = entity.IndicationText,
                DoseText = entity.DoseText,
                Route = entity.Route,
                Frequency = entity.Frequency,
                DurationText = entity.DurationText,
                CautionNote = entity.CautionNote,
                ReviewStatus = entity.ReviewStatus,
                ReviewStatusName = BuildReviewStatusLabel(entity.ReviewStatus),
                SourceType = entity.SourceType,
                SourceTypeName = entity.SourceType == null ? null : BuildSourceTypeLabel(entity.SourceType),
                SourceTitle = entity.SourceTitle,
                SourceYear = entity.SourceYear,
                SourceUrl = entity.SourceUrl,
                SourceNote = entity.SourceNote,
                ReviewedByUserId = entity.ReviewedByUserId,
                ReviewedByUserName = GetActorName(actorNames, entity.ReviewedByUserId),
                ReviewedAt = entity.ReviewedAt,
                ReviewNote = entity.ReviewNote,
                ActivatedByUserId = entity.ActivatedByUserId,
                ActivatedByUserName = GetActorName(actorNames, entity.ActivatedByUserId),
                ActivatedAt = entity.ActivatedAt,
                ActivationNote = entity.ActivationNote,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DiagnosisDrugRecommendationDetailResponse MapDetailResponse(MstDiagnosisDrugRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, actorNames);
            return new DiagnosisDrugRecommendationDetailResponse
            {
                Id = response.Id,
                DiagnosisId = response.DiagnosisId,
                DiagnosisCode = response.DiagnosisCode,
                DiagnosisName = response.DiagnosisName,

                DrugId = response.DrugId,
                RecommendationType = response.RecommendationType,
                RecommendationTypeName = response.RecommendationTypeName,
                IndicationText = response.IndicationText,
                DoseText = response.DoseText,
                Route = response.Route,
                Frequency = response.Frequency,
                DurationText = response.DurationText,
                CautionNote = response.CautionNote,
                ReviewStatus = response.ReviewStatus,
                ReviewStatusName = response.ReviewStatusName,
                SourceType = response.SourceType,
                SourceTypeName = response.SourceTypeName,
                SourceTitle = response.SourceTitle,
                SourceYear = response.SourceYear,
                SourceUrl = response.SourceUrl,
                SourceNote = response.SourceNote,
                ReviewedByUserId = response.ReviewedByUserId,
                ReviewedByUserName = response.ReviewedByUserName,
                ReviewedAt = response.ReviewedAt,
                ReviewNote = response.ReviewNote,
                ActivatedByUserId = response.ActivatedByUserId,
                ActivatedByUserName = response.ActivatedByUserName,
                ActivatedAt = response.ActivatedAt,
                ActivationNote = response.ActivationNote,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static DiagnosisDrugRecommendationCreateResponse ToCreateResponse(MstDiagnosisDrugRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, actorNames);
            return new DiagnosisDrugRecommendationCreateResponse
            {
                Id = response.Id,
                DiagnosisId = response.DiagnosisId,
                DiagnosisCode = response.DiagnosisCode,
                DiagnosisName = response.DiagnosisName,

                DrugId = response.DrugId,
                RecommendationType = response.RecommendationType,
                RecommendationTypeName = response.RecommendationTypeName,
                IndicationText = response.IndicationText,
                DoseText = response.DoseText,
                Route = response.Route,
                Frequency = response.Frequency,
                DurationText = response.DurationText,
                CautionNote = response.CautionNote,
                ReviewStatus = response.ReviewStatus,
                ReviewStatusName = response.ReviewStatusName,
                SourceType = response.SourceType,
                SourceTypeName = response.SourceTypeName,
                SourceTitle = response.SourceTitle,
                SourceYear = response.SourceYear,
                SourceUrl = response.SourceUrl,
                SourceNote = response.SourceNote,
                ReviewedByUserId = response.ReviewedByUserId,
                ReviewedByUserName = response.ReviewedByUserName,
                ReviewedAt = response.ReviewedAt,
                ReviewNote = response.ReviewNote,
                ActivatedByUserId = response.ActivatedByUserId,
                ActivatedByUserName = response.ActivatedByUserName,
                ActivatedAt = response.ActivatedAt,
                ActivationNote = response.ActivationNote,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName
            };
        }

        private static DiagnosisDrugRecommendationUpdateResponse ToUpdateResponse(MstDiagnosisDrugRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapDetailResponse(entity, actorNames);
            return new DiagnosisDrugRecommendationUpdateResponse
            {
                Id = response.Id,
                DiagnosisId = response.DiagnosisId,
                DiagnosisCode = response.DiagnosisCode,
                DiagnosisName = response.DiagnosisName,

                DrugId = response.DrugId,
                RecommendationType = response.RecommendationType,
                RecommendationTypeName = response.RecommendationTypeName,
                IndicationText = response.IndicationText,
                DoseText = response.DoseText,
                Route = response.Route,
                Frequency = response.Frequency,
                DurationText = response.DurationText,
                CautionNote = response.CautionNote,
                ReviewStatus = response.ReviewStatus,
                ReviewStatusName = response.ReviewStatusName,
                SourceType = response.SourceType,
                SourceTypeName = response.SourceTypeName,
                SourceTitle = response.SourceTitle,
                SourceYear = response.SourceYear,
                SourceUrl = response.SourceUrl,
                SourceNote = response.SourceNote,
                ReviewedByUserId = response.ReviewedByUserId,
                ReviewedByUserName = response.ReviewedByUserName,
                ReviewedAt = response.ReviewedAt,
                ReviewNote = response.ReviewNote,
                ActivatedByUserId = response.ActivatedByUserId,
                ActivatedByUserName = response.ActivatedByUserName,
                ActivatedAt = response.ActivatedAt,
                ActivationNote = response.ActivationNote,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = response.UpdateDateTime,
                UpdateBy = response.UpdateBy,
                UpdateByName = response.UpdateByName
            };
        }

        private static CreateDiagnosisDrugRecommendationRequest NormalizeRequest(CreateDiagnosisDrugRecommendationRequest request)
        {
            return new CreateDiagnosisDrugRecommendationRequest
            {

                DiagnosisId = request.DiagnosisId,
                DrugId = request.DrugId,
                RecommendationType = NormalizeRecommendationType(request.RecommendationType),
                IndicationText = NormalizeNullableText(request.IndicationText),
                DoseText = NormalizeNullableText(request.DoseText),
                Route = NormalizeNullableText(request.Route),
                Frequency = NormalizeNullableText(request.Frequency),
                DurationText = NormalizeNullableText(request.DurationText),
                CautionNote = NormalizeNullableText(request.CautionNote),
                SourceType = NormalizeSourceType(request.SourceType),
                SourceTitle = NormalizeNullableText(request.SourceTitle),
                SourceYear = NormalizeNullableText(request.SourceYear),
                SourceUrl = NormalizeNullableText(request.SourceUrl),
                SourceNote = NormalizeNullableText(request.SourceNote)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? currentId, CreateDiagnosisDrugRecommendationRequest request)
        {
            if (request.DiagnosisId == Guid.Empty) return (false, "Diagnosis wajib diisi.");

            var diagnosis = await _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.Id == request.DiagnosisId);

            if (diagnosis == null) return (false, "Diagnosis tidak valid atau tidak aktif.");
            if (diagnosis.IcdVersion != "ICD-10" || diagnosis.DiagnosisType != "ICD10")
                return (false, "Rekomendasi hanya boleh dibuat untuk diagnosis klinis ICD-10.");


            if (request.DrugId == Guid.Empty) return (false, "Obat wajib diisi.");
            if (!RecommendationTypeOptionsValues.Contains(request.RecommendationType, StringComparer.OrdinalIgnoreCase)) return (false, "Tipe rekomendasi tidak valid.");

            var drugExists = await _dbContext.Set<MstDrug>().AnyAsync(x => !x.IsDelete && x.Id == request.DrugId);
            if (!drugExists) return (false, "Obat tidak valid.");

            var duplicate = await _dbContext.Set<MstDiagnosisDrugRecommendation>()
                .AnyAsync(x => !x.IsDelete && x.DiagnosisId == request.DiagnosisId && x.DrugId == request.DrugId && x.RecommendationType == request.RecommendationType && (!currentId.HasValue || x.Id != currentId.Value));
            if (duplicate) return (false, "Rekomendasi obat dengan diagnosis, obat, dan tipe yang sama sudah ada.");

            if (!string.IsNullOrWhiteSpace(request.SourceType) && !SourceTypeOptions.Contains(request.SourceType, StringComparer.OrdinalIgnoreCase))
                return (false, "Tipe sumber literatur tidak valid.");

            return (true, null);
        }


        private static string NormalizeRecommendationType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Supportive";
            var normalized = value.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty).ToUpperInvariant();
            return normalized switch
            {
                "FIRSTLINE" => "FirstLine",
                "ALTERNATIVE" => "Alternative",
                "SYMPTOMATIC" => "Symptomatic",
                "SUPPORTIVE" => "Supportive",
                "CONDITIONAL" => "Conditional",
                _ => value.Trim()
            };
        }


        private static string NormalizeReviewStatus(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return ReviewStatusDraft;
            var normalized = value.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty).ToUpperInvariant();
            return normalized switch
            {
                "DRAFT" => ReviewStatusDraft,
                "DRAFTFROMLITERATURE" => ReviewStatusDraft,
                "DOCTORREVIEWED" => ReviewStatusDoctorReviewed,
                "REVIEWED" => ReviewStatusDoctorReviewed,
                "ACTIVE" => ReviewStatusActiveForSoap,
                "ACTIVEFORSOAP" => ReviewStatusActiveForSoap,
                "INACTIVE" => ReviewStatusInactive,
                _ => value.Trim()
            };
        }

        private static string? NormalizeSourceType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var normalized = value.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty).ToUpperInvariant();
            return normalized switch
            {
                "PNPK" => "PNPK",
                "FORNAS" => "Fornas",
                "PPKRS" => "PPK_RS",
                "MANUALDOCTORINPUT" => "ManualDoctorInput",
                "OTHER" => "Other",
                _ => value.Trim()
            };
        }

        private static string BuildLabel(string value)
        {
            return value switch
            {
                "FirstLine" => "First Line",
                "Alternative" => "Alternative",
                "Symptomatic" => "Symptomatic",
                "Supportive" => "Supportive",
                "Conditional" => "Conditional",
                "Procedure" => "Procedure",
                "Lab" => "Laboratorium",
                "Radiology" => "Radiologi",
                "Monitoring" => "Monitoring",
                "Referral" => "Rujukan",
                "FollowUp" => "Follow Up",
                "General" => "Umum",
                "Diet" => "Diet",
                "Activity" => "Aktivitas",
                "MedicationUse" => "Penggunaan Obat",
                "WarningSign" => "Tanda Bahaya",
                "Prevention" => "Pencegahan",
                _ => value
            };
        }

        private static string BuildReviewStatusLabel(string value)
        {
            return value switch
            {
                ReviewStatusDraft => "Draft dari Literatur",
                ReviewStatusDoctorReviewed => "Direview Dokter",
                ReviewStatusActiveForSoap => "Aktif untuk SOAP",
                ReviewStatusInactive => "Nonaktif",
                _ => value
            };
        }

        private static string BuildSourceTypeLabel(string value)
        {
            return value switch
            {
                "PNPK" => "PNPK Kemenkes",
                "Fornas" => "Formularium Nasional",
                "PPK_RS" => "PPK Internal RS",
                "ManualDoctorInput" => "Input Manual Dokter",
                "Other" => "Lainnya",
                _ => value
            };
        }

        private static Guid? NormalizeNullableGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string? NormalizeNullableText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string NormalizeRequiredText(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }

        private static IEnumerable<Guid> BuildActorIds(IEnumerable<MstDiagnosisDrugRecommendation> entities)
        {
            return entities.SelectMany(x => new[]
            {
                x.CreateBy,
                x.UpdateBy,
                x.ReviewedByUserId ?? Guid.Empty,
                x.ActivatedByUserId ?? Guid.Empty
            }).Where(x => x != Guid.Empty).Distinct();
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<Guid, string?>();
            return await _dbContext.Users.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId)
            => actorId == Guid.Empty ? null : actorNames.TryGetValue(actorId, out var actorName) ? actorName : null;

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid? actorId)
            => !actorId.HasValue || actorId.Value == Guid.Empty ? null : actorNames.TryGetValue(actorId.Value, out var actorName) ? actorName : null;

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

        private static List<DiagnosisDrugRecommendationCustomPeriodOptionResponse> BuildCustomPeriodOptions() => new()
        {
            new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
            new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
        };

        private static List<DiagnosisDrugRecommendationSortOptionResponse> BuildSortOptions() => new()
        {
            new() { Value = "createDateTime", Label = "Tanggal dibuat" },
            new() { Value = "diagnosisCode", Label = "Kode diagnosis" },
            new() { Value = "diagnosisName", Label = "Nama diagnosis" },
            new() { Value = "reviewStatus", Label = "Status review" },
            new() { Value = "sourceType", Label = "Sumber" },
            new() { Value = "isActive", Label = "Status aktif" }
        };

        private static List<DiagnosisDrugRecommendationQueryParameterInfoResponse> BuildQueryParameterInfo() => new()
        {
            new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
            new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
            new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
            new() { Name = "search", Type = "string", Description = "Cari diagnosis, rekomendasi, status review, atau sumber literatur." },
            new() { Name = "diagnosisId", Type = "Guid?", Description = "Filter berdasarkan diagnosis." },
            new() { Name = "reviewStatus", Type = "string", Description = "Filter status review.", Example = "ActiveForSoap" },
            new() { Name = "sourceType", Type = "string", Description = "Filter tipe sumber literatur.", Example = "PNPK" },
            new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif.", Example = "true" }
        };

        private static List<DiagnosisDrugRecommendationFormFieldMetadataResponse> BuildCreateFieldMetadata() => BuildFieldMetadata(false);
        private static List<DiagnosisDrugRecommendationFormFieldMetadataResponse> BuildUpdateFieldMetadata() => BuildFieldMetadata(true);
        private static List<DiagnosisDrugRecommendationFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<DiagnosisDrugRecommendationFormFieldMetadataResponse>
            {
                new() { Name = "diagnosisId", Label = "Diagnosis", Section = "Relation", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", SortOrder = 1 },
                new() { Name = "drugId", Label = "Obat", Section = "Relation", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", SortOrder = 2 },
                new() { Name = "recommendationType", Label = "Tipe Rekomendasi", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "recommendationTypeOptions", Example = "FirstLine", SortOrder = 3 },
                new() { Name = "indicationText", Label = "Indikasi", Section = "Instruction", InputType = "textarea", MaxLength = 500, SortOrder = 4 },
                new() { Name = "doseText", Label = "Dosis", Section = "Dose", InputType = "text", MaxLength = 250, Example = "500 mg", SortOrder = 5 },
                new() { Name = "route", Label = "Rute", Section = "Dose", InputType = "text", MaxLength = 100, Example = "Oral", SortOrder = 6 },
                new() { Name = "frequency", Label = "Frekuensi", Section = "Dose", InputType = "text", MaxLength = 100, Example = "3x sehari", SortOrder = 7 },
                new() { Name = "durationText", Label = "Durasi", Section = "Dose", InputType = "text", MaxLength = 100, Example = "3 hari", SortOrder = 8 },
                new() { Name = "cautionNote", Label = "Catatan Kehati-hatian", Section = "Safety", InputType = "textarea", MaxLength = 500, SortOrder = 9 },
                new() { Name = "sourceType", Label = "Tipe Sumber", Section = "Source", InputType = "select", OptionsSource = "sourceTypeOptions", MaxLength = 50, SortOrder = 80 },
                new() { Name = "sourceTitle", Label = "Judul Sumber", Section = "Source", InputType = "text", MaxLength = 300, SortOrder = 81 },
                new() { Name = "sourceYear", Label = "Tahun Sumber", Section = "Source", InputType = "text", MaxLength = 20, SortOrder = 82 },
                new() { Name = "sourceUrl", Label = "URL Sumber", Section = "Source", InputType = "text", MaxLength = 1000, SortOrder = 83 },
                new() { Name = "sourceNote", Label = "Catatan Sumber", Section = "Source", InputType = "textarea", MaxLength = 1000, SortOrder = 84 }
            };
            if (isUpdate) fields.Add(new DiagnosisDrugRecommendationFormFieldMetadataResponse { Name = "isActive", Label = "Status Aktif", Section = "Status", InputType = "switch", SortOrder = 99 });
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
