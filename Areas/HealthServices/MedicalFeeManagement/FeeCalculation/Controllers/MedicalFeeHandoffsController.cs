using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Controllers
{
    /// <summary>
    /// Controller pengelola penyerahan hasil jasa medis ke Finance AP (MdfFinanceHandoff).
    /// Mengimplementasikan kontrak MDF-API-1.0 §7, MDF-INTEGRATION-1.0 §7, dan matriks izin MDF-PERM-1.0 §2.
    /// Pola arsitektur: Handoff / Integration Gateway (pola BilArHandoff).
    /// 
    /// CATATAN PENTING ARSITEKTUR & KEAMANAN:
    /// 1. Endpoint pembuatan manual (POST /finance-handoffs) sengaja DITOLAK (api-contract.md §7, T-79).
    ///    Penyerahan HANYA lahir dari persetujuan hasil jasa medis periode kalkulasi (MDF-DES-018, T-71).
    /// 2. Nilai penyerahan adalah nilai kotor (GrossAmount = FinalAmount), sebelum potongan pajak PPh 21 atau kasbon (MF-DEC-005, T-72).
    /// 3. Medical Fee TIDAK PERNAH menulis langsung ke tabel modul Finance (Rule 8, MDF-INTEGRATION-1.0 §7).
    /// 4. Endpoint acknowledgement dan failure disediakan khusus untuk dipanggil oleh modul Finance AP.
    /// 5. Pengiriman ulang (resend) mempertahankan HandoffKey agar modul Finance tetap idempoten (MDF-VAL-1.0 §7.7, T-76).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-fee-management/finance-handoffs")]
    [AccessController(
        moduleCode: "MEDICAL_FEE_MANAGEMENT",
        moduleName: "Medical Fee Management",
        displayName: "Medical Fee Finance Handoff",
        AreaName = "HealthServices",
        ControllerName = "MedicalFeeHandoff",
        Description = "Mengelola penyerahan hasil jasa medis ke modul Finance AP (MdfFinanceHandoff)",
        SortOrder = 6
    )]
    [Tags("Health Services / Medical Fee Management / Finance Handoffs")]
    public class MedicalFeeHandoffsController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalFeeManagement.FeeCalculation.FinanceHandoff";

        private readonly MedicalFeeHandoffService _handoffService;
        private readonly LoggerService _loggerService;

        public MedicalFeeHandoffsController(
            MedicalFeeHandoffService handoffService,
            LoggerService loggerService)
        {
            _handoffService = handoffService;
            _loggerService = loggerService;
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value
                        ?? User.FindFirst("id")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        /// <summary>
        /// Mengambil opsi metadata untuk filter penyerahan Finance (Status, Jenis Penerima, Periode).
        /// </summary>
        [HttpGet("metadata")]
        [AccessAction("View", "View Medical Fee Finance Handoff Metadata", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeHandoff", "View")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeHandoffMetadataResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMetadata(CancellationToken ct)
        {
            var metadata = await _handoffService.GetMetadataAsync(ct);
            return Ok(ApiResponse<MedicalFeeHandoffMetadataResponse>.Ok(metadata, "Metadata filter penyerahan Finance berhasil diambil."));
        }

        /// <summary>
        /// Mengambil daftar penyerahan hasil jasa medis ke Finance AP secara terpaginasi (MDF-API-1.0 §7).
        /// </summary>
        [HttpGet]
        [AccessAction("View", "View Medical Fee Finance Handoffs", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("MedicalFeeHandoff", "View")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<MedicalFeeHandoffItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] MedicalFeeHandoffListQuery query, CancellationToken ct)
        {
            var result = await _handoffService.GetPagedAsync(query, ct);
            return Ok(ApiResponse<PagedResult<MedicalFeeHandoffItemDto>>.Ok(result, "Daftar penyerahan jasa ke Finance berhasil diambil."));
        }

        /// <summary>
        /// Mengambil detail lengkap satu baris penyerahan ke Finance berdasarkan ID (MDF-API-1.0 §7).
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("View", "View Medical Fee Finance Handoff Detail", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("MedicalFeeHandoff", "View")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeHandoffDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _handoffService.GetByIdAsync(id, ct);
                return Ok(ApiResponse<MedicalFeeHandoffDetailDto>.Ok(result, "Detail penyerahan ke Finance berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        /// <summary>
        /// Dipanggil oleh modul Finance AP untuk mengonfirmasi penerimaan penyerahan (ACK) — MDF-API-1.0 §7, MDF-VAL-1.0 §7.8, T-75.
        /// HandoffKey wajib disertakan dan cocok dengan data tersimpan.
        /// </summary>
        [HttpPost("{id:guid}/acknowledgement")]
        [AccessAction("Acknowledge", "Acknowledge Medical Fee Finance Handoff", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MedicalFeeHandoff", "Acknowledge")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeHandoffDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Acknowledge(
            [FromRoute] Guid id,
            [FromBody] AcknowledgeHandoffRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data konfirmasi penyerahan tidak valid."));
            }

            var userId = GetUserId();
            try
            {
                var result = await _handoffService.AcknowledgeAsync(id, request, userId, ct);
                await _loggerService.AuditAsync(
                    LogCategory,
                    "MedicalFeeFinanceHandoffAcknowledged",
                    $"Penyerahan hasil jasa ID '{id}' berhasil dikonfirmasi (ACK) oleh Finance. HandoffKey: {request.HandoffKey}.",
                    new { userId });

                return Ok(ApiResponse<MedicalFeeHandoffDetailDto>.Ok(result, "Penyerahan hasil jasa berhasil dikonfirmasi oleh Finance."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        /// <summary>
        /// Dipanggil oleh modul Finance AP untuk menolak penyerahan karena ketidakkonsistenan data (Failure) — MDF-API-1.0 §7, MDF-VAL-1.0 §7.9, T-77.
        /// Wajib menyertakan alasan penolakan minimal 10 karakter.
        /// </summary>
        [HttpPost("{id:guid}/failure")]
        [AccessAction("Acknowledge", "Record Medical Fee Finance Handoff Failure", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MedicalFeeHandoff", "Acknowledge")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeHandoffDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Fail(
            [FromRoute] Guid id,
            [FromBody] FailHandoffRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data penolakan penyerahan tidak valid."));
            }

            var userId = GetUserId();
            try
            {
                var result = await _handoffService.FailAsync(id, request, userId, ct);
                await _loggerService.AuditAsync(
                    LogCategory,
                    "MedicalFeeFinanceHandoffFailed",
                    $"Penyerahan hasil jasa ID '{id}' ditolak oleh Finance. Alasan: {request.FailureReason}.",
                    new { userId });

                return Ok(ApiResponse<MedicalFeeHandoffDetailDto>.Ok(result, "Penolakan penyerahan dari Finance berhasil dicatat."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        /// <summary>
        /// Mengirim ulang penyerahan yang berstatus Failed setelah penyebabnya diperbaiki — MDF-API-1.0 §7, MDF-STATE-1.0 §5, T-76.
        /// HandoffKey sengaja DIPERTAHANKAN dan tidak berubah (Rule 7.7, T-76).
        /// </summary>
        [HttpPost("{id:guid}/resend")]
        [AccessAction("Manage", "Resend Failed Medical Fee Finance Handoff", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("MedicalFeeHandoff", "Manage")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeHandoffDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Resend(
            [FromRoute] Guid id,
            [FromBody] ResendHandoffRequest? request,
            CancellationToken ct)
        {
            var userId = GetUserId();
            try
            {
                var result = await _handoffService.ResendAsync(id, request, userId, ct);
                await _loggerService.AuditAsync(
                    LogCategory,
                    "MedicalFeeFinanceHandoffResent",
                    $"Penyerahan hasil jasa ID '{id}' berhasil dikirim ulang ke status Created dengan HandoffKey tetap: {result.HandoffKey}.",
                    new { userId });

                return Ok(ApiResponse<MedicalFeeHandoffDetailDto>.Ok(result, "Penyerahan hasil jasa yang gagal berhasil dikirim ulang ke status Created dengan HandoffKey tetap."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }
    }
}

