using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseDepartmentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.OrganizationDepartmentResponse>;

using ResponsePositionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.OrganizationPositionResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/organization")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Organization",
        AreaName = "Corporate",
        ControllerName = "Organization",
        Description = "Corporate human resource master data organization department dan position",
        SortOrder = 3
    )]
    [Tags("Corporate / Human Resource / Master Data / Organization")]
    public class OrganizationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public OrganizationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        // =========================================================
        // SUMMARY
        // =========================================================

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Summary",
            displayName: "Ringkasan Data",
            Description = "Melihat ringkasan jumlah department dan position",
            SortOrder = 1
        )]
        [AccessPermission("Organization", "Summary")]
        public async Task<IActionResult> GetSummary()
        {
            var departmentQuery = _dbContext.MstDepartments
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var positionQuery = _dbContext.MstPositions
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new OrganizationSummaryResponse
            {
                TotalDepartment = await departmentQuery.CountAsync(),
                ActiveDepartment = await departmentQuery.CountAsync(x => x.IsActive),
                InactiveDepartment = await departmentQuery.CountAsync(x => !x.IsActive),

                TotalPosition = await positionQuery.CountAsync(),
                ActivePosition = await positionQuery.CountAsync(x => x.IsActive),
                InactivePosition = await positionQuery.CountAsync(x => !x.IsActive)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.GetSummary",
                "Mengambil ringkasan data organization.",
                result
            );

            return Ok(ApiResponse<OrganizationSummaryResponse>.Ok(
                result,
                "Ringkasan organization berhasil diambil."
            ));
        }

        // =========================================================
        // DEPARTMENT
        // =========================================================

        [HttpGet("departments")]
        [ProducesResponseType(typeof(ApiResponse<ResponseDepartmentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "DepartmentIndex",
            displayName: "Lihat Data Department",
            Description = "Melihat daftar department",
            SortOrder = 2
        )]
        [AccessPermission("Organization", "DepartmentIndex")]
        public async Task<IActionResult> GetDepartments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] bool includePositions = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            var query = _dbContext.MstDepartments
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DepartmentCode.ToLower().Contains(keyword) ||
                    x.DepartmentName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    x.Positions.Any(p =>
                        !p.IsDelete &&
                        (
                            p.PositionCode.ToLower().Contains(keyword) ||
                            p.PositionName.ToLower().Contains(keyword) ||
                            (p.Description != null && p.Description.ToLower().Contains(keyword))
                        )));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .ThenBy(x => x.DepartmentName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrganizationDepartmentResponse
                {
                    Id = x.Id,
                    DepartmentCode = x.DepartmentCode,
                    DepartmentName = x.DepartmentName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    PositionCount = x.Positions.Count(p => !p.IsDelete),
                    ActivePositionCount = x.Positions.Count(p => !p.IsDelete && p.IsActive),
                    Positions = includePositions
                        ? x.Positions
                            .Where(p => !p.IsDelete)
                            .OrderBy(p => p.PositionName)
                            .Select(p => new OrganizationPositionCompactResponse
                            {
                                Id = p.Id,
                                DepartmentId = p.DepartmentId,
                                PositionCode = p.PositionCode,
                                PositionName = p.PositionName,
                                IsActive = p.IsActive
                            })
                            .ToList()
                        : new List<OrganizationPositionCompactResponse>()
                })
                .ToListAsync();

            var result = new ResponseDepartmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.GetDepartments",
                "Mengambil data department.",
                new
                {
                    startDate,
                    endDate,
                    customPeriod,
                    search,
                    isActive,
                    includePositions,
                    pageNumber,
                    pageSize,
                    totalData
                }
            );

            return Ok(ApiResponse<ResponseDepartmentPagedResult>.Ok(
                result,
                "Data department berhasil diambil."
            ));
        }

        [HttpGet("departments/options")]
        [ProducesResponseType(typeof(ApiResponse<List<OrganizationDepartmentOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "DepartmentOptions",
            displayName: "Pilihan Department",
            Description = "Mengambil pilihan department aktif",
            SortOrder = 3
        )]
        [AccessPermission("Organization", "DepartmentOptions")]
        public async Task<IActionResult> GetDepartmentOptions([FromQuery] bool onlyActive = true)
        {
            var query = _dbContext.MstDepartments
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            var data = await query
                .OrderBy(x => x.DepartmentName)
                .Select(x => new OrganizationDepartmentOptionResponse
                {
                    Id = x.Id,
                    DepartmentCode = x.DepartmentCode,
                    DepartmentName = x.DepartmentName
                })
                .ToListAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.GetDepartmentOptions",
                "Mengambil data pilihan department.",
                new
                {
                    onlyActive,
                    TotalData = data.Count
                }
            );

            return Ok(ApiResponse<List<OrganizationDepartmentOptionResponse>>.Ok(
                data,
                "Data pilihan department berhasil diambil."
            ));
        }

        [HttpGet("departments/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDepartmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "DepartmentDetail",
            displayName: "Detail Department",
            Description = "Melihat detail department",
            SortOrder = 4
        )]
        [AccessPermission("Organization", "DepartmentDetail")]
        public async Task<IActionResult> GetDepartmentById(
            Guid id,
            [FromQuery] bool includePositions = true)
        {
            var data = await _dbContext.MstDepartments
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new OrganizationDepartmentResponse
                {
                    Id = x.Id,
                    DepartmentCode = x.DepartmentCode,
                    DepartmentName = x.DepartmentName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    PositionCount = x.Positions.Count(p => !p.IsDelete),
                    ActivePositionCount = x.Positions.Count(p => !p.IsDelete && p.IsActive),
                    Positions = includePositions
                        ? x.Positions
                            .Where(p => !p.IsDelete)
                            .OrderBy(p => p.PositionName)
                            .Select(p => new OrganizationPositionCompactResponse
                            {
                                Id = p.Id,
                                DepartmentId = p.DepartmentId,
                                PositionCode = p.PositionCode,
                                PositionName = p.PositionName,
                                IsActive = p.IsActive
                            })
                            .ToList()
                        : new List<OrganizationPositionCompactResponse>()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.GetDepartmentById",
                    "Department tidak ditemukan.",
                    new { Id = id }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Department tidak ditemukan."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.GetDepartmentById",
                "Mengambil detail department.",
                new { Id = id }
            );

            return Ok(ApiResponse<OrganizationDepartmentResponse>.Ok(
                data,
                "Detail department berhasil diambil."
            ));
        }

        [HttpPost("departments")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "DepartmentCreate",
            displayName: "Tambah Department",
            Description = "Membuat department baru",
            SortOrder = 5
        )]
        [AccessPermission("Organization", "DepartmentCreate")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateOrganizationDepartmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.CreateDepartment",
                    "Gagal membuat department. Nama department kosong.",
                    request
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama department wajib diisi."
                ));
            }

            var name = request.DepartmentName.Trim();

            var nameExists = await _dbContext.MstDepartments
                .AnyAsync(x =>
                    x.DepartmentName.ToLower() == name.ToLower() &&
                    !x.IsDelete);

            if (nameExists)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.CreateDepartment",
                    "Gagal membuat department. Nama department sudah digunakan.",
                    new { DepartmentName = name }
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama department sudah digunakan."
                ));
            }

            var code = await GenerateDepartmentCodeAsync();

            var entity = new MstDepartment
            {
                Id = Guid.NewGuid(),
                DepartmentCode = code,
                DepartmentName = name,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstDepartments.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.CreateDepartment",
                "Department berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.DepartmentCode,
                    entity.DepartmentName,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    entity.Id,
                    entity.DepartmentCode,
                    entity.DepartmentName,
                    entity.Description,
                    entity.IsActive
                },
                "Department berhasil dibuat."
            ));
        }

        [HttpPut("departments/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "DepartmentUpdate",
            displayName: "Ubah Department",
            Description = "Mengubah department",
            SortOrder = 6
        )]
        [AccessPermission("Organization", "DepartmentUpdate")]
        public async Task<IActionResult> UpdateDepartment(
            Guid id,
            [FromBody] UpdateOrganizationDepartmentRequest request)
        {
            var entity = await _dbContext.MstDepartments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.UpdateDepartment",
                    "Gagal update department. Department tidak ditemukan.",
                    new { Id = id }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Department tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama department wajib diisi."
                ));
            }

            var name = request.DepartmentName.Trim();

            var nameExists = await _dbContext.MstDepartments
                .AnyAsync(x =>
                    x.Id != id &&
                    x.DepartmentName.ToLower() == name.ToLower() &&
                    !x.IsDelete);

            if (nameExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama department sudah digunakan."
                ));
            }

            var oldStatus = entity.IsActive;
            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId();

            entity.DepartmentName = name;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = userId;

            if (oldStatus && !request.IsActive && request.CascadeToPositions)
            {
                var positions = await _dbContext.MstPositions
                    .Where(x => x.DepartmentId == id && !x.IsDelete)
                    .ToListAsync();

                foreach (var position in positions)
                {
                    position.IsActive = false;
                    position.UpdateDateTime = now;
                    position.UpdateBy = userId;
                }
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.UpdateDepartment",
                "Department berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.DepartmentCode,
                    entity.DepartmentName,
                    entity.IsActive,
                    request.CascadeToPositions
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Department berhasil diperbarui."
            ));
        }

        [HttpPatch("departments/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "DepartmentStatus",
            displayName: "Ubah Status Department",
            Description = "Mengubah status department",
            SortOrder = 7
        )]
        [AccessPermission("Organization", "DepartmentStatus")]
        public async Task<IActionResult> UpdateDepartmentStatus(
            Guid id,
            [FromBody] UpdateOrganizationStatusRequest request)
        {
            var entity = await _dbContext.MstDepartments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.UpdateDepartmentStatus",
                    "Gagal update status department. Department tidak ditemukan.",
                    new { Id = id }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Department tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = userId;

            if (!request.IsActive && request.CascadeToPositions)
            {
                var positions = await _dbContext.MstPositions
                    .Where(x => x.DepartmentId == id && !x.IsDelete)
                    .ToListAsync();

                foreach (var position in positions)
                {
                    position.IsActive = false;
                    position.UpdateDateTime = now;
                    position.UpdateBy = userId;
                }
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.UpdateDepartmentStatus",
                "Status department berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.DepartmentCode,
                    entity.DepartmentName,
                    entity.IsActive,
                    request.CascadeToPositions
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status department berhasil diperbarui."
            ));
        }

        [HttpDelete("departments/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "DepartmentDelete",
            displayName: "Hapus Department",
            Description = "Menghapus department",
            SortOrder = 8
        )]
        [AccessPermission("Organization", "DepartmentDelete")]
        public async Task<IActionResult> DeleteDepartment(
            Guid id,
            [FromQuery] bool cascadePositions = true)
        {
            var entity = await _dbContext.MstDepartments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.DeleteDepartment",
                    "Gagal hapus department. Department tidak ditemukan.",
                    new { Id = id }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Department tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = userId;

            if (cascadePositions)
            {
                var positions = await _dbContext.MstPositions
                    .Where(x => x.DepartmentId == id && !x.IsDelete)
                    .ToListAsync();

                foreach (var position in positions)
                {
                    position.IsDelete = true;
                    position.IsActive = false;
                    position.DeleteDateTime = now;
                    position.DeleteBy = userId;
                }
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.DeleteDepartment",
                "Department berhasil dihapus.",
                new
                {
                    entity.Id,
                    entity.DepartmentCode,
                    entity.DepartmentName,
                    cascadePositions
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Department berhasil dihapus."
            ));
        }

        // =========================================================
        // POSITION
        // =========================================================

        [HttpGet("positions")]
        [ProducesResponseType(typeof(ApiResponse<ResponsePositionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "PositionIndex",
            displayName: "Lihat Data Position",
            Description = "Melihat daftar position pada organization",
            SortOrder = 9
        )]
        [AccessPermission("Organization", "PositionIndex")]
        public async Task<IActionResult> GetPositions(
            [FromQuery] Guid? departmentId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            var query = _dbContext.MstPositions
                .AsNoTracking()
                .Include(x => x.Department)
                .Where(x => !x.IsDelete);

            if (departmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == departmentId.Value);
            }

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PositionCode.ToLower().Contains(keyword) ||
                    x.PositionName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentCode.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .ThenBy(x => x.Department!.DepartmentName)
                .ThenBy(x => x.PositionName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrganizationPositionResponse
                {
                    Id = x.Id,
                    DepartmentId = x.DepartmentId,
                    DepartmentCode = x.Department != null ? x.Department.DepartmentCode : string.Empty,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
                    PositionCode = x.PositionCode,
                    PositionName = x.PositionName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponsePositionPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.GetPositions",
                "Mengambil data position organization.",
                new
                {
                    departmentId,
                    startDate,
                    endDate,
                    customPeriod,
                    search,
                    isActive,
                    pageNumber,
                    pageSize,
                    totalData
                }
            );

            return Ok(ApiResponse<ResponsePositionPagedResult>.Ok(
                result,
                "Data position berhasil diambil."
            ));
        }

        [HttpGet("positions/options")]
        [ProducesResponseType(typeof(ApiResponse<List<OrganizationPositionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "PositionOptions",
            displayName: "Pilihan Position",
            Description = "Mengambil pilihan position berdasarkan department",
            SortOrder = 10
        )]
        [AccessPermission("Organization", "PositionOptions")]
        public async Task<IActionResult> GetPositionOptions([FromQuery] Guid departmentId)
        {
            if (departmentId == Guid.Empty)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.GetPositionOptions",
                    "Gagal mengambil pilihan position. Department kosong."
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department wajib dipilih."
                ));
            }

            var data = await _dbContext.MstPositions
                .AsNoTracking()
                .Where(x =>
                    x.DepartmentId == departmentId &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderBy(x => x.PositionName)
                .Select(x => new OrganizationPositionOptionResponse
                {
                    Id = x.Id,
                    DepartmentId = x.DepartmentId,
                    PositionCode = x.PositionCode,
                    PositionName = x.PositionName
                })
                .ToListAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.GetPositionOptions",
                "Mengambil data pilihan position.",
                new
                {
                    departmentId,
                    TotalData = data.Count
                }
            );

            return Ok(ApiResponse<List<OrganizationPositionOptionResponse>>.Ok(
                data,
                "Data pilihan position berhasil diambil."
            ));
        }

        [HttpGet("positions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationPositionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "PositionDetail",
            displayName: "Detail Position",
            Description = "Melihat detail position pada organization",
            SortOrder = 11
        )]
        [AccessPermission("Organization", "PositionDetail")]
        public async Task<IActionResult> GetPositionById(Guid id)
        {
            var data = await _dbContext.MstPositions
                .AsNoTracking()
                .Include(x => x.Department)
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new OrganizationPositionResponse
                {
                    Id = x.Id,
                    DepartmentId = x.DepartmentId,
                    DepartmentCode = x.Department != null ? x.Department.DepartmentCode : string.Empty,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
                    PositionCode = x.PositionCode,
                    PositionName = x.PositionName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.GetPositionById",
                    "Position tidak ditemukan.",
                    new { Id = id }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position tidak ditemukan."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.GetPositionById",
                "Mengambil detail position.",
                new { Id = id }
            );

            return Ok(ApiResponse<OrganizationPositionResponse>.Ok(
                data,
                "Detail position berhasil diambil."
            ));
        }

        [HttpPost("positions")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "PositionCreate",
            displayName: "Tambah Position",
            Description = "Membuat position baru pada organization",
            SortOrder = 12
        )]
        [AccessPermission("Organization", "PositionCreate")]
        public async Task<IActionResult> CreatePosition([FromBody] CreateOrganizationPositionRequest request)
        {
            if (request.DepartmentId == Guid.Empty)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.CreatePosition",
                    "Gagal membuat position. Department kosong.",
                    request
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department wajib dipilih."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.PositionName))
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.CreatePosition",
                    "Gagal membuat position. Nama position kosong.",
                    request
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama position wajib diisi."
                ));
            }

            var department = await _dbContext.MstDepartments
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.DepartmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (department == null)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.CreatePosition",
                    "Gagal membuat position. Department tidak valid.",
                    request
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department tidak valid atau tidak aktif."
                ));
            }

            var name = request.PositionName.Trim();

            var nameExists = await _dbContext.MstPositions
                .AnyAsync(x =>
                    x.DepartmentId == request.DepartmentId &&
                    x.PositionName.ToLower() == name.ToLower() &&
                    !x.IsDelete);

            if (nameExists)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.CreatePosition",
                    "Gagal membuat position. Nama position sudah digunakan pada department ini.",
                    new
                    {
                        request.DepartmentId,
                        PositionName = name
                    }
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama position sudah digunakan pada department ini."
                ));
            }

            var code = await GeneratePositionCodeAsync();

            var entity = new MstPosition
            {
                Id = Guid.NewGuid(),
                DepartmentId = request.DepartmentId,
                PositionCode = code,
                PositionName = name,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstPositions.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.CreatePosition",
                "Position berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.DepartmentId,
                    DepartmentName = department.DepartmentName,
                    entity.PositionCode,
                    entity.PositionName,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    entity.Id,
                    entity.DepartmentId,
                    department.DepartmentName,
                    entity.PositionCode,
                    entity.PositionName,
                    entity.Description,
                    entity.IsActive
                },
                "Position berhasil dibuat."
            ));
        }

        [HttpPut("positions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "PositionUpdate",
            displayName: "Ubah Position",
            Description = "Mengubah position pada organization",
            SortOrder = 13
        )]
        [AccessPermission("Organization", "PositionUpdate")]
        public async Task<IActionResult> UpdatePosition(
            Guid id,
            [FromBody] UpdateOrganizationPositionRequest request)
        {
            var entity = await _dbContext.MstPositions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.UpdatePosition",
                    "Gagal update position. Position tidak ditemukan.",
                    new { Id = id }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position tidak ditemukan."
                ));
            }

            if (request.DepartmentId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department wajib dipilih."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.PositionName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama position wajib diisi."
                ));
            }

            var department = await _dbContext.MstDepartments
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.DepartmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (department == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department tidak valid atau tidak aktif."
                ));
            }

            var name = request.PositionName.Trim();

            var nameExists = await _dbContext.MstPositions
                .AnyAsync(x =>
                    x.Id != id &&
                    x.DepartmentId == request.DepartmentId &&
                    x.PositionName.ToLower() == name.ToLower() &&
                    !x.IsDelete);

            if (nameExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama position sudah digunakan pada department ini."
                ));
            }

            entity.DepartmentId = request.DepartmentId;
            entity.PositionName = name;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.UpdatePosition",
                "Position berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.DepartmentId,
                    DepartmentName = department.DepartmentName,
                    entity.PositionCode,
                    entity.PositionName,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Position berhasil diperbarui."
            ));
        }

        [HttpPatch("positions/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "PositionStatus",
            displayName: "Ubah Status Position",
            Description = "Mengubah status position",
            SortOrder = 14
        )]
        [AccessPermission("Organization", "PositionStatus")]
        public async Task<IActionResult> UpdatePositionStatus(
            Guid id,
            [FromBody] UpdateOrganizationStatusRequest request)
        {
            var entity = await _dbContext.MstPositions
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.UpdatePositionStatus",
                    "Gagal update status position. Position tidak ditemukan.",
                    new { Id = id }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position tidak ditemukan."
                ));
            }

            if (request.IsActive && entity.Department != null && !entity.Department.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Position tidak dapat aktif ketika department tidak aktif."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.UpdatePositionStatus",
                "Status position berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.DepartmentId,
                    entity.PositionCode,
                    entity.PositionName,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status position berhasil diperbarui."
            ));
        }

        [HttpDelete("positions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "PositionDelete",
            displayName: "Hapus Position",
            Description = "Menghapus position pada organization",
            SortOrder = 15
        )]
        [AccessPermission("Organization", "PositionDelete")]
        public async Task<IActionResult> DeletePosition(Guid id)
        {
            var entity = await _dbContext.MstPositions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Organization.DeletePosition",
                    "Gagal hapus position. Position tidak ditemukan.",
                    new { Id = id }
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position tidak ditemukan."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.DeletePosition",
                "Position berhasil dihapus.",
                new
                {
                    entity.Id,
                    entity.DepartmentId,
                    entity.PositionCode,
                    entity.PositionName
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Position berhasil dihapus."
            ));
        }

        // =========================================================
        // PRIVATE HELPERS
        // =========================================================

        private async Task<string> GenerateDepartmentCodeAsync()
        {
            const string prefix = "DPT";

            var totalData = await _dbContext.MstDepartments
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber.ToString("D6")}";

                var exists = await _dbContext.MstDepartments
                    .AnyAsync(x => x.DepartmentCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private async Task<string> GeneratePositionCodeAsync()
        {
            const string prefix = "POS";

            var totalData = await _dbContext.MstPositions
                .IgnoreQueryFilters()
                .CountAsync();

            var nextNumber = totalData + 1;

            while (true)
            {
                var code = $"{prefix}{nextNumber.ToString("D6")}";

                var exists = await _dbContext.MstPositions
                    .AnyAsync(x => x.PositionCode == code);

                if (!exists)
                {
                    return code;
                }

                nextNumber++;
            }
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLower();
            var today = DateTime.UtcNow.Date;

            DateTime? start = null;
            DateTime? endExclusive = null;

            switch (period)
            {
                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
                    break;

                case "yesterday":
                    start = today.AddDays(-1);
                    endExclusive = today;
                    break;

                case "last7days":
                    start = today.AddDays(-6);
                    endExclusive = today.AddDays(1);
                    break;

                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddMonths(1);
                    break;

                case "lastmonth":
                    var thisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    start = thisMonth.AddMonths(-1);
                    endExclusive = thisMonth;
                    break;

                case "thisyear":
                    start = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddYears(1);
                    break;
            }

            if (!start.HasValue && startDate.HasValue)
            {
                start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            }

            if (!endExclusive.HasValue && endDate.HasValue)
            {
                endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            }

            return (start, endExclusive);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue("user_id");

            if (Guid.TryParse(userIdText, out var userId))
            {
                return userId;
            }

            return Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}