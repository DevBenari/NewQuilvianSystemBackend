namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Jenis instrumen atau formulir klinis berversi — <c>BE-RWI-107</c>, kamus data 0.4 bagian 11.4.
    /// </summary>
    public enum ClinicalInstrumentKind
    {
        /// <summary>Formulir Kajian Umum delapan bagian.</summary>
        GeneralNursingAssessmentForm = 1,

        /// <summary>Skala risiko jatuh.</summary>
        FallRiskScale = 2,

        /// <summary>Skala nyeri.</summary>
        PainScale = 3,

        /// <summary>Formulir Assesment Edukasi.</summary>
        EducationAssessmentForm = 4,

        /// <summary>Formulir Perencanaan Pulang.</summary>
        DischargePlanningForm = 5,

        /// <summary>Checklist Evaluasi Awal MPP.</summary>
        CaseManagementChecklist = 6,
        SurgicalSiteSurveillanceForm = 7,

        // =====================================================================
        // Rehab Medik — RHB-BP-001, BE-RHB-003, RHB-DEC-017 dan RHB-DEC-018 (disetujui
        // owner Rehab Medik, Sukma Giri Pratama, 2026-11-05). Satu nilai per instrumen karena
        // resolusi berlaku satu instrumen per jenis per pasien. NRS/VAS rehab memakai
        // PainScale yang sudah ada. Definisi isinya disusun komite lewat layar instrumen
        // klinis dan baru berlaku setelah disahkan.
        // =====================================================================

        /// <summary>Indeks Barthel — kemandirian aktivitas sehari-hari.</summary>
        RehabBarthelIndex = 8,

        /// <summary>Berg Balance Scale — keseimbangan.</summary>
        RehabBergBalanceScale = 9,

        /// <summary>Timed Up and Go — mobilitas fungsional (detik).</summary>
        RehabTimedUpAndGo = 10,

        /// <summary>Lawton IADL — aktivitas instrumental sehari-hari.</summary>
        RehabLawtonIadl = 11,

        /// <summary>Gugging Swallowing Screen (GUSS) — skrining menelan.</summary>
        RehabGussSwallowing = 12,

        /// <summary>Functional Oral Intake Scale (FOIS) — tingkat asupan oral.</summary>
        RehabFoisOralIntake = 13
    }
}
