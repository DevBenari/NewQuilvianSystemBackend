using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>
/// BE-FIN-051. Dicek sebelum tiap aksi POST rumpun Purchasing yang wajib
/// <c>Idempotency-Key</c> (FIN-DES-006); ditulis SEKALI setelah aksi berhasil. Dipanggil dari
/// controller (bukan dari lima service Purchasing) supaya nol perubahan pada logika bisnis yang
/// sudah berjalan — lihat komentar kelas <see cref="FinPurchasingIdempotencyRecord"/> untuk alasan
/// desain lengkapnya.
/// </summary>
public sealed class PurchasingIdempotencyService
{
    private readonly ApplicationDbContext _dbContext;
    public PurchasingIdempotencyService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    /// <summary>Baris hasil replay: kembalikan apa adanya, jangan hitung ulang apa pun.</summary>
    public sealed record CachedResult(int StatusCode, string ResponseBody);

    /// <summary>
    /// Opsi serialisasi yang sama dengan keluaran MVC (<c>JsonSerializerDefaults.Web</c>, camelCase).
    /// Sebelumnya badan replay ditulis dengan opsi bawaan (PascalCase) sehingga replay berbeda bentuk
    /// dari respons pertama dan klien yang membaca <c>data.grNumber</c> mendapat <c>undefined</c>.
    /// Baris lama yang sudah tersimpan PascalCase tetap dikirim apa adanya.
    /// </summary>
    private static readonly JsonSerializerOptions ResponseJsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Guid.Empty (header tidak dikirim) sengaja diperlakukan sebagai "nol dedup" di sini, bukan
    /// ditolak di lapisan ini — <c>[ApiController]</c> pada seluruh controller Purchasing sudah
    /// menolak `400` otomatis untuk parameter <see cref="Guid"/> wajib yang headernya tidak ada,
    /// jadi jalur ini praktiknya hanya tercapai bila header memang terisi.
    ///
    /// AC-18: kunci hanya boleh mengulang hasil aksi yang SAMA. Kunci yang sudah terpakai oleh
    /// entity/aksi lain, atau oleh dokumen lain pada aksi bersumber-ID, dijawab `409` — bukan
    /// mengembalikan hasil transaksi lain. <paramref name="entityId"/> null pada aksi Create (ID
    /// dokumen baru belum ada saat permintaan masuk). Perbandingan isi permintaan (fingerprint
    /// payload) membutuhkan kolom baru dan belum diaktifkan — lihat spesifikasi schema.
    /// </summary>
    public async Task<CachedResult?> TryReplayAsync(
        Guid idempotencyKey, string entityType, string action, Guid? entityId, CancellationToken cancellationToken)
    {
        if (idempotencyKey == Guid.Empty) return null;

        var record = await _dbContext.FinPurchasingIdempotencyRecords.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (record is null) return null;

        var sameBinding = record.EntityType == entityType
            && record.Action == action
            && (!entityId.HasValue || record.EntityId == entityId.Value);
        if (!sameBinding)
        {
            var conflict = ApiResponse<object>.Fail(
                409, "Idempotency-Key ini sudah dipakai untuk aksi atau dokumen lain. Gunakan kunci baru untuk aksi ini.");
            return new CachedResult(409, JsonSerializer.Serialize(conflict, ResponseJsonOptions));
        }

        return new CachedResult(record.ResponseStatusCode, record.ResponseBody);
    }

    /// <summary>
    /// Merekam hasil sukses. Dipanggil SETELAH `SaveChangesAsync` aksi bisnisnya sendiri sudah
    /// commit (services Purchasing membuka/menutup transaksinya masing-masing) — baris ledger ini
    /// sengaja ditulis dalam `SaveChangesAsync` TERPISAH sesudahnya, bukan digabung ke transaksi
    /// bisnis, supaya nol perubahan pada lima service yang sudah berjalan dan teruji. Konsekuensi:
    /// ada jendela sempit antara commit bisnis dan commit ledger ini — bila proses mati persis di
    /// situ, permintaan ulang akan memproses ulang aksi bisnisnya (bukan silent double-processing
    /// tanpa jejak, karena penjaga status entity yang sudah ada tetap menolak transisi ilegal).
    /// Diterima sebagai trade-off proporsional untuk task ini; dicatat di laporan, bukan didiamkan.
    /// </summary>
    public async Task SaveAsync<TResponse>(
        Guid idempotencyKey, string entityType, string action, Guid entityId,
        int statusCode, TResponse responseBody, CancellationToken cancellationToken)
    {
        if (idempotencyKey == Guid.Empty) return;

        _dbContext.FinPurchasingIdempotencyRecords.Add(new FinPurchasingIdempotencyRecord
        {
            IdempotencyKey = idempotencyKey,
            EntityType = entityType,
            Action = action,
            EntityId = entityId,
            ResponseStatusCode = statusCode,
            ResponseBody = JsonSerializer.Serialize(responseBody, ResponseJsonOptions),
        });

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Indeks unik IdempotencyKey race: permintaan lain ber-kunci sama sudah menulis baris
            // ini lebih dulu (dua retry klien bersamaan). Penulis pertama menang — baris yang sudah
            // ada itulah yang sah, permintaan ini tidak perlu menulis lagi.
        }
    }
}
