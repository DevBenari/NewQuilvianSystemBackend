using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Hasil bacaan status kasir untuk Rawat Inap — kontrak <c>integrasi-billing</c> <c>1.1.0</c>
    /// bagian 9.6 dan integrasi 4.3. Tanpa rupiah.
    /// </summary>
    public sealed class InpClearanceReadResult
    {
        /// <summary>
        /// <c>false</c> bila Billing tidak dapat dibaca. Pemanggil yang memutuskan akibatnya:
        /// keluar ruangan memberi peringatan, penutupan dan koreksi menolak. Tidak pernah dianggap
        /// <c>CLEARED</c>.
        /// </summary>
        public bool IsReadable { get; set; }

        /// <summary><c>PENDING</c>, <c>BLOCKED</c>, <c>CLEARED</c>, <c>REVOKED</c>; <c>null</c> bila tidak terbaca.</summary>
        public string? Status { get; set; }

        public List<InpClearanceReasonItem> Reasons { get; set; } = new();

        public DateTimeOffset? EvaluatedAt { get; set; }

        /// <summary><c>OPEN</c>, <c>FINAL</c>, <c>CLOSED</c>, atau <c>NONE</c>; <c>null</c> bila tidak terbaca.</summary>
        public string? InvoiceStatus { get; set; }

        /// <summary>Pemetaan ke kosakata jejak pengamatan Rawat Inap.</summary>
        public InpClearanceObservation ToObservation()
        {
            if (!IsReadable)
            {
                return InpClearanceObservation.Unreadable;
            }

            return Status switch
            {
                "CLEARED" => InpClearanceObservation.Cleared,
                "BLOCKED" => InpClearanceObservation.Blocked,
                "REVOKED" => InpClearanceObservation.Revoked,
                _ => InpClearanceObservation.Pending
            };
        }
    }

    /// <summary>Satu kendala izin kasir tanpa nominal.</summary>
    public sealed class InpClearanceReasonItem
    {
        public string Code { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Satu-satunya pembaca status kasir untuk Rawat Inap (<c>INV-RWF-01</c>, <c>INT-RWF-02</c>,
    /// <c>INT-RWF-03</c>). Rawat Inap tidak membaca tabel <c>Bil*</c> secara langsung
    /// (<c>RWI-DEC-102</c> butir (e)).
    /// </summary>
    /// <remarks>
    /// Dibuat pada <c>BE-RWI-154</c> karena koreksi penempatan membutuhkan status invoice
    /// (<c>INT-RWF-03</c>). Pembaca lain — <c>billing-status</c>, keluar ruangan, penutupan — memakai
    /// adapter yang sama pada <c>BE-RWI-152</c> dan <c>BE-RWI-153</c>.
    /// </remarks>
    public interface IInpBillingClearanceAdapter
    {
        /// <summary>
        /// Membaca status kasir terbaru satu kunjungan. Tidak pernah melempar kegagalan ke pemanggil:
        /// kegagalan baca dikembalikan sebagai <see cref="InpClearanceReadResult.IsReadable"/> = <c>false</c>.
        /// </summary>
        Task<InpClearanceReadResult> GetStatusAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default);
    }
}
