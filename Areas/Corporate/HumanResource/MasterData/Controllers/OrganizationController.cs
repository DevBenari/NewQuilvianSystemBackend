using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
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
        // FILTER METADATA
        // =========================================================

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Organization",
            Description = "Melihat data organization department dan position",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new OrganizationFilterMetadataResponse
            {
                DepartmentDefaultFilter = new OrganizationDefaultFilterResponse
                {
                    StartDate = null,
                    EndDate = null,
                    CustomPeriod = null,
                    Search = null,
                    IsActive = null,
                    DepartmentId = null,
                    IncludePositions = false,
                    SortBy = "createDateTime",
                    SortDirection = "desc",
                    PageNumber = 1,
                    PageSize = 25
                },
                PositionDefaultFilter = new OrganizationDefaultFilterResponse
                {
                    StartDate = null,
                    EndDate = null,
                    CustomPeriod = null,
                    Search = null,
                    IsActive = null,
                    DepartmentId = null,
                    IncludePositions = false,
                    SortBy = "createDateTime",
                    SortDirection = "desc",
                    PageNumber = 1,
                    PageSize = 25
                },
                CustomPeriods = BuildCustomPeriodOptions(),
                DepartmentSortOptions = new List<OrganizationSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "departmentCode", Label = "Kode department" },
                    new() { Value = "departmentName", Label = "Nama department" },
                    new() { Value = "positionCount", Label = "Jumlah position" },
                    new() { Value = "activePositionCount", Label = "Jumlah position aktif" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                PositionSortOptions = new List<OrganizationSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "positionCode", Label = "Kode position" },
                    new() { Value = "positionName", Label = "Nama position" },
                    new() { Value = "departmentName", Label = "Nama department" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                QueryParameters = new List<OrganizationQueryParameterInfoResponse>
                {
                    new()
                    {
                        Name = "startDate",
                        Type = "date",
                        Required = "No",
                        Description = "Tanggal mulai filter berdasarkan CreateDateTime. Dipakai jika customPeriod kosong atau custom.",
                        Example = "2026-01-01"
                    },
                    new()
                    {
                        Name = "endDate",
                        Type = "date",
                        Required = "No",
                        Description = "Tanggal akhir filter berdasarkan CreateDateTime. Sistem akan membaca sampai akhir hari endDate.",
                        Example = "2026-01-31"
                    },
                    new()
                    {
                        Name = "customPeriod",
                        Type = "string",
                        Required = "No",
                        Description = "Pilihan periode cepat. Nilai tersedia dari response CustomPeriods.",
                        Example = "last7days"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Required = "No",
                        Description = "Pencarian teks untuk kode, nama, dan deskripsi.",
                        Example = "finance"
                    },
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Required = "No",
                        Description = "Filter status aktif. Kosongkan untuk semua status.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "departmentId",
                        Type = "guid",
                        Required = "No",
                        Description = "Khusus endpoint positions. Filter position berdasarkan department.",
                        Example = "00000000-0000-0000-0000-000000000000"
                    },
                    new()
                    {
                        Name = "includePositions",
                        Type = "boolean",
                        Required = "No",
                        Description = "Khusus endpoint departments. Jika true, response department menyertakan list position compact.",
                        Example = "false"
                    },
                    new()
                    {
                        Name = "sortBy",
                        Type = "string",
                        Required = "No",
                        Description = "Field sorting. Nilai tersedia dari DepartmentSortOptions atau PositionSortOptions.",
                        Example = "createDateTime"
                    },
                    new()
                    {
                        Name = "sortDirection",
                        Type = "string",
                        Required = "No",
                        Description = "Arah sorting. Nilai: asc atau desc.",
                        Example = "desc"
                    },
                    new()
                    {
                        Name = "pageNumber",
                        Type = "integer",
                        Required = "No",
                        Description = "Nomor halaman. Minimal 1.",
                        Example = "1"
                    },
                    new()
                    {
                        Name = "pageSize",
                        Type = "integer",
                        Required = "No",
                        Description = "Jumlah data per halaman. Maksimal 100.",
                        Example = "25"
                    }
                }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.GetFilterMetadata",
                "Mengambil metadata filter organization.",
                result
            );

            return Ok(ApiResponse<OrganizationFilterMetadataResponse>.Ok(
                result,
                "Metadata filter organization berhasil diambil."
            ));
        }

        // =========================================================
        // SUMMARY
        // =========================================================

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Organization",
            Description = "Melihat data organization department dan position",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Organization", "Read")]
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
            "Read",
            "Read Organization",
            Description = "Melihat data organization department dan position",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetDepartments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] bool includePositions = false,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = _dbContext.MstDepartments
                .AsNoTracking()
                .Where(x => !x.IsDelete);

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

            var sortedQuery = ApplyDepartmentSorting(query, sortBy, sortDirection);

            var items = await sortedQuery
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
                    AppliedStartDate = dateRange.Start,
                    AppliedEndExclusive = dateRange.EndExclusive,
                    search,
                    isActive,
                    includePositions,
                    sortBy,
                    sortDirection,
                    pageNumber,
                    pageSize,
                    totalData
                }
            );

            return Ok(ApiResponse<ResponseDepartmentPagedResult>.Ok(
                result,
                "Data department berhasil ditampilkan."
            ));
        }

        [HttpGet("departments/options")]
        [ProducesResponseType(typeof(ApiResponse<List<OrganizationDepartmentOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Organization",
            Description = "Melihat data organization department dan position",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetDepartmentOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.MstDepartments
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DepartmentCode.ToLower().Contains(keyword) ||
                    x.DepartmentName.ToLower().Contains(keyword));
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
                    search,
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
            "Read",
            "Read Organization",
            Description = "Melihat data organization department dan position",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Organization", "Read")]
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
            "Create",
            "Create Organization",
            Description = "Membuat data organization department atau position",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Organization", "Create")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateOrganizationDepartmentRequest request)
        {
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
                    x.DepartmentName.ToLower() == name.ToLower() &&
                    !x.IsDelete);

            if (nameExists)
            {
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
            "Update",
            "Update Organization",
            Description = "Mengubah data organization department atau position",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Organization", "Update")]
        public async Task<IActionResult> UpdateDepartment(
            Guid id,
            [FromBody] UpdateOrganizationDepartmentRequest request)
        {
            var entity = await _dbContext.MstDepartments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
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
            "Update",
            "Update Organization",
            Description = "Mengubah data organization department atau position",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Organization", "Update")]
        public async Task<IActionResult> UpdateDepartmentStatus(
            Guid id,
            [FromBody] UpdateOrganizationStatusRequest request)
        {
            var entity = await _dbContext.MstDepartments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
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
            "Delete",
            "Delete Organization",
            Description = "Menghapus data organization department atau position",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Organization", "Delete")]
        public async Task<IActionResult> DeleteDepartment(
            Guid id,
            [FromQuery] bool cascadePositions = true)
        {
            var entity = await _dbContext.MstDepartments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
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
            "Read",
            "Read Organization",
            Description = "Melihat data organization department dan position",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetPositions(
            [FromQuery] Guid? departmentId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = _dbContext.MstPositions
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.DepartmentId == departmentId.Value);
            }

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

            var sortedQuery = ApplyPositionSorting(query, sortBy, sortDirection);

            var items = await sortedQuery
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
                    AppliedStartDate = dateRange.Start,
                    AppliedEndExclusive = dateRange.EndExclusive,
                    search,
                    isActive,
                    sortBy,
                    sortDirection,
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
            "Read",
            "Read Organization",
            Description = "Melihat data organization department dan position",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetPositionOptions(
            [FromQuery] Guid departmentId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            if (departmentId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department wajib dipilih."
                ));
            }

            var query = _dbContext.MstPositions
                .AsNoTracking()
                .Where(x =>
                    x.DepartmentId == departmentId &&
                    !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PositionCode.ToLower().Contains(keyword) ||
                    x.PositionName.ToLower().Contains(keyword));
            }

            var data = await query
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
                    onlyActive,
                    search,
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
            "Read",
            "Read Organization",
            Description = "Melihat data organization department dan position",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetPositionById(Guid id)
        {
            var data = await _dbContext.MstPositions
                .AsNoTracking()
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
            "Create",
            "Create Organization",
            Description = "Membuat data organization department atau position",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Organization", "Create")]
        public async Task<IActionResult> CreatePosition([FromBody] CreateOrganizationPositionRequest request)
        {
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
            "Update",
            "Update Organization",
            Description = "Mengubah data organization department atau position",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Organization", "Update")]
        public async Task<IActionResult> UpdatePosition(
            Guid id,
            [FromBody] UpdateOrganizationPositionRequest request)
        {
            var entity = await _dbContext.MstPositions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
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
            "Update",
            "Update Organization",
            Description = "Mengubah data organization department atau position",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Organization", "Update")]
        public async Task<IActionResult> UpdatePositionStatus(
            Guid id,
            [FromBody] UpdateOrganizationStatusRequest request)
        {
            var entity = await _dbContext.MstPositions
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
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
            "Delete",
            "Delete Organization",
            Description = "Menghapus data organization department atau position",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Organization", "Delete")]
        public async Task<IActionResult> DeletePosition(Guid id)
        {
            var entity = await _dbContext.MstPositions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
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

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            return (pageNumber, pageSize);
        }

        private static DateRangeResolveResult ResolveDateRange(
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
                case null:
                case "":
                case "custom":
                    if (startDate.HasValue)
                    {
                        start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                    }

                    if (endDate.HasValue)
                    {
                        endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    }

                    break;

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

                case "last30days":
                    start = today.AddDays(-29);
                    endExclusive = today.AddDays(1);
                    break;

                case "last90days":
                    start = today.AddDays(-89);
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

                default:
                    return DateRangeResolveResult.Invalid(
                        $"customPeriod '{customPeriod}' tidak valid. Gunakan endpoint filters/metadata untuk melihat daftar customPeriod yang tersedia."
                    );
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResolveResult.Invalid(
                    "startDate tidak boleh lebih besar atau sama dengan endDate."
                );
            }

            return DateRangeResolveResult.Valid(start, endExclusive);
        }

        private static IOrderedQueryable<MstDepartment> ApplyDepartmentSorting(
            IQueryable<MstDepartment> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "departmentcode" => desc
                    ? query.OrderByDescending(x => x.DepartmentCode).ThenBy(x => x.DepartmentName)
                    : query.OrderBy(x => x.DepartmentCode).ThenBy(x => x.DepartmentName),

                "departmentname" => desc
                    ? query.OrderByDescending(x => x.DepartmentName).ThenBy(x => x.DepartmentCode)
                    : query.OrderBy(x => x.DepartmentName).ThenBy(x => x.DepartmentCode),

                "positioncount" => desc
                    ? query.OrderByDescending(x => x.Positions.Count(p => !p.IsDelete)).ThenBy(x => x.DepartmentName)
                    : query.OrderBy(x => x.Positions.Count(p => !p.IsDelete)).ThenBy(x => x.DepartmentName),

                "activepositioncount" => desc
                    ? query.OrderByDescending(x => x.Positions.Count(p => !p.IsDelete && p.IsActive)).ThenBy(x => x.DepartmentName)
                    : query.OrderBy(x => x.Positions.Count(p => !p.IsDelete && p.IsActive)).ThenBy(x => x.DepartmentName),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.DepartmentName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.DepartmentName),

                _ => desc
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.DepartmentName)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.DepartmentName)
            };
        }

        private static IOrderedQueryable<MstPosition> ApplyPositionSorting(
            IQueryable<MstPosition> query,
            string? sortBy,
            string? sortDirection)
        {
            var field = NormalizeSortBy(sortBy);
            var desc = IsDescending(sortDirection);

            return field switch
            {
                "positioncode" => desc
                    ? query.OrderByDescending(x => x.PositionCode).ThenBy(x => x.PositionName)
                    : query.OrderBy(x => x.PositionCode).ThenBy(x => x.PositionName),

                "positionname" => desc
                    ? query.OrderByDescending(x => x.PositionName).ThenBy(x => x.PositionCode)
                    : query.OrderBy(x => x.PositionName).ThenBy(x => x.PositionCode),

                "departmentname" => desc
                    ? query.OrderByDescending(x => x.Department != null ? x.Department.DepartmentName : string.Empty).ThenBy(x => x.PositionName)
                    : query.OrderBy(x => x.Department != null ? x.Department.DepartmentName : string.Empty).ThenBy(x => x.PositionName),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.PositionName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.PositionName),

                _ => desc
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.PositionName)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.PositionName)
            };
        }

        private static string NormalizeSortBy(string? sortBy)
        {
            return string.IsNullOrWhiteSpace(sortBy)
                ? "createdatetime"
                : sortBy.Trim().Replace("_", string.Empty).ToLower();
        }

        private static bool IsDescending(string? sortDirection)
        {
            return !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase
            );
        }

        private static List<OrganizationCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<OrganizationCustomPeriodOptionResponse>
            {
                new()
                {
                    Value = "custom",
                    Label = "Custom Date Range",
                    Description = "Frontend mengirim startDate dan/atau endDate manual. Format tanggal: yyyy-MM-dd.",
                    UsesStartDate = true,
                    UsesEndDate = true
                },
                new()
                {
                    Value = "today",
                    Label = "Hari Ini",
                    Description = "Filter data yang dibuat hari ini berdasarkan waktu UTC.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "yesterday",
                    Label = "Kemarin",
                    Description = "Filter data yang dibuat kemarin berdasarkan waktu UTC.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "last7days",
                    Label = "7 Hari Terakhir",
                    Description = "Filter data dari 7 hari terakhir termasuk hari ini.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "last30days",
                    Label = "30 Hari Terakhir",
                    Description = "Filter data dari 30 hari terakhir termasuk hari ini.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "last90days",
                    Label = "90 Hari Terakhir",
                    Description = "Filter data dari 90 hari terakhir termasuk hari ini.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "thismonth",
                    Label = "Bulan Ini",
                    Description = "Filter data dari tanggal 1 bulan berjalan sampai akhir bulan berjalan.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "lastmonth",
                    Label = "Bulan Lalu",
                    Description = "Filter data dari tanggal 1 bulan lalu sampai akhir bulan lalu.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "thisyear",
                    Label = "Tahun Ini",
                    Description = "Filter data dari tanggal 1 Januari tahun berjalan sampai akhir tahun berjalan.",
                    UsesStartDate = false,
                    UsesEndDate = false
                }
            };
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

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

        private sealed class DateRangeResolveResult
        {
            public bool IsValid { get; private set; }

            public string? ErrorMessage { get; private set; }

            public DateTime? Start { get; private set; }

            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResolveResult Valid(
                DateTime? start,
                DateTime? endExclusive)
            {
                return new DateRangeResolveResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResolveResult Invalid(string errorMessage)
            {
                return new DateRangeResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}