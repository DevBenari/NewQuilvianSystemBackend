using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>Satu pergerakan kartu stok hasil potong atau kembali.</summary>
    public sealed record OptStockMovement(Guid DrugBatchId, Guid StorageLocationId, decimal Quantity, Guid MutationId);

    /// <summary>Hasil potong/kembali stok satu baris pesanan.</summary>
    public sealed class OptStockOutcome
    {
        /// <summary>Benar bila baris tidak lewat stok (lensa custom), atau tidak ada yang perlu dikembalikan.</summary>
        public bool Skipped { get; init; }

        /// <summary>Benar bila pergerakan sudah pernah tercatat dengan kunci yang sama (dipanggil ulang).</summary>
        public bool IsReplay { get; init; }

        public IReadOnlyList<OptStockMovement> Movements { get; init; } = [];

        public decimal TotalQuantity => Movements.Sum(x => x.Quantity);
    }

    /// <summary>
    /// Jalur stok Optik ke Farmasi (<c>BE-OPT-033</c>, <c>INT-OPT-07</c>, <c>OPT-DEC-004</c>, <c>019</c>, <c>038</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Farmasi tetap pemilik stok.</b> Optik tidak pernah menulis saldo atau kartu stok; semua pergerakan lewat
    /// <see cref="DrugStockService"/> dengan jenis dokumen <see cref="DrugStockSourceDocumentTypes.OpticalOrder"/> dan
    /// <c>SourceDocumentId</c> = pesanan.
    /// </para>
    /// <para>
    /// <b>Kunci tetap per pesanan per baris.</b> Potong memakai <c>OPT:{pesanan}:{baris}:issue</c>, kembali
    /// <c>OPT:{pesanan}:{baris}:return</c>. Dipanggil dua kali dengan kunci sama, stok bergerak sekali
    /// (<c>OPT-AC-008</c>); unique index kartu stok menolak balapan dua permintaan bersamaan.
    /// </para>
    /// <para>
    /// <b>Kembali = Stok Masuk batch yang sama</b> ke lokasi asalnya (<c>OPT-DEC-060</c>, menutup <c>OPT-OQ-015</c>), bukan
    /// koreksi mutasi asal dan bukan dokumen retur Farmasi. Nomor batch dan kedaluwarsanya tetap terlacak.
    /// </para>
    /// <para>
    /// <b>Transaksi milik pemanggil.</b> Kelas ini tidak membuka transaksi. <c>BE-OPT-034</c> membungkus potong stok,
    /// perubahan status pesanan, dan kunci resep dalam satu transaksi, sehingga stok kurang membatalkan semuanya.
    /// </para>
    /// </remarks>
    public class OptStockGateway
    {
        /// <summary>Kode Farmasi untuk stok tidak mencukupi (<c>DrugStockService</c>).</summary>
        private const string FarmasiStokKurang = "PHM028";

        private readonly ApplicationDbContext _dbContext;
        private readonly DrugStockService _drugStockService;
        private readonly OptReferenceDataService _referenceDataService;
        private readonly LoggerService _loggerService;

        public OptStockGateway(
            ApplicationDbContext dbContext,
            DrugStockService drugStockService,
            OptReferenceDataService referenceDataService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _drugStockService = drugStockService;
            _referenceDataService = referenceDataService;
            _loggerService = loggerService;
        }

        public static string IssueKey(Guid orderId, Guid itemId) => $"OPT:{orderId:D}:{itemId:D}:issue";

        public static string ReturnKey(Guid orderId, Guid itemId) => $"OPT:{orderId:D}:{itemId:D}:return";

        /// <summary>Lensa custom dipesan ke lab dan tidak pernah ada di stok depo (<c>INT-OPT-07</c>).</summary>
        public static bool RequiresStock(OptOrderItem item) => item.LensSupply != OptLensSupply.Custom;

        /// <summary>
        /// Memotong stok satu baris pesanan dari Depo Optik (FEFO). Stok kurang → <c>409 OPT-VAL-049</c> tanpa satu pun
        /// pergerakan tercatat.
        /// </summary>
        public async Task<OptResult<OptStockOutcome>> IssueAsync(OptOrderItem item, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(item);
            if (!RequiresStock(item))
                return OptResult<OptStockOutcome>.Ok(new OptStockOutcome { Skipped = true });
            if (item.Quantity <= 0)
                return OptResult<OptStockOutcome>.Invalid(OptErrorCodes.InvalidRequest, "Jumlah barang harus lebih dari nol.");

            var depoId = await _referenceDataService.FindOpticalDepotIdAsync(cancellationToken);
            if (depoId is null)
                return OptResult<OptStockOutcome>.Conflict(OptErrorCodes.OpticalDepotMissing,
                    "Depo Optik belum didaftarkan atau tidak aktif. Hubungi bagian Farmasi.");

            var kunci = IssueKey(item.OrderId, item.Id);
            var sudahAda = await MovementsAsync(kunci, cancellationToken);

            StockOperationResponse hasil;
            try
            {
                hasil = await _drugStockService.IssueAsync(new IssueStockRequest
                {
                    DrugId = item.DrugId,
                    StorageLocationId = depoId.Value,
                    Quantity = item.Quantity,
                    SourceDocumentType = DrugStockSourceDocumentTypes.OpticalOrder,
                    SourceDocumentId = item.OrderId,
                    Reason = $"Pesanan Optik baris {item.LineNo}.",
                    IdempotencyKey = kunci,
                }, cancellationToken);
            }
            catch (DrugStockUnprocessableException ex) when (ex.Code == FarmasiStokKurang)
            {
                LepasPergerakanTertunda();
                return OptResult<OptStockOutcome>.Conflict(OptErrorCodes.Val049,
                    $"Stok {await LabelBarangAsync(item, cancellationToken)} di depo Optik tidak cukup.");
            }
            catch (DrugStockUnprocessableException ex)
            {
                LepasPergerakanTertunda();
                return OptResult<OptStockOutcome>.Conflict(OptErrorCodes.StockRejected, ex.Message);
            }
            catch (DrugStockConflictException ex)
            {
                LepasPergerakanTertunda();
                return OptResult<OptStockOutcome>.Conflict(OptErrorCodes.ConcurrencyConflict, ex.Message);
            }
            catch (DrugStockForbiddenException ex)
            {
                LepasPergerakanTertunda();
                return OptResult<OptStockOutcome>.Forbidden(OptErrorCodes.StockForbidden, ex.Message);
            }
            catch (DbException)
            {
                LepasPergerakanTertunda();
                return OptResult<OptStockOutcome>.Unavailable(OptErrorCodes.StockUnavailable,
                    "Stok tidak dapat dipotong karena layanan Farmasi sedang tidak dapat diakses. Silakan coba lagi.");
            }

            return OptResult<OptStockOutcome>.Ok(new OptStockOutcome
            {
                IsReplay = sudahAda.Count > 0,
                Movements = [.. hasil.Allocations.Select(x => new OptStockMovement(x.DrugBatchId, hasil.StorageLocationId, x.Quantity, x.MutationId))],
            });
        }

        /// <summary>
        /// Mengembalikan seluruh stok yang pernah dipotong untuk satu baris: Stok Masuk batch yang sama ke lokasi asalnya
        /// (<c>OPT-DEC-060</c>). Baris yang belum pernah dipotong (atau lensa custom) → <c>Skipped</c>, tanpa pergerakan.
        /// </summary>
        public async Task<OptResult<OptStockOutcome>> ReturnAsync(OptOrderItem item, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(item);

            var kunciKembali = ReturnKey(item.OrderId, item.Id);
            var sudahKembali = await MovementsAsync(kunciKembali, cancellationToken);
            if (sudahKembali.Count > 0)
                return OptResult<OptStockOutcome>.Ok(new OptStockOutcome { IsReplay = true, Movements = Map(sudahKembali) });

            var dipotong = await MovementsAsync(IssueKey(item.OrderId, item.Id), cancellationToken);
            if (dipotong.Count == 0)
                return OptResult<OptStockOutcome>.Ok(new OptStockOutcome { Skipped = true });

            var dicatat = new List<PhmDrugStockMutation>();
            try
            {
                var urut = 0;
                foreach (var keluar in dipotong)
                {
                    dicatat.Add(await _drugStockService.ReceiveBatchAsync(
                        keluar.DrugBatchId,
                        keluar.StorageLocationId,
                        Math.Abs(keluar.QuantityChange),
                        DrugStockSourceDocumentTypes.OpticalOrder,
                        item.OrderId,
                        $"Pesanan Optik baris {item.LineNo} batal; barang kembali ke depo.",
                        $"{kunciKembali}:{urut++}",
                        cancellationToken));
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DrugStockUnprocessableException ex)
            {
                LepasPergerakanTertunda();
                return OptResult<OptStockOutcome>.Conflict(OptErrorCodes.StockRejected, ex.Message);
            }
            catch (DrugStockForbiddenException ex)
            {
                LepasPergerakanTertunda();
                return OptResult<OptStockOutcome>.Forbidden(OptErrorCodes.StockForbidden, ex.Message);
            }
            catch (DbUpdateException)
            {
                // Permintaan lain dengan kunci sama menang lebih dulu (unique index kartu stok). Hasil akhirnya tetap
                // satu pengembalian; jawab dengan milik pemenang.
                LepasPergerakanTertunda();
                var pemenang = await MovementsAsync(kunciKembali, cancellationToken);
                return pemenang.Count > 0
                    ? OptResult<OptStockOutcome>.Ok(new OptStockOutcome { IsReplay = true, Movements = Map(pemenang) })
                    : OptResult<OptStockOutcome>.Conflict(OptErrorCodes.ConcurrencyConflict,
                        "Stok sedang diubah proses lain. Muat ulang lalu coba kembali.");
            }
            catch (DbException)
            {
                LepasPergerakanTertunda();
                return OptResult<OptStockOutcome>.Unavailable(OptErrorCodes.StockUnavailable,
                    "Stok tidak dapat dikembalikan karena layanan Farmasi sedang tidak dapat diakses. Silakan coba lagi.");
            }

            var total = dicatat.Sum(x => x.QuantityChange);
            await _loggerService.AuditAsync("Optical", "OptStock.Return",
                $"Stok baris {item.LineNo} pesanan Optik {item.OrderId:D} dikembalikan ke depo: {total:0.##} lewat {dicatat.Count} batch.",
                new { item.OrderId, OrderItemId = item.Id, Quantity = total });

            return OptResult<OptStockOutcome>.Ok(new OptStockOutcome { Movements = Map(dicatat) });
        }

        // ---------------------------------------------------------------- penolong

        private Task<List<PhmDrugStockMutation>> MovementsAsync(string kunci, CancellationToken cancellationToken)
        {
            var awalan = kunci + ":";
            return _dbContext.Set<PhmDrugStockMutation>().AsNoTracking()
                .Where(x => x.SourceDocumentType == DrugStockSourceDocumentTypes.OpticalOrder
                            && x.CorrelationId != null && x.CorrelationId.StartsWith(awalan) && !x.IsDelete)
                .OrderBy(x => x.CorrelationId)
                .ToListAsync(cancellationToken);
        }

        private static List<OptStockMovement> Map(IEnumerable<PhmDrugStockMutation> rows) =>
            [.. rows.Select(x => new OptStockMovement(x.DrugBatchId, x.StorageLocationId, Math.Abs(x.QuantityChange), x.Id))];

        /// <summary>
        /// Membuang kartu stok dan saldo yang sudah ditambahkan Farmasi tetapi belum tersimpan, misalnya ketika batch kedua
        /// kurang sesudah batch pertama dicatat. Tanpa ini penyimpanan berikutnya oleh pemanggil membawa potongan setengah jadi.
        /// </summary>
        private void LepasPergerakanTertunda()
        {
            foreach (var entri in _dbContext.ChangeTracker.Entries()
                         .Where(x => (x.Entity is PhmDrugStockMutation or PhmDrugStockBalance)
                                     && x.State is EntityState.Added or EntityState.Modified)
                         .ToList())
                entri.State = EntityState.Detached;
        }

        private async Task<string> LabelBarangAsync(OptOrderItem item, CancellationToken cancellationToken)
        {
            var kode = await _dbContext.Set<MstDrug>().AsNoTracking()
                .Where(x => x.Id == item.DrugId)
                .Select(x => x.DrugCode)
                .FirstOrDefaultAsync(cancellationToken);
            var kategori = OptLabels.ItemCategory(item.ItemCategory).ToLowerInvariant();
            return string.IsNullOrWhiteSpace(kode) ? kategori : $"{kategori} {kode}";
        }
    }
}
