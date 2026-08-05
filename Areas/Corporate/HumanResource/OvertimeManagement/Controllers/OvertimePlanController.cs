using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/overtime-management/plans")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Plan",
        AreaName = "Corporate",
        ControllerName = "OvertimePlan",
        Description = "Overtime planning, validation, publication, and request generation",
        SortOrder = 1)]
    [Tags("Corporate / Human Resource / Overtime Management / Overtime Plan")]
    public class OvertimePlanController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.OvertimeManagement";
        private readonly OvertimePlanningService _planningService;
        private readonly OvertimePlanQueryService _queryService;
        private readonly LoggerService _loggerService;

        public OvertimePlanController(
            OvertimePlanningService planningService,
            OvertimePlanQueryService queryService,
            LoggerService loggerService)
        {
            _planningService = planningService;
            _queryService = queryService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Overtime Plan", Description = "Melihat metadata filter overtime plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePlan", "Read")]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<OvertimePlanFilterMetadataResponse>.Ok(_queryService.GetMetadata(), "Metadata overtime plan berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Overtime Plan", Description = "Melihat ringkasan overtime plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePlan", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] OvertimePlanQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<OvertimePlanSummaryResponse>.Ok(data, "Ringkasan overtime plan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Overtime Plan", Description = "Melihat daftar overtime plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePlan", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OvertimePlanQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<QuilvianSystemBackend.Responses.PagedResult<OvertimePlanListResponse>>.Ok(data, "Data overtime plan berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Overtime Plan", Description = "Melihat pilihan overtime plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePlan", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] string? planStatus,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetOptionsAsync(search, planStatus, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<QuilvianSystemBackend.Responses.PagedResult<OvertimePlanOptionResponse>>.Ok(data, "Pilihan overtime plan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Overtime Plan", Description = "Melihat detail overtime plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePlan", "Read")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetDetailAsync(id, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rencana lembur tidak ditemukan."))
                : Ok(ApiResponse<OvertimePlanResponse>.Ok(data, "Detail overtime plan berhasil diambil."));
        }

        [HttpGet("{id:guid}/details/{detailId:guid}")]
        [AccessAction("Read", "Read Overtime Plan Detail", Description = "Melihat detail employee pada overtime plan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePlan", "Read")]
        public async Task<IActionResult> GetPlanDetail(Guid id, Guid detailId, CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetPlanDetailAsync(id, detailId, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Detail rencana lembur tidak ditemukan."))
                : Ok(ApiResponse<OvertimePlanDetailResponse>.Ok(data, "Detail employee overtime plan berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Overtime Plan", Description = "Membuat overtime plan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OvertimePlan", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOvertimePlanRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.CreateAsync(request, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.Create", "Membuat overtime plan.", result.Data);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Overtime Plan", Description = "Mengubah overtime plan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimePlan", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateOvertimePlanRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.UpdateAsync(id, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.Update", "Mengubah overtime plan.", result.Data);
            return ToActionResult(result);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Overtime Plan Status", Description = "Mengubah status aktif overtime plan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimePlan", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateOvertimePlanStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.UpdateStatusAsync(id, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.UpdateStatus", "Mengubah status aktif overtime plan.", result.Data);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Overtime Plan", Description = "Menghapus overtime plan secara soft delete", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("OvertimePlan", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.DeleteAsync(id, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.Delete", "Menghapus overtime plan secara soft delete.", result.Data);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/details")]
        [AccessAction("CreateDetail", "Create Overtime Plan Detail", Description = "Menambahkan employee ke overtime plan", AccessType = AccessTypes.Create, SortOrder = 5)]
        [AccessPermission("OvertimePlan", "CreateDetail")]
        public async Task<IActionResult> AddDetail(
            Guid id,
            [FromBody] CreateOvertimePlanDetailRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.AddDetailAsync(id, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.AddDetail", "Menambahkan detail employee ke overtime plan.", result.Data);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}/details/{detailId:guid}")]
        [AccessAction("UpdateDetail", "Update Overtime Plan Detail", Description = "Mengubah detail employee overtime plan", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("OvertimePlan", "UpdateDetail")]
        public async Task<IActionResult> UpdateDetail(
            Guid id,
            Guid detailId,
            [FromBody] UpdateOvertimePlanDetailRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.UpdateDetailAsync(id, detailId, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.UpdateDetail", "Mengubah detail employee overtime plan.", result.Data);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}/details/{detailId:guid}")]
        [AccessAction("DeleteDetail", "Delete Overtime Plan Detail", Description = "Menghapus detail employee overtime plan", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("OvertimePlan", "DeleteDetail")]
        public async Task<IActionResult> DeleteDetail(Guid id, Guid detailId, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.DeleteDetailAsync(id, detailId, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.DeleteDetail", "Menghapus detail employee overtime plan secara soft delete.", result.Data);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/details/validate-preview")]
        [AccessAction("Validate", "Preview Overtime Plan Detail", Description = "Melakukan preview validasi detail overtime plan", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("OvertimePlan", "Validate")]
        public async Task<IActionResult> PreviewDetail(
            Guid id,
            [FromBody] OvertimePlanDetailPreviewRequest request,
            CancellationToken cancellationToken = default) =>
            ToActionResult(await _planningService.PreviewDetailAsync(id, request, cancellationToken));

        [HttpPost("{id:guid}/validate")]
        [AccessAction("Validate", "Validate Overtime Plan", Description = "Memvalidasi overtime plan dan seluruh detail", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("OvertimePlan", "Validate")]
        public async Task<IActionResult> Validate(Guid id, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.ValidateAsync(id, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.Validate", "Memvalidasi overtime plan dan seluruh detail.", result.Data);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/publish")]
        [AccessAction("Publish", "Publish Overtime Plan", Description = "Mempublikasikan overtime plan yang valid", AccessType = AccessTypes.Update, SortOrder = 10)]
        [AccessPermission("OvertimePlan", "Publish")]
        public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.PublishAsync(id, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.Publish", "Mempublikasikan overtime plan.", result.Data);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/generate-requests")]
        [AccessAction("GenerateRequest", "Generate Overtime Request", Description = "Membuat overtime request Draft dari detail plan secara idempotent", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission("OvertimePlan", "GenerateRequest")]
        public async Task<IActionResult> GenerateRequests(
            Guid id,
            [FromBody] GenerateOvertimeRequestsRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.GenerateRequestsAsync(id, request ?? new GenerateOvertimeRequestsRequest(), actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.GenerateRequests", "Membuat overtime request Draft dari overtime plan.", result.Data);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Overtime Plan", Description = "Membatalkan overtime plan dan request Draft terkait", AccessType = AccessTypes.Update, SortOrder = 12)]
        [AccessPermission("OvertimePlan", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelOvertimePlanRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _planningService.CancelAsync(id, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
                await _loggerService.InfoAsync(LogCategory, "OvertimePlan.Cancel", "Membatalkan overtime plan.", result.Data);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(OvertimePlanningServiceResult<T> result) =>
            result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

        private IActionResult UnauthorizedResult() =>
            Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
