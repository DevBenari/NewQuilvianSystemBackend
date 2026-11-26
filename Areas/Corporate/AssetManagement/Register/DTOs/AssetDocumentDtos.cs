using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

public sealed class AssetDocumentUpsertRequest
{
    public Guid? AssetId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public string? DocumentNumber { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? HolderName { get; set; }
    public string? Notes { get; set; }
    public IFormFile? File { get; set; }
}

public sealed class AssetDocumentPagedQuery
{
    public string? Search { get; set; }
    public Guid? AssetId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public AssetDocumentExpiryState? ExpiryState { get; set; }
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetDocumentAttachmentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}

public sealed class AssetDocumentResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? GroupName { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool HasExpiry { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? HolderName { get; set; }
    public string? Notes { get; set; }
    public AssetDocumentAttachmentResponse? Attachment { get; set; }
    public AssetDocumentExpiryState ExpiryState { get; set; }
    public string ExpiryStateName { get; set; } = string.Empty;
    public int? DaysToExpiry { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public List<string> AvailableActions { get; set; } = [];
}

public sealed class AssetDocumentTypeBriefResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class AssetDocumentCompletenessResponse
{
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public Guid? OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public int RequiredCount { get; set; }
    public int OwnedCount { get; set; }
    public List<AssetDocumentTypeBriefResponse> MissingDocumentTypes { get; set; } = [];
}

public sealed class AssetDocumentSummaryResponse
{
    public int Total { get; set; }
    public int Expired { get; set; }
    public int ExpiringSoon { get; set; }
    public int IncompleteAssets { get; set; }
    public int WarningDays { get; set; }
}
