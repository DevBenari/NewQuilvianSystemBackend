using System;
using System.Collections.Generic;
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
    /// Controller buku formasi jabatan berversi (BE-HRD-032).
    /// Endpoint baca saja untuk melihat formasi yang berlaku pada suatu tanggal dan riwayat perubahannya.
    /// Seluruh operasi penulisan langsung (POST, PUT, PATCH, DELETE) ditolak dengan 405 Method Not Allowed (VR-WFP-10).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-planning/position-formations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PLANNING",
        moduleName: "Human Resource Workforce Planning",
        displayName: "Workforce Position Formation",
        AreaName = "Corporate",
        ControllerName = "WorkforcePositionFormation",
        Description = "Buku formasi jabatan berversi dan riwayat dokumen sumber",
        SortOrder = 20)]
    [Tags("Corporate / Human Resource / Workforce Planning / Position Formation")]
    public sealed class HrdPositionFormationController : ControllerBase
    {
        private readonly IHrdPositionFormationService _service;

        public HrdPositionFormationController(IHrdPositionFormationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mengambil formasi yang sah berlaku per unit × jabatan pada tanggal tertentu (default hari ini).
        /// Mendukung rule FORMATION_CARRIES_FORWARD (BE-HRD-032 kriteria 1).
        /// </summary>
        [HttpGet]
        [AccessAction("Read", "Read Workforce Position Formation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePositionFormation", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PositionFormationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get([FromQuery] PositionFormationPagedQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.GetEffectiveFormationsAsync(query, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengambil seluruh riwayat versi formasi satu unit × jabatan beserta dokumen sumber, penyetuju, dan waktu persetujuan (BE-HRD-032 kriteria 2).
        /// </summary>
        [HttpGet("history")]
        [AccessAction("Read", "Read Workforce Position Formation History", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkforcePositionFormation", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<PositionFormationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistory(
            [FromQuery] Guid organizationUnitId,
            [FromQuery] Guid positionId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetFormationHistoryAsync(organizationUnitId, positionId, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Penolakan mutlak atas pengubahan formasi langsung (VR-WFP-10 / AT-HRD-D1-06 / BE-HRD-032 kriteria 3).
        /// Formasi tidak dapat diubah langsung lewat endpoint mana pun.
        /// </summary>
        [HttpPost("")]
        [HttpPost("{*path}")]
        [HttpPut("")]
        [HttpPut("{*path}")]
        [HttpPatch("")]
        [HttpPatch("{*path}")]
        [HttpDelete("")]
        [HttpDelete("{*path}")]
        [AccessAction("Read", "Workforce Position Formation Direct Write Prohibited", AccessType = AccessTypes.Read, SortOrder = 99)]
        [AccessPermission("WorkforcePositionFormation", "Read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status405MethodNotAllowed)]
        public IActionResult DirectWriteNotAllowed()
        {
            return StatusCode(StatusCodes.Status405MethodNotAllowed,
                ApiResponse<object>.Fail(StatusCodes.Status405MethodNotAllowed,
                    "Formasi tidak dapat diubah langsung. Ajukan perubahan lewat rencana tahunan atau permintaan headcount."));
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
