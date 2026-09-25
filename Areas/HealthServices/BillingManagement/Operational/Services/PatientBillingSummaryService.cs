using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services
{
    /// <summary>
    /// Penyusun ringkasan tagihan satu perawatan rawat inap — <c>BE-RWI-126</c>,
    /// <c>FR-KEP-082</c>, <c>INT-KEP-14</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Hanya membaca.</b> Service ini tidak memanggil <c>SaveChanges</c>, tidak memakai tracking,
    /// dan tidak memanggil satu pun metode tulis milik Billing — kriteria 4. Angka deposit dibaca
    /// lewat <see cref="BillingDepositService.GetEpisodeDepositSummaryAsync"/> yang sudah ada,
    /// supaya kebijakan deposit minimum tidak dihitung dua kali dengan dua cara.
    /// </para>
    /// <para>
    /// <b>Total berjalan.</b> Dijumlah dari baris folio yang tidak berstatus <c>Superseded</c>,
    /// <c>Voided</c>, atau <c>Reversed</c>. Baris yang masih <c>Received</c>, <c>Evaluating</c>,
    /// <c>PendingFinancialReview</c>, atau belum punya <c>GrossAmount</c> dihitung sebagai
    /// <b>belum berharga</b>: jumlahnya disebut, dan totalnya ditandai belum lengkap. Contoh: tiga
    /// baris Rp 1.000.000, Rp 250.000, dan satu baris menunggu review → total Rp 1.250.000,
    /// <c>UnpricedChargeCount = 1</c>, <c>IsRunningTotalComplete = false</c>.
    /// </para>
    /// <para>
    /// <b>Item tidak ditanggung</b> dihitung dari butir pesanan klinis yang membawa penanda
    /// penjaminannya sendiri: tindakan rawat inap yang ditagih dan butir resep yang
    /// <c>IsCoverageApplicable</c>, keduanya dengan <c>IsCoveredByInsurance = false</c>. Pasien
    /// tunai tidak punya penjamin, sehingga nilainya <c>null</c>, bukan nol.
    /// </para>
    /// </remarks>
    public class PatientBillingSummaryService
    {
        private static readonly BillingChargeCalculationStatus[] StatusBarisTidakBerlaku =
        {
            BillingChargeCalculationStatus.Superseded,
            BillingChargeCalculationStatus.Voided,
            BillingChargeCalculationStatus.Reversed
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly BillingDepositService _depositService;

        public PatientBillingSummaryService(
            ApplicationDbContext dbContext,
            BillingDepositService depositService)
        {
            _dbContext = dbContext;
            _depositService = depositService;
        }

        /// <summary>Ringkasan satu episode, atau <c>null</c> bila episodenya tidak ditemukan.</summary>
        public async Task<PatientBillingSummaryResponse?> GetByEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.EncounterId })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
                return null;

            var penjamin = await _dbContext.RegPatientEncounterGuarantors
                .AsNoTracking()
                .Where(x => x.EncounterId == episode.EncounterId && x.IsActive && !x.IsDelete && !x.IsCancel)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Priority)
                .Select(x => new { x.PaymentType, x.PaymentSourceNameSnapshot })
                .FirstOrDefaultAsync(cancellationToken);

            var jenisBayar = penjamin?.PaymentType ?? EncounterPaymentType.Cash;
            var tunai = jenisBayar == EncounterPaymentType.Cash;

            var kelayakan = await _dbContext.BilInpatientClearanceHandoffs
                .AsNoTracking()
                .Where(x => x.EncounterId == episode.EncounterId && !x.IsDelete)
                .OrderByDescending(x => x.FinancialVersion)
                .Select(x => x.ClearanceStatus)
                .FirstOrDefaultAsync(cancellationToken);

            var folioId = await _dbContext.Set<BilFolio>()
                .AsNoTracking()
                .Where(x => x.EncounterId == episode.EncounterId && x.IsActive && !x.IsDelete)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            decimal? totalBerjalan = null;
            var belumBerharga = 0;

            if (folioId.HasValue)
            {
                var baris = await _dbContext.Set<BilChargeLine>()
                    .AsNoTracking()
                    .Where(x =>
                        x.FolioId == folioId.Value &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !StatusBarisTidakBerlaku.Contains(x.CalculationStatus))
                    .Select(x => new { x.CalculationStatus, x.GrossAmount })
                    .ToListAsync(cancellationToken);

                totalBerjalan = baris
                    .Where(x => x.GrossAmount.HasValue &&
                                x.CalculationStatus == BillingChargeCalculationStatus.Recognized)
                    .Sum(x => x.GrossAmount!.Value);

                belumBerharga = baris.Count(x =>
                    !x.GrossAmount.HasValue ||
                    x.CalculationStatus != BillingChargeCalculationStatus.Recognized);
            }

            var deposit = await _depositService.GetEpisodeDepositSummaryAsync(episode.Id, cancellationToken);

            int? tidakDitanggung = null;

            if (!tunai)
            {
                var tindakan = await _dbContext.Set<TrxPatientProcedure>()
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.InpEpisodeId == episode.Id &&
                        !x.IsDelete &&
                        x.IsBillable &&
                        x.ProcedureStatus != PatientProcedureStatus.Cancelled &&
                        !x.IsCoveredByInsurance,
                        cancellationToken);

                var butirResep = await _dbContext.Set<PhmPrescriptionItem>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.IsCoverageApplicable &&
                        !x.IsCoveredByInsurance)
                    .Join(
                        _dbContext.Set<PhmPrescription>().AsNoTracking().Where(p =>
                            p.InpEpisodeId == episode.Id &&
                            !p.IsDelete &&
                            !p.IsCancel &&
                            p.PrescriptionStatus != PrescriptionStatus.Cancelled),
                        item => item.PrescriptionId,
                        resep => resep.Id,
                        (item, resep) => item.Id)
                    .CountAsync(cancellationToken);

                tidakDitanggung = tindakan + butirResep;
            }

            var lengkap = folioId.HasValue && belumBerharga == 0;

            return new PatientBillingSummaryResponse
            {
                EpisodeId = episode.Id,
                EncounterId = episode.EncounterId,
                GuarantorName = !string.IsNullOrWhiteSpace(penjamin?.PaymentSourceNameSnapshot)
                    ? penjamin!.PaymentSourceNameSnapshot!
                    : tunai ? "Tunai / Umum" : jenisBayar.ToString(),
                PaymentType = jenisBayar.ToString(),
                FinancialClearanceStatus = kelayakan,
                FinancialClearanceStatusLabel = kelayakan switch
                {
                    InpatientClearanceStatuses.Cleared => "Layak",
                    InpatientClearanceStatuses.Blocked => "Tertahan",
                    InpatientClearanceStatuses.Pending => "Menunggu penilaian",
                    InpatientClearanceStatuses.Revoked => "Dibatalkan",
                    _ => "Belum dinilai"
                },
                HasBillingFolio = folioId.HasValue,
                RunningTotalAmount = totalBerjalan,
                UnpricedChargeCount = belumBerharga,
                IsRunningTotalComplete = lengkap,
                HasDepositAccount = deposit.HasDepositAccount,
                DepositReceivedAmount = deposit.TotalReceived,
                DepositRemainingAmount = deposit.AvailableBalance,
                DepositShortfallAmount = deposit.PolicyShortfallAmount,
                NotCoveredItemCount = tidakDitanggung,
                Message = !folioId.HasValue
                    ? "Belum ada tagihan tercatat untuk perawatan ini."
                    : lengkap
                        ? "Ringkasan tagihan berjalan."
                        : $"Total berjalan belum lengkap: {belumBerharga} tagihan masih menunggu perhitungan Billing.",
                ReadAt = DateTime.UtcNow
            };
        }
    }
}
