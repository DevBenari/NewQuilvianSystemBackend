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

using JobLevelPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs.JobLevelResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/job-levels")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Job Level",
        AreaName = "Corporate",
        ControllerName = "JobLevel",
        Description = "Corporate human resource master data job level",
        SortOrder = 24)]
    [Tags("Corporate / Human Resource / Master Data / Job Level")]
    public class JobLevelController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "JL-MMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public JobLevelController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Job Level", Description = "Melihat metadata filter job level", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobLevel", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new JobLevelFilterMetadataResponse
            {
                DefaultFilter = new JobLevelDefaultFilterResponse(),
                SortOptions = new List<JobLevelSortOptionResponse>
                {
                    new() { Value = "jobLevelCode", Label = "Kode job level" },
                    new() { Value = "jobLevelName", Label = "Nama job level" },
                    new() { Value = "levelOrder", Label = "Urutan level" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "JobLevel.GetFilterMetadata",
                "Mengambil metadata filter job level.",
                result
            );

            return Ok(ApiResponse<JobLevelFilterMetadataResponse>.Ok(result, "Metadata filter job level berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Job Level", Description = "Melihat ringkasan job level", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobLevel", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new JobLevelSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                WithEmployeeGradeData = await query.CountAsync(x => x.EmployeeGrades.Any(y => !y.IsDelete))
            };

            return Ok(ApiResponse<JobLevelSummaryResponse>.Ok(result, "Ringkasan job level berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Job Level", Description = "Melihat data job level", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobLevel", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "levelOrder",
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
                .Select(x => new JobLevelResponse
                {
                    Id = x.Id,
                    JobLevelCode = x.JobLevelCode,
                    JobLevelName = x.JobLevelName,
                    LevelOrder = x.LevelOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    EmployeeGradeCount = x.EmployeeGrades.Count(y => !y.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            return Ok(ApiResponse<JobLevelPagedResult>.Ok(new JobLevelPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data job level berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Job Level", Description = "Melihat pilihan job level", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobLevel", "Read")]
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
                .OrderBy(x => x.LevelOrder)
                .ThenBy(x => x.JobLevelName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new JobLevelOptionResponse
                {
                    Id = x.Id,
                    JobLevelCode = x.JobLevelCode,
                    JobLevelName = x.JobLevelName,
                    LevelOrder = x.LevelOrder
                })
                .ToListAsync();

            return Ok(ApiResponse<JobLevelOptionPagedResponse>.Ok(new JobLevelOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan job level berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Job Level", Description = "Melihat detail job level", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobLevel", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Job level tidak ditemukan."));

            return Ok(ApiResponse<JobLevelDetailResponse>.Ok(new JobLevelDetailResponse
            {
                Id = entity.Id,
                JobLevelCode = entity.JobLevelCode,
                JobLevelName = entity.JobLevelName,
                LevelOrder = entity.LevelOrder,
                Description = entity.Description,
                IsActive = entity.IsActive,
                EmployeeGradeCount = entity.EmployeeGrades.Count(x => !x.IsDelete),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy
            }, "Detail job level berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Job Level", Description = "Membuat job level", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("JobLevel", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateJobLevelRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstJobLevel
            {
                Id = Guid.NewGuid(),
                JobLevelCode = await GenerateCodeAsync(),
                JobLevelName = request.JobLevelName.Trim(),
                LevelOrder = request.LevelOrder,
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = CurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstJobLevel>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "JobLevel.Create",
                "Membuat data job level.",
                new { entity.Id, entity.JobLevelCode, entity.JobLevelName, entity.LevelOrder, entity.IsActive, entity.CreateDateTime, entity.CreateBy }
            );

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.JobLevelCode, entity.JobLevelName }, "Job level berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Job Level", Description = "Mengubah job level", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("JobLevel", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobLevelRequest request)
        {
            var entity = await _dbContext.Set<MstJobLevel>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Job level tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.JobLevelName = request.JobLevelName.Trim();
            entity.LevelOrder = request.LevelOrder;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "JobLevel.Update",
                "Mengubah data job level.",
                new { entity.Id, entity.JobLevelCode, entity.JobLevelName, entity.LevelOrder, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Job level berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Job Level Status", Description = "Mengubah status job level", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("JobLevel", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateJobLevelStatusRequest request)
        {
            var entity = await _dbContext.Set<MstJobLevel>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Job level tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status job level berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Job Level", Description = "Menghapus job level", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("JobLevel", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstJobLevel>()
                .Include(x => x.EmployeeGrades)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Job level tidak ditemukan."));

            if (entity.EmployeeGrades.Any(x => !x.IsDelete))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Job level tidak dapat dihapus karena masih digunakan oleh employee grade."));

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
                "JobLevel.Delete",
                "Menghapus data job level.",
                new { entity.Id, entity.JobLevelCode, entity.JobLevelName, entity.DeleteDateTime, entity.DeleteBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Job level berhasil dihapus."));
        }

        private IQueryable<MstJobLevel> BaseQuery() =>
            _dbContext.Set<MstJobLevel>().AsNoTracking().Include(x => x.EmployeeGrades).Where(x => !x.IsDelete);

        private static IQueryable<MstJobLevel> ApplyFilter(IQueryable<MstJobLevel> query, bool? isActive, string? search)
        {
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.JobLevelCode.ToLower().Contains(keyword) ||
                    x.JobLevelName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstJobLevel> ApplySorting(IQueryable<MstJobLevel> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "levelOrder").Trim().ToLowerInvariant() switch
            {
                "joblevelcode" => desc ? query.OrderByDescending(x => x.JobLevelCode) : query.OrderBy(x => x.JobLevelCode),
                "joblevelname" => desc ? query.OrderByDescending(x => x.JobLevelName) : query.OrderBy(x => x.JobLevelName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.LevelOrder) : query.OrderBy(x => x.IsActive).ThenBy(x => x.LevelOrder),
                _ => desc ? query.OrderByDescending(x => x.LevelOrder).ThenByDescending(x => x.JobLevelName) : query.OrderBy(x => x.LevelOrder).ThenBy(x => x.JobLevelName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateJobLevelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.JobLevelName)) return (false, "Nama job level wajib diisi.");
            if (request.LevelOrder < 0) return (false, "Urutan job level tidak boleh kurang dari 0.");

            var normalizedName = request.JobLevelName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstJobLevel>().AsNoTracking()
                .Where(x => !x.IsDelete && x.JobLevelName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama job level sudah digunakan.");
            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstJobLevel>().AsNoTracking()
                .Where(x => !x.IsDelete && x.JobLevelCode.StartsWith(CodePrefix))
                .Select(x => x.JobLevelCode)
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
