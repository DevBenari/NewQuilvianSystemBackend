using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Layanan Single Source of Truth evaluasi kelayakan finansial rawat inap, validasi deposit tindakan besar,
/// dan penegakan Auto-Reblock (BKC-DEC-114, BKC-DEC-115, BKC-DEC-116, BKC-DEC-120, BKC-DES-045, BKC-DES-046, BKC-DES-047).
/// </summary>
public sealed class InpatientClearanceService : IInpatientClearanceService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing.InpatientClearance";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public InpatientClearanceService(
        ApplicationDbContext dbContext,
        LoggerService loggerService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    /// <inheritdoc />
    public async Task<BilInpatientClearanceHandoff> EvaluateClearanceAsync(
        Guid encounterId,
        string reasonCode,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken,
        string? revocationReason = null)
    {
        // BKC-DES-045, BKC-DEC-115: Mengambil advisory lock untuk mencegah race condition
        // antara kasir klik lunas dan perawat input tindakan/obat di bangsal
        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"BIL_INPATIENT_CLEARANCE_{encounterId:N}"],
                cancellationToken);
        }

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Encounter rawat inap dengan ID {encounterId} tidak ditemukan.");

        var invoice = await _dbContext.BilInvoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

        string clearanceStatus;
        string? financialOutcome = null;
        decimal outstanding = 0m;
        decimal totalPatientResponsibility = 0m;
        decimal totalPaidOrAllocated = 0m;

        if (invoice == null)
        {
            clearanceStatus = InpatientClearanceStatuses.Pending;
        }
        else
        {
            // Ambil kalkulasi terakhir yang authoritative
            var latestCalc = await _dbContext.BilCalculationVersions.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && !x.IsDelete)
                .OrderByDescending(x => x.VersionNo)
                .FirstOrDefaultAsync(cancellationToken);

            var activeItems = invoice.Items
                .Where(x => x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
                .ToList();

            totalPatientResponsibility = latestCalc?.PatientAmount
                ?? activeItems.Sum(x => x.Quantity * x.UnitPrice);

            // Alokasi pembayaran
            var allocations = await _dbContext.BilPaymentAllocations.AsNoTracking()
                .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var paidAmount = allocations.Sum(x => x.Amount);
            var allocationExcess = 0m;

            // Write-off
            var writeOffs = await _dbContext.BilWriteOffCases.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && x.Status == BillingWriteOffCaseStatuses.Posted && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var writeOffTotal = writeOffs.Sum(x => x.Amount);

            // Adjustment
            var adjustments = await _dbContext.BilAdjustments.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var adjustmentNet = adjustments.Sum(x => x.Direction == "CREDIT" ? x.Amount : -x.Amount);

            // Kredit alihan rajal / refund credit
            var refundableCredits = await _dbContext.BilRefundableCredits.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var creditTotal = refundableCredits.Sum(x => x.OriginalAmount);

            totalPaidOrAllocated = paidAmount + creditTotal;
            outstanding = Math.Max(0m, totalPatientResponsibility - paidAmount - creditTotal + allocationExcess - writeOffTotal - adjustmentNet);

            // Evaluasi Status Kelayakan & Financial Outcome
            var isRevocation = reasonCode is InpatientClearanceReasonCodes.LateChargePosted
                or InpatientClearanceReasonCodes.PaymentReversed;

            if (isRevocation)
            {
                clearanceStatus = InpatientClearanceStatuses.Revoked;
                financialOutcome = null;
            }
            else if (outstanding <= 0m && (totalPatientResponsibility > 0m || totalPaidOrAllocated > 0m || invoice.Status == BillingInvoiceStatuses.Closed))
            {
                clearanceStatus = InpatientClearanceStatuses.Cleared;

                // Tentukan FinancialOutcome berdasarkan metode pembayaran / penjaminan
                var hasDepositAllocation = await _dbContext.BilPaymentAllocations.AsNoTracking()
                    .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                    .Join(_dbContext.BilSettlements.AsNoTracking(), a => a.SettlementId, s => s.Id, (a, s) => s)
                    .AnyAsync(s => s.DepositAccountId.HasValue, cancellationToken);

                var guarantor = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                    .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                    .OrderByDescending(x => x.IsPrimary)
                    .FirstOrDefaultAsync(cancellationToken);
                var hasGuarantor = guarantor != null && guarantor.PaymentType != EncounterPaymentType.Cash;

                if (hasDepositAllocation)
                {
                    financialOutcome = InpatientFinancialOutcomes.SettledWithDeposit;
                }
                else if (hasGuarantor)
                {
                    financialOutcome = InpatientFinancialOutcomes.InsuranceGuaranteed;
                }
                else
                {
                    financialOutcome = InpatientFinancialOutcomes.FullyPaid;
                }
            }
            else
            {
                clearanceStatus = InpatientClearanceStatuses.Blocked;
                financialOutcome = null;
            }
        }

        // BKC-DEC-115: Nomor versi finansial monoton naik per encounter
        var latestHandoff = await _dbContext.BilInpatientClearanceHandoffs
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .OrderByDescending(x => x.FinancialVersion)
            .FirstOrDefaultAsync(cancellationToken);

        long nextVersion = (latestHandoff?.FinancialVersion ?? 0) + 1;

        var handoff = new BilInpatientClearanceHandoff
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            InvoiceId = invoice?.Id ?? Guid.Empty,
            ClearanceStatus = clearanceStatus,
            FinancialOutcome = financialOutcome,
            OutstandingBalance = outstanding,
            TotalPatientResponsibility = totalPatientResponsibility,
            TotalPaidOrAllocated = totalPaidOrAllocated,
            ReasonCode = reasonCode,
            RevocationReason = clearanceStatus == InpatientClearanceStatuses.Revoked
                ? (string.IsNullOrWhiteSpace(revocationReason) ? reasonCode : revocationReason.Trim())
                : null,
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

        _dbContext.BilInpatientClearanceHandoffs.Add(handoff);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(
            LogCategory,
            "InpatientClearance.Evaluated",
            $"Surat kelayakan pemulangan diterbitkan: Status={clearanceStatus}, Versi={nextVersion}, Sisa={outstanding:N0}.",
            new
            {
                handoff.Id,
                handoff.EncounterId,
                handoff.InvoiceId,
                handoff.ClearanceStatus,
                handoff.FinancialOutcome,
                handoff.OutstandingBalance,
                handoff.FinancialVersion,
                handoff.ReasonCode,
                handoff.RevocationReason,
                ActorUserId = actorUserId
            });

        return handoff;
    }

    /// <inheritdoc />
    public async Task<MajorProcedureDepositValidationResult> ValidateMajorProcedureDepositAsync(
        MajorProcedureDepositValidationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // BKC-DEC-114, BKC-DES-047, BIL-VAL-121: Deposit 100% dari ekses/tanggung jawab pasien, bukan total tagihan
        var patientExcess = Math.Max(0m, request.EstimatedCost - (request.GuarantorCoverageAmount ?? 0m));

        var depositAccount = await _dbContext.BilDepositAccounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EncounterId == request.EncounterId && x.Status == BillingDepositAccountStatuses.Active && !x.IsDelete, cancellationToken);

        var availableBalance = depositAccount?.AvailableBalance ?? 0m;
        var shortfall = Math.Max(0m, patientExcess - availableBalance);
        var isSufficient = availableBalance >= patientExcess;

        var message = isSufficient
            ? "Saldo deposit mencukupi untuk porsi tanggung jawab pasien (ekses) tindakan besar."
            : "Saldo deposit belum memenuhi 100% porsi tanggung jawab pasien untuk tindakan besar. Pasien/keluarga wajib menyetor kekurangan deposit";

        return new MajorProcedureDepositValidationResult
        {
            EncounterId = request.EncounterId,
            EstimatedCost = request.EstimatedCost,
            GuarantorCoverageAmount = request.GuarantorCoverageAmount ?? 0m,
            PatientExcess = patientExcess,
            AvailableDepositBalance = availableBalance,
            DepositShortfall = shortfall,
            IsSufficient = isSufficient,
            Message = message
        };
    }

    /// <inheritdoc />
    public async Task<BilInpatientClearanceHandoff?> TriggerAutoReblockIfApplicableAsync(
        Guid invoiceId,
        string triggerReason,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);

        if (invoice == null) return null;

        // Auto-Reblock hanya berlaku untuk episode rawat inap
        if (!string.Equals(invoice.ServiceType, "INPATIENT", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Cek apakah status clearance terakhir untuk encounter ini adalah CLEARED
        var latestHandoff = await _dbContext.BilInpatientClearanceHandoffs
            .Where(x => x.EncounterId == invoice.EncounterId && !x.IsDelete)
            .OrderByDescending(x => x.FinancialVersion)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestHandoff != null && latestHandoff.ClearanceStatus == InpatientClearanceStatuses.Cleared)
        {
            // BKC-DEC-116, BKC-DES-046, BIL-VAL-123: Cabut izin pulang seketika menjadi REVOKED
            return await EvaluateClearanceAsync(
                invoice.EncounterId,
                InpatientClearanceReasonCodes.LateChargePosted,
                actorUserId,
                occurredAt,
                correlationId,
                causationId,
                cancellationToken,
                triggerReason ?? "LATE_CHARGE_POSTED");
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<BilInpatientClearanceHandoff> AcknowledgeClearanceHandoffAsync(
        Guid handoffId,
        Guid actorUserId,
        DateTimeOffset acknowledgedAt,
        CancellationToken cancellationToken)
    {
        var handoff = await _dbContext.BilInpatientClearanceHandoffs
            .FirstOrDefaultAsync(x => x.Id == handoffId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Surat handoff kelayakan dengan ID {handoffId} tidak ditemukan.");

        // BIL-VAL-116, BIL-VAL-125: Idempotensi pengakuan penerimaan surat
        if (handoff.Status == BillingHandoffStatuses.Acknowledged)
        {
            return handoff;
        }

        handoff.Status = BillingHandoffStatuses.Acknowledged;
        handoff.AcknowledgedAt = acknowledgedAt;
        handoff.UpdateDateTime = DateTime.UtcNow;
        handoff.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(
            LogCategory,
            "InpatientClearance.Acknowledged",
            $"Surat handoff kelayakan pemulangan diakui oleh bangsal rawat inap: ID={handoff.Id}, Encounter={handoff.EncounterId}.",
            new
            {
                handoff.Id,
                handoff.EncounterId,
                handoff.ClearanceStatus,
                ActorUserId = actorUserId,
                AcknowledgedAt = acknowledgedAt
            });

        return handoff;
    }

    /// <inheritdoc />
    public async Task<BilInpatientClearanceHandoff?> GetLatestClearanceForEncounterAsync(
        Guid encounterId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.BilInpatientClearanceHandoffs.AsNoTracking()
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .OrderByDescending(x => x.FinancialVersion)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InpatientBillingSummaryResponse> GetInpatientBillingSummaryAsync(
        Guid encounterId,
        bool includeFinancialDetails,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

        var latestHandoff = await GetLatestClearanceForEncounterAsync(encounterId, cancellationToken);

        var depositAccount = await _dbContext.BilDepositAccounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId && x.Status == BillingDepositAccountStatuses.Active && !x.IsDelete, cancellationToken);

        var depositBalance = depositAccount?.AvailableBalance ?? 0m;
        var clearanceStatus = latestHandoff?.ClearanceStatus ?? InpatientClearanceStatuses.Pending;
        var canDischarge = clearanceStatus == InpatientClearanceStatuses.Cleared;

        var blockerReasons = new List<string>();
        decimal outstanding = latestHandoff?.OutstandingBalance ?? 0m;

        if (outstanding > 0m)
        {
            blockerReasons.Add($"Pasien masih memiliki sisa tanggung jawab mandiri (patient excess) sebesar Rp {outstanding:N0} yang belum dilunasi di kasir utama");
        }
        if (clearanceStatus == InpatientClearanceStatuses.Revoked)
        {
            blockerReasons.Add($"Izin pulang sebelumnya dicabut (REVOKED) karena ada tagihan susulan ({latestHandoff?.RevocationReason ?? "LATE_CHARGE_POSTED"}). Silakan hubungi kasir");
        }
        else if (clearanceStatus == InpatientClearanceStatuses.Blocked && outstanding <= 0m)
        {
            blockerReasons.Add("Status kelayakan ditahan (BLOCKED) menunggu penilaian kasir atau verifikasi deposit tindakan");
        }

        var totalCharges = invoice?.Items
            .Where(x => x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
            .Sum(x => x.Quantity * x.UnitPrice);

        return new InpatientBillingSummaryResponse
        {
            EncounterId = encounterId,
            InvoiceId = invoice?.Id,
            InvoiceNumber = invoice?.InvoiceNumber,
            BillingStatus = invoice?.Status ?? BillingInvoiceStatuses.Open,
            FinancialClearanceStatus = clearanceStatus,
            CanDischarge = canDischarge,
            BlockerReasons = blockerReasons,
            DepositRequired = includeFinancialDetails ? 0m : null,
            DepositBalance = includeFinancialDetails ? depositBalance : null,
            DepositShortfall = includeFinancialDetails ? 0m : null,
            TotalCharges = includeFinancialDetails ? totalCharges : null,
            Outstanding = includeFinancialDetails ? outstanding : null
        };
    }
}
