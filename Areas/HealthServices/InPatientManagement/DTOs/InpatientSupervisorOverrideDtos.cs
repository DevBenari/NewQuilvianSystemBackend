namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Permintaan otorisasi supervisor override untuk pelepasan darurat saat clearance kasir belum ada atau dicabut.
    /// </summary>
    public class SupervisorOverrideRequestDto
    {
        /// <summary>
        /// Alasan kedaruratan klinis / rujukan (wajib minimal 20 karakter per VAL-INT-004).
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// PIN otorisasi supervisor untuk verifikasi keamanan (VAL-INT-005).
        /// </summary>
        public string SupervisorPin { get; set; } = string.Empty;
    }
}
