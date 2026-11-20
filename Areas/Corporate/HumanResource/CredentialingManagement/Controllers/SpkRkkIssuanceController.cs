using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services;
using System.Security.Claims;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

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
                // MSL-16: profil reviewer diambil dari token bila body kosong, lalu dicocokkan di service (VR-CRD-15).
                var myProfileId = User.GetWorkforceProfileId();
                if (request.ReviewerWorkforceProfileId == Guid.Empty && myProfileId.HasValue)
                {
                    request.ReviewerWorkforceProfileId = myProfileId.Value;
                }

                var success = await _workflowService.SubmitPeerReviewAssessmentAsync(request, GetUserId(), cancellationToken, myProfileId);
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

        /// <summary>
        /// Mengunduh berkas fisik SPK atau RKK dalam format PDF (FIN-HRD-09-BE-007, FIN-HRD-09-BE-010)
        /// </summary>
        [HttpGet("spk-rkk/{id:guid}/files")]
        [AccessAction("Read", "Melihat Berkas Kredensial & SPK/RKK", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SpkRkkIssuance", "Read")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadSpkRkkFile(
            [FromRoute] Guid id,
            [FromQuery] string fileType = "SPK",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (fileBytes, fileName, contentType) = await _spkRkkService.GetSpkRkkFileAsync(id, fileType, cancellationToken);
                return File(fileBytes, contentType, fileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // =====================================================================
        // WORKLIST KREDENSIAL (register masalah SDM MSL-16, 19 Nov 2026)
        // =====================================================================

        /// <summary>
        /// Daftar permohonan kredensial seluruh tenaga kesehatan (MSL-16). Filter <c>status</c>: Submitted, InReview,
        /// PlenarySessionConcluded. Cari nomor permohonan, nama, atau kode pegawai.
        /// </summary>
        [HttpGet("applications")]
        [AccessAction("Read", "Melihat Berkas Kredensial & SPK/RKK", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SpkRkkIssuance", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CredentialingApplicationListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApplications(
            [FromQuery] CredentialingWorklistQuery query,
            [FromServices] ICredentialingWorklistQueryService worklist,
            CancellationToken cancellationToken = default)
        {
            var result = await worklist.GetApplicationsAsync(query, User.GetUserId(), cancellationToken);
            return Ok(ApiResponse<PagedResult<CredentialingApplicationListItemDto>>.Ok(result, "Daftar permohonan kredensial berhasil dimuat."));
        }

        /// <summary>
        /// Worklist Mitra Bestari (MSL-16): permohonan berstatus InReview yang menunjuk akun login sebagai reviewer.
        /// Akun tanpa profil tenaga kerja mendapat daftar kosong beserta pesan penjelas.
        /// </summary>
        [HttpGet("applications/peer-review-worklist")]
        [AccessAction("AssessPeer", "Menilai Berkas Kredensial (Mitra Bestari)", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("SpkRkkIssuance", "AssessPeer")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CredentialingApplicationListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPeerReviewWorklist(
            [FromQuery] CredentialingWorklistQuery query,
            [FromServices] ICredentialingWorklistQueryService worklist,
            CancellationToken cancellationToken = default)
        {
            var profileId = User.GetWorkforceProfileId() ?? Guid.Empty;
            var result = await worklist.GetPeerReviewWorklistAsync(query, profileId, User.GetUserId(), cancellationToken);
            var message = profileId == Guid.Empty
                ? "Akun Anda belum terhubung ke profil tenaga kerja, sehingga tidak ada penugasan Mitra Bestari yang bisa ditampilkan."
                : "Worklist Mitra Bestari berhasil dimuat.";
            return Ok(ApiResponse<PagedResult<CredentialingApplicationListItemDto>>.Ok(result, message));
        }

        /// <summary>
        /// Lembar penilaian Mitra Bestari (MSL-16): butir kewenangan yang diajukan pemohon. Hanya untuk anggota panel;
        /// selain itu dijawab 404.
        /// </summary>
        [HttpGet("applications/{id:guid}/peer-review-sheet")]
        [AccessAction("AssessPeer", "Menilai Berkas Kredensial (Mitra Bestari)", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("SpkRkkIssuance", "AssessPeer")]
        [ProducesResponseType(typeof(ApiResponse<CredentialingPeerReviewSheetDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPeerReviewSheet(
            [FromRoute] Guid id,
            [FromServices] ICredentialingWorklistQueryService worklist,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await worklist.GetPeerReviewSheetAsync(id, User.GetWorkforceProfileId() ?? Guid.Empty, User.GetUserId(), cancellationToken);
                return Ok(ApiResponse<CredentialingPeerReviewSheetDto>.Ok(result, "Lembar penilaian berhasil dimuat."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        /// <summary>Worklist sidang pleno (MSL-16): permohonan InReview beserta jumlah reviewer dan penilaian yang sudah masuk.</summary>
        [HttpGet("applications/plenary-worklist")]
        [AccessAction("ApproveCommittee", "Mengesahkan Sidang Pleno Komite", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("SpkRkkIssuance", "ApproveCommittee")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CredentialingApplicationListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlenaryWorklist(
            [FromQuery] CredentialingWorklistQuery query,
            [FromServices] ICredentialingWorklistQueryService worklist,
            CancellationToken cancellationToken = default)
        {
            var result = await worklist.GetPlenaryWorklistAsync(query, User.GetUserId(), cancellationToken);
            return Ok(ApiResponse<PagedResult<CredentialingApplicationListItemDto>>.Ok(result, "Worklist sidang pleno berhasil dimuat."));
        }

        /// <summary>
        /// Matriks pleno (MSL-16): butir kewenangan yang diajukan dan penilaian setiap Mitra Bestari, untuk menyelesaikan
        /// beda rekomendasi sebelum BAP pleno disahkan.
        /// </summary>
        [HttpGet("applications/{id:guid}/plenary-matrix")]
        [AccessAction("ApproveCommittee", "Mengesahkan Sidang Pleno Komite", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("SpkRkkIssuance", "ApproveCommittee")]
        [ProducesResponseType(typeof(ApiResponse<CredentialingPlenaryMatrixDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlenaryMatrix(
            [FromRoute] Guid id,
            [FromServices] ICredentialingWorklistQueryService worklist,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await worklist.GetPlenaryMatrixAsync(id, cancellationToken);
                return Ok(ApiResponse<CredentialingPlenaryMatrixDto>.Ok(result, "Matriks pleno berhasil dimuat."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        /// <summary>
        /// Antrean draf SPK-RKK (MSL-16): permohonan yang sudah dipleno, tidak ditolak, dan belum punya SPK-RKK.
        /// </summary>
        [HttpGet("spk-rkk/draft-queue")]
        [AccessAction("ParafDirector", "Memaraf Draf SPK/RKK", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("SpkRkkIssuance", "ParafDirector")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CredentialingApplicationListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSpkRkkDraftQueue(
            [FromQuery] CredentialingWorklistQuery query,
            [FromServices] ICredentialingWorklistQueryService worklist,
            CancellationToken cancellationToken = default)
        {
            var result = await worklist.GetSpkRkkDraftQueueAsync(query, User.GetUserId(), cancellationToken);
            return Ok(ApiResponse<PagedResult<CredentialingApplicationListItemDto>>.Ok(result, "Antrean draf SPK-RKK berhasil dimuat."));
        }

        /// <summary>SPK-RKK yang menunggu tanda tangan Direktur Utama (MSL-16), urut dari yang paling lama menunggu.</summary>
        [HttpGet("spk-rkk/pending-signature")]
        [AccessAction("SignDirector", "Menandatangani SPK/RKK", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("SpkRkkIssuance", "SignDirector")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<SpkRkkPendingSignatureItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSpkRkkPendingSignature(
            [FromQuery] CredentialingWorklistQuery query,
            [FromServices] ICredentialingWorklistQueryService worklist,
            CancellationToken cancellationToken = default)
        {
            var result = await worklist.GetPendingSignatureAsync(query, cancellationToken);
            return Ok(ApiResponse<PagedResult<SpkRkkPendingSignatureItemDto>>.Ok(result, "Daftar SPK-RKK menunggu tanda tangan berhasil dimuat."));
        }
    }
}
