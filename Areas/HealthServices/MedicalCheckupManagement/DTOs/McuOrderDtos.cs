using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs
{
    // =====================================================================
    // MCU-API-v1 — grup Mcu Order, bagian baca (BE-MCU-010)
    // =====================================================================

    public class McuOrderPagedQuery
    {
        /// <summary>Tanggal pemeriksaan. Kosong = seluruh tanggal (layar mengirim hari ini sebagai bawaan).</summary>
        public DateOnly? Date { get; set; }
        public McuOrderStatus? Status { get; set; }
        public McuOrderSource? Source { get; set; }
        public Guid? CompanyGuarantorId { get; set; }
        public Guid? McuDoctorId { get; set; }

        /// <summary>Nama peserta, No. RM, atau No. Order.</summary>
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class McuOrderSummaryQuery
    {
        /// <summary>Kosong = hari ini (WIB).</summary>
        public DateOnly? Date { get; set; }
    }

    public class McuOrderSummaryResponse
    {
        public DateOnly Date { get; set; }
        public int Total { get; set; }

        /// <summary>Selalu sembilan baris, urut alur status, termasuk yang berjumlah nol.</summary>
        public List<McuOrderStatusCountResponse> Statuses { get; set; } = new();
    }

    public class McuOrderStatusCountResponse
    {
        public McuOrderStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class McuOrderListResponse
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateOnly ExaminationDate { get; set; }
        public Guid? PatientId { get; set; }

        /// <summary>Kosong = identitas sementara; layar menulis <i>Belum terdaftar</i>.</summary>
        public string? MedicalRecordNumber { get; set; }
        public string ParticipantName { get; set; } = string.Empty;

        /// <summary>NIK dimasking, contoh <c>3201********0001</c> (<c>MCU-FE-003</c>). NIK utuh hanya di detail.</summary>
        public string? MaskedNik { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public Guid? CompanyGuarantorId { get; set; }
        public string? CompanyGuarantorName { get; set; }
        public Guid? McuDoctorId { get; set; }
        public string? McuDoctorName { get; set; }

        /// <summary>Komponen tuntas — selesai atau dibatalkan resmi.</summary>
        public int CompletedComponentCount { get; set; }
        public int ComponentCount { get; set; }

        /// <summary>Contoh <c>3 dari 4</c>; kosong selama order belum berkomponen.</summary>
        public string? CompletionLabel { get; set; }
        public McuOrderStatus OrderStatus { get; set; }
        public string OrderStatusLabel { get; set; } = string.Empty;
        public McuOrderSource Source { get; set; }
        public string SourceLabel { get; set; } = string.Empty;
        public int Version { get; set; }
    }

    public class McuOrderDetailResponse
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public McuOrderStatus OrderStatus { get; set; }
        public string OrderStatusLabel { get; set; } = string.Empty;
        public int Version { get; set; }

        // Peserta — NIK utuh hanya di detail berizin.
        public Guid? PatientId { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string? ParticipantNik { get; set; }
        public DateOnly? ParticipantBirthDate { get; set; }
        public string? ParticipantPhone { get; set; }
        public McuPatientMatchMethod? PatientMatchMethod { get; set; }
        public string? PatientMatchMethodLabel { get; set; }

        // Paket, penjamin, harga
        public Guid PackageId { get; set; }
        public string PackageCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public Guid? CompanyGuarantorId { get; set; }

        /// <summary>Kosong = perorangan.</summary>
        public string? CompanyGuarantorName { get; set; }
        public Guid? AppliedTariffId { get; set; }
        public string? AppliedTariffName { get; set; }
        public decimal? AppliedPrice { get; set; }

        /// <summary>Dasar mulai pemeriksaan — hanya terisi sesudah pemeriksaan dimulai.</summary>
        public McuClearanceBasis? ClearanceBasis { get; set; }
        public string? ClearanceBasisLabel { get; set; }

        // Sumber dan jadwal
        public McuOrderSource Source { get; set; }
        public string SourceLabel { get; set; } = string.Empty;
        public Guid? SourceEncounterId { get; set; }
        public string? SourceEncounterNumber { get; set; }
        public string? RecommendingDoctorName { get; set; }
        public Guid? BookingImportId { get; set; }
        public DateOnly ExaminationDate { get; set; }
        public DateTime? ArrivalConfirmedAt { get; set; }

        // Kehadiran dan dokter
        public Guid? EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public McuCheckInChannel? CheckInChannel { get; set; }
        public string? CheckInChannelLabel { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public Guid? McuDoctorId { get; set; }
        public string? McuDoctorName { get; set; }
        public DateTime? ExaminationStartedAt { get; set; }
        public DateTime? NoShowAt { get; set; }
        public string? CancelReason { get; set; }

        public int CompletedComponentCount { get; set; }
        public int ComponentCount { get; set; }
        public string? CompletionLabel { get; set; }
        public List<McuOrderComponentResponse> Components { get; set; } = new();
        public List<McuOrderRequirementCheckResponse> RequirementChecks { get; set; } = new();
    }

    public class McuOrderComponentResponse
    {
        public Guid Id { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public McuComponentType ComponentType { get; set; }
        public string ComponentTypeLabel { get; set; } = string.Empty;

        /// <summary><c>true</c> = bagian paket; <c>false</c> = di luar paket, ditagih terpisah.</summary>
        public bool IsPackageCovered { get; set; }
        public McuOrderComponentStatus ComponentStatus { get; set; }
        public string ComponentStatusLabel { get; set; } = string.Empty;
        public DateOnly ScheduledDate { get; set; }
        public Guid? LabOrderId { get; set; }
        public string? LabOrderNumber { get; set; }
        public Guid? RadOrderId { get; set; }
        public string? RadOrderNumber { get; set; }
        public string? PerformingDoctorName { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CancelReason { get; set; }
    }

    public class McuOrderRequirementCheckResponse
    {
        public Guid Id { get; set; }
        public Guid McuOrderComponentId { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public string RequirementText { get; set; } = string.Empty;
        public McuRequirementOutcome Outcome { get; set; }
        public string OutcomeLabel { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CheckedAt { get; set; }
        public string? CheckedByName { get; set; }
    }

    /// <summary>Satu baris riwayat: perpindahan status atau penetapan dokter MCU.</summary>
    public class McuOrderHistoryResponse
    {
        /// <summary><c>Status</c> atau <c>Doctor</c>.</summary>
        public string Kind { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public McuOrderStatus? FromStatus { get; set; }
        public string? FromStatusLabel { get; set; }
        public McuOrderStatus? ToStatus { get; set; }
        public string? ToStatusLabel { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? DoctorEndedAt { get; set; }
        public Guid? ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public string? Reason { get; set; }
    }

    // =====================================================================
    // MCU-API-v1 — grup Mcu Order, booking / jadwal ulang / batal (BE-MCU-011)
    // =====================================================================

    /// <summary>
    /// Booking dari telepon/WhatsApp. Isi <see cref="PatientId"/> untuk pasien terdaftar, <b>atau</b>
    /// identitas sementara — nama, NIK 16 digit, tanggal lahir — untuk peserta baru (<c>MCU-DEC-033</c>).
    /// </summary>
    public class CreateMcuBookingRequest
    {
        public Guid PackageId { get; set; }
        public DateOnly ExaminationDate { get; set; }
        public Guid? PatientId { get; set; }
        public string? ParticipantName { get; set; }
        public string? ParticipantNik { get; set; }
        public DateOnly? ParticipantBirthDate { get; set; }
        public string? ParticipantPhone { get; set; }

        /// <summary>Kosong = perorangan.</summary>
        public Guid? CompanyGuarantorId { get; set; }
    }

    /// <summary>
    /// Pendaftaran <i>walk-in</i> di admisi (<c>BE-MCU-014</c>): order langsung <i>Hadir</i> hari ini.
    /// Pasien wajib sudah terdaftar — peserta baru didaftarkan lebih dulu lewat Patient Management.
    /// Isian check-in sama dengan <see cref="McuCheckInRequest"/>.
    /// </summary>
    public class CreateMcuWalkInRequest
    {
        public Guid PackageId { get; set; }
        public Guid PatientId { get; set; }
        public string? ParticipantPhone { get; set; }

        /// <summary>Kosong = perorangan.</summary>
        public Guid? CompanyGuarantorId { get; set; }

        /// <summary>Wajib (<c>VAL-MCU-26</c>).</summary>
        public Guid? McuDoctorId { get; set; }
        public Guid? PatientCompanyGuarantorId { get; set; }
        public List<McuRequirementOutcomeRequest> RequirementOutcomes { get; set; } = new();
    }

    public class RescheduleMcuOrderRequest
    {
        public DateOnly NewDate { get; set; }

        /// <summary><c>Version</c> order yang dibaca layar (<c>VAL-MCU-46</c>).</summary>
        public int Version { get; set; }

        /// <summary>Opsional; ikut tercatat di riwayat.</summary>
        public string? Reason { get; set; }
    }

    public class CancelMcuOrderRequest
    {
        /// <summary>Wajib (<c>VAL-MCU-23</c>).</summary>
        public string Reason { get; set; } = string.Empty;
        public int Version { get; set; }
    }

    public class McuOrderFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public List<McuOptionItemResponse> StatusOptions { get; set; } = new();
        public List<McuOptionItemResponse> SourceOptions { get; set; } = new();

        /// <summary>Perusahaan yang punya harga paket MCU atau order MCU.</summary>
        public List<McuOptionItemResponse> CompanyOptions { get; set; } = new();

        /// <summary>Dokter yang pernah menjadi dokter MCU order atau dokter jaga.</summary>
        public List<McuOptionItemResponse> McuDoctorOptions { get; set; } = new();
        public List<McuOptionItemResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }
}
