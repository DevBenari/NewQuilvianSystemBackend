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
    /// Controller kalkulasi langsung dan potret historis kesenjangan tenaga kerja (BE-HRD-037, BE-HRD-039).
    /// Memenuhi kontrak API v7 Bagian 13 grup Staffing Gap.
    /// Potret gap bersifat abadi (immutable) tanpa endpoint mutasi (VR-WFP-39).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-planning/staffing-gaps")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PLANNING",
        moduleName: "Human Resource Workforce Planning",
        displayName: "Staffing Gap",
        AreaName = "Corporate",
        ControllerName = "WorkforceStaffingGap",
        Description = "Kalkulasi langsung dan potret historis kesenjangan formasi dan pegawai aktif per unit dan jabatan",
        SortOrder = 35)]
    [Tags("Corporate / Human Resource / Workforce Planning / Staffing Gap")]
    public sealed class HrdStaffingGapController : ControllerBase
    {
        private readonly IHrdStaffingGapCalculationService _calculationService;
        private readonly IHrdStaffingGapSnapshotService _snapshotService;

        public HrdStaffingGapController(
            IHrdStaffingGapCalculationService calculationService,
            IHrdStaffingGapSnapshotService snapshotService)
        {
            _calculationService = calculationService;
            _snapshotService = snapshotService;
        }

        /// <summary>
        /// Mengambil kalkulasi langsung kesenjangan tenaga kerja pada tanggal acuan per kombinasi unit kerja dan jabatan (BE-HRD-037).
        /// Menyajikan perbandingan formasi, terisi, lapisan gap, tiga angka gap, dan status kelengkapan data.
        /// </summary>
        [HttpGet]
        [AccessAction("ReadList", "Read Staffing Gaps", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceStaffingGap", "Read")]
        [ProducesResponseType(typeof(ApiResponse<StaffingGapResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStaffingGaps(
            [FromQuery] StaffingGapQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _calculationService.CalculateStaffingGapsAsync(
                query,
                GetCurrentUserId(),
                cancellationToken);

            return Ok(ApiResponse<StaffingGapResponse>.Ok(
                result,
                "Kalkulasi kesenjangan tenaga kerja langsung berhasil diambil."));
        }

        /// <summary>
        /// Mengambil daftar potret historis kesenjangan tenaga kerja berhalaman (BE-HRD-039).
        /// Mendukung filter jenis potret (Pengajuan/Bulanan), dokumen sumber, dan periode.
        /// </summary>
        [HttpGet("snapshots")]
        [AccessAction("ReadSnapshots", "Read Staffing Gap Snapshots", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkforceStaffingGap", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<StaffingGapSnapshotListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSnapshots(
            [FromQuery] StaffingGapSnapshotPagedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _snapshotService.GetPagedListAsync(
                query,
                GetCurrentUserId(),
                cancellationToken);

            return Ok(ApiResponse<PagedResult<StaffingGapSnapshotListItemResponse>>.Ok(
                result,
                "Daftar potret kesenjangan tenaga kerja berhasil diambil."));
        }

        /// <summary>
        /// Mengambil detail satu potret historis kesenjangan tenaga kerja beserta seluruh baris rinciannya (BE-HRD-039).
        /// </summary>
        [HttpGet("snapshots/{id:guid}")]
        [AccessAction("ReadSnapshotDetail", "Read Staffing Gap Snapshot Detail", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("WorkforceStaffingGap", "Read")]
        [ProducesResponseType(typeof(ApiResponse<StaffingGapSnapshotResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSnapshotById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _snapshotService.GetByIdAsync(
                id,
                GetCurrentUserId(),
                cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Potret kesenjangan tenaga kerja tidak ditemukan."));
            }

            return Ok(ApiResponse<StaffingGapSnapshotResponse>.Ok(
                result,
                "Detail potret kesenjangan tenaga kerja berhasil diambil."));
        }

        /// <summary>
        /// Merekam susulan potret kesenjangan bulanan untuk periode lampau yang belum tercatat (BE-HRD-048).
        /// Hasil potret diberi penanda ReconstructedAt.
        /// </summary>
        [HttpPost("snapshots/reconstruct")]
        [AccessAction("ReconstructSnapshot", "Reconstruct Staffing Gap Monthly Snapshot", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("WorkforceStaffingGap", "Create")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        public async Task<IActionResult> ReconstructSnapshot(
            [FromQuery] Guid hospitalSiteId,
            [FromQuery] DateOnly periodMonth,
            CancellationToken cancellationToken)
        {
            var snapshotId = await _snapshotService.ReconstructMonthlySnapshotAsync(
                hospitalSiteId,
                periodMonth,
                GetCurrentUserId(),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<Guid>.Ok(snapshotId, "Potret kesenjangan bulanan berhasil direkonstruksi."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
