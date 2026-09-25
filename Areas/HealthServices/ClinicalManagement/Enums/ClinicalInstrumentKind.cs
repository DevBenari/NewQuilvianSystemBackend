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
        CaseManagementChecklist = 6
    }
}
