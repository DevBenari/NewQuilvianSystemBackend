using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseSpecializationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs.SpecializationResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/specializations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Specialization",
        AreaName = "Corporate",
        ControllerName = "Specialization",
        Description = "Corporate human resource master data specialization",
        SortOrder = 14)]
    [Tags("Corporate / Human Resource / Master Data / Specialization")]
    public class SpecializationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "SPZ-RSMMC-";
        private const int CodeNumberLength = 5;

        private static readonly string[] SpecializationTypes =
        {
            "Specialization", "SubSpecialization", "Expertise", "PracticeArea"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public SpecializationController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<SpecializationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Specialization", Description = "Melihat metadata filter specialization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Specialization", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new SpecializationFilterMetadataResponse
            {
                DefaultFilter = new SpecializationDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SpecializationTypeOptions = SpecializationTypes
                    .Select(x => new SpecializationStringOptionResponse { Value = x, Label = x })
                    .ToList(),
                SortOptions = new List<SpecializationSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "specializationCode", Label = "Kode specialization" },
                    new() { Value = "specializationName", Label = "Nama specialization" },
                    new() { Value = "professionName", Label = "Profession" },
                    new() { Value = "specializationType", Label = "Tipe specialization" },
                    new() { Value = "childSpecializationCount", Label = "Jumlah sub-specialization" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "Specialization.GetFilterMetadata", "Mengambil metadata filter specialization.", result);
            return Ok(ApiResponse<SpecializationFilterMetadataResponse>.Ok(result, "Metadata filter specialization berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<SpecializationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Specialization", Description = "Melihat ringkasan specialization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Specialization", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();
            var result = new SpecializationSummaryResponse
            {
                TotalSpecialization = await query.CountAsync(),
                ActiveSpecialization = await query.CountAsync(x => x.IsActive),
                InactiveSpecialization = await query.CountAsync(x => !x.IsActive),
                ClinicalSpecialization = await query.CountAsync(x => x.IsClinicalSpecialization),
                CredentialingRequiredSpecialization = await query.CountAsync(x => x.RequiresCredentialing),
                RootSpecialization = await query.CountAsync(x => !x.ParentSpecializationId.HasValue),
                SubSpecialization = await query.CountAsync(x => x.ParentSpecializationId.HasValue)
            };

            return Ok(ApiResponse<SpecializationSummaryResponse>.Ok(result, "Ringkasan specialization berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseSpecializationPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Specialization", Description = "Melihat data specialization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Specialization", "Read")]
        public async Task<IActionResult> GetSpecializations(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? professionId,
            [FromQuery] Guid? parentSpecializationId,
            [FromQuery] string? specializationType,
            [FromQuery] bool? isClinicalSpecialization,
            [FromQuery] bool? requiresCredentialing,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "specializationName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, professionId, parentSpecializationId, specializationType, isClinicalSpecialization, requiresCredentialing, isActive, search);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SpecializationResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : string.Empty,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : string.Empty,
                    ParentSpecializationId = x.ParentSpecializationId,
                    ParentSpecializationCode = x.ParentSpecialization != null ? x.ParentSpecialization.SpecializationCode : null,
                    ParentSpecializationName = x.ParentSpecialization != null ? x.ParentSpecialization.SpecializationName : null,
                    SpecializationCode = x.SpecializationCode,
                    SpecializationName = x.SpecializationName,
                    SpecializationType = x.SpecializationType,
                    IsClinicalSpecialization = x.IsClinicalSpecialization,
                    RequiresCredentialing = x.RequiresCredentialing,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    ChildSpecializationCount = x.ChildSpecializations.Count(c => !c.IsDelete),
                    ClinicalPrivilegeCatalogCount = _dbContext.MstClinicalPrivilegeCatalogs.Count(c => c.SpecializationId == x.Id && !c.IsDelete),
                    CredentialingRequirementCount = _dbContext.MstCredentialingRequirements.Count(r => r.SpecializationId == x.Id && !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ApiResponse<ResponseSpecializationPagedResult>.Ok(
                new ResponseSpecializationPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data specialization berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<SpecializationOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Specialization", Description = "Melihat pilihan specialization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Specialization", "Read")]
        public async Task<IActionResult> GetSpecializationOptions(
            [FromQuery] Guid? professionId,
            [FromQuery] Guid? parentSpecializationId,
            [FromQuery] string? specializationType,
            [FromQuery] bool? isClinicalSpecialization,
            [FromQuery] bool? requiresCredentialing,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(BuildBaseQuery(), professionId, parentSpecializationId, specializationType, isClinicalSpecialization, requiresCredentialing, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.SpecializationName)
                .ThenBy(x => x.SpecializationCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SpecializationOptionResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : string.Empty,
                    ParentSpecializationId = x.ParentSpecializationId,
                    SpecializationCode = x.SpecializationCode,
                    SpecializationName = x.SpecializationName,
                    SpecializationType = x.SpecializationType,
                    IsClinicalSpecialization = x.IsClinicalSpecialization
                })
                .ToListAsync();

            return Ok(ApiResponse<SpecializationOptionPagedResponse>.Ok(
                new SpecializationOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan specialization berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SpecializationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Specialization", Description = "Melihat detail specialization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Specialization", "Read")]
        public async Task<IActionResult> GetSpecializationById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new SpecializationDetailResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : string.Empty,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : string.Empty,
                    ParentSpecializationId = x.ParentSpecializationId,
                    ParentSpecializationCode = x.ParentSpecialization != null ? x.ParentSpecialization.SpecializationCode : null,
                    ParentSpecializationName = x.ParentSpecialization != null ? x.ParentSpecialization.SpecializationName : null,
                    SpecializationCode = x.SpecializationCode,
                    SpecializationName = x.SpecializationName,
                    SpecializationType = x.SpecializationType,
                    IsClinicalSpecialization = x.IsClinicalSpecialization,
                    RequiresCredentialing = x.RequiresCredentialing,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    ChildSpecializationCount = x.ChildSpecializations.Count(c => !c.IsDelete),
                    ClinicalPrivilegeCatalogCount = _dbContext.MstClinicalPrivilegeCatalogs.Count(c => c.SpecializationId == x.Id && !c.IsDelete),
                    CredentialingRequirementCount = _dbContext.MstCredentialingRequirements.Count(r => r.SpecializationId == x.Id && !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Specialization tidak ditemukan."));

            return Ok(ApiResponse<SpecializationDetailResponse>.Ok(data, "Detail specialization berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SpecializationCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Specialization", Description = "Membuat data specialization", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Specialization", "Create")]
        public async Task<IActionResult> CreateSpecialization([FromBody] CreateSpecializationRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data specialization tidak valid."));

            var entity = new MstSpecialization
            {
                Id = Guid.NewGuid(),
                ProfessionId = request.ProfessionId,
                ParentSpecializationId = NormalizeGuid(request.ParentSpecializationId),
                SpecializationCode = await GenerateCodeAsync(),
                SpecializationName = request.SpecializationName.Trim(),
                SpecializationType = NormalizeSpecializationType(request.SpecializationType),
                IsClinicalSpecialization = request.IsClinicalSpecialization,
                RequiresCredentialing = request.RequiresCredentialing,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstSpecializations.Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new SpecializationCreateResponse
            {
                Id = entity.Id,
                ProfessionId = entity.ProfessionId,
                SpecializationCode = entity.SpecializationCode,
                SpecializationName = entity.SpecializationName,
                SpecializationType = entity.SpecializationType,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(LogCategory, "Specialization.CreateSpecialization", "Membuat data specialization.", result);
            return Ok(ApiResponse<SpecializationCreateResponse>.Ok(result, "Specialization berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Specialization", Description = "Mengubah data specialization", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Specialization", "Update")]
        public async Task<IActionResult> UpdateSpecialization(Guid id, [FromBody] UpdateSpecializationRequest request)
        {
            var entity = await _dbContext.MstSpecializations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Specialization tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data specialization tidak valid."));

            entity.ProfessionId = request.ProfessionId;
            entity.ParentSpecializationId = NormalizeGuid(request.ParentSpecializationId);
            entity.SpecializationName = request.SpecializationName.Trim();
            entity.SpecializationType = NormalizeSpecializationType(request.SpecializationType);
            entity.IsClinicalSpecialization = request.IsClinicalSpecialization;
            entity.RequiresCredentialing = request.RequiresCredentialing;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "Specialization.UpdateSpecialization", "Mengubah data specialization.", new { entity.Id, entity.SpecializationCode, entity.SpecializationName, entity.IsActive });
            return Ok(ApiResponse<object>.Ok(null, "Specialization berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Specialization Status", Description = "Mengubah status specialization", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Specialization", "Update")]
        public async Task<IActionResult> UpdateSpecializationStatus(Guid id, [FromBody] UpdateSpecializationStatusRequest request)
        {
            var entity = await _dbContext.MstSpecializations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Specialization tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status specialization berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Specialization", Description = "Menghapus specialization", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Specialization", "Delete")]
        public async Task<IActionResult> DeleteSpecialization(Guid id)
        {
            var entity = await _dbContext.MstSpecializations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Specialization tidak ditemukan."));

            var isUsed =
                await _dbContext.MstSpecializations.AsNoTracking().AnyAsync(x => x.ParentSpecializationId == id && !x.IsDelete) ||
                await _dbContext.MstClinicalPrivilegeCatalogs.AsNoTracking().AnyAsync(x => x.SpecializationId == id && !x.IsDelete) ||
                await _dbContext.MstCredentialingRequirements.AsNoTracking().AnyAsync(x => x.SpecializationId == id && !x.IsDelete) ||
                await _dbContext.MstDoctors.AsNoTracking().AnyAsync(x => x.SpecializationId == id && !x.IsDelete);

            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Specialization tidak dapat dihapus karena sudah digunakan sebagai parent, credentialing master, atau data doctor."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "Specialization.DeleteSpecialization", "Menghapus data specialization.", new { entity.Id, entity.SpecializationCode, entity.SpecializationName, entity.DeleteDateTime });
            return Ok(ApiResponse<object>.Ok(null, "Specialization berhasil dihapus."));
        }

        private IQueryable<MstSpecialization> BuildBaseQuery() =>
            _dbContext.MstSpecializations.AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstSpecialization> ApplyDateFilter(IQueryable<MstSpecialization> query, DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstSpecialization> ApplyStandardFilter(
            IQueryable<MstSpecialization> query,
            Guid? professionId,
            Guid? parentSpecializationId,
            string? specializationType,
            bool? isClinicalSpecialization,
            bool? requiresCredentialing,
            bool? isActive,
            string? search)
        {
            if (professionId.HasValue && professionId.Value != Guid.Empty) query = query.Where(x => x.ProfessionId == professionId.Value);
            if (parentSpecializationId.HasValue && parentSpecializationId.Value != Guid.Empty) query = query.Where(x => x.ParentSpecializationId == parentSpecializationId.Value);
            if (!string.IsNullOrWhiteSpace(specializationType)) query = query.Where(x => x.SpecializationType == specializationType.Trim());
            if (isClinicalSpecialization.HasValue) query = query.Where(x => x.IsClinicalSpecialization == isClinicalSpecialization.Value);
            if (requiresCredentialing.HasValue) query = query.Where(x => x.RequiresCredentialing == requiresCredentialing.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.SpecializationCode.ToLower().Contains(keyword) ||
                    x.SpecializationName.ToLower().Contains(keyword) ||
                    x.SpecializationType.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword) ||
                    x.Profession != null && x.Profession.ProfessionName.ToLower().Contains(keyword) ||
                    x.ParentSpecialization != null && x.ParentSpecialization.SpecializationName.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstSpecialization> ApplySorting(IQueryable<MstSpecialization> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "specializationName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "specializationcode" => desc ? query.OrderByDescending(x => x.SpecializationCode) : query.OrderBy(x => x.SpecializationCode),
                "professionname" => desc ? query.OrderByDescending(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty) : query.OrderBy(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty),
                "specializationtype" => desc ? query.OrderByDescending(x => x.SpecializationType) : query.OrderBy(x => x.SpecializationType),
                "childspecializationcount" => desc ? query.OrderByDescending(x => x.ChildSpecializations.Count(c => !c.IsDelete)) : query.OrderBy(x => x.ChildSpecializations.Count(c => !c.IsDelete)),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.SpecializationName).ThenByDescending(x => x.SpecializationCode) : query.OrderBy(x => x.SpecializationName).ThenBy(x => x.SpecializationCode)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateSpecializationRequest request)
        {
            if (request.ProfessionId == Guid.Empty) return (false, "Profession wajib dipilih.");
            if (string.IsNullOrWhiteSpace(request.SpecializationName)) return (false, "Nama specialization wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.SpecializationType)) return (false, "Tipe specialization wajib diisi.");

            var type = NormalizeSpecializationType(request.SpecializationType);
            if (!SpecializationTypes.Contains(type, StringComparer.OrdinalIgnoreCase)) return (false, "Tipe specialization tidak valid.");

            var professionExists = await _dbContext.MstProfessions.AsNoTracking().AnyAsync(x => x.Id == request.ProfessionId && x.IsActive && !x.IsDelete);
            if (!professionExists) return (false, "Profession tidak ditemukan atau tidak aktif.");

            var parentId = NormalizeGuid(request.ParentSpecializationId);
            if (parentId.HasValue)
            {
                if (excludeId.HasValue && parentId.Value == excludeId.Value) return (false, "Specialization tidak dapat menjadi parent dirinya sendiri.");

                var parent = await _dbContext.MstSpecializations.AsNoTracking()
                    .Where(x => x.Id == parentId.Value && x.IsActive && !x.IsDelete)
                    .Select(x => new { x.Id, x.ProfessionId, x.ParentSpecializationId })
                    .FirstOrDefaultAsync();

                if (parent == null) return (false, "Parent specialization tidak ditemukan atau tidak aktif.");
                if (parent.ProfessionId != request.ProfessionId) return (false, "Parent specialization harus berasal dari profession yang sama.");

                if (excludeId.HasValue && await WouldCreateCycleAsync(excludeId.Value, parentId.Value))
                    return (false, "Parent specialization membentuk hierarki siklus.");
            }

            var normalizedName = request.SpecializationName.Trim().ToLower();
            var duplicateQuery = _dbContext.MstSpecializations.AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.ProfessionId == request.ProfessionId &&
                x.SpecializationName.ToLower() == normalizedName &&
                x.ParentSpecializationId == parentId);

            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Specialization dengan nama, profession, dan parent tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<bool> WouldCreateCycleAsync(Guid specializationId, Guid parentId)
        {
            var currentId = (Guid?)parentId;
            var visited = new HashSet<Guid>();

            while (currentId.HasValue)
            {
                if (currentId.Value == specializationId) return true;
                if (!visited.Add(currentId.Value)) return true;

                currentId = await _dbContext.MstSpecializations.AsNoTracking()
                    .Where(x => x.Id == currentId.Value && !x.IsDelete)
                    .Select(x => x.ParentSpecializationId)
                    .FirstOrDefaultAsync();
            }

            return false;
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.MstSpecializations.AsNoTracking().Where(x => !x.IsDelete && x.SpecializationCode.StartsWith(CodePrefix)).Select(x => x.SpecializationCode).ToListAsync();
            return GenerateNextCode(codes, CodePrefix, CodeNumberLength);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string NormalizeSpecializationType(string value) => SpecializationTypes.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)) ?? value.Trim();
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

        private static List<SpecializationCustomPeriodOptionResponse> BuildPeriodOptions() =>
            new()
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
    }
}
