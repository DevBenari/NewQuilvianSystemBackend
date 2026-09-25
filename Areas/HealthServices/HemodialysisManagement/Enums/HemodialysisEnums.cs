namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums
{
    // Seluruh enum modul Hemodialisa dalam satu berkas (02-backend-architecture.md bagian 5).
    // Seluruhnya disimpan sebagai integer; nilai yang sudah terbit tidak boleh digeser, hanya
    // boleh ditambah di ujung.

    /// <summary>Prioritas permintaan HD masuk.</summary>
    public enum HmdOrderPriority
    {
        Routine = 1,
        Cito = 2
    }

    /// <summary>
    /// Status permintaan HD masuk — <c>state-transition-matrix.md</c> bagian 1.
    /// </summary>
    public enum HmdOrderStatus
    {
        Requested = 1,
        Accepted = 2,
        OnHold = 3,
        Rejected = 4,
        Cancelled = 5,
        Fulfilled = 6
    }

    /// <summary>
    /// Status program HD pasien — <c>state-transition-matrix.md</c> bagian 2.
    /// </summary>
    /// <remarks>
    /// Nilai <see cref="Active"/> = 2 dipakai filter unique index
    /// <c>IX_HmdEpisode_PatientId_Active</c>. Jangan digeser.
    /// </remarks>
    public enum HmdEpisodeStatus
    {
        Draft = 1,
        Active = 2,
        Suspended = 3,
        Closed = 4
    }

    /// <summary>Sebab program HD ditutup.</summary>
    public enum HmdEpisodeClosureReason
    {
        Completed = 1,
        Transferred = 2,
        Discontinued = 3,
        Deceased = 4,
        Other = 99
    }

    /// <summary>Hasil penilaian kelayakan HD oleh dokter.</summary>
    public enum HmdEligibilityOutcome
    {
        Eligible = 1,
        Deferred = 2,
        Modified = 3,
        Referred = 4
    }

    /// <summary>Jenis akses vaskular. Lokasi anatomi dicatat pada <c>AccessSite</c>.</summary>
    public enum HmdVascularAccessType
    {
        ArteriovenousFistula = 1,
        ArteriovenousGraft = 2,
        Catheter = 3,
        Other = 99
    }

    /// <summary>Kelaikan fungsi akses vaskular.</summary>
    public enum HmdVascularAccessStatus
    {
        Usable = 1,
        NeedsAttention = 2,
        NotUsable = 3
    }

    /// <summary>Jenis pemeriksaan serologi yang dirujuk.</summary>
    public enum HmdSerologyTestType
    {
        HBsAg = 1,
        AntiHbs = 2,
        AntiHcv = 3,
        AntiHiv = 4
    }

    /// <summary>Hasil serologi sebagaimana dibaca petugas. SENSITIF.</summary>
    public enum HmdSerologyResultFlag
    {
        Reactive = 1,
        NonReactive = 2,
        Indeterminate = 3
    }

    /// <summary>Status tinjauan rujukan serologi.</summary>
    public enum HmdSerologyReviewStatus
    {
        PendingReview = 1,
        Reviewed = 2
    }

    /// <summary>
    /// Kebutuhan isolasi pasien, sekaligus peruntukan khusus mesin (<c>HmdMachine.DedicatedFor</c>).
    /// </summary>
    /// <remarks>
    /// Aturan Hepatitis B tidak otomatis diberlakukan untuk Hepatitis C dan HIV; keduanya
    /// keputusan terpisah yang dicatat sebagai <see cref="Other"/>.
    /// </remarks>
    public enum HmdIsolationRequirement
    {
        None = 0,
        HepatitisB = 1,
        Other = 2
    }

    /// <summary>
    /// Status resep HD — <c>state-transition-matrix.md</c> bagian 3.
    /// </summary>
    /// <remarks>
    /// Nilai <see cref="Active"/> = 2 dipakai filter unique index
    /// <c>IX_HmdPrescription_EpisodeId_Active</c>. Jangan digeser.
    /// </remarks>
    public enum HmdPrescriptionStatus
    {
        Draft = 1,
        Active = 2,
        Superseded = 3,
        Cancelled = 4
    }

    /// <summary>
    /// Dua belas status sesi HD — <c>state-transition-matrix.md</c> bagian 4.
    /// </summary>
    public enum HmdSessionStatus
    {
        Planned = 1,
        Scheduled = 2,
        CheckedIn = 3,
        PreCheck = 4,
        Held = 5,
        Ready = 6,
        InProgress = 7,
        Stopped = 8,
        Completed = 9,
        AwaitingFinalization = 10,
        Finalized = 11,
        Cancelled = 12
    }

    /// <summary>Shift pelayanan unit HD.</summary>
    public enum HmdShift
    {
        Morning = 1,
        Afternoon = 2,
        Evening = 3
    }

    /// <summary>Sebab sesi dihentikan sebelum selesai (<c>HMD-DEC-012</c>).</summary>
    public enum HmdSessionStopReason
    {
        HemodynamicInstability = 1,
        CircuitClotting = 2,
        VascularAccessProblem = 3,
        PatientRefusal = 4,
        MachineFailure = 5,
        Other = 99
    }

    /// <summary>Tujuan pasien setelah sesi.</summary>
    public enum HmdDisposition
    {
        Home = 1,
        ReturnToWard = 2,
        TransferToEmergency = 3,
        Other = 99
    }

    /// <summary>Hasil satu butir checklist Pra-HD atau butir kesiapan unit.</summary>
    public enum HmdChecklistResult
    {
        NotChecked = 0,
        Met = 1,
        NotMet = 2,
        NotApplicable = 3
    }

    /// <summary>Fase penilaian sesi.</summary>
    public enum HmdAssessmentPhase
    {
        Pre = 1,
        Post = 2
    }

    /// <summary>Jalur pemberian obat selama sesi.</summary>
    public enum HmdMedicationRoute
    {
        IntravenousBolus = 1,
        ContinuousInfusion = 2,
        ExtracorporealCircuit = 3,
        Subcutaneous = 4,
        Oral = 5,
        Other = 99
    }

    /// <summary>Status penerusan fakta pemberian obat ke Farmasi.</summary>
    public enum HmdPharmacyHandoffStatus
    {
        Pending = 1,
        Succeeded = 2,
        Failed = 3
    }

    /// <summary>Jenis komplikasi intradialisis. Selalu dipilih manusia, tidak disimpulkan sistem.</summary>
    public enum HmdComplicationType
    {
        IntradialyticHypotension = 1,
        MuscleCramp = 2,
        ChillsOrFever = 3,
        AccessBleeding = 4,
        ChestPain = 5,
        Arrhythmia = 6,
        NauseaOrVomiting = 7,
        Headache = 8,
        AllergicReaction = 9,
        IntradialyticHypertension = 10,
        Other = 99
    }

    /// <summary>Hasil penanganan komplikasi.</summary>
    public enum HmdComplicationOutcome
    {
        Resolved = 1,
        Ongoing = 2,
        Escalated = 3
    }

    /// <summary>Pengaruh komplikasi terhadap jalannya sesi.</summary>
    public enum HmdComplicationSessionImpact
    {
        Continued = 1,
        Modified = 2,
        Stopped = 3
    }

    /// <summary>Peran petugas pada sesi.</summary>
    public enum HmdStaffRole
    {
        PrimaryNurse = 1,
        AssistantNurse = 2,
        Technician = 3
    }

    /// <summary>
    /// Hasil pemeriksaan kewenangan klinis petugas — syarat 5, <c>HMD-DEC-013</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="NotVerifiable"/> tidak boleh ditulis sebagai <see cref="Verified"/>. Keduanya
    /// berbeda antara "sudah diperiksa dan aman" dan "belum pernah diperiksa".
    /// </remarks>
    public enum HmdCompetencyVerificationStatus
    {
        Verified = 1,
        NotAuthorized = 2,
        NotVerifiable = 3
    }

    /// <summary>Status laik pakai mesin HD — <c>state-transition-matrix.md</c> bagian 5.</summary>
    public enum HmdMachineStatus
    {
        Ready = 1,
        Blocked = 2,
        Maintenance = 3,
        NotEligible = 4
    }

    /// <summary>Status station HD — <c>state-transition-matrix.md</c> bagian 6.</summary>
    public enum HmdStationStatus
    {
        Available = 1,
        Blocked = 2,
        Maintenance = 3
    }

    /// <summary>
    /// Status kesiapan unit — <c>state-transition-matrix.md</c> bagian 7. <c>ConditionallyReady</c>
    /// sengaja tidak ada.
    /// </summary>
    public enum HmdReadinessStatus
    {
        Draft = 1,
        Ready = 2,
        NotReady = 3
    }

    /// <summary>Kategori butir kesiapan unit.</summary>
    public enum HmdReadinessCategory
    {
        Machine = 1,
        Station = 2,
        Water = 3,
        Supply = 4,
        Staff = 5
    }

    /// <summary>Kategori butir checklist Pra-HD.</summary>
    public enum HmdChecklistCategory
    {
        Identity = 1,
        ClinicalContext = 2,
        Resource = 3
    }

    /// <summary>Status serah terima tindakan sesi ke Billing (<c>HMD-DEC-012</c>).</summary>
    public enum HmdBillingHandoffStatus
    {
        NotRequired = 0,
        Pending = 1,
        Succeeded = 2,
        Failed = 3
    }
}
