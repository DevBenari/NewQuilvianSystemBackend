using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingFinalizationService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly IBillingChargeSourceAdapter _chargeSourceAdapter;
    private readonly BillingArApHandoffService _arApHandoffService;
    private readonly BillingInvoiceClosureService _closureService;
    private readonly BilConsumerHandoffService _consumerHandoffService;
    private readonly LoggerService _loggerService;
    private readonly IBillingCalculationService? _calculationService;

    public BillingFinalizationService(
        ApplicationDbContext dbContext,
        IBillingChargeSourceAdapter chargeSourceAdapter,
        BillingArApHandoffService arApHandoffService,
        BillingInvoiceClosureService closureService,
        BilConsumerHandoffService consumerHandoffService,
        LoggerService loggerService,
        IBillingCalculationService? calculationService = null)
    {
        _dbContext = dbContext;
        _chargeSourceAdapter = chargeSourceAdapter;
        _arApHandoffService = arApHandoffService;
        _closureService = closureService;
        _consumerHandoffService = consumerHandoffService;
        _loggerService = loggerService;
        _calculationService = calculationService;
    }

    public async Task<FinalizationPreviewResponse> PreviewAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");
        return await BuildReadinessAsync(invoice, cancellationToken);
    }

    public async Task<FinalizationResponse> FinalizeAsync(
        Guid invoiceId,
        FinalizeInvoiceRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateFinalizeRequest(request, actorUserId);
        var payloadHash = ComputeFinalizePayloadHash(invoiceId, request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_INVOICE_LEDGER_{invoiceId:N}", cancellationToken);
            }

            var prior = await _dbContext.BilFinalizationRecords
                .Include(x => x.Invoice)
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash)
                    throw new BillingFinalizationConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditFinalizationAsync(prior, actorUserId, true);
                return Map(prior, true);
            }

            if (await _dbContext.BilFinalizationRecords.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingFinalizationConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var invoice = await _dbContext.BilInvoices
                .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");
            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingFinalizationConflictException(
                    "Invoice tidak lagi berstatus OPEN untuk difinalisasi.");
            if (invoice.RowVersion != request.ExpectedRowVersion)
                throw new BillingFinalizationConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.",
                    code: "STALE_INVOICE_ROW_VERSION",
                    currentRowVersion: invoice.RowVersion);

            var readiness = await BuildReadinessAsync(invoice, cancellationToken);
            var isDepartureException = !string.IsNullOrWhiteSpace(request.DepartureReason);
            // BE-RWI-155 / INV-RWF-08 / VAL-RWF-16: invoice "perlu diperiksa" tidak pernah
            // difinalkan, termasuk lewat departure exception.
            if (readiness.BlockingCodes.Contains(BillingFinalizationBlockingCodes.RequiresReview))
                throw new BillingFinalizationBlockedException(
                    "Invoice ini perlu diperiksa sebelum difinalkan.", readiness);
            // BE-RWI-155 / RWI-DEC-192 butir (d) / VAL-RWF-17: tidak ada baris "tarif belum ada".
            if (readiness.BlockingCodes.Contains(BillingFinalizationBlockingCodes.TariffNotFound))
                throw new BillingFinalizationBlockedException(
                    "Masih ada layanan yang tarifnya belum diatur. Lengkapi master tarif sebelum memfinalkan.",
                    readiness);
            if (!readiness.AllOrdersComplete || !readiness.CalculationCurrent)
                throw new BillingFinalizationBlockedException(
                    "Invoice belum siap difinalisasi.", readiness);
            if (!isDepartureException && readiness.Outstanding > 0)
                throw new BillingFinalizationBlockedException(
                    "Tanggung jawab pasien belum lunas; ajukan write-off/adjustment atau catat departure exception untuk melanjutkan.",
                    readiness);

            var now = DateTimeOffset.UtcNow;
            var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
                .SingleAsync(
                    x => x.InvoiceId == invoice.Id && x.VersionNo == readiness.CalculationVersion,
                    cancellationToken);

            var (record, closureChange) = await ExecuteInternalFinalizationAndClosureAsync(
                invoice,
                calculation,
                readiness.CalculationVersion,
                readiness.Outstanding,
                isDepartureException,
                request.DepartureReason,
                request.DebtorIdentity,
                request.DebtorRelationship,
                request.Reason,
                idempotencyKey,
                payloadHash,
                request.CorrelationId,
                request.CausationId,
                actorUserId,
                now,
                cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditFinalizationAsync(record, actorUserId, false);
            if (closureChange.Changed)
                await AuditClosureChangeAsync(closureChange, actorUserId);
            return Map(record, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinalizationConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.",
                exception,
                code: "DB_CONCURRENCY_CONFLICT");
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinalizationConflictException(
                "Finalisasi tidak dapat disimpan karena target, correlation, atau idempotency key sudah diproses.",
                exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<CompleteInvoiceResponse> CompleteInvoiceWithoutPatientPaymentAsync(
        Guid invoiceId,
        CompleteInvoiceRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateCompleteRequest(request, actorUserId);
        var payloadHash = ComputeCompletePayloadHash(invoiceId, request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_INVOICE_LEDGER_{invoiceId:N}", cancellationToken);
            }

            var prior = await _dbContext.BilFinalizationRecords
                .Include(x => x.Invoice)
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash)
                    throw new BillingFinalizationConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditFinalizationAsync(prior, actorUserId, true);
                return await MapCompleteResponseAsync(prior, true, cancellationToken);
            }

            if (await _dbContext.BilFinalizationRecords.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingFinalizationConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var invoice = await _dbContext.BilInvoices
                .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");

            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingFinalizationConflictException(
                    "Invoice tidak lagi berstatus OPEN untuk diselesaikan.");
            if (invoice.RowVersion != request.ExpectedRowVersion)
            {
                await _loggerService.AuditAsync(
                    LogCategory,
                    "BillingFinalization.ConcurrencyMismatch",
                    "Penyelesaian invoice ditolak karena ExpectedRowVersion tidak cocok dengan RowVersion terkini di server.",
                    new
                    {
                        InvoiceId = invoice.Id,
                        ExpectedRowVersion = request.ExpectedRowVersion,
                        ActualRowVersion = invoice.RowVersion,
                        CurrentCalculationVersion = invoice.CurrentCalculationVersion,
                        CorrelationId = request.CorrelationId
                    });

                throw new BillingFinalizationConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.",
                    code: "STALE_INVOICE_ROW_VERSION",
                    currentRowVersion: invoice.RowVersion);
            }

            // Pastikan perhitungan authoritative tersedia (jika version == 0, lakukan rekalkulasi terlebih dahulu)
            if (invoice.CurrentCalculationVersion <= 0)
            {
                if (_calculationService is null)
                    throw new BillingFinalizationValidationException("Layanan kalkulasi tidak tersedia untuk perhitungan tagihan.");

                await _calculationService.RecalculateAsync(
                    invoice.Id,
                    new RecalculateInvoiceRequest
                    {
                        ExpectedRowVersion = invoice.RowVersion,
                        Reason = "Perhitungan authoritative awal sebelum penyelesaian invoice."
                    },
                    actorUserId,
                    cancellationToken);

                await _dbContext.Entry(invoice).ReloadAsync(cancellationToken);
            }

            var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
                .SingleOrDefaultAsync(x => x.InvoiceId == invoice.Id
                    && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete, cancellationToken)
                ?? throw new BillingFinalizationValidationException(
                    "Invoice belum memiliki hasil perhitungan terkini.");

            var isCurrent = await IsCalculationCurrentAsync(invoice.Id, calculation, cancellationToken);
            if (!isCurrent)
            {
                if (_calculationService is not null)
                {
                    await _calculationService.RecalculateAsync(
                        invoice.Id,
                        new RecalculateInvoiceRequest
                        {
                            ExpectedRowVersion = invoice.RowVersion,
                            Reason = "Penyegaran kalkulasi sebelum penyelesaian invoice."
                        },
                        actorUserId,
                        cancellationToken);

                    await _dbContext.Entry(invoice).ReloadAsync(cancellationToken);
                    calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
                        .SingleAsync(x => x.InvoiceId == invoice.Id
                            && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete, cancellationToken);
                }
            }

            var readiness = await BuildReadinessAsync(invoice, cancellationToken);
            if (!readiness.AllOrdersComplete)
                throw new BillingFinalizationBlockedException(
                    "Invoice belum dapat diselesaikan karena masih terdapat order yang belum selesai.",
                    readiness);
            if (!readiness.CalculationCurrent)
                throw new BillingFinalizationBlockedException(
                    "Invoice belum memiliki hasil perhitungan terkini.",
                    readiness);
            if (readiness.Outstanding > 0 || calculation.PatientAmount > 0)
                throw new BillingFinalizationBlockedException(
                    "Invoice memiliki kewajiban pasien; gunakan alur pembayaran normal.",
                    readiness);

            var now = DateTimeOffset.UtcNow;
            var (record, closureChange) = await ExecuteInternalFinalizationAndClosureAsync(
                invoice,
                calculation,
                readiness.CalculationVersion,
                outstandingAtFinalization: 0m,
                isDepartureException: false,
                departureReason: null,
                debtorIdentity: null,
                debtorRelationship: null,
                reason: BillingFinalizationReasons.AutoNoPatientPayment,
                idempotencyKey: idempotencyKey,
                payloadHash: payloadHash,
                correlationId: request.CorrelationId,
                causationId: request.CausationId,
                actorUserId: actorUserId,
                now: now,
                cancellationToken: cancellationToken);

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditFinalizationAsync(record, actorUserId, false);
            if (closureChange.Changed)
                await AuditClosureChangeAsync(closureChange, actorUserId);

            return await MapCompleteResponseAsync(record, false, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            await _loggerService.AuditAsync(
                LogCategory,
                "BillingFinalization.DbConcurrencyConflict",
                "Penyelesaian invoice gagal karena konflik konkurensi basis data (DbUpdateConcurrencyException).",
                new
                {
                    InvoiceId = invoiceId,
                    ExpectedRowVersion = request.ExpectedRowVersion,
                    CorrelationId = request.CorrelationId
                });

            throw new BillingFinalizationConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.",
                exception,
                code: "DB_CONCURRENCY_CONFLICT");
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinalizationConflictException(
                "Penyelesaian tidak dapat disimpan karena target, correlation, atau idempotency key sudah diproses.",
                exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<(bool Completed, string? IncompleteReason)> TryCompleteSettledInvoiceAsync(
        Guid invoiceId,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
        if (invoice == null || invoice.Status != BillingInvoiceStatuses.Open)
            return (false, null);

        var readiness = await BuildReadinessAsync(invoice, cancellationToken);
        if (!readiness.AllOrdersComplete)
            return (false, "Pembayaran berhasil, tetapi invoice belum dapat ditutup karena masih terdapat order yang belum selesai.");
        if (!readiness.CalculationCurrent)
            return (false, "Pembayaran berhasil, tetapi invoice belum dapat ditutup karena hasil kalkulasi belum terkini.");
        if (readiness.Outstanding > 0)
            return (false, "Pembayaran berhasil, tetapi sisa tagihan pasien belum lunas.");

        var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.InvoiceId == invoice.Id
                && x.VersionNo == readiness.CalculationVersion && !x.IsDelete, cancellationToken);
        if (calculation == null)
            return (false, "Pembayaran berhasil, tetapi versi kalkulasi tidak ditemukan.");

        var idempotencyKey = Guid.NewGuid();
        var payloadHash = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"SETTLED|{invoice.Id:N}|{correlationId:N}|{causationId:N}")));

        var (record, closureChange) = await ExecuteInternalFinalizationAndClosureAsync(
            invoice,
            calculation,
            readiness.CalculationVersion,
            outstandingAtFinalization: 0m,
            isDepartureException: false,
            departureReason: null,
            debtorIdentity: null,
            debtorRelationship: null,
            reason: BillingFinalizationReasons.AutoSettledByPayment,
            idempotencyKey: idempotencyKey,
            payloadHash: payloadHash,
            correlationId: correlationId,
            causationId: causationId,
            actorUserId: actorUserId,
            now: occurredAt,
            cancellationToken: cancellationToken);

        await AuditFinalizationAsync(record, actorUserId, false);
        if (closureChange.Changed)
            await AuditClosureChangeAsync(closureChange, actorUserId);

        return (true, null);
    }

    internal async Task<(BilFinalizationRecord Record, InvoiceClosureChange ClosureChange)> ExecuteInternalFinalizationAndClosureAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        int calculationVersion,
        decimal outstandingAtFinalization,
        bool isDepartureException,
        string? departureReason,
        string? debtorIdentity,
        string? debtorRelationship,
        string reason,
        Guid idempotencyKey,
        string payloadHash,
        Guid correlationId,
        Guid causationId,
        Guid actorUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var record = new BilFinalizationRecord
        {
            InvoiceId = invoice.Id,
            Invoice = invoice,
            CalculationVersion = calculationVersion,
            OutstandingAtFinalization = outstandingAtFinalization,
            IsDepartureException = isDepartureException,
            DepartureReason = isDepartureException && !string.IsNullOrWhiteSpace(departureReason)
                ? departureReason.Trim().ToUpperInvariant()
                : null,
            DebtorIdentity = isDepartureException && !string.IsNullOrWhiteSpace(debtorIdentity)
                ? debtorIdentity.Trim()
                : null,
            DebtorRelationship = isDepartureException && !string.IsNullOrWhiteSpace(debtorRelationship)
                ? debtorRelationship.Trim()
                : null,
            Reason = reason.Trim(),
            IdempotencyKey = idempotencyKey,
            PayloadHash = payloadHash,
            CorrelationId = correlationId,
            CausationId = causationId,
            FinalizedAt = now,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.BilFinalizationRecords.Add(record);

        // Kontrak BIL-STATE-0.4: finalisasi selalu menghasilkan FINAL terlebih dahulu.
        invoice.Status = BillingInvoiceStatuses.Final;
        invoice.InvoiceDate ??= now;
        invoice.RowVersion = Guid.NewGuid();
        invoice.UpdateDateTime = DateTime.UtcNow;
        invoice.UpdateBy = actorUserId;

        await _arApHandoffService.StageHandoffsForFinalizationAsync(
            invoice, calculation, record, outstandingAtFinalization, isDepartureException,
            actorUserId, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        InvoiceClosureChange closureChange;
        try
        {
            closureChange = await _closureService.SyncClosureAsync(
                invoice.Id, actorUserId, now, cancellationToken, PrescriptionClearanceReasonCodes.InvoiceSettled);
        }
        catch (BillingInvoiceClosureValidationException exception)
        {
            throw new BillingFinalizationValidationException(exception.Message);
        }

        if (closureChange.Changed)
            await _dbContext.SaveChangesAsync(cancellationToken);

        if (closureChange.Changed && closureChange.StatusAfter == BillingInvoiceStatuses.Closed)
        {
            await _consumerHandoffService.PublishForClearanceChangeAsync(
                invoice.Id,
                PrescriptionClearanceReasonCodes.InvoiceSettled,
                actorUserId,
                now,
                record.CorrelationId,
                record.CausationId,
                cancellationToken);
            await _consumerHandoffService.PublishForInpatientClearanceAsync(
                invoice.Id,
                InpatientClearanceReasonCodes.InvoiceSettled,
                actorUserId,
                now,
                record.CorrelationId,
                record.CausationId,
                cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return (record, closureChange);
    }

    private async Task<FinalizationPreviewResponse> BuildReadinessAsync(
        BilInvoice invoice,
        CancellationToken cancellationToken)
    {
        if (invoice.CurrentCalculationVersion <= 0)
            return new FinalizationPreviewResponse
            {
                InvoiceId = invoice.Id,
                AllOrdersComplete = false,
                CalculationCurrent = false,
                Outstanding = 0,
                IsReadyForNormalFinalization = false,
                BlockingReasons = ["Invoice belum memiliki hasil perhitungan."],
                CalculationVersion = 0,
                InvoiceRowVersion = invoice.RowVersion
            };

        var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.InvoiceId == invoice.Id
                && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete, cancellationToken)
            ?? throw new BillingFinalizationValidationException(
                "Invoice belum memiliki hasil perhitungan terkini.");

        var allOrdersComplete = await AreAllOrdersCompleteAsync(invoice.Id, cancellationToken);
        var calculationCurrent = await IsCalculationCurrentAsync(invoice.Id, calculation, cancellationToken);
        var outstanding = await _closureService.CalculateOutstandingAsync(invoice, calculation, cancellationToken);
        var hasTariffNotFound = await HasTariffNotFoundAsync(invoice, calculation, cancellationToken);

        var blockingReasons = new List<string>();
        var blockingCodes = new List<string>();
        if (!allOrdersComplete)
            blockingReasons.Add("Semua order harus selesai sebelum invoice difinalkan.");
        if (!calculationCurrent)
            blockingReasons.Add("Tagihan berubah; hitung ulang sebelum finalisasi.");
        if (outstanding > 0)
            blockingReasons.Add(
                "Tanggung jawab pasien belum lunas; ajukan write-off/adjustment atau catat departure exception untuk melanjutkan.");
        if (invoice.RequiresReview)
        {
            blockingReasons.Add("Invoice ini perlu diperiksa sebelum difinalkan.");
            blockingCodes.Add(BillingFinalizationBlockingCodes.RequiresReview);
        }
        if (hasTariffNotFound)
        {
            blockingReasons.Add("Masih ada layanan yang tarifnya belum diatur. Lengkapi master tarif sebelum memfinalkan.");
            blockingCodes.Add(BillingFinalizationBlockingCodes.TariffNotFound);
        }

        return new FinalizationPreviewResponse
        {
            InvoiceId = invoice.Id,
            AllOrdersComplete = allOrdersComplete,
            CalculationCurrent = calculationCurrent,
            Outstanding = outstanding,
            IsReadyForNormalFinalization = allOrdersComplete && calculationCurrent && outstanding == 0
                && blockingCodes.Count == 0,
            BlockingReasons = blockingReasons,
            BlockingCodes = blockingCodes,
            CalculationVersion = calculation.VersionNo,
            InvoiceRowVersion = invoice.RowVersion
        };
    }

    /// <summary>
    /// "Tarif belum ada" pada kunjungan invoice ini — <c>BE-RWI-155</c>, <c>RWI-DEC-192</c> butir (d).
    /// </summary>
    /// <remarks>
    /// Dua sumber: (1) efek folio yang jembatan tandai <c>TARIFF_NOT_FOUND</c> dan belum diselesaikan
    /// (layanan klinis, termasuk komponen biaya operasi), dan (2) segmen tarif kamar pada hitungan
    /// terkini yang tidak menemukan tarif atau kebijakan tarif kamar. Penyelesaian rekonsiliasi
    /// (<c>Resolved</c>) atau pengiriman ulang yang berhasil menghapus penahannya.
    /// </remarks>
    private async Task<bool> HasTariffNotFoundAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        CancellationToken cancellationToken)
    {
        var unresolvedTariff = await (
                from effect in _dbContext.BilProcessingEffects.AsNoTracking()
                join folio in _dbContext.BilFolios.AsNoTracking() on effect.FolioId equals folio.Id
                where folio.EncounterId == invoice.EncounterId
                    && !effect.IsDelete
                    && effect.InvoiceSyncStatus == BillingInvoiceSyncStatus.ReconciliationRequired
                    && effect.InvoiceSyncErrorCode == BillingBridgeCodes.TariffNotFound
                select effect.Id)
            .AnyAsync(cancellationToken);
        if (unresolvedTariff)
            return true;

        var breakdown = BillingCalculationService.MapResponse(calculation, invoice.RowVersion).Breakdown;
        return breakdown?.RoomCharge?.Segments?.Any(x => x.MissingTariff) == true;
    }

    private async Task<bool> AreAllOrdersCompleteAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var activeItems = await _dbContext.BilInvoiceItems.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId
                && x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
            .ToListAsync(cancellationToken);
        return activeItems.All(_chargeSourceAdapter.IsOrderComplete);
    }

    private async Task<bool> IsCalculationCurrentAsync(
        Guid invoiceId,
        BilCalculationVersion calculation,
        CancellationToken cancellationToken)
    {
        var latestItemChange = await _dbContext.BilInvoiceItems.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId)
            .Select(x => x.UpdateDateTime ?? x.CreateDateTime)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(cancellationToken);
        var latestDiscountChange = await _dbContext.BilDiscountApplications.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId)
            .Select(x => x.UpdateDateTime ?? x.CreateDateTime)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(cancellationToken);
        var latestChange = latestItemChange > latestDiscountChange ? latestItemChange : latestDiscountChange;
        return latestChange <= calculation.CalculatedAt.UtcDateTime;
    }

    private static void ValidateFinalizeRequest(FinalizeInvoiceRequest request, Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (actorUserId == Guid.Empty)
            throw new BillingFinalizationValidationException("Identitas pengguna tidak valid.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingFinalizationValidationException("ExpectedRowVersion wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            throw new BillingFinalizationValidationException(
                "Alasan finalisasi wajib diisi dan maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingFinalizationValidationException("CorrelationId dan CausationId wajib diisi.");

        if (string.IsNullOrWhiteSpace(request.DepartureReason)) return;
        var reason = request.DepartureReason.Trim().ToUpperInvariant();
        if (reason is not (BillingDepartureReasons.Death
            or BillingDepartureReasons.EmergencyTransfer
            or BillingDepartureReasons.Dama))
            throw new BillingFinalizationValidationException(
                "DepartureReason harus DEATH, EMERGENCY_TRANSFER, atau DAMA.");
        if (string.IsNullOrWhiteSpace(request.DebtorIdentity) || string.IsNullOrWhiteSpace(request.DebtorRelationship))
            throw new BillingFinalizationValidationException(
                "Pihak yang menanggung sisa tagihan harus dicatat.");
    }

    private static string ComputeFinalizePayloadHash(Guid invoiceId, FinalizeInvoiceRequest request)
    {
        var canonical = string.Join('|',
            invoiceId.ToString("N"),
            request.DepartureReason?.Trim().ToUpperInvariant() ?? string.Empty,
            request.DebtorIdentity?.Trim() ?? string.Empty,
            request.DebtorRelationship?.Trim() ?? string.Empty,
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational() && _dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite"
            ? _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private Task AuditFinalizationAsync(BilFinalizationRecord record, Guid actorUserId, bool isReplay) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingFinalization.Create",
            "Invoice difinalisasi sebagai satu efek final per invoice.",
            new
            {
                FinalizationRecordId = record.Id,
                record.InvoiceId,
                record.CalculationVersion,
                record.OutstandingAtFinalization,
                record.IsDepartureException,
                record.DepartureReason,
                HasDebtorEvidence = !string.IsNullOrWhiteSpace(record.DebtorIdentity),
                record.CorrelationId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });

    // BKC-DES-036: audit perpindahan FINAL<->CLOSED yang terjadi sebagai efek langsung
    // finalisasi. Kategori/bentuk payload sama dengan yang dipakai BillingSettlementService dan
    // BillingFinancialExceptionService untuk peristiwa closure lainnya (BKC-DES-029/030).
    private Task AuditClosureChangeAsync(InvoiceClosureChange change, Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingInvoice.ClosureSynced",
            "Status penutupan invoice diselaraskan berdasarkan sisa tagihan pasien.",
            new
            {
                change.InvoiceId,
                StatusBefore = change.StatusBefore,
                StatusAfter = change.StatusAfter,
                change.Outstanding,
                Trigger = "Finalization",
                ActorUserId = actorUserId
            });

    private static FinalizationResponse Map(BilFinalizationRecord record, bool isReplay) => new()
    {
        Id = record.Id,
        InvoiceId = record.InvoiceId,
        CalculationVersion = record.CalculationVersion,
        OutstandingAtFinalization = record.OutstandingAtFinalization,
        IsDepartureException = record.IsDepartureException,
        DepartureReason = record.DepartureReason,
        InvoiceStatus = record.Invoice.Status,
        FinalizedAt = record.FinalizedAt,
        InvoiceRowVersion = record.Invoice.RowVersion,
        CorrelationId = record.CorrelationId,
        IsReplay = isReplay
    };

    private static void ValidateCompleteRequest(CompleteInvoiceRequest request, Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (actorUserId == Guid.Empty)
            throw new BillingFinalizationValidationException("Identitas pengguna tidak valid.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingFinalizationValidationException("ExpectedRowVersion wajib diisi.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingFinalizationValidationException("CorrelationId dan CausationId wajib diisi.");
    }

    private static string ComputeCompletePayloadHash(Guid invoiceId, CompleteInvoiceRequest request)
    {
        var canonical = string.Join('|',
            invoiceId.ToString("N"),
            "AUTO_NO_PATIENT_PAYMENT",
            request.ExpectedRowVersion.ToString("N"),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private async Task<CompleteInvoiceResponse> MapCompleteResponseAsync(
        BilFinalizationRecord record,
        bool isReplay,
        CancellationToken cancellationToken)
    {
        var handoffs = await _dbContext.BilArHandoffs.AsNoTracking()
            .Where(x => x.FinalizationRecordId == record.Id && x.DebtorType == BillingArDebtorTypes.Payer && !x.IsDelete)
            .Select(x => new BilArHandoffSummaryDto
            {
                Id = x.Id,
                DebtorType = x.DebtorType,
                DebtorReferenceId = x.DebtorReferenceId,
                Amount = x.Amount,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);

        var invoice = record.Invoice
            ?? await _dbContext.BilInvoices.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == record.InvoiceId, cancellationToken);

        return new CompleteInvoiceResponse
        {
            InvoiceId = record.InvoiceId,
            Status = invoice?.Status ?? BillingInvoiceStatuses.Closed,
            PatientAmount = 0m,
            PatientOutstanding = 0m,
            PaymentRequired = false,
            PaymentProcessed = false,
            AutoCompleted = true,
            ClosedAt = invoice?.ClosedAt,
            FinalizationRecordId = record.Id,
            PayerHandoffs = handoffs,
            IsReplay = isReplay
        };
    }
}

public static class BillingFinalizationReasons
{
    public const string AutoNoPatientPayment = "Automatic completion - no patient payment required.";
    public const string AutoSettledByPayment = "Automatic completion - settled by patient payment.";
}

/// <summary>Kode penahan finalisasi — kontrak <c>integrasi-billing</c> <c>1.1.0</c> API 3.9.</summary>
public static class BillingFinalizationBlockingCodes
{
    /// <summary>Invoice ditandai "perlu diperiksa" (<c>VAL-RWF-16</c>).</summary>
    public const string RequiresReview = "BIL-FIN-020";

    /// <summary>Masih ada layanan "tarif belum ada" (<c>VAL-RWF-17</c>).</summary>
    public const string TariffNotFound = "BIL-FIN-021";
}

public abstract class BillingFinalizationException : Exception
{
    protected BillingFinalizationException(string message) : base(message) { }
    protected BillingFinalizationException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BillingFinalizationValidationException(string message)
    : BillingFinalizationException(message);

public sealed class BillingFinalizationConflictException : BillingFinalizationException
{
    public string? Code { get; }
    public Guid? CurrentRowVersion { get; }

    public BillingFinalizationConflictException(
        string message,
        string? code = null,
        Guid? currentRowVersion = null)
        : base(message)
    {
        Code = code;
        CurrentRowVersion = currentRowVersion;
    }

    public BillingFinalizationConflictException(
        string message,
        Exception innerException,
        string? code = null,
        Guid? currentRowVersion = null)
        : base(message, innerException)
    {
        Code = code;
        CurrentRowVersion = currentRowVersion;
    }
}

public sealed class BillingFinalizationBlockedException : BillingFinalizationException
{
    public BillingFinalizationBlockedException(string message, FinalizationPreviewResponse checklist)
        : base(message)
    {
        Checklist = checklist;
    }

    public FinalizationPreviewResponse Checklist { get; }
}
