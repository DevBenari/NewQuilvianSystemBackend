using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/radiology-management/rad-orders")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_RADIOLOGY_MANAGEMENT",
        moduleName: "Health Service Radiology Management",
        displayName: "Rad Order",
        AreaName = "HealthServices",
        ControllerName = "RadOrder",
        Description = "Pencatatan order pemeriksaan radiologi",
        SortOrder = 1
    )]
    [Tags("Health Services / Radiology Management / Rad Order")]
    public class RadOrderController : ControllerBase
    {
        private readonly RadOrderService _radOrderService;

        public RadOrderController(RadOrderService radOrderService)
        {
            _radOrderService = radOrderService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RadOrderListResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat daftar order radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] Guid? encounterId,
            CancellationToken cancellationToken = default)
        {
            var result = await _radOrderService.GetListAsync(encounterId, cancellationToken);

            return Ok(ApiResponse<List<RadOrderListResponse>>.Ok(
                result, "Daftar order radiologi berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Order", Description = "Melihat detail order radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadOrder", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _radOrderService.GetDetailAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order radiologi tidak ditemukan.",
                    new { Code = RadErrorCodes.OrderNotFound }));
            }

            return Ok(ApiResponse<RadOrderDetailResponse>.Ok(
                result, "Detail order radiologi berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RadOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Rad Order", Description = "Membuat order radiologi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadOrder", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateRadOrderRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.CreateAsync(request, cancellationToken),
                "Order radiologi berhasil dibuat.");

        [HttpPut("{id:guid}/accept")]
        [AccessAction("Process", "Process Rad Order", Description = "Menerima order radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Process")]
        public Task<IActionResult> Accept(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.AcceptAsync(id, request, cancellationToken),
                "Order radiologi berhasil diterima.");

        [HttpPut("{id:guid}/schedule")]
        [AccessAction("Schedule", "Schedule Rad Order", Description = "Menjadwalkan order radiologi", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RadOrder", "Schedule")]
        public Task<IActionResult> Schedule(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.ScheduleAsync(id, request, cancellationToken),
                "Order radiologi berhasil dijadwalkan.");

        [HttpPut("{id:guid}/start")]
        [AccessAction("Process", "Process Rad Order", Description = "Memulai pengerjaan order radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Process")]
        public Task<IActionResult> Start(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.StartAsync(id, request, cancellationToken),
                "Order radiologi mulai dikerjakan.");

        [HttpPut("{id:guid}/complete")]
        [AccessAction("Process", "Process Rad Order", Description = "Menyelesaikan order radiologi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadOrder", "Process")]
        public Task<IActionResult> Complete(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.CompleteAsync(id, request, cancellationToken),
                "Order radiologi berhasil diselesaikan.");

        [HttpPut("{id:guid}/hold")]
        [AccessAction("Hold", "Hold Rad Order", Description = "Menahan order radiologi", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RadOrder", "Hold")]
        public Task<IActionResult> Hold(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.HoldAsync(id, request, cancellationToken),
                "Order radiologi berhasil ditahan.");

        [HttpPut("{id:guid}/resume")]
        [AccessAction("Hold", "Hold Rad Order", Description = "Melanjutkan order radiologi yang ditahan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RadOrder", "Hold")]
        public Task<IActionResult> Resume(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.ResumeAsync(id, request, cancellationToken),
                "Order radiologi berhasil dilanjutkan.");

        [HttpPut("{id:guid}/reject")]
        [AccessAction("Update", "Update Rad Order", Description = "Menolak order radiologi", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("RadOrder", "Update")]
        public Task<IActionResult> Reject(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.RejectAsync(id, request, cancellationToken),
                "Order radiologi berhasil ditolak.");

        [HttpPut("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Rad Order", Description = "Membatalkan order radiologi", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("RadOrder", "Cancel")]
        public Task<IActionResult> Cancel(
            Guid id, [FromBody] RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radOrderService.CancelAsync(id, request, cancellationToken),
                "Order radiologi berhasil dibatalkan.");

        /// <summary>
        /// Menjalankan satu tindakan dan memetakan hasilnya menjadi status HTTP.
        ///
        /// Pemetaan ini yang membuat perbedaan jenis penolakan sampai ke pemanggil.
        /// `SafetyBlocked` dan `PolicyNotConfigured` sama-sama menjadi <c>422</c>, bukan
        /// <c>400</c>: permintaannya sendiri sah, yang belum terpenuhi adalah prasyaratnya.
        /// Kode galatnya tetap berbeda supaya layar dapat menuntun ke tindakan yang benar.
        /// </summary>
        private async Task<IActionResult> Execute<T>(
            Func<Task<RadOperationResult<T>>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return result.Kind switch
                {
                    RadOperationResultKind.Success =>
                        Ok(ApiResponse<T>.Ok(result.Value, successMessage)),

                    RadOperationResultKind.NotFound =>
                        NotFound(ApiResponse<object>.Fail(
                            StatusCodes.Status404NotFound,
                            result.ErrorMessage ?? "Data tidak ditemukan.",
                            new { Code = result.ErrorCode })),

                    RadOperationResultKind.Conflict =>
                        Conflict(ApiResponse<object>.Fail(
                            StatusCodes.Status409Conflict,
                            result.ErrorMessage ?? "Terjadi konflik.",
                            new { Code = result.ErrorCode })),

                    RadOperationResultKind.SafetyBlocked or
                    RadOperationResultKind.PolicyNotConfigured =>
                        UnprocessableEntity(ApiResponse<object>.Fail(
                            StatusCodes.Status422UnprocessableEntity,
                            result.ErrorMessage ?? "Prasyarat belum terpenuhi.",
                            new { Code = result.ErrorCode })),

                    _ => BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        result.ErrorMessage ?? "Permintaan tidak valid.",
                        new { Code = result.ErrorCode })),
                };
            }
            catch (RadConcurrencyException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    exception.Message,
                    new { Code = RadErrorCodes.ConcurrencyConflict }));
            }
        }
    }
}
