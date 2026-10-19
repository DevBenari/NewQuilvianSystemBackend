using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// BE-FIN-096, FIN-DES-101, INT-HR-FIN-002 §P.4. Menghitung status bebas tanggungan dari saldo
/// piutang AKTIF milik satu atau beberapa pegawai — murni baca, nol transaksi, nol kolom
/// tersimpan (FIN-VAL-244: hasil MUST selalu dihitung ulang, TIDAK PERNAH dibaca dari kolom
/// tersimpan maupun diterima dari permintaan).
///
/// Yang dihitung HANYA piutang DebtorType = EMPLOYEE_BENEFIT milik BenefitOwnerId itu
/// (FIN-DEC-177) — porsi benefit yang ditagihkan atas penjamin internal (DebtorType = PAYER)
/// MUST NOT ikut dihitung. Status aktif = OUTSTANDING atau PARTIAL; SETTLED/WRITTEN_OFF/CANCELLED
/// tidak menahan status bebas tanggungan (L.4.4).
/// </summary>
public sealed class FinanceReceivableClearanceService
{
    private readonly ApplicationDbContext _dbContext;

    public FinanceReceivableClearanceService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EmployeeClearanceResponse> GetAsync(Guid benefitOwnerId, CancellationToken cancellationToken)
    {
        var results = await GetBatchAsync([benefitOwnerId], cancellationToken);
        return results[0];
    }

    /// <summary>
    /// Status bebas tanggungan berbasis WorkforceProfileId (karyawan atau dokter tetap). Piutang dihitung bila
    /// membawa WorkforceProfileId itu, atau membawa MstEmployee.Id lama milik profil yang sama (piutang historis).
    /// </summary>
    public async Task<List<EmployeeClearanceResponse>> GetBatchByWorkforceAsync(
        IReadOnlyList<Guid> workforceProfileIds, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var ids = workforceProfileIds.Distinct().ToList();
        var employeeIdsByProfile = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => ids.Contains(x.WorkforceProfileId))
            .Select(x => new { x.Id, x.WorkforceProfileId })
            .ToListAsync(cancellationToken);
        var employeeIds = employeeIdsByProfile.Select(x => x.Id).ToList();
        var profileByEmployee = employeeIdsByProfile.ToDictionary(x => x.Id, x => x.WorkforceProfileId);

        var rows = await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => !x.IsDelete
                && x.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit
                && (x.Status == FinReceivableStatuses.Outstanding || x.Status == FinReceivableStatuses.Partial)
                && ((x.BenefitOwnerWorkforceProfileId.HasValue && ids.Contains(x.BenefitOwnerWorkforceProfileId.Value))
                    || (x.BenefitOwnerId.HasValue && employeeIds.Contains(x.BenefitOwnerId.Value))))
            .Select(x => new { x.BenefitOwnerId, x.BenefitOwnerWorkforceProfileId, x.OutstandingAmount, x.DueDate })
            .ToListAsync(cancellationToken);

        return ids.Select(id =>
        {
            var mine = rows.Where(x => (x.BenefitOwnerWorkforceProfileId ?? (x.BenefitOwnerId.HasValue && profileByEmployee.TryGetValue(x.BenefitOwnerId.Value, out var p) ? p : (Guid?)null)) == id).ToList();
            var outstanding = mine.Sum(x => x.OutstandingAmount);
            return new EmployeeClearanceResponse
            {
                BenefitOwnerId = employeeIdsByProfile.Where(e => e.WorkforceProfileId == id).Select(e => e.Id).FirstOrDefault(),
                WorkforceProfileId = id,
                IsCleared = outstanding == 0m,
                OutstandingAmount = outstanding,
                OpenReceivableCount = mine.Count,
                OldestDueDate = mine.Count == 0 ? null : mine.Min(x => x.DueDate),
                CalculatedAt = now
            };
        }).ToList();
    }

    public async Task<List<EmployeeClearanceResponse>> GetBatchAsync(IReadOnlyList<Guid> benefitOwnerIds, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var ids = benefitOwnerIds.Distinct().ToList();

        // L.4.1: pegawai tanpa piutang sama sekali tetap 200 bebas tanggungan — bukan 404.
        // Karena itu agregat dihitung per id secara eksplisit, bukan hanya dari baris yang ada.
        var active = await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => !x.IsDelete
                && x.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit
                && x.BenefitOwnerId.HasValue && ids.Contains(x.BenefitOwnerId.Value)
                && (x.Status == FinReceivableStatuses.Outstanding || x.Status == FinReceivableStatuses.Partial))
            .GroupBy(x => x.BenefitOwnerId!.Value)
            .Select(g => new
            {
                BenefitOwnerId = g.Key,
                OutstandingAmount = g.Sum(x => x.OutstandingAmount),
                OpenReceivableCount = g.Count(),
                OldestDueDate = g.Min(x => x.DueDate)
            })
            .ToListAsync(cancellationToken);

        var byOwner = active.ToDictionary(x => x.BenefitOwnerId);

        return ids.Select(id =>
        {
            byOwner.TryGetValue(id, out var row);
            var outstanding = row?.OutstandingAmount ?? 0m;
            return new EmployeeClearanceResponse
            {
                BenefitOwnerId = id,
                IsCleared = outstanding == 0m, // FIN-DES-101: satu-satunya definisi "bebas".
                OutstandingAmount = outstanding,
                OpenReceivableCount = row?.OpenReceivableCount ?? 0,
                OldestDueDate = row?.OldestDueDate,
                CalculatedAt = now
            };
        }).ToList();
    }
}
