using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

public sealed class AssetValuationParameterService
{
    private const string LogCategory = "Corporate.AssetManagement.Valuation";
    private const int MaxPageSize = 100;
    private const int MaxNoteLength = 500;
    private const int MinUsefulLifeMonths = 1;
    private const int MaxUsefulLifeMonths = 600;

    private readonly ApplicationDbContext _context;
    private readonly AssetEntityResolver _entityResolver;
    private readonly LoggerService _loggerService;

    public AssetValuationParameterService(
        ApplicationDbContext context,
        AssetEntityResolver entityResolver,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<PagedResult<ValuationParameterResponse>> GetParametersAsync(ValuationPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var (pageNumber, pageSize) = NormalizePaging(request);

        var query = _context.AstValuationParameters
            .AsNoTracking()
            .Where(x => x.LegalEntityId == entity.LegalEntityId && !x.IsDelete);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.EffectiveFromPeriod)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return ToPaged(items, total, pageNumber, pageSize);
    }

    public async Task<ValuationParameterCurrentResponse> GetCurrentParameterAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var currentPeriod = AssetValuationPeriod.Current();

        var rows = await _context.AstValuationParameters
            .AsNoTracking()
            .Where(x => x.LegalEntityId == entity.LegalEntityId && !x.IsDelete)
            .OrderBy(x => x.EffectiveFromPeriod)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return new ValuationParameterCurrentResponse
        {
            LegalEntityId = entity.LegalEntityId,
            LegalEntityName = entity.LegalEntityName,
            CurrentPeriod = currentPeriod,
            NextAllowedPeriod = rows.Count == 0 ? currentPeriod : AssetValuationPeriod.Next(currentPeriod),
            Current = rows.LastOrDefault(x => x.EffectiveFromPeriod <= currentPeriod),
            Upcoming = rows.FirstOrDefault(x => x.EffectiveFromPeriod > currentPeriod)
        };
    }

    public async Task<ValuationParameterResponse> SetParameterAsync(ValuationParameterSetRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        EnsureAmount(request.CapitalizationThreshold);
        EnsureAmount(request.DisposalDirectorThreshold);
        EnsureAmount(request.UtilizationDirectorThreshold);
        var note = OptionalText(request.Note, MaxNoteLength, "Dasar penetapan");

        var isFirstRow = !await _context.AstValuationParameters
            .AnyAsync(x => x.LegalEntityId == entity.LegalEntityId && !x.IsDelete, cancellationToken);
        var period = ResolveEffectivePeriod(request.EffectiveFromPeriod, isFirstRow, AssetValuationMessages.EffectivePeriodFormat);

        if (await _context.AstValuationParameters.AnyAsync(
                x => x.LegalEntityId == entity.LegalEntityId && !x.IsDelete && x.EffectiveFromPeriod == period,
                cancellationToken))
        {
            throw DuplicatePeriod(AssetValuationMessages.DuplicateParameterFormat, period);
        }

        var now = DateTime.UtcNow;
        var row = new AstValuationParameter
        {
            LegalEntityId = entity.LegalEntityId,
            EffectiveFromPeriod = period,
            CapitalizationThreshold = request.CapitalizationThreshold,
            DisposalDirectorThreshold = request.DisposalDirectorThreshold,
            UtilizationDirectorThreshold = request.UtilizationDirectorThreshold,
            Note = note,
            SetBy = actorUserId,
            SetAt = now,
            CreateBy = actorUserId,
            CreateDateTime = now
        };

        _context.AstValuationParameters.Add(row);
        await SaveUniqueAsync(AssetValuationMessages.DuplicateParameterFormat, period, cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "AssetValuationParameter.Set", "Parameter nilai aset ditetapkan.", new
        {
            row.Id,
            row.LegalEntityId,
            row.EffectiveFromPeriod,
            IsFirstRow = isFirstRow,
            ActorUserId = actorUserId
        });

        return ToResponse(row);
    }

    public async Task<PagedResult<DepreciationPolicyGroupResponse>> GetPoliciesAsync(ValuationPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (pageNumber, pageSize) = NormalizePaging(request);
        var currentPeriod = AssetValuationPeriod.Current();

        var groups = _context.MstAssetGroups
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.Category!.IsDepreciable);

        if (request.AssetGroupId is { } assetGroupId && assetGroupId != Guid.Empty)
        {
            groups = groups.Where(x => x.Id == assetGroupId);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            groups = groups.Where(x => x.InventoryCode.ToUpper().Contains(keyword) || x.GroupName.ToUpper().Contains(keyword));
        }

        var total = await groups.CountAsync(cancellationToken);
        var rows = await groups
            .OrderBy(x => x.InventoryCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DepreciationPolicyGroupResponse
            {
                AssetGroupId = x.Id,
                InventoryCode = x.InventoryCode,
                GroupName = x.GroupName,
                CategoryName = x.Category!.CategoryName,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var groupIds = rows.Select(x => x.AssetGroupId).ToList();
        var policies = await _context.AstDepreciationPolicies
            .AsNoTracking()
            .Where(x => groupIds.Contains(x.AssetGroupId) && !x.IsDelete)
            .OrderBy(x => x.EffectiveFromPeriod)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            var groupPolicies = policies.Where(x => x.AssetGroupId == row.AssetGroupId).ToList();
            row.Current = groupPolicies.LastOrDefault(x => x.EffectiveFromPeriod <= currentPeriod);
            row.Upcoming = groupPolicies.FirstOrDefault(x => x.EffectiveFromPeriod > currentPeriod);
        }

        return ToPaged(rows, total, pageNumber, pageSize);
    }

    public async Task<List<DepreciationPolicyResponse>> GetPolicyHistoryAsync(Guid assetGroupId, CancellationToken cancellationToken = default)
    {
        await EnsureGroupExistsAsync(assetGroupId, cancellationToken);

        return await _context.AstDepreciationPolicies
            .AsNoTracking()
            .Where(x => x.AssetGroupId == assetGroupId && !x.IsDelete)
            .OrderByDescending(x => x.EffectiveFromPeriod)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<DepreciationPolicyResponse> SetPolicyAsync(DepreciationPolicySetRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.AssetGroupId is not { } assetGroupId || assetGroupId == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.Field, AssetValuationMessages.GroupRequired);
        }

        if (request.UsefulLifeMonths is < MinUsefulLifeMonths or > MaxUsefulLifeMonths ||
            request.ResidualValuePercent is < 0m or > 100m ||
            !AssetValuationMoney.HasAtMostTwoDecimals(request.ResidualValuePercent))
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.PolicyRange, AssetValuationMessages.PolicyRange);
        }

        var group = await _context.MstAssetGroups
            .AsNoTracking()
            .Where(x => x.Id == assetGroupId && !x.IsDelete)
            .Select(x => new { x.Id, x.InventoryCode, x.Category!.IsDepreciable })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw AssetManagementException.NotFound(AssetValuationErrorCodes.NotFound, AssetValuationMessages.GroupNotFound);

        if (!group.IsDepreciable)
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.GroupNotDepreciable, AssetValuationMessages.GroupNotDepreciable);
        }

        var isFirstRow = !await _context.AstDepreciationPolicies
            .AnyAsync(x => x.AssetGroupId == group.Id && !x.IsDelete, cancellationToken);
        var period = ResolveEffectivePeriod(request.EffectiveFromPeriod, isFirstRow, AssetValuationMessages.PolicyEffectivePeriodFormat);

        if (await _context.AstDepreciationPolicies.AnyAsync(
                x => x.AssetGroupId == group.Id && !x.IsDelete && x.EffectiveFromPeriod == period,
                cancellationToken))
        {
            throw DuplicatePeriod(AssetValuationMessages.DuplicatePolicyFormat, period);
        }

        var now = DateTime.UtcNow;
        var row = new AstDepreciationPolicy
        {
            AssetGroupId = group.Id,
            EffectiveFromPeriod = period,
            UsefulLifeMonths = request.UsefulLifeMonths,
            ResidualValuePercent = request.ResidualValuePercent,
            SetBy = actorUserId,
            SetAt = now,
            CreateBy = actorUserId,
            CreateDateTime = now
        };

        _context.AstDepreciationPolicies.Add(row);
        await SaveUniqueAsync(AssetValuationMessages.DuplicatePolicyFormat, period, cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "AssetDepreciationPolicy.Set", "Kebijakan penyusutan kelompok ditetapkan.", new
        {
            row.Id,
            row.AssetGroupId,
            group.InventoryCode,
            row.EffectiveFromPeriod,
            row.UsefulLifeMonths,
            row.ResidualValuePercent,
            IsFirstRow = isFirstRow,
            ActorUserId = actorUserId
        });

        return ToResponse(row);
    }

    private static DateOnly ResolveEffectivePeriod(DateOnly? requested, bool isFirstRow, string messageFormat)
    {
        var current = AssetValuationPeriod.Current();
        var next = AssetValuationPeriod.Next(current);

        if (requested is not { } value)
        {
            return isFirstRow ? current : next;
        }

        var allowed = AssetValuationPeriod.IsFirstOfMonth(value) && (value == next || (isFirstRow && value == current));
        if (!allowed)
        {
            throw AssetManagementException.Invalid(
                AssetValuationErrorCodes.EffectivePeriod,
                string.Format(messageFormat, AssetValuationPeriod.Format(isFirstRow ? current : next)));
        }

        return value;
    }

    private async Task SaveUniqueAsync(string messageFormat, DateOnly period, CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw DuplicatePeriod(messageFormat, period, exception);
        }
    }

    private async Task EnsureGroupExistsAsync(Guid assetGroupId, CancellationToken cancellationToken)
    {
        if (!await _context.MstAssetGroups.AnyAsync(x => x.Id == assetGroupId && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.NotFound(AssetValuationErrorCodes.NotFound, AssetValuationMessages.GroupNotFound);
        }
    }

    private static void EnsureAmount(decimal value)
    {
        if (value < 0m || !AssetValuationMoney.HasAtMostTwoDecimals(value))
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.ParameterAmount, AssetValuationMessages.ParameterAmount);
        }
    }

    private static string? OptionalText(string? value, int maxLength, string label)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (text.Length > maxLength)
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.Field, $"{label} paling panjang {maxLength} karakter.");
        }

        return text;
    }

    private static AssetManagementException DuplicatePeriod(string messageFormat, DateOnly period, Exception? innerException = null) =>
        AssetManagementException.Conflict(
            AssetValuationErrorCodes.DuplicatePeriod,
            string.Format(messageFormat, AssetValuationPeriod.Format(period)),
            innerException);

    private static (int PageNumber, int PageSize) NormalizePaging(ValuationPagedQuery request) =>
        (request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize));

    private static PagedResult<T> ToPaged<T>(List<T> items, int total, int pageNumber, int pageSize) => new()
    {
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalData = total,
        TotalPage = (int)Math.Ceiling(total / (double)pageSize),
        Items = items
    };

    private static ValuationParameterResponse ToResponse(AstValuationParameter x) => new()
    {
        Id = x.Id,
        LegalEntityId = x.LegalEntityId,
        EffectiveFromPeriod = x.EffectiveFromPeriod,
        CapitalizationThreshold = x.CapitalizationThreshold,
        DisposalDirectorThreshold = x.DisposalDirectorThreshold,
        UtilizationDirectorThreshold = x.UtilizationDirectorThreshold,
        Note = x.Note,
        SetBy = x.SetBy,
        SetAt = x.SetAt
    };

    private static DepreciationPolicyResponse ToResponse(AstDepreciationPolicy x) => new()
    {
        Id = x.Id,
        AssetGroupId = x.AssetGroupId,
        EffectiveFromPeriod = x.EffectiveFromPeriod,
        UsefulLifeMonths = x.UsefulLifeMonths,
        ResidualValuePercent = x.ResidualValuePercent,
        SetBy = x.SetBy,
        SetAt = x.SetAt
    };
}
