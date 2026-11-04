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
        public async Task<IActionResult> GetPaged([FromQuery] EmploymentContractQuery query, CancellationToken cancellationToken)
        {
            var result = await _contractService.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentContractResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _contractService.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/print-data")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentContractPrintDataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPrintData(Guid id, CancellationToken cancellationToken)
        {
            var result = await _contractService.GetPrintDataAsync(id, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/record-signed")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentContractResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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
        public async Task<IActionResult> DownloadScan(Guid id, CancellationToken cancellationToken)
        {
            var download = await _contractService.DownloadScanAsync(id, cancellationToken);
            if (!download.Exists || download.Stream == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Berkas scan kontrak tidak ditemukan."));
            }

            return File(download.Stream, download.ContentType ?? "application/octet-stream", download.FileName ?? "contract-scan.pdf");
        }

        // --- BE-HRD-080: Keputusan Kontrak T16 & Batas 5 Tahun PKWT ---

        [HttpGet("{id:guid}/fixed-term-check")]
        [ProducesResponseType(typeof(ApiResponse<FixedTermCheckResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FixedTermCheck(Guid id, CancellationToken cancellationToken)
        {
            var result = await _contractService.FixedTermCheckAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/decisions")]
        [ProducesResponseType(typeof(ApiResponse<ContractDecisionResponse>), StatusCodes.Status201Created)]
        public async Task<IActionResult> SubmitDecision(
            Guid id,
            [FromBody] SubmitContractDecisionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _contractService.SubmitDecisionAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/decisions/{decisionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ContractDecisionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDecisionById(
            Guid id,
            Guid decisionId,
            CancellationToken cancellationToken)
        {
            var result = await _contractService.GetDecisionByIdAsync(id, decisionId, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}/decisions/{decisionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ContractDecisionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDecision(
            Guid id,
            Guid decisionId,
            [FromBody] SubmitContractDecisionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _contractService.UpdateDecisionAsync(id, decisionId, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}/decisions/{decisionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> WithdrawDecision(
            Guid id,
            Guid decisionId,
            CancellationToken cancellationToken)
        {
            var result = await _contractService.WithdrawDecisionAsync(id, decisionId, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/decisions/{decisionId:guid}/notify")]
        [ProducesResponseType(typeof(ApiResponse<ContractDecisionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecordEmployeeNotified(
            Guid id,
            Guid decisionId,
            [FromBody] RecordEmployeeNotifiedRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _contractService.RecordEmployeeNotifiedAsync(id, decisionId, request, User, cancellationToken);
            return ToActionResult(result);
        }

        // --- BE-HRD-081: Koreksi Kontrak T17 ---

        [HttpPost("{id:guid}/corrections")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ContractCorrectionResponse>), StatusCodes.Status201Created)]
        public async Task<IActionResult> SubmitCorrection(
            Guid id,
            [FromForm] SubmitContractCorrectionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _contractService.SubmitCorrectionAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/corrections/{correctionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ContractCorrectionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCorrectionById(
            Guid id,
            Guid correctionId,
            CancellationToken cancellationToken)
        {
            var result = await _contractService.GetCorrectionByIdAsync(id, correctionId, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}/corrections/{correctionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> WithdrawCorrection(
            Guid id,
            Guid correctionId,
            CancellationToken cancellationToken)
        {
            var result = await _contractService.WithdrawCorrectionAsync(id, correctionId, User, cancellationToken);
            return ToActionResult(result);
        }

        // --- Scan Susulan Dokumen Kontrak Data Awal (VR-ONB-45) ---

        [HttpPost("{id:guid}/document")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentContractResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadLegacyDocument(
            Guid id,
            [FromForm] UploadLegacyContractDocumentRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _contractService.UploadLegacyDocumentAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
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
