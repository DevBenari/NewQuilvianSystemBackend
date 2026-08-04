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

using ResponseProfessionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs.ProfessionResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/professions")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Profession",
        AreaName = "Corporate",
        ControllerName = "Profession",
        Description = "Corporate human resource master data profession",
        SortOrder = 13)]
    [Tags("Corporate / Human Resource / Master Data / Profession")]
    public class ProfessionController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "PRF-RSMMC-";
        private const int CodeNumberLength = 5;

        private static readonly string[] ProfessionGroups =
        {
            "Medical", "Nursing", "AlliedHealth", "Pharmacy",
            "Administration", "Technical", "General"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ProfessionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Profession", Description = "Melihat metadata filter profession", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Profession", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ProfessionFilterMetadataResponse
            {
                DefaultFilter = new ProfessionDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                ProfessionGroupOptions = ProfessionGroups
                    .Select(x => new ProfessionStringOptionResponse { Value = x, Label = x })
                    .ToList(),
                SortOptions = new List<ProfessionSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "professionCode", Label = "Kode profession" },
                    new() { Value = "professionName", Label = "Nama profession" },
                    new() { Value = "professionGroup", Label = "Kelompok profession" },
                    new() { Value = "specializationCount", Label = "Jumlah specialization" },
                    new() { Value = "isClinicalProfession", Label = "Profession klinis" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Profession.GetFilterMetadata",
                "Mengambil metadata filter profession.",
                result);

            return Ok(ApiResponse<ProfessionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter profession berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Profession", Description = "Melihat ringkasan profession", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Profession", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new ProfessionSummaryResponse
            {
                TotalProfession = await query.CountAsync(),
                ActiveProfession = await query.CountAsync(x => x.IsActive),
                InactiveProfession = await query.CountAsync(x => !x.IsActive),
                ClinicalProfession = await query.CountAsync(x => x.IsClinicalProfession),
                CredentialingRequiredProfession = await query.CountAsync(x => x.RequiresCredentialing),
                LicenseRequiredProfession = await query.CountAsync(x => x.RequiresLicense),
                ProfessionWithSpecialization = await query.CountAsync(x => x.Specializations.Any(s => !s.IsDelete))
            };

            return Ok(ApiResponse<ProfessionSummaryResponse>.Ok(
                result,
                "Ringkasan profession berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseProfessionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Profession", Description = "Melihat data profession", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Profession", "Read")]
        public async Task<IActionResult> GetProfessions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? professionGroup,
            [FromQuery] bool? isClinicalProfession,
            [FromQuery] bool? requiresCredentialing,
            [FromQuery] bool? requiresLicense,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "professionName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query,
                professionGroup,
                isClinicalProfession,
                requiresCredentialing,
                requiresLicense,
                isActive,
                search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProfessionResponse
                {
                    Id = x.Id,
                    ProfessionCode = x.ProfessionCode,
                    ProfessionName = x.ProfessionName,
                    ProfessionGroup = x.ProfessionGroup,
                    IsClinicalProfession = x.IsClinicalProfession,
                    RequiresCredentialing = x.RequiresCredentialing,
                    RequiresLicense = x.RequiresLicense,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    SpecializationCount = x.Specializations.Count(s => !s.IsDelete),
                    CertificationTypeCount = x.CertificationTypes.Count(c => !c.IsDelete),
                    LicenseTypeCount = x.LicenseTypes.Count(l => !l.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            var result = new ResponseProfessionPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseProfessionPagedResult>.Ok(
                result,
                "Data profession berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Profession", Description = "Melihat pilihan profession", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Profession", "Read")]
        public async Task<IActionResult> GetProfessionOptions(
            [FromQuery] string? professionGroup,
            [FromQuery] bool? isClinicalProfession,
            [FromQuery] bool? requiresCredentialing,
            [FromQuery] bool? requiresLicense,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(
                BuildBaseQuery(),
                professionGroup,
                isClinicalProfession,
                requiresCredentialing,
                requiresLicense,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.ProfessionName)
                .ThenBy(x => x.ProfessionCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProfessionOptionResponse
                {
                    Id = x.Id,
                    ProfessionCode = x.ProfessionCode,
                    ProfessionName = x.ProfessionName,
                    ProfessionGroup = x.ProfessionGroup,
                    IsClinicalProfession = x.IsClinicalProfession,
                    RequiresCredentialing = x.RequiresCredentialing,
                    RequiresLicense = x.RequiresLicense
                })
                .ToListAsync();

            return Ok(ApiResponse<ProfessionOptionPagedResponse>.Ok(
                new ProfessionOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan profession berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Profession", Description = "Melihat detail profession", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Profession", "Read")]
        public async Task<IActionResult> GetProfessionById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new ProfessionDetailResponse
                {
                    Id = x.Id,
                    ProfessionCode = x.ProfessionCode,
                    ProfessionName = x.ProfessionName,
                    ProfessionGroup = x.ProfessionGroup,
                    IsClinicalProfession = x.IsClinicalProfession,
                    RequiresCredentialing = x.RequiresCredentialing,
                    RequiresLicense = x.RequiresLicense,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    SpecializationCount = x.Specializations.Count(s => !s.IsDelete),
                    CertificationTypeCount = x.CertificationTypes.Count(c => !c.IsDelete),
                    LicenseTypeCount = x.LicenseTypes.Count(l => !l.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profession tidak ditemukan."));
            }

            return Ok(ApiResponse<ProfessionDetailResponse>.Ok(
                data,
                "Detail profession berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProfessionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Profession", Description = "Membuat data profession", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Profession", "Create")]
        public async Task<IActionResult> CreateProfession([FromBody] CreateProfessionRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profession tidak valid."));
            }

            var entity = new MstProfession
            {
                Id = Guid.NewGuid(),
                ProfessionCode = await GenerateCodeAsync(),
                ProfessionName = request.ProfessionName.Trim(),
                ProfessionGroup = NormalizeProfessionGroup(request.ProfessionGroup),
                IsClinicalProfession = request.IsClinicalProfession,
                RequiresCredentialing = request.RequiresCredentialing,
                RequiresLicense = request.RequiresLicense,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstProfessions.Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new ProfessionCreateResponse
            {
                Id = entity.Id,
                ProfessionCode = entity.ProfessionCode,
                ProfessionName = entity.ProfessionName,
                ProfessionGroup = entity.ProfessionGroup,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Profession.CreateProfession",
                "Membuat data profession.",
                result);

            return Ok(ApiResponse<ProfessionCreateResponse>.Ok(
                result,
                "Profession berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Profession", Description = "Mengubah data profession", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Profession", "Update")]
        public async Task<IActionResult> UpdateProfession(Guid id, [FromBody] UpdateProfessionRequest request)
        {
            var entity = await _dbContext.MstProfessions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profession tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profession tidak valid."));
            }

            entity.ProfessionName = request.ProfessionName.Trim();
            entity.ProfessionGroup = NormalizeProfessionGroup(request.ProfessionGroup);
            entity.IsClinicalProfession = request.IsClinicalProfession;
            entity.RequiresCredentialing = request.RequiresCredentialing;
            entity.RequiresLicense = request.RequiresLicense;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Profession.UpdateProfession",
                "Mengubah data profession.",
                new { entity.Id, entity.ProfessionCode, entity.ProfessionName, entity.IsActive });

            return Ok(ApiResponse<object>.Ok(null, "Profession berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Profession Status", Description = "Mengubah status profession", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Profession", "Update")]
        public async Task<IActionResult> UpdateProfessionStatus(Guid id, [FromBody] UpdateProfessionStatusRequest request)
        {
            var entity = await _dbContext.MstProfessions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profession tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Status profession berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Profession", Description = "Menghapus profession", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Profession", "Delete")]
        public async Task<IActionResult> DeleteProfession(Guid id)
        {
            var entity = await _dbContext.MstProfessions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profession tidak ditemukan."));
            }

            var isUsed =
                await _dbContext.MstSpecializations.AsNoTracking().AnyAsync(x => x.ProfessionId == id && !x.IsDelete) ||
                await _dbContext.MstCertificationTypes.AsNoTracking().AnyAsync(x => x.ProfessionId == id && !x.IsDelete) ||
                await _dbContext.MstLicenseTypes.AsNoTracking().AnyAsync(x => x.ProfessionId == id && !x.IsDelete) ||
                await _dbContext.MstClinicalPrivilegeCatalogs.AsNoTracking().AnyAsync(x => x.ProfessionId == id && !x.IsDelete) ||
                await _dbContext.MstCredentialingRequirements.AsNoTracking().AnyAsync(x => x.ProfessionId == id && !x.IsDelete) ||
                await _dbContext.MstEmployees.AsNoTracking().AnyAsync(x => x.ProfessionId == id && !x.IsDelete) ||
                await _dbContext.MstDoctors.AsNoTracking().AnyAsync(x => x.ProfessionId == id && !x.IsDelete) ||
                await _dbContext.MstExternalUsers.AsNoTracking().AnyAsync(x => x.ProfessionId == id && !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Profession tidak dapat dihapus karena sudah digunakan oleh specialization, credentialing master, employee, doctor, atau external user."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Profession.DeleteProfession",
                "Menghapus data profession.",
                new { entity.Id, entity.ProfessionCode, entity.ProfessionName, entity.DeleteDateTime });

            return Ok(ApiResponse<object>.Ok(null, "Profession berhasil dihapus."));
        }

        private IQueryable<MstProfession> BuildBaseQuery() =>
            _dbContext.MstProfessions.AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstProfession> ApplyDateFilter(
            IQueryable<MstProfession> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstProfession> ApplyStandardFilter(
            IQueryable<MstProfession> query,
            string? professionGroup,
            bool? isClinicalProfession,
            bool? requiresCredentialing,
            bool? requiresLicense,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(professionGroup))
                query = query.Where(x => x.ProfessionGroup == professionGroup.Trim());
            if (isClinicalProfession.HasValue)
                query = query.Where(x => x.IsClinicalProfession == isClinicalProfession.Value);
            if (requiresCredentialing.HasValue)
                query = query.Where(x => x.RequiresCredentialing == requiresCredentialing.Value);
            if (requiresLicense.HasValue)
                query = query.Where(x => x.RequiresLicense == requiresLicense.Value);
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ProfessionCode.ToLower().Contains(keyword) ||
                    x.ProfessionName.ToLower().Contains(keyword) ||
                    x.ProfessionGroup.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstProfession> ApplySorting(
            IQueryable<MstProfession> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "professionName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "professioncode" => desc ? query.OrderByDescending(x => x.ProfessionCode) : query.OrderBy(x => x.ProfessionCode),
                "professiongroup" => desc ? query.OrderByDescending(x => x.ProfessionGroup) : query.OrderBy(x => x.ProfessionGroup),
                "specializationcount" => desc ? query.OrderByDescending(x => x.Specializations.Count(s => !s.IsDelete)) : query.OrderBy(x => x.Specializations.Count(s => !s.IsDelete)),
                "isclinicalprofession" => desc ? query.OrderByDescending(x => x.IsClinicalProfession) : query.OrderBy(x => x.IsClinicalProfession),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.ProfessionName).ThenByDescending(x => x.ProfessionCode) : query.OrderBy(x => x.ProfessionName).ThenBy(x => x.ProfessionCode)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateProfessionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProfessionName))
                return (false, "Nama profession wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.ProfessionGroup))
                return (false, "Kelompok profession wajib diisi.");

            var group = NormalizeProfessionGroup(request.ProfessionGroup);
            if (!ProfessionGroups.Contains(group, StringComparer.OrdinalIgnoreCase))
                return (false, "Kelompok profession tidak valid.");

            if (request.RequiresLicense && !request.RequiresCredentialing)
                return (false, "Profession yang membutuhkan lisensi harus membutuhkan credentialing.");

            var normalizedName = request.ProfessionName.Trim().ToLower();
            var duplicateQuery = _dbContext.MstProfessions.AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.ProfessionName.ToLower() == normalizedName &&
                x.ProfessionGroup == group);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Profession dengan nama dan kelompok tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.MstProfessions.AsNoTracking()
                .Where(x => !x.IsDelete && x.ProfessionCode.StartsWith(CodePrefix))
                .Select(x => x.ProfessionCode)
                .ToListAsync();

            return GenerateNextCode(codes, CodePrefix, CodeNumberLength);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static string NormalizeProfessionGroup(string value) =>
            ProfessionGroups.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)) ?? value.Trim();

        private static string? NormalizeNullableString(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize) =>
            (pageNumber < 1 ? 1 : pageNumber, pageSize < 1 ? 25 : Math.Min(pageSize, 100));

        private static string GenerateNextCode(IEnumerable<string> codes, string prefix, int length)
        {
            var used = codes.Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return prefix + next.ToString().PadLeft(length, '0');
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                return (
                    startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null,
                    endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null);
            }

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

        private static List<ProfessionCustomPeriodOptionResponse> BuildPeriodOptions() =>
            new()
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
    }
}
