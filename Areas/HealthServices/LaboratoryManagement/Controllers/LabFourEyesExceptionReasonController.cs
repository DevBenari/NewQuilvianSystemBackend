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
    /// Pengelolaan daftar alasan <b>merangkap peran</b> pada hasil yang sama — pemvalidasi yang
    /// juga pengisi hasil, atau perilis yang juga pemvalidasi (<c>LAB-DEC-003</c>, <c>INV-42</c>,
    /// <c>INV-43</c>; <c>r34</c> bagian 29.6, <c>BE-LAB-71</c>).
    ///
    /// Dua peran dipisahkan lewat nama aksi, sama dengan alasan pengembalian hasil. Kepala
    /// instalasi mengelola isinya; <b>hanya admin sistem</b> — pemegang
    /// <c>LabFourEyesExceptionReason : SystemFlag</c> — yang menyetel <i>wajib catatan</i>.
    ///
    /// <b>Nol penghapusan.</b> Hasil yang sudah memakai sebuah alasan menunjuknya lewat foreign
    /// key <c>Restrict</c> dan menyalin namanya — nama itu tercetak pada penanda pengecualian.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-four-eyes-exception-reasons")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Four Eyes Exception Reason",
        AreaName = "HealthServices",
        ControllerName = "LabFourEyesExceptionReason",
        Description = "Pengelolaan alasan merangkap peran pada validasi dan rilis hasil laboratorium",
        SortOrder = 23
    )]
    [Tags("Health Services / Laboratory Management / Lab Four Eyes Exception Reason")]
    public class LabFourEyesExceptionReasonController : ControllerBase
    {
        private const string ReadDescription = "Melihat daftar, ringkasan, pilihan, dan detail alasan pengecualian empat mata";
        private const string UpdateDescription = "Mengubah alasan pengecualian empat mata beserta status aktifnya";

        private readonly LabFourEyesExceptionReasonService _service;

        public LabFourEyesExceptionReasonController(LabFourEyesExceptionReasonService service)
        {
            _service = service;
        }

        // Daftar untuk layar pengelolaan kepala instalasi. Memuat yang nonaktif juga, sebab
        // tanpa itu baris yang dinonaktifkan nol akan pernah dapat diaktifkan kembali.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabResultReasonResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Four Eyes Exception Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabFourEyesExceptionReason", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabResultReasonPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabResultReasonResponse>>.Ok(
                result, "Daftar alasan pengecualian empat mata berhasil diambil."));
        }

        // Pilihan untuk layar Validasi dan Rilis ketika pelaku merangkap peran. Hanya alasan AKTIF.
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabResultReasonOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Four Eyes Exception Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabFourEyesExceptionReason", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetOptionsAsync(cancellationToken);

            return Ok(ApiResponse<PagedResult<LabResultReasonOptionResponse>>.Ok(
                result, "Pilihan alasan pengecualian empat mata berhasil diambil."));
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Four Eyes Exception Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabFourEyesExceptionReason", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = LabFilterMetadataFactory.LabResultReason();

            return Ok(ApiResponse<LabResultReasonFilterMetadataResponse>.Ok(
                result, "Metadata penyaring alasan pengecualian empat mata berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Four Eyes Exception Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabFourEyesExceptionReason", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<LabResultReasonSummaryResponse>.Ok(
                result, "Ringkasan alasan pengecualian empat mata berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Four Eyes Exception Reason", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabFourEyesExceptionReason", "Read")]
        public Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.GetByIdAsync(id, cancellationToken),
                "Detail alasan pengecualian empat mata berhasil diambil.");

        // Kode wajib huruf besar, angka, dan tanda hubung (VAL-141) dan unik (VAL-140).
        // RequiresNote TIDAK diterima di sini — hanya lewat system-flags.
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Lab Four Eyes Exception Reason", Description = "Menambah alasan pengecualian empat mata", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabFourEyesExceptionReason", "Create")]
        public Task<IActionResult> Create(
            [FromBody] LabResultReasonCreateRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.CreateAsync(request, cancellationToken),
                "Alasan pengecualian empat mata berhasil ditambahkan.");

        // Nama, keterangan, dan urutan. Kode tidak dapat diubah (VAL-142); status lewat PATCH.
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Four Eyes Exception Reason", Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabFourEyesExceptionReason", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] LabResultReasonUpdateRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.UpdateAsync(id, request, cancellationToken),
                "Alasan pengecualian empat mata berhasil diubah.");

        // Penonaktifan, BUKAN penghapusan.
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Four Eyes Exception Reason", Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabFourEyesExceptionReason", "Update")]
        public Task<IActionResult> SetStatus(
            Guid id,
            [FromBody] LabResultReasonStatusRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.SetStatusAsync(id, request.IsActive, cancellationToken),
                request.IsActive
                    ? "Alasan pengecualian empat mata diaktifkan."
                    : "Alasan pengecualian empat mata dinonaktifkan.");

        // Wajib catatan. Hanya pemegang SystemFlag — admin sistem, bukan kepala instalasi.
        [HttpPut("{id:guid}/system-flags")]
        [ProducesResponseType(typeof(ApiResponse<LabResultReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("SystemFlag", "Set Lab Four Eyes Exception Reason System Flags", Description = "Menyetel penanda wajib catatan pada alasan pengecualian empat mata", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabFourEyesExceptionReason", "SystemFlag")]
        public Task<IActionResult> SetSystemFlags(
            Guid id,
            [FromBody] LabResultReasonSystemFlagsRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _service.SetSystemFlagsAsync(id, request, cancellationToken),
                "Penanda alasan pengecualian empat mata berhasil disetel.");

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
