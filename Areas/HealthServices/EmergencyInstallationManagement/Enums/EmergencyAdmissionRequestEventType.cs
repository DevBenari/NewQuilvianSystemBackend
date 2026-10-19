namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    public enum EmergencyAdmissionRequestEventType
    {
        Opened = 1,
        SignatureRequested = 2,
        Returned = 3,
        Signed = 4,
        Cancelled = 5,
        CancelledAfterSend = 6,
        AutoCancelled = 7,
        AdmissionRejected = 8
    }
}
