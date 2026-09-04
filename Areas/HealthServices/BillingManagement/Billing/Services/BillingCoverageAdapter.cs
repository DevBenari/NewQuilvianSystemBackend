using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public interface IBillingCoverageAdapter
{
    Task<BillingCoverageDecision> ResolveAsync(BillingCoverageContext context, CancellationToken cancellationToken);
}

public sealed record BillingCoverageContext(
    Guid InvoiceId,
    Guid EncounterId,
    DateTimeOffset CalculatedAt,
    decimal EligibleAmount,
    IReadOnlyList<BillingCoverageComponent> Components);

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
    IReadOnlyList<BillingCoverageComponentOutcome> ComponentOutcomes);

// Bug fix (di luar roadmap, laporan pengguna): sebelumnya waterfall hanya mengembalikan TOTAL
// gabungan (PrimaryAmount/UnresolvedAmount di atas) - badge per item Menu Pembayaran dan split
// Subtotal/Pajak Mandiri-Asuransi terpaksa memakai flag "coverable" tingkat kategori (bukan hasil
// tiap item sesungguhnya) sebagai pendekatan. ComponentOutcomes membawa hasil PER KOMPONEN (item
// ATAU komponen pajaknya, sesuai ComponentId+ComponentType) - PatientAmount komponen itu TIDAK
// disimpan eksplisit di sini, cukup diturunkan pemanggil sebagai
// component.Amount - PrimaryAmount - UnresolvedAmount (identitas ini selalu benar by construction).
// Komponen yang tidak muncul di daftar ini (mis. jalur SelfPay) dianggap seluruhnya Patient.
public sealed record BillingCoverageComponentOutcome(
    Guid ComponentId,
    string ComponentType,
    decimal PrimaryAmount,
    decimal UnresolvedAmount);

public sealed class RegistrationBillingCoverageAdapter : IBillingCoverageAdapter
{
    public const string ContractVersion = "REGISTRATION-COVERAGE-ADAPTER-1";
    private readonly ApplicationDbContext _dbContext;

    public RegistrationBillingCoverageAdapter(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<BillingCoverageDecision> ResolveAsync(
        BillingCoverageContext context,
        CancellationToken cancellationToken)
    {
        var paymentSource = await _dbContext.TrxPatientEncounterGuarantors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EncounterId == context.EncounterId && x.IsActive && !x.IsDelete, cancellationToken);

        if (paymentSource is null || paymentSource.PaymentType == EncounterPaymentType.Cash)
            return SelfPay();

        if (!paymentSource.IsEligible || !paymentSource.IsPolicyActive || !paymentSource.InsuranceProviderId.HasValue)
            return Unresolved(context.Components, "REJECTED");

        var encounter = await _dbContext.TrxPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.EncounterId && !x.IsDelete, cancellationToken);
        if (encounter is null)
            return Unresolved(context.Components, "UNRESOLVED");

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

        decimal primary = 0;
        decimal unresolved = 0;
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
                // ini - beda dari rule yang ADA tapi butuh verifikasi (NeedApproval/limit
                // bulanan/NotCovered-tanpa-excess, TETAP unresolved seperti sebelumnya, lihat
                // cabang-cabang di bawah). Tidak ada rule berarti provider ini memang tidak
                // menanggung jenis layanan ini sama sekali - langsung jadi tanggungan pasien
                // (Patient implisit lewat outcome 0/0), BUKAN unresolved/menunggu verifikasi manual.
                outcomes.Add(new BillingCoverageComponentOutcome(
                    component.ComponentId, component.ComponentType, 0, 0));
                continue;
            }

            appliedRuleIds.Add(rule.Id);
            // BKC-DEC-062 (amendment BKC-DEC-042): IsNeedApproval/IsNeedGuaranteeLetter TIDAK LAGI
            // menggeser komponen ke unresolved. Sebelumnya, item dengan rule Covered tapi butuh
            // approval/surat jaminan ikut jatuh ke unresolved - padahal approval itu proses
            // administratif terpisah, bukan penolakan coverage. Subtotal Asuransi jadi salah kecil
            // di Menu Pembayaran untuk kasus yang sangat umum (banyak rule asuransi mewajibkan
            // approval/SJP tapi tetap Covered). Scope dipersempit, BUKAN pelepasan gating penuh:
            // CoverageStatus=="NeedApproval" (rule yang secara eksplisit BELUM diputuskan statusnya)
            // dan limit bulanan (butuh pemeriksaan pemakaian kumulatif yang belum tersedia di sini)
            // tetap menggeser ke unresolved seperti sebelumnya.
            //
            // Bug fix (di luar roadmap, laporan pengguna): form master data Insurance Coverage Rule
            // menjanjikan "Isi 0 jika tidak dibatasi" - GetValueOrDefault() > 0 menyelaraskan gerbang
            // ini dengan janji itu, supaya rule tanpa batas bulanan sungguhan (diisi 0, bukan angka
            // nyata) tidak lagi ikut tergeser ke unresolved.
            if (string.Equals(rule.CoverageStatus, "NeedApproval", StringComparison.OrdinalIgnoreCase)
                || rule.MaxAmountPerMonth.GetValueOrDefault() > 0 || rule.MaxQuantityPerMonth.GetValueOrDefault() > 0)
            {
                unresolved += component.Amount;
                outcomes.Add(new BillingCoverageComponentOutcome(
                    component.ComponentId, component.ComponentType, 0, component.Amount));
                continue;
            }

            if (string.Equals(rule.CoverageStatus, "NotCovered", StringComparison.OrdinalIgnoreCase))
            {
                var notCoveredUnresolved = !rule.IsAllowExcessPaymentByPatient ? component.Amount : 0;
                if (notCoveredUnresolved > 0) unresolved += notCoveredUnresolved;
                outcomes.Add(new BillingCoverageComponentOutcome(
                    component.ComponentId, component.ComponentType, 0, notCoveredUnresolved));
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
            var residualUnresolved = !rule.IsAllowExcessPaymentByPatient ? residual : 0;
            if (residualUnresolved > 0) unresolved += residualUnresolved;
            outcomes.Add(new BillingCoverageComponentOutcome(
                component.ComponentId, component.ComponentType, covered, residualUnresolved));
        }

        return new BillingCoverageDecision(
            ContractVersion,
            primary > 0 ? "APPROVED" : unresolved > 0 ? "PENDING_OR_UNRESOLVED" : "NO_COVERAGE",
            "NOT_CONFIGURED",
            primary,
            0,
            unresolved,
            appliedRuleIds.Order().ToArray(),
            outcomes);
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
        new(ContractVersion, "SELF_PAY", "NOT_APPLICABLE", 0, 0, 0, [], []);

    // Provider tidak eligible/aktif, atau encounter tidak ditemukan - SELURUH komponen coverable
    // (bukan cuma totalnya) ditandai unresolved secara eksplisit per komponen, supaya badge/split
    // per item tidak keliru menganggapnya Patient (default kosong-berarti-Patient TIDAK berlaku
    // di sini - beda dari SelfPay, di jalur ini komponen MEMANG coverable, cuma belum bisa
    // diverifikasi, bukan benar-benar tunai).
    private static BillingCoverageDecision Unresolved(
        IReadOnlyList<BillingCoverageComponent> components, string primaryStatus)
    {
        var coverable = components.Where(x => x.Coverable && x.Amount > 0).ToList();
        var outcomes = coverable
            .Select(x => new BillingCoverageComponentOutcome(x.ComponentId, x.ComponentType, 0, x.Amount))
            .ToList();
        var amount = coverable.Sum(x => x.Amount);

        return new(ContractVersion, primaryStatus, "NOT_CONFIGURED", 0, 0, amount, [], outcomes);
    }
}
