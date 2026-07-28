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

using ResponseClinicalPrivilegeCatalogPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs.ClinicalPrivilegeCatalogResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/clinical-privilege-catalogs")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Clinical Privilege Catalog",
        AreaName = "Corporate",
        ControllerName = "ClinicalPrivilegeCatalog",
        Description = "Corporate human resource master data clinical privilege catalog",
        SortOrder = 18)]
    [Tags("Corporate / Human Resource / Master Data / Clinical Privilege Catalog")]
    public class ClinicalPrivilegeCatalogController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "CPC-RSMMC-";
        private const int CodeNumberLength = 5;

        private static readonly string[] PrivilegeCategories =
        {
            "ClinicalProcedure", "ClinicalService", "Diagnostic", "Surgical",
            "Prescribing", "Emergency", "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ClinicalPrivilegeCatalogController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalPrivilegeCatalogFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Privilege Catalog", Description = "Melihat metadata filter clinical privilege catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalPrivilegeCatalog", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ClinicalPrivilegeCatalogFilterMetadataResponse
            {
                DefaultFilter = new ClinicalPrivilegeCatalogDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                PrivilegeCategoryOptions = PrivilegeCategories.Select(x => new ClinicalPrivilegeCatalogStringOptionResponse { Value = x, Label = x }).ToList(),
                CompetencyLevelOptions = Enum.GetValues<CompetencyLevel>()
                    .Select(x => new ClinicalPrivilegeCatalogEnumOptionResponse
                    {
                        Value = Convert.ToInt32(x),
                        Name = x.ToString(),
                        Label = x.ToString()
                    })
                    .ToList(),
                SortOptions = new List<ClinicalPrivilegeCatalogSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "privilegeCode", Label = "Kode privilege" },
                    new() { Value = "privilegeName", Label = "Nama privilege" },
                    new() { Value = "privilegeCategory", Label = "Kategori privilege" },
                    new() { Value = "professionName", Label = "Profession" },
                    new() { Value = "specializationName", Label = "Specialization" },
                    new() { Value = "minimumCompetencyLevel", Label = "Minimum competency level" },
                    new() { Value = "isHighRisk", Label = "High risk" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "ClinicalPrivilegeCatalog.GetFilterMetadata", "Mengambil metadata filter clinical privilege catalog.", result);
            return Ok(ApiResponse<ClinicalPrivilegeCatalogFilterMetadataResponse>.Ok(result, "Metadata filter clinical privilege catalog berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalPrivilegeCatalogSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Privilege Catalog", Description = "Melihat ringkasan clinical privilege catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalPrivilegeCatalog", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();
            var result = new ClinicalPrivilegeCatalogSummaryResponse
            {
                TotalPrivilegeCatalog = await query.CountAsync(),
                ActivePrivilegeCatalog = await query.CountAsync(x => x.IsActive),
                InactivePrivilegeCatalog = await query.CountAsync(x => !x.IsActive),
                HighRiskPrivilegeCatalog = await query.CountAsync(x => x.IsHighRisk),
                SupervisionRequiredPrivilegeCatalog = await query.CountAsync(x => x.RequiresSupervision),
                IndependentPracticePrivilegeCatalog = await query.CountAsync(x => x.AllowsIndependentPractice),
                PrivilegeCatalogWithCompetency = await query.CountAsync(x => x.RequiredCompetencyId.HasValue)
            };

            return Ok(ApiResponse<ClinicalPrivilegeCatalogSummaryResponse>.Ok(result, "Ringkasan clinical privilege catalog berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseClinicalPrivilegeCatalogPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Privilege Catalog", Description = "Melihat data clinical privilege catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalPrivilegeCatalog", "Read")]
        public async Task<IActionResult> GetClinicalPrivilegeCatalogs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? professionId,
            [FromQuery] Guid? specializationId,
            [FromQuery] Guid? requiredCompetencyId,
            [FromQuery] string? privilegeCategory,
            [FromQuery] bool? requiresSupervision,
            [FromQuery] bool? allowsIndependentPractice,
            [FromQuery] bool? isHighRisk,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "privilegeName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, professionId, specializationId, requiredCompetencyId, privilegeCategory, requiresSupervision, allowsIndependentPractice, isHighRisk, isActive, search);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ClinicalPrivilegeCatalogResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : null,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    SpecializationId = x.SpecializationId,
                    SpecializationCode = x.Specialization != null ? x.Specialization.SpecializationCode : null,
                    SpecializationName = x.Specialization != null ? x.Specialization.SpecializationName : null,
                    RequiredCompetencyId = x.RequiredCompetencyId,
                    RequiredCompetencyCode = x.RequiredCompetency != null ? x.RequiredCompetency.CompetencyCode : null,
                    RequiredCompetencyName = x.RequiredCompetency != null ? x.RequiredCompetency.CompetencyName : null,
                    PrivilegeCode = x.PrivilegeCode,
                    PrivilegeName = x.PrivilegeName,
                    PrivilegeCategory = x.PrivilegeCategory,
                    ReferenceProcedureCode = x.ReferenceProcedureCode,
                    MinimumCompetencyLevel = x.MinimumCompetencyLevel,
                    MinimumExperienceMonths = x.MinimumExperienceMonths,
                    RequiresSupervision = x.RequiresSupervision,
                    AllowsIndependentPractice = x.AllowsIndependentPractice,
                    IsHighRisk = x.IsHighRisk,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkforceClinicalPrivilegeCount = _dbContext.Set<WfpClinicalPrivilege>().Count(p => p.ClinicalPrivilegeCatalogId == x.Id && !p.IsDelete),
                    CredentialingRequirementCount = _dbContext.MstCredentialingRequirements.Count(r => r.ClinicalPrivilegeCatalogId == x.Id && !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ApiResponse<ResponseClinicalPrivilegeCatalogPagedResult>.Ok(
                new ResponseClinicalPrivilegeCatalogPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data clinical privilege catalog berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalPrivilegeCatalogOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Privilege Catalog", Description = "Melihat pilihan clinical privilege catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalPrivilegeCatalog", "Read")]
        public async Task<IActionResult> GetClinicalPrivilegeCatalogOptions(
            [FromQuery] Guid? professionId,
            [FromQuery] Guid? specializationId,
            [FromQuery] Guid? requiredCompetencyId,
            [FromQuery] string? privilegeCategory,
            [FromQuery] bool? requiresSupervision,
            [FromQuery] bool? isHighRisk,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(BuildBaseQuery(), professionId, specializationId, requiredCompetencyId, privilegeCategory, requiresSupervision, null, isHighRisk, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.PrivilegeName)
                .ThenBy(x => x.PrivilegeCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ClinicalPrivilegeCatalogOptionResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    SpecializationId = x.SpecializationId,
                    PrivilegeCode = x.PrivilegeCode,
                    PrivilegeName = x.PrivilegeName,
                    PrivilegeCategory = x.PrivilegeCategory,
                    RequiresSupervision = x.RequiresSupervision,
                    AllowsIndependentPractice = x.AllowsIndependentPractice,
                    IsHighRisk = x.IsHighRisk,
                    DefaultValidityMonths = x.DefaultValidityMonths
                })
                .ToListAsync();

            return Ok(ApiResponse<ClinicalPrivilegeCatalogOptionPagedResponse>.Ok(
                new ClinicalPrivilegeCatalogOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan clinical privilege catalog berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalPrivilegeCatalogDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Clinical Privilege Catalog", Description = "Melihat detail clinical privilege catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalPrivilegeCatalog", "Read")]
        public async Task<IActionResult> GetClinicalPrivilegeCatalogById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new ClinicalPrivilegeCatalogDetailResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : null,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    SpecializationId = x.SpecializationId,
                    SpecializationCode = x.Specialization != null ? x.Specialization.SpecializationCode : null,
                    SpecializationName = x.Specialization != null ? x.Specialization.SpecializationName : null,
                    RequiredCompetencyId = x.RequiredCompetencyId,
                    RequiredCompetencyCode = x.RequiredCompetency != null ? x.RequiredCompetency.CompetencyCode : null,
                    RequiredCompetencyName = x.RequiredCompetency != null ? x.RequiredCompetency.CompetencyName : null,
                    PrivilegeCode = x.PrivilegeCode,
                    PrivilegeName = x.PrivilegeName,
                    PrivilegeCategory = x.PrivilegeCategory,
                    ReferenceProcedureCode = x.ReferenceProcedureCode,
                    MinimumCompetencyLevel = x.MinimumCompetencyLevel,
                    MinimumExperienceMonths = x.MinimumExperienceMonths,
                    RequiresSupervision = x.RequiresSupervision,
                    AllowsIndependentPractice = x.AllowsIndependentPractice,
                    IsHighRisk = x.IsHighRisk,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkforceClinicalPrivilegeCount = _dbContext.Set<WfpClinicalPrivilege>().Count(p => p.ClinicalPrivilegeCatalogId == x.Id && !p.IsDelete),
                    CredentialingRequirementCount = _dbContext.MstCredentialingRequirements.Count(r => r.ClinicalPrivilegeCatalogId == x.Id && !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Clinical privilege catalog tidak ditemukan."));

            return Ok(ApiResponse<ClinicalPrivilegeCatalogDetailResponse>.Ok(data, "Detail clinical privilege catalog berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ClinicalPrivilegeCatalogCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Clinical Privilege Catalog", Description = "Membuat data clinical privilege catalog", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ClinicalPrivilegeCatalog", "Create")]
        public async Task<IActionResult> CreateClinicalPrivilegeCatalog([FromBody] CreateClinicalPrivilegeCatalogRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data clinical privilege catalog tidak valid."));

            var entity = new MstClinicalPrivilegeCatalog
            {
                Id = Guid.NewGuid(),
                ProfessionId = NormalizeGuid(request.ProfessionId),
                SpecializationId = NormalizeGuid(request.SpecializationId),
                RequiredCompetencyId = NormalizeGuid(request.RequiredCompetencyId),
                PrivilegeCode = await GenerateCodeAsync(),
                PrivilegeName = request.PrivilegeName.Trim(),
                PrivilegeCategory = NormalizePrivilegeCategory(request.PrivilegeCategory),
                ReferenceProcedureCode = NormalizeNullableString(request.ReferenceProcedureCode),
                MinimumCompetencyLevel = request.MinimumCompetencyLevel,
                MinimumExperienceMonths = request.MinimumExperienceMonths,
                RequiresSupervision = request.RequiresSupervision,
                AllowsIndependentPractice = request.AllowsIndependentPractice,
                IsHighRisk = request.IsHighRisk,
                DefaultValidityMonths = request.DefaultValidityMonths,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstClinicalPrivilegeCatalogs.Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new ClinicalPrivilegeCatalogCreateResponse
            {
                Id = entity.Id,
                PrivilegeCode = entity.PrivilegeCode,
                PrivilegeName = entity.PrivilegeName,
                PrivilegeCategory = entity.PrivilegeCategory,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(LogCategory, "ClinicalPrivilegeCatalog.CreateClinicalPrivilegeCatalog", "Membuat data clinical privilege catalog.", result);
            return Ok(ApiResponse<ClinicalPrivilegeCatalogCreateResponse>.Ok(result, "Clinical privilege catalog berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Clinical Privilege Catalog", Description = "Mengubah data clinical privilege catalog", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ClinicalPrivilegeCatalog", "Update")]
        public async Task<IActionResult> UpdateClinicalPrivilegeCatalog(Guid id, [FromBody] UpdateClinicalPrivilegeCatalogRequest request)
        {
            var entity = await _dbContext.MstClinicalPrivilegeCatalogs.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Clinical privilege catalog tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data clinical privilege catalog tidak valid."));

            entity.ProfessionId = NormalizeGuid(request.ProfessionId);
            entity.SpecializationId = NormalizeGuid(request.SpecializationId);
            entity.RequiredCompetencyId = NormalizeGuid(request.RequiredCompetencyId);
            entity.PrivilegeName = request.PrivilegeName.Trim();
            entity.PrivilegeCategory = NormalizePrivilegeCategory(request.PrivilegeCategory);
            entity.ReferenceProcedureCode = NormalizeNullableString(request.ReferenceProcedureCode);
            entity.MinimumCompetencyLevel = request.MinimumCompetencyLevel;
            entity.MinimumExperienceMonths = request.MinimumExperienceMonths;
            entity.RequiresSupervision = request.RequiresSupervision;
            entity.AllowsIndependentPractice = request.AllowsIndependentPractice;
            entity.IsHighRisk = request.IsHighRisk;
            entity.DefaultValidityMonths = request.DefaultValidityMonths;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "ClinicalPrivilegeCatalog.UpdateClinicalPrivilegeCatalog", "Mengubah data clinical privilege catalog.", new { entity.Id, entity.PrivilegeCode, entity.PrivilegeName, entity.IsActive });
            return Ok(ApiResponse<object>.Ok(null, "Clinical privilege catalog berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Clinical Privilege Catalog Status", Description = "Mengubah status clinical privilege catalog", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("ClinicalPrivilegeCatalog", "Update")]
        public async Task<IActionResult> UpdateClinicalPrivilegeCatalogStatus(Guid id, [FromBody] UpdateClinicalPrivilegeCatalogStatusRequest request)
        {
            var entity = await _dbContext.MstClinicalPrivilegeCatalogs.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Clinical privilege catalog tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status clinical privilege catalog berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Clinical Privilege Catalog", Description = "Menghapus clinical privilege catalog", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("ClinicalPrivilegeCatalog", "Delete")]
        public async Task<IActionResult> DeleteClinicalPrivilegeCatalog(Guid id)
        {
            var entity = await _dbContext.MstClinicalPrivilegeCatalogs.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Clinical privilege catalog tidak ditemukan."));

            var isUsed =
                await _dbContext.Set<WfpClinicalPrivilege>().AsNoTracking().AnyAsync(x => x.ClinicalPrivilegeCatalogId == id && !x.IsDelete) ||
                await _dbContext.MstCredentialingRequirements.AsNoTracking().AnyAsync(x => x.ClinicalPrivilegeCatalogId == id && !x.IsDelete);

            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Clinical privilege catalog tidak dapat dihapus karena sudah digunakan oleh workforce clinical privilege atau credentialing requirement."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "ClinicalPrivilegeCatalog.DeleteClinicalPrivilegeCatalog", "Menghapus data clinical privilege catalog.", new { entity.Id, entity.PrivilegeCode, entity.PrivilegeName, entity.DeleteDateTime });
            return Ok(ApiResponse<object>.Ok(null, "Clinical privilege catalog berhasil dihapus."));
        }

        private IQueryable<MstClinicalPrivilegeCatalog> BuildBaseQuery() =>
            _dbContext.MstClinicalPrivilegeCatalogs.AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstClinicalPrivilegeCatalog> ApplyDateFilter(IQueryable<MstClinicalPrivilegeCatalog> query, DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstClinicalPrivilegeCatalog> ApplyStandardFilter(
            IQueryable<MstClinicalPrivilegeCatalog> query,
            Guid? professionId,
            Guid? specializationId,
            Guid? requiredCompetencyId,
            string? privilegeCategory,
            bool? requiresSupervision,
            bool? allowsIndependentPractice,
            bool? isHighRisk,
            bool? isActive,
            string? search)
        {
            if (professionId.HasValue && professionId.Value != Guid.Empty) query = query.Where(x => x.ProfessionId == professionId.Value);
            if (specializationId.HasValue && specializationId.Value != Guid.Empty) query = query.Where(x => x.SpecializationId == specializationId.Value);
            if (requiredCompetencyId.HasValue && requiredCompetencyId.Value != Guid.Empty) query = query.Where(x => x.RequiredCompetencyId == requiredCompetencyId.Value);
            if (!string.IsNullOrWhiteSpace(privilegeCategory)) query = query.Where(x => x.PrivilegeCategory == privilegeCategory.Trim());
            if (requiresSupervision.HasValue) query = query.Where(x => x.RequiresSupervision == requiresSupervision.Value);
            if (allowsIndependentPractice.HasValue) query = query.Where(x => x.AllowsIndependentPractice == allowsIndependentPractice.Value);
            if (isHighRisk.HasValue) query = query.Where(x => x.IsHighRisk == isHighRisk.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.PrivilegeCode.ToLower().Contains(keyword) ||
                    x.PrivilegeName.ToLower().Contains(keyword) ||
                    x.PrivilegeCategory.ToLower().Contains(keyword) ||
                    x.ReferenceProcedureCode != null && x.ReferenceProcedureCode.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword) ||
                    x.Profession != null && x.Profession.ProfessionName.ToLower().Contains(keyword) ||
                    x.Specialization != null && x.Specialization.SpecializationName.ToLower().Contains(keyword) ||
                    x.RequiredCompetency != null && x.RequiredCompetency.CompetencyName.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstClinicalPrivilegeCatalog> ApplySorting(IQueryable<MstClinicalPrivilegeCatalog> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "privilegeName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "privilegecode" => desc ? query.OrderByDescending(x => x.PrivilegeCode) : query.OrderBy(x => x.PrivilegeCode),
                "privilegecategory" => desc ? query.OrderByDescending(x => x.PrivilegeCategory) : query.OrderBy(x => x.PrivilegeCategory),
                "professionname" => desc ? query.OrderByDescending(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty) : query.OrderBy(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty),
                "specializationname" => desc ? query.OrderByDescending(x => x.Specialization != null ? x.Specialization.SpecializationName : string.Empty) : query.OrderBy(x => x.Specialization != null ? x.Specialization.SpecializationName : string.Empty),
                "minimumcompetencylevel" => desc ? query.OrderByDescending(x => x.MinimumCompetencyLevel) : query.OrderBy(x => x.MinimumCompetencyLevel),
                "ishighrisk" => desc ? query.OrderByDescending(x => x.IsHighRisk) : query.OrderBy(x => x.IsHighRisk),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.PrivilegeName).ThenByDescending(x => x.PrivilegeCode) : query.OrderBy(x => x.PrivilegeName).ThenBy(x => x.PrivilegeCode)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateClinicalPrivilegeCatalogRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PrivilegeName)) return (false, "Nama privilege wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.PrivilegeCategory)) return (false, "Kategori privilege wajib diisi.");
            if (request.MinimumExperienceMonths < 0) return (false, "Minimum experience months tidak boleh negatif.");
            if (request.DefaultValidityMonths.HasValue && request.DefaultValidityMonths.Value <= 0) return (false, "Default validity months harus lebih besar dari 0.");
            if (request.RequiresSupervision && request.AllowsIndependentPractice) return (false, "Privilege yang membutuhkan supervisi tidak dapat sekaligus mengizinkan praktik mandiri.");

            var category = NormalizePrivilegeCategory(request.PrivilegeCategory);
            var professionId = NormalizeGuid(request.ProfessionId);
            var specializationId = NormalizeGuid(request.SpecializationId);
            var competencyId = NormalizeGuid(request.RequiredCompetencyId);

            if (professionId.HasValue && !await _dbContext.MstProfessions.AsNoTracking().AnyAsync(x => x.Id == professionId.Value && x.IsActive && !x.IsDelete))
                return (false, "Profession tidak ditemukan atau tidak aktif.");

            if (competencyId.HasValue && !await _dbContext.MstCompetencies.AsNoTracking().AnyAsync(x => x.Id == competencyId.Value && x.IsActive && !x.IsDelete))
                return (false, "Required competency tidak ditemukan atau tidak aktif.");

            if (specializationId.HasValue)
            {
                var specialization = await _dbContext.MstSpecializations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == specializationId.Value && x.IsActive && !x.IsDelete);
                if (specialization == null) return (false, "Specialization tidak ditemukan atau tidak aktif.");
                if (professionId.HasValue && specialization.ProfessionId != professionId.Value) return (false, "Specialization tidak sesuai dengan profession.");
            }

            var normalizedName = request.PrivilegeName.Trim().ToLower();
            var duplicateQuery = _dbContext.MstClinicalPrivilegeCatalogs.AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.PrivilegeName.ToLower() == normalizedName &&
                x.PrivilegeCategory == category &&
                x.ProfessionId == professionId &&
                x.SpecializationId == specializationId);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Clinical privilege catalog dengan nama, kategori, profession, dan specialization tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.MstClinicalPrivilegeCatalogs.AsNoTracking().Where(x => !x.IsDelete && x.PrivilegeCode.StartsWith(CodePrefix)).Select(x => x.PrivilegeCode).ToListAsync();
            return GenerateNextCode(codes, CodePrefix, CodeNumberLength);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string NormalizePrivilegeCategory(string value) => PrivilegeCategories.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)) ?? value.Trim();
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

        private static List<ClinicalPrivilegeCatalogCustomPeriodOptionResponse> BuildPeriodOptions() =>
            new()
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
    }
}
