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
        HemodialysisSession = 14,

        /// <summary>
        /// Hasil laboratorium yang dirilis — <c>LAB-BP-001</c>, <c>BE-LAB-70</c>, kesepakatan
        /// <c>LAB-COORD-002</c>. Satu baris per <c>LabExamination</c> yang dirilis; dokumennya
        /// langsung tertanda tangan dan terkunci saat rilis.
        ///
        /// <b>15, bukan 14.</b> Rancangan Laboratorium menetapkan 14 sebelum
        /// <see cref="HemodialysisSession"/> mengambil angka itu; pemilik modul Laboratorium
        /// memilih 15 pada 2026-09-29.
        ///
        /// Sengaja BELUM masuk <c>JenisYangDitegakkan</c> pada
        /// <c>ClinicalDocumentIntegrityService</c>: penegakan <c>EnsureMutableAsync</c> baru
        /// dibutuhkan koreksi <c>S6</c>, dan himpunan itu milik Rekam Medis.
        /// </summary>
        LaboratoryResult = 15,

        // =====================================================================
        // Amandemen RM-BP-001 revision 4 (`RM-DEC-039`, `BE-21`).
        //
        // Nilai 16–44 adalah dokumen milik modul lain yang dibaca berkas rekam medis lewat
        // IMedicalRecordDocumentSource. Nomornya didaftarkan sekaligus supaya stabil, tetapi
        // SATU nilai baru tampil di berkas hanya setelah pembacanya terdaftar. Sebelum itu
        // nilainya dilaporkan sebagai "belum tersedia" (MedicalRecordTimelineService), tidak
        // sebagai daftar kosong — keadaan yang pernah menimpa nilai 14 dan 15 (`RM-CAP-035`).
        // =====================================================================

        RadiologyReport = 16,
        EmergencyTriage = 17,
        EmergencyObservation = 18,
        EmergencyResuscitation = 19,
        EmergencyDisposition = 20,
        DischargeSummary = 21,
        AdmissionDocument = 22,
        SurgeryReport = 23,
        AnesthesiaRecord = 24,
        SurgicalSafetyChecklist = 25,
        BirthRecord = 26,
        NutritionCareRecord = 27,
        BloodGroupExam = 28,
        TransfusionReactionNotice = 29,
        Prescription = 30,
        MedicationAdministration = 31,
        MedicationReconciliation = 32,
        McuResume = 33,
        McuExaminationResult = 34,
        NursingCarePlan = 35,
        PhysicianVisit = 36,
        NursingIntervention = 37,
        TransfusionMonitoring = 38,
        DoctorCertificate = 39,
        CaseManagementEvaluation = 40,
        NosocomialInfection = 41,
        TransferHandover = 42,
        PeriodicObservationSummary = 43,

        /// <summary>
        /// Hasil Patologi Anatomi. Sengaja TIDAK punya pembaca: laporan PA saat ini dapat dibuka
        /// kembali sesudah dirilis (`RM-FACT-020`), sehingga ditahan dari berkas (`RM-DEC-058`).
        /// Nilai ini ada supaya berkas dapat menyatakannya "belum tersedia".
        /// </summary>
        PathologyAnatomyResult = 44
    }
}
