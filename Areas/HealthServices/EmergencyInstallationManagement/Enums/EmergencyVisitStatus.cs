namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    public enum EmergencyVisitStatus
    {
        Arrived = 1,
        WaitingForTriage = 2,
        Triaged = 3,
        InTreatment = 4,
        UnderObservation = 5,
        AwaitingDisposition = 6,
        Disposed = 7,
        Cancelled = 8
    }
}
