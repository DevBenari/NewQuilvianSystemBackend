using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services;
using System.Security.Claims;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/credentialing")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_CREDENTIALING",
        moduleName: "Human Resource Credentialing",
        displayName: "Kredensial & Penerbitan SPK/RKK",
        AreaName = "Corporate",
        ControllerName = "SpkRkkIssuance",
        Description = "Permohonan kredensial, panel Mitra Bestari, sidang pleno, dan penandatanganan SPK/RKK",
        SortOrder = 12)]
    [Tags("HR Credentialing - Committees, Peer Review & SPK-RKK Issuance")]
    [Authorize]
    public class SpkRkkIssuanceController : ControllerBase
    {
        private readonly ICredentialingCommitteeService _committeeService;
        private readonly ICredentialingWorkflowService _workflowService;
        private readonly ISpkRkkIssuanceService _spkRkkService;
        private readonly ILogger<SpkRkkIssuanceController> _logger;

        public SpkRkkIssuanceController(
            ICredentialingCommitteeService committeeService,
            ICredentialingWorkflowService workflowService,
            ISpkRkkIssuanceService spkRkkService,
            ILogger<SpkRkkIssuanceController> logger)
        {
            _committeeService = committeeService;
            _workflowService = workflowService;
            _spkRkkService = spkRkkService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var rawId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? User.FindFirst("id")?.Value;

            return Guid.TryParse(rawId, out var id) ? id : Guid.Empty;
        }

        // =====================================================================
        // ENDPOINT 3 KOMITE MANDIRI & WHITEBOOK (BE-HRD-211, HRD-DEC-147)
        // =====================================================================

        /// <summary>
        /// Mengambil seluruh data 3 komite kredensial mandiri (Medik, Keperawatan, Nakes Lain)
        /// </summary>
        [HttpGet("committees")]
        [AccessAction("Read", "Melihat Berkas Kredensial & SPK/RKK", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SpkRkkIssuance", "Read")]
        [ProducesResponseType(typeof(List<CredentialingCommitteeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCommittees(CancellationToken cancellationToken = default)
        {
            var result = await _committeeService.GetAllCommitteesAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil katalog buku putih (whitebook) kewenangan klinis sesuai rumpun profesi
        /// </summary>
        [HttpGet("whitebook-catalog")]
        [AccessAction("Read", "Melihat Berkas Kredensial & SPK/RKK", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SpkRkkIssuance", "Read")]
        [ProducesResponseType(typeof(List<ClinicalPrivilegeCatalogItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWhitebookCatalog(
            [FromQuery] string? committeeCode = null,
            [FromQuery] Guid? professionId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _committeeService.GetWhitebookCatalogAsync(committeeCode, professionId, cancellationToken);
            return Ok(result);
        }

        // =====================================================================
        // ENDPOINT PENGAJUAN & PANEL MITRA BESTARI (BE-HRD-212, VR-CRD-15)
        // =====================================================================

        /// <summary>
        /// Pengajuan berkas kredensial nakes (Validasi PSV Mandatori VR-CRD-02)
        /// </summary>
        [HttpPost("applications")]
        [AccessAction("CreateApplication", "Mengajukan Berkas Kredensial", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("SpkRkkIssuance", "CreateApplication")]
        [ProducesResponseType(typeof(CredentialingApplicationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateApplication(
            [FromBody] CreateCredentialingApplicationDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _workflowService.CreateApplicationAsync(request, GetUserId(), cancellationToken);
                return CreatedAtAction(nameof(GetApplicationDetail), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil rincian berkas permohonan kredensial nakes
        /// </summary>
        [HttpGet("applications/{id:guid}")]
        [AccessAction("Read", "Melihat Berkas Kredensial & SPK/RKK", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SpkRkkIssuance", "Read")]
        [ProducesResponseType(typeof(CredentialingApplicationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApplicationDetail(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _workflowService.GetApplicationDetailAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Penunjukan panel Mitra Bestari (min 2 orang & Anti-Self-Review VR-CRD-15)
        /// </summary>
        [HttpPost("applications/{id:guid}/assign-reviewers")]
        [AccessAction("AssignReviewer", "Menetapkan Panel Mitra Bestari", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SpkRkkIssuance", "AssignReviewer")]
        [ProducesResponseType(typeof(CredentialingApplicationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignReviewers(
            [FromRoute] Guid id,
            [FromBody] AssignPeerReviewersDto request,
            CancellationToken cancellationToken = default)
        {
            if (id != request.ApplicationId)
            {
                return BadRequest(new { message = "ID permohonan pada URL tidak sesuai dengan payload body." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _workflowService.AssignPeerReviewersAsync(request, GetUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =====================================================================
        // ENDPOINT ASESMEN MITRA BESTARI & SIDANG PLENO (BE-HRD-213)
        // =====================================================================

        /// <summary>
        /// Pengisian lembar asesmen portofolio klinis oleh reviewer Mitra Bestari
        /// </summary>
        [HttpPost("applications/{id:guid}/assessments")]
        [AccessAction("AssessPeer", "Menilai Berkas Kredensial (Mitra Bestari)", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("SpkRkkIssuance", "AssessPeer")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitAssessment(
            [FromRoute] Guid id,
            [FromBody] SubmitPeerReviewAssessmentDto request,
            CancellationToken cancellationToken = default)
        {
            if (id != request.ApplicationId)
            {
                return BadRequest(new { message = "ID permohonan pada URL tidak sesuai dengan payload body." });
            }

            try
            {
                var success = await _workflowService.SubmitPeerReviewAssessmentAsync(request, GetUserId(), cancellationToken);
                return Ok(new { success, message = "Asesmen portofolio klinis berhasil disimpan." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Pelaksanaan dan pengesahan Berita Acara Sidang Pleno Komite Kredensial
        /// </summary>
        [HttpPost("applications/{id:guid}/plenary-session")]
        [AccessAction("ApproveCommittee", "Mengesahkan Sidang Pleno Komite", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("SpkRkkIssuance", "ApproveCommittee")]
        [ProducesResponseType(typeof(PlenarySessionResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConcludePlenarySession(
            [FromRoute] Guid id,
            [FromBody] ConcludePlenarySessionDto request,
            CancellationToken cancellationToken = default)
        {
            if (id != request.ApplicationId)
            {
                return BadRequest(new { message = "ID permohonan pada URL tidak sesuai dengan payload body." });
            }

            try
            {
                var result = await _workflowService.ConcludePlenarySessionAsync(request, GetUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =====================================================================
        // ENDPOINT PENERBITAN SPK-RKK & e-SIGN DIRUT (BE-HRD-214 & BE-HRD-215)
        // =====================================================================

        /// <summary>
        /// Menerbitkan draf Surat Penugasan Klinis (SPK) dan RKK 3 tahun (CRD-B3)
        /// </summary>
        [HttpPost("spk-rkk/issue-draft")]
        [AccessAction("ParafDirector", "Memaraf Draf SPK/RKK", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("SpkRkkIssuance", "ParafDirector")]
        [ProducesResponseType(typeof(SpkRkkDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> IssueSpkRkkDraft(
            [FromBody] IssueSpkRkkRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _spkRkkService.IssueSpkRkkDraftAsync(request, GetUserId(), cancellationToken);
                return CreatedAtAction(nameof(GetSpkRkkDetail), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Pengesahan tunggal digital e-Sign Direktur Utama RS & Auto-Arsip E-Dossier (INT-HR-68, VR-CRD-12)
        /// </summary>
        [HttpPost("spk-rkk/{id:guid}/sign-dirut")]
        [AccessAction("SignDirector", "Menandatangani SPK/RKK", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("SpkRkkIssuance", "SignDirector")]
        [ProducesResponseType(typeof(SpkRkkDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SignByDirectorGeneral(
            [FromRoute] Guid id,
            [FromBody] SignSpkRkkDto request,
            CancellationToken cancellationToken = default)
        {
            if (id != request.SpkRkkIssuanceId)
            {
                return BadRequest(new { message = "ID penerbitan SPK-RKK pada URL tidak sesuai dengan payload body." });
            }

            try
            {
                var result = await _spkRkkService.SignSpkRkkByDirectorGeneralAsync(request, GetUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil rincian SPK-RKK dan daftar kewenangan klinis yang disahkan
        /// </summary>
        [HttpGet("spk-rkk/{id:guid}")]
        [AccessAction("Read", "Melihat Berkas Kredensial & SPK/RKK", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SpkRkkIssuance", "Read")]
        [ProducesResponseType(typeof(SpkRkkDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSpkRkkDetail(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _spkRkkService.GetSpkRkkDetailAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil riwayat SPK-RKK milik seorang tenaga kerja
        /// </summary>
        [HttpGet("spk-rkk/employee/{employeeId:guid}")]
        [AccessAction("Read", "Melihat Berkas Kredensial & SPK/RKK", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SpkRkkIssuance", "Read")]
        [ProducesResponseType(typeof(List<SpkRkkDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSpkRkkByEmployee(
            [FromRoute] Guid employeeId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _spkRkkService.GetSpkRkkByEmployeeAsync(employeeId, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
