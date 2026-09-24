namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Services
{
    public class AccAccountingEventSchedulerOptions
    {
        public Guid? SystemActorUserId { get; set; }

        public int GracePeriodSeconds { get; set; } = 120;
    }
}
