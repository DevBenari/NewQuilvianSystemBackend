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
    /// Instansi perujuk — klinik, puskesmas, atau rumah sakit yang mengirim pasien ke sini
    /// (<c>LAB-DEC-035</c>, <c>BE-EXT-02</c>).
    ///
    /// <b>Grup ini baca saja, dan ketiadaan jalur ubah itu disengaja.</b> Penambahan dan
    /// penyuntingan data induk perujuk adalah pekerjaan modul Data Induk, bukan modul yang
    /// memakainya. Yang disediakan di sini hanya daftar pilihan, supaya layar pendaftaran
    /// rujukan luar dapat menawarkan <b>pilihan</b> alih-alih memaksa petugas mengetik nama.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/referral-institutions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Referral Institution",
        AreaName = "HealthServices",
        ControllerName = "ReferralInstitution",
        Description = "Data induk instansi perujuk pasien",
        SortOrder = 30
    )]
    [Tags("Health Services / Master Data / Referral Institution")]
    public class ReferralInstitutionController : ControllerBase
    {
        private readonly ReferralMasterDataService _referralMasterDataService;

        public ReferralInstitutionController(ReferralMasterDataService referralMasterDataService)
        {
            _referralMasterDataService = referralMasterDataService;
        }

        // Daftar pilihan instansi perujuk.
        //
        // Bawaannya hanya yang aktif. Instansi yang tidak lagi bekerja sama dinonaktifkan,
        // bukan dihapus, sehingga kunjungan lama yang menunjuknya tetap dapat dibaca.
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReferralInstitutionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Referral Institution", Description = "Melihat data pilihan instansi perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralInstitution", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] ReferralInstitutionOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _referralMasterDataService.GetInstitutionOptionsAsync(
                query, cancellationToken);

            return Ok(ApiResponse<PagedResult<ReferralInstitutionOptionResponse>>.Ok(
                hasil, "Data pilihan instansi perujuk berhasil diambil."));
        }
    }
}
