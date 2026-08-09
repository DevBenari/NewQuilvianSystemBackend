using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/leave-adjustment-reasons")]
    [AccessController(moduleCode: "HUMAN_RESOURCE_MASTER_DATA", moduleName: "Human Resource Master Data", displayName: "Leave Adjustment Reason", AreaName = "Corporate", ControllerName = "LeaveAdjustmentReason", Description = "Corporate human resource master data leave adjustment reason", SortOrder = 34)]
    [Tags("Corporate / Human Resource / Master Data / Leave and Overtime / Leave Adjustment Reason")]
    public class LeaveAdjustmentReasonController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private readonly LeaveAdjustmentReasonService _service;
        private readonly LoggerService _loggerService;

        public LeaveAdjustmentReasonController(LeaveAdjustmentReasonService service, LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Leave Adjustment Reason", Description = "Melihat metadata filter leave adjustment reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustmentReason", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = _service.GetFilterMetadata();
            await _loggerService.InfoAsync(LogCategory, "LeaveAdjustmentReason.GetFilterMetadata", "Mengambil metadata filter leave adjustment reason.", result);
            return Ok(ApiResponse<LeaveAdjustmentReasonFilterMetadataResponse>.Ok(result, "Metadata filter leave adjustment reason berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Leave Adjustment Reason", Description = "Melihat ringkasan leave adjustment reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustmentReason", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _service.GetSummaryAsync();
            return Ok(ApiResponse<LeaveAdjustmentReasonSummaryResponse>.Ok(result, "Ringkasan leave adjustment reason berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Leave Adjustment Reason", Description = "Melihat data leave adjustment reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustmentReason", "Read")]
        public async Task<IActionResult> GetData(Guid? leaveTypeId, string? reasonCategory, string? allowedDirection, bool? allowOpeningBalance, bool? requiresApproval, bool? isActive, string? search, string? sortBy = "sortOrder", string? sortDirection = "asc", int pageNumber = 1, int pageSize = 25)
        {
            var result = await _service.GetDataAsync(leaveTypeId, reasonCategory, allowedDirection, allowOpeningBalance, requiresApproval, isActive, search, sortBy, sortDirection, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<LeaveAdjustmentReasonResponse>>.Ok(result, "Data leave adjustment reason berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Leave Adjustment Reason", Description = "Melihat pilihan leave adjustment reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustmentReason", "Read")]
        public async Task<IActionResult> GetOptions(Guid? leaveTypeId, string? reasonCategory, string? allowedDirection, bool onlyActive = true, string? search = null, int pageNumber = 1, int pageSize = 25)
        {
            var result = await _service.GetOptionsAsync(leaveTypeId, reasonCategory, allowedDirection, onlyActive, search, pageNumber, pageSize);
            return Ok(ApiResponse<LeaveAdjustmentReasonOptionPagedResponse>.Ok(result, "Pilihan leave adjustment reason berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Leave Adjustment Reason", Description = "Melihat detail leave adjustment reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustmentReason", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(ApiResponse<object>.Fail(404, "Leave adjustment reason tidak ditemukan."));
            return Ok(ApiResponse<LeaveAdjustmentReasonDetailResponse>.Ok(result, "Detail leave adjustment reason berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Leave Adjustment Reason", Description = "Membuat leave adjustment reason", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeaveAdjustmentReason", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeaveAdjustmentReasonRequest request)
        {
            var result = await _service.CreateAsync(request, GetCurrentUserId());
            if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Fail(400, result.ErrorMessage!));
            await _loggerService.InfoAsync(LogCategory, "LeaveAdjustmentReason.Create", "Membuat data leave adjustment reason.", result.Data);
            return Ok(ApiResponse<LeaveAdjustmentReasonCreateResponse>.Ok(result.Data!, "Leave adjustment reason berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Leave Adjustment Reason", Description = "Mengubah leave adjustment reason", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveAdjustmentReason", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveAdjustmentReasonRequest request)
        {
            var result = await _service.UpdateAsync(id, request, GetCurrentUserId());
            if (result.IsNotFound) return NotFound(ApiResponse<object>.Fail(404, "Leave adjustment reason tidak ditemukan."));
            if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Fail(400, result.ErrorMessage!));
            await _loggerService.InfoAsync(LogCategory, "LeaveAdjustmentReason.Update", "Mengubah data leave adjustment reason.", result.Data);
            return Ok(ApiResponse<LeaveAdjustmentReasonUpdateResponse>.Ok(result.Data!, "Leave adjustment reason berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Leave Adjustment Reason Status", Description = "Mengubah status leave adjustment reason", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveAdjustmentReason", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLeaveAdjustmentReasonStatusRequest request)
        {
            if (!await _service.UpdateStatusAsync(id, request, GetCurrentUserId()))
                return NotFound(ApiResponse<object>.Fail(404, "Leave adjustment reason tidak ditemukan."));
            return Ok(ApiResponse<object>.Ok(null, "Status leave adjustment reason berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Leave Adjustment Reason", Description = "Menghapus leave adjustment reason", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LeaveAdjustmentReason", "Delete")]
        public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteLeaveAdjustmentReasonRequest? request = null)
        {
            var result = await _service.DeleteAsync(id, request, GetCurrentUserId());
            if (result == null) return NotFound(ApiResponse<object>.Fail(404, "Leave adjustment reason tidak ditemukan."));
            await _loggerService.InfoAsync(LogCategory, "LeaveAdjustmentReason.Delete", "Menghapus data leave adjustment reason.", result);
            return Ok(ApiResponse<LeaveAdjustmentReasonDeleteResponse>.Ok(result, "Leave adjustment reason berhasil dihapus."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
