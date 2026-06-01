namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    public enum QueueStatus
    {
        WaitingForNurse = 1,
        CalledByNurse = 2,
        InNurseScreening = 3,
        WaitingForDoctor = 4,
        CalledByDoctor = 5,
        InConsultation = 6,
        Skipped = 7,
        NoShow = 8,
        Cancelled = 9,
        Completed = 10
    }
}
