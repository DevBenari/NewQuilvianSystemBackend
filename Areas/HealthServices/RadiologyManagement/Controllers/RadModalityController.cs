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
    [Route("api/v1/health-services/radiology-management/master-data/rad-modalities")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_RADIOLOGY_MANAGEMENT",
        moduleName: "Health Service Radiology Management",
        displayName: "Rad Modality",
        AreaName = "HealthServices",
        ControllerName = "RadModality",
        Description = "Pengelolaan alat pencitraan radiologi",
        SortOrder = 4
    )]
    [Tags("Health Services / Radiology Management / Master Data / Rad Modality")]
    public class RadModalityController : ControllerBase
    {
        private readonly RadModalityService _radModalityService;

        public RadModalityController(RadModalityService radModalityService)
        {
            _radModalityService = radModalityService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RadModalityFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Modality", Description = "Melihat daftar pilihan penyaring alat pencitraan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadModality", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var hasil = _radModalityService.GetFilterMetadata();

            return Ok(ApiResponse<RadModalityFilterMetadataResponse>.Ok(
                hasil, "Metadata filter alat pencitraan berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RadModalitySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Modality", Description = "Melihat rekap alat pencitraan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadModality", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var hasil = await _radModalityService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<RadModalitySummaryResponse>.Ok(
                hasil, "Rekap alat pencitraan berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RadModalityResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Modality", Description = "Melihat daftar alat pencitraan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadModality", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? usesIonisingRadiation = null,
            [FromQuery] bool? supportsContrast = null,
            [FromQuery] bool? hasActiveSafetyRule = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radModalityService.GetPagedAsync(
                new RadModalityPagedQuery
                {
                    Search = search,
                    IsActive = isActive,
                    UsesIonisingRadiation = usesIonisingRadiation,
                    SupportsContrast = supportsContrast,
                    HasActiveSafetyRule = hasActiveSafetyRule,
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                },
                cancellationToken);

            return Ok(ApiResponse<PagedResult<RadModalityResponse>>.Ok(
                hasil, "Daftar alat pencitraan berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<RadModalityOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Modality", Description = "Mengambil pilihan alat pencitraan untuk dropdown", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadModality", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search = null,
            [FromQuery] bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radModalityService.GetOptionsAsync(
                search, onlyActive, cancellationToken);

            return Ok(ApiResponse<List<RadModalityOptionResponse>>.Ok(
                hasil, "Pilihan alat pencitraan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadModalityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Modality", Description = "Melihat rincian satu alat pencitraan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadModality", "Read")]
        public Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radModalityService.GetByIdAsync(id, cancellationToken),
                "Rincian alat pencitraan berhasil diambil.");

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RadModalityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Rad Modality", Description = "Mendaftarkan alat pencitraan baru", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadModality", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateRadModalityRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radModalityService.CreateAsync(request, cancellationToken),
                "Alat pencitraan berhasil didaftarkan.");

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadModalityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Modality", Description = "Mengubah data alat pencitraan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadModality", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateRadModalityRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radModalityService.UpdateAsync(id, request, cancellationToken),
                "Data alat pencitraan berhasil diperbarui.");

        // Menyalakan atau mematikan satu alat. Mematikan alat yang masih dipakai aturan
        // keselamatan berlaku ditolak 409 — aturannya wajib dihentikan lebih dulu lewat
        // pengesahan penanggung jawab klinis.
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<RadModalityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Modality", Description = "Mengubah status aktif alat pencitraan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadModality", "Update")]
        public Task<IActionResult> SetStatus(
            Guid id,
            [FromBody] UpdateRadModalityStatusRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radModalityService.SetStatusAsync(id, request.IsActive, cancellationToken),
                request.IsActive
                    ? "Alat pencitraan berhasil diaktifkan."
                    : "Alat pencitraan berhasil dinonaktifkan.");

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Delete", "Delete Rad Modality", Description = "Menandai alat pencitraan terhapus", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("RadModality", "Delete")]
        public Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radModalityService.DeleteAsync(id, cancellationToken),
                "Alat pencitraan berhasil ditandai terhapus.");

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
