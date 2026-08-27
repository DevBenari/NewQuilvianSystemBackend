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
        public async Task<IActionResult> GetList(CancellationToken cancellationToken = default)
        {
            var result = await _labOrderService.GetListAsync(cancellationToken);

            return Ok(ApiResponse<List<LabOrderListResponse>>.Ok(
                result,
                "Daftar order laboratorium berhasil diambil."));
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

        [HttpPut("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Lab Order", Description = "Membatalkan order laboratorium", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabOrder", "Update")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOrderService.CancelAsync(id, cancellationToken);

                return Ok(ApiResponse<LabOrderDetailResponse>.Ok(
                    result,
                    "Order laboratorium berhasil dibatalkan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
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
