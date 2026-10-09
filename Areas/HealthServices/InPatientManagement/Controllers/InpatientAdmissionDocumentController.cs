using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Workspace PPRI — dokumen penerimaan pasien rawat inap baru (<c>BE-RWI-193</c> s.d. <c>202</c>,
    /// kontrak <c>episode-rawat-inap</c> <c>0.11.0</c> API 12.2).
    /// </summary>
    /// <remarks>
    /// <b>Hak akses per tindakan, bukan per peran.</b> Sepuluh butir <c>InpatientAdmissionDocument</c>
    /// dipetakan satu per satu dari kolom hak akses kontrak 12.2. Slot tanda tangan yang berbeda dijaga
    /// aksi yang berbeda, sehingga setiap slot punya endpoint sendiri (kontrak 12.6). Endpoint
    /// <c>/patient-rights</c> sengaja dijaga <c>InpatientEpisode : Read</c> walaupun tinggal di sini
    /// (permission matrix 10.2).
    ///
    /// <para>
    /// <b>Tanpa rupiah pada bacaan <c>Read</c>.</b> Rupiah hanya keluar dari endpoint berakhiran
    /// <c>/amounts</c> dan <c>/amount-print</c> yang dijaga <c>ViewAmount</c> (<c>RWI-DEC-258</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Tidak ada DELETE</b> (<c>INV-RWA-03</c>). Konsep dibuang lewat <c>PATCH …/discard</c> dan
    /// dokumen dibatalkan lewat <c>PATCH …/cancel</c>; keduanya meninggalkan jejak. Kontrak yang disetujui
    /// memakai <c>PATCH</c> untuk perubahan status dokumen. Penanda rencana tindakan
    /// (<c>PUT /procedure-plan-mark</c>) berada di luar gelombang ini (<c>EPIC-RWA-09</c>).
    /// </para>
    ///
    /// <para>
    /// Payload logger hanya <c>EntityId</c>, controller, aksi, dan status. Nama keluarga, nomor
    /// identitas, butir keyakinan, permintaan privasi, dan alasan tidak pernah masuk logger
    /// (permission matrix 10.5).
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/admission-workspace")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Admission Document",
        AreaName = "HealthServices",
        ControllerName = "InpatientAdmissionDocument",
        Description = "Dokumen penerimaan pasien rawat inap baru di Workspace PPRI",
        SortOrder = 19
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Admission Workspace")]
    public class InpatientAdmissionDocumentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.InPatientManagement.AdmissionWorkspace";
        private const string ControllerLabel = "InpatientAdmissionDocument";
        private const string IdempotencyHeader = "Idempotency-Key";

        private const string ReadDescription = "Membuka Workspace PPRI dan membaca dokumen admisi tanpa rupiah";
        private const string ViewAmountDescription = "Membaca rupiah deposit, IPD, dan mencetak dokumen berupiah";
        private const string CreateDescription = "Membuat konsep dokumen admisi";
        private const string UpdateDescription = "Mengubah, mengunci, membuka kunci, membuang konsep, dan membuat versi koreksi dokumen admisi";
        private const string SignDescription = "Mencatat tanda tangan kertas pasien/keluarga dan atestasi slot petugas admisi";
        private const string SignAsCroDescription = "Atestasi slot CRO pada dokumen admisi";
        private const string SignAsNurseDescription = "Atestasi slot perawat penerima pada dokumen admisi";
        private const string SignAsHeadNurseDescription = "Atestasi slot kepala ruangan pada dokumen admisi";
        private const string PrintDescription = "Mencetak dokumen admisi, gelang, label, IPD, dan mencatat cetak";
        private const string CancelDescription = "Membatalkan dokumen admisi beralasan";

        private readonly InpAdmissionWorkspaceQueryService _queryService;
        private readonly InpAdmissionPrefillService _prefillService;
        private readonly InpAdmissionDocumentService _documentService;
        private readonly InpAdmissionSignatureService _signatureService;
        private readonly InpAdmissionPrintService _printService;
        private readonly LoggerService _loggerService;

        public InpatientAdmissionDocumentController(
            InpAdmissionWorkspaceQueryService queryService,
            InpAdmissionPrefillService prefillService,
            InpAdmissionDocumentService documentService,
            InpAdmissionSignatureService signatureService,
            InpAdmissionPrintService printService,
            LoggerService loggerService)
        {
            _queryService = queryService;
            _prefillService = prefillService;
            _documentService = documentService;
            _signatureService = signatureService;
            _printService = printService;
            _loggerService = loggerService;
        }

        // =====================================================================
        // BE-RWI-193, 195 — Ringkasan, kop surat
        // =====================================================================

        /// <summary>Header pasien, menu beserta lencananya, kelengkapan, dan peringatan tanpa rupiah.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionWorkspaceSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Admission Document", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionDocument", "Read")]
        public async Task<IActionResult> GetSummary(Guid episodeId, CancellationToken cancellationToken = default)
            => FromResult(await _queryService.GetSummaryAsync(episodeId, cancellationToken));

        /// <summary>Status deposit berupiah dan jatuh tempo Pelunasan Deposit yang terlewati.</summary>
        [HttpGet("summary/amounts")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionWorkspaceAmountsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("ViewAmount", "View Inpatient Admission Document Amount", Description = ViewAmountDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("InpatientAdmissionDocument", "ViewAmount")]
        public async Task<IActionResult> GetSummaryAmounts(Guid episodeId, CancellationToken cancellationToken = default)
            => FromResult(await _queryService.GetSummaryAmountsAsync(episodeId, cancellationToken));

        /// <summary>Kop surat dari profil rumah sakit utama; boleh untuk episode status apa pun.</summary>
        [HttpGet("letterhead")]
        [ProducesResponseType(typeof(ApiResponse<LetterheadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Admission Document", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionDocument", "Read")]
        public async Task<IActionResult> GetLetterhead(Guid episodeId, CancellationToken cancellationToken = default)
            => FromResult(await _queryService.GetLetterheadAsync(episodeId, cancellationToken));

        // =====================================================================
        // BE-RWI-198 — General Consent cetak saja (tanpa tulis apa pun)
        // =====================================================================

        /// <summary>Data cetak Surat Persetujuan 12 butir dan Formulir General Consent V1.</summary>
        [HttpGet("general-consent/print-data")]
        [ProducesResponseType(typeof(ApiResponse<GeneralConsentPrintDataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Inpatient Admission Document", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionDocument", "Read")]
        public async Task<IActionResult> GetGeneralConsentPrintData(Guid episodeId, CancellationToken cancellationToken = default)
            => FromResult(await _queryService.GetGeneralConsentPrintDataAsync(episodeId, cancellationToken));

        // =====================================================================
        // BE-RWI-194, 199, 201, 202 — Isian bawaan dan dokumen
        // =====================================================================

        /// <summary>Isian bawaan sebelum dokumen dibuat, termasuk alasan bila dokumen tidak dapat dibuat.</summary>
        [HttpGet("prefill/{documentType}")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentPrefillResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Read", "Read Inpatient Admission Document", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionDocument", "Read")]
        public async Task<IActionResult> GetPrefill(
            Guid episodeId,
            InpAdmissionDocumentType documentType,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(documentType))
            {
                return Failure(StatusCodes.Status400BadRequest, "Jenis dokumen tidak dikenal.");
            }

            return FromResult(await _prefillService.GetPrefillAsync(episodeId, documentType, cancellationToken));
        }

        /// <summary>Daftar dokumen episode; versi lama ikut bila <c>includeHistory</c> = <c>true</c>.</summary>
        [HttpGet("documents")]
        [ProducesResponseType(typeof(ApiResponse<List<AdmissionDocumentSummaryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Read", "Read Inpatient Admission Document", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionDocument", "Read")]
        public async Task<IActionResult> GetDocuments(
            Guid episodeId,
            [FromQuery] InpAdmissionDocumentType? type,
            [FromQuery] bool includeHistory = false,
            CancellationToken cancellationToken = default)
        {
            if (type.HasValue && !Enum.IsDefined(type.Value))
            {
                return Failure(StatusCodes.Status400BadRequest, "Jenis dokumen tidak dikenal.");
            }

            return FromResult(await _documentService.ListAsync(episodeId, type, includeHistory, cancellationToken));
        }

        /// <summary>Satu dokumen lengkap tanpa rupiah, beserta tindakan yang tersedia bagi pengguna.</summary>
        [HttpGet("documents/{documentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Read", "Read Inpatient Admission Document", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionDocument", "Read")]
        public async Task<IActionResult> GetDocument(
            Guid episodeId,
            Guid documentId,
            CancellationToken cancellationToken = default)
            => FromResult(await _documentService.GetAsync(episodeId, documentId, User, cancellationToken));

        /// <summary>Angka Pelunasan Deposit: hidup selama <c>Draft</c>, beku sesudah dikunci.</summary>
        [HttpGet("documents/{documentId:guid}/amounts")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentAmountsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("ViewAmount", "View Inpatient Admission Document Amount", Description = ViewAmountDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("InpatientAdmissionDocument", "ViewAmount")]
        public async Task<IActionResult> GetDocumentAmounts(
            Guid episodeId,
            Guid documentId,
            CancellationToken cancellationToken = default)
            => FromResult(await _documentService.GetAmountsAsync(episodeId, documentId, cancellationToken));

        /// <summary>Simpan konsep baru. Pengulangan dengan <c>Idempotency-Key</c> sama mengembalikan dokumen yang sama.</summary>
        [HttpPost("documents")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Inpatient Admission Document", Description = CreateDescription, AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("InpatientAdmissionDocument", "Create")]
        public async Task<IActionResult> CreateDocument(
            Guid episodeId,
            [FromBody] CreateAdmissionDocumentRequest request,
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var result = await _documentService.CreateAsync(
                episodeId, request, idempotencyKey, User.GetUserId(), User, cancellationToken);

            return await LogAndReturnAsync(result, "CreateDocument", "Membuat konsep dokumen admisi.", x => x.Id);
        }

        /// <summary>Ubah konsep.</summary>
        [HttpPut("documents/{documentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Inpatient Admission Document", Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("InpatientAdmissionDocument", "Update")]
        public async Task<IActionResult> UpdateDocument(
            Guid episodeId,
            Guid documentId,
            [FromBody] UpdateAdmissionDocumentRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _documentService.UpdateAsync(
                episodeId, documentId, request, User.GetUserId(), User, cancellationToken);

            return await LogAndReturnAsync(result, "UpdateDocument", "Mengubah konsep dokumen admisi.", x => x.Id);
        }

        /// <summary>Kunci dan minta tanda tangan; membentuk salinan beku.</summary>
        [HttpPatch("documents/{documentId:guid}/lock")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Inpatient Admission Document", Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("InpatientAdmissionDocument", "Update")]
        public async Task<IActionResult> LockDocument(
            Guid episodeId,
            Guid documentId,
            [FromBody] RowVersionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _documentService.LockAsync(
                episodeId, documentId, request, User.GetUserId(), User, cancellationToken);

            return await LogAndReturnAsync(result, "LockDocument", "Mengunci dokumen admisi.", x => x.Id);
        }

        /// <summary>Buka kunci bila belum ada tanda tangan.</summary>
        [HttpPatch("documents/{documentId:guid}/unlock")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Inpatient Admission Document", Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("InpatientAdmissionDocument", "Update")]
        public async Task<IActionResult> UnlockDocument(
            Guid episodeId,
            Guid documentId,
            [FromBody] RowVersionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _documentService.UnlockAsync(
                episodeId, documentId, request, User.GetUserId(), User, cancellationToken);

            return await LogAndReturnAsync(result, "UnlockDocument", "Membuka kunci dokumen admisi.", x => x.Id);
        }

        /// <summary>Buang konsep sendiri. Alasan wajib dan tidak masuk logger.</summary>
        [HttpPatch("documents/{documentId:guid}/discard")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Inpatient Admission Document", Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("InpatientAdmissionDocument", "Update")]
        public async Task<IActionResult> DiscardDocument(
            Guid episodeId,
            Guid documentId,
            [FromBody] ReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _documentService.DiscardAsync(
                episodeId, documentId, request, User.GetUserId(), User, cancellationToken);

            return await LogAndReturnAsync(result, "DiscardDocument", "Membuang konsep dokumen admisi.", x => x.Id);
        }

        /// <summary>Buat versi koreksi dari dokumen <c>Completed</c>; versi lama menjadi <c>Superseded</c>.</summary>
        [HttpPost("documents/{documentId:guid}/revisions")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Inpatient Admission Document", Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("InpatientAdmissionDocument", "Update")]
        public async Task<IActionResult> ReviseDocument(
            Guid episodeId,
            Guid documentId,
            [FromBody] ReviseAdmissionDocumentRequest request,
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var result = await _documentService.ReviseAsync(
                episodeId, documentId, request, idempotencyKey, User.GetUserId(), User, cancellationToken);

            return await LogAndReturnAsync(result, "ReviseDocument", "Membuat versi koreksi dokumen admisi.", x => x.Id);
        }

        /// <summary>Batalkan dokumen beralasan (10–500 karakter). Alasan tidak masuk logger.</summary>
        [HttpPatch("documents/{documentId:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Cancel", "Cancel Inpatient Admission Document", Description = CancelDescription, AccessType = AccessTypes.Update, SortOrder = 10)]
        [AccessPermission("InpatientAdmissionDocument", "Cancel")]
        public async Task<IActionResult> CancelDocument(
            Guid episodeId,
            Guid documentId,
            [FromBody] ReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _documentService.CancelAsync(
                episodeId, documentId, request, User.GetUserId(), User, cancellationToken);

            return await LogAndReturnAsync(result, "CancelDocument", "Membatalkan dokumen admisi.", x => x.Id);
        }

        // =====================================================================
        // BE-RWI-193 — Tanda tangan: satu slot satu endpoint satu aksi
        // =====================================================================

        /// <summary>Catat lembar kertas yang sudah ditandatangani pasien/keluarga. Nama penanda tangan tidak masuk logger.</summary>
        [HttpPost("documents/{documentId:guid}/signatures/patient-or-family")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Sign", "Sign Inpatient Admission Document", Description = SignDescription, AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("InpatientAdmissionDocument", "Sign")]
        public async Task<IActionResult> RecordPatientOrFamilySignature(
            Guid episodeId,
            Guid documentId,
            [FromBody] RecordPaperSignatureRequest request,
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var result = await _signatureService.RecordPaperSignatureAsync(
                episodeId, documentId, request, idempotencyKey, User.GetUserId(), User, cancellationToken);

            return await LogAndReturnAsync(result, "RecordPatientOrFamilySignature", "Mencatat tanda tangan kertas pasien/keluarga.", x => x.Id);
        }

        /// <summary>Atestasi slot Admission / Petugas PPRI oleh akun yang masuk.</summary>
        [HttpPost("documents/{documentId:guid}/signatures/admission-officer")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Sign", "Sign Inpatient Admission Document", Description = SignDescription, AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("InpatientAdmissionDocument", "Sign")]
        public Task<IActionResult> SignAsAdmissionOfficer(
            Guid episodeId,
            Guid documentId,
            [FromBody] RowVersionRequest request,
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            CancellationToken cancellationToken = default)
            => AttestAsync(episodeId, documentId, InpAdmissionSignatureSlot.AdmissionOfficer, request, idempotencyKey,
                "SignAsAdmissionOfficer", cancellationToken);

        /// <summary>Atestasi slot CRO oleh akun yang masuk.</summary>
        [HttpPost("documents/{documentId:guid}/signatures/cro")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("SignAsCro", "Sign Inpatient Admission Document As CRO", Description = SignAsCroDescription, AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("InpatientAdmissionDocument", "SignAsCro")]
        public Task<IActionResult> SignAsCro(
            Guid episodeId,
            Guid documentId,
            [FromBody] RowVersionRequest request,
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            CancellationToken cancellationToken = default)
            => AttestAsync(episodeId, documentId, InpAdmissionSignatureSlot.CustomerRelationOfficer, request, idempotencyKey,
                "SignAsCro", cancellationToken);

        /// <summary>Atestasi slot Perawat penerima; hanya bila pasien menempati bed aktif (<c>GUARD-RWA-02</c>).</summary>
        [HttpPost("documents/{documentId:guid}/signatures/receiving-nurse")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("SignAsNurse", "Sign Inpatient Admission Document As Receiving Nurse", Description = SignAsNurseDescription, AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("InpatientAdmissionDocument", "SignAsNurse")]
        public Task<IActionResult> SignAsReceivingNurse(
            Guid episodeId,
            Guid documentId,
            [FromBody] RowVersionRequest request,
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            CancellationToken cancellationToken = default)
            => AttestAsync(episodeId, documentId, InpAdmissionSignatureSlot.ReceivingNurse, request, idempotencyKey,
                "SignAsReceivingNurse", cancellationToken);

        /// <summary>Atestasi slot Kepala Ruangan oleh akun yang masuk.</summary>
        [HttpPost("documents/{documentId:guid}/signatures/head-nurse")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("SignAsHeadNurse", "Sign Inpatient Admission Document As Head Nurse", Description = SignAsHeadNurseDescription, AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("InpatientAdmissionDocument", "SignAsHeadNurse")]
        public Task<IActionResult> SignAsHeadNurse(
            Guid episodeId,
            Guid documentId,
            [FromBody] RowVersionRequest request,
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            CancellationToken cancellationToken = default)
            => AttestAsync(episodeId, documentId, InpAdmissionSignatureSlot.HeadNurse, request, idempotencyKey,
                "SignAsHeadNurse", cancellationToken);

        // =====================================================================
        // BE-RWI-193, 196, 197 — Cetak
        // =====================================================================

        /// <summary>Data cetak dokumen tanpa rupiah, dengan penanda versi dari salinan beku.</summary>
        [HttpGet("documents/{documentId:guid}/print")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentPrintResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Print", "Print Inpatient Admission Document", Description = PrintDescription, AccessType = AccessTypes.Read, SortOrder = 9)]
        [AccessPermission("InpatientAdmissionDocument", "Print")]
        public async Task<IActionResult> GetDocumentPrint(
            Guid episodeId,
            Guid documentId,
            CancellationToken cancellationToken = default)
            => FromResult(await _printService.GetDocumentPrintAsync(episodeId, documentId, false, User, cancellationToken));

        /// <summary>
        /// Data cetak dokumen berupiah (Pelunasan Deposit). Dijaga <c>ViewAmount</c>; service juga
        /// memeriksa <c>Print</c> (permission matrix 10.2).
        /// </summary>
        [HttpGet("documents/{documentId:guid}/amount-print")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionDocumentPrintResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("ViewAmount", "View Inpatient Admission Document Amount", Description = ViewAmountDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("InpatientAdmissionDocument", "ViewAmount")]
        public async Task<IActionResult> GetDocumentAmountPrint(
            Guid episodeId,
            Guid documentId,
            CancellationToken cancellationToken = default)
            => FromResult(await _printService.GetDocumentPrintAsync(episodeId, documentId, true, User, cancellationToken));

        /// <summary>Data Gelang Dewasa atau Bayi dan Label Pasien.</summary>
        [HttpGet("identity-labels")]
        [ProducesResponseType(typeof(ApiResponse<IdentityLabelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Print", "Print Inpatient Admission Document", Description = PrintDescription, AccessType = AccessTypes.Read, SortOrder = 9)]
        [AccessPermission("InpatientAdmissionDocument", "Print")]
        public async Task<IActionResult> GetIdentityLabels(Guid episodeId, CancellationToken cancellationToken = default)
            => FromResult(await _queryService.GetIdentityLabelsAsync(episodeId, cancellationToken));

        /// <summary>Data Dasar Rawat Inap (IPD) tanpa rupiah; isian tanpa sumber tercantum di <c>BlankFields</c>.</summary>
        [HttpGet("base-data")]
        [ProducesResponseType(typeof(ApiResponse<InpatientBaseDataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Read", "Read Inpatient Admission Document", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionDocument", "Read")]
        public async Task<IActionResult> GetBaseData(Guid episodeId, CancellationToken cancellationToken = default)
            => FromResult(await _queryService.GetBaseDataAsync(episodeId, cancellationToken));

        /// <summary>"Rencana @ Kamar (Rp)"; <c>NotYetAvailable</c> sampai Billing menyediakan tarif kamar harian.</summary>
        [HttpGet("base-data/amounts")]
        [ProducesResponseType(typeof(ApiResponse<InpatientBaseDataAmountsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("ViewAmount", "View Inpatient Admission Document Amount", Description = ViewAmountDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("InpatientAdmissionDocument", "ViewAmount")]
        public async Task<IActionResult> GetBaseDataAmounts(Guid episodeId, CancellationToken cancellationToken = default)
            => FromResult(await _queryService.GetBaseDataAmountsAsync(episodeId, cancellationToken));

        /// <summary>Riwayat cetak episode, dengan nomor cetakan per kunci cetakan.</summary>
        [HttpGet("print-logs")]
        [ProducesResponseType(typeof(ApiResponse<List<PrintLogResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Read", "Read Inpatient Admission Document", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionDocument", "Read")]
        public async Task<IActionResult> GetPrintLogs(
            Guid episodeId,
            [FromQuery] InpAdmissionPrintKind? kind,
            [FromQuery] Guid? documentId,
            CancellationToken cancellationToken = default)
        {
            if (kind.HasValue && !Enum.IsDefined(kind.Value))
            {
                return Failure(StatusCodes.Status400BadRequest, "Jenis cetakan tidak dikenal.");
            }

            return FromResult(await _printService.GetPrintLogsAsync(episodeId, kind, documentId, cancellationToken));
        }

        /// <summary>Catat cetak atau cetak ulang. Catatan cetak ulang tidak masuk logger.</summary>
        [HttpPost("print-logs")]
        [ProducesResponseType(typeof(ApiResponse<PrintLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Print", "Print Inpatient Admission Document", Description = PrintDescription, AccessType = AccessTypes.Read, SortOrder = 9)]
        [AccessPermission("InpatientAdmissionDocument", "Print")]
        public async Task<IActionResult> RecordPrint(
            Guid episodeId,
            [FromBody] RecordPrintRequest request,
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var result = await _printService.RecordPrintAsync(
                episodeId, request, idempotencyKey, User.GetUserId(), cancellationToken);

            return await LogAndReturnAsync(result, "RecordPrint", "Mencatat cetak dokumen admisi.", x => x.Id);
        }

        // =====================================================================
        // BE-RWI-200 — Ringkasan hak pasien untuk modul lain
        // =====================================================================

        /// <summary>
        /// Nilai Kepercayaan dan Permintaan Privasi <c>Completed</c> episode ini. Endpoint alias yang
        /// dijaga <c>InpatientEpisode : Read</c> (permission matrix 10.2); kemampuannya didaftarkan
        /// endpoint asli di <c>InpatientEpisodeController</c>, sehingga tidak mengulang <c>[AccessAction]</c>.
        /// </summary>
        [HttpGet("patient-rights")]
        [ProducesResponseType(typeof(ApiResponse<PatientRightsSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessPermission("InpatientEpisode", "Read")]
        public async Task<IActionResult> GetPatientRights(Guid episodeId, CancellationToken cancellationToken = default)
            => FromResult(await _queryService.GetPatientRightsAsync(episodeId, cancellationToken));

        // =====================================================================
        // Pembantu
        // =====================================================================

        private async Task<IActionResult> AttestAsync(
            Guid episodeId,
            Guid documentId,
            InpAdmissionSignatureSlot slot,
            RowVersionRequest request,
            string? idempotencyKey,
            string actionName,
            CancellationToken cancellationToken)
        {
            var result = await _signatureService.AttestAsync(
                episodeId, documentId, slot, request, idempotencyKey, User.GetUserId(), User, cancellationToken);

            return await LogAndReturnAsync(result, actionName, "Atestasi tanda tangan dokumen admisi.", x => x.Id);
        }

        /// <summary>Mencatat penulisan yang berhasil lalu mengembalikan hasilnya. Payload hanya id, controller, aksi, dan status.</summary>
        private async Task<IActionResult> LogAndReturnAsync<T>(
            InpAdmissionResult<T> result,
            string actionName,
            string message,
            Func<T, Guid> entityId)
        {
            if (!result.IsSuccess)
            {
                return FromResult(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                $"{ControllerLabel}.{actionName}",
                message,
                new
                {
                    EntityId = entityId(result.Data!),
                    Controller = ControllerLabel,
                    Action = actionName,
                    StatusCode = result.StatusCode
                });

            return FromResult(result);
        }

        /// <summary>
        /// Menerjemahkan hasil service menjadi <c>ApiResponse</c>. Kode status ditentukan aturan
        /// bisnis di service; kode alasan kontrak dan rincian isian yang kurang dibawa pada errors.
        /// </summary>
        private IActionResult FromResult<T>(InpAdmissionResult<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<T>.Ok(result.Data, result.Message));
            }

            return Failure(result.StatusCode, result.Message, result.Code, result.Errors);
        }

        private IActionResult Failure(
            int statusCode,
            string message,
            string? code = null,
            IReadOnlyList<InpAdmissionError>? errors = null)
        {
            object? details = code == null && (errors == null || errors.Count == 0)
                ? null
                : new { Code = code, Details = errors };

            return StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, message, details));
        }
    }
}
