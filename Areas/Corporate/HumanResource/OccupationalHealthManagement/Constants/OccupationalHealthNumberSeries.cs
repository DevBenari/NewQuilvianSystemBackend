using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Constants
{
    /// <summary>
    /// Deret nomor surveilans kesehatan kerja yang dialokasikan <c>NumberSeriesAllocator</c> (<c>QBE-CODE-003</c>).
    /// Menggantikan nomor dari potongan GUID dan nomor serologi yang bisa kembar antarpegawai.
    /// </summary>
    public static class OccupationalHealthNumberSeries
    {
        public static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public const int SequenceDigits = 4;

        public const string RadiationKey = "HRD_OHS_RADIATION_DOSE";
        public const string RadiationPrefix = "RAD";
        public const string RadiationResetPolicy = NumberSeriesResetPolicies.Monthly;

        public const string TiterKey = "HRD_OHS_ANTIBODY_TITER";
        public const string TiterPrefix = "TTR";
        public const string TiterResetPolicy = NumberSeriesResetPolicies.Daily;

        public const string SerologyKey = "HRD_OHS_SEROLOGY";
        public const string SerologyPrefix = "SRV-SER";
        public const string SerologyResetPolicy = NumberSeriesResetPolicies.Monthly;
    }
}
