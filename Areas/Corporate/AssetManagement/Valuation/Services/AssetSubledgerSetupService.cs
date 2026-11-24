using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

public sealed class AssetSubledgerSetupService
{
    public const string SourceMapping = "Pemetaan";
    public const string SourceAutomatic = "Otomatis";
    public const string SourceUnclear = "BelumJelas";
    private const string LogCategory = "Corporate.AssetManagement.Valuation";
    private const int MaxAccountCodeLength = 50;
    private const int MaxSearchLength = 100;
    private const int MaxCostCenterOptions = 500;

    private readonly ApplicationDbContext _context;
    private readonly AssetEntityResolver _entityResolver;
    private readonly LoggerService _loggerService;

    public AssetSubledgerSetupService(
        ApplicationDbContext context,
        AssetEntityResolver entityResolver,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public Task<List<SubledgerAccountSetupResponse>> GetAccountSetupsAsync(CancellationToken cancellationToken = default) =>
        BuildAccountSetupsAsync(null, cancellationToken);

    public async Task<SubledgerAccountSetupResponse> SetAccountSetupAsync(Guid assetGroupId, SubledgerAccountSetupRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var costCode = AccountCode(request.CostControlAccountCode);
        var depreciationCode = AccountCode(request.AccumulatedDepreciationControlAccountCode);
        var impairmentCode = AccountCode(request.AccumulatedImpairmentControlAccountCode);
        if (costCode == null || depreciationCode == null)
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.Field, AssetValuationMessages.AccountRequired);
        }

        var group = await _context.MstAssetGroups
            .AsNoTracking()
            .Where(x => x.Id == assetGroupId && !x.IsDelete)
            .Select(x => new { x.Id, x.InventoryCode })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw AssetManagementException.NotFound(AssetValuationErrorCodes.NotFound, AssetValuationMessages.GroupNotFound);

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var codes = new[] { costCode, depreciationCode, impairmentCode }.OfType<string>().Distinct(StringComparer.Ordinal).ToList();
        var accounts = await LoadAccountsAsync(entity.LegalEntityId, codes, cancellationToken);
        var rejected = codes.Where(code => !accounts.TryGetValue(code, out var account) || !account.IsActive || !account.IsPostable).ToList();
        if (rejected.Count > 0)
        {
            throw AssetManagementException.Invalid(
                AssetValuationErrorCodes.AccountCode,
                string.Format(AssetValuationMessages.AccountCodeFormat, string.Join(", ", rejected)));
        }

        var now = DateTime.UtcNow;
        var row = await _context.AstSubledgerAccountSetups.FirstOrDefaultAsync(x => x.AssetGroupId == group.Id, cancellationToken);
        var isNew = row == null;
        if (row == null)
        {
            row = new AstSubledgerAccountSetup { AssetGroupId = group.Id, CreateBy = actorUserId, CreateDateTime = now };
            _context.AstSubledgerAccountSetups.Add(row);
        }
        else
        {
            row.IsDelete = false;
            row.UpdateBy = actorUserId;
            row.UpdateDateTime = now;
        }

        row.CostControlAccountCode = costCode;
        row.AccumulatedDepreciationControlAccountCode = depreciationCode;
        row.AccumulatedImpairmentControlAccountCode = impairmentCode;
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "AssetSubledgerSetup.SetAccounts", "Akun kontrol subledger kelompok aset ditetapkan.", new
        {
            row.Id,
            row.AssetGroupId,
            group.InventoryCode,
            row.CostControlAccountCode,
            row.AccumulatedDepreciationControlAccountCode,
            row.AccumulatedImpairmentControlAccountCode,
            IsNew = isNew,
            ActorUserId = actorUserId
        });

        return (await BuildAccountSetupsAsync(group.Id, cancellationToken))[0];
    }

    public async Task<List<UnitCostCenterResponse>> GetUnitCostCentersAsync(UnitCostCenterQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var units = _context.MstOrganizationUnits
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.LegalEntityId == entity.LegalEntityId);

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        if (keyword != null)
        {
            keyword = keyword[..Math.Min(keyword.Length, MaxSearchLength)].ToUpper();
            units = units.Where(x => x.UnitCode.ToUpper().Contains(keyword) || x.UnitName.ToUpper().Contains(keyword));
        }

        var rows = await units
            .OrderBy(x => x.UnitName)
            .Select(x => new { x.Id, x.UnitCode, x.UnitName })
            .ToListAsync(cancellationToken);
        var resolved = await ResolveForUnitsAsync(rows.Select(x => x.Id).ToList(), cancellationToken);

        var result = new List<UnitCostCenterResponse>(rows.Count);
        foreach (var unit in rows)
        {
            var item = resolved[unit.Id];
            item.UnitCode = unit.UnitCode;
            item.UnitName = unit.UnitName;
            if (request.UnclearOnly != true || item.IsUnclear)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public async Task<IReadOnlyDictionary<Guid, UnitCostCenterResponse>> ResolveForUnitsAsync(IReadOnlyCollection<Guid> organizationUnitIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(organizationUnitIds);
        var ids = organizationUnitIds.Where(x => x != Guid.Empty).Distinct().ToList();
        var result = new Dictionary<Guid, UnitCostCenterResponse>();
        if (ids.Count == 0)
        {
            return result;
        }

        var mappings = await _context.AstUnitCostCenters
            .AsNoTracking()
            .Where(x => ids.Contains(x.OrganizationUnitId) && !x.IsDelete)
            .Select(x => new
            {
                x.OrganizationUnitId,
                x.CostCenterId,
                x.CostCenter!.CostCenterCode,
                x.CostCenter.CostCenterName,
                IsUsable = x.CostCenter.IsActive && !x.CostCenter.IsDelete
            })
            .ToListAsync(cancellationToken);
        var active = await _context.MstCostCenters
            .AsNoTracking()
            .Where(x => x.OrganizationUnitId.HasValue && ids.Contains(x.OrganizationUnitId.Value) && x.IsActive && !x.IsDelete)
            .OrderBy(x => x.CostCenterCode)
            .Select(x => new { UnitId = x.OrganizationUnitId!.Value, x.Id, x.CostCenterCode, x.CostCenterName })
            .ToListAsync(cancellationToken);

        foreach (var unitId in ids)
        {
            var own = active.Where(x => x.UnitId == unitId).ToList();
            var item = new UnitCostCenterResponse { OrganizationUnitId = unitId, ActiveCostCenterCount = own.Count };
            var mapping = mappings.FirstOrDefault(x => x.OrganizationUnitId == unitId);
            if (mapping != null)
            {
                item.IsMapped = true;
                item.Source = SourceMapping;
                item.CostCenterId = mapping.CostCenterId;
                item.CostCenterCode = mapping.CostCenterCode;
                item.CostCenterName = mapping.CostCenterName;
                if (!mapping.IsUsable)
                {
                    item.IsUnclear = true;
                    item.UnclearReason = string.Format(AssetValuationMessages.MappedCostCenterInactiveFormat, mapping.CostCenterCode);
                }
            }
            else if (own.Count == 1)
            {
                item.Source = SourceAutomatic;
                item.CostCenterId = own[0].Id;
                item.CostCenterCode = own[0].CostCenterCode;
                item.CostCenterName = own[0].CostCenterName;
            }
            else
            {
                item.Source = SourceUnclear;
                item.IsUnclear = true;
                item.UnclearReason = own.Count == 0
                    ? AssetValuationMessages.NoActiveCostCenter
                    : string.Format(AssetValuationMessages.ManyActiveCostCentersFormat, own.Count);
            }

            item.SourceName = item.Source switch
            {
                SourceMapping => "Pemetaan",
                SourceAutomatic => "Otomatis",
                _ => "Belum Jelas"
            };
            result[unitId] = item;
        }

        return result;
    }

    public async Task<List<CostCenterOptionResponse>> GetCostCenterOptionsAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        return await _context.MstCostCenters
            .AsNoTracking()
            .Where(x => x.LegalEntityId == entity.LegalEntityId && x.IsActive && !x.IsDelete)
            .OrderBy(x => x.CostCenterCode)
            .Take(MaxCostCenterOptions)
            .Select(x => new CostCenterOptionResponse
            {
                Id = x.Id,
                Code = x.CostCenterCode,
                Name = x.CostCenterName,
                OrganizationUnitId = x.OrganizationUnitId,
                UnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UnitCostCenterResponse> SetUnitCostCenterAsync(Guid organizationUnitId, UnitCostCenterRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.CostCenterId is not { } costCenterId || costCenterId == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.Field, AssetValuationMessages.CostCenterRequired);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var unit = await FindUnitAsync(organizationUnitId, entity.LegalEntityId, cancellationToken);
        var costCenter = await _context.MstCostCenters
            .AsNoTracking()
            .Where(x => x.Id == costCenterId && !x.IsDelete && x.LegalEntityId == entity.LegalEntityId)
            .Select(x => new { x.Id, x.CostCenterCode, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw AssetManagementException.NotFound(AssetValuationErrorCodes.NotFound, AssetValuationMessages.CostCenterNotFound);
        if (!costCenter.IsActive)
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.CostCenterInactive, AssetValuationMessages.CostCenterInactive);
        }

        var now = DateTime.UtcNow;
        var row = await _context.AstUnitCostCenters.FirstOrDefaultAsync(x => x.OrganizationUnitId == unit.Id, cancellationToken);
        var previousCostCenterId = row is { IsDelete: false } ? row.CostCenterId : (Guid?)null;
        if (row == null)
        {
            row = new AstUnitCostCenter { OrganizationUnitId = unit.Id, CreateBy = actorUserId, CreateDateTime = now };
            _context.AstUnitCostCenters.Add(row);
        }
        else
        {
            row.IsDelete = false;
            row.DeleteBy = Guid.Empty;
            row.DeleteDateTime = null;
            row.UpdateBy = actorUserId;
            row.UpdateDateTime = now;
        }

        row.CostCenterId = costCenter.Id;
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "AssetUnitCostCenter.Set", "Pemetaan unit ke pusat biaya ditetapkan.", new
        {
            row.Id,
            row.OrganizationUnitId,
            unit.UnitName,
            row.CostCenterId,
            costCenter.CostCenterCode,
            PreviousCostCenterId = previousCostCenterId,
            ActorUserId = actorUserId
        });

        return await DescribeUnitAsync(unit.Id, unit.UnitCode, unit.UnitName, cancellationToken);
    }

    public async Task<bool> DeleteUnitCostCenterAsync(Guid organizationUnitId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var unit = await FindUnitAsync(organizationUnitId, entity.LegalEntityId, cancellationToken);
        var row = await _context.AstUnitCostCenters.FirstOrDefaultAsync(x => x.OrganizationUnitId == unit.Id && !x.IsDelete, cancellationToken)
            ?? throw AssetManagementException.NotFound(AssetValuationErrorCodes.NotFound, AssetValuationMessages.MappingNotFound);

        var now = DateTime.UtcNow;
        row.IsDelete = true;
        row.DeleteBy = actorUserId;
        row.DeleteDateTime = now;
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "AssetUnitCostCenter.Delete", "Pemetaan unit ke pusat biaya dihapus; unit kembali memakai pusat biaya otomatis.", new
        {
            row.Id,
            row.OrganizationUnitId,
            unit.UnitName,
            row.CostCenterId,
            ActorUserId = actorUserId
        });

        return true;
    }

    private async Task<List<SubledgerAccountSetupResponse>> BuildAccountSetupsAsync(Guid? onlyGroupId, CancellationToken cancellationToken)
    {
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var groups = _context.MstAssetGroups.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyGroupId is { } groupId)
        {
            groups = groups.Where(x => x.Id == groupId);
        }

        var rows = await groups
            .OrderBy(x => x.InventoryCode)
            .Select(x => new SubledgerAccountSetupResponse
            {
                AssetGroupId = x.Id,
                InventoryCode = x.InventoryCode,
                GroupName = x.GroupName,
                CategoryName = x.Category!.CategoryName,
                IsDepreciable = x.Category!.IsDepreciable,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var groupIds = rows.Select(x => x.AssetGroupId).ToList();
        var setups = await _context.AstSubledgerAccountSetups
            .AsNoTracking()
            .Where(x => groupIds.Contains(x.AssetGroupId) && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var codes = setups
            .SelectMany(x => new[] { x.CostControlAccountCode, x.AccumulatedDepreciationControlAccountCode, x.AccumulatedImpairmentControlAccountCode })
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var accounts = await LoadAccountsAsync(entity.LegalEntityId, codes, cancellationToken);

        foreach (var row in rows)
        {
            var setup = setups.FirstOrDefault(x => x.AssetGroupId == row.AssetGroupId);
            if (setup == null)
            {
                if (row.IsActive)
                {
                    row.Warnings.Add(string.Format(AssetValuationMessages.GroupWithoutAccountFormat, row.InventoryCode));
                }

                continue;
            }

            row.IsConfigured = true;
            row.CostControlAccount = Describe(setup.CostControlAccountCode, accounts, row.Warnings);
            row.AccumulatedDepreciationControlAccount = Describe(setup.AccumulatedDepreciationControlAccountCode, accounts, row.Warnings);
            row.AccumulatedImpairmentControlAccount = setup.AccumulatedImpairmentControlAccountCode is { } impairment
                ? Describe(impairment, accounts, row.Warnings)
                : null;
            row.UpdatedBy = setup.UpdateDateTime.HasValue ? setup.UpdateBy : setup.CreateBy;
            row.UpdatedAt = setup.UpdateDateTime ?? setup.CreateDateTime;
        }

        return rows;
    }

    private static SubledgerAccountRefResponse Describe(string code, IReadOnlyDictionary<string, AccountInfo> accounts, List<string> warnings)
    {
        var known = accounts.TryGetValue(code, out var account) && account.IsActive && account.IsPostable;
        if (!known)
        {
            warnings.Add(string.Format(AssetValuationMessages.AccountUnknownFormat, code));
        }
        else if (!account!.IsControlAccount)
        {
            warnings.Add(string.Format(AssetValuationMessages.AccountNotControlFormat, code));
        }

        return new SubledgerAccountRefResponse
        {
            Code = code,
            Name = account?.Name,
            IsKnown = known,
            IsControlAccount = account?.IsControlAccount ?? false
        };
    }

    private async Task<IReadOnlyDictionary<string, AccountInfo>> LoadAccountsAsync(Guid legalEntityId, IReadOnlyCollection<string> codes, CancellationToken cancellationToken)
    {
        if (codes.Count == 0)
        {
            return new Dictionary<string, AccountInfo>(StringComparer.Ordinal);
        }

        var rows = await _context.AccChartOfAccounts
            .AsNoTracking()
            .Where(x => x.LegalEntityId == legalEntityId && !x.IsDelete && codes.Contains(x.AccountCode))
            .Select(x => new AccountInfo(x.AccountCode, x.AccountName, x.IsActive, x.IsPostable, x.IsControlAccount))
            .ToListAsync(cancellationToken);
        return rows
            .GroupBy(x => x.Code, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(a => a.IsActive).First(), StringComparer.Ordinal);
    }

    private async Task<UnitCostCenterResponse> DescribeUnitAsync(Guid unitId, string unitCode, string unitName, CancellationToken cancellationToken)
    {
        var item = (await ResolveForUnitsAsync([unitId], cancellationToken))[unitId];
        item.UnitCode = unitCode;
        item.UnitName = unitName;
        return item;
    }

    private async Task<UnitInfo> FindUnitAsync(Guid organizationUnitId, Guid legalEntityId, CancellationToken cancellationToken) =>
        await _context.MstOrganizationUnits
            .AsNoTracking()
            .Where(x => x.Id == organizationUnitId && !x.IsDelete && x.IsActive && x.LegalEntityId == legalEntityId)
            .Select(x => new UnitInfo(x.Id, x.UnitCode, x.UnitName))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw AssetManagementException.NotFound(AssetValuationErrorCodes.NotFound, AssetValuationMessages.UnitNotFound);

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw AssetManagementException.Conflict(AssetValuationErrorCodes.Concurrent, AssetValuationMessages.Concurrent, exception);
        }
    }

    private static string? AccountCode(string? value)
    {
        var code = value?.Trim();
        if (string.IsNullOrEmpty(code))
        {
            return null;
        }

        if (code.Length > MaxAccountCodeLength)
        {
            throw AssetManagementException.Invalid(AssetValuationErrorCodes.Field, string.Format(AssetValuationMessages.AccountLengthFormat, MaxAccountCodeLength));
        }

        return code;
    }

    private sealed record AccountInfo(string Code, string Name, bool IsActive, bool IsPostable, bool IsControlAccount);

    private sealed record UnitInfo(Guid Id, string UnitCode, string UnitName);
}
