using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Koreksi salah catat penempatan bed — <c>BE-RWI-154</c>, kontrak <c>integrasi-billing</c>
    /// <c>1.1.0</c> API 3.4, state 5.4, validasi <c>VAL-RWF-07</c> s.d. <c>VAL-RWF-12</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa ada gerbang invoice.</b> Tarif kamar dihitung Billing dari linimasa penempatan.
    /// Mengubah linimasa sesudah tagihan difinalkan akan membuat angka di layar kasir berbeda
    /// dari angka yang sudah ditagihkan. Karena itu koreksi hanya boleh selama invoice rawat inap
    /// masih <c>OPEN</c> menurut bacaan langsung Billing (<c>INT-RWF-03</c>). Invoice yang belum
    /// terbentuk (<c>NONE</c>) juga aman dikoreksi, karena belum ada satu pun angka yang ditagihkan.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Pasien tercatat masuk kelas 1 sejak 1 Okt 08.00, padahal sebenarnya menempati
    /// VIP. Kepala ruangan mengoreksi kelas penempatan itu menjadi VIP dengan alasan "Salah pilih
    /// kelas saat admisi". Baris kelas 1 tetap tersimpan tetapi ditandai sudah dikoreksi; baris VIP
    /// baru menggantikannya, dan Billing menghitung seluruh periode sebagai VIP — tidak ada periode
    /// kelas 1 yang ikut tertagih.
    /// </para>
    /// </remarks>
    public sealed class InpPlacementCorrectionService
    {
        private const string LogCategory = "HealthServices.InPatientManagement.PlacementCorrection";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpBedOccupancyService _bedOccupancyService;
        private readonly IInpBillingClearanceAdapter _billingClearanceAdapter;

        public InpPlacementCorrectionService(
            ApplicationDbContext dbContext,
            InpBedOccupancyService bedOccupancyService,
            IInpBillingClearanceAdapter billingClearanceAdapter)
        {
            _dbContext = dbContext;
            _bedOccupancyService = bedOccupancyService;
            _billingClearanceAdapter = billingClearanceAdapter;
        }

        public async Task<InpPlacementCorrectionResult> CorrectAsync(
            Guid placementId,
            CorrectPlacementRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Invalid, "Isian koreksi wajib diisi.");
            }

            // Alasan wajib dan bukan hanya tanda baca, mis. "..." atau "-".
            if (string.IsNullOrWhiteSpace(request.Reason) || !request.Reason.Any(char.IsLetterOrDigit))
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Invalid,
                    "Alasan koreksi wajib diisi dengan kalimat yang jelas.");
            }

            // VAL-RWF-11
            if (!request.CorrectedBedId.HasValue &&
                !request.CorrectedPatientClassId.HasValue &&
                !request.CorrectedStartDateTime.HasValue &&
                !request.CorrectedEndDateTime.HasValue)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Invalid,
                    "Pilih sekurang-kurangnya satu hal yang dikoreksi: bed, kelas, atau waktu.");
            }

            var encounterId = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x => x.Id == placementId && !x.IsDelete && x.Episode != null)
                .Select(x => (Guid?)x.Episode!.EncounterId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!encounterId.HasValue)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.NotFound, "Penempatan tidak ditemukan.");
            }

            // INT-RWF-03 — gerbang status invoice dari bacaan langsung Billing.
            var billing = await _billingClearanceAdapter.GetStatusAsync(encounterId.Value, cancellationToken);

            if (!billing.IsReadable)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.BusinessRuleRejected,
                    "Status tagihan tidak dapat dibaca. Koreksi penempatan belum dapat disimpan.",
                    InpPlacementCorrectionCodes.InvoiceUnreadable);
            }

            if (!string.Equals(billing.InvoiceStatus, "OPEN", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(billing.InvoiceStatus, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.BusinessRuleRejected,
                    "Penempatan tidak dapat dikoreksi karena tagihan rawat inap sudah difinalkan. " +
                    "Hubungi kasir untuk penyesuaian.",
                    InpPlacementCorrectionCodes.InvoiceNotOpen);
            }

            return await _bedOccupancyService.ApplyPlacementCorrectionAsync(
                placementId,
                request,
                actorUserId,
                cancellationToken);
        }
    }

    /// <summary>Kode penolakan koreksi penempatan — kontrak <c>integrasi-billing</c> <c>1.1.0</c> API 3.4.</summary>
    public static class InpPlacementCorrectionCodes
    {
        /// <summary>Invoice rawat inap bukan <c>OPEN</c>.</summary>
        public const string InvoiceNotOpen = "INP-COR-001";

        /// <summary>Status invoice tidak dapat dibaca.</summary>
        public const string InvoiceUnreadable = "INP-COR-002";

        /// <summary>Versi berubah, atau baris sudah dikoreksi.</summary>
        public const string VersionChanged = "INP-COR-003";

        /// <summary>Bed tujuan tidak layak.</summary>
        public const string BedNotEligible = "INP-COR-004";
    }

    /// <summary>Hasil satu koreksi penempatan.</summary>
    public sealed class InpPlacementCorrectionResult
    {
        private InpPlacementCorrectionResult(
            InpEpisodeOperationStatus status,
            string message,
            string? code,
            Guid? placementId,
            List<PlacementEligibilityFailureResponse>? failures)
        {
            Status = status;
            Message = message;
            Code = code;
            PlacementId = placementId;
            Failures = failures ?? new List<PlacementEligibilityFailureResponse>();
        }

        public InpEpisodeOperationStatus Status { get; }

        public string Message { get; }

        /// <summary>Kode penolakan kontrak, misalnya <c>INP-COR-001</c>; kosong bila tidak ada.</summary>
        public string? Code { get; }

        /// <summary>Id baris koreksi yang baru lahir.</summary>
        public Guid? PlacementId { get; }

        public List<PlacementEligibilityFailureResponse> Failures { get; }

        public static InpPlacementCorrectionResult Ok(string message, Guid placementId)
            => new(InpEpisodeOperationStatus.Success, message, null, placementId, null);

        public static InpPlacementCorrectionResult Fail(
            InpEpisodeOperationStatus status,
            string message,
            string? code = null,
            List<PlacementEligibilityFailureResponse>? failures = null)
            => new(status, message, code, null, failures);
    }
}
