namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Jenis cetakan yang dicatat log cetak — backend 13.9 (<c>BE-RWI-192</c>, <c>RWI-DEC-240</c>).
    /// </summary>
    public enum InpAdmissionPrintKind
    {
        AdultWristband = 1,
        InfantWristband = 2,
        PatientLabel = 3,

        /// <summary>Data Dasar Rawat Inap (IPD).</summary>
        InpatientBaseData = 4,

        /// <summary>Dokumen admisi bertanda tangan; wajib menunjuk dokumennya.</summary>
        AdmissionDocument = 5
    }
}
