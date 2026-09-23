using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public interface IBillingCoverageAdapter
{
    Task<BillingCoverageDecision> ResolveAsync(BillingCoverageContext context, CancellationToken cancellationToken);
}

// BE-BKC-046/MPY-DES-005/CAP-34: Konteks payer kandidat untuk pratinjau evaluasi coverage tanpa menyentuh penjamin kunjungan.
public sealed record CandidatePayerContext(
    EncounterPaymentType PaymentType,
    Guid? PaymentMethodId = null,
    Guid? PatientInsuranceId = null,
    Guid? PatientCompanyGuarantorId = null);

public sealed record BillingCoverageContext(
    Guid InvoiceId,
    Guid EncounterId,
    DateTimeOffset CalculatedAt,
    decimal EligibleAmount,
    IReadOnlyList<BillingCoverageComponent> Components,
    CandidatePayerContext? CandidatePayer = null)
{
    // Konstruktor kompatibilitas penuh untuk pemanggil existing
    public BillingCoverageContext(
        Guid invoiceId,
        Guid encounterId,
        DateTimeOffset calculatedAt,
        decimal eligibleAmount,
        IReadOnlyList<BillingCoverageComponent> components)
        : this(invoiceId, encounterId, calculatedAt, eligibleAmount, components, null)
    {
    }
}

// TariffId/ProcedureId/DrugId/DrugCategoryId/TariffCategoryId: field terpisah per dimensi rujukan
// rule asuransi (MstInsuranceCoverageRule) - BUKAN satu SourceReferenceId gabungan seperti
// sebelumnya, karena satu item bisa dicocokkan rule di granularitas manapun (tarif spesifik,
// prosedur, kategori obat, atau kategori tarif) sekaligus, dan satu Guid tidak bisa mewakili
// keempatnya. Null untuk komponen yang bukan berasal dari item (ADMINISTRATION_FEE/ROOM_CHARGE/
// TAX non-item) - komponen itu memang tidak punya rujukan tarif/prosedur/obat.
public sealed record BillingCoverageComponent(
    Guid ComponentId,
    string ComponentType,
    string CoverageItemType,
    Guid? TariffId,
    Guid? ProcedureId,
    Guid? DrugId,
    Guid? DrugCategoryId,
    Guid? TariffCategoryId,
    decimal Quantity,
    decimal Amount,
    bool Coverable);

public sealed record BillingCoverageDecision(
    string ContractVersion,
    string PrimaryStatus,
    string ExcessStatus,
    decimal PrimaryAmount,
    decimal ExcessAmount,
    decimal UnresolvedAmount,
    IReadOnlyList<Guid> AppliedRuleIds,
    IReadOnlyList<BillingCoverageComponentOutcome> ComponentOutcomes,
    // BE-BKC-025/BKC-DES-010: total rupiah yang tidak dapat dinilai penjaminnya karena data
    // pendaftaran bermasalah (penjamin belum eligible, polis tidak aktif, perusahaan asuransi
    // belum dipilih, atau encounter tidak ditemukan) - BUKAN bucket uang ketiga, melainkan
    // penanda DI ATAS pembagian primary/pasien yang sudah ada (BKC-DES-011). Nominalnya SUDAH
    // ikut masuk porsi pasien lewat identitas turunan yang sama seperti sebelumnya.
    decimal DataAnomalyAmount,
    // BE-BKC-028/BKC-DES-021/022: total rupiah selisih yang menurut kontrak penjamin (rule
    // IsAllowExcessPaymentByPatient = false) TIDAK BOLEH ditagihkan ke pasien - berbeda dari
    // DataAnomalyAmount (masalah data pendaftaran, jatuh ke pasien) dan berbeda dari
    // UnresolvedAmount (jalur NotCovered, menunggu keputusan pemilik). Nominal ini TIDAK jatuh ke
    // pasien maupun ke penjamin - ia menunggu Finance mengajukan write-off kategori
    // NON_BILLABLE_RESIDUAL (BKC-DEC-080). Satu-satunya titik pengisi: cabang residual jalur (5)
    // di ResolveAsync ketika rule.IsAllowExcessPaymentByPatient == false.
    decimal NonBillableResidualAmount,
    IReadOnlyList<BillingCoverageAnomaly> Anomalies,
    // BE-BKC-044/MPY-DES-017: penanda jenis payer pada breakdown tagihan ("CASH", "INSURANCE", "COMPANY_GUARANTOR").
    string PayerKind = "CASH");

// BE-BKC-025/BKC-DES-010: satu masalah data pendaftaran yang membuat penilaian penjamin tidak
// dapat dilakukan dengan benar. Code MUST NOT diterjemahkan (kunci program); Message MUST berupa
// kalimat siap dibaca kasir, bukan nama kolom. Daftar Code yang berlaku: PAYER_NOT_ELIGIBLE,
// POLICY_INACTIVE, INSURANCE_PROVIDER_MISSING, COMPANY_GUARANTOR_MISSING, ENCOUNTER_NOT_FOUND.
public sealed record BillingCoverageAnomaly(string Code, string Message);

// Bug fix (di luar roadmap, laporan pengguna): sebelumnya waterfall hanya mengembalikan TOTAL
// gabungan (PrimaryAmount/UnresolvedAmount di atas) - badge per item Menu Pembayaran dan split
// Subtotal/Pajak Mandiri-Asuransi terpaksa memakai flag "coverable" tingkat kategori (bukan hasil
// tiap item sesungguhnya) sebagai pendekatan. ComponentOutcomes membawa hasil PER KOMPONEN (item
// ATAU komponen pajaknya, sesuai ComponentId+ComponentType) - PatientAmount komponen itu TIDAK
// disimpan eksplisit di sini, cukup diturunkan pemanggil sebagai
// component.Amount - PrimaryAmount - UnresolvedAmount - DataAnomalyAmount (identitas ini selalu
// benar by construction). Komponen yang tidak muncul di daftar ini (mis. jalur SelfPay) dianggap
// seluruhnya Patient.
public sealed record BillingCoverageComponentOutcome(
    Guid ComponentId,
    string ComponentType,
    decimal PrimaryAmount,
    decimal UnresolvedAmount,
    // BE-BKC-025/BKC-DES-010: porsi komponen ini yang tidak dapat dinilai karena data pendaftaran
    // bermasalah. Hanya jalur (4) (Anomaly()) yang pernah mengisinya bukan nol.
    decimal DataAnomalyAmount,
    // BE-BKC-028/BKC-DES-021: porsi komponen ini yang tidak boleh ditagihkan ke pasien menurut
    // kontrak penjamin. Hanya jalur (5) residual dengan IsAllowExcessPaymentByPatient=false yang
    // pernah mengisinya bukan nol.
    decimal NonBillableResidualAmount);

public sealed class RegistrationBillingCoverageAdapter : IBillingCoverageAdapter
{
    public const string ContractVersion = "REGISTRATION-COVERAGE-ADAPTER-2";
    private readonly ApplicationDbContext _dbContext;
    private readonly CompanyGuarantorCoverageService _companyCoverageService;
    private readonly EncounterInsuranceService _encounterInsuranceService;

    public RegistrationBillingCoverageAdapter(
        ApplicationDbContext dbContext,
        CompanyGuarantorCoverageService companyCoverageService,
        EncounterInsuranceService encounterInsuranceService)
    {
        _dbContext = dbContext;
        _companyCoverageService = companyCoverageService;
        _encounterInsuranceService = encounterInsuranceService;
    }

    public async Task<BillingCoverageDecision> ResolveAsync(
        BillingCoverageContext context,
        CancellationToken cancellationToken)
    {
        // BE-BKC-046/MPY-DES-005/CAP-34: Jika CandidatePayer disediakan eksplisit,
        // evaluasi payer kandidat secara murni tanpa membaca maupun menyentuh penjamin
        // persistent pada kunjungan (100% read-only, AsNoTracking, zero-side-effect).
        if (context.CandidatePayer != null)
        {
            return await ResolveCandidatePayerAsync(context, context.CandidatePayer, cancellationToken);
        }

        var paymentSource = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EncounterId == context.EncounterId && x.IsActive && !x.IsDelete, cancellationToken);

        if (paymentSource is null || paymentSource.PaymentType == EncounterPaymentType.Cash)
            return SelfPay();

        // BE-BKC-044/MPY-DES-007: RegistrationBillingCoverageAdapter menjadi dispatcher per jenis payer.
        // Kunjungan CompanyGuarantor diserahkan ke CompanyGuarantorCoverageService dan TIDAK PERNAH
        // mengevaluasi InsuranceProviderId.HasValue (menghapus anomali palsu INSURANCE_PROVIDER_MISSING).
        return paymentSource.PaymentType switch
        {
            EncounterPaymentType.CompanyGuarantor => await ResolveCompanyGuarantorAsync(context, paymentSource, cancellationToken),
            EncounterPaymentType.Insurance => await ResolveInsuranceAsync(context, paymentSource, cancellationToken),
            _ => SelfPay()
        };
    }

    private async Task<BillingCoverageDecision> ResolveCandidatePayerAsync(
        BillingCoverageContext context,
        CandidatePayerContext candidate,
        CancellationToken cancellationToken)
    {
        return candidate.PaymentType switch
        {
            EncounterPaymentType.Cash => SelfPay(),
            EncounterPaymentType.Insurance => await ResolveCandidateInsuranceAsync(context, candidate, cancellationToken),
            EncounterPaymentType.CompanyGuarantor => await ResolveCandidateCompanyGuarantorAsync(context, candidate, cancellationToken),
            _ => SelfPay()
        };
    }

    private async Task<BillingCoverageDecision> ResolveCandidateInsuranceAsync(
        BillingCoverageContext context,
        CandidatePayerContext candidate,
        CancellationToken cancellationToken)
    {
        if (!candidate.PatientInsuranceId.HasValue)
            return Anomaly(context.Components, "INSURANCE_PROVIDER_MISSING",
                "Kartu asuransi pasien kandidat belum dipilih. Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "INSURANCE");

        var effectiveDate = context.CalculatedAt.UtcDateTime.Date;
        var insuranceContext = await _encounterInsuranceService.GetCandidateContextAsync(
            context.EncounterId,
            candidate.PatientInsuranceId.Value,
            effectiveDate,
            cancellationToken);

        if (!insuranceContext.IsValid)
        {
            var code = insuranceContext.ErrorMessage?.Contains("belum mulai berlaku", StringComparison.OrdinalIgnoreCase) == true
                || insuranceContext.ErrorMessage?.Contains("sudah berakhir", StringComparison.OrdinalIgnoreCase) == true
                || insuranceContext.ErrorMessage?.Contains("tidak aktif", StringComparison.OrdinalIgnoreCase) == true
                ? "POLICY_INACTIVE"
                : "PAYER_NOT_ELIGIBLE";

            return Anomaly(context.Components, code,
                insuranceContext.ErrorMessage ?? "Kartu asuransi kandidat tidak valid atau belum layak (eligible).",
                payerKind: "INSURANCE");
        }

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.EncounterId && !x.IsDelete, cancellationToken);
        if (encounter is null)
            return Anomaly(context.Components, "ENCOUNTER_NOT_FOUND",
                "Data kunjungan tidak ditemukan saat memeriksa penjamin. Hubungi tim teknis sebelum menagih.",
                payerKind: "INSURANCE");

        var providerId = insuranceContext.InsuranceProviderId!.Value;
        var benefitPlanCode = insuranceContext.BenefitPlanCode;
        var rules = await _dbContext.MstInsuranceCoverageRules.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.InsuranceProviderId == providerId
                && (x.BenefitPlanCode == null || x.BenefitPlanCode == benefitPlanCode)
                && (x.PatientClassId == null || x.PatientClassId == encounter.PatientClassId)
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= effectiveDate)
                && (x.EffectiveEndDate == null || effectiveDate <= x.EffectiveEndDate))
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.RuleCode)
            .ToListAsync(cancellationToken);

        return CalculateInsuranceCoverageDecision(context, rules);
    }

    private async Task<BillingCoverageDecision> ResolveCandidateCompanyGuarantorAsync(
        BillingCoverageContext context,
        CandidatePayerContext candidate,
        CancellationToken cancellationToken)
    {
        if (!candidate.PatientCompanyGuarantorId.HasValue)
            return Anomaly(context.Components, "COMPANY_GUARANTOR_MISSING",
                "Kartu penjamin perusahaan kandidat belum dipilih. Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "COMPANY_GUARANTOR");

        var candidateCard = await _dbContext.Set<MstPatientCompanyGuarantor>().AsNoTracking()
            .Include(x => x.CompanyGuarantor)
            .FirstOrDefaultAsync(x => x.Id == candidate.PatientCompanyGuarantorId.Value && !x.IsDelete, cancellationToken);

        if (candidateCard is null)
            return Anomaly(context.Components, "COMPANY_GUARANTOR_MISSING",
                "Kartu penjamin perusahaan kandidat tidak ditemukan. Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "COMPANY_GUARANTOR");

        var effectiveDate = context.CalculatedAt.UtcDateTime.Date;
        if (candidateCard.EffectiveStartDate.HasValue && candidateCard.EffectiveStartDate.Value.Date > effectiveDate)
            return Anomaly(context.Components, "POLICY_INACTIVE",
                "Kartu penjamin perusahaan kandidat belum mulai berlaku pada tanggal pelayanan. Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "COMPANY_GUARANTOR");

        if (candidateCard.EffectiveEndDate.HasValue && candidateCard.EffectiveEndDate.Value.Date < effectiveDate)
            return Anomaly(context.Components, "POLICY_INACTIVE",
                "Kartu penjamin perusahaan kandidat sudah berakhir pada tanggal pelayanan. Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "COMPANY_GUARANTOR");

        var guarantor = candidateCard.CompanyGuarantor;
        if (guarantor is null || guarantor.IsDelete || !guarantor.IsActive)
            return Anomaly(context.Components, "COMPANY_GUARANTOR_MISSING",
                "Perusahaan penjamin pada kartu kandidat tidak ditemukan atau tidak aktif. Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "COMPANY_GUARANTOR");

        if (guarantor.ContractStartDate.HasValue && guarantor.ContractStartDate.Value.Date > effectiveDate)
            return Anomaly(context.Components, "POLICY_INACTIVE",
                "Kontrak perusahaan penjamin kandidat belum mulai berlaku pada tanggal pelayanan. Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "COMPANY_GUARANTOR");

        if (guarantor.ContractEndDate.HasValue && guarantor.ContractEndDate.Value.Date < effectiveDate)
            return Anomaly(context.Components, "POLICY_INACTIVE",
                "Kontrak perusahaan penjamin kandidat sudah berakhir pada tanggal pelayanan. Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "COMPANY_GUARANTOR");

        if (!candidateCard.IsActive)
            return Anomaly(context.Components, "POLICY_INACTIVE",
                "Polis atau kartu penjamin perusahaan kandidat tercatat tidak aktif. Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "COMPANY_GUARANTOR");

        if (!candidateCard.IsEligible)
            return Anomaly(context.Components, "PAYER_NOT_ELIGIBLE",
                "Penjamin perusahaan kandidat belum dinyatakan layak (eligible). Seluruh biaya untuk sementara dibebankan ke pasien.",
                payerKind: "COMPANY_GUARANTOR");

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.EncounterId && !x.IsDelete, cancellationToken);
        if (encounter is null)
            return Anomaly(context.Components, "ENCOUNTER_NOT_FOUND",
                "Data kunjungan tidak ditemukan saat memeriksa penjamin. Hubungi tim teknis sebelum menagih.",
                payerKind: "COMPANY_GUARANTOR");

        if (candidateCard.PatientId != encounter.PatientId)
            return Anomaly(context.Components, "PAYER_NOT_ELIGIBLE",
                "Kartu penjamin perusahaan kandidat bukan milik pasien pada kunjungan ini.",
                payerKind: "COMPANY_GUARANTOR");

        return await _companyCoverageService.ResolveCandidateCoverageAsync(context, candidateCard, encounter, cancellationToken);
    }

    private async Task<BillingCoverageDecision> ResolveCompanyGuarantorAsync(
        BillingCoverageContext context,
        RegPatientEncounterGuarantor paymentSource,
        CancellationToken cancellationToken)
    {
        if (!paymentSource.IsEligible)
            return Anomaly(context.Components, "PAYER_NOT_ELIGIBLE",
                "Penjamin kunjungan ini belum dinyatakan layak (eligible). Seluruh biaya untuk sementara dibebankan ke pasien. Periksa data penjamin di Registrasi sebelum menagih.",
                payerKind: "COMPANY_GUARANTOR");
        if (!paymentSource.IsPolicyActive)
            return Anomaly(context.Components, "POLICY_INACTIVE",
                "Polis atau kartu penjamin perusahaan kunjungan ini tercatat tidak aktif. Seluruh biaya untuk sementara dibebankan ke pasien. Periksa data penjamin di Registrasi sebelum menagih.",
                payerKind: "COMPANY_GUARANTOR");
        if (!paymentSource.CompanyGuarantorId.HasValue)
            return Anomaly(context.Components, "COMPANY_GUARANTOR_MISSING",
                "Perusahaan penjamin kunjungan ini belum dipilih. Seluruh biaya untuk sementara dibebankan ke pasien. Lengkapi data penjamin di Registrasi.",
                payerKind: "COMPANY_GUARANTOR");

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.EncounterId && !x.IsDelete, cancellationToken);
        if (encounter is null)
            return Anomaly(context.Components, "ENCOUNTER_NOT_FOUND",
                "Data kunjungan tidak ditemukan saat memeriksa penjamin. Hubungi tim teknis sebelum menagih.",
                payerKind: "COMPANY_GUARANTOR");

        return await _companyCoverageService.ResolveCoverageAsync(context, paymentSource, encounter, cancellationToken);
    }

    private async Task<BillingCoverageDecision> ResolveInsuranceAsync(
        BillingCoverageContext context,
        RegPatientEncounterGuarantor paymentSource,
        CancellationToken cancellationToken)
    {
        if (!paymentSource.IsEligible)
            return Anomaly(context.Components, "PAYER_NOT_ELIGIBLE",
                "Penjamin kunjungan ini belum dinyatakan layak (eligible). Seluruh biaya untuk sementara dibebankan ke pasien. Periksa data penjamin di Registrasi sebelum menagih.",
                payerKind: "INSURANCE");
        if (!paymentSource.IsPolicyActive)
            return Anomaly(context.Components, "POLICY_INACTIVE",
                "Polis asuransi kunjungan ini tercatat tidak aktif. Seluruh biaya untuk sementara dibebankan ke pasien. Periksa data penjamin di Registrasi sebelum menagih.",
                payerKind: "INSURANCE");
        if (!paymentSource.InsuranceProviderId.HasValue)
            return Anomaly(context.Components, "INSURANCE_PROVIDER_MISSING",
                "Perusahaan asuransi kunjungan ini belum dipilih. Seluruh biaya untuk sementara dibebankan ke pasien. Lengkapi data penjamin di Registrasi.",
                payerKind: "INSURANCE");

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.EncounterId && !x.IsDelete, cancellationToken);
        if (encounter is null)
            return Anomaly(context.Components, "ENCOUNTER_NOT_FOUND",
                "Data kunjungan tidak ditemukan saat memeriksa penjamin. Hubungi tim teknis sebelum menagih.",
                payerKind: "INSURANCE");

        var effectiveDate = context.CalculatedAt.UtcDateTime.Date;
        var providerId = paymentSource.InsuranceProviderId.Value;
        var benefitPlanCode = paymentSource.BenefitPlanCodeSnapshot;
        var rules = await _dbContext.MstInsuranceCoverageRules.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.InsuranceProviderId == providerId
                && (x.BenefitPlanCode == null || x.BenefitPlanCode == benefitPlanCode)
                && (x.PatientClassId == null || x.PatientClassId == encounter.PatientClassId)
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= effectiveDate)
                && (x.EffectiveEndDate == null || effectiveDate <= x.EffectiveEndDate))
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.RuleCode)
            .ToListAsync(cancellationToken);

        return CalculateInsuranceCoverageDecision(context, rules);
    }

    private static BillingCoverageDecision CalculateInsuranceCoverageDecision(
        BillingCoverageContext context,
        List<MstInsuranceCoverageRule> rules)
    {
        decimal primary = 0;
        // BE-BKC-030/BKC-DES-027: TIDAK ADA satu jalur pun lagi di bawah yang mengisi variabel ini
        // - selalu bernilai 0 sesudah amendment ini. TETAP DIPERTAHANKAN (bukan dihapus): masih
        // bagian arity BillingCoverageDecision/kolom BilCalculationVersion.UnresolvedCoverageAmount
        // yang MUST NOT diganti nama/dihapus, karena versi kalkulasi LAMA (dibuat sebelum amendment
        // ini) masih memuat angka sungguhan di kolom itu sebagai bukti perhitungan yang sudah
        // terjadi. Angka nol yang jujur pada versi baru lebih baik daripada field yang hilang.
        decimal unresolved = 0;
        // BE-BKC-028/030/BKC-DES-022/026: akumulator BERSAMA untuk jalur (2) NotCovered dan jalur
        // (5) residual, keduanya dengan IsAllowExcessPaymentByPatient=false - SATU akumulator, DUA
        // cabang (BKC-DES-026), bukan dua akumulator terpisah. Lihat
        // BillingCoverageDecision.NonBillableResidualAmount.
        decimal nonBillableResidual = 0;
        var appliedRuleIds = new HashSet<Guid>();
        var appliedPerVisit = new Dictionary<Guid, decimal>();
        var outcomes = new List<BillingCoverageComponentOutcome>();

        foreach (var component in context.Components.Where(x => x.Coverable && x.Amount > 0))
        {
            var rule = rules.FirstOrDefault(x => Matches(x, component));
            if (rule is null)
            {
                // Keputusan pengguna (di luar roadmap, mengubah sebagian gating BE-BKC-021): TIDAK
                // ADA rule sama sekali yang menyasar kategori/tarif/prosedur/obat ini untuk provider
                // ini - beda dari rule yang ADA tapi NotCovered-tanpa-excess (jalur (2) di bawah,
                // sejak BE-BKC-030 sudah tidak lagi mengisi unresolved juga - lihat BKC-DES-027).
                // Tidak ada rule berarti provider ini memang tidak menanggung jenis layanan ini sama
                // sekali - langsung jadi tanggungan pasien (Patient implisit lewat outcome 0/0),
                // BUKAN unresolved/menunggu verifikasi manual.
                outcomes.Add(new BillingCoverageComponentOutcome(
                    component.ComponentId, component.ComponentType, 0, 0, 0, 0));
                continue;
            }

            appliedRuleIds.Add(rule.Id);
            // BKC-DEC-071/072/074 (BE-BKC-024, melanjutkan penyempitan BKC-DEC-062/BE-BKC-021):
            // CoverageStatus=="NeedApproval" dan limit bulanan (MaxAmountPerMonth/MaxQuantityPerMonth)
            // TIDAK LAGI menggeser komponen ke unresolved - keduanya dicabut penuh sesuai jawaban
            // pemilik (01-existing-capability-map.md 17.4.E): rule yang statusnya NeedApproval kini
            // dihitung seperti Covered biasa, dan limit bulanan diperlakukan SELALU TERSEDIA sampai
            // mesin pemakaian kumulatif dibangun (coverage gap tertunda, bukan bagian rilis ini).
            // Kolomnya TIDAK dihapus - tetap terbaca di layar entri, cuma tidak lagi menahan
            // perhitungan tagihan. Limit PER KUNJUNGAN (MaxAmountPerVisit/MaxQuantityPerVisit) di
            // bawah TIDAK ikut tercabut - keduanya bertetangga di kode tapi BKC-DEC-071 hanya
            // mencabut batas bulanan.
            if (string.Equals(rule.CoverageStatus, "NotCovered", StringComparison.OrdinalIgnoreCase))
            {
                // BE-BKC-030/BKC-DES-026/BKC-DEC-089: titik tangkap BKC-DES-022 diperlebar, bukan
                // dipindah dan bukan digandakan. Jalur (2) ini kini menulis ke akumulator
                // nonBillableResidual yang SAMA PERSIS dengan jalur (5) di bawah - "penjamin tidak
                // membayar, DAN kontrak yang sama melarang menagihkannya ke pasien" berlaku identik
                // untuk kedua jalur, apa pun sumber angkanya. unresolved TIDAK LAGI diisi jalur mana
                // pun sesudah ini (BKC-DES-027) - field/kolomnya tetap dipertahankan sebagai bukti
                // perhitungan versi kalkulasi lama, bukan dihapus.
                var notCoveredNonBillable = !rule.IsAllowExcessPaymentByPatient ? component.Amount : 0;
                if (notCoveredNonBillable > 0) nonBillableResidual += notCoveredNonBillable;
                outcomes.Add(new BillingCoverageComponentOutcome(
                    component.ComponentId, component.ComponentType, 0, 0, 0, notCoveredNonBillable));
                continue;
            }

            var covered = CalculateCoveredAmount(component, rule);
            if (rule.MaxAmountPerVisit.GetValueOrDefault() > 0)
            {
                var used = appliedPerVisit.GetValueOrDefault(rule.Id);
                covered = Math.Min(covered, Math.Max(0, rule.MaxAmountPerVisit!.Value - used));
                appliedPerVisit[rule.Id] = used + covered;
            }

            primary += covered;
            var residual = component.Amount - covered;
            // BE-BKC-028/BKC-DES-022/BKC-DEC-080: satu-satunya perubahan perilaku amendment ini.
            // Residual jalur (5) yang kontraknya melarang penagihan ke pasien
            // (IsAllowExcessPaymentByPatient=false) kini masuk nonBillableResidual, BUKAN unresolved
            // lagi - nominal yang sama, ember yang berbeda (menunggu Finance mengajukan write-off
            // kategori NON_BILLABLE_RESIDUAL, bukan menggantung tanpa tindak lanjut). Cabang true
            // TIDAK disentuh: residual tetap jatuh ke pasien lewat identitas turunan (BKC-DEC-070).
            var residualNonBillable = !rule.IsAllowExcessPaymentByPatient ? residual : 0;
            if (residualNonBillable > 0) nonBillableResidual += residualNonBillable;
            outcomes.Add(new BillingCoverageComponentOutcome(
                component.ComponentId, component.ComponentType, covered, 0, 0, residualNonBillable));
        }

        return new BillingCoverageDecision(
            ContractVersion,
            primary > 0 ? "APPROVED" : unresolved > 0 ? "PENDING_OR_UNRESOLVED" : "NO_COVERAGE",
            "NOT_CONFIGURED",
            primary,
            0,
            unresolved,
            appliedRuleIds.Order().ToArray(),
            outcomes,
            0,
            nonBillableResidual,
            [],
            PayerKind: "INSURANCE");
    }

    private static bool Matches(MstInsuranceCoverageRule rule, BillingCoverageComponent component)
    {
        // Bug fix (di luar roadmap, laporan pengguna): sebelumnya gerbang pertama memaksa
        // rule.ItemType harus sama persis dengan component.CoverageItemType - satu tag TUNGGAL
        // yang dipaksakan dari kategori item (CoverageItemType() di BillingCalculationService.cs
        // selalu mengembalikan "Drug" untuk kategori IsPharmacy=true, apa pun rule yang menyasarnya).
        // Akibatnya rule ItemType="ServiceCategory" dengan TariffCategoryId ke kategori Drug/Pharmacy
        // TIDAK PERNAH bisa cocok, walau TariffCategoryId-nya sudah benar. Diselaraskan dengan pola
        // InsuranceCoverageService.FindCoverageRuleAsync (dipakai advisory tariff preview) yang sudah
        // benar: masing-masing dimensi digerbangi ItemType SPESIFIKNYA sendiri secara independen,
        // bukan satu tag tunggal yang dipaksakan dari kategori item.
        return
            (string.Equals(rule.ItemType, "Tariff", StringComparison.OrdinalIgnoreCase)
                && rule.TariffId.HasValue && rule.TariffId == component.TariffId)
            || (string.Equals(rule.ItemType, "Drug", StringComparison.OrdinalIgnoreCase)
                && rule.DrugId.HasValue && rule.DrugId == component.DrugId)
            || (string.Equals(rule.ItemType, "DrugCategory", StringComparison.OrdinalIgnoreCase)
                && rule.DrugCategoryId.HasValue && rule.DrugCategoryId == component.DrugCategoryId)
            || (string.Equals(rule.ItemType, "Procedure", StringComparison.OrdinalIgnoreCase)
                && rule.ProcedureId.HasValue && rule.ProcedureId == component.ProcedureId)
            || (string.Equals(rule.ItemType, "ServiceCategory", StringComparison.OrdinalIgnoreCase)
                && rule.TariffCategoryId.HasValue && rule.TariffCategoryId == component.TariffCategoryId);
    }

    private static decimal CalculateCoveredAmount(BillingCoverageComponent component, MstInsuranceCoverageRule rule)
    {
        // Bug fix (di luar roadmap, laporan pengguna): "0 = tidak dibatasi" sesuai form master data
        // Insurance Coverage Rule - GetValueOrDefault() > 0 dipakai untuk MaxQuantityPerVisit dan
        // MaxCoverageAmount (dua field yang jadi GERBANG batas, bukan pengurang aritmetika murni
        // seperti CoPaymentPercent/CoPaymentAmount di bawah - 0 di situ sudah otomatis tidak
        // berefek tanpa perlu pengecekan tambahan).
        var quantityFactor = 1m;
        if (rule.MaxQuantityPerVisit.GetValueOrDefault() > 0 && component.Quantity > 0)
            quantityFactor = Math.Min(1m, rule.MaxQuantityPerVisit!.Value / component.Quantity);

        var eligible = component.Amount * quantityFactor;
        // Keputusan pengguna (di luar roadmap): CoveragePercent dan CoPaymentPercent SALING
        // MELENGKAPI (selalu berjumlah 100), bukan dua pengurang independen - CoveragePercent
        // satu-satunya input yang menentukan porsi tertanggung, CoPaymentPercent murni nilai
        // turunan/tampilan (lihat InsuranceCoverageRuleController, diturunkan server-side).
        // Sebelumnya kode ini MENUMPUK keduanya (mis. 75% dipotong lagi 25% dari eligible penuh -
        // tertanggung jadi cuma 50%, bukan 75% yang dimaksud). CoPaymentAmount TETAP independen
        // (nominal tetap, bukan persentase yang tumpang tindih dengan CoveragePercent).
        var covered = eligible * Math.Clamp(rule.CoveragePercent, 0, 100) / 100m;
        if (rule.CoPaymentAmount.HasValue)
            covered -= rule.CoPaymentAmount.Value;
        if (rule.MaxCoverageAmount.GetValueOrDefault() > 0)
            covered = Math.Min(covered, rule.MaxCoverageAmount!.Value);

        return Math.Clamp(decimal.Round(covered, 2, MidpointRounding.AwayFromZero), 0, component.Amount);
    }

    // Tanpa outcome eksplisit sama sekali - SETIAP komponen dianggap seluruhnya Patient oleh
    // pemanggil (lihat komentar BillingCoverageComponentOutcome), sesuai semantik SELF_PAY.
    private static BillingCoverageDecision SelfPay() =>
        new(ContractVersion, "SELF_PAY", "NOT_APPLICABLE", 0, 0, 0, [], [], 0, 0, [], PayerKind: "CASH");

    // BE-BKC-025/BKC-DEC-073/BKC-DES-010/011: menggantikan Unresolved(...) untuk jalur (4).
    // Precondition penjamin/encounter bermasalah TIDAK LAGI membuat komponen menggantung
    // (unresolved) - kalkulasi tetap BERHASIL, seluruh komponen coverable (bukan cuma totalnya)
    // dicatat DataAnomalyAmount = Amount secara eksplisit per komponen (supaya badge/split per
    // item tidak keliru menganggapnya Patient lewat default kosong-berarti-Patient), dan
    // nilainya jatuh ke pasien lewat identitas turunan yang sama seperti sebelumnya
    // (PatientAmount = Amount - Primary - Unresolved - DataAnomalyAmount). PrimaryStatus TETAP
    // "NO_COVERAGE" seperti jalur normal tanpa rule cocok - anomali dibedakan lewat Anomalies,
    // bukan lewat status baru, supaya konsumen lama yang hanya membaca PrimaryStatus tidak keliru
    // membacanya sebagai penolakan klaim.
    private static BillingCoverageDecision Anomaly(
        IReadOnlyList<BillingCoverageComponent> components, string code, string message, string payerKind = "CASH")
    {
        var coverable = components.Where(x => x.Coverable && x.Amount > 0).ToList();
        var outcomes = coverable
            .Select(x => new BillingCoverageComponentOutcome(x.ComponentId, x.ComponentType, 0, 0, x.Amount, 0))
            .ToList();
        var amount = coverable.Sum(x => x.Amount);

        return new(
            ContractVersion, "NO_COVERAGE", "NOT_CONFIGURED", 0, 0, 0, [], outcomes,
            amount, 0, [new BillingCoverageAnomaly(code, message)], PayerKind: payerKind);
    }
}
