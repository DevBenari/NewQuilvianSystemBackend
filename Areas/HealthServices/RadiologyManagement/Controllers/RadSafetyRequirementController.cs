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
    [Route("api/v1/health-services/radiology-management/master-data/rad-safety-requirements")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_RADIOLOGY_MANAGEMENT",
        moduleName: "Health Service Radiology Management",
        displayName: "Rad Safety Requirement",
        AreaName = "HealthServices",
        ControllerName = "RadSafetyRequirement",
        Description = "Pengelolaan butir keselamatan radiologi",
        SortOrder = 5
    )]
    [Tags("Health Services / Radiology Management / Master Data / Rad Safety Requirement")]
    public class RadSafetyRequirementController : ControllerBase
    {
        private readonly RadSafetyRequirementService _radSafetyRequirementService;

        public RadSafetyRequirementController(
            RadSafetyRequirementService radSafetyRequirementService)
        {
            _radSafetyRequirementService = radSafetyRequirementService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRequirementFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Safety Requirement", Description = "Melihat daftar pilihan penyaring butir keselamatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRequirement", "Read")]
        public async Task<IActionResult> GetFilterMetadata(
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radSafetyRequirementService.GetFilterMetadataAsync(cancellationToken);

            return Ok(ApiResponse<RadSafetyRequirementFilterMetadataResponse>.Ok(
                hasil, "Metadata filter butir keselamatan berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRequirementSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Safety Requirement", Description = "Melihat rekap butir keselamatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRequirement", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var hasil = await _radSafetyRequirementService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<RadSafetyRequirementSummaryResponse>.Ok(
                hasil, "Rekap butir keselamatan berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RadSafetyRequirementResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Safety Requirement", Description = "Melihat daftar butir keselamatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRequirement", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? requiresNote = null,
            [FromQuery] bool? isUsedByActiveRule = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radSafetyRequirementService.GetPagedAsync(
                new RadSafetyRequirementPagedQuery
                {
                    Search = search,
                    Category = category,
                    IsActive = isActive,
                    RequiresNote = requiresNote,
                    IsUsedByActiveRule = isUsedByActiveRule,
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                },
                cancellationToken);

            return Ok(ApiResponse<PagedResult<RadSafetyRequirementResponse>>.Ok(
                hasil, "Daftar butir keselamatan berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<RadSafetyRequirementOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Safety Requirement", Description = "Mengambil pilihan butir keselamatan untuk dropdown", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRequirement", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search = null,
            [FromQuery] bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radSafetyRequirementService.GetOptionsAsync(
                search, onlyActive, cancellationToken);

            return Ok(ApiResponse<List<RadSafetyRequirementOptionResponse>>.Ok(
                hasil, "Pilihan butir keselamatan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRequirementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Safety Requirement", Description = "Melihat rincian satu butir keselamatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRequirement", "Read")]
        public Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyRequirementService.GetByIdAsync(id, cancellationToken),
                "Rincian butir keselamatan berhasil diambil.");

        // Menambah butir di sini TIDAK membuatnya berlaku bagi pasien mana pun. Butir baru hanya
        // menjadi pilihan yang tersedia; yang mengikat adalah aturan keselamatan yang
        // menyusunnya untuk sebuah alat dan disahkan penanggung jawab klinis.
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRequirementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Rad Safety Requirement", Description = "Menambah butir keselamatan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadSafetyRequirement", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateRadSafetyRequirementRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyRequirementService.CreateAsync(request, cancellationToken),
                "Butir keselamatan berhasil ditambahkan.");

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRequirementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Safety Requirement", Description = "Mengubah butir keselamatan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadSafetyRequirement", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateRadSafetyRequirementRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyRequirementService.UpdateAsync(id, request, cancellationToken),
                "Butir keselamatan berhasil diperbarui.");

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRequirementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Safety Requirement", Description = "Mengubah status aktif butir keselamatan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadSafetyRequirement", "Update")]
        public Task<IActionResult> SetStatus(
            Guid id,
            [FromBody] UpdateRadSafetyRequirementStatusRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(
                () => _radSafetyRequirementService.SetStatusAsync(
                    id, request.IsActive, cancellationToken),
                request.IsActive
                    ? "Butir keselamatan berhasil diaktifkan."
                    : "Butir keselamatan berhasil dinonaktifkan.");

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Delete", "Delete Rad Safety Requirement", Description = "Menandai butir keselamatan terhapus", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("RadSafetyRequirement", "Delete")]
        public Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyRequirementService.DeleteAsync(id, cancellationToken),
                "Butir keselamatan berhasil ditandai terhapus.");

        // Memetakan hasil tindakan menjadi status HTTP sesuai RAD-API-001 bagian 4.
        private async Task<IActionResult> Execute<T>(
            Func<Task<RadOperationResult<T>>> action,
            string successMessage)
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

                RadOperationResultKind.Forbidden =>
                    StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                        StatusCodes.Status403Forbidden,
                        result.ErrorMessage ?? "Anda tidak berwenang melakukan tindakan ini.",
                        new { Code = result.ErrorCode })),

                RadOperationResultKind.Conflict =>
                    Conflict(ApiResponse<object>.Fail(
                        StatusCodes.Status409Conflict,
                        result.ErrorMessage ?? "Tindakan bertabrakan dengan keadaan data saat ini.",
                        new { Code = result.ErrorCode })),

                RadOperationResultKind.BusinessRule or
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
    }
}
