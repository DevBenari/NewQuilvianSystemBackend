using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
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

        private static readonly HashSet<string> AllowedDiagnosisTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "ICD10",
            "Local",
            "Custom"
        };

        private static readonly HashSet<string> AllowedGenderRestrictions = new(StringComparer.OrdinalIgnoreCase)
        {
            "None",
            "Male",
            "Female"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DiagnosisController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat data diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DiagnosisFilterMetadataResponse
            {
                DefaultFilter = new DiagnosisDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<DiagnosisSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "diagnosisCode", Label = "Kode diagnosis" },
                    new() { Value = "diagnosisName", Label = "Nama diagnosis" },
                    new() { Value = "chapterName", Label = "Chapter diagnosis" },
                    new() { Value = "parentDiagnosisName", Label = "Parent diagnosis" },
                    new() { Value = "diagnosisType", Label = "Tipe diagnosis" },
                    new() { Value = "icdVersion", Label = "Versi ICD" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Diagnosis.GetFilterMetadata",
                "Mengambil metadata filter diagnosis.",
                result
            );

            return Ok(ApiResponse<DiagnosisFilterMetadataResponse>.Ok(
                result,
                "Metadata filter diagnosis berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat data diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DiagnosisSummaryResponse
            {
                TotalDiagnosis = await query.CountAsync(),
                ActiveDiagnosis = await query.CountAsync(x => x.IsActive),
                InactiveDiagnosis = await query.CountAsync(x => !x.IsActive),
                SelectableDiagnosis = await query.CountAsync(x => x.IsSelectableForClinicalUse),
                BillableDiagnosis = await query.CountAsync(x => x.IsBillable),
                PrimaryDiagnosisAllowed = await query.CountAsync(x => x.IsPrimaryDiagnosisAllowed),
                SecondaryDiagnosisAllowed = await query.CountAsync(x => x.IsSecondaryDiagnosisAllowed),
                ChronicDiseaseDiagnosis = await query.CountAsync(x => x.IsChronicDisease),
                InfectiousDiseaseDiagnosis = await query.CountAsync(x => x.IsInfectiousDisease),
                ExternalCauseDiagnosis = await query.CountAsync(x => x.IsExternalCause),
                PregnancyRelatedDiagnosis = await query.CountAsync(x => x.IsPregnancyRelated),
                MentalHealthRelatedDiagnosis = await query.CountAsync(x => x.IsMentalHealthRelated),
                RareDiseaseDiagnosis = await query.CountAsync(x => x.IsRareDisease),
                WithChapterDiagnosis = await query.CountAsync(x => x.DiagnosisChapterId.HasValue),
                WithParentDiagnosis = await query.CountAsync(x => x.ParentDiagnosisId.HasValue)
            };

            return Ok(ApiResponse<DiagnosisSummaryResponse>.Ok(
                result,
                "Ringkasan diagnosis berhasil diambil."
            ));
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
            [FromQuery] Guid? diagnosisChapterId,
            [FromQuery] Guid? parentDiagnosisId,
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

            if (diagnosisChapterId.HasValue && diagnosisChapterId.Value != Guid.Empty)
                query = query.Where(x => x.DiagnosisChapterId == diagnosisChapterId.Value);

            if (parentDiagnosisId.HasValue && parentDiagnosisId.Value != Guid.Empty)
                query = query.Where(x => x.ParentDiagnosisId == parentDiagnosisId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseDiagnosisPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDiagnosisPagedResult>.Ok(
                result,
                "Data diagnosis berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat data pilihan diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetDiagnosisOptions(
    [FromQuery] Guid? diagnosisChapterId,
    [FromQuery] Guid? parentDiagnosisId,
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

            if (diagnosisChapterId.HasValue && diagnosisChapterId.Value != Guid.Empty)
                query = query.Where(x => x.DiagnosisChapterId == diagnosisChapterId.Value);

            if (parentDiagnosisId.HasValue && parentDiagnosisId.Value != Guid.Empty)
                query = query.Where(x => x.ParentDiagnosisId == parentDiagnosisId.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DiagnosisCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DiagnosisOptionResponse
                {
                    Id = x.Id,

                    DiagnosisChapterId = x.DiagnosisChapterId,
                    ChapterCode = x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterCode : null,
                    ChapterName = x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterName : null,

                    ParentDiagnosisId = x.ParentDiagnosisId,
                    ParentDiagnosisCode = x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisCode : null,
                    ParentDiagnosisName = x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisName : null,

                    DiagnosisCode = x.DiagnosisCode,
                    DiagnosisName = x.DiagnosisName,
                    ShortName = x.ShortName,
                    DiagnosisGroupName = x.DiagnosisGroupName,
                    DiagnosisCategoryName = x.DiagnosisCategoryName,
                    DiagnosisType = x.DiagnosisType,
                    IcdVersion = x.IcdVersion,

                    IsSelectableForClinicalUse = x.IsSelectableForClinicalUse,
                    IsBillable = x.IsBillable,
                    IsPrimaryDiagnosisAllowed = x.IsPrimaryDiagnosisAllowed,
                    IsSecondaryDiagnosisAllowed = x.IsSecondaryDiagnosisAllowed,

                    GenderRestriction = x.GenderRestriction,
                    MinimumAgeYear = x.MinimumAgeYear,
                    MaximumAgeYear = x.MaximumAgeYear
                })
                .ToListAsync();

            var result = new DiagnosisOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DiagnosisOptionPagedResponse>.Ok(
                result,
                "Data pilihan diagnosis berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat data diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetDiagnosisById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DiagnosisDetailResponse
                {
                    Id = x.Id,
                    DiagnosisChapterId = x.DiagnosisChapterId,
                    ChapterCode = x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterCode : null,
                    ChapterName = x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterName : null,
                    ParentDiagnosisId = x.ParentDiagnosisId,
                    ParentDiagnosisCode = x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisCode : null,
                    ParentDiagnosisName = x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisName : null,
                    DiagnosisCode = x.DiagnosisCode,
                    DiagnosisName = x.DiagnosisName,
                    DiagnosisNameEnglish = x.DiagnosisNameEnglish,
                    ShortName = x.ShortName,
                    DiagnosisGroupName = x.DiagnosisGroupName,
                    DiagnosisCategoryName = x.DiagnosisCategoryName,
                    DiagnosisType = x.DiagnosisType,
                    IcdVersion = x.IcdVersion,
                    IsSelectableForClinicalUse = x.IsSelectableForClinicalUse,
                    IsBillable = x.IsBillable,
                    IsPrimaryDiagnosisAllowed = x.IsPrimaryDiagnosisAllowed,
                    IsSecondaryDiagnosisAllowed = x.IsSecondaryDiagnosisAllowed,
                    IsChronicDisease = x.IsChronicDisease,
                    IsInfectiousDisease = x.IsInfectiousDisease,
                    IsExternalCause = x.IsExternalCause,
                    IsPregnancyRelated = x.IsPregnancyRelated,
                    IsMentalHealthRelated = x.IsMentalHealthRelated,
                    IsRareDisease = x.IsRareDisease,
                    GenderRestriction = x.GenderRestriction,
                    MinimumAgeYear = x.MinimumAgeYear,
                    MaximumAgeYear = x.MaximumAgeYear,
                    ExternalDiagnosisCode = x.ExternalDiagnosisCode,
                    IntegrationCode = x.IntegrationCode,
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
                    "Diagnosis tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DiagnosisDetailResponse>.Ok(
                data,
                "Detail diagnosis berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Diagnosis", Description = "Membuat data diagnosis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Diagnosis", "Create")]
        public async Task<IActionResult> CreateDiagnosis([FromBody] CreateDiagnosisRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                diagnosisChapterId: request.DiagnosisChapterId,
                parentDiagnosisId: request.ParentDiagnosisId,
                diagnosisCode: request.DiagnosisCode,
                diagnosisName: request.DiagnosisName,
                diagnosisType: request.DiagnosisType,
                icdVersion: request.IcdVersion,
                genderRestriction: request.GenderRestriction,
                minimumAgeYear: request.MinimumAgeYear,
                maximumAgeYear: request.MaximumAgeYear
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data diagnosis tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstDiagnosis
            {
                Id = Guid.NewGuid(),
                DiagnosisChapterId = NormalizeNullableGuid(request.DiagnosisChapterId),
                ParentDiagnosisId = NormalizeNullableGuid(request.ParentDiagnosisId),
                DiagnosisCode = request.DiagnosisCode.Trim().ToUpperInvariant(),
                DiagnosisName = request.DiagnosisName.Trim(),
                DiagnosisNameEnglish = NormalizeNullableText(request.DiagnosisNameEnglish),
                ShortName = NormalizeNullableText(request.ShortName),
                DiagnosisGroupName = NormalizeNullableText(request.DiagnosisGroupName),
                DiagnosisCategoryName = NormalizeNullableText(request.DiagnosisCategoryName),
                DiagnosisType = NormalizeDiagnosisType(request.DiagnosisType),
                IcdVersion = NormalizeIcdVersion(request.IcdVersion),
                IsSelectableForClinicalUse = request.IsSelectableForClinicalUse,
                IsBillable = request.IsBillable,
                IsPrimaryDiagnosisAllowed = request.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = request.IsSecondaryDiagnosisAllowed,
                IsChronicDisease = request.IsChronicDisease,
                IsInfectiousDisease = request.IsInfectiousDisease,
                IsExternalCause = request.IsExternalCause,
                IsPregnancyRelated = request.IsPregnancyRelated,
                IsMentalHealthRelated = request.IsMentalHealthRelated,
                IsRareDisease = request.IsRareDisease,
                GenderRestriction = NormalizeGenderRestriction(request.GenderRestriction),
                MinimumAgeYear = request.MinimumAgeYear,
                MaximumAgeYear = request.MaximumAgeYear,
                ExternalDiagnosisCode = NormalizeNullableText(request.ExternalDiagnosisCode),
                IntegrationCode = NormalizeNullableText(request.IntegrationCode),
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDiagnosis>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "Diagnosis.CreateDiagnosis",
                "Membuat data diagnosis.",
                response
            );

            return Ok(ApiResponse<DiagnosisCreateResponse>.Ok(
                response,
                "Diagnosis berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis", Description = "Mengubah data diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Diagnosis", "Update")]
        public async Task<IActionResult> UpdateDiagnosis(Guid id, [FromBody] UpdateDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<MstDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                diagnosisChapterId: request.DiagnosisChapterId,
                parentDiagnosisId: request.ParentDiagnosisId,
                diagnosisCode: request.DiagnosisCode,
                diagnosisName: request.DiagnosisName,
                diagnosisType: request.DiagnosisType,
                icdVersion: request.IcdVersion,
                genderRestriction: request.GenderRestriction,
                minimumAgeYear: request.MinimumAgeYear,
                maximumAgeYear: request.MaximumAgeYear
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data diagnosis tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.DiagnosisChapterId = NormalizeNullableGuid(request.DiagnosisChapterId);
            entity.ParentDiagnosisId = NormalizeNullableGuid(request.ParentDiagnosisId);
            entity.DiagnosisCode = request.DiagnosisCode.Trim().ToUpperInvariant();
            entity.DiagnosisName = request.DiagnosisName.Trim();
            entity.DiagnosisNameEnglish = NormalizeNullableText(request.DiagnosisNameEnglish);
            entity.ShortName = NormalizeNullableText(request.ShortName);
            entity.DiagnosisGroupName = NormalizeNullableText(request.DiagnosisGroupName);
            entity.DiagnosisCategoryName = NormalizeNullableText(request.DiagnosisCategoryName);
            entity.DiagnosisType = NormalizeDiagnosisType(request.DiagnosisType);
            entity.IcdVersion = NormalizeIcdVersion(request.IcdVersion);
            entity.IsSelectableForClinicalUse = request.IsSelectableForClinicalUse;
            entity.IsBillable = request.IsBillable;
            entity.IsPrimaryDiagnosisAllowed = request.IsPrimaryDiagnosisAllowed;
            entity.IsSecondaryDiagnosisAllowed = request.IsSecondaryDiagnosisAllowed;
            entity.IsChronicDisease = request.IsChronicDisease;
            entity.IsInfectiousDisease = request.IsInfectiousDisease;
            entity.IsExternalCause = request.IsExternalCause;
            entity.IsPregnancyRelated = request.IsPregnancyRelated;
            entity.IsMentalHealthRelated = request.IsMentalHealthRelated;
            entity.IsRareDisease = request.IsRareDisease;
            entity.GenderRestriction = NormalizeGenderRestriction(request.GenderRestriction);
            entity.MinimumAgeYear = request.MinimumAgeYear;
            entity.MaximumAgeYear = request.MaximumAgeYear;
            entity.ExternalDiagnosisCode = NormalizeNullableText(request.ExternalDiagnosisCode);
            entity.IntegrationCode = NormalizeNullableText(request.IntegrationCode);
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = new DiagnosisUpdateResponse
            {
                Id = entity.Id,
                DiagnosisChapterId = entity.DiagnosisChapterId,
                ParentDiagnosisId = entity.ParentDiagnosisId,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DiagnosisType = entity.DiagnosisType,
                IcdVersion = entity.IcdVersion,
                IsSelectableForClinicalUse = entity.IsSelectableForClinicalUse,
                IsBillable = entity.IsBillable,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Diagnosis.UpdateDiagnosis",
                "Mengubah data diagnosis.",
                response
            );

            return Ok(ApiResponse<DiagnosisUpdateResponse>.Ok(
                response,
                "Diagnosis berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Diagnosis", Description = "Menghapus data diagnosis", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Diagnosis", "Delete")]
        public async Task<IActionResult> DeleteDiagnosis(Guid id)
        {
            var entity = await _dbContext.Set<MstDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis tidak ditemukan."
                ));
            }

            var isUsedByChildDiagnosis = await _dbContext.Set<MstDiagnosis>()
                .AnyAsync(x => x.ParentDiagnosisId == id && !x.IsDelete);

            if (isUsedByChildDiagnosis)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Diagnosis tidak dapat dihapus karena sudah digunakan sebagai parent diagnosis."
                ));
            }

            var isUsedByPatientDiagnosis = await _dbContext.Set<TrxPatientDiagnosis>()
                .AnyAsync(x => x.DiagnosisId == id && !x.IsDelete);

            var isUsedByFamilyHistory = await _dbContext.Set<TrxPatientFamilyHistory>()
                .AnyAsync(x => x.DiagnosisId == id && !x.IsDelete);

            if (isUsedByPatientDiagnosis || isUsedByFamilyHistory)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Diagnosis tidak dapat dihapus karena sudah digunakan pada data klinis pasien."
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

            return Ok(ApiResponse<object>.Ok(
                null,
                "Diagnosis berhasil dihapus."
            ));
        }

        private IQueryable<MstDiagnosis> BuildBaseQuery()
        {
            return _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDiagnosis> ApplySearch(IQueryable<MstDiagnosis> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var keyword = search.Trim().ToLower();

            return query.Where(x =>
                x.DiagnosisCode.ToLower().Contains(keyword) ||
                x.DiagnosisName.ToLower().Contains(keyword) ||
                x.DiagnosisType.ToLower().Contains(keyword) ||
                x.IcdVersion.ToLower().Contains(keyword) ||
                (x.DiagnosisNameEnglish != null && x.DiagnosisNameEnglish.ToLower().Contains(keyword)) ||
                (x.ShortName != null && x.ShortName.ToLower().Contains(keyword)) ||
                (x.DiagnosisGroupName != null && x.DiagnosisGroupName.ToLower().Contains(keyword)) ||
                (x.DiagnosisCategoryName != null && x.DiagnosisCategoryName.ToLower().Contains(keyword)) ||
                (x.ExternalDiagnosisCode != null && x.ExternalDiagnosisCode.ToLower().Contains(keyword)) ||
                (x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword)) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                (x.DiagnosisChapter != null && x.DiagnosisChapter.ChapterCode.ToLower().Contains(keyword)) ||
                (x.DiagnosisChapter != null && x.DiagnosisChapter.ChapterName.ToLower().Contains(keyword)) ||
                (x.ParentDiagnosis != null && x.ParentDiagnosis.DiagnosisCode.ToLower().Contains(keyword)) ||
                (x.ParentDiagnosis != null && x.ParentDiagnosis.DiagnosisName.ToLower().Contains(keyword)));
        }

        private static DiagnosisResponse ToResponse(MstDiagnosis x)
        {
            return new DiagnosisResponse
            {
                Id = x.Id,
                DiagnosisChapterId = x.DiagnosisChapterId,
                ChapterCode = x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterCode : null,
                ChapterName = x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterName : null,
                ParentDiagnosisId = x.ParentDiagnosisId,
                ParentDiagnosisCode = x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisCode : null,
                ParentDiagnosisName = x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisName : null,
                DiagnosisCode = x.DiagnosisCode,
                DiagnosisName = x.DiagnosisName,
                DiagnosisNameEnglish = x.DiagnosisNameEnglish,
                ShortName = x.ShortName,
                DiagnosisGroupName = x.DiagnosisGroupName,
                DiagnosisCategoryName = x.DiagnosisCategoryName,
                DiagnosisType = x.DiagnosisType,
                IcdVersion = x.IcdVersion,
                IsSelectableForClinicalUse = x.IsSelectableForClinicalUse,
                IsBillable = x.IsBillable,
                IsPrimaryDiagnosisAllowed = x.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = x.IsSecondaryDiagnosisAllowed,
                IsChronicDisease = x.IsChronicDisease,
                IsInfectiousDisease = x.IsInfectiousDisease,
                IsExternalCause = x.IsExternalCause,
                IsPregnancyRelated = x.IsPregnancyRelated,
                IsMentalHealthRelated = x.IsMentalHealthRelated,
                IsRareDisease = x.IsRareDisease,
                GenderRestriction = x.GenderRestriction,
                MinimumAgeYear = x.MinimumAgeYear,
                MaximumAgeYear = x.MaximumAgeYear,
                ExternalDiagnosisCode = x.ExternalDiagnosisCode,
                IntegrationCode = x.IntegrationCode,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid? diagnosisChapterId,
            Guid? parentDiagnosisId,
            string diagnosisCode,
            string diagnosisName,
            string diagnosisType,
            string icdVersion,
            string? genderRestriction,
            int? minimumAgeYear,
            int? maximumAgeYear)
        {
            if (string.IsNullOrWhiteSpace(diagnosisCode))
                return (false, "Kode diagnosis wajib diisi.");

            if (string.IsNullOrWhiteSpace(diagnosisName))
                return (false, "Nama diagnosis wajib diisi.");

            if (string.IsNullOrWhiteSpace(diagnosisType))
                return (false, "Tipe diagnosis wajib diisi.");

            if (!AllowedDiagnosisTypes.Contains(diagnosisType.Trim()))
                return (false, "Tipe diagnosis tidak valid. Gunakan salah satu: ICD10, Local, Custom.");

            if (string.IsNullOrWhiteSpace(icdVersion))
                return (false, "Versi ICD wajib diisi.");

            if (!string.IsNullOrWhiteSpace(genderRestriction) &&
                !AllowedGenderRestrictions.Contains(genderRestriction.Trim()))
            {
                return (false, "Gender restriction tidak valid. Gunakan salah satu: None, Male, Female.");
            }

            if (minimumAgeYear.HasValue && minimumAgeYear.Value < 0)
                return (false, "Minimum age tidak boleh kurang dari 0.");

            if (maximumAgeYear.HasValue && maximumAgeYear.Value < 0)
                return (false, "Maximum age tidak boleh kurang dari 0.");

            if (minimumAgeYear.HasValue && minimumAgeYear.Value > 150)
                return (false, "Minimum age tidak boleh lebih dari 150 tahun.");

            if (maximumAgeYear.HasValue && maximumAgeYear.Value > 150)
                return (false, "Maximum age tidak boleh lebih dari 150 tahun.");

            if (minimumAgeYear.HasValue && maximumAgeYear.HasValue && minimumAgeYear.Value > maximumAgeYear.Value)
                return (false, "Minimum age tidak boleh lebih besar dari maximum age.");

            var normalizedChapterId = NormalizeNullableGuid(diagnosisChapterId);

            if (normalizedChapterId.HasValue)
            {
                var chapterExists = await _dbContext.Set<MstDiagnosisChapter>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedChapterId.Value && x.IsActive && !x.IsDelete);

                if (!chapterExists)
                    return (false, "Diagnosis chapter tidak valid atau tidak aktif.");
            }

            var normalizedParentDiagnosisId = NormalizeNullableGuid(parentDiagnosisId);

            if (normalizedParentDiagnosisId.HasValue)
            {
                if (excludeId.HasValue && normalizedParentDiagnosisId.Value == excludeId.Value)
                    return (false, "Parent diagnosis tidak boleh sama dengan diagnosis yang sedang diubah.");

                var parentExists = await _dbContext.Set<MstDiagnosis>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedParentDiagnosisId.Value && x.IsActive && !x.IsDelete);

                if (!parentExists)
                    return (false, "Parent diagnosis tidak valid atau tidak aktif.");
            }

            var normalizedCode = diagnosisCode.Trim().ToUpperInvariant();
            var normalizedName = diagnosisName.Trim().ToLower();
            var normalizedType = NormalizeDiagnosisType(diagnosisType);
            var normalizedIcdVersion = NormalizeIcdVersion(icdVersion);

            var duplicateCodeQuery = _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DiagnosisCode.ToUpper() == normalizedCode);

            if (excludeId.HasValue)
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateCodeQuery.AnyAsync())
                return (false, "Kode diagnosis sudah digunakan.");

            var duplicateNameQuery = _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DiagnosisName.ToLower() == normalizedName &&
                    x.DiagnosisType.ToUpper() == normalizedType.ToUpper() &&
                    x.IcdVersion.ToUpper() == normalizedIcdVersion.ToUpper());

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama diagnosis pada tipe dan versi ICD tersebut sudah digunakan.");

            return (true, null);
        }

        private static DiagnosisCreateResponse ToCreateUpdateResponse(MstDiagnosis entity)
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
                IsBillable = entity.IsBillable,
                IsActive = entity.IsActive
            };
        }

        private static IQueryable<MstDiagnosis> ApplySorting(
            IQueryable<MstDiagnosis> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "diagnosiscode" => isDesc
                    ? query.OrderByDescending(x => x.DiagnosisCode)
                    : query.OrderBy(x => x.DiagnosisCode),

                "diagnosisname" => isDesc
                    ? query.OrderByDescending(x => x.DiagnosisName)
                    : query.OrderBy(x => x.DiagnosisName),

                "chaptername" => isDesc
                    ? query.OrderByDescending(x => x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterName : "")
                    : query.OrderBy(x => x.DiagnosisChapter != null ? x.DiagnosisChapter.ChapterName : ""),

                "parentdiagnosisname" => isDesc
                    ? query.OrderByDescending(x => x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisName : "")
                    : query.OrderBy(x => x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisName : ""),

                "diagnosistype" => isDesc
                    ? query.OrderByDescending(x => x.DiagnosisType)
                    : query.OrderBy(x => x.DiagnosisType),

                "icdversion" => isDesc
                    ? query.OrderByDescending(x => x.IcdVersion)
                    : query.OrderBy(x => x.IcdVersion),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.DiagnosisCode)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.DiagnosisCode)
            };
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateTime.UtcNow.Date;
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
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar dari EndDate.");

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<DiagnosisCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<DiagnosisCustomPeriodOptionResponse>
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

        private static List<DiagnosisQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<DiagnosisQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Jika bukan custom, startDate dan endDate diabaikan.", Example = "thisMonth" },
                new() { Name = "diagnosisChapterId", Type = "guid", Description = "Relasi filter 1: diagnosis chapter." },
                new() { Name = "parentDiagnosisId", Type = "guid", Description = "Relasi filter 2: parent diagnosis." },
                new() { Name = "isActive", Type = "boolean", Description = "Filter status aktif." },
                new() { Name = "search", Type = "string", Description = "Pencarian kode diagnosis, nama diagnosis, chapter, parent, type, versi ICD, atau keterangan." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting." },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<DiagnosisFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return new List<DiagnosisFormFieldMetadataResponse>
            {
                new() { Name = "diagnosisChapterId", Label = "Diagnosis chapter", DataType = "guid", InputType = "select", Required = false, IsReadonly = false, Description = "Relasi ke master diagnosis chapter." },
                new() { Name = "parentDiagnosisId", Label = "Parent diagnosis", DataType = "guid", InputType = "select", Required = false, IsReadonly = false, Description = "Opsional untuk struktur parent-child diagnosis." },
                new() { Name = "diagnosisCode", Label = "Kode diagnosis", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "A00.0", Description = "Kode resmi diagnosis seperti ICD-10, contoh A00.0, I10, E11.9." },
                new() { Name = "diagnosisName", Label = "Nama diagnosis", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "Cholera due to Vibrio cholerae 01, biovar cholerae" },
                new() { Name = "diagnosisNameEnglish", Label = "Nama diagnosis Inggris", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "shortName", Label = "Nama singkat", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "diagnosisGroupName", Label = "Group diagnosis", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "diagnosisCategoryName", Label = "Kategori diagnosis", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "diagnosisType", Label = "Tipe diagnosis", DataType = "string", InputType = "select", Required = true, IsReadonly = false, Placeholder = "ICD10", Description = "ICD10, Local, atau Custom." },
                new() { Name = "icdVersion", Label = "Versi ICD", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "ICD-10" },
                new() { Name = "isSelectableForClinicalUse", Label = "Bisa dipilih klinis", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isBillable", Label = "Billable", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isPrimaryDiagnosisAllowed", Label = "Boleh diagnosis primer", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isSecondaryDiagnosisAllowed", Label = "Boleh diagnosis sekunder", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isChronicDisease", Label = "Penyakit kronis", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isInfectiousDisease", Label = "Penyakit infeksi", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isExternalCause", Label = "External cause", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isPregnancyRelated", Label = "Terkait kehamilan", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isMentalHealthRelated", Label = "Terkait mental health", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isRareDisease", Label = "Rare disease", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "genderRestriction", Label = "Gender restriction", DataType = "string", InputType = "select", Required = false, IsReadonly = false, Placeholder = "None" },
                new() { Name = "minimumAgeYear", Label = "Minimum age", DataType = "integer", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "maximumAgeYear", Label = "Maximum age", DataType = "integer", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "externalDiagnosisCode", Label = "Kode eksternal", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "integrationCode", Label = "Kode integrasi", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "integer", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "description", Label = "Deskripsi", DataType = "string", InputType = "textarea", Required = false, IsReadonly = false }
            };
        }

        private static List<DiagnosisFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildCreateFieldMetadata();

            fields.Add(new DiagnosisFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status aktif",
                DataType = "boolean",
                InputType = "switch",
                Required = false,
                IsReadonly = false
            });

            return fields;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static string NormalizeDiagnosisType(string value)
        {
            var trimmed = string.IsNullOrWhiteSpace(value)
                ? "ICD10"
                : value.Trim();

            var matched = AllowedDiagnosisTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "ICD10";
        }

        private static string NormalizeIcdVersion(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "ICD-10"
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeGenderRestriction(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();

            if (string.Equals(trimmed, "None", StringComparison.OrdinalIgnoreCase))
                return "None";

            if (string.Equals(trimmed, "Male", StringComparison.OrdinalIgnoreCase))
                return "Male";

            if (string.Equals(trimmed, "Female", StringComparison.OrdinalIgnoreCase))
                return "Female";

            return trimmed;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
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
