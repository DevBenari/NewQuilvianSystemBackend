using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Services
{
    /// <summary>
    /// Implementasi port in-process <see cref="IBillingDoctorShareResolver"/> untuk modul Billing (BE-MDF-016).
    /// Menjawab persentase hak jasa dokter/nakes ke Billing saat pencatatan tagihan (MDF-INTEGRATION-1.1 §4, MDF-DES-019).
    /// Mematuhi 7 kewajiban teknis integrasi:
    /// 1. Tidak pernah melempar exception: seluruh kesalahan/pembatalan dikonversi menjadi Unavailable.
    /// 2. Meneruskan CancellationToken ke seluruh kueri basis data.
    /// 3. Read-only: AsNoTracking, tanpa SaveChanges, tanpa membuka transaksi.
    /// 4. Paling banyak 3 kueri terindeks: Peran, Kesepakatan (beserta baris &amp; kontrak), dan Kategori Tarif.
    /// 5. Log hanya mencatat Outcome, SharingRuleId, dan ReasonCode tanpa data nominal/nama tenaga medis.
    /// 6. Didaftarkan melalui Replace di Program.cs setelah AddBillingManagement().
    /// 7. Tidak membuka endpoint HTTP apa pun.
    /// </summary>
    public class MedicalFeeDoctorShareResolver : IBillingDoctorShareResolver
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly MedicalFeeSharingRuleMatcher _ruleMatcher;
        private readonly ILogger<MedicalFeeDoctorShareResolver> _logger;

        public MedicalFeeDoctorShareResolver(
            ApplicationDbContext dbContext,
            MedicalFeeSharingRuleMatcher ruleMatcher,
            ILogger<MedicalFeeDoctorShareResolver> logger)
        {
            _dbContext = dbContext;
            _ruleMatcher = ruleMatcher;
            _logger = logger;
        }

        /// <summary>
        /// Menyelesaikan persentase bagi hasil dokter/nakes berdasarkan kueri layanan berjasa dari Billing.
        /// Mengikuti langkah 1-7 sesuai MDF-INTEGRATION-1.1 §4.4.
        /// </summary>
        public async Task<BillingDoctorShareResolution> ResolveAsync(
            BillingDoctorShareQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Kewajiban 2: Periksa cancellation token segera
                cancellationToken.ThrowIfCancellationRequested();

                // Langkah 1: Petakan PerformerType ke jenis penerima hak jasa Medical Fee (MDF-INTEGRATION-1.1 §4.4)
                string payeeType;
                if (string.Equals(query.PerformerType, "DOCTOR", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(query.PerformerType, MdfPayeeTypes.Doctor, StringComparison.OrdinalIgnoreCase))
                {
                    payeeType = MdfPayeeTypes.Doctor;
                }
                else if (string.Equals(query.PerformerType, "WORKFORCE", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(query.PerformerType, MdfPayeeTypes.Workforce, StringComparison.OrdinalIgnoreCase))
                {
                    payeeType = MdfPayeeTypes.Workforce;
                }
                else
                {
                    // T-98: PerformerType selain DOCTOR/WORKFORCE -> Unavailable
                    _logger.LogWarning(
                        "Medical fee doctor share resolution rejected. Outcome: {Outcome}, ReasonCode: {ReasonCode}",
                        BillingDoctorShareOutcomes.Unavailable, "INVALID_PERFORMER_TYPE");
                    return BillingDoctorShareResolution.Unavailable();
                }

                // Langkah 2: Tentukan peran pelaksana (Kueri 1)
                MstMedicalFeeRole? role;
                if (string.IsNullOrWhiteSpace(query.RoleCode))
                {
                    // T-93: RoleCode kosong -> cari peran utama aktif (IsPrimaryRole = true)
                    role = await _dbContext.MstMedicalFeeRoles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => !r.IsDelete && r.IsActive && r.IsPrimaryRole, cancellationToken);
                }
                else
                {
                    // T-94: RoleCode terisi -> cari peran aktif berdasarkan RoleCode
                    var normalizedRoleCode = query.RoleCode.Trim();
                    role = await _dbContext.MstMedicalFeeRoles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => !r.IsDelete && r.IsActive && r.RoleCode == normalizedRoleCode, cancellationToken);
                }

                if (role == null)
                {
                    // T-93 / T-94: Peran tidak ditemukan atau tidak aktif -> NoRule
                    _logger.LogInformation(
                        "Medical fee doctor share resolution finished. Outcome: {Outcome}, ReasonCode: {ReasonCode}",
                        BillingDoctorShareOutcomes.NoRule, "ROLE_UNMAPPED_OR_INACTIVE");
                    return BillingDoctorShareResolution.NoRule();
                }

                // Langkah 3 & 4: Cari kesepakatan aktif dan evaluasi masa berlaku kontrak HR (Kueri 2)
                var agreement = await _dbContext.MdfSharingAgreements
                    .AsNoTracking()
                    .Include(a => a.SharingRules.Where(sr => !sr.IsDelete &&
                                                             sr.RoleId == role.Id &&
                                                             sr.EffectiveStart <= query.ServiceDate &&
                                                             (sr.EffectiveEnd == null || sr.EffectiveEnd >= query.ServiceDate)))
                    .Include(a => a.SourceContractHistory)
                    .FirstOrDefaultAsync(a => !a.IsDelete &&
                                              a.PayeeType == payeeType &&
                                              a.PayeeReferenceId == query.PerformerReferenceId &&
                                              a.Status == MdfSharingAgreementStatuses.Active &&
                                              a.EffectiveStart <= query.ServiceDate &&
                                              (a.EffectiveEnd == null || a.EffectiveEnd >= query.ServiceDate),
                                         cancellationToken);

                if (agreement == null)
                {
                    // T-95: Penerima tanpa kesepakatan aktif pada tanggal layanan -> NoRule
                    _logger.LogInformation(
                        "Medical fee doctor share resolution finished. Outcome: {Outcome}, ReasonCode: {ReasonCode}",
                        BillingDoctorShareOutcomes.NoRule, "AGREEMENT_MISSING");
                    return BillingDoctorShareResolution.NoRule();
                }

                // Langkah 4: Periksa masa berlaku kontrak HR yang ditunjuk kesepakatan (MDF-DES-008)
                var contract = agreement.SourceContractHistory;
                if (contract != null && contract.EndDate.HasValue && query.ServiceDate > DateOnly.FromDateTime(contract.EndDate.Value))
                {
                    // T-96: Kontrak HR telah berakhir sebelum tanggal layanan -> NoRule
                    _logger.LogInformation(
                        "Medical fee doctor share resolution finished. Outcome: {Outcome}, ReasonCode: {ReasonCode}",
                        BillingDoctorShareOutcomes.NoRule, "CONTRACT_EXPIRED");
                    return BillingDoctorShareResolution.NoRule();
                }

                // Langkah 5: Cari kategori tarif jika TariffId terisi (Kueri 3)
                Guid? tariffCategoryId = null;
                if (query.TariffId != Guid.Empty)
                {
                    tariffCategoryId = await _dbContext.MstTariffs
                        .AsNoTracking()
                        .Where(t => !t.IsDelete && t.Id == query.TariffId)
                        .Select(t => (Guid?)t.TariffCategoryId)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                // Langkah 5 & 6: Cocokkan aturan tarif memakai komponen bersama MedicalFeeSharingRuleMatcher (MDF-DES-020)
                var matchResult = _ruleMatcher.MatchRule(
                    agreement.SharingRules,
                    role.Id,
                    query.ServiceDate,
                    query.TariffId,
                    tariffCategoryId,
                    role.RoleCode,
                    agreementNumber: agreement.AgreementNumber);

                if (matchResult.Status == MedicalFeeSharingRuleMatchStatus.NoRule)
                {
                    _logger.LogInformation(
                        "Medical fee doctor share resolution finished. Outcome: {Outcome}, ReasonCode: {ReasonCode}",
                        BillingDoctorShareOutcomes.NoRule, "RULE_MISSING");
                    return BillingDoctorShareResolution.NoRule();
                }

                if (matchResult.Status == MedicalFeeSharingRuleMatchStatus.Overlap)
                {
                    // T-97: Dua baris sama khusus bertabrakan -> Unavailable, catat log peringatan
                    _logger.LogWarning(
                        "Medical fee doctor share resolution rule overlap detected. Outcome: {Outcome}, ReasonCode: {ReasonCode}",
                        BillingDoctorShareOutcomes.Unavailable, "MDF_RULE_OVERLAP");
                    return BillingDoctorShareResolution.Unavailable();
                }

                // Langkah 7: Baris tarif terpilih (T-91, T-92) -> Resolved
                var matchedRule = matchResult.Rule!;
                _logger.LogInformation(
                    "Medical fee doctor share resolution succeeded. Outcome: {Outcome}, SharingRuleId: {SharingRuleId}, ReasonCode: {ReasonCode}",
                    BillingDoctorShareOutcomes.Resolved, matchedRule.Id, "RESOLVED");

                return BillingDoctorShareResolution.Resolved(matchedRule.SharingPercentage, matchedRule.Id);
            }
            catch (OperationCanceledException)
            {
                // T-99 / Kewajiban 1: Token dibatalkan atau habis waktu -> Unavailable tanpa melempar exception
                _logger.LogWarning(
                    "Medical fee doctor share resolution cancelled. Outcome: {Outcome}, ReasonCode: {ReasonCode}",
                    BillingDoctorShareOutcomes.Unavailable, "OPERATION_CANCELLED");
                return BillingDoctorShareResolution.Unavailable();
            }
            catch (Exception ex)
            {
                // T-99 / Kewajiban 1: Kesalahan basis data atau sistem -> Unavailable tanpa melempar exception
                _logger.LogError(
                    ex,
                    "Medical fee doctor share resolution encountered an unhandled error. Outcome: {Outcome}, ReasonCode: {ReasonCode}",
                    BillingDoctorShareOutcomes.Unavailable, "UNEXPECTED_ERROR");
                return BillingDoctorShareResolution.Unavailable();
            }
        }
    }
}

