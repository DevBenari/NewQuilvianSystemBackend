using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Constants.MedicalCheckupPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Controllers
{
    /// <summary>
    /// Katalog paket MCU — <c>MCU-API-v1</c> grup Mcu Package (<c>BE-MCU-005</c>).
    /// </summary>
    /// <remarks>
    /// Action sengaja tanpa <c>&lt;summary&gt;</c> XML: teksnya muncul di baris endpoint Swagger,
    /// dan pemilik modul menolaknya. <c>filters/metadata</c> dideklarasikan tepat sesudah
    /// konstruktor.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-checkup-management/packages")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Mcu Package",
        AreaName = P.AreaName,
        ControllerName = P.Package.Resource,
        Description = "Paket MCU beserta komponen, persyaratan, dan harga perusahaan",
        SortOrder = 1
    )]
    [Tags("Health Services / Medical Checkup Management / Mcu Package")]
    public class McuPackageController : ControllerBase
    {
        private readonly McuPackageService _service;
        private readonly LoggerService _logger;

        public McuPackageController(McuPackageService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<McuPackageFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Mcu Package", Description = "Melihat konfigurasi penyaring paket MCU", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Package.Resource, P.Read)]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<McuPackageFilterMetadataResponse>.Ok(
                McuPackageService.BuildFilterMetadata(), "Metadata filter paket MCU berhasil diambil."));

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<McuPackageListResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Mcu Package", Description = "Melihat daftar paket MCU", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Package.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] McuPackagePagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<McuPackageListResponse>>.Ok(
                await _service.GetPackagesAsync(query, cancellationToken), "Daftar paket MCU berhasil diambil."));

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<McuPackageOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Mcu Package", Description = "Melihat pilihan paket aktif beserta harga yang berlaku", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Package.Resource, P.Read)]
        public async Task<IActionResult> GetOptions([FromQuery] McuPackageOptionsQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<McuPackageOptionResponse>>.Ok(
                await _service.GetOptionsAsync(query, cancellationToken), "Pilihan paket MCU berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<McuPackageDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Mcu Package", Description = "Melihat rincian paket MCU", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Package.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPackageAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Paket MCU tidak ditemukan."))
                : Ok(ApiResponse<McuPackageDetailResponse>.Ok(result, "Rincian paket MCU berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<McuPackageDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Create, "Create Mcu Package", Description = "Membuat paket MCU", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Package.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateMcuPackageRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.CreatePackageAsync(request, McuHttp.ActorId(User), cancellationToken),
                "Paket MCU berhasil dibuat.", _logger, P.Package.Resource, P.Create,
                x => $"Paket MCU {x.PackageCode} dibuat dengan {x.Components.Count} komponen.");

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<McuPackageDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Update, "Update Mcu Package", Description = "Mengubah paket MCU, komponen, dan persyaratannya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Package.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMcuPackageRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.UpdatePackageAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Paket MCU berhasil diperbarui.", _logger, P.Package.Resource, P.Update,
                x => $"Paket MCU {x.PackageCode} diperbarui; {x.Components.Count} komponen.");

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<McuPackageStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Update, "Update Mcu Package", Description = "Mengaktifkan atau menonaktifkan paket MCU", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Package.Resource, P.Update)]
        public async Task<IActionResult> SetStatus(Guid id, [FromBody] McuPackageStatusRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.SetStatusAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                request.IsActive ? "Paket MCU berhasil diaktifkan." : "Paket MCU berhasil dinonaktifkan.",
                _logger, P.Package.Resource, P.Update,
                x => $"Paket MCU {x.PackageCode} {(x.IsActive ? "diaktifkan" : "dinonaktifkan")}.");

        [HttpPost("{id:guid}/company-prices")]
        [ProducesResponseType(typeof(ApiResponse<McuCompanyPriceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Update, "Update Mcu Package", Description = "Menambah harga perusahaan pada paket MCU", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Package.Resource, P.Update)]
        public async Task<IActionResult> AddCompanyPrice(Guid id, [FromBody] CreateMcuCompanyPriceRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.AddCompanyPriceAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Harga perusahaan berhasil ditambahkan.", _logger, P.Package.Resource, P.Update,
                x => $"Harga perusahaan {x.CompanyGuarantorName} ditambahkan pada paket {x.PackageId} " +
                     $"berlaku {McuPackageService.FormatPeriod(x.EffectiveStartDate, x.EffectiveEndDate)}.");

        [HttpPatch("{id:guid}/company-prices/{priceId:guid}/end")]
        [ProducesResponseType(typeof(ApiResponse<McuCompanyPriceEndResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Update, "Update Mcu Package", Description = "Mengakhiri masa berlaku harga perusahaan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Package.Resource, P.Update)]
        public async Task<IActionResult> EndCompanyPrice(Guid id, Guid priceId, [FromBody] EndMcuCompanyPriceRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.EndCompanyPriceAsync(id, priceId, request, McuHttp.ActorId(User), cancellationToken),
                "Masa berlaku harga perusahaan berhasil diakhiri.", _logger, P.Package.Resource, P.Update,
                x => $"Harga perusahaan {x.Id} pada paket {x.PackageId} diakhiri per {x.EffectiveEndDate:yyyy-MM-dd}.");
    }
}
