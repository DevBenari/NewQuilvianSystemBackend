namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums
{
    public enum NvwPatientCallCloseReason
    {
        NurseArrived = 1,
        TabletUnavailable = 2,
        HandledNotConfirmedAtBed = 3,
        Other = 4,
        CancelledByPatient = 5,
        PatientTransferred = 6,
        PatientDischarged = 7,
        AdmissionCancelled = 8
    }
}
