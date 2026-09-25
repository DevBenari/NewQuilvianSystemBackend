using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Cara sebuah unit layanan memberitahu Registrasi bahwa satu kunjungan
    /// <b>benar-benar dilanjutkan</b> di unitnya (<c>BE-EXT-05</c>, <c>LAB-DEC-054</c>,
    /// <c>LAB-DEC-058</c>).
    ///
    /// <b>Kenapa berupa antarmuka, bukan pembacaan langsung.</b> Penutupan otomatis adalah
    /// wewenang Registrasi, tetapi <i>apa artinya dilanjutkan</i> hanya diketahui unit yang
    /// melayani: bagi Laboratorium artinya ada pesanan pemeriksaan yang terbit atas kunjungan
    /// itu. Bila Registrasi membaca tabel unit lain secara langsung, arah ketergantungan antar
    /// modul berbalik — hari ini <c>RegistrationManagement</c> <b>nol</b> menyebut
    /// <c>LaboratoryManagement</c>, sedangkan sebaliknya memang ada. Antarmuka ini menjaga arah
    /// itu tetap satu jalan: Registrasi menetapkan pertanyaannya, unit layanan menjawabnya.
    ///
    /// <b>Ketiadaan implementasi berarti tidak ada yang ditutup.</b> Itu bukan kelalaian,
    /// melainkan penjagaan yang disengaja — lihat <see cref="KioskEncounterClosureService"/>.
    /// </summary>
    public interface IEncounterContinuationProbe
    {
        /// <summary>
        /// Tujuan layanan kiosk yang diwakili probe ini. Nilai inilah yang dipakai sebagai
        /// penyaring penutupan, sehingga penyaring tidak pernah dapat melebar melebihi unit
        /// yang benar-benar punya penjawab.
        /// </summary>
        KioskServiceTarget TargetService { get; }

        /// <summary>
        /// Mengembalikan bagian dari <paramref name="encounterIds"/> yang <b>sudah
        /// dilanjutkan</b> di unit ini. Kunjungan yang tidak ikut disebut dianggap belum
        /// dilanjutkan.
        /// </summary>
        Task<IReadOnlySet<Guid>> FindContinuedEncounterIdsAsync(
            IReadOnlyCollection<Guid> encounterIds,
            CancellationToken cancellationToken = default);
    }
}
