using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_CREDENTIALING",
        moduleName: "Human Resource Credentialing",
        displayName: "Workforce Clinical Privilege",
        AreaName = "Corporate",
        ControllerName = "WorkforceClinicalPrivilege",
        Description = "Corporate human resource workforce clinical privilege",
        SortOrder = 3)]
    [Tags("Corporate / Human Resource / Credentialing Management / Clinical Privilege")]
    public class WfpClinicalPrivilegeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Credentialing";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpClinicalPrivilegeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Melihat metadata filter hak klinis workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetFilterMetadata(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var catalogOptions = await _dbContext.Set<MstClinicalPrivilegeCatalog>()
                .AsNoTracking()
                .Include(x => x.Profession)
                .Include(x => x.Specialization)
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.PrivilegeName)
                .Take(1000)
                .Select(x => new WfpClinicalPrivilegeCatalogOptionResponse
                {
                    Id = x.Id,
                    Code = x.PrivilegeCode,
                    Name = x.PrivilegeName,
                    Category = x.PrivilegeCategory,
                    ProfessionId = x.ProfessionId,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    SpecializationId = x.SpecializationId,
                    SpecializationName = x.Specialization != null ? x.Specialization.SpecializationName : null,
                    RequiresSupervision = x.RequiresSupervision,
                    AllowsIndependentPractice = x.AllowsIndependentPractice,
                    IsHighRisk = x.IsHighRisk,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    Label = x.PrivilegeCode + " - " + x.PrivilegeName
                })
                .ToListAsync(cancellationToken);

            var today = DateTime.UtcNow.Date;
            var licenseOptions = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    !x.IsRevoked)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.ExpiredDate)
                .Select(x => new WfpClinicalPrivilegeLicenseOptionResponse
                {
                    Id = x.Id,
                    LicenseType = x.LicenseType,
                    LicenseNumber = x.LicenseNumber,
                    ExpiredDate = x.ExpiredDate,
                    IsPrimary = x.IsPrimary,
                    IsVerified = x.IsVerified,
                    IsRevoked = x.IsRevoked,
                    IsActive = x.IsActive,
                    Label = x.LicenseType + " - " + x.LicenseNumber
                })
                .ToListAsync(cancellationToken);

            licenseOptions = licenseOptions
                .Where(x => x.ExpiredDate.Date >= today)
                .ToList();

            var result = new WfpClinicalPrivilegeFilterMetadataResponse
            {
                DefaultFilter = new WfpClinicalPrivilegeDefaultFilterResponse(),
                CatalogOptions = catalogOptions,
                CredentialLicenseOptions = licenseOptions,
                PrivilegeTypeOptions = BuildEnumOptions<ClinicalPrivilegeType>(),
                PrivilegeStatusOptions = BuildEnumOptions<ClinicalPrivilegeStatus>(),
                SortOptions = new List<WfpClinicalPrivilegeSortOptionResponse>
                {
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "privilegeCode", Label = "Kode hak klinis" },
                    new() { Value = "privilegeName", Label = "Nama hak klinis" },
                    new() { Value = "privilegeType", Label = "Jenis hak klinis" },
                    new() { Value = "privilegeStatus", Label = "Status hak klinis" },
                    new() { Value = "isTemporary", Label = "Hak sementara" },
                    new() { Value = "isSupervisionRequired", Label = "Perlu supervisi" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpClinicalPrivilegeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter hak klinis workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Melihat ringkasan hak klinis workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var today = DateTime.UtcNow.Date;
            var expiringLimit = today.AddDays(90);
            var items = await _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .Select(x => new
                {
                    x.IsActive,
                    x.PrivilegeStatus,
                    x.IsTemporary,
                    x.IsSupervisionRequired,
                    x.IsSchedulingBlocked,
                    x.IsClinicalServiceBlocked,
                    x.EffectiveEndDate
                })
                .ToListAsync(cancellationToken);

            var result = new WfpClinicalPrivilegeSummaryResponse
            {
                TotalPrivilege = items.Count,
                ActivePrivilege = items.Count(x => x.IsActive),
                InactivePrivilege = items.Count(x => !x.IsActive),
                GrantedPrivilege = items.Count(x => IsStatusName(x.PrivilegeStatus, "Granted", "Approved", "Active")),
                RejectedPrivilege = items.Count(x => IsStatusName(x.PrivilegeStatus, "Rejected")),
                SuspendedPrivilege = items.Count(x => IsStatusName(x.PrivilegeStatus, "Suspended")),
                RevokedPrivilege = items.Count(x => IsStatusName(x.PrivilegeStatus, "Revoked")),
                TemporaryPrivilege = items.Count(x => x.IsTemporary),
                SupervisionRequiredPrivilege = items.Count(x => x.IsSupervisionRequired),
                SchedulingBlockedPrivilege = items.Count(x => x.IsSchedulingBlocked),
                ClinicalServiceBlockedPrivilege = items.Count(x => x.IsClinicalServiceBlocked),
                ExpiredPrivilege = items.Count(x => x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today),
                ExpiringSoonPrivilege = items.Count(x =>
                    x.IsActive &&
                    x.EffectiveEndDate.HasValue &&
                    x.EffectiveEndDate.Value.Date >= today &&
                    x.EffectiveEndDate.Value.Date <= expiringLimit)
            };

            return Ok(ApiResponse<WfpClinicalPrivilegeSummaryResponse>.Ok(
                result,
                "Ringkasan hak klinis workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpClinicalPrivilegeResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Melihat data hak klinis workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetClinicalPrivileges(
            Guid workforceProfileId,
            [FromQuery] Guid? clinicalPrivilegeCatalogId,
            [FromQuery] Guid? credentialLicenseId,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] ClinicalPrivilegeType? privilegeType,
            [FromQuery] ClinicalPrivilegeStatus? privilegeStatus,
            [FromQuery] bool? isTemporary,
            [FromQuery] bool? isEmergencyPrivilege,
            [FromQuery] bool? isSupervisionRequired,
            [FromQuery] bool? isSchedulingBlocked,
            [FromQuery] bool? isClinicalServiceBlocked,
            [FromQuery] bool? isExpired,
            [FromQuery] bool? isActive,
            [FromQuery] int? expiringWithinDays,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(workforceProfileId),
                clinicalPrivilegeCatalogId,
                credentialLicenseId,
                departmentId,
                positionId,
                privilegeType,
                privilegeStatus,
                isTemporary,
                isEmergencyPrivilege,
                isSupervisionRequired,
                isSchedulingBlocked,
                isClinicalServiceBlocked,
                isExpired,
                isActive,
                expiringWithinDays,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var actorNames = await GetActorNameMapAsync(
                entities.SelectMany(x => new[]
                {
                    x.CreateBy,
                    x.SupervisorUserId ?? Guid.Empty,
                    x.GrantedByUserId ?? Guid.Empty,
                    x.RejectedByUserId ?? Guid.Empty,
                    x.SuspendedByUserId ?? Guid.Empty,
                    x.RevokedByUserId ?? Guid.Empty
                }),
                cancellationToken);

            var result = new PagedResult<WfpClinicalPrivilegeResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<PagedResult<WfpClinicalPrivilegeResponse>>.Ok(
                result,
                "Data hak klinis workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Clinical Privilege", Description = "Melihat detail hak klinis workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceClinicalPrivilege", "Read")]
        public async Task<IActionResult> GetClinicalPrivilegeById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Hak klinis workforce tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(
                new[]
                {
                    entity.CreateBy,
                    entity.UpdateBy,
                    entity.SupervisorUserId ?? Guid.Empty,
                    entity.GrantedByUserId ?? Guid.Empty,
                    entity.RejectedByUserId ?? Guid.Empty,
                    entity.SuspendedByUserId ?? Guid.Empty,
                    entity.RevokedByUserId ?? Guid.Empty
                },
                cancellationToken);

            return Ok(ApiResponse<WfpClinicalPrivilegeDetailResponse>.Ok(
                MapDetailResponse(entity, actorNames),
                "Detail hak klinis workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Clinical Privilege", Description = "Membuat hak klinis workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceClinicalPrivilege", "Create")]
        public async Task<IActionResult> CreateClinicalPrivilege(
            Guid workforceProfileId,
            [FromBody] CreateWfpClinicalPrivilegeRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var validation = await ValidateRequestAsync(workforceProfileId, null, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var catalog = await FindCatalogAsync(request.ClinicalPrivilegeCatalogId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpClinicalPrivilege
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CredentialLicenseId = NormalizeNullableGuid(request.CredentialLicenseId),
                ClinicalPrivilegeCatalogId = NormalizeNullableGuid(request.ClinicalPrivilegeCatalogId),
                CredentialingDecisionId = NormalizeNullableGuid(request.CredentialingDecisionId),
                DepartmentId = NormalizeNullableGuid(request.DepartmentId),
                PositionId = NormalizeNullableGuid(request.PositionId),
                SupervisorUserId = NormalizeNullableGuid(request.SupervisorUserId),
                PrivilegeCode = catalog?.PrivilegeCode ?? request.PrivilegeCode.Trim(),
                PrivilegeName = catalog?.PrivilegeName ?? request.PrivilegeName.Trim(),
                PrivilegeType = request.PrivilegeType,
                ClinicalScope = NormalizeNullableText(request.ClinicalScope),
                SpecialtyName = NormalizeNullableText(request.SpecialtyName) ?? catalog?.Specialization?.SpecializationName,
                SubSpecialtyName = NormalizeNullableText(request.SubSpecialtyName),
                ProcedureGroup = NormalizeNullableText(request.ProcedureGroup) ?? catalog?.PrivilegeCategory,
                ProcedureName = NormalizeNullableText(request.ProcedureName),
                PracticeLocation = NormalizeNullableText(request.PracticeLocation),
                EffectiveStartDate = NormalizeUtcDate(request.EffectiveStartDate),
                EffectiveEndDate = NormalizeNullableUtcDate(request.EffectiveEndDate),
                PrivilegeStatus = request.PrivilegeStatus,
                IsTemporary = request.IsTemporary,
                IsEmergencyPrivilege = request.IsEmergencyPrivilege,
                IsSupervisionRequired = request.IsSupervisionRequired || catalog?.RequiresSupervision == true,
                Restrictions = NormalizeNullableText(request.Restrictions),
                IsSchedulingBlocked = request.IsSchedulingBlocked,
                IsClinicalServiceBlocked = request.IsClinicalServiceBlocked,
                SupportingFilePath = NormalizeNullableText(request.SupportingFilePath),
                SupportingFileContentType = NormalizeNullableText(request.SupportingFileContentType),
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpClinicalPrivilege>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceClinicalPrivilege.CreateClinicalPrivilege",
                "Membuat hak klinis workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.PrivilegeCode, entity.PrivilegeStatus });

            return await GetClinicalPrivilegeById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Clinical Privilege", Description = "Mengubah hak klinis workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> UpdateClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpClinicalPrivilegeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Hak klinis workforce tidak ditemukan."));

            if (entity.RevokedAt.HasValue)
                return BadRequest(ApiResponse<object>.Fail(400, "Hak klinis yang sudah dicabut tidak dapat diubah."));

            var validation = await ValidateRequestAsync(workforceProfileId, id, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var catalog = await FindCatalogAsync(request.ClinicalPrivilegeCatalogId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.CredentialLicenseId = NormalizeNullableGuid(request.CredentialLicenseId);
            entity.ClinicalPrivilegeCatalogId = NormalizeNullableGuid(request.ClinicalPrivilegeCatalogId);
            entity.CredentialingDecisionId = NormalizeNullableGuid(request.CredentialingDecisionId);
            entity.DepartmentId = NormalizeNullableGuid(request.DepartmentId);
            entity.PositionId = NormalizeNullableGuid(request.PositionId);
            entity.SupervisorUserId = NormalizeNullableGuid(request.SupervisorUserId);
            entity.PrivilegeCode = catalog?.PrivilegeCode ?? request.PrivilegeCode.Trim();
            entity.PrivilegeName = catalog?.PrivilegeName ?? request.PrivilegeName.Trim();
            entity.PrivilegeType = request.PrivilegeType;
            entity.ClinicalScope = NormalizeNullableText(request.ClinicalScope);
            entity.SpecialtyName = NormalizeNullableText(request.SpecialtyName) ?? catalog?.Specialization?.SpecializationName;
            entity.SubSpecialtyName = NormalizeNullableText(request.SubSpecialtyName);
            entity.ProcedureGroup = NormalizeNullableText(request.ProcedureGroup) ?? catalog?.PrivilegeCategory;
            entity.ProcedureName = NormalizeNullableText(request.ProcedureName);
            entity.PracticeLocation = NormalizeNullableText(request.PracticeLocation);
            entity.EffectiveStartDate = NormalizeUtcDate(request.EffectiveStartDate);
            entity.EffectiveEndDate = NormalizeNullableUtcDate(request.EffectiveEndDate);
            entity.PrivilegeStatus = request.PrivilegeStatus;
            entity.IsTemporary = request.IsTemporary;
            entity.IsEmergencyPrivilege = request.IsEmergencyPrivilege;
            entity.IsSupervisionRequired = request.IsSupervisionRequired || catalog?.RequiresSupervision == true;
            entity.Restrictions = NormalizeNullableText(request.Restrictions);
            entity.IsSchedulingBlocked = request.IsSchedulingBlocked;
            entity.IsClinicalServiceBlocked = request.IsClinicalServiceBlocked;
            entity.SupportingFilePath = NormalizeNullableText(request.SupportingFilePath);
            entity.SupportingFileContentType = NormalizeNullableText(request.SupportingFileContentType);
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetClinicalPrivilegeById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Clinical Privilege", Description = "Mengubah status aktif hak klinis workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> UpdateClinicalPrivilegeStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpClinicalPrivilegeStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Hak klinis workforce tidak ditemukan."));

            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < entity.EffectiveStartDate.Date)
                return BadRequest(ApiResponse<object>.Fail(400, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai."));

            entity.IsActive = request.IsActive;
            entity.EffectiveEndDate = NormalizeNullableUtcDate(request.EffectiveEndDate) ?? entity.EffectiveEndDate;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetClinicalPrivilegeById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/grant")]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Grant Workforce Clinical Privilege", Description = "Memberikan hak klinis workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> GrantClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] GrantWfpClinicalPrivilegeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Hak klinis workforce tidak ditemukan."));

            if (!TryResolveStatus(out var status, "Granted", "Approved", "Active"))
                return InvalidLifecycleStatus("grant", "Granted/Approved/Active");

            var validation = await ValidateGrantEligibilityAsync(entity, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.PrivilegeStatus = status;
            entity.GrantedAt = now;
            entity.GrantedByUserId = actorUserId;
            entity.ApprovalNotes = NormalizeNullableText(request.ApprovalNotes);
            entity.RejectedAt = null;
            entity.RejectedByUserId = null;
            entity.RejectionReason = null;
            entity.SuspendedAt = null;
            entity.SuspendedByUserId = null;
            entity.SuspensionReason = null;
            entity.RevokedAt = null;
            entity.RevokedByUserId = null;
            entity.RevocationReason = null;
            entity.IsSchedulingBlocked = request.IsSchedulingBlocked;
            entity.IsClinicalServiceBlocked = request.IsClinicalServiceBlocked;
            entity.IsActive = true;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetClinicalPrivilegeById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Reject Workforce Clinical Privilege", Description = "Menolak hak klinis workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> RejectClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RejectWfpClinicalPrivilegeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Hak klinis workforce tidak ditemukan."));
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
                return BadRequest(ApiResponse<object>.Fail(400, "Alasan penolakan wajib diisi."));
            if (!TryResolveStatus(out var status, "Rejected"))
                return InvalidLifecycleStatus("reject", "Rejected");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.PrivilegeStatus = status;
            entity.RejectedAt = now;
            entity.RejectedByUserId = actorUserId;
            entity.RejectionReason = request.RejectionReason.Trim();
            entity.IsSchedulingBlocked = true;
            entity.IsClinicalServiceBlocked = true;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetClinicalPrivilegeById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/suspend")]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Suspend Workforce Clinical Privilege", Description = "Menangguhkan hak klinis workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> SuspendClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SuspendWfpClinicalPrivilegeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Hak klinis workforce tidak ditemukan."));
            if (string.IsNullOrWhiteSpace(request.SuspensionReason))
                return BadRequest(ApiResponse<object>.Fail(400, "Alasan penangguhan wajib diisi."));
            if (entity.RevokedAt.HasValue)
                return BadRequest(ApiResponse<object>.Fail(400, "Hak klinis yang sudah dicabut tidak dapat ditangguhkan."));
            if (!TryResolveStatus(out var status, "Suspended"))
                return InvalidLifecycleStatus("suspend", "Suspended");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.PrivilegeStatus = status;
            entity.SuspendedAt = now;
            entity.SuspendedByUserId = actorUserId;
            entity.SuspensionReason = request.SuspensionReason.Trim();
            entity.IsSchedulingBlocked = request.IsSchedulingBlocked;
            entity.IsClinicalServiceBlocked = request.IsClinicalServiceBlocked;
            entity.IsActive = true;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetClinicalPrivilegeById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<WfpClinicalPrivilegeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Revoke Workforce Clinical Privilege", Description = "Mencabut hak klinis workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceClinicalPrivilege", "Update")]
        public async Task<IActionResult> RevokeClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RevokeWfpClinicalPrivilegeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Hak klinis workforce tidak ditemukan."));
            if (string.IsNullOrWhiteSpace(request.RevocationReason))
                return BadRequest(ApiResponse<object>.Fail(400, "Alasan pencabutan wajib diisi."));
            if (entity.RevokedAt.HasValue)
                return BadRequest(ApiResponse<object>.Fail(400, "Hak klinis sudah dicabut sebelumnya."));
            if (!TryResolveStatus(out var status, "Revoked"))
                return InvalidLifecycleStatus("revoke", "Revoked");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.PrivilegeStatus = status;
            entity.RevokedAt = now;
            entity.RevokedByUserId = actorUserId;
            entity.RevocationReason = request.RevocationReason.Trim();
            entity.IsSchedulingBlocked = true;
            entity.IsClinicalServiceBlocked = true;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetClinicalPrivilegeById(workforceProfileId, id, cancellationToken);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Clinical Privilege", Description = "Menghapus hak klinis workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceClinicalPrivilege", "Delete")]
        public async Task<IActionResult> DeleteClinicalPrivilege(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Hak klinis workforce tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsSchedulingBlocked = true;
            entity.IsClinicalServiceBlocked = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceClinicalPrivilege.DeleteClinicalPrivilege",
                "Menghapus hak klinis workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.PrivilegeCode });

            return Ok(ApiResponse<object>.Ok(null, "Hak klinis workforce berhasil dihapus."));
        }

        private IQueryable<WfpClinicalPrivilege> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.CredentialLicense)
                .Include(x => x.ClinicalPrivilegeCatalog)
                    .ThenInclude(x => x!.Profession)
                .Include(x => x.ClinicalPrivilegeCatalog)
                    .ThenInclude(x => x!.Specialization)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.SupervisorUser)
                .Include(x => x.GrantedByUser)
                .Include(x => x.RejectedByUser)
                .Include(x => x.SuspendedByUser)
                .Include(x => x.RevokedByUser)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpClinicalPrivilege> ApplyFilter(
            IQueryable<WfpClinicalPrivilege> query,
            Guid? clinicalPrivilegeCatalogId,
            Guid? credentialLicenseId,
            Guid? departmentId,
            Guid? positionId,
            ClinicalPrivilegeType? privilegeType,
            ClinicalPrivilegeStatus? privilegeStatus,
            bool? isTemporary,
            bool? isEmergencyPrivilege,
            bool? isSupervisionRequired,
            bool? isSchedulingBlocked,
            bool? isClinicalServiceBlocked,
            bool? isExpired,
            bool? isActive,
            int? expiringWithinDays,
            string? search)
        {
            var today = DateTime.UtcNow.Date;

            if (clinicalPrivilegeCatalogId.HasValue && clinicalPrivilegeCatalogId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicalPrivilegeCatalogId == clinicalPrivilegeCatalogId.Value);
            if (credentialLicenseId.HasValue && credentialLicenseId.Value != Guid.Empty)
                query = query.Where(x => x.CredentialLicenseId == credentialLicenseId.Value);
            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
                query = query.Where(x => x.DepartmentId == departmentId.Value);
            if (positionId.HasValue && positionId.Value != Guid.Empty)
                query = query.Where(x => x.PositionId == positionId.Value);
            if (privilegeType.HasValue)
                query = query.Where(x => x.PrivilegeType == privilegeType.Value);
            if (privilegeStatus.HasValue)
                query = query.Where(x => x.PrivilegeStatus == privilegeStatus.Value);
            if (isTemporary.HasValue)
                query = query.Where(x => x.IsTemporary == isTemporary.Value);
            if (isEmergencyPrivilege.HasValue)
                query = query.Where(x => x.IsEmergencyPrivilege == isEmergencyPrivilege.Value);
            if (isSupervisionRequired.HasValue)
                query = query.Where(x => x.IsSupervisionRequired == isSupervisionRequired.Value);
            if (isSchedulingBlocked.HasValue)
                query = query.Where(x => x.IsSchedulingBlocked == isSchedulingBlocked.Value);
            if (isClinicalServiceBlocked.HasValue)
                query = query.Where(x => x.IsClinicalServiceBlocked == isClinicalServiceBlocked.Value);
            if (isExpired.HasValue)
                query = isExpired.Value
                    ? query.Where(x => x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < today)
                    : query.Where(x => !x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today);
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (expiringWithinDays.HasValue && expiringWithinDays.Value >= 0)
            {
                var limit = today.AddDays(Math.Min(expiringWithinDays.Value, 3650));
                query = query.Where(x =>
                    x.EffectiveEndDate.HasValue &&
                    x.EffectiveEndDate.Value >= today &&
                    x.EffectiveEndDate.Value <= limit);
            }

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
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "effectiveenddate" => desc ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "privilegecode" => desc ? query.OrderByDescending(x => x.PrivilegeCode) : query.OrderBy(x => x.PrivilegeCode),
                "privilegename" => desc ? query.OrderByDescending(x => x.PrivilegeName) : query.OrderBy(x => x.PrivilegeName),
                "privilegetype" => desc ? query.OrderByDescending(x => x.PrivilegeType) : query.OrderBy(x => x.PrivilegeType),
                "privilegestatus" => desc ? query.OrderByDescending(x => x.PrivilegeStatus) : query.OrderBy(x => x.PrivilegeStatus),
                "istemporary" => desc ? query.OrderByDescending(x => x.IsTemporary) : query.OrderBy(x => x.IsTemporary),
                "issupervisionrequired" => desc ? query.OrderByDescending(x => x.IsSupervisionRequired) : query.OrderBy(x => x.IsSupervisionRequired),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? currentId,
            CreateWfpClinicalPrivilegeRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.PrivilegeCode))
                return (false, "Kode hak klinis wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.PrivilegeName))
                return (false, "Nama hak klinis wajib diisi.");
            if (request.EffectiveStartDate == default)
                return (false, "Tanggal mulai berlaku wajib diisi.");
            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date)
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai.");

            var catalog = await FindCatalogAsync(request.ClinicalPrivilegeCatalogId, cancellationToken);
            if (request.ClinicalPrivilegeCatalogId.HasValue && catalog == null)
                return (false, "Master katalog hak klinis tidak ditemukan atau tidak aktif.");

            var licenseId = NormalizeNullableGuid(request.CredentialLicenseId);
            if (licenseId.HasValue)
            {
                var license = await _dbContext.Set<WfpCredentialLicense>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == licenseId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsRevoked &&
                        !x.IsDelete,
                        cancellationToken);

                if (license == null)
                    return (false, "Lisensi tidak ditemukan, tidak aktif, sudah dicabut, atau bukan milik workforce ini.");

                if (license.ExpiredDate.Date < request.EffectiveStartDate.Date)
                    return (false, "Lisensi sudah kedaluwarsa sebelum hak klinis mulai berlaku.");
            }

            var departmentId = NormalizeNullableGuid(request.DepartmentId);
            if (departmentId.HasValue)
            {
                var departmentExists = await _dbContext.Set<MstDepartment>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == departmentId.Value && x.IsActive && !x.IsDelete, cancellationToken);
                if (!departmentExists)
                    return (false, "Department tidak ditemukan atau tidak aktif.");
            }

            var positionId = NormalizeNullableGuid(request.PositionId);
            if (positionId.HasValue)
            {
                var position = await _dbContext.Set<MstPosition>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == positionId.Value && x.IsActive && !x.IsDelete, cancellationToken);
                if (position == null)
                    return (false, "Position tidak ditemukan atau tidak aktif.");
                if (departmentId.HasValue && position.DepartmentId != departmentId.Value)
                    return (false, "Position tidak sesuai dengan department yang dipilih.");
            }

            var supervisorUserId = NormalizeNullableGuid(request.SupervisorUserId);
            var supervisionRequired = request.IsSupervisionRequired || catalog?.RequiresSupervision == true;
            if (supervisionRequired && !supervisorUserId.HasValue)
                return (false, "Supervisor wajib dipilih untuk hak klinis yang memerlukan supervisi.");

            if (supervisorUserId.HasValue)
            {
                var supervisorExists = await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == supervisorUserId.Value, cancellationToken);
                if (!supervisorExists)
                    return (false, "User supervisor tidak ditemukan.");
            }

            var decisionId = NormalizeNullableGuid(request.CredentialingDecisionId);
            if (decisionId.HasValue)
            {
                var decisionExists = await _dbContext.Set<TrxCredentialingDecision>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == decisionId.Value && !x.IsDelete, cancellationToken);
                if (!decisionExists)
                    return (false, "Credentialing decision tidak ditemukan.");
            }

            var normalizedCode = catalog?.PrivilegeCode ?? request.PrivilegeCode.Trim();
            var duplicate = await _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.PrivilegeCode == normalizedCode &&
                    x.Id != currentId &&
                    !x.IsDelete,
                    cancellationToken);

            if (duplicate)
                return (false, "Kode hak klinis yang sama sudah tersedia untuk workforce ini.");

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGrantEligibilityAsync(
            WfpClinicalPrivilege entity,
            CancellationToken cancellationToken)
        {
            if (entity.EffectiveEndDate.HasValue && entity.EffectiveEndDate.Value.Date < DateTime.UtcNow.Date)
                return (false, "Hak klinis yang sudah melewati tanggal akhir tidak dapat diberikan.");

            if (entity.IsSupervisionRequired && !entity.SupervisorUserId.HasValue)
                return (false, "Supervisor belum ditentukan untuk hak klinis yang memerlukan supervisi.");

            if (entity.CredentialLicenseId.HasValue)
            {
                var validLicense = await _dbContext.Set<WfpCredentialLicense>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == entity.CredentialLicenseId.Value &&
                        x.WorkforceProfileId == entity.WorkforceProfileId &&
                        x.IsActive &&
                        !x.IsRevoked &&
                        x.IsVerified &&
                        x.ExpiredDate >= DateTime.UtcNow.Date &&
                        !x.IsDelete,
                        cancellationToken);

                if (!validLicense)
                    return (false, "Lisensi pendukung belum valid, belum terverifikasi, kedaluwarsa, atau sudah dicabut.");
            }

            return (true, null);
        }

        private async Task<MstClinicalPrivilegeCatalog?> FindCatalogAsync(Guid? id, CancellationToken cancellationToken)
        {
            id = NormalizeNullableGuid(id);
            if (!id.HasValue)
                return null;

            return await _dbContext.Set<MstClinicalPrivilegeCatalog>()
                .AsNoTracking()
                .Include(x => x.Specialization)
                .FirstOrDefaultAsync(x => x.Id == id.Value && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private async Task<WfpClinicalPrivilege?> FindEntityAsync(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpClinicalPrivilege>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);
        }

        private async Task<bool> WorkforceProfileExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return id != Guid.Empty && await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private IActionResult WorkforceProfileNotFound() =>
            NotFound(ApiResponse<object>.Fail(404, "Profil tenaga kerja tidak ditemukan."));

        private IActionResult InvalidLifecycleStatus(string action, string expectedStatus) =>
            BadRequest(ApiResponse<object>.Fail(
                400,
                $"Action {action} tidak dapat dijalankan karena enum ClinicalPrivilegeStatus tidak memiliki nilai {expectedStatus}. Nilai tersedia: {string.Join(", ", Enum.GetNames<ClinicalPrivilegeStatus>())}."));

        private static WfpClinicalPrivilegeResponse MapResponse(
            WfpClinicalPrivilege x,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var today = DateTime.UtcNow.Date;
            var days = x.EffectiveEndDate.HasValue
                ? (int?)(x.EffectiveEndDate.Value.Date - today).TotalDays
                : null;

            return new WfpClinicalPrivilegeResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                CredentialLicenseId = x.CredentialLicenseId,
                CredentialLicenseType = x.CredentialLicense?.LicenseType,
                CredentialLicenseNumber = x.CredentialLicense?.LicenseNumber,
                ClinicalPrivilegeCatalogId = x.ClinicalPrivilegeCatalogId,
                ClinicalPrivilegeCatalogCode = x.ClinicalPrivilegeCatalog?.PrivilegeCode,
                ClinicalPrivilegeCatalogName = x.ClinicalPrivilegeCatalog?.PrivilegeName,
                PrivilegeCategory = x.ClinicalPrivilegeCatalog?.PrivilegeCategory,
                ProfessionId = x.ClinicalPrivilegeCatalog?.ProfessionId,
                ProfessionName = x.ClinicalPrivilegeCatalog?.Profession?.ProfessionName,
                SpecializationId = x.ClinicalPrivilegeCatalog?.SpecializationId,
                SpecializationName = x.ClinicalPrivilegeCatalog?.Specialization?.SpecializationName,
                CredentialingDecisionId = x.CredentialingDecisionId,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department?.DepartmentName,
                PositionId = x.PositionId,
                PositionName = x.Position?.PositionName,
                SupervisorUserId = x.SupervisorUserId,
                SupervisorUserName = GetActorName(actorNames, x.SupervisorUserId ?? Guid.Empty),
                PrivilegeCode = x.PrivilegeCode,
                PrivilegeName = x.PrivilegeName,
                PrivilegeType = x.PrivilegeType,
                PrivilegeTypeName = BuildEnumLabel(x.PrivilegeType.ToString()),
                ClinicalScope = x.ClinicalScope,
                SpecialtyName = x.SpecialtyName,
                SubSpecialtyName = x.SubSpecialtyName,
                ProcedureGroup = x.ProcedureGroup,
                ProcedureName = x.ProcedureName,
                PracticeLocation = x.PracticeLocation,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                IsExpired = x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today,
                DaysUntilExpiry = days,
                PrivilegeStatus = x.PrivilegeStatus,
                PrivilegeStatusName = BuildEnumLabel(x.PrivilegeStatus.ToString()),
                IsTemporary = x.IsTemporary,
                IsEmergencyPrivilege = x.IsEmergencyPrivilege,
                IsSupervisionRequired = x.IsSupervisionRequired,
                Restrictions = x.Restrictions,
                IsSchedulingBlocked = x.IsSchedulingBlocked,
                IsClinicalServiceBlocked = x.IsClinicalServiceBlocked,
                SupportingFilePath = x.SupportingFilePath,
                SupportingFileContentType = x.SupportingFileContentType,
                HasSupportingFile = !string.IsNullOrWhiteSpace(x.SupportingFilePath),
                GrantedAt = x.GrantedAt,
                GrantedByUserId = x.GrantedByUserId,
                GrantedByUserName = GetActorName(actorNames, x.GrantedByUserId ?? Guid.Empty),
                ApprovalNotes = x.ApprovalNotes,
                RejectedAt = x.RejectedAt,
                RejectedByUserId = x.RejectedByUserId,
                RejectedByUserName = GetActorName(actorNames, x.RejectedByUserId ?? Guid.Empty),
                RejectionReason = x.RejectionReason,
                SuspendedAt = x.SuspendedAt,
                SuspendedByUserId = x.SuspendedByUserId,
                SuspendedByUserName = GetActorName(actorNames, x.SuspendedByUserId ?? Guid.Empty),
                SuspensionReason = x.SuspensionReason,
                RevokedAt = x.RevokedAt,
                RevokedByUserId = x.RevokedByUserId,
                RevokedByUserName = GetActorName(actorNames, x.RevokedByUserId ?? Guid.Empty),
                RevocationReason = x.RevocationReason,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy)
            };
        }

        private static WfpClinicalPrivilegeDetailResponse MapDetailResponse(
            WfpClinicalPrivilege x,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var value = MapResponse(x, actorNames);
            return new WfpClinicalPrivilegeDetailResponse
            {
                Id = value.Id,
                WorkforceProfileId = value.WorkforceProfileId,
                WorkforceProfileCode = value.WorkforceProfileCode,
                WorkforceDisplayName = value.WorkforceDisplayName,
                CredentialLicenseId = value.CredentialLicenseId,
                CredentialLicenseType = value.CredentialLicenseType,
                CredentialLicenseNumber = value.CredentialLicenseNumber,
                ClinicalPrivilegeCatalogId = value.ClinicalPrivilegeCatalogId,
                ClinicalPrivilegeCatalogCode = value.ClinicalPrivilegeCatalogCode,
                ClinicalPrivilegeCatalogName = value.ClinicalPrivilegeCatalogName,
                PrivilegeCategory = value.PrivilegeCategory,
                ProfessionId = value.ProfessionId,
                ProfessionName = value.ProfessionName,
                SpecializationId = value.SpecializationId,
                SpecializationName = value.SpecializationName,
                CredentialingDecisionId = value.CredentialingDecisionId,
                DepartmentId = value.DepartmentId,
                DepartmentName = value.DepartmentName,
                PositionId = value.PositionId,
                PositionName = value.PositionName,
                SupervisorUserId = value.SupervisorUserId,
                SupervisorUserName = value.SupervisorUserName,
                PrivilegeCode = value.PrivilegeCode,
                PrivilegeName = value.PrivilegeName,
                PrivilegeType = value.PrivilegeType,
                PrivilegeTypeName = value.PrivilegeTypeName,
                ClinicalScope = value.ClinicalScope,
                SpecialtyName = value.SpecialtyName,
                SubSpecialtyName = value.SubSpecialtyName,
                ProcedureGroup = value.ProcedureGroup,
                ProcedureName = value.ProcedureName,
                PracticeLocation = value.PracticeLocation,
                EffectiveStartDate = value.EffectiveStartDate,
                EffectiveEndDate = value.EffectiveEndDate,
                IsExpired = value.IsExpired,
                DaysUntilExpiry = value.DaysUntilExpiry,
                PrivilegeStatus = value.PrivilegeStatus,
                PrivilegeStatusName = value.PrivilegeStatusName,
                IsTemporary = value.IsTemporary,
                IsEmergencyPrivilege = value.IsEmergencyPrivilege,
                IsSupervisionRequired = value.IsSupervisionRequired,
                Restrictions = value.Restrictions,
                IsSchedulingBlocked = value.IsSchedulingBlocked,
                IsClinicalServiceBlocked = value.IsClinicalServiceBlocked,
                SupportingFilePath = value.SupportingFilePath,
                SupportingFileContentType = value.SupportingFileContentType,
                HasSupportingFile = value.HasSupportingFile,
                GrantedAt = value.GrantedAt,
                GrantedByUserId = value.GrantedByUserId,
                GrantedByUserName = value.GrantedByUserName,
                ApprovalNotes = value.ApprovalNotes,
                RejectedAt = value.RejectedAt,
                RejectedByUserId = value.RejectedByUserId,
                RejectedByUserName = value.RejectedByUserName,
                RejectionReason = value.RejectionReason,
                SuspendedAt = value.SuspendedAt,
                SuspendedByUserId = value.SuspendedByUserId,
                SuspendedByUserName = value.SuspendedByUserName,
                SuspensionReason = value.SuspensionReason,
                RevokedAt = value.RevokedAt,
                RevokedByUserId = value.RevokedByUserId,
                RevokedByUserName = value.RevokedByUserName,
                RevocationReason = value.RevocationReason,
                Description = value.Description,
                IsActive = value.IsActive,
                CreateDateTime = value.CreateDateTime,
                CreateBy = value.CreateBy,
                CreateByName = value.CreateByName,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            };
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds,
            CancellationToken cancellationToken)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId) =>
            actorId == Guid.Empty
                ? null
                : actorNames.TryGetValue(actorId, out var value) ? value : null;

        private static List<WfpClinicalPrivilegeEnumOptionResponse> BuildEnumOptions<TEnum>()
            where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(x => new WfpClinicalPrivilegeEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x.ToString())
                })
                .ToList();
        }

        private static bool TryResolveStatus(out ClinicalPrivilegeStatus status, params string[] names)
        {
            foreach (var name in names)
            {
                if (Enum.TryParse(name, true, out status) && Enum.IsDefined(typeof(ClinicalPrivilegeStatus), status))
                    return true;
            }

            status = default;
            return false;
        }

        private static bool IsStatusName(ClinicalPrivilegeStatus status, params string[] names) =>
            names.Any(x => status.ToString().Equals(x, StringComparison.OrdinalIgnoreCase));

        private static string BuildEnumLabel(string value) =>
            Regex.Replace(value, "([a-z0-9])([A-Z])", "$1 $2");

        private Guid GetCurrentUserId()
        {
            var text = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(text, out var id) ? id : Guid.Empty;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private static string? NormalizeNullableText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static DateTime NormalizeUtcDate(DateTime value) =>
            DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

        private static DateTime? NormalizeNullableUtcDate(DateTime? value) =>
            value.HasValue ? NormalizeUtcDate(value.Value) : null;
    }
}
