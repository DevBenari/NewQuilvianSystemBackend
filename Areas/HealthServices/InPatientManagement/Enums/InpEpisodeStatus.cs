namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    // Tepat lima nilai. Nilai InCare sengaja tidak ada — lihat erd/data-dictionary.md bagian 15.
    public enum InpEpisodeStatus
    {
        Draft = 0,
        Admitted = 1,
        DischargePending = 2,
        Closed = 3,
        Cancelled = 4
    }
}
