using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs
{
    public class HmdOrderPagedQuery
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? InpEpisodeId { get; set; }
        public HmdOrderStatus? OrderStatus { get; set; }
        public HmdOrderPriority? Priority { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateHmdOrderRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>Kunjungan yang sah milik pasien; wajib (<c>HMD-VAL-001</c>).</summary>
        public Guid? EncounterId { get; set; }

        public Guid? InpEpisodeId { get; set; }

        /// <summary>Dokter yang meminta, bila pembuat permintaannya perawat.</summary>
        public Guid? RequestingDoctorId { get; set; }

        public HmdOrderPriority Priority { get; set; } = HmdOrderPriority.Routine;

        /// <summary>Indikasi klinis; wajib (<c>HMD-VAL-001</c>).</summary>
        [MaxLength(1000)]
        public string? ClinicalReason { get; set; }

        public DateOnly? RequestedDate { get; set; }
    }

    public class AcceptHmdOrderRequest
    {
        /// <summary>Program HD pasien yang menampung permintaan ini, bila sudah ada.</summary>
        public Guid? EpisodeId { get; set; }
    }

    public class HoldHmdOrderRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class RejectHmdOrderRequest
    {
        [MaxLength(1000)]
        public string? ClinicalReason { get; set; }
    }

    public class CancelHmdOrderRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class HmdOrderResponse
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public Guid? InpEpisodeId { get; set; }
        public Guid? EpisodeId { get; set; }
        public string? EpisodeNumber { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string? RequestedByName { get; set; }
        public Guid? RequestingDoctorId { get; set; }
        public string? RequestingDoctorName { get; set; }
        public DateTime RequestedAt { get; set; }
        public HmdOrderPriority Priority { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public bool IsCito => Priority == HmdOrderPriority.Cito;
        public DateOnly? RequestedDate { get; set; }
        public HmdOrderStatus OrderStatus { get; set; }
        public string OrderStatusName { get; set; } = string.Empty;
        public DateTime? DecisionAt { get; set; }
        public Guid? DecisionByUserId { get; set; }
    }

    /// <summary>Rincian permintaan. Alasan klinis hanya tampil di sini, tidak pada daftar.</summary>
    public class HmdOrderDetailResponse : HmdOrderResponse
    {
        public string ClinicalReason { get; set; } = string.Empty;
        public HmdOrderStatus? StatusBeforeHold { get; set; }
        public string? DecisionReason { get; set; }
        public string? DecisionByName { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }

    public class HmdOrderSummaryResponse
    {
        public int TotalOrder { get; set; }
        public int RequestedOrder { get; set; }
        public int AcceptedOrder { get; set; }
        public int OnHoldOrder { get; set; }
        public int RejectedOrder { get; set; }
        public int CancelledOrder { get; set; }
        public int FulfilledOrder { get; set; }

        /// <summary>Permintaan cito yang masih menunggu diproses — kondisi yang perlu perhatian.</summary>
        public int PendingCitoOrder { get; set; }
    }

    public class HmdOrderDefaultFilterResponse
    {
        public string? Search { get; set; }
        public HmdOrderStatus? OrderStatus { get; set; }
        public HmdOrderPriority? Priority { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string SortBy { get; set; } = "requestedAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class HmdOrderFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public HmdOrderDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<HmdOptionItemResponse> OrderStatusOptions { get; set; } = new();
        public List<HmdOptionItemResponse> PriorityOptions { get; set; } = new();
        public List<HmdSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }
}
