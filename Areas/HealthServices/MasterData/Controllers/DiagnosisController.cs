using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
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

        private static readonly string[] IcdVersionOptions =
        {
            "ICD-10",
            "ICD-11"
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
                SortOptions = BuildSortOptions(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DiagnosisTypeOptions = AllowedDiagnosisTypes
                    .OrderBy(x => x)
                    .Select(x => new DiagnosisStringOptionResponse
                    {
                        Value = x,
                        Label = BuildDiagnosisTypeLabel(x)
                    })
                    .ToList(),
                GenderRestrictionOptions = AllowedGenderRestrictions
                    .OrderBy(x => x)
                    .Select(x => new DiagnosisStringOptionResponse
                    {
                        Value = x,
                        Label = BuildGenderRestrictionLabel(x)
                    })
                    .ToList(),
                IcdVersionOptions = IcdVersionOptions
                    .Select(x => new DiagnosisStringOptionResponse { Value = x, Label = x })
                    .ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset Filter"
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
                WithParentDiagnosis = await query.CountAsync(x => x.ParentDiagnosisId.HasValue),
                GroupOnlyDiagnosis = await query.CountAsync(x => !x.IsSelectableForClinicalUse)
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
            [FromQuery] string? diagnosisType,
            [FromQuery] string? icdVersion,
            [FromQuery] string? genderRestriction,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isSelectableForClinicalUse,
            [FromQuery] bool? isBillable,
            [FromQuery] bool? isPrimaryDiagnosisAllowed,
            [FromQuery] bool? isSecondaryDiagnosisAllowed,
            [FromQuery] bool? isChronicDisease,
            [FromQuery] bool? isInfectiousDisease,
            [FromQuery] bool? isExternalCause,
            [FromQuery] bool? isPregnancyRelated,
            [FromQuery] bool? isMentalHealthRelated,
            [FromQuery] bool? isRareDisease,
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
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                diagnosisChapterId,
                parentDiagnosisId,
                diagnosisType,
                icdVersion,
                genderRestriction,
                isActive,
                isSelectableForClinicalUse,
                isBillable,
                isPrimaryDiagnosisAllowed,
                isSecondaryDiagnosisAllowed,
                isChronicDisease,
                isInfectiousDisease,
                isExternalCause,
                isPregnancyRelated,
                isMentalHealthRelated,
                isRareDisease,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

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
            [FromQuery] string? diagnosisType = null,
            [FromQuery] string? icdVersion = null,
            [FromQuery] bool selectableOnly = false,
            [FromQuery] bool onlyActive = true,
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
                diagnosisChapterId,
                parentDiagnosisId,
                diagnosisType,
                icdVersion,
                null,
                onlyActive ? true : null,
                selectableOnly ? true : null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DiagnosisCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

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
        [AccessAction("Read", "Read Diagnosis", Description = "Melihat detail diagnosis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Diagnosis", "Read")]
        public async Task<IActionResult> GetDiagnosisById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

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
            var validation = await ValidateRequestAsync(null, request);

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

            var response = ToCreateResponse(entity);

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

            var validation = await ValidateRequestAsync(id, request);

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

            var response = ToUpdateResponse(entity);

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

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Diagnosis Status", Description = "Mengubah status diagnosis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Diagnosis", "Update")]
        public async Task<IActionResult> UpdateDiagnosisStatus(
            Guid id,
            [FromBody] UpdateDiagnosisStatusRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<DiagnosisUpdateResponse>.Ok(
                response,
                "Status diagnosis berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Diagnosis", Description = "Menghapus data diagnosis", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Diagnosis", "Delete")]
        public async Task<IActionResult> DeleteDiagnosis(
            Guid id,
            [FromBody] DeleteDiagnosisRequest? request = null)
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

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var response = new DiagnosisDeleteResponse
            {
                Id = entity.Id,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Diagnosis.DeleteDiagnosis",
                "Menghapus data diagnosis.",
                response
            );

            return Ok(ApiResponse<DiagnosisDeleteResponse>.Ok(
                response,
                "Diagnosis berhasil dihapus."
            ));
        }

        private IQueryable<MstDiagnosis> BuildBaseQuery()
        {
            return _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Include(x => x.DiagnosisChapter)
                .Include(x => x.ParentDiagnosis)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDiagnosis> ApplyDateFilter(
            IQueryable<MstDiagnosis> query,
            DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            return query;
        }

        private static IQueryable<MstDiagnosis> ApplyStandardFilter(
            IQueryable<MstDiagnosis> query,
            Guid? diagnosisChapterId,
            Guid? parentDiagnosisId,
            string? diagnosisType,
            string? icdVersion,
            string? genderRestriction,
            bool? isActive,
            bool? isSelectableForClinicalUse,
            bool? isBillable,
            bool? isPrimaryDiagnosisAllowed,
            bool? isSecondaryDiagnosisAllowed,
            bool? isChronicDisease,
            bool? isInfectiousDisease,
            bool? isExternalCause,
            bool? isPregnancyRelated,
            bool? isMentalHealthRelated,
            bool? isRareDisease,
            string? search)
        {
            var normalizedChapterId = NormalizeNullableGuid(diagnosisChapterId);
            if (normalizedChapterId.HasValue)
            {
                query = query.Where(x => x.DiagnosisChapterId == normalizedChapterId.Value);
            }

            var normalizedParentDiagnosisId = NormalizeNullableGuid(parentDiagnosisId);
            if (normalizedParentDiagnosisId.HasValue)
            {
                query = query.Where(x => x.ParentDiagnosisId == normalizedParentDiagnosisId.Value);
            }

            if (!string.IsNullOrWhiteSpace(diagnosisType))
            {
                var normalizedDiagnosisType = NormalizeDiagnosisType(diagnosisType);
                query = query.Where(x => x.DiagnosisType == normalizedDiagnosisType);
            }

            if (!string.IsNullOrWhiteSpace(icdVersion))
            {
                var normalizedIcdVersion = NormalizeIcdVersion(icdVersion);
                query = query.Where(x => x.IcdVersion == normalizedIcdVersion);
            }

            if (!string.IsNullOrWhiteSpace(genderRestriction))
            {
                var normalizedGenderRestriction = NormalizeGenderRestriction(genderRestriction);
                query = query.Where(x => x.GenderRestriction == normalizedGenderRestriction);
            }

            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (isSelectableForClinicalUse.HasValue) query = query.Where(x => x.IsSelectableForClinicalUse == isSelectableForClinicalUse.Value);
            if (isBillable.HasValue) query = query.Where(x => x.IsBillable == isBillable.Value);
            if (isPrimaryDiagnosisAllowed.HasValue) query = query.Where(x => x.IsPrimaryDiagnosisAllowed == isPrimaryDiagnosisAllowed.Value);
            if (isSecondaryDiagnosisAllowed.HasValue) query = query.Where(x => x.IsSecondaryDiagnosisAllowed == isSecondaryDiagnosisAllowed.Value);
            if (isChronicDisease.HasValue) query = query.Where(x => x.IsChronicDisease == isChronicDisease.Value);
            if (isInfectiousDisease.HasValue) query = query.Where(x => x.IsInfectiousDisease == isInfectiousDisease.Value);
            if (isExternalCause.HasValue) query = query.Where(x => x.IsExternalCause == isExternalCause.Value);
            if (isPregnancyRelated.HasValue) query = query.Where(x => x.IsPregnancyRelated == isPregnancyRelated.Value);
            if (isMentalHealthRelated.HasValue) query = query.Where(x => x.IsMentalHealthRelated == isMentalHealthRelated.Value);
            if (isRareDisease.HasValue) query = query.Where(x => x.IsRareDisease == isRareDisease.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
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

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateDiagnosisRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DiagnosisCode))
            {
                return (false, "Kode diagnosis wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.DiagnosisName))
            {
                return (false, "Nama diagnosis wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.DiagnosisType))
            {
                return (false, "Tipe diagnosis wajib diisi.");
            }

            if (!AllowedDiagnosisTypes.Contains(request.DiagnosisType.Trim()))
            {
                return (false, "Tipe diagnosis tidak valid. Gunakan salah satu: ICD10, Local, Custom.");
            }

            if (string.IsNullOrWhiteSpace(request.IcdVersion))
            {
                return (false, "Versi ICD wajib diisi.");
            }

            if (!string.IsNullOrWhiteSpace(request.GenderRestriction) &&
                !AllowedGenderRestrictions.Contains(request.GenderRestriction.Trim()))
            {
                return (false, "Gender restriction tidak valid. Gunakan salah satu: None, Male, Female.");
            }

            if (request.MinimumAgeYear.HasValue && request.MinimumAgeYear.Value < 0)
            {
                return (false, "Minimum age tidak boleh kurang dari 0.");
            }

            if (request.MaximumAgeYear.HasValue && request.MaximumAgeYear.Value < 0)
            {
                return (false, "Maximum age tidak boleh kurang dari 0.");
            }

            if (request.MinimumAgeYear.HasValue && request.MinimumAgeYear.Value > 150)
            {
                return (false, "Minimum age tidak boleh lebih dari 150 tahun.");
            }

            if (request.MaximumAgeYear.HasValue && request.MaximumAgeYear.Value > 150)
            {
                return (false, "Maximum age tidak boleh lebih dari 150 tahun.");
            }

            if (request.MinimumAgeYear.HasValue &&
                request.MaximumAgeYear.HasValue &&
                request.MinimumAgeYear.Value > request.MaximumAgeYear.Value)
            {
                return (false, "Minimum age tidak boleh lebih besar dari maximum age.");
            }

            if (!request.IsPrimaryDiagnosisAllowed && !request.IsSecondaryDiagnosisAllowed)
            {
                return (false, "Diagnosis minimal harus diizinkan sebagai primary atau secondary diagnosis.");
            }

            var normalizedChapterId = NormalizeNullableGuid(request.DiagnosisChapterId);

            if (normalizedChapterId.HasValue)
            {
                var chapterExists = await _dbContext.Set<MstDiagnosisChapter>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedChapterId.Value && x.IsActive && !x.IsDelete);

                if (!chapterExists)
                {
                    return (false, "Diagnosis chapter tidak valid atau tidak aktif.");
                }
            }

            var normalizedParentDiagnosisId = NormalizeNullableGuid(request.ParentDiagnosisId);

            if (normalizedParentDiagnosisId.HasValue)
            {
                if (excludeId.HasValue && normalizedParentDiagnosisId.Value == excludeId.Value)
                {
                    return (false, "Parent diagnosis tidak boleh sama dengan diagnosis yang sedang diubah.");
                }

                var parentExists = await _dbContext.Set<MstDiagnosis>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedParentDiagnosisId.Value && x.IsActive && !x.IsDelete);

                if (!parentExists)
                {
                    return (false, "Parent diagnosis tidak valid atau tidak aktif.");
                }

                if (excludeId.HasValue)
                {
                    var createsCycle = await DoesParentSelectionCreateCycleAsync(
                        currentDiagnosisId: excludeId.Value,
                        proposedParentDiagnosisId: normalizedParentDiagnosisId.Value
                    );

                    if (createsCycle)
                    {
                        return (false, "Parent diagnosis tidak boleh menggunakan child diagnosis dari diagnosis yang sedang diubah.");
                    }
                }
            }

            var normalizedCode = request.DiagnosisCode.Trim().ToUpperInvariant();
            var normalizedName = request.DiagnosisName.Trim().ToLower();
            var normalizedDiagnosisType = NormalizeDiagnosisType(request.DiagnosisType);
            var normalizedIcdVersion = NormalizeIcdVersion(request.IcdVersion);

            var duplicateCodeQuery = _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DiagnosisCode.ToUpper() == normalizedCode &&
                    x.IcdVersion.ToUpper() == normalizedIcdVersion.ToUpper());

            if (excludeId.HasValue)
            {
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateCodeQuery.AnyAsync())
            {
                return (false, "Kode diagnosis pada versi ICD tersebut sudah digunakan.");
            }

            var duplicateNameQuery = _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DiagnosisName.ToLower() == normalizedName &&
                    x.DiagnosisType.ToUpper() == normalizedDiagnosisType.ToUpper() &&
                    x.IcdVersion.ToUpper() == normalizedIcdVersion.ToUpper());

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama diagnosis pada tipe dan versi ICD tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<bool> DoesParentSelectionCreateCycleAsync(
            Guid currentDiagnosisId,
            Guid proposedParentDiagnosisId)
        {
            var visited = new HashSet<Guid>();
            var cursor = proposedParentDiagnosisId;

            while (cursor != Guid.Empty && visited.Add(cursor))
            {
                if (cursor == currentDiagnosisId)
                {
                    return true;
                }

                var parent = await _dbContext.Set<MstDiagnosis>()
                    .AsNoTracking()
                    .Where(x => x.Id == cursor && !x.IsDelete)
                    .Select(x => x.ParentDiagnosisId)
                    .FirstOrDefaultAsync();

                if (!parent.HasValue || parent.Value == Guid.Empty)
                {
                    return false;
                }

                cursor = parent.Value;
            }

            return false;
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
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static DiagnosisResponse MapResponse(
            MstDiagnosis entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
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
                DiagnosisNameEnglish = entity.DiagnosisNameEnglish,
                ShortName = entity.ShortName,
                DiagnosisGroupName = entity.DiagnosisGroupName,
                DiagnosisCategoryName = entity.DiagnosisCategoryName,
                DiagnosisType = entity.DiagnosisType,
                DiagnosisTypeName = BuildDiagnosisTypeLabel(entity.DiagnosisType),
                IcdVersion = entity.IcdVersion,
                IsSelectableForClinicalUse = entity.IsSelectableForClinicalUse,
                IsBillable = entity.IsBillable,
                IsPrimaryDiagnosisAllowed = entity.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = entity.IsSecondaryDiagnosisAllowed,
                IsChronicDisease = entity.IsChronicDisease,
                IsInfectiousDisease = entity.IsInfectiousDisease,
                IsExternalCause = entity.IsExternalCause,
                IsPregnancyRelated = entity.IsPregnancyRelated,
                IsMentalHealthRelated = entity.IsMentalHealthRelated,
                IsRareDisease = entity.IsRareDisease,
                GenderRestriction = entity.GenderRestriction,
                GenderRestrictionName = BuildGenderRestrictionLabel(entity.GenderRestriction),
                MinimumAgeYear = entity.MinimumAgeYear,
                MaximumAgeYear = entity.MaximumAgeYear,
                ExternalDiagnosisCode = entity.ExternalDiagnosisCode,
                IntegrationCode = entity.IntegrationCode,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DiagnosisDetailResponse MapDetailResponse(
            MstDiagnosis entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DiagnosisDetailResponse
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
                DiagnosisNameEnglish = entity.DiagnosisNameEnglish,
                ShortName = entity.ShortName,
                DiagnosisGroupName = entity.DiagnosisGroupName,
                DiagnosisCategoryName = entity.DiagnosisCategoryName,
                DiagnosisType = entity.DiagnosisType,
                DiagnosisTypeName = BuildDiagnosisTypeLabel(entity.DiagnosisType),
                IcdVersion = entity.IcdVersion,
                IsSelectableForClinicalUse = entity.IsSelectableForClinicalUse,
                IsBillable = entity.IsBillable,
                IsPrimaryDiagnosisAllowed = entity.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = entity.IsSecondaryDiagnosisAllowed,
                IsChronicDisease = entity.IsChronicDisease,
                IsInfectiousDisease = entity.IsInfectiousDisease,
                IsExternalCause = entity.IsExternalCause,
                IsPregnancyRelated = entity.IsPregnancyRelated,
                IsMentalHealthRelated = entity.IsMentalHealthRelated,
                IsRareDisease = entity.IsRareDisease,
                GenderRestriction = entity.GenderRestriction,
                GenderRestrictionName = BuildGenderRestrictionLabel(entity.GenderRestriction),
                MinimumAgeYear = entity.MinimumAgeYear,
                MaximumAgeYear = entity.MaximumAgeYear,
                ExternalDiagnosisCode = entity.ExternalDiagnosisCode,
                IntegrationCode = entity.IntegrationCode,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                Description = entity.Description,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static DiagnosisOptionResponse MapOptionResponse(MstDiagnosis entity)
        {
            return new DiagnosisOptionResponse
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
                ShortName = entity.ShortName,
                DiagnosisGroupName = entity.DiagnosisGroupName,
                DiagnosisCategoryName = entity.DiagnosisCategoryName,
                DiagnosisType = entity.DiagnosisType,
                DiagnosisTypeName = BuildDiagnosisTypeLabel(entity.DiagnosisType),
                IcdVersion = entity.IcdVersion,
                IsSelectableForClinicalUse = entity.IsSelectableForClinicalUse,
                IsBillable = entity.IsBillable,
                IsPrimaryDiagnosisAllowed = entity.IsPrimaryDiagnosisAllowed,
                IsSecondaryDiagnosisAllowed = entity.IsSecondaryDiagnosisAllowed,
                GenderRestriction = entity.GenderRestriction,
                GenderRestrictionName = BuildGenderRestrictionLabel(entity.GenderRestriction),
                MinimumAgeYear = entity.MinimumAgeYear,
                MaximumAgeYear = entity.MaximumAgeYear,
                IsActive = entity.IsActive
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

        private static DiagnosisCreateResponse ToCreateResponse(MstDiagnosis entity)
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
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            };
        }

        private static DiagnosisUpdateResponse ToUpdateResponse(MstDiagnosis entity)
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
                IsBillable = entity.IsBillable,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };
        }

        private static IOrderedQueryable<MstDiagnosis> ApplySorting(
            IQueryable<MstDiagnosis> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
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

        private static List<DiagnosisSortOptionResponse> BuildSortOptions()
        {
            return new List<DiagnosisSortOptionResponse>
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
            };
        }

        private static List<DiagnosisQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<DiagnosisQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Jika bukan custom, startDate dan endDate diabaikan.", Example = "thisMonth" },
                new() { Name = "diagnosisChapterId", Type = "guid", Description = "Filter berdasarkan diagnosis chapter." },
                new() { Name = "parentDiagnosisId", Type = "guid", Description = "Filter berdasarkan parent diagnosis." },
                new() { Name = "diagnosisType", Type = "string", Description = "Filter tipe diagnosis.", Example = "ICD10" },
                new() { Name = "icdVersion", Type = "string", Description = "Filter versi ICD.", Example = "ICD-10" },
                new() { Name = "genderRestriction", Type = "string", Description = "Filter gender restriction.", Example = "Male" },
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
                new() { Name = "diagnosisChapterId", Label = "Diagnosis chapter", DataType = "guid", InputType = "select", Required = false, IsReadonly = false, Description = "Relasi ke master diagnosis chapter.", SortOrder = 1 },
                new() { Name = "parentDiagnosisId", Label = "Parent diagnosis", DataType = "guid", InputType = "select", Required = false, IsReadonly = false, Description = "Opsional untuk struktur parent-child diagnosis.", SortOrder = 2 },
                new() { Name = "diagnosisCode", Label = "Kode diagnosis", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "A00.0", Description = "Kode resmi diagnosis seperti ICD-10, contoh A00.0, I10, E11.9.", SortOrder = 3 },
                new() { Name = "diagnosisName", Label = "Nama diagnosis", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "Cholera due to Vibrio cholerae 01, biovar cholerae", SortOrder = 4 },
                new() { Name = "diagnosisNameEnglish", Label = "Nama diagnosis Inggris", DataType = "string", InputType = "text", Required = false, IsReadonly = false, SortOrder = 5 },
                new() { Name = "shortName", Label = "Nama singkat", DataType = "string", InputType = "text", Required = false, IsReadonly = false, SortOrder = 6 },
                new() { Name = "diagnosisGroupName", Label = "Group diagnosis", DataType = "string", InputType = "text", Required = false, IsReadonly = false, SortOrder = 7 },
                new() { Name = "diagnosisCategoryName", Label = "Kategori diagnosis", DataType = "string", InputType = "text", Required = false, IsReadonly = false, SortOrder = 8 },
                new() { Name = "diagnosisType", Label = "Tipe diagnosis", DataType = "string", InputType = "select", Required = true, IsReadonly = false, Placeholder = "ICD10", Description = "ICD10, Local, atau Custom.", SortOrder = 9 },
                new() { Name = "icdVersion", Label = "Versi ICD", DataType = "string", InputType = "select", Required = true, IsReadonly = false, Placeholder = "ICD-10", SortOrder = 10 },
                new() { Name = "isSelectableForClinicalUse", Label = "Bisa dipilih klinis", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 11 },
                new() { Name = "isBillable", Label = "Billable", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 12 },
                new() { Name = "isPrimaryDiagnosisAllowed", Label = "Boleh diagnosis primer", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 13 },
                new() { Name = "isSecondaryDiagnosisAllowed", Label = "Boleh diagnosis sekunder", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 14 },
                new() { Name = "isChronicDisease", Label = "Penyakit kronis", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 15 },
                new() { Name = "isInfectiousDisease", Label = "Penyakit infeksi", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 16 },
                new() { Name = "isExternalCause", Label = "External cause", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 17 },
                new() { Name = "isPregnancyRelated", Label = "Terkait kehamilan", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 18 },
                new() { Name = "isMentalHealthRelated", Label = "Terkait mental health", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 19 },
                new() { Name = "isRareDisease", Label = "Rare disease", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false, SortOrder = 20 },
                new() { Name = "genderRestriction", Label = "Gender restriction", DataType = "string", InputType = "select", Required = false, IsReadonly = false, Placeholder = "None", SortOrder = 21 },
                new() { Name = "minimumAgeYear", Label = "Minimum age", DataType = "integer", InputType = "number", Required = false, IsReadonly = false, SortOrder = 22 },
                new() { Name = "maximumAgeYear", Label = "Maximum age", DataType = "integer", InputType = "number", Required = false, IsReadonly = false, SortOrder = 23 },
                new() { Name = "externalDiagnosisCode", Label = "Kode eksternal", DataType = "string", InputType = "text", Required = false, IsReadonly = false, SortOrder = 24 },
                new() { Name = "integrationCode", Label = "Kode integrasi", DataType = "string", InputType = "text", Required = false, IsReadonly = false, SortOrder = 25 },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "integer", InputType = "number", Required = false, IsReadonly = false, SortOrder = 26 },
                new() { Name = "description", Label = "Deskripsi", DataType = "string", InputType = "textarea", Required = false, IsReadonly = false, SortOrder = 27 }
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
                IsReadonly = false,
                SortOrder = 99
            });

            return fields.OrderBy(x => x.SortOrder).ToList();
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

        private static string BuildDiagnosisTypeLabel(string? value)
        {
            return value switch
            {
                "ICD10" => "ICD-10",
                "Local" => "Local",
                "Custom" => "Custom",
                _ => value ?? string.Empty
            };
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
            {
                return null;
            }

            var trimmed = value.Trim();

            if (string.Equals(trimmed, "None", StringComparison.OrdinalIgnoreCase))
            {
                return "None";
            }

            if (string.Equals(trimmed, "Male", StringComparison.OrdinalIgnoreCase))
            {
                return "Male";
            }

            if (string.Equals(trimmed, "Female", StringComparison.OrdinalIgnoreCase))
            {
                return "Female";
            }

            return trimmed;
        }

        private static string? BuildGenderRestrictionLabel(string? value)
        {
            return value switch
            {
                "None" => "None",
                "Male" => "Male",
                "Female" => "Female",
                null => null,
                _ => value
            };
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

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
