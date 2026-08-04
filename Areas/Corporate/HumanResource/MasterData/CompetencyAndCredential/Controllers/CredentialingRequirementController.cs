using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseCredentialingRequirementPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs.CredentialingRequirementResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/credentialing-requirements")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Credentialing Requirement",
        AreaName = "Corporate",
        ControllerName = "CredentialingRequirement",
        Description = "Corporate human resource master data credentialing requirement",
        SortOrder = 17)]
    [Tags("Corporate / Human Resource / Master Data / Credentialing Requirement")]
    public class CredentialingRequirementController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "CRQ-RSMMC-";
        private const int CodeNumberLength = 5;

        private static readonly string[] RequirementTypes =
        {
            "Document", "Competency", "Training", "Certification",
            "License", "Experience", "ClinicalPrivilege"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public CredentialingRequirementController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<CredentialingRequirementFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Credentialing Requirement", Description = "Melihat metadata filter credentialing requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CredentialingRequirement", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new CredentialingRequirementFilterMetadataResponse
            {
                DefaultFilter = new CredentialingRequirementDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                RequirementTypeOptions = RequirementTypes.Select(x => new CredentialingRequirementStringOptionResponse { Value = x, Label = x }).ToList(),
                CompetencyLevelOptions = Enum.GetValues<CompetencyLevel>()
                    .Select(x => new CredentialingRequirementEnumOptionResponse
                    {
                        Value = Convert.ToInt32(x),
                        Name = x.ToString(),
                        Label = x.ToString()
                    })
                    .ToList(),
                SortOptions = new List<CredentialingRequirementSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "requirementCode", Label = "Kode requirement" },
                    new() { Value = "requirementName", Label = "Nama requirement" },
                    new() { Value = "requirementType", Label = "Tipe requirement" },
                    new() { Value = "professionName", Label = "Profession" },
                    new() { Value = "specializationName", Label = "Specialization" },
                    new() { Value = "positionName", Label = "Position" },
                    new() { Value = "isMandatory", Label = "Mandatory" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal efektif" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "CredentialingRequirement.GetFilterMetadata", "Mengambil metadata filter credentialing requirement.", result);
            return Ok(ApiResponse<CredentialingRequirementFilterMetadataResponse>.Ok(result, "Metadata filter credentialing requirement berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CredentialingRequirementSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Credentialing Requirement", Description = "Melihat ringkasan credentialing requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CredentialingRequirement", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();
            var today = DateTime.UtcNow.Date;
            var result = new CredentialingRequirementSummaryResponse
            {
                TotalRequirement = await query.CountAsync(),
                ActiveRequirement = await query.CountAsync(x => x.IsActive),
                InactiveRequirement = await query.CountAsync(x => !x.IsActive),
                MandatoryRequirement = await query.CountAsync(x => x.IsMandatory),
                DocumentRequiredRequirement = await query.CountAsync(x => x.RequiresDocument),
                VerificationRequiredRequirement = await query.CountAsync(x => x.RequiresVerification),
                ExpiryRequiredRequirement = await query.CountAsync(x => x.RequiresExpiryDate),
                CurrentlyEffectiveRequirement = await query.CountAsync(x =>
                    x.IsActive &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
            };

            return Ok(ApiResponse<CredentialingRequirementSummaryResponse>.Ok(result, "Ringkasan credentialing requirement berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseCredentialingRequirementPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Credentialing Requirement", Description = "Melihat data credentialing requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CredentialingRequirement", "Read")]
        public async Task<IActionResult> GetCredentialingRequirements(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? professionId,
            [FromQuery] Guid? specializationId,
            [FromQuery] Guid? positionId,
            [FromQuery] string? requirementType,
            [FromQuery] bool? isMandatory,
            [FromQuery] bool? requiresVerification,
            [FromQuery] bool? isCurrentlyEffective,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "requirementName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, professionId, specializationId, positionId, requirementType, isMandatory, requiresVerification, isCurrentlyEffective, isActive, search);
            var totalData = await query.CountAsync();
            var today = DateTime.UtcNow.Date;

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CredentialingRequirementResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : null,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    SpecializationId = x.SpecializationId,
                    SpecializationCode = x.Specialization != null ? x.Specialization.SpecializationCode : null,
                    SpecializationName = x.Specialization != null ? x.Specialization.SpecializationName : null,
                    PositionId = x.PositionId,
                    PositionCode = x.Position != null ? x.Position.PositionCode : null,
                    PositionName = x.Position != null ? x.Position.PositionName : null,
                    CompetencyId = x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : null,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : null,
                    TrainingCatalogId = x.TrainingCatalogId,
                    CertificationTypeId = x.CertificationTypeId,
                    CertificationTypeCode = x.CertificationType != null ? x.CertificationType.CertificationTypeCode : null,
                    CertificationTypeName = x.CertificationType != null ? x.CertificationType.CertificationTypeName : null,
                    LicenseTypeId = x.LicenseTypeId,
                    LicenseTypeCode = x.LicenseType != null ? x.LicenseType.LicenseTypeCode : null,
                    LicenseTypeName = x.LicenseType != null ? x.LicenseType.LicenseTypeName : null,
                    ClinicalPrivilegeCatalogId = x.ClinicalPrivilegeCatalogId,
                    ClinicalPrivilegeCode = x.ClinicalPrivilegeCatalog != null ? x.ClinicalPrivilegeCatalog.PrivilegeCode : null,
                    ClinicalPrivilegeName = x.ClinicalPrivilegeCatalog != null ? x.ClinicalPrivilegeCatalog.PrivilegeName : null,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    RequirementType = x.RequirementType,
                    MinimumCompetencyLevel = x.MinimumCompetencyLevel,
                    MinimumExperienceMonths = x.MinimumExperienceMonths,
                    RequiredQuantity = x.RequiredQuantity,
                    ValidityMonths = x.ValidityMonths,
                    IsMandatory = x.IsMandatory,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsCurrentlyEffective = x.IsActive && (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today),
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkforceCertificationCount = _dbContext.Set<WfpCertification>().Count(c => c.CredentialingRequirementId == x.Id && !c.IsDelete),
                    WorkforceLicenseCount = _dbContext.Set<WfpCredentialLicense>().Count(c => c.CredentialingRequirementId == x.Id && !c.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ApiResponse<ResponseCredentialingRequirementPagedResult>.Ok(
                new ResponseCredentialingRequirementPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data credentialing requirement berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<CredentialingRequirementOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Credentialing Requirement", Description = "Melihat pilihan credentialing requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CredentialingRequirement", "Read")]
        public async Task<IActionResult> GetCredentialingRequirementOptions(
            [FromQuery] Guid? professionId,
            [FromQuery] Guid? specializationId,
            [FromQuery] Guid? positionId,
            [FromQuery] string? requirementType,
            [FromQuery] bool? isMandatory,
            [FromQuery] bool onlyCurrentlyEffective = true,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(BuildBaseQuery(), professionId, specializationId, positionId, requirementType, isMandatory, null, onlyCurrentlyEffective ? true : null, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();
            var today = DateTime.UtcNow.Date;
            var items = await query
                .OrderBy(x => x.RequirementName)
                .ThenBy(x => x.RequirementCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CredentialingRequirementOptionResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    RequirementType = x.RequirementType,
                    ProfessionId = x.ProfessionId,
                    SpecializationId = x.SpecializationId,
                    PositionId = x.PositionId,
                    IsMandatory = x.IsMandatory,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    IsCurrentlyEffective = x.IsActive && (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today)
                })
                .ToListAsync();

            return Ok(ApiResponse<CredentialingRequirementOptionPagedResponse>.Ok(
                new CredentialingRequirementOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan credentialing requirement berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CredentialingRequirementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Credentialing Requirement", Description = "Melihat detail credentialing requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CredentialingRequirement", "Read")]
        public async Task<IActionResult> GetCredentialingRequirementById(Guid id)
        {
            var today = DateTime.UtcNow.Date;
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new CredentialingRequirementDetailResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : null,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    SpecializationId = x.SpecializationId,
                    SpecializationCode = x.Specialization != null ? x.Specialization.SpecializationCode : null,
                    SpecializationName = x.Specialization != null ? x.Specialization.SpecializationName : null,
                    PositionId = x.PositionId,
                    PositionCode = x.Position != null ? x.Position.PositionCode : null,
                    PositionName = x.Position != null ? x.Position.PositionName : null,
                    CompetencyId = x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : null,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : null,
                    TrainingCatalogId = x.TrainingCatalogId,
                    CertificationTypeId = x.CertificationTypeId,
                    CertificationTypeCode = x.CertificationType != null ? x.CertificationType.CertificationTypeCode : null,
                    CertificationTypeName = x.CertificationType != null ? x.CertificationType.CertificationTypeName : null,
                    LicenseTypeId = x.LicenseTypeId,
                    LicenseTypeCode = x.LicenseType != null ? x.LicenseType.LicenseTypeCode : null,
                    LicenseTypeName = x.LicenseType != null ? x.LicenseType.LicenseTypeName : null,
                    ClinicalPrivilegeCatalogId = x.ClinicalPrivilegeCatalogId,
                    ClinicalPrivilegeCode = x.ClinicalPrivilegeCatalog != null ? x.ClinicalPrivilegeCatalog.PrivilegeCode : null,
                    ClinicalPrivilegeName = x.ClinicalPrivilegeCatalog != null ? x.ClinicalPrivilegeCatalog.PrivilegeName : null,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    RequirementType = x.RequirementType,
                    MinimumCompetencyLevel = x.MinimumCompetencyLevel,
                    MinimumExperienceMonths = x.MinimumExperienceMonths,
                    RequiredQuantity = x.RequiredQuantity,
                    ValidityMonths = x.ValidityMonths,
                    IsMandatory = x.IsMandatory,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsCurrentlyEffective = x.IsActive && (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today),
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkforceCertificationCount = _dbContext.Set<WfpCertification>().Count(c => c.CredentialingRequirementId == x.Id && !c.IsDelete),
                    WorkforceLicenseCount = _dbContext.Set<WfpCredentialLicense>().Count(c => c.CredentialingRequirementId == x.Id && !c.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Credentialing requirement tidak ditemukan."));

            return Ok(ApiResponse<CredentialingRequirementDetailResponse>.Ok(data, "Detail credentialing requirement berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CredentialingRequirementCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Credentialing Requirement", Description = "Membuat data credentialing requirement", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("CredentialingRequirement", "Create")]
        public async Task<IActionResult> CreateCredentialingRequirement([FromBody] CreateCredentialingRequirementRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data credentialing requirement tidak valid."));

            var entity = new MstCredentialingRequirement
            {
                Id = Guid.NewGuid(),
                ProfessionId = NormalizeGuid(request.ProfessionId),
                SpecializationId = NormalizeGuid(request.SpecializationId),
                PositionId = NormalizeGuid(request.PositionId),
                CompetencyId = NormalizeGuid(request.CompetencyId),
                TrainingCatalogId = NormalizeGuid(request.TrainingCatalogId),
                CertificationTypeId = NormalizeGuid(request.CertificationTypeId),
                LicenseTypeId = NormalizeGuid(request.LicenseTypeId),
                ClinicalPrivilegeCatalogId = NormalizeGuid(request.ClinicalPrivilegeCatalogId),
                RequirementCode = await GenerateCodeAsync(),
                RequirementName = request.RequirementName.Trim(),
                RequirementType = NormalizeRequirementType(request.RequirementType),
                MinimumCompetencyLevel = request.MinimumCompetencyLevel,
                MinimumExperienceMonths = request.MinimumExperienceMonths,
                RequiredQuantity = request.RequiredQuantity,
                ValidityMonths = request.ValidityMonths,
                IsMandatory = request.IsMandatory,
                RequiresDocument = request.RequiresDocument,
                RequiresVerification = request.RequiresVerification,
                RequiresExpiryDate = request.RequiresExpiryDate,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstCredentialingRequirements.Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new CredentialingRequirementCreateResponse
            {
                Id = entity.Id,
                RequirementCode = entity.RequirementCode,
                RequirementName = entity.RequirementName,
                RequirementType = entity.RequirementType,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(LogCategory, "CredentialingRequirement.CreateCredentialingRequirement", "Membuat data credentialing requirement.", result);
            return Ok(ApiResponse<CredentialingRequirementCreateResponse>.Ok(result, "Credentialing requirement berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Credentialing Requirement", Description = "Mengubah data credentialing requirement", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CredentialingRequirement", "Update")]
        public async Task<IActionResult> UpdateCredentialingRequirement(Guid id, [FromBody] UpdateCredentialingRequirementRequest request)
        {
            var entity = await _dbContext.MstCredentialingRequirements.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Credentialing requirement tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data credentialing requirement tidak valid."));

            entity.ProfessionId = NormalizeGuid(request.ProfessionId);
            entity.SpecializationId = NormalizeGuid(request.SpecializationId);
            entity.PositionId = NormalizeGuid(request.PositionId);
            entity.CompetencyId = NormalizeGuid(request.CompetencyId);
            entity.TrainingCatalogId = NormalizeGuid(request.TrainingCatalogId);
            entity.CertificationTypeId = NormalizeGuid(request.CertificationTypeId);
            entity.LicenseTypeId = NormalizeGuid(request.LicenseTypeId);
            entity.ClinicalPrivilegeCatalogId = NormalizeGuid(request.ClinicalPrivilegeCatalogId);
            entity.RequirementName = request.RequirementName.Trim();
            entity.RequirementType = NormalizeRequirementType(request.RequirementType);
            entity.MinimumCompetencyLevel = request.MinimumCompetencyLevel;
            entity.MinimumExperienceMonths = request.MinimumExperienceMonths;
            entity.RequiredQuantity = request.RequiredQuantity;
            entity.ValidityMonths = request.ValidityMonths;
            entity.IsMandatory = request.IsMandatory;
            entity.RequiresDocument = request.RequiresDocument;
            entity.RequiresVerification = request.RequiresVerification;
            entity.RequiresExpiryDate = request.RequiresExpiryDate;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "CredentialingRequirement.UpdateCredentialingRequirement", "Mengubah data credentialing requirement.", new { entity.Id, entity.RequirementCode, entity.RequirementName, entity.IsActive });
            return Ok(ApiResponse<object>.Ok(null, "Credentialing requirement berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Credentialing Requirement Status", Description = "Mengubah status credentialing requirement", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("CredentialingRequirement", "Update")]
        public async Task<IActionResult> UpdateCredentialingRequirementStatus(Guid id, [FromBody] UpdateCredentialingRequirementStatusRequest request)
        {
            var entity = await _dbContext.MstCredentialingRequirements.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Credentialing requirement tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status credentialing requirement berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Credentialing Requirement", Description = "Menghapus credentialing requirement", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("CredentialingRequirement", "Delete")]
        public async Task<IActionResult> DeleteCredentialingRequirement(Guid id)
        {
            var entity = await _dbContext.MstCredentialingRequirements.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Credentialing requirement tidak ditemukan."));

            var isUsed =
                await _dbContext.Set<WfpCertification>().AsNoTracking().AnyAsync(x => x.CredentialingRequirementId == id && !x.IsDelete) ||
                await _dbContext.Set<WfpCredentialLicense>().AsNoTracking().AnyAsync(x => x.CredentialingRequirementId == id && !x.IsDelete);

            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Credentialing requirement tidak dapat dihapus karena sudah digunakan oleh workforce certification atau credential license."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "CredentialingRequirement.DeleteCredentialingRequirement", "Menghapus data credentialing requirement.", new { entity.Id, entity.RequirementCode, entity.RequirementName, entity.DeleteDateTime });
            return Ok(ApiResponse<object>.Ok(null, "Credentialing requirement berhasil dihapus."));
        }

        private IQueryable<MstCredentialingRequirement> BuildBaseQuery() =>
            _dbContext.MstCredentialingRequirements.AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstCredentialingRequirement> ApplyDateFilter(IQueryable<MstCredentialingRequirement> query, DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstCredentialingRequirement> ApplyStandardFilter(
            IQueryable<MstCredentialingRequirement> query,
            Guid? professionId,
            Guid? specializationId,
            Guid? positionId,
            string? requirementType,
            bool? isMandatory,
            bool? requiresVerification,
            bool? isCurrentlyEffective,
            bool? isActive,
            string? search)
        {
            if (professionId.HasValue && professionId.Value != Guid.Empty) query = query.Where(x => x.ProfessionId == professionId.Value);
            if (specializationId.HasValue && specializationId.Value != Guid.Empty) query = query.Where(x => x.SpecializationId == specializationId.Value);
            if (positionId.HasValue && positionId.Value != Guid.Empty) query = query.Where(x => x.PositionId == positionId.Value);
            if (!string.IsNullOrWhiteSpace(requirementType)) query = query.Where(x => x.RequirementType == requirementType.Trim());
            if (isMandatory.HasValue) query = query.Where(x => x.IsMandatory == isMandatory.Value);
            if (requiresVerification.HasValue) query = query.Where(x => x.RequiresVerification == requiresVerification.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (isCurrentlyEffective.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                query = isCurrentlyEffective.Value
                    ? query.Where(x => x.IsActive && (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
                    : query.Where(x => !x.IsActive || x.EffectiveStartDate.HasValue && x.EffectiveStartDate.Value > today || x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < today);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequirementCode.ToLower().Contains(keyword) ||
                    x.RequirementName.ToLower().Contains(keyword) ||
                    x.RequirementType.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword) ||
                    x.Profession != null && x.Profession.ProfessionName.ToLower().Contains(keyword) ||
                    x.Specialization != null && x.Specialization.SpecializationName.ToLower().Contains(keyword) ||
                    x.Position != null && x.Position.PositionName.ToLower().Contains(keyword) ||
                    x.Competency != null && x.Competency.CompetencyName.ToLower().Contains(keyword) ||
                    x.CertificationType != null && x.CertificationType.CertificationTypeName.ToLower().Contains(keyword) ||
                    x.LicenseType != null && x.LicenseType.LicenseTypeName.ToLower().Contains(keyword) ||
                    x.ClinicalPrivilegeCatalog != null && x.ClinicalPrivilegeCatalog.PrivilegeName.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstCredentialingRequirement> ApplySorting(IQueryable<MstCredentialingRequirement> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "requirementName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "requirementcode" => desc ? query.OrderByDescending(x => x.RequirementCode) : query.OrderBy(x => x.RequirementCode),
                "requirementtype" => desc ? query.OrderByDescending(x => x.RequirementType) : query.OrderBy(x => x.RequirementType),
                "professionname" => desc ? query.OrderByDescending(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty) : query.OrderBy(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty),
                "specializationname" => desc ? query.OrderByDescending(x => x.Specialization != null ? x.Specialization.SpecializationName : string.Empty) : query.OrderBy(x => x.Specialization != null ? x.Specialization.SpecializationName : string.Empty),
                "positionname" => desc ? query.OrderByDescending(x => x.Position != null ? x.Position.PositionName : string.Empty) : query.OrderBy(x => x.Position != null ? x.Position.PositionName : string.Empty),
                "ismandatory" => desc ? query.OrderByDescending(x => x.IsMandatory) : query.OrderBy(x => x.IsMandatory),
                "effectivestartdate" => desc ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.RequirementName).ThenByDescending(x => x.RequirementCode) : query.OrderBy(x => x.RequirementName).ThenBy(x => x.RequirementCode)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateCredentialingRequirementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RequirementName)) return (false, "Nama requirement wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.RequirementType)) return (false, "Tipe requirement wajib diisi.");
            if (request.MinimumExperienceMonths < 0) return (false, "Minimum experience months tidak boleh negatif.");
            if (request.RequiredQuantity <= 0) return (false, "Required quantity minimal 1.");
            if (request.ValidityMonths.HasValue && request.ValidityMonths.Value <= 0) return (false, "Validity months harus lebih besar dari 0.");
            if (request.EffectiveEndDate.HasValue && request.EffectiveStartDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date) return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");

            var type = NormalizeRequirementType(request.RequirementType);
            if (!RequirementTypes.Contains(type, StringComparer.OrdinalIgnoreCase)) return (false, "Tipe requirement tidak valid.");

            var professionId = NormalizeGuid(request.ProfessionId);
            var specializationId = NormalizeGuid(request.SpecializationId);
            var positionId = NormalizeGuid(request.PositionId);
            var competencyId = NormalizeGuid(request.CompetencyId);
            var trainingCatalogId = NormalizeGuid(request.TrainingCatalogId);
            var certificationTypeId = NormalizeGuid(request.CertificationTypeId);
            var licenseTypeId = NormalizeGuid(request.LicenseTypeId);
            var catalogId = NormalizeGuid(request.ClinicalPrivilegeCatalogId);

            if (professionId.HasValue && !await _dbContext.MstProfessions.AsNoTracking().AnyAsync(x => x.Id == professionId.Value && x.IsActive && !x.IsDelete)) return (false, "Profession tidak ditemukan atau tidak aktif.");
            if (positionId.HasValue && !await _dbContext.MstPositions.AsNoTracking().AnyAsync(x => x.Id == positionId.Value && x.IsActive && !x.IsDelete)) return (false, "Position tidak ditemukan atau tidak aktif.");
            if (competencyId.HasValue && !await _dbContext.MstCompetencies.AsNoTracking().AnyAsync(x => x.Id == competencyId.Value && x.IsActive && !x.IsDelete)) return (false, "Competency tidak ditemukan atau tidak aktif.");
            if (trainingCatalogId.HasValue && !await _dbContext.MstTrainingCatalogs.AsNoTracking().AnyAsync(x => x.Id == trainingCatalogId.Value && !x.IsDelete)) return (false, "Training catalog tidak ditemukan atau tidak aktif.");

            MstSpecialization? specialization = null;
            if (specializationId.HasValue)
            {
                specialization = await _dbContext.MstSpecializations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == specializationId.Value && x.IsActive && !x.IsDelete);
                if (specialization == null) return (false, "Specialization tidak ditemukan atau tidak aktif.");
                if (professionId.HasValue && specialization.ProfessionId != professionId.Value) return (false, "Specialization tidak sesuai dengan profession.");
            }

            MstCertificationType? certificationType = null;
            if (certificationTypeId.HasValue)
            {
                certificationType = await _dbContext.MstCertificationTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == certificationTypeId.Value && x.IsActive && !x.IsDelete);
                if (certificationType == null) return (false, "Certification type tidak ditemukan atau tidak aktif.");
                if (professionId.HasValue && certificationType.ProfessionId.HasValue && certificationType.ProfessionId.Value != professionId.Value) return (false, "Certification type tidak sesuai dengan profession.");
            }

            MstLicenseType? licenseType = null;
            if (licenseTypeId.HasValue)
            {
                licenseType = await _dbContext.MstLicenseTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == licenseTypeId.Value && x.IsActive && !x.IsDelete);
                if (licenseType == null) return (false, "License type tidak ditemukan atau tidak aktif.");
                if (professionId.HasValue && licenseType.ProfessionId.HasValue && licenseType.ProfessionId.Value != professionId.Value) return (false, "License type tidak sesuai dengan profession.");
            }

            MstClinicalPrivilegeCatalog? catalog = null;
            if (catalogId.HasValue)
            {
                catalog = await _dbContext.MstClinicalPrivilegeCatalogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == catalogId.Value && x.IsActive && !x.IsDelete);
                if (catalog == null) return (false, "Clinical privilege catalog tidak ditemukan atau tidak aktif.");
                if (professionId.HasValue && catalog.ProfessionId.HasValue && catalog.ProfessionId.Value != professionId.Value) return (false, "Clinical privilege catalog tidak sesuai dengan profession.");
                if (specializationId.HasValue && catalog.SpecializationId.HasValue && catalog.SpecializationId.Value != specializationId.Value) return (false, "Clinical privilege catalog tidak sesuai dengan specialization.");
            }

            switch (type)
            {
                case "Competency" when !competencyId.HasValue:
                    return (false, "Competency wajib dipilih untuk requirement type Competency.");
                case "Training" when !trainingCatalogId.HasValue:
                    return (false, "Training catalog wajib dipilih untuk requirement type Training.");
                case "Certification" when !certificationTypeId.HasValue:
                    return (false, "Certification type wajib dipilih untuk requirement type Certification.");
                case "License" when !licenseTypeId.HasValue:
                    return (false, "License type wajib dipilih untuk requirement type License.");
                case "ClinicalPrivilege" when !catalogId.HasValue:
                    return (false, "Clinical privilege catalog wajib dipilih untuk requirement type ClinicalPrivilege.");
                case "Experience" when request.MinimumExperienceMonths <= 0:
                    return (false, "Minimum experience months wajib lebih besar dari 0 untuk requirement type Experience.");
            }

            var normalizedName = request.RequirementName.Trim().ToLower();
            var duplicateQuery = _dbContext.MstCredentialingRequirements.AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.RequirementName.ToLower() == normalizedName &&
                x.RequirementType == type &&
                x.ProfessionId == professionId &&
                x.SpecializationId == specializationId &&
                x.PositionId == positionId);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Credentialing requirement dengan nama, tipe, dan scope tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.MstCredentialingRequirements.AsNoTracking().Where(x => !x.IsDelete && x.RequirementCode.StartsWith(CodePrefix)).Select(x => x.RequirementCode).ToListAsync();
            return GenerateNextCode(codes, CodePrefix, CodeNumberLength);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string NormalizeRequirementType(string value) => RequirementTypes.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)) ?? value.Trim();
        private static string? NormalizeNullableString(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize) => (pageNumber < 1 ? 1 : pageNumber, pageSize < 1 ? 25 : Math.Min(pageSize, 100));

        private static string GenerateNextCode(IEnumerable<string> codes, string prefix, int length)
        {
            var used = codes.Select(x => x.Replace(prefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).Where(x => x > 0).ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return prefix + next.ToString().PadLeft(length, '0');
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
                return (startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null, endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null);

            var today = DateTime.UtcNow.Date;
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "last7days" => (today.AddDays(-6), today.AddDays(1)),
                "thismonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => (null, null)
            };
        }

        private static List<CredentialingRequirementCustomPeriodOptionResponse> BuildPeriodOptions() =>
            new()
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
    }
}
