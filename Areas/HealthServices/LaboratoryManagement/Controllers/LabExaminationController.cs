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
    /// Pemeriksaan terpesan — satuan yang ditagihkan, terpisah dari wadah fisik yang
    /// menopangnya.
    ///
    /// Satu wadah menopang satu atau lebih pemeriksaan. Membatalkan satu pemeriksaan di sini
    /// tidak menyentuh pemeriksaan lain pada wadah yang sama; menggugurkan seluruh isi wadah
    /// adalah akibat penolakan wadah, dan itu pekerjaan grup Lab Specimen.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-examinations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Examination",
        AreaName = "HealthServices",
        ControllerName = "LabExamination",
        Description = "Pengelolaan pemeriksaan terpesan laboratorium",
        SortOrder = 6
    )]
    [Tags("Health Services / Laboratory Management / Lab Examination")]
    public class LabExaminationController : ControllerBase
    {
        private readonly LabExaminationService _labExaminationService;

        public LabExaminationController(LabExaminationService labExaminationService)
        {
            _labExaminationService = labExaminationService;
        }

        // Daftar pemeriksaan terpesan pada satu pesanan.
        [HttpGet("by-order/{labOrderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<LabExaminationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Examination", Description = "Melihat pemeriksaan terpesan pada satu pesanan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabExamination", "Read")]
        public Task<IActionResult> GetByOrder(
            Guid labOrderId,
            CancellationToken cancellationToken = default) =>
            ExecuteListAsync(
                () => _labExaminationService.GetByOrderAsync(labOrderId, cancellationToken),
                "Daftar pemeriksaan terpesan berhasil diambil.");

        // Daftar pemeriksaan yang ditopang satu wadah. Inilah yang membuktikan satu tabung
        // dapat menopang beberapa pemeriksaan sekaligus (AC-35).
        [HttpGet("by-specimen/{specimenId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<LabExaminationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Examination", Description = "Melihat pemeriksaan yang ditopang satu wadah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabExamination", "Read")]
        public Task<IActionResult> GetBySpecimen(
            Guid specimenId,
            CancellationToken cancellationToken = default) =>
            ExecuteListAsync(
                () => _labExaminationService.GetBySpecimenAsync(specimenId, cancellationToken),
                "Daftar pemeriksaan pada wadah berhasil diambil.");

        // Menambah pemeriksaan terpesan dan menautkannya ke wadah penopangnya.
        //
        // Harga tidak diterima dari pemanggil; backend menyalinnya dari tarif yang berlaku.
        // Jenis pemeriksaan yang bukan laboratorium ditolak 422 (VAL-17); tarif yang belum
        // diatur ditolak 422 (VAL-20); wadah yang sudah diputuskan ditolak 409 (VAL-18).
        [HttpPost("by-order/{labOrderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Lab Examination", Description = "Menambah pemeriksaan terpesan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabExamination", "Create")]
        public Task<IActionResult> Add(
            Guid labOrderId,
            [FromBody] AddLabExaminationRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labExaminationService.AddAsync(labOrderId, request, cancellationToken),
                "Pemeriksaan terpesan berhasil ditambahkan.");

        // Membatalkan satu pemeriksaan terpesan. Pemeriksaan lain pada wadah yang sama tidak
        // berubah, dan status wadahnya sendiri tidak disentuh.
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Cancel Lab Examination", Description = "Membatalkan satu pemeriksaan terpesan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabExamination", "Update")]
        public Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLabExaminationRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labExaminationService.CancelAsync(id, request, cancellationToken),
                "Pemeriksaan terpesan berhasil dibatalkan.");

        // Menandai satu pemeriksaan cito atau mengembalikannya menjadi biasa.
        //
        // Kesegeraan melekat pada pemeriksaan, bukan pada pesanan (LAB-DEC-026). Tidak ada
        // endpoint sejenis pada grup Lab Order, dan ketiadaan itu disengaja (AC-40).
        //
        // Bukan dokter pemesan ditolak 403 (VAL-03); pesanan yang sudah selesai atau
        // dibatalkan ditolak 409 (VAL-04).
        [HttpPut("{id:guid}/urgency")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Set Lab Examination Urgency", Description = "Menandai satu pemeriksaan cito atau mengembalikannya biasa", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabExamination", "Update")]
        public Task<IActionResult> SetUrgency(
            Guid id,
            [FromBody] SetLabExaminationUrgencyRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labExaminationService.SetUrgencyAsync(id, request, cancellationToken),
                "Kesegeraan pemeriksaan berhasil diperbarui.");

        // Menandai satu pemeriksaan dikerjakan ganda, atau membatalkan penandaannya.
        //
        // Penandaan hanya berlaku pada pemeriksaan itu sendiri; pemeriksaan lain pada wadah
        // yang sama tidak ikut berubah (AC-40). Wadah yang sudah ditolak ditolak 409.
        [HttpPut("{id:guid}/duplo")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Set Lab Examination Duplo", Description = "Menandai satu pemeriksaan dikerjakan ganda", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabExamination", "Update")]
        public Task<IActionResult> SetDuplo(
            Guid id,
            [FromBody] SetLabExaminationDuploRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labExaminationService.SetDuploAsync(id, request, cancellationToken),
                "Penanda duplo pemeriksaan berhasil diperbarui.");

        private async Task<IActionResult> ExecuteListAsync(
            Func<Task<List<LabExaminationResponse>>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<List<LabExaminationResponse>>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        /// <summary>
        /// Menjalankan satu perubahan dan menerjemahkan kegagalannya menjadi status HTTP yang
        /// tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabExaminationResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabExaminationResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabExaminationForbiddenException exception)
            {
                // VAL-03. Objek yang diminta memang ada dan pemanggilnya memang sudah masuk;
                // yang tidak dia miliki adalah kewenangan atas pesanan ini. Karena itu 403,
                // bukan 404 maupun 422.
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
            }
            catch (LabExaminationConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabExaminationValidationException exception)
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
