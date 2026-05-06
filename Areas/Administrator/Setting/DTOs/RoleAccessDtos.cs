using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.Setting.DTOs
{
    public class RoleAccessResourceResponse
    {
        public List<RoleAccessDepartmentResponse> Departments { get; set; } = new();
        public List<RoleAccessModuleResponse> Modules { get; set; } = new();
    }

    public class RoleAccessDepartmentResponse
    {
        public Guid Id { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public List<RoleAccessPositionResponse> Positions { get; set; } = new();
    }

    public class RoleAccessPositionResponse
    {
        public Guid Id { get; set; }
        public string PositionCode { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
    }

    public class RoleAccessModuleResponse
    {
        public Guid Id { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string? AreaName { get; set; }
        public int SortOrder { get; set; }
        public List<RoleAccessControllerResponse> Controllers { get; set; } = new();
    }

    public class RoleAccessControllerResponse
    {
        public Guid Id { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? RoutePath { get; set; }
        public int SortOrder { get; set; }
        public List<RoleAccessActionResponse> Actions { get; set; } = new();
    }

    public class RoleAccessActionResponse
    {
        public Guid Id { get; set; }
        public Guid ControllerAccessId { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AccessType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class RoleAccessPolicyResponse
    {
        public Guid DepartmentId { get; set; }
        public Guid PositionId { get; set; }
        public List<RoleAccessPolicyItemResponse> Permissions { get; set; } = new();
    }

    public class RoleAccessPolicyItemResponse
    {
        public Guid ControllerAccessId { get; set; }
        public Guid ActionAccessId { get; set; }
        public bool IsAllowed { get; set; }
    }

    public class SaveRoleAccessPolicyRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public List<SaveRoleAccessPolicyItemRequest> Permissions { get; set; } = new();
    }

    public class SaveRoleAccessPolicyItemRequest
    {
        [Required]
        public Guid ControllerAccessId { get; set; }

        [Required]
        public Guid ActionAccessId { get; set; }

        public bool IsAllowed { get; set; } = true;
    }
}
