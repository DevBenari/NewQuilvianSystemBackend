using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;

/// <summary>
/// Controller API resmi integrasi Rawat Inap (Inpatient) dengan Billing Management.
/// Mengelola penerimaan beban sewa kamar harian, ringkasan kelayakan finansial bangsal,
/// evaluasi ulang status clearance kepulangan pasien ranap, validasi deposit tindakan besar,
/// dan pengakuan surat handoff kelayakan (BKC-DEC-112, BKC-DEC-114, BKC-DEC-115, BKC-DES-043, BKC-DES-045, BKC-DES-047, BIL-API-1.4, BIL-PERMISSION-1.2).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/billing")]
[AccessController(
    "HEALTH_SERVICE_BILLING_MANAGEMENT_INPATIENT",
    "Health Service Billing Management",
    "Billing Inpatient Integration",
    AreaName = "HealthServices",
    ControllerName = "BillingInpatient",
    Description = "Inpatient billing integration, room occupancy charges, and clearance handoff",
    SortOrder = 7)]
[Tags("BillingInpatientIntegration")]
public sealed class InpatientClearanceController : ControllerBase
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing.InpatientClearanceController";
    private readonly IInpatientClearanceService _clearanceService;
    private readonly IInpatientRoomChargeCalculationService _roomChargeCalculationService;
    private readonly LoggerService _loggerService;

    public InpatientClearanceController(
        IInpatientClearanceService clearanceService,
        IInpatientRoomChargeCalculationService roomChargeCalculationService,
        LoggerService loggerService)
    {
        _clearanceService = clearanceService ?? throw new ArgumentNullException(nameof(clearanceService));
        _roomChargeCalculationService = roomChargeCalculationService ?? throw new ArgumentNullException(nameof(roomChargeCalculationService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    /// <summary>
    /// Menerima beban sewa kamar harian dari outbox Rawat Inap (ROOM_STAY).
    /// Menerapkan potongan jam masuk &amp; split transfer kamar harian.
    /// Mematuhi BKC-DEC-112, BKC-DES-043, BIL-API-1.4, BIL-VAL-118, BIL-VAL-119.
    /// </summary>
    [HttpPost("invoices/occupancy-charges")]
    [AccessAction("Create", "Create Occupancy Charge", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("BillingInpatient", "Create")]
    [ProducesResponseType(typeof(ApiResponse<OccupancyChargeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessOccupancyCharge(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] OccupancyChargeRequest request,
        CancellationToken cancellationToken)
    {
        if (idempotencyKey == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Header 'Idempotency-Key' wajib disertakan dan berupa GUID valid."));
        }

        if (request == null)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Payload beban sewa kamar tidak boleh kosong."));
        }

        if (request.EncounterId == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Identitas kunjungan (EncounterId) wajib diisi."));
        }

        try
        {
            var result = await _roomChargeCalculationService.ProcessOccupancyChargeAsync(request, cancellationToken);
            return Ok(ApiResponse<OccupancyChargeResponse>.Ok(
                result, "Beban sewa kamar berhasil dicatat pada invoice berjalan."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, exception.Message));
        }
    }

    /// <summary>
    /// Menyajikan ringkasan kelayakan finansial rawat inap untuk bangsal (status clearance, blocker operasional, saldo deposit, tagihan berjalan).
    /// Tanpa rincian nominal per item demi privasi perawat (BKC-DEC-115, BKC-DES-045, BIL-API-1.4).
    /// </summary>
    [HttpGet("invoices/encounter/{encounterId:guid}/inpatient-summary")]
    [AccessAction("Read", "Read Inpatient Billing Summary", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("BillingInpatient", "Read")]
    [ProducesResponseType(typeof(ApiResponse<InpatientBillingSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInpatientSummary(
        [FromRoute] Guid encounterId,
        [FromQuery] bool? includeFinancial,
        CancellationToken cancellationToken)
    {
        if (encounterId == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Identitas kunjungan (EncounterId) tidak boleh kosong."));
        }

        try
        {
            // Proteksi data sensitif finansial: hanya peran dengan hak finansial/kasir/admin yang menerima rincian angka rupiah
            var hasFinancialPermission = User.Claims.Any(c =>
                (c.Type == "permission" && (c.Value.Contains("BillingInvoice") || c.Value.Contains("BillingSettlement") || c.Value.Contains("Financial")))
                || (c.Type == ClaimTypes.Role && (c.Value.Contains("Admin") || c.Value.Contains("Cashier") || c.Value.Contains("Finance"))));

            var shouldIncludeFinancial = hasFinancialPermission && (includeFinancial ?? true);

            var result = await _clearanceService.GetInpatientBillingSummaryAsync(
                encounterId, shouldIncludeFinancial, cancellationToken);

            return Ok(ApiResponse<InpatientBillingSummaryResponse>.Ok(
                result, "Ringkasan tagihan rawat inap berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
    }

    /// <summary>
    /// Memeriksa ulang seluruh prasyarat finansial encounter rawat inap dan menerbitkan status kelayakan pulang (CLEARED, BLOCKED, atau REVOKED).
    /// Mematuhi BKC-DEC-115, BKC-DES-045, BIL-API-1.4. Larangan keras: Tidak ada override manual; status dievaluasi secara deklaratif dari invoice &amp; pelunasan.
    /// </summary>
    [HttpPost("inpatient-clearance/reevaluate")]
    [AccessAction("Clearance", "Reevaluate Inpatient Financial Clearance", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("BillingInpatient", "Clearance")]
    [ProducesResponseType(typeof(ApiResponse<InpatientClearanceHandoffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReevaluateClearance(
        [FromBody] ReevaluateInpatientClearanceRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null || request.EncounterId == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Identitas kunjungan (EncounterId) wajib diisi untuk evaluasi ulang clearance."));
        }

        try
        {
            var actorUserId = CurrentUserId();
            var actorUserName = CurrentUserName();
            var now = DateTimeOffset.UtcNow;
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();

            var handoff = await _clearanceService.EvaluateClearanceAsync(
                request.EncounterId,
                InpatientClearanceReasonCodes.ReevaluatedByCashier,
                actorUserId,
                now,
                correlationId,
                causationId,
                cancellationToken,
                request.Reason);

            var response = MapToHandoffResponse(handoff, actorUserName);
            return Ok(ApiResponse<InpatientClearanceHandoffResponse>.Ok(
                response, "Pemeriksaan kelayakan berhasil; status clearance diterbitkan."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, exception.Message));
        }
    }

    /// <summary>
    /// Validasi deposit 100% dari ekses/tanggung jawab pasien atas tindakan/operasi besar.
    /// Mematuhi BKC-DEC-114, BKC-DES-047, BIL-VAL-121.
    /// </summary>
    [HttpPost("inpatient-clearance/validate-major-procedure-deposit")]
    [AccessAction("ValidateDeposit", "Validate Major Procedure Deposit", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("BillingInpatient", "ValidateDeposit")]
    [ProducesResponseType(typeof(ApiResponse<MajorProcedureDepositValidationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateMajorProcedureDeposit(
        [FromBody] MajorProcedureDepositValidationRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null || request.EncounterId == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Identitas kunjungan (EncounterId) wajib diisi untuk validasi deposit tindakan besar."));
        }

        try
        {
            var result = await _clearanceService.ValidateMajorProcedureDepositAsync(request, cancellationToken);
            return Ok(ApiResponse<MajorProcedureDepositValidationResult>.Ok(result, result.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, exception.Message));
        }
    }

    /// <summary>
    /// Mengakui penerimaan surat handoff kelayakan oleh bangsal rawat inap secara idempoten (BIL-VAL-116, BIL-VAL-125).
    /// </summary>
    [HttpPatch("inpatient-clearance/{id:guid}/acknowledge")]
    [AccessAction("Acknowledge", "Acknowledge Inpatient Clearance Handoff", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("BillingInpatient", "Acknowledge")]
    [ProducesResponseType(typeof(ApiResponse<InpatientClearanceHandoffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcknowledgeClearance(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Identitas surat handoff tidak boleh kosong."));
        }

        try
        {
            var actorUserId = CurrentUserId();
            var actorUserName = CurrentUserName();
            var now = DateTimeOffset.UtcNow;

            var handoff = await _clearanceService.AcknowledgeClearanceHandoffAsync(
                id, actorUserId, now, cancellationToken);

            var response = MapToHandoffResponse(handoff, actorUserName);
            return Ok(ApiResponse<InpatientClearanceHandoffResponse>.Ok(
                response, "Surat kelayakan rawat inap berhasil diakui."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
    }

    /// <summary>
    /// Mengambil surat kelayakan rawat inap aktif terakhir untuk encounter.
    /// </summary>
    [HttpGet("inpatient-clearance/encounter/{encounterId:guid}/latest")]
    [AccessAction("ReadLatest", "Read Latest Inpatient Clearance", AccessType = AccessTypes.Read, SortOrder = 6)]
    [AccessPermission("BillingInpatient", "ReadLatest")]
    [ProducesResponseType(typeof(ApiResponse<InpatientClearanceHandoffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestClearance(
        [FromRoute] Guid encounterId,
        CancellationToken cancellationToken)
    {
        if (encounterId == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Identitas kunjungan (EncounterId) tidak boleh kosong."));
        }

        var handoff = await _clearanceService.GetLatestClearanceForEncounterAsync(encounterId, cancellationToken);
        if (handoff == null)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, $"Surat kelayakan rawat inap untuk encounter {encounterId} tidak ditemukan."));
        }

        var response = MapToHandoffResponse(handoff);
        return Ok(ApiResponse<InpatientClearanceHandoffResponse>.Ok(
            response, "Surat kelayakan rawat inap terbaru berhasil diambil."));
    }

    private static InpatientClearanceHandoffResponse MapToHandoffResponse(
        BilInpatientClearanceHandoff handoff,
        string? actorUserName = null)
    {
        return new InpatientClearanceHandoffResponse
        {
            HandoffId = handoff.Id,
            EncounterId = handoff.EncounterId,
            InvoiceId = handoff.InvoiceId,
            ClearanceStatus = handoff.ClearanceStatus,
            FinancialOutcome = handoff.FinancialOutcome,
            OutstandingBalance = handoff.OutstandingBalance,
            TotalPatientResponsibility = handoff.TotalPatientResponsibility,
            TotalPaidOrAllocated = handoff.TotalPaidOrAllocated,
            ReasonCode = handoff.ReasonCode,
            RevocationReason = handoff.RevocationReason,
            FinancialVersion = handoff.FinancialVersion,
            EffectiveAt = handoff.EffectiveAt,
            Status = handoff.Status,
            AcknowledgedAt = handoff.AcknowledgedAt,
            Reason = handoff.FinancialOutcome ?? handoff.RevocationReason ?? handoff.ReasonCode,
            ClearedAt = handoff.ClearanceStatus == InpatientClearanceStatuses.Cleared ? handoff.EffectiveAt : null,
            ClearedByUserName = actorUserName
        };
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }

    private string? CurrentUserName()
    {
        return User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("name")
            ?? User.FindFirstValue("preferred_username");
    }
}
