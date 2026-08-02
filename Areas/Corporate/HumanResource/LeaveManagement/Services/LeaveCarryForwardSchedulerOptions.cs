namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveCarryForwardSchedulerOptions
    {
        public bool Enabled { get; set; } = true;
        public int PollIntervalSeconds { get; set; } = 60;
        public bool AutoEnqueueDueCarryForward { get; set; } = true;
        public bool AutoProcessDueExpiry { get; set; } = true;
        public string TimeZoneId { get; set; } = "Asia/Jakarta";
        public int DailyEnqueueHour { get; set; } = 2;
        public int DailyEnqueueMinute { get; set; } = 0;
        public int LookBackDays { get; set; } = 7;
        public int DefaultMaximumRetryCount { get; set; } = 3;
        public int RunningRunTimeoutMinutes { get; set; } = 120;
        public int ExpiryBatchSize { get; set; } = 500;
        public Guid? SystemActorUserId { get; set; }
        public string WorkerInstanceName { get; set; } = "quilvian-leave-carry-forward-scheduler";
    }
}
