using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Kelengkapan dokumen admisi, lencana menu, dan peringatan jatuh tempo — <c>BE-RWI-195</c>,
    /// <c>RWI-DEC-234</c>, <c>250</c>, <c>256</c>, <c>260</c>, state matrix 10.2.
    /// </summary>
    /// <remarks>
    /// <b>Selalu wajib:</b> Serah Terima, Gelang (dicetak minimal sekali), IPD (dicetak minimal sekali),
    /// Nilai Kepercayaan. <b>Wajib bersyarat:</b> Selisih Biaya bila penjamin utama asuransi atau
    /// perusahaan; Pelunasan Deposit bila Billing mencatat kekurangan lebih dari 0. <b>Tidak dihitung:</b>
    /// Permintaan Privasi, Label, General Consent (<i>fail-closed</i>), dan Estimasi Biaya sampai
    /// <c>EPIC-RWA-09</c> dikirim (<c>BE-RWI-203</c>).
    ///
    /// <para>
    /// Sumber aturan yang gagal dibaca membuat butirnya "tidak dapat dihitung": tidak ikut pembilang
    /// maupun penyebut. Kelengkapan <b>hanya memperingatkan</b>; tidak menahan apa pun.
    /// </para>
    ///
    /// <para>
    /// Contoh: Tn. Budi (asuransi, deposit kurang Rp 3.000.000, tanpa operasi) wajib 6 dokumen;
    /// Ny. Wati (tunai, deposit cukup) wajib 4.
    /// </para>
    /// </remarks>
    public sealed class InpAdmissionCompletenessEvaluator
    {
        public const string MenuGeneralConsent = "GeneralConsent";
        public const string MenuNewPatientHandover = "NewPatientHandover";
        public const string MenuWristbandAndLabel = "WristbandAndLabel";
        public const string MenuPrivacyRequest = "PrivacyRequest";
        public const string MenuInpatientBaseData = "InpatientBaseData";
        public const string MenuDepositSettlement = "DepositSettlementStatement";
        public const string MenuCostDifference = "CostDifferenceStatement";
        public const string MenuBeliefValues = "BeliefValues";

        public const string UncountableWarning = "Kelengkapan dokumen admisi tidak dapat dihitung";

        private static readonly InpAdmissionDocumentStatus[] ActiveStatuses =
        {
            InpAdmissionDocumentStatus.Draft,
            InpAdmissionDocumentStatus.AwaitingSignature,
            InpAdmissionDocumentStatus.Completed
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly InpAdmissionSourceReader _sourceReader;

        public InpAdmissionCompletenessEvaluator(
            ApplicationDbContext dbContext,
            InpAdmissionSourceReader sourceReader)
        {
            _dbContext = dbContext;
            _sourceReader = sourceReader;
        }

        /// <summary>
        /// Menghitung kelengkapan satu episode. Penjamin dan deposit boleh diberikan pemanggil yang
        /// sudah membacanya, supaya sumber yang sama tidak dibaca dua kali.
        /// </summary>
        public async Task<InpAdmissionCompleteness> EvaluateAsync(
            Guid episodeId,
            Guid encounterId,
            InpAdmissionSourceResult<EncounterInsuranceContext>? guarantor = null,
            InpAdmissionSourceResult<InpEpisodeDepositSummaryDto>? deposit = null,
            CancellationToken cancellationToken = default)
        {
            guarantor ??= await _sourceReader.GetGuarantorAsync(encounterId, cancellationToken);
            deposit ??= await _sourceReader.GetDepositSummaryAsync(episodeId, cancellationToken);

            var documents = await _dbContext.Set<InpAdmissionDocument>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && ActiveStatuses.Contains(x.Status))
                .Select(x => new
                {
                    x.Id,
                    x.DocumentType,
                    x.Status,
                    DueAt = x.DepositStatement != null ? x.DepositStatement.DueAt : null
                })
                .ToListAsync(cancellationToken);

            var printedKinds = await _dbContext.Set<InpAdmissionPrintLog>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .Select(x => x.PrintKind)
                .Distinct()
                .ToListAsync(cancellationToken);

            var result = new InpAdmissionCompleteness
            {
                GuarantorState = guarantor.State,
                DepositState = deposit.State
            };

            InpAdmissionMenuState DocumentMenu(string key, string label, InpAdmissionDocumentType type, bool? isRequired)
            {
                var active = documents.FirstOrDefault(x => x.DocumentType == type);
                var menu = new InpAdmissionMenuState
                {
                    Key = key,
                    Label = label,
                    ActiveDocumentId = active?.Id,
                    IsRequired = isRequired == true,
                    IsUncountable = isRequired == null,
                    IsCompleted = active?.Status == InpAdmissionDocumentStatus.Completed
                };

                menu.Badge = isRequired == null
                    ? "Uncountable"
                    : active == null
                        ? (isRequired == true || type == InpAdmissionDocumentType.PrivacyRequest ? "NotCreated" : "NotRequired")
                        : active.Status switch
                        {
                            InpAdmissionDocumentStatus.Completed => "Completed",
                            InpAdmissionDocumentStatus.AwaitingSignature => "AwaitingSignature",
                            _ => "Draft"
                        };

                return menu;
            }

            InpAdmissionMenuState PrintMenu(string key, string label, bool printed)
                => new()
                {
                    Key = key,
                    Label = label,
                    IsRequired = true,
                    IsCompleted = printed,
                    Badge = printed ? "Printed" : "NotPrinted"
                };

            // Aturan bersyarat. null = sumber aturan gagal dibaca → tidak dapat dihitung.
            bool? costDifferenceRequired = guarantor.IsAvailable
                ? guarantor.Value!.PaymentType is EncounterPaymentType.Insurance or EncounterPaymentType.CompanyGuarantor
                : null;

            bool? depositRequired = deposit.IsAvailable
                ? deposit.Value!.PolicyShortfallAmount > 0
                : null;

            var wristbandPrinted = printedKinds.Contains(InpAdmissionPrintKind.AdultWristband) ||
                printedKinds.Contains(InpAdmissionPrintKind.InfantWristband);

            // Urutan navigasi V1 tanpa Assessment Edukasi dan MP Benefit; Estimasi Biaya tidak
            // tampil sampai EPIC-RWA-09 dikirim (frontend 14.2).
            result.Menus.Add(new InpAdmissionMenuState
            {
                Key = MenuGeneralConsent,
                Label = "General Consent",
                Badge = "PrintOnly",
                IsRequired = false
            });
            result.Menus.Add(DocumentMenu(MenuNewPatientHandover, "Serah Terima Pasien", InpAdmissionDocumentType.NewPatientHandover, true));
            result.Menus.Add(PrintMenu(MenuWristbandAndLabel, "Gelang & Label Pasien", wristbandPrinted));
            result.Menus.Add(DocumentMenu(MenuPrivacyRequest, "Permintaan Privasi", InpAdmissionDocumentType.PrivacyRequest, false));
            result.Menus.Add(PrintMenu(MenuInpatientBaseData, "IPD", printedKinds.Contains(InpAdmissionPrintKind.InpatientBaseData)));
            result.Menus.Add(DocumentMenu(MenuDepositSettlement, "Pelunasan Deposit", InpAdmissionDocumentType.DepositSettlementStatement, depositRequired));
            result.Menus.Add(DocumentMenu(MenuCostDifference, "Selisih Biaya", InpAdmissionDocumentType.CostDifferenceStatement, costDifferenceRequired));
            result.Menus.Add(DocumentMenu(MenuBeliefValues, "Nilai Kepercayaan", InpAdmissionDocumentType.BeliefValues, true));

            foreach (var menu in result.Menus)
            {
                if (menu.IsUncountable)
                {
                    result.UncountableCount++;
                    result.UncountableNames.Add(menu.Label);
                    continue;
                }

                if (!menu.IsRequired)
                {
                    continue;
                }

                result.RequiredCount++;

                if (menu.IsCompleted)
                {
                    result.CompletedCount++;
                }
                else
                {
                    result.MissingNames.Add(menu.Label);
                }
            }

            // RWI-DEC-260: jatuh tempo surat lengkap terlewati dan Billing masih mencatat kekurangan.
            var completedDeposit = documents.FirstOrDefault(x =>
                x.DocumentType == InpAdmissionDocumentType.DepositSettlementStatement &&
                x.Status == InpAdmissionDocumentStatus.Completed &&
                x.DueAt.HasValue);

            if (completedDeposit != null && deposit.IsAvailable &&
                deposit.Value!.PolicyShortfallAmount > 0 &&
                DateTime.UtcNow > completedDeposit.DueAt!.Value)
            {
                result.OverdueDeposit = new InpAdmissionOverdueDeposit(
                    completedDeposit.Id,
                    completedDeposit.DueAt.Value,
                    deposit.Value.PolicyShortfallAmount);
            }

            return result;
        }

        /// <summary>
        /// Peringatan tanpa rupiah untuk Detail Episode (<c>INT-RWA-13</c>). Kegagalan apa pun
        /// tidak menggagalkan detail episode; peringatannya menjadi "tidak dapat dihitung".
        /// </summary>
        public async Task<List<string>> BuildEpisodeWarningsAsync(
            Guid episodeId,
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var completeness = await EvaluateAsync(episodeId, encounterId, null, null, cancellationToken);
                var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);

                return BuildWarnings(completeness, InpAdmissionSourceReader.ResolveTimeZone(hospital));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return new List<string> { UncountableWarning };
            }
        }

        /// <summary>Kalimat peringatan validation 15.8, tanpa rupiah.</summary>
        public static List<string> BuildWarnings(InpAdmissionCompleteness completeness, TimeZoneInfo timeZone)
        {
            var warnings = new List<string>();

            if (completeness.MissingNames.Count > 0)
            {
                warnings.Add(
                    $"Dokumen admisi belum lengkap: {completeness.MissingNames.Count} ({string.Join(", ", completeness.MissingNames)})");
            }

            if (completeness.OverdueDeposit != null)
            {
                warnings.Add(
                    $"Pelunasan deposit jatuh tempo {InpAdmissionText.FormatShortDateTime(completeness.OverdueDeposit.DueAt, timeZone)} terlewati — lihat kasir");
            }

            if (completeness.UncountableCount > 0)
            {
                warnings.Add(UncountableWarning);
            }

            return warnings;
        }
    }

    /// <summary>Kelengkapan dokumen admisi satu episode.</summary>
    public sealed class InpAdmissionCompleteness
    {
        public List<InpAdmissionMenuState> Menus { get; } = new();

        public int CompletedCount { get; set; }

        public int RequiredCount { get; set; }

        public int UncountableCount { get; set; }

        public List<string> MissingNames { get; } = new();

        public List<string> UncountableNames { get; } = new();

        public InpAdmissionOverdueDeposit? OverdueDeposit { get; set; }

        public InpAdmissionSourceState GuarantorState { get; init; }

        public InpAdmissionSourceState DepositState { get; init; }
    }

    public sealed class InpAdmissionMenuState
    {
        public string Key { get; init; } = string.Empty;

        public string Label { get; init; } = string.Empty;

        public string Badge { get; set; } = string.Empty;

        public Guid? ActiveDocumentId { get; init; }

        public bool IsRequired { get; init; }

        public bool IsCompleted { get; init; }

        public bool IsUncountable { get; init; }
    }

    /// <summary>Surat Pelunasan Deposit lengkap yang jatuh temponya terlewati.</summary>
    public sealed record InpAdmissionOverdueDeposit(Guid DocumentId, DateTime DueAt, decimal CurrentShortfallAmount);
}
