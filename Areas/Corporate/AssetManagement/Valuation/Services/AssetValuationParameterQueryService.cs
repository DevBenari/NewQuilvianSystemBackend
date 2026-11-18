using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

public sealed class AssetValuationParameterQueryService
{
    private readonly ApplicationDbContext _context;

    public AssetValuationParameterQueryService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<AstValuationParameter?> GetParameterAtAsync(Guid legalEntityId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var period = AssetValuationPeriod.Of(date);
        return _context.AstValuationParameters
            .AsNoTracking()
            .Where(x => x.LegalEntityId == legalEntityId && !x.IsDelete && x.EffectiveFromPeriod <= period)
            .OrderByDescending(x => x.EffectiveFromPeriod)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AstValuationParameter> GetRequiredParameterAtAsync(Guid legalEntityId, DateOnly date, CancellationToken cancellationToken = default) =>
        await GetParameterAtAsync(legalEntityId, date, cancellationToken)
        ?? throw AssetManagementException.Conflict(AssetValuationErrorCodes.ParameterMissing, AssetValuationMessages.ParameterMissing);

    public Task<AstDepreciationPolicy?> GetPolicyAtAsync(Guid assetGroupId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var period = AssetValuationPeriod.Of(date);
        return _context.AstDepreciationPolicies
            .AsNoTracking()
            .Where(x => x.AssetGroupId == assetGroupId && !x.IsDelete && x.EffectiveFromPeriod <= period)
            .OrderByDescending(x => x.EffectiveFromPeriod)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
