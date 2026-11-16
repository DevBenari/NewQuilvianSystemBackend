using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

public sealed record AssetBookEstimate(int UsefulLifeMonths, decimal ResidualValue, int RemainingMonths, Guid? DepreciationPolicyId);

public sealed class AssetBookService
{
    private const string AssetDocumentType = "AstAsset";
    private const string GrantSourcePrefix = "GRANT:";
    private const string FirstSourceVersion = "1";

    private readonly ApplicationDbContext _context;
    private readonly AssetValuationParameterQueryService _parameterQuery;
    private readonly AssetAccountingEventService _accountingEvents;

    public AssetBookService(
        ApplicationDbContext context,
        AssetValuationParameterQueryService parameterQuery,
        AssetAccountingEventService accountingEvents)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _parameterQuery = parameterQuery ?? throw new ArgumentNullException(nameof(parameterQuery));
        _accountingEvents = accountingEvents ?? throw new ArgumentNullException(nameof(accountingEvents));
    }

    public static decimal GetNetBookValue(AstAssetBook book)
    {
        ArgumentNullException.ThrowIfNull(book);
        return book.CapitalizedCost - book.AccumulatedDepreciation - book.AccumulatedImpairment;
    }

    public static DateOnly GetDepreciationStartPeriod(DateOnly readyForUseDate) =>
        AssetValuationPeriod.Next(readyForUseDate);

    public async Task<AstAssetBook> CreateForRegisteredAssetAsync(
        AstAsset asset,
        Guid actorUserId,
        DateTime approvedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);

        var existing = await FindExistingAsync(asset.Id, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var approvedDate = FinanceBusinessDate.ToDateOnly(approvedAt);
        var book = await BuildBookAsync(asset, approvedDate, actorUserId, cancellationToken);

        AddEntry(book, ValueEntryType.Perolehan, AssetValuationPeriod.Of(approvedDate), book.CapitalizedCost, 0m, actorUserId, asset);

        if (asset.AcquisitionType == AssetAcquisitionType.HibahMasuk && asset.RegisteredAcquisitionCost > 0m)
        {
            var inventoryCode = await _context.MstAssetGroups
                .AsNoTracking()
                .Where(x => x.Id == asset.AssetGroupId)
                .Select(x => x.InventoryCode)
                .FirstAsync(cancellationToken);

            await _accountingEvents.QueueAsync(new AssetAccountingEventRequest(
                AssetAccountingEventCodes.GrantAcquisition,
                GrantSourcePrefix + asset.Id,
                FirstSourceVersion,
                approvedDate,
                asset.AcquisitionDate,
                asset.RegisteredAcquisitionCost,
                null,
                [new AssetAccountingEventComponent(inventoryCode, asset.RegisteredAcquisitionCost)],
                null,
                AssetDocumentType,
                asset.Id,
                actorUserId), cancellationToken);
        }

        return book;
    }

    public async Task<AstAssetBook> CreateOpeningBalanceAsync(
        AstAsset asset,
        decimal accumulatedDepreciation,
        DateOnly cutoverDate,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);

        var existing = await FindExistingAsync(asset.Id, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var book = await BuildBookAsync(asset, cutoverDate, actorUserId, cancellationToken);
        var cutoverPeriod = AssetValuationPeriod.Of(cutoverDate);

        if (book.IsCapitalized)
        {
            var accumulated = AssetValuationMoney.Round(accumulatedDepreciation);
            if (accumulated < 0m ||
                accumulated > book.CapitalizedCost - book.ResidualValue ||
                (!book.IsDepreciable && accumulated > 0m))
            {
                throw AssetManagementException.Invalid(AssetValuationErrorCodes.OpeningAccumulation, AssetValuationMessages.OpeningAccumulation);
            }

            if (book.IsDepreciable)
            {
                var elapsed = Math.Max(0, AssetValuationPeriod.MonthsBetween(book.DepreciationStartPeriod, cutoverPeriod));
                book.DepreciatedMonths = book.UsefulLifeMonths > 0 ? Math.Min(elapsed, book.UsefulLifeMonths) : elapsed;
                book.LastDepreciatedPeriod = book.DepreciatedMonths > 0 ? cutoverPeriod.AddMonths(-1) : null;

                if (book.UsefulLifeMonths > 0 &&
                    book.DepreciatedMonths >= book.UsefulLifeMonths &&
                    book.CapitalizedCost - accumulated > book.ResidualValue)
                {
                    throw AssetManagementException.Invalid(AssetValuationErrorCodes.OpeningBalanceExhausted, AssetValuationMessages.OpeningBalanceExhausted);
                }
            }

            book.AccumulatedDepreciation = accumulated;
        }

        AddEntry(book, ValueEntryType.SaldoAwal, cutoverPeriod, book.CapitalizedCost, book.AccumulatedDepreciation, actorUserId, asset);
        return book;
    }

    public async Task<AssetBookEstimate> ResolveEstimateAsync(AstAssetBook book, DateOnly period, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(book);

        if (book.HasEstimateOverride)
        {
            return ToEstimate(book.UsefulLifeMonths, book.ResidualValue, book.DepreciatedMonths, book.DepreciationPolicyId);
        }

        var groupId = await _context.AstAssets
            .AsNoTracking()
            .Where(x => x.Id == book.AssetId)
            .Select(x => x.AssetGroupId)
            .FirstAsync(cancellationToken);

        var policy = await _parameterQuery.GetPolicyAtAsync(groupId, period, cancellationToken);
        if (policy == null)
        {
            return ToEstimate(book.UsefulLifeMonths, book.ResidualValue, book.DepreciatedMonths, book.DepreciationPolicyId);
        }

        return ToEstimate(
            policy.UsefulLifeMonths,
            ResidualFromPercent(book.CapitalizedCost, policy.ResidualValuePercent),
            book.DepreciatedMonths,
            policy.Id);
    }

    private async Task<AstAssetBook> BuildBookAsync(AstAsset asset, DateOnly parameterDate, Guid actorUserId, CancellationToken cancellationToken)
    {
        var parameter = await _parameterQuery.GetRequiredParameterAtAsync(asset.LegalEntityId, parameterDate, cancellationToken);

        var classification = await _context.MstAssetGroups
            .AsNoTracking()
            .Where(x => x.Id == asset.AssetGroupId)
            .Select(x => new
            {
                x.Category!.IsDepreciable,
                IsCapitalizable = _context.MstAssetOwnershipTypes
                    .Where(o => o.Id == asset.OwnershipTypeId)
                    .Select(o => o.IsCapitalizable)
                    .FirstOrDefault()
            })
            .FirstAsync(cancellationToken);

        var cost = AssetValuationMoney.Round(asset.RegisteredAcquisitionCost);
        var isCapitalized = classification.IsCapitalizable && cost >= parameter.CapitalizationThreshold;
        var isDepreciable = isCapitalized && classification.IsDepreciable;
        var startPeriod = GetDepreciationStartPeriod(asset.ReadyForUseDate);

        var policy = isDepreciable
            ? await _parameterQuery.GetPolicyAtAsync(asset.AssetGroupId, startPeriod, cancellationToken)
            : null;

        var book = new AstAssetBook
        {
            AssetId = asset.Id,
            IsCapitalized = isCapitalized,
            IsDepreciable = isDepreciable,
            AcquisitionCost = cost,
            CapitalizedCost = isCapitalized ? cost : 0m,
            ResidualValue = policy != null ? ResidualFromPercent(cost, policy.ResidualValuePercent) : 0m,
            UsefulLifeMonths = policy?.UsefulLifeMonths ?? 0,
            DepreciationStartPeriod = startPeriod,
            DepreciationPolicyId = policy?.Id,
            BookStatus = AssetBookStatus.Aktif,
            CreateBy = actorUserId,
            CreateDateTime = DateTime.UtcNow
        };

        _context.AstAssetBooks.Add(book);
        return book;
    }

    private void AddEntry(
        AstAssetBook book,
        ValueEntryType entryType,
        DateOnly period,
        decimal costDelta,
        decimal accumulatedDepreciationDelta,
        Guid actorUserId,
        AstAsset asset)
    {
        var now = DateTime.UtcNow;
        _context.AstAssetValueEntries.Add(new AstAssetValueEntry
        {
            AssetId = book.AssetId,
            EntryType = entryType,
            Period = period,
            EntryDate = now,
            CostDelta = costDelta,
            AccumulatedDepreciationDelta = accumulatedDepreciationDelta,
            AccumulatedImpairmentDelta = 0m,
            SourceDocumentType = AssetDocumentType,
            SourceDocumentId = asset.Id,
            SourceDocumentNumber = asset.InventoryNumber,
            ActorUserId = actorUserId,
            CreateBy = actorUserId,
            CreateDateTime = now
        });
    }

    private async Task<AstAssetBook?> FindExistingAsync(Guid assetId, CancellationToken cancellationToken) =>
        _context.AstAssetBooks.Local.FirstOrDefault(x => x.AssetId == assetId)
        ?? await _context.AstAssetBooks.FirstOrDefaultAsync(x => x.AssetId == assetId && !x.IsDelete, cancellationToken);

    private static decimal ResidualFromPercent(decimal cost, decimal percent) =>
        AssetValuationMoney.Round(cost * percent / 100m);

    private static AssetBookEstimate ToEstimate(int usefulLifeMonths, decimal residualValue, int depreciatedMonths, Guid? policyId) =>
        new(usefulLifeMonths, residualValue, Math.Max(0, usefulLifeMonths - depreciatedMonths), policyId);
}
