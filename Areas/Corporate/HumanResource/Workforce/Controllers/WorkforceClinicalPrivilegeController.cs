using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceClinicalPrivilegePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceClinicalPrivilegeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
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
        private const string CodePrefix = "CLP-RSMMC-";
        private const int CodeNumberLength = 5;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
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
        private readonly IConfiguration _configuration;

        public WorkforceClinicalPrivilegeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
            _configuration = configuration;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Melihat metadata filter clinical privilege workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetFilterMetadata(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var result = new WorkforceClinicalPrivilegeFilterMetadataResponse
            {
                DefaultFilter = new WorkforceClinicalPrivilegeDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceClinicalPrivilegeCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkforceClinicalPrivilegeSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "privilegeCode", Label = "Kode privilege" },
                    new() { Value = "privilegeName", Label = "Nama privilege" },
                    new() { Value = "privilegeType", Label = "Tipe privilege" },
                    new() { Value = "clinicalScope", Label = "Scope klinis" },
                    new() { Value = "departmentName", Label = "Department" },
                    new() { Value = "effectiveStartDate", Label = "Mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Akhir berlaku" },
                    new() { Value = "privilegeStatus", Label = "Status privilege" },
                    new() { Value = "isTemporary", Label = "Temporary" },
                    new() { Value = "isEmergencyPrivilege", Label = "Emergency" },
                    new() { Value = "isSupervisionRequired", Label = "Butuh supervisi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                PrivilegeTypeOptions = BuildPrivilegeTypeOptions(),
                PrivilegeStatusOptions = BuildPrivilegeStatusOptions(),
                ClinicalScopeOptions = BuildClinicalScopeOptions()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceClinicalPrivilege.GetFilterMetadata",
                "Mengambil metadata filter clinical privilege workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceClinicalPrivilegeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter clinical privilege workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Melihat ringkasan clinical privilege workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = AppDateTimeHelper.OperationalDate();
            var query = BuildBaseQuery(workforceProfileId);

            var result = new WorkforceClinicalPrivilegeSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalClinicalPrivilege = await query.CountAsync(),
                ActiveClinicalPrivilege = await query.CountAsync(x => x.IsActive),
                InactiveClinicalPrivilege = await query.CountAsync(x => !x.IsActive),
                PendingApprovalClinicalPrivilege = await query.CountAsync(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.PendingApproval),
                GrantedClinicalPrivilege = await query.CountAsync(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Active),
                RejectedClinicalPrivilege = await query.CountAsync(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Rejected),
                SuspendedClinicalPrivilege = await query.CountAsync(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Suspended),
                RevokedClinicalPrivilege = await query.CountAsync(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked),
                ExpiredClinicalPrivilege = await query.CountAsync(x => x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today),
                CurrentlyValidClinicalPrivilege = await query.CountAsync(x => x.IsActive && x.PrivilegeStatus == ClinicalPrivilegeStatus.Active && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)),
                TemporaryClinicalPrivilege = await query.CountAsync(x => x.IsTemporary),
                EmergencyClinicalPrivilege = await query.CountAsync(x => x.IsEmergencyPrivilege),
                SupervisionRequiredClinicalPrivilege = await query.CountAsync(x => x.IsSupervisionRequired),
                ClinicalPrivilegeWithCredentialLicense = await query.CountAsync(x => x.CredentialLicenseId.HasValue),
                ClinicalPrivilegeWithSupportingFile = await query.CountAsync(x => x.SupportingFilePath != null && x.SupportingFilePath != string.Empty)
            };

            return Ok(ApiResponse<WorkforceClinicalPrivilegeSummaryResponse>.Ok(
                result,
                "Ringkasan clinical privilege workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceClinicalPrivilegePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Melihat clinical privilege workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetClinicalPrivileges(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] ClinicalPrivilegeType? privilegeType,
            [FromQuery] ClinicalPrivilegeStatus? privilegeStatus,
            [FromQuery] Guid? credentialLicenseId,
            [FromQuery] Guid? departmentId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, privilegeType, privilegeStatus, credentialLicenseId, departmentId, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, profile, actorNames))
                .ToList();

            var result = new ResponseWorkforceClinicalPrivilegePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceClinicalPrivilegePagedResult>.Ok(
                result,
                "Data clinical privilege workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Melihat pilihan clinical privilege workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] ClinicalPrivilegeType? privilegeType,
            [FromQuery] ClinicalPrivilegeStatus? privilegeStatus,
            [FromQuery] Guid? credentialLicenseId,
            [FromQuery] Guid? departmentId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profileExists = await ProfileExistsAsync(workforceProfileId);

            if (!profileExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyStandardFilter(query, privilegeType, privilegeStatus, credentialLicenseId, departmentId, onlyActive ? true : null, search);

            var totalData = await query.CountAsync();
            var today = AppDateTimeHelper.OperationalDate();

            var items = await query
                .OrderByDescending(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Active)
                .ThenBy(x => x.PrivilegeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceClinicalPrivilegeOptionResponse
                {
                    Id = x.Id,
                    PrivilegeCode = x.PrivilegeCode,
                    PrivilegeName = x.PrivilegeName,
                    PrivilegeType = x.PrivilegeType,
                    ClinicalScope = x.ClinicalScope,
                    SpecialtyName = x.SpecialtyName,
                    ProcedureGroup = x.ProcedureGroup,
                    ProcedureName = x.ProcedureName,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    PrivilegeStatus = x.PrivilegeStatus,
                    HasSupportingFile = x.SupportingFilePath != null && x.SupportingFilePath != string.Empty,
                    IsExpired = x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today,
                    IsCurrentlyValid = x.IsActive && x.PrivilegeStatus == ClinicalPrivilegeStatus.Active && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)
                })
                .ToListAsync();

            var result = new WorkforceClinicalPrivilegeOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceClinicalPrivilegeOptionPagedResponse>.Ok(
                result,
                "Data pilihan clinical privilege workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Melihat detail clinical privilege workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetClinicalPrivilegeById(Guid workforceProfileId, Guid id)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAuditResponse(data);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>.Ok(
                data,
                "Detail clinical privilege workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Clinical Privilege", Description = "Menambah clinical privilege workforce profile", AccessType = AccessTypes.Create, SortOrder = 2)]
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

            var validation = await ValidateRequestAsync(workforceProfileId, null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data clinical privilege tidak valid."
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
                PrivilegeCode = await GeneratePrivilegeCodeAsync(),
                PrivilegeName = NormalizeRequiredText(request.PrivilegeName),
                PrivilegeType = request.PrivilegeType,
                ClinicalScope = NormalizeClinicalScope(request.ClinicalScope),
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

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceClinicalPrivilege.CreateClinicalPrivilege",
                "Clinical privilege workforce berhasil dibuat.",
                new { workforceProfileId, entity.Id, entity.PrivilegeCode, entity.PrivilegeName, entity.PrivilegeStatus }
            );

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAuditResponse(data);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>.Ok(
                data,
                "Clinical privilege workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Clinical Privilege", Description = "Mengubah clinical privilege workforce profile", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> UpdateClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceClinicalPrivilegeRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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

            var validation = await ValidateRequestAsync(workforceProfileId, id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data clinical privilege tidak valid."
                ));
            }

            if (request.ReplaceExistingFile && request.SupportingFile == null)
            {
                DeletePhysicalFileIfExists(entity.SupportingFilePath);
                entity.SupportingFilePath = null;
                entity.SupportingFileContentType = null;
            }

            if (request.SupportingFile != null)
            {
                if (request.ReplaceExistingFile)
                {
                    DeletePhysicalFileIfExists(entity.SupportingFilePath);
                }

                if (request.ReplaceExistingFile || string.IsNullOrWhiteSpace(entity.SupportingFilePath))
                {
                    var savedFile = await SaveSupportingFileAsync(workforceProfileId, request.SupportingFile);
                    entity.SupportingFilePath = savedFile.FilePath;
                    entity.SupportingFileContentType = savedFile.ContentType;
                }
            }

            entity.CredentialLicenseId = NormalizeNullableGuid(request.CredentialLicenseId);
            entity.DepartmentId = NormalizeNullableGuid(request.DepartmentId);
            entity.PositionId = NormalizeNullableGuid(request.PositionId);
            entity.PrivilegeName = NormalizeRequiredText(request.PrivilegeName);
            entity.PrivilegeType = request.PrivilegeType;
            entity.ClinicalScope = NormalizeClinicalScope(request.ClinicalScope);
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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAuditResponse(data);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>.Ok(
                data,
                "Clinical privilege workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Clinical Privilege", Description = "Mengubah status aktif clinical privilege workforce profile", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> UpdateClinicalPrivilegeStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceClinicalPrivilegeStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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

            return Ok(ApiResponse<object>.Ok(null, "Status clinical privilege workforce berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/grant")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Grant Workforce Clinical Privilege", Description = "Grant clinical privilege workforce profile", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> GrantClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] GrantWorkforceClinicalPrivilegeRequest request)
        {
            var result = await ChangeLifecycleAsync(
                workforceProfileId,
                id,
                targetStatus: ClinicalPrivilegeStatus.Active,
                note: request.GrantNote,
                requiredReason: false,
                actionName: "grant",
                nextReviewDate: request.NextReviewDate);

            return result;
        }

        [HttpPatch("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Reject Workforce Clinical Privilege", Description = "Reject clinical privilege workforce profile", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> RejectClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RejectWorkforceClinicalPrivilegeRequest request)
        {
            return await ChangeLifecycleAsync(
                workforceProfileId,
                id,
                ClinicalPrivilegeStatus.Rejected,
                request.RejectedReason,
                requiredReason: true,
                actionName: "reject");
        }

        [HttpPatch("{id:guid}/suspend")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Suspend Workforce Clinical Privilege", Description = "Suspend clinical privilege workforce profile", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> SuspendClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SuspendWorkforceClinicalPrivilegeRequest request)
        {
            return await ChangeLifecycleAsync(
                workforceProfileId,
                id,
                ClinicalPrivilegeStatus.Suspended,
                request.SuspensionReason,
                requiredReason: true,
                actionName: "suspend");
        }

        [HttpPatch("{id:guid}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Revoke Workforce Clinical Privilege", Description = "Revoke clinical privilege workforce profile", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> RevokeClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RevokeWorkforceClinicalPrivilegeRequest request)
        {
            return await ChangeLifecycleAsync(
                workforceProfileId,
                id,
                ClinicalPrivilegeStatus.Revoked,
                request.RevokedReason,
                requiredReason: true,
                actionName: "revoke");
        }

        [HttpGet("{id:guid}/preview")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Preview file clinical privilege workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> PreviewClinicalPrivilegeFile(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetClinicalPrivilegeFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File clinical privilege workforce tidak ditemukan."
                ));
            }

            Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = fileValidation.FileName
            }.ToString();

            var stream = new FileStream(
                fileValidation.PhysicalPath!,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            return File(stream, fileValidation.ContentType!, enableRangeProcessing: true);
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Download file clinical privilege workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> DownloadClinicalPrivilegeFile(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetClinicalPrivilegeFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File clinical privilege workforce tidak ditemukan."
                ));
            }

            var stream = new FileStream(
                fileValidation.PhysicalPath!,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            return File(
                stream,
                fileValidation.ContentType!,
                fileValidation.DownloadName,
                enableRangeProcessing: true
            );
        }

        [HttpDelete("{id:guid}/file")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Clinical Privilege File", Description = "Menghapus file clinical privilege workforce", AccessType = AccessTypes.Delete, SortOrder = 9)]
        [AccessPermission("WorkforceClinicalPrivilege", "Delete")]
        public async Task<IActionResult> DeleteClinicalPrivilegeFile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] DeleteWorkforceClinicalPrivilegeFileRequest? request = null)
        {
            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (request?.DeletePhysicalFile ?? true)
            {
                DeletePhysicalFileIfExists(entity.SupportingFilePath);
            }

            entity.SupportingFilePath = null;
            entity.SupportingFileContentType = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "File clinical privilege workforce berhasil dihapus."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Clinical Privilege", Description = "Menghapus clinical privilege workforce profile", AccessType = AccessTypes.Delete, SortOrder = 10)]
        [AccessPermission("WorkforceClinicalPrivilege", "Delete")]
        public async Task<IActionResult> DeleteClinicalPrivilege(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Clinical privilege workforce berhasil dihapus."));
        }

        private IQueryable<WfpClinicalPrivilege> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.CredentialLicense)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.SupervisorUser)
                .Include(x => x.GrantedByUser)
                .Include(x => x.RejectedByUser)
                .Include(x => x.SuspendedByUser)
                .Include(x => x.RevokedByUser)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpClinicalPrivilege> ApplyDateFilter(
            IQueryable<WfpClinicalPrivilege> query,
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

        private static IQueryable<WfpClinicalPrivilege> ApplyStandardFilter(
            IQueryable<WfpClinicalPrivilege> query,
            ClinicalPrivilegeType? privilegeType,
            ClinicalPrivilegeStatus? privilegeStatus,
            Guid? credentialLicenseId,
            Guid? departmentId,
            bool? isActive,
            string? search)
        {
            if (privilegeType.HasValue && privilegeType.Value != ClinicalPrivilegeType.Unknown)
                query = query.Where(x => x.PrivilegeType == privilegeType.Value);

            if (privilegeStatus.HasValue)
                query = query.Where(x => x.PrivilegeStatus == privilegeStatus.Value);

            if (credentialLicenseId.HasValue && credentialLicenseId.Value != Guid.Empty)
                query = query.Where(x => x.CredentialLicenseId == credentialLicenseId.Value);

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
                query = query.Where(x => x.DepartmentId == departmentId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PrivilegeCode.ToLower().Contains(keyword) ||
                    x.PrivilegeName.ToLower().Contains(keyword) ||
                    (x.ClinicalScope != null && x.ClinicalScope.ToLower().Contains(keyword)) ||
                    (x.SpecialtyName != null && x.SpecialtyName.ToLower().Contains(keyword)) ||
                    (x.SubSpecialtyName != null && x.SubSpecialtyName.ToLower().Contains(keyword)) ||
                    (x.ProcedureGroup != null && x.ProcedureGroup.ToLower().Contains(keyword)) ||
                    (x.ProcedureName != null && x.ProcedureName.ToLower().Contains(keyword)) ||
                    (x.PracticeLocation != null && x.PracticeLocation.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.CredentialLicense != null && x.CredentialLicense.LicenseNumber != null && x.CredentialLicense.LicenseNumber.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.PositionName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpClinicalPrivilege> ApplySorting(
            IQueryable<WfpClinicalPrivilege> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "privilegecode" => isDescending ? query.OrderByDescending(x => x.PrivilegeCode) : query.OrderBy(x => x.PrivilegeCode),
                "privilegename" => isDescending ? query.OrderByDescending(x => x.PrivilegeName) : query.OrderBy(x => x.PrivilegeName),
                "privilegetype" => isDescending ? query.OrderByDescending(x => x.PrivilegeType).ThenBy(x => x.PrivilegeName) : query.OrderBy(x => x.PrivilegeType).ThenBy(x => x.PrivilegeName),
                "clinicalscope" => isDescending ? query.OrderByDescending(x => x.ClinicalScope).ThenBy(x => x.PrivilegeName) : query.OrderBy(x => x.ClinicalScope).ThenBy(x => x.PrivilegeName),
                "departmentname" => isDescending
                    ? query.OrderByDescending(x => x.Department != null ? x.Department.DepartmentName : string.Empty).ThenBy(x => x.PrivilegeName)
                    : query.OrderBy(x => x.Department != null ? x.Department.DepartmentName : string.Empty).ThenBy(x => x.PrivilegeName),
                "effectivestartdate" => isDescending ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDescending ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "privilegestatus" => isDescending ? query.OrderByDescending(x => x.PrivilegeStatus).ThenBy(x => x.PrivilegeName) : query.OrderBy(x => x.PrivilegeStatus).ThenBy(x => x.PrivilegeName),
                "istemporary" => isDescending ? query.OrderByDescending(x => x.IsTemporary).ThenBy(x => x.PrivilegeName) : query.OrderBy(x => x.IsTemporary).ThenBy(x => x.PrivilegeName),
                "isemergencyprivilege" => isDescending ? query.OrderByDescending(x => x.IsEmergencyPrivilege).ThenBy(x => x.PrivilegeName) : query.OrderBy(x => x.IsEmergencyPrivilege).ThenBy(x => x.PrivilegeName),
                "issupervisionrequired" => isDescending ? query.OrderByDescending(x => x.IsSupervisionRequired).ThenBy(x => x.PrivilegeName) : query.OrderBy(x => x.IsSupervisionRequired).ThenBy(x => x.PrivilegeName),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.PrivilegeName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.PrivilegeName),
                _ => isDescending ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.PrivilegeName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.PrivilegeName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWorkforceClinicalPrivilegeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PrivilegeName))
                return (false, "PrivilegeName wajib diisi.");

            if (request.PrivilegeType == ClinicalPrivilegeType.Unknown)
                return (false, "PrivilegeType wajib dipilih.");

            if (request.ClinicalScope.HasValue && request.ClinicalScope.Value == WorkforceClinicalScope.Unknown)
                return (false, "ClinicalScope tidak valid. Gunakan Department, Service, Procedure, Specialty, Unit, Telemedicine, atau Other.");

            if (request.EffectiveStartDate == default)
                return (false, "EffectiveStartDate wajib diisi.");

            if (request.EffectiveEndDate.HasValue && request.EffectiveStartDate.Date > request.EffectiveEndDate.Value.Date)
                return (false, "EffectiveStartDate tidak boleh lebih besar dari EffectiveEndDate.");

            if (request.IsSupervisionRequired && (!request.SupervisorUserId.HasValue || request.SupervisorUserId.Value == Guid.Empty))
                return (false, "SupervisorUserId wajib diisi jika IsSupervisionRequired = true.");

            if (request.SupportingFile != null)
            {
                var fileValidation = ValidateFile(request.SupportingFile);

                if (!fileValidation.IsValid)
                    return fileValidation;
            }

            var credentialValidation = await ValidateCredentialRequirementAsync(
                workforceProfileId,
                request.CredentialLicenseId,
                request.IsEmergencyPrivilege);

            if (!credentialValidation.IsValid)
                return credentialValidation;

            var departmentPositionValidation = await ValidateDepartmentAndPositionAsync(
                request.DepartmentId,
                request.PositionId);

            if (!departmentPositionValidation.IsValid)
                return departmentPositionValidation;

            var supervisorValidation = await ValidateSupervisorAsync(
                request.IsSupervisionRequired,
                request.SupervisorUserId);

            if (!supervisorValidation.IsValid)
                return supervisorValidation;

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length <= 0)
                return (false, "File clinical privilege kosong.");

            if (file.Length > MaxFileSizeBytes)
                return (false, "Ukuran file clinical privilege maksimal 10 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                return (false, "Format file tidak didukung. Gunakan PDF, JPG, JPEG, PNG, DOC, DOCX, XLS, atau XLSX.");

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCredentialRequirementAsync(
            Guid workforceProfileId,
            Guid? credentialLicenseId,
            bool isEmergencyPrivilege)
        {
            if (!credentialLicenseId.HasValue || credentialLicenseId.Value == Guid.Empty)
            {
                return isEmergencyPrivilege
                    ? (true, null)
                    : (false, "CredentialLicenseId wajib dipilih untuk clinical privilege reguler. Kosong hanya diperbolehkan jika IsEmergencyPrivilege = true.");
            }

            var today = AppDateTimeHelper.OperationalDate();

            var validSelectedLicense = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == credentialLicenseId.Value &&
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    x.IsVerified &&
                    x.VerificationStatus == CredentialVerificationStatus.Verified &&
                    x.ExpiredDate >= today &&
                    !x.IsDelete);

            return validSelectedLicense
                ? (true, null)
                : (false, "CredentialLicenseId tidak ditemukan, tidak verified, nonaktif, atau sudah expired.");
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDepartmentAndPositionAsync(
            Guid? departmentId,
            Guid? positionId)
        {
            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                var departmentExists = await _dbContext.Set<MstDepartment>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == departmentId.Value && x.IsActive && !x.IsDelete);

                if (!departmentExists)
                    return (false, "Department tidak ditemukan atau tidak aktif.");
            }

            if (positionId.HasValue && positionId.Value != Guid.Empty)
            {
                var position = await _dbContext.Set<MstPosition>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == positionId.Value && x.IsActive && !x.IsDelete);

                if (position == null)
                    return (false, "Position tidak ditemukan atau tidak aktif.");

                if (departmentId.HasValue && departmentId.Value != Guid.Empty && position.DepartmentId != departmentId.Value)
                    return (false, "Position tidak berada pada department yang dipilih.");
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

        private async Task<IActionResult> ChangeLifecycleAsync(
            Guid workforceProfileId,
            Guid id,
            ClinicalPrivilegeStatus targetStatus,
            string? note,
            bool requiredReason,
            string actionName,
            DateTime? nextReviewDate = null)
        {
            if (requiredReason && string.IsNullOrWhiteSpace(note))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Alasan {actionName} wajib diisi."
                ));
            }

            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Clinical privilege workforce tidak ditemukan."
                ));
            }

            if (entity.EffectiveEndDate.HasValue && entity.EffectiveEndDate.Value.Date < AppDateTimeHelper.OperationalDate() && targetStatus == ClinicalPrivilegeStatus.Active)
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

            if (targetStatus == ClinicalPrivilegeStatus.Active)
            {
                if (entity.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked || entity.PrivilegeStatus == ClinicalPrivilegeStatus.Rejected)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Clinical privilege rejected/revoked tidak bisa di-grant. Buat pengajuan baru."
                    ));
                }

                var credentialValidation = await ValidateCredentialRequirementAsync(
                    workforceProfileId,
                    entity.CredentialLicenseId,
                    entity.IsEmergencyPrivilege);

                if (!credentialValidation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        credentialValidation.ErrorMessage ?? "Credential license belum valid."
                    ));
                }
            }

            if (targetStatus == ClinicalPrivilegeStatus.Suspended && entity.PrivilegeStatus != ClinicalPrivilegeStatus.Active)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya clinical privilege active yang bisa di-suspend."
                ));
            }

            if (targetStatus == ClinicalPrivilegeStatus.Rejected && entity.PrivilegeStatus == ClinicalPrivilegeStatus.Active)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege active tidak bisa di-reject. Gunakan suspend atau revoke."
                ));
            }

            if (targetStatus == ClinicalPrivilegeStatus.Revoked && entity.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Clinical privilege sudah revoked."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var normalizedNote = NormalizeNullableText(note);

            entity.PrivilegeStatus = targetStatus;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            switch (targetStatus)
            {
                case ClinicalPrivilegeStatus.Active:
                    entity.IsActive = true;
                    entity.GrantedByUserId = actorUserId;
                    entity.GrantedAt = now;
                    entity.GrantNote = normalizedNote;
                    entity.RejectedByUserId = null;
                    entity.RejectedAt = null;
                    entity.RejectedReason = null;
                    entity.SuspendedByUserId = null;
                    entity.SuspendedAt = null;
                    entity.SuspensionReason = null;
                    entity.RevokedByUserId = null;
                    entity.RevokedAt = null;
                    entity.RevokedReason = null;
                    if (nextReviewDate.HasValue)
                        entity.NextReviewDate = nextReviewDate.Value.Date;
                    break;

                case ClinicalPrivilegeStatus.Rejected:
                    entity.IsActive = false;
                    entity.RejectedByUserId = actorUserId;
                    entity.RejectedAt = now;
                    entity.RejectedReason = normalizedNote;
                    break;

                case ClinicalPrivilegeStatus.Suspended:
                    entity.IsActive = false;
                    entity.SuspendedByUserId = actorUserId;
                    entity.SuspendedAt = now;
                    entity.SuspensionReason = normalizedNote;
                    break;

                case ClinicalPrivilegeStatus.Revoked:
                    entity.IsActive = false;
                    entity.RevokedByUserId = actorUserId;
                    entity.RevokedAt = now;
                    entity.RevokedReason = normalizedNote;
                    break;
            }

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAuditResponse(data);

            return Ok(ApiResponse<WorkforceClinicalPrivilegeDetailResponse>.Ok(
                data,
                $"Clinical privilege workforce berhasil di-{actionName}."
            ));
        }

        private async Task<string> GeneratePrivilegeCodeAsync()
        {
            var existingCodes = await _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .Where(x => x.PrivilegeCode.StartsWith(CodePrefix))
                .Select(x => x.PrivilegeCode)
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

        private async Task<(string FilePath, string? ContentType)> SaveSupportingFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var storage = GetFileStoragePaths();
            var relativeFolder = Path.Combine("workforce-clinical-privileges", workforceProfileId.ToString());
            var absoluteFolder = Path.Combine(storage.RootPath, relativeFolder);

            Directory.CreateDirectory(absoluteFolder);

            var absolutePath = Path.Combine(absoluteFolder, fileName);

            await using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var publicPath = CombineUrlPath(storage.PublicRequestPath, relativeFolder.Replace("\\", "/"), fileName);

            return (publicPath, file.ContentType);
        }

        private async Task<FileResolveResult> GetClinicalPrivilegeFileAsync(Guid workforceProfileId, Guid id)
        {
            var privilege = await _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (privilege == null)
                return FileResolveResult.Invalid("Clinical privilege workforce tidak ditemukan.");

            if (string.IsNullOrWhiteSpace(privilege.SupportingFilePath))
                return FileResolveResult.Invalid("File clinical privilege workforce belum tersedia.");

            var physicalPath = ResolvePhysicalPath(privilege.SupportingFilePath);

            if (!System.IO.File.Exists(physicalPath))
                return FileResolveResult.Invalid("File fisik clinical privilege tidak ditemukan di server.");

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
                contentType = privilege.SupportingFileContentType ?? "application/octet-stream";

            var extension = Path.GetExtension(physicalPath);
            var fileName = Path.GetFileName(physicalPath);
            var safePrivilegeName = SanitizeFileName(privilege.PrivilegeName);
            var downloadName = $"{privilege.PrivilegeCode}_{safePrivilegeName}{extension}";

            return FileResolveResult.Valid(physicalPath, contentType, fileName, downloadName);
        }

        private void DeletePhysicalFileIfExists(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var physicalPath = ResolvePhysicalPath(filePath);

            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);
        }

        private string ResolvePhysicalPath(string filePath)
        {
            var storage = GetFileStoragePaths();
            var normalizedFilePath = filePath.Replace("\\", "/").Trim();
            var publicPrefix = storage.PublicRequestPath.TrimEnd('/');

            string relativePath;

            if (normalizedFilePath.StartsWith(publicPrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = normalizedFilePath[(publicPrefix.Length + 1)..];
            }
            else
            {
                relativePath = normalizedFilePath.TrimStart('/');
            }

            var fullPath = Path.GetFullPath(Path.Combine(storage.RootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString())));
            var rootPath = Path.GetFullPath(storage.RootPath);

            if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Path file tidak valid.");

            return fullPath;
        }

        private (string RootPath, string PublicRequestPath) GetFileStoragePaths()
        {
            var publicRequestPath = _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";

            if (!publicRequestPath.StartsWith('/'))
                publicRequestPath = "/" + publicRequestPath;

            publicRequestPath = publicRequestPath.TrimEnd('/');

            var configuredRoot = _configuration["FileStorage:UploadRootPath"];

            if (!string.IsNullOrWhiteSpace(configuredRoot))
            {
                Directory.CreateDirectory(configuredRoot);
                return (configuredRoot, publicRequestPath);
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");

            var rootPath = Path.Combine(webRootPath, publicRequestPath.TrimStart('/'));
            Directory.CreateDirectory(rootPath);

            return (rootPath, publicRequestPath);
        }

        private WorkforceClinicalPrivilegeResponse MapResponse(
            WfpClinicalPrivilege entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var today = AppDateTimeHelper.OperationalDate();
            var hasFile = !string.IsNullOrWhiteSpace(entity.SupportingFilePath);
            var runtimeStatus = ResolveRuntimeStatus(entity.PrivilegeStatus, entity.EffectiveEndDate);

            return new WorkforceClinicalPrivilegeResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                CredentialLicenseId = entity.CredentialLicenseId,
                CredentialLicenseType = entity.CredentialLicense?.LicenseType,
                CredentialLicenseNumber = entity.CredentialLicense?.LicenseNumber,
                DepartmentId = entity.DepartmentId,
                DepartmentCode = entity.Department?.DepartmentCode,
                DepartmentName = entity.Department?.DepartmentName,
                PositionId = entity.PositionId,
                PositionCode = entity.Position?.PositionCode,
                PositionName = entity.Position?.PositionName,
                PrivilegeCode = entity.PrivilegeCode,
                PrivilegeName = entity.PrivilegeName,
                PrivilegeType = entity.PrivilegeType,
                ClinicalScope = entity.ClinicalScope,
                SpecialtyName = entity.SpecialtyName,
                SubSpecialtyName = entity.SubSpecialtyName,
                ProcedureGroup = entity.ProcedureGroup,
                ProcedureName = entity.ProcedureName,
                PracticeLocation = entity.PracticeLocation,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                PrivilegeStatus = runtimeStatus,
                IsTemporary = entity.IsTemporary,
                IsEmergencyPrivilege = entity.IsEmergencyPrivilege,
                IsSupervisionRequired = entity.IsSupervisionRequired,
                SupervisorUserId = entity.SupervisorUserId,
                SupervisorUserName = entity.SupervisorUser?.DisplayName,
                GrantedByUserId = entity.GrantedByUserId,
                GrantedByUserName = entity.GrantedByUser?.DisplayName,
                GrantedAt = entity.GrantedAt,
                GrantNote = entity.GrantNote,
                RejectedByUserId = entity.RejectedByUserId,
                RejectedByUserName = entity.RejectedByUser?.DisplayName,
                RejectedAt = entity.RejectedAt,
                RejectedReason = entity.RejectedReason,
                SuspendedByUserId = entity.SuspendedByUserId,
                SuspendedByUserName = entity.SuspendedByUser?.DisplayName,
                SuspendedAt = entity.SuspendedAt,
                SuspensionReason = entity.SuspensionReason,
                RevokedByUserId = entity.RevokedByUserId,
                RevokedByUserName = entity.RevokedByUser?.DisplayName,
                RevokedAt = entity.RevokedAt,
                RevokedReason = entity.RevokedReason,
                LastReviewDate = entity.LastReviewDate,
                NextReviewDate = entity.NextReviewDate,
                HasSupportingFile = hasFile,
                SupportingFilePath = entity.SupportingFilePath,
                SupportingFileContentType = entity.SupportingFileContentType,
                SupportingFileName = hasFile ? Path.GetFileName(entity.SupportingFilePath) : null,
                SupportingFilePreviewUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "preview") : null,
                SupportingFileDownloadUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "download") : null,
                IsExpired = entity.EffectiveEndDate.HasValue && entity.EffectiveEndDate.Value.Date < today,
                IsCurrentlyValid = entity.IsActive && runtimeStatus == ClinicalPrivilegeStatus.Active && (!entity.EffectiveEndDate.HasValue || entity.EffectiveEndDate.Value.Date >= today),
                IsActive = entity.IsActive,
                Description = entity.Description,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private WorkforceClinicalPrivilegeDetailResponse MapDetailResponse(
            WfpClinicalPrivilege entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, profile, actorNames);

            return new WorkforceClinicalPrivilegeDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                CredentialLicenseId = response.CredentialLicenseId,
                CredentialLicenseType = response.CredentialLicenseType,
                CredentialLicenseNumber = response.CredentialLicenseNumber,
                DepartmentId = response.DepartmentId,
                DepartmentCode = response.DepartmentCode,
                DepartmentName = response.DepartmentName,
                PositionId = response.PositionId,
                PositionCode = response.PositionCode,
                PositionName = response.PositionName,
                PrivilegeCode = response.PrivilegeCode,
                PrivilegeName = response.PrivilegeName,
                PrivilegeType = response.PrivilegeType,
                ClinicalScope = response.ClinicalScope,
                SpecialtyName = response.SpecialtyName,
                SubSpecialtyName = response.SubSpecialtyName,
                ProcedureGroup = response.ProcedureGroup,
                ProcedureName = response.ProcedureName,
                PracticeLocation = response.PracticeLocation,
                EffectiveStartDate = response.EffectiveStartDate,
                EffectiveEndDate = response.EffectiveEndDate,
                PrivilegeStatus = response.PrivilegeStatus,
                IsTemporary = response.IsTemporary,
                IsEmergencyPrivilege = response.IsEmergencyPrivilege,
                IsSupervisionRequired = response.IsSupervisionRequired,
                SupervisorUserId = response.SupervisorUserId,
                SupervisorUserName = response.SupervisorUserName,
                GrantedByUserId = response.GrantedByUserId,
                GrantedByUserName = response.GrantedByUserName,
                GrantedAt = response.GrantedAt,
                GrantNote = response.GrantNote,
                RejectedByUserId = response.RejectedByUserId,
                RejectedByUserName = response.RejectedByUserName,
                RejectedAt = response.RejectedAt,
                RejectedReason = response.RejectedReason,
                SuspendedByUserId = response.SuspendedByUserId,
                SuspendedByUserName = response.SuspendedByUserName,
                SuspendedAt = response.SuspendedAt,
                SuspensionReason = response.SuspensionReason,
                RevokedByUserId = response.RevokedByUserId,
                RevokedByUserName = response.RevokedByUserName,
                RevokedAt = response.RevokedAt,
                RevokedReason = response.RevokedReason,
                LastReviewDate = response.LastReviewDate,
                NextReviewDate = response.NextReviewDate,
                HasSupportingFile = response.HasSupportingFile,
                SupportingFilePath = response.SupportingFilePath,
                SupportingFileContentType = response.SupportingFileContentType,
                SupportingFileName = response.SupportingFileName,
                SupportingFilePreviewUrl = response.SupportingFilePreviewUrl,
                SupportingFileDownloadUrl = response.SupportingFileDownloadUrl,
                IsExpired = response.IsExpired,
                IsCurrentlyValid = response.IsCurrentlyValid,
                IsActive = response.IsActive,
                Description = response.Description,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
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

        private static void NormalizeAuditResponse(WorkforceClinicalPrivilegeDetailResponse data)
        {
            if (data.UpdateDateTime.HasValue &&
                data.UpdateDateTime.Value == DateTime.MinValue)
            {
                data.UpdateDateTime = null;
            }

            if (!data.CreateBy.HasValue || data.CreateBy.Value == Guid.Empty)
            {
                data.CreateBy = null;
                data.CreateByName = null;
            }

            if (!data.UpdateBy.HasValue || data.UpdateBy.Value == Guid.Empty)
            {
                data.UpdateBy = null;
                data.UpdateByName = null;
            }
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

        private string BuildEndpointUrl(Guid workforceProfileId, Guid id, string action)
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            var path = $"{pathBase}/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/clinical-privileges/{id}/{action}";

            return $"{Request.Scheme}://{Request.Host}{path}";
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<bool> ProfileExistsAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);
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

            if (effectiveEndDate.HasValue && effectiveEndDate.Value.Date < AppDateTimeHelper.OperationalDate())
            {
                return ClinicalPrivilegeStatus.Expired;
            }

            return status;
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

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
        }

        private static string NormalizeRequiredText(string value)
        {
            return value.Trim();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeClinicalScope(WorkforceClinicalScope? value)
        {
            if (!value.HasValue || value.Value == WorkforceClinicalScope.Unknown)
                return null;

            return value.Value.ToString();
        }

        private static string SanitizeFileName(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(value.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray());

            return string.IsNullOrWhiteSpace(cleaned)
                ? "clinical-privilege"
                : cleaned.Trim();
        }

        private static string CombineUrlPath(params string[] parts)
        {
            return "/" + string.Join(
                "/",
                parts
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim('/'))
            );
        }

        private static List<WorkforceClinicalPrivilegeEnumOptionResponse> BuildPrivilegeTypeOptions()
        {
            return Enum.GetValues<ClinicalPrivilegeType>()
                .Where(x => x != ClinicalPrivilegeType.Unknown)
                .Select(x => new WorkforceClinicalPrivilegeEnumOptionResponse
                {
                    Value = x.ToString(),
                    NumericValue = Convert.ToInt32(x),
                    Label = x.ToString(),
                    Description = GetPrivilegeTypeDescription(x)
                })
                .ToList();
        }

        private static List<WorkforceClinicalPrivilegeEnumOptionResponse> BuildPrivilegeStatusOptions()
        {
            return Enum.GetValues<ClinicalPrivilegeStatus>()
                .Select(x => new WorkforceClinicalPrivilegeEnumOptionResponse
                {
                    Value = x.ToString(),
                    NumericValue = Convert.ToInt32(x),
                    Label = x.ToString(),
                    Description = GetPrivilegeStatusDescription(x)
                })
                .ToList();
        }

        private static List<WorkforceClinicalPrivilegeEnumOptionResponse> BuildClinicalScopeOptions()
        {
            return Enum.GetValues<WorkforceClinicalScope>()
                .Where(x => x != WorkforceClinicalScope.Unknown)
                .Select(x => new WorkforceClinicalPrivilegeEnumOptionResponse
                {
                    Value = x.ToString(),
                    NumericValue = Convert.ToInt32(x),
                    Label = x.ToString(),
                    Description = GetClinicalScopeDescription(x)
                })
                .ToList();
        }

        private static string GetPrivilegeTypeDescription(ClinicalPrivilegeType value)
        {
            return value.ToString() switch
            {
                "CorePrivilege" => "Privilege inti yang umum melekat pada profesi/jabatan klinis.",
                "SpecialPrivilege" => "Privilege tambahan untuk kompetensi atau layanan khusus.",
                "ProcedurePrivilege" => "Privilege untuk tindakan/prosedur klinis tertentu.",
                "TemporaryPrivilege" => "Privilege sementara dengan periode terbatas.",
                "EmergencyPrivilege" => "Privilege untuk kondisi emergency sesuai kebijakan rumah sakit.",
                _ => "Tipe clinical privilege."
            };
        }

        private static string GetPrivilegeStatusDescription(ClinicalPrivilegeStatus value)
        {
            return value.ToString() switch
            {
                "PendingApproval" => "Menunggu persetujuan atau proses kredensial/komite medik.",
                "Active" => "Privilege sudah diberikan dan dapat digunakan selama masih berlaku.",
                "Rejected" => "Pengajuan privilege ditolak.",
                "Suspended" => "Privilege sementara dibekukan.",
                "Revoked" => "Privilege dicabut.",
                "Expired" => "Privilege sudah melewati tanggal berlaku.",
                _ => "Status clinical privilege."
            };
        }

        private static string GetClinicalScopeDescription(WorkforceClinicalScope value)
        {
            return value switch
            {
                WorkforceClinicalScope.Department => "Privilege berlaku pada department tertentu.",
                WorkforceClinicalScope.Service => "Privilege berlaku pada layanan klinis tertentu.",
                WorkforceClinicalScope.Procedure => "Privilege berlaku pada tindakan/prosedur tertentu.",
                WorkforceClinicalScope.Specialty => "Privilege berlaku pada bidang spesialisasi tertentu.",
                WorkforceClinicalScope.Unit => "Privilege berlaku pada unit pelayanan tertentu.",
                WorkforceClinicalScope.Telemedicine => "Privilege berlaku pada layanan telemedicine.",
                WorkforceClinicalScope.Other => "Scope lainnya.",
                _ => "Scope clinical privilege."
            };
        }

        private sealed class FileResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public string? PhysicalPath { get; private set; }
            public string? ContentType { get; private set; }
            public string? FileName { get; private set; }
            public string? DownloadName { get; private set; }

            public static FileResolveResult Valid(
                string physicalPath,
                string contentType,
                string fileName,
                string downloadName)
            {
                return new FileResolveResult
                {
                    IsValid = true,
                    PhysicalPath = physicalPath,
                    ContentType = contentType,
                    FileName = fileName,
                    DownloadName = downloadName
                };
            }

            public static FileResolveResult Invalid(string errorMessage)
            {
                return new FileResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
