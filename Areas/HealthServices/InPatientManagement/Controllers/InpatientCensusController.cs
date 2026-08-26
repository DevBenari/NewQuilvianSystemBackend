using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Layar census: siapa dirawat, di mana, dan sudah berapa hari.
    /// </summary>
    /// <remarks>
    /// <b>Seluruhnya hanya membaca.</b> Tidak ada satu pun endpoint yang mengubah data, dan
    /// mengikuti konvensi project, tidak satu pun dicatat logger.
    ///
    /// <para>
    /// <b>Census dihitung, bukan disimpan.</b> Ia selalu diturunkan dari baris penempatan yang
    /// masih aktif, sehingga tidak pernah ada versi kedua yang perlu disamakan. Pasien yang
    /// kepergian fisiknya sudah dicatat tidak muncul di sini walaupun episodenya belum
    /// ditutup.
    /// </para>
    ///
    /// <para>
    /// <b>Isi klinis tidak pernah ikut.</b> Census dibaca hampir seluruh peran ruangan;
    /// diagnosis, resume, dan keterangan kebutuhan isolasi tidak boleh bocor lewat sini.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/census")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Census",
        AreaName = "HealthServices",
        ControllerName = "InpatientCensus",
        Description = "Melihat pasien yang sedang dirawat beserta lokasi dan lama dirawatnya",
        SortOrder = 12
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Census")]
    public class InpatientCensusController : ControllerBase
    {
        private readonly InpCensusQueryService _censusQueryService;

        public InpatientCensusController(InpCensusQueryService censusQueryService)
        {
            _censusQueryService = censusQueryService;
        }

        /// <summary>Pilihan penyaring census beserta nilai bawaannya.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<CensusFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Census", Description = "Melihat metadata filter census rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientCensus", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default)
        {
            var result = await _censusQueryService.GetCensusFilterMetadataAsync(cancellationToken);

            return Ok(ApiResponse<CensusFilterMetadataResponse>.Ok(
                result,
                "Metadata filter census berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah pasien dirawat per unit layanan dan per kelas perawatan.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CensusSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Census", Description = "Melihat ringkasan census rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientCensus", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] CensusQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _censusQueryService.GetCensusSummaryAsync(query, cancellationToken);

            return Ok(ApiResponse<CensusSummaryResponse>.Ok(
                result,
                "Ringkasan census berhasil diambil."));
        }

        /// <summary>
        /// Daftar pasien yang sedang dirawat beserta lokasi, DPJP, perawat, dan lama
        /// dirawatnya.
        /// </summary>
        /// <remarks>
        /// Lama dirawat dihitung dari selisih tanggal dan bernilai paling sedikit 1 hari.
        /// Pasien yang masuk 21 September pukul 22:30 dan dibaca 22 September pukul 06:00
        /// tercatat 1 hari, bukan 0.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<CensusPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Census", Description = "Melihat daftar pasien yang sedang dirawat", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientCensus", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] CensusQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _censusQueryService.GetCensusAsync(query, cancellationToken);

            return Ok(ApiResponse<CensusPagedResult>.Ok(
                result,
                "Daftar pasien yang sedang dirawat berhasil diambil."));
        }
    }
}
