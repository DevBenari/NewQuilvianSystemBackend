using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Constants.OpticalPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Controllers
{
    /// <summary>
    /// Data referensi Optik — <c>OPT-API-v1</c> grup Opt Reference Data (<c>BE-OPT-005</c>): daftar pilihan berlabel
    /// Indonesia, katalog barang Optik beserta stok depo Optik, profil atribut barang, dan pengaturan unit Optik.
    /// </summary>
    /// <remarks>
    /// Satu resource <c>OptReferenceData</c> dengan tiga action: <c>Read</c>, <c>Update</c> (profil barang), dan
    /// <c>UpdateSetting</c> (pengaturan, Admin Optik). Action sengaja tanpa <c>&lt;summary&gt;</c> XML; metadata ditaruh
    /// paling atas.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/optical-management/reference-data")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Opt Reference Data",
        AreaName = P.AreaName,
        ControllerName = P.ReferenceData.Resource,
        Description = "Daftar pilihan, katalog dan profil barang Optik, serta pengaturan unit Optik",
        SortOrder = 1
    )]
    [Tags("Health Services / Optical Management / Opt Reference Data")]
    public class OptReferenceDataController : ControllerBase
    {
        private readonly OptReferenceDataService _service;
        private readonly LoggerService _logger;

        public OptReferenceDataController(OptReferenceDataService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("metadata")]
        [ProducesResponseType(typeof(ApiResponse<OptMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Opt Reference Data", Description = "Melihat daftar pilihan berlabel modul Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ReferenceData.Resource, P.Read)]
        public IActionResult GetMetadata() =>
            Ok(ApiResponse<OptMetadataResponse>.Ok(OptReferenceDataService.GetMetadata(), "Daftar pilihan Optik berhasil diambil."));

        [HttpGet("item-catalog")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OptItemProfileResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Opt Reference Data", Description = "Melihat katalog barang Optik beserta stok depo Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ReferenceData.Resource, P.Read)]
        public async Task<IActionResult> GetItemCatalog([FromQuery] OptItemCatalogQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<OptItemProfileResponse>>.Ok(
                await _service.GetItemCatalogAsync(query, cancellationToken), "Katalog barang Optik berhasil diambil."));

        [HttpPut("item-profiles/{drugId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OptItemProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Update, "Update Opt Reference Data", Description = "Membuat atau mengubah profil Optik untuk satu barang", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.ReferenceData.Resource, P.Update)]
        public async Task<IActionResult> UpsertItemProfile(Guid drugId, [FromBody] OptItemProfileUpsertRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.UpsertItemProfileAsync(drugId, request, OptHttp.ActorId(User), cancellationToken),
                "Profil barang Optik berhasil disimpan.", _logger, P.ReferenceData.Resource, P.Update,
                x => $"Profil Optik barang {x.DrugCode} disimpan sebagai {x.ItemCategoryLabel}; {(x.IsActive ? "aktif" : "nonaktif")}.");

        [HttpGet("settings")]
        [ProducesResponseType(typeof(ApiResponse<OptSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Opt Reference Data", Description = "Melihat pengaturan unit Optik beserta riwayat perubahannya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ReferenceData.Resource, P.Read)]
        public async Task<IActionResult> GetSetting([FromQuery] Guid? serviceUnitId, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetSettingAsync(serviceUnitId, cancellationToken), "Pengaturan Optik berhasil diambil.");

        [HttpPut("settings")]
        [ProducesResponseType(typeof(ApiResponse<OptSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.ReferenceData.UpdateSetting, "Update Opt Setting", Description = "Mengubah pengaturan unit Optik (berlaku untuk dokumen baru)", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.ReferenceData.Resource, P.ReferenceData.UpdateSetting)]
        public async Task<IActionResult> UpdateSetting([FromBody] OptSettingUpdateRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.UpdateSettingAsync(request, OptHttp.ActorId(User), cancellationToken),
                "Pengaturan Optik berhasil diperbarui.", _logger, P.ReferenceData.Resource, P.ReferenceData.UpdateSetting,
                x => $"Pengaturan Optik unit {x.ServiceUnitName} diperbarui ke versi {x.Version}.");
    }
}
