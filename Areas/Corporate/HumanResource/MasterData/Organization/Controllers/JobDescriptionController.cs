using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/job-descriptions")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Job Description",
        AreaName = "Corporate",
        ControllerName = "JobDescription",
        Description = "Corporate human resource master data job description",
        SortOrder = 22)]
    [Tags("Corporate / Human Resource / Master Data / Job Description")]
    public class JobDescriptionController : ControllerBase
    {
        private readonly IJobDescriptionService _jobDescriptionService;

        public JobDescriptionController(IJobDescriptionService jobDescriptionService)
        {
            _jobDescriptionService = jobDescriptionService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Job Description Filter Metadata", Description = "Melihat metadata filter uraian tugas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobDescription", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = await _jobDescriptionService.GetFilterMetadataAsync();
            return Ok(ApiResponse<JobDescriptionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter uraian tugas berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Job Description Summary", Description = "Melihat ringkasan uraian tugas", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("JobDescription", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _jobDescriptionService.GetSummaryAsync();
            return Ok(ApiResponse<JobDescriptionSummaryResponse>.Ok(
                result,
                "Ringkasan uraian tugas berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Job Description List", Description = "Melihat daftar uraian tugas", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("JobDescription", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] Guid? positionId,
            [FromQuery] Guid? departmentId,
            [FromQuery] string? status,
            [FromQuery] DateTime? asOfDate,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var result = await _jobDescriptionService.GetDataAsync(
                positionId,
                departmentId,
                status,
                asOfDate,
                search,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResult<JobDescriptionResponse>>.Ok(
                result,
                "Data uraian tugas berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Job Description Detail", Description = "Melihat detail uraian tugas", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("JobDescription", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _jobDescriptionService.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Uraian tugas tidak ditemukan."));
            }

            return Ok(ApiResponse<JobDescriptionDetailResponse>.Ok(
                result,
                "Detail uraian tugas berhasil diambil."));
        }

        [HttpGet("position/{positionId:guid}/effective")]
        [AccessAction("Read", "Read Position Effective Job Description", Description = "Melihat uraian tugas efektif per jabatan", AccessType = AccessTypes.Read, SortOrder = 5)]
        [AccessPermission("JobDescription", "Read")]
        public async Task<IActionResult> GetEffectiveByPosition(Guid positionId, [FromQuery] DateTime? asOfDate)
        {
            var result = await _jobDescriptionService.GetEffectiveByPositionAsync(positionId, asOfDate);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Belum ada uraian tugas aktif yang berlaku untuk jabatan ini."));
            }

            return Ok(ApiResponse<JobDescriptionDetailResponse>.Ok(
                result,
                "Uraian tugas efektif berhasil diambil."));
        }

        [HttpGet("position/{positionId:guid}/history")]
        [AccessAction("Read", "Read Position Job Description History", Description = "Melihat riwayat versi uraian tugas per jabatan", AccessType = AccessTypes.Read, SortOrder = 6)]
        [AccessPermission("JobDescription", "Read")]
        public async Task<IActionResult> GetHistoryByPosition(Guid positionId)
        {
            var result = await _jobDescriptionService.GetHistoryByPositionAsync(positionId);

            return Ok(ApiResponse<List<JobDescriptionHistoryResponse>>.Ok(
                result,
                "Riwayat versi uraian tugas berhasil diambil."));
        }

        [HttpPost("draft")]
        [AccessAction("Create", "Create Job Description Draft", Description = "Membuat draft uraian tugas baru", AccessType = AccessTypes.Create, SortOrder = 7)]
        [AccessPermission("JobDescription", "Create")]
        public async Task<IActionResult> CreateDraft([FromBody] CreateJobDescriptionDraftRequest request)
        {
            var actorId = CurrentUserId();
            var (success, error, statusCode, result) = await _jobDescriptionService.CreateDraftAsync(request, actorId);

            if (!success)
            {
                return StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, error ?? "Gagal membuat draft uraian tugas."));
            }

            return StatusCode(statusCode, ApiResponse<JobDescriptionDetailResponse>.Ok(
                result!,
                "Draft uraian tugas berhasil dibuat."));
        }

        [HttpPut("{id:guid}/draft")]
        [AccessAction("Update", "Update Job Description Draft", Description = "Mengubah draft uraian tugas", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("JobDescription", "Update")]
        public async Task<IActionResult> UpdateDraft(Guid id, [FromBody] UpdateJobDescriptionDraftRequest request)
        {
            var actorId = CurrentUserId();
            var (success, error, statusCode, result) = await _jobDescriptionService.UpdateDraftAsync(id, request, actorId);

            if (!success)
            {
                return StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, error ?? "Gagal memperbarui draft uraian tugas."));
            }

            return StatusCode(statusCode, ApiResponse<JobDescriptionDetailResponse>.Ok(
                result!,
                "Draft uraian tugas berhasil diperbarui."));
        }

        [HttpPost("{id:guid}/activate")]
        [AccessAction("Update", "Activate Job Description Version", Description = "Mengaktifkan versi uraian tugas", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("JobDescription", "Update")]
        public async Task<IActionResult> ActivateVersion(Guid id, [FromBody] ActivateJobDescriptionRequest? request)
        {
            var actorId = CurrentUserId();
            var (success, error, statusCode, result) = await _jobDescriptionService.ActivateVersionAsync(id, request, actorId);

            if (!success)
            {
                return StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, error ?? "Gagal mengaktifkan versi uraian tugas."));
            }

            return StatusCode(statusCode, ApiResponse<JobDescriptionDetailResponse>.Ok(
                result!,
                "Versi uraian tugas berhasil diaktifkan."));
        }

        [HttpPost("{id:guid}/deactivate")]
        [AccessAction("Update", "Deactivate Job Description", Description = "Menonaktifkan uraian tugas", AccessType = AccessTypes.Update, SortOrder = 10)]
        [AccessPermission("JobDescription", "Update")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var actorId = CurrentUserId();
            var (success, error, statusCode) = await _jobDescriptionService.DeactivateAsync(id, actorId);

            if (!success)
            {
                return StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, error ?? "Gagal menonaktifkan uraian tugas."));
            }

            return StatusCode(statusCode, ApiResponse<object>.Ok(
                null,
                "Uraian tugas berhasil dinonaktifkan."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Job Description Draft", Description = "Menghapus draft uraian tugas", AccessType = AccessTypes.Delete, SortOrder = 11)]
        [AccessPermission("JobDescription", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var actorId = CurrentUserId();
            var (success, error, statusCode) = await _jobDescriptionService.DeleteAsync(id, actorId);

            if (!success)
            {
                return StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, error ?? "Gagal menghapus uraian tugas."));
            }

            return StatusCode(statusCode, ApiResponse<object>.Ok(
                null,
                "Uraian tugas berhasil dihapus."));
        }

        private Guid CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
