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
    /// Tanggungan benefit — sub-proses ter-scope ke enrollment induknya
    /// (transaction-endpoint-standard.md 2.3). BE-FIN-103, FIN-API-1.10 R.2.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{enrollmentId:guid}/dependents")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_BENEFIT_MANAGEMENT",
        moduleName: "Human Resource Benefit Management",
        displayName: "Employee Benefit Dependent",
        AreaName = "Corporate",
        ControllerName = "EmployeeBenefitDependent",
        Description = "Pendaftaran dan verifikasi tanggungan benefit pegawai",
        SortOrder = 20)]
    [Tags("Corporate / Human Resource / Benefit Management / Employee Benefit Dependent")]
    public sealed class EmployeeBenefitDependentController : ControllerBase
    {
        private readonly EmployeeBenefitDependentService _service;

        public EmployeeBenefitDependentController(EmployeeBenefitDependentService service) => _service = service;

        [HttpGet]
        [AccessAction("Read", "Read Employee Benefit Dependent", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitDependent", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<EmployeeBenefitDependentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public Task<IActionResult> GetList(Guid enrollmentId, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<List<EmployeeBenefitDependentResponse>>.Ok(
                await _service.GetListAsync(enrollmentId, cancellationToken), "Daftar tanggungan berhasil diambil.")));

        [HttpPost]
        [AccessAction("Create", "Create Employee Benefit Dependent", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmployeeBenefitDependent", "Create")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitDependentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public Task<IActionResult> Create(Guid enrollmentId, [FromBody] CreateEmployeeBenefitDependentRequest request, CancellationToken cancellationToken) =>
            Run(async () => StatusCode(StatusCodes.Status201Created, ApiResponse<EmployeeBenefitDependentResponse>.Ok(
                await _service.CreateAsync(enrollmentId, request, CurrentUserId(), cancellationToken), "Tanggungan berhasil didaftarkan.")));

        [HttpPost("{id:guid}/request-verification")]
        [AccessAction("Request", "Request Employee Benefit Dependent Verification", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmployeeBenefitDependent", "Request")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitDependentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> RequestVerification(Guid enrollmentId, Guid id, [FromBody] DependentTransitionRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitDependentResponse>.Ok(
                await _service.RequestVerificationAsync(enrollmentId, id, request, CurrentUserId(), cancellationToken), "Tanggungan diajukan untuk verifikasi.")));

        [HttpPost("{id:guid}/verify")]
        [AccessAction("Verify", "Verify Employee Benefit Dependent", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmployeeBenefitDependent", "Verify")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitDependentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public Task<IActionResult> Verify(Guid enrollmentId, Guid id, [FromBody] VerifyEmployeeBenefitDependentRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitDependentResponse>.Ok(
                await _service.VerifyAsync(enrollmentId, id, request, CurrentUserId(), cancellationToken), "Tanggungan terverifikasi.")));

        [HttpPost("{id:guid}/reject")]
        [AccessAction("Reject", "Reject Employee Benefit Dependent", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("EmployeeBenefitDependent", "Reject")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitDependentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> Reject(Guid enrollmentId, Guid id, [FromBody] RejectEmployeeBenefitDependentRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitDependentResponse>.Ok(
                await _service.RejectAsync(enrollmentId, id, request, CurrentUserId(), cancellationToken), "Tanggungan ditolak.")));

        [HttpPost("{id:guid}/end")]
        [AccessAction("End", "End Employee Benefit Dependent", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("EmployeeBenefitDependent", "End")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitDependentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> End(Guid enrollmentId, Guid id, [FromBody] EndEmployeeBenefitDependentRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<EmployeeBenefitDependentResponse>.Ok(
                await _service.EndAsync(enrollmentId, id, request, CurrentUserId(), cancellationToken), "Tanggungan diakhiri.")));

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
}
