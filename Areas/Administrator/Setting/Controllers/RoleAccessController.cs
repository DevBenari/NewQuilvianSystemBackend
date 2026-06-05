using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Authorize]
    [AccessController(
        moduleCode: "SETTING",
        moduleName: "Setting",
        displayName: "Setting / Role Access",
        AreaName = "Administrator",
        ControllerName = RoleAccessControllerName,
        Description = "Manajemen role akses berdasarkan department dan position",
        SortOrder = 990,
        VisibleInRoleAccess = true,
        IsSystemOnly = false
    )]
    public class RoleAccessController : ControllerBase
    {
        private const string RoleAccessControllerName = "RoleAccess";
        private const string EmptyAreaName = "Other";

        private readonly ApplicationDbContext _dbContext;

        public RoleAccessController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Endpoint lama tetap dipertahankan.
        // Tambahan query optional tidak merusak call lama: /resources tetap bisa dipakai seperti sebelumnya.
        [HttpGet("resources")]
        [AccessPermission(RoleAccessControllerName, "Read")]
        [AccessAction(
            "Read",
            "Read Role Access",
            Description = "Melihat resource role access",
            AccessType = AccessTypes.Read,
            VisibleInRoleAccess = true,
            IsSystemOnly = false,
            SortOrder = 1
        )]
        [ProducesResponseType(typeof(ApiResponse<RoleAccessResourceResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResources(
            [FromQuery] string? areaName,
            [FromQuery] string? moduleCode,
            [FromQuery] string? search)
        {
            var isSuperAdmin = IsCurrentUserSuperAdmin();

            var departments = await BuildDepartmentsAsync();
            var modules = await BuildModulesAsync(
                isSuperAdmin,
                areaName,
                moduleCode,
                search,
                selectedActionIds: new HashSet<Guid>()
            );

            var areas = BuildAreas(modules);

            var response = new RoleAccessResourceResponse
            {
                Departments = departments,
                Modules = modules,
                Areas = areas
            };

            return Ok(ApiResponse<RoleAccessResourceResponse>.Ok(
                response,
                "Resource role access berhasil diambil."
            ));
        }

        // Endpoint baru untuk frontend matrix.
        // Ini sudah memisahkan Area > Module/Submodule > Menu/Controller > Action.
        [HttpGet("resources/structured")]
        [AccessPermission(RoleAccessControllerName, "Read")]
        [AccessAction(
            "Read",
            "Read Structured Role Access",
            Description = "Melihat resource role access dalam struktur area, module, controller, dan action",
            AccessType = AccessTypes.Read,
            VisibleInRoleAccess = false,
            IsSystemOnly = false,
            SortOrder = 4
        )]
        [ProducesResponseType(typeof(ApiResponse<RoleAccessStructuredResourceResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStructuredResources(
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] string? areaName,
            [FromQuery] string? moduleCode,
            [FromQuery] string? search)
        {
            var isSuperAdmin = IsCurrentUserSuperAdmin();

            var selectedActionIds = await GetSelectedActionIdsAsync(
                departmentId,
                positionId,
                isSuperAdmin
            );

            var departments = await BuildDepartmentsAsync();
            var modules = await BuildModulesAsync(
                isSuperAdmin,
                areaName,
                moduleCode,
                search,
                selectedActionIds
            );

            var areas = BuildAreas(modules);
            var summary = BuildSummary(areas, departmentId, positionId);

            var response = new RoleAccessStructuredResourceResponse
            {
                Departments = departments,
                Summary = summary,
                Areas = areas
            };

            return Ok(ApiResponse<RoleAccessStructuredResourceResponse>.Ok(
                response,
                "Resource role access terstruktur berhasil diambil."
            ));
        }

        // Endpoint baru untuk kartu summary di UI.
        [HttpGet("summary")]
        [AccessPermission(RoleAccessControllerName, "Read")]
        [AccessAction(
            "Read",
            "Read Role Access Summary",
            Description = "Melihat ringkasan role access berdasarkan department dan position",
            AccessType = AccessTypes.Read,
            VisibleInRoleAccess = false,
            IsSystemOnly = false,
            SortOrder = 5
        )]
        [ProducesResponseType(typeof(ApiResponse<RoleAccessSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSummary(
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] string? areaName,
            [FromQuery] string? moduleCode,
            [FromQuery] string? search)
        {
            if ((departmentId.HasValue && departmentId.Value == Guid.Empty) ||
                (positionId.HasValue && positionId.Value == Guid.Empty))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department dan position tidak valid."
                ));
            }

            if (departmentId.HasValue != positionId.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Department dan position harus diisi bersamaan."
                ));
            }

            var isSuperAdmin = IsCurrentUserSuperAdmin();

            var selectedActionIds = await GetSelectedActionIdsAsync(
                departmentId,
                positionId,
                isSuperAdmin
            );

            var modules = await BuildModulesAsync(
                isSuperAdmin,
                areaName,
                moduleCode,
                search,
                selectedActionIds
            );

            var areas = BuildAreas(modules);
            var summary = BuildSummary(areas, departmentId, positionId);

            return Ok(ApiResponse<RoleAccessSummaryResponse>.Ok(
                summary,
                "Ringkasan role access berhasil diambil."
            ));
        }

        [HttpGet("policies")]
        [AccessPermission(RoleAccessControllerName, "Read")]
        [AccessAction(
            "Read",
            "Read Role Access Policy",
            Description = "Melihat policy role access berdasarkan department dan position",
            AccessType = AccessTypes.Read,
            VisibleInRoleAccess = true,
            IsSystemOnly = false,
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

            var isSuperAdmin = IsCurrentUserSuperAdmin();

            var permissions = await BuildPolicyQuery(departmentId, positionId, isSuperAdmin)
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
        [AccessPermission(RoleAccessControllerName, "Update")]
        [AccessAction(
            "Update",
            "Update Role Access",
            Description = "Menyimpan policy role access berdasarkan department dan position",
            AccessType = AccessTypes.Update,
            VisibleInRoleAccess = true,
            IsSystemOnly = false,
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

            var isSuperAdmin = IsCurrentUserSuperAdmin();
            var currentUserId = GetCurrentUserId();

            var roleValidation = await ValidateDepartmentAndPositionAsync(
                request.DepartmentId,
                request.PositionId
            );

            if (!roleValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    roleValidation.ErrorMessage ?? "Department atau position tidak valid."
                ));
            }

            var saveResult = await ApplyPoliciesAsync(
                request.DepartmentId,
                request.PositionId,
                request.Permissions,
                overwriteTarget: true,
                isSuperAdmin,
                currentUserId
            );

            if (!saveResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    saveResult.ErrorMessage ?? "Policy role access tidak valid."
                ));
            }

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    request.DepartmentId,
                    request.PositionId,
                    TotalAllowed = saveResult.TargetTotalAllowed
                },
                "Policy role access berhasil disimpan."
            ));
        }

        // Endpoint baru untuk fitur Salin dari Role Lain.
        [HttpPost("policies/copy")]
        [AccessPermission(RoleAccessControllerName, "Update")]       
        [ProducesResponseType(typeof(ApiResponse<CopyRoleAccessPolicyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CopyPolicies([FromBody] CopyRoleAccessPolicyRequest request)
        {
            if (request.SourceDepartmentId == Guid.Empty ||
                request.SourcePositionId == Guid.Empty ||
                request.TargetDepartmentId == Guid.Empty ||
                request.TargetPositionId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Role sumber dan role tujuan wajib diisi."
                ));
            }

            if (request.SourceDepartmentId == request.TargetDepartmentId &&
                request.SourcePositionId == request.TargetPositionId)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Role sumber dan role tujuan tidak boleh sama."
                ));
            }

            var isSuperAdmin = IsCurrentUserSuperAdmin();
            var currentUserId = GetCurrentUserId();

            var sourceValidation = await ValidateDepartmentAndPositionAsync(
                request.SourceDepartmentId,
                request.SourcePositionId
            );

            if (!sourceValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Role sumber tidak valid atau tidak aktif."
                ));
            }

            var targetValidation = await ValidateDepartmentAndPositionAsync(
                request.TargetDepartmentId,
                request.TargetPositionId
            );

            if (!targetValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Role tujuan tidak valid atau tidak aktif."
                ));
            }

            var sourcePermissions = await BuildPolicyQuery(
                    request.SourceDepartmentId,
                    request.SourcePositionId,
                    isSuperAdmin
                )
                .Select(x => new SaveRoleAccessPolicyItemRequest
                {
                    ControllerAccessId = x.ControllerAccessId,
                    ActionAccessId = x.ActionAccessId,
                    IsAllowed = true
                })
                .ToListAsync();

            if (!sourcePermissions.Any())
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Role sumber belum memiliki akses yang bisa disalin."
                ));
            }

            var saveResult = await ApplyPoliciesAsync(
                request.TargetDepartmentId,
                request.TargetPositionId,
                sourcePermissions,
                request.OverwriteTarget,
                isSuperAdmin,
                currentUserId
            );

            if (!saveResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    saveResult.ErrorMessage ?? "Gagal menyalin role access."
                ));
            }

            var response = new CopyRoleAccessPolicyResponse
            {
                SourceDepartmentId = request.SourceDepartmentId,
                SourcePositionId = request.SourcePositionId,
                SourceDepartmentName = sourceValidation.DepartmentName,
                SourcePositionName = sourceValidation.PositionName,
                TargetDepartmentId = request.TargetDepartmentId,
                TargetPositionId = request.TargetPositionId,
                TargetDepartmentName = targetValidation.DepartmentName,
                TargetPositionName = targetValidation.PositionName,
                OverwriteTarget = request.OverwriteTarget,
                SourcePermissionCount = sourcePermissions.Count,
                CopiedPermissionCount = saveResult.CopiedPermissionCount,
                SkippedPermissionCount = saveResult.SkippedPermissionCount,
                TargetTotalAllowed = saveResult.TargetTotalAllowed
            };

            return Ok(ApiResponse<CopyRoleAccessPolicyResponse>.Ok(
                response,
                request.OverwriteTarget
                    ? "Role access berhasil disalin dan mengganti akses tujuan."
                    : "Role access berhasil ditambahkan ke akses tujuan."
            ));
        }

        private async Task<List<RoleAccessDepartmentResponse>> BuildDepartmentsAsync()
        {
            return await _dbContext.MstDepartments
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
        }

        private async Task<List<RoleAccessModuleResponse>> BuildModulesAsync(
            bool isSuperAdmin,
            string? areaName,
            string? moduleCode,
            string? search,
            HashSet<Guid> selectedActionIds)
        {
            var moduleQuery = _dbContext.SysApplicationModules
                .AsNoTracking()
                .Where(m => m.IsActive && !m.IsDelete);

            if (!string.IsNullOrWhiteSpace(areaName))
            {
                var normalizedArea = areaName.Trim().ToLower();
                moduleQuery = moduleQuery.Where(m =>
                    m.AreaName != null &&
                    m.AreaName.ToLower() == normalizedArea);
            }

            if (!string.IsNullOrWhiteSpace(moduleCode))
            {
                var normalizedModuleCode = moduleCode.Trim().ToLower();
                moduleQuery = moduleQuery.Where(m =>
                    m.ModuleCode.ToLower() == normalizedModuleCode);
            }

            var modules = await moduleQuery
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
                            !c.IsSystemOnly &&
                            (
                                isSuperAdmin ||
                                c.ControllerName != RoleAccessControllerName
                            ))
                        .OrderBy(c => c.SortOrder)
                        .ThenBy(c => c.DisplayName)
                        .Select(c => new RoleAccessControllerResponse
                        {
                            Id = c.Id,
                            ControllerName = c.ControllerName,
                            DisplayName = c.DisplayName,
                            RoutePath = c.RoutePath,
                            SortOrder = c.SortOrder,
                            IsSystemOnly = c.IsSystemOnly,
                            CanAssign = true,
                            Actions = c.Actions
                                .Where(a =>
                                    a.IsActive &&
                                    !a.IsDelete &&
                                    a.VisibleInRoleAccess &&
                                    !a.IsSystemOnly &&
                                    AccessTypes.AllowedForRoleAccess.Contains(a.AccessType) &&
                                    (
                                        isSuperAdmin ||
                                        c.ControllerName != RoleAccessControllerName
                                    ))
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
                                    SortOrder = a.SortOrder,
                                    IsSystemOnly = a.IsSystemOnly,
                                    CanAssign = true,
                                    IsSelected = false
                                })
                                .ToList()
                        })
                        .Where(c => c.Actions.Any())
                        .ToList()
                })
                .Where(m => m.Controllers.Any())
                .ToListAsync();

            HydrateModuleUiFields(modules, selectedActionIds);

            if (!string.IsNullOrWhiteSpace(search))
            {
                modules = ApplyModuleSearch(modules, search);
                HydrateModuleUiFields(modules, selectedActionIds);
            }

            return modules;
        }

        private IQueryable<SysAccessPolicy> BuildPolicyQuery(
            Guid departmentId,
            Guid positionId,
            bool isSuperAdmin)
        {
            return _dbContext.SysAccessPolicies
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
                    AccessTypes.AllowedForRoleAccess.Contains(x.ActionAccess.AccessType) &&
                    (
                        isSuperAdmin ||
                        x.ControllerAccess.ControllerName != RoleAccessControllerName
                    ));
        }

        private async Task<HashSet<Guid>> GetSelectedActionIdsAsync(
            Guid? departmentId, Guid? positionId, bool isSuperAdmin)
        {
            if (!departmentId.HasValue || !positionId.HasValue ||
                departmentId.Value == Guid.Empty || positionId.Value == Guid.Empty)
            {
                return new HashSet<Guid>();
            }

            var actionIds = await BuildPolicyQuery(
                    departmentId.Value,
                    positionId.Value,
                    isSuperAdmin
                )
                .Select(x => x.ActionAccessId)
                .ToListAsync();

            return actionIds.ToHashSet();
        }

        private static void HydrateModuleUiFields(
            List<RoleAccessModuleResponse> modules,
            HashSet<Guid> selectedActionIds)
        {
            foreach (var module in modules)
            {
                var areaName = NormalizeAreaName(module.AreaName);
                var areaDisplayName = FormatAreaDisplayName(areaName);
                var moduleSegments = BuildModulePathSegments(
                    module.ModuleCode,
                    module.ModuleName,
                    areaName
                );

                module.AreaName = areaName;
                module.AreaDisplayName = areaDisplayName;
                module.ModulePathSegments = moduleSegments;
                module.ModuleDisplayName = string.Join(" / ", moduleSegments);
                module.FullPath = JoinPath(areaDisplayName, module.ModuleDisplayName);

                foreach (var controller in module.Controllers)
                {
                    controller.MenuName = ExtractMenuName(controller.DisplayName, controller.ControllerName);
                    controller.FullPath = JoinPath(module.FullPath, controller.MenuName);
                    controller.TotalAction = controller.Actions.Count;

                    foreach (var action in controller.Actions)
                    {
                        action.IsSelected = selectedActionIds.Contains(action.Id);
                    }
                }

                module.TotalController = module.Controllers.Count;
                module.TotalAction = module.Controllers.Sum(x => x.Actions.Count);
            }
        }

        private static List<RoleAccessModuleResponse> ApplyModuleSearch(
            List<RoleAccessModuleResponse> modules,
            string search)
        {
            var keyword = NormalizeSearchText(search);

            if (string.IsNullOrWhiteSpace(keyword))
                return modules;

            var result = new List<RoleAccessModuleResponse>();

            foreach (var module in modules)
            {
                var moduleMatched = ContainsSearch(module.AreaDisplayName, keyword) ||
                                    ContainsSearch(module.ModuleCode, keyword) ||
                                    ContainsSearch(module.ModuleName, keyword) ||
                                    ContainsSearch(module.ModuleDisplayName, keyword) ||
                                    ContainsSearch(module.FullPath, keyword);

                var matchedControllers = module.Controllers
                    .Where(controller =>
                        moduleMatched ||
                        ContainsSearch(controller.ControllerName, keyword) ||
                        ContainsSearch(controller.DisplayName, keyword) ||
                        ContainsSearch(controller.MenuName, keyword) ||
                        ContainsSearch(controller.RoutePath, keyword) ||
                        ContainsSearch(controller.FullPath, keyword) ||
                        controller.Actions.Any(action =>
                            ContainsSearch(action.ActionName, keyword) ||
                            ContainsSearch(action.DisplayName, keyword) ||
                            ContainsSearch(action.AccessType, keyword) ||
                            ContainsSearch(action.Description, keyword)))
                    .ToList();

                if (!matchedControllers.Any())
                    continue;

                result.Add(new RoleAccessModuleResponse
                {
                    Id = module.Id,
                    ModuleCode = module.ModuleCode,
                    ModuleName = module.ModuleName,
                    AreaName = module.AreaName,
                    SortOrder = module.SortOrder,
                    Controllers = matchedControllers,
                    AreaDisplayName = module.AreaDisplayName,
                    ModuleDisplayName = module.ModuleDisplayName,
                    ModulePathSegments = module.ModulePathSegments,
                    FullPath = module.FullPath,
                    TotalController = matchedControllers.Count,
                    TotalAction = matchedControllers.Sum(x => x.Actions.Count)
                });
            }

            return result;
        }

        private static List<RoleAccessAreaResponse> BuildAreas(List<RoleAccessModuleResponse> modules)
        {
            return modules
                .GroupBy(module => module.AreaName ?? EmptyAreaName)
                .Select(areaGroup =>
                {
                    var areaName = NormalizeAreaName(areaGroup.Key);
                    var areaDisplayName = FormatAreaDisplayName(areaName);
                    var structuredModules = areaGroup
                        .OrderBy(m => m.SortOrder)
                        .ThenBy(m => m.ModuleDisplayName)
                        .Select(module =>
                        {
                            var controllers = module.Controllers
                                .OrderBy(c => c.SortOrder)
                                .ThenBy(c => c.MenuName)
                                .Select(controller =>
                                {
                                    var actions = controller.Actions
                                        .OrderBy(a => a.SortOrder)
                                        .ThenBy(a => a.DisplayName)
                                        .Select(action => new RoleAccessStructuredActionResponse
                                        {
                                            Id = action.Id,
                                            ControllerAccessId = action.ControllerAccessId,
                                            ActionName = action.ActionName,
                                            DisplayName = action.DisplayName,
                                            AccessType = action.AccessType,
                                            Description = action.Description,
                                            SortOrder = action.SortOrder,
                                            IsSystemOnly = action.IsSystemOnly,
                                            CanAssign = action.CanAssign,
                                            IsSelected = action.IsSelected
                                        })
                                        .ToList();

                                    var selectedAction = actions.Count(a => a.IsSelected);

                                    return new RoleAccessStructuredControllerResponse
                                    {
                                        Id = controller.Id,
                                        ControllerName = controller.ControllerName,
                                        DisplayName = controller.DisplayName,
                                        MenuName = controller.MenuName,
                                        RoutePath = controller.RoutePath,
                                        SortOrder = controller.SortOrder,
                                        FullPath = controller.FullPath,
                                        IsSystemOnly = controller.IsSystemOnly,
                                        CanAssign = controller.CanAssign,
                                        TotalAction = actions.Count,
                                        SelectedAction = selectedAction,
                                        IsFullAccess = actions.Count > 0 && selectedAction == actions.Count,
                                        Actions = actions
                                    };
                                })
                                .ToList();

                            var moduleTotalAction = controllers.Sum(c => c.TotalAction);
                            var moduleSelectedAction = controllers.Sum(c => c.SelectedAction);

                            return new RoleAccessStructuredModuleResponse
                            {
                                Id = module.Id,
                                ModuleCode = module.ModuleCode,
                                ModuleName = module.ModuleName,
                                ModuleDisplayName = module.ModuleDisplayName,
                                AreaName = areaName,
                                AreaDisplayName = areaDisplayName,
                                SortOrder = module.SortOrder,
                                PathSegments = module.ModulePathSegments,
                                FullPath = module.FullPath,
                                TotalController = controllers.Count,
                                TotalAction = moduleTotalAction,
                                SelectedAction = moduleSelectedAction,
                                SelectedPercentage = CalculatePercentage(moduleSelectedAction, moduleTotalAction),
                                Controllers = controllers
                            };
                        })
                        .ToList();

                    var totalAction = structuredModules.Sum(m => m.TotalAction);
                    var selectedAction = structuredModules.Sum(m => m.SelectedAction);

                    return new RoleAccessAreaResponse
                    {
                        AreaName = areaName,
                        AreaDisplayName = areaDisplayName,
                        SortOrder = GetAreaSortOrder(areaName),
                        TotalModule = structuredModules.Count,
                        TotalController = structuredModules.Sum(m => m.TotalController),
                        TotalAction = totalAction,
                        SelectedAction = selectedAction,
                        SelectedPercentage = CalculatePercentage(selectedAction, totalAction),
                        Modules = structuredModules
                    };
                })
                .OrderBy(a => a.SortOrder)
                .ThenBy(a => a.AreaDisplayName)
                .ToList();
        }

        private static RoleAccessSummaryResponse BuildSummary(
            List<RoleAccessAreaResponse> areas,
            Guid? departmentId,
            Guid? positionId)
        {
            var totalAction = areas.Sum(a => a.TotalAction);
            var selectedAction = areas.Sum(a => a.SelectedAction);

            var accessTypes = areas
                .SelectMany(area => area.Modules)
                .SelectMany(module => module.Controllers)
                .SelectMany(controller => controller.Actions)
                .GroupBy(action => action.AccessType)
                .Select(group => new RoleAccessAccessTypeSummaryResponse
                {
                    AccessType = group.Key,
                    TotalAction = group.Count(),
                    SelectedAction = group.Count(x => x.IsSelected)
                })
                .OrderBy(x => GetAccessTypeSortOrder(x.AccessType))
                .ThenBy(x => x.AccessType)
                .ToList();

            return new RoleAccessSummaryResponse
            {
                DepartmentId = departmentId,
                PositionId = positionId,
                TotalArea = areas.Count,
                TotalModule = areas.Sum(a => a.TotalModule),
                TotalController = areas.Sum(a => a.TotalController),
                TotalAction = totalAction,
                SelectedAction = selectedAction,
                UnselectedAction = Math.Max(totalAction - selectedAction, 0),
                SelectedPercentage = CalculatePercentage(selectedAction, totalAction),
                Areas = areas
                    .Select(a => new RoleAccessAreaSummaryResponse
                    {
                        AreaName = a.AreaName,
                        AreaDisplayName = a.AreaDisplayName,
                        SortOrder = a.SortOrder,
                        TotalModule = a.TotalModule,
                        TotalController = a.TotalController,
                        TotalAction = a.TotalAction,
                        SelectedAction = a.SelectedAction,
                        SelectedPercentage = a.SelectedPercentage
                    })
                    .ToList(),
                AccessTypes = accessTypes
            };
        }

        private async Task<RoleIdentityValidationResult> ValidateDepartmentAndPositionAsync(
            Guid departmentId,
            Guid positionId)
        {
            if (departmentId == Guid.Empty || positionId == Guid.Empty)
            {
                return RoleIdentityValidationResult.Fail("Department dan position wajib diisi.");
            }

            var role = await _dbContext.MstPositions
                .AsNoTracking()
                .Where(position =>
                    position.Id == positionId &&
                    position.DepartmentId == departmentId &&
                    position.IsActive &&
                    !position.IsDelete &&
                    position.Department != null &&
                    position.Department.IsActive &&
                    !position.Department.IsDelete)
                .Select(position => new
                {
                    DepartmentName = position.Department!.DepartmentName,
                    PositionName = position.PositionName
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return RoleIdentityValidationResult.Fail("Position tidak valid atau tidak sesuai department.");
            }

            return RoleIdentityValidationResult.Ok(role.DepartmentName, role.PositionName);
        }

        private async Task<PolicySaveResult> ApplyPoliciesAsync(
            Guid departmentId,
            Guid positionId,
            List<SaveRoleAccessPolicyItemRequest> permissions,
            bool overwriteTarget,
            bool isSuperAdmin,
            Guid currentUserId)
        {
            var requestedPermissions = permissions
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

            if (requestedActionIds.Any())
            {
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
                        !a.ControllerAccess.IsSystemOnly &&
                        (
                            isSuperAdmin ||
                            a.ControllerAccess.ControllerName != RoleAccessControllerName
                        ))
                    .Select(a => new
                    {
                        ActionAccessId = a.Id,
                        a.ControllerAccessId
                    })
                    .ToListAsync();

                if (validActions.Count != requestedActionIds.Count)
                {
                    var message = isSuperAdmin
                        ? "Terdapat akses yang tidak valid, bukan CRUD, hidden, atau system-only."
                        : "Terdapat akses yang tidak valid. User non-SuperAdmin tidak boleh memberikan akses Role Access.";

                    return PolicySaveResult.Fail(message);
                }

                var validActionMap = validActions
                    .ToDictionary(x => x.ActionAccessId, x => x.ControllerAccessId);

                foreach (var permission in requestedPermissions)
                {
                    if (!validActionMap.TryGetValue(permission.ActionAccessId, out var expectedControllerId) ||
                        expectedControllerId != permission.ControllerAccessId)
                    {
                        return PolicySaveResult.Fail("ControllerAccessId dan ActionAccessId tidak sesuai.");
                    }
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var existingPolicies = await _dbContext.SysAccessPolicies
                .Where(x =>
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId &&
                    (
                        isSuperAdmin ||
                        (
                            x.ControllerAccess != null &&
                            x.ControllerAccess.ControllerName != RoleAccessControllerName
                        )
                    ))
                .ToListAsync();

            var now = DateTime.UtcNow;
            var skippedPermissionCount = 0;

            var permissionsToApply = requestedPermissions;

            if (!overwriteTarget)
            {
                var existingActiveKeys = existingPolicies
                    .Where(x => x.IsAllowed && x.IsActive && !x.IsDelete)
                    .Select(x => (x.ControllerAccessId, x.ActionAccessId))
                    .ToHashSet();

                permissionsToApply = requestedPermissions
                    .Where(x => !existingActiveKeys.Contains((x.ControllerAccessId, x.ActionAccessId)))
                    .ToList();

                skippedPermissionCount = requestedPermissions.Count - permissionsToApply.Count;
            }

            if (overwriteTarget)
            {
                foreach (var policy in existingPolicies)
                {
                    policy.IsAllowed = false;
                    policy.IsActive = false;
                    policy.IsDelete = true;
                    policy.DeleteDateTime = now;
                    policy.DeleteBy = currentUserId;
                    policy.UpdateDateTime = now;
                    policy.UpdateBy = currentUserId;
                }
            }

            foreach (var permission in permissionsToApply)
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
                    existing.UpdateDateTime = now;
                    existing.UpdateBy = currentUserId;
                }
                else
                {
                    var policy = new SysAccessPolicy
                    {
                        Id = Guid.NewGuid(),
                        DepartmentId = departmentId,
                        PositionId = positionId,
                        ControllerAccessId = permission.ControllerAccessId,
                        ActionAccessId = permission.ActionAccessId,
                        IsAllowed = true,
                        IsActive = true,
                        CreateDateTime = now,
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
            await transaction.CommitAsync();

            var targetTotalAllowed = await BuildPolicyQuery(departmentId, positionId, isSuperAdmin)
                .CountAsync();

            return PolicySaveResult.Ok(
                copiedPermissionCount: permissionsToApply.Count,
                skippedPermissionCount: skippedPermissionCount,
                targetTotalAllowed: targetTotalAllowed
            );
        }

        private static string NormalizeAreaName(string? areaName)
        {
            return string.IsNullOrWhiteSpace(areaName)
                ? EmptyAreaName
                : areaName.Trim();
        }

        private static string FormatAreaDisplayName(string areaName)
        {
            var normalized = areaName.Replace(" ", string.Empty).Trim();

            return normalized.ToLowerInvariant() switch
            {
                "administrator" => "Administrator",
                "corporate" => "Corporate",
                "healthservices" => "Health Services",
                "healthservice" => "Health Services",
                "selfservices" => "Self Services",
                "selfservice" => "Self Services",
                "other" => "Other",
                _ => SplitPascalAndTitle(areaName)
            };
        }

        private static List<string> BuildModulePathSegments(
            string moduleCode,
            string moduleName,
            string areaName)
        {
            var tokens = moduleCode
                .Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            tokens = RemoveAreaPrefixTokens(tokens, areaName);

            if (!tokens.Any())
            {
                return SplitModuleNameFallback(moduleName, areaName);
            }

            if (tokens.Count > 2 &&
                string.Equals(tokens[^2], "MASTER", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(tokens[^1], "DATA", StringComparison.OrdinalIgnoreCase))
            {
                var prefixTokens = tokens.Take(tokens.Count - 2).ToList();

                if (!prefixTokens.Any())
                    return new List<string> { "Master Data" };

                return new List<string>
                {
                    ToTitleText(prefixTokens),
                    "Master Data"
                };
            }

            return new List<string> { ToTitleText(tokens) };
        }

        private static List<string> RemoveAreaPrefixTokens(List<string> tokens, string areaName)
        {
            var normalizedArea = areaName.Replace(" ", string.Empty).ToLowerInvariant();

            if ((normalizedArea == "healthservices" || normalizedArea == "healthservice") &&
                tokens.Count >= 2 &&
                string.Equals(tokens[0], "HEALTH", StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(tokens[1], "SERVICE", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(tokens[1], "SERVICES", StringComparison.OrdinalIgnoreCase)))
            {
                return tokens.Skip(2).ToList();
            }

            if (normalizedArea == "selfservices" || normalizedArea == "selfservice")
            {
                if (tokens.Count >= 2 &&
                    string.Equals(tokens[0], "SELF", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(tokens[1], "SERVICE", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(tokens[1], "SERVICES", StringComparison.OrdinalIgnoreCase)))
                {
                    return tokens.Skip(2).ToList();
                }
            }

            if (normalizedArea == "administrator" &&
                tokens.Count >= 1 &&
                string.Equals(tokens[0], "ADMINISTRATOR", StringComparison.OrdinalIgnoreCase))
            {
                return tokens.Skip(1).ToList();
            }

            if (normalizedArea == "corporate" &&
                tokens.Count >= 1 &&
                string.Equals(tokens[0], "CORPORATE", StringComparison.OrdinalIgnoreCase))
            {
                return tokens.Skip(1).ToList();
            }

            return tokens;
        }

        private static List<string> SplitModuleNameFallback(string moduleName, string areaName)
        {
            var areaDisplay = FormatAreaDisplayName(areaName);
            var result = moduleName
                .Replace(areaDisplay, string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(areaName, string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();

            return string.IsNullOrWhiteSpace(result)
                ? new List<string> { moduleName }
                : new List<string> { result };
        }

        private static string ExtractMenuName(string? displayName, string controllerName)
        {
            var source = string.IsNullOrWhiteSpace(displayName)
                ? controllerName
                : displayName.Trim();

            if (source.Contains('/'))
            {
                var segments = source
                    .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (segments.Any())
                    return segments.Last();
            }

            return source;
        }

        private static string JoinPath(params string?[] segments)
        {
            return string.Join(
                " / ",
                segments
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
            );
        }

        private static string ToTitleText(List<string> tokens)
        {
            return string.Join(" ", tokens.Select(ToTitleWord));
        }

        private static string ToTitleWord(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var lower = value.Trim().ToLowerInvariant();
            return char.ToUpperInvariant(lower[0]) + lower[1..];
        }

        private static string SplitPascalAndTitle(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var chars = new List<char>();

            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];

                if (i > 0 && char.IsUpper(current) && !char.IsWhiteSpace(value[i - 1]))
                    chars.Add(' ');

                chars.Add(current);
            }

            return string.Join(
                " ",
                new string(chars.ToArray())
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(ToTitleWord)
            );
        }

        private static bool ContainsSearch(string? value, string keyword)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return NormalizeSearchText(value).Contains(keyword);
        }

        private static string NormalizeSearchText(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        private static decimal CalculatePercentage(int selected, int total)
        {
            if (total <= 0)
                return 0;

            return Math.Round(selected * 100m / total, 2);
        }

        private static int GetAreaSortOrder(string areaName)
        {
            return areaName.Replace(" ", string.Empty).ToLowerInvariant() switch
            {
                "administrator" => 10,
                "corporate" => 20,
                "healthservices" => 30,
                "healthservice" => 30,
                "selfservices" => 40,
                "selfservice" => 40,
                _ => 900
            };
        }

        private static int GetAccessTypeSortOrder(string accessType)
        {
            return accessType.Trim().ToLowerInvariant() switch
            {
                "read" => 1,
                "create" => 2,
                "update" => 3,
                "delete" => 4,
                _ => 99
            };
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

        private bool IsCurrentUserSuperAdmin()
        {
            return User.IsInRole("SuperAdmin") ||
                   string.Equals(
                       User.FindFirstValue("user_type"),
                       "SuperAdmin",
                       StringComparison.OrdinalIgnoreCase
                   );
        }

        private sealed class RoleIdentityValidationResult
        {
            public bool IsValid { get; private init; }
            public string? ErrorMessage { get; private init; }
            public string DepartmentName { get; private init; } = string.Empty;
            public string PositionName { get; private init; } = string.Empty;

            public static RoleIdentityValidationResult Ok(string departmentName, string positionName)
            {
                return new RoleIdentityValidationResult
                {
                    IsValid = true,
                    DepartmentName = departmentName,
                    PositionName = positionName
                };
            }

            public static RoleIdentityValidationResult Fail(string errorMessage)
            {
                return new RoleIdentityValidationResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }

        private sealed class PolicySaveResult
        {
            public bool IsValid { get; private init; }
            public string? ErrorMessage { get; private init; }
            public int CopiedPermissionCount { get; private init; }
            public int SkippedPermissionCount { get; private init; }
            public int TargetTotalAllowed { get; private init; }

            public static PolicySaveResult Ok(
                int copiedPermissionCount,
                int skippedPermissionCount,
                int targetTotalAllowed)
            {
                return new PolicySaveResult
                {
                    IsValid = true,
                    CopiedPermissionCount = copiedPermissionCount,
                    SkippedPermissionCount = skippedPermissionCount,
                    TargetTotalAllowed = targetTotalAllowed
                };
            }

            public static PolicySaveResult Fail(string errorMessage)
            {
                return new PolicySaveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
