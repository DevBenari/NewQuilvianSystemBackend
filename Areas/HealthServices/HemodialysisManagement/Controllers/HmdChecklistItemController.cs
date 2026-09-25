using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Constants.HemodialysisPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Controllers
{
    /// <summary>
    /// Master butir checklist Pra-HD beserta penanda boleh-dilewati (<c>BE-HMD-05</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>GET /</c> mengembalikan daftar utuh, bukan halaman — mengikuti kontrak
    /// <c>HMD-CONTRACT-v1</c>, karena butirnya sedikit dan urutannya adalah urutan pemeriksaan.
    /// </para>
    /// <para>
    /// <b>Sengaja tanpa <c>DELETE</c> maupun <c>PATCH /{id}/status</c>.</b> Menghapus atau
    /// menonaktifkan butir pengaman keselamatan sebelum tindakan adalah keputusan tata kelola
    /// klinis (<c>HMD-GATE-002</c>), dan matriks hak akses tidak menyediakan butir untuk itu.
    /// Penonaktifan tetap mungkin lewat <c>PUT /{id}</c> oleh pemegang
    /// <c>HemodialysisChecklistItem : Update</c>.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/master-data/hemodialysis-checklist-items")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Checklist Item",
        AreaName = P.AreaName,
        ControllerName = P.ChecklistItem.Resource,
        Description = "Master butir checklist Pra-HD",
        SortOrder = 18
    )]
    [Tags("Health Services / Hemodialysis Management / Master Data / Hemodialysis Checklist Item")]
    public class HmdChecklistItemController : ControllerBase
    {
        private readonly HmdResourceService _service;
        private readonly LoggerService _logger;

        public HmdChecklistItemController(HmdResourceService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<HmdChecklistItemFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Checklist Item", Description = "Melihat konfigurasi penyaring butir checklist Pra-HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ChecklistItem.Resource, P.Read)]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<HmdChecklistItemFilterMetadataResponse>.Ok(
                HmdResourceService.BuildChecklistItemFilterMetadata(), "Metadata filter butir checklist berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<HmdChecklistItemSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Checklist Item", Description = "Melihat ringkasan butir checklist Pra-HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ChecklistItem.Resource, P.Read)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<HmdChecklistItemSummaryResponse>.Ok(
                await _service.GetChecklistItemSummaryAsync(cancellationToken), "Ringkasan butir checklist berhasil diambil."));

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<HmdChecklistItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Checklist Item", Description = "Melihat daftar butir checklist Pra-HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ChecklistItem.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] HmdChecklistItemQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<HmdChecklistItemResponse>>.Ok(
                await _service.GetChecklistItemsAsync(query, cancellationToken), "Daftar butir checklist berhasil diambil."));

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<HmdChecklistItemOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Checklist Item", Description = "Melihat pilihan butir checklist Pra-HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ChecklistItem.Resource, P.Read)]
        public async Task<IActionResult> GetOptions([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<HmdChecklistItemOptionResponse>>.Ok(
                await _service.GetChecklistItemOptionsAsync(onlyActive, cancellationToken), "Pilihan butir checklist berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdChecklistItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Checklist Item", Description = "Melihat rincian butir checklist Pra-HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ChecklistItem.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetChecklistItemAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Butir checklist tidak ditemukan atau sudah dihapus."))
                : Ok(ApiResponse<HmdChecklistItemResponse>.Ok(result, "Rincian butir checklist berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<HmdChecklistItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Hemodialysis Checklist Item", Description = "Menambah butir checklist Pra-HD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.ChecklistItem.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateHmdChecklistItemRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateChecklistItemAsync(request, HmdHttp.ActorId(User), cancellationToken),
                "Butir checklist berhasil ditambahkan.", _logger, P.ChecklistItem.Resource, P.Create,
                x => new { x.Id, x.ItemCode });

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdChecklistItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Update, "Update Hemodialysis Checklist Item", Description = "Memperbarui butir checklist Pra-HD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.ChecklistItem.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHmdChecklistItemRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.UpdateChecklistItemAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Butir checklist berhasil diperbarui.", _logger, P.ChecklistItem.Resource, P.Update,
                x => new { x.Id, x.ItemCode, x.IsMandatory, x.IsActive });

        /// <summary>
        /// Menetapkan apakah butir boleh dilewati dokter. Hanya pemegang akun tata kelola klinis
        /// yang diberi butir hak akses ini lewat layar Akses Role.
        /// </summary>
        [HttpPatch("{id:guid}/overridable")]
        [ProducesResponseType(typeof(ApiResponse<HmdChecklistItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.ChecklistItem.SetOverridable, "Set Hemodialysis Checklist Item Overridable", Description = "Menetapkan butir checklist Pra-HD yang boleh dilewati dokter", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.ChecklistItem.Resource, P.ChecklistItem.SetOverridable)]
        public async Task<IActionResult> SetOverridable(Guid id, [FromBody] SetOverridableRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.SetChecklistItemOverridableAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Kebijakan pelewatan butir checklist berhasil diperbarui.", _logger, P.ChecklistItem.Resource, P.ChecklistItem.SetOverridable,
                x => new { x.Id, x.ItemCode, x.IsOverridable });
    }
}
