using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs
{
    public class NvwBedTabletRuntimeResponse
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string BedCode { get; set; } = string.Empty;
        public string BedName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string FloorCode { get; set; } = string.Empty;
        public bool IsBedOccupied { get; set; }
        public NvwTabletCallResponse? ActiveCall { get; set; }
        public int RefreshIntervalSeconds { get; set; }
        public DateTime ServerTimeUtc { get; set; }
    }

    public class NvwTabletCallResponse
    {
        public Guid CallId { get; set; }
        public NvwPatientCallPriority Priority { get; set; }
        public string PriorityLabel { get; set; } = string.Empty;
        public NvwPatientCallStatus CallStatus { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime FirstPressedAt { get; set; }
        public int WaitingSeconds { get; set; }
        public int PressCount { get; set; }
        public string? ClaimedByName { get; set; }
        public bool CanCancel { get; set; }
        public bool CanEscalate { get; set; }
        public bool CanConfirmNurseArrived { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
    }

    public sealed class NvwBedTabletLoginContext
    {
        public bool IsBedTabletAccount { get; init; }
        public Guid? TabletId { get; init; }
        public string DeviceCode { get; init; } = string.Empty;
        public string SessionStamp { get; init; } = string.Empty;
        public bool CanLogin { get; init; }
        public string? BlockReason { get; init; }

        public static NvwBedTabletLoginContext None() => new();
    }
}
