using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;

/// <summary>
/// Pembacaan Batas Pembayaran Langsung yang BERLAKU bagi piutang dan utang supplier (BE-FIN-076, FIN-DES-086, FIN-DEC-134).
/// Master berjejak **satu baris aktif** — bukan appsettings.json.
///
/// FIN-BE-DPT-001 mengubah keputusan lama: <c>UpdateAsync</c> (yang menimpa <c>Amount</c> seketika tanpa persetujuan) DIHAPUS.
/// Penetapan dan perubahan nominal kini hanya lewat <see cref="DirectPaymentThresholdProposalService"/>
/// (Pengajuan → Persetujuan → Aktivasi), dan baris yang dibaca di sini hanya diubah oleh aktivasi. Tabel riwayat yang dahulu
/// sengaja tidak dibuat (FIN-DES-086) kini ada: <c>MstDirectPaymentThresholdProposal</c> dan <c>MstDirectPaymentThresholdEvent</c>.
///
/// Tanpa baris aktif, <see cref="GetActiveAsync"/> melempar <see cref="KeyNotFoundException"/> (404) — fail-closed, bukan dianggap
/// tak terbatas, dan pengajuan yang belum diaktifkan TIDAK dianggap batas aktif (FIN-VAL-197).
/// </summary>
public sealed class DirectPaymentThresholdService
{
    private readonly ApplicationDbContext _dbContext;

    public DirectPaymentThresholdService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DirectPaymentThresholdResponse> GetActiveAsync(CancellationToken cancellationToken)
    {
        var entity = await _dbContext.MstDirectPaymentThresholds.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException(
                "Batas pembayaran langsung belum ditetapkan, sehingga jalur ini belum dapat dipakai.");

        return await MapAsync(entity, cancellationToken);
    }

    private async Task<DirectPaymentThresholdResponse> MapAsync(
        MstDirectPaymentThreshold x, CancellationToken cancellationToken)
    {
        var lastChangedBy = x.UpdateDateTime.HasValue ? x.UpdateBy : x.CreateBy;

        return new DirectPaymentThresholdResponse
        {
            Id = x.Id,
            Amount = x.Amount,
            ChangeReason = x.ChangeReason,
            RowVersion = x.RowVersion,
            LastChangedBy = lastChangedBy,
            LastChangedByName = await GetUserNameAsync(lastChangedBy, cancellationToken),
            LastChangedAt = x.UpdateDateTime ?? x.CreateDateTime
        };
    }

    /// <summary>
    /// Nama tampilan satu pengguna (FIN-DEC-151, FIN-DES-095), DIBACA saat menyusun respons dari audit (<c>UpdateBy</c>/<c>CreateBy</c>)
    /// — bukan disalin ke tabel Finance dan bukan nama pengguna yang sedang login. Memulangkan <c>null</c> bila ID kosong atau
    /// penggunanya tidak ditemukan; pemanggil MUST NOT menggantinya dengan ID mentah.
    /// </summary>
    private async Task<string?> GetUserNameAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
