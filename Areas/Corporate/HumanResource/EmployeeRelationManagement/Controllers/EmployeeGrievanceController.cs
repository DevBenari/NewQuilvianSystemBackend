using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_RELATION",
        moduleName: "Human Resource Employee Relation",
        displayName: "Keluhan Kerja & Whistleblowing",
        AreaName = "Corporate",
        ControllerName = "EmployeeGrievance",
        Description = "Tiket keluhan hubungan kerja, mediasi, dan kotak masuk whistleblower rahasia",
        SortOrder = 11)]
    [Tags("Corporate - HRD Grievance & Confidential Whistleblowing")]
    [Route("api/v1/hrd/grievances")]
    public class EmployeeGrievanceController : ControllerBase
    {
        private readonly IEmployeeGrievanceService _grievanceService;
        private readonly IWhistleblowerCaseService _whistleblowerService;

        public EmployeeGrievanceController(
            IEmployeeGrievanceService grievanceService,
            IWhistleblowerCaseService whistleblowerService)
        {
            _grievanceService = grievanceService ?? throw new ArgumentNullException(nameof(grievanceService));
            _whistleblowerService = whistleblowerService ?? throw new ArgumentNullException(nameof(whistleblowerService));
        }

        [HttpPost]
        [AccessAction("Submit", "Mengajukan Keluhan Hubungan Kerja", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("EmployeeGrievance", "Submit")]
        public async Task<IActionResult> SubmitGrievance([FromBody] SubmitGrievanceRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
                var result = await _grievanceService.SubmitGrievanceAsync(request, actorEmail, ResolveComplainant(), cancellationToken);
                return StatusCode(201, ApiResponse<GrievanceTicketResponseDto>.Ok(result, "Tiket keluhan kerja pegawai berhasil diajukan dengan target SLA 14 hari kerja."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GrievanceTicketResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpPost("whistleblower")]
        [AccessAction("SubmitWhistleblower", "Mengirim Laporan Whistleblower", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmployeeGrievance", "SubmitWhistleblower")]
        public async Task<IActionResult> SubmitWhistleblower([FromBody] SubmitWhistleblowerRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
                var result = await _whistleblowerService.SubmitWhistleblowerCaseAsync(request, actorEmail, cancellationToken);
                return StatusCode(201, ApiResponse<WhistleblowerTicketReceiptDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<WhistleblowerTicketReceiptDto>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<WhistleblowerTicketReceiptDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet("my-tickets")]
        [AccessAction("ViewOwn", "Melihat Tiket Keluhan Sendiri", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("EmployeeGrievance", "ViewOwn")]
        public async Task<IActionResult> GetMyTickets(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Remediasi kelompok G nomor 1 (10 November 2026): identitas pelapor dari token, parameter employeeId dihapus.
                var result = await _grievanceService.GetMyTicketsAsync(ResolveComplainant(), status, page, pageSize, cancellationToken);
                return Ok(ApiResponse<PagedResult<GrievanceTicketSummaryDto>>.Ok(result, "Daftar tiket keluhan kerja berhasil dimuat."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<GrievanceTicketSummaryDto>>.Fail(500, ex.Message));
            }
        }

        [HttpGet("compliance-inbox")]
        [AccessAction("WhistleblowerReview", "Membuka Kotak Masuk Whistleblower", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("EmployeeGrievance", "WhistleblowerReview")]
        public async Task<IActionResult> GetComplianceInbox(
            [FromQuery] string? category,
            [FromQuery] string? severityLevel,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _whistleblowerService.GetComplianceInboxAsync(category, severityLevel, status, page, pageSize, cancellationToken);
                return Ok(ApiResponse<PagedResult<WhistleblowerCaseDto>>.Ok(result, "Kotak masuk investigasi rahasia Kepatuhan berhasil dimuat."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<WhistleblowerCaseDto>>.Fail(500, ex.Message));
            }
        }

        [HttpPut("{grievanceId}/mediation")]
        [AccessAction("Mediate", "Mencatat Hasil Mediasi Keluhan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("EmployeeGrievance", "Mediate")]
        public async Task<IActionResult> RecordMediation(Guid grievanceId, [FromBody] RecordMediationResolutionRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
                var result = await _grievanceService.RecordMediationResolutionAsync(grievanceId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<GrievanceTicketResponseDto>.Ok(result, "Hasil risalah mediasi hubungan kerja berhasil diperbarui."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<GrievanceTicketResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GrievanceTicketResponseDto>.Fail(500, ex.Message));
            }
        }

        /// <summary>
        /// Nama pengguna yang sedang login untuk jejak aktor. Sengaja tidak memakai aktor cadangan:
        /// permintaan tanpa identitas ditolak, bukan dicatat atas nama orang lain.
        /// </summary>
        /// <summary>Identitas pelapor dari token (remediasi kelompok G, 10 November 2026): profil tenaga kerja, karyawan, dan akun.</summary>
        private GrievanceComplainantIdentity ResolveComplainant()
        {
            return new GrievanceComplainantIdentity(User.ResolveWorkforceProfileId(), User.GetEmployeeId(), User.RequireUserId());
        }

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
