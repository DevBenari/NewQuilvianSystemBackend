namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeSchedulerOptions
    {
        public bool Enabled { get; set; } = false;
        public int PollIntervalSeconds { get; set; } = 30;
        public bool AutoEnqueueDailyCycle { get; set; } = true;
        public bool AutoClosePeriods { get; set; } = false;
        public bool AutoExpireCompensatory { get; set; } = true;
        public string TimeZoneId { get; set; } = "Asia/Jakarta";
        public int DailyCycleHour { get; set; } = 3;
        public int DailyCycleMinute { get; set; } = 0;
        public int ProcessDaysBack { get; set; } = 2;
        public int MaximumCatchUpDays { get; set; } = 7;
        public int CalculationDelayMinutes { get; set; } = 60;
        public int MaximumItemsPerJob { get; set; } = 500;
        public int DefaultMaximumRetryCount { get; set; } = 3;
        public int RetryDelayMinutes { get; set; } = 15;
        public int RunningJobTimeoutMinutes { get; set; } = 60;
        public string? SystemActorUserId { get; set; }
        public string WorkerInstanceName { get; set; } = "quilvian-overtime-scheduler";
    }
}
