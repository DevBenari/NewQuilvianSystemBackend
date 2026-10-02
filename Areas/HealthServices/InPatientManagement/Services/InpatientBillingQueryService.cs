using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Implementasi kueri status penagihan kasir untuk rawat inap.
    /// Memastikan pemisahan mutlak pandangan operasional non-finansial dari rincian uang (VAL-INT-006, RWI-DEC-160).
    /// </summary>
    public class InpatientBillingQueryService : IInpatientBillingQueryService
    {
        private static readonly BillingChargeCalculationStatus[] InactiveCalculationStatuses =
        {
            BillingChargeCalculationStatus.Superseded,
            BillingChargeCalculationStatus.Voided,
            BillingChargeCalculationStatus.Reversed
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly BillingDepositService _depositService;

        public InpatientBillingQueryService(
            ApplicationDbContext dbContext,
            BillingDepositService depositService)
        {
            _dbContext = dbContext;
            _depositService = depositService;
        }

        /// <inheritdoc />
        public async Task<InpatientBillingStatusResponseDto?> GetOperationalBillingStatusAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Include(x => x.Patient)
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return null;
            }

            var folio = await _dbContext.Set<BilFolio>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EncounterId == episode.EncounterId && x.IsActive && !x.IsDelete, cancellationToken);

            var folioStatus = folio != null ? folio.Status.ToString().ToUpperInvariant() : "NONE";
            var clearanceStatus = episode.ClearanceStatus.ToString().ToUpperInvariant();

            bool canPhysicallyDischarge;
            string statusText;
            string statusColor;
            var blockerReasons = new List<string>();

            if (episode.ClearanceStatus == BillingClearanceStatus.Cleared)
            {
                canPhysicallyDischarge = true;
                statusText = "Clearance Disetujui (Lunas/Dijamin)";
                statusColor = "green";
            }
            else if (episode.ClearanceStatus == BillingClearanceStatus.Overridden || episode.IsSupervisorOverridden)
            {
                canPhysicallyDischarge = true;
                statusText = "Disetujui Melalui Supervisor Override";
                statusColor = "purple";
            }
            else if (episode.ClearanceStatus == BillingClearanceStatus.Revoked)
            {
                canPhysicallyDischarge = false;
                statusText = "Clearance Dicabut (Auto-Reblock)";
                statusColor = "red";
                var reason = !string.IsNullOrWhiteSpace(episode.ClearanceRevokedReason)
                    ? episode.ClearanceRevokedReason
                    : "Pencabutan clearance oleh kasir akibat tagihan susulan belum diselesaikan.";
                blockerReasons.Add($"Persetujuan kasir dicabut: {reason}");
            }
            else if (episode.ClearanceStatus == BillingClearanceStatus.Pending)
            {
                canPhysicallyDischarge = false;
                statusText = "Menunggu Penyelesaian Kasir";
                statusColor = "amber";
                blockerReasons.Add("Keluarga pasien belum menyelesaikan administrasi pelunasan di kasir utama");
            }
            else
            {
                canPhysicallyDischarge = false;
                statusText = "Belum Ada Pengajuan Kepulangan";
                statusColor = "gray";
                blockerReasons.Add("Pasien masih dalam perawatan aktif, instruksi kepulangan belum diterbitkan DPJP");
            }

            return new InpatientBillingStatusResponseDto
            {
                EpisodeId = episode.Id,
                EncounterId = episode.EncounterId.ToString(),
                PatientName = episode.Patient?.FullName ?? string.Empty,
                MedicalRecordNumber = episode.Patient?.MedicalRecordNumber ?? string.Empty,
                FolioStatus = folioStatus,
                ClearanceStatus = clearanceStatus,
                OperationalStatusText = statusText,
                StatusColor = statusColor,
                CanPhysicallyDischarge = canPhysicallyDischarge,
                BlockerReasons = blockerReasons,
                LastCheckedAtUtc = DateTime.UtcNow
            };
        }

        /// <inheritdoc />
        public async Task<InpatientBillingDetailsResponseDto?> GetFinancialDetailsAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return null;
            }

            var folio = await _dbContext.Set<BilFolio>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EncounterId == episode.EncounterId && x.IsActive && !x.IsDelete, cancellationToken);

            decimal totalCharges = 0;
            decimal coveredAmount = 0;
            var items = new List<BillingDetailItemDto>();

            if (folio != null)
            {
                var chargeLines = await _dbContext.Set<BilChargeLine>()
                    .AsNoTracking()
                    .Where(x =>
                        x.FolioId == folio.Id &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !InactiveCalculationStatuses.Contains(x.CalculationStatus))
                    .ToListAsync(cancellationToken);

                totalCharges = chargeLines
                    .Where(x => x.GrossAmount.HasValue && x.CalculationStatus == BillingChargeCalculationStatus.Recognized)
                    .Sum(x => x.GrossAmount!.Value);

                coveredAmount = chargeLines
                    .Where(x => x.EligibleAmount.HasValue && x.CalculationStatus == BillingChargeCalculationStatus.Recognized)
                    .Sum(x => x.EligibleAmount!.Value);

                foreach (var line in chargeLines)
                {
                    if (line.GrossAmount.HasValue && line.GrossAmount.Value > 0)
                    {
                        items.Add(new BillingDetailItemDto
                        {
                            Category = !string.IsNullOrWhiteSpace(line.SourceContext) ? line.SourceContext : "CHARGE",
                            Description = line.EffectType ?? "Biaya Layanan Rawat Inap",
                            Amount = line.GrossAmount.Value
                        });
                    }
                }
            }

            var deposit = await _depositService.GetEpisodeDepositSummaryAsync(episode.Id, cancellationToken);
            var depositPaid = deposit?.TotalReceived ?? 0;
            var patientExcess = Math.Max(0, totalCharges - coveredAmount);
            var outstandingAmount = Math.Max(0, patientExcess - depositPaid);

            return new InpatientBillingDetailsResponseDto
            {
                EpisodeId = episode.Id,
                EncounterId = episode.EncounterId.ToString(),
                TotalCharges = totalCharges,
                CoveredAmount = coveredAmount,
                PatientExcess = patientExcess,
                DepositPaid = depositPaid,
                OutstandingAmount = outstandingAmount,
                ClearanceStatus = episode.ClearanceStatus.ToString().ToUpperInvariant(),
                Items = items
            };
        }
    }
}
