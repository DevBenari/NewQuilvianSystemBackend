namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Status pelaksanaan sliding scale — <c>BE-RWI-123</c>, state matrix 0.5.0 bagian 5.6.
    /// </summary>
    public enum SlidingScaleExecutionStatus
    {
        /// <summary>Tercatat.</summary>
        Recorded = 1,

        /// <summary>Dibatalkan bersama koreksi dosis MAR-nya menjadi Cancelled.</summary>
        Cancelled = 2
    }
}
