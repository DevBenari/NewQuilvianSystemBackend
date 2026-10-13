namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    public enum EmergencyAdmissionRequestStatus
    {
        Draft = 1,
        AwaitingSignature = 2,
        Signed = 3,
        Cancelled = 4
    }

    public enum EmergencyAdmissionRequestDisplayStatus
    {
        Draft = 1,
        AwaitingSignature = 2,
        Sent = 3,
        Completed = 4,
        Cancelled = 5,
        CancelledByAdmission = 6
    }

    public enum EmergencyAdmissionAllergyState
    {
        NotRecorded = 1,
        Recorded = 2
    }
}
