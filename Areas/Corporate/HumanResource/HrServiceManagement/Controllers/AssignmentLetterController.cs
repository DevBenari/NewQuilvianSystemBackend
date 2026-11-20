using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Controllers
{
    /// <summary>
    /// Surat Tugas multi-pegawai berjenjang (BE-HRD-382/383/384). Transaksi ber-lifecycle: perpindahan status lewat
    /// <c>POST /{id}/&lt;aksi&gt;</c>; tidak ada DELETE (batal lewat <c>cancel</c>).
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/human-resource/assignment-letters")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SERVICE_MANAGEMENT",
        moduleName: "Human Resource Service Management",
        displayName: "Surat Tugas",
        AreaName = "Corporate",
        ControllerName = "AssignmentLetter",
        Description = "Pengajuan, verifikasi HRD, pengesahan, penerbitan, pelaksanaan, dan pembatalan Surat Tugas multi-pegawai",
        SortOrder = 11)]
    [Tags("HR Services - Assignment Letters (Surat Tugas)")]
    public class AssignmentLetterController : ControllerBase
    {
        private readonly AssignmentLetterService _service;

        public AssignmentLetterController(AssignmentLetterService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Surat Tugas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AssignmentLetter", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AssignmentLetterListItemResponse>>), StatusCodes.Status200OK)]
        public Task<IActionResult> GetPaged([FromQuery] AssignmentLetterQuery query, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<PagedResult<AssignmentLetterListItemResponse>>.Ok(
                await _service.GetPagedAsync(query, User, cancellationToken), "Daftar Surat Tugas berhasil diambil.")));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Surat Tugas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AssignmentLetter", "Read")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.GetByIdAsync(id, User, cancellationToken), "Detail Surat Tugas berhasil diambil.")));

        [HttpGet("{id:guid}/document")]
        [AccessAction("Read", "Melihat Surat Tugas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AssignmentLetter", "Read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken) =>
            Run(async () =>
            {
                var (content, contentType, fileName) = await _service.GetDocumentAsync(id, User, cancellationToken);
                return File(content, contentType, fileName);
            });

        [HttpPost]
        [AccessAction("Create", "Menyusun Surat Tugas", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AssignmentLetter", "Create")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public Task<IActionResult> Create([FromBody] SaveAssignmentLetterRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.CreateAsync(request, User, cancellationToken), "Draf Surat Tugas tersimpan.")));

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Mengubah Draf Surat Tugas", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AssignmentLetter", "Update")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public Task<IActionResult> Update(Guid id, [FromBody] SaveAssignmentLetterRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.UpdateAsync(id, request, User, cancellationToken), "Draf Surat Tugas diperbarui.")));

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Mengajukan Surat Tugas ke HRD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AssignmentLetter", "Submit")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.SubmitAsync(id, User, cancellationToken), "Surat Tugas diajukan ke HRD.")));

        [HttpPost("{id:guid}/verify")]
        [AccessAction("Verify", "Memverifikasi Surat Tugas (HRD)", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("AssignmentLetter", "Verify")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public Task<IActionResult> Verify(Guid id, [FromBody] VerifyAssignmentLetterRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.VerifyAsync(id, request, User, cancellationToken), "Hasil verifikasi tersimpan.")));

        [HttpPost("{id:guid}/endorse")]
        [AccessAction("Endorse", "Mengesahkan Surat Tugas (Pejabat)", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("AssignmentLetter", "Endorse")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public Task<IActionResult> Endorse(Guid id, [FromBody] EndorseAssignmentLetterRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.EndorseAsync(id, request, User, cancellationToken), "Keputusan pengesahan tersimpan.")));

        [HttpPost("{id:guid}/issue")]
        [AccessAction("Issue", "Menerbitkan Surat Tugas (HRD)", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("AssignmentLetter", "Issue")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public Task<IActionResult> Issue(Guid id, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.IssueAsync(id, User, cancellationToken), "Surat Tugas diterbitkan.")));

        [HttpPost("{id:guid}/start-execution")]
        [AccessAction("Report", "Mencatat Pelaksanaan Surat Tugas", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("AssignmentLetter", "Report")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public Task<IActionResult> StartExecution(Guid id, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.StartExecutionAsync(id, User, cancellationToken), "Pelaksanaan Surat Tugas dimulai.")));

        [HttpPost("{id:guid}/complete")]
        [AccessAction("Report", "Mencatat Pelaksanaan Surat Tugas", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("AssignmentLetter", "Report")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public Task<IActionResult> Complete(Guid id, [FromBody] CompleteAssignmentLetterRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.CompleteAsync(id, request, User, cancellationToken), "Surat Tugas selesai dan laporan tersimpan.")));

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Membatalkan Surat Tugas", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("AssignmentLetter", "Cancel")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentLetterDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public Task<IActionResult> Cancel(Guid id, [FromBody] CancelAssignmentLetterRequest request, CancellationToken cancellationToken) =>
            Run(async () => Ok(ApiResponse<AssignmentLetterDetailResponse>.Ok(await _service.CancelAsync(id, request, User, cancellationToken), "Surat Tugas dibatalkan.")));

        /// <summary>Pemetaan galat domain: 404 tidak ditemukan/di luar cakupan, 403 aktor tidak berwenang, 409 status, 422 validasi.</summary>
        private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, ex.Message));
            }
            catch (HrServiceConflictException ex)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }
    }
}
