using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
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
    private readonly BillingEmployeeBenefitSplitService? _splitService;
    private readonly EmployeeBenefitBalanceService? _balanceService;
    private readonly ClinicalDocumentIntegrityService? _integrityService;

    public BillingFinalizationService(
        ApplicationDbContext dbContext,
        IBillingChargeSourceAdapter chargeSourceAdapter,
        BillingArApHandoffService arApHandoffService,
        BillingInvoiceClosureService closureService,
        BilConsumerHandoffService consumerHandoffService,
        LoggerService loggerService,
        BillingEmployeeBenefitSplitService? splitService = null,
        IBillingCalculationService? calculationService = null,
        EmployeeBenefitBalanceService? balanceService = null,
        ClinicalDocumentIntegrityService? integrityService = null)
    {
        _dbContext = dbContext;
        _chargeSourceAdapter = chargeSourceAdapter;
        _arApHandoffService = arApHandoffService;
        _closureService = closureService;
        _consumerHandoffService = consumerHandoffService;
        _loggerService = loggerService;
        _splitService = splitService;
        _calculationService = calculationService;
        _balanceService = balanceService ?? splitService?.BalanceService;
        _integrityService = integrityService;
    }

    public async Task<FinalizationPreviewResponse> PreviewAsync(
        Guid invoiceId,
        CancellationToken cancellationToken,
        decimal? choiceAmount = null,
        string? consentGivenBy = null,
        string? financingChoice = null,
        decimal? employeeDeferredAmount = null)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");
        return await BuildReadinessAsync(
            invoice,
            cancellationToken,
            choiceAmount: choiceAmount,
            consentGivenBy: consentGivenBy,
            financingChoice: financingChoice,
            employeeDeferredAmount: employeeDeferredAmount);
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
                var replayResponse = Map(prior, true);
                if (_splitService is not null)
                {
                    var replayCalculation = await _dbContext.BilCalculationVersions.AsNoTracking()
                        .SingleOrDefaultAsync(x => x.InvoiceId == prior.InvoiceId && x.VersionNo == prior.CalculationVersion, cancellationToken);
                    if (replayCalculation is not null)
                    {
                        var replaySplit = await _splitService.EvaluateSplitAsync(
                            prior.Invoice,
                            replayCalculation,
                            prior.OutstandingAtFinalization,
                            request.EmployeeDeferredAmount ?? request.EmployeeReceivableChoiceAmount ?? 0m,
                            request.EmployeeReceivableConsentGivenBy,
                            isFinalizing: false,
                            cancellationToken,
                            financingChoice: request.FinancingChoice,
                            employeeDeferredAmount: request.EmployeeDeferredAmount,
                            consentNotes: request.EmployeeReceivableConsentNotes);
                        replayResponse.EmployeeBenefitSplit = replaySplit?.Response;
                    }
                }
                return replayResponse;
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

            var readiness = await BuildReadinessAsync(
                invoice,
                cancellationToken,
                choiceAmount: request.EmployeeDeferredAmount ?? request.EmployeeReceivableChoiceAmount ?? 0m,
                consentGivenBy: request.EmployeeReceivableConsentGivenBy,
                financingChoice: request.FinancingChoice,
                employeeDeferredAmount: request.EmployeeDeferredAmount);
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
            // BE-BKC-FIX-021: finalisasi normal tertahan oleh pelayanan yang belum masuk invoice.
            // Departure exception (meninggal, transfer darurat, APS) tidak ditahan — BKC-DEC-033;
            // tagihan yang menyusul diajukan sebagai adjustment seperti late charge lain.
            if (!isDepartureException
                && readiness.BlockingCodes.Contains(BillingFinalizationBlockingCodes.PendingClinicalCharge))
                throw new BillingFinalizationBlockedException(
                    "Masih ada pelayanan yang belum masuk invoice. Selesaikan antrean rekonsiliasi sebelum memfinalkan.",
                    readiness);
            if (!readiness.AllOrdersComplete || !readiness.CalculationCurrent)
                throw new BillingFinalizationBlockedException(
                    "Invoice belum siap difinalisasi.", readiness);
            var now = DateTimeOffset.UtcNow;
            var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
                .SingleAsync(
                    x => x.InvoiceId == invoice.Id && x.VersionNo == readiness.CalculationVersion,
                    cancellationToken);

            // BE-FIN-107: Evaluasi pembagian dua porsi penjamin internal (RS Benefit & pegawai)
            EmployeeBenefitSplitCalculationResult? splitResult = null;
            if (_splitService is not null)
            {
                splitResult = await _splitService.EvaluateSplitAsync(
                    invoice,
                    calculation,
                    readiness.Outstanding,
                    choiceAmount: request.EmployeeDeferredAmount ?? request.EmployeeReceivableChoiceAmount ?? 0m,
                    consentGivenBy: request.EmployeeReceivableConsentGivenBy,
                    isFinalizing: true,
                    cancellationToken: cancellationToken,
                    financingChoice: request.FinancingChoice,
                    employeeDeferredAmount: request.EmployeeDeferredAmount,
                    consentNotes: request.EmployeeReceivableConsentNotes);
            }

            var effectiveOutstanding = splitResult is not null
                ? splitResult.PatientRemainingAmount
                : readiness.Outstanding;

            if (!isDepartureException && effectiveOutstanding > 0)
                throw new BillingFinalizationBlockedException(
                    "Tanggung jawab pasien belum lunas; ajukan write-off/adjustment atau catat departure exception untuk melanjutkan.",
                    readiness);

            var (record, closureChange) = await ExecuteInternalFinalizationAndClosureAsync(
                invoice,
                calculation,
                readiness.CalculationVersion,
                effectiveOutstanding,
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
                cancellationToken,
                splitResult);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditFinalizationAsync(record, actorUserId, false);
            if (closureChange.Changed)
                await AuditClosureChangeAsync(closureChange, actorUserId);
            var response = Map(record, false);
            response.EmployeeBenefitSplit = splitResult?.Response;
            return response;
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
        catch (Exception exception) when (IsSerializationFailure(exception))
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinalizationConflictException(
                "Terjadi konflik transaksi bersamaan pada data yang sama (serialization failure). Muat ulang dan coba kembali.",
                exception,
                code: "SERIALIZATION_FAILURE");
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

    /// <summary>
    /// Finalisasi invoice dengan penangguhan sisa kewajiban pasien ke Piutang Pasien (Finance AR).
    /// Menggunakan otorisasi khusus BillingFinalization.DeferToAR. Menghasilkan BilArHandoff dengan
    /// DebtorType = PATIENT_GUARANTOR dan mempertahankan status invoice FINAL (bukan CLOSED) selama
    /// piutang belum lunas diselesaikan di Finance.
    /// </summary>
    public async Task<FinalizationResponse> FinalizeWithPatientReceivableAsync(
        Guid invoiceId,
        FinalizeWithPatientReceivableRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateFinalizeWithPatientReceivableRequest(request, actorUserId);
        var payloadHash = ComputePatientArPayloadHash(invoiceId, request);
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

            // Pemeriksaan kelayakan invoice sebelum finalisasi:
            if (readiness.BlockingCodes.Contains(BillingFinalizationBlockingCodes.RequiresReview))
                throw new BillingFinalizationBlockedException(
                    "Invoice ini perlu diperiksa sebelum difinalkan.", readiness);
            if (readiness.BlockingCodes.Contains(BillingFinalizationBlockingCodes.TariffNotFound))
                throw new BillingFinalizationBlockedException(
                    "Masih ada layanan yang tarifnya belum diatur. Lengkapi master tarif sebelum memfinalkan.",
                    readiness);
            if (readiness.BlockingCodes.Contains(BillingFinalizationBlockingCodes.PendingClinicalCharge))
                throw new BillingFinalizationBlockedException(
                    "Masih ada pelayanan yang belum masuk invoice. Selesaikan antrean rekonsiliasi sebelum memfinalkan.",
                    readiness);
            if (!readiness.AllOrdersComplete || !readiness.CalculationCurrent)
                throw new BillingFinalizationBlockedException(
                    "Invoice belum siap difinalisasi.", readiness);

            var now = DateTimeOffset.UtcNow;
            var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
                .SingleAsync(
                    x => x.InvoiceId == invoice.Id && x.VersionNo == readiness.CalculationVersion,
                    cancellationToken);

            EmployeeBenefitSplitCalculationResult? splitResult = null;
            if (_splitService is not null)
            {
                splitResult = await _splitService.EvaluateSplitAsync(
                    invoice,
                    calculation,
                    readiness.Outstanding,
                    choiceAmount: 0m,
                    consentGivenBy: null,
                    isFinalizing: true,
                    cancellationToken: cancellationToken);
            }

            var effectiveOutstanding = splitResult is not null
                ? splitResult.PatientRemainingAmount
                : readiness.Outstanding;

            if (effectiveOutstanding <= 0)
                throw new BillingFinalizationValidationException(
                    "Invoice tidak memiliki sisa kewajiban pasien untuk ditangguhkan ke piutang. Seluruh tagihan sudah selesai; gunakan finalisasi normal.");

            // Identitas debitur penanggung authoritative dari master pasien / encounter
            var encounter = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Include(e => e.Patient)
                .SingleOrDefaultAsync(e => e.Id == invoice.EncounterId && !e.IsDelete, cancellationToken);
            var patient = encounter?.Patient;
            var patientId = encounter?.PatientId ?? Guid.Empty;

            string debtorIdentity;
            string debtorRelationship;
            var isSelf = string.IsNullOrWhiteSpace(request.GuarantorRelationship)
                || string.Equals(request.GuarantorRelationship.Trim(), "SELF", StringComparison.OrdinalIgnoreCase);

            if (isSelf)
            {
                debtorIdentity = patient is not null
                    ? $"{patient.FullName} (No. RM: {patient.MedicalRecordNumber})"
                    : "Pasien Sendiri";
                debtorRelationship = "SELF";
            }
            else
            {
                var guarantorName = !string.IsNullOrWhiteSpace(request.GuarantorName)
                    ? request.GuarantorName.Trim()
                    : (patient?.FullName ?? "Penanggung");
                debtorIdentity = patient is not null
                    ? $"{guarantorName} (Penanggung untuk {patient.FullName}, No. RM: {patient.MedicalRecordNumber})"
                    : guarantorName;
                debtorRelationship = !string.IsNullOrWhiteSpace(request.GuarantorRelationship)
                    ? request.GuarantorRelationship.Trim().ToUpperInvariant()
                    : "PENANGGUNG";
            }

            var deferralReason = $"Penangguhan Piutang Pasien: {request.DeferralReason.Trim()}";

            var (record, closureChange) = await ExecuteInternalFinalizationAndClosureAsync(
                invoice,
                calculation,
                readiness.CalculationVersion,
                effectiveOutstanding,
                isDepartureException: false,
                departureReason: null,
                debtorIdentity: debtorIdentity,
                debtorRelationship: debtorRelationship,
                reason: deferralReason,
                idempotencyKey: idempotencyKey,
                payloadHash: payloadHash,
                correlationId: request.CorrelationId,
                causationId: request.CausationId,
                actorUserId: actorUserId,
                now: now,
                cancellationToken: cancellationToken,
                employeeBenefitSplit: splitResult,
                isPatientArDeferred: true,
                patientArDueDate: request.DueDate,
                patientArConsentGivenBy: request.GuarantorName ?? patient?.FullName,
                patientArConsentNotes: request.AgreementDocumentNumber);

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditFinalizationAsync(record, actorUserId, false);
            if (closureChange.Changed)
                await AuditClosureChangeAsync(closureChange, actorUserId);

            await _loggerService.AuditAsync(
                LogCategory,
                "BillingFinalization.PatientArDeferred",
                $"Invoice {invoice.InvoiceNumber} berhasil difinalisasi dengan penangguhan piutang pasien sebesar {effectiveOutstanding:N2}.",
                new
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    PatientId = patientId,
                    DebtorIdentity = debtorIdentity,
                    DebtorRelationship = debtorRelationship,
                    Amount = effectiveOutstanding,
                    DueDate = request.DueDate,
                    AgreementDocumentNumber = request.AgreementDocumentNumber,
                    CorrelationId = request.CorrelationId,
                    FinalizationRecordId = record.Id
                });

            var response = Map(record, false);
            response.EmployeeBenefitSplit = splitResult?.Response;
            return response;
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
                "Penangguhan piutang finalisasi tidak dapat disimpan karena target, correlation, atau idempotency key sudah diproses.",
                exception);
        }
        catch (Exception exception) when (IsSerializationFailure(exception))
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinalizationConflictException(
                "Terjadi konflik transaksi bersamaan pada data yang sama (serialization failure). Muat ulang dan coba kembali.",
                exception,
                code: "SERIALIZATION_FAILURE");
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
            // BE-BKC-FIX-021: penahan yang sama dengan finalisasi manual normal.
            if (readiness.BlockingCodes.Count > 0)
                throw new BillingFinalizationBlockedException(
                    readiness.BlockingReasons.LastOrDefault() ?? "Invoice belum siap diselesaikan.",
                    readiness);
            if (readiness.Outstanding > 0 || calculation.PatientAmount > 0)
                throw new BillingFinalizationBlockedException(
                    "Invoice memiliki kewajiban pasien; gunakan alur pembayaran normal.",
                    readiness);

            EmployeeBenefitSplitCalculationResult? splitResult = null;
            if (_splitService is not null)
            {
                splitResult = await _splitService.EvaluateSplitAsync(
                    invoice,
                    calculation,
                    patientOutstanding: 0m,
                    choiceAmount: 0m,
                    consentGivenBy: null,
                    isFinalizing: true,
                    cancellationToken);
            }

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
                cancellationToken: cancellationToken,
                employeeBenefitSplit: splitResult);

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
        catch (Exception exception) when (IsSerializationFailure(exception))
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinalizationConflictException(
                "Terjadi konflik transaksi bersamaan pada data yang sama (serialization failure). Muat ulang dan coba kembali.",
                exception,
                code: "SERIALIZATION_FAILURE");
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
        // BE-BKC-FIX-021: penutupan otomatis memakai penahan yang sama dengan finalisasi manual.
        if (readiness.BlockingCodes.Contains(BillingFinalizationBlockingCodes.PendingClinicalCharge))
            return (false, "Pembayaran berhasil, tetapi invoice belum dapat ditutup karena masih ada pelayanan yang belum masuk invoice.");
        if (readiness.BlockingCodes.Count > 0)
            return (false, "Pembayaran berhasil, tetapi invoice belum dapat ditutup: " +
                (readiness.BlockingReasons.LastOrDefault() ?? "invoice perlu diperiksa."));
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

        EmployeeBenefitSplitCalculationResult? splitResult = null;
        if (_splitService is not null)
        {
            splitResult = await _splitService.EvaluateSplitAsync(
                invoice,
                calculation,
                patientOutstanding: 0m,
                choiceAmount: 0m,
                consentGivenBy: null,
                isFinalizing: true,
                cancellationToken);
        }

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
            cancellationToken: cancellationToken,
            employeeBenefitSplit: splitResult);

        await AuditFinalizationAsync(record, actorUserId, false);
        if (closureChange.Changed)
            await AuditClosureChangeAsync(closureChange, actorUserId);

        return (true, null);
    }

    /// <summary>
    /// TASK BE-04: Transisi EncounterStatus menjadi Completed (9) setelah billing lunas,
    /// dengan seluruh guard kelayakan klinis dan finansial yang ketat.
    /// </summary>
    public async Task<(bool Completed, string? IncompleteReason)> TryCompleteEncounterForInvoiceAsync(
        Guid invoiceId,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
        if (invoice == null)
            return (false, "Invoice tidak ditemukan.");

        // Guard finansial: tagihan harus sudah lunas (outstanding <= 0)
        var outstanding = await _closureService.CalculateOutstandingAsync(invoice, cancellationToken);
        if (outstanding > 0)
            return (false, "Encounter belum dapat diselesaikan karena invoice masih memiliki sisa tagihan.");

        // Guard transaksi: tidak boleh ada tender yang masih pending
        var hasPendingTenders = await _dbContext.BilTenders
            .AnyAsync(t => t.Settlement.InvoiceId == invoice.Id && t.Status == BillingTenderStatuses.Pending && !t.IsDelete, cancellationToken);
        if (hasPendingTenders)
            return (false, "Encounter belum dapat diselesaikan karena masih ada pembayaran yang berstatus pending.");

        var encounter = await _dbContext.Set<RegPatientEncounter>()
            .SingleOrDefaultAsync(e => e.Id == invoice.EncounterId && !e.IsDelete, cancellationToken);
        if (encounter == null)
            return (false, "Encounter pasien tidak ditemukan.");

        // Idempotent guard: sudah completed
        if (encounter.EncounterStatus == EncounterStatus.Completed)
            return (true, null);

        // Guard keselamatan: encounter yang dibatalkan atau no-show tidak boleh diubah menjadi selesai
        if (encounter.CancelledAt.HasValue || encounter.NoShowAt.HasValue ||
            encounter.EncounterStatus == EncounterStatus.Cancelled || encounter.EncounterStatus == EncounterStatus.NoShow)
        {
            return (false, "Encounter telah dibatalkan atau no-show, tidak dapat diselesaikan.");
        }

        // Clinical / discharge guards per tipe encounter
        if (encounter.EncounterType == EncounterType.Inpatient)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .FirstOrDefaultAsync(ep => ep.EncounterId == encounter.Id && !ep.IsDelete, cancellationToken);
            if (episode != null && episode.EpisodeStatus != InpEpisodeStatus.Closed && !episode.PhysicallyLeftAt.HasValue)
            {
                return (false, "Encounter rawat inap belum dapat diselesaikan karena pasien belum menyelesaikan prosedur kepulangan klinis.");
            }
        }
        else if (encounter.EncounterType == EncounterType.Emergency)
        {
            var emgVisit = await _dbContext.Set<EmgVisit>()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.EncounterId == encounter.Id && !v.IsDelete, cancellationToken);
            if (emgVisit != null && !emgVisit.ClosedByDispositionId.HasValue)
            {
                return (false, "Encounter gawat darurat belum dapat diselesaikan karena disposisi IGD belum ditutup.");
            }
        }

        var statusBefore = encounter.EncounterStatus;
        encounter.EncounterStatus = EncounterStatus.Completed;
        encounter.CompletedAt = occurredAt.UtcDateTime;
        encounter.UpdateDateTime = DateTime.UtcNow;
        encounter.UpdateBy = actorUserId;

        if (_integrityService != null)
        {
            await _integrityService.LockOpenDocumentsForEncounterAsync(
                encounter.Id, actorUserId, occurredAt.UtcDateTime, encounter.CompletedAt, cancellationToken: cancellationToken);
        }

        await _loggerService.AuditAsync(
            LogCategory,
            "BillingEncounter.Completed",
            $"Encounter {encounter.EncounterNumber} otomatis diselesaikan setelah tagihan lunas (Invoice {invoice.InvoiceNumber}).",
            new
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                EncounterId = encounter.Id,
                EncounterNumber = encounter.EncounterNumber,
                EncounterStatusBefore = (int)statusBefore,
                EncounterStatusAfter = (int)EncounterStatus.Completed,
                ActorUserId = actorUserId,
                OccurredAt = occurredAt
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
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
        CancellationToken cancellationToken,
        EmployeeBenefitSplitCalculationResult? employeeBenefitSplit = null,
        bool isPatientArDeferred = false,
        DateTimeOffset? patientArDueDate = null,
        string? patientArConsentGivenBy = null,
        string? patientArConsentNotes = null)
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
            DebtorIdentity = !string.IsNullOrWhiteSpace(debtorIdentity)
                ? debtorIdentity.Trim()
                : null,
            DebtorRelationship = !string.IsNullOrWhiteSpace(debtorRelationship)
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
            actorUserId, cancellationToken, employeeBenefitSplit,
            isPatientArDeferred: isPatientArDeferred,
            patientArDueDate: patientArDueDate,
            patientArConsentGivenBy: patientArConsentGivenBy,
            patientArConsentNotes: patientArConsentNotes);

        // BKC-DEC-164 / BKC-DES-086 (BE-BKC-085): Sisa kredit administrasi rawat jalan menjadi kredit refund
        await RecognizeUnappliedReferredAdminCreditAsync(
            invoice, calculation, calculationVersion, actorUserId, now, cancellationToken);

        // BE-FIN-108: Kurangi plafon di HR tepat satu kali di dalam batas transaksi pemanggil
        // (FIN-DEC-202, FIN-DEC-205, FIN-INTEGRATION-1.9 R.3, FR-FIN-244, AC 1-5).
        // Plafon hanya dipotong untuk porsi RS Benefit (PayerAmount = P = min(C, S)).
        // Bila P = 0, pemakaian plafon dilewati per AC 4 / R.3.4 langkah 5.
        if (employeeBenefitSplit is { IsEmployeeBenefitVisit: true, PayerAmount: > 0m }
            && employeeBenefitSplit.PayerHandoffKey.HasValue
            && _balanceService is not null)
        {
            if (string.IsNullOrWhiteSpace(employeeBenefitSplit.BenefitPersonKind)
                || !employeeBenefitSplit.BenefitPersonId.HasValue)
            {
                throw new BillingFinalizationValidationException(
                    "Identitas kepesertaan benefit pegawai tidak lengkap untuk pemotongan plafon.");
            }

            var useResult = await _balanceService.UseAsync(new BenefitBalanceUseRequest
            {
                SourceHandoffKey = employeeBenefitSplit.PayerHandoffKey.Value,
                InvoiceId = invoice.Id,
                BenefitPersonKind = employeeBenefitSplit.BenefitPersonKind,
                BenefitPersonId = employeeBenefitSplit.BenefitPersonId.Value,
                Amount = employeeBenefitSplit.PayerAmount,
                ActorUserId = actorUserId
            }, cancellationToken);

            if (string.Equals(useResult.Result, BenefitUseResults.Insufficient, StringComparison.OrdinalIgnoreCase))
            {
                throw new BillingFinalizationValidationException(
                    useResult.Message ?? $"Sisa plafon tidak cukup. Sisa saat ini Rp {useResult.RemainingAmount:N0}.");
            }

            if (string.Equals(useResult.Result, BenefitUseResults.KeyConflict, StringComparison.OrdinalIgnoreCase))
            {
                throw new BillingFinalizationConflictException(
                    useResult.Message ?? "Konflik kunci serah terima saat pemakaian plafon benefit.");
            }

            if (!string.Equals(useResult.Result, BenefitUseResults.Applied, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(useResult.Result, BenefitUseResults.AlreadyApplied, StringComparison.OrdinalIgnoreCase))
            {
                throw new BillingFinalizationValidationException(
                    useResult.Message ?? $"Gagal memakai plafon benefit pegawai: {useResult.Result}.");
            }
        }

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

            // TASK BE-04: Transisi EncounterStatus menjadi Completed (9) jika memenuhi prasyarat
            await TryCompleteEncounterForInvoiceAsync(invoice.Id, actorUserId, now, cancellationToken);
        }

        return (record, closureChange);
    }

    private async Task<FinalizationPreviewResponse> BuildReadinessAsync(
        BilInvoice invoice,
        CancellationToken cancellationToken,
        decimal? choiceAmount = null,
        string? consentGivenBy = null,
        string? financingChoice = null,
        decimal? employeeDeferredAmount = null)
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
        // BE-BKC-FIX-021 / ISSUE-BKC-001 T1: pelayanan eligible yang belum menjadi item tidak dapat
        // diperiksa lewat item invoice. "Tarif belum ada" sudah punya kode sendiri, jadi tidak
        // dihitung dua kali di sini.
        var pendingChargeCount = (await BillingPendingChargeQuery.CountByEncounterAsync(
                _dbContext, [invoice.EncounterId], cancellationToken, excludeTariffNotFound: true))
            .GetValueOrDefault(invoice.EncounterId);

        EmployeeBenefitSplitCalculationResult? splitResult = null;
        if (_splitService is not null)
        {
            splitResult = await _splitService.EvaluateSplitAsync(
                invoice,
                calculation,
                outstanding,
                choiceAmount: employeeDeferredAmount ?? choiceAmount ?? 0m,
                consentGivenBy: consentGivenBy,
                isFinalizing: false,
                cancellationToken: cancellationToken,
                financingChoice: financingChoice,
                employeeDeferredAmount: employeeDeferredAmount,
                consentNotes: null);
        }

        var effectiveOutstanding = splitResult is not null
            ? splitResult.PatientRemainingAmount
            : outstanding;

        var blockingReasons = new List<string>();
        var blockingCodes = new List<string>();
        if (!allOrdersComplete)
            blockingReasons.Add("Semua order harus selesai sebelum invoice difinalkan.");
        if (!calculationCurrent)
            blockingReasons.Add("Tagihan berubah; hitung ulang sebelum finalisasi.");
        if (effectiveOutstanding > 0)
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
        if (pendingChargeCount > 0)
        {
            blockingReasons.Add(
                $"Masih ada {pendingChargeCount} pelayanan yang sudah layak ditagih tetapi belum masuk invoice " +
                "(menunggu diteruskan, gagal, atau perlu ditinjau). Kirim ulang atau selesaikan lewat antrean " +
                "rekonsiliasi sebelum memfinalkan.");
            blockingCodes.Add(BillingFinalizationBlockingCodes.PendingClinicalCharge);
        }

        if (splitResult is not null)
        {
            if (!splitResult.Response.IsUsable && !string.IsNullOrWhiteSpace(splitResult.Response.BlockingMessage))
            {
                blockingReasons.Add($"Benefit karyawan tidak dapat digunakan: {splitResult.Response.BlockingMessage}");
                blockingCodes.Add(splitResult.Response.BlockingReasonCode ?? "BIL-FIN-BENEFIT-UNUSABLE");
            }
        }

        return new FinalizationPreviewResponse
        {
            InvoiceId = invoice.Id,
            AllOrdersComplete = allOrdersComplete,
            CalculationCurrent = calculationCurrent,
            Outstanding = outstanding,
            IsReadyForNormalFinalization = allOrdersComplete && calculationCurrent && effectiveOutstanding == 0
                && blockingCodes.Count == 0,
            BlockingReasons = blockingReasons,
            BlockingCodes = blockingCodes,
            CalculationVersion = calculation.VersionNo,
            InvoiceRowVersion = invoice.RowVersion,
            EmployeeBenefitSplit = splitResult?.Response
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
            request.EmployeeReceivableChoiceAmount?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty,
            request.EmployeeReceivableConsentGivenBy?.Trim() ?? string.Empty,
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static void ValidateFinalizeWithPatientReceivableRequest(FinalizeWithPatientReceivableRequest request, Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (actorUserId == Guid.Empty)
            throw new BillingFinalizationValidationException("Identitas pengguna tidak valid.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingFinalizationValidationException("ExpectedRowVersion wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.DeferralReason) || request.DeferralReason.Trim().Length > 500)
            throw new BillingFinalizationValidationException(
                "Alasan penangguhan piutang pasien wajib diisi dan maksimal 500 karakter.");
        if (!request.UserConfirmation)
            throw new BillingFinalizationValidationException(
                "Konfirmasi otorisasi penangguhan piutang pasien wajib disetujui.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingFinalizationValidationException("CorrelationId dan CausationId wajib diisi.");
    }

    private static string ComputePatientArPayloadHash(Guid invoiceId, FinalizeWithPatientReceivableRequest request)
    {
        var canonical = string.Join('|',
            invoiceId.ToString("N"),
            request.DeferralReason.Trim(),
            request.AgreementDocumentNumber?.Trim() ?? string.Empty,
            request.DueDate?.ToString("O") ?? string.Empty,
            request.GuarantorName?.Trim() ?? string.Empty,
            request.GuarantorRelationship?.Trim().ToUpperInvariant() ?? string.Empty,
            request.UserConfirmation ? "TRUE" : "FALSE",
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

    /// <summary>
    /// BKC-DEC-164 / BKC-DES-086 (BE-BKC-085):
    /// Bila ReferredOutpatientAdminUnappliedCreditAmount versi yang difinalisasi lebih dari 0 dan
    /// belum ada kredit REFERRED_OUTPATIENT_ADMIN dengan SourceId = id invoice rawat inap,
    /// satu kredit refund dibuat dan dicatat audit BillingRefundableCredit.RecognizeReferredAdmin.
    /// Bila kredit sudah ada tetapi nominal sisanya berbeda (invoice sempat kembali OPEN lalu difinalisasi ulang),
    /// kredit lama tidak diubah dan peristiwanya dicatat audit BillingRefundableCredit.ReferredAdminDrift untuk ditinjau Finance.
    /// Berjalan di dalam batas transaksi pemanggil.
    /// </summary>
    private async Task RecognizeUnappliedReferredAdminCreditAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        int calculationVersion,
        Guid actorUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (calculation.ReferredOutpatientAdminUnappliedCreditAmount <= 0m)
            return;

        var existingCredit = await _dbContext.BilRefundableCredits
            .Where(c => c.SourceType == BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin
                     && c.SourceId == invoice.Id
                     && !c.IsDelete)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingCredit is null)
        {
            var credit = new BilRefundableCredit
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                SourceType = BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin,
                SourceId = invoice.Id,
                OriginalAmount = calculation.ReferredOutpatientAdminUnappliedCreditAmount,
                AvailableAmount = calculation.ReferredOutpatientAdminUnappliedCreditAmount,
                Status = BillingRefundableCreditStatuses.Available,
                RecognizedAt = now,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.BilRefundableCredits.Add(credit);

            await _loggerService.AuditAsync(
                LogCategory,
                "BillingRefundableCredit.RecognizeReferredAdmin",
                "Kredit refund admin Rawat Jalan lahir saat finalisasi.",
                new
                {
                    CreditId = credit.Id,
                    InpatientInvoiceId = invoice.Id,
                    Amount = credit.OriginalAmount,
                    CalculationVersionNo = calculationVersion,
                    ActorUserId = actorUserId
                });
        }
        else if (existingCredit.OriginalAmount != calculation.ReferredOutpatientAdminUnappliedCreditAmount)
        {
            await _loggerService.AuditAsync(
                LogCategory,
                "BillingRefundableCredit.ReferredAdminDrift",
                "Finalisasi ulang menghasilkan sisa kredit berbeda dari kredit yang sudah ada.",
                new
                {
                    CreditId = existingCredit.Id,
                    InpatientInvoiceId = invoice.Id,
                    ExistingAmount = existingCredit.OriginalAmount,
                    NewUnappliedAmount = calculation.ReferredOutpatientAdminUnappliedCreditAmount,
                    ActorUserId = actorUserId
                });
        }
    }

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

    private static bool IsSerializationFailure(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (string.Equals(current.GetType().Name, "PostgresException", StringComparison.OrdinalIgnoreCase))
            {
                var sqlStateProp = current.GetType().GetProperty("SqlState");
                var sqlState = sqlStateProp?.GetValue(current) as string;
                if (sqlState == "40001") return true;
            }

            if (current.Message.Contains("40001", StringComparison.OrdinalIgnoreCase)
                || current.Message.Contains("could not serialize access", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
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

    /// <summary>
    /// Masih ada pelayanan eligible yang belum menjadi item invoice — menunggu diteruskan, gagal,
    /// atau perlu ditinjau (<c>BE-BKC-FIX-021</c>, <c>ISSUE-BKC-001</c> T1).
    /// </summary>
    public const string PendingClinicalCharge = "BIL-FIN-022";
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
