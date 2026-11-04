using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/human-resource/lifecycle-management/probations")]
    [Route("api/v1/corporate/human-resource/probations")]
    [Authorize]
    [Tags("Corporate / Human Resource / Lifecycle Management / Probation")]
    public class HrdProbationController : ControllerBase
    {
        private readonly IHrdProbationService _probationService;

        public HrdProbationController(IHrdProbationService probationService)
        {
            _probationService = probationService ?? throw new ArgumentNullException(nameof(probationService));
        }

        /// <summary>
        /// Mengambil daftar masa percobaan (probation) dengan filter status, unit organisasi, jatuh tempo, tanggal berakhir, dan pencarian.
        /// </summary>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(RecruitmentResult<PagedResult<HrdProbationResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] HrdProbationQuery query, CancellationToken cancellationToken)
        {
            var result = await _probationService.GetPagedAsync(query, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Mengambil detail lengkap masa percobaan beserta seluruh riwayat penilaiannya (assessments).
        /// </summary>
        [HttpGet("{id:guid}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(RecruitmentResult<HrdProbationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _probationService.GetByIdAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Mengajukan hasil penilaian masa percobaan (Lulus atau Tidak Lulus) oleh atasan langsung dan memulai alur workflow T15.
        /// VR-ONB-55: Hanya sah saat masa percobaan Running dan sebelum/pada tanggal akhir.
        /// VR-ONB-56: Penilaian Tidak Lulus (Fail) wajib melampirkan berkas bukti evaluasi kinerja.
        /// VR-ONB-57: Hanya 1 penilaian terbuka per masa percobaan.
        /// </summary>
        [HttpPost("{id:guid}/assessments")]
        [Consumes("multipart/form-data")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(RecruitmentResult<ProbationAssessmentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SubmitAssessment(
            Guid id,
            [FromForm] SubmitProbationAssessmentRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(RecruitmentResult<ProbationAssessmentResponse>.BadRequest("Validasi pengajuan penilaian gagal.", errors));
            }

            var result = await _probationService.SubmitAssessmentAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Memperbarui penilaian yang dikembalikan (Returned/Minta Revisi), lalu mengajukan ulang ke workflow T15.
        /// </summary>
        [HttpPut("{id:guid}/assessments/{assessmentId:guid}")]
        [Consumes("multipart/form-data")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(RecruitmentResult<ProbationAssessmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateAssessment(
            Guid id,
            Guid assessmentId,
            [FromForm] UpdateProbationAssessmentRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(RecruitmentResult<ProbationAssessmentResponse>.BadRequest("Validasi pembaruan penilaian gagal.", errors));
            }

            var result = await _probationService.UpdateAssessmentAsync(id, assessmentId, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Menarik pengajuan penilaian masa percobaan sebelum keputusan final ditetapkan oleh HRD.
        /// </summary>
        [HttpPost("{id:guid}/assessments/{assessmentId:guid}/withdraw")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(RecruitmentResult<ProbationAssessmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> WithdrawAssessment(
            Guid id,
            Guid assessmentId,
            [FromBody] WithdrawAssessmentRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _probationService.WithdrawAssessmentAsync(id, assessmentId, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Mengunduh berkas dokumen bukti pendukung penilaian masa percobaan.
        /// </summary>
        [HttpGet("{id:guid}/assessments/{assessmentId:guid}/evidence")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadEvidence(
            Guid id,
            Guid assessmentId,
            CancellationToken cancellationToken)
        {
            var file = await _probationService.DownloadEvidenceAsync(id, assessmentId, cancellationToken);
            if (!file.Exists || file.Stream == null)
            {
                return NotFound(RecruitmentResult<object>.NotFound("Berkas bukti penilaian masa percobaan tidak ditemukan atau belum diunggah."));
            }

            return File(file.Stream, file.ContentType ?? "application/octet-stream", file.FileName ?? "bukti-penilaian.pdf");
        }

        // CATATAN INVARIANT REGULASI RUMAH SAKIT:
        // VR-ONB-59: DILARANG KERAS menyediakan endpoint perpanjangan masa percobaan.
        // Berdasarkan UU Ketenagakerjaan No. 13/2003 Pasal 60 dan arsitektur Quilvian, masa percobaan PKWT/PKWTT
        // tidak dapat diperpanjang dan harus menghasilkan keputusan Lulus/Tidak Lulus atau Lulus Otomatis.
    }
}
