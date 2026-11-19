using System.Text.RegularExpressions;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetNumberingService
{
    public const int SequenceDigits = 6;
    public const string InventorySequenceKeyPrefix = "AST_INV_";
    public const string InventoryNumberPrefix = "AST-";

    private readonly NumberSeriesAllocator _allocator;

    public AssetNumberingService(NumberSeriesAllocator allocator)
    {
        _allocator = allocator ?? throw new ArgumentNullException(nameof(allocator));
    }

    public Task<string> AllocateInventoryNumberAsync(
        string inventoryCode,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var code = NormalizeInventoryCode(inventoryCode);
        return AllocateAsync(
            InventorySequenceKeyPrefix + code,
            InventoryNumberPrefix + code,
            actorUserId,
            AssetRegisterMessages.NumberAllocationFailed,
            cancellationToken);
    }

    public Task<string> AllocateDocumentNumberAsync(
        AssetDocumentNumberSeries series,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(series);
        return AllocateAsync(
            series.SequenceKey,
            series.Prefix,
            actorUserId,
            AssetRegisterMessages.DocumentNumberAllocationFailed,
            cancellationToken);
    }

    public static string NormalizeInventoryCode(string inventoryCode)
    {
        var code = inventoryCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!InventoryCodePattern().IsMatch(code))
        {
            throw new ArgumentException("Kode inventaris wajib 2–11 huruf besar atau angka.", nameof(inventoryCode));
        }

        return code;
    }

    private async Task<string> AllocateAsync(
        string sequenceKey,
        string prefix,
        Guid actorUserId,
        string failureMessage,
        CancellationToken cancellationToken)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("Pengguna penerbit nomor wajib diisi.", nameof(actorUserId));
        }

        var request = new NumberAllocationRequest(
            SequenceKey: sequenceKey,
            Prefix: prefix,
            ResetPolicy: NumberSeriesResetPolicies.Never,
            SequenceDigits: SequenceDigits,
            ActorUserId: actorUserId,
            Instant: DateTimeOffset.UtcNow);

        try
        {
            return await _allocator.AllocateAsync(request, cancellationToken);
        }
        catch (NumberSeriesAllocationException ex)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.NumberAllocationFailed, failureMessage, ex);
        }
    }

    [GeneratedRegex("^[A-Z0-9]{2,11}$", RegexOptions.CultureInvariant)]
    private static partial Regex InventoryCodePattern();
}

public sealed class AssetDocumentNumberSeries
{
    public static readonly AssetDocumentNumberSeries Handover = new("AST_HANDOVER", "AST-PNY");
    public static readonly AssetDocumentNumberSeries ConditionReport = new("AST_CONDITION", "AST-KND");
    public static readonly AssetDocumentNumberSeries Utilization = new("AST_UTILIZATION", "AST-PMF");
    public static readonly AssetDocumentNumberSeries Import = new("AST_IMPORT", "AST-IMP");
    public static readonly AssetDocumentNumberSeries Transfer = new("AST_TRANSFER", "AST-MUT");

    private AssetDocumentNumberSeries(string sequenceKey, string prefix)
    {
        SequenceKey = sequenceKey;
        Prefix = prefix;
    }

    public string SequenceKey { get; }

    public string Prefix { get; }
}
