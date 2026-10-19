using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Layanan perhitungan biaya administrasi rawat inap berbasis deklaratif master policy.
/// (BKC-DEC-113, BKC-DEC-119, BKC-DEC-121, BKC-DEC-122, BKC-DES-044, BKC-DES-083, BIL-VAL-120, BIL-VAL-159).
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
    public async Task<ReferredAdminReplacementResult> ResolveReferredAdminReplacementAsync(
        BilInvoice invoice,
        RegPatientEncounter encounter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(encounter);

        // 1. Invoice RAJAL: cari tautan aktif dengan LinkedEncounterId = kunjungan invoice ini.
        if (string.Equals(invoice.ServiceType, AdministrationFeeServiceTypes.Rajal, StringComparison.OrdinalIgnoreCase))
        {
            var activeRanapLinks = await (
                from link in _dbContext.BilInvoiceEncounterLinks.AsNoTracking()
                join ranapInv in _dbContext.BilInvoices.AsNoTracking() on link.RanapInvoiceId equals ranapInv.Id
                where !link.IsDelete
                    && link.LinkedEncounterId == invoice.EncounterId
                    && !ranapInv.IsDelete
                    && !ranapInv.IsCancel
                    && ranapInv.ServiceType == AdministrationFeeServiceTypes.Ranap
                orderby ranapInv.CreateDateTime ascending
                select new { RanapInvoiceId = ranapInv.Id }
            ).ToListAsync(cancellationToken);

            if (activeRanapLinks.Count > 0)
            {
                return new ReferredAdminReplacementResult
                {
                    IsSupersededByInpatientAdmission = true,
                    SupersededByInpatientInvoiceId = activeRanapLinks[0].RanapInvoiceId,
                    SupersededReasonCode = DefaultVoidReasonSuperseded,
                    ReferredOutpatientAdminCreditedAmount = 0m,
                    ReferredOutpatientAdminUnappliedCreditAmount = 0m,
                    ReferredOutpatientAdminOrigins = []
                };
            }

            return new ReferredAdminReplacementResult();
        }

        // 2. Invoice RANAP: ambil tautan aktif dengan RanapInvoiceId = invoice ini.
        if (string.Equals(invoice.ServiceType, AdministrationFeeServiceTypes.Ranap, StringComparison.OrdinalIgnoreCase))
        {
            var activeOrigins = await (
                from link in _dbContext.BilInvoiceEncounterLinks.AsNoTracking()
                join rajalInv in _dbContext.BilInvoices.AsNoTracking() on link.LinkedEncounterId equals rajalInv.EncounterId
                where !link.IsDelete
                    && link.RanapInvoiceId == invoice.Id
                    && !rajalInv.IsDelete
                    && !rajalInv.IsCancel
                    && rajalInv.ServiceType == AdministrationFeeServiceTypes.Rajal
                orderby link.CreateDateTime ascending
                select rajalInv
            ).ToListAsync(cancellationToken);

            if (activeOrigins.Count == 0)
            {
                return new ReferredAdminReplacementResult();
            }

            var originResults = new List<ReferredAdminOriginResult>();
            decimal totalCreditAmount = 0m;

            foreach (var rajalInv in activeOrigins)
            {
                decimal adminPatientAmount = 0m;
                if (rajalInv.CurrentCalculationVersion > 0)
                {
                    var latestVersion = await _dbContext.BilCalculationVersions.AsNoTracking()
                        .Where(v => v.InvoiceId == rajalInv.Id && v.VersionNo == rajalInv.CurrentCalculationVersion && !v.IsDelete)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (latestVersion != null && !string.IsNullOrWhiteSpace(latestVersion.BreakdownSnapshot))
                    {
                        var snapshot = BillingCalculationService.DeserializeBreakdown(latestVersion.BreakdownSnapshot);
                        var adminFee = snapshot.AdministrationFee;
                        if (adminFee != null)
                        {
                            adminPatientAmount = Math.Max(0m, adminFee.AppliedAmount - adminFee.PrimaryAmount - adminFee.UnresolvedAmount - adminFee.NonBillableResidualAmount);
                        }
                    }
                }

                string treatment;
                decimal creditAmount = 0m;

                if (string.Equals(rajalInv.Status, BillingInvoiceStatuses.Open, StringComparison.OrdinalIgnoreCase))
                {
                    treatment = "SUPERSEDED";
                    creditAmount = 0m;
                }
                else if (string.Equals(rajalInv.Status, BillingInvoiceStatuses.Final, StringComparison.OrdinalIgnoreCase)
                      || string.Equals(rajalInv.Status, BillingInvoiceStatuses.Closed, StringComparison.OrdinalIgnoreCase))
                {
                    creditAmount = adminPatientAmount;
                    treatment = adminPatientAmount > 0 ? "CREDITED" : "NO_ADMIN";
                }
                else if (string.Equals(rajalInv.Status, BillingInvoiceStatuses.SettledByWriteOff, StringComparison.OrdinalIgnoreCase))
                {
                    var netAllocations = await _dbContext.BilPaymentAllocations.AsNoTracking()
                        .Where(a => a.TargetType == BillingAllocationTargetTypes.Invoice && a.TargetId == rajalInv.Id && !a.IsDelete)
                        .SumAsync(a => (decimal?)(a.ReversesAllocationId.HasValue ? -a.Amount : a.Amount), cancellationToken) ?? 0m;
                    var paidAmount = Math.Max(0m, netAllocations);
                    creditAmount = Math.Min(adminPatientAmount, paidAmount);
                    treatment = creditAmount > 0 ? "CREDITED" : "NO_ADMIN";
                }
                else
                {
                    treatment = "NO_ADMIN";
                    creditAmount = 0m;
                }

                totalCreditAmount += creditAmount;

                originResults.Add(new ReferredAdminOriginResult
                {
                    OriginInvoiceId = rajalInv.Id,
                    OriginInvoiceNumber = rajalInv.InvoiceNumber,
                    OriginEncounterId = rajalInv.EncounterId,
                    OriginInvoiceStatus = rajalInv.Status,
                    Treatment = treatment,
                    AdminPatientAmount = adminPatientAmount,
                    CreditAmount = creditAmount
                });
            }

            return new ReferredAdminReplacementResult
            {
                IsSupersededByInpatientAdmission = false,
                SupersededByInpatientInvoiceId = null,
                SupersededReasonCode = null,
                ReferredOutpatientAdminCreditedAmount = totalCreditAmount,
                ReferredOutpatientAdminUnappliedCreditAmount = 0m,
                ReferredOutpatientAdminOrigins = originResults
            };
        }

        // 3. Invoice IGD / OTC / dll: tidak dibaca; hasil kosong
        return new ReferredAdminReplacementResult();
    }

    private async Task<(decimal TotalPriorApplied, int MaxPriority)> GetPriorAppliedAdminFeeAmountAsync(
        AdminFeeCalculationRequest request,
        MstAdministrationFeePolicy currentPolicy,
        CancellationToken cancellationToken)
    {
        // BKC-DES-087 (BE-BKC-084): Untuk RANAP, biaya admin rawat jalan yang digantikan diproses lewat
        // tautan aktif (ResolveReferredAdminReplacementAsync). Pengurangan flat Rp 50.000 berbasis kedekatan tanggal
        // dihentikan (mengembalikan 0). Perilaku RAJAL / IGD / OTC tetap tidak berubah.
        if (string.Equals(request.ServiceType, AdministrationFeeServiceTypes.Ranap, StringComparison.OrdinalIgnoreCase))
        {
            return (0m, int.MinValue);
        }

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

