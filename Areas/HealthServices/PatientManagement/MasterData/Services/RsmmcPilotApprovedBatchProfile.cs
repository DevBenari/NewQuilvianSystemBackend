namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Keadaan batch migrasi yang disetujui pemilik, dipakai sebagai gerbang sebelum eksekusi.
    /// </summary>
    /// <remarks>
    /// Angka-angka ini hanya berlaku untuk batch kanonik `BE-PAT-MIG-001`
    /// (<see cref="Canonical"/>). Batch lain tidak punya ekspektasi yang disetujui, sehingga
    /// eksekusinya ditolak — gagal tertutup. Simulasi tetap boleh dan melaporkan hasil gerbangnya.
    /// </remarks>
    public sealed record RsmmcPilotApprovedBatchProfile(
        Guid BatchId,
        int ExpectedFinalizedPlannerRows,
        int ExpectedPilotRows,
        int ExpectedBackupRows)
    {
        /// <summary>
        /// Batch kanonik `PAT-DEC-004` beserta keadaan yang dilaporkan pemilik
        /// (`PAT-FACT-002`, `PAT-FACT-010`, `PAT-FACT-025`).
        /// </summary>
        public static RsmmcPilotApprovedBatchProfile Canonical { get; } = new(
            Guid.Parse("45eeefba-dd39-6831-1bd2-d986851f7c1f"),
            ExpectedFinalizedPlannerRows: 705223,
            ExpectedPilotRows: 715,
            ExpectedBackupRows: 715);
    }
}
