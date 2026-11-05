using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Controller restitusi salah potong cicilan piutang RS ke slip gaji periode berikutnya (BE-FIN-111). Sejak Payroll menjadi modul
/// mandiri rute utamanya di bawah payroll-management; rute lama human-resource dipertahankan sebagai alias kompatibilitas.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/payroll-restitutions")]
// Alias kompatibilitas: rute sebelum Payroll dipisah dari HRD. Hapus bertahap setelah seluruh consumer berpindah ke rute utama.
[Route("api/v1/corporate/human-resource/payroll-restitutions")]
[AccessController(
    moduleCode: "CORPORATE_HUMAN_RESOURCE_PAYROLL_RESTITUTION",
    moduleName: "Corporate Human Resource Payroll Restitution",
    displayName: "Payroll Restitution",
    AreaName = "Corporate",
    ControllerName = "PayrollRestitution",
    Description = "Layanan restitusi salah potong cicilan piutang RS ke slip gaji periode berikutnya di HR",
    SortOrder = 56)]
[Tags("Corporate / Payroll Management / Restitution")]
public sealed class PayrollRestitutionController : ControllerBase
{
    private readonly PayrollRestitutionService _service;
    private readonly PayrollEmployeeDeductionService _deductionService;

    public PayrollRestitutionController(PayrollRestitutionService service, PayrollEmployeeDeductionService deductionService)
    {
        _service = service;
        _deductionService = deductionService;
    }

    /// <summary>
    /// Daftar restitusi salah potong yang disematkan pada slip periode berikutnya (monitoring read-only). Berisi posisi tiap restitusi
    /// pada siklus run tujuannya: menunggu hitung, sudah dihitung, atau terkunci.
    /// </summary>
    [HttpGet]
    [AccessAction("Read", "Read Payroll Restitutions", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("PayrollRestitution", "Read")]
    public Task<IActionResult> GetRestitutions([FromQuery] PayrollRestitutionQuery query, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _deductionService.GetRestitutionsAsync(query, cancellationToken), "Daftar restitusi berhasil dimuat.");

    /// <summary>
    /// Eksekusi restitusi salah potong cicilan ke slip gaji periode berikutnya.
    /// </summary>
    [HttpPost]
    [AccessAction("ProcessRestitution", "Process Restitution", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("PayrollRestitution", "ProcessRestitution")]
    [ProducesResponseType(typeof(ApiResponse<PayrollRestitutionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessRestitution([FromBody] PayrollRestitutionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.ActorUserId == Guid.Empty)
            {
                request.ActorUserId = CurrentUserId();
            }

            var result = await _service.ProcessRestitutionAsync(request, cancellationToken);
            return Ok(ApiResponse<PayrollRestitutionResponse>.Ok(result, result.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(404, ex.Message));
        }
        catch (PayrollRunConflictException ex)
        {
            // Run periode tujuan sudah tidak menerima penyesuaian dan tidak ada periode berikutnya: bukan kegagalan sunyi.
            return Conflict(ApiResponse<object>.Fail(409, ex.Message));
        }
    }

    /// <summary>
    /// Ambil informasi restitusi berdasarkan SourceId (misal Id piutang yang dibatalkan).
    /// </summary>
    [HttpGet("by-source/{sourceId:guid}")]
    [AccessAction("GetBySourceId", "Get Restitution By Source Id", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PayrollRestitution", "GetBySourceId")]
    [ProducesResponseType(typeof(ApiResponse<PayrollRestitutionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySourceId(Guid sourceId, CancellationToken cancellationToken)
    {
        var result = await _service.GetRestitutionBySourceIdAsync(sourceId, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, $"Restitusi untuk SourceId {sourceId} tidak ditemukan."));
        }
        return Ok(ApiResponse<PayrollRestitutionResponse>.Ok(result, "Data restitusi berhasil ditemukan."));
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}

