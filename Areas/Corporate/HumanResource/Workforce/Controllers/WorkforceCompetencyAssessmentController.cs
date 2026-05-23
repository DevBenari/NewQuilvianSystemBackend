using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/competencies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Competency Assessment",
        AreaName = "Corporate",
        ControllerName = "WorkforceCompetencyAssessment",
        Description = "Corporate human resource workforce competency assessment",
        SortOrder = 12
    )]
    [Tags("Corporate / Human Resource / Workforce / Competency Assessment")]
    public class WorkforceCompetencyAssessmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceCompetencyAssessmentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("assessments")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Competency Assessment",
            Description = "Melihat competency assessment workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetAssessments(Guid workforceProfileId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;

            var items = await _dbContext.WfpCompetencyAssessments
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.AssessmentDate)
                .ThenBy(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty)
                .Select(x => new WorkforceCompetencyAssessmentResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    CompetencyId = x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : string.Empty,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : string.Empty,
                    CompetencyCategory = x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other,
                    AssessmentDate = x.AssessmentDate,
                    CompetencyLevel = x.CompetencyLevel,
                    ResultStatus = x.ResultStatus,
                    AssessedByUserId = x.AssessedByUserId,
                    AssessedByUserName = x.AssessedByUser != null ? x.AssessedByUser.DisplayName : null,
                    ExpiredDate = x.ExpiredDate,
                    IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    Notes = x.Notes,
                    IsVerified = x.IsVerified,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceCompetencyAssessmentListResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                VerifiedData = items.Count(x => x.IsVerified),
                PassedData = items.Count(x => x.ResultStatus == CompetencyAssessmentResultStatus.Passed),
                FailedData = items.Count(x => x.ResultStatus == CompetencyAssessmentResultStatus.Failed),
                NeedTrainingData = items.Count(x => x.ResultStatus == CompetencyAssessmentResultStatus.NeedTraining),
                ExpiredData = items.Count(x => x.IsExpired),
                Items = items
            };

            return Ok(ApiResponse<WorkforceCompetencyAssessmentListResponse>.Ok(
                result,
                "Data competency assessment workforce berhasil diambil."
            ));
        }

        [HttpGet("assessments/{assessmentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Competency Assessment",
            Description = "Melihat detail competency assessment workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetAssessmentById(
            Guid workforceProfileId,
            Guid assessmentId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;

            var data = await _dbContext.WfpCompetencyAssessments
                .AsNoTracking()
                .Where(x =>
                    x.Id == assessmentId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new WorkforceCompetencyAssessmentResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    CompetencyId = x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : string.Empty,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : string.Empty,
                    CompetencyCategory = x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other,
                    AssessmentDate = x.AssessmentDate,
                    CompetencyLevel = x.CompetencyLevel,
                    ResultStatus = x.ResultStatus,
                    AssessedByUserId = x.AssessedByUserId,
                    AssessedByUserName = x.AssessedByUser != null ? x.AssessedByUser.DisplayName : null,
                    ExpiredDate = x.ExpiredDate,
                    IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    Notes = x.Notes,
                    IsVerified = x.IsVerified,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceCompetencyAssessmentResponse>.Ok(
                data,
                "Detail competency assessment workforce berhasil diambil."
            ));
        }

        [HttpPost("assessments")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Competency Assessment",
            Description = "Membuat competency assessment workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceCompetencyAssessment", "Create")]
        public async Task<IActionResult> CreateAssessment(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceCompetencyAssessmentRequest request)
        {
            var validation = await ValidateAssessmentRequestAsync(
                workforceProfileId,
                request.CompetencyId,
                request.AssessedByUserId,
                request.AssessmentDate,
                request.ExpiredDate
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

            var entity = new WfpCompetencyAssessment
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CompetencyId = request.CompetencyId,
                AssessmentDate = request.AssessmentDate.Date,
                CompetencyLevel = request.CompetencyLevel,
                ResultStatus = request.ResultStatus,
                AssessedByUserId = NormalizeNullableGuid(request.AssessedByUserId),
                ExpiredDate = request.ExpiredDate?.Date,
                FilePath = NormalizeNullableText(request.FilePath),
                FileContentType = NormalizeNullableText(request.FileContentType),
                Notes = NormalizeNullableText(request.Notes),
                IsVerified = request.IsVerified,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.WfpCompetencyAssessments.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCompetencyAssessment.CreateAssessment",
                "Competency assessment workforce berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.CompetencyId
                }
            );

            return await GetAssessmentById(workforceProfileId, entity.Id);
        }

        [HttpPut("assessments/{assessmentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Competency Assessment",
            Description = "Mengubah competency assessment workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceCompetencyAssessment", "Update")]
        public async Task<IActionResult> UpdateAssessment(
            Guid workforceProfileId,
            Guid assessmentId,
            [FromBody] UpdateWorkforceCompetencyAssessmentRequest request)
        {
            var entity = await _dbContext.WfpCompetencyAssessments
                .FirstOrDefaultAsync(x =>
                    x.Id == assessmentId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            var validation = await ValidateAssessmentRequestAsync(
                workforceProfileId,
                request.CompetencyId,
                request.AssessedByUserId,
                request.AssessmentDate,
                request.ExpiredDate
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

            entity.CompetencyId = request.CompetencyId;
            entity.AssessmentDate = request.AssessmentDate.Date;
            entity.CompetencyLevel = request.CompetencyLevel;
            entity.ResultStatus = request.ResultStatus;
            entity.AssessedByUserId = NormalizeNullableGuid(request.AssessedByUserId);
            entity.ExpiredDate = request.ExpiredDate?.Date;
            entity.FilePath = NormalizeNullableText(request.FilePath);
            entity.FileContentType = NormalizeNullableText(request.FileContentType);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsVerified = request.IsVerified;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetAssessmentById(workforceProfileId, entity.Id);
        }

        [HttpPatch("assessments/{assessmentId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Competency Assessment Status",
            Description = "Mengubah status aktif competency assessment workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceCompetencyAssessment", "Update")]
        public async Task<IActionResult> UpdateAssessmentStatus(
            Guid workforceProfileId,
            Guid assessmentId,
            [FromBody] UpdateWorkforceCompetencyAssessmentStatusRequest request)
        {
            var entity = await _dbContext.WfpCompetencyAssessments
                .FirstOrDefaultAsync(x =>
                    x.Id == assessmentId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetAssessmentById(workforceProfileId, entity.Id);
        }

        [HttpPatch("assessments/{assessmentId:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Verify Workforce Competency Assessment",
            Description = "Verifikasi competency assessment workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceCompetencyAssessment", "Update")]
        public async Task<IActionResult> VerifyAssessment(
            Guid workforceProfileId,
            Guid assessmentId,
            [FromBody] VerifyWorkforceCompetencyAssessmentRequest request)
        {
            var entity = await _dbContext.WfpCompetencyAssessments
                .FirstOrDefaultAsync(x =>
                    x.Id == assessmentId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            entity.IsVerified = request.IsVerified;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetAssessmentById(workforceProfileId, entity.Id);
        }

        [HttpDelete("assessments/{assessmentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Competency Assessment",
            Description = "Menghapus competency assessment workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceCompetencyAssessment", "Delete")]
        public async Task<IActionResult> DeleteAssessment(
            Guid workforceProfileId,
            Guid assessmentId)
        {
            var entity = await _dbContext.WfpCompetencyAssessments
                .FirstOrDefaultAsync(x =>
                    x.Id == assessmentId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
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
                "Competency assessment berhasil dihapus."
            ));
        }

        [HttpGet("matrix")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyMatrixResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Competency Matrix",
            Description = "Melihat competency matrix workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 7
        )]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetCompetencyMatrix(Guid workforceProfileId)
        {
            var profile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.ProfileCode,
                    x.DisplayName,
                    x.UserType,
                    x.PrimaryPositionId,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null
                })
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var result = new WorkforceCompetencyMatrixResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                PrimaryPositionId = profile.PrimaryPositionId,
                PrimaryPositionName = profile.PrimaryPositionName
            };

            if (!profile.PrimaryPositionId.HasValue)
            {
                return Ok(ApiResponse<WorkforceCompetencyMatrixResponse>.Ok(
                    result,
                    "Workforce belum memiliki primary position."
                ));
            }

            var today = DateTime.UtcNow.Date;

            var requirements = await _dbContext.MstPositionCompetencyRequirements
                .AsNoTracking()
                .Where(x =>
                    x.PositionId == profile.PrimaryPositionId.Value &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderBy(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty)
                .Select(x => new
                {
                    x.Id,
                    x.PositionId,
                    PositionName = x.Position != null ? x.Position.PositionName : string.Empty,
                    x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : string.Empty,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : string.Empty,
                    CompetencyCategory = x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other,
                    x.IsRequired,
                    x.MinimumLevel,
                    x.IsCertificationRequired,
                    x.IsTrainingRequired
                })
                .ToListAsync();

            var assessments = await _dbContext.WfpCompetencyAssessments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderByDescending(x => x.AssessmentDate)
                .ToListAsync();

            foreach (var requirement in requirements)
            {
                var latestAssessment = assessments
                    .Where(x => x.CompetencyId == requirement.CompetencyId)
                    .OrderByDescending(x => x.AssessmentDate)
                    .FirstOrDefault();

                var isExpired = latestAssessment?.ExpiredDate.HasValue == true &&
                                latestAssessment.ExpiredDate.Value.Date < today;

                var isPassed = latestAssessment != null &&
                               latestAssessment.ResultStatus == CompetencyAssessmentResultStatus.Passed &&
                               latestAssessment.IsVerified &&
                               !isExpired;

                var isLevelMet = latestAssessment != null &&
                                 (int)latestAssessment.CompetencyLevel >= (int)requirement.MinimumLevel;

                var status = ResolveMatrixStatus(latestAssessment, isPassed, isLevelMet, isExpired);

                result.Items.Add(new WorkforceCompetencyMatrixItemResponse
                {
                    RequirementId = requirement.Id,
                    PositionId = requirement.PositionId,
                    PositionName = requirement.PositionName,
                    CompetencyId = requirement.CompetencyId,
                    CompetencyCode = requirement.CompetencyCode,
                    CompetencyName = requirement.CompetencyName,
                    CompetencyCategory = requirement.CompetencyCategory,
                    IsRequired = requirement.IsRequired,
                    MinimumLevel = requirement.MinimumLevel,
                    IsCertificationRequired = requirement.IsCertificationRequired,
                    IsTrainingRequired = requirement.IsTrainingRequired,
                    LatestAssessmentId = latestAssessment?.Id,
                    LatestCompetencyLevel = latestAssessment?.CompetencyLevel,
                    LatestResultStatus = latestAssessment?.ResultStatus,
                    LatestAssessmentDate = latestAssessment?.AssessmentDate,
                    ExpiredDate = latestAssessment?.ExpiredDate,
                    IsVerified = latestAssessment?.IsVerified ?? false,
                    IsExpired = isExpired,
                    IsPassed = isPassed,
                    IsLevelMet = isLevelMet,
                    Status = status
                });
            }

            result.TotalRequirement = result.Items.Count;
            result.CompletedRequirement = result.Items.Count(x => x.Status == "Completed");
            result.MissingRequirement = result.Items.Count(x => x.Status == "Missing");
            result.NeedTrainingRequirement = result.Items.Count(x => x.Status == "NeedTraining");
            result.ExpiredRequirement = result.Items.Count(x => x.Status == "Expired");

            return Ok(ApiResponse<WorkforceCompetencyMatrixResponse>.Ok(
                result,
                "Competency matrix workforce berhasil diambil."
            ));
        }

        private async Task<(bool IsValid, string Message)> ValidateAssessmentRequestAsync(
            Guid workforceProfileId,
            Guid competencyId,
            Guid? assessedByUserId,
            DateTime assessmentDate,
            DateTime? expiredDate)
        {
            var profileExists = await _dbContext.MstWorkforceProfiles
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return (false, "Workforce profile tidak ditemukan.");
            }

            var competencyExists = await _dbContext.MstCompetencies
                .AnyAsync(x => x.Id == competencyId && !x.IsDelete);

            if (!competencyExists)
            {
                return (false, "Competency tidak ditemukan.");
            }

            if (assessmentDate == default)
            {
                return (false, "AssessmentDate wajib diisi.");
            }

            if (expiredDate.HasValue && expiredDate.Value.Date < assessmentDate.Date)
            {
                return (false, "ExpiredDate tidak boleh lebih kecil dari AssessmentDate.");
            }

            if (assessedByUserId.HasValue && assessedByUserId.Value != Guid.Empty)
            {
                var userExists = await _dbContext.Users
                    .AnyAsync(x => x.Id == assessedByUserId.Value);

                if (!userExists)
                {
                    return (false, "AssessedByUser tidak ditemukan.");
                }
            }

            return (true, string.Empty);
        }

        private async Task<WorkforceProfileHeader?> GetWorkforceProfileHeaderAsync(Guid workforceProfileId)
        {
            return await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => new WorkforceProfileHeader
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName,
                    UserType = x.UserType
                })
                .FirstOrDefaultAsync();
        }

        private static string ResolveMatrixStatus(
            WfpCompetencyAssessment? latestAssessment,
            bool isPassed,
            bool isLevelMet,
            bool isExpired)
        {
            if (latestAssessment == null)
            {
                return "Missing";
            }

            if (isExpired)
            {
                return "Expired";
            }

            if (latestAssessment.ResultStatus == CompetencyAssessmentResultStatus.NeedTraining)
            {
                return "NeedTraining";
            }

            if (latestAssessment.ResultStatus == CompetencyAssessmentResultStatus.Failed)
            {
                return "Failed";
            }

            if (!latestAssessment.IsVerified)
            {
                return "NeedVerification";
            }

            if (isPassed && isLevelMet)
            {
                return "Completed";
            }

            return "NotMet";
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

        private class WorkforceProfileHeader
        {
            public Guid Id { get; set; }

            public string ProfileCode { get; set; } = string.Empty;

            public string DisplayName { get; set; } = string.Empty;

            public UserType UserType { get; set; }
        }
    }
}