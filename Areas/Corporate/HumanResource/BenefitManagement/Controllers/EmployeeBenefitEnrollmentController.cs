using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Controllers
{
    /// <summary>
    /// Enrollment benefit pegawai — aggregate ber-lifecycle (transaction-endpoint-standard.md 2.1).
    /// BE-FIN-103 (lifecycle) dan BE-FIN-104 (buku mutasi), FIN-API-1.10 R.1. Perpindahan status lewat POST /{id}/&lt;aksi&gt;; tidak ada
    /// GET /options, PATCH /{id}/status generik, maupun DELETE — pembatalan lewat aksi cancel.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_BENEFIT_MANAGEMENT",
        moduleName: "Human Resource Benefit Management",
        displayName: "Employee Benefit Enrollment",
        AreaName = "Corporate",
        ControllerName = "EmployeeBenefitEnrollment",
        Description = "Enrollment benefit pegawai beserta saldo plafon layanan RS",
        SortOrder = 10)]
    [Tags("Corporate / Human Resource / Benefit Management / Employee Benefit Enrollment")]
    public sealed class EmployeeBenefitEnrollmentController : ControllerBase
    {
        private readonly EmployeeBenefitEnrollmentService _service;
        private readonly EmployeeBenefitBalanceService _balanceService;

        public EmployeeBenefitEnrollmentController(EmployeeBenefitEnrollmentService service, EmployeeBenefitBalanceService balanceService)
        {
            _service = service;
            _balanceService = balanceService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Employee Benefit Enrollment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitEnrollment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitEnrollmentFilterMetadataResponse>), StatusCodes.Status200OK)]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<EmployeeBenefitEnrollmentFilterMetadataResponse>.Ok(_service.GetFilterMetadata(), "Metadata filter enrollment berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Employee Benefit Enrollment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitEnrollment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitEnrollmentSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
            Ok(ApiResponse<EmployeeBenefitEnrollmentSummaryResponse>.Ok(await _service.GetSummaryAsync(cancellationToken), "Ringkasan enrollment berhasil diambil."));

        [HttpGet]
        [AccessAction("Read", "Read Employee Benefit Enrollment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitEnrollment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeBenefitEnrollmentListResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] EmployeeBenefitEnrollmentPagedQuery query, CancellationToken cancellationToken) =>
            Ok(ApiResponse<PagedResult<EmployeeBenefitEnrollmentListResponse>>.Ok(await _service.GetPagedAsync(query, cancellationToken), "Daftar enrollment berhasil diambil."));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Employee Benefit Enrollment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitEnrollment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitEnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitEnrollmentResponse>.Ok(
                await _service.GetByIdAsync(id, CurrentUserId(), cancellationToken), "Rincian enrollment berhasil diambil.")));

        [HttpGet("by-employee/{employeeId:guid}")]
        [AccessAction("Read", "Read Employee Benefit Enrollment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitEnrollment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public Task<IActionResult> GetByEmployee(Guid employeeId, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitSummaryResponse>.Ok(
                await _service.GetByEmployeeAsync(employeeId, cancellationToken), "Ringkasan benefit pegawai berhasil diambil.")));

        [HttpGet("{id:guid}/status-history")]
        [AccessAction("Read", "Read Employee Benefit Enrollment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitEnrollment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<EnrollmentStatusHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<List<EnrollmentStatusHistoryResponse>>.Ok(
                await _service.GetStatusHistoryAsync(id, cancellationToken), "Riwayat status enrollment berhasil diambil.")));

        /// <summary>Buku mutasi plafon enrollment ini, pegawai sendiri dan tanggungannya (BE-FIN-104).</summary>
        [HttpGet("{id:guid}/movements")]
        [AccessAction("Read", "Read Employee Benefit Enrollment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitEnrollment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<BenefitMovementResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public Task<IActionResult> GetMovements(Guid id, [FromQuery] BenefitMovementPagedQuery query, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<PagedResult<BenefitMovementResponse>>.Ok(
                await _balanceService.GetMovementsAsync(id, query, cancellationToken), "Buku mutasi plafon berhasil diambil.")));

        [HttpPost]
        [AccessAction("Create", "Create Employee Benefit Enrollment", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmployeeBenefitEnrollment", "Create")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitEnrollmentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public Task<IActionResult> Create([FromBody] CreateEmployeeBenefitEnrollmentRequest request, CancellationToken cancellationToken) =>
            Run(async () => StatusCode(StatusCodes.Status201Created, ApiResponse<EmployeeBenefitEnrollmentResponse>.Ok(
                await _service.CreateAsync(request, CurrentUserId(), cancellationToken), "Enrollment benefit berhasil dibuat.")));

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Employee Benefit Enrollment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmployeeBenefitEnrollment", "Update")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitEnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeBenefitEnrollmentRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitEnrollmentResponse>.Ok(
                await _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken), "Enrollment benefit berhasil diperbarui.")));

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit Employee Benefit Enrollment", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmployeeBenefitEnrollment", "Submit")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitEnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> Submit(Guid id, [FromBody] EnrollmentTransitionRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitEnrollmentResponse>.Ok(
                await _service.SubmitAsync(id, request, CurrentUserId(), cancellationToken), "Enrollment benefit diajukan.")));

        [HttpPost("{id:guid}/approve")]
        [AccessAction("Approve", "Approve Employee Benefit Enrollment", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("EmployeeBenefitEnrollment", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitEnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public Task<IActionResult> Approve(Guid id, [FromBody] EnrollmentTransitionRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitEnrollmentResponse>.Ok(
                await _service.ApproveAsync(id, request, CurrentUserId(), cancellationToken), "Enrollment benefit disetujui.")));

        [HttpPost("{id:guid}/activate")]
        [AccessAction("Activate", "Activate Employee Benefit Enrollment", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("EmployeeBenefitEnrollment", "Activate")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitEnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public Task<IActionResult> Activate(Guid id, [FromBody] EnrollmentTransitionRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitEnrollmentResponse>.Ok(
                await _service.ActivateAsync(id, request, CurrentUserId(), cancellationToken), "Enrollment benefit diaktifkan.")));

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Employee Benefit Enrollment", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("EmployeeBenefitEnrollment", "Cancel")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitEnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> Cancel(Guid id, [FromBody] EnrollmentCancelRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitEnrollmentResponse>.Ok(
                await _service.CancelAsync(id, request, CurrentUserId(), cancellationToken), "Enrollment benefit dibatalkan.")));

        private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
        {
            try { return await action(); }
            catch (Exception exception) when (EmployeeBenefitResults.IsHandled(exception)) { return EmployeeBenefitResults.ToResult(this, exception); }
        }

        private Guid CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>Pemetaan exception layanan benefit ke kode status (transaction-endpoint-standard.md 5.2).</summary>
    internal static class EmployeeBenefitResults
    {
        public static bool IsHandled(Exception exception) =>
            exception is KeyNotFoundException or EmployeeBenefitValidationException or EmployeeBenefitTransitionException
                or EmployeeBenefitConflictException or EmployeeBenefitForbiddenException;

        public static IActionResult ToResult(ControllerBase controller, Exception exception) => exception switch
        {
            KeyNotFoundException => controller.NotFound(ApiResponse<object>.Fail(404, exception.Message)),
            EmployeeBenefitTransitionException => controller.BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
            EmployeeBenefitForbiddenException => controller.StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, exception.Message)),
            EmployeeBenefitConflictException => controller.Conflict(ApiResponse<object>.Fail(409, exception.Message)),
            EmployeeBenefitValidationException => controller.UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
            _ => throw exception
        };
    }
}
