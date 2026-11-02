using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs
{
    // =====================================================================
    // MCU-API-v1 — grup Mcu Check In (BE-MCU-013)
    // =====================================================================

    public static class McuPatientMatchStatuses
    {
        /// <summary>Order sudah tertaut pasien sejak booking.</summary>
        public const string Linked = "Linked";

        /// <summary>Ada pasien dengan NIK dan tanggal lahir yang sama.</summary>
        public const string FullMatch = "FullMatch";

        /// <summary>NIK sama, tanggal lahir berbeda — petugas wajib memeriksa KTP lalu mengonfirmasi.</summary>
        public const string NikMatchBirthDateDiffers = "NikMatchBirthDateDiffers";

        /// <summary>Belum ada pasien — daftarkan lewat Patient Management lalu check-in dengan pasien itu.</summary>
        public const string NoMatch = "NoMatch";
    }

    public class McuPatientMatchResponse
    {
        public Guid OrderId { get; set; }
        public string MatchStatus { get; set; } = string.Empty;
        public string MatchStatusLabel { get; set; } = string.Empty;

        /// <summary>Identitas sementara saat booking, dipakai mengisi awal pendaftaran pasien baru.</summary>
        public string ParticipantName { get; set; } = string.Empty;
        public string? ParticipantNik { get; set; }
        public DateOnly? ParticipantBirthDate { get; set; }
        public string? ParticipantPhone { get; set; }
        public List<McuPatientCandidateResponse> Candidates { get; set; } = new();
    }

    public class McuPatientCandidateResponse
    {
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? IdentityNumber { get; set; }
        public DateOnly? BirthDate { get; set; }
    }

    public class McuCheckInPreviewResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int Version { get; set; }
        public McuOrderStatus OrderStatus { get; set; }
        public string OrderStatusLabel { get; set; } = string.Empty;
        public DateOnly ExaminationDate { get; set; }

        /// <summary>Kosong = check-in boleh dilakukan; selain itu kalimat penolakannya (mis. <c>VAL-MCU-24</c>).</summary>
        public string? CheckInBlockedReason { get; set; }

        public Guid? PatientId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public Guid? CompanyGuarantorId { get; set; }
        public string? CompanyGuarantorName { get; set; }

        /// <summary>Harga yang akan dipakai pada tanggal check-in (<c>MCU-DEC-020</c>, <c>025</c>).</summary>
        public Guid? TariffId { get; set; }
        public string? TariffName { get; set; }
        public decimal? Price { get; set; }

        /// <summary><c>General</c> atau <c>Company</c>.</summary>
        public string PriceSource { get; set; } = string.Empty;
        public string PriceSourceLabel { get; set; } = string.Empty;
        public DateOnly? CompanyPriceStartDate { get; set; }
        public DateOnly? CompanyPriceEndDate { get; set; }
        public bool IsTariffActive { get; set; }

        /// <summary>Isian bawaan dokter MCU = dokter jaga tanggal itu (<c>MCU-DEC-042</c>).</summary>
        public Guid? DefaultMcuDoctorId { get; set; }
        public string? DefaultMcuDoctorName { get; set; }

        public List<McuCheckInRequirementResponse> Requirements { get; set; } = new();

        /// <summary>Kartu penjamin pasien yang berlaku untuk perusahaan order — terisi bila pasien sudah tertaut.</summary>
        public List<McuCompanyCardResponse> CompanyCards { get; set; } = new();
    }

    public class McuCheckInRequirementResponse
    {
        public Guid PackageComponentRequirementId { get; set; }
        public Guid McuOrderComponentId { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public string RequirementText { get; set; } = string.Empty;

        /// <summary><c>true</c> = wajib ditandai (<c>VAL-MCU-27</c>).</summary>
        public bool IsConfirmationRequired { get; set; }
    }

    public class McuCompanyCardResponse
    {
        public Guid PatientCompanyGuarantorId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class McuCheckInRequest
    {
        /// <summary>
        /// Pasien yang ditautkan. Wajib untuk order identitas sementara: pasien cocok dari
        /// <c>patient-matches</c>, atau pasien baru yang baru saja didaftarkan lewat Patient Management.
        /// Kosong untuk order yang sudah tertaut sejak booking.
        /// </summary>
        public Guid? PatientId { get; set; }

        /// <summary>Petugas sudah memeriksa KTP dan mengonfirmasi pasien meski tanggal lahir/NIK berbeda (<c>VAL-MCU-25</c>).</summary>
        public bool MatchConfirmation { get; set; }

        /// <summary>Wajib (<c>VAL-MCU-26</c>).</summary>
        public Guid? McuDoctorId { get; set; }

        /// <summary>Kartu penjamin perusahaan pasien; kosong = dipilih otomatis (kartu utama lebih dulu).</summary>
        public Guid? PatientCompanyGuarantorId { get; set; }

        public List<McuRequirementOutcomeRequest> RequirementOutcomes { get; set; } = new();

        public int Version { get; set; }
    }

    /// <summary>Ganti dokter MCU oleh kepala unit (<c>MCU-DEC-040</c>, <c>BE-MCU-032</c>).</summary>
    public class ReassignMcuDoctorRequest
    {
        public Guid DoctorId { get; set; }

        /// <summary>Wajib (<c>VAL-MCU-40</c>), contoh <i>Dokter selesai jaga</i>.</summary>
        public string Reason { get; set; } = string.Empty;

        public int Version { get; set; }
    }

    public class McuRequirementOutcomeRequest
    {
        public Guid PackageComponentRequirementId { get; set; }
        public McuRequirementOutcome Outcome { get; set; }

        /// <summary>Wajib bila <c>ProceedWithNote</c> (<c>VAL-MCU-28</c>).</summary>
        public string? Note { get; set; }

        /// <summary>Wajib bila <c>ComponentRescheduled</c>: tanggal susulan komponen, tanpa memakai kuota.</summary>
        public DateOnly? RescheduledDate { get; set; }
    }
}
