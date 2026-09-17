namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Permintaan konfirmasi pelepasan fisik pasien dari kamar rawat inap oleh perawat bangsal.
    /// </summary>
    public class ConfirmPhysicalDischargeRequestDto
    {
        /// <summary>
        /// Waktu aktual pasien meninggalkan kamar/tempat tidur secara fisik.
        /// Tidak boleh mendahului jam mulai penempatan kamar (VAL-INT-008).
        /// </summary>
        public DateTime PhysicalDischargeDateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Catatan tambahan kepergian fisik / penjemputan pasien.
        /// </summary>
        public string? Notes { get; set; }
    }
}
