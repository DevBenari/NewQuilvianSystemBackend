using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/radiology-management/master-data/rad-safety-rules")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_RADIOLOGY_MANAGEMENT",
        moduleName: "Health Service Radiology Management",
        displayName: "Rad Safety Rule",
        AreaName = "HealthServices",
        ControllerName = "RadSafetyRule",
        Description = "Pengelolaan aturan keselamatan radiologi beserta pengesahannya",
        SortOrder = 3
    )]
    [Tags("Health Services / Radiology Management / Master Data / Rad Safety Rule")]
    public class RadSafetyRuleController : ControllerBase
    {
        private readonly RadSafetyPolicyService _radSafetyPolicyService;

        public RadSafetyRuleController(RadSafetyPolicyService radSafetyPolicyService)
        {
            _radSafetyPolicyService = radSafetyPolicyService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRuleFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Safety Rule", Description = "Melihat daftar pilihan penyaring aturan keselamatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRule", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var hasil = _radSafetyPolicyService.GetFilterMetadata();

            return Ok(ApiResponse<RadSafetyRuleFilterMetadataResponse>.Ok(
                hasil, "Metadata filter aturan keselamatan berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRuleSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Safety Rule", Description = "Melihat rekap aturan keselamatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRule", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var hasil = await _radSafetyPolicyService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<RadSafetyRuleSummaryResponse>.Ok(
                hasil, "Rekap aturan keselamatan berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RadSafetyRuleResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Safety Rule", Description = "Melihat daftar aturan keselamatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRule", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? search = null,
            [FromQuery] Guid? modalityId = null,
            [FromQuery] Guid? safetyRequirementId = null,
            [FromQuery] RadSafetyRuleStatus? ruleStatus = null,
            [FromQuery] bool? isMandatory = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radSafetyPolicyService.GetPagedAsync(
                new RadSafetyRulePagedQuery
                {
                    Search = search,
                    ModalityId = modalityId,
                    SafetyRequirementId = safetyRequirementId,
                    RuleStatus = ruleStatus,
                    IsMandatory = isMandatory,
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                },
                cancellationToken);

            return Ok(ApiResponse<PagedResult<RadSafetyRuleResponse>>.Ok(
                hasil, "Daftar aturan keselamatan berhasil diambil."));
        }

        // Daftar alat yang BELUM punya aturan keselamatan berlaku.
        //
        // Daftar kosong berarti seluruh alat siap dipakai. Setiap baris yang muncul adalah alat
        // yang akan menolak seluruh pemeriksaannya hari ini, karena gerbang bersifat
        // fail-closed — RJ-BIL-DEC-014.
        [HttpGet("coverage")]
        [ProducesResponseType(typeof(ApiResponse<List<RadModalityCoverageResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Safety Rule", Description = "Memeriksa alat yang belum punya aturan keselamatan berlaku", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRule", "Read")]
        public async Task<IActionResult> GetCoverage(CancellationToken cancellationToken = default)
        {
            var hasil = await _radSafetyPolicyService.GetCoverageAsync(cancellationToken);

            var pesan = hasil.Count == 0
                ? "Seluruh alat pencitraan sudah punya aturan keselamatan yang berlaku."
                : $"{hasil.Count} alat pencitraan belum punya aturan keselamatan yang berlaku.";

            return Ok(ApiResponse<List<RadModalityCoverageResponse>>.Ok(hasil, pesan));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Safety Rule", Description = "Melihat rincian satu aturan keselamatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadSafetyRule", "Read")]
        public Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyPolicyService.GetByIdAsync(id, cancellationToken),
                "Rincian aturan keselamatan berhasil diambil.");

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Rad Safety Rule", Description = "Menyusun draf aturan keselamatan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadSafetyRule", "Create")]
        public Task<IActionResult> CreateDraft(
            [FromBody] CreateRadSafetyRuleRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyPolicyService.CreateDraftAsync(request, cancellationToken),
                "Draf aturan keselamatan berhasil disusun.");

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Rad Safety Rule", Description = "Mengubah draf aturan keselamatan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadSafetyRule", "Update")]
        public Task<IActionResult> UpdateDraft(
            Guid id,
            [FromBody] UpdateRadSafetyRuleRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyPolicyService.UpdateDraftAsync(id, request, cancellationToken),
                "Draf aturan keselamatan berhasil diperbarui.");

        [HttpPost("{id:guid}/submit")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Submit", "Submit Rad Safety Rule", Description = "Mengajukan aturan keselamatan untuk disahkan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RadSafetyRule", "Submit")]
        public Task<IActionResult> Submit(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyPolicyService.SubmitAsync(id, cancellationToken),
                "Aturan keselamatan berhasil diajukan untuk disahkan.");

        // Pengesahan. Penanda hak akses di bawah ini sekaligus menentukan siapa yang dihitung
        // sebagai penanggung jawab klinis — RAD-DEC-015. Service memeriksa satu hal lagi yang
        // tidak dapat dijawab sistem izin: penyusun dan pengaju tidak boleh mengesahkan
        // aturannya sendiri.
        [HttpPost("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Approve", "Approve Rad Safety Rule", Description = "Mengesahkan aturan keselamatan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RadSafetyRule", "Approve")]
        public Task<IActionResult> Approve(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyPolicyService.ApproveAsync(id, cancellationToken),
                "Aturan keselamatan berhasil disahkan dan mulai berlaku.");

        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Reject", "Reject Rad Safety Rule", Description = "Menolak pengajuan aturan keselamatan", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("RadSafetyRule", "Reject")]
        public Task<IActionResult> Reject(
            Guid id,
            [FromBody] RadSafetyRuleRejectRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyPolicyService.RejectAsync(id, request, cancellationToken),
                "Pengajuan aturan keselamatan berhasil ditolak.");

        [HttpPost("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<RadSafetyRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Deactivate", "Deactivate Rad Safety Rule", Description = "Menghentikan aturan keselamatan yang berlaku", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("RadSafetyRule", "Deactivate")]
        public Task<IActionResult> Deactivate(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radSafetyPolicyService.DeactivateAsync(id, cancellationToken),
                "Aturan keselamatan berhasil dihentikan.");

        // Memetakan hasil tindakan menjadi status HTTP sesuai RAD-API-001 bagian 4.
        //
        // Forbidden menjadi 403 dan bukan 400: tidak ada perbaikan isian yang dapat menolong,
        // yang salah adalah siapa yang menekan tombolnya. BusinessRule menjadi 422: bentuk
        // permintaannya benar, keadaan data yang dirujuknya yang menolak.
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
