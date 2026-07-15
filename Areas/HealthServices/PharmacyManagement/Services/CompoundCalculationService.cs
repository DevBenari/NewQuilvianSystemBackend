using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Kalkulator racikan murni (tanpa akses database). Service ini memisahkan
    /// jumlah klinis, jumlah sumber teoritis, dan quantity yang dipakai untuk tarif.
    /// </summary>
    public sealed class CompoundCalculationService
    {
        private static readonly Regex StrengthWithDenominatorRegex = new(
            @"(?<value>\d+(?:[\.,]\d+)?)\s*(?<unit>ng|mcg|ug|µg|μg|mg|g|kg|mmol|mol|meq|iu|ui|me|unit|%)\s*(?:/|per)\s*(?<denominator>\d+(?:[\.,]\d+)?)?\s*(?<denominatorUnit>ml|l|g|mg|mcg|tablet|tab|kaplet|kapsul|capsule|caps|vial|ampul|ampoule|amp|drop|tetes|puff|dose)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleStrengthRegex = new(
            @"(?<value>\d+(?:[\.,]\d+)?)\s*(?<unit>ng|mcg|ug|µg|μg|mg|g|kg|mmol|mol|meq|iu|ui|me|unit|%)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ParsedDrugStrength ParseStrength(string? strengthText)
        {
            if (string.IsNullOrWhiteSpace(strengthText))
                return ParsedDrugStrength.Empty;

            var normalized = strengthText.Trim();
            var allMatches = SimpleStrengthRegex.Matches(normalized);
            var denominatorMatch = StrengthWithDenominatorRegex.Match(normalized);

            Match selectedMatch = denominatorMatch.Success
                ? denominatorMatch
                : SimpleStrengthRegex.Match(normalized);

            if (!selectedMatch.Success)
                return new ParsedDrugStrength
                {
                    RawText = normalized,
                    IsParsed = false,
                    RequiresManualVerification = true,
                    Warning = "Kekuatan obat tidak dapat dibaca otomatis. Verifikasi manual diperlukan."
                };

            var value = ParseDecimal(selectedMatch.Groups["value"].Value);
            var denominator = selectedMatch.Groups["denominator"].Success
                ? ParseDecimal(selectedMatch.Groups["denominator"].Value)
                : 1m;

            return new ParsedDrugStrength
            {
                RawText = normalized,
                IsParsed = value > 0,
                StrengthValue = value > 0 ? value : null,
                StrengthUnitToken = NormalizeUnitToken(selectedMatch.Groups["unit"].Value),
                SourceContentQuantity = denominator > 0 ? denominator : 1m,
                SourceContentUnitToken = selectedMatch.Groups["denominatorUnit"].Success
                    ? NormalizeUnitToken(selectedMatch.Groups["denominatorUnit"].Value)
                    : null,
                RequiresManualVerification = allMatches.Count > 1,
                Warning = allMatches.Count > 1
                    ? "Obat memiliki lebih dari satu komponen kekuatan. Hasil otomatis memakai komponen pertama dan wajib diverifikasi farmasi."
                    : null
            };
        }

        public CompoundCalculationResult Calculate(CompoundCalculationRequest request)
        {
            if (request == null)
                return CompoundCalculationResult.Fail("Permintaan kalkulasi racikan tidak tersedia.");

            var totalPackage = PositiveOrDefault(request.TotalPackage, 1m);
            var legacyAmountPerPackage = PositiveOrDefault(request.LegacyAmountPerPackage, 1m);
            var legacyTotalQuantity = PositiveOrDefault(
                request.LegacyTotalQuantity,
                RoundClinical(legacyAmountPerPackage * totalPackage));

            var ingredientMode = ResolveIngredientMode(
                request.CompoundCalculationMode,
                request.IngredientCalculationMode,
                request.IngredientRole,
                request.IsQuantitySufficientToFinal);

            if (ingredientMode == CompoundIngredientCalculationMode.LegacySourceQuantity)
            {
                var pricingQuantity = ResolvePricingQuantity(
                    request.VerifiedSourceQuantity,
                    legacyTotalQuantity,
                    request.AllowFractionalSource);

                return CompoundCalculationResult.Success(
                    ingredientMode,
                    legacyAmountPerPackage,
                    legacyTotalQuantity,
                    null,
                    null,
                    legacyTotalQuantity,
                    pricingQuantity,
                    "Legacy",
                    "Jumlah sumber mengikuti per kemasan × jumlah kemasan.");
            }

            if (ingredientMode == CompoundIngredientCalculationMode.ManualSourceQuantity)
            {
                var manualQuantity = request.VerifiedSourceQuantity.HasValue &&
                                     request.VerifiedSourceQuantity.Value > 0
                    ? request.VerifiedSourceQuantity.Value
                    : legacyTotalQuantity;

                var pricingQuantity = ResolvePricingQuantity(
                    request.VerifiedSourceQuantity,
                    manualQuantity,
                    request.AllowFractionalSource);

                return CompoundCalculationResult.Success(
                    ingredientMode,
                    RoundClinical(manualQuantity / totalPackage),
                    RoundClinical(manualQuantity),
                    null,
                    null,
                    RoundClinical(manualQuantity),
                    pricingQuantity,
                    "Manual",
                    "Jumlah sumber ditentukan manual dan perlu diverifikasi farmasi.");
            }

            if (ingredientMode == CompoundIngredientCalculationMode.QuantitySufficient)
            {
                var estimatedQuantity = request.VerifiedSourceQuantity.HasValue &&
                                        request.VerifiedSourceQuantity.Value > 0
                    ? request.VerifiedSourceQuantity.Value
                    : legacyTotalQuantity;

                var pricingQuantity = ResolvePricingQuantity(
                    request.VerifiedSourceQuantity,
                    estimatedQuantity,
                    request.AllowFractionalSource);

                return CompoundCalculationResult.Success(
                    ingredientMode,
                    RoundClinical(estimatedQuantity / totalPackage),
                    RoundClinical(estimatedQuantity),
                    null,
                    request.FinalQuantityUnit,
                    RoundClinical(estimatedQuantity),
                    pricingQuantity,
                    "PharmacyVerificationRequired",
                    "Bahan q.s. ad jumlah akhir. Jumlah sumber adalah estimasi sampai diverifikasi farmasi.");
            }

            if (!request.TargetValue.HasValue || request.TargetValue.Value <= 0)
                return CompoundCalculationResult.Fail(
                    "Target dosis, kadar, atau konsentrasi bahan wajib lebih dari 0.",
                    ingredientMode);

            if (!request.SourceStrengthValue.HasValue || request.SourceStrengthValue.Value <= 0)
                return CompoundCalculationResult.Fail(
                    "Kekuatan sediaan sumber belum tersedia. Lengkapi strength obat atau gunakan mode jumlah sumber manual.",
                    ingredientMode);

            if (request.SourceStrengthUnit == null)
                return CompoundCalculationResult.Fail(
                    "Satuan kekuatan sediaan sumber belum tersedia.",
                    ingredientMode);

            decimal calculatedActiveAmount;
            CompoundUnitDescriptor? calculatedActiveUnit;
            string formulaText;

            switch (ingredientMode)
            {
                case CompoundIngredientCalculationMode.TargetDosePerUnit:
                    if (request.TargetUnit == null)
                        return CompoundCalculationResult.Fail(
                            "Satuan target dosis per unit belum tersedia.",
                            ingredientMode);

                    calculatedActiveAmount = request.TargetValue.Value * totalPackage;
                    calculatedActiveUnit = request.TargetUnit;
                    formulaText = $"{Format(request.TargetValue.Value)} {DisplayUnit(request.TargetUnit)} × {Format(totalPackage)} unit akhir";
                    break;

                case CompoundIngredientCalculationMode.TargetPercentage:
                    if (!request.FinalQuantity.HasValue || request.FinalQuantity.Value <= 0)
                        return CompoundCalculationResult.Fail(
                            "Jumlah akhir racikan wajib diisi untuk kalkulasi persentase.",
                            ingredientMode);

                    if (request.FinalQuantityUnit == null)
                        return CompoundCalculationResult.Fail(
                            "Satuan jumlah akhir racikan wajib tersedia untuk kalkulasi persentase.",
                            ingredientMode);

                    calculatedActiveAmount = request.FinalQuantity.Value * request.TargetValue.Value / 100m;
                    calculatedActiveUnit = request.FinalQuantityUnit;
                    formulaText = $"{Format(request.TargetValue.Value)}% × {Format(request.FinalQuantity.Value)} {DisplayUnit(request.FinalQuantityUnit)}";
                    break;

                case CompoundIngredientCalculationMode.TargetConcentration:
                    if (!request.FinalQuantity.HasValue || request.FinalQuantity.Value <= 0)
                        return CompoundCalculationResult.Fail(
                            "Jumlah akhir racikan wajib diisi untuk kalkulasi konsentrasi.",
                            ingredientMode);

                    if (request.TargetUnit == null)
                        return CompoundCalculationResult.Fail(
                            "Satuan zat aktif pada konsentrasi belum tersedia.",
                            ingredientMode);

                    calculatedActiveAmount = request.TargetValue.Value * request.FinalQuantity.Value;
                    calculatedActiveUnit = request.TargetUnit;
                    formulaText = $"{Format(request.TargetValue.Value)} {DisplayUnit(request.TargetUnit)}/{DisplayUnit(request.FinalQuantityUnit)} × {Format(request.FinalQuantity.Value)} {DisplayUnit(request.FinalQuantityUnit)}";
                    break;

                default:
                    return CompoundCalculationResult.Fail(
                        "Mode kalkulasi bahan racikan belum didukung.",
                        ingredientMode);
            }

            if (!TryConvert(
                    calculatedActiveAmount,
                    calculatedActiveUnit,
                    request.SourceStrengthUnit,
                    out var activeAmountInStrengthUnit,
                    out var conversionError))
            {
                return CompoundCalculationResult.Fail(
                    conversionError ?? "Satuan target tidak dapat dikonversi ke satuan strength sumber.",
                    ingredientMode);
            }

            var sourceContentQuantity = PositiveOrDefault(request.SourceContentQuantity, 1m);
            var theoreticalSourceQuantity =
                activeAmountInStrengthUnit /
                request.SourceStrengthValue.Value *
                sourceContentQuantity;

            if (theoreticalSourceQuantity <= 0)
                return CompoundCalculationResult.Fail(
                    "Hasil jumlah sumber teoritis tidak valid.",
                    ingredientMode);

            theoreticalSourceQuantity = RoundClinical(theoreticalSourceQuantity);
            var calculatedPricingQuantity = ResolvePricingQuantity(
                request.VerifiedSourceQuantity,
                theoreticalSourceQuantity,
                request.AllowFractionalSource);
            var amountPerPackage = RoundClinical(theoreticalSourceQuantity / totalPackage);

            var warnings = new List<string>();
            if (request.RequiresManualStrengthVerification)
            {
                warnings.Add(
                    "Strength obat terdeteksi memiliki beberapa komponen; hasil wajib diverifikasi farmasi.");
            }

            if (!request.AllowFractionalSource && calculatedPricingQuantity > theoreticalSourceQuantity)
            {
                warnings.Add(
                    $"Quantity tarif dibulatkan ke atas dari {Format(theoreticalSourceQuantity)} menjadi {Format(calculatedPricingQuantity)} karena sediaan tidak mengizinkan pecahan.");
            }

            var note =
                $"{formulaText} = {Format(calculatedActiveAmount)} {DisplayUnit(calculatedActiveUnit)}; " +
                $"sumber teoritis {Format(theoreticalSourceQuantity)} {DisplayUnit(request.SourceContentUnit)}.";

            return CompoundCalculationResult.Success(
                ingredientMode,
                amountPerPackage,
                theoreticalSourceQuantity,
                RoundClinical(calculatedActiveAmount),
                calculatedActiveUnit,
                theoreticalSourceQuantity,
                calculatedPricingQuantity,
                warnings.Count > 0 ? "ReadyWithWarning" : "Ready",
                note,
                warnings);
        }

        public static CompoundIngredientCalculationMode ResolveIngredientMode(
            CompoundCalculationMode compoundMode,
            CompoundIngredientCalculationMode requestedMode,
            CompoundIngredientRole role,
            bool isQuantitySufficientToFinal = false)
        {
            if (isQuantitySufficientToFinal &&
                role is CompoundIngredientRole.Vehicle or CompoundIngredientRole.Excipient)
            {
                return CompoundIngredientCalculationMode.QuantitySufficient;
            }

            if (role == CompoundIngredientRole.Vehicle || role == CompoundIngredientRole.Excipient)
            {
                if (requestedMode == CompoundIngredientCalculationMode.QuantitySufficient ||
                    requestedMode == CompoundIngredientCalculationMode.ManualSourceQuantity)
                    return requestedMode;
            }

            if (requestedMode != CompoundIngredientCalculationMode.LegacySourceQuantity)
                return requestedMode;

            return compoundMode switch
            {
                CompoundCalculationMode.DividedDose =>
                    CompoundIngredientCalculationMode.TargetDosePerUnit,
                CompoundCalculationMode.FinalWeight =>
                    CompoundIngredientCalculationMode.TargetPercentage,
                CompoundCalculationMode.FinalVolume =>
                    CompoundIngredientCalculationMode.TargetConcentration,
                CompoundCalculationMode.Manual =>
                    CompoundIngredientCalculationMode.ManualSourceQuantity,
                _ => CompoundIngredientCalculationMode.LegacySourceQuantity
            };
        }

        private static decimal ResolvePricingQuantity(
            decimal? verifiedSourceQuantity,
            decimal theoreticalSourceQuantity,
            bool allowFractionalSource)
        {
            var raw = verifiedSourceQuantity.HasValue && verifiedSourceQuantity.Value > 0
                ? verifiedSourceQuantity.Value
                : theoreticalSourceQuantity;

            if (raw <= 0)
                raw = 1m;

            return allowFractionalSource
                ? RoundClinical(raw)
                : Math.Ceiling(raw);
        }

        private static bool TryConvert(
            decimal value,
            CompoundUnitDescriptor? from,
            CompoundUnitDescriptor? to,
            out decimal converted,
            out string? error)
        {
            converted = value;
            error = null;

            if (from == null || to == null)
            {
                error = "Satuan konversi belum lengkap.";
                return false;
            }

            if (from.Id.HasValue && to.Id.HasValue && from.Id == to.Id)
                return true;

            var fromToken = NormalizeUnitToken(from.Symbol ?? from.Name);
            var toToken = NormalizeUnitToken(to.Symbol ?? to.Name);

            if (!string.IsNullOrWhiteSpace(fromToken) && fromToken == toToken)
                return true;

            var fromInfo = ResolveConvertibleUnit(fromToken);
            var toInfo = ResolveConvertibleUnit(toToken);

            if (fromInfo == null || toInfo == null ||
                !string.Equals(fromInfo.Dimension, toInfo.Dimension, StringComparison.Ordinal))
            {
                error = $"Satuan {DisplayUnit(from)} tidak kompatibel dengan {DisplayUnit(to)}.";
                return false;
            }

            converted = value * fromInfo.FactorToBase / toInfo.FactorToBase;
            return true;
        }

        private static ConvertibleUnit? ResolveConvertibleUnit(string? token)
        {
            return token switch
            {
                "ng" => new ConvertibleUnit("mass", 0.000001m),
                "mcg" or "ug" => new ConvertibleUnit("mass", 0.001m),
                "mg" => new ConvertibleUnit("mass", 1m),
                "g" or "gr" or "gram" => new ConvertibleUnit("mass", 1000m),
                "kg" => new ConvertibleUnit("mass", 1000000m),
                "ul" or "mcl" => new ConvertibleUnit("volume", 0.001m),
                "ml" => new ConvertibleUnit("volume", 1m),
                "cl" => new ConvertibleUnit("volume", 10m),
                "dl" => new ConvertibleUnit("volume", 100m),
                "l" or "liter" or "litre" => new ConvertibleUnit("volume", 1000m),
                "mmol" => new ConvertibleUnit("amount", 1m),
                "mol" => new ConvertibleUnit("amount", 1000m),
                "meq" => new ConvertibleUnit("equivalent", 1m),
                _ => null
            };
        }

        public static string? NormalizeUnitToken(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var token = value
                .Trim()
                .ToLowerInvariant()
                .Replace("µ", "u")
                .Replace("μ", "u")
                .Replace(".", string.Empty)
                .Replace(" ", string.Empty);

            return token switch
            {
                "microgram" or "micrograms" or "mikrogram" => "mcg",
                "milligram" or "milligrams" or "miligram" => "mg",
                "gram" or "grams" => "g",
                "milliliter" or "milliliters" or "millilitre" or "mililiter" => "ml",
                "liter" or "liters" or "litre" => "l",
                "capsule" or "capsules" or "caps" => "kapsul",
                "tab" or "tabs" => "tablet",
                "ampoule" or "amp" => "ampul",
                _ => token
            };
        }

        private static decimal PositiveOrDefault(decimal value, decimal fallback)
            => value > 0 ? value : fallback;

        private static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0m;
            return decimal.TryParse(
                value.Replace(',', '.'),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var result)
                ? result
                : 0m;
        }

        private static decimal RoundClinical(decimal value)
            => Math.Round(value, 4, MidpointRounding.AwayFromZero);

        private static string Format(decimal value)
            => value.ToString("0.####", CultureInfo.InvariantCulture);

        private static string DisplayUnit(CompoundUnitDescriptor? unit)
            => unit == null
                ? "unit"
                : !string.IsNullOrWhiteSpace(unit.Symbol)
                    ? unit.Symbol!
                    : !string.IsNullOrWhiteSpace(unit.Name)
                        ? unit.Name!
                        : "unit";

        private sealed class ConvertibleUnit
        {
            public ConvertibleUnit(string dimension, decimal factorToBase)
            {
                Dimension = dimension;
                FactorToBase = factorToBase;
            }

            public string Dimension { get; }
            public decimal FactorToBase { get; }
        }
    }

    public sealed class CompoundCalculationRequest
    {
        public CompoundCalculationMode CompoundCalculationMode { get; set; }
        public CompoundIngredientCalculationMode IngredientCalculationMode { get; set; }
        public CompoundIngredientRole IngredientRole { get; set; } = CompoundIngredientRole.ActiveIngredient;
        public decimal TotalPackage { get; set; } = 1m;
        public decimal? FinalQuantity { get; set; }
        public CompoundUnitDescriptor? FinalQuantityUnit { get; set; }
        public decimal LegacyAmountPerPackage { get; set; } = 1m;
        public decimal LegacyTotalQuantity { get; set; } = 1m;
        public decimal? TargetValue { get; set; }
        public CompoundUnitDescriptor? TargetUnit { get; set; }
        public string? TargetConcentrationUnit { get; set; }
        public decimal? SourceStrengthValue { get; set; }
        public CompoundUnitDescriptor? SourceStrengthUnit { get; set; }
        public decimal SourceContentQuantity { get; set; } = 1m;
        public CompoundUnitDescriptor? SourceContentUnit { get; set; }
        public decimal? VerifiedSourceQuantity { get; set; }
        public bool AllowFractionalSource { get; set; }
        public bool IsQuantitySufficientToFinal { get; set; }
        public bool RequiresManualStrengthVerification { get; set; }
    }

    public sealed class CompoundCalculationResult
    {
        public bool IsValid { get; private set; }
        public string? ErrorMessage { get; private set; }
        public CompoundIngredientCalculationMode IngredientCalculationMode { get; private set; }
        public decimal AmountPerPackage { get; private set; }
        public decimal TotalQuantity { get; private set; }
        public decimal? CalculatedActiveAmount { get; private set; }
        public CompoundUnitDescriptor? CalculatedActiveUnit { get; private set; }
        public decimal? TheoreticalSourceQuantity { get; private set; }
        public decimal PricingQuantity { get; private set; }
        public string CalculationStatus { get; private set; } = "Invalid";
        public string? CalculationNote { get; private set; }
        public List<string> Warnings { get; private set; } = new();

        public static CompoundCalculationResult Fail(
            string message,
            CompoundIngredientCalculationMode mode = CompoundIngredientCalculationMode.LegacySourceQuantity)
            => new()
            {
                IsValid = false,
                ErrorMessage = message,
                IngredientCalculationMode = mode,
                CalculationStatus = "Invalid",
                CalculationNote = message
            };

        public static CompoundCalculationResult Success(
            CompoundIngredientCalculationMode mode,
            decimal amountPerPackage,
            decimal totalQuantity,
            decimal? calculatedActiveAmount,
            CompoundUnitDescriptor? calculatedActiveUnit,
            decimal? theoreticalSourceQuantity,
            decimal pricingQuantity,
            string status,
            string? note,
            IEnumerable<string>? warnings = null)
            => new()
            {
                IsValid = true,
                IngredientCalculationMode = mode,
                AmountPerPackage = amountPerPackage,
                TotalQuantity = totalQuantity,
                CalculatedActiveAmount = calculatedActiveAmount,
                CalculatedActiveUnit = calculatedActiveUnit,
                TheoreticalSourceQuantity = theoreticalSourceQuantity,
                PricingQuantity = pricingQuantity,
                CalculationStatus = status,
                CalculationNote = note,
                Warnings = warnings?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList()
                    ?? new List<string>()
            };
    }

    public sealed class CompoundUnitDescriptor
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }

        public static CompoundUnitDescriptor? Create(Guid? id, string? name, string? symbol)
        {
            if ((!id.HasValue || id.Value == Guid.Empty) &&
                string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(symbol))
                return null;

            return new CompoundUnitDescriptor
            {
                Id = id.HasValue && id.Value != Guid.Empty ? id : null,
                Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim(),
                Symbol = string.IsNullOrWhiteSpace(symbol) ? null : symbol.Trim()
            };
        }
    }

    public sealed class ParsedDrugStrength
    {
        public static ParsedDrugStrength Empty => new();
        public string? RawText { get; set; }
        public bool IsParsed { get; set; }
        public decimal? StrengthValue { get; set; }
        public string? StrengthUnitToken { get; set; }
        public decimal SourceContentQuantity { get; set; } = 1m;
        public string? SourceContentUnitToken { get; set; }
        public bool RequiresManualVerification { get; set; }
        public string? Warning { get; set; }
    }
}
