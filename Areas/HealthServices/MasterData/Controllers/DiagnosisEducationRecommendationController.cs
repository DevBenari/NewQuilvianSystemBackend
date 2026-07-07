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

using ResponseDiagnosisEducationRecommendationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DiagnosisEducationRecommendationResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/diagnosis-education-recommendations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Diagnosis Education Recommendation",
        AreaName = "HealthServices",
        ControllerName = "DiagnosisEducationRecommendation",
        Description = "Rekomendasi edukasi pasien berdasarkan diagnosis ICD",
        SortOrder = 14
    )]
    [Tags("Health Services / Master Data / Diagnosis Education Recommendation")]
    public class DiagnosisEducationRecommendationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string ReviewStatusDraft = "DraftFromLiterature";
        private const string ReviewStatusDoctorReviewed = "DoctorReviewed";
        private const string ReviewStatusActiveForSoap = "ActiveForSoap";
        private const string ReviewStatusInactive = "Inactive";

        private static readonly string[] EducationTypeOptionsValues = { "General", "Diet", "Activity", "MedicationUse", "WarningSign", "FollowUp", "Prevention" };
        private static readonly string[] ReviewStatusOptions = { ReviewStatusDraft, ReviewStatusDoctorReviewed, ReviewStatusActiveForSoap, ReviewStatusInactive };
        private static readonly string[] SourceTypeOptions = { "PNPK", "Fornas", "PPK_RS", "ManualDoctorInput", "Other" };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DiagnosisEducationRecommendationController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisEducationRecommendationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Education Recommendation", Description = "Melihat metadata filter rekomendasi edukasi diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisEducationRecommendation", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DiagnosisEducationRecommendationFilterMetadataResponse
            {
                DefaultFilter = new DiagnosisEducationRecommendationDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = BuildSortOptions(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EducationTypeOptions = EducationTypeOptionsValues.Select(x => new DiagnosisEducationRecommendationStringOptionResponse { Value = x, Label = BuildLabel(x) }).ToList(),
                ReviewStatusOptions = ReviewStatusOptions.Select(x => new DiagnosisEducationRecommendationStringOptionResponse { Value = x, Label = BuildReviewStatusLabel(x) }).ToList(),
                SourceTypeOptions = SourceTypeOptions.Select(x => new DiagnosisEducationRecommendationStringOptionResponse { Value = x, Label = BuildSourceTypeLabel(x) }).ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(LogCategory, "DiagnosisEducationRecommendation.GetFilterMetadata", "Mengambil metadata filter rekomendasi edukasi diagnosis.", result);
            return Ok(ApiResponse<DiagnosisEducationRecommendationFilterMetadataResponse>.Ok(result, "Metadata filter rekomendasi edukasi diagnosis berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisEducationRecommendationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Education Recommendation", Description = "Melihat ringkasan rekomendasi edukasi diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisEducationRecommendation", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDiagnosisEducationRecommendation>().AsNoTracking().Where(x => !x.IsDelete);
            var result = new DiagnosisEducationRecommendationSummaryResponse
            {
                TotalRecommendation = await query.CountAsync(),
                ActiveRecommendation = await query.CountAsync(x => x.IsActive),
                InactiveRecommendation = await query.CountAsync(x => !x.IsActive),
                DraftFromLiteratureRecommendation = await query.CountAsync(x => x.ReviewStatus == ReviewStatusDraft),
                DoctorReviewedRecommendation = await query.CountAsync(x => x.ReviewStatus == ReviewStatusDoctorReviewed),
                ActiveForSoapRecommendation = await query.CountAsync(x => x.ReviewStatus == ReviewStatusActiveForSoap),
                GeneralEducation = await query.CountAsync(x => x.EducationType == "General"),
                DietEducation = await query.CountAsync(x => x.EducationType == "Diet"),
                ActivityEducation = await query.CountAsync(x => x.EducationType == "Activity"),
                MedicationUseEducation = await query.CountAsync(x => x.EducationType == "MedicationUse"),
                WarningSignEducation = await query.CountAsync(x => x.EducationType == "WarningSign"),
                FollowUpEducation = await query.CountAsync(x => x.EducationType == "FollowUp"),
                PreventionEducation = await query.CountAsync(x => x.EducationType == "Prevention")
            };
            return Ok(ApiResponse<DiagnosisEducationRecommendationSummaryResponse>.Ok(result, "Ringkasan rekomendasi edukasi diagnosis berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDiagnosisEducationRecommendationPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Diagnosis Education Recommendation", Description = "Melihat data rekomendasi edukasi diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisEducationRecommendation", "Read")]
        public async Task<IActionResult> GetRecommendations(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] Guid? diagnosisId, [FromQuery] string? educationType, [FromQuery] string? reviewStatus, [FromQuery] string? sourceType, [FromQuery] bool? isActive,
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
            query = ApplyStandardFilter(query, search, diagnosisId, educationType, reviewStatus, sourceType, isActive);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(BuildActorIds(entities));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<ResponseDiagnosisEducationRecommendationPagedResult>.Ok(new ResponseDiagnosisEducationRecommendationPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data rekomendasi edukasi diagnosis berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<DiagnosisEducationRecommendationOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis Education Recommendation", Description = "Melihat pilihan rekomendasi edukasi diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisEducationRecommendation", "Read")]
        public async Task<IActionResult> GetRecommendationOptions([FromQuery] Guid? diagnosisId, [FromQuery] string? educationType, [FromQuery] string? reviewStatus, [FromQuery] bool onlyActive = true, [FromQuery] string? search = null, [FromQuery] int take = 100)
        {
            if (take <= 0) take = 100;
            if (take > 200) take = 200;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(query, search, diagnosisId, educationType, reviewStatus, null, onlyActive ? true : null);

            var optionEntities = await ApplySorting(query, "diagnosisCode", "asc")
                .Take(take)
                .ToListAsync();

            var data = optionEntities.Select(x => new DiagnosisEducationRecommendationOptionResponse
            {
                Id = x.Id,
                DiagnosisId = x.DiagnosisId,
                DiagnosisCode = x.Diagnosis?.DiagnosisCode ?? string.Empty,
                DiagnosisName = x.Diagnosis?.DiagnosisName ?? string.Empty,

                EducationType = x.EducationType,
                EducationTypeName = BuildLabel(x.EducationType),
                EducationTitle = x.EducationTitle,
                EducationText = x.EducationText,
                ReviewStatus = x.ReviewStatus,
                ReviewStatusName = BuildReviewStatusLabel(x.ReviewStatus),
                IsActive = x.IsActive
            }).ToList();

            return Ok(ApiResponse<List<DiagnosisEducationRecommendationOptionResponse>>.Ok(data, "Data pilihan rekomendasi edukasi diagnosis berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisEducationRecommendationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Diagnosis Education Recommendation", Description = "Melihat detail rekomendasi edukasi diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisEducationRecommendation", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi edukasi diagnosis tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(BuildActorIds(new[] { entity }));
            return Ok(ApiResponse<DiagnosisEducationRecommendationDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail rekomendasi edukasi diagnosis berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisEducationRecommendationCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Diagnosis Education Recommendation", Description = "Membuat rekomendasi edukasi diagnosis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DiagnosisEducationRecommendation", "Create")]
        public async Task<IActionResult> CreateRecommendation([FromBody] CreateDiagnosisEducationRecommendationRequest request)
        {
            var normalized = NormalizeRequest(request);
            var validation = await ValidateRequestAsync(null, normalized);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data rekomendasi edukasi diagnosis tidak valid."));

            var actorUserId = GetCurrentUserId();
            var entity = new MstDiagnosisEducationRecommendation
            {
                Id = Guid.NewGuid(),

                DiagnosisId = normalized.DiagnosisId,
                EducationType = normalized.EducationType,
                EducationTitle = normalized.EducationTitle,
                EducationText = normalized.EducationText,
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

            _dbContext.Set<MstDiagnosisEducationRecommendation>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var loaded = await BuildBaseQuery().FirstAsync(x => x.Id == entity.Id);
            var actorNames = await GetActorNameMapAsync(BuildActorIds(new[] { loaded }));
            var result = ToCreateResponse(loaded, actorNames);
            await _loggerService.InfoAsync(LogCategory, "DiagnosisEducationRecommendation.CreateRecommendation", "Membuat rekomendasi edukasi diagnosis.", result);

            return Ok(ApiResponse<DiagnosisEducationRecommendationCreateResponse>.Ok(result, "Rekomendasi edukasi diagnosis berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisEducationRecommendationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Education Recommendation", Description = "Mengubah rekomendasi edukasi diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisEducationRecommendation", "Update")]
        public async Task<IActionResult> UpdateRecommendation(Guid id, [FromBody] UpdateDiagnosisEducationRecommendationRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosisEducationRecommendation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi edukasi diagnosis tidak ditemukan."));

            var normalized = NormalizeRequest(request);
            var validation = await ValidateRequestAsync(id, normalized);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data rekomendasi edukasi diagnosis tidak valid."));

            var actorUserId = GetCurrentUserId();

            entity.DiagnosisId = normalized.DiagnosisId;
            entity.EducationType = normalized.EducationType;
            entity.EducationTitle = normalized.EducationTitle;
            entity.EducationText = normalized.EducationText;
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
            await _loggerService.InfoAsync(LogCategory, "DiagnosisEducationRecommendation.UpdateRecommendation", "Mengubah rekomendasi edukasi diagnosis.", result);

            return Ok(ApiResponse<DiagnosisEducationRecommendationUpdateResponse>.Ok(result, "Rekomendasi edukasi diagnosis berhasil diubah."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisEducationRecommendationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Education Recommendation", Description = "Mengubah status aktif rekomendasi edukasi diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisEducationRecommendation", "Update")]
        public async Task<IActionResult> UpdateRecommendationStatus(Guid id, [FromBody] UpdateDiagnosisEducationRecommendationStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosisEducationRecommendation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi edukasi diagnosis tidak ditemukan."));

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
            return Ok(ApiResponse<DiagnosisEducationRecommendationUpdateResponse>.Ok(ToUpdateResponse(loaded, actorNames), "Status aktif rekomendasi edukasi diagnosis berhasil diubah."));
        }

        [HttpPatch("{id:guid}/review-status")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisEducationRecommendationUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Education Recommendation", Description = "Mengubah status review rekomendasi edukasi diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DiagnosisEducationRecommendation", "Update")]
        public async Task<IActionResult> UpdateRecommendationReviewStatus(Guid id, [FromBody] UpdateDiagnosisEducationRecommendationReviewStatusRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data status review tidak valid."));

            var normalizedStatus = NormalizeReviewStatus(request.ReviewStatus);
            if (!ReviewStatusOptions.Contains(normalizedStatus, StringComparer.OrdinalIgnoreCase))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Status review tidak valid."));

            var entity = await _dbContext.Set<MstDiagnosisEducationRecommendation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi edukasi diagnosis tidak ditemukan."));

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
            await _loggerService.InfoAsync(LogCategory, "DiagnosisEducationRecommendation.UpdateRecommendationReviewStatus", "Mengubah status review rekomendasi edukasi diagnosis.", result);

            return Ok(ApiResponse<DiagnosisEducationRecommendationUpdateResponse>.Ok(result, "Status review rekomendasi edukasi diagnosis berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisEducationRecommendationDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Diagnosis Education Recommendation", Description = "Menghapus rekomendasi edukasi diagnosis", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DiagnosisEducationRecommendation", "Delete")]
        public async Task<IActionResult> DeleteRecommendation(Guid id, [FromBody] DeleteDiagnosisEducationRecommendationRequest? request = null)
        {
            var entity = await _dbContext.Set<MstDiagnosisEducationRecommendation>().Include(x => x.Diagnosis).FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rekomendasi edukasi diagnosis tidak ditemukan."));

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
            var result = new DiagnosisEducationRecommendationDeleteResponse
            {
                Id = entity.Id,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.Diagnosis?.DiagnosisCode ?? string.Empty,
                DiagnosisName = entity.Diagnosis?.DiagnosisName ?? string.Empty,
                EducationTitle = entity.EducationTitle,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(LogCategory, "DiagnosisEducationRecommendation.DeleteRecommendation", "Menghapus rekomendasi edukasi diagnosis.", new { result, request?.DeleteReason });
            return Ok(ApiResponse<DiagnosisEducationRecommendationDeleteResponse>.Ok(result, "Rekomendasi edukasi diagnosis berhasil dihapus."));
        }

        private IQueryable<MstDiagnosisEducationRecommendation> BuildBaseQuery()
        {
            return _dbContext.Set<MstDiagnosisEducationRecommendation>()
                .AsNoTracking()
                .Include(x => x.Diagnosis)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDiagnosisEducationRecommendation> ApplyDateFilter(IQueryable<MstDiagnosisEducationRecommendation> query, DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstDiagnosisEducationRecommendation> ApplyStandardFilter(IQueryable<MstDiagnosisEducationRecommendation> query, string? search, Guid? diagnosisId, string? educationType, string? reviewStatus, string? sourceType, bool? isActive)
        {
            var normalizedDiagnosisId = NormalizeNullableGuid(diagnosisId);
            if (normalizedDiagnosisId.HasValue) query = query.Where(x => x.DiagnosisId == normalizedDiagnosisId.Value);


            if (!string.IsNullOrWhiteSpace(educationType)) query = query.Where(x => x.EducationType == NormalizeEducationType(educationType));

            if (!string.IsNullOrWhiteSpace(reviewStatus)) query = query.Where(x => x.ReviewStatus == NormalizeReviewStatus(reviewStatus));
            if (!string.IsNullOrWhiteSpace(sourceType)) query = query.Where(x => x.SourceType != null && x.SourceType == NormalizeSourceType(sourceType));
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>

                    x.EducationType.ToLower().Contains(keyword) ||
                    x.EducationTitle.ToLower().Contains(keyword) ||
                    x.EducationText.ToLower().Contains(keyword) ||
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

        private static IOrderedQueryable<MstDiagnosisEducationRecommendation> ApplySorting(IQueryable<MstDiagnosisEducationRecommendation> query, string? sortBy, string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "diagnosisCode").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "diagnosiscode" => isDesc ? query.OrderByDescending(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty) : query.OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty),
                "diagnosisname" => isDesc ? query.OrderByDescending(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisName : string.Empty) : query.OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisName : string.Empty),

                "educationtype" => isDesc ? query.OrderByDescending(x => x.EducationType) : query.OrderBy(x => x.EducationType),
                "educationtitle" => isDesc ? query.OrderByDescending(x => x.EducationTitle) : query.OrderBy(x => x.EducationTitle),
                "reviewstatus" => isDesc ? query.OrderByDescending(x => x.ReviewStatus) : query.OrderBy(x => x.ReviewStatus),
                "sourcetype" => isDesc ? query.OrderByDescending(x => x.SourceType) : query.OrderBy(x => x.SourceType),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc ? query.OrderByDescending(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty) : query.OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty)
            };
        }

        private static DiagnosisEducationRecommendationResponse MapResponse(MstDiagnosisEducationRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisEducationRecommendationResponse
            {
                Id = entity.Id,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.Diagnosis?.DiagnosisCode ?? string.Empty,
                DiagnosisName = entity.Diagnosis?.DiagnosisName ?? string.Empty,

                EducationType = entity.EducationType,
                EducationTypeName = BuildLabel(entity.EducationType),
                EducationTitle = entity.EducationTitle,
                EducationText = entity.EducationText,
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

        private static DiagnosisEducationRecommendationDetailResponse MapDetailResponse(MstDiagnosisEducationRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, actorNames);
            return new DiagnosisEducationRecommendationDetailResponse
            {
                Id = response.Id,
                DiagnosisId = response.DiagnosisId,
                DiagnosisCode = response.DiagnosisCode,
                DiagnosisName = response.DiagnosisName,

                EducationType = response.EducationType,
                EducationTypeName = response.EducationTypeName,
                EducationTitle = response.EducationTitle,
                EducationText = response.EducationText,
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

        private static DiagnosisEducationRecommendationCreateResponse ToCreateResponse(MstDiagnosisEducationRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, actorNames);
            return new DiagnosisEducationRecommendationCreateResponse
            {
                Id = response.Id,
                DiagnosisId = response.DiagnosisId,
                DiagnosisCode = response.DiagnosisCode,
                DiagnosisName = response.DiagnosisName,

                EducationType = response.EducationType,
                EducationTypeName = response.EducationTypeName,
                EducationTitle = response.EducationTitle,
                EducationText = response.EducationText,
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

        private static DiagnosisEducationRecommendationUpdateResponse ToUpdateResponse(MstDiagnosisEducationRecommendation entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapDetailResponse(entity, actorNames);
            return new DiagnosisEducationRecommendationUpdateResponse
            {
                Id = response.Id,
                DiagnosisId = response.DiagnosisId,
                DiagnosisCode = response.DiagnosisCode,
                DiagnosisName = response.DiagnosisName,

                EducationType = response.EducationType,
                EducationTypeName = response.EducationTypeName,
                EducationTitle = response.EducationTitle,
                EducationText = response.EducationText,
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

        private static CreateDiagnosisEducationRecommendationRequest NormalizeRequest(CreateDiagnosisEducationRecommendationRequest request)
        {
            return new CreateDiagnosisEducationRecommendationRequest
            {

                DiagnosisId = request.DiagnosisId,
                EducationType = NormalizeEducationType(request.EducationType),
                EducationTitle = NormalizeRequiredText(request.EducationTitle),
                EducationText = NormalizeRequiredText(request.EducationText),
                SourceType = NormalizeSourceType(request.SourceType),
                SourceTitle = NormalizeNullableText(request.SourceTitle),
                SourceYear = NormalizeNullableText(request.SourceYear),
                SourceUrl = NormalizeNullableText(request.SourceUrl),
                SourceNote = NormalizeNullableText(request.SourceNote)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? currentId, CreateDiagnosisEducationRecommendationRequest request)
        {
            if (request.DiagnosisId == Guid.Empty) return (false, "Diagnosis wajib diisi.");

            var diagnosis = await _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.Id == request.DiagnosisId);

            if (diagnosis == null) return (false, "Diagnosis tidak valid atau tidak aktif.");
            if (diagnosis.IcdVersion != "ICD-10" || diagnosis.DiagnosisType != "ICD10")
                return (false, "Rekomendasi hanya boleh dibuat untuk diagnosis klinis ICD-10.");


            if (!EducationTypeOptionsValues.Contains(request.EducationType, StringComparer.OrdinalIgnoreCase)) return (false, "Tipe edukasi tidak valid.");
            if (string.IsNullOrWhiteSpace(request.EducationTitle)) return (false, "Judul edukasi wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.EducationText)) return (false, "Isi edukasi wajib diisi.");

            var duplicate = await _dbContext.Set<MstDiagnosisEducationRecommendation>()
                .AnyAsync(x => !x.IsDelete && x.DiagnosisId == request.DiagnosisId && x.EducationType == request.EducationType && x.EducationTitle.ToLower() == request.EducationTitle.ToLower() && (!currentId.HasValue || x.Id != currentId.Value));
            if (duplicate) return (false, "Rekomendasi edukasi dengan diagnosis, tipe, dan judul yang sama sudah ada.");

            if (!string.IsNullOrWhiteSpace(request.SourceType) && !SourceTypeOptions.Contains(request.SourceType, StringComparer.OrdinalIgnoreCase))
                return (false, "Tipe sumber literatur tidak valid.");

            return (true, null);
        }


        private static string NormalizeEducationType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "General";
            var normalized = value.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty).ToUpperInvariant();
            return normalized switch
            {
                "GENERAL" => "General",
                "DIET" => "Diet",
                "ACTIVITY" => "Activity",
                "MEDICATIONUSE" => "MedicationUse",
                "WARNINGSIGN" => "WarningSign",
                "FOLLOWUP" => "FollowUp",
                "PREVENTION" => "Prevention",
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

        private static IEnumerable<Guid> BuildActorIds(IEnumerable<MstDiagnosisEducationRecommendation> entities)
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

        private static List<DiagnosisEducationRecommendationCustomPeriodOptionResponse> BuildCustomPeriodOptions() => new()
        {
            new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
            new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
            new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
        };

        private static List<DiagnosisEducationRecommendationSortOptionResponse> BuildSortOptions() => new()
        {
            new() { Value = "createDateTime", Label = "Tanggal dibuat" },
            new() { Value = "diagnosisCode", Label = "Kode diagnosis" },
            new() { Value = "diagnosisName", Label = "Nama diagnosis" },
            new() { Value = "reviewStatus", Label = "Status review" },
            new() { Value = "sourceType", Label = "Sumber" },
            new() { Value = "isActive", Label = "Status aktif" }
        };

        private static List<DiagnosisEducationRecommendationQueryParameterInfoResponse> BuildQueryParameterInfo() => new()
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

        private static List<DiagnosisEducationRecommendationFormFieldMetadataResponse> BuildCreateFieldMetadata() => BuildFieldMetadata(false);
        private static List<DiagnosisEducationRecommendationFormFieldMetadataResponse> BuildUpdateFieldMetadata() => BuildFieldMetadata(true);
        private static List<DiagnosisEducationRecommendationFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<DiagnosisEducationRecommendationFormFieldMetadataResponse>
            {
                new() { Name = "diagnosisId", Label = "Diagnosis", Section = "Relation", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", SortOrder = 1 },
                new() { Name = "educationType", Label = "Tipe Edukasi", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "educationTypeOptions", Example = "Diet", SortOrder = 2 },
                new() { Name = "educationTitle", Label = "Judul Edukasi", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 250, SortOrder = 3 },
                new() { Name = "educationText", Label = "Isi Edukasi", Section = "Instruction", InputType = "textarea", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 2000, SortOrder = 4 },
                new() { Name = "sourceType", Label = "Tipe Sumber", Section = "Source", InputType = "select", OptionsSource = "sourceTypeOptions", MaxLength = 50, SortOrder = 80 },
                new() { Name = "sourceTitle", Label = "Judul Sumber", Section = "Source", InputType = "text", MaxLength = 300, SortOrder = 81 },
                new() { Name = "sourceYear", Label = "Tahun Sumber", Section = "Source", InputType = "text", MaxLength = 20, SortOrder = 82 },
                new() { Name = "sourceUrl", Label = "URL Sumber", Section = "Source", InputType = "text", MaxLength = 1000, SortOrder = 83 },
                new() { Name = "sourceNote", Label = "Catatan Sumber", Section = "Source", InputType = "textarea", MaxLength = 1000, SortOrder = 84 }
            };
            if (isUpdate) fields.Add(new DiagnosisEducationRecommendationFormFieldMetadataResponse { Name = "isActive", Label = "Status Aktif", Section = "Status", InputType = "switch", SortOrder = 99 });
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
