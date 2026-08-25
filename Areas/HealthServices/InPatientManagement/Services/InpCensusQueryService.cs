using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Menyusun census, menghitung lama dirawat, menyusun papan ketersediaan tempat tidur,
    /// daftar pantau, dan laporan selisih tempat tidur. Service ini hanya membaca.
    /// </summary>
    /// <remarks>
    /// <b>Kerangka.</b> Task <c>BE-RWI-004</c> hanya mendaftarkan service ini ke dependency
    /// injection supaya controller yang dibuat task berikutnya benar-benar dapat dibentuk.
    /// Perilakunya diisi task berikut, satu per satu:
    ///
    /// <list type="bullet">
    /// <item><description><c>BE-RWI-009</c> — daftar dan detail episode</description></item>
    /// <item><description><c>BE-RWI-016</c> — census dan lama dirawat</description></item>
    /// <item><description><c>BE-RWI-015</c> dan <c>BE-RWI-029</c> — empat daftar pantau dan laporan selisih</description></item>
    /// </list>
    ///
    /// Tiga batas desain yang sudah terkunci sejak sekarang dan tidak boleh dilanggar saat
    /// badan method diisi: seluruh query memakai <c>AsNoTracking</c> dan projection langsung
    /// ke DTO; census TIDAK pernah disimpan sebagai tabel melainkan selalu dihitung dari
    /// penempatan yang masih aktif; dan lama dirawat dihitung dari selisih tanggal, bukan
    /// selisih jam, sehingga hasilnya bertambah pada pergantian tanggal.
    ///
    /// <para>
    /// <b>Contoh lama dirawat.</b> Tn. Budi masuk 21 September pukul 22:30 dan pulang
    /// 22 September pukul 06:00. Selisih jamnya hanya 7,5 jam, tetapi tanggalnya berbeda,
    /// sehingga lama dirawat tercatat 1 hari — bukan 0 hari.
    /// </para>
    /// </remarks>
    public class InpCensusQueryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpSettingService _settingService;

        public InpCensusQueryService(
            ApplicationDbContext dbContext,
            InpSettingService settingService)
        {
            _dbContext = dbContext;
            _settingService = settingService;
        }
    }
}
