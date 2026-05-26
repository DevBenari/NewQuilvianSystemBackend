namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums
{
    public enum LeaveType
    {
        Unknown = 0,

        AnnualLeave = 1,          // Cuti tahunan
        SickLeave = 2,            // Cuti sakit
        MaternityLeave = 3,       // Cuti melahirkan
        PaternityLeave = 4,       // Cuti ayah
        MarriageLeave = 5,        // Cuti menikah
        BereavementLeave = 6,     // Cuti kedukaan
        UnpaidLeave = 7,          // Cuti tidak dibayar
        OfficialDuty = 8,         // Dinas / tugas resmi
        TrainingLeave = 9,        // Pelatihan
        Other = 99
    }
}
