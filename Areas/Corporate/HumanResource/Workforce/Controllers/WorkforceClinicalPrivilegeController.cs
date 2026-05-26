using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Clinical Privilege",
        AreaName = "Corporate",
        ControllerName = "WorkforceClinicalPrivilege",
        Description = "Workforce clinical privilege management",
        SortOrder = 28
    )]
    [Tags("Corporate / Human Resource / Workforce / Clinical Privilege")]
    public class WorkforceClinicalPrivilegeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.ClinicalPrivilege";
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IWebHostEnvironment _environment;

        public WorkforceClinicalPrivilegeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Clinical Privilege",
            Description = "Melihat clinical privilege workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetClinicalPrivileges(
            Guid workforceProfileId,
            [FromQuery] ClinicalPrivilegeStatus? privilegeStatus,
            [FromQuery] ClinicalPrivilegeType? privilegeType,
            [FromQuery] Guid? credentialLicenseId,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isExpired)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;

            var query = _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (privilegeStatus.HasValue)
            {
                query = query.Where(x => x.PrivilegeStatus == privilegeStatus.Value);
            }

            if (privilegeType.HasValue)
            {
                query = query.Where(x => x.PrivilegeType == privilegeType.Value);
            }

            if (credentialLicenseId.HasValue && credentialLicenseId.Value != Guid.Empty)
            {
                query = query.Where(x => x.CredentialLicenseId == credentialLicenseId.Value);
            }

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.DepartmentId == departmentId.Value);
            }

            if (positionId.HasValue && positionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PositionId == positionId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (isExpired.HasValue)
            {
                query = isExpired.Value
                    ? query.Where(x => x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today)
                    : query.Where(x => !x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today);
            }

            var rawItems = await query
                .OrderByDescending(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Active)
                .ThenByDescending(x => x.IsActive)
                .ThenBy(x => x.EffectiveEndDate)
                .ThenBy(x => x.PrivilegeName)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    x.CredentialLicenseId,
                    CredentialLicenseType = x.CredentialLicense != null ? x.CredentialLicense.LicenseType : null,
                    CredentialLicenseNumber = x.CredentialLicense != null ? x.CredentialLicense.LicenseNumber : null,
                    x.DepartmentId,
                    DepartmentCode = x.Department != null ? x.Department.DepartmentCode : null,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    x.PositionId,
                    PositionCode = x.Position != null ? x.Position.PositionCode : null,
                    PositionName = x.Position != null ? x.Position.PositionName : null,
                    x.PrivilegeCode,
                    x.PrivilegeName,
                    x.PrivilegeType,
                    x.ClinicalScope,
                    x.SpecialtyName,
                    x.SubSpecialtyName,
                    x.ProcedureGroup,
                    x.ProcedureName,
                    x.PracticeLocation,
                    x.EffectiveStartDate,
                    x.EffectiveEndDate,
                    x.PrivilegeStatus,
                    x.IsTemporary,
                    x.IsEmergencyPrivilege,
                    x.IsSupervisionRequired,
                    x.SupervisorUserId,
                    SupervisorUserName = x.SupervisorUser != null ? x.SupervisorUser.DisplayName : null,
                    x.GrantedByUserId,
                    GrantedByUserName = x.GrantedByUser != null ? x.GrantedByUser.DisplayName : null,
                    x.GrantedAt,
                    x.GrantNote,
                    x.RejectedByUserId,
                    RejectedByUserName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
                    x.RejectedAt,
                    x.RejectedReason,
                    x.SuspendedByUserId,
                    SuspendedByUserName = x.SuspendedByUser != null ? x.SuspendedByUser.DisplayName : null,
                    x.SuspendedAt,
                    x.SuspensionReason,
                    x.RevokedByUserId,
                    RevokedByUserName = x.RevokedByUser != null ? x.RevokedByUser.DisplayName : null,
                    x.RevokedAt,
                    x.RevokedReason,
                    x.LastReviewDate,
                    x.NextReviewDate,
                    x.SupportingFilePath,
                    x.SupportingFileContentType,
                    x.IsActive,
                    x.Description,
                    x.CreateDateTime
                })
                .ToListAsync();

            var items = rawItems.Select(x => new WorkforceClinicalPrivilegeResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                CredentialLicenseId = x.CredentialLicenseId,
                CredentialLicenseType = x.CredentialLicenseType,
                CredentialLicenseNumber = x.CredentialLicenseNumber,
                DepartmentId = x.DepartmentId,
                DepartmentCode = x.DepartmentCode,
                DepartmentName = x.DepartmentName,
                PositionId = x.PositionId,
                PositionCode = x.PositionCode,
                PositionName = x.PositionName,
                PrivilegeCode = x.PrivilegeCode,
                PrivilegeName = x.PrivilegeName,
                PrivilegeType = x.PrivilegeType,
                ClinicalScope = x.ClinicalScope,
                SpecialtyName = x.SpecialtyName,
                SubSpecialtyName = x.SubSpecialtyName,
                ProcedureGroup = x.ProcedureGroup,
                ProcedureName = x.ProcedureName,
                PracticeLocation = x.PracticeLocation,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                PrivilegeStatus = ResolveRuntimeStatus(x.PrivilegeStatus, x.EffectiveEndDate),
                IsTemporary = x.IsTemporary,
                IsEmergencyPrivilege = x.IsEmergencyPrivilege,
                IsSupervisionRequired = x.IsSupervisionRequired,
                SupervisorUserId = x.SupervisorUserId,
                SupervisorUserName = x.SupervisorUserName,
                GrantedByUserId = x.GrantedByUserId,
                GrantedByUserName = x.GrantedByUserName,
                GrantedAt = x.GrantedAt,
                GrantNote = x.GrantNote,
                RejectedByUserId = x.RejectedByUserId,
                RejectedByUserName = x.RejectedByUserName,
                RejectedAt = x.RejectedAt,
                RejectedReason = x.RejectedReason,
                SuspendedByUserId = x.SuspendedByUserId,
                SuspendedByUserName = x.SuspendedByUserName,
                SuspendedAt = x.SuspendedAt,
                SuspensionReason = x.SuspensionReason,
                RevokedByUserId = x.RevokedByUserId,
                RevokedByUserName = x.RevokedByUserName,
                RevokedAt = x.RevokedAt,
                RevokedReason = x.RevokedReason,
                LastReviewDate = x.LastReviewDate,
                NextReviewDate = x.NextReviewDate,
                SupportingFilePath = x.SupportingFilePath,
                SupportingFileContentType = x.SupportingFileContentType,
                HasSupportingFile = !string.IsNullOrWhiteSpace(x.SupportingFilePath),
                IsExpired = x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today,
                IsCurrentlyValid = x.IsActive &&
                    x.PrivilegeStatus == ClinicalPrivilegeStatus.Active &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today),
                IsActive = x.IsActive,
                Description = x.Description,
                CreateDateTime = x.CreateDateTime
            }).ToList();

            var result = new WorkforceClinicalPrivilegeListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Active),
                PendingApprovalData = items.Count(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.PendingApproval),
                SuspendedData = items.Count(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Suspended),
                RejectedData = items.Count(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Rejected),
                RevokedData = items.Count(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked),
                ExpiredData = items.Count(x => x.IsExpired),
                CurrentlyValidData = items.Count(x => x.IsCurrentlyValid),
                WithCredentialLicenseData = items.Count(x => x.CredentialLicenseId.HasValue),
                WithSupportingFileData = items.Count(x => x.HasSupportingFile),
                Items = items
            };

            return Ok(ApiResponse<WorkforceClinicalPrivilegeListResponse>.Ok(
                result,
                "Data clinical privilege workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Clinical Privilege",
            Description = "Melihat detail clinical privilege workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetClinicalPrivilegeById(Guid workforceProfileId, Guid id)
        {
            var response = await BuildClinicalPrivilegeResponseAsync(id, workforceProfileId);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceClinicalPrivilegeResponse>.Ok(
                response,
                "Detail clinical privilege workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Clinical Privilege",
            Description = "Menambah clinical privilege workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Create")]
        public async Task<IActionResult> CreateClinicalPrivilege(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceClinicalPrivilegeRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = ValidateClinicalPrivilegeRequest(
                request.PrivilegeCode,
                request.PrivilegeName,
                request.PrivilegeType,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                request.IsSupervisionRequired,
                request.SupervisorUserId,
                request.SupportingFile);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data clinical privilege tidak valid."
                ));
            }

            var credentialValidation = await ValidateCredentialRequirementAsync(
                workforceProfileId,
                request.CredentialLicenseId);

            if (!credentialValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    credentialValidation.ErrorMessage ?? "Credential license belum valid."
                ));
            }

            var departmentPositionValidation = await ValidateDepartmentAndPositionAsync(
                request.DepartmentId,
                request.PositionId);

            if (!departmentPositionValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    departmentPositionValidation.ErrorMessage ?? "Department/position tidak valid."
                ));
            }

            var supervisorValidation = await ValidateSupervisorAsync(
                request.IsSupervisionRequired,
                request.SupervisorUserId);

            if (!supervisorValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    supervisorValidation.ErrorMessage ?? "Supervisor tidak valid."
                ));
            }

            var normalizedPrivilegeCode = NormalizeCode(request.PrivilegeCode);

            var duplicate = await _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.PrivilegeCode == normalizedPrivilegeCode &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PrivilegeCode sudah terdaftar pada workforce profile ini."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            string? supportingFilePath = null;
            string? supportingFileContentType = null;

            if (request.SupportingFile != null)
            {
                var savedFile = await SaveSupportingFileAsync(workforceProfileId, request.SupportingFile);
                supportingFilePath = savedFile.FilePath;
                supportingFileContentType = savedFile.ContentType;
            }

            var entity = new WfpClinicalPrivilege
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CredentialLicenseId = NormalizeNullableGuid(request.CredentialLicenseId),
                DepartmentId = NormalizeNullableGuid(request.DepartmentId),
                PositionId = NormalizeNullableGuid(request.PositionId),
                PrivilegeCode = normalizedPrivilegeCode,
                PrivilegeName = request.PrivilegeName.Trim(),
                PrivilegeType = request.PrivilegeType,
                ClinicalScope = NormalizeNullableText(request.ClinicalScope),
                SpecialtyName = NormalizeNullableText(request.SpecialtyName),
                SubSpecialtyName = NormalizeNullableText(request.SubSpecialtyName),
                ProcedureGroup = NormalizeNullableText(request.ProcedureGroup),
                ProcedureName = NormalizeNullableText(request.ProcedureName),
                PracticeLocation = NormalizeNullableText(request.PracticeLocation),
                EffectiveStartDate = request.EffectiveStartDate.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                PrivilegeStatus = ClinicalPrivilegeStatus.PendingApproval,
                IsTemporary = request.IsTemporary,
                IsEmergencyPrivilege = request.IsEmergencyPrivilege,
                IsSupervisionRequired = request.IsSupervisionRequired,
                SupervisorUserId = NormalizeNullableGuid(request.SupervisorUserId),
                LastReviewDate = request.LastReviewDate?.Date,
                NextReviewDate = request.NextReviewDate?.Date,
                SupportingFilePath = supportingFilePath,
                SupportingFileContentType = supportingFileContentType,
                IsActive = request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpClinicalPrivilege>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildClinicalPrivilegeResponseAsync(entity.Id, workforceProfileId);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceClinicalPrivilege.CreateClinicalPrivilege",
                "Clinical privilege workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.PrivilegeCode,
                    entity.PrivilegeName,
                    entity.PrivilegeStatus
                }
            );

            return Ok(ApiResponse<WorkforceClinicalPrivilegeResponse>.Ok(
                response!,
                "Clinical privilege workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Clinical Privilege",
            Description = "Mengubah clinical privilege workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> UpdateClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceClinicalPrivilegeRequest request)
        {
            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (entity.PrivilegeStatus == ClinicalPrivilegeStatus.Active ||
                entity.PrivilegeStatus == ClinicalPrivilegeStatus.Suspended ||
                entity.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege active/suspended/revoked tidak boleh diubah langsung. Gunakan lifecycle action."
                ));
            }

            var validation = ValidateClinicalPrivilegeRequest(
                request.PrivilegeCode,
                request.PrivilegeName,
                request.PrivilegeType,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                request.IsSupervisionRequired,
                request.SupervisorUserId,
                request.SupportingFile);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data clinical privilege tidak valid."
                ));
            }

            var credentialValidation = await ValidateCredentialRequirementAsync(
                workforceProfileId,
                request.CredentialLicenseId);

            if (!credentialValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    credentialValidation.ErrorMessage ?? "Credential license belum valid."
                ));
            }

            var departmentPositionValidation = await ValidateDepartmentAndPositionAsync(
                request.DepartmentId,
                request.PositionId);

            if (!departmentPositionValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    departmentPositionValidation.ErrorMessage ?? "Department/position tidak valid."
                ));
            }

            var supervisorValidation = await ValidateSupervisorAsync(
                request.IsSupervisionRequired,
                request.SupervisorUserId);

            if (!supervisorValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    supervisorValidation.ErrorMessage ?? "Supervisor tidak valid."
                ));
            }

            var normalizedPrivilegeCode = NormalizeCode(request.PrivilegeCode);

            var duplicate = await _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id != id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    x.PrivilegeCode == normalizedPrivilegeCode &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PrivilegeCode sudah terdaftar pada workforce profile ini."
                ));
            }

            if (request.ReplaceExistingFile && request.SupportingFile == null)
            {
                entity.SupportingFilePath = null;
                entity.SupportingFileContentType = null;
            }

            if (request.SupportingFile != null)
            {
                var savedFile = await SaveSupportingFileAsync(workforceProfileId, request.SupportingFile);
                entity.SupportingFilePath = savedFile.FilePath;
                entity.SupportingFileContentType = savedFile.ContentType;
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.CredentialLicenseId = NormalizeNullableGuid(request.CredentialLicenseId);
            entity.DepartmentId = NormalizeNullableGuid(request.DepartmentId);
            entity.PositionId = NormalizeNullableGuid(request.PositionId);
            entity.PrivilegeCode = normalizedPrivilegeCode;
            entity.PrivilegeName = request.PrivilegeName.Trim();
            entity.PrivilegeType = request.PrivilegeType;
            entity.ClinicalScope = NormalizeNullableText(request.ClinicalScope);
            entity.SpecialtyName = NormalizeNullableText(request.SpecialtyName);
            entity.SubSpecialtyName = NormalizeNullableText(request.SubSpecialtyName);
            entity.ProcedureGroup = NormalizeNullableText(request.ProcedureGroup);
            entity.ProcedureName = NormalizeNullableText(request.ProcedureName);
            entity.PracticeLocation = NormalizeNullableText(request.PracticeLocation);
            entity.EffectiveStartDate = request.EffectiveStartDate.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.IsTemporary = request.IsTemporary;
            entity.IsEmergencyPrivilege = request.IsEmergencyPrivilege;
            entity.IsSupervisionRequired = request.IsSupervisionRequired;
            entity.SupervisorUserId = NormalizeNullableGuid(request.SupervisorUserId);
            entity.LastReviewDate = request.LastReviewDate?.Date;
            entity.NextReviewDate = request.NextReviewDate?.Date;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildClinicalPrivilegeResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeResponse>.Ok(
                response!,
                "Clinical privilege workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Clinical Privilege",
            Description = "Mengubah status aktif clinical privilege workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> UpdateClinicalPrivilegeStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceClinicalPrivilegeStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (request.IsActive &&
                (entity.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked ||
                 entity.PrivilegeStatus == ClinicalPrivilegeStatus.Rejected))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege rejected/revoked tidak boleh diaktifkan ulang."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildClinicalPrivilegeResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeResponse>.Ok(
                response!,
                "Status clinical privilege workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/grant")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Clinical Privilege",
            Description = "Grant clinical privilege workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> GrantClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] GrantWorkforceClinicalPrivilegeRequest request)
        {
            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (entity.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked ||
                entity.PrivilegeStatus == ClinicalPrivilegeStatus.Rejected)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege rejected/revoked tidak bisa di-grant. Buat pengajuan baru."
                ));
            }

            if (entity.EffectiveEndDate.HasValue && entity.EffectiveEndDate.Value.Date < DateTime.UtcNow.Date)
            {
                entity.PrivilegeStatus = ClinicalPrivilegeStatus.Expired;
                entity.IsActive = false;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = GetCurrentUserId();
                await _dbContext.SaveChangesAsync();

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege sudah expired dan tidak bisa di-grant."
                ));
            }

            var credentialValidation = await ValidateCredentialRequirementAsync(
                workforceProfileId,
                entity.CredentialLicenseId);

            if (!credentialValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    credentialValidation.ErrorMessage ?? "Credential license belum valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.PrivilegeStatus = ClinicalPrivilegeStatus.Active;
            entity.IsActive = true;
            entity.GrantedByUserId = actorUserId;
            entity.GrantedAt = now;
            entity.GrantNote = NormalizeNullableText(request.GrantNote);
            entity.RejectedByUserId = null;
            entity.RejectedAt = null;
            entity.RejectedReason = null;
            entity.SuspendedByUserId = null;
            entity.SuspendedAt = null;
            entity.SuspensionReason = null;
            entity.RevokedByUserId = null;
            entity.RevokedAt = null;
            entity.RevokedReason = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (request.NextReviewDate.HasValue)
            {
                entity.NextReviewDate = request.NextReviewDate.Value.Date;
            }

            await _dbContext.SaveChangesAsync();

            var response = await BuildClinicalPrivilegeResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeResponse>.Ok(
                response!,
                "Clinical privilege workforce berhasil di-grant."
            ));
        }

        [HttpPatch("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Clinical Privilege",
            Description = "Reject clinical privilege workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> RejectClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RejectWorkforceClinicalPrivilegeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan reject wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (entity.PrivilegeStatus == ClinicalPrivilegeStatus.Active)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege active tidak bisa di-reject. Gunakan suspend atau revoke."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.PrivilegeStatus = ClinicalPrivilegeStatus.Rejected;
            entity.IsActive = false;
            entity.RejectedByUserId = actorUserId;
            entity.RejectedAt = now;
            entity.RejectedReason = request.RejectedReason.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildClinicalPrivilegeResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeResponse>.Ok(
                response!,
                "Clinical privilege workforce berhasil di-reject."
            ));
        }

        [HttpPatch("{id:guid}/suspend")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Clinical Privilege",
            Description = "Suspend clinical privilege workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> SuspendClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SuspendWorkforceClinicalPrivilegeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SuspensionReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan suspend wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (entity.PrivilegeStatus != ClinicalPrivilegeStatus.Active)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya clinical privilege active yang bisa di-suspend."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.PrivilegeStatus = ClinicalPrivilegeStatus.Suspended;
            entity.IsActive = false;
            entity.SuspendedByUserId = actorUserId;
            entity.SuspendedAt = now;
            entity.SuspensionReason = request.SuspensionReason.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildClinicalPrivilegeResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeResponse>.Ok(
                response!,
                "Clinical privilege workforce berhasil di-suspend."
            ));
        }

        [HttpPatch("{id:guid}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Clinical Privilege",
            Description = "Revoke clinical privilege workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> RevokeClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RevokeWorkforceClinicalPrivilegeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RevokedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan revoke wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (entity.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege sudah revoked."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.PrivilegeStatus = ClinicalPrivilegeStatus.Revoked;
            entity.IsActive = false;
            entity.RevokedByUserId = actorUserId;
            entity.RevokedAt = now;
            entity.RevokedReason = request.RevokedReason.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildClinicalPrivilegeResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeResponse>.Ok(
                response!,
                "Clinical privilege workforce berhasil di-revoke."
            ));
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Clinical Privilege",
            Description = "Download file clinical privilege workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> DownloadClinicalPrivilegeFile(Guid workforceProfileId, Guid id)
        {
            var privilege = await _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (privilege == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(privilege.SupportingFilePath))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege belum memiliki file pendukung."
                ));
            }

            var relativePath = privilege.SupportingFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var physicalPath = Path.Combine(rootPath, relativePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File clinical privilege tidak ditemukan di server."
                ));
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = privilege.SupportingFileContentType ?? "application/octet-stream";
            }

            var fileName = Path.GetFileName(physicalPath);
            var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);

            return File(bytes, contentType, fileName);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Clinical Privilege",
            Description = "Menghapus clinical privilege workforce profile",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceClinicalPrivilege", "Delete")]
        public async Task<IActionResult> DeleteClinicalPrivilege(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (entity.PrivilegeStatus == ClinicalPrivilegeStatus.Active ||
                entity.PrivilegeStatus == ClinicalPrivilegeStatus.Suspended ||
                entity.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege active/suspended/revoked tidak boleh dihapus. Gunakan lifecycle action agar audit trail aman."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Clinical privilege workforce berhasil dihapus."
            ));
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<WorkforceClinicalPrivilegeResponse?> BuildClinicalPrivilegeResponseAsync(
            Guid id,
            Guid workforceProfileId)
        {
            var today = DateTime.UtcNow.Date;

            var item = await _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    x.CredentialLicenseId,
                    CredentialLicenseType = x.CredentialLicense != null ? x.CredentialLicense.LicenseType : null,
                    CredentialLicenseNumber = x.CredentialLicense != null ? x.CredentialLicense.LicenseNumber : null,
                    x.DepartmentId,
                    DepartmentCode = x.Department != null ? x.Department.DepartmentCode : null,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    x.PositionId,
                    PositionCode = x.Position != null ? x.Position.PositionCode : null,
                    PositionName = x.Position != null ? x.Position.PositionName : null,
                    x.PrivilegeCode,
                    x.PrivilegeName,
                    x.PrivilegeType,
                    x.ClinicalScope,
                    x.SpecialtyName,
                    x.SubSpecialtyName,
                    x.ProcedureGroup,
                    x.ProcedureName,
                    x.PracticeLocation,
                    x.EffectiveStartDate,
                    x.EffectiveEndDate,
                    x.PrivilegeStatus,
                    x.IsTemporary,
                    x.IsEmergencyPrivilege,
                    x.IsSupervisionRequired,
                    x.SupervisorUserId,
                    SupervisorUserName = x.SupervisorUser != null ? x.SupervisorUser.DisplayName : null,
                    x.GrantedByUserId,
                    GrantedByUserName = x.GrantedByUser != null ? x.GrantedByUser.DisplayName : null,
                    x.GrantedAt,
                    x.GrantNote,
                    x.RejectedByUserId,
                    RejectedByUserName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
                    x.RejectedAt,
                    x.RejectedReason,
                    x.SuspendedByUserId,
                    SuspendedByUserName = x.SuspendedByUser != null ? x.SuspendedByUser.DisplayName : null,
                    x.SuspendedAt,
                    x.SuspensionReason,
                    x.RevokedByUserId,
                    RevokedByUserName = x.RevokedByUser != null ? x.RevokedByUser.DisplayName : null,
                    x.RevokedAt,
                    x.RevokedReason,
                    x.LastReviewDate,
                    x.NextReviewDate,
                    x.SupportingFilePath,
                    x.SupportingFileContentType,
                    x.IsActive,
                    x.Description,
                    x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return null;
            }

            return new WorkforceClinicalPrivilegeResponse
            {
                Id = item.Id,
                WorkforceProfileId = item.WorkforceProfileId,
                ProfileCode = item.ProfileCode,
                DisplayName = item.DisplayName,
                CredentialLicenseId = item.CredentialLicenseId,
                CredentialLicenseType = item.CredentialLicenseType,
                CredentialLicenseNumber = item.CredentialLicenseNumber,
                DepartmentId = item.DepartmentId,
                DepartmentCode = item.DepartmentCode,
                DepartmentName = item.DepartmentName,
                PositionId = item.PositionId,
                PositionCode = item.PositionCode,
                PositionName = item.PositionName,
                PrivilegeCode = item.PrivilegeCode,
                PrivilegeName = item.PrivilegeName,
                PrivilegeType = item.PrivilegeType,
                ClinicalScope = item.ClinicalScope,
                SpecialtyName = item.SpecialtyName,
                SubSpecialtyName = item.SubSpecialtyName,
                ProcedureGroup = item.ProcedureGroup,
                ProcedureName = item.ProcedureName,
                PracticeLocation = item.PracticeLocation,
                EffectiveStartDate = item.EffectiveStartDate,
                EffectiveEndDate = item.EffectiveEndDate,
                PrivilegeStatus = ResolveRuntimeStatus(item.PrivilegeStatus, item.EffectiveEndDate),
                IsTemporary = item.IsTemporary,
                IsEmergencyPrivilege = item.IsEmergencyPrivilege,
                IsSupervisionRequired = item.IsSupervisionRequired,
                SupervisorUserId = item.SupervisorUserId,
                SupervisorUserName = item.SupervisorUserName,
                GrantedByUserId = item.GrantedByUserId,
                GrantedByUserName = item.GrantedByUserName,
                GrantedAt = item.GrantedAt,
                GrantNote = item.GrantNote,
                RejectedByUserId = item.RejectedByUserId,
                RejectedByUserName = item.RejectedByUserName,
                RejectedAt = item.RejectedAt,
                RejectedReason = item.RejectedReason,
                SuspendedByUserId = item.SuspendedByUserId,
                SuspendedByUserName = item.SuspendedByUserName,
                SuspendedAt = item.SuspendedAt,
                SuspensionReason = item.SuspensionReason,
                RevokedByUserId = item.RevokedByUserId,
                RevokedByUserName = item.RevokedByUserName,
                RevokedAt = item.RevokedAt,
                RevokedReason = item.RevokedReason,
                LastReviewDate = item.LastReviewDate,
                NextReviewDate = item.NextReviewDate,
                SupportingFilePath = item.SupportingFilePath,
                SupportingFileContentType = item.SupportingFileContentType,
                HasSupportingFile = !string.IsNullOrWhiteSpace(item.SupportingFilePath),
                IsExpired = item.EffectiveEndDate.HasValue && item.EffectiveEndDate.Value.Date < today,
                IsCurrentlyValid = item.IsActive &&
                    item.PrivilegeStatus == ClinicalPrivilegeStatus.Active &&
                    (!item.EffectiveEndDate.HasValue || item.EffectiveEndDate.Value.Date >= today),
                IsActive = item.IsActive,
                Description = item.Description,
                CreateDateTime = item.CreateDateTime
            };
        }

        private static (bool IsValid, string? ErrorMessage) ValidateClinicalPrivilegeRequest(
            string privilegeCode,
            string privilegeName,
            ClinicalPrivilegeType privilegeType,
            DateTime effectiveStartDate,
            DateTime? effectiveEndDate,
            bool isSupervisionRequired,
            Guid? supervisorUserId,
            IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(privilegeCode))
            {
                return (false, "PrivilegeCode wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(privilegeName))
            {
                return (false, "PrivilegeName wajib diisi.");
            }

            if (privilegeType == ClinicalPrivilegeType.Unknown)
            {
                return (false, "PrivilegeType wajib dipilih.");
            }

            if (effectiveStartDate == default)
            {
                return (false, "EffectiveStartDate wajib diisi.");
            }

            if (effectiveEndDate.HasValue && effectiveStartDate.Date > effectiveEndDate.Value.Date)
            {
                return (false, "EffectiveStartDate tidak boleh lebih besar dari EffectiveEndDate.");
            }

            if (isSupervisionRequired && (!supervisorUserId.HasValue || supervisorUserId.Value == Guid.Empty))
            {
                return (false, "SupervisorUserId wajib diisi jika IsSupervisionRequired = true.");
            }

            if (file != null)
            {
                if (file.Length <= 0)
                {
                    return (false, "File clinical privilege kosong.");
                }

                if (file.Length > MaxFileSizeBytes)
                {
                    return (false, "Ukuran file clinical privilege maksimal 10 MB.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                {
                    return (false, "Format file tidak didukung. Gunakan PDF, JPG, JPEG, PNG, DOC, DOCX, XLS, atau XLSX.");
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCredentialRequirementAsync(
            Guid workforceProfileId,
            Guid? credentialLicenseId)
        {
            var today = DateTime.UtcNow.Date;

            var query = _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    x.IsVerified &&
                    x.VerificationStatus == CredentialVerificationStatus.Verified &&
                    x.ExpiredDate >= today &&
                    !x.IsDelete);

            if (credentialLicenseId.HasValue && credentialLicenseId.Value != Guid.Empty)
            {
                var validSelectedLicense = await query.AnyAsync(x => x.Id == credentialLicenseId.Value);

                return validSelectedLicense
                    ? (true, null)
                    : (false, "CredentialLicenseId tidak ditemukan, tidak verified, nonaktif, atau sudah expired.");
            }

            var hasAnyValidLicense = await query.AnyAsync();

            return hasAnyValidLicense
                ? (true, null)
                : (false, "Minimal harus ada credential license yang verified, aktif, dan belum expired sebelum membuat clinical privilege.");
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDepartmentAndPositionAsync(
            Guid? departmentId,
            Guid? positionId)
        {
            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                var departmentExists = await _dbContext.Set<MstDepartment>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == departmentId.Value && !x.IsDelete);

                if (!departmentExists)
                {
                    return (false, "Department tidak ditemukan.");
                }
            }

            if (positionId.HasValue && positionId.Value != Guid.Empty)
            {
                var positionExists = await _dbContext.Set<MstPosition>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == positionId.Value && !x.IsDelete);

                if (!positionExists)
                {
                    return (false, "Position tidak ditemukan.");
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateSupervisorAsync(
            bool isSupervisionRequired,
            Guid? supervisorUserId)
        {
            if (!supervisorUserId.HasValue || supervisorUserId.Value == Guid.Empty)
            {
                return isSupervisionRequired
                    ? (false, "SupervisorUserId wajib diisi jika supervisi diperlukan.")
                    : (true, null);
            }

            var exists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == supervisorUserId.Value && x.IsActive);

            return exists
                ? (true, null)
                : (false, "Supervisor user tidak ditemukan atau tidak aktif.");
        }

        private async Task<(string FilePath, string? ContentType)> SaveSupportingFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var relativeFolder = Path.Combine("uploads", "workforce-clinical-privileges", workforceProfileId.ToString());
            var physicalFolder = Path.Combine(rootPath, relativeFolder);

            if (!Directory.Exists(physicalFolder))
            {
                Directory.CreateDirectory(physicalFolder);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(physicalFolder, fileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var filePath = "/" + Path.Combine(relativeFolder, fileName).Replace(Path.DirectorySeparatorChar, '/');

            return (filePath, file.ContentType);
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

        private static ClinicalPrivilegeStatus ResolveRuntimeStatus(
            ClinicalPrivilegeStatus status,
            DateTime? effectiveEndDate)
        {
            if (status == ClinicalPrivilegeStatus.Revoked ||
                status == ClinicalPrivilegeStatus.Rejected ||
                status == ClinicalPrivilegeStatus.Suspended)
            {
                return status;
            }

            if (effectiveEndDate.HasValue && effectiveEndDate.Value.Date < DateTime.UtcNow.Date)
            {
                return ClinicalPrivilegeStatus.Expired;
            }

            return status;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
        }

        private static string NormalizeCode(string value)
        {
            return value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
