using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/travel/requests")]
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
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var empClaim = User.FindFirst("EmployeeId")?.Value;

            Guid.TryParse(userClaim, out var userId);
            Guid.TryParse(empClaim, out var empId);

            return (userId, empId != Guid.Empty ? empId : userId);
        }

        /// <summary>
        /// Mengambil daftar pengajuan SPPD dinas pegawai/tim (Kontrak v25)
        /// </summary>
        [HttpGet]
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

        /// <summary>
        /// Mendaftarkan pengajuan perjalanan dinas baru (SPPD) via ESS (Kontrak v25)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRequest([FromBody] CreateBusinessTravelRequestDto dto)
        {
            try
            {
                var (userId, empId) = GetCurrentUserContext();
                var result = await _travelRequestService.CreateRequestAsync(dto, userId, empId);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<BusinessTravelResponseDto>.Ok(result, "Pengajuan SPPD berhasil didaftarkan"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BusinessTravelResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Memperbarui draf pengajuan SPPD sebelum diajukan resmi (Kontrak v25)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRequest([FromRoute] Guid id, [FromBody] UpdateBusinessTravelRequestDto dto)
        {
            try
            {
                var (userId, _) = GetCurrentUserContext();
                var result = await _travelRequestService.UpdateRequestAsync(id, dto, userId);
                return Ok(ApiResponse<BusinessTravelResponseDto>.Ok(result, "Draf pengajuan SPPD berhasil diperbarui"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BusinessTravelResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Membatalkan perjalanan dinas yang belum berjalan dengan alasan minimal 15 karakter (Kontrak v25)
        /// </summary>
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<BusinessTravelResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelRequest([FromRoute] Guid id, [FromBody] CancelTravelRequestDto dto)
        {
            try
            {
                var (userId, _) = GetCurrentUserContext();
                var result = await _travelRequestService.CancelRequestAsync(id, dto, userId);
                return Ok(ApiResponse<BusinessTravelResponseDto>.Ok(result, "Perjalanan dinas berhasil dibatalkan"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BusinessTravelResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Mengunduh lembar surat tugas dan SPPD resmi ber-barcode untuk verifikasi fisik (Kontrak v25)
        /// </summary>
        [HttpGet("{id}/document-pdf")]
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
