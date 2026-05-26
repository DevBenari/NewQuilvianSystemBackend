namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    public enum PatientRegistrationSource
    {
        Unknown = 0,
        Kiosk = 1,
        OutpatientAdmission = 2,
        InpatientAdmission = 3,
        EmergencyAdmission = 4,
        Marketing = 5,
        Migration = 6,
        Other = 99
    }
}
