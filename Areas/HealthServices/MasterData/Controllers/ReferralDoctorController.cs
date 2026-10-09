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
    /// Dokter perujuk — dokter <b>di luar</b> rumah sakit ini yang berpraktik pada sebuah instansi
    /// perujuk (<c>LAB-DEC-035</c>, <c>BE-EXT-02</c>).
    ///
    /// <b>Sejak <c>RJ-DOC-REV-BE-017</c></b> grup ini memiliki sembilan endpoint standar master
    /// data (<c>RJ-DOC-DEC-080</c>). Endpoint <c>options</c> yang sudah dipakai Laboratorium
    /// tidak berubah perilakunya.
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
        private const string LogCategory = "HealthServices.MasterData.Referral";

        private readonly ReferralMasterDataService _referralMasterDataService;
        private readonly LoggerService _loggerService;

        public ReferralDoctorController(
            ReferralMasterDataService referralMasterDataService,
            LoggerService loggerService)
        {
            _referralMasterDataService = referralMasterDataService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring, pengurutan, dan isian form halaman master.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ReferralMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Referral Doctor", Description = "Melihat konfigurasi penyaring dokter perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralDoctor", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<ReferralMasterFilterMetadataResponse>.Ok(
                ReferralMasterDataService.BuildDoctorFilterMetadata(),
                "Konfigurasi penyaring dokter perujuk berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah dokter perujuk.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ReferralDoctorSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Referral Doctor", Description = "Melihat ringkasan dokter perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralDoctor", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            return Ok(ApiResponse<ReferralDoctorSummaryResponse>.Ok(
                await _referralMasterDataService.GetDoctorSummaryAsync(cancellationToken),
                "Ringkasan dokter perujuk berhasil diambil."));
        }

        /// <summary>Daftar dokter perujuk untuk tabel utama.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReferralDoctorResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Referral Doctor", Description = "Melihat daftar dokter perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralDoctor", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? referralInstitutionId,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.GetDoctorsPagedAsync(
                search, isActive, referralInstitutionId, sortBy, sortDirection, pageNumber, pageSize, cancellationToken);

            return Ok(ApiResponse<PagedResult<ReferralDoctorResponse>>.Ok(
                result, "Daftar dokter perujuk berhasil diambil."));
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

        /// <summary>Pilihan dokter perujuk untuk akun perangkat Kiosk (<c>RJ-DOC-DEC-076</c>).</summary>
        /// <remarks>
        /// Sengaja tanpa <c>[AccessAction]</c>/<c>[AccessPermission]</c>; memakai policy
        /// <see cref="AuthorizationPolicies.KioskRead"/>, sama dengan
        /// <c>KioskPatientLookupController</c>. Selalu hanya dokter aktif.
        /// </remarks>
        [HttpGet("kiosk/options")]
        [Authorize(Policy = AuthorizationPolicies.KioskRead)]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReferralDoctorOptionResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetKioskOptions(
            [FromQuery] ReferralDoctorOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            query.OnlyActive = true;

            var hasil = await _referralMasterDataService.GetDoctorOptionsAsync(
                query, cancellationToken);

            return Ok(ApiResponse<PagedResult<ReferralDoctorOptionResponse>>.Ok(
                hasil, "Data pilihan dokter perujuk berhasil diambil."));
        }

        /// <summary>Detail satu dokter perujuk.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ReferralDoctorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Referral Doctor", Description = "Melihat detail dokter perujuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ReferralDoctor", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var response = await _referralMasterDataService.GetDoctorByIdAsync(id, cancellationToken);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokter perujuk tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<ReferralDoctorResponse>.Ok(
                response, "Detail dokter perujuk berhasil diambil."));
        }

        /// <summary>Menambah dokter perujuk pada instansi yang aktif.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ReferralDoctorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Referral Doctor", Description = "Menambah dokter perujuk", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ReferralDoctor", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateReferralDoctorRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.CreateDoctorAsync(
                request, GetCurrentUserId(), cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            await LogAsync("ReferralDoctor.Create", "Menambah dokter perujuk.", result.Data!.Id, result.Data.IsActive);

            return Ok(ApiResponse<ReferralDoctorResponse>.Ok(result.Data!, result.Message));
        }

        /// <summary>Mengubah instansi, nama, dan status dokter perujuk.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ReferralDoctorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Referral Doctor", Description = "Mengubah dokter perujuk", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ReferralDoctor", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateReferralDoctorRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.UpdateDoctorAsync(
                id, request, GetCurrentUserId(), cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            await LogAsync("ReferralDoctor.Update", "Mengubah dokter perujuk.", id, result.Data!.IsActive);

            return Ok(ApiResponse<ReferralDoctorResponse>.Ok(result.Data!, result.Message));
        }

        /// <summary>Mengaktifkan atau menonaktifkan dokter perujuk.</summary>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ReferralDoctorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Referral Doctor Status", Description = "Mengubah status aktif dokter perujuk", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ReferralDoctor", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateReferralMasterStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.UpdateDoctorStatusAsync(
                id, request.IsActive, GetCurrentUserId(), cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            await LogAsync("ReferralDoctor.UpdateStatus", "Mengubah status dokter perujuk.", id, request.IsActive);

            return Ok(ApiResponse<ReferralDoctorResponse>.Ok(result.Data!, result.Message));
        }

        /// <summary>Menandai dokter perujuk terhapus. Ditolak <c>409</c> bila sudah dipakai kunjungan.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Delete", "Delete Referral Doctor", Description = "Menghapus dokter perujuk", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("ReferralDoctor", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _referralMasterDataService.DeleteDoctorAsync(
                id, GetCurrentUserId(), cancellationToken);

            if (result.Status != ReferralMasterStatus.Success)
                return MapFailure(result.Status, result.Message);

            await LogAsync("ReferralDoctor.Delete", "Menghapus dokter perujuk.", id, false);

            return Ok(ApiResponse<bool>.Ok(true, result.Message));
        }

        private Task LogAsync(string action, string message, Guid id, bool isActive)
            => _loggerService.InfoAsync(
                LogCategory,
                action,
                message,
                new
                {
                    EntityId = id,
                    IsActive = isActive,
                    Controller = "ReferralDoctor",
                    Action = action
                });

        private IActionResult MapFailure(ReferralMasterStatus status, string message)
            => status switch
            {
                ReferralMasterStatus.NotFound => NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, message)),
                ReferralMasterStatus.DuplicateIdentity => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message)),
                ReferralMasterStatus.InUse => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message)),
                _ => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, message))
            };

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
