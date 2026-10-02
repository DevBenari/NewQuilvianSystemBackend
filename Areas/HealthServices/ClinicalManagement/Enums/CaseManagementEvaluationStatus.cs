namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Status Evaluasi Awal MPP — <c>BE-RWI-113</c>, state matrix 0.5.0 bagian 5.3.
    /// </summary>
    public enum CaseManagementEvaluationStatus
    {
        /// <summary>Konsep.</summary>
        Draft = 1,

        /// <summary>Selesai; koreksi lewat addendum.</summary>
        Completed = 2,

        /// <summary>Dibatalkan beralasan; terminal.</summary>
        Cancelled = 3
    }
}
