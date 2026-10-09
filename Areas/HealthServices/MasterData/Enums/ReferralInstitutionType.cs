namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    /// <summary>
    /// Jenis fasilitas perujuk mitra (<c>DEC-FRJ-001</c>). Nilai <c>0</c> hanya dimiliki baris
    /// lama yang dibuat sebelum jenis fasilitas diwajibkan; master baru selalu memilih salah satu
    /// nilai lain.
    /// </summary>
    public enum ReferralInstitutionType
    {
        Unknown = 0,
        Clinic = 1,
        CommunityHealthCenter = 2,
        Hospital = 3,
        IndependentPractice = 4,
        Laboratory = 5,
        Other = 99
    }
}
