using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Consumer outbox Inventory: menerjemahkan pemakaian material operasi menjadi mutasi stok
/// Farmasi.
/// </summary>
/// <remarks>
/// <para>
/// Sebelum ini pemakaian material operasi hanya menulis baris outbox yang tidak pernah dibaca
/// siapa pun, sehingga obat yang habis di kamar operasi tetap tercatat sebagai stok yang ada di
/// Farmasi. Kelas inilah pembacanya.
/// </para>
/// <para>
/// Pembagian kepemilikannya tetap: <b>Operasi mencatat pemakaian, Farmasi memiliki stok</b>.
/// Kelas ini tidak pernah menyentuh saldo secara langsung — seluruh perubahan angka melewati
/// <see cref="DrugStockService"/>, satu-satunya pihak yang berwenang menulis kartu stok. Yang
/// dikerjakan di sini hanyalah menentukan depo mana dan batch mana, lalu mencatat hasilnya
/// kembali ke outbox.
/// </para>
/// <para>
/// Kamar operasi bukan depo bersaldo mandiri. Depo sumbernya ditentukan
/// <see cref="OprStockSource"/>, bukan konstanta di dalam kode, sehingga rumah
/// sakit yang kelak membuka Depo OK cukup mengubah satu baris pemetaan.
/// </para>
/// <para>
/// Kegagalan tidak dilempar keluar per pesan. Satu pemakaian yang tidak dapat dibukukan —
/// pemetaannya belum ada, stoknya kurang, batchnya tidak dikenal — ditandai
/// <see cref="OprDeliveryStatus.Failed"/> berikut kode sebabnya, dan pesan lain tetap diproses.
/// Kegagalan yang menghentikan seluruh antrean hanya akan menyembunyikan sisanya.
/// </para>
/// </remarks>
public sealed class OperatingRoomInventoryDispatchService
{
    /// <summary>Batas jumlah pesan per pemanggilan, supaya satu permintaan tidak berjalan tanpa ujung.</summary>
    private const int MaxBatchSize = 200;

    private const string DispatchAction = "InventoryDispatch";
    private const string PayloadPrefix = "OprMaterialUsage/";

    private readonly ApplicationDbContext _dbContext;
    private readonly DrugStockService _drugStockService;
    private readonly DrugUnitConversionResolver _unitResolver;
    private readonly DrugReturnService _drugReturnService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    public OperatingRoomInventoryDispatchService(ApplicationDbContext dbContext,
        DrugStockService drugStockService, DrugUnitConversionResolver unitResolver,
        DrugReturnService drugReturnService, IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _drugStockService = drugStockService;
        _unitResolver = unitResolver;
        _drugReturnService = drugReturnService;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Memproses pesan Inventory yang masih tertunda untuk satu kasus operasi.
    /// </summary>
    /// <remarks>
    /// Pesan yang sudah <see cref="OprDeliveryStatus.Accepted"/> tidak pernah diproses lagi;
    /// itulah yang menjaga pemanggilan berulang tidak menggandakan pemotongan stok. Pesan
    /// <see cref="OprDeliveryStatus.Failed"/> baru ikut setelah diantrekan ulang secara sadar.
    /// </remarks>
    public async Task<OprInventoryDispatchResponse> DispatchCaseAsync(Guid caseId,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = GetUserId(_httpContextAccessor);

        var caseInfo = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new { x.Id, x.CaseNumber, x.Status })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");

        var pending = await _dbContext.OprIntegrationDeliveries
            .Where(x => x.OprCaseId == caseId && !x.IsDelete &&
                x.Destination == OperatingRoomIntegrationService.InventoryDestination &&
                x.Status == OprDeliveryStatus.Pending)
            .OrderBy(x => x.CreateDateTime).ThenBy(x => x.IdempotencyKey)
            .Take(MaxBatchSize)
            .ToListAsync(cancellationToken);

        var results = new List<OprInventoryDispatchResultResponse>();
        var now = DateTime.UtcNow;

        foreach (var delivery in pending)
        {
            var result = new OprInventoryDispatchResultResponse
            {
                DeliveryId = delivery.Id,
                IdempotencyKey = delivery.IdempotencyKey
            };

            try
            {
                var reference = await PostToLedgerAsync(delivery, actorUserId, cancellationToken);
                delivery.Status = OprDeliveryStatus.Accepted;
                delivery.AcceptedReference = reference;
                delivery.LastErrorCode = null;
                result.Accepted = true;
                result.AcceptedReference = reference;
            }
            catch (Exception exception) when (exception is DrugStockUnprocessableException
                or DrugStockConflictException or OperatingRoomUnprocessableException
                or DrugReturnUnprocessableException or DrugReturnConflictException
                or KeyNotFoundException)
            {
                // Perubahan saldo apa pun yang sempat tercatat untuk pesan ini dibuang, supaya
                // pesan yang gagal tidak meninggalkan potongan stok separuh jalan.
                DiscardPendingStockChanges();
                delivery.Status = OprDeliveryStatus.Failed;
                delivery.LastErrorCode = ErrorCodeOf(exception);
                result.Accepted = false;
                result.ErrorCode = delivery.LastErrorCode;
                result.ErrorMessage = exception.Message;
            }

            delivery.RetryCount++;
            delivery.LastAttemptAt = now;
            delivery.UpdateDateTime = now;
            delivery.UpdateBy = actorUserId;

            _dbContext.OprStatusHistories.Add(NewHistory(caseId, caseInfo.Status, caseInfo.Status,
                DispatchAction, $"{delivery.Destination}:{delivery.Status}",
                $"{delivery.Id:N}:{delivery.RetryCount}",
                Hash($"{delivery.Id}:{delivery.RetryCount}"), actorUserId, now));

            // Disimpan per pesan, bukan sekali di akhir. Satu `SaveChanges` adalah satu
            // transaksi, sehingga mutasi stok sebuah pesan dan penandaan hasilnya selalu
            // masuk bersama-sama — atau tidak sama sekali.
            await _dbContext.SaveChangesAsync(cancellationToken);

            results.Add(result);
        }

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomInventory.Dispatch",
            "Membukukan pemakaian material operasi ke kartu stok Farmasi.",
            new
            {
                OprCaseId = caseId, caseInfo.CaseNumber, ActorUserId = actorUserId,
                Processed = results.Count,
                Accepted = results.Count(x => x.Accepted),
                Failed = results.Count(x => !x.Accepted)
            });

        return new OprInventoryDispatchResponse
        {
            OprCaseId = caseId,
            CaseNumber = caseInfo.CaseNumber,
            ProcessedCount = results.Count,
            AcceptedCount = results.Count(x => x.Accepted),
            FailedCount = results.Count(x => !x.Accepted),
            Results = results
        };
    }

    /// <summary>
    /// Membukukan satu pesan ke kartu stok dan mengembalikan rujukan mutasinya.
    /// </summary>
    private async Task<string> PostToLedgerAsync(OprIntegrationDelivery delivery, Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var usage = await LoadUsageAsync(delivery, cancellationToken);
        var locationId = await ResolveSourceLocationAsync(usage.OprCaseId, cancellationToken);
        var (direction, quantity) = await ResolveMovementAsync(usage, cancellationToken);

        if (quantity == 0)
            return "NO-OP:quantity-unchanged";

        var reason = BuildReason(usage);
        var mutationIds = new List<Guid>();

        if (direction < 0)
        {
            foreach (var (batchId, batchQuantity) in await PlanIssueAsync(usage, locationId,
                quantity, cancellationToken))
            {
                var mutation = await _drugStockService.IssueBatchAsync(batchId, locationId,
                    batchQuantity, consumeReservation: false,
                    DrugStockSourceDocumentTypes.DrugUsage, usage.Id, reason,
                    $"opr-{usage.Id:N}-{mutationIds.Count + 1}", cancellationToken);
                mutationIds.Add(mutation.Id);
            }
        }
        else
        {
            // Barang yang kembali dari kamar operasi tidak dibukukan ke stok di sini sama
            // sekali. Ia diserahkan ke alur Retur Obat Farmasi sebagai draft, dan stoknya baru
            // bertambah setelah apoteker memeriksa kemasan, batch, kedaluwarsa, serta rantai
            // penyimpanannya — pemeriksaan yang sama dengan retur dari bangsal, sehingga tidak
            // ada aturan kedua yang harus dijaga tetap sejalan.
            return await RaiseReturnAsync(usage, locationId, quantity, cancellationToken);
        }

        return string.Join(',', mutationIds.Select(x => x.ToString("N")));
    }

    /// <summary>
    /// Menyerahkan barang yang kembali ke alur Retur Obat Farmasi sebagai draft.
    /// </summary>
    /// <remarks>
    /// Kunci idempotency-nya diturunkan dari catatan pemakaian, dan `CreateAsync` memakai kunci
    /// itu sebagai identitas dokumen, sehingga pembukuan yang diulang menemukan retur yang sama
    /// alih-alih membuat yang kedua.
    /// </remarks>
    private async Task<string> RaiseReturnAsync(OprMaterialUsage usage, Guid locationId,
        decimal quantity, CancellationToken cancellationToken)
    {
        var caseInfo = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == usage.OprCaseId)
            .Select(x => new { x.EncounterId, x.CaseNumber })
            .FirstAsync(cancellationToken);

        // Pengembali dicatat sebagai tenaga, bukan akun pengguna: itulah yang dipakai dokumen
        // retur Farmasi, dan itulah yang bermakna bagi apoteker yang memeriksanya.
        var workforceId = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == usage.RecordedBy)
            .Select(x => x.WorkforceProfileId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new OperatingRoomUnprocessableException("OPR-INV-013",
                "Pencatat pemakaian tidak terhubung ke data tenaga, sehingga returnya tidak " +
                "dapat diajukan atas namanya.");

        var batchId = await ResolveSingleBatchAsync(usage, locationId, cancellationToken);

        // Jumlahnya sudah diterjemahkan ke satuan stok, jadi satuan yang dicantumkan pada
        // dokumen retur harus satuan stok pula — bukan satuan yang dicatat petugas.
        var stockUnitId = await _dbContext.Set<MstDrug>().AsNoTracking()
            .Where(x => x.Id == usage.ExternalItemId && !x.IsDelete)
            .Select(x => x.StockUnitMeasurementId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new OperatingRoomUnprocessableException("PHM091",
                "Obat ini belum memiliki satuan stok pada master farmasi.");

        var detail = await _drugReturnService.CreateAsync(new CreateDrugReturnRequest
        {
            EncounterId = caseInfo.EncounterId,
            StorageLocationId = locationId,
            ReturnedByWorkforceId = workforceId,
            SourceOprMaterialUsageId = usage.Id,
            ReturnedAt = usage.OccurredAt,
            Reason = $"Sisa material operasi {caseInfo.CaseNumber}.",
            Items =
            [
                new DrugReturnItemInput
                {
                    DrugId = usage.ExternalItemId,
                    DrugBatchId = batchId,
                    MeasurementId = stockUnitId,
                    Quantity = quantity,
                    Note = BuildReason(usage)
                }
            ],
            IdempotencyKey = $"opr-return-{usage.Id:N}-{usage.Revision}"
        }, cancellationToken);

        return $"DrugReturn/{detail.ReturnNumber}";
    }

    private async Task<OprMaterialUsage> LoadUsageAsync(OprIntegrationDelivery delivery,
        CancellationToken cancellationToken)
    {
        if (!delivery.PayloadReference.StartsWith(PayloadPrefix, StringComparison.Ordinal) ||
            !Guid.TryParse(delivery.PayloadReference[PayloadPrefix.Length..], out var usageId))
            throw new OperatingRoomUnprocessableException("OPR-INV-001",
                "Rujukan pesan tidak menunjuk catatan pemakaian material.");

        return await _dbContext.OprMaterialUsages.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == usageId && !x.IsDelete, cancellationToken)
            ?? throw new OperatingRoomUnprocessableException("OPR-INV-002",
                "Catatan pemakaian material yang dirujuk tidak ditemukan.");
    }

    /// <summary>
    /// Menentukan depo farmasi yang stoknya dipakai kasus ini.
    /// </summary>
    /// <remarks>
    /// Depo ditentukan dari kamar pada jadwal yang sedang berlaku. Kasus tanpa jadwal berarti
    /// belum ada kamar, dan tanpa kamar tidak ada dasar untuk memilih depo — itu ditolak,
    /// bukan ditebak.
    /// </remarks>
    private async Task<Guid> ResolveSourceLocationAsync(Guid caseId, CancellationToken cancellationToken)
    {
        var roomId = await _dbContext.OprSchedules.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && x.IsCurrent && !x.IsDelete)
            .Select(x => (Guid?)x.RoomId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new OperatingRoomUnprocessableException("OPR-INV-003",
                "Kasus belum memiliki jadwal berlaku, sehingga kamar operasinya belum diketahui.");

        var locationId = await _dbContext.OprStockSources.AsNoTracking()
            .Where(x => x.RoomId == roomId && x.IsActive && !x.IsDelete)
            .Select(x => (Guid?)x.StorageLocationId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new OperatingRoomUnprocessableException("OPR-INV-004",
                "Kamar operasi ini belum dipetakan ke depo farmasi mana pun. " +
                "Tetapkan pemetaannya lebih dahulu.");

        return locationId;
    }

    /// <summary>
    /// Menentukan arah dan besar perubahan stok untuk satu catatan pemakaian.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Used</c> dan <c>Wasted</c> sama-sama mengurangi stok: barangnya habis, yang berbeda
    /// hanya sebabnya, dan sebab itu tercatat pada alasan mutasi. <c>Returned</c> menambah.
    /// </para>
    /// <para>
    /// <c>Corrected</c> menyatakan jumlah yang benar bagi catatan yang dikoreksinya, sehingga
    /// yang dibukukan adalah selisihnya saja terhadap catatan itu — bukan jumlah penuh, yang
    /// akan membukukan barang yang sama dua kali. Arahnya mengikuti arah catatan asal.
    /// </para>
    /// </remarks>
    private async Task<(int Direction, decimal Quantity)> ResolveMovementAsync(OprMaterialUsage usage,
        CancellationToken cancellationToken)
    {
        // Seluruh perhitungan di bawah memakai satuan stok. Satuan yang dicatat petugas boleh
        // berbeda — box, ampul, vial — dan diterjemahkan lebih dahulu; membandingkan angka
        // dalam dua satuan yang berbeda akan menghasilkan selisih yang tampak masuk akal.
        var quantity = await _unitResolver.ToStockQuantityAsync(usage.ExternalItemId,
            usage.UnitMeasurementId, usage.Quantity, cancellationToken);

        if (usage.Outcome != OprMaterialOutcome.Corrected)
            return (DirectionOf(usage.Outcome), quantity);

        if (!usage.CorrectionOfUsageId.HasValue)
            throw new OperatingRoomUnprocessableException("OPR-INV-005",
                "Koreksi ini tidak menyebut catatan yang dikoreksi, sehingga selisih stoknya " +
                "tidak dapat dihitung.");

        var corrected = await _dbContext.OprMaterialUsages.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == usage.CorrectionOfUsageId.Value && !x.IsDelete,
                cancellationToken)
            ?? throw new OperatingRoomUnprocessableException("OPR-INV-006",
                "Catatan pemakaian yang dikoreksi tidak ditemukan.");

        var correctedQuantity = await _unitResolver.ToStockQuantityAsync(corrected.ExternalItemId,
            corrected.UnitMeasurementId, corrected.Quantity, cancellationToken);

        var direction = DirectionOf(corrected.Outcome);
        var delta = quantity - correctedQuantity;

        // Selisih negatif membalik arahnya: koreksi yang mengurangi jumlah terpakai berarti
        // barang dikembalikan ke depo, bukan diambil lagi.
        return delta >= 0 ? (direction, delta) : (-direction, -delta);
    }

    private static int DirectionOf(OprMaterialOutcome outcome) => outcome switch
    {
        OprMaterialOutcome.Used => -1,
        OprMaterialOutcome.Wasted => -1,
        OprMaterialOutcome.Returned => 1,
        _ => throw new OperatingRoomUnprocessableException("OPR-INV-007",
            "Jenis hasil pemakaian ini tidak memiliki arah perubahan stok yang ditetapkan.")
    };

    /// <summary>
    /// Menyusun rencana pengambilan: batch yang disebut petugas bila ada, FEFO bila tidak.
    /// </summary>
    /// <remarks>
    /// Nomor batch yang dicatat di kamar operasi lebih dipercaya daripada FEFO, karena ia
    /// menyebut barang yang benar-benar dipakai pada pasien itu. FEFO hanya dipakai ketika
    /// nomor batchnya memang tidak dicatat.
    /// </remarks>
    private async Task<List<(Guid BatchId, decimal Quantity)>> PlanIssueAsync(OprMaterialUsage usage,
        Guid locationId, decimal quantity, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(usage.BatchNumber))
            return [(await ResolveSingleBatchAsync(usage, locationId, cancellationToken), quantity)];

        var plan = await _drugStockService.PlanFefoAsync(usage.ExternalItemId, locationId,
            quantity, cancellationToken);

        if (plan.Count == 0)
            throw new OperatingRoomUnprocessableException("OPR-INV-008",
                "Tidak ada batch dengan stok yang cukup di depo sumber.");

        return [.. plan.Select(x => (x.DrugBatchId, x.Quantity))];
    }

    private async Task<Guid> ResolveSingleBatchAsync(OprMaterialUsage usage, Guid locationId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(usage.BatchNumber))
        {
            var batchNumber = usage.BatchNumber.Trim();
            return await _dbContext.PhmDrugBatches.AsNoTracking()
                .Where(x => x.DrugId == usage.ExternalItemId && x.BatchNumber == batchNumber && !x.IsDelete)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new OperatingRoomUnprocessableException("OPR-INV-009",
                    $"Batch {batchNumber} tidak dikenal untuk obat ini.");
        }

        // Tanpa nomor batch, barang yang kembali dibukukan ke batch yang paling dekat
        // kedaluwarsanya di depo itu — batch yang paling mungkin baru saja dikeluarkan.
        // Saldonya boleh nol: yang dicari adalah identitas batch, bukan ketersediaannya.
        return await _dbContext.PhmDrugStockBalances.AsNoTracking()
            .Where(x => x.DrugId == usage.ExternalItemId && x.StorageLocationId == locationId &&
                !x.IsDelete)
            .Join(_dbContext.PhmDrugBatches.AsNoTracking().Where(b => !b.IsDelete),
                balance => balance.DrugBatchId, batch => batch.Id,
                (balance, batch) => new { batch.Id, batch.ExpiryDate })
            .OrderBy(x => x.ExpiryDate == null).ThenBy(x => x.ExpiryDate)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new OperatingRoomUnprocessableException("OPR-INV-010",
                "Nomor batch tidak dicatat dan depo sumber tidak mengenal batch obat ini, " +
                "sehingga pengembalian tidak dapat dibukukan.");
    }

    private static string BuildReason(OprMaterialUsage usage) =>
        $"Operasi {usage.Outcome} {usage.Quantity:0.####} {usage.UnitCode}" +
        (usage.Revision > 1 ? $" (revisi {usage.Revision})" : string.Empty) +
        (string.IsNullOrWhiteSpace(usage.SerialNumber) ? string.Empty : $", serial {usage.SerialNumber}");

    private static string ErrorCodeOf(Exception exception) => exception switch
    {
        DrugStockUnprocessableException drug => drug.Code,
        DrugStockConflictException conflict => conflict.Code,
        OperatingRoomUnprocessableException opr => opr.Code,
        DrugReturnUnprocessableException ret => ret.Code,
        DrugReturnConflictException retConflict => retConflict.Code,
        KeyNotFoundException => "OPR-INV-002",
        _ => "UNKNOWN"
    };

    /// <summary>
    /// Membuang perubahan saldo yang belum tersimpan setelah satu pesan gagal.
    /// </summary>
    /// <remarks>
    /// Sebuah pemakaian dapat menyentuh beberapa batch. Bila batch keempat kehabisan stok,
    /// potongan pada tiga batch sebelumnya sudah ada di change tracker dan akan ikut tersimpan
    /// bersama pesan berikutnya yang berhasil. Pembuangan ini memastikan pesan yang gagal tidak
    /// meninggalkan potongan separuh jalan.
    /// </remarks>
    private void DiscardPendingStockChanges()
    {
        foreach (var entry in _dbContext.ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is not (PhmDrugStockBalance or PhmDrugStockMutation)) continue;

            if (entry.State == EntityState.Added)
            {
                entry.State = EntityState.Detached;
                continue;
            }

            // Mengembalikan nilai ke apa yang tersimpan; sekadar menandai `Unchanged` akan
            // menyembunyikan perubahannya, bukan membatalkannya.
            entry.Reload();
        }
    }
}
