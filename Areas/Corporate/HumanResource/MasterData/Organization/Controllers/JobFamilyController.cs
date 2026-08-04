using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using JobFamilyPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs.JobFamilyResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/job-families")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Job Family",
        AreaName = "Corporate",
        ControllerName = "JobFamily",
        Description = "Corporate human resource master data job family",
        SortOrder = 23)]
    [Tags("Corporate / Human Resource / Master Data / Job Family")]
    public class JobFamilyController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "JF-MMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public JobFamilyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Job Family", Description = "Melihat metadata filter job family", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobFamily", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new JobFamilyFilterMetadataResponse
            {
                DefaultFilter = new JobFamilyDefaultFilterResponse(),
                SortOptions = new List<JobFamilySortOptionResponse>
                {
                    new() { Value = "jobFamilyCode", Label = "Kode job family" },
                    new() { Value = "jobFamilyName", Label = "Nama job family" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "JobFamily.GetFilterMetadata",
                "Mengambil metadata filter job family.",
                result
            );

            return Ok(ApiResponse<JobFamilyFilterMetadataResponse>.Ok(result, "Metadata filter job family berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Job Family", Description = "Melihat ringkasan job family", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobFamily", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new JobFamilySummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive)
            };

            return Ok(ApiResponse<JobFamilySummaryResponse>.Ok(result, "Ringkasan job family berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Job Family", Description = "Melihat data job family", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobFamily", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "jobFamilyName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), isActive, search);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new JobFamilyResponse
                {
                    Id = x.Id,
                    JobFamilyCode = x.JobFamilyCode,
                    JobFamilyName = x.JobFamilyName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            return Ok(ApiResponse<JobFamilyPagedResult>.Ok(new JobFamilyPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data job family berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Job Family", Description = "Melihat pilihan job family", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobFamily", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), onlyActive ? true : null, search);
            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.JobFamilyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new JobFamilyOptionResponse
                {
                    Id = x.Id,
                    JobFamilyCode = x.JobFamilyCode,
                    JobFamilyName = x.JobFamilyName
                })
                .ToListAsync();

            return Ok(ApiResponse<JobFamilyOptionPagedResponse>.Ok(new JobFamilyOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan job family berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Job Family", Description = "Melihat detail job family", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobFamily", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Job family tidak ditemukan."));

            return Ok(ApiResponse<JobFamilyDetailResponse>.Ok(new JobFamilyDetailResponse
            {
                Id = entity.Id,
                JobFamilyCode = entity.JobFamilyCode,
                JobFamilyName = entity.JobFamilyName,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy
            }, "Detail job family berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Job Family", Description = "Membuat job family", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("JobFamily", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateJobFamilyRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstJobFamily
            {
                Id = Guid.NewGuid(),
                JobFamilyCode = await GenerateCodeAsync(),
                JobFamilyName = request.JobFamilyName.Trim(),
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = CurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstJobFamily>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "JobFamily.Create",
                "Membuat data job family.",
                new { entity.Id, entity.JobFamilyCode, entity.JobFamilyName, entity.IsActive, entity.CreateDateTime, entity.CreateBy }
            );

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.JobFamilyCode, entity.JobFamilyName }, "Job family berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Job Family", Description = "Mengubah job family", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("JobFamily", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobFamilyRequest request)
        {
            var entity = await _dbContext.Set<MstJobFamily>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Job family tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.JobFamilyName = request.JobFamilyName.Trim();
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "JobFamily.Update",
                "Mengubah data job family.",
                new { entity.Id, entity.JobFamilyCode, entity.JobFamilyName, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Job family berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Job Family Status", Description = "Mengubah status job family", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("JobFamily", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateJobFamilyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstJobFamily>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Job family tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status job family berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Job Family", Description = "Menghapus job family", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("JobFamily", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstJobFamily>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Job family tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "JobFamily.Delete",
                "Menghapus data job family.",
                new { entity.Id, entity.JobFamilyCode, entity.JobFamilyName, entity.DeleteDateTime, entity.DeleteBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Job family berhasil dihapus."));
        }

        private IQueryable<MstJobFamily> BaseQuery() =>
            _dbContext.Set<MstJobFamily>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstJobFamily> ApplyFilter(IQueryable<MstJobFamily> query, bool? isActive, string? search)
        {
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.JobFamilyCode.ToLower().Contains(keyword) ||
                    x.JobFamilyName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstJobFamily> ApplySorting(IQueryable<MstJobFamily> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "jobFamilyName").Trim().ToLowerInvariant() switch
            {
                "jobfamilycode" => desc ? query.OrderByDescending(x => x.JobFamilyCode) : query.OrderBy(x => x.JobFamilyCode),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.JobFamilyName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.JobFamilyName),
                _ => desc ? query.OrderByDescending(x => x.JobFamilyName) : query.OrderBy(x => x.JobFamilyName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateJobFamilyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.JobFamilyName))
                return (false, "Nama job family wajib diisi.");

            var normalizedName = request.JobFamilyName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstJobFamily>().AsNoTracking()
                .Where(x => !x.IsDelete && x.JobFamilyName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama job family sudah digunakan.");
            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstJobFamily>().AsNoTracking()
                .Where(x => !x.IsDelete && x.JobFamilyCode.StartsWith(CodePrefix))
                .Select(x => x.JobFamilyCode)
                .ToListAsync();
            var used = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

        private Guid CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static void NormalizePaging(ref int pageNumber, ref int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
        }

        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
