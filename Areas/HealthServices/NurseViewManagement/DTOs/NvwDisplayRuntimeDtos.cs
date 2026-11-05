namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs
{
    public class NvwDisplayRuntimeCurrentResponse
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string FloorCode { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public int RefreshIntervalSeconds { get; set; }
        public int StaleAfterSeconds { get; set; }
        public DateTime ServerTimeUtc { get; set; }
    }

    public sealed class NvwDeviceLoginContext
    {
        public bool IsNurseViewAccount { get; init; }
        public Guid? DeviceId { get; init; }
        public string DeviceCode { get; init; } = string.Empty;
        public string SessionStamp { get; init; } = string.Empty;
        public bool CanLogin { get; init; }
        public string? BlockReason { get; init; }

        public static NvwDeviceLoginContext None() => new();
    }
}
