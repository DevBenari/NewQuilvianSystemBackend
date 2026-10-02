using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Konfigurasi instrumen dan formulir klinis berversi — <c>BE-RWI-107</c>, <c>BE-RWI-108</c>,
    /// api-contract <c>keperawatan</c> 0.5.0 bagian 7.2.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Hak akses.</b> Resource <c>ClinicalInstrumentConfiguration</c> dengan tiga aksi terpisah:
    /// <c>Read</c>, <c>Update</c> (membuat dan mengubah konsep), dan <c>Approve</c> (mengesahkan dan
    /// memensiunkan). Pemisahan ini membuat admin dapat memberi komite keperawatan hak mengesahkan tanpa
    /// hak mengubah. Pengesah yang sama dengan pengubah terakhir tetap ditolak service.
    /// </para>
    /// <para>
    /// <b>Delta kontrak.</b> <c>GET /resolve</c> pada kontrak memakai hak <c>PatientAssessment : Read</c>.
    /// Argumen pertama <c>[AccessPermission]</c> wajib sama dengan <c>ControllerName</c>, sehingga
    /// endpoint itu dipasang pada grup Patient Assessment (<c>GET patient-assessments/instruments/resolve</c>)
    /// dan grup Case Management Evaluation, bukan di sini.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/clinical-instruments")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Clinical Instrument Configuration",
        AreaName = "HealthServices",
        ControllerName = "ClinicalInstrumentConfiguration",
        Description = "Konfigurasi instrumen dan formulir klinis berversi beserta pengesahannya",
        SortOrder = 20)]
    [Tags("Health Services / Clinical Management / Clinical Instrument")]
    public class ClinicalInstrumentController : ControllerBase
    {
        private readonly ClinicalInstrumentService _service;

        public ClinicalInstrumentController(ClinicalInstrumentService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ClinicalInstrumentListItem>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Instrument Configuration", Description = "Melihat daftar instrumen klinis beserta versi sah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] ClinicalInstrumentKind? instrumentKind,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _service.ListAsync(instrumentKind, isActive, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<ClinicalInstrumentListItem>>.Ok(hasil, "Daftar instrumen klinis berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalInstrumentResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Instrument Configuration", Description = "Melihat instrumen beserta seluruh versi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.GetAsync(id, cancellationToken));

        [HttpGet("versions/{versionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalInstrumentVersionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Instrument Configuration", Description = "Melihat satu versi instrumen beserta hasil validasinya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Read")]
        public async Task<IActionResult> GetVersion(Guid versionId, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.GetVersionAsync(versionId, cancellationToken));

        /// <summary>Membuat instrumen. <c>409</c> bila kode dipakai atau rentang usia bertumpuk (<c>VAL-KEP-19c</c>).</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ClinicalInstrumentResponse>), StatusCodes.Status201Created)]
        [AccessAction("Update", "Update Clinical Instrument Configuration", Description = "Membuat dan mengubah instrumen serta versi konsepnya", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Update")]
        public async Task<IActionResult> Create([FromBody] CreateClinicalInstrumentRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.CreateInstrumentAsync(request, this.CurrentUserId(), cancellationToken));

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalInstrumentResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Clinical Instrument Configuration", Description = "Mengubah nama, rentang usia, dan keaktifan instrumen", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClinicalInstrumentRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.UpdateInstrumentAsync(id, request, this.CurrentUserId(), cancellationToken));

        /// <summary>Membuat versi <c>Draft</c> baru, disalin dari versi terakhir bila definisi tidak dikirim.</summary>
        [HttpPost("{id:guid}/versions")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalInstrumentVersionResponse>), StatusCodes.Status201Created)]
        [AccessAction("Update", "Update Clinical Instrument Configuration", Description = "Membuat versi konsep instrumen", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Update")]
        public async Task<IActionResult> CreateVersion(Guid id, [FromBody] CreateClinicalInstrumentVersionRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.CreateVersionAsync(id, request, this.CurrentUserId(), cancellationToken));

        /// <summary>Mengubah definisi versi <c>Draft</c>; <c>ExpectedDefinitionHash</c> wajib cocok.</summary>
        [HttpPut("versions/{versionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalInstrumentVersionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Clinical Instrument Configuration", Description = "Mengubah definisi versi konsep instrumen", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Update")]
        public async Task<IActionResult> UpdateVersion(Guid versionId, [FromBody] UpdateClinicalInstrumentVersionRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.UpdateVersionAsync(versionId, request, this.CurrentUserId(), cancellationToken));

        /// <summary>
        /// Mengesahkan versi. Versi sah lama menjadi <c>Retired</c> pada transaksi yang sama. <c>403</c> bila
        /// pengesah adalah pengubah terakhir (<c>VAL-KEP-20a</c>).
        /// </summary>
        [HttpPatch("versions/{versionId:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalInstrumentVersionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Approve", "Approve Clinical Instrument Configuration", Description = "Mengesahkan atau memensiunkan versi instrumen klinis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Approve")]
        public async Task<IActionResult> Approve(Guid versionId, [FromBody] ApproveClinicalInstrumentVersionRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.ApproveVersionAsync(versionId, request, this.CurrentUserId(), cancellationToken));

        [HttpPatch("versions/{versionId:guid}/retire")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalInstrumentVersionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Approve", "Approve Clinical Instrument Configuration", Description = "Memensiunkan versi sah tanpa pengganti", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Approve")]
        public async Task<IActionResult> Retire(Guid versionId, [FromBody] RetireClinicalInstrumentVersionRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.RetireVersionAsync(versionId, request, this.CurrentUserId(), cancellationToken));

        /// <summary>Uji hitung tanpa menyimpan.</summary>
        [HttpPost("versions/{versionId:guid}/score-preview")]
        [ProducesResponseType(typeof(ApiResponse<InstrumentScoreResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Instrument Configuration", Description = "Menguji hitung skor dan pita instrumen tanpa menyimpan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalInstrumentConfiguration", "Read")]
        public async Task<IActionResult> ScorePreview(Guid versionId, [FromBody] ScorePreviewRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.ScorePreviewAsync(versionId, request, cancellationToken));
    }
}
