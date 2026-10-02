using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Evaluasi Awal Manajer Pelayanan Pasien — <c>BE-RWI-113</c>, api-contract <c>keperawatan</c> 0.5.0
    /// bagian 7.3.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Arketipe: sub-proses ter-scope episode</b>, satu dokumen hidup per episode. Tidak ada
    /// <c>DELETE</c>; konsep yang salah dibatalkan beralasan.
    /// </para>
    /// <para>
    /// <b>Hak akses.</b> <c>CaseManagementEvaluation : Read</c> dibaca juga perawat pelaksana;
    /// <c>: Create</c>/<c>: Update</c> adalah penanda MPP (<c>G-11</c>) — ditentukan layar Akses Role, bukan
    /// nama peran di kode. Di atasnya service menegakkan penempatan unit dan penulis konsep.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/case-management-evaluations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Case Management Evaluation",
        AreaName = "HealthServices",
        ControllerName = "CaseManagementEvaluation",
        Description = "Evaluasi Awal Manajer Pelayanan Pasien pada perawatan rawat inap",
        SortOrder = 21)]
    [Tags("Health Services / Clinical Management / Case Management Evaluation")]
    public class CaseManagementEvaluationController : ControllerBase
    {
        private readonly CaseManagementEvaluationService _service;

        public CaseManagementEvaluationController(CaseManagementEvaluationService service)
        {
            _service = service;
        }

        /// <summary>Evaluasi Awal hidup satu episode; data <c>null</c> berarti "Belum diisi — milik MPP".</summary>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CaseManagementEvaluationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Case Management Evaluation", Description = "Melihat Evaluasi Awal satu perawatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CaseManagementEvaluation", "Read")]
        public async Task<IActionResult> GetByEpisode(Guid episodeId, CancellationToken cancellationToken = default)
        {
            var hasil = await _service.GetByEpisodeAsync(episodeId, this.CurrentUserId(), cancellationToken);

            return Ok(ApiResponse<CaseManagementEvaluationResponse?>.Ok(
                hasil,
                hasil == null ? "Belum diisi — milik MPP." : "Evaluasi Awal berhasil diambil."));
        }

        /// <summary>Checklist Evaluasi Awal yang berlaku bagi pasien satu episode.</summary>
        [HttpGet("checklist/resolve")]
        [ProducesResponseType(typeof(ApiResponse<ResolvedInstrumentResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Case Management Evaluation", Description = "Melihat checklist Evaluasi Awal yang berlaku", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CaseManagementEvaluation", "Read")]
        public async Task<IActionResult> ResolveChecklist([FromQuery] Guid episodeId, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.ResolveChecklistAsync(episodeId, cancellationToken));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CaseManagementEvaluationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Case Management Evaluation", Description = "Melihat detail Evaluasi Awal beserta delapan bagiannya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CaseManagementEvaluation", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.GetAsync(id, this.CurrentUserId(), cancellationToken));

        /// <summary>
        /// Membuat konsep. <c>403</c> tidak ditempatkan di unit pasien; <c>409</c> episode sudah punya
        /// Evaluasi Awal; <c>422</c> perawatan ditutup atau checklist belum tersedia.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CaseManagementEvaluationResponse>), StatusCodes.Status201Created)]
        [AccessAction("Create", "Create Case Management Evaluation", Description = "MPP membuat konsep Evaluasi Awal", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("CaseManagementEvaluation", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateCaseManagementEvaluationRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.CreateAsync(request, this.IdempotencyKey(request.IdempotencyKey), User, this.CurrentUserId(), cancellationToken));

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CaseManagementEvaluationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Case Management Evaluation", Description = "MPP menyimpan ulang konsep Evaluasi Awal", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CaseManagementEvaluation", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCaseManagementEvaluationRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.UpdateAsync(id, request, User, this.CurrentUserId(), cancellationToken));

        [HttpPatch("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<CaseManagementEvaluationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Case Management Evaluation", Description = "MPP menyelesaikan Evaluasi Awal", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CaseManagementEvaluation", "Update")]
        public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.CompleteAsync(id, User, this.CurrentUserId(), cancellationToken));

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<CaseManagementEvaluationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Case Management Evaluation", Description = "MPP membatalkan konsep Evaluasi Awal beralasan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CaseManagementEvaluation", "Update")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelCaseManagementEvaluationRequest request, CancellationToken cancellationToken = default) =>
            this.ToActionResult(await _service.CancelAsync(id, request.Reason, User, this.CurrentUserId(), cancellationToken));

        /// <summary>
        /// Addendum Evaluasi Awal — <b>belum tersedia</b>, menjawab <c>501</c> sampai jenis dokumen <c>14</c>
        /// disetujui pemilik <c>MedicalRecordManagement</c> (<c>INT-KEP-12</c>).
        /// </summary>
        [HttpPost("{id:guid}/addendums")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status501NotImplemented)]
        [AccessAction("Amend", "Amend Case Management Evaluation", Description = "Menambah addendum Evaluasi Awal yang sudah selesai", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("CaseManagementEvaluation", "Amend")]
        public IActionResult CreateAddendum(Guid id, [FromBody] CaseManagementEvaluationAddendumRequest request) =>
            StatusCode(StatusCodes.Status501NotImplemented, ApiResponse<object>.Fail(
                StatusCodes.Status501NotImplemented,
                CaseManagementEvaluationService.AlasanAddendumBelumTersedia,
                new { code = "ADDENDUM_NOT_AVAILABLE", dependency = "INT-KEP-12" }));
    }
}
