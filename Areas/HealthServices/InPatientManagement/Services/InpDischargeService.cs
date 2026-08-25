using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Keputusan pulang, resume pulang, penandaan daftar periksa administrasi, penandaan
    /// kelayakan keuangan, dan pemeriksaan lima syarat penutupan episode.
    /// </summary>
    /// <remarks>
    /// <b>Kerangka.</b> Task <c>BE-RWI-004</c> hanya mendaftarkan service ini ke dependency
    /// injection supaya controller yang dibuat task berikutnya benar-benar dapat dibentuk.
    /// Perilakunya diisi task berikut, satu per satu:
    ///
    /// <list type="bullet">
    /// <item><description><c>BE-RWI-020</c> — keputusan pasien boleh pulang</description></item>
    /// <item><description><c>BE-RWI-021</c> dan <c>BE-RWI-022</c> — resume pulang, tanda tangan DPJP, dan versi resume</description></item>
    /// <item><description><c>BE-RWI-023</c> — penandaan daftar periksa administrasi</description></item>
    /// <item><description><c>BE-RWI-024</c> — penandaan kelayakan keuangan oleh kasir</description></item>
    /// <item><description><c>BE-RWI-025</c> — pemeriksaan lima syarat penutupan</description></item>
    /// <item><description><c>BE-RWI-027</c> — pencatatan kepergian fisik pasien</description></item>
    /// </list>
    ///
    /// Dua batas desain yang sudah terkunci sejak sekarang dan tidak boleh dilanggar saat
    /// badan method diisi: pemeriksaan penutupan mengembalikan daftar syarat yang belum
    /// terpenuhi — bukan sekadar boleh atau tidak — supaya layar dapat menyebut alasan
    /// pastinya kepada petugas; dan pencatatan kepergian fisik melepas tempat tidur tanpa
    /// mengubah status episode.
    /// </remarks>
    public class InpDischargeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpEpisodeService _episodeService;

        public InpDischargeService(
            ApplicationDbContext dbContext,
            InpEpisodeService episodeService)
        {
            _dbContext = dbContext;
            _episodeService = episodeService;
        }
    }
}
