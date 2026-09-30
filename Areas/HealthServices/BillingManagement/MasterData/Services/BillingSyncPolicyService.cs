using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;

/// <summary>
/// Membaca dan mengubah dua kebijakan kirim ulang (<c>FACT_DISPATCH</c>, <c>INVOICE_SYNC</c>) tanpa
/// rilis (<c>BE-RJE-012</c>, <c>RJ-E2E-DEC-009</c>). Kedua baris adalah seed tetap; tidak ada
/// tambah maupun hapus. Menonaktifkan kebijakan menghentikan kirim ulang otomatis (fail-closed).
/// </summary>
public sealed class BillingSyncPolicyService
{
    private const string LogCategory = "BillingManagement";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public BillingSyncPolicyService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<List<BillingSyncPolicyResponse>> ListAsync(CancellationToken cancellationToken) =>
        await _dbContext.Set<MstBillingSyncPolicy>().AsNoTracking()
            .Where(x => !x.IsDelete)
            .OrderBy(x => x.PolicyCode)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);

    public async Task<BillingSyncPolicyResponse> UpdateAsync(
        Guid id,
        UpdateBillingSyncPolicyRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (request.MaxAttemptCount is < 0 or > 20
            || request.BaseDelaySeconds is < 10 or > 3600
            || request.MaxDelaySeconds < request.BaseDelaySeconds
            || request.MaxDelaySeconds > 86400)
            throw new BillingSyncPolicyException("RJE-VAL-030", 422, "Angka kebijakan di luar batas yang diizinkan.");

        var policy = await _dbContext.Set<MstBillingSyncPolicy>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kebijakan kirim ulang tidak ditemukan.");

        if (policy.RowVersion != request.RowVersion)
            throw Stale();

        var before = new { policy.MaxAttemptCount, policy.BaseDelaySeconds, policy.MaxDelaySeconds, policy.IsActive };
        policy.MaxAttemptCount = request.MaxAttemptCount;
        policy.BaseDelaySeconds = request.BaseDelaySeconds;
        policy.MaxDelaySeconds = request.MaxDelaySeconds;
        policy.IsActive = request.IsActive;
        policy.RowVersion = Guid.NewGuid();
        policy.UpdateDateTime = DateTime.UtcNow;
        policy.UpdateBy = actorUserId;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw Stale();
        }

        await _loggerService.AuditAsync(LogCategory, "BillingSyncPolicy.Update",
            "Mengubah kebijakan kirim ulang otomatis.",
            new
            {
                policy.Id, policy.PolicyCode, ActorUserId = actorUserId, Before = before,
                After = new { policy.MaxAttemptCount, policy.BaseDelaySeconds, policy.MaxDelaySeconds, policy.IsActive }
            });

        return Map(policy);
    }

    private static BillingSyncPolicyException Stale() =>
        new("RJE-VAL-031", 409, "Kebijakan sudah diubah pengguna lain. Muat ulang, lalu ulangi perubahan.");

    private static BillingSyncPolicyResponse Map(MstBillingSyncPolicy x) => new()
    {
        Id = x.Id,
        PolicyCode = x.PolicyCode,
        PolicyName = x.PolicyName,
        MaxAttemptCount = x.MaxAttemptCount,
        BaseDelaySeconds = x.BaseDelaySeconds,
        MaxDelaySeconds = x.MaxDelaySeconds,
        IsActive = x.IsActive,
        Description = x.Description,
        RowVersion = x.RowVersion,
        UpdatedAt = x.UpdateDateTime
    };
}

public sealed class BillingSyncPolicyException(string code, int statusCode, string message) : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}
