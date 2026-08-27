using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/billing-management/master-data/registers")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Billing Management Master Data",
        displayName: "Register",
        AreaName = "HealthServices",
        ControllerName = "Register",
        Description = "Health service billing management master data cashier register",
        SortOrder = 22
    )]
    [Tags("Health Services / Billing Management / Master Data / Register")]
    public sealed class RegisterController : ControllerBase
    {
        private readonly RegisterService _service;

        public RegisterController(RegisterService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RegisterResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Register", Description = "Melihat data register", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Register", "Read")]
        public async Task<IActionResult> GetRegisters(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "registerName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                search, isActive, sortBy, sortDirection, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<RegisterResponse>>.Ok(
                result, "Data register berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RegisterOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Register", Description = "Melihat data register", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Register", "Read")]
        public async Task<IActionResult> GetRegisterOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetOptionsAsync(onlyActive, search, cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<RegisterOptionResponse>>.Ok(
                result, "Data pilihan register berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Register", Description = "Melihat data register", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Register", "Read")]
        public async Task<IActionResult> GetRegisterById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, cancellationToken);
                return Ok(ApiResponse<RegisterResponse>.Ok(result, "Detail register berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Register", Description = "Membuat data register", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Register", "Create")]
        public async Task<IActionResult> CreateRegister(
            [FromBody] CreateRegisterRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.CreateAsync(request, GetCurrentUserId(), cancellationToken);
                return Ok(ApiResponse<RegisterResponse>.Ok(result, "Register berhasil dibuat."));
            }
            catch (RegisterValidationException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Register", Description = "Mengubah data register", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Register", "Update")]
        public async Task<IActionResult> UpdateRegister(
            Guid id, [FromBody] UpdateRegisterRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request, GetCurrentUserId(), cancellationToken);
                return Ok(ApiResponse<RegisterResponse>.Ok(result, "Register berhasil diperbarui."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
            }
            catch (RegisterValidationException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
            }
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<RegisterStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Register", Description = "Mengaktifkan data register", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Register", "Update")]
        public async Task<IActionResult> ActivateRegister(Guid id, CancellationToken cancellationToken) =>
            await ChangeStatusAsync(id, true, "Register berhasil diaktifkan.", cancellationToken);

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<RegisterStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Register", Description = "Menonaktifkan data register", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Register", "Update")]
        public async Task<IActionResult> DeactivateRegister(Guid id, CancellationToken cancellationToken) =>
            await ChangeStatusAsync(id, false, "Register berhasil dinonaktifkan.", cancellationToken);

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RegisterDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Register", Description = "Menghapus data register", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Register", "Delete")]
        public async Task<IActionResult> DeleteRegister(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.DeleteAsync(id, GetCurrentUserId(), cancellationToken);
                return Ok(ApiResponse<RegisterDeleteResponse>.Ok(result, "Register berhasil dihapus."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
            }
        }

        private async Task<IActionResult> ChangeStatusAsync(
            Guid id, bool isActive, string message, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.ChangeStatusAsync(id, isActive, GetCurrentUserId(), cancellationToken);
                return Ok(ApiResponse<RegisterStatusResponse>.Ok(result, message));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdText, out var userId) ? userId : Guid.Empty;
        }
    }
}
