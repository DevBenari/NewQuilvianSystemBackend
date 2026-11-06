namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs
{
    public class NvwFloorBoardResponse
    {
        public string FloorCode { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public DateTime AsOfUtc { get; set; }
        public bool IntervalsConfigured { get; set; }
        public List<NvwWardBoardResponse> Wards { get; set; } = new();
    }

    public class NvwWardBoardResponse
    {
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;
        public bool RosterAvailable { get; set; }
        public List<NvwOnDutyNurseResponse> OnDutyNurses { get; set; } = new();
        public List<NvwBedCardResponse> Beds { get; set; } = new();
    }

    public class NvwOnDutyNurseResponse
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? ShiftLabel { get; set; }
    }

    public class NvwBedCardResponse
    {
        public Guid BedId { get; set; }
        public string BedCode { get; set; } = string.Empty;
        public string BedName { get; set; } = string.Empty;
        public string? RoomName { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public bool IsNewbornBed { get; set; }
        public NvwMaskedPatientResponse? Patient { get; set; }
        public string? ResponsibleNurseName { get; set; }
        public bool HasResponsibleNurse { get; set; }
        public NvwVitalSignCardResponse? VitalSign { get; set; }
        public bool IsCritical { get; set; }
        public bool HasHighFallRisk { get; set; }
        public bool HasActiveAllergy { get; set; }
        public bool RequiresIsolation { get; set; }
        public bool IsolationMismatch { get; set; }
    }

    public class NvwMaskedPatientResponse
    {
        public string Initials { get; set; } = string.Empty;
        public string GenderCode { get; set; } = string.Empty;
        public string AgeLabel { get; set; } = string.Empty;
    }

    public class NvwVitalSignCardResponse
    {
        public DateTime ObservedAtUtc { get; set; }
        public int AgeMinutes { get; set; }
        public int? EarlyWarningScore { get; set; }
        public string EwsRiskLevel { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
        public int? Systolic { get; set; }
        public int? Diastolic { get; set; }
        public int? PulseRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public bool IsOverdue { get; set; }
        public int? OverdueMinutes { get; set; }
    }

    public class NvwBoardChangedEvent
    {
        public string EventType { get; set; } = string.Empty;
        public Guid BedId { get; set; }
        public string FloorCode { get; set; } = string.Empty;
        public DateTime OccurredAtUtc { get; set; }
    }
}
