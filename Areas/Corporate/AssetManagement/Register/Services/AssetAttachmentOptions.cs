namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetAttachmentOptions
{
    public const string SectionName = "AssetManagement:Attachment";

    public List<string> AllowedExtensions { get; set; } = [];

    public long? MaxFileSizeBytes { get; set; }

    public bool IsConfigured =>
        MaxFileSizeBytes is > 0 &&
        AllowedExtensions.Any(x => !string.IsNullOrWhiteSpace(x));
}
