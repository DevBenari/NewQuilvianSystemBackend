using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using QuilvianSystemBackend.Areas.Administrator.Setting.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Administrator.Setting.Controllers
{
    [ApiController]
    [Route("api/v1/administrator/setting/role-access")]
    [Tags("Administrator / Setting / Role Access")]
    [Authorize(Roles = "SuperAdmin")]
    [AccessController(
    moduleCode: "SETTING",
    moduleName: "Setting",
    displayName: "Setting / Role Access",
    AreaName = "Administrator",
    Description = "Manajemen role akses berdasarkan department dan position",
    SortOrder = 990,
    VisibleInRoleAccess = false,
    IsSystemOnly = true
)]
    public class RoleAccessController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleAccessController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("resources")]
        [AccessAction(
            "Read",
            "Read Role Access",
            Description = "Melihat resource role access",
            AccessType = AccessTypes.Read,
            VisibleInRoleAccess = false,
            IsSystemOnly = true,
            SortOrder = 1
        )]
        [ProducesResponseType(typeof(ApiResponse<RoleAccessResourceResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResources()
        {
            var departments = await _dbContext.MstDepartments
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.DepartmentName)
                .Select(x => new RoleAccessDepartmentResponse
                {
                    Id = x.Id,
                    DepartmentCode = x.DepartmentCode,
                    DepartmentName = x.DepartmentName,
                    Positions = x.Positions
                        .Where(p => p.IsActive && !p.IsDelete)
                        .OrderBy(p => p.PositionName)
                        .Select(p => new RoleAccessPositionResponse
                        {
                            Id = p.Id,
                            DepartmentId = p.DepartmentId,
                            PositionCode = p.PositionCode,
                            PositionName = p.PositionName
                        })
                        .ToList()
                })
                .ToListAsync();

            var modules = await _dbContext.SysApplicationModules
                .AsNoTracking()
                .Where(m => m.IsActive && !m.IsDelete)
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.ModuleName)
                .Select(m => new RoleAccessModuleResponse
                {
                    Id = m.Id,
                    ModuleCode = m.ModuleCode,
                    ModuleName = m.ModuleName,
                    AreaName = m.AreaName,
                    SortOrder = m.SortOrder,
                    Controllers = m.Controllers
                        .Where(c =>
                            c.IsActive &&
                            !c.IsDelete &&
                            c.VisibleInRoleAccess &&
                            !c.IsSystemOnly)
                        .OrderBy(c => c.SortOrder)
                        .ThenBy(c => c.DisplayName)
                        .Select(c => new RoleAccessControllerResponse
                        {
                            Id = c.Id,
                            ControllerName = c.ControllerName,
                            DisplayName = c.DisplayName,
                            RoutePath = c.RoutePath,
                            SortOrder = c.SortOrder,
                            Actions = c.Actions
                                .Where(a =>
                                    a.IsActive &&
                                    !a.IsDelete &&
                                    a.VisibleInRoleAccess &&
                                    !a.IsSystemOnly &&
                                    AccessTypes.AllowedForRoleAccess.Contains(a.AccessType))
                                .OrderBy(a => a.SortOrder)
                                .ThenBy(a => a.DisplayName)
                                .Select(a => new RoleAccessActionResponse
                                {
                                    Id = a.Id,
                                    ControllerAccessId = a.ControllerAccessId,
                                    ActionName = a.ActionName,
                                    DisplayName = a.DisplayName,
                                    AccessType = a.AccessType,
                                    Description = a.Description,
                                    SortOrder = a.SortOrder
                                })
                                .ToList()
                        })
                        .Where(c => c.Actions.Any())
                        .ToList()
                })
                .Where(m => m.Controllers.Any())
                .ToListAsync();

            var response = new RoleAccessResourceResponse
            {
                Departments = departments,
                Modules = modules
            };

            return Ok(ApiResponse<RoleAccessResourceResponse>.Ok(
                response,
                "Resource role access berhasil diambil."
            ));
        }

        [HttpGet("policies")]
        [AccessAction(
            "Read",
            "Read Role Access Policy",
            Description = "Melihat policy role access berdasarkan department dan position",
            AccessType = AccessTypes.Read,
            VisibleInRoleAccess = false,
            IsSystemOnly = true,
            SortOrder = 2
        )]
        [ProducesResponseType(typeof(ApiResponse<RoleAccessPolicyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPolicies(
            [FromQuery] Guid departmentId,
            [FromQuery] Guid positionId)
        {
            if (departmentId == Guid.Empty || positionId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department dan position wajib diisi."
                ));
            }

            var permissions = await _dbContext.SysAccessPolicies
                .AsNoTracking()
                .Where(x =>
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId &&
                    x.IsAllowed &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.ControllerAccess != null &&
                    x.ControllerAccess.IsActive &&
                    !x.ControllerAccess.IsDelete &&
                    x.ControllerAccess.VisibleInRoleAccess &&
                    !x.ControllerAccess.IsSystemOnly &&
                    x.ActionAccess != null &&
                    x.ActionAccess.IsActive &&
                    !x.ActionAccess.IsDelete &&
                    x.ActionAccess.VisibleInRoleAccess &&
                    !x.ActionAccess.IsSystemOnly &&
                    AccessTypes.AllowedForRoleAccess.Contains(x.ActionAccess.AccessType))
                .Select(x => new RoleAccessPolicyItemResponse
                {
                    ControllerAccessId = x.ControllerAccessId,
                    ActionAccessId = x.ActionAccessId,
                    IsAllowed = x.IsAllowed
                })
                .ToListAsync();

            var response = new RoleAccessPolicyResponse
            {
                DepartmentId = departmentId,
                PositionId = positionId,
                Permissions = permissions
            };

            return Ok(ApiResponse<RoleAccessPolicyResponse>.Ok(
                response,
                "Policy role access berhasil diambil."
            ));
        }

        [HttpPost("policies")]
        [AccessAction(
            "Update",
            "Update Role Access",
            Description = "Menyimpan policy role access berdasarkan department dan position",
            AccessType = AccessTypes.Update,
            VisibleInRoleAccess = false,
            IsSystemOnly = true,
            SortOrder = 3
        )]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SavePolicies([FromBody] SaveRoleAccessPolicyRequest request)
        {
            if (request.DepartmentId == Guid.Empty || request.PositionId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department dan position wajib diisi."
                ));
            }

            var departmentExists = await _dbContext.MstDepartments
                .AsNoTracking()
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
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.PositionId &&
                    x.DepartmentId == request.DepartmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!positionExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Position tidak valid atau tidak sesuai department."
                ));
            }

            var requestedPermissions = request.Permissions
                .Where(x =>
                    x.ControllerAccessId != Guid.Empty &&
                    x.ActionAccessId != Guid.Empty &&
                    x.IsAllowed)
                .GroupBy(x => new
                {
                    x.ControllerAccessId,
                    x.ActionAccessId
                })
                .Select(x => x.First())
                .ToList();

            var requestedActionIds = requestedPermissions
                .Select(x => x.ActionAccessId)
                .Distinct()
                .ToList();

            var validActions = await _dbContext.SysActionAccesses
                .AsNoTracking()
                .Where(a =>
                    requestedActionIds.Contains(a.Id) &&
                    a.IsActive &&
                    !a.IsDelete &&
                    a.VisibleInRoleAccess &&
                    !a.IsSystemOnly &&
                    AccessTypes.AllowedForRoleAccess.Contains(a.AccessType) &&
                    a.ControllerAccess != null &&
                    a.ControllerAccess.IsActive &&
                    !a.ControllerAccess.IsDelete &&
                    a.ControllerAccess.VisibleInRoleAccess &&
                    !a.ControllerAccess.IsSystemOnly)
                .Select(a => new
                {
                    ActionAccessId = a.Id,
                    a.ControllerAccessId
                })
                .ToListAsync();

            if (validActions.Count != requestedActionIds.Count)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Terdapat akses yang tidak valid, bukan CRUD, hidden, atau system-only."
                ));
            }

            var validActionMap = validActions
                .ToDictionary(x => x.ActionAccessId, x => x.ControllerAccessId);

            foreach (var permission in requestedPermissions)
            {
                if (!validActionMap.TryGetValue(permission.ActionAccessId, out var expectedControllerId) ||
                    expectedControllerId != permission.ControllerAccessId)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "ControllerAccessId dan ActionAccessId tidak sesuai."
                    ));
                }
            }

            var currentUserId = GetCurrentUserId();

            var existingPolicies = await _dbContext.SysAccessPolicies
                .Where(x =>
                    x.DepartmentId == request.DepartmentId &&
                    x.PositionId == request.PositionId)
                .ToListAsync();

            foreach (var policy in existingPolicies)
            {
                policy.IsAllowed = false;
                policy.IsActive = false;
                policy.IsDelete = true;
                policy.DeleteDateTime = DateTime.UtcNow;
                policy.DeleteBy = currentUserId;
                policy.UpdateDateTime = DateTime.UtcNow;
                policy.UpdateBy = currentUserId;
            }

            foreach (var permission in requestedPermissions)
            {
                var existing = existingPolicies.FirstOrDefault(x =>
                    x.ControllerAccessId == permission.ControllerAccessId &&
                    x.ActionAccessId == permission.ActionAccessId);

                if (existing != null)
                {
                    existing.IsAllowed = true;
                    existing.IsActive = true;
                    existing.IsDelete = false;
                    existing.DeleteDateTime = null;
                    existing.DeleteBy = Guid.Empty;
                    existing.UpdateDateTime = DateTime.UtcNow;
                    existing.UpdateBy = currentUserId;
                }
                else
                {
                    var policy = new SysAccessPolicy
                    {
                        Id = Guid.NewGuid(),
                        DepartmentId = request.DepartmentId,
                        PositionId = request.PositionId,
                        ControllerAccessId = permission.ControllerAccessId,
                        ActionAccessId = permission.ActionAccessId,
                        IsAllowed = true,
                        IsActive = true,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = currentUserId,
                        UpdateBy = Guid.Empty,
                        DeleteBy = Guid.Empty,
                        CancelBy = Guid.Empty,
                        IsDelete = false,
                        IsCancel = false
                    };

                    _dbContext.SysAccessPolicies.Add(policy);
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    request.DepartmentId,
                    request.PositionId,
                    TotalAllowed = requestedPermissions.Count
                },
                "Policy role access berhasil disimpan."
            ));
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
