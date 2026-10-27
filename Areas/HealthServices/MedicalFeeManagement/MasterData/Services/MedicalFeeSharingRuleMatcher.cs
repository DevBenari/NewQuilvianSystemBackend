using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Services
{
    /// <summary>
    /// Status hasil pencocokan baris aturan tarif bagi hasil dokter (MDF-DES-020).
    /// </summary>
    public enum MedicalFeeSharingRuleMatchStatus
    {
        /// <summary>Ditemukan tepat satu baris aturan tarif yang sah dan paling spesifik.</summary>
        Matched,

        /// <summary>Tidak ditemukan baris aturan tarif yang berlaku (keadaan bisnis normal).</summary>
        NoRule,

        /// <summary>Ditemukan lebih dari satu baris aturan tarif pada tingkat kekhususan tertinggi (konflik konfigurasi data).</summary>
        Overlap
    }

    /// <summary>
    /// Hasil resolusi pemilihan baris aturan tarif bagi hasil dokter (MDF-DES-020).
    /// </summary>
    public sealed record MedicalFeeSharingRuleMatchResult(
        MedicalFeeSharingRuleMatchStatus Status,
        MdfSharingRule? Rule = null,
        int? Specificity = null,
        int? ConflictCount = null,
        string? ReasonCode = null,
        string? Message = null)
    {
        public static MedicalFeeSharingRuleMatchResult Matched(MdfSharingRule rule, int specificity) =>
            new(MedicalFeeSharingRuleMatchStatus.Matched, rule, specificity, ReasonCode: "RESOLVED");

        public static MedicalFeeSharingRuleMatchResult NoRule(string reasonCode, string message) =>
            new(MedicalFeeSharingRuleMatchStatus.NoRule, ReasonCode: reasonCode, Message: message);

        public static MedicalFeeSharingRuleMatchResult Overlap(int conflictCount, int specificity, string message) =>
            new(MedicalFeeSharingRuleMatchStatus.Overlap, Specificity: specificity, ConflictCount: conflictCount, ReasonCode: "MDF_RULE_OVERLAP", Message: message);
    }

    /// <summary>
    /// Komponen bersama untuk pencocokan baris aturan tarif bagi hasil dokter (MDF-DES-020, MDF-INTEGRATION-1.1 §4.4).
    /// Digunakan secara identik oleh:
    /// 1. Port in-process Billing <c>MedicalFeeDoctorShareResolver</c> (<c>IBillingDoctorShareResolver</c>)
    /// 2. Mesin perhitungan jasa medis bulanan <c>MedicalFeeCalculationService</c>
    ///
    /// Menegakkan hierarki tingkat kekhususan (Specificity):
    /// 4 = Tarif spesifik (<c>TariffId</c> cocok persis)
    /// 3 = Kategori tarif (<c>TariffCategoryId</c> cocok)
    /// 2 = Bawaan peran (<c>TariffId</c> dan <c>TariffCategoryId</c> kosong)
    /// 1 = Tidak cocok
    /// </summary>
    public class MedicalFeeSharingRuleMatcher
    {
        /// <summary>
        /// Mencocokkan baris aturan tarif yang paling spesifik untuk satu peran, tanggal layanan, dan lingkup tarif.
        /// </summary>
        public MedicalFeeSharingRuleMatchResult MatchRule(
            IEnumerable<MdfSharingRule> rules,
            Guid roleId,
            DateOnly serviceDate,
            Guid? tariffId,
            Guid? tariffCategoryId,
            string? roleCode = null,
            string? serviceDescription = null,
            string? agreementNumber = null)
        {
            var candidateRules = rules
                .Where(r => !r.IsDelete &&
                            r.RoleId == roleId &&
                            r.EffectiveStart <= serviceDate &&
                            (r.EffectiveEnd == null || r.EffectiveEnd >= serviceDate))
                .ToList();

            if (candidateRules.Count == 0)
            {
                var roleLabel = !string.IsNullOrWhiteSpace(roleCode) ? $"'{roleCode}'" : $"ID {roleId}";
                var agreementLabel = !string.IsNullOrWhiteSpace(agreementNumber) ? $" pada kesepakatan {agreementNumber}" : string.Empty;
                return MedicalFeeSharingRuleMatchResult.NoRule(
                    "RULE_MISSING",
                    $"Tidak ditemukan aturan tarif untuk peran {roleLabel}{agreementLabel} per {serviceDate}.");
            }

            var ruleGroups = candidateRules
                .Select(r => new
                {
                    Rule = r,
                    Specificity = r.TariffId.HasValue && tariffId.HasValue && r.TariffId == tariffId ? 4 :
                                  r.TariffCategoryId.HasValue && tariffCategoryId.HasValue && r.TariffCategoryId == tariffCategoryId ? 3 :
                                  !r.TariffId.HasValue && !r.TariffCategoryId.HasValue ? 2 : 1
                })
                .Where(x => x.Specificity > 1 || (!candidateRules.Any(cr => cr.TariffId.HasValue || cr.TariffCategoryId.HasValue)))
                .OrderByDescending(x => x.Specificity)
                .ToList();

            if (ruleGroups.Count == 0)
            {
                var descLabel = !string.IsNullOrWhiteSpace(serviceDescription) ? $" untuk layanan '{serviceDescription}'." : ".";
                return MedicalFeeSharingRuleMatchResult.NoRule(
                    "RULE_MISSING",
                    $"Tidak ada baris tarif dengan lingkup tindakan yang cocok{descLabel}");
            }

            var topSpecificity = ruleGroups[0].Specificity;
            var bestMatches = ruleGroups.Where(x => x.Specificity == topSpecificity).Select(x => x.Rule).ToList();

            if (bestMatches.Count > 1)
            {
                var roleLabel = !string.IsNullOrWhiteSpace(roleCode) ? $"'{roleCode}'" : $"ID {roleId}";
                return MedicalFeeSharingRuleMatchResult.Overlap(
                    bestMatches.Count,
                    topSpecificity,
                    $"MDF_RULE_OVERLAP: Ditemukan {bestMatches.Count} baris aturan tarif yang bertabrakan pada tingkat kekhususan {topSpecificity} untuk peran {roleLabel} pada tanggal {serviceDate}.");
            }

            return MedicalFeeSharingRuleMatchResult.Matched(bestMatches[0], topSpecificity);
        }
    }
}

