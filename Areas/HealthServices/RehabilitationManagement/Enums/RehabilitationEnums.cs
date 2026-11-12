namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums
{
    // Seluruh enum modul Rehab Medik dalam satu berkas — RHB-BP-001, data-dictionary.md bagian 3.
    // Seluruhnya disimpan sebagai integer; nilai yang sudah terbit tidak boleh digeser, hanya
    // boleh ditambah di ujung.

    /// <summary>Asal permintaan rehab.</summary>
    /// <remarks>
    /// Nilai <see cref="ExternalReferral"/> = 4 dipakai check constraint
    /// <c>CK_RhbRequest_Source</c>. Jangan digeser.
    /// </remarks>
    public enum RehabRequestSource
    {
        Outpatient = 1,
        Inpatient = 2,
        Emergency = 3,
        ExternalReferral = 4
    }

    /// <summary>Status permintaan rehab — <c>state-transition-matrix.md</c> bagian 1.</summary>
    public enum RehabRequestStatus
    {
        Draft = 1,
        Submitted = 2,
        NeedsClarification = 3,
        Accepted = 4,
        Redirected = 5,
        Rejected = 6,
        Cancelled = 7
    }

    /// <summary>Status episode rehab — <c>state-transition-matrix.md</c> bagian 2.</summary>
    public enum RehabEpisodeStatus
    {
        AwaitingMedicalAssessment = 1,
        Planning = 2,
        Active = 3,
        Closed = 4
    }

    /// <summary>Alasan penutupan episode. Selesai klinis dibedakan dari putus layanan dan meninggal.</summary>
    public enum RehabEpisodeClosureReason
    {
        ClinicalCompletion = 1,
        Discontinued = 2,
        Referred = 3,
        Transferred = 4,
        LostToFollowUp = 5,
        Deceased = 6,
        PatientRefused = 7
    }

    /// <summary>Jenis asesmen awal.</summary>
    /// <remarks>
    /// Nilai <see cref="MedicalInitial"/> = 1 dipakai unique index
    /// <c>UX_RhbAssessment_MedicalFinal</c>. Jangan digeser.
    /// </remarks>
    public enum RehabAssessmentType
    {
        MedicalInitial = 1,
        DisciplineInitial = 2
    }

    /// <summary>
    /// Status dokumen klinis rehab. Berjalan berdampingan dengan status keutuhan Rekam Medis
    /// (<c>ClinicalDocumentIntegrityStatus</c>), bukan menggantikannya.
    /// </summary>
    /// <remarks>Nilai <see cref="Final"/> = 2 dipakai unique index <c>UX_RhbAssessment_MedicalFinal</c>.</remarks>
    public enum RehabRecordStatus
    {
        Draft = 1,
        Final = 2,
        Cancelled = 3
    }

    /// <summary>Sumber satu hasil ukur.</summary>
    public enum RehabMeasurementSource
    {
        /// <summary>Instrumen berversi dari mesin instrumen klinis (RHB-DEC-006).</summary>
        Instrument = 1,

        /// <summary>Pengukuran terstruktur: nama, nilai, satuan (RHB-DEC-017).</summary>
        Structured = 2
    }

    public enum RehabGoalTerm
    {
        ShortTerm = 1,
        LongTerm = 2
    }

    public enum RehabGoalStatus
    {
        Active = 1,
        Achieved = 2,
        PartiallyAchieved = 3,
        NotAchieved = 4,
        Discontinued = 5,
        Superseded = 6
    }

    /// <summary>Status program terapi — <c>state-transition-matrix.md</c> bagian 4.</summary>
    /// <remarks>
    /// Nilai <see cref="Active"/> = 2 dipakai unique index
    /// <c>UX_RhbProgram_ActivePerDiscipline</c>. Jangan digeser.
    /// </remarks>
    public enum RehabProgramStatus
    {
        Draft = 1,
        Active = 2,
        Superseded = 3,
        Stopped = 4,
        Completed = 5,
        Cancelled = 6
    }

    public enum RehabConsentStatus
    {
        Agreed = 1,
        PartiallyAgreed = 2,
        Refused = 3,
        NotRequired = 4
    }

    public enum RehabUnderstandingLevel
    {
        Understood = 1,
        PartiallyUnderstood = 2,
        NotUnderstood = 3,
        NotAssessed = 4
    }

    /// <summary>Status pelaksanaan sesi — <c>state-transition-matrix.md</c> bagian 5.</summary>
    public enum RehabSessionStatus
    {
        InProgress = 1,
        Completed = 2,
        StoppedEarly = 3,
        NotDone = 4
    }

    /// <summary>Alasan sesi tidak terlaksana. Sesi ini tidak pernah tertagih (RHB-INV-06).</summary>
    public enum RehabNotDoneReason
    {
        NoShow = 1,
        CancelledByPatient = 2,
        Postponed = 3,
        PatientRefused = 4,
        ClinicalCondition = 5,
        ResourceUnavailable = 6
    }

    public enum RehabDeviationType
    {
        None = 0,
        Modified = 1,
        StoppedEarly = 2,
        Escalated = 3
    }

    /// <summary>Status penyerahan tindakan sesi ke Billing — <c>state-transition-matrix.md</c> bagian 6.</summary>
    public enum RehabBillingHandoffStatus
    {
        NotRequired = 0,
        Pending = 1,
        Succeeded = 2,
        Failed = 3
    }

    /// <summary>Keputusan reasesmen — <c>state-transition-matrix.md</c> bagian 7.</summary>
    public enum RehabProgressDecision
    {
        Continue = 1,
        Modify = 2,
        Stop = 3,
        Refer = 4,
        Complete = 5
    }

    public enum RehabFollowUpType
    {
        None = 0,
        Control = 1,
        HomeProgram = 2,
        ReferBack = 3,
        ReferOther = 4
    }
}
