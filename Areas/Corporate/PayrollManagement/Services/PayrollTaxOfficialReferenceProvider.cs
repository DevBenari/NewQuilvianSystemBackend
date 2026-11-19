using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Ringkasan kesiapan dataset regulasi PPh 21 resmi pada Backend (read-only untuk metadata dan validasi kebijakan).</summary>
public sealed record PayrollTaxOfficialReferenceStatus(
    string ReferenceCode,
    string ReferenceVersion,
    string CalculationType,
    string CalculationTypeLabel,
    string CalculationMethodLabel,
    string DefaultLegalBasis,
    bool IsReady,
    string StatusLabel,
    int PtkpCount,
    int TerRateCount,
    int BracketCount,
    IReadOnlyList<string> Issues);

/// <summary>Dataset regulasi PPh 21 resmi terverifikasi untuk pengisian otomatis sisi server pada <see cref="PrlTaxPolicy"/>.</summary>
public sealed record PayrollTaxOfficialReferenceDataset(
    PayrollTaxOfficialReferenceStatus Status,
    decimal PositionCostRatePercent,
    decimal PositionCostMonthlyCap,
    decimal PositionCostAnnualCap,
    decimal NoNpwpSurchargePercent,
    decimal PkpRoundingUnit,
    bool RoundTaxToWholeRupiah,
    string EmployerPremiumPrograms,
    string PensionDeductionPrograms,
    IReadOnlyList<PtkpEntry> Ptkp,
    IReadOnlyList<TerRateEntry> TerRates,
    IReadOnlyList<TaxBracketEntry> Brackets);

/// <summary>
/// Sumber referensi tarif resmi PPh Pasal 21 Pegawai Tetap berbasis:
/// - PP Nomor 58 Tahun 2023 (Lampiran Tarif Efektif Bulanan Kategori A, B, dan C: 44 + 40 + 41 = 125 lapisan);
/// - PMK Nomor 168 Tahun 2023 (biaya jabatan 5%, batas Rp500.000/bulan dan Rp6.000.000/tahun, pembulatan PKP Rp1.000,
///   rekonsiliasi masa pajak terakhir Pasal 17);
/// - UU Nomor 7 Tahun 2021 tentang Harmonisasi Peraturan Perpajakan (UU HPP — 5 lapisan progresif Pasal 17);
/// - PMK Nomor 101/PMK.010/2016 (PTKP TK/0 s.d. K/3).
///
/// Seluruh tabel diverifikasi secara statis melalui <see cref="PayrollTaxPolicyEngine.ValidateCompleteness"/> agar tidak
/// terdapat celah, tumpang tindih, atau ketidaklengkapan sebelum dapat dipakai menyusun atau mengesahkan kebijakan PPh.
/// </summary>
public static class PayrollTaxOfficialReferenceProvider
{
    public const string ReferenceCode = "ID-PPH21-PP58-2023-PMK168-2023";
    public const string ReferenceVersion = "PP58-2023-PMK168-2023-V1";
    public const string CalculationTypeLabel = "PPh 21 Pegawai Tetap";
    public const string CalculationMethodLabel = "TER Bulanan + Rekonsiliasi Tahunan";
    public const string DefaultLegalBasis =
        "PP Nomor 58 Tahun 2023 jo. PMK Nomor 168 Tahun 2023 jo. UU Nomor 7 Tahun 2021 (UU HPP) dan PMK 101/PMK.010/2016";

    private static readonly Lazy<PayrollTaxOfficialReferenceDataset> CachedDataset = new(BuildAndVerify);

    public static PayrollTaxOfficialReferenceDataset GetCurrent() => CachedDataset.Value;

    public static PayrollTaxOfficialReferenceStatus GetStatus() => CachedDataset.Value.Status;

    private static PayrollTaxOfficialReferenceDataset BuildAndVerify()
    {
        var ptkp = BuildOfficialPtkp();
        var terRates = BuildOfficialTerMonthly();
        var brackets = BuildOfficialPasal17Brackets();

        const decimal positionCostRatePercent = 5m;
        const decimal positionCostMonthlyCap = 500_000m;
        const decimal positionCostAnnualCap = 6_000_000m;
        const decimal noNpwpSurchargePercent = 20m;
        const decimal pkpRoundingUnit = 1_000m;
        const bool roundTaxToWholeRupiah = true;
        const string employerPremiumPrograms = "KES,JKK,JKM";
        const string pensionDeductionPrograms = "JHT,JP";

        var synthetic = new PayrollTaxPolicy(
            Id: Guid.Empty,
            Version: ReferenceVersion,
            LegalBasis: DefaultLegalBasis,
            EffectiveStartDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveEndDate: null,
            PositionCostRatePercent: positionCostRatePercent,
            PositionCostMonthlyCap: positionCostMonthlyCap,
            PositionCostAnnualCap: positionCostAnnualCap,
            NoNpwpSurchargePercent: noNpwpSurchargePercent,
            PkpRoundingUnit: pkpRoundingUnit,
            RoundTaxToWholeRupiah: roundTaxToWholeRupiah,
            EmployerPremiumPrograms: PayrollTaxPolicyEngine.ParseProgramList(employerPremiumPrograms),
            PensionDeductionPrograms: PayrollTaxPolicyEngine.ParseProgramList(pensionDeductionPrograms),
            NonEmployeeBasisPercent: null,
            NonEmployeeMethod: null,
            Pph26RatePercent: null,
            Ptkp: ptkp,
            TerRates: terRates,
            Brackets: brackets);

        var issues = PayrollTaxPolicyEngine.ValidateCompleteness(synthetic);
        if (ptkp.Count < 8) issues.Add("Dataset PTKP resmi tidak lengkap (minimal 8 status TK/0 s.d. K/3).");
        if (terRates.Count(r => r.Category == TaxTerCategories.A) != 44) issues.Add("Dataset TER Bulanan Kategori A tidak sesuai 44 lapisan PP 58/2023.");
        if (terRates.Count(r => r.Category == TaxTerCategories.B) != 40) issues.Add("Dataset TER Bulanan Kategori B tidak sesuai 40 lapisan PP 58/2023.");
        if (terRates.Count(r => r.Category == TaxTerCategories.C) != 41) issues.Add("Dataset TER Bulanan Kategori C tidak sesuai 41 lapisan PP 58/2023.");
        if (brackets.Count != 5) issues.Add("Dataset lapisan tarif Pasal 17 UU HPP harus berjumlah 5 lapisan.");

        var isReady = issues.Count == 0;
        var status = new PayrollTaxOfficialReferenceStatus(
            ReferenceCode: ReferenceCode,
            ReferenceVersion: ReferenceVersion,
            CalculationType: TaxPolicyCalculationTypes.PermanentEmployeeTer,
            CalculationTypeLabel: CalculationTypeLabel,
            CalculationMethodLabel: CalculationMethodLabel,
            DefaultLegalBasis: DefaultLegalBasis,
            IsReady: isReady,
            StatusLabel: isReady ? "Terverifikasi" : "Belum Siap",
            PtkpCount: ptkp.Count,
            TerRateCount: terRates.Count,
            BracketCount: brackets.Count,
            Issues: issues);

        return new PayrollTaxOfficialReferenceDataset(
            Status: status,
            PositionCostRatePercent: positionCostRatePercent,
            PositionCostMonthlyCap: positionCostMonthlyCap,
            PositionCostAnnualCap: positionCostAnnualCap,
            NoNpwpSurchargePercent: noNpwpSurchargePercent,
            PkpRoundingUnit: pkpRoundingUnit,
            RoundTaxToWholeRupiah: roundTaxToWholeRupiah,
            EmployerPremiumPrograms: employerPremiumPrograms,
            PensionDeductionPrograms: pensionDeductionPrograms,
            Ptkp: ptkp,
            TerRates: terRates,
            Brackets: brackets);
    }

    /// <summary>PTKP resmi PMK 101/PMK.010/2016 dan pemetaan kategori TER sesuai Pasal 2 PP 58 Tahun 2023.</summary>
    private static IReadOnlyList<PtkpEntry> BuildOfficialPtkp() => new List<PtkpEntry>
    {
        new("TK/0", 54_000_000m, TaxTerCategories.A),
        new("TK/1", 58_500_000m, TaxTerCategories.A),
        new("K/0", 58_500_000m, TaxTerCategories.A),
        new("TK/2", 63_000_000m, TaxTerCategories.B),
        new("TK/3", 67_500_000m, TaxTerCategories.B),
        new("K/1", 63_000_000m, TaxTerCategories.B),
        new("K/2", 67_500_000m, TaxTerCategories.B),
        new("K/3", 72_000_000m, TaxTerCategories.C)
    };

    /// <summary>Tarif progresif Pasal 17 ayat (1) huruf a UU PPh sebagaimana diubah dengan UU Nomor 7 Tahun 2021 (UU HPP).</summary>
    private static IReadOnlyList<TaxBracketEntry> BuildOfficialPasal17Brackets() => new List<TaxBracketEntry>
    {
        new(0m, 60_000_000m, 5m),
        new(60_000_000m, 250_000_000m, 15m),
        new(250_000_000m, 500_000_000m, 25m),
        new(500_000_000m, 5_000_000_000m, 30m),
        new(5_000_000_000m, null, 35m)
    };

    private static List<TerRateEntry> BuildCategoryTiers(string category, (decimal? Upper, decimal Rate)[] tiers)
    {
        var list = new List<TerRateEntry>(tiers.Length);
        var lower = 0m;
        foreach (var (upper, rate) in tiers)
        {
            list.Add(new TerRateEntry(category, TaxTerPeriodKinds.Monthly, lower, upper, rate));
            if (upper.HasValue) lower = upper.Value;
        }
        return list;
    }

    /// <summary>Lampiran PP Nomor 58 Tahun 2023 — Tabel Tarif Efektif Bulanan Kategori A (44), Kategori B (40), dan Kategori C (41).</summary>
    private static IReadOnlyList<TerRateEntry> BuildOfficialTerMonthly()
    {
        var rows = new List<TerRateEntry>(125);

        // Lampiran Huruf A PP 58/2023 — Kategori A (TK/0, TK/1, K/0): 44 lapisan
        rows.AddRange(BuildCategoryTiers(TaxTerCategories.A, new (decimal? Upper, decimal Rate)[]
        {
            (5_400_000m, 0.00m),
            (5_650_000m, 0.25m),
            (5_950_000m, 0.50m),
            (6_300_000m, 0.75m),
            (6_750_000m, 1.00m),
            (7_500_000m, 1.25m),
            (8_550_000m, 1.50m),
            (9_650_000m, 1.75m),
            (10_050_000m, 2.00m),
            (10_350_000m, 2.25m),
            (10_700_000m, 2.50m),
            (11_050_000m, 3.00m),
            (11_600_000m, 3.50m),
            (12_500_000m, 4.00m),
            (13_750_000m, 5.00m),
            (15_100_000m, 6.00m),
            (16_950_000m, 7.00m),
            (19_750_000m, 8.00m),
            (24_150_000m, 9.00m),
            (26_450_000m, 10.00m),
            (28_000_000m, 11.00m),
            (30_050_000m, 12.00m),
            (32_400_000m, 13.00m),
            (35_400_000m, 14.00m),
            (39_100_000m, 15.00m),
            (43_850_000m, 16.00m),
            (47_800_000m, 17.00m),
            (51_400_000m, 18.00m),
            (56_300_000m, 19.00m),
            (62_200_000m, 20.00m),
            (68_600_000m, 21.00m),
            (77_500_000m, 22.00m),
            (89_000_000m, 23.00m),
            (103_000_000m, 24.00m),
            (125_000_000m, 25.00m),
            (157_000_000m, 26.00m),
            (206_000_000m, 27.00m),
            (337_000_000m, 28.00m),
            (454_000_000m, 29.00m),
            (550_000_000m, 30.00m),
            (695_000_000m, 31.00m),
            (910_000_000m, 32.00m),
            (1_400_000_000m, 33.00m),
            (null, 34.00m)
        }));

        // Lampiran Huruf B PP 58/2023 — Kategori B (TK/2, TK/3, K/1, K/2): 40 lapisan
        rows.AddRange(BuildCategoryTiers(TaxTerCategories.B, new (decimal? Upper, decimal Rate)[]
        {
            (6_200_000m, 0.00m),
            (6_500_000m, 0.25m),
            (6_850_000m, 0.50m),
            (7_300_000m, 0.75m),
            (9_200_000m, 1.00m),
            (10_750_000m, 1.50m),
            (11_250_000m, 2.00m),
            (11_600_000m, 2.50m),
            (12_600_000m, 3.00m),
            (13_600_000m, 4.00m),
            (14_950_000m, 5.00m),
            (16_400_000m, 6.00m),
            (18_450_000m, 7.00m),
            (21_850_000m, 8.00m),
            (26_000_000m, 9.00m),
            (27_700_000m, 10.00m),
            (29_350_000m, 11.00m),
            (31_450_000m, 12.00m),
            (33_950_000m, 13.00m),
            (37_100_000m, 14.00m),
            (41_100_000m, 15.00m),
            (45_800_000m, 16.00m),
            (49_500_000m, 17.00m),
            (53_800_000m, 18.00m),
            (58_500_000m, 19.00m),
            (64_000_000m, 20.00m),
            (71_000_000m, 21.00m),
            (80_000_000m, 22.00m),
            (93_000_000m, 23.00m),
            (109_000_000m, 24.00m),
            (129_000_000m, 25.00m),
            (163_000_000m, 26.00m),
            (211_000_000m, 27.00m),
            (374_000_000m, 28.00m),
            (459_000_000m, 29.00m),
            (555_000_000m, 30.00m),
            (704_000_000m, 31.00m),
            (957_000_000m, 32.00m),
            (1_405_000_000m, 33.00m),
            (null, 34.00m)
        }));

        // Lampiran Huruf C PP 58/2023 — Kategori C (K/3): 41 lapisan
        rows.AddRange(BuildCategoryTiers(TaxTerCategories.C, new (decimal? Upper, decimal Rate)[]
        {
            (6_600_000m, 0.00m),
            (6_950_000m, 0.25m),
            (7_350_000m, 0.50m),
            (7_800_000m, 0.75m),
            (8_850_000m, 1.00m),
            (9_800_000m, 1.25m),
            (10_950_000m, 1.50m),
            (11_200_000m, 1.75m),
            (12_050_000m, 2.00m),
            (12_950_000m, 3.00m),
            (14_150_000m, 4.00m),
            (15_550_000m, 5.00m),
            (17_050_000m, 6.00m),
            (19_500_000m, 7.00m),
            (22_700_000m, 8.00m),
            (26_600_000m, 9.00m),
            (28_100_000m, 10.00m),
            (30_100_000m, 11.00m),
            (32_600_000m, 12.00m),
            (35_400_000m, 13.00m),
            (38_900_000m, 14.00m),
            (43_000_000m, 15.00m),
            (47_400_000m, 16.00m),
            (51_200_000m, 17.00m),
            (55_800_000m, 18.00m),
            (60_400_000m, 19.00m),
            (66_700_000m, 20.00m),
            (74_500_000m, 21.00m),
            (83_200_000m, 22.00m),
            (95_600_000m, 23.00m),
            (110_000_000m, 24.00m),
            (134_000_000m, 25.00m),
            (169_000_000m, 26.00m),
            (221_000_000m, 27.00m),
            (390_000_000m, 28.00m),
            (463_000_000m, 29.00m),
            (561_000_000m, 30.00m),
            (709_000_000m, 31.00m),
            (965_000_000m, 32.00m),
            (1_419_000_000m, 33.00m),
            (null, 34.00m)
        }));

        return rows;
    }
}
