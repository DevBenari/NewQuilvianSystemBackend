namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceSchedulerOptions
    {
        public bool Enabled { get; set; } = true;
        public int PollIntervalSeconds { get; set; } = 30;
        public bool AutoEnqueueDailyProcessing { get; set; } = true;
        public bool AutoClosePeriods { get; set; } = false;
        public string TimeZoneId { get; set; } = "Asia/Jakarta";
        public int DailyProcessingHour { get; set; } = 2;
        public int DailyProcessingMinute { get; set; } = 15;
        public int ProcessDaysBack { get; set; } = 1;
        public int MaximumCatchUpDays { get; set; } = 7;
        public int DefaultMaximumRetryCount { get; set; } = 3;
        public int RetryDelayMinutes { get; set; } = 15;
        public int RunningJobTimeoutMinutes { get; set; } = 60;
        public string? SystemActorUserId { get; set; }
        public string WorkerInstanceName { get; set; } = "quilvian-attendance-scheduler";
    }
}
