namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Kejadian di dalam kunjungan yang menambah dokumen wajib (`RM-DEC-047`).
    ///
    /// Operasi, hemodialisa, dan transfusi bukan jenis kunjungan, sehingga dokumen wajibnya
    /// hanya dituntut bila kejadiannya memang ada pada kunjungan itu.
    /// </summary>
    public enum CompletenessCondition
    {
        None = 0,
        Surgery = 1,
        HemodialysisSession = 2,
        Transfusion = 3
    }
}
