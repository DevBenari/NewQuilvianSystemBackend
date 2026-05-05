namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.DTOs
{
    public class AccessResourceModuleResponse
    {
        public Guid ModuleId { get; set; }

        public string ModuleCode { get; set; } = string.Empty;

        public string ModuleName { get; set; } = string.Empty;

        public string? AreaName { get; set; }

        public int SortOrder { get; set; }

        public List<AccessResourceControllerResponse> Controllers { get; set; } = new();
    }

    public class AccessResourceControllerResponse
    {
        public Guid ControllerAccessId { get; set; }

        public string ControllerName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RoutePath { get; set; }

        public int SortOrder { get; set; }

        public List<AccessResourceActionResponse> Actions { get; set; } = new();
    }

    public class AccessResourceActionResponse
    {
        public Guid ActionAccessId { get; set; }

        public string ActionName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? HttpMethod { get; set; }

        public string? RoutePath { get; set; }

        public int SortOrder { get; set; }
    }

    public class AccessPolicyResponse
    {
        public Guid DepartmentId { get; set; }

        public Guid PositionId { get; set; }

        public List<AccessPolicyItemResponse> Permissions { get; set; } = new();
    }

    public class AccessPolicyItemResponse
    {
        public Guid ControllerAccessId { get; set; }

        public Guid ActionAccessId { get; set; }

        public bool IsAllowed { get; set; }
    }

    public class AccessManagementListResponse
    {
        public Guid DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public Guid PositionId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public int TotalPermission { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public bool IsActive { get; set; }
    }

    public class AccessManagementDetailResponse
    {
        public Guid DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public Guid PositionId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public List<AccessManagementModuleDetailResponse> Modules { get; set; } = new();
    }

    public class AccessManagementModuleDetailResponse
    {
        public Guid ModuleId { get; set; }

        public string ModuleCode { get; set; } = string.Empty;

        public string ModuleName { get; set; } = string.Empty;

        public string? AreaName { get; set; }

        public int SortOrder { get; set; }

        public List<AccessManagementControllerDetailResponse> Controllers { get; set; } = new();
    }

    public class AccessManagementControllerDetailResponse
    {
        public Guid ControllerAccessId { get; set; }

        public string ControllerName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RoutePath { get; set; }

        public int SortOrder { get; set; }

        public List<AccessManagementActionDetailResponse> Actions { get; set; } = new();
    }

    public class AccessManagementActionDetailResponse
    {
        public Guid ActionAccessId { get; set; }

        public string ActionName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? HttpMethod { get; set; }

        public string? RoutePath { get; set; }

        public int SortOrder { get; set; }

        public bool IsAllowed { get; set; }
    }

    public class SaveAccessPolicyRequest
    {
        public Guid DepartmentId { get; set; }

        public Guid PositionId { get; set; }

        public List<SaveAccessPolicyItemRequest> Permissions { get; set; } = new();
    }

    public class SaveAccessPolicyItemRequest
    {
        public Guid ControllerAccessId { get; set; }

        public Guid ActionAccessId { get; set; }

        public bool IsAllowed { get; set; }
    }
}
