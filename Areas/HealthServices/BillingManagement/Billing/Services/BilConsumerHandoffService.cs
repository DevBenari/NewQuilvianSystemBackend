using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// BKC-DES-038: Satu service penerbit fakta ke konsumen hilir (Finance dan Farmasi).
/// Tidak pernah membuka transaksi sendiri dan selalu berjalan di dalam batas transaksi pemanggil,
/// sehingga uang dan suratnya tidak pernah terpisah nasib (BKC-DEC-106).
/// Memakai ulang kunci penasihat tagihan yang sudah ada (BKC-DES-032).
/// </summary>
public sealed class BilConsumerHandoffService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly IInpatientClearanceService _inpatientClearanceService;

    public BilConsumerHandoffService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        IInpatientClearanceService? inpatientClearanceService = null)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _inpatientClearanceService = inpatientClearanceService ?? new InpatientClearanceService(dbContext, loggerService);
    }

    /// <summary>
    /// Menerbitkan surat penerimaan uang (BilCollectionHandoff) untuk Finance saat tender mencapai
    /// terminal status (SUCCEEDED atau REVERSED).
    /// Mematuhi BIL-INT-013, BIL-VAL-110 (idempotensi), dan BIL-VAL-111 (shift kasir wajib untuk tunai).
    /// </summary>
    public async Task<BilCollectionHandoff?> PublishForTenderAsync(
        BilTender tender,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tender);

        if (tender.Status != BillingTenderStatuses.Succeeded
            && tender.Status != BillingTenderStatuses.Reversed)
        {
            return null;
        }

        var settlement = tender.Settlement
            ?? await _dbContext.BilSettlements.SingleOrDefaultAsync(
                x => x.Id == tender.SettlementId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Settlement tidak ditemukan.");

        if (!settlement.InvoiceId.HasValue)
        {
            // Settlement tanpa InvoiceId (misalnya DepositTopUp) tidak memiliki invoice penagihan
            // untuk diterbitkan surat penerimaan tagihannya.
            return null;
        }

        var invoice = await _dbContext.BilInvoices.SingleOrDefaultAsync(
            x => x.Id == settlement.InvoiceId.Value && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");

        var paymentMethod = await _dbContext.MstPaymentMethods.SingleOrDefaultAsync(
            x => x.Id == tender.PaymentMethodId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Metode pembayaran tidak ditemukan.");

        // BIL-VAL-111: Tender tunai wajib membawa identitas shift kasir.
        // Bila tidak membawa shift, penerbitan ditolak beserta transaksinya.
        var isCash = paymentMethod.IsCash
            || string.Equals(paymentMethod.PaymentMethodType, "Cash", StringComparison.OrdinalIgnoreCase);

        if (isCash && !tender.CashierShiftId.HasValue)
        {
            throw new BillingConsumerHandoffValidationException(
                "Pembayaran tunai tidak dapat diteruskan ke pembukuan karena shift kasirnya tidak diketahui. Tutup dan buka kembali shift, lalu ulangi");
        }

        // BIL-VAL-110: Satu tender satu surat per keadaan (idempotensi).
        var existingByTenderAndStatus = await _dbContext.BilCollectionHandoffs
            .FirstOrDefaultAsync(
                x => x.TenderId == tender.Id && x.TenderStatus == tender.Status && !x.IsDelete,
                cancellationToken);

        if (existingByTenderAndStatus != null)
        {
            return existingByTenderAndStatus;
        }

        var handoffKey = ComputeHandoffKey(tender.Id, tender.Status);
        var existingByKey = await _dbContext.BilCollectionHandoffs
            .FirstOrDefaultAsync(
                x => x.HandoffKey == handoffKey && !x.IsDelete,
                cancellationToken);

        if (existingByKey != null)
        {
            return existingByKey;
        }

        // BKC-DES-032: Mengambil kunci penasihat tagihan di dalam transaksi berjalan
        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"BIL_INVOICE_LEDGER_{invoice.Id:N}"],
                cancellationToken);
        }

        var allocationIds = await _dbContext.BilPaymentAllocations
            .Where(x => x.SettlementId == tender.SettlementId && !x.IsDelete)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var paymentAllocationIds = allocationIds.Count > 0
            ? string.Join(",", allocationIds)
            : null;

        var handoff = new BilCollectionHandoff
        {
            Id = Guid.NewGuid(),
            TenderId = tender.Id,
            SettlementId = tender.SettlementId,
            InvoiceId = invoice.Id,
            PaymentAllocationIds = paymentAllocationIds,
            PaymentMethodId = tender.PaymentMethodId,
            PaymentMethodAccountId = tender.PaymentMethodAccountId,
            Amount = tender.Amount,
            KwitansiNumber = tender.KwitansiNumber,
            CashierShiftId = tender.CashierShiftId,
            ProviderReference = tender.ProviderReference,
            ProviderEventId = tender.LastProviderEventId,
            OccurredAt = occurredAt,
            SourceInvoiceStatus = invoice.Status,
            TenderStatus = tender.Status,
            HandoffKey = handoffKey,
            CorrelationId = tender.CorrelationId,
            CausationId = tender.CausationId,
            Status = BillingHandoffStatuses.Created,
            AcknowledgedAt = null,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.BilCollectionHandoffs.Add(handoff);

        await _loggerService.AuditAsync(
            LogCategory,
            "BillingCollectionHandoff.Created",
            "Surat penerimaan uang diterbitkan untuk Finance.",
            new
            {
                HandoffId = handoff.Id,
                handoff.TenderId,
                handoff.SettlementId,
                handoff.InvoiceId,
                handoff.TenderStatus,
                handoff.Amount,
                handoff.HandoffKey,
                ActorUserId = actorUserId
            });

        return handoff;
    }

    /// <summary>
    /// Kunci handoff diturunkan secara deterministik dari pasangan TenderId dan TenderStatus.
    /// </summary>
    public static Guid ComputeHandoffKey(Guid tenderId, string tenderStatus)
    {
        var raw = Encoding.UTF8.GetBytes($"BIL_COLLECTION_{tenderId:N}_{tenderStatus}");
        var hash = MD5.HashData(raw);
        return new Guid(hash);
    }

    /// <summary>
    /// BKC-DEC-106, BKC-DES-038, BKC-DES-039, BIL-INT-014:
    /// Menerbitkan surat clearance resep (BilPrescriptionClearanceHandoff) ke Farmasi saat keadaan clearance berubah.
    /// Menegakkan BIL-VAL-112 (nomor versi naik monoton), BIL-VAL-113 (sebab sesuai arah),
    /// BIL-VAL-114 (hasil finansial wajib saat CLEARED), dan BIL-VAL-115 (biaya bukan obat tidak mencabut clearance).
    /// Menggunakan kunci penasihat tagihan yang sudah ada (BKC-DES-032).
    /// </summary>
    public async Task<List<BilPrescriptionClearanceHandoff>> PublishForClearanceChangeAsync(
        Guid invoiceId,
        string reasonCode,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken,
        Guid? specificPrescriptionId = null)
    {
        // BIL-VAL-115: Penambahan biaya tindakan, lab, rad, atau kamar TIDAK melahirkan surat pencabutan.
        // Jika reasonCode bukan salah satu dari 6 reason code resmi, abaikan tanpa melempar galat.
        if (reasonCode != PrescriptionClearanceReasonCodes.InvoiceSettled
            && reasonCode != PrescriptionClearanceReasonCodes.InvoiceWrittenOff
            && reasonCode != PrescriptionClearanceReasonCodes.PrescriptionChargeIncreased
            && reasonCode != PrescriptionClearanceReasonCodes.PaymentReversed
            && reasonCode != PrescriptionClearanceReasonCodes.WriteOffReversed
            && reasonCode != PrescriptionClearanceReasonCodes.PayerCoverageReversed)
        {
            return [];
        }

        // Tentukan arah status clearance
        var isCleared = reasonCode is PrescriptionClearanceReasonCodes.InvoiceSettled
            or PrescriptionClearanceReasonCodes.InvoiceWrittenOff;

        var clearanceStatus = isCleared
            ? PrescriptionClearanceStatuses.Cleared
            : PrescriptionClearanceStatuses.Revoked;

        // BIL-VAL-113: Sebab wajib sesuai arah
        if (isCleared && (reasonCode is PrescriptionClearanceReasonCodes.PrescriptionChargeIncreased
            or PrescriptionClearanceReasonCodes.PaymentReversed
            or PrescriptionClearanceReasonCodes.WriteOffReversed
            or PrescriptionClearanceReasonCodes.PayerCoverageReversed))
        {
            throw new BillingConsumerHandoffValidationException(
                "Sebab clearance tidak sesuai arah status clearance.");
        }

        if (!isCleared && (reasonCode is PrescriptionClearanceReasonCodes.InvoiceSettled
            or PrescriptionClearanceReasonCodes.InvoiceWrittenOff))
        {
            throw new BillingConsumerHandoffValidationException(
                "Sebab pencabutan tidak sesuai arah status clearance.");
        }

        // Tentukan hasil finansial (FinancialOutcome)
        string? financialOutcome = null;
        if (isCleared)
        {
            if (reasonCode == PrescriptionClearanceReasonCodes.InvoiceWrittenOff)
            {
                financialOutcome = PrescriptionFinancialOutcomes.PaymentWaived;
            }
            else // INVOICE_SETTLED
            {
                // BKC-DEC-106, PHA-DEC-065, BIL-AT-140:
                // Tender bercampur menghasilkan hasil penjaminan terlepas dari proporsi nominal.
                // BilTender hanya menyimpan PaymentMethodId (tanpa navigasi) — dicocokkan manual
                // ke MstPaymentMethod, sama seperti pola pencarian tunggal pada PublishForTenderAsync.
                var hasInsuranceOrGuarantor = await (
                    from t in _dbContext.BilTenders.AsNoTracking()
                    join pm in _dbContext.MstPaymentMethods.AsNoTracking() on t.PaymentMethodId equals pm.Id
                    where t.Settlement.InvoiceId == invoiceId
                        && t.Status == BillingTenderStatuses.Succeeded
                        && !t.IsDelete
                    select pm)
                    .AnyAsync(pm => pm.IsInsurance
                        || pm.IsCompanyGuarantor
                        || pm.PaymentMethodType == "Insurance"
                        || pm.PaymentMethodType == "CompanyGuarantor",
                        cancellationToken);

                financialOutcome = hasInsuranceOrGuarantor
                    ? PrescriptionFinancialOutcomes.InsuranceApproved
                    : PrescriptionFinancialOutcomes.Paid;
            }
        }

        // BIL-VAL-114: Hasil finansial wajib ada saat menyatakan boleh diambil (CLEARED), dan harus kosong saat REVOKED
        if (isCleared && string.IsNullOrWhiteSpace(financialOutcome))
        {
            throw new BillingConsumerHandoffValidationException(
                "Hasil finansial wajib ada saat menyatakan clearance resep.");
        }
        if (!isCleared && financialOutcome != null)
        {
            throw new BillingConsumerHandoffValidationException(
                "Hasil finansial harus kosong saat clearance resep dicabut.");
        }

        // BKC-DES-032: Ambil kunci penasihat tagihan di dalam transaksi berjalan
        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"BIL_INVOICE_LEDGER_{invoiceId:N}"],
                cancellationToken);
        }

        // Kumpulkan daftar PrescriptionId yang terikat ke invoice ini
        List<Guid> targetPrescriptionIds;
        if (specificPrescriptionId.HasValue)
        {
            targetPrescriptionIds = [specificPrescriptionId.Value];
        }
        else
        {
            var pharmacyItems = await _dbContext.BilInvoiceItems.AsNoTracking()
                .Where(x => x.InvoiceId == invoiceId
                    && x.SourceDomain == "PHARMACY"
                    && x.Status != BillingInvoiceItemStatuses.Voided
                    && !x.IsDelete)
                .ToListAsync(cancellationToken);

            targetPrescriptionIds = pharmacyItems
                .Select(x => Guid.TryParse(x.SourceDetailId, out var presId) ? presId : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .Distinct()
                .ToList();
        }

        if (targetPrescriptionIds.Count == 0)
        {
            return [];
        }

        var publishedHandoffs = new List<BilPrescriptionClearanceHandoff>();

        foreach (var prescriptionId in targetPrescriptionIds)
        {
            // Ambil surat clearance terakhir untuk resep ini untuk menentukan versi dan mencegah duplikasi keadaan
            var latestHandoff = await _dbContext.BilPrescriptionClearanceHandoffs
                .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete)
                .OrderByDescending(x => x.FinancialVersion)
                .FirstOrDefaultAsync(cancellationToken);

            // Jika keadaan terakhir sudah identik (sama-sama CLEARED dengan outcome yang sama, atau sama-sama REVOKED dengan reason yang sama), no-op
            if (latestHandoff != null
                && latestHandoff.ClearanceStatus == clearanceStatus
                && latestHandoff.FinancialOutcome == financialOutcome
                && latestHandoff.ReasonCode == reasonCode)
            {
                continue;
            }

            // BIL-VAL-112, BKC-DES-039: FinancialVersion naik monoton per resep
            var nextVersion = (latestHandoff?.FinancialVersion ?? 0L) + 1L;

            var handoff = new BilPrescriptionClearanceHandoff
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescriptionId,
                InvoiceId = invoiceId,
                ClearanceStatus = clearanceStatus,
                FinancialOutcome = financialOutcome,
                ReasonCode = reasonCode,
                FinancialVersion = nextVersion,
                EffectiveAt = occurredAt,
                CorrelationId = correlationId,
                CausationId = causationId,
                Status = BillingHandoffStatuses.Created,
                AcknowledgedAt = null,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.BilPrescriptionClearanceHandoffs.Add(handoff);
            publishedHandoffs.Add(handoff);

            await _loggerService.AuditAsync(
                LogCategory,
                "BillingPrescriptionClearanceHandoff.Created",
                "Surat clearance resep diterbitkan untuk Farmasi.",
                new
                {
                    HandoffId = handoff.Id,
                    handoff.PrescriptionId,
                    handoff.InvoiceId,
                    handoff.ClearanceStatus,
                    handoff.FinancialOutcome,
                    handoff.ReasonCode,
                    handoff.FinancialVersion,
                    ActorUserId = actorUserId
                });
        }

        return publishedHandoffs;
    }

    /// <summary>
    /// BKC-DEC-107, BKC-DES-040, PHA-DEC-063, BIL-INT-014:
    /// Membaca keadaan clearance terkini sebuah resep secara in-process untuk rekonsiliasi Farmasi.
    /// Operasi baca murni tanpa efek samping (read-only AsNoTracking), aman dipanggil berulang.
    /// Resep yang belum pernah memiliki surat dijawab UNKNOWN (IsKnown: false, IsCleared: false),
    /// bukan galat dan bukan boleh diambil (BIL-VAL-117, BIL-AT-141-F).
    /// </summary>
    public async Task<PrescriptionClearanceStatusResponse> ReadPrescriptionClearanceAsync(
        Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var latestHandoff = await _dbContext.BilPrescriptionClearanceHandoffs.AsNoTracking()
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete)
            .OrderByDescending(x => x.FinancialVersion)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestHandoff == null)
        {
            return new PrescriptionClearanceStatusResponse(
                PrescriptionId: prescriptionId,
                ClearanceStatus: PrescriptionClearanceStatuses.Unknown,
                FinancialOutcome: null,
                ReasonCode: null,
                FinancialVersion: null,
                EffectiveAt: null,
                IsKnown: false,
                IsCleared: false);
        }

        return new PrescriptionClearanceStatusResponse(
            PrescriptionId: prescriptionId,
            ClearanceStatus: latestHandoff.ClearanceStatus,
            FinancialOutcome: latestHandoff.FinancialOutcome,
            ReasonCode: latestHandoff.ReasonCode,
            FinancialVersion: latestHandoff.FinancialVersion,
            EffectiveAt: latestHandoff.EffectiveAt,
            IsKnown: true,
            IsCleared: latestHandoff.ClearanceStatus == PrescriptionClearanceStatuses.Cleared);
    }

    /// <summary>
    /// Membaca daftar surat handoff yang belum diambil konsumen (BKC-DEC-108, BIL-API-1.3, BIL-SCR-41).
    /// </summary>
    public async Task<PagedResult<PendingHandoffResponse>> GetPendingHandoffsAsync(
        PendingHandoffQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate.Value > query.ToDate.Value)
        {
            throw new BillingConsumerHandoffValidationException(
                "Rentang tanggal terbalik. Tanggal awal tidak boleh lebih besar dari tanggal akhir.");
        }

        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

        var handoffType = query.HandoffType?.Trim().ToUpperInvariant();
        var includeCollection = string.IsNullOrEmpty(handoffType)
            || handoffType == BillingHandoffTypes.Collection
            || handoffType == "ALL";
        var includePrescription = string.IsNullOrEmpty(handoffType)
            || handoffType == BillingHandoffTypes.Prescription
            || handoffType == "ALL";
        var includeInpatient = string.IsNullOrEmpty(handoffType)
            || handoffType == BillingHandoffTypes.Inpatient
            || handoffType == "ALL";

        var pendingItems = new List<PendingHandoffResponse>();

        if (includeCollection)
        {
            var collectionQuery = _dbContext.BilCollectionHandoffs
                .AsNoTracking()
                .Include(x => x.Invoice)
                .Where(x => x.Status == BillingHandoffStatuses.Created && !x.IsDelete);

            if (query.FromDate.HasValue)
            {
                collectionQuery = collectionQuery.Where(x => x.OccurredAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                collectionQuery = collectionQuery.Where(x => x.OccurredAt <= query.ToDate.Value);
            }

            var collectionList = await collectionQuery
                .Select(x => new PendingHandoffResponse(
                    x.Id,
                    BillingHandoffTypes.Collection,
                    BillingHandoffTargetModules.Finance,
                    x.OccurredAt,
                    x.KwitansiNumber ?? (x.Invoice != null ? x.Invoice.InvoiceNumber : x.Id.ToString()),
                    x.InvoiceId,
                    x.Invoice != null ? x.Invoice.InvoiceNumber : null,
                    x.Status,
                    $"TenderStatus: {x.TenderStatus}, Amount: {x.Amount:N2}"))
                .ToListAsync(cancellationToken);

            pendingItems.AddRange(collectionList);
        }

        if (includePrescription)
        {
            var prescriptionQuery = _dbContext.BilPrescriptionClearanceHandoffs
                .AsNoTracking()
                .Include(x => x.Invoice)
                .Where(x => x.Status == BillingHandoffStatuses.Created && !x.IsDelete);

            if (query.FromDate.HasValue)
            {
                prescriptionQuery = prescriptionQuery.Where(x => x.EffectiveAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                prescriptionQuery = prescriptionQuery.Where(x => x.EffectiveAt <= query.ToDate.Value);
            }

            var prescriptionList = await prescriptionQuery
                .Select(x => new PendingHandoffResponse(
                    x.Id,
                    BillingHandoffTypes.Prescription,
                    BillingHandoffTargetModules.Pharmacy,
                    x.EffectiveAt,
                    x.PrescriptionId.ToString(),
                    x.InvoiceId,
                    x.Invoice != null ? x.Invoice.InvoiceNumber : null,
                    x.Status,
                    $"ClearanceStatus: {x.ClearanceStatus}, Outcome: {x.FinancialOutcome ?? "-"}, Reason: {x.ReasonCode}"))
                .ToListAsync(cancellationToken);

            pendingItems.AddRange(prescriptionList);
        }

        if (includeInpatient)
        {
            var inpatientQuery = _dbContext.BilInpatientClearanceHandoffs
                .AsNoTracking()
                .Include(x => x.Invoice)
                .Where(x => x.Status == BillingHandoffStatuses.Created && !x.IsDelete);

            if (query.FromDate.HasValue)
            {
                inpatientQuery = inpatientQuery.Where(x => x.EffectiveAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                inpatientQuery = inpatientQuery.Where(x => x.EffectiveAt <= query.ToDate.Value);
            }

            var inpatientList = await inpatientQuery
                .Select(x => new PendingHandoffResponse(
                    x.Id,
                    BillingHandoffTypes.Inpatient,
                    BillingHandoffTargetModules.Inpatient,
                    x.EffectiveAt,
                    x.EncounterId.ToString(),
                    x.InvoiceId,
                    x.Invoice != null ? x.Invoice.InvoiceNumber : null,
                    x.Status,
                    $"ClearanceStatus: {x.ClearanceStatus}, Outcome: {x.FinancialOutcome ?? "-"}, Sisa: {x.OutstandingBalance:N2}"))
                .ToListAsync(cancellationToken);

            pendingItems.AddRange(inpatientList);
        }

        var totalData = pendingItems.Count;
        var totalPage = (int)Math.Ceiling(totalData / (double)pageSize);

        var pagedItems = pendingItems
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<PendingHandoffResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = totalPage,
            Items = pagedItems
        };
    }

    /// <summary>
    /// Mencatat pengakuan penerimaan surat handoff oleh modul konsumen (BKC-DEC-108, BIL-API-1.3, BIL-AT-142).
    /// Menegakkan penolakan 409 Conflict bila sudah pernah diakui sebelumnya.
    /// Mematuhi BIL-PERMISSION-1.1: mencatat audit identitas surat, jenis, pelaku, dan waktu tanpa menyertakan kolom sensitif.
    /// </summary>
    public async Task<HandoffResponse> AcknowledgeHandoffAsync(
        Guid handoffId,
        AcknowledgeHandoffRequest? request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (handoffId == Guid.Empty)
        {
            throw new BillingConsumerHandoffValidationException("Identitas surat handoff tidak boleh kosong.");
        }

        var requestedType = request?.HandoffType?.Trim().ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;

        // 1. Periksa tabel BilCollectionHandoff
        if (string.IsNullOrEmpty(requestedType) || requestedType == BillingHandoffTypes.Collection)
        {
            var collectionHandoff = await _dbContext.BilCollectionHandoffs
                .SingleOrDefaultAsync(x => x.Id == handoffId && !x.IsDelete, cancellationToken);

            if (collectionHandoff != null)
            {
                if (collectionHandoff.Status == BillingHandoffStatuses.Acknowledged)
                {
                    // BIL-AT-142: Pengakuan kedua atas surat yang sama ditolak tanpa mengubah apa pun
                    throw new BillingConsumerHandoffConflictException(
                        $"Surat penerimaan uang dengan ID '{handoffId}' sudah pernah diakui sebelumnya pada {collectionHandoff.AcknowledgedAt:yyyy-MM-dd HH:mm:ss} UTC. Pengakuan kedua tidak mengubah apa pun.");
                }

                collectionHandoff.Status = BillingHandoffStatuses.Acknowledged;
                collectionHandoff.AcknowledgedAt = now;
                collectionHandoff.UpdateDateTime = now.UtcDateTime;
                collectionHandoff.UpdateBy = actorUserId;
                collectionHandoff.RowVersion = Guid.NewGuid();

                // BIL-PERMISSION-1.1: Audit log MUST NOT memuat ProviderReference, ProviderEventId, maupun identitas pasien
                await _loggerService.AuditAsync(
                    LogCategory,
                    "BillingConsumerHandoff.Acknowledged",
                    "Surat penerimaan uang ke Finance telah diakui oleh konsumen.",
                    new
                    {
                        HandoffId = handoffId,
                        HandoffType = BillingHandoffTypes.Collection,
                        TargetModule = BillingHandoffTargetModules.Finance,
                        ActorUserId = actorUserId,
                        AcknowledgedAt = now
                    });

                await _dbContext.SaveChangesAsync(cancellationToken);

                return new HandoffResponse(
                    collectionHandoff.Id,
                    BillingHandoffTypes.Collection,
                    BillingHandoffTargetModules.Finance,
                    collectionHandoff.Status,
                    collectionHandoff.AcknowledgedAt,
                    "Surat penerimaan uang berhasil diakui.");
            }
        }

        // 2. Periksa tabel BilPrescriptionClearanceHandoff
        if (string.IsNullOrEmpty(requestedType) || requestedType == BillingHandoffTypes.Prescription)
        {
            var prescriptionHandoff = await _dbContext.BilPrescriptionClearanceHandoffs
                .SingleOrDefaultAsync(x => x.Id == handoffId && !x.IsDelete, cancellationToken);

            if (prescriptionHandoff != null)
            {
                if (prescriptionHandoff.Status == BillingHandoffStatuses.Acknowledged)
                {
                    // BIL-AT-142: Pengakuan kedua atas surat yang sama ditolak tanpa mengubah apa pun
                    throw new BillingConsumerHandoffConflictException(
                        $"Surat clearance resep dengan ID '{handoffId}' sudah pernah diakui sebelumnya pada {prescriptionHandoff.AcknowledgedAt:yyyy-MM-dd HH:mm:ss} UTC. Pengakuan kedua tidak mengubah apa pun.");
                }

                prescriptionHandoff.Status = BillingHandoffStatuses.Acknowledged;
                prescriptionHandoff.AcknowledgedAt = now;
                prescriptionHandoff.UpdateDateTime = now.UtcDateTime;
                prescriptionHandoff.UpdateBy = actorUserId;
                prescriptionHandoff.RowVersion = Guid.NewGuid();

                // BIL-PERMISSION-1.1: Audit log
                await _loggerService.AuditAsync(
                    LogCategory,
                    "BillingConsumerHandoff.Acknowledged",
                    "Surat clearance resep ke Farmasi telah diakui oleh konsumen.",
                    new
                    {
                        HandoffId = handoffId,
                        HandoffType = BillingHandoffTypes.Prescription,
                        TargetModule = BillingHandoffTargetModules.Pharmacy,
                        ActorUserId = actorUserId,
                        AcknowledgedAt = now
                    });

                await _dbContext.SaveChangesAsync(cancellationToken);

                return new HandoffResponse(
                    prescriptionHandoff.Id,
                    BillingHandoffTypes.Prescription,
                    BillingHandoffTargetModules.Pharmacy,
                    prescriptionHandoff.Status,
                    prescriptionHandoff.AcknowledgedAt,
                    "Surat clearance resep berhasil diakui.");
            }
        }

        // 3. Periksa tabel BilInpatientClearanceHandoff (BKC-DEC-115, BKC-DES-045, BIL-INT-016)
        if (string.IsNullOrEmpty(requestedType) || requestedType == BillingHandoffTypes.Inpatient)
        {
            var inpatientHandoff = await _dbContext.BilInpatientClearanceHandoffs
                .SingleOrDefaultAsync(x => x.Id == handoffId && !x.IsDelete, cancellationToken);

            if (inpatientHandoff != null)
            {
                if (inpatientHandoff.Status == BillingHandoffStatuses.Acknowledged)
                {
                    throw new BillingConsumerHandoffConflictException(
                        $"Surat clearance rawat inap dengan ID '{handoffId}' sudah pernah diakui sebelumnya pada {inpatientHandoff.AcknowledgedAt:yyyy-MM-dd HH:mm:ss} UTC. Pengakuan kedua tidak mengubah apa pun.");
                }

                inpatientHandoff.Status = BillingHandoffStatuses.Acknowledged;
                inpatientHandoff.AcknowledgedAt = now;
                inpatientHandoff.UpdateDateTime = now.UtcDateTime;
                inpatientHandoff.UpdateBy = actorUserId;
                inpatientHandoff.RowVersion = Guid.NewGuid();

                await _loggerService.AuditAsync(
                    LogCategory,
                    "BillingConsumerHandoff.Acknowledged",
                    "Surat clearance rawat inap ke Bangsal telah diakui oleh konsumen.",
                    new
                    {
                        HandoffId = handoffId,
                        HandoffType = BillingHandoffTypes.Inpatient,
                        TargetModule = BillingHandoffTargetModules.Inpatient,
                        ActorUserId = actorUserId,
                        AcknowledgedAt = now
                    });

                await _dbContext.SaveChangesAsync(cancellationToken);

                return new HandoffResponse(
                    inpatientHandoff.Id,
                    BillingHandoffTypes.Inpatient,
                    BillingHandoffTargetModules.Inpatient,
                    inpatientHandoff.Status,
                    inpatientHandoff.AcknowledgedAt,
                    "Surat clearance rawat inap berhasil diakui.");
            }
        }

        throw new KeyNotFoundException($"Surat handoff konsumen dengan ID '{handoffId}' tidak ditemukan.");
    }

    /// <summary>
    /// BKC-DEC-115, BKC-DES-045, BIL-INT-016:
    /// Menerbitkan surat kelayakan pemulangan rawat inap (BilInpatientClearanceHandoff) saat status pelunasan tagihan berubah.
    /// Berjalan di dalam batas transaksi pemanggil dan memakai kunci penasihat.
    /// </summary>
    public async Task<BilInpatientClearanceHandoff?> PublishForInpatientClearanceAsync(
        Guid invoiceId,
        string reasonCode,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken,
        string? revocationReason = null)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
        if (invoice == null) return null;

        if (!string.Equals(invoice.ServiceType, "INPATIENT", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return await _inpatientClearanceService.EvaluateClearanceAsync(
            invoice.EncounterId,
            reasonCode,
            actorUserId,
            occurredAt,
            correlationId,
            causationId,
            cancellationToken,
            revocationReason);
    }

    /// <summary>
    /// BKC-DEC-116, BKC-DES-046, BIL-VAL-123:
    /// Menegakkan Auto-Reblock seketika saat tagihan susulan masuk pada invoice ranap yang sebelumnya berstatus CLEARED.
    /// </summary>
    public async Task<BilInpatientClearanceHandoff?> TriggerInpatientAutoReblockIfApplicableAsync(
        Guid invoiceId,
        string triggerReason,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken)
    {
        return await _inpatientClearanceService.TriggerAutoReblockIfApplicableAsync(
            invoiceId,
            triggerReason,
            actorUserId,
            occurredAt,
            correlationId,
            causationId,
            cancellationToken);
    }
}

public abstract class BillingConsumerHandoffException : Exception
{
    protected BillingConsumerHandoffException(string message) : base(message) { }
}

public sealed class BillingConsumerHandoffValidationException(string message)
    : BillingConsumerHandoffException(message);

public sealed class BillingConsumerHandoffConflictException(string message)
    : BillingConsumerHandoffException(message);
