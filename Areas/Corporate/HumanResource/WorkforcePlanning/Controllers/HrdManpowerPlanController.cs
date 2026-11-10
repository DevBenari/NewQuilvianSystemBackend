using System;
using System.Collections.Generic;
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
    /// Controller rencana kebutuhan tenaga tahunan dan addendum (BE-HRD-046).
    /// Memenuhi kontrak API v7 Bagian 13 grup Manpower Plan.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-planning/manpower-plans")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PLANNING",
        moduleName: "Human Resource Workforce Planning",
        displayName: "Manpower Plan",
        AreaName = "Corporate",
        ControllerName = "WorkforceManpowerPlan",
        Description = "Penyusunan rencana formasi tahunan rumah sakit, addendum, dan persetujuan bertingkat",
        SortOrder = 38)]
    [Tags("Corporate / Human Resource / Workforce Planning / Manpower Plan")]
    public sealed class HrdManpowerPlanController : ControllerBase
    {
        private readonly IHrdManpowerPlanService _service;

        public HrdManpowerPlanController(IHrdManpowerPlanService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mengambil daftar rencana kebutuhan tenaga kerja berhalaman dengan filter tahun, jenis rencana, dan status.
        /// </summary>
        [HttpGet]
        [AccessAction("ReadList", "Read Manpower Plans", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceManpowerPlan", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ManpowerPlanListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] ManpowerPlanPagedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedListAsync(query, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengambil metadata filter dan opsi pilihan enumerasi untuk layar rencana kebutuhan tenaga.
        /// </summary>
        [HttpGet("filters/metadata")]
        [AccessAction("ReadFilterMetadata", "Read Manpower Plan Filter Metadata", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkforceManpowerPlan", "Read")]
        [ProducesResponseType(typeof(ApiResponse<ManpowerPlanFilterMetadataResponse>), StatusCodes.Status200OK)]
        public IActionResult GetFilterMetadata()
        {
            var metadata = _service.GetFilterMetadata();
            return Ok(ApiResponse<ManpowerPlanFilterMetadataResponse>.Ok(metadata, "Metadata filter rencana berhasil diambil."));
        }

        /// <summary>
        /// Mengambil detail satu dokumen rencana kebutuhan tenaga atau addendum beserta seluruh baris jabatan.
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("ReadDetail", "Read Manpower Plan Detail", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("WorkforceManpowerPlan", "Read")]
        [ProducesResponseType(typeof(ApiResponse<ManpowerPlanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Membuat draf rencana kebutuhan tenaga tahunan baru (Annual Plan).
        /// Ditolak 409 jika sudah ada rencana tahunan untuk rumah sakit dan tahun yang sama (VR-WFP-15).
        /// </summary>
        [HttpPost]
        [AccessAction("CreateAnnualPlan", "Create Annual Manpower Plan", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("WorkforceManpowerPlan", "Create")]
        [ProducesResponseType(typeof(ApiResponse<ManpowerPlanResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateAnnualPlan(
            [FromBody] CreateAnnualPlanRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAnnualPlanAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Membuat addendum perubahan formasi dari rencana tahunan yang telah disetujui (VR-WFP-16).
        /// </summary>
        [HttpPost("addendums")]
        [AccessAction("CreateAddendum", "Create Manpower Plan Addendum", AccessType = AccessTypes.Create, SortOrder = 5)]
        [AccessPermission("WorkforceManpowerPlan", "Create")]
        [ProducesResponseType(typeof(ApiResponse<ManpowerPlanResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreatePlanAddendum(
            [FromBody] CreatePlanAddendumRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreatePlanAddendumAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Memperbarui draf rencana atau addendum.
        /// Ditolak 409 jika dokumen sudah Approved dengan arahan membuat addendum (VR-WFP-17).
        /// </summary>
        [HttpPut("{id:guid}")]
        [AccessAction("UpdateDraft", "Update Manpower Plan Draft", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceManpowerPlan", "Update")]
        [ProducesResponseType(typeof(ApiResponse<ManpowerPlanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateDraft(
            [FromRoute] Guid id,
            [FromBody] UpdateManpowerPlanRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateDraftAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Menghitung ulang formasi saat ini, angka rekomendasi, rincian formula, dan estimasi biaya (kriteria 4).
        /// </summary>
        [HttpPost("{id:guid}/recalculate")]
        [AccessAction("Recalculate", "Recalculate Manpower Plan", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("WorkforceManpowerPlan", "Update")]
        [ProducesResponseType(typeof(ApiResponse<ManpowerPlanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Recalculate(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.RecalculateAsync(id, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengajukan rencana kebutuhan tenaga atau addendum ke alur persetujuan workflow T10 (kriteria 5).
        /// </summary>
        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit Manpower Plan for Approval", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("WorkforceManpowerPlan", "Update")]
        [ProducesResponseType(typeof(ApiResponse<ManpowerPlanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Submit(
            [FromRoute] Guid id,
            [FromBody] SubmitManpowerPlanRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.SubmitAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengoreksi total biaya rencana oleh peninjau Keuangan / Finance pada langkah persetujuan T10 (kriteria 6).
        /// </summary>
        [HttpPost("{id:guid}/finance-adjustment")]
        [AccessAction("FinanceAdjustment", "Adjust Manpower Plan Cost by Finance", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("WorkforceManpowerPlan", "Update")]
        [ProducesResponseType(typeof(ApiResponse<ManpowerPlanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinanceAdjustment(
            [FromRoute] Guid id,
            [FromBody] FinancePlanAdjustmentRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.FinanceAdjustmentAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengambil daftar potret historis kesenjangan tenaga kerja yang menempel pada dokumen rencana ini (kriteria 7).
        /// </summary>
        [HttpGet("{id:guid}/gap-snapshots")]
        [AccessAction("ReadGapSnapshots", "Read Manpower Plan Gap Snapshots", AccessType = AccessTypes.Read, SortOrder = 10)]
        [AccessPermission("WorkforceManpowerPlan", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<StaffingGapSnapshotListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGapSnapshots(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetGapSnapshotsAsync(id, User, cancellationToken);
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
