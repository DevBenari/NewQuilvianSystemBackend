namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Jenis dokumen klinis yang dapat tunduk pada aturan keutuhan rekam medis.
    ///
    /// Tiga belas nilai didaftarkan sekaligus supaya nomornya stabil sejak awal dan tidak
    /// bergeser di kemudian hari. Namun rilis pertama hanya menegakkan aturan keutuhan untuk
    /// <see cref="ProgressNote"/>, sesuai RM-DEC-019. Dua belas nilai lain sudah punya tempat,
    /// tetapi belum dipakai.
    /// </summary>
    public enum ClinicalDocumentKind
    {
        ProgressNote = 1,
        Consultation = 2,
        Assessment = 3,
        Diagnosis = 4,
        Procedure = 5,
        VitalSign = 6,
        Allergy = 7,
        MedicalHistory = 8,
        FamilyHistory = 9,
        ClinicalDocument = 10,
        NoteAttachment = 11,
        MedicalCertificate = 12,
        Consent = 13,

        /// <summary>
        /// Catatan sesi hemodialisa — <c>HMD-BP-001</c>, <c>BE-HMD-02</c>.
        ///
        /// Menambah nilai di sini saja TIDAK cukup (Temuan Kritis 1): jenis ini juga wajib ada
        /// pada himpunan <c>JenisYangDitegakkan</c> di <c>ClinicalDocumentIntegrityService</c>.
        /// Tanpa itu sesi tampil disahkan tetapi catatannya tetap dapat disunting.
        /// </summary>
        HemodialysisSession = 14
    }
}
