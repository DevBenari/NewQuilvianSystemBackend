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
    public string FinancingChoice { get; set; } = "EMPLOYEE_BENEFIT";
    public Guid? CompanyGuarantorId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? FamilyMemberId { get; set; }
    public string? RelationshipCode { get; set; }
    public string? ConsentGivenBy { get; set; }
    public string? ConsentNotes { get; set; }
    public string? BenefitPersonKind { get; set; } // ENROLLMENT / DEPENDENT (BE-FIN-108)
    public Guid? BenefitPersonId { get; set; } // Id enrollment atau dependent (BE-FIN-108)
    public Guid? PayerHandoffKey { get; set; } // HandoffKey baris PAYER (BE-FIN-108)
    public decimal GrossAmount { get; set; }
    public decimal PayerAmount { get; set; } // P = min(C, S)
    public decimal ExcessAmount { get; set; } // E = C - P
    public decimal ChoiceAmount { get; set; } // D (backward-compatible)
    public decimal RemainingPatientObligation { get; set; } // E + K
    public decimal EmployeeDeferredAmount { get; set; } // D
    public decimal DirectPayAmount { get; set; } // (E + K) - D
    public decimal EmployeeReceivableAmount { get; set; } // D (hanya nominal yang ditangguhkan)
    public decimal PatientRemainingAmount { get; set; } // Sisa yang wajib dibayar di kasir
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
        CancellationToken cancellationToken,
        string? financingChoice = null,
        decimal? employeeDeferredAmount = null,
        string? consentNotes = null)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(calculation);

        // Section 5: Bila pasien eksplisit memilih bayar umum/sendiri, jangan gunakan benefit
        if (string.Equals(financingChoice, "GENERAL_SELF_PAY", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

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

        // Jika saat registrasi sudah memilih GENERAL_SELF_PAY dan tidak ada override pembiayaan
        if (financingChoice == null && string.Equals(benefitGuarantor.FinancingChoice, "GENERAL_SELF_PAY", StringComparison.OrdinalIgnoreCase))
        {
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
        var asOfDate = (invoice.InvoiceDate ?? DateTimeOffset.UtcNow).UtcDateTime;
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
            await _loggerService.ErrorAsync(LogCategory, "ReadBalance", "Gagal membaca saldo benefit HR.", ex);
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

        // Hitung pembagian porsi sesuai rumus terpadu (Section 6):
        // GrossAmount = total biaya tagihan (calculation.GrossAmount)
        // C = porsi yang ditanggung penjamin RS Benefit (PrimaryAmount)
        // S = sisa plafon hasil BacaPlafon (remainingAllowanceAmount)
        // P = min(C, S) (Porsi RS Benefit yang sah ditanggung)
        // E = C - P (Kelebihan di atas plafon)
        // K = sisa kewajiban pasien sebelum penangguhan (patientOutstanding)
        // Total Kewajiban Pasien yang harus diselesaikan = E + K
        // D = pilihan nominal utang pegawai yang ditangguhkan (0 <= D <= E + K)
        // DirectPay = (E + K) - D
        var coveredByRuleAmount = calculation.PrimaryAmount; // C
        var remainingAllowanceAmount = balanceResult.RemainingAmount; // S
        var rsBenefitAmount = Math.Min(coveredByRuleAmount, remainingAllowanceAmount); // P
        var excessOverAllowanceAmount = coveredByRuleAmount - rsBenefitAmount; // E
        var patientPortionAmount = patientOutstanding; // K
        var totalRemainingPatientObligation = excessOverAllowanceAmount + patientPortionAmount; // E + K

        var deferredD = employeeDeferredAmount ?? choiceAmount;

        // Validasi batasan pilihan D (Section 6, AC 5, FIN-VAL-262)
        if (deferredD < 0)
        {
            throw new BillingFinalizationValidationException(
                "Nominal pilihan utang pegawai tidak boleh negatif.");
        }

        if (deferredD > totalRemainingPatientObligation)
        {
            throw new BillingFinalizationValidationException(
                $"Nominal pilihan utang pegawai (Rp {deferredD:N0}) tidak boleh melebihi sisa kewajiban pasien (Rp {totalRemainingPatientObligation:N0}).");
        }

        // Validasi persetujuan (FIN-DEC-218, S.4, FIN-VAL-289, Section 7)
        var isSelf = string.Equals(relationshipCode, "SELF", StringComparison.OrdinalIgnoreCase);
        var requiresConsent = !isSelf && deferredD > 0;
        if (isFinalizing && requiresConsent && string.IsNullOrWhiteSpace(consentGivenBy))
        {
            throw new BillingFinalizationValidationException(
                "Nama pemberi persetujuan wajib diisi bila sisa tagihan pasien selain pegawai dijadikan utang pegawai.");
        }

        var directPayAmount = totalRemainingPatientObligation - deferredD;
        var employeeReceivableAmount = deferredD;
        var cashAtCashierAmount = directPayAmount;

        var response = new EmployeeBenefitSplitResponse
        {
            IsEmployeeBenefitVisit = true,
            FinancingChoice = "EMPLOYEE_BENEFIT",
            BenefitOwnerName = employeeName,
            RelationshipCode = relationshipCode,
            GrossAmount = calculation.GrossAmount,
            CoveredByRuleAmount = coveredByRuleAmount,
            RemainingAllowanceAmount = remainingAllowanceAmount,
            RsBenefitAmount = rsBenefitAmount,
            ExcessOverAllowanceAmount = excessOverAllowanceAmount,
            PatientPortionAmount = patientPortionAmount,
            RemainingPatientObligation = totalRemainingPatientObligation,
            DirectPayAmount = directPayAmount,
            EmployeeDeferredAmount = deferredD,
            ChoiceAmount = deferredD,
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
            FinancingChoice = "EMPLOYEE_BENEFIT",
            CompanyGuarantorId = benefitGuarantor.CompanyGuarantorId,
            EmployeeId = benefitGuarantor.EmployeeIdSnapshot.Value,
            FamilyMemberId = benefitGuarantor.FamilyMemberIdSnapshot,
            RelationshipCode = relationshipCode,
            ConsentGivenBy = consentGivenBy?.Trim(),
            ConsentNotes = consentNotes?.Trim(),
            BenefitPersonKind = balanceResult.BenefitPersonKind,
            BenefitPersonId = balanceResult.BenefitPersonId,
            GrossAmount = calculation.GrossAmount,
            PayerAmount = rsBenefitAmount,
            ExcessAmount = excessOverAllowanceAmount,
            RemainingPatientObligation = totalRemainingPatientObligation,
            EmployeeDeferredAmount = deferredD,
            DirectPayAmount = directPayAmount,
            ChoiceAmount = deferredD,
            EmployeeReceivableAmount = employeeReceivableAmount,
            PatientRemainingAmount = cashAtCashierAmount,
            Response = response
        };
    }
}
