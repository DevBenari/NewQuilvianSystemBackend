using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;

public sealed class PettyCashCategoryQuery
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "categoryName";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class CreatePettyCashCategoryRequest
{
    [Required, MaxLength(30)] public string CategoryCode { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string CategoryName { get; set; } = string.Empty;
    [MaxLength(300)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdatePettyCashCategoryRequest : CreatePettyCashCategoryRequest;

public sealed class UpdatePettyCashCategoryStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed class PettyCashCategoryResponse
{
    public Guid Id { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
}

public sealed class PettyCashCategoryDeleteResponse
{
    public Guid Id { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsDelete { get; set; }
}

public sealed class PettyCashCategoryOptionResponse
{
    public Guid Id { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class PettyCashCategorySummaryResponse
{
    public int TotalCategory { get; set; }
    public int ActiveCategory { get; set; }
    public int InactiveCategory { get; set; }
}

public sealed class PettyCashCategoryDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "categoryName";
    public string SortDirection { get; set; } = "asc";
}

public sealed class PettyCashCategoryFilterMetadataResponse
{
    public PettyCashCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<int> PageSizeOptions { get; set; } = new();
    public List<string> SortableFields { get; set; } = new();
}
