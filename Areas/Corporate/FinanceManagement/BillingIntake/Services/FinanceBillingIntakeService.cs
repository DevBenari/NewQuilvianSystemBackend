using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;

/// <summary>
/// Membaca handoff Billing, membuat piutang/penerimaan yang sesuai, lalu menandai ACK
/// (FIN-DES-008, 02-backend-architecture.md). Dibangun pada BE-FIN-009 karena tidak ada task
/// roadmap eksplisit yang memilikinya — gap ini dilaporkan pertama kali di BE-FIN-005, ditutup
/// di sini atas keputusan eksplisit pemilik repository (21 September 2026), bukan diselipkan
/// sepihak.
///
/// Cakupan: HandoffType AR (BilArHandoff → FinReceivable, dibuat langsung di sini) dan COLLECTION
/// (BilCollectionHandoff → FinReceipt, BE-FIN-016, otorisasi eksplisit 22 September 2026 —
/// pembuatannya didelegasikan ke FinanceReceiptService sejak BE-FIN-017, 02-backend-architecture.md
/// §4.22, bukan ditulis inline di sini). AP/ADJUSTMENT TIDAK diproses — FinPayable belum ada task
/// pemilik. Baris intake dengan tipe itu akan tetap NEW tanpa penangan.
///
/// BE-FIN-011: setiap piutang yang berhasil diakui menulis kejadian PENGAKUAN-PIUTANG ke
/// FinAccountingEventOutbox lewat FinanceAccountingOutboxService, di dalam transaksi
/// ProcessArIntakeAsync yang sama (FIN-DES-017, FR-FIN-070). Kejadian penerimaan
/// (PENERIMAAN-KASIR/PEMBALIKAN-PENERIMAAN-KASIR) ditulis dengan cara yang sama oleh
/// FinanceReceiptService, tetap di dalam transaksi ProcessCollectionIntakeAsync yang sama.
/// </summary>
public sealed class FinanceBillingIntakeService
{
    private const string LogCategory = "Corporate.FinanceManagement.BillingIntake";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;
    private readonly FinanceReceiptService _receiptService;
    private readonly FinanceSubledgerMovementService _subledgerMovementService;

    public FinanceBillingIntakeService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceAccountingOutboxService accountingOutboxService,
        FinanceReceiptService receiptService,
        FinanceSubledgerMovementService subledgerMovementService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _accountingOutboxService = accountingOutboxService;
        _receiptService = receiptService;
        _subledgerMovementService = subledgerMovementService;
    }

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<BillingIntakeResponse>> GetPagedAsync(BillingIntakeQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinBillingHandoffIntakes.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.HandoffType)) query = query.Where(x => x.HandoffType == request.HandoffType);

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => Map(x)).ToListAsync(cancellationToken);
        return new PagedResult<BillingIntakeResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<BillingIntakeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var intake = await _dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
        return Map(intake);
    }

    public async Task<BillingIntakeSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.FinBillingHandoffIntakes.AsNoTracking().Where(x => !x.IsDelete);
        var counts = await query.GroupBy(x => x.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        int Count(string status) => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;

        return new BillingIntakeSummaryResponse
        {
            TotalIntake = counts.Sum(c => c.Count),
            NewCount = Count(FinBillingHandoffIntakeStatuses.New),
            ErrorCount = Count(FinBillingHandoffIntakeStatuses.Error),
            ConsumedCount = Count(FinBillingHandoffIntakeStatuses.Consumed),
            AcknowledgedCount = Count(FinBillingHandoffIntakeStatuses.Acknowledged)
        };
    }

    public Task<BillingIntakeFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new BillingIntakeFilterMetadataResponse
        {
            PageSizeOptions = [10, 25, 50, 100],
            SortableFields = ["createDateTime", "status"],
            StatusOptions =
            [
                FinBillingHandoffIntakeStatuses.New, FinBillingHandoffIntakeStatuses.Consumed,
                FinBillingHandoffIntakeStatuses.Acknowledged, FinBillingHandoffIntakeStatuses.Error
            ]
        });

    // ------------------------------------------------------------------------------------
    // Penemuan fakta baru — belum ada hosted job yang menjadwalkannya (di luar cakupan
    // BE-FIN-009); dipicu manual lewat endpoint sampai ada keputusan penjadwalan otomatis.
    // ------------------------------------------------------------------------------------

    public async Task<int> SyncNewFactsAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        var existingKeysByType = (await _dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .Where(x => !x.IsDelete)
            .Select(x => new { x.HandoffType, x.SourceHandoffKey })
            .ToListAsync(cancellationToken))
            .ToLookup(x => x.HandoffType, x => x.SourceHandoffKey);

        var arCandidates = await _dbContext.BilArHandoffs.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == BillingHandoffStatuses.Created)
            .ToListAsync(cancellationToken);
        var arToCreate = arCandidates.Where(x => !existingKeysByType[FinBillingHandoffTypes.Ar].Contains(x.HandoffKey)).ToList();

        // BE-FIN-097, FIN-DEC-187: hanya BilHandoffAdjustment yang menunjuk handoff EMPLOYEE_BENEFIT
        // yang relevan bagi Finance (koreksi pemilik manfaat salah orang) — penyesuaian Billing lain
        // (refund, write-off invoice biasa, dsb.) sudah punya jalur sendiri dan TIDAK diproses ulang
        // di sini. FinBillingHandoffTypes.Adjustment sudah ada sejak sebelum task ini tetapi belum
        // pernah disinkron maupun diproses — ditemukan dorman, bukan ditambahkan baru.
        var employeeBenefitHandoffIds = await _dbContext.BilArHandoffs.AsNoTracking()
            .Where(x => !x.IsDelete && x.DebtorType == BillingArDebtorTypes.EmployeeBenefit)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var adjustmentCandidates = employeeBenefitHandoffIds.Count == 0
            ? new List<BilHandoffAdjustment>()
            : await _dbContext.BilHandoffAdjustments.AsNoTracking()
                .Where(x => !x.IsDelete && x.ArHandoffId.HasValue && employeeBenefitHandoffIds.Contains(x.ArHandoffId.Value))
                .ToListAsync(cancellationToken);
        var adjustmentToCreate = adjustmentCandidates.Where(x => !existingKeysByType[FinBillingHandoffTypes.Adjustment].Contains(x.Id)).ToList();

        // BE-FIN-016: BilCollectionHandoff dibuat dengan Status = CREATED untuk kedua keadaan
        // terminal tender (SUCCEEDED maupun REVERSED) — keduanya disinkron, dibedakan saat proses.
        var collectionCandidates = await _dbContext.BilCollectionHandoffs.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == BillingHandoffStatuses.Created)
            .ToListAsync(cancellationToken);
        var collectionToCreate = collectionCandidates.Where(x => !existingKeysByType[FinBillingHandoffTypes.Collection].Contains(x.HandoffKey)).ToList();

        // BE-FIN-025: Jalur sinkronisasi mutasi deposit (ALLOCATION dan RELEASE)
        // BE-FIN-046: REVERSAL ikut disinkron — mutasi pembalik atas TOP_UP adalah fakta bahwa uang
        // muka yang sempat tercatat ternyata tidak jadi diterima (FIN-DES-057). TOP_UP sendiri
        // TIDAK disinkron sebagai fakta biasa; ia hanya dibaca pendeteksi FIN-VAL-142 di bawah.
        var depositCandidatesAll = await _dbContext.BilDepositMovements.AsNoTracking()
            .Where(x => !x.IsDelete)
            .ToListAsync(cancellationToken);

        var depositCandidates = depositCandidatesAll
            .Where(x => x.MovementType == BillingDepositMovementTypes.Allocation
                     || x.MovementType == BillingDepositMovementTypes.Release
                     || x.MovementType == BillingDepositMovementTypes.Reversal)
            .ToList();
        var depositToCreate = depositCandidates.Where(x => !existingKeysByType[FinBillingHandoffTypes.DepositMovement].Contains(x.IdempotencyKey)).ToList();

        // BE-FIN-025: Jalur sinkronisasi kelebihan bayar (ALLOCATION_EXCESS dan SETTLEMENT)
        var creditCandidates = await _dbContext.BilRefundableCredits.AsNoTracking()
            .Where(x => !x.IsDelete && (x.SourceType == BillingRefundableCreditSourceTypes.AllocationExcess || x.SourceType == BillingRefundableCreditSourceTypes.Settlement))
            .ToListAsync(cancellationToken);
        var creditToCreate = creditCandidates.Where(x => !existingKeysByType[FinBillingHandoffTypes.RefundableCredit].Contains(x.Id)).ToList();

        // BE-FIN-025: Jalur sinkronisasi pengembalian uang (BilRefundCase EXECUTED)
        var refundCandidates = await _dbContext.BilRefundCases.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == BillingRefundCaseStatuses.Executed)
            .ToListAsync(cancellationToken);
        var refundToCreate = refundCandidates.Where(x => !existingKeysByType[FinBillingHandoffTypes.RefundCase].Contains(x.IdempotencyKey)).ToList();

        // BE-FIN-025: Jalur sinkronisasi telaah selisih kas shift (BilCashVarianceReview)
        var varianceCandidates = await _dbContext.BilCashVarianceReviews.AsNoTracking()
            .Where(x => !x.IsDelete)
            .ToListAsync(cancellationToken);
        var varianceToCreate = varianceCandidates.Where(x => !existingKeysByType[FinBillingHandoffTypes.CashVarianceReview].Contains(x.Id)).ToList();

        // BE-FIN-046, FIN-VAL-142 / FIN-DES-057: JARING PENGAMAN, bukan jalur utama.
        //
        // Saat tender yang mendanai top-up deposit dibalik, Billing MUST menulis mutasi pembalik
        // atas TOP_UP-nya (BKC-DEC-128..131, sudah berjalan sejak BE-BKC-079). Bila suatu saat
        // jalur itu berubah dan mutasi pembaliknya tidak ada, saldo deposit tetap mencatat uang
        // yang tidak pernah jadi diterima — dan Finance tidak akan melihat fakta apa pun untuk
        // diterbitkan. Pemeriksaan ini membuat lubang itu TERLIHAT, bukan mendiamkannya.
        //
        // Nol tulisan ke tabel Bil* — BilTender dan BilDepositMovement dibaca saja.
        var reversedTopUpSettlementIds = await (
            from tender in _dbContext.BilTenders.AsNoTracking()
            join settlement in _dbContext.BilSettlements.AsNoTracking() on tender.SettlementId equals settlement.Id
            where !tender.IsDelete && !settlement.IsDelete
                  && tender.Status == BillingTenderStatuses.Reversed
                  && settlement.Purpose == BillingSettlementPurposes.DepositTopUp
            select settlement.Id).Distinct().ToListAsync(cancellationToken);

        var orphanTopUps = new List<BilDepositMovement>();
        if (reversedTopUpSettlementIds.Count > 0)
        {
            var topUpsOfReversedTenders = depositCandidatesAll
                .Where(x => x.MovementType == BillingDepositMovementTypes.TopUp
                         && x.SettlementId.HasValue
                         && reversedTopUpSettlementIds.Contains(x.SettlementId.Value))
                .ToList();

            // Mutasi pembalik yang bersesuaian: REVERSAL yang menunjuk TOP_UP itu lewat
            // ReversesMovementId (penanda eksplisit yang disepakati BKC-DEC-132).
            var reversedMovementIds = depositCandidatesAll
                .Where(x => x.MovementType == BillingDepositMovementTypes.Reversal && x.ReversesMovementId.HasValue)
                .Select(x => x.ReversesMovementId!.Value)
                .ToHashSet();

            orphanTopUps = topUpsOfReversedTenders
                .Where(x => !reversedMovementIds.Contains(x.Id)
                         && !existingKeysByType[FinBillingHandoffTypes.DepositMovement].Contains(x.IdempotencyKey))
                .ToList();
        }

        var totalToCreate = arToCreate.Count + collectionToCreate.Count + depositToCreate.Count
            + creditToCreate.Count + refundToCreate.Count + varianceToCreate.Count + orphanTopUps.Count
            + adjustmentToCreate.Count;
        if (totalToCreate == 0) return 0;

        var now = DateTime.UtcNow;
        foreach (var handoff in arToCreate)
        {
            _dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.Ar,
                SourceHandoffId = handoff.Id,
                SourceHandoffKey = handoff.HandoffKey,
                Status = FinBillingHandoffIntakeStatuses.New,
                CorrelationId = handoff.CorrelationId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
        foreach (var handoff in collectionToCreate)
        {
            _dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.Collection,
                SourceHandoffId = handoff.Id,
                SourceHandoffKey = handoff.HandoffKey,
                Status = FinBillingHandoffIntakeStatuses.New,
                CorrelationId = handoff.CorrelationId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
        foreach (var movement in depositToCreate)
        {
            _dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.DepositMovement,
                SourceHandoffId = movement.Id,
                SourceHandoffKey = movement.IdempotencyKey,
                Status = FinBillingHandoffIntakeStatuses.New,
                CorrelationId = movement.CorrelationId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
        foreach (var credit in creditToCreate)
        {
            _dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.RefundableCredit,
                SourceHandoffId = credit.Id,
                SourceHandoffKey = credit.Id,
                Status = FinBillingHandoffIntakeStatuses.New,
                CorrelationId = credit.InvoiceId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
        foreach (var refund in refundToCreate)
        {
            _dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.RefundCase,
                SourceHandoffId = refund.Id,
                SourceHandoffKey = refund.IdempotencyKey,
                Status = FinBillingHandoffIntakeStatuses.New,
                CorrelationId = refund.CorrelationId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
        foreach (var review in varianceToCreate)
        {
            _dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.CashVarianceReview,
                SourceHandoffId = review.Id,
                SourceHandoffKey = review.Id,
                Status = FinBillingHandoffIntakeStatuses.New,
                CorrelationId = review.ShiftId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
        foreach (var adjustment in adjustmentToCreate)
        {
            _dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.Adjustment,
                SourceHandoffId = adjustment.Id,
                SourceHandoffKey = adjustment.Id, // BilHandoffAdjustment tidak punya HandoffKey sendiri — pola sama dengan RefundableCredit.
                Status = FinBillingHandoffIntakeStatuses.New,
                CorrelationId = adjustment.CorrelationId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        // BE-FIN-046, FIN-VAL-142: baris ditulis LANGSUNG berstatus ERROR — bukan NEW. Ia bukan
        // fakta yang menunggu diolah, melainkan lubang yang menunggu diperbaiki di sisi Billing.
        // Barisnya menunjuk mutasi TOP_UP yang seharusnya sudah punya pembalik, supaya petugas
        // dapat menelusurinya dari layar pantauan tanpa membuka database.
        foreach (var topUp in orphanTopUps)
        {
            _dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.DepositMovement,
                SourceHandoffId = topUp.Id,
                SourceHandoffKey = topUp.IdempotencyKey,
                Status = FinBillingHandoffIntakeStatuses.Error,
                ErrorMessage =
                    "Tender top-up deposit sudah dibalik, tetapi mutasi deposit pembaliknya tidak ditemukan " +
                    "(FIN-VAL-142). Saldo deposit berpotensi mencatat uang yang tidak pernah jadi diterima. " +
                    "Nol kejadian akuntansi diterbitkan untuk fakta yang belum lengkap ini — perbaikannya ada " +
                    "di sisi Billing, dicatat FIN-OQ-034.",
                CorrelationId = topUp.CorrelationId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Race dengan sinkronisasi lain yang berjalan bersamaan — index unik
            // IX_FinBillingHandoffIntake_Identity sudah mencegah duplikat; percobaan ini
            // dianggap tidak menghasilkan baris baru, bukan kegagalan.
            foreach (var entry in _dbContext.ChangeTracker.Entries<FinBillingHandoffIntake>().Where(e => e.State == EntityState.Added).ToList())
                entry.State = EntityState.Detached;
            return 0;
        }
        return totalToCreate;
    }

    // ------------------------------------------------------------------------------------
    // Pengolahan (FR-FIN-010..013) — satu-satunya jalur yang membuat FinReceivable dari fakta AR.
    // ------------------------------------------------------------------------------------

    public async Task<BillingIntakeResponse> ProcessAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var current = await _dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
        // FR-FIN-013: yang sudah berhasil diolah tidak dapat diulang.
        if (current.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
            throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");

        try
        {
            switch (current.HandoffType)
            {
                case FinBillingHandoffTypes.Collection:
                    await ProcessCollectionIntakeAsync(intakeId, actorUserId, cancellationToken);
                    break;
                case FinBillingHandoffTypes.Ar:
                    await ProcessArIntakeAsync(intakeId, actorUserId, cancellationToken);
                    break;
                case FinBillingHandoffTypes.DepositMovement:
                    await ProcessDepositMovementIntakeAsync(intakeId, actorUserId, cancellationToken);
                    break;
                case FinBillingHandoffTypes.RefundableCredit:
                    await ProcessRefundableCreditIntakeAsync(intakeId, actorUserId, cancellationToken);
                    break;
                case FinBillingHandoffTypes.RefundCase:
                    await ProcessRefundCaseIntakeAsync(intakeId, actorUserId, cancellationToken);
                    break;
                case FinBillingHandoffTypes.CashVarianceReview:
                    await ProcessCashVarianceReviewIntakeAsync(intakeId, actorUserId, cancellationToken);
                    break;
                case FinBillingHandoffTypes.Adjustment:
                    await ProcessArAdjustmentIntakeAsync(intakeId, actorUserId, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Konsumen untuk HandoffType '{current.HandoffType}' belum didukung.");
            }
        }
        catch (Exception exception) when (exception is not (KeyNotFoundException or BillingIntakeValidationException))
        {
            // Transaksi di method pengolahan sudah di-rollback; bersihkan change tracker
            // supaya percobaan yang gagal tidak ikut tersimpan saat MarkErrorAsync memanggil
            // SaveChangesAsync berikutnya.
            _dbContext.ChangeTracker.Clear();
            // FR-FIN-011: kegagalan tersimpan dan terlihat, bukan hilang diam-diam.
            await MarkErrorAsync(intakeId, exception.Message, actorUserId, cancellationToken);
        }

        var refreshed = await _dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .SingleAsync(x => x.Id == intakeId, cancellationToken);
        return Map(refreshed);
    }

    private async Task ProcessArIntakeAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BILLING_INTAKE_{intakeId:N}", cancellationToken);

            var intake = await _dbContext.FinBillingHandoffIntakes
                .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
            if (intake.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
                throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");
            if (intake.HandoffType != FinBillingHandoffTypes.Ar)
                throw new InvalidOperationException(
                    $"Konsumen untuk HandoffType '{intake.HandoffType}' belum dibangun (lihat laporan task BE-FIN-009 bagian 1).");

            var handoff = await _dbContext.BilArHandoffs
                .SingleOrDefaultAsync(x => x.Id == intake.SourceHandoffId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Fakta AR sumber tidak ditemukan di Billing.");

            // FIN-VAL-012: satu fakta AR menghasilkan satu piutang — lapis kedua di atas
            // IX_FinReceivable_SourceHandoffKey untuk pesan yang lebih jelas.
            if (await _dbContext.FinReceivables.AnyAsync(x => !x.IsDelete && x.SourceHandoffKey == handoff.HandoffKey, cancellationToken))
                throw new InvalidOperationException("Piutang untuk fakta ini sudah pernah dibuat.");

            // BE-FIN-091, FIN-VAL-247: lapis service di atas CK_BilArHandoff_BenefitOwner dan
            // CK_FinReceivable_BenefitOwner — pesan 422 yang jelas sebelum baris menyentuh database.
            if (handoff.DebtorType == BillingArDebtorTypes.EmployeeBenefit && handoff.BenefitOwnerId is null)
                throw new BillingIntakeValidationException(
                    "Serah terima piutang manfaat karyawan wajib membawa ID pemilik manfaat yang valid.");

            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == handoff.InvoiceId, cancellationToken);

            // FR-FIN-024: PatientId ditelusuri lewat BilInvoice.EncounterId -> RegPatientEncounter.PatientId.
            // Diverifikasi 23 September 2026 (laporan BE-FIN-009 bagian 1 butir #5): RegPatientEncounter
            // menyimpan PatientId langsung (Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs),
            // bukan skema yang belum diverifikasi seperti dicatat sebelumnya.
            Guid? patientId = null;
            if (invoice is not null)
            {
                patientId = await _dbContext.RegPatientEncounters.AsNoTracking()
                    .Where(x => x.Id == invoice.EncounterId)
                    .Select(x => (Guid?)x.PatientId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var now = DateTimeOffset.UtcNow;
            var receivable = new FinReceivable
            {
                ReceivableNumber = GenerateReceivableNumber(),
                SourceHandoffKey = handoff.HandoffKey,
                SourceHandoffId = handoff.Id,
                InvoiceId = handoff.InvoiceId,
                // Nilai string BillingArDebtorTypes sama persis dengan FinReceivableDebtorTypes,
                // disalin apa adanya. BE-FIN-091: EMPLOYEE_BENEFIT kini punya jalur intake penuh
                // (FIN-DES-024 OPEN DECISION ditutup) — PAYER/PATIENT_GUARANTOR tetap seperti semula.
                DebtorType = handoff.DebtorType,
                DebtorReferenceId = handoff.DebtorReferenceId,
                // BE-FIN-091, FIN-DEC-184/185: disalin apa adanya, tidak pernah dihitung ulang.
                // NULL untuk baris PAYER/PATIENT_GUARANTOR — ditegakkan CK_BilArHandoff_BenefitOwner
                // di sisi sumber dan CK_FinReceivable_BenefitOwner di sini.
                BenefitOwnerId = handoff.BenefitOwnerId,
                BenefitRelationship = handoff.BenefitRelationship,
                // FIN-DES-011 area: nilai disalin dari handoff, tidak pernah dihitung ulang.
                // BilArHandoff.Amount sudah berupa sisa tanggungan penjamin (FR-FIN-020) —
                // dihitung Billing sebelum handoff dibuat, bukan oleh Finance.
                OriginalAmount = handoff.Amount,
                OutstandingAmount = handoff.Amount,
                // BilArHandoff.DueDate nullable; tidak diatur eksplisit dokumen manapun bila
                // kosong — dipakai tanggal pengakuan hari ini sebagai nilai aman, dicatat di
                // laporan task, bukan keputusan bisnis baru.
                DueDate = handoff.DueDate.HasValue ? FinanceBusinessDate.ToDateOnly(handoff.DueDate.Value) : FinanceBusinessDate.ToDateOnly(now),
                Status = FinReceivableStatuses.Outstanding,
                ClaimStatus = FinReceivableClaimStatuses.NotRequired,
                RecognizedAt = now,
                CorrelationId = handoff.CorrelationId,
                CausationId = handoff.CausationId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            // Kardinalitas 1..* (02-backend-architecture.md §3.2): satu baris rincian minimal,
            // sebesar OriginalAmount. PatientId diisi dari RegPatientEncounter (lihat pencarian
            // di atas) — menutup FR-FIN-024, bukan lagi dikosongkan.
            receivable.Items.Add(new FinReceivableItem
            {
                EncounterId = invoice?.EncounterId,
                InvoiceId = handoff.InvoiceId,
                PatientId = patientId,
                Description = invoice is not null ? $"Piutang dari tagihan {invoice.InvoiceNumber}" : "Piutang dari tagihan Billing",
                Amount = handoff.Amount,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
            _dbContext.FinReceivables.Add(receivable);

            // BE-FIN-060, FIN-DES-079: Mutasi subledger PENGAKUAN piutang baru (Jalur 1)
            await _subledgerMovementService.RecordReceivableMovementAsync(
                receivable: receivable,
                movementType: FinReceivableMovementTypes.Pengakuan,
                deltaAmount: receivable.OriginalAmount,
                balanceBefore: 0m,
                occurredAt: now,
                actorUserId: actorUserId,
                correlationId: receivable.CorrelationId,
                causationId: receivable.CausationId,
                referenceNumber: receivable.ReceivableNumber,
                notes: $"Pengakuan piutang dari intake Billing {handoff.HandoffKey}",
                cancellationToken: cancellationToken);

            // BE-FIN-011, FIN-DES-017: kejadian PENGAKUAN-PIUTANG ditulis DI DALAM transaksi yang
            // sama dengan piutangnya sendiri (FR-FIN-070) — StageEventAsync hanya Add(), commit
            // sesungguhnya terjadi lewat SaveChangesAsync/CommitAsync di bawah, milik method ini.
            await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
            {
                EventTypeCode = FinAccountingEventTypeCodes.PengakuanPiutang,
                SourceTransactionId = receivable.ReceivableNumber,
                EventOccurredAt = now,
                AccountingDate = FinanceBusinessDate.ToDateOnly(now),
                Amount = receivable.OriginalAmount,
                CorrelationId = receivable.CorrelationId,
                CausationId = receivable.CausationId,
                ActorUserId = actorUserId
            }, cancellationToken);

            // FIN-BIL-005: Finance wajib mengirim ACK balik ke Billing setelah berhasil.
            handoff.Status = BillingHandoffStatuses.Acknowledged;
            handoff.AcknowledgedAt = now;
            handoff.RowVersion = Guid.NewGuid();

            intake.Status = FinBillingHandoffIntakeStatuses.Acknowledged;
            intake.TargetEntityId = receivable.Id;
            intake.ConsumedAt = now;
            intake.AcknowledgedAt = now;
            intake.ErrorMessage = null;
            intake.UpdateDateTime = DateTime.UtcNow;
            intake.UpdateBy = actorUserId;
            intake.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Intake.Acknowledged", intake.Id, actorUserId, receivable.Id);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Pengolahan (BE-FIN-016, FR-FIN-030..034) — satu-satunya jalur yang memicu pembuatan
    // FinReceipt dari fakta BilCollectionHandoff. Pembuatan sesungguhnya didelegasikan ke
    // FinanceReceiptService (BE-FIN-017, 02-backend-architecture.md §4.22) — method itu MUST NOT
    // membuka transaksi sendiri, ikut transaksi Serializable milik method ini.
    // ------------------------------------------------------------------------------------

    private async Task ProcessCollectionIntakeAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BILLING_INTAKE_{intakeId:N}", cancellationToken);

            var intake = await _dbContext.FinBillingHandoffIntakes
                .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
            if (intake.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
                throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");
            if (intake.HandoffType != FinBillingHandoffTypes.Collection)
                throw new InvalidOperationException(
                    $"Konsumen untuk HandoffType '{intake.HandoffType}' belum dibangun (lihat laporan task BE-FIN-016 bagian 1).");

            var handoff = await _dbContext.BilCollectionHandoffs
                .SingleOrDefaultAsync(x => x.Id == intake.SourceHandoffId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Fakta penerimaan sumber tidak ditemukan di Billing.");

            var now = DateTimeOffset.UtcNow;
            var receipt = await _receiptService.CreateFromTenderIntakeAsync(handoff, actorUserId, cancellationToken);

            // FIN-BIL-005: Finance wajib mengirim ACK balik ke Billing setelah berhasil.
            handoff.Status = BillingHandoffStatuses.Acknowledged;
            handoff.AcknowledgedAt = now;
            handoff.RowVersion = Guid.NewGuid();

            intake.Status = FinBillingHandoffIntakeStatuses.Acknowledged;
            intake.TargetEntityId = receipt.Id;
            intake.ConsumedAt = now;
            intake.AcknowledgedAt = now;
            intake.ErrorMessage = null;
            intake.UpdateDateTime = DateTime.UtcNow;
            intake.UpdateBy = actorUserId;
            intake.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Intake.Acknowledged", intake.Id, actorUserId, receipt.Id);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-025: Pengolahan mutasi deposit (ALLOCATION / RELEASE)
    // ------------------------------------------------------------------------------------

    private async Task ProcessDepositMovementIntakeAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BILLING_INTAKE_{intakeId:N}", cancellationToken);

            var intake = await _dbContext.FinBillingHandoffIntakes
                .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
            if (intake.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
                throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");
            if (intake.HandoffType != FinBillingHandoffTypes.DepositMovement)
                throw new InvalidOperationException($"Konsumen untuk HandoffType '{intake.HandoffType}' tidak cocok.");

            var movement = await _dbContext.BilDepositMovements.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == intake.SourceHandoffId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Fakta mutasi deposit sumber tidak ditemukan di Billing.");

            var now = DateTimeOffset.UtcNow;

            if (movement.MovementType == BillingDepositMovementTypes.Release)
            {
                // BE-FIN-047 (FIN-DES-064/065, FIN-DEC-080/081): pembatalan alokasi uang muka —
                // nol kas bergerak, tagihan terbuka kembali dan saldo deposit pulih. FIN-VAL-144
                // ("RELEASE dilarang menjadi PENGEMBALIAN-UANG-MUKA") terpenuhi secara STRUKTURAL:
                // cabang ini tidak pernah menulis kode itu untuk RELEASE, apa pun hasil pemasangannya.
                //
                // TEMUAN KRITIS — kunci pemasangan yang tertulis kontrak TIDAK DAPAT DIPAKAI.
                // FIN-VAL-145/146 (FIN-VAL-1.5, approved) dan FIN-DES-065 menulis "RELEASE
                // berpasangan dengan REVERSAL ber-SettlementId sama". Diverifikasi langsung ke
                // BillingSettlementService.cs (HandleDepositTopUpReversalAsync): RELEASE.SettlementId
                // = original.SettlementId (settlement ALOKASI LAMA yang dibatalkan — transaksi
                // berbeda), sedangkan REVERSAL.SettlementId = tender.SettlementId (settlement
                // top-up HARI INI yang dibalik). Keduanya TIDAK PERNAH sama secara struktural —
                // diikuti literal, setiap RELEASE akan gagal berpasangan dan fitur ini nol pernah
                // menerbitkan kejadian, membatalkan maksud FIN-DEC-081 sepenuhnya.
                //
                // Kunci yang BENAR-BENAR sama pada kedua baris (diverifikasi baris yang sama):
                // RELEASE.CausationId = tender.CorrelationId, REVERSAL.CausationId =
                // tender.CorrelationId — keduanya ditulis dalam satu pemanggilan method yang sama
                // untuk satu tender yang dibalik. Ini pemasangan yang dipakai di sini. Keputusan
                // bisnis FIN-DEC-081 (pasangkan RELEASE dengan REVERSAL dari operasi pembalikan
                // yang sama, fail-closed bila tidak ada) TIDAK berubah — yang berubah murni kunci
                // teknis pemasangannya. Dilaporkan sebagai temuan pada laporan task; kontrak
                // FIN-VAL-145/146 MUST ditinjau ulang teksnya oleh pemilik desain.
                var hasPairedReversal = await _dbContext.BilDepositMovements.AsNoTracking()
                    .AnyAsync(x => !x.IsDelete
                        && x.MovementType == BillingDepositMovementTypes.Reversal
                        && x.CausationId == movement.CausationId, cancellationToken);

                if (!hasPairedReversal)
                {
                    // FIN-VAL-145: fail-closed. Arah jurnalnya tidak dapat ditentukan tanpa
                    // pasangan REVERSAL yang bersesuaian — Finance TIDAK MENEBAK.
                    throw new InvalidOperationException(
                        "Mutasi RELEASE tidak berpasangan dengan mutasi REVERSAL manapun dari operasi " +
                        "pembalikan tender yang sama. Lawan jurnalnya tidak dapat ditentukan — nol " +
                        "kejadian diterbitkan. Lihat FIN-OQ-037.");
                }

                // FIN-DES-064: SourceTransactionId = BilDepositMovement.Id (mutasi ini sendiri,
                // BUKAN tender/settlement); SourceVersion dipatok "1" — satu mutasi adalah satu
                // fakta yang tidak pernah berulang; Amount = Amount mutasi RELEASE (FIN-VAL-146:
                // tepat satu kejadian per baris RELEASE, bukan satu per operasi pembalikan —
                // BKC-DEC-133 menulis 1 baris RELEASE per alokasi yang dibatalkan, jadi satu
                // operasi dapat menghasilkan lebih dari satu kejadian ini, masing-masing dari
                // baris RELEASE-nya sendiri).
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PembalikanPemakaianUangMukaDeposit,
                    SourceTransactionId = movement.Id.ToString(),
                    SourceVersion = "1",
                    EventOccurredAt = movement.OccurredAt,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(movement.OccurredAt),
                    Amount = movement.Amount,
                    CorrelationId = movement.CorrelationId,
                    CausationId = movement.CausationId,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            // BE-FIN-046 (FIN-DES-057): mutasi pembalik atas TOP_UP adalah fakta bahwa uang muka
            // yang sempat tercatat ternyata TIDAK JADI diterima — uangnya ditarik kembali.
            //
            // TIDAK ADA RISIKO KEJADIAN GANDA dengan jalur penerimaan (BE-FIN-024), dan ini
            // diverifikasi ke source, bukan diasumsikan: BilConsumerHandoffService.PublishForTenderAsync
            // mengembalikan null bila settlement tidak punya InvoiceId — dan settlement top-up
            // deposit memang tidak punya. Tender top-up karena itu TIDAK PERNAH menghasilkan
            // BilCollectionHandoff, sehingga jalur FinReceipt tidak pernah menyentuh kasus ini.
            if (movement.MovementType == BillingDepositMovementTypes.Reversal)
            {
                var reversedMovement = movement.ReversesMovementId.HasValue
                    ? await _dbContext.BilDepositMovements.AsNoTracking()
                        .SingleOrDefaultAsync(x => x.Id == movement.ReversesMovementId!.Value && !x.IsDelete, cancellationToken)
                    : null;

                if (reversedMovement is null)
                {
                    // Fail-closed: tanpa penanda eksplisit, arah jurnalnya tidak dapat ditentukan.
                    // Finance TIDAK MENEBAK — lihat pola yang sama pada RELEASE di atas.
                    throw new InvalidOperationException(
                        "Mutasi REVERSAL tidak menyebut mutasi yang dibalikkannya (ReversesMovementId kosong " +
                        "atau tidak ditemukan), sehingga arah jurnalnya tidak dapat ditentukan. Nol kejadian " +
                        "diterbitkan — lihat FIN-OQ-037.");
                }

                if (reversedMovement.MovementType != BillingDepositMovementTypes.TopUp)
                {
                    // REVERSAL atas ALLOCATION adalah wilayah PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT,
                    // yang penulisnya BE-FIN-047 — BUKAN task ini. Ditolak fail-closed supaya
                    // faktanya tetap terlihat dan dapat diolah ulang begitu task itu selesai,
                    // bukan diam-diam ditandai CONSUMED tanpa kejadian.
                    throw new InvalidOperationException(
                        $"Mutasi REVERSAL atas '{reversedMovement.MovementType}' belum punya penulis kejadian " +
                        "di Finance — penulisnya BE-FIN-047 (PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT). Nol " +
                        "kejadian diterbitkan.");
                }

                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PembalikanPenerimaanUangMuka,
                    SourceTransactionId = movement.Id.ToString(),
                    SourceVersion = "1",
                    EventOccurredAt = movement.OccurredAt,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(movement.OccurredAt),
                    Amount = movement.Amount,
                    CorrelationId = movement.CorrelationId,
                    CausationId = movement.CausationId,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            // TOP_UP tidak pernah menjadi fakta yang diolah jalur ini — ia hanya dibaca pendeteksi
            // FIN-VAL-142 saat sinkronisasi. Baris intake bertipe TOP_UP hanya lahir sebagai
            // penanda ERROR, dan menandainya CONSUMED tanpa kejadian akan menyembunyikan lubang
            // yang justru ingin ditampakkan pendeteksi itu.
            if (movement.MovementType == BillingDepositMovementTypes.TopUp)
            {
                throw new InvalidOperationException(
                    "Mutasi TOP_UP tidak diolah sebagai fakta masuk. Baris ini lahir dari pendeteksi " +
                    "FIN-VAL-142 dan tetap ERROR sampai mutasi pembaliknya ada di sisi Billing (FIN-OQ-034).");
            }

            if (movement.MovementType == BillingDepositMovementTypes.Allocation)
            {
                // ALLOCATION -> PEMAKAIAN-UANG-MUKA-DEPOSIT tanpa FinReceipt baru.
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PemakaianUangMukaDeposit,
                    SourceTransactionId = movement.Id.ToString(),
                    SourceVersion = "1",
                    EventOccurredAt = movement.OccurredAt,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(movement.OccurredAt),
                    Amount = movement.Amount,
                    CorrelationId = movement.CorrelationId,
                    CausationId = movement.CausationId,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            // Keempat jenis berhenti di CONSUMED, tidak mengirim ACK ke Billing (nol tulisan ke tabel Bil*).
            intake.Status = FinBillingHandoffIntakeStatuses.Consumed;
            intake.TargetEntityId = null;
            intake.ConsumedAt = now;
            intake.ErrorMessage = null;
            intake.UpdateDateTime = DateTime.UtcNow;
            intake.UpdateBy = actorUserId;
            intake.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Intake.Consumed", intake.Id, actorUserId, null);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-025: Pengolahan kelebihan bayar (ALLOCATION_EXCESS / SETTLEMENT)
    // ------------------------------------------------------------------------------------

    private async Task ProcessRefundableCreditIntakeAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BILLING_INTAKE_{intakeId:N}", cancellationToken);

            var intake = await _dbContext.FinBillingHandoffIntakes
                .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
            if (intake.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
                throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");
            if (intake.HandoffType != FinBillingHandoffTypes.RefundableCredit)
                throw new InvalidOperationException($"Konsumen untuk HandoffType '{intake.HandoffType}' tidak cocok.");

            var credit = await _dbContext.BilRefundableCredits.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == intake.SourceHandoffId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Fakta kredit kelebihan bayar sumber tidak ditemukan di Billing.");

            if (credit.SourceType == BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin)
            {
                throw new InvalidOperationException(
                    $"Kredit berjenis '{credit.SourceType}' belum dapat diakui kejadiannya karena perlakuan akuntansinya belum ditetapkan (FIN-OQ-031).");
            }

            var now = DateTimeOffset.UtcNow;

            // Syarat ketiga FIN-DEC-067: kode ini MUST NOT terbit bila pembayaran asalnya PENERIMAAN-UANG-MUKA
            // (kelebihannya sudah tercatat di Uang Muka Pasien; menerbitkannya akan mencatat kewajiban dua kali).
            // FinReceipt tidak menyimpan EventTypeCode — pemicu PENERIMAAN-UANG-MUKA di
            // FinanceReceiptService adalah persis SourceInvoiceStatus == Open (lihat CreateAsync).
            var isFromAdvancePayment = await _dbContext.FinReceipts.AsNoTracking()
                .AnyAsync(r => !r.IsDelete
                    && r.InvoiceId == credit.InvoiceId
                    && r.SourceInvoiceStatus == BillingInvoiceStatuses.Open,
                    cancellationToken);

            if (!isFromAdvancePayment && credit.OriginalAmount > 0)
            {
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PengakuanKelebihanBayar,
                    SourceTransactionId = credit.Id.ToString(),
                    SourceVersion = "1",
                    EventOccurredAt = credit.RecognizedAt,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(credit.RecognizedAt),
                    Amount = credit.OriginalAmount,
                    CorrelationId = credit.InvoiceId,
                    CausationId = credit.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            intake.Status = FinBillingHandoffIntakeStatuses.Consumed;
            intake.TargetEntityId = null;
            intake.ConsumedAt = now;
            intake.ErrorMessage = null;
            intake.UpdateDateTime = DateTime.UtcNow;
            intake.UpdateBy = actorUserId;
            intake.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Intake.Consumed", intake.Id, actorUserId, null);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-025: Pengolahan pengembalian uang (BilRefundCase EXECUTED)
    // ------------------------------------------------------------------------------------

    private async Task ProcessRefundCaseIntakeAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BILLING_INTAKE_{intakeId:N}", cancellationToken);

            var intake = await _dbContext.FinBillingHandoffIntakes
                .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
            if (intake.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
                throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");
            if (intake.HandoffType != FinBillingHandoffTypes.RefundCase)
                throw new InvalidOperationException($"Konsumen untuk HandoffType '{intake.HandoffType}' tidak cocok.");

            var refundCase = await _dbContext.BilRefundCases.AsNoTracking()
                .Include(x => x.RefundableCredit)
                .SingleOrDefaultAsync(x => x.Id == intake.SourceHandoffId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Fakta pengembalian (refund case) sumber tidak ditemukan di Billing.");

            var credit = refundCase.RefundableCredit;
            if (credit is null && refundCase.RefundableCreditId.HasValue)
            {
                credit = await _dbContext.BilRefundableCredits.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == refundCase.RefundableCreditId.Value && !x.IsDelete, cancellationToken);
            }

            // FIN-VAL-141: Refund atas kredit REFERRED_OUTPATIENT_ADMIN ditulis sebagai baris intake ERROR, nol kejadian.
            if (credit is not null && credit.SourceType == BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin)
            {
                throw new InvalidOperationException(
                    $"Pengembalian untuk kredit berjenis '{credit.SourceType}' belum dapat diterbitkan kejadiannya karena perlakuan akuntansinya belum ditetapkan (FIN-VAL-141, FIN-OQ-031).");
            }

            var now = DateTimeOffset.UtcNow;
            var eventTime = refundCase.CompletedAt ?? refundCase.SubmittedAt;

            if (refundCase.RequestedAmount > 0)
            {
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PengembalianUangMuka,
                    SourceTransactionId = refundCase.Id.ToString(),
                    SourceVersion = "1",
                    EventOccurredAt = eventTime,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(eventTime),
                    Amount = refundCase.RequestedAmount,
                    CorrelationId = refundCase.CorrelationId,
                    CausationId = refundCase.CausationId,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            intake.Status = FinBillingHandoffIntakeStatuses.Consumed;
            intake.TargetEntityId = null;
            intake.ConsumedAt = now;
            intake.ErrorMessage = null;
            intake.UpdateDateTime = DateTime.UtcNow;
            intake.UpdateBy = actorUserId;
            intake.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Intake.Consumed", intake.Id, actorUserId, null);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-025: Pengolahan telaah selisih kas shift (BilCashVarianceReview)
    // ------------------------------------------------------------------------------------

    private async Task ProcessCashVarianceReviewIntakeAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BILLING_INTAKE_{intakeId:N}", cancellationToken);

            var intake = await _dbContext.FinBillingHandoffIntakes
                .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
            if (intake.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
                throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");
            if (intake.HandoffType != FinBillingHandoffTypes.CashVarianceReview)
                throw new InvalidOperationException($"Konsumen untuk HandoffType '{intake.HandoffType}' tidak cocok.");

            var review = await _dbContext.BilCashVarianceReviews.AsNoTracking()
                .Include(x => x.Shift)
                .SingleOrDefaultAsync(x => x.Id == intake.SourceHandoffId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Fakta telaah selisih kas sumber tidak ditemukan di Billing.");

            var shift = review.Shift ?? await _dbContext.BilCashierShifts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == review.ShiftId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Shift kasir sumber tidak ditemukan.");

            var now = DateTimeOffset.UtcNow;

            // FIN-DES-053:
            // 1. Kapan terbit: saat shift mencapai REVIEWED (baris review yang MENYELESAIKAN selisihnya).
            //    Baris NEEDS_FOLLOW_UP (shift PERLU_TINDAK_LANJUT) -> nol kejadian.
            // 2. Variance == 0 -> nol kejadian.
            // 3. Kode: SELISIH-KAS-KURANG bila Variance < 0; SELISIH-KAS-LEBIH bila Variance > 0.
            // 4. Amount: Math.Abs(shift.Variance) — selalu positif.
            // 5. AccountingDate: tanggal shift (OpenedAt).
            // 6. SourceTransactionId: BilCashierShift.Id (Bukan BilCashVarianceReview.Id).
            // 7. SourceVersion: dipatok "1".
            // 8. CorrelationId: shift.Id, CausationId: review.Id.
            if (shift.Status == CashierShiftStatuses.Reviewed && shift.Variance != 0)
            {
                var eventTypeCode = shift.Variance < 0
                    ? FinAccountingEventTypeCodes.SelisihKasKurang
                    : FinAccountingEventTypeCodes.SelisihKasLebih;

                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = eventTypeCode,
                    SourceTransactionId = shift.Id.ToString(),
                    SourceVersion = "1",
                    EventOccurredAt = review.ReviewedAt,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(shift.OpenedAt),
                    Amount = Math.Abs(shift.Variance),
                    CorrelationId = shift.Id,
                    CausationId = review.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            intake.Status = FinBillingHandoffIntakeStatuses.Consumed;
            intake.TargetEntityId = null;
            intake.ConsumedAt = now;
            intake.ErrorMessage = null;
            intake.UpdateDateTime = DateTime.UtcNow;
            intake.UpdateBy = actorUserId;
            intake.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Intake.Consumed", intake.Id, actorUserId, null);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-097, FIN-DEC-173/187/195/200, INT-BIL-FIN-002 §P.6 — Koreksi pemilik manfaat salah
    // orang: batalkan kartu piutang lama (mutasi pembalik, BUKAN edit manual debitur), terbitkan
    // kartu baru atas pegawai yang benar.
    //
    // TEMUAN BLOCKER, dicatat karena menentukan apa yang BENAR-BENAR terjadi saat method ini
    // berjalan: diverifikasi ke BillingArApHandoffService.cs — satu-satunya jalur Billing yang
    // menulis BilHandoffAdjustment HANYA mencatat baris penyesuaian untuk refund/write-off
    // invoice umum; Billing TIDAK PERNAH menerbitkan BilArHandoff baru sebagai pasangannya, dan
    // tidak ada field eksplisit yang menautkan adjustment ke handoff pengganti selain berbagi
    // CorrelationId (pola yang sama dipakai handoff↔intake di tempat lain pada file ini). Karena
    // itu `newHandoff` di bawah MEMANG tidak akan pernah ditemukan sampai Billing membangun jalur
    // penerbitan BilArHandoff pengganti untuk skenario ini — bukan bug di sini, dicatat sebagai
    // gap lintas modul di laporan task, BUKAN diam-diam diasumsikan sudah berjalan.
    //
    // Restitusi payroll (langkah 3 kontrak §P.6 — event pembalikan angsuran ke HR) SENGAJA BELUM
    // diimplementasikan pada task ini: butuh menentukan apakah cicilan atas pegawai lama sempat
    // terpotong (FinReceivableInstallment.PaidAmount > 0), lalu menulis entri pembalik HR —
    // kombinasi keputusan desain yang belum sempat dikerjakan dalam anggaran task ini. Dicatat
    // NOT IMPLEMENTED pada laporan, bukan ditulis asal agar terlihat lengkap.
    // ------------------------------------------------------------------------------------

    private async Task ProcessArAdjustmentIntakeAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BILLING_INTAKE_{intakeId:N}", cancellationToken);

            var intake = await _dbContext.FinBillingHandoffIntakes
                .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
            if (intake.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
                throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");
            if (intake.HandoffType != FinBillingHandoffTypes.Adjustment)
                throw new InvalidOperationException($"Konsumen untuk HandoffType '{intake.HandoffType}' tidak cocok.");

            var adjustment = await _dbContext.BilHandoffAdjustments
                .SingleOrDefaultAsync(x => x.Id == intake.SourceHandoffId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Penyesuaian serah terima sumber tidak ditemukan di Billing.");

            var oldHandoff = await _dbContext.BilArHandoffs
                .SingleOrDefaultAsync(x => adjustment.ArHandoffId.HasValue && x.Id == adjustment.ArHandoffId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Serah terima AR lama milik penyesuaian ini tidak ditemukan.");

            var now = DateTimeOffset.UtcNow;

            var oldReceivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => !x.IsDelete && x.SourceHandoffKey == oldHandoff.HandoffKey, cancellationToken);

            if (oldReceivable is not null && oldReceivable.Status != FinReceivableStatuses.Cancelled)
            {
                var balanceBefore = oldReceivable.OutstandingAmount;
                oldReceivable.OutstandingAmount = 0m;
                oldReceivable.AdjustedAmount += balanceBefore;
                oldReceivable.Status = FinReceivableStatuses.Cancelled;
                oldReceivable.UpdateDateTime = DateTime.UtcNow;
                oldReceivable.UpdateBy = actorUserId;
                oldReceivable.RowVersion = Guid.NewGuid();

                if (balanceBefore != 0)
                {
                    await _subledgerMovementService.RecordReceivableMovementAsync(
                        receivable: oldReceivable,
                        movementType: FinReceivableMovementTypes.Penyesuaian,
                        deltaAmount: -balanceBefore,
                        balanceBefore: balanceBefore,
                        occurredAt: now,
                        actorUserId: actorUserId,
                        correlationId: adjustment.CorrelationId,
                        causationId: adjustment.Id,
                        referenceNumber: oldReceivable.ReceivableNumber,
                        notes: $"Pembatalan piutang — {adjustment.Reason} (BE-FIN-097)",
                        cancellationToken: cancellationToken);
                }
            }

            // Lihat catatan blocker di atas kelas method ini — pencarian ini akan tetap kosong
            // sampai Billing membangun jalur penerbitan BilArHandoff pengganti.
            var newHandoff = await _dbContext.BilArHandoffs
                .SingleOrDefaultAsync(x => !x.IsDelete && x.CorrelationId == adjustment.CorrelationId && x.Id != oldHandoff.Id, cancellationToken);

            Guid? targetEntityId = oldReceivable?.Id;

            if (newHandoff is not null
                && !await _dbContext.FinReceivables.AnyAsync(x => !x.IsDelete && x.SourceHandoffKey == newHandoff.HandoffKey, cancellationToken))
            {
                var newInvoice = await _dbContext.BilInvoices.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == newHandoff.InvoiceId, cancellationToken);
                Guid? newPatientId = null;
                if (newInvoice is not null)
                {
                    newPatientId = await _dbContext.RegPatientEncounters.AsNoTracking()
                        .Where(x => x.Id == newInvoice.EncounterId)
                        .Select(x => (Guid?)x.PatientId)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                var newReceivable = new FinReceivable
                {
                    ReceivableNumber = GenerateReceivableNumber(),
                    SourceHandoffKey = newHandoff.HandoffKey,
                    SourceHandoffId = newHandoff.Id,
                    InvoiceId = newHandoff.InvoiceId,
                    DebtorType = newHandoff.DebtorType,
                    DebtorReferenceId = newHandoff.DebtorReferenceId,
                    BenefitOwnerId = newHandoff.BenefitOwnerId,
                    BenefitRelationship = newHandoff.BenefitRelationship,
                    OriginalAmount = newHandoff.Amount,
                    OutstandingAmount = newHandoff.Amount,
                    DueDate = newHandoff.DueDate.HasValue ? FinanceBusinessDate.ToDateOnly(newHandoff.DueDate.Value) : FinanceBusinessDate.ToDateOnly(now),
                    Status = FinReceivableStatuses.Outstanding,
                    ClaimStatus = FinReceivableClaimStatuses.NotRequired,
                    RecognizedAt = now,
                    CorrelationId = adjustment.CorrelationId,
                    CausationId = adjustment.Id,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                newReceivable.Items.Add(new FinReceivableItem
                {
                    EncounterId = newInvoice?.EncounterId,
                    InvoiceId = newHandoff.InvoiceId,
                    PatientId = newPatientId,
                    Description = $"Piutang pengganti — koreksi salah orang ({adjustment.Reason})",
                    Amount = newHandoff.Amount,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
                _dbContext.FinReceivables.Add(newReceivable);

                await _subledgerMovementService.RecordReceivableMovementAsync(
                    receivable: newReceivable,
                    movementType: FinReceivableMovementTypes.Pengakuan,
                    deltaAmount: newReceivable.OriginalAmount,
                    balanceBefore: 0m,
                    occurredAt: now,
                    actorUserId: actorUserId,
                    correlationId: adjustment.CorrelationId,
                    causationId: adjustment.Id,
                    referenceNumber: newReceivable.ReceivableNumber,
                    notes: $"Pengakuan piutang pengganti dari koreksi salah orang {adjustment.Id}",
                    cancellationToken: cancellationToken);

                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PengakuanPiutang,
                    SourceTransactionId = newReceivable.ReceivableNumber,
                    EventOccurredAt = now,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(now),
                    Amount = newReceivable.OriginalAmount,
                    CorrelationId = adjustment.CorrelationId,
                    CausationId = adjustment.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);

                newHandoff.Status = BillingHandoffStatuses.Acknowledged;
                newHandoff.AcknowledgedAt = now;
                newHandoff.RowVersion = Guid.NewGuid();

                targetEntityId = newReceivable.Id;
            }

            // TODO BE-FIN-097 (belum diimplementasikan, lihat catatan di atas kelas method ini):
            // restitusi payroll — event pembalikan angsuran ke HR bila cicilan pegawai lama sempat
            // terpotong (FIN-DEC-195).

            intake.Status = newHandoff is not null ? FinBillingHandoffIntakeStatuses.Acknowledged : FinBillingHandoffIntakeStatuses.Consumed;
            intake.TargetEntityId = targetEntityId;
            intake.ConsumedAt = now;
            if (newHandoff is not null) intake.AcknowledgedAt = now;
            intake.ErrorMessage = null;
            intake.UpdateDateTime = DateTime.UtcNow;
            intake.UpdateBy = actorUserId;
            intake.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Intake.Adjustment.Processed", intake.Id, actorUserId, targetEntityId);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // BE-FIN-045: Penanda penutupan shift kasir (FIN-DES-054, FIN-DEC-070/072/073/075)
    // BE-FIN-070: Penanda pembukaan shift kasir (FIN-DEC-115, FIN-DEC-121, FIN-DES-084)
    //
    // BENTUK YANG DIPILIH, dan alasannya — dicatat karena desain menetapkan pemicu, nilai, dan
    // aturan versinya, tetapi TIDAK menetapkan dari mana penanda ini ditulis:
    //
    //   Jalur ini SENGAJA TIDAK memakai baris FinBillingHandoffIntake. Dua sebab:
    //   (a) Kunci dedup intake adalah (HandoffType, SourceHandoffKey) — satu baris per sumber
    //       selamanya. Padahal satu shift MEMANG boleh ditutup dan dibuka berulang kali, dan
    //       setiap siklus adalah kejadian tersendiri (FIN-DES-054). Kunci itu justru akan
    //       menghalangi siklus kedua dan seterusnya.
    //   (b) Jenis handoff baru menuntut perubahan check constraint HandoffType, yaitu migration
    //       pada tabel yang sudah berjalan — dan itu BUKAN bagian cakupan task ini.
    //
    //   Sebagai gantinya, idempotensi diambil dari kotak keluar itu sendiri: unique index dua
    //   lapis (SourceModule, SourceTransactionId, EventTypeCode, SourceVersion) sudah cukup,
    //   asalkan SourceVersion dipatok PER SIKLUS. Nomor siklus diturunkan dari isi kotak keluar,
    //   bukan dari kolom status baru: jumlah penanda pembalik yang sudah terbit untuk shift itu.
    //
    // Shift kasir DIBACA SAJA (FIN-CAP-024, FIN-OOS-001..004) — nol tulisan ke tabel Bil* mana pun.
    // ------------------------------------------------------------------------------------

    public async Task<CashierShiftClosureMarkerSyncResult> SyncCashierShiftClosureMarkersAsync(
        Guid actorUserId, CancellationToken cancellationToken)
    {
        // BE-FIN-070, FIN-DEC-121: Diperluas dari 3 menjadi 7 status.
        // "Final" = CLOSED atau REVIEWED (penanda penutupan diterbitkan).
        // "Belum final" = OPEN, HANDED_OVER, REOPENED, CLOSED_WITH_VARIANCE, PERLU_TINDAK_LANJUT
        //                 (penanda PEMBUKAAN-SHIFT-KASIR diterbitkan, menahan tutup bulan Accounting).
        //
        // Status OPEN sengaja TIDAK dikecualikan — FIN-DEC-121 menyatakan seluruh status
        // selain CLOSED dan REVIEWED adalah "belum final" dan HARUS diterbitkan penanda pembukaannya.
        // Dengan demikian Accounting dapat mendeteksi shift yang sama sekali belum ditutup.
        var shifts = await _dbContext.BilCashierShifts.AsNoTracking()
            .Where(x => !x.IsDelete && (
                x.Status == CashierShiftStatuses.Open ||
                x.Status == CashierShiftStatuses.HandedOver ||
                x.Status == CashierShiftStatuses.Closed ||
                x.Status == CashierShiftStatuses.ClosedWithVariance ||
                x.Status == CashierShiftStatuses.Reviewed ||
                x.Status == CashierShiftStatuses.Reopened ||
                x.Status == CashierShiftStatuses.PerluTindakLanjut))
            .ToListAsync(cancellationToken);

        if (shifts.Count == 0)
            return new CashierShiftClosureMarkerSyncResult(0, 0, 0);

        var shiftKeys = shifts.Select(x => x.Id.ToString()).ToList();

        var existingMarkers = await _dbContext.Set<FinAccountingEventOutbox>().AsNoTracking()
            .Where(x => !x.IsDelete
                && x.SourceModule == FinAccountingEventSourceModules.Finance
                && shiftKeys.Contains(x.SourceTransactionId)
                && (x.EventTypeCode == FinAccountingEventTypeCodes.PenutupanShiftKasir
                 || x.EventTypeCode == FinAccountingEventTypeCodes.PembalikanPenutupanShiftKasir
                 || x.EventTypeCode == FinAccountingEventTypeCodes.PembukaanShiftKasir))
            .Select(x => new { x.SourceTransactionId, x.EventTypeCode, x.SourceVersion })
            .ToListAsync(cancellationToken);

        var closureCounts = existingMarkers
            .Where(x => x.EventTypeCode == FinAccountingEventTypeCodes.PenutupanShiftKasir)
            .GroupBy(x => x.SourceTransactionId)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        var reversalCounts = existingMarkers
            .Where(x => x.EventTypeCode == FinAccountingEventTypeCodes.PembalikanPenutupanShiftKasir)
            .GroupBy(x => x.SourceTransactionId)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        // BE-FIN-070: Kumpulan kunci idempotensi penanda PEMBUKAAN-SHIFT-KASIR berbentuk
        // "<shiftId>:<SourceVersion>" sehingga satu shift multi-siklus tidak menggandakan baris.
        var openingMarkerKeys = existingMarkers
            .Where(x => x.EventTypeCode == FinAccountingEventTypeCodes.PembukaanShiftKasir)
            .Select(x => $"{x.SourceTransactionId}:{x.SourceVersion}")
            .ToHashSet(StringComparer.Ordinal);

        var closureIssued = 0;
        var reversalIssued = 0;
        var openingIssued = 0;

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);

            foreach (var shift in shifts)
            {
                var key = shift.Id.ToString();
                closureCounts.TryGetValue(key, out var closureCount);
                reversalCounts.TryGetValue(key, out var reversalCount);

                if (shift.Status is CashierShiftStatuses.Closed or CashierShiftStatuses.Reviewed)
                {
                    // BE-FIN-062, FIN-DES-081: Shift yang mencapai CLOSED/REVIEWED menulis mutasi kas KAS-SHIFT (Sumber 1)
                    var shiftCash = FinanceCashManagementService.GetShiftCash(shift);
                    if (shiftCash > 0m)
                    {
                        await _subledgerMovementService.RecordCashMovementAsync(
                            movementType: FinCashMovementTypes.KasShift,
                            direction: FinCashMovementDirections.In,
                            amount: shiftCash,
                            businessDate: FinanceBusinessDate.ToDateOnly(shift.OpenedAt),
                            occurredAt: shift.ClosedAt ?? shift.OpenedAt,
                            sourceReferenceType: FinCashMovementSourceReferenceTypes.CashierShift,
                            sourceReferenceId: key,
                            actorUserId: actorUserId,
                            correlationId: shift.Id,
                            causationId: shift.Id,
                            cashierShiftId: shift.Id,
                            notes: $"Penerimaan kas kasir shift {shift.ShiftNumber}",
                            ignoreDuplicate: true,
                            cancellationToken: cancellationToken);
                    }

                    // Siklus berjalan = jumlah pembalik yang sudah terbit + 1. Bila penanda untuk
                    // siklus ini sudah ada, tidak ada yang dikerjakan — inilah idempotensinya.
                    var cycle = reversalCount + 1;
                    if (closureCount >= cycle) continue;

                    await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                    {
                        EventTypeCode = FinAccountingEventTypeCodes.PenutupanShiftKasir,
                        SourceTransactionId = key,
                        SourceVersion = cycle.ToString(),
                        // Waktu kejadian: saat shift ditutup. ClosedAt terisi CloseAsync; jatuh
                        // balik ke OpenedAt hanya bila data lama tidak memilikinya.
                        EventOccurredAt = shift.ClosedAt ?? shift.OpenedAt,
                        // FIN-DES-054: AccountingDate adalah TANGGAL SHIFT, bukan tanggal tutup —
                        // konvensi yang sama dengan SELISIH-KAS-* (BE-FIN-025).
                        AccountingDate = FinanceBusinessDate.ToDateOnly(shift.OpenedAt),
                        // Amount = 0: penanda status, bukan transaksi. Diterima ValidateRequest
                        // HANYA karena kode ini ada pada daftar tertutup ZeroAmountAllowedEventTypes
                        // (FIN-VAL-138, BE-FIN-023). FIN-DEC-075 MENOLAK jalan pintas nilai
                        // simbolis non-nol — angka palsu di buku besar lebih berbahaya daripada
                        // baris PENDING yang menunggu.
                        Amount = 0m,
                        CorrelationId = shift.Id,
                        CausationId = shift.Id,
                        ActorUserId = actorUserId
                    }, cancellationToken);

                    closureIssued++;
                }
                else if (shift.Status == CashierShiftStatuses.Reopened)
                {
                    // REOPENED: terbitkan pembalik hanya bila siklus yang dibuka itu memang sudah
                    // pernah menerbitkan penanda penutupan. Shift yang dibuka kembali tanpa pernah
                    // tertutup di mata Accounting tidak punya apa pun untuk dibalik.
                    if (closureCount <= reversalCount) continue;

                    await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                    {
                        EventTypeCode = FinAccountingEventTypeCodes.PembalikanPenutupanShiftKasir,
                        SourceTransactionId = key,
                        SourceVersion = (reversalCount + 1).ToString(),
                        // BilCashierShift tidak menyimpan waktu pembukaan kembali, sehingga waktu
                        // kejadian diambil saat sinkronisasi ini berjalan. Dicatat sebagai
                        // keterbatasan pada laporan task, bukan ditebak dari kolom lain.
                        EventOccurredAt = DateTimeOffset.UtcNow,
                        AccountingDate = FinanceBusinessDate.ToDateOnly(shift.OpenedAt),
                        Amount = 0m,
                        CorrelationId = shift.Id,
                        CausationId = shift.Id,
                        ActorUserId = actorUserId
                    }, cancellationToken);

                    reversalIssued++;

                    // BE-FIN-070, FIN-DEC-121: Setelah menerbitkan pembalik penutupan untuk siklus
                    // lama, terbitkan juga penanda PEMBUKAAN-SHIFT-KASIR untuk siklus baru yang dimulai
                    // oleh pembukaan kembali ini. Urutan: PEMBALIKAN-PENUTUPAN dulu, PEMBUKAAN sesudahnya.
                    // SourceVersion penanda pembukaan = jumlah pembalik yang sudah terbit (setelah
                    // penambahan di atas) = reversalCount + 1 = siklus baru ini.
                    var newOpeningCycle = (reversalCount + 1).ToString();
                    var openingKey = $"{key}:{newOpeningCycle}";
                    if (!openingMarkerKeys.Contains(openingKey))
                    {
                        await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                        {
                            EventTypeCode = FinAccountingEventTypeCodes.PembukaanShiftKasir,
                            SourceTransactionId = key,
                            SourceVersion = newOpeningCycle,
                            EventOccurredAt = DateTimeOffset.UtcNow,
                            AccountingDate = FinanceBusinessDate.ToDateOnly(shift.OpenedAt),
                            // Amount = 0: penanda status — diterima ValidateRequest karena kode ini
                            // ada dalam ZeroAmountAllowedEventTypes (FIN-VAL-138, FIN-DEC-115).
                            Amount = 0m,
                            CorrelationId = shift.Id,
                            CausationId = shift.Id,
                            ActorUserId = actorUserId
                        }, cancellationToken);

                        openingIssued++;
                    }
                }
                else
                {
                    // BE-FIN-070, FIN-DEC-121: Status "belum final": OPEN, HANDED_OVER,
                    // CLOSED_WITH_VARIANCE, PERLU_TINDAK_LANJUT.
                    // Terbitkan PEMBUKAAN-SHIFT-KASIR untuk siklus saat ini bila belum ada.
                    // SourceVersion = jumlah pembalik yang sudah terbit + 1 (pola siklus yang sama).
                    var cycle = reversalCount + 1;
                    var openingKey = $"{key}:{cycle}";
                    if (openingMarkerKeys.Contains(openingKey)) continue;

                    await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                    {
                        EventTypeCode = FinAccountingEventTypeCodes.PembukaanShiftKasir,
                        SourceTransactionId = key,
                        SourceVersion = cycle.ToString(),
                        EventOccurredAt = DateTimeOffset.UtcNow,
                        AccountingDate = FinanceBusinessDate.ToDateOnly(shift.OpenedAt),
                        // Amount = 0: penanda status, bukan transaksi. FIN-DEC-115 menetapkan
                        // penanda ini sebagai sinyal keberadaan shift yang belum final.
                        Amount = 0m,
                        CorrelationId = shift.Id,
                        CausationId = shift.Id,
                        ActorUserId = actorUserId
                    }, cancellationToken);

                    openingIssued++;
                }
            }

            if (closureIssued + reversalIssued + openingIssued > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await CommitAsync(transaction, cancellationToken);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }

        if (closureIssued + reversalIssued + openingIssued > 0)
        {
            // AuditAsync milik kelas ini berbentuk per-intake (parameter keempatnya ReceivableId),
            // sedangkan jalur ini tidak punya baris intake. Karena itu logger dipanggil langsung
            // dengan bentuk yang sesuai — bukan memaksakan helper yang artinya berbeda.
            await _loggerService.AuditAsync(LogCategory, "FinanceBillingIntake.CashierShiftClosureMarker.Sync",
                $"Penanda shift kasir diterbitkan. Penutupan={closureIssued} Pembalik={reversalIssued} Pembukaan={openingIssued}",
                new { ClosureIssued = closureIssued, ReversalIssued = reversalIssued, OpeningIssued = openingIssued, ActorUserId = actorUserId });
        }

        return new CashierShiftClosureMarkerSyncResult(closureIssued, reversalIssued, openingIssued);
    }

    private async Task MarkErrorAsync(Guid intakeId, string errorMessage, Guid actorUserId, CancellationToken cancellationToken)
    {
        var intake = await _dbContext.FinBillingHandoffIntakes.SingleAsync(x => x.Id == intakeId, cancellationToken);
        intake.Status = FinBillingHandoffIntakeStatuses.Error;
        intake.ErrorMessage = Truncate(errorMessage, 1000);
        intake.RetryCount += 1;
        intake.UpdateDateTime = DateTime.UtcNow;
        intake.UpdateBy = actorUserId;
        intake.RowVersion = Guid.NewGuid();
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Intake.Error", intake.Id, actorUserId, null);
    }

    // ------------------------------------------------------------------------------------
    // Infrastruktur bersama
    // ------------------------------------------------------------------------------------

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    // Nomor piutang tidak punya penomor seri resmi di modul ini (BillingNumberSeriesService
    // adalah milik BillingManagement) — dibuat unik lewat Guid, bukan Count/Max/Last+1
    // (QBE-CODE-002/003), sampai ada keputusan skema penomoran resmi untuk Finance.
    private static string GenerateReceivableNumber()
    {
        var candidate = $"AR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string Truncate(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    private Task AuditAsync(string action, Guid intakeId, Guid actorUserId, Guid? receivableId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceBillingIntake.{action}",
            $"Transisi fakta masuk Billing dicatat. IntakeId={intakeId} ReceivableId={receivableId?.ToString() ?? "-"}",
            new { IntakeId = intakeId, ReceivableId = receivableId, ActorUserId = actorUserId });

    private static BillingIntakeResponse Map(FinBillingHandoffIntake x) => new()
    {
        Id = x.Id,
        HandoffType = x.HandoffType,
        SourceHandoffId = x.SourceHandoffId,
        SourceHandoffKey = x.SourceHandoffKey,
        Status = x.Status,
        TargetEntityId = x.TargetEntityId,
        ConsumedAt = x.ConsumedAt,
        AcknowledgedAt = x.AcknowledgedAt,
        RetryCount = x.RetryCount,
        ErrorMessage = x.ErrorMessage,
        CorrelationId = x.CorrelationId,
        RowVersion = x.RowVersion,
        CreateDateTime = x.CreateDateTime
    };
}

public sealed class BillingIntakeValidationException(string message) : Exception(message);

/// <summary>
/// Hasil SyncCashierShiftClosureMarkersAsync (BE-FIN-045, BE-FIN-070). Ketiganya menghitung baris
/// kotak keluar yang BENAR-BENAR diterbitkan pada pemanggilan itu — shift yang penandanya sudah ada
/// tidak dihitung, karena jalur ini idempoten.
/// </summary>
public sealed record CashierShiftClosureMarkerSyncResult(int ClosureIssued, int ReversalIssued, int OpeningIssued);
