using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/billing/patient-funds")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING", "Health Service Billing Management", "Billing Patient Funds",
    AreaName = "HealthServices", ControllerName = "BillingPatientFunds", Description = "Inpatient deposit balance and append-only top-up ledger", SortOrder = 2)]
[Tags("Health Services / Billing Management / Billing / Patient Funds")]
public sealed class BillingPatientFundsController : ControllerBase
{
    private readonly BillingDepositService _service;
    private readonly BillingAllocationService _allocationService;

    public BillingPatientFundsController(
        BillingDepositService service,
        BillingAllocationService allocationService)
    {
        _service = service;
        _allocationService = allocationService;
    }

    [HttpPost("deposits/{encounterId:guid}/allocations")]
    [AccessAction("Allocate", "Allocate Inpatient Deposit", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("BillingDeposit", "Allocate")]
    [ProducesResponseType(typeof(ApiResponse<AllocationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AllocateDeposit(
        Guid encounterId,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] DepositAllocationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _allocationService.AllocateDepositAsync(
                encounterId, request, idempotencyKey, CurrentUserId(), cancellationToken);
            var message = result.IsReplay
                ? "Allocation sudah diproses; hasil sebelumnya dikembalikan."
                : "Dana deposit berhasil dialokasikan ke invoice.";
            return Ok(ApiResponse<AllocationResponse>.Ok(result, message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingAllocationConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingAllocationValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpGet("deposits/{encounterId:guid}")]
    [AccessAction("Read", "Read Inpatient Deposit", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingDeposit", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DepositResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeposit(
        Guid encounterId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetByEncounterAsync(encounterId, cancellationToken);
            return Ok(ApiResponse<DepositResponse>.Ok(
                result, "Saldo dan ledger deposit rawat inap berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingDepositValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    [HttpPost("deposits/{encounterId:guid}/top-ups")]
    [AccessAction("Create", "Create Inpatient Deposit Top Up", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("BillingDeposit", "Create")]
    [ProducesResponseType(typeof(ApiResponse<SettlementResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TopUp(
        Guid encounterId,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] DepositTopUpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.TopUpAsync(
                encounterId, request, idempotencyKey, CurrentUserId(), cancellationToken);
            var message = result.IsReplay
                ? "Top-up sudah diproses; hasil sebelumnya dikembalikan."
                : "Top-up deposit rawat inap berhasil dicatat.";
            return Ok(ApiResponse<SettlementResponse>.Ok(result, message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingDepositConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingDepositValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
