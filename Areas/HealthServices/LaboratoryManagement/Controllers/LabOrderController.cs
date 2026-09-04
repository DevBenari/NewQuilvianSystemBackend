using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-orders")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Order",
        AreaName = "HealthServices",
        ControllerName = "LabOrder",
        Description = "Pencatatan order pemeriksaan laboratorium",
        SortOrder = 1
    )]
    [Tags("Health Services / Laboratory Management / Lab Order")]
    public class LabOrderController : ControllerBase
    {
        private readonly LabOrderService _labOrderService;

        public LabOrderController(LabOrderService labOrderService)
        {
            _labOrderService = labOrderService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<LabOrderListResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Order", Description = "Melihat daftar order laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrder", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] Guid? encounterId,
            CancellationToken cancellationToken = default)
        {
            // BE-RWI-042 - penyaring kunjungan, bentuknya sama persis dengan RadOrderController
            // supaya kedua daftar pesanan penunjang dipanggil dengan cara yang sama.
            var result = await _labOrderService.GetListAsync(encounterId, cancellationToken);

            return Ok(ApiResponse<List<LabOrderListResponse>>.Ok(
                result,
                "Daftar order laboratorium berhasil diambil."));
        }

        /// <summary>
        /// Pesanan laboratorium dan ketersediaan hasilnya untuk satu perawatan rawat inap.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-052</c>, <c>api-contract.md</c> bagian 7. Hasil yang belum final ditandai
        /// dan <b>tidak</b> disajikan sebagai hasil sah - <c>VAL-DOK-30</c>. Tidak ada satu pun
        /// baris hasil yang disalin ke Rawat Inap; yang dibaca adalah baris milik Laboratorium
        /// apa adanya - <c>RUL-DOK-02</c>.
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<LabOrderListResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Order", Description = "Melihat pesanan dan hasil laboratorium satu perawatan rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrder", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var result = await _labOrderService.GetByEpisodeAsync(episodeId, cancellationToken);

            return Ok(ApiResponse<List<LabOrderListResponse>>.Ok(
                result,
                "Pesanan laboratorium perawatan rawat inap berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Order", Description = "Melihat detail order laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrder", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _labOrderService.GetDetailAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order laboratorium tidak ditemukan."));
            }

            return Ok(ApiResponse<LabOrderDetailResponse>.Ok(
                result,
                "Detail order laboratorium berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Lab Order", Description = "Membuat order pemeriksaan laboratorium", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabOrder", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLabOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOrderService.CreateAsync(request, cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    ApiResponse<LabOrderDetailResponse>.Ok(
                        result,
                        "Order laboratorium berhasil dibuat."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }
        }

        /// <summary>
        /// Menandai pesanan mulai dikerjakan laboratorium.
        /// </summary>
        [HttpPut("{id:guid}/start-process")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Process Lab Order", Description = "Menandai order mulai dikerjakan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabOrder", "Process")]
        public Task<IActionResult> StartProcess(Guid id, CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrderService.StartProcessAsync(id, cancellationToken),
                "Order laboratorium mulai dikerjakan.");

        /// <summary>
        /// Menandai pekerjaan laboratorium selesai. Tidak menerbitkan fakta tagihan; kelayakan
        /// tagih sudah terbentuk pada saat sampel dinyatakan layak.
        /// </summary>
        [HttpPut("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Process Lab Order", Description = "Menandai order selesai dikerjakan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabOrder", "Process")]
        public Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrderService.CompleteAsync(id, cancellationToken),
                "Order laboratorium selesai dikerjakan.");

        [HttpPut("{id:guid}/hold")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Hold Lab Order", Description = "Menahan order laboratorium", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabOrder", "Hold")]
        public Task<IActionResult> Hold(
            Guid id,
            [FromBody] HoldLabRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrderService.HoldAsync(id, request, cancellationToken),
                "Order laboratorium ditahan.");

        [HttpPut("{id:guid}/resume")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Hold Lab Order", Description = "Melanjutkan order laboratorium yang ditahan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabOrder", "Hold")]
        public Task<IActionResult> Resume(
            Guid id,
            [FromBody] ResumeLabRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrderService.ResumeAsync(id, request, cancellationToken),
                "Order laboratorium dilanjutkan.");

        /// <summary>
        /// Membatalkan order laboratorium beserta sampel yang masih berjalan.
        ///
        /// Pembatalan ini bersifat klinis. Untuk sampel yang sebelumnya sudah dinyatakan layak,
        /// diterbitkan fakta pembatalan sebagai revisi baru sehingga tagihan lama tetap utuh
        /// dan Billing yang menentukan koreksinya. Laboratorium tidak menghapus tagihan.
        /// </summary>
        [HttpPut("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderCancellationResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Cancel Lab Order", Description = "Membatalkan order laboratorium", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabOrder", "Update")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLabSpecimenRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOrderService.CancelAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabOrderCancellationResult>.Ok(
                    result,
                    "Order laboratorium berhasil dibatalkan secara klinis."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message));
            }
            catch (LabConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }
        }

        /// <summary>
        /// Menjalankan satu perpindahan status dan menerjemahkan kegagalannya menjadi status
        /// HTTP yang tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabOrderDetailResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabOrderDetailResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message));
            }
            catch (LabConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }
        }
    }
}
