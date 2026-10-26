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
    /// Penyerahan hasil MCU — <c>MCU-API-v1</c> grup Mcu Delivery. <c>BE-MCU-033</c> memasang data cetak hasil;
    /// <c>BE-MCU-034</c> riwayat penyerahan, pemberitahuan koreksi, dan persetujuan pelepasan.
    /// </summary>
    /// <remarks>Action sengaja tanpa <c>&lt;summary&gt;</c> XML. Tujuan penyerahan dan alasan pencabutan tidak masuk log.</remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-checkup-management/orders/{id:guid}")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Mcu Delivery",
        AreaName = P.AreaName,
        ControllerName = P.Delivery.Resource,
        Description = "Penyerahan hasil MCU: data cetak hasil lengkap dan ringkasan kelayakan",
        SortOrder = 7
    )]
    [Tags("Health Services / Medical Checkup Management / Mcu Delivery")]
    public class McuDeliveryController : ControllerBase
    {
        private readonly McuDeliveryService _service;
        private readonly LoggerService _logger;

        public McuDeliveryController(McuDeliveryService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("result-document")]
        // 200: McuResultDocumentResponse (variant=full) atau McuFitnessSummaryDocumentResponse (variant=fitness-summary).
        [ProducesResponseType(typeof(ApiResponse<McuResultDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Read, "Read Mcu Delivery", Description = "Melihat data cetak hasil MCU lengkap atau ringkasan kelayakan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Delivery.Resource, P.Read)]
        public async Task<IActionResult> GetResultDocument(Guid id, [FromQuery] string? variant, CancellationToken cancellationToken = default) =>
            variant switch
            {
                McuDeliveryService.VariantFull =>
                    McuHttp.Map(this, await _service.GetFullDocumentAsync(id, cancellationToken), "Data cetak hasil MCU berhasil diambil."),
                McuDeliveryService.VariantFitnessSummary =>
                    McuHttp.Map(this, await _service.GetFitnessSummaryAsync(id, cancellationToken), "Ringkasan kelayakan MCU berhasil diambil."),
                _ => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest,
                    $"Pilih variant {McuDeliveryService.VariantFull} atau {McuDeliveryService.VariantFitnessSummary}."))
            };

        [HttpGet("deliveries")]
        [ProducesResponseType(typeof(ApiResponse<List<McuResultDeliveryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Mcu Delivery", Description = "Melihat data cetak hasil MCU lengkap atau ringkasan kelayakan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Delivery.Resource, P.Read)]
        public async Task<IActionResult> GetDeliveries(Guid id, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _service.GetDeliveriesAsync(id, cancellationToken), "Riwayat penyerahan hasil MCU berhasil diambil.");

        [HttpPost("deliveries")]
        [ProducesResponseType(typeof(ApiResponse<McuResultDeliveryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Mcu Delivery", Description = "Mencatat penyerahan hasil MCU dan pemberitahuan koreksi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Delivery.Resource, P.Create)]
        public async Task<IActionResult> CreateDelivery(Guid id, [FromBody] CreateMcuResultDeliveryRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.CreateDeliveryAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Penyerahan hasil MCU dicatat.", _logger, P.Delivery.Resource, P.Create,
                x => $"Penyerahan hasil MCU order {id} versi {x.ResumeVersionNumber}: {x.ContentTypeLabel} kepada {x.RecipientTypeLabel} lewat {x.ChannelLabel}.");

        [HttpPatch("deliveries/{deliveryId:guid}/correction-notice")]
        [ProducesResponseType(typeof(ApiResponse<McuResultDeliveryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Mcu Delivery", Description = "Mencatat penyerahan hasil MCU dan pemberitahuan koreksi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Delivery.Resource, P.Create)]
        public async Task<IActionResult> RecordCorrectionNotice(Guid id, Guid deliveryId, [FromBody] RecordCorrectionNoticeRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.RecordCorrectionNoticeAsync(id, deliveryId, request, McuHttp.ActorId(User), cancellationToken),
                "Pemberitahuan koreksi dicatat.", _logger, P.Delivery.Resource, P.Create,
                x => $"Pemberitahuan koreksi penyerahan versi {x.ResumeVersionNumber} order MCU {id} kepada {x.RecipientTypeLabel} dicatat.");

        [HttpGet("release-consents")]
        [ProducesResponseType(typeof(ApiResponse<List<McuReleaseConsentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Mcu Delivery", Description = "Melihat data cetak hasil MCU lengkap atau ringkasan kelayakan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Delivery.Resource, P.Read)]
        public async Task<IActionResult> GetReleaseConsents(Guid id, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _service.GetConsentsAsync(id, cancellationToken), "Persetujuan pelepasan berhasil diambil.");

        [HttpPost("release-consents")]
        [ProducesResponseType(typeof(ApiResponse<McuReleaseConsentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Delivery.ManageConsent, "Manage Mcu Release Consent", Description = "Mencatat dan mencabut persetujuan pelepasan hasil lengkap kepada perusahaan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Delivery.Resource, P.Delivery.ManageConsent)]
        public async Task<IActionResult> CreateReleaseConsent(Guid id, [FromBody] CreateMcuReleaseConsentRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.CreateConsentAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Persetujuan pelepasan dicatat.", _logger, P.Delivery.Resource, P.Delivery.ManageConsent,
                x => $"Persetujuan pelepasan hasil lengkap order MCU {id} kepada {x.CompanyGuarantorName} dicatat ({x.EvidenceTypeLabel}).");

        [HttpPatch("release-consents/{consentId:guid}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<McuReleaseConsentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Delivery.ManageConsent, "Manage Mcu Release Consent", Description = "Mencatat dan mencabut persetujuan pelepasan hasil lengkap kepada perusahaan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Delivery.Resource, P.Delivery.ManageConsent)]
        public async Task<IActionResult> RevokeReleaseConsent(Guid id, Guid consentId, [FromBody] RevokeMcuReleaseConsentRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.RevokeConsentAsync(id, consentId, request, McuHttp.ActorId(User), cancellationToken),
                "Persetujuan pelepasan dicabut.", _logger, P.Delivery.Resource, P.Delivery.ManageConsent,
                // Alasan pencabutan tidak dicatat.
                x => $"Persetujuan pelepasan hasil lengkap order MCU {id} kepada {x.CompanyGuarantorName} dicabut.");
    }
}
