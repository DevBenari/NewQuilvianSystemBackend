using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs
{
    public class HmdReadinessQuery
    {
        public Guid? ServiceUnitId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public HmdShift? Shift { get; set; }
        public HmdReadinessStatus? ReadinessStatus { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateHmdUnitReadinessRequest
    {
        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        public DateOnly ReadinessDate { get; set; }

        [Required]
        public HmdShift Shift { get; set; }
    }

    public class SaveHmdReadinessItemInput
    {
        [Required]
        public Guid ReadinessItemId { get; set; }

        [Required]
        public HmdChecklistResult Result { get; set; }

        /// <summary>Tanggal hasil untuk butir yang punya masa berlaku, misalnya uji air.</summary>
        public DateOnly? ResultDate { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class SaveHmdReadinessItemsRequest
    {
        [Required, MinLength(1)]
        public List<SaveHmdReadinessItemInput> Items { get; set; } = new();
    }

    public class DeclareNotReadyRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class HmdUnitReadinessResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public DateOnly ReadinessDate { get; set; }
        public HmdShift Shift { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public HmdReadinessStatus ReadinessStatus { get; set; }
        public string ReadinessStatusName { get; set; } = string.Empty;
        public Guid? DeclaredByUserId { get; set; }
        public string? DeclaredByName { get; set; }
        public DateTime? DeclaredAt { get; set; }
        public string? NotReadyReason { get; set; }
        public int MandatoryItemCount { get; set; }
        public int MandatoryItemMetCount { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class HmdUnitReadinessItemResponse
    {
        public Guid Id { get; set; }
        public Guid ReadinessItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public HmdReadinessCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public bool RequiresResultDate { get; set; }
        public int CheckSequence { get; set; }
        public HmdChecklistResult Result { get; set; }
        public string ResultName { get; set; } = string.Empty;
        public DateOnly? ResultDate { get; set; }
        public DateTime? ResultValidUntil { get; set; }
        public bool? IsResultExpired { get; set; }
        public string? ReferenceNumber { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? Note { get; set; }
    }

    public class HmdUnitReadinessDetailResponse : HmdUnitReadinessResponse
    {
        public int WaterResultValidityHours { get; set; }
        public List<HmdUnitReadinessItemResponse> Items { get; set; } = new();
        public List<string> AvailableActions { get; set; } = new();
    }

    /// <summary>Butir yang menahan pernyataan siap, disertakan pada rincian penolakan <c>422</c>.</summary>
    public class HmdReadinessBlockingItem
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
