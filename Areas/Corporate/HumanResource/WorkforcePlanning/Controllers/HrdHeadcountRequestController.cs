using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Controllers
{
    /// <summary>
    /// Controller pengelolaan permintaan penambahan tenaga kerja / headcount request (BE-HRD-040).
    /// Memenuhi kontrak API v7 Bagian 13 grup Headcount Request.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-planning/headcount-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PLANNING",
        moduleName: "Human Resource Workforce Planning",
        displayName: "Headcount Request",
        AreaName = "Corporate",
        ControllerName = "WorkforceHeadcountRequest",
        Description = "Pengelolaan permintaan penambahan pegawai, kursi formasi, dan persetujuan bertingkat",
        SortOrder = 36)]
    [Tags("Corporate / Human Resource / Workforce Planning / Headcount Request")]
    public sealed class HrdHeadcountRequestController : ControllerBase
    {
        private readonly IHrdHeadcountRequestService _service;
        private readonly IHrdHeadcountFulfilmentService _fulfilmentService;

        public HrdHeadcountRequestController(
            IHrdHeadcountRequestService service,
            IHrdHeadcountFulfilmentService fulfilmentService)
        {
            _service = service;
            _fulfilmentService = fulfilmentService;
        }

        /// <summary>
        /// Mengambil daftar permintaan penambahan tenaga kerja berhalaman dengan filter status, tipe, unit, dan jabatan.
        /// </summary>
        [HttpGet]
        [AccessAction("ReadList", "Read Headcount Requests", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHeadcountRequest", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HeadcountRequestListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] HeadcountRequestPagedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedListAsync(query, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengambil metadata opsi pilihan enumerasi untuk filter layar permintaan penambahan tenaga kerja.
        /// </summary>
        [HttpGet("filters/metadata")]
        [AccessAction("ReadFilterMetadata", "Read Headcount Request Filter Metadata", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkforceHeadcountRequest", "Read")]
        [ProducesResponseType(typeof(ApiResponse<HeadcountRequestFilterMetadataResponse>), StatusCodes.Status200OK)]
        public IActionResult GetFilterMetadata()
        {
            var metadata = _service.GetFilterMetadata();
            return Ok(ApiResponse<HeadcountRequestFilterMetadataResponse>.Ok(metadata, "Metadata filter berhasil diambil."));
        }

        /// <summary>
        /// Mengambil angka sisa kursi formasi yang tersedia pada unit dan jabatan tertentu pada tanggal acuan (kriteria 1).
        /// </summary>
        [HttpGet("remaining-seats")]
        [AccessAction("ReadRemainingSeats", "Read Remaining Headcount Seats", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("WorkforceHeadcountRequest", "Read")]
        [ProducesResponseType(typeof(ApiResponse<RemainingSeatsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRemainingSeats(
            [FromQuery] Guid hospitalSiteId,
            [FromQuery] Guid organizationUnitId,
            [FromQuery] Guid positionId,
            [FromQuery] DateOnly? asOfDate,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetRemainingSeatsAsync(hospitalSiteId, organizationUnitId, positionId, asOfDate, cancellationToken);
            return Ok(ApiResponse<RemainingSeatsResponse>.Ok(result, "Informasi sisa kursi berhasil dihitung."));
        }

        /// <summary>
        /// Mengambil detail satu permintaan penambahan tenaga kerja beserta seluruh alokasi kursi (kriteria 8).
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("ReadDetail", "Read Headcount Request Detail", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("WorkforceHeadcountRequest", "Read")]
        [ProducesResponseType(typeof(ApiResponse<HeadcountRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Membuat draf baru permintaan penambahan tenaga kerja.
        /// </summary>
        [HttpPost]
        [AccessAction("CreateDraft", "Create Headcount Request Draft", AccessType = AccessTypes.Create, SortOrder = 5)]
        [AccessPermission("WorkforceHeadcountRequest", "Create")]
        [ProducesResponseType(typeof(ApiResponse<HeadcountRequestResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateDraft(
            [FromBody] CreateHeadcountRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateDraftAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Memperbarui draf atau permintaan yang dikembalikan untuk revisi.
        /// </summary>
        [HttpPut("{id:guid}")]
        [AccessAction("UpdateDraft", "Update Headcount Request Draft", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceHeadcountRequest", "Update")]
        [ProducesResponseType(typeof(ApiResponse<HeadcountRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateDraft(
            [FromRoute] Guid id,
            [FromBody] UpdateHeadcountRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateDraftAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengajukan permintaan penambahan tenaga kerja ke alur persetujuan workflow T11 atau T12 (kriteria 2-5).
        /// Merekam potret gap pengajuan secara immutable dan memverifikasi ketersediaan sisa kursi formasi.
        /// </summary>
        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit Headcount Request for Approval", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("WorkforceHeadcountRequest", "Update")]
        [ProducesResponseType(typeof(ApiResponse<HeadcountRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Submit(
            [FromRoute] Guid id,
            [FromBody] SubmitHeadcountRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _service.SubmitAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Menarik kembali permintaan penambahan tenaga kerja sebelum disetujui penuh untuk rekrutmen (kriteria 6).
        /// </summary>
        [HttpPost("{id:guid}/withdraw")]
        [AccessAction("Withdraw", "Withdraw Headcount Request", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("WorkforceHeadcountRequest", "Update")]
        [ProducesResponseType(typeof(ApiResponse<HeadcountRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Withdraw(
            [FromRoute] Guid id,
            [FromBody] WithdrawHeadcountRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _service.WithdrawAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengoreksi estimasi biaya oleh peninjau Keuangan / Finance pada langkah persetujuan T12 (kriteria 7).
        /// </summary>
        [HttpPost("{id:guid}/finance-adjustment")]
        [AccessAction("FinanceAdjustment", "Adjust Headcount Request Cost by Finance", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("WorkforceHeadcountRequest", "Update")]
        [ProducesResponseType(typeof(ApiResponse<HeadcountRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinanceAdjustment(
            [FromRoute] Guid id,
            [FromBody] FinanceAdjustmentRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.FinanceAdjustmentAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengubah rencana pemenuhan sumber kursi (luar vs internal) oleh HR Manager yang bukan pembuat permintaan (BE-HRD-042).
        /// </summary>
        [HttpPost("{id:guid}/fulfilment-plan")]
        [AccessAction("ChangeFulfilmentPlan", "Change Headcount Fulfilment Plan", AccessType = AccessTypes.Update, SortOrder = 10)]
        [AccessPermission("WorkforceHeadcountRequest", "Update")]
        [ProducesResponseType(typeof(ApiResponse<FulfilmentPlanChangeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ChangeFulfilmentPlan(
            [FromRoute] Guid id,
            [FromBody] ChangeFulfilmentPlanRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.ChangeFulfilmentPlanAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Menautkan kursi pemenuhan ke penempatan pegawai baru (luar) atau SK mutasi (internal) (BE-HRD-043).
        /// </summary>
        [HttpPost("{id:guid}/seats/{seatId:guid}/fulfilment-link")]
        [AccessAction("CreateFulfilmentLink", "Create Headcount Seat Fulfilment Link", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission("WorkforceHeadcountRequest", "Update")]
        [ProducesResponseType(typeof(ApiResponse<FulfilmentLinkResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateFulfilmentLink(
            [FromRoute] Guid id,
            [FromRoute] Guid seatId,
            [FromBody] CreateFulfilmentLinkRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _fulfilmentService.CreateFulfilmentLinkAsync(id, seatId, request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Melepas tautan kursi pemenuhan sebelum tanggal mulai berlaku penempatan tiba (BE-HRD-043).
        /// </summary>
        [HttpDelete("{id:guid}/seats/{seatId:guid}/fulfilment-link")]
        [AccessAction("RemoveFulfilmentLink", "Remove Headcount Seat Fulfilment Link", AccessType = AccessTypes.Delete, SortOrder = 12)]
        [AccessPermission("WorkforceHeadcountRequest", "Delete")]
        [ProducesResponseType(typeof(ApiResponse<FulfilmentLinkResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RemoveFulfilmentLink(
            [FromRoute] Guid id,
            [FromRoute] Guid seatId,
            CancellationToken cancellationToken)
        {
            var result = await _fulfilmentService.RemoveFulfilmentLinkAsync(id, seatId, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengajukan pembatalan kursi formasi yang belum terisi ke alur persetujuan workflow T13 (BE-HRD-044).
        /// </summary>
        [HttpPost("{id:guid}/cancellations")]
        [AccessAction("RequestCancellation", "Request Headcount Seat Cancellation", AccessType = AccessTypes.Create, SortOrder = 13)]
        [AccessPermission("WorkforceHeadcountRequest", "Create")]
        [ProducesResponseType(typeof(ApiResponse<CancellationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RequestCancellation(
            [FromRoute] Guid id,
            [FromBody] CreateCancellationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.RequestCancellationAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(WorkforcePlanningResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
