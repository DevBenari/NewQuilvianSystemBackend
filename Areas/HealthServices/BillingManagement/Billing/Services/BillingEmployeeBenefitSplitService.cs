using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
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
    /// <summary>MstEmployee.Id (snapshot lama); kosong untuk dokter tetap tanpa data karyawan.</summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>Identitas canonical pemilik benefit (WorkforceProfileId). Dipakai handoff Billing dan Finance.</summary>
    public Guid? WorkforceProfileId { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerName { get; set; }
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
    public decimal ExcessPaidAmount { get; set; } // bagian E yang sudah dibayar langsung (BENEFIT_EXCESS)
    public decimal ExcessRemainingAmount { get; set; } // E sisa = E - ExcessPaidAmount
    /// <summary>OVER_ALLOWANCE / PATIENT_PORTION / MIXED (lihat BillingEmployeeArOrigins); kosong bila D = 0.</summary>
    public string? EmployeeArOrigin { get; set; }
    public decimal ChoiceAmount { get; set; } // D (backward-compatible)
    public decimal RemainingPatientObligation { get; set; } // E sisa + K
    public decimal EmployeeDeferredAmount { get; set; } // D
    public decimal DirectPayAmount { get; set; } // (E + K) - D
    public decimal EmployeeReceivableAmount { get; set; } // D (hanya nominal yang ditangguhkan)
    public decimal PatientRemainingAmount { get; set; } // Sisa yang wajib dibayar di kasir
    public EmployeeBenefitSplitResponse Response { get; set; } = null!;
}

/// <summary>
/// Kebijakan penawaran "AR Karyawan / Utang Pegawai" kepada kasir di Menu Pembayaran. Satu tempat, supaya layar tidak
/// menebak batasnya sendiri.
///
/// KEPUTUSAN SAAT INI (konservatif): yang ditawarkan hanya kelebihan plafon yang belum dibayar (E sisa, asal piutang
/// OVER_ALLOWANCE). Kewajiban pribadi dari item non-covered (K, asal PATIENT_PORTION atau MIXED) TIDAK ditawarkan
/// lewat Menu Pembayaran, sehingga seluruh K tidak pernah masuk AR secara otomatis.
///
/// NEEDS BUSINESS DECISION (pemilik Billing/Finance/HR): apakah K boleh ditangguhkan menjadi utang pegawai, sampai
/// berapa, dan dengan persetujuan siapa. Validasi finalisasi sendiri (0 &lt;= D &lt;= E sisa + K, persetujuan wajib untuk
/// hubungan selain SELF) tidak diubah, jadi layar Detail Billing yang sudah ada tetap dapat memakainya.
/// </summary>
public static class BillingEmployeeArOfferPolicy
{
    public static decimal ResolveMaxOfferedAmount(decimal excessRemainingAmount) =>
        Math.Max(excessRemainingAmount, 0m);
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
    private readonly WorkforceBenefitOwnerResolver _ownerResolver;

    public EmployeeBenefitBalanceService BalanceService => _balanceService;

    public BillingEmployeeBenefitSplitService(
        ApplicationDbContext dbContext,
        EmployeeBenefitBalanceService balanceService,
        LoggerService loggerService,
        WorkforceBenefitOwnerResolver? ownerResolver = null)
    {
        _dbContext = dbContext;
        _balanceService = balanceService;
        _loggerService = loggerService;
        _ownerResolver = ownerResolver ?? new WorkforceBenefitOwnerResolver(dbContext);
    }

    /// <summary>
    /// Total kelebihan plafon (E) yang sudah dibayar langsung pada invoice: alokasi BENEFIT_EXCESS bersih setelah reversal.
    /// Target ini sengaja terpisah dari INVOICE agar tidak ikut dibaca sebagai pembayaran sisa tagihan K.
    /// </summary>
    public async Task<decimal> GetPaidBenefitExcessAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var net = await _dbContext.BilPaymentAllocations.AsNoTracking()
            .Where(x => x.TargetType == BillingAllocationTargetTypes.BenefitExcess
                && x.TargetId == invoiceId && !x.IsDelete)
            .SumAsync(
                x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount),
                cancellationToken) ?? 0m;
        return Math.Max(net, 0m);
    }

    /// <summary>
    /// Sisa kelebihan plafon (E sisa) yang boleh dibayar langsung lewat settlement baru, dibaca dari HR saat ini.
    /// Nol (gagal tertutup) untuk kunjungan non-benefit, pilihan GENERAL_SELF_PAY, atau bila benefit tidak dapat dipakai
    /// (HR tidak terbaca, plafon tidak aktif, dst.). Tidak pernah melempar ke pemanggil: kegagalan berarti "tidak ada E".
    /// </summary>
    public async Task<decimal> GetOpenBenefitExcessAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        decimal patientOutstanding,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await EvaluateSplitAsync(
                invoice,
                calculation,
                patientOutstanding,
                choiceAmount: 0m,
                consentGivenBy: null,
                isFinalizing: false,
                cancellationToken);
            return result is { Response.IsUsable: true }
                ? Math.Max(result.Response.ExcessRemainingAmount, 0m)
                : 0m;
        }
        catch (BillingFinalizationValidationException)
        {
            return 0m;
        }
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
            || x.EmployeeIdSnapshot.HasValue
            || x.WorkforceProfileIdSnapshot.HasValue);

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

        // Penjaga FIN-DEC-230: Kartu internal harus ditautkan ke data HR (pemilik benefit: WorkforceProfileId pada
        // snapshot baru, atau EmployeeId pada snapshot lama; dokter tetap hanya memiliki yang pertama).
        if (!benefitGuarantor.EmployeeIdSnapshot.HasValue && !benefitGuarantor.WorkforceProfileIdSnapshot.HasValue)
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

        // Pemilik dibaca lewat WorkforceProfileId; transaksi lama (hanya EmployeeIdSnapshot) diterjemahkan ke Workforce
        // tanpa mengubah snapshot histori. Pemilik tidak aktif tetap dikenali untuk nama, dan alasan blokirnya
        // diberikan pembacaan plafon di bawah.
        var ownerProfileId = benefitGuarantor.WorkforceProfileIdSnapshot
            ?? (benefitGuarantor.EmployeeIdSnapshot.HasValue
                ? await _dbContext.MstEmployees.AsNoTracking()
                    .Where(x => x.Id == benefitGuarantor.EmployeeIdSnapshot.Value)
                    .Select(x => (Guid?)x.WorkforceProfileId)
                    .FirstOrDefaultAsync(cancellationToken)
                : null);
        WorkforceBenefitOwner? owner = null;
        if (ownerProfileId.HasValue)
        {
            var owners = await _ownerResolver.ResolveManyAsync(new[] { ownerProfileId.Value }, cancellationToken);
            owners.TryGetValue(ownerProfileId.Value, out owner);
        }

        var employeeName = owner?.StaffName ?? benefitGuarantor.EmployeeNameSnapshot;
        var relationshipCode = benefitGuarantor.BenefitRelationshipSnapshot ?? "SELF";

        // Baca saldo plafon otoritatif dari HR (R.3.1 / INT-BIL-HR-001)
        var asOfDate = (invoice.InvoiceDate ?? DateTimeOffset.UtcNow).UtcDateTime;
        BenefitBalanceReadResult balanceResult;
        try
        {
            balanceResult = await _balanceService.ReadAsync(new BenefitBalanceReadRequest
            {
                WorkforceProfileId = ownerProfileId,
                EmployeeId = benefitGuarantor.EmployeeIdSnapshot ?? Guid.Empty,
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
                    BenefitOwnerWorkforceProfileId = ownerProfileId,
                    BenefitOwnerType = owner?.OwnerType,
                    BenefitOwnerStaffNumber = owner?.StaffNumber,
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
                    BenefitOwnerWorkforceProfileId = ownerProfileId,
                    BenefitOwnerType = owner?.OwnerType,
                    BenefitOwnerStaffNumber = owner?.StaffNumber,
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

        // E yang sudah dibayar langsung lewat tender (alokasi BENEFIT_EXCESS). E dihitung ulang dari HR setiap kali;
        // bila plafon berubah sehingga E baru lebih kecil dari yang sudah dibayar, jangan memutar uang diam-diam:
        // blokir finalisasi sampai selisihnya dikoreksi/direfund lewat jalur yang sah.
        var excessPaidAmount = await GetPaidBenefitExcessAsync(invoice.Id, cancellationToken);
        if (excessPaidAmount > excessOverAllowanceAmount)
        {
            var overpaidMessage =
                $"Pembayaran kelebihan plafon (Rp {excessPaidAmount:N0}) melebihi kelebihan plafon yang dihitung ulang dari HR " +
                $"(Rp {excessOverAllowanceAmount:N0}); plafon benefit berubah setelah pembayaran. Koreksi atau refund selisihnya sebelum finalisasi.";
            if (isFinalizing)
                throw new BillingFinalizationValidationException(overpaidMessage);

            return new EmployeeBenefitSplitCalculationResult
            {
                IsEmployeeBenefitVisit = true,
                BenefitPersonKind = balanceResult.BenefitPersonKind,
                BenefitPersonId = balanceResult.BenefitPersonId,
                Response = new EmployeeBenefitSplitResponse
                {
                    IsEmployeeBenefitVisit = true,
                    BenefitOwnerName = employeeName,
                    BenefitOwnerWorkforceProfileId = ownerProfileId,
                    BenefitOwnerType = owner?.OwnerType,
                    BenefitOwnerStaffNumber = owner?.StaffNumber,
                    RelationshipCode = relationshipCode,
                    ExcessOverAllowanceAmount = excessOverAllowanceAmount,
                    ExcessPaidAmount = excessPaidAmount,
                    IsUsable = false,
                    BlockingReasonCode = "BENEFIT_EXCESS_OVERPAID",
                    BlockingMessage = overpaidMessage
                }
            };
        }

        var excessRemainingAmount = excessOverAllowanceAmount - excessPaidAmount; // E sisa
        var totalRemainingPatientObligation = excessRemainingAmount + patientPortionAmount; // E sisa + K

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

        // Asal piutang karyawan: D seluruhnya berasal dari E sisa (OVER_ALLOWANCE), seluruhnya dari sisa tagihan pasien
        // (PATIENT_PORTION), atau campuran. Hanya diisi bila ada yang ditangguhkan.
        string? employeeArOrigin = null;
        if (deferredD > 0)
        {
            employeeArOrigin = excessRemainingAmount <= 0
                ? BillingEmployeeArOrigins.PatientPortion
                : deferredD <= excessRemainingAmount
                    ? BillingEmployeeArOrigins.OverAllowance
                    : BillingEmployeeArOrigins.Mixed;
        }

        var response = new EmployeeBenefitSplitResponse
        {
            IsEmployeeBenefitVisit = true,
            FinancingChoice = "EMPLOYEE_BENEFIT",
            BenefitOwnerName = employeeName,
            BenefitOwnerWorkforceProfileId = balanceResult.WorkforceProfileId ?? ownerProfileId,
            BenefitOwnerType = balanceResult.OwnerType ?? owner?.OwnerType,
            BenefitOwnerStaffNumber = owner?.StaffNumber,
            RelationshipCode = relationshipCode,
            GrossAmount = calculation.GrossAmount,
            CoveredByRuleAmount = coveredByRuleAmount,
            RemainingAllowanceAmount = remainingAllowanceAmount,
            RsBenefitAmount = rsBenefitAmount,
            ExcessOverAllowanceAmount = excessOverAllowanceAmount,
            ExcessPaidAmount = excessPaidAmount,
            ExcessRemainingAmount = excessRemainingAmount,
            PatientPortionAmount = patientPortionAmount,
            RemainingPatientObligation = totalRemainingPatientObligation,
            DirectPayAmount = directPayAmount,
            EmployeeDeferredAmount = deferredD,
            ChoiceAmount = deferredD,
            EmployeeReceivableAmount = employeeReceivableAmount,
            CashAtCashierAmount = cashAtCashierAmount,
            EmployeeArMaxAmount = BillingEmployeeArOfferPolicy.ResolveMaxOfferedAmount(excessRemainingAmount),
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
            EmployeeId = benefitGuarantor.EmployeeIdSnapshot,
            WorkforceProfileId = balanceResult.WorkforceProfileId ?? ownerProfileId,
            OwnerType = balanceResult.OwnerType ?? owner?.OwnerType,
            OwnerName = employeeName,
            FamilyMemberId = benefitGuarantor.FamilyMemberIdSnapshot,
            RelationshipCode = relationshipCode,
            ConsentGivenBy = consentGivenBy?.Trim(),
            ConsentNotes = consentNotes?.Trim(),
            BenefitPersonKind = balanceResult.BenefitPersonKind,
            BenefitPersonId = balanceResult.BenefitPersonId,
            GrossAmount = calculation.GrossAmount,
            PayerAmount = rsBenefitAmount,
            ExcessAmount = excessOverAllowanceAmount,
            ExcessPaidAmount = excessPaidAmount,
            ExcessRemainingAmount = excessRemainingAmount,
            EmployeeArOrigin = employeeArOrigin,
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
