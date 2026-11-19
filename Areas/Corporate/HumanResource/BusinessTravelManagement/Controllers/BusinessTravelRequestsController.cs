using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/travel/requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_BUSINESS_TRAVEL",
        moduleName: "Human Resource Business Travel",
        displayName: "Permohonan Perjalanan Dinas (SPPD)",
        AreaName = "Corporate",
        ControllerName = "BusinessTravelRequest",
        Description = "Pengajuan, perubahan draf, pembatalan, dan dokumen SPPD",
        SortOrder = 10)]
    [Tags("Corporate - HRD Business Travel Requests")]
    public class BusinessTravelRequestsController : ControllerBase
    {
        private readonly IBusinessTravelRequestService _travelRequestService;
        private readonly ITravelDocumentGenerationService _documentService;

        public BusinessTravelRequestsController(
            IBusinessTravelRequestService travelRequestService,
            ITravelDocumentGenerationService documentService)
        {
            _travelRequestService = travelRequestService;
            _documentService = documentService;
        }

        private (Guid UserId, Guid EmployeeId) GetCurrentUserContext()
        {
            // Kolom WorkforceProfileId pada data perjalanan dinas diisi dari klaim workforce_profile_id → employee_id → ID akun
            // (remediasi kelompok G, 10 November 2026).
            return (User.GetUserId(), User.ResolveWorkforceProfileId());
        }

        /// <summary>
        /// Mengambil daftar pengajuan SPPD dinas pegawai/tim (Kontrak v25)
        /// </summary>
        [HttpGet]
        [AccessAction("Read", "Melihat Riwayat SPPD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BusinessTravelRequest", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<BusinessTravelResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRequests(
            [FromQuery] string? status,
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] Guid? departmentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (userId, empId) = GetCurrentUserContext();
            var (items, totalCount) = await _travelRequestService.GetRequestsAsync(
                null, status, startDate, endDate, departmentId, pageNumber, pageSize);

            return Ok(ApiResponse<List<BusinessTravelResponseDto>>.Ok(items, $"Total {totalCount} data SPPD ditemukan"));
        }

        /// <summary>
        /// Mengambil detail lengkap SPPD, agenda rute, estimasi biaya, dan status approval (Kontrak v25)
        /// </summary>
        [HttpGet("{id}")]
        [AccessAction("Read", "Melihat Riwayat SPPD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BusinessTravelRequest", "Read")]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelDetailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRequestById([FromRoute] Guid id)
        {
            try
            {
                var result = await _travelRequestService.GetRequestByIdAsync(id);
                return Ok(ApiResponse<BusinessTravelDetailResponseDto>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BusinessTravelDetailResponseDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        [HttpPost]
        [AccessAction("Create", "Mengajukan Perjalanan Dinas", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BusinessTravelRequest", "Create")]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelDetailResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelDetailResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRequest([FromBody] CreateBusinessTravelRequestDto dto)
        {
            try
            {
                var (userId, empId) = GetCurrentUserContext();
                var result = await _travelRequestService.CreateRequestAsync(dto, userId, empId);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<BusinessTravelDetailResponseDto>.Ok(result, "SPPD disimpan sebagai Draf. Tekan Ajukan untuk mengirim ke atasan."));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<BusinessTravelDetailResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPut("{id}")]
        [AccessAction("Update", "Mengubah Draf SPPD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BusinessTravelRequest", "Update")]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateRequest([FromRoute] Guid id, [FromBody] UpdateBusinessTravelRequestDto dto)
        {
            try
            {
                var (userId, _) = GetCurrentUserContext();
                var result = await _travelRequestService.UpdateRequestAsync(id, dto, userId);
                return Ok(ApiResponse<BusinessTravelResponseDto>.Ok(result, "Draf pengajuan SPPD berhasil diperbarui"));
            }
            catch (BusinessTravelRequestException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<BusinessTravelResponseDto>.Fail(ex.StatusCode, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BusinessTravelResponseDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<BusinessTravelResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPost("{id}/submit")]
        [AccessAction("Create", "Mengajukan Perjalanan Dinas", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BusinessTravelRequest", "Create")]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelDetailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelDetailResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelDetailResponseDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelDetailResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelDetailResponseDto>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SubmitRequest([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var (userId, empId) = GetCurrentUserContext();
                var result = await _travelRequestService.SubmitRequestAsync(id, userId, empId, cancellationToken);
                return Ok(ApiResponse<BusinessTravelDetailResponseDto>.Ok(result, "SPPD berhasil diajukan dan menunggu persetujuan."));
            }
            catch (BusinessTravelRequestException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<BusinessTravelDetailResponseDto>.Fail(ex.StatusCode, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BusinessTravelDetailResponseDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        [HttpPut("{id}/cancel")]
        [AccessAction("Cancel", "Membatalkan Perjalanan Dinas", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BusinessTravelRequest", "Cancel")]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CancelRequest([FromRoute] Guid id, [FromBody] CancelTravelRequestDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var (userId, _) = GetCurrentUserContext();
                var result = await _travelRequestService.CancelRequestAsync(id, dto, userId, cancellationToken);
                return Ok(ApiResponse<BusinessTravelResponseDto>.Ok(result, "Perjalanan dinas berhasil dibatalkan"));
            }
            catch (BusinessTravelRequestException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<BusinessTravelResponseDto>.Fail(ex.StatusCode, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BusinessTravelResponseDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<BusinessTravelResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Mengunduh lembar surat tugas dan SPPD resmi ber-barcode untuk verifikasi fisik (Kontrak v25)
        /// </summary>
        [HttpGet("{id}/document-pdf")]
        [AccessAction("Read", "Melihat Riwayat SPPD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BusinessTravelRequest", "Read")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadDocumentPdf([FromRoute] Guid id)
        {
            try
            {
                var (fileBytes, fileName, contentType) = await _documentService.GenerateTravelOrderPdfAsync(id);
                return File(fileBytes, contentType, fileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }
    }
}
