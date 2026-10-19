namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Konfigurasi guard reservasi nomor pasien normal selama kampanye migrasi RSMMC
    /// (`BE-PAT-MIG-003`, `PAT-DEC-066`, `PAT-DEC-067`).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bagian konfigurasi: <c>RsmmcMigrationReservationGuard</c>, contoh environment variable
    /// <c>RsmmcMigrationReservationGuard__Enabled=true</c>. Nilainya diatur pemilik saat deployment;
    /// source dan <c>appsettings*</c> tidak mengisinya.
    /// </para>
    /// <para>
    /// <see cref="Enabled"/> sengaja <c>bool?</c>. Di environment <c>Staging</c> persis, nilai yang
    /// hilang harus dibedakan dari <c>false</c> dan dari nilai salah bentuk, lalu ketiganya gagal
    /// tertutup. Tipe <c>bool</c> biasa akan diam-diam menjadi <c>false</c>.
    /// </para>
    /// <para>
    /// Lantai nomor <b>bukan</b> konfigurasi. Nilainya konstanta pada
    /// <see cref="RsmmcIdentifierReservationProfile"/>, supaya tidak dapat diturunkan lewat
    /// konfigurasi (`PAT-GAP-003`).
    /// </para>
    /// </remarks>
    public sealed class RsmmcMigrationReservationGuardOptions
    {
        public const string SectionName = "RsmmcMigrationReservationGuard";

        public bool? Enabled { get; set; }
    }
}
