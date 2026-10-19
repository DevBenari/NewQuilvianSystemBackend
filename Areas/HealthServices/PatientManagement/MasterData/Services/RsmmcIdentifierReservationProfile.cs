using Microsoft.Extensions.Configuration;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Keadaan guard reservasi nomor pasien normal untuk environment dan konfigurasi saat ini.
    /// </summary>
    public enum RsmmcReservationGuardState
    {
        /// <summary>Bukan environment <c>Staging</c> persis. Penomoran lama berlaku apa adanya.</summary>
        NotApplicable = 0,

        /// <summary><c>Staging</c> persis dan <c>Enabled = true</c>. Lantai reservasi berlaku.</summary>
        Active = 1,

        /// <summary><c>Staging</c> persis dan <c>Enabled = false</c>. Gagal tertutup (`PAT-DEC-067`).</summary>
        Disabled = 2,

        /// <summary><c>Staging</c> persis, bagian atau kunci <c>Enabled</c> tidak ada. Gagal tertutup.</summary>
        ConfigMissing = 3,

        /// <summary><c>Staging</c> persis, nilai <c>Enabled</c> bukan boolean atau tidak terbaca. Gagal tertutup.</summary>
        ConfigMalformed = 4
    }

    public sealed record RsmmcReservationGuardEvaluation(
        string? EnvironmentName,
        RsmmcReservationGuardState State)
    {
        public bool IsExactStaging => State != RsmmcReservationGuardState.NotApplicable;

        /// <summary>Allocator aman reservasi berjalan.</summary>
        public bool IsActive => State == RsmmcReservationGuardState.Active;

        /// <summary>Pendaftaran normal wajib ditolak sebelum mutasi apa pun.</summary>
        public bool IsFailClosed => IsExactStaging && !IsActive;

        /// <summary>
        /// Konfigurasi sah untuk environment ini. Di <c>Staging</c> persis hanya
        /// <c>Enabled = true</c> yang sah; di luar itu konfigurasi guard tidak dibutuhkan.
        /// </summary>
        public bool ConfigurationValid => !IsExactStaging || IsActive;

        public string StateCode => RsmmcIdentifierReservationProfile.ToStateCode(State);
    }

    /// <summary>
    /// Profil reservasi nomor pasien normal selama kampanye migrasi RSMMC (`BE-PAT-MIG-003`).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Target preview RSMMC sudah memesan suffix <c>PatientCode</c> 742–705.249, dan perencana MRN
    /// memesan MRN numerik sampai 890.187 (`PAT-FACT-084`). Selama kampanye berjalan, pendaftaran
    /// pasien normal di <c>Staging</c> persis tidak boleh memakai nomor di bawah lantai berikut
    /// (`PAT-DEC-054`, `PAT-DEC-064`). Lantai adalah konstanta yang disetujui pemilik, bukan
    /// konfigurasi.
    /// </para>
    /// <para>
    /// Aturan environment (`PAT-DEC-066`, diamandemen `PAT-DEC-067`): di <c>Staging</c> persis
    /// allocator aman reservasi wajib dipakai dan hanya berjalan bila <c>Enabled = true</c>; nilai
    /// lain gagal tertutup. Di luar <c>Staging</c> persis penomoran lama berlaku dan konfigurasi
    /// guard tidak dibaca sama sekali.
    /// </para>
    /// </remarks>
    public static class RsmmcIdentifierReservationProfile
    {
        public const string PatientCodePrefix = "PAT-RSMMC-";

        /// <summary>Panjang minimum angka <c>PatientCode</c>, sama dengan pembangkit lama.</summary>
        public const int PatientCodeNumberLength = 5;

        /// <summary>Suffix <c>PatientCode</c> normal terkecil di <c>Staging</c> — <c>PAT-RSMMC-705250</c>.</summary>
        public const long PatientCodeFloor = 705250;

        /// <summary>MRN numerik normal terkecil di <c>Staging</c> — <c>00-89-01-88</c>.</summary>
        public const long MedicalRecordNumberFloor = 890188;

        /// <summary>Batas 8 digit MRN, sama dengan pembangkit lama (<c>99-99-99-99</c>).</summary>
        public const long MedicalRecordNumberMaximum = 99999999;

        /// <summary>Panjang MRN berformat kanonik <c>00-00-00-00</c>.</summary>
        public const int FormattedMedicalRecordNumberLength = 11;

        /// <summary>Pola <c>LIKE</c> MRN berformat kanonik.</summary>
        public const string FormattedMedicalRecordNumberPattern = "__-__-__-__";

        /// <summary>
        /// Kunci pertama (namespace) kunci transaksi PostgreSQL dua bagian untuk alokasi nomor pasien
        /// normal: <c>0x50415449</c> = 1.346.458.697, teks ASCII <c>"PATI"</c> —
        /// PATIENT_IDENTIFIER_ALLOCATION.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dipakai sebagai <c>pg_advisory_xact_lock(AllocationLockNamespace, AllocationLockId)</c>.
        /// Nilainya tetap dan sama bagi seluruh instance aplikasi, tanpa <c>hashtext()</c>, sehingga
        /// kunci yang diambil kiosk, admin, dan rute biasa di instance mana pun selalu kunci yang sama.
        /// </para>
        /// <para>
        /// Dipesan khusus untuk allocator ini. Impact scan 16 Oktober 2026 pada HEAD dan
        /// <c>origin/QuilvianIntegrationBackend</c>: seluruh kunci lain di repository adalah bentuk
        /// <b>satu</b> kunci 64-bit (<c>hashtext(...)</c> atau bigint), yang menurut dokumentasi
        /// PostgreSQL tidak berbagi ruang kunci dengan bentuk dua kunci 32-bit. Satu-satunya kunci
        /// dua bagian lain yang direncanakan — worker <c>BE-PAT-MIG-002</c> — memakai namespace
        /// <c>1173286842</c> (<c>0x45EEEFBA</c>), berbeda dari nilai ini.
        /// </para>
        /// </remarks>
        public const int AllocationLockNamespace = 0x50415449;

        /// <summary>
        /// Kunci kedua: <c>0x4E4F524D</c> = 1.313.821.261, teks ASCII <c>"NORM"</c> —
        /// NORMAL_PATIENT_CREATE. Satu kunci global untuk seluruh pendaftaran pasien normal.
        /// </summary>
        public const int AllocationLockId = 0x4E4F524D;

        public const string GuardConfigInvalidErrorCode = "PATIENT_RESERVATION_GUARD_CONFIG_INVALID";

        public const string CheckerEnvironmentNotAllowedErrorCode = "RSMMC_RESERVATION_GUARD_ENVIRONMENT_NOT_ALLOWED";

        public static string FormatPatientCode(long number) =>
            PatientCodePrefix + number.ToString().PadLeft(PatientCodeNumberLength, '0');

        public static string FormatMedicalRecordNumber(long number) =>
            PatientQrPayloadBuilder.FormatMedicalRecordNumber(number.ToString().PadLeft(8, '0'));

        /// <summary>
        /// Menilai guard. Pencocokan environment memakai daftar izin positif yang sama dengan
        /// gerbang Pilot: persis <c>Staging</c>, ordinal.
        /// </summary>
        public static RsmmcReservationGuardEvaluation Evaluate(
            string? environmentName,
            IConfiguration configuration)
        {
            if (!RsmmcPilotEnvironmentGate.IsAllowed(environmentName))
            {
                return new RsmmcReservationGuardEvaluation(
                    environmentName,
                    RsmmcReservationGuardState.NotApplicable);
            }

            return new RsmmcReservationGuardEvaluation(environmentName, ReadState(configuration));
        }

        public static string ToStateCode(RsmmcReservationGuardState state) => state switch
        {
            RsmmcReservationGuardState.Active => "ACTIVE",
            RsmmcReservationGuardState.Disabled => "DISABLED",
            RsmmcReservationGuardState.ConfigMissing => "CONFIG_MISSING",
            RsmmcReservationGuardState.ConfigMalformed => "CONFIG_MALFORMED",
            _ => "NOT_APPLICABLE"
        };

        // Hanya dipanggil di Staging persis. Kegagalan membaca konfigurasi diperlakukan sama dengan
        // nilai salah bentuk: gagal tertutup, bukan dianggap false.
        private static RsmmcReservationGuardState ReadState(IConfiguration configuration)
        {
            try
            {
                var section = configuration.GetSection(RsmmcMigrationReservationGuardOptions.SectionName);

                if (!section.Exists() ||
                    section[nameof(RsmmcMigrationReservationGuardOptions.Enabled)] is null)
                {
                    return RsmmcReservationGuardState.ConfigMissing;
                }

                var options = section.Get<RsmmcMigrationReservationGuardOptions>();

                return options?.Enabled switch
                {
                    true => RsmmcReservationGuardState.Active,
                    false => RsmmcReservationGuardState.Disabled,
                    null => RsmmcReservationGuardState.ConfigMalformed
                };
            }
            catch (Exception)
            {
                return RsmmcReservationGuardState.ConfigMalformed;
            }
        }
    }
}
