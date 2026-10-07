using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/operating-room-management/cases/{caseId:guid}/preparation")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Preparation",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomPreparation",
    Description = "Persiapan, checklist keselamatan, dan kesiapan kasus operasi",
    SortOrder = 3)]
[Tags("Health Services / Operating Room Management / Preparation")]
public class OperatingRoomPreparationController(OperatingRoomPreparationService service,
    OprWardPreOpService wardPreOpService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<OprPreparationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Preparation", Description = "Melihat consent, checklist, dan sign-off kesiapan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomPreparation", "Read")]
    public async Task<IActionResult> Get(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await service.GetAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus operasi tidak ditemukan."))
            : Ok(ApiResponse<OprPreparationResponse>.Ok(result, "Data persiapan operasi berhasil diambil."));
    }

    [HttpPut("checklists/{phase}")]
    [ProducesResponseType(typeof(ApiResponse<OprChecklistResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Preparation", Description = "Menyimpan checklist keselamatan per fase", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomPreparation", "Update")]
    public async Task<IActionResult> SaveChecklist(Guid caseId, OprChecklistPhase phase,
        [FromBody] SaveOprChecklistRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.SaveChecklistAsync(caseId, phase, request, cancellationToken);
            return Ok(ApiResponse<OprChecklistResponse>.Ok(result, "Checklist keselamatan berhasil disimpan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPost("sign-offs")]
    [ProducesResponseType(typeof(ApiResponse<OprPreparationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Preparation", Description = "Memberikan sign-off kesiapan operasi", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomPreparation", "Update")]
    public async Task<IActionResult> CreateSignOff(Guid caseId,
        [FromBody] CreateOprReadinessSignOffRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.CreateSignOffAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprPreparationResponse>.Ok(result, "Sign-off kesiapan berhasil dicatat."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPost("emergency-bypass")]
    [ProducesResponseType(typeof(ApiResponse<OprPreparationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Preparation", Description = "Mencatat jalur darurat persiapan operasi", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomPreparation", "Update")]
    public async Task<IActionResult> CreateEmergencyBypass(Guid caseId,
        [FromBody] CreateOprEmergencyBypassRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.CreateEmergencyBypassAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprPreparationResponse>.Ok(result, "Jalur darurat persiapan berhasil dicatat."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    // =====================================================================
    // BE-RWI-176 — Catatan Pra-Operasi bangsal (kontrak 0.10.0 API 11.3)
    // =====================================================================

    /// <summary>Versi Catatan Pra-Operasi terbaru beserta butir dan penandaan.</summary>
    /// <remarks>Kasus tanpa versi mendapat templat butir aktif (<c>IsTemplate = true</c>).</remarks>
    [HttpGet("ward-pre-op")]
    [ProducesResponseType(typeof(ApiResponse<WardPreOpResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Ward Pre-Op", Description = "Melihat Catatan Pra-Operasi bangsal", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomWardPreOp", "Read")]
    public async Task<IActionResult> GetWardPreOp(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await wardPreOpService.GetLatestAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus operasi tidak ditemukan."))
            : Ok(ApiResponse<WardPreOpResponse>.Ok(result, result.IsTemplate
                ? "Catatan pra-operasi belum dibuat; templat butir aktif dikembalikan."
                : "Catatan pra-operasi berhasil diambil."));
    }

    /// <summary>Seluruh versi Catatan Pra-Operasi, termasuk yang perlu diperbarui dan yang digantikan.</summary>
    [HttpGet("ward-pre-op/versions")]
    [ProducesResponseType(typeof(ApiResponse<List<WardPreOpVersionSummary>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Ward Pre-Op", Description = "Melihat riwayat versi Catatan Pra-Operasi bangsal", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomWardPreOp", "Read")]
    public async Task<IActionResult> GetWardPreOpVersions(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await wardPreOpService.GetVersionsAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus operasi tidak ditemukan."))
            : Ok(ApiResponse<List<WardPreOpVersionSummary>>.Ok(result, "Riwayat versi catatan pra-operasi berhasil diambil."));
    }

    /// <summary>Simpan draf pengirim; membuat versi baru bila versi terbaru perlu diperbarui.</summary>
    [HttpPut("ward-pre-op/draft")]
    [ProducesResponseType(typeof(ApiResponse<WardPreOpResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Send", "Send Operating Room Ward Pre-Op", Description = "Mengisi dan mengirim Catatan Pra-Operasi dari bangsal", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomWardPreOp", "Send")]
    public async Task<IActionResult> SaveWardPreOpDraft(Guid caseId, [FromBody] SaveWardPreOpDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await wardPreOpService.SaveDraftAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<WardPreOpResponse>.Ok(result, "Draf catatan pra-operasi berhasil disimpan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    /// <summary>Kirim draf; tanda vital dan nyeri terakhir dibekukan sebagai potret.</summary>
    [HttpPatch("ward-pre-op/send")]
    [ProducesResponseType(typeof(ApiResponse<WardPreOpResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Send", "Send Operating Room Ward Pre-Op", Description = "Mengisi dan mengirim Catatan Pra-Operasi dari bangsal", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomWardPreOp", "Send")]
    public async Task<IActionResult> SendWardPreOp(Guid caseId, [FromBody] SendWardPreOpRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await wardPreOpService.SendAsync(caseId, request, idempotencyKey, cancellationToken);
            return Ok(ApiResponse<WardPreOpResponse>.Ok(result, "Catatan pra-operasi berhasil dikirim ke Kamar Operasi."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    /// <summary>Konfirmasi penerima per butir dan penandaan, dari akun yang berbeda dengan pengirim.</summary>
    [HttpPatch("ward-pre-op/confirm")]
    [ProducesResponseType(typeof(ApiResponse<WardPreOpResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Confirm", "Confirm Operating Room Ward Pre-Op", Description = "Mengonfirmasi Catatan Pra-Operasi bangsal di Kamar Operasi", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("OperatingRoomWardPreOp", "Confirm")]
    public async Task<IActionResult> ConfirmWardPreOp(Guid caseId, [FromBody] ConfirmWardPreOpRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await wardPreOpService.ConfirmAsync(caseId, request, idempotencyKey, cancellationToken);
            return Ok(ApiResponse<WardPreOpResponse>.Ok(result, "Konfirmasi catatan pra-operasi berhasil disimpan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
