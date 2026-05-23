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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/disciplinary-actions")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Disciplinary Action",
        AreaName = "Corporate",
        ControllerName = "WorkforceDisciplinaryAction",
        Description = "Corporate human resource workforce disciplinary action",
        SortOrder = 14
    )]
    [Tags("Corporate / Human Resource / Workforce / Disciplinary Action")]
    public class WorkforceDisciplinaryActionController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private static readonly string[] AllowedFileExtensions =
        {
            ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IWebHostEnvironment _environment;

        public WorkforceDisciplinaryActionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDisciplinaryActionListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Disciplinary Action",
            Description = "Melihat data disciplinary action workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceDisciplinaryAction", "Read")]
        public async Task<IActionResult> GetDisciplinaryActions(Guid workforceProfileId)
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

            var items = await _dbContext.WfpDisciplinaryActions
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.IssuedDate)
                .ThenByDescending(x => x.IncidentDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new WorkforceDisciplinaryActionResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    ActionType = x.ActionType,
                    IncidentDate = x.IncidentDate,
                    IssuedDate = x.IssuedDate,
                    SeverityLevel = x.SeverityLevel,
                    Reason = x.Reason,
                    Description = x.Description,
                    IssuedByUserId = x.IssuedByUserId,
                    IssuedByUserName = x.IssuedByUser != null ? x.IssuedByUser.DisplayName : null,
                    EffectiveUntil = x.EffectiveUntil,
                    FilePath = x.FilePath,
                    HasFile = x.FilePath != null && x.FilePath != "",
                    ActionStatus = x.ActionStatus,
                    Notes = x.Notes,
                    IsExpired = x.EffectiveUntil.HasValue &&
                        x.EffectiveUntil.Value < today &&
                        x.ActionStatus != DisciplinaryActionStatus.Resolved &&
                        x.ActionStatus != DisciplinaryActionStatus.Cancelled,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceDisciplinaryActionListResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                DraftData = items.Count(x => x.ActionStatus == DisciplinaryActionStatus.Draft),
                IssuedData = items.Count(x => x.ActionStatus == DisciplinaryActionStatus.Issued),
                AcknowledgedData = items.Count(x => x.ActionStatus == DisciplinaryActionStatus.Acknowledged),
                UnderReviewData = items.Count(x => x.ActionStatus == DisciplinaryActionStatus.UnderReview),
                ResolvedData = items.Count(x => x.ActionStatus == DisciplinaryActionStatus.Resolved),
                CancelledData = items.Count(x => x.ActionStatus == DisciplinaryActionStatus.Cancelled),
                ExpiredData = items.Count(x => x.IsExpired || x.ActionStatus == DisciplinaryActionStatus.Expired),
                HighSeverityData = items.Count(x => x.SeverityLevel == DisciplinarySeverityLevel.High),
                CriticalSeverityData = items.Count(x => x.SeverityLevel == DisciplinarySeverityLevel.Critical),
                WithFileData = items.Count(x => x.HasFile),
                Items = items
            };

            return Ok(ApiResponse<WorkforceDisciplinaryActionListResponse>.Ok(
                result,
                "Data disciplinary action workforce berhasil diambil."
            ));
        }

        [HttpGet("{disciplinaryActionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDisciplinaryActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Disciplinary Action",
            Description = "Melihat detail disciplinary action workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceDisciplinaryAction", "Read")]
        public async Task<IActionResult> GetDisciplinaryActionById(
            Guid workforceProfileId,
            Guid disciplinaryActionId)
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

            var data = await _dbContext.WfpDisciplinaryActions
                .AsNoTracking()
                .Where(x =>
                    x.Id == disciplinaryActionId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new WorkforceDisciplinaryActionResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    ActionType = x.ActionType,
                    IncidentDate = x.IncidentDate,
                    IssuedDate = x.IssuedDate,
                    SeverityLevel = x.SeverityLevel,
                    Reason = x.Reason,
                    Description = x.Description,
                    IssuedByUserId = x.IssuedByUserId,
                    IssuedByUserName = x.IssuedByUser != null ? x.IssuedByUser.DisplayName : null,
                    EffectiveUntil = x.EffectiveUntil,
                    FilePath = x.FilePath,
                    HasFile = x.FilePath != null && x.FilePath != "",
                    ActionStatus = x.ActionStatus,
                    Notes = x.Notes,
                    IsExpired = x.EffectiveUntil.HasValue &&
                        x.EffectiveUntil.Value < today &&
                        x.ActionStatus != DisciplinaryActionStatus.Resolved &&
                        x.ActionStatus != DisciplinaryActionStatus.Cancelled,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Disciplinary action tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceDisciplinaryActionResponse>.Ok(
                data,
                "Detail disciplinary action workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDisciplinaryActionResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Disciplinary Action",
            Description = "Membuat disciplinary action workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceDisciplinaryAction", "Create")]
        public async Task<IActionResult> CreateDisciplinaryAction(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceDisciplinaryActionRequest request)
        {
            var validation = await ValidateRequestAsync(
                workforceProfileId,
                request.ActionType,
                request.IncidentDate,
                request.IssuedDate,
                request.SeverityLevel,
                request.Reason,
                request.IssuedByUserId,
                request.EffectiveUntil,
                request.File
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

            try
            {
                var filePath = await SaveFileAsync(workforceProfileId, request.File);

                var entity = new WfpDisciplinaryAction
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    ActionType = request.ActionType,
                    IncidentDate = request.IncidentDate.Date,
                    IssuedDate = request.IssuedDate.Date,
                    SeverityLevel = request.SeverityLevel,
                    Reason = request.Reason.Trim(),
                    Description = NormalizeNullableText(request.Description),
                    IssuedByUserId = request.IssuedByUserId,
                    EffectiveUntil = request.EffectiveUntil?.Date,
                    FilePath = filePath,
                    ActionStatus = request.ActionStatus,
                    Notes = NormalizeNullableText(request.Notes),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.WfpDisciplinaryActions.Add(entity);
                await _dbContext.SaveChangesAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceDisciplinaryAction.CreateDisciplinaryAction",
                    "Disciplinary action workforce berhasil dibuat.",
                    new
                    {
                        entity.Id,
                        entity.WorkforceProfileId,
                        entity.ActionType,
                        entity.SeverityLevel,
                        entity.ActionStatus
                    }
                );

                return await GetDisciplinaryActionById(workforceProfileId, entity.Id);
            }
            catch (Exception ex)
            {
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceDisciplinaryAction.CreateDisciplinaryAction",
                    "Gagal membuat disciplinary action workforce.",
                    ex,
                    new { workforceProfileId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal membuat disciplinary action workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpPut("{disciplinaryActionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDisciplinaryActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Disciplinary Action",
            Description = "Mengubah disciplinary action workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceDisciplinaryAction", "Update")]
        public async Task<IActionResult> UpdateDisciplinaryAction(
            Guid workforceProfileId,
            Guid disciplinaryActionId,
            [FromForm] UpdateWorkforceDisciplinaryActionRequest request)
        {
            var entity = await _dbContext.WfpDisciplinaryActions
                .FirstOrDefaultAsync(x =>
                    x.Id == disciplinaryActionId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Disciplinary action tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                request.ActionType,
                request.IncidentDate,
                request.IssuedDate,
                request.SeverityLevel,
                request.Reason,
                request.IssuedByUserId,
                request.EffectiveUntil,
                request.File
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

            if (request.File != null)
            {
                DeletePhysicalFile(entity.FilePath);
                entity.FilePath = await SaveFileAsync(workforceProfileId, request.File);
            }
            else if (request.ReplaceExistingFile)
            {
                DeletePhysicalFile(entity.FilePath);
                entity.FilePath = null;
            }

            entity.ActionType = request.ActionType;
            entity.IncidentDate = request.IncidentDate.Date;
            entity.IssuedDate = request.IssuedDate.Date;
            entity.SeverityLevel = request.SeverityLevel;
            entity.Reason = request.Reason.Trim();
            entity.Description = NormalizeNullableText(request.Description);
            entity.IssuedByUserId = request.IssuedByUserId;
            entity.EffectiveUntil = request.EffectiveUntil?.Date;
            entity.ActionStatus = request.ActionStatus;
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetDisciplinaryActionById(workforceProfileId, entity.Id);
        }

        [HttpPatch("{disciplinaryActionId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDisciplinaryActionResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Disciplinary Action Status",
            Description = "Mengubah status disciplinary action workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceDisciplinaryAction", "Update")]
        public async Task<IActionResult> UpdateDisciplinaryActionStatus(
            Guid workforceProfileId,
            Guid disciplinaryActionId,
            [FromBody] UpdateWorkforceDisciplinaryActionStatusRequest request)
        {
            var entity = await _dbContext.WfpDisciplinaryActions
                .FirstOrDefaultAsync(x =>
                    x.Id == disciplinaryActionId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Disciplinary action tidak ditemukan."
                ));
            }

            entity.ActionStatus = request.ActionStatus;
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetDisciplinaryActionById(workforceProfileId, entity.Id);
        }

        [HttpDelete("{disciplinaryActionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Disciplinary Action",
            Description = "Menghapus disciplinary action workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceDisciplinaryAction", "Delete")]
        public async Task<IActionResult> DeleteDisciplinaryAction(
            Guid workforceProfileId,
            Guid disciplinaryActionId)
        {
            var entity = await _dbContext.WfpDisciplinaryActions
                .FirstOrDefaultAsync(x =>
                    x.Id == disciplinaryActionId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Disciplinary action tidak ditemukan."
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
                "Disciplinary action berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string Message)> ValidateRequestAsync(
            Guid workforceProfileId,
            DisciplinaryActionType actionType,
            DateTime incidentDate,
            DateTime issuedDate,
            DisciplinarySeverityLevel severityLevel,
            string reason,
            Guid issuedByUserId,
            DateTime? effectiveUntil,
            IFormFile? file)
        {
            var profileExists = await _dbContext.MstWorkforceProfiles
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return (false, "Workforce profile tidak ditemukan.");
            }

            if (actionType == DisciplinaryActionType.Unknown)
            {
                return (false, "ActionType wajib dipilih.");
            }

            if (incidentDate == default)
            {
                return (false, "IncidentDate wajib diisi.");
            }

            if (issuedDate == default)
            {
                return (false, "IssuedDate wajib diisi.");
            }

            if (issuedDate.Date < incidentDate.Date)
            {
                return (false, "IssuedDate tidak boleh lebih kecil dari IncidentDate.");
            }

            if (severityLevel == DisciplinarySeverityLevel.Unknown)
            {
                return (false, "SeverityLevel wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "Reason wajib diisi.");
            }

            if (issuedByUserId == Guid.Empty)
            {
                return (false, "IssuedByUserId wajib diisi.");
            }

            var issuedByUserExists = await _dbContext.Users
                .AnyAsync(x => x.Id == issuedByUserId);

            if (!issuedByUserExists)
            {
                return (false, "IssuedBy user tidak ditemukan.");
            }

            if (effectiveUntil.HasValue && effectiveUntil.Value.Date < issuedDate.Date)
            {
                return (false, "EffectiveUntil tidak boleh lebih kecil dari IssuedDate.");
            }

            var fileValidation = ValidateFile(file);

            if (!fileValidation.IsValid)
            {
                return fileValidation;
            }

            return (true, string.Empty);
        }

        private static (bool IsValid, string Message) ValidateFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return (true, string.Empty);
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ukuran file maksimal 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedFileExtensions.Contains(extension))
            {
                return (false, "Format file harus PDF, DOC, DOCX, JPG, JPEG, atau PNG.");
            }

            return (true, string.Empty);
        }

        private async Task<string?> SaveFileAsync(Guid workforceProfileId, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var uploadRoot = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "uploads",
                "workforce",
                "disciplinary-actions",
                workforceProfileId.ToString()
            );

            Directory.CreateDirectory(uploadRoot);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadRoot, fileName);

            await using var stream = new FileStream(physicalPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/workforce/disciplinary-actions/{workforceProfileId}/{fileName}";
        }

        private void DeletePhysicalFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var relativePath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var physicalPath = Path.Combine(webRootPath, relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
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

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
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
