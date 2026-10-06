using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/operating-room-management/cases")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Case",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomCase",
    Description = "Pengelolaan kasus operasi pasien",
    SortOrder = 1)]
[Tags("Health Services / Operating Room Management / Cases")]
public class OperatingRoomCaseController(OperatingRoomCaseService service,
    OperatingRoomExecutionService executionService,
    OperatingRoomPostOperativeSummaryQuery postOperativeSummaryQuery) : ControllerBase
{
    /// <summary>Ringkasan operasi baca-saja untuk bangsal dan dokter (BE-RWI-180, API 11.5.1).</summary>
    /// <remarks>
    /// Laporan operasi draft → <c>ReportFinal = false</c>, isi klinis kosong, pesan "Laporan operasi
    /// belum final". Tidak memuat rupiah dan tidak menulis apa pun.
    /// </remarks>
    [HttpGet("{id:guid}/post-operative-summary")]
    [ProducesResponseType(typeof(ApiResponse<PostOperativeSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Case", Description = "Melihat ringkasan operasi baca-saja", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> GetPostOperativeSummary(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await postOperativeSummaryQuery.GetAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus operasi tidak ditemukan."));

        return Ok(ApiResponse<PostOperativeSummaryResponse>.Ok(result, result.ReportFinal
            ? "Ringkasan operasi berhasil diambil."
            : OperatingRoomPostOperativeSummaryQuery.ReportNotFinalMessage));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OprCaseSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Operating Room Case", Description = "Melihat daftar kasus operasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] OprCasePagedQuery request, CancellationToken cancellationToken = default)
    {
        var result = await service.GetPagedAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<OprCaseSummaryResponse>>.Ok(result, "Daftar kasus operasi berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Case", Description = "Melihat detail kasus operasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await service.GetDetailAsync(id, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus operasi tidak ditemukan."))
            : Ok(ApiResponse<OprCaseDetailResponse>.Ok(result, "Detail kasus operasi berhasil diambil."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OprCaseDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [AccessAction("Create", "Create Operating Room Case", Description = "Membuat permintaan operasi", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("OperatingRoomCase", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateOprCaseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<OprCaseDetailResponse>.Ok(result, "Permintaan operasi berhasil dibuat."));
        }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Case", Description = "Memperbarui permintaan operasi", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("OperatingRoomCase", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOprCaseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.UpdateAsync(id, request, cancellationToken);
            return Ok(ApiResponse<OprCaseDetailResponse>.Ok(result, "Permintaan operasi berhasil diperbarui."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    /// <summary>Menolak order operasi berstatus Diminta dengan alasan (BE-RWI-174, API 11.5.1).</summary>
    /// <remarks>
    /// Alasan 10–500 karakter, header <c>Idempotency-Key</c>, dan <c>ExpectedVersion</c>. Kasus selain
    /// Diminta → 422 <c>OPR-CASE-REJ-001</c>; versi berubah → 409. Status Ditolak final.
    /// </remarks>
    [HttpPatch("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Reject", "Reject Operating Room Case", Description = "Menolak order operasi berstatus Diminta dengan alasan", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("OperatingRoomCase", "Reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectOprCaseRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.RejectAsync(id, request, idempotencyKey, cancellationToken);
            return Ok(ApiResponse<OprCaseDetailResponse>.Ok(result, "Order operasi berhasil ditolak."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPatch("{id:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Execution", Description = "Memulai operasi", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("OperatingRoomExecution", "Update")]
    public async Task<IActionResult> Start(Guid id, [FromBody] StartOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await executionService.StartAsync(id, request, cancellationToken);
            return Ok(ApiResponse<OprCaseStatusResponse>.Ok(result, "Operasi berhasil dimulai."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Cancel", "Cancel Operating Room Case", Description = "Membatalkan kasus operasi sebelum dimulai", AccessType = AccessTypes.Delete, SortOrder = 5)]
    [AccessPermission("OperatingRoomCase", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await executionService.CancelAsync(id, request, cancellationToken);
            return Ok(ApiResponse<OprCaseStatusResponse>.Ok(result, "Kasus operasi berhasil dibatalkan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
