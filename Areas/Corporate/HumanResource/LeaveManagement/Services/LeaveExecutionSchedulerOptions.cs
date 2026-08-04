namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveExecutionSchedulerOptions
    {
        public bool Enabled { get; set; } = true;
        public int PollIntervalSeconds { get; set; } = 60;
        public bool AutoProcessDueLeave { get; set; } = true;
        public string TimeZoneId { get; set; } = "Asia/Jakarta";
        public int DailyProcessingHour { get; set; } = 0;
        public int DailyProcessingMinute { get; set; } = 30;
        public int LookBackDays { get; set; } = 7;
        public int MaximumBatchSize { get; set; } = 500;
        public Guid? SystemActorUserId { get; set; }
        public string WorkerInstanceName { get; set; } = "quilvian-leave-execution-scheduler";
    }
}
