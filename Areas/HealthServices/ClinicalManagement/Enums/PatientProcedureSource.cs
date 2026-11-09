namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    public enum PatientProcedureSource
    {
        DoctorOrder = 1,
        NursingAction = 2,
        ManualEntry = 3,
        EmergencyVisitFee = 4,

        /// <summary>
        /// Tindakan dari intervensi sesi terapi Rehab Medik yang sudah ditandatangani
        /// (<c>BE-RHB-013</c>, INT-RHB-08). Disetujui pemilik ClinicalManagement/Billing lewat
        /// <c>RHB-OQ-015</c> (2026-11-09).
        /// </summary>
        RehabilitationSession = 5
    }
}
