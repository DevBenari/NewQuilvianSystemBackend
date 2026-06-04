using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseCompetencyPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.CompetencyResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/competencies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Competency",
        AreaName = "Corporate",
        ControllerName = "Competency",
        Description = "Corporate human resource master data competency and position competency requirement",
        SortOrder = 12
    )]
    [Tags("Corporate / Human Resource / Master Data / Competency")]
    public class CompetencyController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "CMP-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public CompetencyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<CompetencyFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Competency", Description = "Melihat metadata filter competency", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new CompetencyFilterMetadataResponse
            {
                DefaultFilter = new CompetencyDefaultFilterResponse(),
                CustomPeriods = new List<CompetencyCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<CompetencySortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "competencyCode", Label = "Kode competency" },
                    new() { Value = "competencyName", Label = "Nama competency" },
                    new() { Value = "competencyCategory", Label = "Kategori competency" },
                    new() { Value = "positionRequirementCount", Label = "Jumlah requirement posisi" },
                    new() { Value = "assessmentCount", Label = "Jumlah assessment" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                CompetencyCategoryOptions = BuildEnumOptions<CompetencyCategory>(),
                CompetencyLevelOptions = BuildEnumOptions<CompetencyLevel>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Competency.GetFilterMetadata",
                "Mengambil metadata filter competency.",
                result
            );

            return Ok(ApiResponse<CompetencyFilterMetadataResponse>.Ok(
                result,
                "Metadata filter competency berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CompetencySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Competency", Description = "Melihat ringkasan competency", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var competencyQuery = _dbContext.Set<MstCompetency>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var requirementQuery = _dbContext.Set<MstPositionCompetencyRequirement>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new CompetencySummaryResponse
            {
                TotalCompetency = await competencyQuery.CountAsync(),
                ActiveCompetency = await competencyQuery.CountAsync(x => x.IsActive),
                InactiveCompetency = await competencyQuery.CountAsync(x => !x.IsActive),
                CompetencyWithPositionRequirement = await competencyQuery.CountAsync(x => x.PositionRequirements.Any(r => !r.IsDelete)),
                CompetencyWithoutPositionRequirement = await competencyQuery.CountAsync(x => !x.PositionRequirements.Any(r => !r.IsDelete)),
                CompetencyWithAssessment = await competencyQuery.CountAsync(x => x.CompetencyAssessments.Any(a => !a.IsDelete)),
                RequiredPositionRequirement = await requirementQuery.CountAsync(x => x.IsRequired),
                CertificationRequiredRequirement = await requirementQuery.CountAsync(x => x.IsCertificationRequired),
                TrainingRequiredRequirement = await requirementQuery.CountAsync(x => x.IsTrainingRequired)
            };

            return Ok(ApiResponse<CompetencySummaryResponse>.Ok(
                result,
                "Ringkasan competency berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseCompetencyPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Competency", Description = "Melihat data competency", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetCompetencies(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? positionId,
            [FromQuery] Guid? departmentId,
            [FromQuery] CompetencyCategory? competencyCategory,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "competencyName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query,
                positionId,
                departmentId,
                competencyCategory,
                isActive,
                search
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CompetencyResponse
                {
                    Id = x.Id,
                    CompetencyCode = x.CompetencyCode,
                    CompetencyName = x.CompetencyName,
                    CompetencyCategory = x.CompetencyCategory,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    PositionRequirementCount = x.PositionRequirements.Count(r => !r.IsDelete),
                    ActivePositionRequirementCount = x.PositionRequirements.Count(r => !r.IsDelete && r.IsActive),
                    AssessmentCount = x.CompetencyAssessments.Count(a => !a.IsDelete),
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseCompetencyPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseCompetencyPagedResult>.Ok(
                result,
                "Data competency berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<CompetencyOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Competency", Description = "Melihat pilihan competency", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetCompetencyOptions(
            [FromQuery] Guid? positionId,
            [FromQuery] Guid? departmentId,
            [FromQuery] CompetencyCategory? competencyCategory,
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
                positionId,
                departmentId,
                competencyCategory,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.CompetencyName)
                .ThenBy(x => x.CompetencyCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CompetencyOptionResponse
                {
                    Id = x.Id,
                    CompetencyCode = x.CompetencyCode,
                    CompetencyName = x.CompetencyName,
                    CompetencyCategory = x.CompetencyCategory
                })
                .ToListAsync();

            var result = new CompetencyOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<CompetencyOptionPagedResponse>.Ok(
                result,
                "Data pilihan competency berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompetencyDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Competency", Description = "Melihat detail competency", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetCompetencyById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new CompetencyDetailResponse
                {
                    Id = x.Id,
                    CompetencyCode = x.CompetencyCode,
                    CompetencyName = x.CompetencyName,
                    CompetencyCategory = x.CompetencyCategory,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    PositionRequirementCount = x.PositionRequirements.Count(r => !r.IsDelete),
                    ActivePositionRequirementCount = x.PositionRequirements.Count(r => !r.IsDelete && r.IsActive),
                    AssessmentCount = x.CompetencyAssessments.Count(a => !a.IsDelete),
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<CompetencyDetailResponse>.Ok(
                data,
                "Detail competency berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CompetencyCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Competency", Description = "Membuat data competency", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Competency", "Create")]
        public async Task<IActionResult> CreateCompetency([FromBody] CreateCompetencyRequest request)
        {
            var validation = await ValidateCompetencyRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data competency tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstCompetency
            {
                Id = Guid.NewGuid(),
                CompetencyCode = await GenerateCompetencyCodeAsync(),
                CompetencyName = request.CompetencyName.Trim(),
                CompetencyCategory = request.CompetencyCategory,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstCompetency>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new CompetencyCreateResponse
            {
                Id = entity.Id,
                CompetencyCode = entity.CompetencyCode,
                CompetencyName = entity.CompetencyName,
                CompetencyCategory = entity.CompetencyCategory,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Competency.CreateCompetency",
                "Membuat data competency.",
                result
            );

            return Ok(ApiResponse<CompetencyCreateResponse>.Ok(
                result,
                "Competency berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Competency", Description = "Mengubah data competency", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Competency", "Update")]
        public async Task<IActionResult> UpdateCompetency(
            Guid id,
            [FromBody] UpdateCompetencyRequest request)
        {
            var entity = await _dbContext.Set<MstCompetency>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency tidak ditemukan."
                ));
            }

            var validation = await ValidateCompetencyRequestAsync(
                excludeId: id,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data competency tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.CompetencyName = request.CompetencyName.Trim();
            entity.CompetencyCategory = request.CompetencyCategory;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Competency.UpdateCompetency",
                "Mengubah data competency.",
                new { entity.Id, entity.CompetencyCode, entity.CompetencyName, entity.IsActive }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Competency berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Competency Status", Description = "Mengubah status competency", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Competency", "Update")]
        public async Task<IActionResult> UpdateCompetencyStatus(
            Guid id,
            [FromBody] UpdateCompetencyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstCompetency>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status competency berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Competency", Description = "Menghapus competency", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Competency", "Delete")]
        public async Task<IActionResult> DeleteCompetency(Guid id)
        {
            var entity = await _dbContext.Set<MstCompetency>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency tidak ditemukan."
                ));
            }

            var usedByPositionRequirement = await _dbContext.Set<MstPositionCompetencyRequirement>()
                .AsNoTracking()
                .AnyAsync(x => x.CompetencyId == id && !x.IsDelete);

            var usedByAssessment = await _dbContext.WfpCompetencyAssessments
                .AsNoTracking()
                .AnyAsync(x => x.CompetencyId == id && !x.IsDelete);

            if (usedByPositionRequirement || usedByAssessment)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Competency tidak dapat dihapus karena sudah digunakan oleh position requirement atau competency assessment."
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
                "Competency.DeleteCompetency",
                "Menghapus data competency.",
                new { entity.Id, entity.CompetencyCode, entity.CompetencyName, entity.DeleteDateTime }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Competency berhasil dihapus."
            ));
        }

        [HttpGet("positions/{positionId:guid}/requirements")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Position Competency Requirement", Description = "Melihat competency requirement per position", AccessType = AccessTypes.Read, SortOrder = 6)]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetPositionRequirements(
            Guid positionId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? competencyId,
            [FromQuery] CompetencyCategory? competencyCategory,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "competencyName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var position = await _dbContext.MstPositions
                .AsNoTracking()
                .Where(x => x.Id == positionId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.PositionCode,
                    x.PositionName,
                    x.DepartmentId,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null
                })
                .FirstOrDefaultAsync();

            if (position == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position tidak ditemukan."
                ));
            }

            var query = _dbContext.Set<MstPositionCompetencyRequirement>()
                .AsNoTracking()
                .Where(x => x.PositionId == positionId && !x.IsDelete);

            query = ApplyRequirementDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyRequirementStandardFilter(query, competencyId, competencyCategory, isActive, search);

            var totalData = await query.CountAsync();
            var activeData = await query.CountAsync(x => x.IsActive);
            var requiredData = await query.CountAsync(x => x.IsRequired);
            var certificationRequiredData = await query.CountAsync(x => x.IsCertificationRequired);
            var trainingRequiredData = await query.CountAsync(x => x.IsTrainingRequired);

            var items = await ApplyRequirementSorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PositionCompetencyRequirementResponse
                {
                    Id = x.Id,
                    PositionId = x.PositionId,
                    PositionCode = x.Position != null ? x.Position.PositionCode : string.Empty,
                    PositionName = x.Position != null ? x.Position.PositionName : string.Empty,
                    DepartmentId = x.Position != null ? x.Position.DepartmentId : null,
                    DepartmentCode = x.Position != null && x.Position.Department != null ? x.Position.Department.DepartmentCode : null,
                    DepartmentName = x.Position != null && x.Position.Department != null ? x.Position.Department.DepartmentName : null,
                    CompetencyId = x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : string.Empty,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : string.Empty,
                    CompetencyCategory = x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other,
                    IsRequired = x.IsRequired,
                    MinimumLevel = x.MinimumLevel,
                    IsCertificationRequired = x.IsCertificationRequired,
                    IsTrainingRequired = x.IsTrainingRequired,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new PositionCompetencyRequirementListResponse
            {
                PositionId = position.Id,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName,
                DepartmentId = position.DepartmentId,
                DepartmentName = position.DepartmentName,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                ActiveData = activeData,
                RequiredData = requiredData,
                CertificationRequiredData = certificationRequiredData,
                TrainingRequiredData = trainingRequiredData,
                Items = items
            };

            return Ok(ApiResponse<PositionCompetencyRequirementListResponse>.Ok(
                result,
                "Data competency requirement position berhasil diambil."
            ));
        }

        [HttpPost("positions/{positionId:guid}/requirements")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Position Competency Requirement", Description = "Membuat competency requirement position", AccessType = AccessTypes.Create, SortOrder = 7)]
        [AccessPermission("Competency", "Create")]
        public async Task<IActionResult> CreatePositionRequirement(
            Guid positionId,
            [FromBody] CreatePositionCompetencyRequirementRequest request)
        {
            var validation = await ValidatePositionRequirementRequestAsync(
                positionId,
                request.CompetencyId,
                null
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstPositionCompetencyRequirement
            {
                Id = Guid.NewGuid(),
                PositionId = positionId,
                CompetencyId = request.CompetencyId,
                IsRequired = request.IsRequired,
                MinimumLevel = request.MinimumLevel,
                IsCertificationRequired = request.IsCertificationRequired,
                IsTrainingRequired = request.IsTrainingRequired,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPositionCompetencyRequirement>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Competency.CreatePositionRequirement",
                "Membuat competency requirement position.",
                new { entity.Id, entity.PositionId, entity.CompetencyId }
            );

            return await GetPositionRequirements(
                positionId,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "competencyName",
                "asc",
                1,
                25
            );
        }

        [HttpPut("positions/{positionId:guid}/requirements/{requirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Position Competency Requirement", Description = "Mengubah competency requirement position", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("Competency", "Update")]
        public async Task<IActionResult> UpdatePositionRequirement(
            Guid positionId,
            Guid requirementId,
            [FromBody] UpdatePositionCompetencyRequirementRequest request)
        {
            var entity = await _dbContext.Set<MstPositionCompetencyRequirement>()
                .FirstOrDefaultAsync(x =>
                    x.Id == requirementId &&
                    x.PositionId == positionId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position competency requirement tidak ditemukan."
                ));
            }

            var validation = await ValidatePositionRequirementRequestAsync(
                positionId,
                request.CompetencyId,
                requirementId
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            entity.CompetencyId = request.CompetencyId;
            entity.IsRequired = request.IsRequired;
            entity.MinimumLevel = request.MinimumLevel;
            entity.IsCertificationRequired = request.IsCertificationRequired;
            entity.IsTrainingRequired = request.IsTrainingRequired;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Competency.UpdatePositionRequirement",
                "Mengubah competency requirement position.",
                new { entity.Id, entity.PositionId, entity.CompetencyId, entity.IsActive }
            );

            return await GetPositionRequirements(
                positionId,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "competencyName",
                "asc",
                1,
                25
            );
        }

        [HttpPatch("positions/{positionId:guid}/requirements/{requirementId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Position Competency Requirement Status", Description = "Mengubah status competency requirement position", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("Competency", "Update")]
        public async Task<IActionResult> UpdatePositionRequirementStatus(
            Guid positionId,
            Guid requirementId,
            [FromBody] UpdatePositionCompetencyRequirementStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPositionCompetencyRequirement>()
                .FirstOrDefaultAsync(x =>
                    x.Id == requirementId &&
                    x.PositionId == positionId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position competency requirement tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetPositionRequirements(
                positionId,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "competencyName",
                "asc",
                1,
                25
            );
        }

        [HttpDelete("positions/{positionId:guid}/requirements/{requirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Position Competency Requirement", Description = "Menghapus competency requirement position", AccessType = AccessTypes.Delete, SortOrder = 10)]
        [AccessPermission("Competency", "Delete")]
        public async Task<IActionResult> DeletePositionRequirement(
            Guid positionId,
            Guid requirementId)
        {
            var entity = await _dbContext.Set<MstPositionCompetencyRequirement>()
                .FirstOrDefaultAsync(x =>
                    x.Id == requirementId &&
                    x.PositionId == positionId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position competency requirement tidak ditemukan."
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
                "Competency.DeletePositionRequirement",
                "Menghapus competency requirement position.",
                new { entity.Id, entity.PositionId, entity.CompetencyId, entity.DeleteDateTime }
            );

            return await GetPositionRequirements(
                positionId,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "competencyName",
                "asc",
                1,
                25
            );
        }

        private IQueryable<MstCompetency> BuildBaseQuery()
        {
            return _dbContext.Set<MstCompetency>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstCompetency> ApplyDateFilter(
            IQueryable<MstCompetency> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var now = DateTime.UtcNow;
                var today = now.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x => x.CreateDateTime >= today && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x => x.CreateDateTime >= today.AddDays(-6) && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x => x.CreateDateTime >= thisMonthStart && x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x => x.CreateDateTime >= lastMonthStart && x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstCompetency> ApplyStandardFilter(
            IQueryable<MstCompetency> query,
            Guid? positionId,
            Guid? departmentId,
            CompetencyCategory? competencyCategory,
            bool? isActive,
            string? search)
        {
            if (positionId.HasValue && positionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PositionRequirements.Any(r =>
                    !r.IsDelete &&
                    r.PositionId == positionId.Value));
            }

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PositionRequirements.Any(r =>
                    !r.IsDelete &&
                    r.Position != null &&
                    r.Position.DepartmentId == departmentId.Value));
            }

            if (competencyCategory.HasValue)
                query = query.Where(x => x.CompetencyCategory == competencyCategory.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CompetencyCode.ToLower().Contains(keyword) ||
                    x.CompetencyName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    x.PositionRequirements.Any(r =>
                        !r.IsDelete &&
                        r.Position != null &&
                        (
                            r.Position.PositionCode.ToLower().Contains(keyword) ||
                            r.Position.PositionName.ToLower().Contains(keyword) ||
                            (r.Position.Department != null && r.Position.Department.DepartmentName.ToLower().Contains(keyword))
                        )));
            }

            return query;
        }

        private static IOrderedQueryable<MstCompetency> ApplySorting(
            IQueryable<MstCompetency> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "competencyName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "competencycode" => isDescending ? query.OrderByDescending(x => x.CompetencyCode) : query.OrderBy(x => x.CompetencyCode),
                "competencyname" => isDescending ? query.OrderByDescending(x => x.CompetencyName) : query.OrderBy(x => x.CompetencyName),
                "competencycategory" => isDescending ? query.OrderByDescending(x => x.CompetencyCategory) : query.OrderBy(x => x.CompetencyCategory),
                "positionrequirementcount" => isDescending ? query.OrderByDescending(x => x.PositionRequirements.Count(r => !r.IsDelete)) : query.OrderBy(x => x.PositionRequirements.Count(r => !r.IsDelete)),
                "assessmentcount" => isDescending ? query.OrderByDescending(x => x.CompetencyAssessments.Count(a => !a.IsDelete)) : query.OrderBy(x => x.CompetencyAssessments.Count(a => !a.IsDelete)),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDescending
                    ? query.OrderByDescending(x => x.CompetencyName).ThenByDescending(x => x.CompetencyCode)
                    : query.OrderBy(x => x.CompetencyName).ThenBy(x => x.CompetencyCode)
            };
        }

        private static IQueryable<MstPositionCompetencyRequirement> ApplyRequirementDateFilter(
            IQueryable<MstPositionCompetencyRequirement> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var now = DateTime.UtcNow;
                var today = now.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x => x.CreateDateTime >= today && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x => x.CreateDateTime >= today.AddDays(-6) && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x => x.CreateDateTime >= thisMonthStart && x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x => x.CreateDateTime >= lastMonthStart && x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstPositionCompetencyRequirement> ApplyRequirementStandardFilter(
            IQueryable<MstPositionCompetencyRequirement> query,
            Guid? competencyId,
            CompetencyCategory? competencyCategory,
            bool? isActive,
            string? search)
        {
            if (competencyId.HasValue && competencyId.Value != Guid.Empty)
                query = query.Where(x => x.CompetencyId == competencyId.Value);

            if (competencyCategory.HasValue)
                query = query.Where(x => x.Competency != null && x.Competency.CompetencyCategory == competencyCategory.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.Competency != null && x.Competency.CompetencyCode.ToLower().Contains(keyword)) ||
                    (x.Competency != null && x.Competency.CompetencyName.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.PositionCode.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.PositionName.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.Department != null && x.Position.Department.DepartmentName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstPositionCompetencyRequirement> ApplyRequirementSorting(
            IQueryable<MstPositionCompetencyRequirement> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "competencyName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "positioncode" => isDescending
                    ? query.OrderByDescending(x => x.Position != null ? x.Position.PositionCode : string.Empty)
                    : query.OrderBy(x => x.Position != null ? x.Position.PositionCode : string.Empty),
                "positionname" => isDescending
                    ? query.OrderByDescending(x => x.Position != null ? x.Position.PositionName : string.Empty)
                    : query.OrderBy(x => x.Position != null ? x.Position.PositionName : string.Empty),
                "competencycode" => isDescending
                    ? query.OrderByDescending(x => x.Competency != null ? x.Competency.CompetencyCode : string.Empty)
                    : query.OrderBy(x => x.Competency != null ? x.Competency.CompetencyCode : string.Empty),
                "competencyname" => isDescending
                    ? query.OrderByDescending(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty)
                    : query.OrderBy(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty),
                "competencycategory" => isDescending
                    ? query.OrderByDescending(x => x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other)
                    : query.OrderBy(x => x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other),
                "minimumlevel" => isDescending ? query.OrderByDescending(x => x.MinimumLevel) : query.OrderBy(x => x.MinimumLevel),
                "isrequired" => isDescending ? query.OrderByDescending(x => x.IsRequired) : query.OrderBy(x => x.IsRequired),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDescending
                    ? query.OrderByDescending(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty)
                    : query.OrderBy(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCompetencyRequestAsync(
            Guid? excludeId,
            CreateCompetencyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CompetencyName))
                return (false, "Nama competency wajib diisi.");

            var normalizedName = request.CompetencyName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstCompetency>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.CompetencyName.ToLower() == normalizedName &&
                    x.CompetencyCategory == request.CompetencyCategory);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Competency dengan nama dan kategori tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<(bool IsValid, string Message)> ValidatePositionRequirementRequestAsync(
            Guid positionId,
            Guid competencyId,
            Guid? excludeRequirementId)
        {
            if (positionId == Guid.Empty)
                return (false, "Position wajib dipilih.");

            if (competencyId == Guid.Empty)
                return (false, "Competency wajib dipilih.");

            var positionExists = await _dbContext.MstPositions
                .AsNoTracking()
                .AnyAsync(x => x.Id == positionId && !x.IsDelete && x.IsActive);

            if (!positionExists)
                return (false, "Position tidak ditemukan atau tidak aktif.");

            var competencyExists = await _dbContext.Set<MstCompetency>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == competencyId && !x.IsDelete && x.IsActive);

            if (!competencyExists)
                return (false, "Competency tidak ditemukan atau tidak aktif.");

            var duplicate = await _dbContext.Set<MstPositionCompetencyRequirement>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.PositionId == positionId &&
                    x.CompetencyId == competencyId &&
                    !x.IsDelete &&
                    (!excludeRequirementId.HasValue || x.Id != excludeRequirementId.Value));

            if (duplicate)
                return (false, "Competency sudah menjadi requirement untuk position ini.");

            return (true, string.Empty);
        }

        private async Task<string> GenerateCompetencyCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstCompetency>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.CompetencyCode.StartsWith(CodePrefix))
                .Select(x => x.CompetencyCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
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

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static List<CompetencyEnumOptionResponse> BuildEnumOptions<TEnum>()
            where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(x => new CompetencyEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = x.ToString()
                })
                .ToList();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
