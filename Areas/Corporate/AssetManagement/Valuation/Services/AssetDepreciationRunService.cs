using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

public sealed partial class AssetDepreciationRunService
{
    public const string RunDocumentType = "AstDepreciationRun";
    private const string LogCategory = "Corporate.AssetManagement.Valuation";
    private const string SourceVersion = "1";
    private const int MaxListedNumbers = 5;

    private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

    private readonly ApplicationDbContext _context;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetSubledgerSetupService _subledgerService;
    private readonly AssetAccountingEventService _accountingEvents;
    private readonly LoggerService _loggerService;

    public AssetDepreciationRunService(
        ApplicationDbContext context,
        AssetEntityResolver entityResolver,
        AssetSubledgerSetupService subledgerService,
        AssetAccountingEventService accountingEvents,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _subledgerService = subledgerService ?? throw new ArgumentNullException(nameof(subledgerService));
        _accountingEvents = accountingEvents ?? throw new ArgumentNullException(nameof(accountingEvents));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<DepreciationPreviewResponse> PreviewAsync(DepreciationPreviewQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var sequence = await ReadSequenceAsync(entity.LegalEntityId, cancellationToken);
        var period = string.IsNullOrWhiteSpace(request.Period)
            ? sequence.NextAllowed ?? AssetValuationPeriod.Current()
            : ParsePeriod(request.Period);

        var response = new DepreciationPreviewResponse
        {
            Period = period,
            PeriodLabel = AssetValuationPeriod.Format(period),
            LegalEntityId = entity.LegalEntityId,
            LegalEntityName = entity.LegalEntityName,
            NextAllowedPeriod = sequence.NextAllowed,
            NextAllowedPeriodLabel = sequence.NextAllowed is { } next ? AssetValuationPeriod.Format(next) : null
        };

        var blocker = await CheckBlockerAsync(entity.LegalEntityId, period, sequence, cancellationToken);
        if (blocker != null)
        {
            response.Blocker = blocker.Issue;
            response.ProcessedRunId = blocker.ProcessedRunId;
            if (blocker.ProcessedRunId.HasValue)
            {
                return response;
            }
        }

        var plan = await BuildPlanAsync(entity.LegalEntityId, period, cancellationToken);
        FillTotals(response, plan);
        response.CanProcess = response.Blocker == null && plan.Errors.Count == 0;
        return response;
    }

    public async Task<DepreciationRunResponse> ProcessAsync(DepreciationRunCreateRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Period))
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.Field, AssetValuationMessages.RunPeriodFormat);
        }

        var period = ParsePeriod(request.Period);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var sequence = await ReadSequenceAsync(entity.LegalEntityId, cancellationToken);
        var blocker = await CheckBlockerAsync(entity.LegalEntityId, period, sequence, cancellationToken);
        if (blocker != null)
        {
            throw AssetManagementException.Conflict(blocker.Issue.Code, blocker.Issue.Message);
        }

        var plan = await BuildPlanAsync(entity.LegalEntityId, period, cancellationToken);
        if (plan.Errors.Count > 0)
        {
            throw AssetManagementException.Conflict(
                AssetValuationErrorCodes.RunPreviewErrors,
                string.Format(Indonesian, AssetValuationMessages.RunPreviewErrorsFormat, plan.Errors.Count));
        }

        var now = DateTime.UtcNow;
        var run = new AstDepreciationRun
        {
            LegalEntityId = entity.LegalEntityId,
            Period = period,
            AssetCount = plan.Lines.Select(x => x.AssetId).Distinct().Count(),
            TotalAmount = plan.Lines.Sum(x => x.Amount),
            AdjustmentAmount = plan.Lines.Where(x => x.Reason != DepreciationAdjustmentReason.Tidak).Sum(x => x.Amount),
            ProcessedBy = actorUserId,
            ProcessedAt = now,
            CreateBy = actorUserId,
            CreateDateTime = now
        };
        _context.AstDepreciationRuns.Add(run);

        var documentNumber = $"PENYUSUTAN {period:yyyy-MM}";
        foreach (var line in plan.Lines)
        {
            _context.AstDepreciationRunLines.Add(new AstDepreciationRunLine
            {
                RunId = run.Id,
                AssetId = line.AssetId,
                AssetGroupId = line.AssetGroupId,
                OrganizationUnitId = line.OrganizationUnitId,
                CostCenterId = line.CostCenterId,
                Amount = line.Amount,
                AdjustmentReason = line.Reason,
                AdjustmentPeriod = line.AdjustmentPeriod,
                CreateBy = actorUserId,
                CreateDateTime = now
            });
            _context.AstAssetValueEntries.Add(new AstAssetValueEntry
            {
                AssetId = line.AssetId,
                EntryType = line.Reason == DepreciationAdjustmentReason.Tidak ? ValueEntryType.Penyusutan : ValueEntryType.PenyesuaianPenyusutan,
                Period = period,
                EntryDate = now,
                CostDelta = 0m,
                AccumulatedDepreciationDelta = line.Amount,
                AccumulatedImpairmentDelta = 0m,
                CostCenterId = line.CostCenterId,
                SourceDocumentType = RunDocumentType,
                SourceDocumentId = run.Id,
                SourceDocumentNumber = documentNumber,
                Note = line.Note,
                ActorUserId = actorUserId,
                CreateBy = actorUserId,
                CreateDateTime = now
            });
        }

        var bookIds = plan.Books.Select(x => x.BookId).ToList();
        var books = await _context.AstAssetBooks.Where(x => bookIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var update in plan.Books)
        {
            var book = books[update.BookId];
            book.AccumulatedDepreciation = update.AccumulatedDepreciation;
            book.DepreciatedMonths = update.DepreciatedMonths;
            book.LastDepreciatedPeriod = period;
            if (!book.HasEstimateOverride)
            {
                book.UsefulLifeMonths = update.UsefulLifeMonths;
                book.ResidualValue = update.ResidualValue;
                book.DepreciationPolicyId = update.DepreciationPolicyId;
            }

            book.RowVersion = Guid.NewGuid();
            book.UpdateBy = actorUserId;
            book.UpdateDateTime = now;
        }

        var accountingDate = period.AddMonths(1).AddDays(-1);
        foreach (var total in plan.Totals)
        {
            await _accountingEvents.QueueAsync(new AssetAccountingEventRequest(
                AssetAccountingEventCodes.Depreciation,
                $"DEP:{entity.LegalEntityId:N}:{period:yyyyMM}:{total.InventoryCode}:{total.CostCenterId:N}",
                SourceVersion,
                accountingDate,
                accountingDate,
                total.Amount,
                total.CostCenterId,
                [new AssetAccountingEventComponent(total.InventoryCode, total.Amount)],
                null,
                RunDocumentType,
                run.Id,
                actorUserId), cancellationToken);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw AssetManagementException.Conflict(
                AssetValuationErrorCodes.RunAlreadyProcessed,
                string.Format(Indonesian, AssetValuationMessages.RunAlreadyProcessedFormat, AssetValuationPeriod.Format(period), "-", "pengguna lain"),
                exception);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw AssetManagementException.Conflict(AssetValuationErrorCodes.Concurrent, AssetValuationMessages.Concurrent, exception);
        }

        await _loggerService.AuditAsync(LogCategory, "AssetDepreciationRun.Process", "Penyusutan aset diproses.", new
        {
            run.Id,
            run.LegalEntityId,
            Period = period,
            run.AssetCount,
            run.TotalAmount,
            run.AdjustmentAmount,
            Lines = plan.Lines.Count,
            Events = plan.Totals.Count,
            ActorUserId = actorUserId
        });

        return await GetRunAsync(run.Id, cancellationToken);
    }

    public static DateOnly ParsePeriod(string value)
    {
        var text = (value ?? string.Empty).Trim();
        if (DateOnly.TryParseExact(text, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month) ||
            DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out month))
        {
            return AssetValuationPeriod.Of(month);
        }

        throw AssetManagementException.Invalid(AssetValuationErrorCodes.Field, AssetValuationMessages.RunPeriodFormat);
    }

    public static DateTime PeriodEndUtc(DateOnly period)
    {
        var nextMonth = AssetValuationPeriod.Next(period).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(nextMonth, FinanceBusinessDate.BusinessTimeZone);
    }

    private async Task<RunSequence> ReadSequenceAsync(Guid legalEntityId, CancellationToken cancellationToken)
    {
        var first = await _context.AstValuationParameters
            .AsNoTracking()
            .Where(x => x.LegalEntityId == legalEntityId && !x.IsDelete)
            .OrderBy(x => x.EffectiveFromPeriod)
            .Select(x => (DateOnly?)x.EffectiveFromPeriod)
            .FirstOrDefaultAsync(cancellationToken);
        var last = await _context.AstDepreciationRuns
            .AsNoTracking()
            .Where(x => x.LegalEntityId == legalEntityId && !x.IsDelete)
            .OrderByDescending(x => x.Period)
            .Select(x => (DateOnly?)x.Period)
            .FirstOrDefaultAsync(cancellationToken);
        var next = last is { } lastPeriod ? AssetValuationPeriod.Next(lastPeriod) : first;
        return new RunSequence(first, last, next);
    }

    private async Task<RunBlocker?> CheckBlockerAsync(Guid legalEntityId, DateOnly period, RunSequence sequence, CancellationToken cancellationToken)
    {
        var label = AssetValuationPeriod.Format(period);
        var existing = await _context.AstDepreciationRuns
            .AsNoTracking()
            .Where(x => x.LegalEntityId == legalEntityId && x.Period == period && !x.IsDelete)
            .Select(x => new { x.Id, x.ProcessedAt, x.ProcessedBy })
            .FirstOrDefaultAsync(cancellationToken);
        if (existing != null)
        {
            var name = await _context.Users.AsNoTracking().Where(x => x.Id == existing.ProcessedBy).Select(x => x.DisplayName).FirstOrDefaultAsync(cancellationToken) ?? "-";
            var at = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(existing.ProcessedAt, DateTimeKind.Utc), FinanceBusinessDate.BusinessTimeZone);
            return new RunBlocker(
                Issue(AssetValuationErrorCodes.RunAlreadyProcessed,
                    string.Format(Indonesian, AssetValuationMessages.RunAlreadyProcessedFormat, label, at.ToString("dd MMMM yyyy HH.mm", Indonesian), name)),
                existing.Id);
        }

        if (sequence.First is not { } first)
        {
            return new RunBlocker(Issue(AssetValuationErrorCodes.ParameterMissing, AssetValuationMessages.ParameterMissing), null);
        }

        if (period > AssetValuationPeriod.Current())
        {
            return new RunBlocker(Issue(AssetValuationErrorCodes.RunFuturePeriod, string.Format(Indonesian, AssetValuationMessages.RunFuturePeriodFormat, label)), null);
        }

        if (period < first)
        {
            return new RunBlocker(Issue(AssetValuationErrorCodes.RunSequence, string.Format(Indonesian, AssetValuationMessages.RunBeforeFirstFormat, AssetValuationPeriod.Format(first))), null);
        }

        if (sequence.NextAllowed is { } next && period > next)
        {
            return new RunBlocker(Issue(AssetValuationErrorCodes.RunSequence, string.Format(Indonesian, AssetValuationMessages.RunSequenceFormat, AssetValuationPeriod.Format(next))), null);
        }

        return null;
    }

    private async Task<RunPlan> BuildPlanAsync(Guid legalEntityId, DateOnly period, CancellationToken cancellationToken)
    {
        var cutoff = PeriodEndUtc(period);
        var books = await _context.AstAssetBooks
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.IsCapitalized && x.IsDepreciable &&
                        x.Asset!.LegalEntityId == legalEntityId && !x.Asset.IsDelete && x.Asset.RecordStatus == AssetRecordStatus.Terdaftar &&
                        x.DepreciationStartPeriod <= period &&
                        (x.LastDepreciatedPeriod == null || x.LastDepreciatedPeriod < period) &&
                        (x.BookStatus == AssetBookStatus.Aktif || (x.StoppedAt != null && x.StoppedAt >= cutoff)))
            .Select(x => new BookRow(
                x.Id,
                x.AssetId,
                x.Asset!.InventoryNumber,
                x.Asset.AssetGroupId,
                x.Asset.AssetGroup!.InventoryCode,
                x.CapitalizedCost,
                x.AccumulatedDepreciation,
                x.AccumulatedImpairment,
                x.ResidualValue,
                x.UsefulLifeMonths,
                x.DepreciatedMonths,
                x.DepreciationStartPeriod,
                x.LastDepreciatedPeriod,
                x.HasEstimateOverride,
                x.DepreciationPolicyId))
            .ToListAsync(cancellationToken);

        var plan = new RunPlan();
        if (books.Count == 0)
        {
            return plan;
        }

        var assetIds = books.Select(x => x.AssetId).ToList();
        var groupIds = books.Select(x => x.AssetGroupId).Distinct().ToList();
        var policies = await _context.AstDepreciationPolicies
            .AsNoTracking()
            .Where(x => groupIds.Contains(x.AssetGroupId) && !x.IsDelete && x.EffectiveFromPeriod <= period)
            .Select(x => new { x.Id, x.AssetGroupId, x.EffectiveFromPeriod, x.UsefulLifeMonths, x.ResidualValuePercent })
            .ToListAsync(cancellationToken);
        var corrections = await _context.AstAssetValueEntries
            .AsNoTracking()
            .Where(x => assetIds.Contains(x.AssetId) && !x.IsDelete && x.EntryType == ValueEntryType.KoreksiHargaPerolehan && x.Period == period)
            .Select(x => x.AssetId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var restorations = await _context.AstAssetValueEntries
            .AsNoTracking()
            .Where(x => assetIds.Contains(x.AssetId) && !x.IsDelete && x.EntryType == ValueEntryType.Pemulihan && x.Period <= period)
            .GroupBy(x => x.AssetId)
            .Select(x => new { AssetId = x.Key, Period = x.Max(e => e.Period) })
            .ToListAsync(cancellationToken);
        var placements = await _context.AstAssetPlacements
            .AsNoTracking()
            .Where(x => assetIds.Contains(x.AssetId) && !x.IsDelete && x.StartAt < cutoff && (x.EndAt == null || x.EndAt >= cutoff))
            .Select(x => new { x.AssetId, x.OrganizationUnitId, x.StartAt })
            .ToListAsync(cancellationToken);
        var unitByAsset = placements
            .GroupBy(x => x.AssetId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(p => p.StartAt).First().OrganizationUnitId);
        var costCenters = await _subledgerService.ResolveForUnitsAsync(unitByAsset.Values.Distinct().ToList(), cancellationToken);

        var unplaced = new List<string>();
        var unclearUnits = new Dictionary<Guid, int>();
        var groupsWithoutPolicy = new Dictionary<string, (Guid GroupId, int Count)>(StringComparer.Ordinal);

        foreach (var book in books)
        {
            var estimate = ResolveEstimate(book, policies.Where(x => x.AssetGroupId == book.AssetGroupId)
                .OrderByDescending(x => x.EffectiveFromPeriod)
                .Select(x => new PolicyRow(x.Id, x.UsefulLifeMonths, x.ResidualValuePercent))
                .FirstOrDefault());
            if (estimate.UsefulLifeMonths <= 0)
            {
                groupsWithoutPolicy[book.InventoryCode] = (book.AssetGroupId, groupsWithoutPolicy.GetValueOrDefault(book.InventoryCode).Count + 1);
                continue;
            }

            if (!unitByAsset.TryGetValue(book.AssetId, out var unitId))
            {
                unplaced.Add(book.InventoryNumber ?? book.AssetId.ToString());
                continue;
            }

            var costCenter = costCenters.GetValueOrDefault(unitId);
            if (costCenter is not { IsUnclear: false, CostCenterId: { } costCenterId })
            {
                unclearUnits[unitId] = unclearUnits.GetValueOrDefault(unitId) + 1;
                continue;
            }

            var state = new DepreciationBookState(
                book.CapitalizedCost,
                book.AccumulatedDepreciation,
                book.AccumulatedImpairment,
                estimate.ResidualValue,
                estimate.UsefulLifeMonths,
                book.DepreciatedMonths);

            void AddLine(decimal amount, DepreciationAdjustmentReason reason, DateOnly? adjustmentPeriod, string? note) =>
                plan.Lines.Add(new PlanLine(book.AssetId, book.AssetGroupId, book.InventoryCode, unitId, costCenterId, amount, reason, adjustmentPeriod, note));

            if (corrections.Contains(book.AssetId))
            {
                var adjustment = DepreciationCalculator.PriceCorrectionAdjustment(state);
                if (adjustment != 0m)
                {
                    AddLine(adjustment, DepreciationAdjustmentReason.KoreksiHarga, period, $"Penyesuaian koreksi harga perolehan untuk {state.DepreciatedMonths} bulan yang sudah disusutkan");
                    state = DepreciationCalculator.Apply(state, adjustment, 0);
                }
            }

            var firstMonth = book.LastDepreciatedPeriod is { } last && AssetValuationPeriod.Next(last) > book.DepreciationStartPeriod
                ? AssetValuationPeriod.Next(last)
                : book.DepreciationStartPeriod;
            var catchUpMonths = Math.Max(0, AssetValuationPeriod.MonthsBetween(firstMonth, period));
            if (catchUpMonths > 0)
            {
                var amounts = DepreciationCalculator.Schedule(state, catchUpMonths);
                var caughtUp = amounts.Sum();
                if (caughtUp > 0m)
                {
                    var restored = restorations.Any(x => x.AssetId == book.AssetId && (book.LastDepreciatedPeriod == null || x.Period > book.LastDepreciatedPeriod));
                    var lastCaught = firstMonth.AddMonths(amounts.Count - 1);
                    AddLine(caughtUp, restored ? DepreciationAdjustmentReason.Pemulihan : DepreciationAdjustmentReason.TerlambatDicatat, firstMonth,
                        $"Penyesuaian {AssetValuationPeriod.Format(firstMonth)} s.d. {AssetValuationPeriod.Format(lastCaught)} ({amounts.Count} bulan)");
                    state = DepreciationCalculator.Apply(state, caughtUp, amounts.Count);
                }
            }

            var monthly = DepreciationCalculator.MonthlyAmount(state);
            if (monthly > 0m)
            {
                AddLine(monthly, DepreciationAdjustmentReason.Tidak, null, null);
                state = DepreciationCalculator.Apply(state, monthly, 1);
            }

            plan.Books.Add(new BookUpdate(book.Id, state.AccumulatedDepreciation, state.DepreciatedMonths, estimate.UsefulLifeMonths, estimate.ResidualValue, estimate.PolicyId));
        }

        await DescribeIssuesAsync(plan, unplaced, unclearUnits, groupsWithoutPolicy, period, cancellationToken);
        await BuildTotalsAsync(plan, cancellationToken);
        return plan;
    }

    private static BookEstimate ResolveEstimate(BookRow book, PolicyRow? policy)
    {
        if (book.HasEstimateOverride || policy == null)
        {
            return new BookEstimate(book.UsefulLifeMonths, book.ResidualValue, book.DepreciationPolicyId);
        }

        return new BookEstimate(
            policy.UsefulLifeMonths,
            AssetValuationMoney.Round(book.CapitalizedCost * policy.ResidualValuePercent / 100m),
            policy.Id);
    }

    private async Task DescribeIssuesAsync(
        RunPlan plan,
        List<string> unplaced,
        Dictionary<Guid, int> unclearUnits,
        Dictionary<string, (Guid GroupId, int Count)> groupsWithoutPolicy,
        DateOnly period,
        CancellationToken cancellationToken)
    {
        if (unclearUnits.Count > 0)
        {
            var unitIds = unclearUnits.Keys.ToList();
            var names = await _context.MstOrganizationUnits.AsNoTracking().Where(x => unitIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.UnitName, cancellationToken);
            foreach (var (unitId, count) in unclearUnits)
            {
                var issue = Issue(AssetValuationErrorCodes.RunCostCenterUnclear, string.Format(Indonesian, AssetValuationMessages.RunCostCenterUnclearFormat, names.GetValueOrDefault(unitId) ?? "-"));
                issue.AssetCount = count;
                issue.OrganizationUnitId = unitId;
                plan.Errors.Add(issue);
            }
        }

        if (unplaced.Count > 0)
        {
            var listed = string.Join(", ", unplaced.Take(MaxListedNumbers)) + (unplaced.Count > MaxListedNumbers ? ", …" : string.Empty);
            var issue = Issue(AssetValuationErrorCodes.RunCostCenterUnclear,
                string.Format(Indonesian, AssetValuationMessages.RunUnplacedFormat, unplaced.Count, AssetValuationPeriod.Format(period), listed));
            issue.AssetCount = unplaced.Count;
            plan.Errors.Add(issue);
        }

        foreach (var (code, value) in groupsWithoutPolicy)
        {
            var issue = Issue(AssetValuationErrorCodes.RunGroupWithoutPolicy, string.Format(Indonesian, AssetValuationMessages.RunGroupWithoutPolicyFormat, code));
            issue.AssetCount = value.Count;
            issue.AssetGroupId = value.GroupId;
            plan.Errors.Add(issue);
        }

        var lineGroups = plan.Lines.Select(x => x.AssetGroupId).Distinct().ToList();
        if (lineGroups.Count == 0)
        {
            return;
        }

        var configured = await _context.AstSubledgerAccountSetups.AsNoTracking()
            .Where(x => lineGroups.Contains(x.AssetGroupId) && !x.IsDelete)
            .Select(x => x.AssetGroupId)
            .ToListAsync(cancellationToken);
        foreach (var group in plan.Lines.Where(x => !configured.Contains(x.AssetGroupId)).GroupBy(x => new { x.AssetGroupId, x.InventoryCode }))
        {
            var issue = Issue(AssetValuationErrorCodes.RunGroupWithoutAccount, string.Format(Indonesian, AssetValuationMessages.GroupWithoutAccountFormat, group.Key.InventoryCode));
            issue.AssetCount = group.Select(x => x.AssetId).Distinct().Count();
            issue.AssetGroupId = group.Key.AssetGroupId;
            plan.Warnings.Add(issue);
        }
    }

    private async Task BuildTotalsAsync(RunPlan plan, CancellationToken cancellationToken)
    {
        if (plan.Lines.Count == 0)
        {
            return;
        }

        var groupIds = plan.Lines.Select(x => x.AssetGroupId).Distinct().ToList();
        var costCenterIds = plan.Lines.Select(x => x.CostCenterId).Distinct().ToList();
        var groupNames = await _context.MstAssetGroups.AsNoTracking().Where(x => groupIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.GroupName, cancellationToken);
        var costCenters = await _context.MstCostCenters.AsNoTracking().Where(x => costCenterIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => new { x.CostCenterCode, x.CostCenterName }, cancellationToken);

        plan.Totals.AddRange(plan.Lines
            .GroupBy(x => new { x.AssetGroupId, x.InventoryCode, x.CostCenterId })
            .OrderBy(x => x.Key.InventoryCode)
            .ThenBy(x => costCenters.GetValueOrDefault(x.Key.CostCenterId)?.CostCenterCode)
            .Select(x => new DepreciationGroupTotalResponse
            {
                AssetGroupId = x.Key.AssetGroupId,
                InventoryCode = x.Key.InventoryCode,
                GroupName = groupNames.GetValueOrDefault(x.Key.AssetGroupId) ?? string.Empty,
                CostCenterId = x.Key.CostCenterId,
                CostCenterCode = costCenters.GetValueOrDefault(x.Key.CostCenterId)?.CostCenterCode,
                CostCenterName = costCenters.GetValueOrDefault(x.Key.CostCenterId)?.CostCenterName,
                AssetCount = x.Select(l => l.AssetId).Distinct().Count(),
                Amount = x.Sum(l => l.Amount),
                AdjustmentAmount = x.Where(l => l.Reason != DepreciationAdjustmentReason.Tidak).Sum(l => l.Amount)
            }));
    }

    private static void FillTotals(DepreciationPreviewResponse response, RunPlan plan)
    {
        response.AssetCount = plan.Lines.Select(x => x.AssetId).Distinct().Count();
        response.LineCount = plan.Lines.Count;
        response.TotalAmount = plan.Lines.Sum(x => x.Amount);
        response.AdjustmentAmount = plan.Lines.Where(x => x.Reason != DepreciationAdjustmentReason.Tidak).Sum(x => x.Amount);
        response.Groups = plan.Totals;
        response.Errors = plan.Errors;
        response.Warnings = plan.Warnings;
    }

    private static DepreciationIssueResponse Issue(string code, string message) => new() { Code = code, Message = message };

    private sealed record RunSequence(DateOnly? First, DateOnly? Last, DateOnly? NextAllowed);

    private sealed record RunBlocker(DepreciationIssueResponse Issue, Guid? ProcessedRunId);

    private sealed record BookRow(
        Guid Id,
        Guid AssetId,
        string? InventoryNumber,
        Guid AssetGroupId,
        string InventoryCode,
        decimal CapitalizedCost,
        decimal AccumulatedDepreciation,
        decimal AccumulatedImpairment,
        decimal ResidualValue,
        int UsefulLifeMonths,
        int DepreciatedMonths,
        DateOnly DepreciationStartPeriod,
        DateOnly? LastDepreciatedPeriod,
        bool HasEstimateOverride,
        Guid? DepreciationPolicyId);

    private sealed record PolicyRow(Guid Id, int UsefulLifeMonths, decimal ResidualValuePercent);

    private sealed record BookEstimate(int UsefulLifeMonths, decimal ResidualValue, Guid? PolicyId);

    private sealed record PlanLine(
        Guid AssetId,
        Guid AssetGroupId,
        string InventoryCode,
        Guid OrganizationUnitId,
        Guid CostCenterId,
        decimal Amount,
        DepreciationAdjustmentReason Reason,
        DateOnly? AdjustmentPeriod,
        string? Note);

    private sealed record BookUpdate(Guid BookId, decimal AccumulatedDepreciation, int DepreciatedMonths, int UsefulLifeMonths, decimal ResidualValue, Guid? DepreciationPolicyId);

    private sealed class RunPlan
    {
        public List<PlanLine> Lines { get; } = [];
        public List<BookUpdate> Books { get; } = [];
        public List<DepreciationGroupTotalResponse> Totals { get; } = [];
        public List<DepreciationIssueResponse> Errors { get; } = [];
        public List<DepreciationIssueResponse> Warnings { get; } = [];
    }
}
