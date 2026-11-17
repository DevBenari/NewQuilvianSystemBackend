using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetHistoryService
{
    private readonly ApplicationDbContext _context;

    public AssetHistoryService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public AstAssetHistory Append(AssetHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.AssetId == Guid.Empty)
        {
            throw new ArgumentException("AssetId wajib diisi.", nameof(entry));
        }

        if (entry.ActorUserId == Guid.Empty)
        {
            throw new ArgumentException("ActorUserId wajib diisi.", nameof(entry));
        }

        if (!Enum.IsDefined(entry.EventType))
        {
            throw new ArgumentOutOfRangeException(nameof(entry), entry.EventType, "Jenis kejadian linimasa tidak dikenal.");
        }

        if (!AssetSubModules.All.Contains(entry.SubModule))
        {
            throw new ArgumentOutOfRangeException(nameof(entry), entry.SubModule, "Sub-modul linimasa tidak dikenal.");
        }

        EnsureMaxLength(entry.FromValue, 250, nameof(entry.FromValue));
        EnsureMaxLength(entry.ToValue, 250, nameof(entry.ToValue));
        EnsureMaxLength(entry.SourceDocumentType, 40, nameof(entry.SourceDocumentType));
        EnsureMaxLength(entry.SourceDocumentNumber, 30, nameof(entry.SourceDocumentNumber));
        EnsureMaxLength(entry.Reason, 500, nameof(entry.Reason));

        var now = DateTime.UtcNow;
        var history = new AstAssetHistory
        {
            AssetId = entry.AssetId,
            EventType = entry.EventType,
            SubModule = entry.SubModule,
            FromValue = entry.FromValue,
            ToValue = entry.ToValue,
            SourceDocumentType = entry.SourceDocumentType,
            SourceDocumentId = entry.SourceDocumentId,
            SourceDocumentNumber = entry.SourceDocumentNumber,
            ActorUserId = entry.ActorUserId,
            Reason = entry.Reason,
            OccurredAt = entry.OccurredAt ?? now,
            CreateBy = entry.ActorUserId,
            CreateDateTime = now
        };

        _context.AstAssetHistories.Add(history);
        return history;
    }

    private static void EnsureMaxLength(string? value, int maxLength, string fieldName)
    {
        if (value != null && value.Length > maxLength)
        {
            throw new ArgumentException($"{fieldName} melebihi {maxLength} karakter.", fieldName);
        }
    }
}

public sealed record AssetHistoryEntry(
    Guid AssetId,
    AssetHistoryEventType EventType,
    string SubModule,
    Guid ActorUserId,
    string? FromValue = null,
    string? ToValue = null,
    string? SourceDocumentType = null,
    Guid? SourceDocumentId = null,
    string? SourceDocumentNumber = null,
    string? Reason = null,
    DateTime? OccurredAt = null);

public static class AssetSubModules
{
    public const string Register = "register";
    public const string Movement = "pergerakan";
    public const string PhysicalInventory = "inventarisasi";
    public const string Valuation = "nilai";
    public const string Disposal = "penghapusan";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>([Register, Movement, PhysicalInventory, Valuation, Disposal], StringComparer.Ordinal);
}
