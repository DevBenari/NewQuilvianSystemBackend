namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Katalog kriteria dokumen wajib (`DC-RM-06a`, blueprint `RM-BP-001` revision 4 bagian A4.11).
    ///
    /// Arti "terpenuhi" setiap nilai ditulis di program dan disahkan lewat keputusan
    /// (`RM-DEC-047`, `053`–`056`, `059`, `060`). Master profil dokumen wajib hanya MEMILIH
    /// kriteria mana yang dituntut sebuah jenis kunjungan — ia tidak mendefinisikannya.
    ///
    /// Nilai yang dijawab modul lain dijawab lewat <c>IMedicalRecordCompletenessProbe</c> milik
    /// modul itu, karena arti final dokumennya milik pemiliknya (`RM-DEC-041`).
    /// </summary>
    public enum CompletenessCriterion
    {
        /// <summary>Asesmen awal keperawatan atau medis berstatus selesai.</summary>
        InitialAssessmentAny = 1,

        /// <summary>Asesmen awal keperawatan (<c>Initial</c>) berstatus selesai.</summary>
        InitialNursingAssessment = 2,

        /// <summary>Asesmen awal medis (<c>MedicalInitial</c>) berstatus selesai.</summary>
        InitialMedicalAssessment = 3,

        /// <summary>Diagnosis dicentang utama atau berjenis Primary, bukan dibatalkan/disingkirkan.</summary>
        PrimaryDiagnosis = 4,

        /// <summary>CPPT berprofesi dokter atau konsultasi dokter, tidak dibatalkan.</summary>
        DoctorNoteOrConsultation = 5,

        /// <summary>Minimal satu CPPT yang tidak dibatalkan.</summary>
        ProgressNoteAny = 6,

        EmergencyTriage = 7,
        EmergencyDisposition = 8,
        InpatientGeneralConsent = 9,
        DischargeSummary = 10,
        McuResume = 11,
        SurgeryReport = 12,
        AnesthesiaRecord = 13,
        SurgicalSafetyChecklist = 14,
        HemodialysisSessionSigned = 15,
        TransfusionMonitoring = 16
    }
}
