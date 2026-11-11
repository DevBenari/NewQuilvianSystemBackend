using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    /// <summary>
    /// Controller pengajuan cuti mandiri ESS, kalkulasi half-day, pembatalan pre-leave, dan upload berkas surat sakit
    /// (BE-HRD-122, BE-HRD-123, BE-HRD-125, HRD-ASM-20, HRD-ASM-21, HRD-DEC-110, HRD-DEC-112).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/requests")]
    [Route("api/corporate/human-resource/leave/requests")]
    [Route("api/v1/leave/requests")]
    [Route("api/leave/requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Request ESS",
        AreaName = "Corporate",
        ControllerName = "LeaveRequestEss",
        Description = "Pengajuan cuti mandiri ESS, kalkulasi half-day, pembatalan pre-leave, dan upload berkas surat sakit",
        SortOrder = 9)]
    [Tags("HR Leave - Requests & ESS")]
    public class LeaveRequestEssController : ControllerBase
    {
        private readonly LeaveRequestService _leaveRequestService;
        private readonly LeaveCancellationService _cancellationService;
        private readonly MedicalCertificateVerificationService _medicalService;

        public LeaveRequestEssController(
            LeaveRequestService leaveRequestService,
            LeaveCancellationService cancellationService,
            MedicalCertificateVerificationService medicalService)
        {
            _leaveRequestService = leaveRequestService;
            _cancellationService = cancellationService;
            _medicalService = medicalService;
        }

        /// <summary>
        /// Daftar permohonan cuti aktif dan historis pegawai mandiri (ESS).
        /// </summary>
        [HttpGet("my")]
        [AccessAction("ViewSelf", "Melihat Permohonan Cuti Pribadi", Description = "Melihat riwayat cuti mandiri ESS", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRequest", "ViewSelf")]
        [ProducesResponseType(typeof(ApiResponse<LeaveRequestPagedResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRequests(
            [FromQuery] LeaveRequestQueryRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _leaveRequestService.GetPagedAsync(userId, request, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Detail rincian permohonan cuti, status verifikasi surat sakit, dan timeline.
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("View", "Melihat Detail Permohonan Cuti", Description = "Melihat rincian cuti tertentu", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LeaveRequest", "View")]
        [ProducesResponseType(typeof(ApiResponse<LeaveRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _leaveRequestService.GetByIdAsync(id, userId, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengajukan permohonan cuti mandiri ESS (mendukung half-day 0.5 hari).
        /// </summary>
        [HttpPost]
        [AccessAction("Create", "Mengajukan Permohonan Cuti", Description = "Mengajukan permohonan cuti mandiri", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("LeaveRequest", "Create")]
        [ProducesResponseType(typeof(ApiResponse<LeaveRequestActionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromBody] CreateLeaveRequestRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var createResult = await _leaveRequestService.CreateAsync(userId, request, cancellationToken);
            if (!createResult.Success || createResult.Data == null)
            {
                return ToActionResult(createResult);
            }

            // Langsung memproses submit jika valid
            var submitResult = await _leaveRequestService.SubmitAsync(
                createResult.Data.Id,
                userId,
                new SubmitLeaveRequestRequest { Comment = request.Reason },
                cancellationToken);

            if (!submitResult.Success)
            {
                return ToActionResult(submitResult);
            }

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<LeaveRequestActionResponse>.Ok(submitResult.Data, "Permohonan cuti mandiri berhasil diajukan dan sedang menunggu persetujuan."));
        }

        /// <summary>
        /// Pembatalan mandiri cuti yang telah disetujui sebelum hari pelaksanaan (Pre-Leave Cancel pulih 100%).
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        [AccessAction("CancelSelf", "Membatalkan Cuti Mandiri Pre-Leave", Description = "Membatalkan cuti sebelum hari-H dan memulihkan saldo 100%", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveRequest", "CancelSelf")]
        [ProducesResponseType(typeof(ApiResponse<LeaveLifecycleActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CancelPreLeave(
            Guid id,
            [FromBody] CancelLeaveRequestRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _cancellationService.CancelPreLeaveAsync(
                id,
                userId,
                request?.Reason ?? "Pembatalan mandiri cuti sebelum hari pelaksanaan",
                cancellationToken);

            return ToActionResult(result);
        }

        /// <summary>
        /// Mengunggah berkas Surat Keterangan Sakit dokter ber-SIP (wajib jika sakit >= 2 hari).
        /// </summary>
        [HttpPost("{id:guid}/medical-certificate")]
        [AccessAction("Create", "Mengunggah Surat Keterangan Sakit", Description = "Upload berkas surat dokter ber-SIP", AccessType = AccessTypes.Create, SortOrder = 5)]
        [AccessPermission("LeaveRequest", "Create")]
        [ProducesResponseType(typeof(ApiResponse<MedicalCertificateDetailDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMedicalCertificate(
            Guid id,
            [FromBody] UploadMedicalCertificateRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (request != null)
            {
                request.LeaveRequestId = id;
            }

            var result = await _medicalService.UploadAndAttachCertificateAsync(request!, userId, cancellationToken);
            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<MedicalCertificateDetailDto>.Ok(result.Data, result.Message)
                : ApiResponse<MedicalCertificateDetailDto>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Melihat rincian surat sakit (diagnosa dimaskir kecuali untuk Petugas K3RS).
        /// </summary>
        [HttpGet("{id:guid}/medical-certificate")]
        [AccessAction("View", "Melihat Surat Sakit", Description = "Melihat rincian surat sakit", AccessType = AccessTypes.Read, SortOrder = 6)]
        [AccessPermission("LeaveRequest", "View")]
        [ProducesResponseType(typeof(ApiResponse<MedicalCertificateDetailDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMedicalCertificate(
            Guid id,
            CancellationToken cancellationToken)
        {
            bool isK3Rs = User.IsInRole("K3RSMedis") || User.IsInRole("MedicalOfficer") || User.HasClaim("permission", "K3RSMedis:ViewDiagnosis");
            var result = await _medicalService.GetCertificateDetailAsync(id, isK3Rs, cancellationToken);

            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<MedicalCertificateDetailDto>.Ok(result.Data, result.Message)
                : ApiResponse<MedicalCertificateDetailDto>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }

        private IActionResult ToActionResult<T>(LeaveRequestServiceResult<T> result)
        {
            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<T>.Ok(result.Data, result.Message)
                    : ApiResponse<T>.Fail(result.StatusCode, result.Message));
        }
    }
}
