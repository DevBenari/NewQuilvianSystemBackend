using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Controllers
{
    [ApiController]
    [Tags("Corporate - HRD Bipartite Negotiation & Labor Relations")]
    [Route("api/v1/hrd/bipartite/forums")]
    public class BipartiteForumController : ControllerBase
    {
        private readonly IBipartiteForumService _bipartiteService;

        public BipartiteForumController(IBipartiteForumService bipartiteService)
        {
            _bipartiteService = bipartiteService ?? throw new ArgumentNullException(nameof(bipartiteService));
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleMeeting([FromBody] ScheduleBipartiteMeetingRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _bipartiteService.ScheduleMeetingAsync(request, actorEmail, cancellationToken);
                return StatusCode(201, ApiResponse<BipartiteForumResponseDto>.Ok(result, "Jadwal sidang forum Bipartit RS berhasil didaftarkan secara sah."));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<BipartiteForumResponseDto>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BipartiteForumResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetForums(
            [FromQuery] string? disputeType,
            [FromQuery] string? status,
            [FromQuery] int? year,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _bipartiteService.GetBipartiteForumsAsync(disputeType, status, year, page, pageSize, cancellationToken);
                return Ok(ApiResponse<PagedResult<BipartiteForumResponseDto>>.Ok(result, "Daftar arsip sidang forum Bipartit berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<BipartiteForumResponseDto>>.Fail(500, ex.Message));
            }
        }

        [HttpPut("{forumId}/minutes")]
        public async Task<IActionResult> RecordMinutes(Guid forumId, [FromBody] RecordMinutesRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _bipartiteService.RecordMinutesAsync(forumId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<BipartiteForumResponseDto>.Ok(result, "Risalah notulen perundingan Bipartit berhasil dicatat."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BipartiteForumResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BipartiteForumResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpPost("{forumId}/joint-agreement")]
        public async Task<IActionResult> UploadJointAgreement(Guid forumId, [FromBody] UploadJointAgreementRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _bipartiteService.UploadJointAgreementAsync(forumId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<BipartiteForumResponseDto>.Ok(result, "Naskah Perjanjian Bersama (PB) resmi berhasil diunggah dan disahkan."));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<BipartiteForumResponseDto>.Fail(422, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BipartiteForumResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BipartiteForumResponseDto>.Fail(500, ex.Message));
            }
        }
    }
}
