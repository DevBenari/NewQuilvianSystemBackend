using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.Setting.DTOs
{
    public class RoleAccessResourceResponse
    {
        public List<RoleAccessDepartmentResponse> Departments { get; set; } = new();
        public List<RoleAccessModuleResponse> Modules { get; set; } = new();

        // Tambahan untuk UI baru. Tidak menghapus Modules lama, jadi frontend lama tetap aman.
        public List<RoleAccessAreaResponse> Areas { get; set; } = new();
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

        // Tambahan opsional untuk UI baru.
        public string AreaDisplayName { get; set; } = string.Empty;
        public string ModuleDisplayName { get; set; } = string.Empty;
        public List<string> ModulePathSegments { get; set; } = new();
        public string FullPath { get; set; } = string.Empty;
        public int TotalController { get; set; }
        public int TotalAction { get; set; }
    }

    public class RoleAccessControllerResponse
    {
        public Guid Id { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? RoutePath { get; set; }
        public int SortOrder { get; set; }

        public bool IsSystemOnly { get; set; }
        public bool CanAssign { get; set; } = true;

        public List<RoleAccessActionResponse> Actions { get; set; } = new();

        // Tambahan opsional untuk UI baru.
        public string MenuName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public int TotalAction { get; set; }
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

        public bool IsSystemOnly { get; set; }
        public bool CanAssign { get; set; } = true;

        // Tambahan opsional untuk UI matrix.
        public bool IsSelected { get; set; }
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

    // =========================================================
    // DTO tambahan untuk desain Role Access Matrix baru.
    // =========================================================

    public class RoleAccessStructuredResourceResponse
    {
        public List<RoleAccessDepartmentResponse> Departments { get; set; } = new();
        public RoleAccessSummaryResponse Summary { get; set; } = new();
        public List<RoleAccessAreaResponse> Areas { get; set; } = new();
    }

    public class RoleAccessSummaryResponse
    {
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }

        public int TotalArea { get; set; }
        public int TotalModule { get; set; }
        public int TotalController { get; set; }
        public int TotalAction { get; set; }
        public int SelectedAction { get; set; }
        public int UnselectedAction { get; set; }
        public decimal SelectedPercentage { get; set; }

        public List<RoleAccessAreaSummaryResponse> Areas { get; set; } = new();
        public List<RoleAccessAccessTypeSummaryResponse> AccessTypes { get; set; } = new();
    }

    public class RoleAccessAreaResponse
    {
        public string AreaName { get; set; } = string.Empty;
        public string AreaDisplayName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public int TotalModule { get; set; }
        public int TotalController { get; set; }
        public int TotalAction { get; set; }
        public int SelectedAction { get; set; }
        public decimal SelectedPercentage { get; set; }
        public List<RoleAccessStructuredModuleResponse> Modules { get; set; } = new();
    }

    public class RoleAccessStructuredModuleResponse
    {
        public Guid Id { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleDisplayName { get; set; } = string.Empty;
        public string AreaName { get; set; } = string.Empty;
        public string AreaDisplayName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<string> PathSegments { get; set; } = new();
        public string FullPath { get; set; } = string.Empty;
        public int TotalController { get; set; }
        public int TotalAction { get; set; }
        public int SelectedAction { get; set; }
        public decimal SelectedPercentage { get; set; }
        public List<RoleAccessStructuredControllerResponse> Controllers { get; set; } = new();
    }

    public class RoleAccessStructuredControllerResponse
    {
        public Guid Id { get; set; }
        public string ControllerName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string MenuName { get; set; } = string.Empty;
        public string? RoutePath { get; set; }
        public int SortOrder { get; set; }
        public string FullPath { get; set; } = string.Empty;
        public bool IsSystemOnly { get; set; }
        public bool CanAssign { get; set; } = true;
        public int TotalAction { get; set; }
        public int SelectedAction { get; set; }
        public bool IsFullAccess { get; set; }
        public List<RoleAccessStructuredActionResponse> Actions { get; set; } = new();
    }

    public class RoleAccessStructuredActionResponse
    {
        public Guid Id { get; set; }
        public Guid ControllerAccessId { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AccessType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsSystemOnly { get; set; }
        public bool CanAssign { get; set; } = true;
        public bool IsSelected { get; set; }
    }

    public class RoleAccessAreaSummaryResponse
    {
        public string AreaName { get; set; } = string.Empty;
        public string AreaDisplayName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public int TotalModule { get; set; }
        public int TotalController { get; set; }
        public int TotalAction { get; set; }
        public int SelectedAction { get; set; }
        public decimal SelectedPercentage { get; set; }
    }

    public class RoleAccessAccessTypeSummaryResponse
    {
        public string AccessType { get; set; } = string.Empty;
        public int TotalAction { get; set; }
        public int SelectedAction { get; set; }
    }

    public class CopyRoleAccessPolicyRequest
    {
        [Required]
        public Guid SourceDepartmentId { get; set; }

        [Required]
        public Guid SourcePositionId { get; set; }

        [Required]
        public Guid TargetDepartmentId { get; set; }

        [Required]
        public Guid TargetPositionId { get; set; }

        // true = akses target diganti sama persis dengan role sumber.
        // false = akses sumber ditambahkan ke target tanpa menghapus akses target yang sudah ada.
        public bool OverwriteTarget { get; set; } = true;
    }

    public class CopyRoleAccessPolicyResponse
    {
        public Guid SourceDepartmentId { get; set; }
        public Guid SourcePositionId { get; set; }
        public string SourceDepartmentName { get; set; } = string.Empty;
        public string SourcePositionName { get; set; } = string.Empty;

        public Guid TargetDepartmentId { get; set; }
        public Guid TargetPositionId { get; set; }
        public string TargetDepartmentName { get; set; } = string.Empty;
        public string TargetPositionName { get; set; } = string.Empty;

        public bool OverwriteTarget { get; set; }
        public int SourcePermissionCount { get; set; }
        public int CopiedPermissionCount { get; set; }
        public int SkippedPermissionCount { get; set; }
        public int TargetTotalAllowed { get; set; }
    }
}
