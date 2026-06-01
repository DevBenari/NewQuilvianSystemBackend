namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    public enum EncounterStatus
    {
        Draft = 0,
        Registered = 1,
        Queued = 2,
        WaitingForNurse = 3,
        InNurseScreening = 4,
        WaitingForDoctor = 5,
        InConsultation = 6,
        ConsultationCompleted = 7,
        Billing = 8,
        Completed = 9,
        Cancelled = 10,
        NoShow = 11
    }
}
