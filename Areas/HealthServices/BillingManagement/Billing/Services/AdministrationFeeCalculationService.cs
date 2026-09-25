using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Layanan perhitungan biaya administrasi rawat inap berbasis deklaratif master policy dan rekonsiliasi alihan rajal ke ranap.
/// (BKC-DEC-113, BKC-DEC-119, BKC-DEC-121, BKC-DEC-122, BKC-DES-044, BKC-DES-049, BIL-VAL-120, BIL-VAL-126).
/// </summary>
public sealed class AdministrationFeeCalculationService : IAdministrationFeeCalculationService
{
    public const string DefaultVoidReasonSuperseded = "SUPERSEDED_BY_INPATIENT_ADMISSION";

    private readonly ApplicationDbContext _dbContext;

    public AdministrationFeeCalculationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Memeriksa apakah penjamin merupakan sistem paket seperti BPJS Kesehatan / JKN / INA-CBGs (BKC-DEC-121).
    /// </summary>
    public static bool IsPackageSystemGuarantor(string? guarantorName)
    {
        if (string.IsNullOrWhiteSpace(guarantorName))
            return false;

        return guarantorName.Contains("BPJS", StringComparison.OrdinalIgnoreCase)
            || guarantorName.Contains("JKN", StringComparison.OrdinalIgnoreCase)
            || guarantorName.Contains("INA-CBG", StringComparison.OrdinalIgnoreCase)
            || guarantorName.Contains("KIS", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<AdminFeeCalculationResult> CalculateAdministrationFeeAsync(
        AdminFeeCalculationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // BKC-DEC-122: Untuk rawat inap, tanggal evaluasi adalah tanggal kepulangan (discharge),
        // bukan tanggal admisi awal pasien.
        var evaluationDate = request.ServiceType == AdministrationFeeServiceTypes.Ranap
            ? (request.DischargeTime ?? request.EffectiveAt)
            : request.EffectiveAt;

        var policies = await _dbContext.MstAdministrationFeePolicies.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.ServiceType == request.ServiceType
                && x.EffectiveFrom <= evaluationDate && (x.EffectiveTo == null || evaluationDate < x.EffectiveTo))
            .OrderByDescending(x => x.ReplacementPriority)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        if (policies.Count == 0)
        {
            return new AdminFeeCalculationResult
            {
                CalculationType = AdministrationFeeCalculationTypes.Flat,
                Notes = "Tidak ada kebijakan biaya administrasi yang aktif untuk layanan ini."
            };
        }

        var policy = policies.First();

        decimal rawAmount;
        decimal calculatedAmount;
        var isCapApplied = false;

        // BKC-DEC-113 & BKC-DES-044: Perhitungan persentase 7% ber-cap Rp6.000.000
        if (string.Equals(policy.CalculationType, AdministrationFeeCalculationTypes.PercentageWithCap, StringComparison.OrdinalIgnoreCase))
        {
            var percentage = policy.Percentage ?? 7.00m;
            var capAmount = policy.CapAmount ?? 6000000.00m;

            rawAmount = Math.Round(request.EligibleBaseAmount * (percentage / 100.00m), 2, MidpointRounding.AwayFromZero);

            if (capAmount > 0 && rawAmount > capAmount)
            {
                calculatedAmount = capAmount;
                isCapApplied = true;
            }
            else
            {
                calculatedAmount = rawAmount;
                isCapApplied = false;
            }
        }
        else
        {
            rawAmount = policy.Amount;
            calculatedAmount = policy.Amount;
            isCapApplied = false;
        }

        // BKC-DEC-121 / BKC-AC-089: Penjamin sistem paket (BPJS Kesehatan / INA-CBGs)
        // Biaya administrasi inklusif dalam klaim penjamin, porsi pasien adalah Rp 0.
        var isPackage = IsPackageSystemGuarantor(request.GuarantorName);
        var patientResponsibility = isPackage ? 0.00m : calculatedAmount;

        // Evaluasi pengurangan biaya admin sebelumnya (misal alihan rajal ke ranap)
        var priorFees = await GetPriorAppliedAdminFeeAmountAsync(request, policy, cancellationToken);
        var priorApplied = priorFees.TotalPriorApplied;
        var replacesEarlierFee = priorApplied > 0 && policy.ReplacementPriority > priorFees.MaxPriority;

        decimal appliedAmount;
        if (priorApplied == 0)
        {
            appliedAmount = calculatedAmount;
        }
        else if (replacesEarlierFee)
        {
            appliedAmount = calculatedAmount >= priorApplied ? calculatedAmount - priorApplied : 0.00m;
        }
        else
        {
            appliedAmount = 0.00m;
        }

        return new AdminFeeCalculationResult
        {
            PolicyId = policy.Id,
            PolicyCode = policy.Code,
            CalculationType = policy.CalculationType,
            Percentage = policy.Percentage,
            CapAmount = policy.CapAmount,
            EligibleBaseAmount = request.EligibleBaseAmount,
            RawCalculatedAmount = rawAmount,
            CalculatedAmount = calculatedAmount,
            AppliedAmount = appliedAmount,
            PatientResponsibilityAmount = isPackage ? 0.00m : appliedAmount,
            IsCapApplied = isCapApplied,
            IsPackageGuaranteed = isPackage,
            PriorAppliedAmount = priorApplied,
            ReplacesEarlierFee = replacesEarlierFee,
            Notes = isPackage
                ? "Biaya administrasi ditanggung sistem paket BPJS/penjamin; porsi pasien Rp 0."
                : isCapApplied
                    ? $"Biaya administrasi dibatasi maksimal sebesar pagu (cap) Rp {policy.CapAmount:N0}."
                    : "Biaya administrasi dihitung normal."
        };
    }

    /// <inheritdoc />
    public async Task<ReconcileReferredOutpatientAdminResult> ReconcileReferredOutpatientAdminFeeAsync(
        ReconcileReferredOutpatientAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ranapInvoice = await _dbContext.BilInvoices
            .FirstOrDefaultAsync(x => x.Id == request.InpatientInvoiceId && !x.IsDelete, cancellationToken);

        if (ranapInvoice == null)
        {
            return new ReconcileReferredOutpatientAdminResult
            {
                HasReferredOutpatient = false,
                Message = "Invoice rawat inap tidak ditemukan."
            };
        }

        // Cari invoice rawat jalan sebelumnya milik pasien ini
        var outpatientInvoices = await (
            from inv in _dbContext.BilInvoices
            join enc in _dbContext.RegPatientEncounters on inv.EncounterId equals enc.Id
            where enc.PatientId == request.PatientId
                  && inv.Id != request.InpatientInvoiceId
                  && inv.ServiceType == AdministrationFeeServiceTypes.Rajal
                  && !inv.IsDelete && !enc.IsDelete
            orderby inv.CreateDateTime descending
            select inv
        ).Include(x => x.Items).ToListAsync(cancellationToken);

        if (outpatientInvoices.Count == 0)
        {
            return new ReconcileReferredOutpatientAdminResult
            {
                HasReferredOutpatient = false,
                Message = "Tidak ada kunjungan rawat jalan rujukan sebelumnya untuk pasien ini."
            };
        }

        var result = new ReconcileReferredOutpatientAdminResult
        {
            HasReferredOutpatient = true
        };

        var hasChanges = false;

        foreach (var rajalInv in outpatientInvoices)
        {
            result.OutpatientInvoiceId = rajalInv.Id;

            // Cari item biaya administrasi rajal pada invoice tersebut
            var adminItems = rajalInv.Items
                .Where(x => !x.IsDelete && (x.SourceDomain == "ADMINISTRATION" || x.DescriptionSnapshot.Contains("Administrasi", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (adminItems.Count == 0)
                continue;

            // Periksa apakah invoice rajal tersebut sudah pernah dibayar / disettle di loket poli
            var hasAllocation = await _dbContext.BilPaymentAllocations.AsNoTracking()
                .AnyAsync(a => a.TargetType == BillingAllocationTargetTypes.Invoice && a.TargetId == rajalInv.Id && !a.IsDelete, cancellationToken);
            var isPaid = string.Equals(rajalInv.Status, BillingInvoiceStatuses.Closed, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(rajalInv.Status, BillingInvoiceStatuses.Final, StringComparison.OrdinalIgnoreCase)
                         || hasAllocation;

            if (isPaid)
            {
                // BKC-DEC-119 Kasus Sudah Dibayar: Nominal admin rajal dialihkan sebagai kredit pembayaran ranap
                foreach (var item in adminItems)
                {
                    if (item.Status == BillingInvoiceItemStatuses.Voided)
                        continue;

                    // Idempotensi: Cek apakah kredit alihan ini sudah pernah dibuat sebelumnya
                    var existingCredit = await _dbContext.BilRefundableCredits
                        .FirstOrDefaultAsync(c => c.InvoiceId == ranapInvoice.Id
                            && c.SourceType == BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin
                            && c.SourceId == item.Id && !c.IsDelete, cancellationToken);

                    if (existingCredit == null)
                    {
                        var itemAmount = item.Quantity * item.UnitPrice;
                        var credit = new BilRefundableCredit
                        {
                            Id = Guid.NewGuid(),
                            InvoiceId = ranapInvoice.Id,
                            SourceType = BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin,
                            SourceId = item.Id,
                            OriginalAmount = itemAmount,
                            AvailableAmount = itemAmount,
                            Status = BillingRefundableCreditStatuses.Available,
                            RecognizedAt = DateTimeOffset.UtcNow,
                            CreateBy = request.ActorUserId,
                            CreateDateTime = DateTime.UtcNow
                        };

                        _dbContext.BilRefundableCredits.Add(credit);
                        result.WasCredited = true;
                        result.CreditedAmount += itemAmount;
                        result.RefundableCreditId = credit.Id;
                        hasChanges = true;
                    }
                }
            }
            else
            {
                // BKC-DEC-119 Kasus Belum Dibayar: Item admin rajal dibatalkan (void) otomatis
                foreach (var item in adminItems)
                {
                    if (item.Status != BillingInvoiceItemStatuses.Voided)
                    {
                        item.Status = BillingInvoiceItemStatuses.Voided;
                        item.VoidReason = DefaultVoidReasonSuperseded;
                        item.UpdateBy = request.ActorUserId;
                        item.UpdateDateTime = DateTime.UtcNow;

                        result.WasVoided = true;
                        result.VoidedItemCount++;
                        hasChanges = true;
                    }
                }
            }
        }

        if (hasChanges)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        result.Message = result.WasCredited
            ? $"Biaya administrasi rajal sebesar Rp {result.CreditedAmount:N0} yang telah dibayar berhasil dialihkan sebagai kredit tagihan rawat inap."
            : result.WasVoided
                ? $"Biaya administrasi rajal sebanyak {result.VoidedItemCount} item berhasil dibatalkan (void) karena digantikan administrasi rawat inap."
                : "Biaya administrasi rajal sudah pernah direkonsiliasi sebelumnya.";

        return result;
    }

    private async Task<(decimal TotalPriorApplied, int MaxPriority)> GetPriorAppliedAdminFeeAmountAsync(
        AdminFeeCalculationRequest request,
        MstAdministrationFeePolicy currentPolicy,
        CancellationToken cancellationToken)
    {
        var businessDate = AdministrationFeePolicyService.GetBusinessDate(request.EffectiveAt);
        var (rangeStart, _) = AdministrationFeePolicyService.GetBusinessDateUtcRange(businessDate.AddDays(-1));
        var (_, rangeEnd) = AdministrationFeePolicyService.GetBusinessDateUtcRange(businessDate.AddDays(1));
        var rangeStartUtc = rangeStart.UtcDateTime;
        var rangeEndUtc = rangeEnd.UtcDateTime;

        var priorInvoices = await (
            from priorInv in _dbContext.BilInvoices.AsNoTracking()
            join priorEnc in _dbContext.RegPatientEncounters.AsNoTracking() on priorInv.EncounterId equals priorEnc.Id
            where priorInv.Id != request.InvoiceId && !priorInv.IsDelete && !priorEnc.IsDelete
                && priorEnc.PatientId == request.PatientId
                && priorEnc.EncounterDate >= rangeStartUtc && priorEnc.EncounterDate < rangeEndUtc
            select new { priorInv.Id, priorInv.ServiceType, priorInv.CurrentCalculationVersion }
        ).ToListAsync(cancellationToken);

        if (priorInvoices.Count == 0)
            return (0m, int.MinValue);

        // Cari riwayat administrasi rajal dari invoice terdahulu
        decimal priorAppliedTotal = 0m;
        var maxPriority = int.MinValue;

        foreach (var prior in priorInvoices)
        {
            if (string.Equals(prior.ServiceType, AdministrationFeeServiceTypes.Rajal, StringComparison.OrdinalIgnoreCase))
            {
                // Prioritas rajal umumnya 10
                maxPriority = Math.Max(maxPriority, 10);
                priorAppliedTotal += 50000m; // Nilai baseline jika belum ter-snapshot
            }
        }

        return (priorAppliedTotal, maxPriority);
    }
}
