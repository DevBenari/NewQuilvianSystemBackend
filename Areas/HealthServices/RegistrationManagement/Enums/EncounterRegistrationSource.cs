namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    public enum EncounterRegistrationSource
    {
        FrontDesk = 1,
        Kiosk = 2,
        Appointment = 3,
        MobileApp = 4,
        WalkIn = 5,

        /// <summary>Kunjungan hasil pelengkapan Reservasi Prioritas (RJ-PRI-DEC-009/010).</summary>
        PriorityReservation = 6
    }
}
