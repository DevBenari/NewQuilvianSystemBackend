using System;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/corporate/human-resource/employment-contracts")]
    [Produces(MediaTypeNames.Application.Json)]
    [Tags("Corporate / Human Resource / Lifecycle Management / Employment Contracts")]
    public class HrdEmploymentContractController : ControllerBase
    {
        private readonly IHrdEmploymentContractService _contractService;

        public HrdEmploymentContractController(IHrdEmploymentContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmploymentContractResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employment Contracts", Description = "Melihat daftar kontrak kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentContracts", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] EmploymentContractQuery query, CancellationToken cancellationToken)
        {
            var result = await _contractService.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentContractResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Employment Contract Detail", Description = "Melihat detail kontrak kerja", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("EmploymentContracts", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _contractService.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/print-data")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentContractPrintDataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Employment Contract Print Data", Description = "Melihat data cetak kontrak kerja", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("EmploymentContracts", "Read")]
        public async Task<IActionResult> GetPrintData(Guid id, CancellationToken cancellationToken)
        {
            var result = await _contractService.GetPrintDataAsync(id, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/record-signed")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentContractResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Record Contract Signed", Description = "Mencatat kontrak telah ditandatangani", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmploymentContracts", "Update")]
        public async Task<IActionResult> RecordSigned(
            Guid id,
            [FromForm] RecordContractSignedRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetUserId(User);
            var result = await _contractService.RecordSignedAsync(id, request, userId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/download-scan")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Download Contract Scan", Description = "Mengunduh pindaian scan kontrak kerja", AccessType = AccessTypes.Read, SortOrder = 5)]
        [AccessPermission("EmploymentContracts", "Read")]
        public async Task<IActionResult> DownloadScan(Guid id, CancellationToken cancellationToken)
        {
            var download = await _contractService.DownloadScanAsync(id, cancellationToken);
            if (!download.Exists || download.Stream == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Berkas scan kontrak tidak ditemukan."));
            }

            return File(download.Stream, download.ContentType ?? "application/octet-stream", download.FileName ?? "contract-scan.pdf");
        }

        private IActionResult ToActionResult<T>(RecruitmentResult<T> result)
        {
            if (result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message));
            }

            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message, result.Errors));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }
    }
}
