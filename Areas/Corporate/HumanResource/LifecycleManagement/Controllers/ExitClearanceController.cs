using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers;

/// <summary>
/// Controller Exit Clearance HR dengan penegakan gerbang bebas tanggungan Finance (BE-FIN-112).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/human-resource/lifecycle-management/exit-clearances")]
[AccessController(
    moduleCode: "HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE",
    moduleName: "Human Resource Lifecycle Exit Clearance",
    displayName: "Exit Clearance Administration",
    AreaName = "Corporate",
    ControllerName = "ExitClearance",
    Description = "Administrasi exit clearance karyawan dan penegakan gerbang bebas tanggungan Finance",
    SortOrder = 21)]
[Tags("Corporate / Human Resource / Lifecycle Management / Exit Clearance")]
public sealed class ExitClearanceController : ControllerBase
{
    private readonly ExitClearanceService _service;

    public ExitClearanceController(ExitClearanceService service)
    {
        _service = service;
    }

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Exit Clearance", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("ExitClearance", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ExitClearanceFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken)
    {
        var result = await _service.GetFilterMetadataAsync(cancellationToken);
        return Ok(ApiResponse<ExitClearanceFilterMetadataResponse>.Ok(result, "Metadata filter berhasil diambil."));
    }

    [HttpGet("summary")]
    [AccessAction("Read", "Read Exit Clearance", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("ExitClearance", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ExitClearanceSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _service.GetSummaryAsync(cancellationToken);
        return Ok(ApiResponse<ExitClearanceSummaryResponse>.Ok(result, "Ringkasan exit clearance berhasil diambil."));
    }

    [HttpGet]
    [AccessAction("Read", "Read Exit Clearance", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("ExitClearance", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ExitClearanceListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] ExitClearanceQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExitClearanceListResponse>>.Ok(result, "Daftar exit clearance berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Exit Clearance", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("ExitClearance", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ExitClearanceDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return Ok(ApiResponse<ExitClearanceDetailResponse>.Ok(result, "Detail exit clearance berhasil diambil."));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    [HttpGet("by-separation/{separationId:guid}")]
    [AccessAction("Read", "Read Exit Clearance", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("ExitClearance", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ExitClearanceDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySeparationId(Guid separationId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetBySeparationIdAsync(separationId, cancellationToken);
            return Ok(ApiResponse<ExitClearanceDetailResponse>.Ok(result, "Exit clearance untuk pemisahan pegawai berhasil diambil."));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    [HttpPost]
    [AccessAction("Create", "Create Exit Clearance", AccessType = AccessTypes.Create, SortOrder = 6)]
    [AccessPermission("ExitClearance", "Create")]
    [ProducesResponseType(typeof(ApiResponse<ExitClearanceDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateExitClearanceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ExitClearanceDetailResponse>.Ok(result, "Exit clearance berhasil dibuat."));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Memperbarui checklist aspek departemen, aset, IT, dan HR.
    /// AC 3: Flag IsFinanceCleared TIDAK DAPAT diubah lewat form ini.
    /// </summary>
    [HttpPut("{id:guid}/checklist")]
    [AccessAction("Update", "Update Exit Clearance Checklist", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("ExitClearance", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ExitClearanceDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateChecklist(
        Guid id, [FromBody] UpdateExitClearanceChecklistRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.UpdateChecklistAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ExitClearanceDetailResponse>.Ok(result, "Checklist exit clearance berhasil diperbarui."));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Memeriksa ulang live status bebas tanggungan langsung ke Finance API (AC 2, INT-HR-FIN-002 §P.4).
    /// </summary>
    [HttpPost("{id:guid}/verify-finance")]
    [AccessAction("Update", "Verify Finance Clearance", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("ExitClearance", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ExitClearanceFinanceCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyFinance(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.VerifyFinanceClearanceAsync(id, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ExitClearanceFinanceCheckResponse>.Ok(result, result.Message));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Menyelesaikan exit clearance (AC 1 & AC 2).
    /// Gerbang penegakan: DITOLAK bila IsFinanceCleared == false (masih ada sisa piutang di Finance).
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [AccessAction("Complete", "Complete Exit Clearance", AccessType = AccessTypes.Update, SortOrder = 9)]
    [AccessPermission("ExitClearance", "Complete")]
    [ProducesResponseType(typeof(ApiResponse<ExitClearanceDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Complete(
        Guid id, [FromBody] CompleteExitClearanceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CompleteAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ExitClearanceDetailResponse>.Ok(result, "Exit clearance berhasil disahkan dan diselesaikan."));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Exit Clearance", AccessType = AccessTypes.Update, SortOrder = 10)]
    [AccessPermission("ExitClearance", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<ExitClearanceDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(
        Guid id, [FromBody] CancelExitClearanceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CancelAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ExitClearanceDetailResponse>.Ok(result, "Exit clearance berhasil dibatalkan."));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message)),
        InvalidOperationException => UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message)),
        ArgumentException => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) =>
        exception is KeyNotFoundException or InvalidOperationException or ArgumentException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}

