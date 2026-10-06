using Microsoft.EntityFrameworkCore;
using Npgsql;
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
///
/// BE-FIN-086 (FIN-DES-094/095, FIN-DEC-145/146/151/153) menambahkan tiga hal dan membuang satu:
/// (1) penanda versi `RowVersion` diperiksa pada setiap perubahan, sehingga dua pejabat tidak saling
/// menimpa (`FIN-VAL-228`, 409); (2) pengecualian WAJIB untuk penetapan ambang pertama, yang tanpa
/// itu ambang tidak akan pernah dapat ditetapkan; (3) nama pengubah terakhir DIBACA saat menyusun
/// respons, bukan disimpan. Yang dibuang: `EffectiveFrom` — ambang selalu berlaku SEKETIKA.
///
/// Urutan pemeriksaan pada `PUT` MUST tetap: alasan kosong (422) → nominal tidak sah (400) →
/// benturan versi (409). Isian yang jelas salah dijawab lebih dulu (`FIN-VAL-1.9` H.1).
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

        return await MapAsync(entity, cancellationToken);
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
            // Penetapan PERTAMA: belum ada baris, sehingga ExpectedRowVersion tidak diperiksa
            // (FIN-VAL-228 pengecualian, FIN-API-1.7 G.1). Tanpa jalur ini ambang tidak akan pernah
            // dapat ditetapkan dan seluruh pembayaran langsung terkunci permanen.
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
            // FIN-VAL-228: baris sudah ada, sehingga penanda versi WAJIB dan MUST cocok. Urutannya
            // SENGAJA sesudah FIN-VAL-205 dan FIN-VAL-206 di atas — isian yang jelas salah dijawab
            // lebih dulu, sebelum pengguna disuruh memuat ulang karena benturan versi.
            if (request.ExpectedRowVersion is null
                || request.ExpectedRowVersion == Guid.Empty
                || request.ExpectedRowVersion != entity.RowVersion)
            {
                throw new DirectPaymentThresholdConflictException(
                    "Ambang sudah diubah oleh orang lain. Muat ulang sebelum melanjutkan.");
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
        }

        entity.Amount = request.Amount;
        entity.ChangeReason = request.ChangeReason.Trim();

        // Memutar penanda versi mengikuti pola FinanceOpeningBalanceService: nilai lama tetap dipakai
        // EF pada klausa WHERE (ConcurrencyCheck), nilai baru menjadi versi berikutnya.
        entity.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Lapis kedua: benturan yang terjadi di antara pembacaan di atas dan penyimpanan ini.
            // Dijawab sama seperti FIN-VAL-228 — fail-closed, bukan ditimpa.
            throw new DirectPaymentThresholdConflictException(
                "Ambang sudah diubah oleh orang lain. Muat ulang sebelum melanjutkan.");
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            // Dua pejabat menetapkan ambang PERTAMA bersamaan. Keduanya melihat 404, keduanya lolos
            // pemeriksaan di atas, lalu IX_MstDirectPaymentThreshold_Active menolak yang kedua.
            // Penanda versi TIDAK dapat menangkap ini — pada penetapan pertama belum ada versi untuk
            // dibandingkan. Dijawab 409 yang sama supaya pengguna memuat ulang dan melihat ambang
            // yang baru ditetapkan rekannya, BUKAN 500 yang tidak memberi tahu apa pun.
            throw new DirectPaymentThresholdConflictException(
                "Ambang sudah ditetapkan oleh orang lain. Muat ulang sebelum melanjutkan.");
        }

        await _loggerService.AuditAsync(
            LogCategory,
            isFirstTime ? "DirectPaymentThreshold.Create" : "DirectPaymentThreshold.Update",
            "Perubahan ambang pembayaran langsung.",
            new
            {
                ThresholdId = entity.Id,
                entity.Amount,
                entity.ChangeReason,
                ActorUserId = actorUserId
            });

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
    /// Benturan unique index, bukan kegagalan lain. Mengikuti pola kanonik repository
    /// (<c>HmdServiceSupport.IsUniqueViolation</c>, <c>EncounterIntakeService</c>): hanya
    /// <c>23505</c> yang diterjemahkan menjadi benturan — galat database lain MUST tetap naik
    /// apa adanya, supaya tidak tersamarkan sebagai "sudah diubah orang lain".
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException exception)
        => (exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation;

    /// <summary>
    /// Nama tampilan satu pengguna (FIN-DEC-151, FIN-DES-095). Mengikuti pola <c>GetUserNamesAsync</c>
    /// pada <c>FinanceOpeningBalanceService</c>: nama DIBACA saat menyusun respons, bukan disalin ke
    /// tabel Finance — menyalinnya membuat nama membeku ketika nama aslinya berubah.
    ///
    /// Memulangkan <c>null</c> bila ID kosong atau penggunanya tidak ditemukan; pemanggil MUST NOT
    /// menggantinya dengan ID mentah.
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
