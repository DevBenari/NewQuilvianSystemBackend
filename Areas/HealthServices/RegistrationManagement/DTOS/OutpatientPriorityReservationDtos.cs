using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    // rawat-jalan Amendment RP — kontrak RJ-PRI-RESV-001@1.0.0, api-contract.md
    // "Outpatient Priority Reservation". Tanggal kalender (visitDate) dalam WIB, format yyyy-MM-dd.

    /// <summary>Penyaring daftar reservasi. Tanpa tanggal berarti hari ini (WIB).</summary>
    public class PriorityReservationListQuery
    {
        public DateTime? VisitDateFrom { get; set; }

        public DateTime? VisitDateTo { get; set; }

        public PriorityReservationStatus? Status { get; set; }

        /// <summary>No. RM, nama pasien, atau nomor reservasi; minimal 3 karakter.</summary>
        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class PriorityReservationListItemResponse
    {
        public Guid Id { get; set; }

        public string ReservationNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public DateTime VisitDate { get; set; }

        public PriorityReservationStatus Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public DateTime? ArrivedAt { get; set; }

        public PriorityReservationArrivalSource? ArrivalSource { get; set; }

        public bool IsScreened { get; set; }

        public string? MembershipTierCodeSnapshot { get; set; }

        /// <summary>
        /// <c>true</c> bila membership snapshot sudah Dibatalkan: reservasi tetap ada, tetapi
        /// antreannya nanti Reguler (RJ-PRI-DEC-024).
        /// </summary>
        public bool IsPriorityRevoked { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid RowVersion { get; set; }

        /// <summary>Waktu reservasi dibuat (UTC). Kontrak <c>1.1.0</c>.</summary>
        public DateTime CreatedAt { get; set; }

        public string? CreatedByName { get; set; }

        /// <summary>Waktu perubahan terakhir (UTC); kosong bila belum pernah berubah sejak dibuat.</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Nama petugas yang terakhir mengubah, atau "Link Pasien"/"Sistem" bila perubahan terakhir
        /// datang dari link konfirmasi atau penutupan otomatis.
        /// </summary>
        public string? UpdatedByName { get; set; }
    }

    public class PriorityReservationSummaryResponse
    {
        /// <summary>Sama dengan <see cref="VisitDateFrom"/>; dipertahankan untuk kontrak <c>1.0.0</c>.</summary>
        public DateTime VisitDate { get; set; }

        public DateTime VisitDateFrom { get; set; }

        public DateTime VisitDateTo { get; set; }

        public int AwaitingArrival { get; set; }

        public int Arrived { get; set; }

        public int Converted { get; set; }

        public int Cancelled { get; set; }

        public int NoShow { get; set; }

        public int RegisteredElsewhere { get; set; }
    }

    public class PriorityReservationScreeningResponse
    {
        public bool? Cough { get; set; }

        public bool? Fever { get; set; }

        public bool? FallRisk { get; set; }

        public DateTime? ScreenedAt { get; set; }

        public string? ScreenedByName { get; set; }
    }

    public class PriorityReservationHistoryResponse
    {
        public string EventCode { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;

        public PriorityReservationStatus? FromStatus { get; set; }

        public PriorityReservationStatus ToStatus { get; set; }

        public PriorityReservationEventSource Source { get; set; }

        public string? ActorName { get; set; }

        public string? Reason { get; set; }

        /// <summary>Tanggal kunjungan sebelum diganti. Hanya terisi pada <c>RESCHEDULED</c>.</summary>
        public DateTime? FromVisitDate { get; set; }

        /// <summary>Tanggal kunjungan sesudah diganti. Hanya terisi pada <c>RESCHEDULED</c>.</summary>
        public DateTime? ToVisitDate { get; set; }

        public DateTime OccurredAt { get; set; }
    }

    public class PriorityReservationDetailResponse : PriorityReservationListItemResponse
    {
        public PriorityReservationScreeningResponse Screening { get; set; } = new();

        public DateTime? ClosedAt { get; set; }

        public string? ClosureReason { get; set; }

        public DateTime ArrivalCodeIssuedAt { get; set; }

        public bool CanMarkArrival { get; set; }

        public bool CanRevertArrival { get; set; }

        public bool CanScreen { get; set; }

        public bool CanReissueCode { get; set; }

        public bool CanComplete { get; set; }

        public bool CanCancel { get; set; }

        /// <summary>
        /// Menunggu Kedatangan dan hari ini (WIB) sebelum tanggal kunjungan (RJ-PRI-DEC-034).
        /// Status member dan tanggal baru baru diperiksa saat simpan.
        /// </summary>
        public bool CanReschedule { get; set; }

        public List<PriorityReservationHistoryResponse> Histories { get; set; } = new();
    }

    public class PriorityReservationEligibilityResponse
    {
        public const string Eligible = "ELIGIBLE";
        public const string NotActiveMember = "NOT_ACTIVE_MEMBER";
        public const string OpenReservationExists = "OPEN_RESERVATION_EXISTS";
        public const string ActiveEncounterExists = "ACTIVE_ENCOUNTER_EXISTS";
        public const string DateOutOfRange = "DATE_OUT_OF_RANGE";

        public bool IsEligible { get; set; }

        public string ReasonCode { get; set; } = Eligible;

        public string Message { get; set; } = string.Empty;

        public string? MembershipTierCode { get; set; }

        public string? ExistingReservationNumber { get; set; }
    }

    public class CreatePriorityReservationRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public DateTime VisitDate { get; set; }
    }

    /// <summary>
    /// Ganti tanggal sebelum hari kunjungan (RJ-PRI-DEC-034 s.d. 036). <see cref="NewVisitDate"/>
    /// kosong dan alasan yang tidak sah dijawab service dengan pesan validasi, bukan oleh model binding.
    /// </summary>
    public class ReschedulePriorityReservationRequest
    {
        [Required]
        public Guid ExpectedRowVersion { get; set; }

        [Required]
        public DateTime NewVisitDate { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>
    /// Jawaban buat, kirim ulang, dan ganti tanggal. <see cref="ArrivalLink"/> hanya muncul di
    /// respons ini; kode aslinya tidak disimpan (INV-RP-05).
    /// </summary>
    public class PriorityReservationIssuedResponse
    {
        public PriorityReservationDetailResponse Reservation { get; set; } = new();

        public string ArrivalLink { get; set; } = string.Empty;

        /// <summary>Teks siap kirim: nomor reservasi, tanggal, dan link. Tanpa poli dan dokter.</summary>
        public string WhatsAppMessageText { get; set; } = string.Empty;

        public string? PatientPhoneNumber { get; set; }
    }

    public class PriorityReservationRowVersionRequest
    {
        [Required]
        public Guid ExpectedRowVersion { get; set; }
    }

    public class PriorityReservationReasonRequest
    {
        [Required]
        public Guid ExpectedRowVersion { get; set; }

        public string? Reason { get; set; }
    }

    public class PriorityReservationScreeningRequest
    {
        [Required]
        public Guid ExpectedRowVersion { get; set; }

        public bool? Cough { get; set; }

        public bool? Fever { get; set; }

        public bool? FallRisk { get; set; }
    }

    /// <summary>
    /// Jawaban halaman publik link konfirmasi (BE-RJP-012, RJ-PRI-FE-001). Sengaja hanya nomor
    /// reservasi, tanggal, keadaan, dan pesan: tanpa nama, No. RM, NIK, tingkat member, poli,
    /// dokter, maupun ID internal (R-RP-13).
    /// </summary>
    public class PriorityReservationArrivalPublicResponse
    {
        public const string StateNotYetOpen = "NOT_YET_OPEN";
        public const string StateReady = "READY";
        public const string StateConfirmed = "CONFIRMED";

        public string ReservationNumber { get; set; } = string.Empty;

        public DateTime VisitDate { get; set; }

        public string State { get; set; } = StateReady;

        public DateTime? ConfirmedAt { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
