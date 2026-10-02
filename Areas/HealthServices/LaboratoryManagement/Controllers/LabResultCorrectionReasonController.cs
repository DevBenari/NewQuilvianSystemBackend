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
    /// Pengelolaan daftar alasan pengembalian hasil (<c>LAB-DEC-138</c>, <c>LAB-DEC-082</c>;
    /// <c>r34</c> bagian 29.6, <c>BE-LAB-71</c>).
    ///
    /// Dua peran dipisahkan lewat nama aksi. Kepala instalasi menambah, mengubah, dan
    /// mengaktifkan atau menonaktifkan alasan. <b>Hanya admin sistem</b> — pemegang
    /// <c>LabResultCorrectionReason : SystemFlag</c> — yang menyetel <i>wajib catatan</i> (pola
    /// <c>LAB-DEC-019</c>).
    ///
    /// <b>Nol penghapusan.</b> Riwayat pengembalian menyimpan kode alasan yang dipilih saat itu;
    /// alasan yang tidak lagi dipakai dinonaktifkan lewat <c>PATCH /{id}/status</c>.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-result-correction-reasons")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Result Correction Reason",
        AreaName = "HealthServices",
        ControllerName = "LabResultCorrectionReason",
        Description = "Pengelolaan alasan pengembalian dan koreksi hasil laboratorium",
        SortOrder = 22
    )]
    [Tags("Health Services / Laboratory Management / Lab Result Correction Reason")]
    public class LabResultCorrectionReasonController : ControllerBase
    {
        private const string ReadDescription = "Melihat daftar, ringkasan, pilihan, dan detail alasan pengembalian hasil";
        private const string UpdateDescription = "Mengubah alasan pengembalian hasil beserta status aktifnya";

        private readonly LabResultCorrectionReasonService _service;

        public LabResultCorrectionReasonController(LabResultCorrectionReasonService service)
        {
            _service = service;
        }

        // Daftar untuk layar pengelolaan kepala instalasi. Memuat yang nonaktif juga, sebab
        // tanpa itu baris yang dinonaktifkan nol akan pernah dapat diaktifkan kembali.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabResultReasonResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Result Correction Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabResultCorrectionReason", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabResultReasonPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabResultReasonResponse>>.Ok(
                result, "Daftar alasan pengembalian hasil berhasil diambil."));
        }

        // Pilihan untuk Kembalikan ke analis. Hanya alasan AKTIF.
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabResultReasonOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Result Correction Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabResultCorrectionReason", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetOptionsAsync(cancellationToken);

            return Ok(ApiResponse<PagedResult<LabResultReasonOptionResponse>>.Ok(
                result, "Pilihan alasan pengembalian hasil berhasil diambil."));
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Result Correction Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabResultCorrectionReason", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = LabFilterMetadataFactory.LabResultReason();

            return Ok(ApiResponse<LabResultReasonFilterMetadataResponse>.Ok(
                result, "Metadata penyaring alasan pengembalian hasil berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Result Correction Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabResultCorrectionReason", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<LabResultReasonSummaryResponse>.Ok(
                result, "Ringkasan alasan pengembalian hasil berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Result Correction Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabResultCorrectionReason", "Read")]
        public Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.GetByIdAsync(id, cancellationToken),
                "Detail alasan pengembalian hasil berhasil diambil.");

        // Kode wajib huruf besar, angka, dan tanda hubung (VAL-141) dan unik (VAL-140).
        // RequiresNote TIDAK diterima di sini — hanya lewat system-flags.
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Lab Result Correction Reason", Description = "Menambah alasan pengembalian hasil", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabResultCorrectionReason", "Create")]
        public Task<IActionResult> Create(
            [FromBody] LabResultReasonCreateRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.CreateAsync(request, cancellationToken),
                "Alasan pengembalian hasil berhasil ditambahkan.");

        // Nama, keterangan, dan urutan. Kode tidak dapat diubah (VAL-142); status lewat PATCH.
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Result Correction Reason", Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabResultCorrectionReason", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] LabResultReasonUpdateRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.UpdateAsync(id, request, cancellationToken),
                "Alasan pengembalian hasil berhasil diubah.");

        // Penonaktifan, BUKAN penghapusan.
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Result Correction Reason", Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabResultCorrectionReason", "Update")]
        public Task<IActionResult> SetStatus(
            Guid id,
            [FromBody] LabResultReasonStatusRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.SetStatusAsync(id, request.IsActive, cancellationToken),
                request.IsActive
                    ? "Alasan pengembalian hasil diaktifkan."
                    : "Alasan pengembalian hasil dinonaktifkan.");

        // Wajib catatan. Hanya pemegang SystemFlag — admin sistem, bukan kepala instalasi.
        [HttpPut("{id:guid}/system-flags")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("SystemFlag", "Set Lab Result Correction Reason System Flags", Description = "Menyetel penanda wajib catatan pada alasan pengembalian hasil", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabResultCorrectionReason", "SystemFlag")]
        public Task<IActionResult> SetSystemFlags(
            Guid id,
            [FromBody] LabResultReasonSystemFlagsRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.SetSystemFlagsAsync(id, request, cancellationToken),
                "Penanda alasan pengembalian hasil berhasil disetel.");

        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabResultReasonResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabResultReasonResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabResultReasonConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabResultReasonValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }
    }
}
