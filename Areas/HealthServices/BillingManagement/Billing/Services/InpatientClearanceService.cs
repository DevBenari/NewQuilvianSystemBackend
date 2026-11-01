using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
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

        if (invoice == null)
        {
            throw new InvalidOperationException("Tagihan rawat inap belum terbentuk (INVOICE_NOT_FORMED). Evaluasi kelayakan tidak dapat diproses.");
        }

        // Ambil kalkulasi terakhir yang authoritative
        var latestCalc = await _dbContext.BilCalculationVersions.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id && !x.IsDelete)
            .OrderByDescending(x => x.VersionNo)
            .FirstOrDefaultAsync(cancellationToken);

        var activeItems = invoice.Items
            .Where(x => x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
            .ToList();

        // Gunakan layanan penutupan invoice otoritatif untuk kalkulasi sisa kewajiban pasien (outstanding)
        var closureService = new BillingInvoiceClosureService(_dbContext);
        decimal totalPatientResponsibility = latestCalc?.PatientAmount
            ?? activeItems.Sum(x => x.Quantity * x.UnitPrice);
        decimal outstanding;

        if (latestCalc != null)
        {
            outstanding = await closureService.CalculateOutstandingAsync(invoice, latestCalc, cancellationToken);
        }
        else
        {
            var totalAllocated = await _dbContext.BilPaymentAllocations.AsNoTracking()
                .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                .SumAsync(x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount), cancellationToken) ?? 0m;
            outstanding = Math.Max(0m, totalPatientResponsibility - totalAllocated);
        }

        var totalPaidOrAllocated = await _dbContext.BilPaymentAllocations.AsNoTracking()
            .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
            .SumAsync(x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount), cancellationToken) ?? 0m;

        // Evaluasi verifikasi penjamin / asuransi
        var guarantor = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .OrderByDescending(x => x.IsPrimary)
            .FirstOrDefaultAsync(cancellationToken);

        var isNonCashGuarantor = guarantor != null && guarantor.PaymentType != EncounterPaymentType.Cash;
        bool isGuarantorVerified = false;
        if (isNonCashGuarantor && guarantor != null)
        {
            isGuarantorVerified = guarantor.IsActive && guarantor.IsEligible && guarantor.IsPolicyActive;
        }

        // Syarat kelayakan invoice: invoice harus sudah dihitung, item aktif ada, tidak butuh review, dan penjamin terverifikasi
        var isRevocation = reasonCode is InpatientClearanceReasonCodes.LateChargePosted
            or InpatientClearanceReasonCodes.PaymentReversed;

        bool isInvoiceUncalculatedOrIncomplete = latestCalc == null
            || (activeItems.Count == 0 && invoice.Status != BillingInvoiceStatuses.Closed)
            || invoice.RequiresReview;

        bool hasUnverifiedGuarantor = isNonCashGuarantor && (!isGuarantorVerified || (latestCalc != null && latestCalc.UnresolvedCoverageAmount > 0));

        
        // Evaluasi verifikasi episode kepulangan rawat inap
        var episode = await _dbContext.InpEpisodes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

        bool isClinicalDischargeBlocked = false;
        string? clinicalBlockerMessage = null;

        if (episode != null)
        {
            bool isDischargeDecided = episode.EpisodeStatus == InpEpisodeStatus.DischargePending 
                || episode.EpisodeStatus == InpEpisodeStatus.Closed
                || episode.DischargeDecidedAt.HasValue;

            var summary = await _dbContext.InpDischargeSummaries.AsNoTracking()
                .Where(x => x.EpisodeId == episode.Id && !x.IsDelete)
                .Select(x => new { x.SignedAt })
                .FirstOrDefaultAsync(cancellationToken);
            bool isSummarySigned = summary?.SignedAt != null;

            var markedItemIds = await _dbContext.InpClearanceMarks.AsNoTracking()
                .Where(x => x.EpisodeId == episode.Id && !x.IsDelete)
                .Select(x => x.ClearanceItemId)
                .ToListAsync(cancellationToken);

            var missingMandatoryItems = await _dbContext.MstInpatientClearanceItems.AsNoTracking()
                .Where(x => !x.IsDelete && x.ChecklistType == MstClearanceChecklistType.EpisodeClosure && x.IsMandatory && x.IsActive && !markedItemIds.Contains(x.Id))
                .Select(x => x.ItemName)
                .ToListAsync(cancellationToken);
            bool isChecklistComplete = missingMandatoryItems.Count == 0;

            if (!isDischargeDecided || !isSummarySigned || !isChecklistComplete)
            {
                isClinicalDischargeBlocked = true;
                var blockerParts = new List<string>();
                if (!isDischargeDecided)
                    blockerParts.Add("Keputusan pemulangan dari Dokter DPJP belum ada.");
                if (!isSummarySigned)
                    blockerParts.Add("Resume medis pemulangan belum ditandatangani DPJP.");
                if (!isChecklistComplete)
                    blockerParts.Add($"Butir wajib administrasi belum lengkap: {string.Join(", ", missingMandatoryItems)}.");
                
                clinicalBlockerMessage = string.Join(" ", blockerParts);
            }
        }
string clearanceStatus;
        string? financialOutcome = null;

        if (isRevocation)
        {
            clearanceStatus = InpatientClearanceStatuses.Revoked;
            financialOutcome = null;
        }
        else if (isClinicalDischargeBlocked)
        {
            clearanceStatus = InpatientClearanceStatuses.Blocked;
            financialOutcome = null;
        }
        else if (isInvoiceUncalculatedOrIncomplete || hasUnverifiedGuarantor)
        {
            clearanceStatus = InpatientClearanceStatuses.Blocked;
            financialOutcome = null;
        }
        else if (outstanding <= 0m && (totalPatientResponsibility > 0m || totalPaidOrAllocated > 0m || (latestCalc != null && latestCalc.PrimaryAmount > 0m) || invoice.Status == BillingInvoiceStatuses.Closed))
        {
            clearanceStatus = InpatientClearanceStatuses.Cleared;

            var hasDepositAllocation = await _dbContext.BilPaymentAllocations.AsNoTracking()
                .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                .Join(
                    _dbContext.BilSettlements.AsNoTracking().Where(s => !s.IsDelete && (s.Status == BillingSettlementStatuses.Settled || s.Status == BillingSettlementStatuses.PartiallySettled)),
                    a => a.SettlementId,
                    s => s.Id,
                    (a, s) => s)
                .AnyAsync(s => s.DepositAccountId.HasValue, cancellationToken);

            if (hasDepositAllocation)
            {
                financialOutcome = InpatientFinancialOutcomes.SettledWithDeposit;
            }
            else if (isNonCashGuarantor && isGuarantorVerified && latestCalc != null && latestCalc.PrimaryAmount > 0m)
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
            clearanceStatus = InpatientClearanceStatuses.Pending;
            financialOutcome = null;
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
            InvoiceId = invoice.Id,
            ClearanceStatus = clearanceStatus,
            FinancialOutcome = financialOutcome,
            OutstandingBalance = outstanding,
            TotalPatientResponsibility = totalPatientResponsibility,
            TotalPaidOrAllocated = totalPaidOrAllocated,
            ReasonCode = reasonCode,
            RevocationReason = clearanceStatus == InpatientClearanceStatuses.Revoked
                ? (string.IsNullOrWhiteSpace(revocationReason) ? reasonCode : revocationReason.Trim())
                : clearanceStatus == InpatientClearanceStatuses.Blocked
                    ? (clinicalBlockerMessage ?? revocationReason)
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

        // Auto-Reblock hanya berlaku untuk episode rawat inap. Invoice rawat inap selalu berlabel "RANAP"
        if (!string.Equals(invoice.ServiceType, "RANAP", StringComparison.OrdinalIgnoreCase))
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
            var reasonCode = triggerReason switch
            {
                "PaymentReversed" => InpatientClearanceReasonCodes.PaymentReversed,
                _ => InpatientClearanceReasonCodes.LateChargePosted
            };

            return await EvaluateClearanceAsync(
                invoice.EncounterId,
                reasonCode,
                actorUserId,
                occurredAt,
                correlationId,
                causationId,
                cancellationToken,
                triggerReason ?? reasonCode);
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
    public async Task<InpatientClearanceStatusView> GetLatestStatusAsync(
        Guid encounterId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .Select(x => new { x.Status, x.RequiresReview, x.ReviewReasonCode })
            .FirstOrDefaultAsync(cancellationToken);

        // Invoice belum ada: status PENDING dan InvoiceStatus NONE (kontrak integrasi 4.3).
        if (invoice == null)
        {
            return new InpatientClearanceStatusView
            {
                Status = InpatientClearanceStatuses.Pending,
                InvoiceStatus = InpatientInvoiceStatusViews.None,
                Reasons =
                [
                    new InpatientClearanceReasonView
                    {
                        Code = "INVOICE_NOT_FORMED",
                        Label = "Tagihan rawat inap belum terbentuk."
                    }
                ]
            };
        }

        var latest = await GetLatestClearanceForEncounterAsync(encounterId, cancellationToken);
        var view = new InpatientClearanceStatusView
        {
            Status = latest?.ClearanceStatus ?? InpatientClearanceStatuses.Pending,
            EvaluatedAt = latest?.EffectiveAt,
            InvoiceStatus = invoice.Status
        };

        if (latest != null && latest.ClearanceStatus != InpatientClearanceStatuses.Cleared)
        {
            var code = latest.RevocationReason ?? latest.ReasonCode;
            string label;
            if (latest.ClearanceStatus == InpatientClearanceStatuses.Revoked)
            {
                label = "Izin kasir dicabut karena ada perubahan tagihan.";
            }
            else if (latest.ClearanceStatus == InpatientClearanceStatuses.Blocked)
            {
                label = !string.IsNullOrWhiteSpace(latest.RevocationReason)
                    ? latest.RevocationReason
                    : "Kasir menahan izin pulang.";
            }
            else
            {
                label = "Menunggu pembayaran di kasir.";
            }

            view.Reasons.Add(new InpatientClearanceReasonView
            {
                Code = code,
                Label = label
            });
        }
        else if (latest == null)
        {
            view.Reasons.Add(new InpatientClearanceReasonView
            {
                Code = "AWAITING_PAYMENT",
                Label = "Menunggu pembayaran di kasir."
            });
        }

        return view;
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

        BilCalculationVersion? currentCalc = null;
        decimal currentOutstanding = latestHandoff?.OutstandingBalance ?? 0m;
        decimal currentTotalCharges = invoice?.Items
            .Where(x => x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
            .Sum(x => x.Quantity * x.UnitPrice) ?? 0m;

        if (invoice != null)
        {
            currentCalc = await _dbContext.BilCalculationVersions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.InvoiceId == invoice.Id && !x.IsDelete && x.VersionNo == invoice.CurrentCalculationVersion, cancellationToken)
                ?? await _dbContext.BilCalculationVersions.AsNoTracking()
                    .Where(x => x.InvoiceId == invoice.Id && !x.IsDelete)
                    .OrderByDescending(x => x.VersionNo)
                    .FirstOrDefaultAsync(cancellationToken);

            var closureService = new BillingInvoiceClosureService(_dbContext);
            if (currentCalc != null)
            {
                currentOutstanding = await closureService.CalculateOutstandingAsync(invoice, currentCalc, cancellationToken);
                currentTotalCharges = currentCalc.GrossAmount + currentCalc.AdministrationFeeAmount + currentCalc.RoomChargeAmount - currentCalc.ItemDiscount + currentCalc.TaxAmount + currentCalc.RoundingAmount;
            }
            else
            {
                var totalPaid = await _dbContext.BilPaymentAllocations.AsNoTracking()
                    .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                    .SumAsync(x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount), cancellationToken) ?? 0m;
                currentOutstanding = Math.Max(0m, currentTotalCharges - totalPaid);
            }
        }

        var clearanceStatus = latestHandoff?.ClearanceStatus ?? InpatientClearanceStatuses.Pending;
        if (invoice != null && (invoice.Status == BillingInvoiceStatuses.Closed || currentOutstanding <= 0m))
        {
            if (clearanceStatus != InpatientClearanceStatuses.Revoked && clearanceStatus != InpatientClearanceStatuses.Blocked)
            {
                clearanceStatus = InpatientClearanceStatuses.Cleared;
            }
        }
        else if (currentOutstanding > 0m && clearanceStatus == InpatientClearanceStatuses.Cleared)
        {
            clearanceStatus = InpatientClearanceStatuses.Pending;
        }

        var canDischarge = clearanceStatus == InpatientClearanceStatuses.Cleared;
        var blockerReasons = new List<string>();
        decimal outstanding = currentOutstanding;
        var totalCharges = currentTotalCharges;

        // Evaluasi verifikasi kesiapan pemulangan klinis & administrasi episode ranap
        var readiness = await EvaluateInpatientDischargePaymentReadinessAsync(
            _dbContext, encounterId, cancellationToken);

        if (!readiness.CanProcessPayment)
        {
            blockerReasons.AddRange(readiness.Blockers);
            if (clearanceStatus != InpatientClearanceStatuses.Cleared)
            {
                clearanceStatus = InpatientClearanceStatuses.Blocked;
                canDischarge = false;
            }
        }

        if (invoice == null)
        {
            blockerReasons.Add(includeFinancialDetails
                ? "Tagihan rawat inap belum terbentuk (INVOICE_NOT_FORMED). Silakan hubungi kasir."
                : "Tagihan rawat inap belum terbentuk. Silakan hubungi kasir.");
        }
        else
        {
            if (clearanceStatus == InpatientClearanceStatuses.Revoked)
            {
                blockerReasons.Add(includeFinancialDetails
                    ? $"Izin pulang sebelumnya dicabut (REVOKED) karena ada tagihan susulan ({latestHandoff?.RevocationReason ?? "LATE_CHARGE_POSTED"}). Silakan hubungi kasir."
                    : "Izin pulang sebelumnya dicabut karena ada perubahan atau tagihan susulan. Silakan hubungi kasir.");
            }
        }

        return new InpatientBillingSummaryResponse
        {
            EncounterId = encounterId,
            InvoiceId = invoice?.Id,
            InvoiceNumber = invoice?.InvoiceNumber,
            BillingStatus = invoice?.Status ?? BillingInvoiceStatuses.Open,
            FinancialClearanceStatus = clearanceStatus,
            CanDischarge = canDischarge,
            CanProcessPayment = readiness.CanProcessPayment,
            BlockerReasons = blockerReasons,
            PaymentBlockerReasons = readiness.Blockers,
            IsDischargeDecided = readiness.IsDischargeDecided,
            IsSummarySigned = readiness.IsSummarySigned,
            IsChecklistComplete = readiness.IsChecklistComplete,
            DepositRequired = includeFinancialDetails ? 0m : null,
            DepositBalance = includeFinancialDetails ? depositBalance : null,
            DepositShortfall = includeFinancialDetails ? 0m : null,
            TotalCharges = includeFinancialDetails ? totalCharges : null,
            Outstanding = includeFinancialDetails ? outstanding : null,
            FinancialVersion = latestHandoff?.FinancialVersion,
            EvaluatedAt = latestHandoff?.EffectiveAt,
            CorrelationId = latestHandoff?.CorrelationId,
            CausationId = latestHandoff?.CausationId
        };
    }

    /// <summary>
    /// Evaluasi otoritatif kelayakan pembayaran akhir rawat inap di kasir berdasarkan persyaratan klinis (DPJP)
    /// dan administrasi admisi/episode, terpisah dari status kelayakan pemulangan fisik (Financial Clearance / canDischarge).
    /// </summary>
    public static async Task<InpatientDischargePaymentReadinessResult> EvaluateInpatientDischargePaymentReadinessAsync(
        ApplicationDbContext dbContext,
        Guid encounterId,
        CancellationToken cancellationToken)
    {
        var episode = await dbContext.InpEpisodes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

        if (episode == null)
        {
            return new InpatientDischargePaymentReadinessResult
            {
                CanProcessPayment = true,
                IsDischargeDecided = true,
                IsSummarySigned = true,
                IsChecklistComplete = true,
                Blockers = new List<string>()
            };
        }

        bool isDischargeDecided = episode.EpisodeStatus == InpEpisodeStatus.DischargePending 
            || episode.EpisodeStatus == InpEpisodeStatus.Closed
            || episode.DischargeDecidedAt.HasValue;

        var summary = await dbContext.InpDischargeSummaries.AsNoTracking()
            .Where(x => x.EpisodeId == episode.Id && !x.IsDelete)
            .Select(x => new { x.SignedAt })
            .FirstOrDefaultAsync(cancellationToken);
        bool isSummarySigned = summary?.SignedAt != null;

        var markedItemIds = await dbContext.InpClearanceMarks.AsNoTracking()
            .Where(x => x.EpisodeId == episode.Id && !x.IsDelete)
            .Select(x => x.ClearanceItemId)
            .ToListAsync(cancellationToken);

        var missingMandatoryItems = await dbContext.MstInpatientClearanceItems.AsNoTracking()
            .Where(x => !x.IsDelete && x.ChecklistType == MstClearanceChecklistType.EpisodeClosure && x.IsMandatory && x.IsActive && !markedItemIds.Contains(x.Id))
            .Select(x => x.ItemName)
            .ToListAsync(cancellationToken);
        bool isChecklistComplete = missingMandatoryItems.Count == 0;

        var blockers = new List<string>();
        if (!isDischargeDecided)
        {
            blockers.Add("Keputusan pemulangan dari Dokter DPJP belum ditetapkan.");
        }
        if (!isSummarySigned)
        {
            blockers.Add("Resume medis pemulangan belum ditandatangani oleh dokter DPJP.");
        }
        if (!isChecklistComplete)
        {
            blockers.Add($"Daftar periksa administrasi kepulangan belum lengkap: {string.Join(", ", missingMandatoryItems)}.");
        }

        return new InpatientDischargePaymentReadinessResult
        {
            CanProcessPayment = blockers.Count == 0,
            IsDischargeDecided = isDischargeDecided,
            IsSummarySigned = isSummarySigned,
            IsChecklistComplete = isChecklistComplete,
            Blockers = blockers
        };
    }
}
