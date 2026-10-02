using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Dokter perujuk — dokter <b>di luar</b> rumah sakit ini yang mengirim pasien
    /// (<c>LAB-DEC-035</c>, <c>BE-EXT-02</c>).
    ///
    /// <b>Bukan dokter internal.</b> Dokter pada data induk rumah sakit punya jadwal praktik,
    /// menerima jasa medis, dan dapat menjadi DPJP; dokter perujuk tidak satu pun dari
    /// ketiganya. Keduanya sengaja tidak disatukan.
    ///
    /// <b>Grup ini baca saja</b>, sama seperti instansi perujuk.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/referral-doctors")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Referral Doctor",
        AreaName = "HealthServices",
        ControllerName = "ReferralDoctor",
        Description = "Data induk dokter perujuk pasien",
        SortOrder = 31
    )]
    [Tags("Health Services / Master Data / Referral Doctor")]
    public class ReferralDoctorController : ControllerBase
    {
        private readonly ReferralMasterDataService _referralMasterDataService;

        public ReferralDoctorController(ReferralMasterDataService referralMasterDataService)
        {
            _referralMasterDataService = referralMasterDataService;
        }

        // Daftar pilihan dokter perujuk, dapat disaring menurut instansinya.
        //
        // Menyaring dengan `referralInstitutionId` sangat dianjurkan: pendaftaran rujukan
        // menolak dokter yang tidak berpraktik pada instansi yang dipilih, sehingga daftar
        // yang tidak tersaring hanya akan menawarkan pilihan yang pasti ditolak.
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReferralDoctorOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Referral Doctor", Description = "Melihat data pilihan dokter perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralDoctor", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] ReferralDoctorOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _referralMasterDataService.GetDoctorOptionsAsync(
                query, cancellationToken);

            return Ok(ApiResponse<PagedResult<ReferralDoctorOptionResponse>>.Ok(
                hasil, "Data pilihan dokter perujuk berhasil diambil."));
        }
    }
}
