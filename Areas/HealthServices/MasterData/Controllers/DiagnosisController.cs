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

using ResponseDiagnosisPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DiagnosisResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/diagnoses")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Diagnosis",
        AreaName = "HealthServices",
        ControllerName = "Diagnosis",
        Description = "Health service master data diagnosis",
        SortOrder = 11
    )]
    [Tags("Health Services / Master Data / Diagnosis")]
    public class DiagnosisController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

        private static readonly string[] IcdVersionOptions = { "ICD-9", "ICD-10" };
        private static readonly string[] DiagnosisTypeOptions = { "ICD9", "ICD10", "Local", "Custom" };
        private static readonly string[] UsageScopeOptions = { "All", "ClinicalDiagnosis", "ProcedureCode" };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DiagnosisController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat metadata filter diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DiagnosisFilterMetadataResponse
            {
                DefaultFilter = new DiagnosisDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = BuildSortOptions(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DiagnosisTypeOptions = DiagnosisTypeOptions.Select(x => new DiagnosisStringOptionResponse { Value = x, Label = BuildDiagnosisTypeLabel(x) }).ToList(),
                IcdVersionOptions = IcdVersionOptions.Select(x => new DiagnosisStringOptionResponse { Value = x, Label = x }).ToList(),
                UsageScopeOptions = BuildUsageScopeOptions(),
                DiagnosisChapterOptions = await GetActiveChapterOptionsAsync(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(LogCategory, "Diagnosis.GetFilterMetadata", "Mengambil metadata filter diagnosis.", result);
            return Ok(ApiResponse<DiagnosisFilterMetadataResponse>.Ok(result, "Metadata filter diagnosis berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat ringkasan diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDiagnosis>().AsNoTracking().Where(x => !x.IsDelete);

            var result = new DiagnosisSummaryResponse
            {
                TotalDiagnosis = await query.CountAsync(),
                ActiveDiagnosis = await query.CountAsync(x => x.IsActive),
                InactiveDiagnosis = await query.CountAsync(x => !x.IsActive),
                SelectableDiagnosis = await query.CountAsync(x => x.IsSelectableForClinicalUse),
                NonSelectableDiagnosis = await query.CountAsync(x => !x.IsSelectableForClinicalUse),
                WithChapterDiagnosis = await query.CountAsync(x => x.DiagnosisChapterId.HasValue),
                WithoutChapterDiagnosis = await query.CountAsync(x => !x.DiagnosisChapterId.HasValue),
                WithParentDiagnosis = await query.CountAsync(x => x.ParentDiagnosisId.HasValue),
                ParentDiagnosis = await query.CountAsync(x => !x.ParentDiagnosisId.HasValue),
                DetailDiagnosis = await query.CountAsync(x => x.ParentDiagnosisId.HasValue),
                PrimaryDiagnosisAllowed = await query.CountAsync(x => x.IsPrimaryDiagnosisAllowed),
                SecondaryDiagnosisAllowed = await query.CountAsync(x => x.IsSecondaryDiagnosisAllowed),
                Icd10Diagnosis = await query.CountAsync(x => x.IcdVersion == "ICD-10"),
                Icd9Diagnosis = await query.CountAsync(x => x.IcdVersion == "ICD-9")
            };

            return Ok(ApiResponse<DiagnosisSummaryResponse>.Ok(result, "Ringkasan diagnosis berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDiagnosisPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat data diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetDiagnoses(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] Guid? diagnosisChapterId,
            [FromQuery] Guid? parentDiagnosisId,
            [FromQuery] string? diagnosisType,
            [FromQuery] string? icdVersion,
            [FromQuery] string? usageScope,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isSelectableForClinicalUse,
            [FromQuery] bool? isPrimaryDiagnosisAllowed,
            [FromQuery] bool? isSecondaryDiagnosisAllowed,
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
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, dateRange.ErrorMessage ?? "Filter tanggal tidak valid."));
            }

            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(query, search, diagnosisChapterId, parentDiagnosisId, diagnosisType, icdVersion, usageScope, isActive, isSelectableForClinicalUse, isPrimaryDiagnosisAllowed, isSecondaryDiagnosisAllowed);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<ResponseDiagnosisPagedResult>.Ok(new ResponseDiagnosisPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data diagnosis berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat pilihan diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetDiagnosisOptions(
            [FromQuery] string? search,
            [FromQuery] Guid? diagnosisChapterId,
            [FromQuery] Guid? parentDiagnosisId,
            [FromQuery] string? diagnosisType,
            [FromQuery] string? icdVersion,
            [FromQuery] string? usageScope,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlySelectable = true,
            [FromQuery] string? sortBy = "diagnosisCode",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(query, search, diagnosisChapterId, parentDiagnosisId, diagnosisType, icdVersion, usageScope, null, null, null, null);
            if (onlyActive) query = query.Where(x => x.IsActive);
            if (onlySelectable) query = query.Where(x => x.IsSelectableForClinicalUse);

            var totalData = await query.CountAsync();
            var optionEntities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var items = optionEntities.Select(MapOptionResponse).ToList();

            return Ok(ApiResponse<DiagnosisOptionPagedResponse>.Ok(new DiagnosisOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data pilihan diagnosis berhasil diambil."));
        }

        [HttpGet("clinical-options")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat pilihan diagnosis klinis ICD-10 untuk SOAP dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetClinicalDiagnosisOptions(
            [FromQuery] string? search,
            [FromQuery] Guid? diagnosisChapterId,
            [FromQuery] Guid? parentDiagnosisId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlySelectable = true,
            [FromQuery] bool? onlyPrimaryAllowed = null,
            [FromQuery] bool? onlySecondaryAllowed = null,
            [FromQuery] string? sortBy = "diagnosisCode",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(
                query,
                search,
                diagnosisChapterId,
                parentDiagnosisId,
                "ICD10",
                "ICD-10",
                "ClinicalDiagnosis",
                onlyActive ? true : null,
                onlySelectable ? true : null,
                onlyPrimaryAllowed,
                onlySecondaryAllowed);

            var totalData = await query.CountAsync();
            var optionEntities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = optionEntities.Select(MapOptionResponse).ToList();

            return Ok(ApiResponse<DiagnosisOptionPagedResponse>.Ok(new DiagnosisOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data pilihan diagnosis klinis ICD-10 berhasil diambil."));
        }

        [HttpGet("icd9-procedure-options")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat pilihan kode prosedur ICD-9 CM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetIcd9ProcedureOptions(
            [FromQuery] string? search,
            [FromQuery] Guid? diagnosisChapterId,
            [FromQuery] Guid? parentDiagnosisId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlySelectable = false,
            [FromQuery] string? sortBy = "diagnosisCode",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(
                query,
                search,
                diagnosisChapterId,
                parentDiagnosisId,
                "ICD9",
                "ICD-9",
                "ProcedureCode",
                onlyActive ? true : null,
                onlySelectable ? true : null,
                null,
                null);

            var totalData = await query.CountAsync();
            var optionEntities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = optionEntities.Select(MapOptionResponse).ToList();

            return Ok(ApiResponse<DiagnosisOptionPagedResponse>.Ok(new DiagnosisOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data pilihan kode prosedur ICD-9 berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat detail diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Diagnosis tidak ditemukan."));
            }

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            return Ok(ApiResponse<DiagnosisDetailResponse>.Ok(MapDetailResponse(entity, actorNames), "Detail diagnosis berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Diagnosis", Description = "Membuat diagnosis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Diagnosis", "Create")]
        public async Task<IActionResult> CreateDiagnosis([FromBody] CreateDiagnosisRequest request)
        {
            var normalized = NormalizeRequest(request);
            var validation = await ValidateRequestAsync(null, normalized);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data diagnosis tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var entity = new MstDiagnosis
            {
                Id = Guid.NewGuid(),
                DiagnosisChapterId = NormalizeNullableGuid(normalized.DiagnosisChapterId),
                ParentDiagnosisId = NormalizeNullableGuid(normalized.ParentDiagnosisId),
                DiagnosisCode = normalized.DiagnosisCode,
                DiagnosisName = normalized.DiagnosisName,
                DiagnosisType = normalized.DiagnosisType,
                IcdVersion = normalized.IcdVersion,
                IsSelectableForClinicalUse = normalized.IsSelectableForClinicalUse,
                IsPrimaryDiagnosisAllowed = normalized.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = normalized.IsSecondaryDiagnosisAllowed,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstDiagnosis>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = ToCreateResponse(entity, actorNames);
            await _loggerService.InfoAsync(LogCategory, "Diagnosis.CreateDiagnosis", "Membuat diagnosis.", result);

            return Ok(ApiResponse<DiagnosisCreateResponse>.Ok(result, "Diagnosis berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis", Description = "Mengubah diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Diagnosis", "Update")]
        public async Task<IActionResult> UpdateDiagnosis(Guid id, [FromBody] UpdateDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosis>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Diagnosis tidak ditemukan."));
            }

            var normalized = NormalizeRequest(request);
            var validation = await ValidateRequestAsync(id, normalized);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data diagnosis tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.DiagnosisChapterId = NormalizeNullableGuid(normalized.DiagnosisChapterId);
            entity.ParentDiagnosisId = NormalizeNullableGuid(normalized.ParentDiagnosisId);
            entity.DiagnosisCode = normalized.DiagnosisCode;
            entity.DiagnosisName = normalized.DiagnosisName;
            entity.DiagnosisType = normalized.DiagnosisType;
            entity.IcdVersion = normalized.IcdVersion;
            entity.IsSelectableForClinicalUse = normalized.IsSelectableForClinicalUse;
            entity.IsPrimaryDiagnosisAllowed = normalized.IsPrimaryDiagnosisAllowed;
            entity.IsSecondaryDiagnosisAllowed = normalized.IsSecondaryDiagnosisAllowed;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = ToUpdateResponse(entity, actorNames);
            await _loggerService.InfoAsync(LogCategory, "Diagnosis.UpdateDiagnosis", "Mengubah diagnosis.", result);

            return Ok(ApiResponse<DiagnosisUpdateResponse>.Ok(result, "Diagnosis berhasil diubah."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis", Description = "Mengubah status diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Diagnosis", "Update")]
        public async Task<IActionResult> UpdateDiagnosisStatus(Guid id, [FromBody] UpdateDiagnosisStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosis>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Diagnosis tidak ditemukan."));
            }

            var actorUserId = GetCurrentUserId();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            return Ok(ApiResponse<DiagnosisUpdateResponse>.Ok(ToUpdateResponse(entity, actorNames), "Status diagnosis berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Diagnosis", Description = "Menghapus diagnosis", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Diagnosis", "Delete")]
        public async Task<IActionResult> DeleteDiagnosis(Guid id, [FromBody] DeleteDiagnosisRequest? request = null)
        {
            var entity = await _dbContext.Set<MstDiagnosis>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Diagnosis tidak ditemukan."));
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

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new DiagnosisDeleteResponse
            {
                Id = entity.Id,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(LogCategory, "Diagnosis.DeleteDiagnosis", "Menghapus diagnosis.", new { result, request?.DeleteReason });
            return Ok(ApiResponse<DiagnosisDeleteResponse>.Ok(result, "Diagnosis berhasil dihapus."));
        }

        private IQueryable<MstDiagnosis> BuildBaseQuery()
        {
            return _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Include(x => x.DiagnosisChapter)
                .Include(x => x.ParentDiagnosis)
                .Include(x => x.DrugRecommendations)
                .Include(x => x.ProcedureRecommendations)
                .Include(x => x.EducationRecommendations)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDiagnosis> ApplyDateFilter(IQueryable<MstDiagnosis> query, DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstDiagnosis> ApplyStandardFilter(
            IQueryable<MstDiagnosis> query,
            string? search,
            Guid? diagnosisChapterId,
            Guid? parentDiagnosisId,
            string? diagnosisType,
            string? icdVersion,
            string? usageScope,
            bool? isActive,
            bool? isSelectableForClinicalUse,
            bool? isPrimaryDiagnosisAllowed,
            bool? isSecondaryDiagnosisAllowed)
        {
            var normalizedChapterId = NormalizeNullableGuid(diagnosisChapterId);
            if (normalizedChapterId.HasValue) query = query.Where(x => x.DiagnosisChapterId == normalizedChapterId.Value);

            var normalizedParentId = NormalizeNullableGuid(parentDiagnosisId);
            if (normalizedParentId.HasValue) query = query.Where(x => x.ParentDiagnosisId == normalizedParentId.Value);

            if (!string.IsNullOrWhiteSpace(diagnosisType)) query = query.Where(x => x.DiagnosisType == NormalizeDiagnosisType(diagnosisType));
            if (!string.IsNullOrWhiteSpace(icdVersion)) query = query.Where(x => x.IcdVersion == NormalizeIcdVersion(icdVersion));

            var normalizedUsageScope = NormalizeUsageScope(usageScope);
            if (normalizedUsageScope == "ClinicalDiagnosis") query = query.Where(x => x.IcdVersion == "ICD-10" && x.DiagnosisType == "ICD10");
            if (normalizedUsageScope == "ProcedureCode") query = query.Where(x => x.IcdVersion == "ICD-9" && x.DiagnosisType == "ICD9");

            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (isSelectableForClinicalUse.HasValue) query = query.Where(x => x.IsSelectableForClinicalUse == isSelectableForClinicalUse.Value);
            if (isPrimaryDiagnosisAllowed.HasValue) query = query.Where(x => x.IsPrimaryDiagnosisAllowed == isPrimaryDiagnosisAllowed.Value);
            if (isSecondaryDiagnosisAllowed.HasValue) query = query.Where(x => x.IsSecondaryDiagnosisAllowed == isSecondaryDiagnosisAllowed.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.DiagnosisCode.ToLower().Contains(keyword) ||
                    x.DiagnosisName.ToLower().Contains(keyword) ||
                    x.DiagnosisType.ToLower().Contains(keyword) ||
                    x.IcdVersion.ToLower().Contains(keyword) ||
                    (x.DiagnosisChapter != null && x.DiagnosisChapter.ChapterCode.ToLower().Contains(keyword)) ||
                    (x.DiagnosisChapter != null && x.DiagnosisChapter.ChapterName.ToLower().Contains(keyword)) ||
                    (x.ParentDiagnosis != null && x.ParentDiagnosis.DiagnosisCode.ToLower().Contains(keyword)) ||
                    (x.ParentDiagnosis != null && x.ParentDiagnosis.DiagnosisName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstDiagnosis> ApplySorting(IQueryable<MstDiagnosis> query, string? sortBy, string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "diagnosisCode").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "diagnosiscode" => isDesc ? query.OrderByDescending(x => x.DiagnosisCode) : query.OrderBy(x => x.DiagnosisCode),
                "diagnosisname" => isDesc ? query.OrderByDescending(x => x.DiagnosisName) : query.OrderBy(x => x.DiagnosisName),
                "chaptername" => isDesc ? query.OrderByDescending(x => x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterName : string.Empty).ThenBy(x => x.DiagnosisCode) : query.OrderBy(x => x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterName : string.Empty).ThenBy(x => x.DiagnosisCode),
                "parentdiagnosisname" => isDesc ? query.OrderByDescending(x => x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisName : string.Empty).ThenBy(x => x.DiagnosisCode) : query.OrderBy(x => x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisName : string.Empty).ThenBy(x => x.DiagnosisCode),
                "diagnosistype" => isDesc ? query.OrderByDescending(x => x.DiagnosisType).ThenBy(x => x.DiagnosisCode) : query.OrderBy(x => x.DiagnosisType).ThenBy(x => x.DiagnosisCode),
                "icdversion" => isDesc ? query.OrderByDescending(x => x.IcdVersion).ThenBy(x => x.DiagnosisCode) : query.OrderBy(x => x.IcdVersion).ThenBy(x => x.DiagnosisCode),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.DiagnosisCode) : query.OrderBy(x => x.IsActive).ThenBy(x => x.DiagnosisCode),
                _ => isDesc ? query.OrderByDescending(x => x.DiagnosisCode) : query.OrderBy(x => x.DiagnosisCode)
            };
        }

        private static DiagnosisResponse MapResponse(MstDiagnosis entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisResponse
            {
                Id = entity.Id,
                DiagnosisChapterId = entity.DiagnosisChapterId,
                ChapterCode = entity.DiagnosisChapter?.ChapterCode,
                ChapterName = entity.DiagnosisChapter?.ChapterName,
                ParentDiagnosisId = entity.ParentDiagnosisId,
                ParentDiagnosisCode = entity.ParentDiagnosis?.DiagnosisCode,
                ParentDiagnosisName = entity.ParentDiagnosis?.DiagnosisName,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DiagnosisType = entity.DiagnosisType,
                DiagnosisTypeName = BuildDiagnosisTypeLabel(entity.DiagnosisType),
                IcdVersion = entity.IcdVersion,
                IsSelectableForClinicalUse = entity.IsSelectableForClinicalUse,
                IsPrimaryDiagnosisAllowed = entity.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = entity.IsSecondaryDiagnosisAllowed,
                DrugRecommendationCount = entity.DrugRecommendations.Count(x => !x.IsDelete),
                ProcedureRecommendationCount = entity.ProcedureRecommendations.Count(x => !x.IsDelete),
                EducationRecommendationCount = entity.EducationRecommendations.Count(x => !x.IsDelete),
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DiagnosisDetailResponse MapDetailResponse(MstDiagnosis entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new DiagnosisDetailResponse
            {
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            var baseResponse = MapResponse(entity, actorNames);
            response.Id = baseResponse.Id;
            response.DiagnosisChapterId = baseResponse.DiagnosisChapterId;
            response.ChapterCode = baseResponse.ChapterCode;
            response.ChapterName = baseResponse.ChapterName;
            response.ParentDiagnosisId = baseResponse.ParentDiagnosisId;
            response.ParentDiagnosisCode = baseResponse.ParentDiagnosisCode;
            response.ParentDiagnosisName = baseResponse.ParentDiagnosisName;
            response.DiagnosisCode = baseResponse.DiagnosisCode;
            response.DiagnosisName = baseResponse.DiagnosisName;
            response.DiagnosisType = baseResponse.DiagnosisType;
            response.DiagnosisTypeName = baseResponse.DiagnosisTypeName;
            response.IcdVersion = baseResponse.IcdVersion;
            response.IsSelectableForClinicalUse = baseResponse.IsSelectableForClinicalUse;
            response.IsPrimaryDiagnosisAllowed = baseResponse.IsPrimaryDiagnosisAllowed;
            response.IsSecondaryDiagnosisAllowed = baseResponse.IsSecondaryDiagnosisAllowed;
            response.DrugRecommendationCount = baseResponse.DrugRecommendationCount;
            response.ProcedureRecommendationCount = baseResponse.ProcedureRecommendationCount;
            response.EducationRecommendationCount = baseResponse.EducationRecommendationCount;
            response.IsActive = baseResponse.IsActive;
            response.CreateDateTime = baseResponse.CreateDateTime;
            response.CreateBy = baseResponse.CreateBy;
            response.CreateByName = baseResponse.CreateByName;

            return response;
        }

        private static DiagnosisOptionResponse MapOptionResponse(MstDiagnosis entity)
        {
            return new DiagnosisOptionResponse
            {
                Id = entity.Id,
                DiagnosisChapterId = entity.DiagnosisChapterId,
                ChapterCode = entity.DiagnosisChapter != null ? entity.DiagnosisChapter.ChapterCode : null,
                ChapterName = entity.DiagnosisChapter != null ? entity.DiagnosisChapter.ChapterName : null,
                ParentDiagnosisId = entity.ParentDiagnosisId,
                ParentDiagnosisCode = entity.ParentDiagnosis != null ? entity.ParentDiagnosis.DiagnosisCode : null,
                ParentDiagnosisName = entity.ParentDiagnosis != null ? entity.ParentDiagnosis.DiagnosisName : null,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DiagnosisType = entity.DiagnosisType,
                DiagnosisTypeName = BuildDiagnosisTypeLabel(entity.DiagnosisType),
                IcdVersion = entity.IcdVersion,
                IsSelectableForClinicalUse = entity.IsSelectableForClinicalUse,
                IsPrimaryDiagnosisAllowed = entity.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = entity.IsSecondaryDiagnosisAllowed,
                IsActive = entity.IsActive
            };
        }

        private static DiagnosisCreateResponse ToCreateResponse(MstDiagnosis entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisCreateResponse
            {
                Id = entity.Id,
                DiagnosisChapterId = entity.DiagnosisChapterId,
                ParentDiagnosisId = entity.ParentDiagnosisId,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DiagnosisType = entity.DiagnosisType,
                IcdVersion = entity.IcdVersion,
                IsSelectableForClinicalUse = entity.IsSelectableForClinicalUse,
                IsPrimaryDiagnosisAllowed = entity.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = entity.IsSecondaryDiagnosisAllowed,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DiagnosisUpdateResponse ToUpdateResponse(MstDiagnosis entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisUpdateResponse
            {
                Id = entity.Id,
                DiagnosisChapterId = entity.DiagnosisChapterId,
                ParentDiagnosisId = entity.ParentDiagnosisId,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DiagnosisType = entity.DiagnosisType,
                IcdVersion = entity.IcdVersion,
                IsSelectableForClinicalUse = entity.IsSelectableForClinicalUse,
                IsPrimaryDiagnosisAllowed = entity.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = entity.IsSecondaryDiagnosisAllowed,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static CreateDiagnosisRequest NormalizeRequest(CreateDiagnosisRequest request)
        {
            return new CreateDiagnosisRequest
            {
                DiagnosisChapterId = NormalizeNullableGuid(request.DiagnosisChapterId),
                ParentDiagnosisId = NormalizeNullableGuid(request.ParentDiagnosisId),
                DiagnosisCode = NormalizeRequiredText(request.DiagnosisCode).ToUpperInvariant(),
                DiagnosisName = NormalizeRequiredText(request.DiagnosisName),
                DiagnosisType = NormalizeDiagnosisType(request.DiagnosisType),
                IcdVersion = NormalizeIcdVersion(request.IcdVersion),
                IsSelectableForClinicalUse = request.IsSelectableForClinicalUse,
                IsPrimaryDiagnosisAllowed = request.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = request.IsSecondaryDiagnosisAllowed
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? currentId, CreateDiagnosisRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DiagnosisCode)) return (false, "Kode diagnosis wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.DiagnosisName)) return (false, "Nama diagnosis wajib diisi.");
            if (!DiagnosisTypeOptions.Contains(request.DiagnosisType, StringComparer.OrdinalIgnoreCase)) return (false, "Tipe diagnosis tidak valid.");
            if (!IcdVersionOptions.Contains(request.IcdVersion, StringComparer.OrdinalIgnoreCase)) return (false, "Versi ICD tidak valid.");

            if (request.DiagnosisType == "ICD10" && request.IcdVersion != "ICD-10")
                return (false, "DiagnosisType ICD10 harus memakai IcdVersion ICD-10.");

            if (request.DiagnosisType == "ICD9" && request.IcdVersion != "ICD-9")
                return (false, "DiagnosisType ICD9 harus memakai IcdVersion ICD-9.");

            var duplicate = await _dbContext.Set<MstDiagnosis>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.IcdVersion == request.IcdVersion &&
                    x.DiagnosisCode == request.DiagnosisCode &&
                    (!currentId.HasValue || x.Id != currentId.Value));

            if (duplicate) return (false, "Kode diagnosis untuk versi ICD tersebut sudah digunakan.");

            if (request.DiagnosisChapterId.HasValue)
            {
                var chapter = await _dbContext.Set<MstDiagnosisChapter>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.Id == request.DiagnosisChapterId.Value);

                if (chapter == null) return (false, "Diagnosis chapter tidak valid atau tidak aktif.");
                if (chapter.IcdVersion != request.IcdVersion) return (false, "Versi ICD pada diagnosis chapter harus sama dengan versi ICD diagnosis.");
            }

            if (request.ParentDiagnosisId.HasValue)
            {
                if (currentId.HasValue && request.ParentDiagnosisId.Value == currentId.Value)
                    return (false, "Parent diagnosis tidak boleh sama dengan diagnosis yang sedang diubah.");

                var parent = await _dbContext.Set<MstDiagnosis>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.Id == request.ParentDiagnosisId.Value);

                if (parent == null) return (false, "Parent diagnosis tidak valid atau tidak aktif.");
                if (parent.IcdVersion != request.IcdVersion) return (false, "Versi ICD pada parent diagnosis harus sama dengan versi ICD diagnosis.");
                if (parent.DiagnosisType != request.DiagnosisType) return (false, "Tipe diagnosis pada parent diagnosis harus sama dengan tipe diagnosis.");
            }

            return (true, null);
        }

        private async Task<List<DiagnosisChapterOptionResponse>> GetActiveChapterOptionsAsync()
        {
            return await _dbContext.Set<MstDiagnosisChapter>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.ChapterCode)
                .Take(200)
                .Select(x => new DiagnosisChapterOptionResponse
                {
                    Id = x.Id,
                    ChapterCode = x.ChapterCode,
                    ChapterName = x.ChapterName,
                    DiagnosisCodeRangeStart = x.DiagnosisCodeRangeStart,
                    DiagnosisCodeRangeEnd = x.DiagnosisCodeRangeEnd,
                    IcdVersion = x.IcdVersion,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId)
        {
            return actorId == Guid.Empty ? null : actorNames.TryGetValue(actorId, out var actorName) ? actorName : null;
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
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
            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar atau sama dengan EndDate.");
            }

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<DiagnosisCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<DiagnosisCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<DiagnosisSortOptionResponse> BuildSortOptions()
        {
            return new List<DiagnosisSortOptionResponse>
            {
                new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                new() { Value = "diagnosisCode", Label = "Kode diagnosis" },
                new() { Value = "diagnosisName", Label = "Nama diagnosis" },
                new() { Value = "chapterName", Label = "Chapter diagnosis" },
                new() { Value = "parentDiagnosisName", Label = "Parent diagnosis" },
                new() { Value = "diagnosisType", Label = "Tipe diagnosis" },
                new() { Value = "icdVersion", Label = "Versi ICD" },
                new() { Value = "isActive", Label = "Status aktif" }
            };
        }


        private static List<DiagnosisStringOptionResponse> BuildUsageScopeOptions()
        {
            return new List<DiagnosisStringOptionResponse>
            {
                new() { Value = "All", Label = "Semua" },
                new() { Value = "ClinicalDiagnosis", Label = "Diagnosis klinis ICD-10" },
                new() { Value = "ProcedureCode", Label = "Kode prosedur ICD-9" }
            };
        }

        private static List<DiagnosisQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<DiagnosisQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari kode diagnosis, nama diagnosis, chapter, parent, tipe, atau versi ICD.", Example = "I10" },
                new() { Name = "diagnosisChapterId", Type = "Guid?", Description = "Filter berdasarkan chapter diagnosis." },
                new() { Name = "parentDiagnosisId", Type = "Guid?", Description = "Filter berdasarkan parent diagnosis." },
                new() { Name = "diagnosisType", Type = "string", Description = "Filter tipe diagnosis.", Example = "ICD10" },
                new() { Name = "icdVersion", Type = "string", Description = "Filter versi ICD.", Example = "ICD-10" },
                new() { Name = "usageScope", Type = "string", Description = "Filter scope penggunaan: All, ClinicalDiagnosis, ProcedureCode.", Example = "ClinicalDiagnosis" },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "isSelectableForClinicalUse", Type = "bool?", Description = "Filter diagnosis yang dapat dipilih dokter.", Example = "true" },
                new() { Name = "isPrimaryDiagnosisAllowed", Type = "bool?", Description = "Filter diagnosis yang boleh menjadi diagnosis primer.", Example = "true" },
                new() { Name = "isSecondaryDiagnosisAllowed", Type = "bool?", Description = "Filter diagnosis yang boleh menjadi diagnosis sekunder.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "diagnosisCode" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<DiagnosisFormFieldMetadataResponse> BuildCreateFieldMetadata() => BuildFieldMetadata(isUpdate: false);
        private static List<DiagnosisFormFieldMetadataResponse> BuildUpdateFieldMetadata() => BuildFieldMetadata(isUpdate: true);

        private static List<DiagnosisFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<DiagnosisFormFieldMetadataResponse>
            {
                new() { Name = "diagnosisChapterId", Label = "Diagnosis Chapter", Section = "Relation", InputType = "select", OptionsSource = "diagnosisChapterOptions", Description = "Opsional. Grouping besar ICD.", SortOrder = 1 },
                new() { Name = "parentDiagnosisId", Label = "Parent Diagnosis", Section = "Relation", InputType = "select", Description = "Opsional. Parent ICD, contoh A00 untuk A00.0.", SortOrder = 2 },
                new() { Name = "diagnosisCode", Label = "Kode Diagnosis", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 50, Example = "I10", SortOrder = 3 },
                new() { Name = "diagnosisName", Label = "Nama Diagnosis", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 500, Example = "Essential hypertension", SortOrder = 4 },
                new() { Name = "diagnosisType", Label = "Tipe Diagnosis", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "diagnosisTypeOptions", Example = "ICD10", SortOrder = 5 },
                new() { Name = "icdVersion", Label = "Versi ICD", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "icdVersionOptions", Example = "ICD-10", SortOrder = 6 },
                new() { Name = "isSelectableForClinicalUse", Label = "Bisa Dipilih Dokter", Section = "Clinical Rule", InputType = "switch", Description = "False jika hanya parent/group ICD.", SortOrder = 7 },
                new() { Name = "isPrimaryDiagnosisAllowed", Label = "Boleh Diagnosis Primer", Section = "Clinical Rule", InputType = "switch", SortOrder = 8 },
                new() { Name = "isSecondaryDiagnosisAllowed", Label = "Boleh Diagnosis Sekunder", Section = "Clinical Rule", InputType = "switch", SortOrder = 9 }
            };

            if (isUpdate)
            {
                fields.Add(new DiagnosisFormFieldMetadataResponse { Name = "isActive", Label = "Status Aktif", Section = "Status", InputType = "switch", SortOrder = 99 });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }


        private static string NormalizeUsageScope(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "All";

            var normalized = value.Trim().Replace("-", string.Empty).Replace("_", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
            return normalized switch
            {
                "ALL" => "All",
                "CLINICAL" => "ClinicalDiagnosis",
                "CLINICALDIAGNOSIS" => "ClinicalDiagnosis",
                "ICD10" => "ClinicalDiagnosis",
                "DIAGNOSIS" => "ClinicalDiagnosis",
                "PROCEDURE" => "ProcedureCode",
                "PROCEDURECODE" => "ProcedureCode",
                "ICD9" => "ProcedureCode",
                _ => "All"
            };
        }

        private static string BuildDiagnosisTypeLabel(string value)
        {
            return value switch
            {
                "ICD9" => "ICD-9",
                "ICD10" => "ICD-10",
                "Local" => "Lokal RS",
                "Custom" => "Custom",
                _ => value
            };
        }

        private static string NormalizeDiagnosisType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "ICD10";
            var normalized = value.Trim().Replace("-", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
            return normalized switch
            {
                "ICD9" => "ICD9",
                "ICD10" => "ICD10",
                "LOCAL" => "Local",
                "CUSTOM" => "Custom",
                _ => value.Trim()
            };
        }

        private static string NormalizeIcdVersion(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "ICD-10";
            var normalized = value.Trim().ToUpperInvariant().Replace(" ", string.Empty);
            return normalized switch
            {
                "ICD9" => "ICD-9",
                "ICD-9" => "ICD-9",
                "ICD10" => "ICD-10",
                "ICD-10" => "ICD-10",
                _ => value.Trim()
            };
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        }

        private static string NormalizeRequiredText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
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
