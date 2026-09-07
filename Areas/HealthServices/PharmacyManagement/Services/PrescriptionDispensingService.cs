using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Penyerahan obat resep kepada pasien, dicatat per baris resep.
/// </summary>
/// <remarks>
/// <para>
/// Sebelum ini alur resep berhenti di <see cref="PrescriptionFulfillmentStatus.ReadyToDispense"/>.
/// Obat dinyatakan siap diserahkan, lalu tidak ada lagi yang tercatat: tidak ada berapa yang
/// benar-benar diberikan, tidak ada sisanya, dan stok Farmasi tidak pernah berkurang karenanya.
/// Service inilah yang menutup jarak itu.
/// </para>
/// <para>
/// <b>Tidak ada tabel penyerahan tersendiri.</b> Penyerahan dicatat sebagai
/// <see cref="TrxDrugUsage"/> yang menunjuk resepnya. Obat yang keluar dari stok untuk seorang
/// pasien hanya boleh punya satu catatan yang authoritative; membuat tabel kedua akan membuat
/// pertanyaan "berapa yang sudah diserahkan" punya dua jawaban yang bisa berbeda, dan selisihnya
/// baru ketahuan saat stok opname.
/// </para>
/// <para>
/// Dua langkah, bukan satu. <see cref="PrepareAsync"/> menahan stok — barangnya belum berpindah,
/// tetapi sudah tidak dapat dijanjikan kepada pasien lain. <see cref="DispenseAsync"/> baru
/// mengurangi saldo fisik sambil melepas tahanan itu. Pemisahan ini yang membuat obat tidak
/// terjanjikan dua kali selama petugas menyiapkannya.
/// </para>
/// <para>
/// Jumlah sisa dan penanda <c>det</c>/<c>nedet</c> <b>tidak pernah disimpan</b>. Keduanya
/// dihitung ulang dari histori penyerahan setiap kali dibaca. Menyimpannya sebagai status akan
/// membuka kemungkinan penanda dan histori menyatakan dua hal yang berbeda — dan pada dokumen
/// yang dibawa pasien ke apotek lain, selisih itu berakibat nyata.
/// </para>
/// <para>
/// Seluruh perubahan saldo melewati <see cref="DrugStockService"/>. Service ini tidak pernah
/// menulis saldo sendiri.
/// </para>
/// </remarks>
public sealed class PrescriptionDispensingService
{
    private const string LogCategory = "PharmacyManagement";

    /// <summary>
    /// Keadaan pembayaran yang sudah membolehkan obat diserahkan.
    /// </summary>
    /// <remarks>
    /// Termasuk penjaminan yang disetujui dan pembayaran yang ditiadakan; keduanya sah sebagai
    /// dasar penyerahan meskipun tidak ada uang yang berpindah.
    /// </remarks>
    private static readonly PrescriptionPaymentStatus[] SettledPayments =
    [
        PrescriptionPaymentStatus.Paid,
        PrescriptionPaymentStatus.InsuranceApproved,
        PrescriptionPaymentStatus.PaymentWaived
    ];

    /// <summary>Tahap alur yang sudah boleh menyiapkan penyerahan.</summary>
    private static readonly PrescriptionFulfillmentStatus[] DispensableStages =
    [
        PrescriptionFulfillmentStatus.ReadyToDispense,
        PrescriptionFulfillmentStatus.PartiallyDispensed
    ];

    /// <summary>Status pemakaian yang berarti obatnya benar-benar sudah keluar.</summary>
    private static readonly DrugUsageStatus[] DispensedStatuses =
    [
        DrugUsageStatus.NotBilled,
        DrugUsageStatus.Billed
    ];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly DrugStockService _drugStockService;

    public PrescriptionDispensingService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        DrugStockService drugStockService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _drugStockService = drugStockService;
    }

    // ================================================================= rekapitulasi

    /// <summary>
    /// Berapa yang diresepkan, sudah diserahkan, dan tersisa untuk tiap baris resep.
    /// </summary>
    /// <remarks>
    /// Inilah bahan copy resep. Angkanya berasal dari penyerahan yang benar-benar tercatat,
    /// sehingga tidak mungkin menyatakan obat sudah diserahkan padahal stoknya utuh.
    /// </remarks>
    public async Task<PrescriptionDispensingSummaryResponse?> GetSummaryAsync(Guid prescriptionId,
        CancellationToken cancellationToken = default)
    {
        var header = await _dbContext.TrxPrescriptions.AsNoTracking()
            .Where(x => x.Id == prescriptionId && !x.IsDelete)
            .Select(x => new { x.Id, x.PrescriptionNumber, x.FulfillmentStatus })
            .FirstOrDefaultAsync(cancellationToken);
        if (header == null) return null;

        var items = await LoadPrescriptionItemsAsync(prescriptionId, cancellationToken);
        var usages = await LoadUsagesAsync(prescriptionId, cancellationToken);

        var lines = items.Select(item => BuildLine(item, usages)).ToList();

        return new PrescriptionDispensingSummaryResponse
        {
            PrescriptionId = header.Id,
            PrescriptionNumber = header.PrescriptionNumber,
            FulfillmentStatus = header.FulfillmentStatus,
            IsFullyDispensed = lines.Count > 0 && lines.All(x => x.QuantityRemaining <= 0m),
            Items = lines,
            History = [.. usages
                .OrderByDescending(x => x.RecordedAt ?? x.UsedAt)
                .Select(MapEvent)]
        };
    }

    // ================================================================== penyiapan

    /// <summary>
    /// Menyiapkan penyerahan: menahan stok tanpa memindahkannya.
    /// </summary>
    /// <remarks>
    /// Batch dipilih FEFO dan disimpan sebagai alokasi. Batch itulah yang nanti dikeluarkan,
    /// bukan hasil pemilihan ulang saat penyerahan — kalau dipilih ulang, barang yang ditahan
    /// dan barang yang keluar bisa berbeda, dan penelusuran batch ke pasien menjadi salah.
    /// </remarks>
    public async Task<PrescriptionDispensingSummaryResponse> PrepareAsync(Guid prescriptionId,
        PreparePrescriptionDispensingRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        var usageId = DeterministicId(request.IdempotencyKey);
        var duplicate = await _dbContext.TrxDrugUsages.AsNoTracking()
            .AnyAsync(x => x.Id == usageId && !x.IsDelete, cancellationToken);
        if (duplicate) return (await GetSummaryAsync(prescriptionId, cancellationToken))!;

        var prescription = await _dbContext.TrxPrescriptions
            .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Resep tidak ditemukan.");

        EnsureDispensable(prescription);
        await EnsureDispensingLocationAsync(request.StorageLocationId, cancellationToken);
        await EnsureWorkforceAsync(request.PreparedByWorkforceId, cancellationToken);

        if (request.Items.Count == 0)
            throw new PrescriptionDispensingUnprocessableException("PHM101",
                "Penyiapan harus memuat sekurang-kurangnya satu baris resep.");

        var items = await LoadPrescriptionItemsAsync(prescriptionId, cancellationToken);
        var usages = await LoadUsagesAsync(prescriptionId, cancellationToken);
        var byId = items.ToDictionary(x => x.Id);

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var usage = new TrxDrugUsage
        {
            Id = usageId,
            UsageNumber = $"DSP-{now:yyyyMMdd}-{usageId.ToString("N")[..6].ToUpperInvariant()}",
            EncounterId = prescription.EncounterId,
            PrescriptionId = prescription.Id,
            StorageLocationId = request.StorageLocationId,
            RecordedByWorkforceId = request.PreparedByWorkforceId,
            Status = DrugUsageStatus.Draft,
            UsedAt = now,
            Notes = Normalize(request.Notes),
            ItemCount = request.Items.Count,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        _dbContext.TrxDrugUsages.Add(usage);

        var lineNumber = 1;
        foreach (var input in request.Items)
        {
            if (!byId.TryGetValue(input.PrescriptionItemId, out var prescriptionItem))
                throw new PrescriptionDispensingUnprocessableException("PHM102",
                    "Baris resep yang diminta tidak ada pada resep ini.");

            if (input.Quantity <= 0m)
                throw new PrescriptionDispensingUnprocessableException("PHM103",
                    $"{prescriptionItem.DrugName}: jumlah penyerahan harus lebih dari nol.");

            var line = BuildLine(prescriptionItem, usages);

            // Yang boleh disiapkan adalah sisa dikurangi yang sedang ditahan draft lain.
            // Tanpa pengurangan itu, dua draft berturut-turut dapat menahan jumlah yang sama
            // dan penyerahan keduanya melampaui jumlah yang diresepkan.
            var allowed = line.QuantityRemaining - line.QuantityReserved;
            if (input.Quantity > allowed)
                throw new PrescriptionDispensingUnprocessableException("PHM104",
                    $"{prescriptionItem.DrugName}: jumlah melebihi sisa yang boleh diserahkan. " +
                    $"Sisa {Trim(line.QuantityRemaining)}, sedang disiapkan {Trim(line.QuantityReserved)}.");

            // Satuan penyerahan wajib ada. Baris resep tanpa satuan tidak dapat dibukukan ke
            // stok, dan menebak satuannya akan mengurangi saldo dengan angka yang salah.
            if (!prescriptionItem.MeasurementId.HasValue)
                throw new PrescriptionDispensingUnprocessableException("PHM115",
                    $"{prescriptionItem.DrugName}: baris resep ini belum memiliki satuan " +
                    "penyerahan, sehingga tidak dapat diserahkan.");

            var usageItem = new TrxDrugUsageItem
            {
                DrugUsageId = usage.Id,
                DrugId = prescriptionItem.DrugId,
                PrescriptionItemId = prescriptionItem.Id,
                MeasurementId = prescriptionItem.MeasurementId.Value,
                DrugCodeSnapshot = prescriptionItem.DrugCode,
                DrugNameSnapshot = prescriptionItem.DrugName,
                MeasurementNameSnapshot = prescriptionItem.MeasurementName,
                Quantity = input.Quantity,
                Note = Normalize(input.Note),
                LineNumber = lineNumber++,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            _dbContext.TrxDrugUsageItems.Add(usageItem);

            await ReserveAsync(usageItem, prescriptionItem.DrugName, request.StorageLocationId,
                input.Quantity, actorUserId, now, cancellationToken);
        }

        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "PrescriptionDispensing.Prepare",
            "Menahan stok untuk penyerahan resep.",
            new
            {
                PrescriptionId = prescriptionId, prescription.PrescriptionNumber,
                DrugUsageId = usage.Id, usage.UsageNumber, ActorUserId = actorUserId,
                usage.ItemCount
            });

        return (await GetSummaryAsync(prescriptionId, cancellationToken))!;
    }

    // ================================================================= penyerahan

    /// <summary>
    /// Menyerahkan obat: stok berkurang tepat satu kali, tahanannya dilepas.
    /// </summary>
    /// <remarks>
    /// Pemanggilan ulang atas penyerahan yang sudah tercatat mengembalikan keadaan apa adanya
    /// tanpa mengurangi stok lagi. Itulah yang membuat tombol "kirim ulang" aman ditekan
    /// petugas yang ragu apakah permintaannya sudah sampai.
    /// </remarks>
    public async Task<PrescriptionDispensingSummaryResponse> DispenseAsync(Guid prescriptionId,
        Guid drugUsageId, PrescriptionDispensingCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        var prescription = await _dbContext.TrxPrescriptions
            .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Resep tidak ditemukan.");

        var usage = await LoadDraftableUsageAsync(prescriptionId, drugUsageId, cancellationToken);

        // Sudah diserahkan sebelumnya: kembalikan apa adanya, jangan potong stok lagi.
        if (DispensedStatuses.Contains(usage.Status))
            return (await GetSummaryAsync(prescriptionId, cancellationToken))!;

        if (usage.Status != DrugUsageStatus.Draft)
            throw new PrescriptionDispensingConflictException("PHM105",
                "Hanya penyiapan berstatus draft yang dapat diserahkan.");

        EnsureVersion(usage.Version, request.ExpectedVersion);
        EnsureDispensable(prescription);

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var item in usage.Items.Where(x => !x.IsDelete).OrderBy(x => x.LineNumber))
        {
            foreach (var allocation in item.Allocations.Where(x => !x.IsDelete)
                .OrderBy(x => x.SequenceNumber))
            {
                try
                {
                    // consumeReservation: true — tahanan yang dibuat saat penyiapan dilepas
                    // pada saat yang sama saldo fisiknya berkurang, sehingga tidak ada
                    // keadaan antara yang menahan sekaligus sudah mengeluarkan.
                    await _drugStockService.IssueBatchAsync(allocation.DrugBatchId,
                        usage.StorageLocationId, allocation.Quantity, consumeReservation: true,
                        DrugStockSourceDocumentTypes.DrugUsage, usage.Id,
                        $"Penyerahan resep {prescription.PrescriptionNumber} baris {item.LineNumber}.",
                        $"dsp-{allocation.Id:N}", cancellationToken);
                }
                catch (DrugStockUnprocessableException ex)
                {
                    throw new PrescriptionDispensingUnprocessableException(ex.Code,
                        $"{item.DrugNameSnapshot}: {ex.Message}");
                }
            }
        }

        usage.Status = DrugUsageStatus.NotBilled;
        usage.RecordedAt = now;
        usage.Version++;
        usage.UpdateDateTime = now;
        usage.UpdateBy = actorUserId;

        await ApplyFulfillmentStatusAsync(prescription, usage.Id, actorUserId, now,
            cancellationToken);
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "PrescriptionDispensing.Dispense",
            "Menyerahkan obat resep dan mengurangi stok.",
            new
            {
                PrescriptionId = prescriptionId, prescription.PrescriptionNumber,
                DrugUsageId = usage.Id, usage.UsageNumber, ActorUserId = actorUserId,
                FulfillmentStatus = prescription.FulfillmentStatus.ToString()
            });

        return (await GetSummaryAsync(prescriptionId, cancellationToken))!;
    }

    // =================================================================== batal

    /// <summary>
    /// Membatalkan penyiapan dan melepas tahanan stoknya.
    /// </summary>
    /// <remarks>
    /// Hanya berlaku selama obat belum diserahkan. Penyerahan yang sudah tercatat tidak
    /// dibatalkan dari sini: stoknya sudah keluar, dan mengembalikannya adalah retur yang
    /// wajib melewati pemeriksaan apoteker.
    /// </remarks>
    public async Task<PrescriptionDispensingSummaryResponse> CancelAsync(Guid prescriptionId,
        Guid drugUsageId, CancelPrescriptionDispensingRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new PrescriptionDispensingUnprocessableException("PHM106",
                "Alasan pembatalan wajib diisi.");

        var usage = await LoadDraftableUsageAsync(prescriptionId, drugUsageId, cancellationToken);

        if (usage.Status == DrugUsageStatus.Cancelled)
            return (await GetSummaryAsync(prescriptionId, cancellationToken))!;

        if (usage.Status != DrugUsageStatus.Draft)
            throw new PrescriptionDispensingConflictException("PHM107",
                "Obat yang sudah diserahkan tidak dapat dibatalkan; gunakan retur obat.");

        EnsureVersion(usage.Version, request.ExpectedVersion);

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var item in usage.Items.Where(x => !x.IsDelete))
        {
            foreach (var allocation in item.Allocations.Where(x => !x.IsDelete))
            {
                await _drugStockService.ReleaseReservationAsync(allocation.DrugBatchId,
                    usage.StorageLocationId, allocation.Quantity, cancellationToken);
            }
        }

        usage.Status = DrugUsageStatus.Cancelled;
        usage.CancelReason = request.Reason.Trim();
        usage.Version++;
        usage.UpdateDateTime = now;
        usage.UpdateBy = actorUserId;

        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "PrescriptionDispensing.Cancel",
            "Membatalkan penyiapan penyerahan dan melepas tahanan stok.",
            new
            {
                PrescriptionId = prescriptionId, DrugUsageId = usage.Id, usage.UsageNumber,
                ActorUserId = actorUserId, Reason = usage.CancelReason
            });

        return (await GetSummaryAsync(prescriptionId, cancellationToken))!;
    }

    // ================================================================== internal

    /// <summary>Menahan stok satu baris memakai rencana FEFO, lalu mencatat alokasinya.</summary>
    private async Task ReserveAsync(TrxDrugUsageItem usageItem, string drugName,
        Guid storageLocationId, decimal quantity, Guid actorUserId, DateTime now,
        CancellationToken cancellationToken)
    {
        List<(Guid DrugBatchId, decimal Quantity)> plan;
        try
        {
            plan = await _drugStockService.PlanFefoAsync(usageItem.DrugId, storageLocationId,
                quantity, cancellationToken);
        }
        catch (DrugStockUnprocessableException ex)
        {
            throw new PrescriptionDispensingUnprocessableException(ex.Code, $"{drugName}: {ex.Message}");
        }

        var sequence = 1;
        foreach (var (batchId, batchQuantity) in plan)
        {
            try
            {
                await _drugStockService.ReserveBatchAsync(batchId, storageLocationId,
                    batchQuantity, cancellationToken);
            }
            catch (DrugStockUnprocessableException ex)
            {
                throw new PrescriptionDispensingUnprocessableException(ex.Code,
                    $"{drugName}: {ex.Message}");
            }

            _dbContext.TrxDrugUsageAllocations.Add(new TrxDrugUsageAllocation
            {
                DrugUsageItemId = usageItem.Id,
                DrugBatchId = batchId,
                SequenceNumber = sequence++,
                Quantity = batchQuantity,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
    }

    /// <summary>
    /// Menyesuaikan status pemenuhan resep dari sisa seluruh barisnya.
    /// </summary>
    /// <remarks>
    /// Statusnya diturunkan, bukan diminta pemanggil. Bila ia dapat diisi bebas, sebuah resep
    /// dapat dinyatakan selesai padahal masih ada baris yang belum diserahkan.
    /// </remarks>
    private async Task ApplyFulfillmentStatusAsync(TrxPrescription prescription,
        Guid justDispensedUsageId, Guid actorUserId, DateTime now,
        CancellationToken cancellationToken)
    {
        var items = await LoadPrescriptionItemsAsync(prescription.Id, cancellationToken);
        var usages = await LoadUsagesAsync(prescription.Id, cancellationToken);

        // Penyerahan yang baru saja terjadi belum tersimpan ketika status ini dihitung, jadi
        // pembacaan di atas masih melihatnya sebagai draft. Statusnya diperbaiki di sini
        // supaya resep yang sudah lunas seluruhnya tidak tertinggal sebagai "sebagian".
        foreach (var usage in usages.Where(x => x.Id == justDispensedUsageId))
        {
            usage.Status = DrugUsageStatus.NotBilled;
        }

        var lines = items.Select(item => BuildLine(item, usages)).ToList();

        var complete = lines.Count > 0 && lines.All(x => x.QuantityRemaining <= 0m);

        prescription.FulfillmentStatus = complete
            ? PrescriptionFulfillmentStatus.Dispensed
            : PrescriptionFulfillmentStatus.PartiallyDispensed;

        if (complete)
        {
            prescription.DispensedAt = now;
            prescription.DispensedByUserId = actorUserId;
        }

        prescription.UpdateDateTime = now;
        prescription.UpdateBy = actorUserId;
    }

    /// <summary>Menghitung satu baris rekapitulasi dari histori penyerahan.</summary>
    private static PrescriptionItemDispensingResponse BuildLine(PrescriptionItemView item,
        IReadOnlyCollection<UsageView> usages)
    {
        decimal SumWhere(Func<UsageView, bool> predicate) =>
            usages.Where(predicate)
                .SelectMany(x => x.Items)
                .Where(x => x.PrescriptionItemId == item.Id)
                .Sum(x => x.Quantity);

        var dispensed = SumWhere(x => DispensedStatuses.Contains(x.Status));
        var reserved = SumWhere(x => x.Status == DrugUsageStatus.Draft);

        // Tidak boleh negatif. Kalaupun data lama menyimpan penyerahan melebihi yang
        // diresepkan, sisa negatif tidak punya arti bagi pasien maupun bagi copy resep.
        var remaining = Math.Max(0m, item.Quantity - dispensed);

        return new PrescriptionItemDispensingResponse
        {
            PrescriptionItemId = item.Id,
            LineNumber = item.SortOrder,
            DrugId = item.DrugId,
            DrugName = item.DrugName,
            DispenseUnit = item.MeasurementName,
            QuantityPrescribed = item.Quantity,
            QuantityDispensed = dispensed,
            QuantityReserved = reserved,
            QuantityRemaining = remaining,
            Mark = remaining <= 0m
                ? PrescriptionItemDispensingMark.Det
                : PrescriptionItemDispensingMark.Nedet
        };
    }

    private static PrescriptionDispensingEventResponse MapEvent(UsageView usage) => new()
    {
        DrugUsageId = usage.Id,
        UsageNumber = usage.UsageNumber,
        Status = usage.Status,
        StorageLocationId = usage.StorageLocationId,
        StorageLocationName = usage.StorageLocationName,
        UsedAt = usage.UsedAt,
        RecordedAt = usage.RecordedAt,
        Version = usage.Version,
        Items = [.. usage.Items.OrderBy(x => x.LineNumber).Select(item =>
            new PrescriptionDispensingEventItemResponse
            {
                DrugUsageItemId = item.Id,
                PrescriptionItemId = item.PrescriptionItemId,
                DrugId = item.DrugId,
                DrugName = item.DrugName,
                Quantity = item.Quantity,
                Allocations = [.. item.Allocations.OrderBy(x => x.SequenceNumber).Select(a =>
                    new PrescriptionDispensingAllocationResponse
                    {
                        DrugBatchId = a.DrugBatchId,
                        BatchNumber = a.BatchNumber,
                        ExpiryDate = a.ExpiryDate,
                        Quantity = a.Quantity,
                        SequenceNumber = a.SequenceNumber
                    })]
            })]
    };

    private async Task<List<PrescriptionItemView>> LoadPrescriptionItemsAsync(Guid prescriptionId,
        CancellationToken cancellationToken) =>
        await _dbContext.TrxPrescriptionItems.AsNoTracking()
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete)
            .OrderBy(x => x.SortOrder)
            .Select(x => new PrescriptionItemView
            {
                Id = x.Id,
                DrugId = x.DrugId,
                DrugCode = x.DrugCodeSnapshot ?? string.Empty,
                DrugName = x.DrugNameSnapshot ?? string.Empty,
                MeasurementId = x.DispenseUnitMeasurementId,
                MeasurementName = x.DispenseUnitSymbolSnapshot ?? x.DispenseUnitNameSnapshot,
                Quantity = x.Quantity,
                SortOrder = x.SortOrder
            })
            .ToListAsync(cancellationToken);

    private async Task<List<UsageView>> LoadUsagesAsync(Guid prescriptionId,
        CancellationToken cancellationToken) =>
        await _dbContext.TrxDrugUsages.AsNoTracking()
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete)
            .Select(x => new UsageView
            {
                Id = x.Id,
                UsageNumber = x.UsageNumber,
                Status = x.Status,
                StorageLocationId = x.StorageLocationId,
                StorageLocationName = x.StorageLocation!.StorageLocationName,
                UsedAt = x.UsedAt,
                RecordedAt = x.RecordedAt,
                Version = x.Version,
                Items = x.Items.Where(i => !i.IsDelete).Select(i => new UsageItemView
                {
                    Id = i.Id,
                    PrescriptionItemId = i.PrescriptionItemId,
                    DrugId = i.DrugId,
                    DrugName = i.DrugNameSnapshot,
                    Quantity = i.Quantity,
                    LineNumber = i.LineNumber,
                    Allocations = i.Allocations.Where(a => !a.IsDelete).Select(a =>
                        new UsageAllocationView
                        {
                            DrugBatchId = a.DrugBatchId,
                            BatchNumber = a.DrugBatch!.BatchNumber,
                            ExpiryDate = a.DrugBatch!.ExpiryDate,
                            Quantity = a.Quantity,
                            SequenceNumber = a.SequenceNumber
                        }).ToList()
                }).ToList()
            })
            .ToListAsync(cancellationToken);

    private async Task<TrxDrugUsage> LoadDraftableUsageAsync(Guid prescriptionId, Guid drugUsageId,
        CancellationToken cancellationToken)
    {
        var usage = await _dbContext.TrxDrugUsages
            .Include(x => x.Items).ThenInclude(x => x.Allocations)
            .FirstOrDefaultAsync(x => x.Id == drugUsageId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Penyiapan penyerahan tidak ditemukan.");

        // Penyiapan milik resep lain tidak boleh diserahkan lewat resep ini.
        if (usage.PrescriptionId != prescriptionId)
            throw new PrescriptionDispensingUnprocessableException("PHM108",
                "Penyiapan tersebut bukan milik resep ini.");

        return usage;
    }

    private static void EnsureDispensable(TrxPrescription prescription)
    {
        if (prescription.PrescriptionStatus == PrescriptionStatus.Cancelled)
            throw new PrescriptionDispensingConflictException("PHM109",
                "Resep sudah dibatalkan.");

        if (!DispensableStages.Contains(prescription.FulfillmentStatus))
            throw new PrescriptionDispensingConflictException("PHM110",
                "Obat baru dapat diserahkan setelah penyiapan farmasi selesai.");

        if (!SettledPayments.Contains(prescription.PaymentStatus))
            throw new PrescriptionDispensingConflictException("PHM111",
                "Pembayaran atau penjaminan resep ini belum memenuhi syarat penyerahan.");
    }

    private async Task EnsureDispensingLocationAsync(Guid storageLocationId,
        CancellationToken cancellationToken)
    {
        var location = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == storageLocationId && !x.IsDelete, cancellationToken)
            ?? throw new PrescriptionDispensingUnprocessableException("PHM112",
                "Depo penyerahan tidak ditemukan.");

        if (!location.IsActive)
            throw new PrescriptionDispensingUnprocessableException("PHM112",
                $"Depo {location.StorageLocationName} sudah tidak aktif.");

        if (!location.IsAllowDispensing)
            throw new PrescriptionDispensingUnprocessableException("PHM112",
                $"Depo {location.StorageLocationName} tidak diizinkan menyerahkan obat.");
    }

    private async Task EnsureWorkforceAsync(Guid workforceId, CancellationToken cancellationToken)
    {
        var valid = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!valid)
            throw new PrescriptionDispensingUnprocessableException("PHM113",
                "Petugas penyerah tidak ditemukan atau tidak aktif.");
    }

    private static void EnsureVersion(int current, int expected)
    {
        if (current != expected)
            throw new PrescriptionDispensingConflictException("PHM114",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
    }

    private static void EnsureIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key wajib diisi.");
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new PrescriptionDispensingConflictException("PHM114",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new PrescriptionDispensingForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Trim(decimal value) =>
        value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"PrescriptionDispensing:{key.Trim()}"))[..16]);

    // -------------------------------------------------------------- bentuk baca

    private sealed class PrescriptionItemView
    {
        public Guid Id { get; init; }
        public Guid DrugId { get; init; }
        public string DrugCode { get; init; } = string.Empty;
        public string DrugName { get; init; } = string.Empty;
        public Guid? MeasurementId { get; init; }
        public string? MeasurementName { get; init; }
        public decimal Quantity { get; init; }
        public int SortOrder { get; init; }
    }

    private sealed class UsageView
    {
        public Guid Id { get; init; }
        public string UsageNumber { get; init; } = string.Empty;

        /// <summary>Dapat disetel ulang untuk memperhitungkan penyerahan yang belum tersimpan.</summary>
        public DrugUsageStatus Status { get; set; }
        public Guid StorageLocationId { get; init; }
        public string? StorageLocationName { get; init; }
        public DateTime UsedAt { get; init; }
        public DateTime? RecordedAt { get; init; }
        public int Version { get; init; }
        public List<UsageItemView> Items { get; init; } = [];
    }

    private sealed class UsageItemView
    {
        public Guid Id { get; init; }
        public Guid? PrescriptionItemId { get; init; }
        public Guid DrugId { get; init; }
        public string DrugName { get; init; } = string.Empty;
        public decimal Quantity { get; init; }
        public int LineNumber { get; init; }
        public List<UsageAllocationView> Allocations { get; init; } = [];
    }

    private sealed class UsageAllocationView
    {
        public Guid DrugBatchId { get; init; }
        public string? BatchNumber { get; init; }
        public DateOnly? ExpiryDate { get; init; }
        public decimal Quantity { get; init; }
        public int SequenceNumber { get; init; }
    }
}

public sealed class PrescriptionDispensingConflictException(string code, string message)
    : Exception(message)
{
    public string Code { get; } = code;
}

public sealed class PrescriptionDispensingForbiddenException(string message) : Exception(message);

public sealed class PrescriptionDispensingUnprocessableException(string code, string message)
    : Exception(message)
{
    public string Code { get; } = code;
}
