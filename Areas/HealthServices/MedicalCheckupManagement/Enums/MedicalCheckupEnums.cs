namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums
{
    // Seluruh enum modul MCU dalam satu berkas (data-dictionary.md bagian 5). Seluruhnya disimpan
    // sebagai integer; nilai 0 sengaja tidak dipakai agar nilai kosong tidak tersimpan diam-diam.
    // Nilai yang sudah terbit tidak boleh digeser, hanya boleh ditambah di ujung.

    /// <summary>Jenis komponen paket dan komponen order.</summary>
    public enum McuComponentType
    {
        Laboratory = 1,
        Radiology = 2,
        McuExamination = 3
    }

    /// <summary>Bentuk isian pada formulir Pemeriksaan MCU.</summary>
    public enum McuExaminationFieldType
    {
        Text = 1,
        Number = 2,
        Option = 3,
        YesNo = 4
    }

    /// <summary>Pintu asal order MCU.</summary>
    public enum McuOrderSource
    {
        DirectBooking = 1,
        OutpatientRecommendation = 2,
        InpatientRecommendation = 3,
        EmergencyRecommendation = 4,
        WalkInAdmission = 5,
        WalkInKiosk = 6,
        CompanyImport = 7
    }

    /// <summary>
    /// Status order MCU — <c>state-transition-matrix.md</c> (<c>MCU-STATE-v1</c>).
    /// </summary>
    public enum McuOrderStatus
    {
        /// <summary>Terjadwal.</summary>
        Scheduled = 1,

        /// <summary>Tidak Hadir.</summary>
        NoShow = 2,

        /// <summary>Hadir.</summary>
        CheckedIn = 3,

        /// <summary>Dalam Pemeriksaan.</summary>
        InExamination = 4,

        /// <summary>Menunggu Resume.</summary>
        AwaitingResume = 5,

        /// <summary>Disetujui Dokter.</summary>
        DoctorApproved = 6,

        /// <summary>Terverifikasi.</summary>
        Verified = 7,

        /// <summary>Diserahkan.</summary>
        Delivered = 8,

        /// <summary>Dibatalkan.</summary>
        Cancelled = 9
    }

    /// <summary>
    /// Status komponen order.
    /// </summary>
    /// <remarks>
    /// Nilai <see cref="Cancelled"/> = 6 dipakai filter unique index komponen
    /// (<c>IX_McuOrderComponent_Order_Procedure</c>, <c>IX_McuOrderComponent_Order_ExaminationType</c>).
    /// Jangan digeser.
    /// </remarks>
    public enum McuOrderComponentStatus
    {
        Pending = 1,
        Ordered = 2,
        ReadyToFill = 3,
        Rescheduled = 4,
        Completed = 5,
        Cancelled = 6
    }

    /// <summary>Hasil penandaan persyaratan saat check-in.</summary>
    public enum McuRequirementOutcome
    {
        Fulfilled = 1,
        ProceedWithNote = 2,
        ComponentRescheduled = 3
    }

    /// <summary>Cara order tertaut ke data induk pasien.</summary>
    public enum McuPatientMatchMethod
    {
        ExistingAtBooking = 1,
        NikAndBirthDateMatch = 2,
        ConfirmedByStaff = 3,
        NewPatientRegistered = 4
    }

    public enum McuCheckInChannel
    {
        Admission = 1,
        Kiosk = 2
    }

    /// <summary>Dasar pemeriksaan boleh dimulai: lunas atau terjamin perusahaan.</summary>
    public enum McuClearanceBasis
    {
        Paid = 1,
        Guaranteed = 2
    }

    public enum McuResumeStatus
    {
        Draft = 1,
        Approved = 2,
        Verified = 3,
        Corrected = 4
    }

    /// <summary>Bentuk bukti persetujuan pelepasan hasil ke perusahaan (menunggu <c>MCU-OQ-003</c>).</summary>
    public enum McuConsentEvidenceType
    {
        PaperForm = 1,
        ElectronicAcknowledgement = 2
    }

    public enum McuDeliveryRecipient
    {
        Participant = 1,
        Company = 2
    }

    public enum McuDeliveryContent
    {
        Full = 1,
        FitnessSummary = 2
    }

    public enum McuDeliveryChannel
    {
        Pickup = 1,
        WhatsApp = 2,
        Email = 3,
        Phone = 4
    }
}
