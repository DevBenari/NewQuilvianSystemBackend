using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Penahanan stok milik resep: dibuat saat finalisasi, dipakai Farmasi, dikonsumsi saat
/// penyerahan, dan dilepas oleh sistem (<c>PHA-DEC-066</c>).
/// </summary>
/// <remarks>
/// <para>
/// <b>Yang dipakai apa adanya dan tidak digandakan.</b> Jumlah yang ditahan tetap disimpan pada
/// <c>PhmDrugStockBalance.QuantityReserved</c>, pemilihan batch tetap memakai FEFO milik
/// <c>DrugStockService</c>, penolakan kekurangan stok tetap <c>PHM028</c> dari
/// <c>PlanFefo</c>, dan proteksi balapan tetap kolom <c>Version</c> yang sudah terdaftar
/// <c>IsConcurrencyToken()</c>. Service ini hanya menambahkan yang belum ada: kepemilikan oleh
/// resep, batas waktu, konsumsi sekali saja, dan jejak audit.
/// </para>
/// <para>
/// <b>Atomik per resep.</b> <c>PHA-DEC-066</c> butir 1 menuntut seluruh butir satu resep
/// berhasil bersama-sama. Service ini tidak membuka transaksinya sendiri — pemanggilnya
/// (<c>ConsultationFinalizationService</c>) sudah berjalan di dalam satu transaksi, sehingga
/// kegagalan satu butir mengembalikan seluruhnya. Memulai transaksi kedua di sini justru akan
/// memecah keatomikan itu.
/// </para>
/// </remarks>
public class PrescriptionStockReservationService
{
    /// <summary>
    /// <c>PHA-DEC-066</c> butir 6: batas penahanan rawat jalan/APS sebelum pembayaran valid.
    /// </summary>
    public static readonly TimeSpan OutpatientTimeToLive = TimeSpan.FromHours(4);

    private readonly ApplicationDbContext _dbContext;
    private readonly PharmacyDepotRoutingService _depotRoutingService;

    public PrescriptionStockReservationService(
        ApplicationDbContext dbContext,
        PharmacyDepotRoutingService depotRoutingService)
    {
        _dbContext = dbContext;
        _depotRoutingService = depotRoutingService;
    }

    /// <summary>
    /// Menahan stok untuk seluruh butir sebuah resep yang baru difinalkan.
    /// </summary>
    /// <remarks>
    /// Idempoten terhadap <paramref name="idempotencyKey"/>: pemanggilan ulang dengan kunci yang
    /// sama mengembalikan penahanan yang sudah ada tanpa menambah apa pun
    /// (<c>PHA-DEC-066</c> butir 2 dan 3).
    /// </remarks>
    public async Task<PrescriptionReservationResult> ReserveForPrescriptionAsync(
        PhmPrescription prescription,
        Guid actorUserId,
        DateTime nowUtc,
        string idempotencyKey,
        string sourceEvent,
        CancellationToken cancellationToken = default)
    {
        if (prescription == null)
            return PrescriptionReservationResult.Failure(
                PrescriptionReservationErrorCodes.ReservationUnavailable,
                "Resep tidak tersedia untuk penahanan stok.");

        // Idempotency dijaga lebih dulu, sebelum menyentuh stok sama sekali. Retry finalisasi
        // karena itu tidak pernah menambah penahanan kedua.
        var existing = await _dbContext.Set<PhmPrescriptionStockReservation>()
            .Where(x => x.PrescriptionId == prescription.Id && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var aktif = existing
            .Where(x => x.State == PrescriptionReservationState.Reserved)
            .ToList();

        if (aktif.Count > 0)
            return PrescriptionReservationResult.AlreadyReserved(aktif);

        if (existing.Any(x => x.IdempotencyKey == idempotencyKey && idempotencyKey.Length > 0))
            return PrescriptionReservationResult.AlreadyReserved(existing
                .Where(x => x.IdempotencyKey == idempotencyKey)
                .ToList());

        var depot = await _depotRoutingService.ResolveAsync(prescription.EncounterId, cancellationToken);

        if (!depot.IsSuccess || !depot.StorageLocationId.HasValue)
            return PrescriptionReservationResult.Failure(
                PrescriptionReservationErrorCodes.DepotUnresolved, depot.Message);

        var storageLocationId = depot.StorageLocationId.Value;
        var expiresAt = await ResolveExpiryAsync(prescription, nowUtc, cancellationToken);
        var kebutuhan = await LoadRequirementsAsync(prescription.Id, cancellationToken);

        if (kebutuhan.Count == 0)
            return PrescriptionReservationResult.Failure(
                PrescriptionReservationErrorCodes.ReservationUnavailable,
                "Resep tidak memiliki butir yang dapat ditahan.");

        var dibuat = new List<PhmPrescriptionStockReservation>();

        // Kenaikan `QuantityReserved` yang sudah diterapkan, supaya dapat dibatalkan sendiri
        // bila satu butir gagal. Mengandalkan rollback transaksi pemanggil saja tidak cukup:
        // pemanggil lain — atau uji — dapat memakai service ini tanpa transaksi, dan keadaan
        // parsial akan tertinggal. Jaminan "nol penahanan parsial" harus berlaku di sini.
        var terapan = new List<(PhmDrugStockBalance Balance, decimal Quantity)>();

        foreach (var butir in kebutuhan)
        {
            var saldo = await LoadFefoBalancesAsync(butir.DrugId, storageLocationId, cancellationToken);
            var sisa = butir.Quantity;

            foreach (var balance in saldo)
            {
                if (sisa <= 0) break;

                var tersedia = balance.QuantityOnHand - balance.QuantityReserved;
                if (tersedia <= 0) continue;

                var ambil = Math.Min(tersedia, sisa);

                balance.QuantityReserved += ambil;
                balance.Version++;
                balance.UpdateDateTime = nowUtc;
                balance.UpdateBy = actorUserId;
                terapan.Add((balance, ambil));

                var baris = new PhmPrescriptionStockReservation
                {
                    PrescriptionId = prescription.Id,
                    PrescriptionItemId = butir.PrescriptionItemId,
                    PrescriptionCompoundItemId = butir.PrescriptionCompoundItemId,
                    DrugId = butir.DrugId,
                    DrugBatchId = balance.DrugBatchId,
                    StorageLocationId = storageLocationId,
                    Quantity = ambil,
                    State = PrescriptionReservationState.Reserved,
                    ReservedAt = nowUtc,
                    ExpiresAt = expiresAt,
                    ActorUserId = actorUserId,
                    SourceEvent = sourceEvent,
                    IdempotencyKey = idempotencyKey,
                    CreateDateTime = nowUtc,
                    CreateBy = actorUserId
                };

                _dbContext.Set<PhmPrescriptionStockReservation>().Add(baris);
                dibuat.Add(baris);

                sisa -= ambil;
            }

            if (sisa > 0)
            {
                // Satu butir gagal berarti seluruh resep gagal (`PHA-DEC-066` butir 1).
                // Seluruh kenaikan yang sudah diterapkan dibatalkan, dan baris penahanan yang
                // sudah ditambahkan dikeluarkan dari pelacak, sehingga tidak ada penahanan
                // parsial yang tertinggal bahkan ketika pemanggilnya tidak memakai transaksi.
                BatalkanTerapanAsync(terapan, dibuat);

                return PrescriptionReservationResult.Failure(
                    PrescriptionReservationErrorCodes.InsufficientStock,
                    $"Stok {butir.DrugName} tidak mencukupi di depo pelayanan. " +
                    $"Kurang {Trim(sisa)}. Pilih obat atau jumlah pengganti.");
            }
        }

        return PrescriptionReservationResult.Reserved(dibuat, storageLocationId, expiresAt);
    }

    /// <summary>
    /// Memastikan resep masih memegang penahanan aktif, dipakai Billing sebelum melanjutkan
    /// (<c>PHA-DEC-066</c> butir 2) dan Farmasi sebelum memproses (butir 3).
    /// </summary>
    public async Task<PrescriptionReservationResult> EnsureActiveAsync(
        Guid prescriptionId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var baris = await _dbContext.Set<PhmPrescriptionStockReservation>()
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var aktif = baris
            .Where(x => x.State == PrescriptionReservationState.Reserved)
            .ToList();

        if (aktif.Count == 0)
        {
            var pernahKedaluwarsa = baris.Any(x => x.State == PrescriptionReservationState.Expired);

            return pernahKedaluwarsa
                ? PrescriptionReservationResult.Failure(
                    PrescriptionReservationErrorCodes.ReservationExpired,
                    "Penahanan stok resep ini sudah kedaluwarsa. Stok harus divalidasi dan " +
                    "ditahan ulang sebelum resep dapat dilanjutkan.")
                : PrescriptionReservationResult.Failure(
                    PrescriptionReservationErrorCodes.ReservationUnavailable,
                    "Resep ini tidak memegang penahanan stok.");
        }

        // Baris yang batas waktunya sudah lewat tidak dianggap aktif walaupun penyapu belum
        // berjalan. Tanpa ini, pembacaan tepat sebelum penyapu akan memberi jawaban yang
        // berbeda dari pembacaan sesudahnya untuk keadaan yang sama.
        if (aktif.Any(x => x.ExpiresAt.HasValue && x.ExpiresAt.Value <= nowUtc))
            return PrescriptionReservationResult.Failure(
                PrescriptionReservationErrorCodes.ReservationExpired,
                "Penahanan stok resep ini sudah melewati batas waktu sebelum pembayaran.");

        return PrescriptionReservationResult.Reserved(
            aktif, aktif[0].StorageLocationId, aktif[0].ExpiresAt);
    }

    /// <summary>
    /// Menghapus batas waktu setelah pembayaran/jaminan dinyatakan valid
    /// (<c>PHA-DEC-066</c> butir 7). Idempoten.
    /// </summary>
    public async Task<PrescriptionReservationResult> ConfirmFinanciallyClearedAsync(
        Guid prescriptionId,
        Guid actorUserId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var keadaan = await EnsureActiveAsync(prescriptionId, nowUtc, cancellationToken);
        if (!keadaan.IsSuccess) return keadaan;

        foreach (var baris in keadaan.Reservations.Where(x => x.ExpiresAt.HasValue))
        {
            baris.ExpiresAt = null;
            baris.UpdateDateTime = nowUtc;
            baris.UpdateBy = actorUserId;
        }

        return PrescriptionReservationResult.Reserved(
            keadaan.Reservations, keadaan.StorageLocationId, null);
    }

    /// <summary>
    /// Mengonsumsi penahanan ketika obat benar-benar diserahkan
    /// (<c>PHA-DEC-066</c> butir 4). Idempoten: penyerahan yang diulang tidak mengonsumsi dua
    /// kali, karena baris yang sudah <c>Consumed</c> tidak lagi berkeadaan <c>Reserved</c>.
    /// </summary>
    public async Task<PrescriptionReservationResult> ConsumeAsync(
        Guid prescriptionId,
        Guid actorUserId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var baris = await _dbContext.Set<PhmPrescriptionStockReservation>()
            .Where(x => x.PrescriptionId == prescriptionId
                        && x.State == PrescriptionReservationState.Reserved
                        && !x.IsDelete)
            .ToListAsync(cancellationToken);

        if (baris.Count == 0)
            return PrescriptionReservationResult.Failure(
                PrescriptionReservationErrorCodes.ReservationUnavailable,
                "Tidak ada penahanan stok aktif yang dapat dikonsumsi.");

        foreach (var item in baris)
        {
            item.State = PrescriptionReservationState.Consumed;
            item.ConsumedAt = nowUtc;
            item.ActorUserId = actorUserId;
            item.UpdateDateTime = nowUtc;
            item.UpdateBy = actorUserId;
        }

        return PrescriptionReservationResult.Reserved(baris, baris[0].StorageLocationId, null);
    }

    /// <summary>
    /// Melepas penahanan oleh sistem (<c>PHA-DEC-066</c> butir 5, <c>PHA-DEC-023</c>).
    /// </summary>
    /// <remarks>
    /// Idempoten, dan sengaja <b>tidak</b> mengembalikan kegagalan ketika tidak ada yang dapat
    /// dilepas: pembatalan resep yang penahanannya sudah lepas tetap pembatalan yang sah.
    /// </remarks>
    public async Task<int> ReleaseAsync(
        Guid prescriptionId,
        string releaseReasonCode,
        Guid? actorUserId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var baris = await _dbContext.Set<PhmPrescriptionStockReservation>()
            .Where(x => x.PrescriptionId == prescriptionId
                        && x.State == PrescriptionReservationState.Reserved
                        && !x.IsDelete)
            .ToListAsync(cancellationToken);

        if (baris.Count == 0) return 0;

        var kedaluwarsa = releaseReasonCode == PrescriptionReservationReleaseReasons.ExpiredBeforePayment;

        foreach (var item in baris)
        {
            await KembalikanSaldoAsync(item, nowUtc, actorUserId, cancellationToken);

            item.State = kedaluwarsa
                ? PrescriptionReservationState.Expired
                : PrescriptionReservationState.Released;
            item.ReleasedAt = nowUtc;
            item.ReleaseReasonCode = releaseReasonCode;
            item.ActorUserId = actorUserId;
            item.UpdateDateTime = nowUtc;
            item.UpdateBy = actorUserId ?? Guid.Empty;
        }

        return baris.Count;
    }

    /// <summary>
    /// Melepas seluruh penahanan rawat jalan yang melewati batas waktu sebelum pembayaran
    /// (<c>PHA-DEC-066</c> butir 6).
    /// </summary>
    /// <remarks>
    /// Penahanan yang batas waktunya sudah dihapus — yaitu yang pembayarannya sudah valid, dan
    /// seluruh rawat inap serta IGD — tidak pernah tersentuh, karena <c>ExpiresAt</c>-nya
    /// <c>null</c>.
    /// </remarks>
    public async Task<int> ReleaseExpiredAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var prescriptionIds = await _dbContext.Set<PhmPrescriptionStockReservation>()
            .Where(x => x.State == PrescriptionReservationState.Reserved
                        && x.ExpiresAt != null
                        && x.ExpiresAt <= nowUtc
                        && !x.IsDelete)
            .Select(x => x.PrescriptionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var total = 0;

        foreach (var id in prescriptionIds)
        {
            total += await ReleaseAsync(
                id,
                PrescriptionReservationReleaseReasons.ExpiredBeforePayment,
                null,
                nowUtc,
                cancellationToken);
        }

        return total;
    }

    /// <summary>
    /// Keadaan penahanan sebuah resep untuk layar Farmasi (<c>PHA-FE-001</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hanya membaca. Tidak ada satu pun jalur di sini yang mengubah penahanan, karena layar
    /// Farmasi tidak memegang kewenangan itu: penahanan dibuat dokter saat finalisasi
    /// (<c>PHA-DEC-066</c> butir 1) dan dilepas sistem (butir 5).
    /// </para>
    /// <para>
    /// <c>CanProceed</c> dan <c>BlockReason</c> dihitung di sini, bukan di layar, supaya satu
    /// aturan yang sama berlaku bagi setiap pembaca. Ketika alur normal, keduanya kosong dan
    /// layar tidak memunculkan peringatan apa pun.
    /// </para>
    /// </remarks>
    public async Task<PrescriptionStockReservationStateResponse> DescribeAsync(
        Guid prescriptionId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var baris = await _dbContext.Set<PhmPrescriptionStockReservation>()
            .AsNoTracking()
            .Include(x => x.Drug)
            .Include(x => x.DrugBatch)
            .Include(x => x.StorageLocation)
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete)
            .OrderBy(x => x.ReservedAt)
            .ToListAsync(cancellationToken);

        var response = new PrescriptionStockReservationStateResponse
        {
            PrescriptionId = prescriptionId
        };

        if (baris.Count == 0)
        {
            response.State = "NOT_RESERVED";
            response.BlockReasonCode = PrescriptionReservationErrorCodes.ReservationUnavailable;
            response.BlockReason = "Resep ini tidak memegang penahanan stok.";
            return response;
        }

        var aktif = baris.Where(x => x.State == PrescriptionReservationState.Reserved).ToList();
        var acuan = aktif.Count > 0 ? aktif[0] : baris[^1];

        response.StorageLocationId = acuan.StorageLocationId;
        response.StorageLocationName = acuan.StorageLocation?.StorageLocationName ?? string.Empty;
        response.ReservedAt = acuan.ReservedAt;
        response.ExpiresAt = acuan.ExpiresAt;
        response.ConsumedAt = baris.Max(x => x.ConsumedAt);
        response.ReleasedAt = baris.Max(x => x.ReleasedAt);
        response.ReleaseReasonCode = acuan.ReleaseReasonCode ?? string.Empty;
        response.ReservedQuantity = aktif.Sum(x => x.Quantity);

        response.Lines = baris.Select(x => new PrescriptionStockReservationLineResponse
        {
            DrugId = x.DrugId,
            DrugName = x.Drug?.DrugName ?? string.Empty,
            DrugBatchId = x.DrugBatchId,
            BatchNumber = x.DrugBatch?.BatchNumber ?? string.Empty,
            Quantity = x.Quantity,
            State = x.State,
            StateLabel = StateLabel(x.State)
        }).ToList();

        if (aktif.Count == 0)
        {
            var kedaluwarsa = baris.Any(x => x.State == PrescriptionReservationState.Expired);
            var dikonsumsi = baris.All(x => x.State == PrescriptionReservationState.Consumed);

            response.State = dikonsumsi ? "CONSUMED" : kedaluwarsa ? "EXPIRED" : "RELEASED";

            // Penahanan yang sudah dikonsumsi bukan penghalang: obatnya memang sudah
            // diserahkan. Yang menghalangi hanya pelepasan dan kedaluwarsa.
            if (!dikonsumsi)
            {
                response.BlockReasonCode = kedaluwarsa
                    ? PrescriptionReservationErrorCodes.ReservationExpired
                    : PrescriptionReservationErrorCodes.ReservationUnavailable;
                response.BlockReason = kedaluwarsa
                    ? "Penahanan stok resep ini sudah kedaluwarsa sebelum pembayaran. " +
                      "Stok harus divalidasi dan ditahan ulang."
                    : "Penahanan stok resep ini sudah dilepas.";
            }

            return response;
        }

        response.State = "RESERVED";

        if (aktif.Any(x => x.ExpiresAt.HasValue && x.ExpiresAt.Value <= nowUtc))
        {
            response.BlockReasonCode = PrescriptionReservationErrorCodes.ReservationExpired;
            response.BlockReason =
                "Penahanan stok resep ini sudah melewati batas waktu sebelum pembayaran.";
            return response;
        }

        response.CanProceed = true;
        return response;
    }

    private static string StateLabel(PrescriptionReservationState state) => state switch
    {
        PrescriptionReservationState.Reserved => "Ditahan",
        PrescriptionReservationState.Consumed => "Sudah diserahkan",
        PrescriptionReservationState.Released => "Dilepas",
        PrescriptionReservationState.Expired => "Kedaluwarsa",
        _ => "Tidak diketahui"
    };

    // ───────────────────────────────────────────────────────────────── pembantu

    /// <summary>
    /// Membatalkan kenaikan penahanan yang sudah diterapkan dalam pemanggilan yang gagal.
    /// </summary>
    /// <remarks>
    /// Baris penahanan yang sudah ditambahkan dikeluarkan dari pelacak EF, bukan ditandai
    /// terhapus: ia belum pernah tersimpan, sehingga tidak ada apa pun yang perlu disisakan
    /// sebagai histori.
    /// </remarks>
    private void BatalkanTerapanAsync(
        List<(PhmDrugStockBalance Balance, decimal Quantity)> terapan,
        List<PhmPrescriptionStockReservation> dibuat)
    {
        foreach (var (balance, quantity) in terapan)
        {
            balance.QuantityReserved -= quantity;
            balance.Version--;
        }

        foreach (var baris in dibuat)
        {
            _dbContext.Entry(baris).State = EntityState.Detached;
        }

        terapan.Clear();
        dibuat.Clear();
    }

    /// <summary>
    /// Mengembalikan jumlah yang ditahan ke saldo. Dipakai pelepasan dan kedaluwarsa, tetapi
    /// <b>tidak</b> oleh konsumsi — pada konsumsi, penahanan berubah menjadi pengurangan stok
    /// fisik yang dikerjakan jalur penyerahan, bukan dikembalikan.
    /// </summary>
    private async Task KembalikanSaldoAsync(
        PhmPrescriptionStockReservation item,
        DateTime nowUtc,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        var balance = await _dbContext.Set<PhmDrugStockBalance>()
            .FirstOrDefaultAsync(x => x.DrugBatchId == item.DrugBatchId
                                      && x.StorageLocationId == item.StorageLocationId
                                      && x.Status == DrugStockStatus.Available
                                      && !x.IsDelete, cancellationToken);

        if (balance == null) return;

        // Tidak pernah membuat `QuantityReserved` negatif: bila saldonya sudah lebih kecil —
        // misalnya karena koreksi stok oleh gudang — yang dikembalikan hanya sebanyak yang
        // masih tercatat ditahan.
        var kembali = Math.Min(balance.QuantityReserved, item.Quantity);
        if (kembali <= 0) return;

        balance.QuantityReserved -= kembali;
        balance.Version++;
        balance.UpdateDateTime = nowUtc;
        balance.UpdateBy = actorUserId ?? Guid.Empty;
    }

    /// <summary>
    /// Batas waktu penahanan. <c>null</c> untuk rawat inap dan IGD
    /// (<c>PHA-DEC-066</c> butir 8).
    /// </summary>
    private async Task<DateTime?> ResolveExpiryAsync(
        PhmPrescription prescription,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        if (prescription.InpEpisodeId.HasValue) return null;

        var encounterType = await _dbContext.Set<RegPatientEncounter>()
            .AsNoTracking()
            .Where(x => x.Id == prescription.EncounterId)
            .Select(x => (EncounterType?)x.EncounterType)
            .FirstOrDefaultAsync(cancellationToken);

        if (encounterType is EncounterType.Inpatient or EncounterType.Emergency) return null;

        return nowUtc.Add(OutpatientTimeToLive);
    }

    /// <summary>Kebutuhan per obat: butir biasa dan bahan racikan dijumlahkan.</summary>
    private async Task<List<ReservationRequirement>> LoadRequirementsAsync(
        Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var butir = await _dbContext.Set<PhmPrescriptionItem>()
            .AsNoTracking()
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete && !x.IsCancel && x.IsActive)
            .Select(x => new ReservationRequirement
            {
                PrescriptionItemId = x.Id,
                DrugId = x.DrugId,
                DrugName = x.DrugNameSnapshot,
                Quantity = x.Quantity
            })
            .ToListAsync(cancellationToken);

        var racikan = await _dbContext.Set<PhmPrescriptionCompoundItem>()
            .AsNoTracking()
            .Where(x => x.PrescriptionCompound != null
                        && x.PrescriptionCompound.PrescriptionId == prescriptionId
                        && !x.IsDelete && !x.IsCancel && x.IsActive)
            .Select(x => new ReservationRequirement
            {
                PrescriptionCompoundItemId = x.Id,
                DrugId = x.DrugId,
                DrugName = x.DrugNameSnapshot,
                Quantity = x.TotalQuantity
            })
            .ToListAsync(cancellationToken);

        return butir.Concat(racikan).Where(x => x.Quantity > 0).ToList();
    }

    /// <summary>
    /// Saldo kandidat berurut FEFO, mengikuti urutan yang sama dengan
    /// <c>DrugStockService.LoadFefoCandidatesAsync</c>: batch tanpa tanggal kedaluwarsa paling
    /// akhir, sisanya dari yang paling dekat kedaluwarsa.
    /// </summary>
    private async Task<List<PhmDrugStockBalance>> LoadFefoBalancesAsync(
        Guid drugId,
        Guid storageLocationId,
        CancellationToken cancellationToken)
        => await _dbContext.Set<PhmDrugStockBalance>()
            .Include(x => x.DrugBatch)
            .Where(x => x.DrugId == drugId
                        && x.StorageLocationId == storageLocationId
                        && x.Status == DrugStockStatus.Available
                        && !x.IsDelete)
            .OrderBy(x => x.DrugBatch!.ExpiryDate == null)
            .ThenBy(x => x.DrugBatch!.ExpiryDate)
            .ToListAsync(cancellationToken);

    private static string Trim(decimal nilai)
        => nilai.ToString(
            nilai == decimal.Truncate(nilai) ? "0" : "0.###",
            System.Globalization.CultureInfo.InvariantCulture);

    private sealed class ReservationRequirement
    {
        public Guid? PrescriptionItemId { get; init; }
        public Guid? PrescriptionCompoundItemId { get; init; }
        public Guid DrugId { get; init; }
        public string DrugName { get; init; } = string.Empty;
        public decimal Quantity { get; init; }
    }
}
