namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimeSettingDto
    {
        public decimal DailyMaxHoursRegular { get; set; } = 4m;
        public decimal WeeklyMaxHoursRegular { get; set; } = 18m;
        public int RoundingIntervalMinutes { get; set; } = 30;
        public int MinimumThresholdMinutes { get; set; } = 30;
        public int CtoValidityDays { get; set; } = 90;
        public int MonthlyCutoffDay { get; set; } = 20;
        public int BiometricDiscrepancyTolerance { get; set; } = 30;
        public int PostClaimCitoGraceDays { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
    }

    public class UpdateOvertimeSettingCommand
    {
        public decimal DailyMaxHoursRegular { get; set; } = 4m;
        public decimal WeeklyMaxHoursRegular { get; set; } = 18m;
        public int RoundingIntervalMinutes { get; set; } = 30;
        public int MinimumThresholdMinutes { get; set; } = 30;
        public int CtoValidityDays { get; set; } = 90;
        public int MonthlyCutoffDay { get; set; } = 20;
        public int BiometricDiscrepancyTolerance { get; set; } = 30;
        public int PostClaimCitoGraceDays { get; set; } = 1;
    }
}
