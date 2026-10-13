using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class EmployeeBenefitSplitCalculationResult
{
    public bool IsEmployeeBenefitVisit { get; set; }
    public Guid? CompanyGuarantorId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? FamilyMemberId { get; set; }
    public string? RelationshipCode { get; set; }
    public string? ConsentGivenBy { get; set; }
    public string? BenefitPersonKind { get; set; } // ENROLLMENT / DEPENDENT (BE-FIN-108)
    public Guid? BenefitPersonId { get; set; } // Id enrollment atau dependent (BE-FIN-108)
    public Guid? PayerHandoffKey { get; set; } // HandoffKey baris PAYER (BE-FIN-108)
    public decimal PayerAmount { get; set; } // P = min(C, S)
    public decimal ExcessAmount { get; set; } // E = C - P
    public decimal ChoiceAmount { get; set; } // D
    public decimal EmployeeReceivableAmount { get; set; } // E + D
    public decimal PatientRemainingAmount { get; set; } // K - D
    public EmployeeBenefitSplitResponse Response { get; set; } = null!;
}

/// <summary>
/// Layanan kalkulasi pembagian porsi tagihan untuk kunjungan berpenjamin internal
/// (RS Benefit / Employee Benefit) saat proses finalisasi invoice (BE-FIN-107, BE-FIN-108;
/// FIN-DEC-164, 183, 184, 185, 186, 198, 202, 205, 208, 210, 218;
/// FIN-INTEGRATION-1.9 R.3, R.4; FIN-API-1.10 R.5; FIN-VAL-262, 264, 265, 289).
/// </summary>
public sealed class BillingEmployeeBenefitSplitService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing.EmployeeBenefitSplit";
    private readonly ApplicationDbContext _dbContext;
    private readonly EmployeeBenefitBalanceService _balanceService;
    private readonly LoggerService _loggerService;

    public EmployeeBenefitBalanceService BalanceService => _balanceService;

    public BillingEmployeeBenefitSplitService(
        ApplicationDbContext dbContext,
        EmployeeBenefitBalanceService balanceService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _balanceService = balanceService;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Mengevaluasi pembagian porsi benefit karyawan untuk pratinjau atau eksekusi finalisasi.
    /// Mengembalikan null bila invoice bukan kunjungan berpenjamin internal.
    /// </summary>
    public async Task<EmployeeBenefitSplitCalculationResult?> EvaluateSplitAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        decimal patientOutstanding,
        decimal choiceAmount,
        string? consentGivenBy,
        bool isFinalizing,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(calculation);

        // Cari penjamin aktif pada encounter
        var guarantors = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
            .Include(x => x.CompanyGuarantor)
            .Where(x => x.EncounterId == invoice.EncounterId && x.IsActive && !x.IsDelete && !x.IsCancel)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Priority)
            .ToListAsync(cancellationToken);

        var benefitGuarantor = guarantors.FirstOrDefault(x =>
            (x.CompanyGuarantor != null && x.CompanyGuarantor.IsEmployeeBenefitGuarantor)
            || x.EmployeeIdSnapshot.HasValue);

        if (benefitGuarantor == null)
        {
            // Bukan kunjungan berpenjamin internal
            return null;
        }

        // Penjaga R.4.4 / AC 7: Ada penjamin kedua (ExcessAmount > 0 atau guarantors.Count > 1) ditolak
        if (guarantors.Count > 1 || calculation.ExcessAmount > 0)
        {
            if (isFinalizing)
            {
                throw new BillingFinalizationValidationException(
                    "Penjamin kedua pada kunjungan berpenjamin internal belum didukung.");
            }

            return new EmployeeBenefitSplitCalculationResult
            {
                IsEmployeeBenefitVisit = true,
                Response = new EmployeeBenefitSplitResponse
                {
                    IsEmployeeBenefitVisit = true,
                    IsUsable = false,
                    BlockingReasonCode = "MULTIPLE_GUARANTORS_NOT_SUPPORTED",
                    BlockingMessage = "Penjamin kedua pada kunjungan berpenjamin internal belum didukung."
                }
            };
        }

        // Penjaga FIN-DEC-230: Kartu internal harus ditautkan ke data HR
        if (!benefitGuarantor.EmployeeIdSnapshot.HasValue)
        {
            if (isFinalizing)
            {
                throw new BillingFinalizationValidationException(
                    "Kartu penjamin internal belum ditautkan dengan data pegawai HR.");
            }

            return new EmployeeBenefitSplitCalculationResult
            {
                IsEmployeeBenefitVisit = true,
                Response = new EmployeeBenefitSplitResponse
                {
                    IsEmployeeBenefitVisit = true,
                    IsUsable = false,
                    BlockingReasonCode = "CARD_NOT_LINKED",
                    BlockingMessage = "Kartu penjamin internal belum ditautkan dengan data pegawai HR."
                }
            };
        }

        var employee = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => x.Id == benefitGuarantor.EmployeeIdSnapshot.Value)
            .Select(x => new { x.FullName, x.EmployeeNumber })
            .FirstOrDefaultAsync(cancellationToken);

        var employeeName = employee?.FullName ?? benefitGuarantor.EmployeeNameSnapshot;
        var relationshipCode = benefitGuarantor.BenefitRelationshipSnapshot ?? "SELF";

        // Baca saldo plafon otoritatif dari HR (R.3.1 / INT-BIL-HR-001)
        var asOfDate = invoice.InvoiceDate ?? DateTimeOffset.UtcNow;
        BenefitBalanceReadResult balanceResult;
        try
        {
            balanceResult = await _balanceService.ReadAsync(new BenefitBalanceReadRequest
            {
                EmployeeId = benefitGuarantor.EmployeeIdSnapshot.Value,
                FamilyMemberId = benefitGuarantor.FamilyMemberIdSnapshot,
                AsOfDate = asOfDate
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            await _loggerService.ErrorAsync(LogCategory, "Gagal membaca saldo benefit HR.", ex);
            if (isFinalizing)
            {
                throw new BillingFinalizationValidationException(
                    "Layanan HR tidak dapat diakses untuk membaca plafon benefit.");
            }

            return new EmployeeBenefitSplitCalculationResult
            {
                IsEmployeeBenefitVisit = true,
                Response = new EmployeeBenefitSplitResponse
                {
                    IsEmployeeBenefitVisit = true,
                    BenefitOwnerName = employeeName,
                    RelationshipCode = relationshipCode,
                    IsUsable = false,
                    BlockingReasonCode = "HR_UNAVAILABLE",
                    BlockingMessage = "Layanan HR tidak dapat diakses untuk membaca plafon benefit."
                }
            };
        }

        if (!balanceResult.IsUsable)
        {
            if (isFinalizing)
            {
                throw new BillingFinalizationValidationException(
                    balanceResult.Message ?? "Plafon benefit pegawai pada HR tidak dapat dipastikan atau tidak aktif.");
            }

            return new EmployeeBenefitSplitCalculationResult
            {
                IsEmployeeBenefitVisit = true,
                BenefitPersonKind = balanceResult.BenefitPersonKind,
                BenefitPersonId = balanceResult.BenefitPersonId,
                Response = new EmployeeBenefitSplitResponse
                {
                    IsEmployeeBenefitVisit = true,
                    BenefitOwnerName = employeeName,
                    RelationshipCode = relationshipCode,
                    IsUsable = false,
                    BlockingReasonCode = balanceResult.ReasonCode ?? "BENEFIT_NOT_USABLE",
                    BlockingMessage = balanceResult.Message ?? "Plafon benefit pegawai pada HR tidak dapat dipastikan atau tidak aktif."
                }
            };
        }

        // Hitung pembagian porsi sesuai rumus R.4.2:
        // C = porsi yang ditanggung penjamin RS Benefit (PrimaryAmount)
        // S = sisa plafon hasil BacaPlafon
        // P = min(C, S) (Porsi RS Benefit)
        // E = C - P (Kelebihan di atas plafon)
        // K = sisa tagihan pasien di luar C (outstanding)
        // D = pilihan pegawai (0 <= D <= K)
        var coveredByRuleAmount = calculation.PrimaryAmount; // C
        var remainingAllowanceAmount = balanceResult.RemainingAmount; // S
        var rsBenefitAmount = Math.Min(coveredByRuleAmount, remainingAllowanceAmount); // P
        var excessOverAllowanceAmount = coveredByRuleAmount - rsBenefitAmount; // E
        var patientPortionAmount = patientOutstanding; // K

        var choiceD = choiceAmount;

        // Validasi batasan pilihan D (AC 5, FIN-VAL-262)
        if (choiceD < 0)
        {
            throw new BillingFinalizationValidationException(
                "Nominal pilihan utang pegawai tidak boleh negatif.");
        }

        if (choiceD > patientPortionAmount)
        {
            throw new BillingFinalizationValidationException(
                $"Nominal pilihan utang pegawai (Rp {choiceD:N0}) tidak boleh melebihi sisa tagihan pasien (Rp {patientPortionAmount:N0}).");
        }

        // Validasi persetujuan (FIN-DEC-218, S.4, FIN-VAL-289)
        var isSelf = string.Equals(relationshipCode, "SELF", StringComparison.OrdinalIgnoreCase);
        var requiresConsent = !isSelf && choiceD > 0;
        if (isFinalizing && requiresConsent && string.IsNullOrWhiteSpace(consentGivenBy))
        {
            throw new BillingFinalizationValidationException(
                "Nama pemberi persetujuan wajib diisi bila sisa tagihan pasien selain pegawai dijadikan utang pegawai.");
        }

        var employeeReceivableAmount = excessOverAllowanceAmount + choiceD; // E + D
        var cashAtCashierAmount = patientPortionAmount - choiceD; // K - D

        var response = new EmployeeBenefitSplitResponse
        {
            IsEmployeeBenefitVisit = true,
            BenefitOwnerName = employeeName,
            RelationshipCode = relationshipCode,
            CoveredByRuleAmount = coveredByRuleAmount,
            RemainingAllowanceAmount = remainingAllowanceAmount,
            RsBenefitAmount = rsBenefitAmount,
            ExcessOverAllowanceAmount = excessOverAllowanceAmount,
            PatientPortionAmount = patientPortionAmount,
            ChoiceAmount = choiceD,
            EmployeeReceivableAmount = employeeReceivableAmount,
            CashAtCashierAmount = cashAtCashierAmount,
            IsUsable = true,
            RequiresConsent = requiresConsent,
            BlockingReasonCode = null,
            BlockingMessage = null
        };

        return new EmployeeBenefitSplitCalculationResult
        {
            IsEmployeeBenefitVisit = true,
            CompanyGuarantorId = benefitGuarantor.CompanyGuarantorId,
            EmployeeId = benefitGuarantor.EmployeeIdSnapshot.Value,
            FamilyMemberId = benefitGuarantor.FamilyMemberIdSnapshot,
            RelationshipCode = relationshipCode,
            ConsentGivenBy = consentGivenBy?.Trim(),
            BenefitPersonKind = balanceResult.BenefitPersonKind,
            BenefitPersonId = balanceResult.BenefitPersonId,
            PayerAmount = rsBenefitAmount,
            ExcessAmount = excessOverAllowanceAmount,
            ChoiceAmount = choiceD,
            EmployeeReceivableAmount = employeeReceivableAmount,
            PatientRemainingAmount = cashAtCashierAmount,
            Response = response
        };
    }
}
