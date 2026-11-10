namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants
{
    /// <summary>
    /// Kode penolakan Rehab Medik — <c>contracts/validation-matrix.md</c> v1. Dikirim sebagai
    /// <c>Code</c> di argumen ketiga <c>ApiResponse.Fail</c> agar layar dapat bereaksi tanpa
    /// membaca kalimat pesan.
    /// </summary>
    public static class RhbErrorCodes
    {
        // Permintaan
        public const string PatientRequired = "RHB-VAL-001";
        public const string SourceEncounterInvalid = "RHB-VAL-002";
        public const string ExternalReferralIncomplete = "RHB-VAL-003";
        public const string IndicationRequired = "RHB-VAL-004";
        public const string DisciplineRequired = "RHB-VAL-005";
        public const string RequestedAtInFuture = "RHB-VAL-006";
        public const string DuplicateRequest = "RHB-VAL-007";
        public const string ReasonRequired = "RHB-VAL-008";
        public const string ResponsibleDoctorRequired = "RHB-VAL-009";

        // Umum
        public const string StaleVersion = "RHB-VAL-010";
        public const string InvalidState = "RHB-VAL-020";
        public const string DocumentLocked = "RHB-VAL-021";
        public const string NotAuthor = "RHB-VAL-022";
        public const string EpisodeClosed = "RHB-VAL-023";

        // Asesmen dan pengukuran
        public const string NoDisciplinePrescribed = "RHB-VAL-030";
        public const string ProfessionMismatch = "RHB-VAL-031";
        public const string DisciplineNotPrescribed = "RHB-VAL-032";
        public const string MedicalAssessmentAlreadyFinal = "RHB-VAL-033";
        public const string AssessmentIncomplete = "RHB-VAL-034";
        public const string InstrumentNotApproved = "RHB-VAL-035";
        public const string InstrumentChangeUnexplained = "RHB-VAL-036";

        // Program dan tujuan
        public const string MedicalAssessmentNotFinal = "RHB-VAL-040";
        public const string DisciplineAssessmentNotFinal = "RHB-VAL-041";
        public const string InterventionRequired = "RHB-VAL-042";
        public const string GoalRequired = "RHB-VAL-043";
        public const string ProgramScheduleIncomplete = "RHB-VAL-044";
        public const string ConsentRefused = "RHB-VAL-045";
        public const string GoalInUse = "RHB-VAL-046";
        public const string GoalIncomplete = "RHB-VAL-047";

        // Sesi
        public const string ProgramNotActive = "RHB-VAL-050";
        public const string SessionEncounterInvalid = "RHB-VAL-051";
        public const string PatientDischarged = "RHB-VAL-052";
        public const string SafetyCheckIncomplete = "RHB-VAL-053";
        public const string ContraindicationUnexplained = "RHB-VAL-054";
        public const string InterventionNotRecorded = "RHB-VAL-055";
        public const string DeviationIncomplete = "RHB-VAL-056";
        public const string UnplannedInterventionWithoutDeviation = "RHB-VAL-057";
        public const string NotDoneReasonRequired = "RHB-VAL-058";
        public const string ResponseRequired = "RHB-VAL-059";

        // Penutupan
        public const string EpisodeNotReadyToClose = "RHB-VAL-060";
        public const string DischargeIncomplete = "RHB-VAL-061";

        // Konfigurasi
        public const string DisciplineProfessionRequired = "RHB-VAL-070";
        public const string DisciplineCodeTaken = "RHB-VAL-071";

        // Delta BE-RHB-005 (tercatat di laporan task): kode teknis tambahan konfigurasi.
        public const string DisciplineInUse = "RHB-VAL-072";
        public const string ProcedureNotAvailable = "RHB-VAL-073";
        public const string InstrumentKindInvalid = "RHB-VAL-074";
        public const string DisciplineCodeInvalid = "RHB-VAL-075";
        public const string PageRangeInvalid = "RHB-VAL-076";

        // Delta BE-RHB-006: pencarian pasien minimal 3 karakter (api-contract v1, Rehab Lookup).
        public const string LookupSearchTooShort = "RHB-VAL-077";

        // Delta BE-RHB-007: kode teknis permintaan yang tidak punya baris di validation-matrix v1.
        public const string RequestingDoctorInvalid = "RHB-VAL-078";
        public const string FieldTooLong = "RHB-VAL-079";
        public const string NumberAllocationFailed = "RHB-VAL-080";

        // Delta BE-RHB-009: kode teknis asesmen yang tidak punya baris di validation-matrix v1.
        // 081 dipakai bersama seluruh dokumen klinis rehab (asesmen, edukasi) sejak BE-RHB-010.
        public const string ClinicalEncounterInvalid = "RHB-VAL-081";
        public const string MeasurementInvalid = "RHB-VAL-082";

        // Delta BE-RHB-010: edukasi dan persetujuan belum punya baris di validation-matrix v1.
        public const string EducationIncomplete = "RHB-VAL-083";
        public const string ProgramNotInEpisode = "RHB-VAL-084";

        // Delta BE-RHB-014: reasesmen belum lengkap saat tanda tangan (ringkasan, keputusan, alasan, tujuan rujukan).
        public const string ProgressReviewIncomplete = "RHB-VAL-085";
    }
}
