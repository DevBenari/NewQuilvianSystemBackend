using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeSelfService.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeSelfService.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeSelfService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/overtime")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Overtime",
        AreaName = "SelfServices",
        ControllerName = "MyOvertime",
        Description = "Employee self service overtime request",
        SortOrder = 8)]
    [Tags("Self Services / Human Resource / My Overtime")]
    public class OvertimeSelfServiceController : ControllerBase
    {
        private const string LogCategory = "SelfServices.HumanResource.Overtime";
        private readonly OvertimeSelfServiceQueryService _queryService;
        private readonly OvertimeSelfServiceService _service;
        private readonly LoggerService _loggerService;

        public OvertimeSelfServiceController(
            OvertimeSelfServiceQueryService queryService,
            OvertimeSelfServiceService service,
            LoggerService loggerService)
        {
            _queryService = queryService;
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read My Overtime", Description = "Melihat metadata filter pengajuan lembur milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        public IActionResult GetMetadata() =>
            Ok(ApiResponse<MyOvertimeMetadataResponse>.Ok(
                _queryService.GetMetadata(),
                "Metadata pengajuan lembur berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read My Overtime", Description = "Melihat ringkasan pengajuan lembur milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetSummary(
            [FromQuery] MyOvertimeQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            return ToActionResult(await _queryService.GetSummaryAsync(actor, request, cancellationToken));
        }

        [HttpGet]
        [AccessAction("Read", "Read My Overtime", Description = "Melihat daftar pengajuan lembur milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] MyOvertimeQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            return ToActionResult(await _queryService.GetPagedAsync(actor, request, cancellationToken));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read My Overtime", Description = "Melihat detail pengajuan lembur milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetDetail(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            return ToActionResult(await _queryService.GetDetailAsync(actor, id, cancellationToken));
        }

        [HttpPost("validate-preview")]
        [AccessAction("Validate", "Validate My Overtime", Description = "Melakukan preview validasi pengajuan lembur", AccessType = AccessTypes.Read, SortOrder = 2)]
        public async Task<IActionResult> Preview(
            [FromBody] PreviewMyOvertimeRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            return ToActionResult(await _service.PreviewAsync(actor, request, cancellationToken));
        }

        [HttpPost]
        [AccessAction("Create", "Create My Overtime", Description = "Membuat Draft pengajuan lembur milik sendiri", AccessType = AccessTypes.Create, SortOrder = 3)]
        public async Task<IActionResult> CreateDraft(
            [FromBody] CreateMyOvertimeRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.CreateDraftAsync(actor, request, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "MyOvertime.CreateDraft", "Membuat Draft pengajuan lembur employee self service.", result.Data);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update My Overtime", Description = "Mengubah Draft atau NeedRevision pengajuan lembur milik sendiri", AccessType = AccessTypes.Update, SortOrder = 4)]
        public async Task<IActionResult> UpdateDraft(
            Guid id,
            [FromBody] UpdateMyOvertimeRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.UpdateDraftAsync(actor, id, request, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "MyOvertime.UpdateDraft", "Mengubah Draft pengajuan lembur employee self service.", result.Data);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit My Overtime", Description = "Membuat atau submit ulang Generic Workflow OVERTIME_REQUEST", AccessType = AccessTypes.Update, SortOrder = 5)]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] SubmitMyOvertimeRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.SubmitAsync(actor, id, request, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "MyOvertime.Submit", "Submit pengajuan lembur employee self service ke Generic Workflow.", result.Data);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel My Overtime", Description = "Membatalkan atau menarik pengajuan lembur melalui Generic Workflow", AccessType = AccessTypes.Update, SortOrder = 6)]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelMyOvertimeRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.CancelAsync(actor, id, request, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "MyOvertime.Cancel", "Membatalkan pengajuan lembur employee self service.", result.Data);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete My Overtime", Description = "Menghapus Draft pengajuan lembur secara soft delete", AccessType = AccessTypes.Delete, SortOrder = 7)]
        public async Task<IActionResult> DeleteDraft(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.DeleteDraftAsync(actor, id, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "MyOvertime.DeleteDraft", "Menghapus Draft pengajuan lembur employee self service.", result.Data);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(OvertimeSelfServiceServiceResult<T> result) =>
            result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

        private IActionResult UnauthorizedResult() =>
            Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid."));

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
