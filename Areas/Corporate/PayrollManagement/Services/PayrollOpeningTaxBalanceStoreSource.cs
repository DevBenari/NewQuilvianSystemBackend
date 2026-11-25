using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Pemeriksa keberadaan tabel saldo awal PPh. Tabel dibuat lewat DDL yang disetujui pemilik (bukan migration), sehingga backend yang
/// naik SEBELUM DDL diterapkan tidak boleh gagal: pada PostgreSQL, query ke tabel yang tidak ada membuat transaksi payroll yang sedang
/// berjalan tidak dapat dilanjutkan. Karena itu keberadaan tabel diperiksa lebih dulu lewat <c>to_regclass</c> (tidak melempar galat).
/// Hasil positif disimpan; hasil negatif diperiksa ulang pada pemanggilan berikutnya.
/// </summary>
internal static class PayrollOpeningTaxBalanceStorage
{
    private static volatile bool _exists;

    public static async Task<bool> ExistsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (_exists) return true;
        if (!dbContext.Database.IsRelational()) return false;

        var exists = await dbContext.Database
            .SqlQuery<bool>($"SELECT to_regclass('public.\"PrlOpeningTaxBalance\"') IS NOT NULL AS \"Value\"")
            .FirstAsync(cancellationToken);
        if (exists) _exists = true;
        return exists;
    }
}

/// <summary>
/// Sumber saldo awal PPh yang membaca tabel <c>PrlOpeningTaxBalance</c>: hanya status APPROVED dan yang belum dihapus; versi lama sudah
/// berstatus SUPERSEDED sehingga tidak ikut. Murni baca. Bila tabel belum ada, ia menyatakan dirinya TIDAK TERSEDIA (sama seperti sumber
/// sebelumnya), bukan saldo nol, sehingga rekonsiliasi masa pajak terakhir tetap tertahan dengan alasan yang jujur.
///
/// <see cref="IsAvailable"/> diisi saat <see cref="LoadApprovedAsync"/>; pemanggil membacanya sesudah memuat (urutan yang dipakai
/// <c>PayrollCalculationService</c>).
/// </summary>
public sealed class PayrollOpeningTaxBalanceStoreSource : IPayrollOpeningTaxBalanceSource
{
    private readonly ApplicationDbContext _dbContext;
    private bool _available;

    public PayrollOpeningTaxBalanceStoreSource(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public bool IsAvailable => _available;

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<PayrollOpeningTaxBalanceFacts>>> LoadApprovedAsync(
        IReadOnlyCollection<Guid> workforceProfileIds, int taxYear, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, IReadOnlyList<PayrollOpeningTaxBalanceFacts>>();
        _available = await PayrollOpeningTaxBalanceStorage.ExistsAsync(_dbContext, cancellationToken);
        if (!_available || workforceProfileIds.Count == 0) return result;

        var ids = workforceProfileIds.ToList();
        var rows = await _dbContext.PrlOpeningTaxBalances.AsNoTracking()
            .Where(x => !x.IsDelete && x.TaxYear == taxYear && x.Status == OpeningTaxBalanceStatuses.Approved
                && ids.Contains(x.WorkforceProfileId))
            .ToListAsync(cancellationToken);

        foreach (var group in rows.GroupBy(x => x.WorkforceProfileId))
        {
            result[group.Key] = group
                .OrderBy(x => x.EmployerSource).ThenByDescending(x => x.Version)
                .Select(x => new PayrollOpeningTaxBalanceFacts(
                    x.Id, x.TaxYear, x.CoveredMonths.OrderBy(m => m).ToList(), x.TaxableGross, x.EmployerPremium, x.PensionDeduction,
                    x.TaxWithheld, x.EmployerSource, x.Status, x.SourceReference, x.Version))
                .ToList();
        }

        return result;
    }
}
