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

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public CompetencyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<CompetencyListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Competency",
            Description = "Melihat data competency",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetCompetencies(
            [FromQuery] string? search,
            [FromQuery] CompetencyCategory? competencyCategory,
            [FromQuery] bool? isActive)
        {
            var query = _dbContext.MstCompetencies
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CompetencyCode.ToLower().Contains(keyword) ||
                    x.CompetencyName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (competencyCategory.HasValue)
            {
                query = query.Where(x => x.CompetencyCategory == competencyCategory.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var items = await query
                .OrderBy(x => x.CompetencyCategory)
                .ThenBy(x => x.CompetencyName)
                .Select(x => new CompetencyResponse
                {
                    Id = x.Id,
                    CompetencyCode = x.CompetencyCode,
                    CompetencyName = x.CompetencyName,
                    CompetencyCategory = x.CompetencyCategory,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    PositionRequirementCount = x.PositionRequirements.Count(r => !r.IsDelete),
                    AssessmentCount = x.CompetencyAssessments.Count(a => !a.IsDelete),
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new CompetencyListResponse
            {
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                InactiveData = items.Count(x => !x.IsActive),
                Items = items
            };

            return Ok(ApiResponse<CompetencyListResponse>.Ok(
                result,
                "Data competency berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<CompetencyOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Competency",
            Description = "Melihat pilihan competency",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetCompetencyOptions(
            [FromQuery] CompetencyCategory? competencyCategory,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.MstCompetencies
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (competencyCategory.HasValue)
            {
                query = query.Where(x => x.CompetencyCategory == competencyCategory.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CompetencyCode.ToLower().Contains(keyword) ||
                    x.CompetencyName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderBy(x => x.CompetencyName)
                .Select(x => new CompetencyOptionResponse
                {
                    Id = x.Id,
                    CompetencyCode = x.CompetencyCode,
                    CompetencyName = x.CompetencyName,
                    CompetencyCategory = x.CompetencyCategory
                })
                .ToListAsync();

            return Ok(ApiResponse<List<CompetencyOptionResponse>>.Ok(
                data,
                "Data pilihan competency berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompetencyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Competency",
            Description = "Melihat detail competency",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetCompetencyById(Guid id)
        {
            var data = await _dbContext.MstCompetencies
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new CompetencyResponse
                {
                    Id = x.Id,
                    CompetencyCode = x.CompetencyCode,
                    CompetencyName = x.CompetencyName,
                    CompetencyCategory = x.CompetencyCategory,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    PositionRequirementCount = x.PositionRequirements.Count(r => !r.IsDelete),
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

            return Ok(ApiResponse<CompetencyResponse>.Ok(
                data,
                "Detail competency berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CompetencyResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Competency",
            Description = "Membuat data competency",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Competency", "Create")]
        public async Task<IActionResult> CreateCompetency([FromBody] CreateCompetencyRequest request)
        {
            var code = NormalizeRequiredCode(request.CompetencyCode);

            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CompetencyCode wajib diisi."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.CompetencyName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CompetencyName wajib diisi."
                ));
            }

            var duplicate = await _dbContext.MstCompetencies
                .AnyAsync(x =>
                    x.CompetencyCode.ToLower() == code.ToLower() &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CompetencyCode sudah digunakan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstCompetency
            {
                Id = Guid.NewGuid(),
                CompetencyCode = code,
                CompetencyName = request.CompetencyName.Trim(),
                CompetencyCategory = request.CompetencyCategory,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstCompetencies.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Competency.CreateCompetency",
                "Competency berhasil dibuat.",
                new { entity.Id, entity.CompetencyCode }
            );

            return await GetCompetencyById(entity.Id);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompetencyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Competency",
            Description = "Mengubah data competency",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Competency", "Update")]
        public async Task<IActionResult> UpdateCompetency(
            Guid id,
            [FromBody] UpdateCompetencyRequest request)
        {
            var entity = await _dbContext.MstCompetencies
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency tidak ditemukan."
                ));
            }

            var code = NormalizeRequiredCode(request.CompetencyCode);

            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CompetencyCode wajib diisi."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.CompetencyName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CompetencyName wajib diisi."
                ));
            }

            var duplicate = await _dbContext.MstCompetencies
                .AnyAsync(x =>
                    x.Id != id &&
                    x.CompetencyCode.ToLower() == code.ToLower() &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CompetencyCode sudah digunakan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.CompetencyCode = code;
            entity.CompetencyName = request.CompetencyName.Trim();
            entity.CompetencyCategory = request.CompetencyCategory;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetCompetencyById(entity.Id);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<CompetencyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Competency Status",
            Description = "Mengubah status competency",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("Competency", "Update")]
        public async Task<IActionResult> UpdateCompetencyStatus(
            Guid id,
            [FromBody] UpdateCompetencyStatusRequest request)
        {
            var entity = await _dbContext.MstCompetencies
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

            return await GetCompetencyById(entity.Id);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Competency",
            Description = "Menghapus competency",
            AccessType = AccessTypes.Delete,
            SortOrder = 5
        )]
        [AccessPermission("Competency", "Delete")]
        public async Task<IActionResult> DeleteCompetency(Guid id)
        {
            var entity = await _dbContext.MstCompetencies
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Competency berhasil dihapus."
            ));
        }

        [HttpGet("positions/{positionId:guid}/requirements")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Position Competency Requirement",
            Description = "Melihat competency requirement per position",
            AccessType = AccessTypes.Read,
            SortOrder = 6
        )]
        [AccessPermission("Competency", "Read")]
        public async Task<IActionResult> GetPositionRequirements(Guid positionId)
        {
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

            var items = await _dbContext.MstPositionCompetencyRequirements
                .AsNoTracking()
                .Where(x => x.PositionId == positionId && !x.IsDelete)
                .OrderBy(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty)
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
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                RequiredData = items.Count(x => x.IsRequired),
                CertificationRequiredData = items.Count(x => x.IsCertificationRequired),
                TrainingRequiredData = items.Count(x => x.IsTrainingRequired),
                Items = items
            };

            return Ok(ApiResponse<PositionCompetencyRequirementListResponse>.Ok(
                result,
                "Data competency requirement position berhasil diambil."
            ));
        }

        [HttpPost("positions/{positionId:guid}/requirements")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Position Competency Requirement",
            Description = "Membuat competency requirement position",
            AccessType = AccessTypes.Create,
            SortOrder = 7
        )]
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

            _dbContext.MstPositionCompetencyRequirements.Add(entity);
            await _dbContext.SaveChangesAsync();

            return await GetPositionRequirements(positionId);
        }

        [HttpPut("positions/{positionId:guid}/requirements/{requirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Position Competency Requirement",
            Description = "Mengubah competency requirement position",
            AccessType = AccessTypes.Update,
            SortOrder = 8
        )]
        [AccessPermission("Competency", "Update")]
        public async Task<IActionResult> UpdatePositionRequirement(
            Guid positionId,
            Guid requirementId,
            [FromBody] UpdatePositionCompetencyRequirementRequest request)
        {
            var entity = await _dbContext.MstPositionCompetencyRequirements
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

            return await GetPositionRequirements(positionId);
        }

        [HttpPatch("positions/{positionId:guid}/requirements/{requirementId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Position Competency Requirement Status",
            Description = "Mengubah status competency requirement position",
            AccessType = AccessTypes.Update,
            SortOrder = 9
        )]
        [AccessPermission("Competency", "Update")]
        public async Task<IActionResult> UpdatePositionRequirementStatus(
            Guid positionId,
            Guid requirementId,
            [FromBody] UpdatePositionCompetencyRequirementStatusRequest request)
        {
            var entity = await _dbContext.MstPositionCompetencyRequirements
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

            return await GetPositionRequirements(positionId);
        }

        [HttpDelete("positions/{positionId:guid}/requirements/{requirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PositionCompetencyRequirementListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Position Competency Requirement",
            Description = "Menghapus competency requirement position",
            AccessType = AccessTypes.Delete,
            SortOrder = 10
        )]
        [AccessPermission("Competency", "Delete")]
        public async Task<IActionResult> DeletePositionRequirement(
            Guid positionId,
            Guid requirementId)
        {
            var entity = await _dbContext.MstPositionCompetencyRequirements
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

            await _dbContext.SaveChangesAsync();

            return await GetPositionRequirements(positionId);
        }

        private async Task<(bool IsValid, string Message)> ValidatePositionRequirementRequestAsync(
            Guid positionId,
            Guid competencyId,
            Guid? excludeRequirementId)
        {
            var positionExists = await _dbContext.MstPositions
                .AnyAsync(x => x.Id == positionId && !x.IsDelete);

            if (!positionExists)
            {
                return (false, "Position tidak ditemukan.");
            }

            var competencyExists = await _dbContext.MstCompetencies
                .AnyAsync(x => x.Id == competencyId && !x.IsDelete);

            if (!competencyExists)
            {
                return (false, "Competency tidak ditemukan.");
            }

            var duplicate = await _dbContext.MstPositionCompetencyRequirements
                .AnyAsync(x =>
                    x.PositionId == positionId &&
                    x.CompetencyId == competencyId &&
                    !x.IsDelete &&
                    (!excludeRequirementId.HasValue || x.Id != excludeRequirementId.Value));

            if (duplicate)
            {
                return (false, "Competency sudah menjadi requirement untuk position ini.");
            }

            return (true, string.Empty);
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

        private static string NormalizeRequiredCode(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}