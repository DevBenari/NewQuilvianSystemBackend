using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

public static class AssetAccountingEventCodes
{
    public const string Depreciation = "AST-PENYUSUTAN";
    public const string Impairment = "AST-PENURUNAN-NILAI";
    public const string GrantAcquisition = "AST-PEROLEHAN-HIBAH";
    public const string Disposal = "AST-PENGHAPUSAN";
    public const string Restoration = "AST-PEMULIHAN";
    public const string SubledgerBalance = "AST-SALDO-SUBLEDGER";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        [Depreciation, Impairment, GrantAcquisition, Disposal, Restoration, SubledgerBalance],
        StringComparer.Ordinal);
}

public sealed class AssetAccountingDispatchOptions
{
    public const string SectionName = "AssetManagement:AccountingDispatch";

    public List<string> EnabledEventCodes { get; set; } = [];
}

public sealed record AssetAccountingEventComponent(string ComponentCode, decimal Amount);

public sealed record AssetAccountingEventRequest(
    string EventTypeCode,
    string SourceTransactionId,
    string SourceVersion,
    DateOnly AccountingDate,
    DateOnly DocumentDate,
    decimal Amount,
    Guid? CostCenterId,
    IReadOnlyList<AssetAccountingEventComponent>? Components,
    string? SubledgerJson,
    string SourceDocumentType,
    Guid SourceDocumentId,
    Guid ActorUserId);

public sealed class AssetAccountingEventService
{
    private readonly ApplicationDbContext _context;
    private readonly AssetAccountingDispatchOptions _options;

    public AssetAccountingEventService(ApplicationDbContext context, IOptions<AssetAccountingDispatchOptions> options)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public AccountingOutboxStatus ResolveInitialStatus(string eventTypeCode) =>
        _options.EnabledEventCodes.Any(code => string.Equals(code?.Trim(), eventTypeCode, StringComparison.Ordinal))
            ? AccountingOutboxStatus.Pending
            : AccountingOutboxStatus.Held;

    public async Task<AstAccountingOutbox> QueueAsync(AssetAccountingEventRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!AssetAccountingEventCodes.All.Contains(request.EventTypeCode))
        {
            throw AssetManagementException.Invalid(
                AssetValuationErrorCodes.EventCode,
                string.Format(AssetValuationMessages.EventCodeFormat, request.EventTypeCode));
        }

        var existing = _context.AstAccountingOutboxes.Local.FirstOrDefault(x =>
                           x.SourceTransactionId == request.SourceTransactionId && x.SourceVersion == request.SourceVersion)
                       ?? await _context.AstAccountingOutboxes.FirstOrDefaultAsync(
                           x => x.SourceTransactionId == request.SourceTransactionId && x.SourceVersion == request.SourceVersion,
                           cancellationToken);

        if (existing != null)
        {
            return existing;
        }

        var row = new AstAccountingOutbox
        {
            EventTypeCode = request.EventTypeCode,
            SourceTransactionId = request.SourceTransactionId,
            SourceVersion = request.SourceVersion,
            AccountingDate = request.AccountingDate,
            DocumentDate = request.DocumentDate,
            Amount = AssetValuationMoney.Round(request.Amount),
            CostCenterId = request.CostCenterId,
            ComponentsJson = request.Components is { Count: > 0 }
                ? JsonSerializer.Serialize(request.Components.Select(x => new
                {
                    x.ComponentCode,
                    Amount = AssetValuationMoney.Round(x.Amount)
                }))
                : null,
            SubledgerJson = request.SubledgerJson,
            SourceDocumentType = request.SourceDocumentType,
            SourceDocumentId = request.SourceDocumentId,
            Status = ResolveInitialStatus(request.EventTypeCode),
            CreateBy = request.ActorUserId,
            CreateDateTime = DateTime.UtcNow
        };

        _context.AstAccountingOutboxes.Add(row);
        return row;
    }
}
