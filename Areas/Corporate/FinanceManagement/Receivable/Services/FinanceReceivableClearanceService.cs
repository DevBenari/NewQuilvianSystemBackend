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
