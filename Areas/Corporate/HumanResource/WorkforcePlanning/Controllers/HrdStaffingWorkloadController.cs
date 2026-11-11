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
    /// Controller pencatatan dan pengelolaan angka beban kerja manual per unit dan jabatan (BE-HRD-034).
    /// Memenuhi kontrak API v7 Bagian 13 grup Staffing Workload.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-planning/staffing-workloads")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PLANNING",
        moduleName: "Human Resource Workforce Planning",
        displayName: "Staffing Workload",
        AreaName = "Corporate",
        ControllerName = "WorkforceStaffingWorkload",
        Description = "Pengelolaan angka beban kerja per unit dan jabatan",
        SortOrder = 33)]
    [Tags("Corporate / Human Resource / Workforce Planning / Staffing Workload")]
    public sealed class HrdStaffingWorkloadController : ControllerBase
    {
        private readonly IHrdStaffingWorkloadService _service;

        public HrdStaffingWorkloadController(IHrdStaffingWorkloadService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mengambil daftar angka beban kerja berhalaman dengan filter unit, jabatan, dan tanggal acuan periode.
        /// Kepala Unit hanya melihat unit dalam cakupan organisasinya (VR-WFP-38).
        /// </summary>
        [HttpGet]
        [AccessAction("ReadList", "Read Staffing Workloads", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceStaffingWorkload", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<StaffingWorkloadResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] StaffingWorkloadPagedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedListAsync(query, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengambil detail satu isian angka beban kerja berdasarkan ID.
        /// Jika di luar cakupan Kepala Unit, mengembalikan 404 (VR-WFP-38).
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("ReadDetail", "Read Staffing Workload Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkforceStaffingWorkload", "Read")]
        [ProducesResponseType(typeof(ApiResponse<StaffingWorkloadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mencatat isian angka beban kerja baru untuk satu periode (kriteria 1-5).
        /// Menolak sumber kosong (400 VR-WFP-08), isian ganda pada unit x jabatan x periode mulai (409 VR-WFP-09),
        /// dan akses di luar cakupan Kepala Unit (404 VR-WFP-38).
        /// </summary>
        [HttpPost]
        [AccessAction("Create", "Create Staffing Workload", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("WorkforceStaffingWorkload", "Create")]
        [ProducesResponseType(typeof(ApiResponse<StaffingWorkloadResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromBody] UpsertStaffingWorkloadRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Memperbarui isian angka beban kerja yang sudah ada.
        /// </summary>
        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Staffing Workload", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceStaffingWorkload", "Update")]
        [ProducesResponseType(typeof(ApiResponse<StaffingWorkloadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpsertStaffingWorkloadRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(id, request, GetCurrentUserId(), cancellationToken);
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
