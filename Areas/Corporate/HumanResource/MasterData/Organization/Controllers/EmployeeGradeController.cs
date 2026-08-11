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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

using EmployeeGradePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs.EmployeeGradeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/employee-grades")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Employee Grade",
        AreaName = "Corporate",
        ControllerName = "EmployeeGrade",
        Description = "Corporate human resource master data employee grade",
        SortOrder = 25)]
    [Tags("Corporate / Human Resource / Master Data / Employee Grade")]
    public class EmployeeGradeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "GRD-MMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmployeeGradeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Employee Grade", Description = "Melihat metadata filter employee grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeGrade", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new EmployeeGradeFilterMetadataResponse
            {
                DefaultFilter = new EmployeeGradeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<EmployeeGradeSortOptionResponse>
                {
                    new() { Value = "gradeCode", Label = "Kode grade" },
                    new() { Value = "gradeName", Label = "Nama grade" },
                    new() { Value = "gradeOrder", Label = "Urutan grade" },
                    new() { Value = "jobLevelName", Label = "Job level" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "EmployeeGrade.GetFilterMetadata",
                "Mengambil metadata filter employee grade.",
                result
            );

            return Ok(ApiResponse<EmployeeGradeFilterMetadataResponse>.Ok(result, "Metadata filter employee grade berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Employee Grade", Description = "Melihat ringkasan employee grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeGrade", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new EmployeeGradeSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                WithoutJobLevelData = await query.CountAsync(x => !x.JobLevelId.HasValue)
            };

            return Ok(ApiResponse<EmployeeGradeSummaryResponse>.Ok(result, "Ringkasan employee grade berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Employee Grade", Description = "Melihat data employee grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeGrade", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? jobLevelId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "gradeOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), jobLevelId, isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EmployeeGradeResponse
                {
                    Id = x.Id,
                    JobLevelId = x.JobLevelId,
                    JobLevelCode = x.JobLevel != null ? x.JobLevel.JobLevelCode : null,
                    JobLevelName = x.JobLevel != null ? x.JobLevel.JobLevelName : null,
                    GradeCode = x.GradeCode,
                    GradeName = x.GradeName,
                    GradeOrder = x.GradeOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            return Ok(ApiResponse<EmployeeGradePagedResult>.Ok(new EmployeeGradePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data employee grade berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Employee Grade", Description = "Melihat pilihan employee grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeGrade", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? jobLevelId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), jobLevelId, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.JobLevel != null ? x.JobLevel.LevelOrder : int.MaxValue)
                .ThenBy(x => x.GradeOrder)
                .ThenBy(x => x.GradeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EmployeeGradeOptionResponse
                {
                    Id = x.Id,
                    JobLevelId = x.JobLevelId,
                    JobLevelName = x.JobLevel != null ? x.JobLevel.JobLevelName : null,
                    GradeCode = x.GradeCode,
                    GradeName = x.GradeName,
                    GradeOrder = x.GradeOrder
                })
                .ToListAsync();

            return Ok(ApiResponse<EmployeeGradeOptionPagedResponse>.Ok(new EmployeeGradeOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan employee grade berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Employee Grade", Description = "Melihat detail employee grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeGrade", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employee grade tidak ditemukan."));

            return Ok(ApiResponse<EmployeeGradeDetailResponse>.Ok(new EmployeeGradeDetailResponse
            {
                Id = entity.Id,
                JobLevelId = entity.JobLevelId,
                JobLevelCode = entity.JobLevel?.JobLevelCode,
                JobLevelName = entity.JobLevel?.JobLevelName,
                GradeCode = entity.GradeCode,
                GradeName = entity.GradeName,
                GradeOrder = entity.GradeOrder,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy
            }, "Detail employee grade berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Employee Grade", Description = "Membuat employee grade", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmployeeGrade", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeGradeRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstEmployeeGrade
            {
                Id = Guid.NewGuid(),
                JobLevelId = NormalizeGuid(request.JobLevelId),
                GradeCode = await GenerateCodeAsync(),
                GradeName = request.GradeName.Trim(),
                GradeOrder = request.GradeOrder,
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = CurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmployeeGrade>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "EmployeeGrade.Create",
                "Membuat data employee grade.",
                new { entity.Id, entity.JobLevelId, entity.GradeCode, entity.GradeName, entity.GradeOrder, entity.IsActive, entity.CreateDateTime, entity.CreateBy }
            );

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.GradeCode, entity.GradeName }, "Employee grade berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Employee Grade", Description = "Mengubah employee grade", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmployeeGrade", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeGradeRequest request)
        {
            var entity = await _dbContext.Set<MstEmployeeGrade>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employee grade tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.JobLevelId = NormalizeGuid(request.JobLevelId);
            entity.GradeName = request.GradeName.Trim();
            entity.GradeOrder = request.GradeOrder;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "EmployeeGrade.Update",
                "Mengubah data employee grade.",
                new { entity.Id, entity.JobLevelId, entity.GradeCode, entity.GradeName, entity.GradeOrder, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Employee grade berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Employee Grade Status", Description = "Mengubah status employee grade", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmployeeGrade", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEmployeeGradeStatusRequest request)
        {
            var entity = await _dbContext.Set<MstEmployeeGrade>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employee grade tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status employee grade berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Employee Grade", Description = "Menghapus employee grade", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmployeeGrade", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstEmployeeGrade>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employee grade tidak ditemukan."));

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
                "EmployeeGrade.Delete",
                "Menghapus data employee grade.",
                new { entity.Id, entity.GradeCode, entity.GradeName, entity.DeleteDateTime, entity.DeleteBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Employee grade berhasil dihapus."));
        }

        private IQueryable<MstEmployeeGrade> BaseQuery() =>
            _dbContext.Set<MstEmployeeGrade>().AsNoTracking().Include(x => x.JobLevel).Where(x => !x.IsDelete);

        private static IQueryable<MstEmployeeGrade> ApplyFilter(
            IQueryable<MstEmployeeGrade> query,
            Guid? jobLevelId,
            bool? isActive,
            string? search)
        {
            if (jobLevelId.HasValue && jobLevelId.Value != Guid.Empty) query = query.Where(x => x.JobLevelId == jobLevelId.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.GradeCode.ToLower().Contains(keyword) ||
                    x.GradeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.JobLevel != null && x.JobLevel.JobLevelName.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstEmployeeGrade> ApplySorting(IQueryable<MstEmployeeGrade> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "gradeOrder").Trim().ToLowerInvariant() switch
            {
                "gradecode" => desc ? query.OrderByDescending(x => x.GradeCode) : query.OrderBy(x => x.GradeCode),
                "gradename" => desc ? query.OrderByDescending(x => x.GradeName) : query.OrderBy(x => x.GradeName),
                "joblevelname" => desc
                    ? query.OrderByDescending(x => x.JobLevel != null ? x.JobLevel.JobLevelName : string.Empty).ThenByDescending(x => x.GradeOrder)
                    : query.OrderBy(x => x.JobLevel != null ? x.JobLevel.JobLevelName : string.Empty).ThenBy(x => x.GradeOrder),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.GradeOrder) : query.OrderBy(x => x.IsActive).ThenBy(x => x.GradeOrder),
                _ => desc ? query.OrderByDescending(x => x.GradeOrder).ThenByDescending(x => x.GradeName) : query.OrderBy(x => x.GradeOrder).ThenBy(x => x.GradeName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateEmployeeGradeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GradeName)) return (false, "Nama employee grade wajib diisi.");
            if (request.GradeOrder < 0) return (false, "Urutan employee grade tidak boleh kurang dari 0.");

            var jobLevelId = NormalizeGuid(request.JobLevelId);
            if (jobLevelId.HasValue)
            {
                var exists = await _dbContext.Set<MstJobLevel>().AsNoTracking()
                    .AnyAsync(x => x.Id == jobLevelId.Value && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Job level tidak ditemukan atau tidak aktif.");
            }

            var normalizedName = request.GradeName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstEmployeeGrade>().AsNoTracking()
                .Where(x => !x.IsDelete && x.JobLevelId == jobLevelId && x.GradeName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama employee grade sudah digunakan pada job level tersebut.");
            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstEmployeeGrade>().AsNoTracking()
                .Where(x => !x.IsDelete && x.GradeCode.StartsWith(CodePrefix))
                .Select(x => x.GradeCode)
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

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static List<EmployeeGradeCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<EmployeeGradeCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
