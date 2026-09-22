using System.Linq.Expressions;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Satu-satunya rumus episode IGD: kapan sebuah encounter dianggap sudah berakhir.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-051</c>, keputusan <c>IGD-DEC-139</c> butir 5 beserta koreksi amendment
    /// (tanda kelima <c>NoShowAt</c>), kontrak validation <c>0.8.0</c> §10.1 aturan 1.
    ///
    /// <para>
    /// "Berakhir" di tabel encounter tidak ditandai seragam. Jalur umum Registrasi menulis
    /// <c>Completed</c> tanpa <c>CompletedAt</c>; pembatalannya mengisi <c>IsCancel</c> dan
    /// <c>CancelledAt</c> tanpa mengubah <c>EncounterStatus</c>; antrean dokter mengisi
    /// <c>NoShowAt</c> bersama statusnya. Karena itu encounter dinyatakan berakhir bila
    /// <b>salah satu</b> dari lima tanda terisi, bukan hanya statusnya.
    /// </para>
    ///
    /// <para>
    /// Rumus disediakan dalam dua bentuk dari sumber yang sama: <see cref="EncounterEnded"/>
    /// untuk kueri yang diterjemahkan ke SQL, dan <see cref="IsEncounterEnded"/> untuk objek
    /// yang sudah dimuat. Keduanya tidak boleh disalin ke tempat lain — salinan yang menyimpang
    /// satu tanda saja membuat daftar triage, penjaga episode, dan rekonsiliasi saling tidak
    /// sepakat tentang pasien yang sama.
    /// </para>
    ///
    /// <para>
    /// Static tanpa DI dan tanpa <c>Program.cs</c>, mengikuti pola
    /// <see cref="EmergencyVisitService.PeriksaJenisEncounter"/>, supaya kelak dapat dipanggil
    /// dari pintu encounter Registrasi (<c>BE-IGD-053</c>) tanpa registrasi service baru.
    /// </para>
    /// </remarks>
    public static class EmergencyEpisodeRule
    {
        /// <summary>
        /// Rumus "encounter berakhir" dalam bentuk yang dapat diterjemahkan EF ke SQL.
        /// </summary>
        public static readonly Expression<Func<RegPatientEncounter, bool>> EncounterEnded =
            encounter => encounter.EncounterStatus == EncounterStatus.Completed
                         || encounter.EncounterStatus == EncounterStatus.Cancelled
                         || encounter.EncounterStatus == EncounterStatus.NoShow
                         || encounter.IsCancel
                         || encounter.CancelledAt != null
                         || encounter.CompletedAt != null
                         || encounter.NoShowAt != null;

        private static readonly Func<RegPatientEncounter, bool> EncounterEndedCompiled =
            EncounterEnded.Compile();

        /// <summary>
        /// Rumus "encounter berakhir" untuk encounter yang sudah dimuat.
        /// </summary>
        /// <example>
        /// Encounter berstatus <c>Registered</c> dengan <c>CancelledAt</c> terisi dianggap
        /// <b>berakhir</b>, walau statusnya belum <c>Cancelled</c>.
        /// </example>
        public static bool IsEncounterEnded(RegPatientEncounter encounter)
        {
            ArgumentNullException.ThrowIfNull(encounter);
            return EncounterEndedCompiled(encounter);
        }
    }
}
