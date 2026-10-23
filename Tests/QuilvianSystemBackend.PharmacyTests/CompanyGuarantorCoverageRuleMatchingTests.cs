using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Regresi pencocokan aturan tanggungan penjamin perusahaan (Benefit Karyawan RS).
///
/// Kasus asal: invoice BIL-20261022-00000001 (RAJAL, penjamin "Benefit Karyawan RS"), item ABBOTIC GRANUL
/// Rp347.670,00 + PPN 11% = Rp385.913,70 tampil "Tunai". Aturan CCR-RSMMC-00001 (Kategori Tarif = Drug,
/// Covered 100%, kelas RAWAT JALAN) disimpan master data sebagai ItemType "TariffCategory" (alias lama
/// "ServiceCategory" dipetakan ke sana), sedangkan mesin hanya mengenali "ServiceCategory", sehingga aturan
/// tidak pernah cocok dan porsi penjamin 0.
///
/// Fungsi yang diuji murni (tanpa database): Matches, Normalize, dan CalculateCoverageDecision.
/// </summary>
public class CompanyGuarantorCoverageRuleMatchingTests
{
    private static readonly Guid DrugCategoryTariffId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OutpatientClassId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid InpatientClassId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly DateTimeOffset CalculatedAt = new(2026, 10, 22, 3, 0, 0, TimeSpan.Zero);

    private const decimal ItemAmount = 347_670.00m;
    private const decimal TaxAmount = 38_243.70m;

    private static MstCompanyGuarantorCoverageRule CategoryRule(
        string itemType,
        Guid? tariffCategoryId = null,
        string coverageStatus = "Covered",
        decimal coveragePercent = 100m,
        Guid? patientClassId = null,
        int priority = 0,
        string ruleCode = "CCR-RSMMC-00001") => new()
    {
        RuleCode = ruleCode,
        ItemType = itemType,
        TariffCategoryId = tariffCategoryId ?? DrugCategoryTariffId,
        PatientClassId = patientClassId,
        CoverageStatus = coverageStatus,
        CoveragePercent = coveragePercent,
        Priority = priority,
        IsActive = true
    };

    private static BillingCoverageComponent Component(
        decimal amount,
        string componentType = "ITEM",
        Guid? componentId = null,
        Guid? categoryId = null,
        Guid? patientClassId = null,
        bool coverable = true) => new(
        componentId ?? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        componentType,
        "Drug",
        TariffId: Guid.NewGuid(),
        ProcedureId: null,
        DrugId: Guid.NewGuid(),
        DrugCategoryId: Guid.NewGuid(),
        TariffCategoryId: categoryId ?? DrugCategoryTariffId,
        Quantity: 1,
        Amount: amount,
        Coverable: coverable,
        ServiceOccurredAt: CalculatedAt,
        PatientClassId: patientClassId ?? OutpatientClassId);

    private static BillingCoverageDecision Decide(
        IReadOnlyList<BillingCoverageComponent> components,
        params MstCompanyGuarantorCoverageRule[] rules) =>
        CompanyGuarantorCoverageService.CalculateCoverageDecision(
            new BillingCoverageContext(
                Guid.NewGuid(), Guid.NewGuid(), CalculatedAt, components.Sum(x => x.Amount), components),
            rules.ToList());

    // ------------------------------------------------------------------
    // Matches: representasi ItemType kategori
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("TariffCategory")]
    [InlineData("ServiceCategory")]
    [InlineData("tariffcategory")]
    [InlineData("SERVICECATEGORY")]
    [InlineData(" TariffCategory ")]
    public void Matches_CategoryRule_AcceptsTariffCategoryAndLegacyServiceCategory(string itemType)
    {
        var rule = CategoryRule(itemType);

        Assert.True(CompanyGuarantorCoverageService.Matches(rule, Component(ItemAmount)));
    }

    [Theory]
    [InlineData("TariffCategory")]
    [InlineData("ServiceCategory")]
    public void Matches_CategoryRule_DoesNotMatchOtherCategory(string itemType)
    {
        var rule = CategoryRule(itemType);

        Assert.False(CompanyGuarantorCoverageService.Matches(rule, Component(ItemAmount, categoryId: OtherCategoryId)));
    }

    [Theory]
    [InlineData("TariffCategory")]
    [InlineData("ServiceCategory")]
    public void Matches_CategoryRule_WithoutCategoryIdNeverMatches(string itemType)
    {
        var rule = CategoryRule(itemType);
        rule.TariffCategoryId = null;

        Assert.False(CompanyGuarantorCoverageService.Matches(rule, Component(ItemAmount)));
    }

    [Fact]
    public void Matches_SpecificRuleTypes_DoNotMatchByCategory()
    {
        // Aturan Drug/Tariff/Procedure/DrugCategory tidak boleh ikut cocok hanya karena TariffCategoryId sama.
        foreach (var itemType in new[] { "Drug", "Tariff", "Procedure", "DrugCategory" })
        {
            var rule = CategoryRule(itemType);
            rule.DrugId = null;
            rule.TariffId = null;
            rule.ProcedureId = null;
            rule.DrugCategoryId = null;

            Assert.False(
                CompanyGuarantorCoverageService.Matches(rule, Component(ItemAmount)),
                $"ItemType {itemType} tidak boleh cocok lewat TariffCategoryId.");
        }
    }

    // ------------------------------------------------------------------
    // Normalizer bersama (master data dan mesin tanggungan memakai satu definisi)
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("ServiceCategory", "TariffCategory")]
    [InlineData("  servicecategory ", "TariffCategory")]
    [InlineData("TariffCategory", "TariffCategory")]
    [InlineData("Tariff", "Tariff")]
    [InlineData("Drug", "Drug")]
    [InlineData(null, "")]
    public void Normalize_MapsLegacyAliasAndLeavesOtherTypesAlone(string? input, string expected)
    {
        Assert.Equal(expected, CompanyGuarantorCoverageItemTypes.Normalize(input));
    }

    // ------------------------------------------------------------------
    // CalculateCoverageDecision: kasus ABBOTIC dan pembeda alasan hasil
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("TariffCategory")]
    [InlineData("ServiceCategory")]
    public void Decision_AbboticItemAndTax_AreFullyCoveredByCategoryRule(string itemType)
    {
        var itemId = Guid.NewGuid();
        var components = new[]
        {
            Component(ItemAmount, "ITEM", itemId),
            Component(TaxAmount, "TAX", itemId)
        };
        var rule = CategoryRule(itemType, patientClassId: OutpatientClassId);

        var decision = Decide(components, rule);

        Assert.Equal("COMPANY_GUARANTOR", decision.PayerKind);
        Assert.Equal("APPROVED", decision.PrimaryStatus);
        Assert.Equal(ItemAmount + TaxAmount, decision.PrimaryAmount);
        Assert.Equal(0m, decision.UnresolvedAmount);
        Assert.Equal(0m, decision.DataAnomalyAmount);
        Assert.Equal(new[] { rule.Id }, decision.AppliedRuleIds);
        Assert.All(decision.ComponentOutcomes, outcome => Assert.Equal(BillingCoverageReasonCodes.MatchedCovered, outcome.CoverageReason));
        Assert.Equal(ItemAmount, decision.ComponentOutcomes.Single(x => x.ComponentType == "ITEM").PrimaryAmount);
        Assert.Equal(TaxAmount, decision.ComponentOutcomes.Single(x => x.ComponentType == "TAX").PrimaryAmount);
    }

    [Fact]
    public void Decision_PartialCoverage_ReportsMatchedPartial()
    {
        var components = new[] { Component(ItemAmount) };

        var decision = Decide(components, CategoryRule("TariffCategory", coveragePercent: 50m));

        var outcome = Assert.Single(decision.ComponentOutcomes);
        Assert.Equal(173_835.00m, outcome.PrimaryAmount);
        Assert.Equal(BillingCoverageReasonCodes.MatchedPartial, outcome.CoverageReason);
    }

    [Fact]
    public void Decision_NoMatchingRule_ReportsNoMatchingRuleWithZeroAmounts()
    {
        // Aturan ada tetapi untuk kategori lain: tidak ada yang cocok. Nominal tetap 0 (kebijakan existing:
        // pasien membayar); hanya alasannya yang dibedakan.
        var components = new[] { Component(ItemAmount) };

        var decision = Decide(components, CategoryRule("TariffCategory", tariffCategoryId: OtherCategoryId));

        var outcome = Assert.Single(decision.ComponentOutcomes);
        Assert.Equal(BillingCoverageReasonCodes.NoMatchingRule, outcome.CoverageReason);
        Assert.Equal(0m, outcome.PrimaryAmount);
        Assert.Equal(0m, outcome.UnresolvedAmount);
        Assert.Equal(0m, outcome.DataAnomalyAmount);
        Assert.Equal(0m, outcome.NonBillableResidualAmount);
        Assert.Equal(0m, decision.PrimaryAmount);
        Assert.Equal("NO_COVERAGE", decision.PrimaryStatus);
        Assert.Empty(decision.AppliedRuleIds);
    }

    [Fact]
    public void Decision_RuleForAnotherPatientClass_ReportsNoMatchingRule()
    {
        // Aturan kelas RAWAT JALAN tidak berlaku untuk komponen kelas rawat inap.
        var components = new[] { Component(ItemAmount, patientClassId: InpatientClassId) };

        var decision = Decide(components, CategoryRule("TariffCategory", patientClassId: OutpatientClassId));

        Assert.Equal(BillingCoverageReasonCodes.NoMatchingRule, Assert.Single(decision.ComponentOutcomes).CoverageReason);
        Assert.Equal(0m, decision.PrimaryAmount);
    }

    [Fact]
    public void Decision_RuleOutsideEffectivePeriod_ReportsNoMatchingRule()
    {
        var components = new[] { Component(ItemAmount) };
        var rule = CategoryRule("TariffCategory");
        rule.EffectiveEndDate = CalculatedAt.UtcDateTime.Date.AddDays(-1);

        var decision = Decide(components, rule);

        Assert.Equal(BillingCoverageReasonCodes.NoMatchingRule, Assert.Single(decision.ComponentOutcomes).CoverageReason);
    }

    [Fact]
    public void Decision_ExplicitNotCovered_IsDistinctFromNoMatchingRule()
    {
        var components = new[] { Component(ItemAmount) };

        var decision = Decide(components, CategoryRule("TariffCategory", coverageStatus: "NotCovered"));

        var outcome = Assert.Single(decision.ComponentOutcomes);
        Assert.Equal(BillingCoverageReasonCodes.ExplicitNotCovered, outcome.CoverageReason);
        Assert.Equal(0m, outcome.PrimaryAmount);
        Assert.Single(decision.AppliedRuleIds);
    }

    [Fact]
    public void Decision_ZeroPercentRule_IsExplicitNotCoveredNotNoMatchingRule()
    {
        var components = new[] { Component(ItemAmount) };

        var decision = Decide(components, CategoryRule("TariffCategory", coveragePercent: 0m));

        Assert.Equal(BillingCoverageReasonCodes.ExplicitNotCovered, Assert.Single(decision.ComponentOutcomes).CoverageReason);
        Assert.Equal(0m, decision.PrimaryAmount);
    }

    [Fact]
    public void Decision_NonCoverableComponent_IsSkippedSoCashAssignmentIsPreserved()
    {
        // Penugasan CASH menjadikan komponen non-coverable: mesin tidak menilainya sama sekali, jadi aturan yang
        // cocok pun tidak boleh mengubahnya menjadi tanggungan penjamin.
        var components = new[] { Component(ItemAmount, coverable: false) };

        var decision = Decide(components, CategoryRule("TariffCategory"));

        Assert.Empty(decision.ComponentOutcomes);
        Assert.Equal(0m, decision.PrimaryAmount);
        Assert.Empty(decision.AppliedRuleIds);
    }

    [Fact]
    public void Decision_ClassSpecificRuleWinsOverGenericRule_ForBothRepresentations()
    {
        var components = new[] { Component(ItemAmount) };
        var generic = CategoryRule("ServiceCategory", coveragePercent: 50m, ruleCode: "CCR-RSMMC-00002", priority: 10);
        var classSpecific = CategoryRule(
            "TariffCategory", coveragePercent: 100m, patientClassId: OutpatientClassId, ruleCode: "CCR-RSMMC-00001");

        var decision = Decide(components, generic, classSpecific);

        Assert.Equal(ItemAmount, decision.PrimaryAmount);
        Assert.Equal(new[] { classSpecific.Id }, decision.AppliedRuleIds);
    }

    // ------------------------------------------------------------------
    // Kode alasan dari kondisi anomali payer
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("PAYER_NOT_ELIGIBLE", BillingCoverageReasonCodes.PayerNotEligible)]
    [InlineData("POLICY_INACTIVE", BillingCoverageReasonCodes.CoverageUnresolved)]
    [InlineData("COMPANY_GUARANTOR_MISSING", BillingCoverageReasonCodes.CoverageUnresolved)]
    [InlineData("ENCOUNTER_NOT_FOUND", BillingCoverageReasonCodes.CoverageUnresolved)]
    public void FromAnomalyCode_MapsToReasonVocabulary(string anomalyCode, string expected)
    {
        Assert.Equal(expected, BillingCoverageReasonCodes.FromAnomalyCode(anomalyCode));
    }
}
