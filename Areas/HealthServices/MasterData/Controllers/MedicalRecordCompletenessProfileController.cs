using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using MedicalRecordCompletenessProfilePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.MedicalRecordCompletenessProfileListItemResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <remarks>
    /// Layar pengelola dokumen wajib rekam medis (`BE-37`, kontrak API 0.2.0 A3). Unit rekam medis
    /// menentukan dokumen apa yang dituntut setiap jenis kunjungan, jenis unit, dan kondisi —
    /// tanpa rilis program (`RM-DEC-045`).
    ///
    /// <b>Contoh.</b> Komite rekam medis memutuskan Rawat Jalan wajib CPPT. Petugas menambah
    /// kriteria "CPPT" pada profil <c>RJ</c> lewat <c>PUT /{id}</c>; sejak saat itu kunjungan
    /// Rawat Jalan tanpa CPPT dinilai belum lengkap.
    ///
    /// Tidak ada endpoint penghapusan, dan itu disengaja: profil yang tidak berlaku lagi
    /// dinonaktifkan lewat <c>PATCH /{id}/status</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/medical-record-completeness-profiles")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Medical Record Completeness Profile",
        AreaName = "HealthServices",
        ControllerName = "MedicalRecordCompletenessProfile",
        Description = "Mengelola profil dokumen wajib kelengkapan berkas rekam medis",
        SortOrder = 47
    )]
    [Tags("Health Services / Master Data / Medical Record Completeness Profile")]
    public class MedicalRecordCompletenessProfileController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.MedicalRecord";

        private readonly MedicalRecordCompletenessProfileService _profileService;
        private readonly LoggerService _loggerService;

        public MedicalRecordCompletenessProfileController(
            MedicalRecordCompletenessProfileService profileService,
            LoggerService loggerService)
        {
            _profileService = profileService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessProfileFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Completeness Profile", Description = "Melihat konfigurasi penyaring profil dokumen wajib", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordCompletenessProfile", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<MedicalRecordCompletenessProfileFilterMetadataResponse>.Ok(
                MedicalRecordCompletenessProfileService.BuildFilterMetadata(),
                "Konfigurasi penyaring profil dokumen wajib berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessProfileSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Completeness Profile", Description = "Melihat ringkasan profil dokumen wajib", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordCompletenessProfile", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _profileService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<MedicalRecordCompletenessProfileSummaryResponse>.Ok(
                summary,
                "Ringkasan profil dokumen wajib berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessProfilePagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Medical Record Completeness Profile", Description = "Melihat daftar profil dokumen wajib", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordCompletenessProfile", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] EncounterType? encounterType,
            [FromQuery] ServiceUnitType? serviceUnitType,
            [FromQuery] CompletenessCondition? condition,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var galat = MedicalRecordCompletenessProfileService.ValidateFilter(encounterType, serviceUnitType, condition);
            if (galat != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, galat));

            var result = await _profileService.GetPagedAsync(
                search,
                isActive,
                encounterType,
                serviceUnitType,
                condition,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<MedicalRecordCompletenessProfilePagedResult>.Ok(
                result,
                "Daftar profil dokumen wajib berhasil diambil."));
        }

        [HttpGet("criteria/options")]
        [ProducesResponseType(typeof(ApiResponse<List<MedicalRecordCompletenessCriterionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Completeness Profile", Description = "Melihat katalog kriteria dokumen wajib", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordCompletenessProfile", "Read")]
        public IActionResult GetCriterionOptions()
        {
            return Ok(ApiResponse<List<MedicalRecordCompletenessCriterionOptionResponse>>.Ok(
                MedicalRecordCompletenessProfileService.GetCriterionOptions(),
                "Katalog kriteria dokumen wajib berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessProfileDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medical Record Completeness Profile", Description = "Melihat detail profil dokumen wajib", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordCompletenessProfile", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var profile = await _profileService.GetDetailAsync(id, cancellationToken);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    MedicalRecordCompletenessProfileService.NotFoundMessage));
            }

            return Ok(ApiResponse<MedicalRecordCompletenessProfileDetailResponse>.Ok(
                profile,
                "Detail profil dokumen wajib berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessProfileDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Medical Record Completeness Profile", Description = "Membuat profil dokumen wajib", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicalRecordCompletenessProfile", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateMedicalRecordCompletenessProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _profileService.CreateAsync(request, GetCurrentUserId(), cancellationToken);

            return await RespondAsync(result, "MedicalRecordCompletenessProfile.Create");
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessProfileDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Medical Record Completeness Profile", Description = "Mengubah profil dokumen wajib beserta kriterianya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalRecordCompletenessProfile", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateMedicalRecordCompletenessProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _profileService.UpdateAsync(id, request, GetCurrentUserId(), cancellationToken);

            return await RespondAsync(result, "MedicalRecordCompletenessProfile.Update");
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessProfileDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Medical Record Completeness Profile Status", Description = "Mengaktifkan atau menonaktifkan profil dokumen wajib", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalRecordCompletenessProfile", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateMedicalRecordCompletenessProfileStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _profileService.UpdateStatusAsync(id, request.IsActive, GetCurrentUserId(), cancellationToken);

            return await RespondAsync(result, "MedicalRecordCompletenessProfile.UpdateStatus");
        }

        private async Task<IActionResult> RespondAsync(MedicalRecordCompletenessProfileResult result, string action)
        {
            switch (result.Status)
            {
                case MedicalRecordCompletenessProfileStatus.NotFound:
                    return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message));
                case MedicalRecordCompletenessProfileStatus.Conflict:
                    return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message));
                case MedicalRecordCompletenessProfileStatus.Invalid:
                    return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message));
            }

            // Isi perubahan ditulis di teks pesan: logger tidak menyimpan objek data penyertanya.
            await _loggerService.AuditAsync(LogCategory, action, result.AuditMessage!);

            return Ok(ApiResponse<MedicalRecordCompletenessProfileDetailResponse>.Ok(result.Profile!, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
