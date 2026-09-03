using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Pengajuan dan persetujuan perubahan batas kritis.
    ///
    /// Batas kritis menentukan pada angka berapa seorang pasien dianggap terancam, dan karena itu
    /// perubahannya tidak pernah langsung berlaku. Ia diajukan kepala instalasi, lalu diputuskan
    /// pihak berwenang — dan <b>tidak pernah oleh orang yang sama</b> (<c>VAL-33</c>).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-value-bounds/{valueBoundId:guid}/critical-change-requests")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Critical Bound Approval",
        AreaName = "HealthServices",
        ControllerName = "LabCriticalBound",
        Description = "Pengajuan dan persetujuan perubahan batas kritis laboratorium",
        SortOrder = 4
    )]
    [Tags("Health Services / Laboratory Management / Lab Critical Bound Approval")]
    public class LabCriticalBoundApprovalController : ControllerBase
    {
        private readonly LabCriticalBoundApprovalService _service;

        public LabCriticalBoundApprovalController(LabCriticalBoundApprovalService service)
        {
            _service = service;
        }

        /// <summary>
        /// Daftar pengajuan perubahan batas kritis untuk satu batas nilai, terbaru lebih dulu.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<LabBoundChangeRequestResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Critical Bound Approval", Description = "Melihat daftar pengajuan perubahan batas kritis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabCriticalBound", "Read")]
        public async Task<IActionResult> GetList(
            Guid valueBoundId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetListAsync(valueBoundId, cancellationToken);

                return Ok(ApiResponse<List<LabBoundChangeRequestResponse>>.Ok(
                    result, "Daftar pengajuan perubahan batas kritis berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        /// <summary>
        /// Mengajukan perubahan batas kritis. Batas yang berlaku tidak berubah sama sekali
        /// sampai pengajuan ini disetujui.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabBoundChangeRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Submit Lab Critical Bound Change", Description = "Mengajukan perubahan batas kritis", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("LabValueBound", "Update")]
        public Task<IActionResult> Submit(
            Guid valueBoundId,
            [FromBody] SubmitCriticalBoundChangeRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.SubmitAsync(valueBoundId, request, cancellationToken),
                "Pengajuan perubahan batas kritis berhasil dibuat.");

        /// <summary>
        /// Menyetujui pengajuan; batas kritis yang baru mulai berlaku dan satu baris riwayat
        /// diterbitkan beserta penyetujunya.
        ///
        /// Ditolak <c>403</c> bila yang memutuskan adalah pengajunya sendiri (<c>VAL-33</c>).
        /// </summary>
        [HttpPost("{requestId:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<LabBoundChangeRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Approve", "Decide Lab Critical Bound Change", Description = "Menyetujui perubahan batas kritis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabCriticalBound", "Approve")]
        public Task<IActionResult> Approve(
            Guid valueBoundId,
            Guid requestId,
            [FromBody] DecideCriticalBoundChangeRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.ApproveAsync(valueBoundId, requestId, request, cancellationToken),
                "Perubahan batas kritis disetujui dan mulai berlaku.");

        /// <summary>
        /// Menolak pengajuan. Batas kritis yang berlaku tidak berubah sama sekali.
        /// </summary>
        [HttpPost("{requestId:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<LabBoundChangeRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Approve", "Decide Lab Critical Bound Change", Description = "Menolak perubahan batas kritis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabCriticalBound", "Approve")]
        public Task<IActionResult> Reject(
            Guid valueBoundId,
            Guid requestId,
            [FromBody] DecideCriticalBoundChangeRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.RejectAsync(valueBoundId, requestId, request, cancellationToken),
                "Pengajuan perubahan batas kritis ditolak.");

        /// <summary>
        /// Menarik pengajuan sendiri. Hanya pengaju yang boleh (<c>VAL-35</c>).
        /// </summary>
        [HttpPost("{requestId:guid}/withdraw")]
        [ProducesResponseType(typeof(ApiResponse<LabBoundChangeRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Submit Lab Critical Bound Change", Description = "Menarik pengajuan perubahan batas kritis", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("LabValueBound", "Update")]
        public Task<IActionResult> Withdraw(
            Guid valueBoundId,
            Guid requestId,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.WithdrawAsync(valueBoundId, requestId, cancellationToken),
                "Pengajuan perubahan batas kritis ditarik.");

        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabBoundChangeRequestResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabBoundChangeRequestResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabCriticalBoundForbiddenException exception)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
            }
            catch (LabCriticalBoundConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabCriticalBoundValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }
    }
}
