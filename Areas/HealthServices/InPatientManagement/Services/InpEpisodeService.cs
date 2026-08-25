using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Satu-satunya pintu perubahan status episode, penugasan DPJP, dan penugasan perawat.
    /// </summary>
    /// <remarks>
    /// <b>Kerangka.</b> Task <c>BE-RWI-004</c> hanya mendaftarkan service ini ke dependency
    /// injection supaya controller yang dibuat task berikutnya benar-benar dapat dibentuk.
    /// Perilakunya diisi task berikut, satu per satu:
    ///
    /// <list type="bullet">
    /// <item><description><c>BE-RWI-007</c> — membuka admisi dan melahirkan episode bernomor</description></item>
    /// <item><description><c>BE-RWI-008</c> — mengubah, membatalkan, dan menggugurkan episode Draft</description></item>
    /// <item><description><c>BE-RWI-012</c> — penjagaan INV-INP-10, satu pasien satu episode yang hadir</description></item>
    /// <item><description><c>BE-RWI-014</c> — penetapan kebutuhan isolasi beserta penjaga kewenangannya</description></item>
    /// <item><description><c>BE-RWI-017</c> dan <c>BE-RWI-018</c> — penugasan DPJP dan perawat</description></item>
    /// <item><description><c>BE-RWI-025</c> s.d. <c>BE-RWI-026</c> — penutupan episode dan jalan keluar supervisor</description></item>
    /// </list>
    ///
    /// Tiga batas desain yang sudah terkunci sejak sekarang dan tidak boleh dilanggar saat
    /// badan method diisi: status episode hanya boleh berubah lewat satu method di service
    /// ini, method itu selalu menulis <c>InpStatusHistory</c> di dalam transaksi yang sama,
    /// dan penjaga kewenangan DPJP berada di service ini — bukan di mesin hak akses.
    /// </remarks>
    public class InpEpisodeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpSettingService _settingService;
        private readonly InpEpisodeNumberService _episodeNumberService;
        private readonly InpBedOccupancyService _bedOccupancyService;

        public InpEpisodeService(
            ApplicationDbContext dbContext,
            InpSettingService settingService,
            InpEpisodeNumberService episodeNumberService,
            InpBedOccupancyService bedOccupancyService)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _episodeNumberService = episodeNumberService;
            _bedOccupancyService = bedOccupancyService;
        }
    }
}
