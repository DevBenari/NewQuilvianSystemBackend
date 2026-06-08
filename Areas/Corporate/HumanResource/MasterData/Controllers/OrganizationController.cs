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
        private const string DepartmentCodePrefix = "DPT-RSMMC-";
        private const string PositionCodePrefix = "POS-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public OrganizationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Organization", Description = "Melihat data organization department dan position", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new OrganizationFilterMetadataResponse
            {
                DepartmentDefaultFilter = new OrganizationDepartmentDefaultFilterResponse(),
                PositionDefaultFilter = new OrganizationPositionDefaultFilterResponse(),
                CustomPeriods = new List<OrganizationCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
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
                    new() { Value = "departmentCode", Label = "Kode department" },
                    new() { Value = "departmentName", Label = "Nama department" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
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

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Organization", Description = "Melihat data organization department dan position", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var departmentQuery = _dbContext.Set<MstDepartment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var positionQuery = _dbContext.Set<MstPosition>()
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

            return Ok(ApiResponse<OrganizationSummaryResponse>.Ok(
                result,
                "Ringkasan organization berhasil diambil."
            ));
        }

        [HttpGet("departments")]
        [ProducesResponseType(typeof(ApiResponse<ResponseDepartmentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Organization", Description = "Melihat data department", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetDepartments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? positionId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] bool includePositions = false,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildDepartmentBaseQuery();

            query = ApplyDepartmentDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyDepartmentStandardFilter(query, positionId, isActive, search);

            var totalData = await query.CountAsync();

            var items = await ApplyDepartmentSorting(query, sortBy, sortDirection)
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
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault(),
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

            return Ok(ApiResponse<ResponseDepartmentPagedResult>.Ok(
                result,
                "Data department berhasil diambil."
            ));
        }

        [HttpGet("departments/options")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDepartmentOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Organization", Description = "Melihat data pilihan department", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetDepartmentOptions(
            [FromQuery] Guid? positionId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildDepartmentBaseQuery();

            query = ApplyDepartmentStandardFilter(
                query,
                positionId,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.DepartmentName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrganizationDepartmentOptionResponse
                {
                    Id = x.Id,
                    DepartmentCode = x.DepartmentCode,
                    DepartmentName = x.DepartmentName
                })
                .ToListAsync();

            var result = new OrganizationDepartmentOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<OrganizationDepartmentOptionPagedResponse>.Ok(
                result,
                "Data pilihan department berhasil diambil."
            ));
        }

        [HttpGet("departments/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDepartmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Organization", Description = "Melihat detail department", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetDepartmentById(
            Guid id,
            [FromQuery] bool includePositions = true)
        {
            var data = await BuildDepartmentBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new OrganizationDepartmentResponse
                {
                    Id = x.Id,
                    DepartmentCode = x.DepartmentCode,
                    DepartmentName = x.DepartmentName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault(),

                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : (Guid?)x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault(),

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
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Department tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<OrganizationDepartmentResponse>.Ok(
                data,
                "Detail department berhasil diambil."
            ));
        }

        [HttpPost("departments")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDepartmentCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Organization", Description = "Membuat data department", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Organization", "Create")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateOrganizationDepartmentRequest request)
        {
            var validation = await ValidateDepartmentRequestAsync(null, request.DepartmentName);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data department tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstDepartment
            {
                Id = Guid.NewGuid(),
                DepartmentCode = await GenerateDepartmentCodeAsync(),
                DepartmentName = request.DepartmentName.Trim(),
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDepartment>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new OrganizationDepartmentCreateResponse
            {
                Id = entity.Id,
                DepartmentCode = entity.DepartmentCode,
                DepartmentName = entity.DepartmentName,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.CreateDepartment",
                "Membuat data department.",
                result
            );

            return Ok(ApiResponse<OrganizationDepartmentCreateResponse>.Ok(
                result,
                "Department berhasil dibuat."
            ));
        }

        [HttpPut("departments/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Organization", Description = "Mengubah data department", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Organization", "Update")]
        public async Task<IActionResult> UpdateDepartment(
            Guid id,
            [FromBody] UpdateOrganizationDepartmentRequest request)
        {
            var entity = await _dbContext.Set<MstDepartment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Department tidak ditemukan."
                ));
            }

            var validation = await ValidateDepartmentRequestAsync(id, request.DepartmentName);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data department tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.DepartmentName = request.DepartmentName.Trim();
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!request.IsActive && request.CascadeToPositions)
            {
                var positions = await _dbContext.Set<MstPosition>()
                    .Where(x => x.DepartmentId == id && !x.IsDelete)
                    .ToListAsync();

                foreach (var position in positions)
                {
                    position.IsActive = false;
                    position.UpdateDateTime = now;
                    position.UpdateBy = actorUserId;
                }
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.UpdateDepartment",
                "Mengubah data department.",
                new { entity.Id, entity.DepartmentCode, entity.DepartmentName, entity.IsActive }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Department berhasil diperbarui."
            ));
        }

        [HttpPatch("departments/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Organization Status", Description = "Mengubah status department", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Organization", "Update")]
        public async Task<IActionResult> UpdateDepartmentStatus(
            Guid id,
            [FromBody] UpdateOrganizationStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDepartment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Department tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!request.IsActive && request.CascadeToPositions)
            {
                var positions = await _dbContext.Set<MstPosition>()
                    .Where(x => x.DepartmentId == id && !x.IsDelete)
                    .ToListAsync();

                foreach (var position in positions)
                {
                    position.IsActive = false;
                    position.UpdateDateTime = now;
                    position.UpdateBy = actorUserId;
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status department berhasil diperbarui."
            ));
        }

        [HttpDelete("departments/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Organization", Description = "Menghapus data department", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Organization", "Delete")]
        public async Task<IActionResult> DeleteDepartment(
            Guid id,
            [FromQuery] bool cascadePositions = true)
        {
            var entity = await _dbContext.Set<MstDepartment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Department tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (cascadePositions)
            {
                var positions = await _dbContext.Set<MstPosition>()
                    .Where(x => x.DepartmentId == id && !x.IsDelete)
                    .ToListAsync();

                foreach (var position in positions)
                {
                    position.IsDelete = true;
                    position.IsActive = false;
                    position.DeleteDateTime = now;
                    position.DeleteBy = actorUserId;
                    position.UpdateDateTime = now;
                    position.UpdateBy = actorUserId;
                }
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.DeleteDepartment",
                "Menghapus data department.",
                new { entity.Id, entity.DepartmentCode, entity.DepartmentName, cascadePositions }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Department berhasil dihapus."
            ));
        }

        [HttpGet("positions")]
        [ProducesResponseType(typeof(ApiResponse<ResponsePositionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Organization", Description = "Melihat data position", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetPositions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? departmentId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildPositionBaseQuery();

            query = ApplyPositionDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyPositionStandardFilter(query, departmentId, isActive, search);

            var totalData = await query.CountAsync();

            var items = await ApplyPositionSorting(query, sortBy, sortDirection)
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
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault()
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

            return Ok(ApiResponse<ResponsePositionPagedResult>.Ok(
                result,
                "Data position berhasil diambil."
            ));
        }

        [HttpGet("positions/options")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationPositionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Organization", Description = "Melihat data pilihan position", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetPositionOptions(
            [FromQuery] Guid? departmentId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildPositionBaseQuery();

            query = ApplyPositionStandardFilter(
                query,
                departmentId,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Department != null ? x.Department.DepartmentName : string.Empty)
                .ThenBy(x => x.PositionName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrganizationPositionOptionResponse
                {
                    Id = x.Id,
                    DepartmentId = x.DepartmentId,
                    DepartmentCode = x.Department != null ? x.Department.DepartmentCode : string.Empty,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
                    PositionCode = x.PositionCode,
                    PositionName = x.PositionName
                })
                .ToListAsync();

            var result = new OrganizationPositionOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<OrganizationPositionOptionPagedResponse>.Ok(
                result,
                "Data pilihan position berhasil diambil."
            ));
        }

        [HttpGet("positions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationPositionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Organization", Description = "Melihat detail position", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Organization", "Read")]
        public async Task<IActionResult> GetPositionById(Guid id)
        {
            var data = await BuildPositionBaseQuery()
                .Where(x => x.Id == id)
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
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault(),

                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : (Guid?)x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<OrganizationPositionResponse>.Ok(
                data,
                "Detail position berhasil diambil."
            ));
        }

        [HttpPost("positions")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationPositionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Organization", Description = "Membuat data position", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Organization", "Create")]
        public async Task<IActionResult> CreatePosition([FromBody] CreateOrganizationPositionRequest request)
        {
            var validation = await ValidatePositionRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data position tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstPosition
            {
                Id = Guid.NewGuid(),
                DepartmentId = request.DepartmentId,
                PositionCode = await GeneratePositionCodeAsync(),
                PositionName = request.PositionName.Trim(),
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPosition>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new OrganizationPositionCreateResponse
            {
                Id = entity.Id,
                DepartmentId = entity.DepartmentId,
                DepartmentName = validation.DepartmentName ?? string.Empty,
                PositionCode = entity.PositionCode,
                PositionName = entity.PositionName,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.CreatePosition",
                "Membuat data position.",
                result
            );

            return Ok(ApiResponse<OrganizationPositionCreateResponse>.Ok(
                result,
                "Position berhasil dibuat."
            ));
        }

        [HttpPut("positions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Organization", Description = "Mengubah data position", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Organization", "Update")]
        public async Task<IActionResult> UpdatePosition(
            Guid id,
            [FromBody] UpdateOrganizationPositionRequest request)
        {
            var entity = await _dbContext.Set<MstPosition>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position tidak ditemukan."
                ));
            }

            var validation = await ValidatePositionRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data position tidak valid."
                ));
            }

            entity.DepartmentId = request.DepartmentId;
            entity.PositionName = request.PositionName.Trim();
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Organization.UpdatePosition",
                "Mengubah data position.",
                new { entity.Id, entity.DepartmentId, entity.PositionCode, entity.PositionName, entity.IsActive }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Position berhasil diperbarui."
            ));
        }

        [HttpPatch("positions/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Organization Status", Description = "Mengubah status position", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Organization", "Update")]
        public async Task<IActionResult> UpdatePositionStatus(
            Guid id,
            [FromBody] UpdateOrganizationStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPosition>()
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

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status position berhasil diperbarui."
            ));
        }

        [HttpDelete("positions/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Organization", Description = "Menghapus data position", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("Organization", "Delete")]
        public async Task<IActionResult> DeletePosition(Guid id)
        {
            var entity = await _dbContext.Set<MstPosition>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position tidak ditemukan."
                ));
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
                "Organization.DeletePosition",
                "Menghapus data position.",
                new { entity.Id, entity.DepartmentId, entity.PositionCode, entity.PositionName }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Position berhasil dihapus."
            ));
        }

        private IQueryable<MstDepartment> BuildDepartmentBaseQuery()
        {
            return _dbContext.Set<MstDepartment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private IQueryable<MstPosition> BuildPositionBaseQuery()
        {
            return _dbContext.Set<MstPosition>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDepartment> ApplyDepartmentDateFilter(
            IQueryable<MstDepartment> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = DateTime.UtcNow.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x => x.CreateDateTime >= today && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x => x.CreateDateTime >= today.AddDays(-6) && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x => x.CreateDateTime >= thisMonthStart && x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x => x.CreateDateTime >= lastMonthStart && x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstPosition> ApplyPositionDateFilter(
            IQueryable<MstPosition> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = DateTime.UtcNow.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x => x.CreateDateTime >= today && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x => x.CreateDateTime >= today.AddDays(-6) && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x => x.CreateDateTime >= thisMonthStart && x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x => x.CreateDateTime >= lastMonthStart && x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstDepartment> ApplyDepartmentStandardFilter(
            IQueryable<MstDepartment> query,
            Guid? positionId,
            bool? isActive,
            string? search)
        {
            if (positionId.HasValue && positionId.Value != Guid.Empty)
                query = query.Where(x => x.Positions.Any(p => p.Id == positionId.Value && !p.IsDelete));

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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

            return query;
        }

        private static IQueryable<MstPosition> ApplyPositionStandardFilter(
            IQueryable<MstPosition> query,
            Guid? departmentId,
            bool? isActive,
            string? search)
        {
            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
                query = query.Where(x => x.DepartmentId == departmentId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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

            return query;
        }

        private static IOrderedQueryable<MstDepartment> ApplyDepartmentSorting(
            IQueryable<MstDepartment> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "departmentcode" => isDescending ? query.OrderByDescending(x => x.DepartmentCode) : query.OrderBy(x => x.DepartmentCode),
                "departmentname" => isDescending ? query.OrderByDescending(x => x.DepartmentName) : query.OrderBy(x => x.DepartmentName),
                "positioncount" => isDescending ? query.OrderByDescending(x => x.Positions.Count(p => !p.IsDelete)) : query.OrderBy(x => x.Positions.Count(p => !p.IsDelete)),
                "activepositioncount" => isDescending ? query.OrderByDescending(x => x.Positions.Count(p => !p.IsDelete && p.IsActive)) : query.OrderBy(x => x.Positions.Count(p => !p.IsDelete && p.IsActive)),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDescending ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.DepartmentName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.DepartmentName)
            };
        }

        private static IOrderedQueryable<MstPosition> ApplyPositionSorting(
            IQueryable<MstPosition> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "positioncode" => isDescending ? query.OrderByDescending(x => x.PositionCode) : query.OrderBy(x => x.PositionCode),
                "positionname" => isDescending ? query.OrderByDescending(x => x.PositionName) : query.OrderBy(x => x.PositionName),
                "departmentcode" => isDescending
                    ? query.OrderByDescending(x => x.Department != null ? x.Department.DepartmentCode : string.Empty)
                    : query.OrderBy(x => x.Department != null ? x.Department.DepartmentCode : string.Empty),
                "departmentname" => isDescending
                    ? query.OrderByDescending(x => x.Department != null ? x.Department.DepartmentName : string.Empty)
                    : query.OrderBy(x => x.Department != null ? x.Department.DepartmentName : string.Empty),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDescending ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.PositionName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.PositionName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDepartmentRequestAsync(
            Guid? excludeId,
            string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
                return (false, "Nama department wajib diisi.");

            var normalizedName = departmentName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstDepartment>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DepartmentName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Nama department sudah digunakan.");

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage, string? DepartmentName)> ValidatePositionRequestAsync(
            Guid? excludeId,
            CreateOrganizationPositionRequest request)
        {
            if (request.DepartmentId == Guid.Empty)
                return (false, "Department wajib dipilih.", null);

            if (string.IsNullOrWhiteSpace(request.PositionName))
                return (false, "Nama position wajib diisi.", null);

            var department = await _dbContext.Set<MstDepartment>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == request.DepartmentId &&
                    !x.IsDelete &&
                    x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.DepartmentName
                })
                .FirstOrDefaultAsync();

            if (department == null)
                return (false, "Department tidak ditemukan atau tidak aktif.", null);

            var normalizedName = request.PositionName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstPosition>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DepartmentId == request.DepartmentId &&
                    x.PositionName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Nama position sudah digunakan pada department ini.", null);

            return (true, null, department.DepartmentName);
        }

        private async Task<string> GenerateDepartmentCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstDepartment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.DepartmentCode.StartsWith(DepartmentCodePrefix))
                .Select(x => x.DepartmentCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(DepartmentCodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return DepartmentCodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<string> GeneratePositionCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstPosition>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PositionCode.StartsWith(PositionCodePrefix))
                .Select(x => x.PositionCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(PositionCodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return PositionCodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

            return (pageNumber, pageSize);
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
