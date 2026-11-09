using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs
{
    public class NvwConsoleCallListResponse
    {
        public string FloorCode { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public bool ThresholdsConfigured { get; set; }
        public List<NvwConsoleCallResponse> Items { get; set; } = new();
        public DateTime ServerTimeUtc { get; set; }
    }

    public class NvwConsoleCallResponse
    {
        public Guid CallId { get; set; }
        public Guid BedId { get; set; }
        public string BedCode { get; set; } = string.Empty;
        public string BedName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public NvwMaskedPatientResponse? Patient { get; set; }
        public NvwPatientCallPriority Priority { get; set; }
        public string PriorityLabel { get; set; } = string.Empty;
        public NvwPatientCallStatus CallStatus { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime FirstPressedAt { get; set; }
        public int WaitingSeconds { get; set; }
        public int PressCount { get; set; }
        public Guid? ClaimedByUserId { get; set; }
        public string? ClaimedByName { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public bool IsClaimedByMe { get; set; }
        public string WaitLevel { get; set; } = string.Empty;
        public List<string> AvailableActions { get; set; } = new();
        public int Version { get; set; }
    }

    public class NvwConsoleCallVersionRequest
    {
        [Required]
        public int Version { get; set; }
    }

    public class NvwConsoleCloseRequest
    {
        public NvwPatientCallCloseReason? Reason { get; set; }

        public string? Note { get; set; }

        [Required]
        public int Version { get; set; }
    }

    public class NvwPatientCallChangedEvent
    {
        public Guid CallId { get; set; }
        public Guid BedId { get; set; }
        public string FloorCode { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public DateTime OccurredAtUtc { get; set; }
    }
}
