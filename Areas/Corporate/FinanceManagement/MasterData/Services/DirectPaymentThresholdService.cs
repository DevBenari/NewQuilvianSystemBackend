using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;

/// <summary>
/// Ambang nilai pembayaran langsung piutang dan utang supplier (BE-FIN-076, FIN-DES-086, FIN-DEC-134).
/// Master berjejak **satu baris aktif** — bukan appsettings.json. Tabel riwayat perubahan SENGAJA
/// TIDAK dibuat (FIN-DES-086): kolom audit `IdentityModel` (`UpdateBy`/`UpdateDateTime`, atau
/// `CreateBy`/`CreateDateTime` bila belum pernah diubah) sudah menjawab perubahan TERAKHIR, dan
/// baris yang sama diperbarui di tempat pada setiap `PUT` — bukan menonaktifkan baris lama dan
/// menyisipkan baris baru, yang diam-diam akan menjadi tabel riwayat implisit.
///
/// Tanpa baris aktif, `GetActiveAsync` melempar <see cref="KeyNotFoundException"/> (404) — fail-closed
/// sesuai FIN-DES-086, bukan dianggap tak terbatas. `BE-FIN-077`/`078` akan menegakkan hal yang sama
/// pada jalur pembayaran langsung (`FIN-VAL-197`).
/// </summary>
public sealed class DirectPaymentThresholdService
{
    private const string LogCategory = "Corporate.FinanceManagement.MasterData";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public DirectPaymentThresholdService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<DirectPaymentThresholdResponse> GetActiveAsync(CancellationToken cancellationToken)
    {
        var entity = await _dbContext.MstDirectPaymentThresholds.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException(
                "Ambang pembayaran langsung belum ditetapkan, sehingga jalur ini belum dapat dipakai.");

        return Map(entity);
    }

    public async Task<DirectPaymentThresholdResponse> UpdateAsync(
        UpdateDirectPaymentThresholdRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        // FIN-VAL-205.
        if (string.IsNullOrWhiteSpace(request.ChangeReason))
        {
            throw new DirectPaymentThresholdValidationException("Alasan perubahan ambang wajib diisi.");
        }

        // FIN-VAL-206.
        if (request.Amount <= 0)
        {
            throw new DirectPaymentThresholdBadRequestException("Ambang harus berupa angka lebih besar dari nol.");
        }

        var entity = await _dbContext.MstDirectPaymentThresholds
            .SingleOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);

        var isFirstTime = entity == null;
        if (entity == null)
        {
            entity = new MstDirectPaymentThreshold
            {
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.Set<MstDirectPaymentThreshold>().Add(entity);
        }
        else
        {
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
        }

        entity.Amount = request.Amount;
        entity.ChangeReason = request.ChangeReason.Trim();
        entity.EffectiveFrom = request.EffectiveFrom;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(
            LogCategory,
            isFirstTime ? "DirectPaymentThreshold.Create" : "DirectPaymentThreshold.Update",
            "Perubahan ambang pembayaran langsung.",
            new
            {
                ThresholdId = entity.Id,
                entity.Amount,
                entity.ChangeReason,
                entity.EffectiveFrom,
                ActorUserId = actorUserId
            });

        return Map(entity);
    }

    private static DirectPaymentThresholdResponse Map(MstDirectPaymentThreshold x) => new()
    {
        Id = x.Id,
        Amount = x.Amount,
        ChangeReason = x.ChangeReason,
        EffectiveFrom = x.EffectiveFrom,
        LastChangedBy = x.UpdateDateTime.HasValue ? x.UpdateBy : x.CreateBy,
        LastChangedAt = x.UpdateDateTime ?? x.CreateDateTime
    };
}
