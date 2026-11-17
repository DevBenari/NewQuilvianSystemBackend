namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs
{
    public class NvwPatientCallWaitThresholdReplaceRequest
    {
        public int? WarningAfterMinutes { get; set; }
        public int? CriticalAfterMinutes { get; set; }
        public string? RatificationReference { get; set; }
        public DateOnly? RatifiedOn { get; set; }
    }

    public class NvwPatientCallWaitThresholdResponse
    {
        public bool IsConfigured { get; set; }
        public int? WarningAfterMinutes { get; set; }
        public int? CriticalAfterMinutes { get; set; }
        public string? RatificationReference { get; set; }
        public DateOnly? RatifiedOn { get; set; }
        public string? RecordedByName { get; set; }
        public DateTime? RecordedAt { get; set; }
    }
}
