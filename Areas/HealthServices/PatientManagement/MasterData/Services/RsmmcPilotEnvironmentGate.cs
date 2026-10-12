namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Gerbang environment rekonsiliasi MRN Pilot RSMMC: daftar izin positif yang gagal tertutup.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hanya environment bernama persis <c>Staging</c> — huruf demi huruf, termasuk huruf
    /// besar-kecilnya — yang diizinkan, untuk simulasi maupun eksekusi (`PAT-OQ-002`). Nama lain
    /// ditolak: <c>staging</c>, <c>STAGING</c>, <c>Production</c>, <c>Development</c>, kosong,
    /// salah ketik, maupun nama khusus.
    /// </para>
    /// <para>
    /// Pola <c>!environment.IsProduction()</c> sengaja tidak dipakai karena menganggap nama yang
    /// tidak dikenal sebagai "bukan produksi" (`PAT-OQ-006`). <c>IsStaging()</c> juga tidak dipakai
    /// karena membandingkan tanpa membedakan huruf besar-kecil.
    /// </para>
    /// </remarks>
    public static class RsmmcPilotEnvironmentGate
    {
        public const string AllowedEnvironmentName = "Staging";

        public static bool IsAllowed(string? environmentName) =>
            string.Equals(environmentName, AllowedEnvironmentName, StringComparison.Ordinal);
    }
}
