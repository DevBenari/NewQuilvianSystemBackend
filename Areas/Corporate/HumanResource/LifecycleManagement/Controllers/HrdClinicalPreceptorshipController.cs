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
    [Route("api/v1/corporate/human-resource/clinical-preceptorships")]
    [Authorize]
    [Tags("HRD Clinical Preceptorships")]
    public class HrdClinicalPreceptorshipController : ControllerBase
    {
        private readonly IHrdClinicalPreceptorshipService _preceptorshipService;

        public HrdClinicalPreceptorshipController(IHrdClinicalPreceptorshipService preceptorshipService)
        {
            _preceptorshipService = preceptorshipService ?? throw new ArgumentNullException(nameof(preceptorshipService));
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] ClinicalPreceptorshipQuery query, CancellationToken cancellationToken)
        {
            var result = await _preceptorshipService.GetPagedAsync(query, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _preceptorshipService.GetByIdAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/preceptor")]
        public async Task<IActionResult> ChangePreceptor(
            Guid id,
            [FromBody] ChangePreceptorRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(RecruitmentResult<ClinicalPreceptorshipResponse>.BadRequest("Data pergantian preceptor tidak valid."));
            }

            var result = await _preceptorshipService.ChangePreceptorAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/declare-independent")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> DeclareIndependent(
            Guid id,
            [FromForm] DeclareClinicalIndependentRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(RecruitmentResult<ClinicalPreceptorshipResponse>.BadRequest("Data pernyataan siap mandiri tidak valid."));
            }

            var result = await _preceptorshipService.DeclareIndependentAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}/evidence")]
        public async Task<IActionResult> DownloadEvidence(Guid id, CancellationToken cancellationToken)
        {
            var (exists, stream, contentType, fileName) = await _preceptorshipService.DownloadEvidenceAsync(id, cancellationToken);
            if (!exists || stream == null)
            {
                return NotFound(RecruitmentResult<object>.NotFound("Berkas bukti asesmen klinis tidak ditemukan."));
            }

            return File(stream, contentType ?? "application/octet-stream", fileName ?? "evaluasi-mandiri.pdf");
        }
    }
}
