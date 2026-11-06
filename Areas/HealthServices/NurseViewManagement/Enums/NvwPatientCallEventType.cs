namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums
{
    public enum NvwPatientCallEventType
    {
        Pressed = 1,
        PressedAgain = 2,
        EscalatedToEmergency = 3,
        Claimed = 4,
        TakenOver = 5,
        Released = 6,
        NurseArrived = 7,
        ClosedFromConsole = 8,
        CancelledByPatient = 9,
        ClosedBySystem = 10
    }
}
