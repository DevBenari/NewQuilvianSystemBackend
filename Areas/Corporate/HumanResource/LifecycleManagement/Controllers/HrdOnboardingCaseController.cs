using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/human-resource/onboarding-cases")]
    [Authorize]
    [Tags("HRD Onboarding Cases")]
    public class HrdOnboardingCaseController : ControllerBase
    {
        private readonly IHrdOnboardingCaseService _caseService;

        public HrdOnboardingCaseController(IHrdOnboardingCaseService caseService)
        {
            _caseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] OnboardingCaseQuery query, CancellationToken cancellationToken)
        {
            var result = await _caseService.GetPagedAsync(query, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _caseService.GetSummaryAsync(User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _caseService.GetByIdAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/first-day-attendance")]
        public async Task<IActionResult> RecordFirstDayAttendance(
            Guid id,
            [FromBody] RecordFirstDayAttendanceRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _caseService.RecordFirstDayAttendanceAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}/first-day-attendance")]
        public async Task<IActionResult> CancelFirstDayAttendance(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _caseService.CancelFirstDayAttendanceAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/items/{itemId}/complete")]
        [Consumes("multipart/form-data", "application/json")]
        public async Task<IActionResult> CompleteItem(
            Guid id,
            Guid itemId,
            [FromForm] CompleteChecklistItemRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _caseService.CompleteItemAsync(id, itemId, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/items/{itemId}/uncomplete")]
        public async Task<IActionResult> UncompleteItem(
            Guid id,
            Guid itemId,
            CancellationToken cancellationToken)
        {
            var result = await _caseService.UncompleteItemAsync(id, itemId, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/check-automatic-items")]
        public async Task<IActionResult> CheckAutomaticItems(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _caseService.CheckAutomaticItemsAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/coordinator")]
        public async Task<IActionResult> ReassignCoordinator(
            Guid id,
            [FromBody] ReassignCoordinatorRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _caseService.ReassignCoordinatorAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/declare-ready")]
        public async Task<IActionResult> DeclareReadyForWork(
            Guid id,
            [FromBody] DeclareReadyForWorkRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _caseService.DeclareReadyForWorkAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/cancel-join")]
        public async Task<IActionResult> CancelJoin(
            Guid id,
            [FromBody] CancelOnboardingJoinRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _caseService.CancelJoinAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}/items/{itemId}/evidence")]
        public async Task<IActionResult> DownloadItemEvidence(
            Guid id,
            Guid itemId,
            CancellationToken cancellationToken)
        {
            var (exists, stream, contentType, fileName) = await _caseService.DownloadItemEvidenceAsync(id, itemId, cancellationToken);
            if (!exists || stream == null)
            {
                return NotFound(new { message = "Berkas bukti tidak ditemukan atau belum diunggah." });
            }

            return File(stream, contentType ?? "application/octet-stream", fileName ?? "evidence.bin");
        }
    }
}
