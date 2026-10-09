using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Instansi perujuk — klinik, puskesmas, atau rumah sakit yang mengirim pasien ke sini
    /// (<c>LAB-DEC-035</c>, <c>BE-EXT-02</c>).
    ///
    /// <b>Sejak <c>RJ-DOC-REV-BE-017</c></b> grup ini memiliki sembilan endpoint standar master
    /// data beserta tanda <c>IsPartner</c> (<c>RJ-DOC-DEC-073</c>, <c>RJ-DOC-DEC-080</c>).
    /// Sebelumnya grup ini baca saja. Endpoint <c>options</c> yang sudah dipakai Laboratorium
    /// tidak berubah perilakunya, hanya menambah ruas <c>isPartner</c>.
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
        private const string LogCategory = "HealthServices.MasterData.Referral";

        private readonly ReferralMasterDataService _referralMasterDataService;
        private readonly LoggerService _loggerService;

        public ReferralInstitutionController(
            ReferralMasterDataService referralMasterDataService,
            LoggerService loggerService)
        {
            _referralMasterDataService = referralMasterDataService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring, pengurutan, dan isian form halaman master.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ReferralMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Referral Institution", Description = "Melihat konfigurasi penyaring instansi perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralInstitution", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<ReferralMasterFilterMetadataResponse>.Ok(
                ReferralMasterDataService.BuildInstitutionFilterMetadata(),
                "Konfigurasi penyaring instansi perujuk berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah instansi: total, aktif, nonaktif, dan bermitra.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ReferralInstitutionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Referral Institution", Description = "Melihat ringkasan instansi perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralInstitution", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            return Ok(ApiResponse<ReferralInstitutionSummaryResponse>.Ok(
                await _referralMasterDataService.GetInstitutionSummaryAsync(cancellationToken),
                "Ringkasan instansi perujuk berhasil diambil."));
        }

        /// <summary>Daftar instansi perujuk untuk tabel utama.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReferralInstitutionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Referral Institution", Description = "Melihat daftar instansi perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralInstitution", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] ReferralInstitutionListQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.GetInstitutionsPagedAsync(query, cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            return Ok(ApiResponse<PagedResult<ReferralInstitutionResponse>>.Ok(result.Data!, result.Message));
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

        /// <summary>
        /// Pilihan fasilitas mitra <b>layak</b> untuk Pendaftaran Rawat Jalan (<c>DEC-FRJ-001</c>):
        /// aktif, mitra, dan perjanjiannya berlaku pada <c>serviceDate</c> (bawaan hari ini, WIB).
        /// </summary>
        /// <remarks>
        /// Feed terpisah dari <c>options</c> supaya perilaku <c>options</c> yang dipakai Laboratorium
        /// tidak berubah. Kelayakan tetap divalidasi ulang saat kunjungan disimpan.
        /// </remarks>
        [HttpGet("partner-options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReferralInstitutionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Referral Institution", Description = "Melihat data pilihan fasilitas perujuk mitra", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralInstitution", "Read")]
        public async Task<IActionResult> GetPartnerOptions(
            [FromQuery] ReferralPartnerOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _referralMasterDataService.GetPartnerOptionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<ReferralInstitutionOptionResponse>>.Ok(
                hasil, "Data pilihan fasilitas perujuk mitra berhasil diambil."));
        }

        /// <summary>Pilihan instansi perujuk untuk akun perangkat Kiosk (<c>RJ-DOC-DEC-076</c>).</summary>
        /// <remarks>
        /// <b>Sengaja tanpa <c>[AccessAction]</c> dan <c>[AccessPermission]</c></b>, mengikuti
        /// <c>KioskPatientLookupController</c>: akun Kiosk tidak punya baris di matriks Akses Role,
        /// dan policy <see cref="AuthorizationPolicies.KioskRead"/> adalah otorisasi alternatif
        /// yang disetujui. Sejak <c>DEC-FRJ-001</c> hanya mitra layak hari ini (aktif, mitra, dan
        /// perjanjian berlaku), sama dengan <c>partner-options</c>.
        /// </remarks>
        [HttpGet("kiosk/options")]
        [Authorize(Policy = AuthorizationPolicies.KioskRead)]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReferralInstitutionOptionResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetKioskOptions(
            [FromQuery] ReferralInstitutionOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            // Kiosk selalu mendaftarkan untuk hari ini; tanggal layanan tidak diterima dari perangkat.
            var hasil = await _referralMasterDataService.GetPartnerOptionsAsync(
                new ReferralPartnerOptionQuery
                {
                    Search = query.Search,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                },
                cancellationToken);

            return Ok(ApiResponse<PagedResult<ReferralInstitutionOptionResponse>>.Ok(
                hasil, "Data pilihan instansi perujuk berhasil diambil."));
        }

        /// <summary>Detail satu instansi perujuk.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ReferralInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Referral Institution", Description = "Melihat detail instansi perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralInstitution", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var response = await _referralMasterDataService.GetInstitutionByIdAsync(id, cancellationToken);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Institusi perujuk tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<ReferralInstitutionResponse>.Ok(
                response, "Detail instansi perujuk berhasil diambil."));
        }

        /// <summary>Menambah instansi perujuk.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ReferralInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Referral Institution", Description = "Menambah instansi perujuk", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ReferralInstitution", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateReferralInstitutionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.CreateInstitutionAsync(
                request, GetCurrentUserId(), cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            await LogAsync("ReferralInstitution.Create", "Menambah instansi perujuk.", result.Data!.Id, result.Data.IsPartner, result.Data.IsActive);

            return Ok(ApiResponse<ReferralInstitutionResponse>.Ok(result.Data!, result.Message));
        }

        /// <summary>Mengubah seluruh ruas bisnis instansi perujuk, termasuk tanda Bermitra.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ReferralInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Referral Institution", Description = "Mengubah instansi perujuk", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ReferralInstitution", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateReferralInstitutionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.UpdateInstitutionAsync(
                id, request, GetCurrentUserId(), cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            await LogAsync("ReferralInstitution.Update", "Mengubah instansi perujuk.", id, result.Data!.IsPartner, result.Data.IsActive);

            return Ok(ApiResponse<ReferralInstitutionResponse>.Ok(result.Data!, result.Message));
        }

        /// <summary>
        /// Menambah periode perjanjian kerja sama (perpanjangan). Ditolak <c>400</c> bila
        /// bertumpang tindih dan <c>409</c> bila master sudah diubah pengguna lain.
        /// </summary>
        [HttpPost("{id:guid}/agreements")]
        [ProducesResponseType(typeof(ApiResponse<ReferralInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Referral Institution", Description = "Menambah perjanjian kerja sama fasilitas perujuk", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ReferralInstitution", "Update")]
        public async Task<IActionResult> AddAgreement(
            Guid id,
            [FromBody] CreateReferralInstitutionAgreementRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.AddAgreementAsync(
                id, request, GetCurrentUserId(), cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            await LogAsync("ReferralInstitution.AddAgreement", "Menambah perjanjian kerja sama fasilitas perujuk.", id, result.Data!.IsPartner, result.Data.IsActive);

            return Ok(ApiResponse<ReferralInstitutionResponse>.Ok(result.Data!, result.Message));
        }

        /// <summary>Mengaktifkan atau menonaktifkan instansi perujuk.</summary>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ReferralInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Referral Institution Status", Description = "Mengubah status aktif instansi perujuk", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ReferralInstitution", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateReferralMasterStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.UpdateInstitutionStatusAsync(
                id, request.IsActive, GetCurrentUserId(), cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            await LogAsync("ReferralInstitution.UpdateStatus", "Mengubah status instansi perujuk.", id, result.Data!.IsPartner, request.IsActive);

            return Ok(ApiResponse<ReferralInstitutionResponse>.Ok(result.Data!, result.Message));
        }

        /// <summary>Menandai instansi perujuk terhapus. Ditolak <c>409</c> bila sudah dipakai.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Delete", "Delete Referral Institution", Description = "Menghapus instansi perujuk", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("ReferralInstitution", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.DeleteInstitutionAsync(
                id, GetCurrentUserId(), cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            await LogAsync("ReferralInstitution.Delete", "Menghapus instansi perujuk.", id, result.Data!.IsPartner, false);

            return Ok(ApiResponse<bool>.Ok(true, result.Message));
        }

        private Task LogAsync(string action, string message, Guid id, bool isPartner, bool isActive)
            => _loggerService.InfoAsync(
                LogCategory,
                action,
                message,
                new
                {
                    EntityId = id,
                    IsPartner = isPartner,
                    IsActive = isActive,
                    Controller = "ReferralInstitution",
                    Action = action
                });

        private IActionResult MapFailure(ReferralMasterStatus status, string message)
            => status switch
            {
                ReferralMasterStatus.NotFound => NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, message)),
                ReferralMasterStatus.DuplicateIdentity => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message)),
                ReferralMasterStatus.InUse => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message)),
                ReferralMasterStatus.Conflict => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message)),
                _ => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, message))
            };

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
