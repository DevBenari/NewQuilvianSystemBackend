using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-requirements")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Requirement",
        AreaName = "Corporate",
        ControllerName = "WorkforceRequirement",
        Description = "Workforce profile requirement management",
        SortOrder = 22
    )]
    [Tags("Corporate / Human Resource / Workforce / Requirement")]
    public class WorkforceRequirementController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Requirement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceRequirementController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Requirement",
            Description = "Melihat master requirement workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetRequirements(
            [FromQuery] UserType? userType,
            [FromQuery] string? requirementCategory,
            [FromQuery] string? search,
            [FromQuery] bool? isActive)
        {
            var query = _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (userType.HasValue)
            {
                query = query.Where(x => x.UserType == userType.Value);
            }

            if (!string.IsNullOrWhiteSpace(requirementCategory))
            {
                var normalizedCategory = NormalizeRequirementCategory(requirementCategory);
                query = query.Where(x => x.RequirementCategory == normalizedCategory);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText = search.Trim().ToLower();

                query = query.Where(x =>
                    x.RequirementCode.ToLower().Contains(searchText) ||
                    x.RequirementName.ToLower().Contains(searchText) ||
                    x.RequirementCategory.ToLower().Contains(searchText) ||
                    (x.Description != null && x.Description.ToLower().Contains(searchText)));
            }

            var items = await query
                .OrderBy(x => x.UserType)
                .ThenBy(x => x.RequirementCategory)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.RequirementName)
                .Select(x => new WorkforceRequirementResponse
                {
                    Id = x.Id,
                    UserType = x.UserType,
                    RequirementCategory = x.RequirementCategory,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    IsRequired = x.IsRequired,
                    IsMultipleAllowed = x.IsMultipleAllowed,
                    IsFileRequired = x.IsFileRequired,
                    IsNumberRequired = x.IsNumberRequired,
                    IsIssueDateRequired = x.IsIssueDateRequired,
                    IsExpiredDateRequired = x.IsExpiredDateRequired,
                    IsVerificationRequired = x.IsVerificationRequired,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceRequirementListResponse
            {
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                RequiredData = items.Count(x => x.IsRequired),
                Items = items
            };

            return Ok(ApiResponse<WorkforceRequirementListResponse>.Ok(
                result,
                "Data workforce requirement berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Requirement",
            Description = "Melihat detail master requirement workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetRequirementById(Guid id)
        {
            var data = await _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceRequirementResponse
                {
                    Id = x.Id,
                    UserType = x.UserType,
                    RequirementCategory = x.RequirementCategory,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    IsRequired = x.IsRequired,
                    IsMultipleAllowed = x.IsMultipleAllowed,
                    IsFileRequired = x.IsFileRequired,
                    IsNumberRequired = x.IsNumberRequired,
                    IsIssueDateRequired = x.IsIssueDateRequired,
                    IsExpiredDateRequired = x.IsExpiredDateRequired,
                    IsVerificationRequired = x.IsVerificationRequired,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceRequirementResponse>.Ok(
                data,
                "Detail workforce requirement berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Requirement",
            Description = "Menambah master requirement workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceRequirement", "Create")]
        public async Task<IActionResult> CreateRequirement(
            [FromBody] CreateWorkforceRequirementRequest request)
        {
            var validation = await ValidateRequirementRequestAsync(
                request.UserType,
                request.RequirementCategory,
                request.RequirementCode,
                request.RequirementName,
                excludeId: null);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data workforce requirement tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstWorkforceRequirement
            {
                Id = Guid.NewGuid(),
                UserType = request.UserType,
                RequirementCategory = NormalizeRequirementCategory(request.RequirementCategory),
                RequirementCode = NormalizeRequirementCode(request.RequirementCode),
                RequirementName = request.RequirementName.Trim(),
                IsRequired = request.IsRequired,
                IsMultipleAllowed = request.IsMultipleAllowed,
                IsFileRequired = request.IsFileRequired,
                IsNumberRequired = request.IsNumberRequired,
                IsIssueDateRequired = request.IsIssueDateRequired,
                IsExpiredDateRequired = request.IsExpiredDateRequired,
                IsVerificationRequired = request.IsVerificationRequired,
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstWorkforceRequirement>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildRequirementResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.CreateRequirement",
                "Workforce requirement berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.UserType,
                    entity.RequirementCategory,
                    entity.RequirementCode
                }
            );

            return Ok(ApiResponse<WorkforceRequirementResponse>.Ok(
                response!,
                "Workforce requirement berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Requirement",
            Description = "Mengubah master requirement workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceRequirement", "Update")]
        public async Task<IActionResult> UpdateRequirement(
            Guid id,
            [FromBody] UpdateWorkforceRequirementRequest request)
        {
            var entity = await _dbContext.Set<MstWorkforceRequirement>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            var validation = await ValidateRequirementRequestAsync(
                request.UserType,
                request.RequirementCategory,
                request.RequirementCode,
                request.RequirementName,
                excludeId: id);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data workforce requirement tidak valid."
                ));
            }

            entity.UserType = request.UserType;
            entity.RequirementCategory = NormalizeRequirementCategory(request.RequirementCategory);
            entity.RequirementCode = NormalizeRequirementCode(request.RequirementCode);
            entity.RequirementName = request.RequirementName.Trim();
            entity.IsRequired = request.IsRequired;
            entity.IsMultipleAllowed = request.IsMultipleAllowed;
            entity.IsFileRequired = request.IsFileRequired;
            entity.IsNumberRequired = request.IsNumberRequired;
            entity.IsIssueDateRequired = request.IsIssueDateRequired;
            entity.IsExpiredDateRequired = request.IsExpiredDateRequired;
            entity.IsVerificationRequired = request.IsVerificationRequired;
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildRequirementResponseAsync(entity.Id);

            return Ok(ApiResponse<WorkforceRequirementResponse>.Ok(
                response!,
                "Workforce requirement berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Requirement",
            Description = "Mengubah status master requirement workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceRequirement", "Update")]
        public async Task<IActionResult> UpdateRequirementStatus(
            Guid id,
            [FromBody] UpdateWorkforceRequirementStatusRequest request)
        {
            var entity = await _dbContext.Set<MstWorkforceRequirement>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status workforce requirement berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Requirement",
            Description = "Menghapus master requirement workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceRequirement", "Delete")]
        public async Task<IActionResult> DeleteRequirement(Guid id)
        {
            var entity = await _dbContext.Set<MstWorkforceRequirement>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Workforce requirement berhasil dihapus."
            ));
        }

        [HttpGet("/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/requirements")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceProfileRequirementChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Requirement",
            Description = "Melihat checklist requirement berdasarkan workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetProfileRequirements(Guid workforceProfileId)
        {
            var profile = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var requirements = await _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .Where(x =>
                    x.UserType == profile.UserType &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderBy(x => x.RequirementCategory)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.RequirementName)
                .ToListAsync();

            var actualRecords = await BuildActualRequirementRecordsAsync(workforceProfileId);

            var today = DateTime.UtcNow.Date;
            var checklistItems = new List<WorkforceRequirementChecklistItemResponse>();

            foreach (var requirement in requirements)
            {
                var actual = actualRecords
                    .Where(x =>
                        x.RequirementCategory.Equals(requirement.RequirementCategory, StringComparison.OrdinalIgnoreCase) &&
                        x.RequirementCode.Equals(requirement.RequirementCode, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.IsVerified)
                    .ThenByDescending(x => x.CreateDateTime)
                    .FirstOrDefault();

                var isSubmitted = actual != null;
                var isVerified = actual?.IsVerified == true;
                var isExpired = actual?.ExpiredDate.HasValue == true &&
                                actual.ExpiredDate.Value.Date < today;

                var status = ResolveRequirementStatus(
                    requirement,
                    isSubmitted,
                    isVerified,
                    isExpired);

                checklistItems.Add(new WorkforceRequirementChecklistItemResponse
                {
                    RequirementId = requirement.Id,
                    UserType = requirement.UserType,
                    RequirementCategory = requirement.RequirementCategory,
                    RequirementCode = requirement.RequirementCode,
                    RequirementName = requirement.RequirementName,
                    IsRequired = requirement.IsRequired,
                    IsFileRequired = requirement.IsFileRequired,
                    IsNumberRequired = requirement.IsNumberRequired,
                    IsIssueDateRequired = requirement.IsIssueDateRequired,
                    IsExpiredDateRequired = requirement.IsExpiredDateRequired,
                    IsVerificationRequired = requirement.IsVerificationRequired,
                    IsSubmitted = isSubmitted,
                    IsVerified = isVerified,
                    IsExpired = isExpired,
                    SourceId = actual?.SourceId,
                    SourceType = actual?.SourceType,
                    FilePath = actual?.FilePath,
                    ExpiredDate = actual?.ExpiredDate,
                    Status = status,
                    SortOrder = requirement.SortOrder,
                    Description = requirement.Description
                });
            }

            var groups = checklistItems
                .GroupBy(x => x.RequirementCategory)
                .Select(group => new WorkforceRequirementChecklistGroupResponse
                {
                    RequirementCategory = group.Key,
                    TotalData = group.Count(),
                    CompletedData = group.Count(x => x.Status == "Completed"),
                    MissingData = group.Count(x => x.Status == "Missing"),
                    NeedVerificationData = group.Count(x => x.Status == "NeedVerification"),
                    ExpiredData = group.Count(x => x.Status == "Expired"),
                    Items = group
                        .OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.RequirementName)
                        .ToList()
                })
                .OrderBy(x => x.RequirementCategory)
                .ToList();

            var result = new WorkforceProfileRequirementChecklistResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                TotalRequirement = checklistItems.Count,
                CompletedRequirement = checklistItems.Count(x => x.Status == "Completed"),
                MissingRequirement = checklistItems.Count(x => x.Status == "Missing"),
                NeedVerificationRequirement = checklistItems.Count(x => x.Status == "NeedVerification"),
                ExpiredRequirement = checklistItems.Count(x => x.Status == "Expired"),
                Groups = groups
            };

            return Ok(ApiResponse<WorkforceProfileRequirementChecklistResponse>.Ok(
                result,
                "Checklist requirement workforce profile berhasil diambil."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequirementRequestAsync(
            UserType userType,
            string? requirementCategory,
            string? requirementCode,
            string? requirementName,
            Guid? excludeId)
        {
            if (string.IsNullOrWhiteSpace(requirementCategory))
            {
                return (false, "RequirementCategory wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(requirementCode))
            {
                return (false, "RequirementCode wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(requirementName))
            {
                return (false, "RequirementName wajib diisi.");
            }

            var normalizedCategory = NormalizeRequirementCategory(requirementCategory);
            var normalizedCode = NormalizeRequirementCode(requirementCode);

            var allowedCategories = new[]
            {
                "Document",
                "Education",
                "Training",
                "Certification",
                "License"
            };

            if (!allowedCategories.Contains(normalizedCategory))
            {
                return (false, "RequirementCategory hanya boleh Document, Education, Training, Certification, atau License.");
            }

            var duplicateExists = await _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id != excludeId &&
                    x.UserType == userType &&
                    x.RequirementCategory == normalizedCategory &&
                    x.RequirementCode == normalizedCode &&
                    !x.IsDelete);

            if (duplicateExists)
            {
                return (false, "RequirementCode sudah terdaftar untuk UserType dan RequirementCategory tersebut.");
            }

            return (true, null);
        }

        private async Task<WorkforceRequirementResponse?> BuildRequirementResponseAsync(Guid id)
        {
            return await _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceRequirementResponse
                {
                    Id = x.Id,
                    UserType = x.UserType,
                    RequirementCategory = x.RequirementCategory,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    IsRequired = x.IsRequired,
                    IsMultipleAllowed = x.IsMultipleAllowed,
                    IsFileRequired = x.IsFileRequired,
                    IsNumberRequired = x.IsNumberRequired,
                    IsIssueDateRequired = x.IsIssueDateRequired,
                    IsExpiredDateRequired = x.IsExpiredDateRequired,
                    IsVerificationRequired = x.IsVerificationRequired,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();
        }

        private async Task<List<RequirementActualRecord>> BuildActualRequirementRecordsAsync(
            Guid workforceProfileId)
        {
            var result = new List<RequirementActualRecord>();

            var documents = await _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => new RequirementActualRecord
                {
                    SourceId = x.Id,
                    SourceType = "Document",
                    RequirementCategory = "Document",
                    RequirementCode = x.RequirementCode ?? x.DocumentType,
                    FilePath = x.FilePath,
                    IsVerified = x.IsVerified,
                    ExpiredDate = x.ExpiredDate,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            result.AddRange(documents);

            var educations = await _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => new RequirementActualRecord
                {
                    SourceId = x.Id,
                    SourceType = "Education",
                    RequirementCategory = "Education",
                    RequirementCode = x.RequirementCode ?? x.EducationLevel,
                    FilePath = x.FilePath,
                    IsVerified = x.IsVerified,
                    ExpiredDate = null,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            result.AddRange(educations);

            var trainings = await _dbContext.Set<WfpTrainingRecord>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => new RequirementActualRecord
                {
                    SourceId = x.Id,
                    SourceType = "Training",
                    RequirementCategory = "Training",
                    RequirementCode = x.RequirementCode ?? x.TrainingType,
                    FilePath = x.FilePath,
                    IsVerified = x.IsVerified,
                    ExpiredDate = null,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            result.AddRange(trainings);

            var certifications = await _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => new RequirementActualRecord
                {
                    SourceId = x.Id,
                    SourceType = "Certification",
                    RequirementCategory = "Certification",
                    RequirementCode = x.RequirementCode ?? x.CertificationName,
                    FilePath = x.FilePath,
                    IsVerified = x.IsVerified,
                    ExpiredDate = x.ExpiredDate,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            result.AddRange(certifications);

            var licenses = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => new RequirementActualRecord
                {
                    SourceId = x.Id,
                    SourceType = "License",
                    RequirementCategory = "License",
                    RequirementCode = x.RequirementCode ?? x.LicenseType,
                    FilePath = x.FilePath,
                    IsVerified = x.IsVerified,
                    ExpiredDate = x.ExpiredDate,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            result.AddRange(licenses);

            return result;
        }

        private static string ResolveRequirementStatus(
            MstWorkforceRequirement requirement,
            bool isSubmitted,
            bool isVerified,
            bool isExpired)
        {
            if (!isSubmitted)
            {
                return requirement.IsRequired
                    ? "Missing"
                    : "Optional";
            }

            if (isExpired)
            {
                return "Expired";
            }

            if (requirement.IsVerificationRequired && !isVerified)
            {
                return "NeedVerification";
            }

            return "Completed";
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string NormalizeRequirementCategory(string value)
        {
            var text = value.Trim();

            return text.ToLower() switch
            {
                "document" => "Document",
                "education" => "Education",
                "training" => "Training",
                "certification" => "Certification",
                "license" => "License",
                "credential" => "License",
                "credentiallicense" => "License",
                "credential_license" => "License",
                "credential-license" => "License",
                _ => text
            };
        }

        private static string NormalizeRequirementCode(string value)
        {
            return value.Trim().ToUpperInvariant().Replace(" ", "_");
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private sealed class RequirementActualRecord
        {
            public Guid SourceId { get; set; }

            public string SourceType { get; set; } = string.Empty;

            public string RequirementCategory { get; set; } = string.Empty;

            public string RequirementCode { get; set; } = string.Empty;

            public string? FilePath { get; set; }

            public bool IsVerified { get; set; }

            public DateTime? ExpiredDate { get; set; }

            public DateTime CreateDateTime { get; set; }
        }
    }
}