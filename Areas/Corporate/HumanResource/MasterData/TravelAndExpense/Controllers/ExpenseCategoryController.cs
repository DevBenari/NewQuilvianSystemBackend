using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Controllers
{
    /// <summary>
    /// Master pos beban biaya operasional (<c>Hrd.Expense.Category.Manage</c>, BE-HRD-333).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/expense-categories")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Kategori Biaya Operasional",
        AreaName = "Corporate",
        ControllerName = "ExpenseCategory",
        Description = "Master pos beban biaya operasional untuk klaim dan perjalanan dinas",
        SortOrder = 95)]
    [Tags("Corporate / Human Resource / Master Data / Expense Category")]
    public class ExpenseCategoryController : ControllerBase
    {
        private readonly ExpenseCategoryService _service;

        public ExpenseCategoryController(ExpenseCategoryService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Melihat Kategori Biaya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ExpenseCategory", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken ct)
            => Ok(ApiResponse<ExpenseCategoryFilterMetadataResponse>.Ok(await _service.GetFilterMetadataAsync(ct)));

        [HttpGet("summary")]
        [AccessAction("Read", "Melihat Kategori Biaya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ExpenseCategory", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
            => Ok(ApiResponse<ExpenseCategorySummaryResponse>.Ok(await _service.GetSummaryAsync(ct)));

        [HttpGet]
        [AccessAction("Read", "Melihat Kategori Biaya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ExpenseCategory", "Read")]
        public async Task<IActionResult> GetList([FromQuery] string? search, [FromQuery] string? categoryType, [FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => Ok(ApiResponse<PagedResult<ExpenseCategoryResponse>>.Ok(await _service.GetListAsync(search, categoryType, isActive, pageNumber, pageSize, ct)));

        [HttpGet("options")]
        [AccessAction("Read", "Melihat Kategori Biaya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ExpenseCategory", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken ct)
            => Ok(ApiResponse<List<ExpenseCategoryOptionResponse>>.Ok(await _service.GetOptionsAsync(ct)));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Kategori Biaya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ExpenseCategory", "Read")]
        public Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => Run(() => _service.GetByIdAsync(id, ct), "Detail kategori biaya berhasil diambil.");

        [HttpPost]
        [AccessAction("Create", "Menambah Kategori Biaya", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ExpenseCategory", "Create")]
        public Task<IActionResult> Create([FromBody] ExpenseCategoryRequest request, CancellationToken ct)
            => Run(() => _service.CreateAsync(request, User.RequireUserId(), ct), "Kategori biaya berhasil dibuat.");

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Mengubah Kategori Biaya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ExpenseCategory", "Update")]
        public Task<IActionResult> Update(Guid id, [FromBody] ExpenseCategoryRequest request, CancellationToken ct)
            => Run(() => _service.UpdateAsync(id, request, User.RequireUserId(), ct), "Kategori biaya berhasil diubah.");

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Mengubah Kategori Biaya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ExpenseCategory", "Update")]
        public Task<IActionResult> SetStatus(Guid id, [FromBody] ExpenseCategoryStatusRequest request, CancellationToken ct)
            => Run(() => _service.SetStatusAsync(id, request.IsActive, User.RequireUserId(), ct), "Status kategori biaya berhasil diubah.");

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Menghapus Kategori Biaya", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("ExpenseCategory", "Delete")]
        public Task<IActionResult> Delete(Guid id, CancellationToken ct)
            => Run(async () => { await _service.DeleteAsync(id, User.RequireUserId(), ct); return true; }, "Kategori biaya berhasil dihapus.");

        private async Task<IActionResult> Run<T>(Func<Task<T>> action, string message)
        {
            try
            {
                return Ok(ApiResponse<T>.Ok(await action(), message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<T>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (ExpenseCategoryConflictException ex)
            {
                return Conflict(ApiResponse<T>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<T>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }
    }
}
