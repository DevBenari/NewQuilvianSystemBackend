namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Status verifikasi instruksi dokter atas pesanan tindakan yang dibuat perawat — BE-RWI-097 / BE-RWI-098.
    /// </summary>
    public enum PatientProcedureInstructionVerificationStatus
    {
        /// <summary>
        /// Pesanan dibuat langsung oleh dokter; tidak memerlukan verifikasi instruksi.
        /// </summary>
        NotRequired = 0,

        /// <summary>
        /// Pesanan dibuat oleh perawat atas instruksi dokter; menunggu verifikasi dokter pemberi instruksi.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Instruksi telah diverifikasi oleh dokter pemberi instruksi yang bersangkutan.
        /// </summary>
        Verified = 2
    }
}
