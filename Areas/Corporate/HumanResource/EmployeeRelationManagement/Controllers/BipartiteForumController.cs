using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_RELATION",
        moduleName: "Human Resource Employee Relation",
        displayName: "Forum Bipartit",
        AreaName = "Corporate",
        ControllerName = "BipartiteForum",
        Description = "Penjadwalan sidang, risalah, dan Perjanjian Bersama forum LKS Bipartit",
        SortOrder = 12)]
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
        [AccessAction("Schedule", "Menjadwalkan Sidang Bipartit", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("BipartiteForum", "Schedule")]
        public async Task<IActionResult> ScheduleMeeting([FromBody] ScheduleBipartiteMeetingRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
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
        [AccessAction("Read", "Melihat Arsip Forum Bipartit", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("BipartiteForum", "Read")]
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
        [AccessAction("RecordMinutes", "Mencatat Risalah Sidang Bipartit", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BipartiteForum", "RecordMinutes")]
        public async Task<IActionResult> RecordMinutes(Guid forumId, [FromBody] RecordMinutesRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
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
        [AccessAction("UploadAgreement", "Mengunggah Perjanjian Bersama", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BipartiteForum", "UploadAgreement")]
        public async Task<IActionResult> UploadJointAgreement(Guid forumId, [FromBody] UploadJointAgreementRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
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

        /// <summary>
        /// Nama pengguna yang sedang login untuk jejak aktor. Sengaja tidak memakai aktor cadangan:
        /// permintaan tanpa identitas ditolak, bukan dicatat atas nama orang lain.
        /// </summary>
        private string ResolveActorName()
        {
            var actorName = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(actorName))
            {
                throw new UnauthorizedAccessException("Identitas user pada token tidak valid.");
            }

            return actorName;
        }
    }
}
