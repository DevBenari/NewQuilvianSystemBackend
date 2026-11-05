using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs
{
    public class NvwVitalSignIntervalItemRequest
    {
        public EwsRiskLevel EwsRiskLevel { get; set; }
        public int IntervalMinutes { get; set; }
    }

    public class NvwVitalSignIntervalReplaceRequest
    {
        public List<NvwVitalSignIntervalItemRequest>? Items { get; set; }
        public string? RatificationReference { get; set; }
        public DateOnly? RatifiedOn { get; set; }
    }

    public class NvwVitalSignIntervalItemResponse
    {
        public EwsRiskLevel EwsRiskLevel { get; set; }
        public string EwsRiskLevelLabel { get; set; } = string.Empty;
        public int IntervalMinutes { get; set; }
    }

    public class NvwVitalSignIntervalSetResponse
    {
        public List<NvwVitalSignIntervalItemResponse> Items { get; set; } = new();
        public string? RatificationReference { get; set; }
        public DateOnly? RatifiedOn { get; set; }
        public string? RecordedByName { get; set; }
        public DateTime? RecordedAt { get; set; }
        public bool IsConfigured { get; set; }
    }
}
