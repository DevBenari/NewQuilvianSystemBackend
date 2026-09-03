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
    /// Pengelolaan alasan penolakan sampel.
    ///
    /// Lima endpoint di sini melayani dua peran yang sengaja dipisahkan. Kepala instalasi
    /// menambah, mengubah nama dan urutan, serta mengaktifkan atau menonaktifkan alasan.
    /// Administrator sistem — dan hanya dia — menyetel penanda kesalahan internal serta
    /// penanda wajib catatan lewat <c>PUT /{id}/system-flags</c>, karena kedua penanda itu
    /// menentukan siapa menanggung biaya pengambilan ulang (<c>LAB-DEC-019</c>).
    ///
    /// Upaya kepala instalasi mengubah kedua penanda itu lewat <c>PUT /{id}</c> biasa ditolak
    /// <c>403</c> (<c>VAL-37</c>).
    ///
    /// <c>GET /lab-specimens/rejection-reasons</c> yang sudah ada tetap menjadi jalur baca bagi
    /// petugas yang sedang menolak sampel; endpoint di sini adalah jalur pengelolaan terpisah.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-rejection-reasons")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Rejection Reason",
        AreaName = "HealthServices",
        ControllerName = "LabRejectionReason",
        Description = "Pengelolaan alasan penolakan sampel laboratorium",
        SortOrder = 5
    )]
    [Tags("Health Services / Laboratory Management / Lab Rejection Reason")]
    public class LabRejectionReasonController : ControllerBase
    {
        private readonly LabRejectionReasonService _labRejectionReasonService;

        public LabRejectionReasonController(LabRejectionReasonService labRejectionReasonService)
        {
            _labRejectionReasonService = labRejectionReasonService;
        }

        /// <summary>
        /// Daftar alasan penolakan untuk layar pengelolaan, termasuk yang sudah dinonaktifkan.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabRejectionReasonResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Rejection Reason", Description = "Melihat daftar alasan penolakan sampel", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabRejectionReason", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabRejectionReasonPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labRejectionReasonService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabRejectionReasonResponse>>.Ok(
                result,
                "Daftar alasan penolakan sampel berhasil diambil."));
        }

        /// <summary>
        /// Menambah alasan penolakan baru.
        ///
        /// Kedua penanda terkunci tidak dapat diisi dari sini; alasan baru selalu lahir dengan
        /// nilai bawaan (<c>AC-26</c>). Kode alasan yang sudah dipakai ditolak <c>409</c>
        /// (<c>VAL-36</c>).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabRejectionReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Lab Rejection Reason", Description = "Menambah alasan penolakan sampel", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabRejectionReason", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateLabRejectionReasonRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labRejectionReasonService.CreateAsync(request, cancellationToken),
                "Alasan penolakan sampel berhasil ditambahkan.");

        /// <summary>
        /// Mengubah nama, keterangan, dan urutan tampil.
        ///
        /// Penanda kesalahan internal dan penanda wajib catatan <b>tidak</b> dapat diubah lewat
        /// sini. Permintaan yang memuat salah satunya ditolak <c>403</c> (<c>VAL-37</c>).
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabRejectionReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Rejection Reason", Description = "Mengubah alasan penolakan sampel", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabRejectionReason", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabRejectionReasonRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labRejectionReasonService.UpdateAsync(id, request, cancellationToken),
                "Alasan penolakan sampel berhasil diubah.");

        /// <summary>
        /// Mengaktifkan atau menonaktifkan satu alasan penolakan. Alasan aktif terakhir tidak
        /// dapat dinonaktifkan (<c>VAL-38</c>).
        /// </summary>
        [HttpPut("{id:guid}/activation")]
        [ProducesResponseType(typeof(ApiResponse<LabRejectionReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Rejection Reason", Description = "Mengaktifkan atau menonaktifkan alasan penolakan sampel", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabRejectionReason", "Update")]
        public Task<IActionResult> SetActivation(
            Guid id,
            [FromBody] SetLabRejectionReasonActivationRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labRejectionReasonService.SetActivationAsync(id, request, cancellationToken),
                request.IsActive
                    ? "Alasan penolakan sampel berhasil diaktifkan."
                    : "Alasan penolakan sampel berhasil dinonaktifkan.");

        /// <summary>
        /// Menyetel penanda kesalahan internal dan penanda wajib catatan.
        ///
        /// Hanya pemegang <c>LabRejectionReason : SystemFlag</c> yang dapat memanggilnya.
        /// Pemisahan hak akses inilah yang membuat kepala instalasi tidak dapat memindahkan
        /// beban biaya pengambilan ulang sendirian.
        /// </summary>
        [HttpPut("{id:guid}/system-flags")]
        [ProducesResponseType(typeof(ApiResponse<LabRejectionReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("SystemFlag", "Set Lab Rejection Reason System Flags", Description = "Menyetel penanda sistem pada alasan penolakan sampel", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabRejectionReason", "SystemFlag")]
        public Task<IActionResult> SetSystemFlags(
            Guid id,
            [FromBody] SetLabRejectionReasonSystemFlagsRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labRejectionReasonService.SetSystemFlagsAsync(id, request, cancellationToken),
                "Penanda sistem pada alasan penolakan sampel berhasil disetel.");

        /// <summary>
        /// Menjalankan satu perubahan dan menerjemahkan kegagalannya menjadi status HTTP yang
        /// tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabRejectionReasonResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabRejectionReasonResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabRejectionReasonForbiddenException exception)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
            }
            catch (LabRejectionReasonConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabRejectionReasonValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, exception.Message));
            }
        }
    }
}
