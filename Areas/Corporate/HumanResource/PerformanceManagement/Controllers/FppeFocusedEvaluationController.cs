using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Controllers
{
    /// <summary>
    /// REST API Controller untuk Tata Kelola FPPE (Evaluasi Praktik Profesional Terfokus) & Investigasi Insiden Klinis
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/clinical-performance/fppe")]
    [Tags("Corporate - HRD FPPE Incident & Focused Evaluation")]
    public class FppeFocusedEvaluationController : ControllerBase
    {
        private readonly IFppeTriggerDetectionService _triggerService;
        private readonly IProctoringWorkflowService _proctoringService;

        public FppeFocusedEvaluationController(
            IFppeTriggerDetectionService triggerService,
            IProctoringWorkflowService proctoringService)
        {
            _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));
            _proctoringService = proctoringService ?? throw new ArgumentNullException(nameof(proctoringService));
        }

        #region TRIGGERS FEED
        /// <summary>
        /// Menampilkan feed real-time indikator ambang batas kritis pemicu FPPE dari SIMRS/KMKP (INT-HR-74)
        /// </summary>
        [HttpGet("triggers/feed")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<FppeTriggerFeedDto>>), 200)]
        public async Task<IActionResult> GetTriggerFeed(CancellationToken cancellationToken = default)
        {
            var feed = await _triggerService.GetActiveTriggerFeedAsync(cancellationToken);
            return Ok(ApiResponse<List<FppeTriggerFeedDto>>.Ok(feed, "Feed indikator pemicu kritis FPPE berhasil diambil."));
        }

        /// <summary>
        /// Menerima webhook / event pemicu insiden keselamatan pasien asinkronus dari SIMRS/KMKP
        /// </summary>
        [HttpPost("triggers/incoming")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FppeTriggerFeedDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> ReceiveIncomingTrigger(
            [FromBody] FppeTriggerFeedDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Payload data event pemicu FPPE tidak boleh kosong."));
            }

            var result = await _triggerService.ReceiveTriggerEventAsync(request, cancellationToken);
            return Ok(ApiResponse<FppeTriggerFeedDto>.Ok(result, "Event pemicu kritis FPPE berhasil diterima dan dicatat."));
        }
        #endregion

        #region FPPE ENGAGEMENTS
        /// <summary>
        /// Menginisiasi penugasan FPPE terarah (staf baru, prosedur baru, insiden) & menunjuk proctor senior
        /// </summary>
        [HttpPost("engagements")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FppeEngagementResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CreateEngagement(
            [FromBody] CreateFppeEngagementRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi formulir inisiasi FPPE gagal."));
            }

            try
            {
                var engagement = await _proctoringService.CreateEngagementAsync(request, cancellationToken);
                return StatusCode(201, ApiResponse<FppeEngagementResponseDto>.Ok(engagement, "Penugasan FPPE berhasil diinisiasi dan proctor telah ditunjuk."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat inisiasi FPPE: {ex.Message}"));
            }
        }

        /// <summary>
        /// Menampilkan rincian penugasan FPPE, target kasus klinis, logbook tindakan, dan kesimpulan pleno
        /// </summary>
        [HttpGet("engagements/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FppeEngagementResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> GetEngagementById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var engagement = await _proctoringService.GetEngagementByIdAsync(id, cancellationToken);
            if (engagement == null)
            {
                return NotFound(ApiResponse<string>.Fail(404, $"Penugasan FPPE dengan ID {id} tidak ditemukan."));
            }

            return Ok(ApiResponse<FppeEngagementResponseDto>.Ok(engagement, "Rincian penugasan FPPE berhasil diambil."));
        }
        #endregion

        #region CASE LOGBOOK
        /// <summary>
        /// Menambahkan entri tindakan klinis terproctoring ke dalam logbook FPPE dokter berpseudonim (VR-OPP-11)
        /// </summary>
        [HttpPost("engagements/{id:guid}/case-logs")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FppeCaseLogResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> AddCaseLog(
            [FromRoute] Guid id,
            [FromBody] CreateFppeCaseLogRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi entri logbook klinis gagal."));
            }

            try
            {
                var caseLog = await _proctoringService.AddCaseLogAsync(id, request, cancellationToken);
                return StatusCode(201, ApiResponse<FppeCaseLogResponseDto>.Ok(caseLog, "Kasus klinis terproctoring berhasil ditambahkan dengan nomor RM terpseudonimkan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat pencatatan kasus: {ex.Message}"));
            }
        }

        /// <summary>
        /// Verifikasi dan penilaian kemandirian tindakan kasus oleh Dokter Proctor pendamping (VR-OPP-12)
        /// </summary>
        [HttpPut("case-logs/{logId:guid}/verify")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FppeCaseLogResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> VerifyCaseLog(
            [FromRoute] Guid logId,
            [FromBody] VerifyFppeCaseLogRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi evaluasi proctor gagal."));
            }

            try
            {
                var verifiedLog = await _proctoringService.VerifyCaseLogAsync(logId, request, cancellationToken);
                return Ok(ApiResponse<FppeCaseLogResponseDto>.Ok(verifiedLog, "Kasus klinis terproctoring berhasil diverifikasi oleh Dokter Proctor pendamping."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat verifikasi proctor: {ex.Message}"));
            }
        }
        #endregion

        #region CONCLUSION & PRIVILEGE SYNC
        /// <summary>
        /// Mengesahkan vonis sidang pleno Komite Medik atas kesimpulan FPPE (VR-OPP-13, VR-OPP-14)
        /// </summary>
        [HttpPost("engagements/{id:guid}/conclude")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FppeConclusionResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> ConcludeEngagement(
            [FromRoute] Guid id,
            [FromBody] ConcludeFppeEngagementRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi formulir putusan sidang pleno gagal."));
            }

            try
            {
                var conclusion = await _proctoringService.ConcludeEngagementAsync(id, request, cancellationToken);
                return Ok(ApiResponse<FppeConclusionResponseDto>.Ok(conclusion, "Berita acara kesimpulan sidang pleno FPPE berhasil disahkan dan ditandatangani."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat pengesahan pleno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Memperbarui status kewenangan klinis staf di sistem dan transmisi sinkronisasi ke SIMRS EMR (INT-HR-76)
        /// </summary>
        [HttpPost("engagements/{id:guid}/sync-privilege")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SyncFppePrivilegeResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> SyncPrivilege(
            [FromRoute] Guid id,
            [FromBody] SyncFppePrivilegeRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _proctoringService.SyncPrivilegeAsync(id, request, cancellationToken);
                return Ok(ApiResponse<SyncFppePrivilegeResponseDto>.Ok(result, "Pembaruan kewenangan klinis pasca-FPPE berhasil disinkronkan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat sinkronisasi kewenangan klinis: {ex.Message}"));
            }
        }
        #endregion
    }
}
