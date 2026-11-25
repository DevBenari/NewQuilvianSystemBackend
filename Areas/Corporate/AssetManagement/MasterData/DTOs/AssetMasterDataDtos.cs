namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;

public sealed class AssetMasterPagedQuery
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ClassificationId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public bool? IsDepreciable { get; set; }
    public bool? IsMedical { get; set; }
    public bool? IsCapitalizable { get; set; }
    public bool? HasExpiry { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetMasterStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed class AssetMasterOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class AssetMasterDeleteResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDelete { get; set; }
}

public abstract class AssetMasterResponseBase
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

public sealed class AssetCategoryUpsertRequest
{
    public string? CategoryCode { get; set; }
    public string? CategoryName { get; set; }
    public bool IsDepreciable { get; set; } = true;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AssetCategoryResponse : AssetMasterResponseBase
{
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsDepreciable { get; set; }
    public string? Description { get; set; }
    public List<AssetMasterOptionResponse> DocumentRequirements { get; set; } = [];
}

public sealed class AssetCategorySummaryResponse
{
    public int TotalCategory { get; set; }
    public int ActiveCategory { get; set; }
    public int InactiveCategory { get; set; }
    public int DepreciableCategory { get; set; }
    public int NonDepreciableCategory { get; set; }
}

public sealed class AssetClassificationUpsertRequest
{
    public string? ClassificationCode { get; set; }
    public string? ClassificationName { get; set; }
    public bool IsMedical { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AssetClassificationResponse : AssetMasterResponseBase
{
    public string ClassificationCode { get; set; } = string.Empty;
    public string ClassificationName { get; set; } = string.Empty;
    public bool IsMedical { get; set; }
}

public sealed class AssetClassificationSummaryResponse
{
    public int TotalClassification { get; set; }
    public int ActiveClassification { get; set; }
    public int InactiveClassification { get; set; }
    public int MedicalClassification { get; set; }
}

public sealed class AssetGroupUpsertRequest
{
    public Guid? CategoryId { get; set; }
    public Guid? ClassificationId { get; set; }
    public string? InventoryCode { get; set; }
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AssetGroupResponse : AssetMasterResponseBase
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid ClassificationId { get; set; }
    public string ClassificationName { get; set; } = string.Empty;
    public bool IsMedical { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsInventoryCodeLocked { get; set; }
}

public sealed class AssetGroupSummaryResponse
{
    public int TotalGroup { get; set; }
    public int ActiveGroup { get; set; }
    public int InactiveGroup { get; set; }
    public int MedicalGroup { get; set; }
    public int LockedInventoryCodeGroup { get; set; }
}

public sealed class AssetInventoryCodeResponse
{
    public Guid GroupId { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsInventoryCodeLocked { get; set; }
    public long? LastSequence { get; set; }
    public string? LastIssuedNumber { get; set; }
    public DateTimeOffset? LastIssuedAt { get; set; }
}

public sealed class AssetOwnershipTypeUpsertRequest
{
    public string? OwnershipCode { get; set; }
    public string? OwnershipName { get; set; }
    public bool IsCapitalizable { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AssetOwnershipTypeResponse : AssetMasterResponseBase
{
    public string OwnershipCode { get; set; } = string.Empty;
    public string OwnershipName { get; set; } = string.Empty;
    public bool IsCapitalizable { get; set; }
}

public sealed class AssetOwnershipTypeSummaryResponse
{
    public int TotalOwnershipType { get; set; }
    public int ActiveOwnershipType { get; set; }
    public int InactiveOwnershipType { get; set; }
    public int CapitalizableOwnershipType { get; set; }
}

public sealed class AssetDocumentTypeUpsertRequest
{
    public string? DocumentTypeCode { get; set; }
    public string? DocumentTypeName { get; set; }
    public bool HasExpiry { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AssetDocumentTypeResponse : AssetMasterResponseBase
{
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool HasExpiry { get; set; }
}

public sealed class AssetDocumentTypeSummaryResponse
{
    public int TotalDocumentType { get; set; }
    public int ActiveDocumentType { get; set; }
    public int InactiveDocumentType { get; set; }
    public int ExpiringDocumentType { get; set; }
}

public sealed class AssetLocationUpsertRequest
{
    public string? LocationCode { get; set; }
    public string? LocationName { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public string? BuildingName { get; set; }
    public string? FloorName { get; set; }
    public string? RoomName { get; set; }
    public Guid? RoomId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AssetLocationResponse : AssetMasterResponseBase
{
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public Guid OrganizationUnitId { get; set; }
    public string OrganizationUnitName { get; set; } = string.Empty;
    public string? BuildingName { get; set; }
    public string? FloorName { get; set; }
    public string? RoomName { get; set; }
    public Guid? RoomId { get; set; }
    public string? LinkedRoomName { get; set; }
}

public sealed class AssetLocationSummaryResponse
{
    public int TotalLocation { get; set; }
    public int ActiveLocation { get; set; }
    public int InactiveLocation { get; set; }
    public int LinkedRoomLocation { get; set; }
}

public sealed class AssetSettingUpdateRequest
{
    public int IdleThresholdDays { get; set; }
    public int DocumentExpiryWarningDays { get; set; }
    public int UtilizationMaxMonthsForAssetHead { get; set; }
    public DateOnly? CutoverDate { get; set; }
}

public sealed class AssetSettingResponse
{
    public Guid? Id { get; set; }
    public bool IsSaved { get; set; }
    public Guid LegalEntityId { get; set; }
    public string LegalEntityName { get; set; } = string.Empty;
    public Guid HospitalSiteId { get; set; }
    public string HospitalSiteName { get; set; } = string.Empty;
    public int IdleThresholdDays { get; set; }
    public int DocumentExpiryWarningDays { get; set; }
    public int UtilizationMaxMonthsForAssetHead { get; set; }
    public DateOnly? CutoverDate { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

public sealed class AssetMasterFilterMetadataResponse
{
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string ResetButtonLabel { get; set; } = "Reset";
    public AssetMasterDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<AssetMasterCustomPeriodResponse> CustomPeriods { get; set; } = [];
    public List<AssetMasterSortOptionResponse> SortOptions { get; set; } = [];
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<AssetMasterOptionSourceResponse> FlagOptions { get; set; } = [];
    public List<AssetMasterQueryParameterResponse> QueryParameters { get; set; } = [];
    public List<AssetMasterFormFieldResponse> CreateFields { get; set; } = [];
    public List<AssetMasterFormFieldResponse> UpdateFields { get; set; } = [];
}

public sealed class AssetMasterDefaultFilterResponse
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ClassificationId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public bool? IsDepreciable { get; set; }
    public bool? IsMedical { get; set; }
    public bool? IsCapitalizable { get; set; }
    public bool? HasExpiry { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetMasterCustomPeriodResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool UsesStartDate { get; set; }
    public bool UsesEndDate { get; set; }
}

public sealed class AssetMasterSortOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class AssetMasterOptionSourceResponse
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<AssetMasterSortOptionResponse> Options { get; set; } = [];
}

public sealed class AssetMasterQueryParameterResponse
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Required { get; set; } = "No";
    public string Description { get; set; } = string.Empty;
    public string? Example { get; set; }
}

public sealed class AssetMasterFormFieldResponse
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public bool IsRequiredOnCreate { get; set; }
    public bool IsRequiredOnUpdate { get; set; }
    public string RequiredType { get; set; } = "Optional";
    public int? MaxLength { get; set; }
    public string? OptionsSource { get; set; }
    public string? Description { get; set; }
    public string? Example { get; set; }
    public int SortOrder { get; set; }
}
