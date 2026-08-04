namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveAccrualSchedulerOptions
    {
        public bool Enabled { get; set; } = true;
        public int PollIntervalSeconds { get; set; } = 60;
        public bool AutoEnqueueDueAccruals { get; set; } = true;
        public string TimeZoneId { get; set; } = "Asia/Jakarta";
        public int DailyEnqueueHour { get; set; } = 1;
        public int DailyEnqueueMinute { get; set; } = 30;
        public int LookBackDays { get; set; } = 3;
        public int DefaultMaximumRetryCount { get; set; } = 3;
        public int RunningRunTimeoutMinutes { get; set; } = 60;
        public Guid? SystemActorUserId { get; set; }
        public string WorkerInstanceName { get; set; } = "quilvian-leave-accrual-scheduler";
    }
}
