using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/employee-self-service/leave/requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Request Self Service",
        AreaName = "Corporate",
        ControllerName = "LeaveRequestSelfService",
        Description = "Employee self-service leave request, calculation, attachment, reservation, and workflow submission",
        SortOrder = 6)]
    [Tags("Corporate / Human Resource / Employee Self Service / Leave Request")]
    public class LeaveRequestController : ControllerBase
    {
        private readonly LeaveRequestService _service;
        private readonly LeaveRequestCalculationService _calculationService;
        private readonly LeaveRequestAttachmentService _attachmentService;

        public LeaveRequestController(
            LeaveRequestService service,
            LeaveRequestCalculationService calculationService,
            LeaveRequestAttachmentService attachmentService)
        {
            _service = service;
            _calculationService = calculationService;
            _attachmentService = attachmentService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Leave Request", Description = "Melihat metadata ESS leave request", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequestSelfService", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<LeaveRequestFilterMetadataResponse>.Ok(
                _service.GetMetadata(),
                "Metadata pengajuan cuti berhasil diambil."));
        }

        [HttpGet("balances/options")]
        [AccessAction("Read", "Read Leave Balance Options", Description = "Melihat pilihan jenis cuti dan saldo employee", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequestSelfService", "Read")]
        public async Task<IActionResult> GetBalanceOptions(
            [FromQuery] DateOnly? asOfDate,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetBalanceOptionsAsync(
                GetCurrentUserId(),
                asOfDate,
                cancellationToken));
        }

        [HttpGet("reasons/options")]
        [AccessAction("Read", "Read Leave Request Reasons", Description = "Melihat pilihan alasan pengajuan cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequestSelfService", "Read")]
        public async Task<IActionResult> GetReasonOptions(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetReasonOptionsAsync(search, cancellationToken));
        }

        [HttpPost("calculate")]
        [AccessAction("Read", "Calculate Leave Request", Description = "Menghitung hari cuti, jadwal, konflik, dan saldo", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequestSelfService", "Read")]
        public async Task<IActionResult> Calculate(
            [FromBody] LeaveRequestCalculationRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _calculationService.CalculateAsync(
                GetCurrentUserId(),
                request,
                cancellationToken));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Leave Request Summary", Description = "Melihat ringkasan pengajuan cuti employee", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequestSelfService", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] LeaveRequestQueryRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetSummaryAsync(
                GetCurrentUserId(),
                request,
                cancellationToken));
        }

        [HttpGet]
        [AccessAction("Read", "Read Leave Request", Description = "Melihat daftar pengajuan cuti milik employee", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequestSelfService", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] LeaveRequestQueryRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetPagedAsync(
                GetCurrentUserId(),
                request,
                cancellationToken));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Leave Request Detail", Description = "Melihat detail pengajuan cuti milik employee", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequestSelfService", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetByIdAsync(
                id,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpGet("{id:guid}/workflow")]
        [AccessAction("Read", "Read Leave Request Workflow", Description = "Melihat workflow pengajuan cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequestSelfService", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetWorkflowAsync(
                id,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost]
        [AccessAction("Create", "Create Leave Request", Description = "Membuat draft pengajuan cuti", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeaveRequestSelfService", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLeaveRequestRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.CreateAsync(
                GetCurrentUserId(),
                request,
                cancellationToken));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Leave Request", Description = "Mengubah draft atau revisi pengajuan cuti", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveRequestSelfService", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLeaveRequestRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.UpdateAsync(
                id,
                GetCurrentUserId(),
                request,
                cancellationToken));
        }

        [HttpPost("{id:guid}/prepare-workflow")]
        [AccessAction("Update", "Prepare Leave Request Workflow", Description = "Menyiapkan workflow LEAVE_REQUEST", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveRequestSelfService", "Update")]
        public async Task<IActionResult> PrepareWorkflow(
            Guid id,
            [FromBody] PrepareLeaveRequestWorkflowRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.PrepareWorkflowAsync(
                id,
                GetCurrentUserId(),
                request,
                cancellationToken));
        }

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit Leave Request", Description = "Submit pengajuan cuti dan reservasi saldo", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LeaveRequestSelfService", "Submit")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] SubmitLeaveRequestRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.SubmitAsync(
                id,
                GetCurrentUserId(),
                request,
                cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Leave Request", Description = "Membatalkan atau menarik pengajuan cuti", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LeaveRequestSelfService", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLeaveRequestRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.CancelAsync(
                id,
                GetCurrentUserId(),
                request,
                cancellationToken));
        }

        [HttpPost("{id:guid}/attachments")]
        [Consumes("multipart/form-data")]
        [AccessAction("Update", "Upload Leave Request Attachment", Description = "Mengunggah dokumen pendukung pengajuan cuti", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("LeaveRequestSelfService", "Update")]
        public async Task<IActionResult> UploadAttachment(
            Guid id,
            [FromForm] IFormFile file,
            [FromForm] string? attachmentType,
            [FromForm] bool isRequiredDocument,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _attachmentService.UploadAsync(
                id,
                GetCurrentUserId(),
                file,
                attachmentType,
                isRequiredDocument,
                cancellationToken));
        }

        [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
        [AccessAction("Read", "Download Leave Request Attachment", Description = "Mengunduh attachment pengajuan cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequestSelfService", "Read")]
        public async Task<IActionResult> DownloadAttachment(
            Guid id,
            Guid attachmentId,
            CancellationToken cancellationToken)
        {
            var result = await _attachmentService.GetDownloadAsync(
                id,
                attachmentId,
                GetCurrentUserId(),
                cancellationToken);

            if (!result.Success || result.Data == null)
                return ToActionResult(result);

            return PhysicalFile(
                result.Data.PhysicalPath,
                result.Data.ContentType,
                result.Data.DownloadFileName);
        }

        [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
        [AccessAction("Update", "Delete Leave Request Attachment", Description = "Menghapus attachment draft pengajuan cuti", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("LeaveRequestSelfService", "Update")]
        public async Task<IActionResult> DeleteAttachment(
            Guid id,
            Guid attachmentId,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _attachmentService.DeleteAsync(
                id,
                attachmentId,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Leave Request", Description = "Menghapus draft pengajuan cuti", AccessType = AccessTypes.Delete, SortOrder = 8)]
        [AccessPermission("LeaveRequestSelfService", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.DeleteAsync(
                id,
                GetCurrentUserId(),
                cancellationToken));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private IActionResult ToActionResult<T>(LeaveRequestServiceResult<T> result)
        {
            if (result.Success)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<T>.Ok(result.Data!, result.Message));
            }

            return StatusCode(
                result.StatusCode,
                ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
