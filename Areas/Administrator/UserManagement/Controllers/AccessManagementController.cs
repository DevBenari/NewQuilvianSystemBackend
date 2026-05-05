using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/access-management")]
    [AccessController(
        moduleCode: "USER_MANAGEMENT",
        moduleName: "User Management",
        displayName: "Access Management",
        AreaName = "Administrator",
        ControllerName = "AccessManagement",
        Description = "Pengaturan role access berdasarkan department dan position",
        SortOrder = 2
    )]
    [Tags("User Management / Access Management")]
    public class AccessManagementController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public AccessManagementController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AccessManagementListResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Index",
            displayName: "Lihat Data",
            Description = "Melihat daftar role access",
            SortOrder = 1
        )]
        [AccessPermission("AccessManagement", "Index")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            var query = _dbContext.SysAccessPolicies
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x =>
                    !x.IsDelete &&
                    x.Department != null &&
                    !x.Department.IsDelete &&
                    x.Position != null &&
                    !x.Position.IsDelete);

            if (departmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == departmentId.Value);
            }

            if (positionId.HasValue)
            {
                query = query.Where(x => x.PositionId == positionId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.Department!.DepartmentCode.ToLower().Contains(keyword) ||
                    x.Department.DepartmentName.ToLower().Contains(keyword) ||
                    x.Position!.PositionCode.ToLower().Contains(keyword) ||
                    x.Position.PositionName.ToLower().Contains(keyword));
            }

            var groupedQuery = query
                .GroupBy(x => new
                {
                    x.DepartmentId,
                    DepartmentCode = x.Department!.DepartmentCode,
                    DepartmentName = x.Department.DepartmentName,
                    x.PositionId,
                    PositionCode = x.Position!.PositionCode,
                    PositionName = x.Position.PositionName
                })
                .Select(g => new AccessManagementListResponse
                {
                    DepartmentId = g.Key.DepartmentId,
                    DepartmentCode = g.Key.DepartmentCode,
                    DepartmentName = g.Key.DepartmentName,
                    PositionId = g.Key.PositionId,
                    PositionCode = g.Key.PositionCode,
                    PositionName = g.Key.PositionName,
                    TotalPermission = g.Count(x => x.IsAllowed && x.IsActive && !x.IsDelete),
                    LastUpdatedAt = g.Max(x => x.UpdateDateTime ?? x.CreateDateTime),
                    IsActive = g.Any(x => x.IsActive && !x.IsDelete)
                });

            var totalData = await groupedQuery.CountAsync();

            var items = await groupedQuery
                .OrderBy(x => x.DepartmentName)
                .ThenBy(x => x.PositionName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<AccessManagementListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                "AccessManagement",
                "AccessManagement.GetAll",
                "Mengambil daftar role access.",
                new
                {
                    search,
                    departmentId,
                    positionId,
                    isActive,
                    pageNumber,
                    pageSize,
                    totalData
                }
            );

            return Ok(ApiResponse<PagedResult<AccessManagementListResponse>>.Ok(
                result,
                "Daftar role access berhasil diambil."
            ));
        }

        [HttpGet("{departmentId:guid}/{positionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AccessManagementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Detail",
            displayName: "Detail Data",
            Description = "Melihat detail role access",
            SortOrder = 2
        )]
        [AccessPermission("AccessManagement", "Detail")]
        public async Task<IActionResult> GetDetail(Guid departmentId, Guid positionId)
        {
            var department = await _dbContext.MstDepartments
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == departmentId &&
                    !x.IsDelete);

            if (department == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Department tidak ditemukan."
                ));
            }

            var position = await _dbContext.MstPositions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == positionId &&
                    x.DepartmentId == departmentId &&
                    !x.IsDelete);

            if (position == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Position tidak ditemukan atau tidak sesuai dengan department."
                ));
            }

            var allowedPolicies = await _dbContext.SysAccessPolicies
                .AsNoTracking()
                .Where(x =>
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId &&
                    x.IsAllowed &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.ControllerAccessId,
                    x.ActionAccessId
                })
                .ToListAsync();

            var allowedSet = allowedPolicies
                .Select(x => $"{x.ControllerAccessId}|{x.ActionAccessId}")
                .ToHashSet();

            var modules = await _dbContext.SysApplicationModules
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ModuleName)
                .Select(module => new AccessManagementModuleDetailResponse
                {
                    ModuleId = module.Id,
                    ModuleCode = module.ModuleCode,
                    ModuleName = module.ModuleName,
                    AreaName = module.AreaName,
                    SortOrder = module.SortOrder,
                    Controllers = _dbContext.SysControllerAccesses
                        .AsNoTracking()
                        .Where(controller =>
                            controller.ModuleId == module.Id &&
                            controller.IsActive &&
                            !controller.IsDelete)
                        .OrderBy(controller => controller.SortOrder)
                        .ThenBy(controller => controller.DisplayName)
                        .Select(controller => new AccessManagementControllerDetailResponse
                        {
                            ControllerAccessId = controller.Id,
                            ControllerName = controller.ControllerName,
                            DisplayName = controller.DisplayName,
                            RoutePath = controller.RoutePath,
                            SortOrder = controller.SortOrder,
                            Actions = _dbContext.SysActionAccesses
                                .AsNoTracking()
                                .Where(action =>
                                    action.ControllerAccessId == controller.Id &&
                                    action.IsActive &&
                                    !action.IsDelete)
                                .OrderBy(action => action.SortOrder)
                                .ThenBy(action => action.DisplayName)
                                .Select(action => new AccessManagementActionDetailResponse
                                {
                                    ActionAccessId = action.Id,
                                    ActionName = action.ActionName,
                                    DisplayName = action.DisplayName,
                                    HttpMethod = action.HttpMethod,
                                    RoutePath = action.RoutePath,
                                    SortOrder = action.SortOrder,
                                    IsAllowed = false
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            foreach (var module in modules)
            {
                foreach (var controller in module.Controllers)
                {
                    foreach (var action in controller.Actions)
                    {
                        var key = $"{controller.ControllerAccessId}|{action.ActionAccessId}";
                        action.IsAllowed = allowedSet.Contains(key);
                    }
                }
            }

            var response = new AccessManagementDetailResponse
            {
                DepartmentId = department.Id,
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName,
                PositionId = position.Id,
                PositionCode = position.PositionCode,
                PositionName = position.PositionName,
                Modules = modules
            };

            await _loggerService.InfoAsync(
                "AccessManagement",
                "AccessManagement.GetDetail",
                "Mengambil detail role access.",
                new
                {
                    departmentId,
                    positionId,
                    TotalModule = modules.Count
                }
            );

            return Ok(ApiResponse<AccessManagementDetailResponse>.Ok(
                response,
                "Detail role access berhasil diambil."
            ));
        }

        [HttpDelete("{departmentId:guid}/{positionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            actionName: "Delete",
            displayName: "Hapus Data",
            Description = "Menghapus policy akses berdasarkan department dan position",
            SortOrder = 3
        )]
        [AccessPermission("AccessManagement", "Delete")]
        public async Task<IActionResult> DeletePolicies(Guid departmentId, Guid positionId)
        {
            var policies = await _dbContext.SysAccessPolicies
                .Where(x =>
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId &&
                    !x.IsDelete)
                .ToListAsync();

            if (!policies.Any())
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Policy akses tidak ditemukan."
                ));
            }

            foreach (var policy in policies)
            {
                policy.IsDelete = true;
                policy.IsActive = false;
                policy.DeleteDateTime = DateTime.UtcNow;
                policy.DeleteBy = GetCurrentUserId();
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "AccessManagement",
                "AccessManagement.Delete",
                "Policy akses berhasil dihapus.",
                new
                {
                    departmentId,
                    positionId,
                    TotalDeleted = policies.Count
                }
            );

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    departmentId,
                    positionId,
                    totalDeleted = policies.Count
                },
                "Policy akses berhasil dihapus."
            ));
        }

        [HttpGet("resources")]
        [ProducesResponseType(typeof(ApiResponse<List<AccessResourceModuleResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Resources",
            displayName: "Daftar Resource",
            Description = "Mengambil daftar module controller action",
            SortOrder = 4
        )]
        [AccessPermission("AccessManagement", "Resources")]
        public async Task<IActionResult> GetResources()
        {
            var modules = await _dbContext.SysApplicationModules
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ModuleName)
                .Select(module => new AccessResourceModuleResponse
                {
                    ModuleId = module.Id,
                    ModuleCode = module.ModuleCode,
                    ModuleName = module.ModuleName,
                    AreaName = module.AreaName,
                    SortOrder = module.SortOrder,
                    Controllers = _dbContext.SysControllerAccesses
                        .AsNoTracking()
                        .Where(controller =>
                            controller.ModuleId == module.Id &&
                            controller.IsActive &&
                            !controller.IsDelete)
                        .OrderBy(controller => controller.SortOrder)
                        .ThenBy(controller => controller.DisplayName)
                        .Select(controller => new AccessResourceControllerResponse
                        {
                            ControllerAccessId = controller.Id,
                            ControllerName = controller.ControllerName,
                            DisplayName = controller.DisplayName,
                            RoutePath = controller.RoutePath,
                            SortOrder = controller.SortOrder,
                            Actions = _dbContext.SysActionAccesses
                                .AsNoTracking()
                                .Where(action =>
                                    action.ControllerAccessId == controller.Id &&
                                    action.IsActive &&
                                    !action.IsDelete)
                                .OrderBy(action => action.SortOrder)
                                .ThenBy(action => action.DisplayName)
                                .Select(action => new AccessResourceActionResponse
                                {
                                    ActionAccessId = action.Id,
                                    ActionName = action.ActionName,
                                    DisplayName = action.DisplayName,
                                    HttpMethod = action.HttpMethod,
                                    RoutePath = action.RoutePath,
                                    SortOrder = action.SortOrder
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            await _loggerService.InfoAsync(
                "AccessManagement",
                "Resources.Get",
                "Mengambil daftar resource akses.",
                new
                {
                    TotalModule = modules.Count
                }
            );

            return Ok(ApiResponse<List<AccessResourceModuleResponse>>.Ok(
                modules,
                "Daftar resource akses berhasil diambil."
            ));
        }

        [HttpGet("policies")]
        [ProducesResponseType(typeof(ApiResponse<AccessPolicyResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "Policies",
            displayName: "Data Policy",
            Description = "Mengambil data policy akses",
            SortOrder = 5
        )]
        [AccessPermission("AccessManagement", "Policies")]
        public async Task<IActionResult> GetPolicies(
            [FromQuery] Guid departmentId,
            [FromQuery] Guid positionId)
        {
            if (departmentId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department wajib dipilih."
                ));
            }

            if (positionId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Position wajib dipilih."
                ));
            }

            var permissions = await _dbContext.SysAccessPolicies
                .AsNoTracking()
                .Where(x =>
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => new AccessPolicyItemResponse
                {
                    ControllerAccessId = x.ControllerAccessId,
                    ActionAccessId = x.ActionAccessId,
                    IsAllowed = x.IsAllowed
                })
                .ToListAsync();

            var response = new AccessPolicyResponse
            {
                DepartmentId = departmentId,
                PositionId = positionId,
                Permissions = permissions
            };

            await _loggerService.InfoAsync(
                "AccessManagement",
                "Policies.Get",
                "Mengambil data policy akses.",
                new
                {
                    departmentId,
                    positionId,
                    TotalPermission = permissions.Count
                }
            );

            return Ok(ApiResponse<AccessPolicyResponse>.Ok(
                response,
                "Data policy akses berhasil diambil."
            ));
        }

        [HttpPost("policies")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            actionName: "SavePolicies",
            displayName: "Simpan Policy",
            Description = "Menyimpan policy akses berdasarkan department dan position",
            SortOrder = 6
        )]
        [AccessPermission("AccessManagement", "SavePolicies")]
        public async Task<IActionResult> SavePolicies([FromBody] SaveAccessPolicyRequest request)
        {
            if (request.DepartmentId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department wajib dipilih."
                ));
            }

            if (request.PositionId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Position wajib dipilih."
                ));
            }

            var departmentExists = await _dbContext.MstDepartments
                .AnyAsync(x =>
                    x.Id == request.DepartmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!departmentExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department tidak valid atau tidak aktif."
                ));
            }

            var positionExists = await _dbContext.MstPositions
                .AnyAsync(x =>
                    x.Id == request.PositionId &&
                    x.DepartmentId == request.DepartmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!positionExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Position tidak valid atau tidak sesuai dengan department."
                ));
            }

            var allowedPermissions = request.Permissions
                .Where(x => x.IsAllowed)
                .GroupBy(x => new
                {
                    x.ControllerAccessId,
                    x.ActionAccessId
                })
                .Select(x => x.First())
                .ToList();

            var controllerIds = allowedPermissions
                .Select(x => x.ControllerAccessId)
                .Distinct()
                .ToList();

            var actionIds = allowedPermissions
                .Select(x => x.ActionAccessId)
                .Distinct()
                .ToList();

            var validActionCount = await _dbContext.SysActionAccesses
                .CountAsync(action =>
                    actionIds.Contains(action.Id) &&
                    controllerIds.Contains(action.ControllerAccessId) &&
                    action.IsActive &&
                    !action.IsDelete);

            if (validActionCount != allowedPermissions.Count)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Sebagian permission tidak valid."
                ));
            }

            var existingPolicies = await _dbContext.SysAccessPolicies
                .Where(x =>
                    x.DepartmentId == request.DepartmentId &&
                    x.PositionId == request.PositionId &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var policy in existingPolicies)
            {
                policy.IsDelete = true;
                policy.IsActive = false;
                policy.DeleteDateTime = DateTime.UtcNow;
                policy.DeleteBy = GetCurrentUserId();
            }

            foreach (var item in allowedPermissions)
            {
                var policy = new SysAccessPolicy
                {
                    Id = Guid.NewGuid(),
                    DepartmentId = request.DepartmentId,
                    PositionId = request.PositionId,
                    ControllerAccessId = item.ControllerAccessId,
                    ActionAccessId = item.ActionAccessId,
                    IsAllowed = true,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = GetCurrentUserId(),
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.SysAccessPolicies.Add(policy);
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "AccessManagement",
                "Policies.Save",
                "Policy akses berhasil disimpan.",
                new
                {
                    request.DepartmentId,
                    request.PositionId,
                    TotalPermission = allowedPermissions.Count
                }
            );

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    request.DepartmentId,
                    request.PositionId,
                    totalPermission = allowedPermissions.Count
                },
                "Policy akses berhasil disimpan."
            ));
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
    }
}