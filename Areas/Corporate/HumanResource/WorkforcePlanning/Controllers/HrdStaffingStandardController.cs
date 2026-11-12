using System;
using System.Security.Claims;
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
    /// Controller standar kebutuhan tenaga kerja rumah sakit, perhitungan rekomendasi, dan pengesahan T9 (BE-HRD-035).
    /// Memenuhi kontrak API v7 Bagian 13 grup Staffing Standard.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-planning/staffing-standards")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PLANNING",
        moduleName: "Human Resource Workforce Planning",
        displayName: "Staffing Standard",
        AreaName = "Corporate",
        ControllerName = "WorkforceStaffingStandard",
        Description = "Pengelolaan standar kebutuhan tenaga kerja, rekomendasi headcount, dan pengesahan T9",
        SortOrder = 34)]
    [Tags("Corporate / Human Resource / Workforce Planning / Staffing Standard")]
    public sealed class HrdStaffingStandardController : ControllerBase
    {
        private readonly IHrdStaffingStandardService _service;

        public HrdStaffingStandardController(IHrdStaffingStandardService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mengambil daftar standar kebutuhan tenaga kerja berhalaman dengan filter unit, status, tahun berlaku, dan pencarian teks.
        /// </summary>
        [HttpGet]
        [AccessAction("ReadList", "Read Staffing Standards", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceStaffingStandard", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<StaffingStandardListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] StaffingStandardPagedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedListAsync(query, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengambil metadata filter dan opsi pilihan enumerasi untuk layar standar kebutuhan tenaga.
        /// </summary>
        [HttpGet("filters/metadata")]
        [AccessAction("ReadFilterMetadata", "Read Staffing Standard Filter Metadata", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkforceStaffingStandard", "Read")]
        [ProducesResponseType(typeof(ApiResponse<StaffingStandardFilterMetadataResponse>), StatusCodes.Status200OK)]
        public IActionResult GetFilterMetadata()
        {
            var metadata = _service.GetFilterMetadata();
            return Ok(ApiResponse<StaffingStandardFilterMetadataResponse>.Ok(metadata, "Metadata filter standar kebutuhan tenaga berhasil diambil."));
        }

        /// <summary>
        /// Mengambil standar kebutuhan tenaga yang aktif berlaku untuk satu unit pada tanggal tertentu (kontrak baca Penjadwalan).
        /// </summary>
        [HttpGet("effective")]
        [AccessAction("ReadEffective", "Read Effective Staffing Standard", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("WorkforceStaffingStandard", "Read")]
        [ProducesResponseType(typeof(ApiResponse<StaffingStandardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEffective(
            [FromQuery] Guid organizationUnitId,
            [FromQuery] DateOnly date,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetEffectiveAsync(organizationUnitId, date, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengambil detail satu versi standar kebutuhan tenaga kerja beserta baris standar dan komposisi kualifikasi per baris.
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("ReadDetail", "Read Staffing Standard Detail", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("WorkforceStaffingStandard", "Read")]
        [ProducesResponseType(typeof(ApiResponse<StaffingStandardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Membuat draf versi baru standar kebutuhan tenaga untuk satu unit kerja (kriteria 1).
        /// Bila sudah ada versi aktif, baris standar disalin secara utuh sebagai titik awal perubahan.
        /// Menolak jika unit sudah memiliki draf terbuka (400 VR-WFP-01).
        /// </summary>
        [HttpPost]
        [AccessAction("CreateDraft", "Create Staffing Standard Draft", AccessType = AccessTypes.Create, SortOrder = 5)]
        [AccessPermission("WorkforceStaffingStandard", "Create")]
        [ProducesResponseType(typeof(ApiResponse<StaffingStandardResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateDraft(
            [FromBody] CreateStaffingStandardRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateDraftAsync(request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Memperbarui rincian baris draf standar kebutuhan tenaga kerja (kriteria 2).
        /// Menolak WorkloadAnalysis (VR-WFP-02), ReliefFactor &lt; 1 (VR-WFP-05), duplikasi jabatan (VR-WFP-07),
        /// dan komposisi skill-mix yang tidak mencapai 100% (VR-WFP-10).
        /// </summary>
        [HttpPut("{id:guid}")]
        [AccessAction("UpdateDraft", "Update Staffing Standard Draft", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceStaffingStandard", "Update")]
        [ProducesResponseType(typeof(ApiResponse<StaffingStandardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDraft(
            [FromRoute] Guid id,
            [FromBody] UpdateStaffingStandardRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateDraftAsync(id, request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Menghitung angka rekomendasi kebutuhan tenaga kerja per baris berdasarkan rumus Bagian 16.9 (kriteria 4).
        /// Mengembalikan teks rincian perhitungan dan daftar masukan yang belum lengkap (VR-WFP-104).
        /// </summary>
        [HttpGet("{id:guid}/recommendation")]
        [AccessAction("ReadRecommendation", "Read Staffing Recommendation", AccessType = AccessTypes.Read, SortOrder = 7)]
        [AccessPermission("WorkforceStaffingStandard", "Read")]
        [ProducesResponseType(typeof(ApiResponse<StaffingRecommendationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRecommendation(
            [FromRoute] Guid id,
            [FromQuery] DateOnly periodStartDate,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetRecommendationAsync(id, periodStartDate, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengajukan standar kebutuhan tenaga kerja ke alur persetujuan workflow T9 (kriteria 3).
        /// Validasi kelengkapan isian per metode (VR-WFP-03, VR-WFP-04) dan tanggal berlaku tidak mundur (VR-WFP-06).
        /// </summary>
        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit Staffing Standard for Approval", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("WorkforceStaffingStandard", "Update")]
        [ProducesResponseType(typeof(ApiResponse<StaffingStandardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Submit(
            [FromRoute] Guid id,
            [FromBody] SubmitStaffingStandardRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.SubmitAsync(id, request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Membatalkan draf atau dokumen standar kebutuhan tenaga yang dikembalikan dari workflow.
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Staffing Standard", AccessType = AccessTypes.Delete, SortOrder = 9)]
        [AccessPermission("WorkforceStaffingStandard", "Delete")]
        [ProducesResponseType(typeof(ApiResponse<StaffingStandardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(
            [FromRoute] Guid id,
            [FromBody] CancelStaffingStandardRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(WorkforcePlanningResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
