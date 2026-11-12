using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Controllers
{
    /// <summary>
    /// Controller batch formasi awal rumah sakit (BE-HRD-033).
    /// Mengelola templat isian, validasi baris, pembuatan draf, unggah scan SK, pengajuan ke T10, dan pembatalan.
    /// Memenuhi API v7 Bagian 13 grup Initial Formation Batch (10 endpoint).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-planning/initial-formation-batches")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PLANNING",
        moduleName: "Human Resource Workforce Planning",
        displayName: "Initial Formation Batch",
        AreaName = "Corporate",
        ControllerName = "WorkforceInitialFormationBatch",
        Description = "Pengelolaan batch formasi awal rumah sakit",
        SortOrder = 32)]
    [Tags("Corporate / Human Resource / Workforce Planning / Initial Formation Batch")]
    public sealed class HrdInitialFormationBatchController : ControllerBase
    {
        private readonly IHrdInitialFormationBatchService _service;

        public HrdInitialFormationBatchController(IHrdInitialFormationBatchService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mengunduh berkas template Excel (.xlsx) untuk isian formasi awal (kriteria 1).
        /// </summary>
        [HttpGet("template")]
        [AccessAction("DownloadTemplate", "Download Template Batch Formasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInitialFormationBatch", "Read")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public IActionResult DownloadTemplate()
        {
            var bytes = _service.GenerateTemplate();
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Template_Formasi_Awal.xlsx");
        }

        /// <summary>
        /// Memvalidasi baris-baris formasi awal terhadap aturan VR-WFP-11 sebelum disimpan/diajukan (kriteria 2).
        /// </summary>
        [HttpPost("validate")]
        [AccessAction("ValidateRows", "Validasi Baris Formasi", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkforceInitialFormationBatch", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<InitialFormationBatchRowValidationResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateRows(
            [FromBody] List<InitialFormationBatchRowRequest> rows,
            CancellationToken cancellationToken)
        {
            var result = await _service.ValidateRowsAsync(rows, null, cancellationToken);
            return Ok(ApiResponse<List<InitialFormationBatchRowValidationResponse>>.Ok(result, "Validasi baris formasi selesai."));
        }

        /// <summary>
        /// Mengambil daftar batch formasi awal berhalaman.
        /// </summary>
        [HttpGet]
        [AccessAction("ReadList", "Read Initial Formation Batches", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("WorkforceInitialFormationBatch", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<InitialFormationBatchListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] InitialFormationBatchPagedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedListAsync(query, cancellationToken);
            return Ok(ApiResponse<PagedResult<InitialFormationBatchListItemResponse>>.Ok(result, "Daftar batch formasi awal berhasil diambil."));
        }

        /// <summary>
        /// Mengambil detail satu batch formasi awal beserta baris-barisnya.
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("ReadDetail", "Read Initial Formation Batch Detail", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("WorkforceInitialFormationBatch", "Read")]
        [ProducesResponseType(typeof(ApiResponse<InitialFormationBatchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan."));
            }

            return Ok(ApiResponse<InitialFormationBatchResponse>.Ok(result, "Detail batch formasi awal berhasil diambil."));
        }

        /// <summary>
        /// Membuat draf batch formasi awal baru (status Draft).
        /// </summary>
        [HttpPost]
        [AccessAction("CreateDraft", "Create Initial Formation Batch Draft", AccessType = AccessTypes.Create, SortOrder = 5)]
        [AccessPermission("WorkforceInitialFormationBatch", "Create")]
        [ProducesResponseType(typeof(ApiResponse<InitialFormationBatchResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDraft(
            [FromBody] CreateInitialFormationBatchRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateDraftAsync(request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Memperbarui draf batch formasi awal selagi berstatus Draft atau Returned (kriteria 8).
        /// </summary>
        [HttpPut("{id:guid}")]
        [AccessAction("UpdateDraft", "Update Initial Formation Batch Draft", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceInitialFormationBatch", "Update")]
        [ProducesResponseType(typeof(ApiResponse<InitialFormationBatchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateDraft(
            [FromRoute] Guid id,
            [FromBody] UpdateInitialFormationBatchRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateDraftAsync(id, request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengunggah scan dokumen fisik SK formasi resmi (PDF/JPG/PNG max 10MB, kriteria 3).
        /// </summary>
        [HttpPost("{id:guid}/upload-source-document")]
        [Consumes("multipart/form-data")]
        [AccessAction("UploadSourceDocument", "Upload SK Formasi Resmi", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("WorkforceInitialFormationBatch", "Update")]
        [ProducesResponseType(typeof(ApiResponse<InitialFormationBatchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadSourceDocument(
            [FromRoute] Guid id,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var result = await _service.UploadSourceDocumentAsync(id, file, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengunduh berkas scan dokumen SK formasi resmi.
        /// </summary>
        [HttpGet("{id:guid}/source-document")]
        [AccessAction("DownloadSourceDocument", "Download Scan SK Formasi", AccessType = AccessTypes.Read, SortOrder = 8)]
        [AccessPermission("WorkforceInitialFormationBatch", "Read")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadSourceDocument(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var fileData = await _service.GetSourceDocumentStreamAsync(id, cancellationToken);
            if (fileData == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Berkas scan SK formasi tidak ditemukan pada batch ini."));
            }

            return File(fileData.Value.Stream, fileData.Value.ContentType, fileData.Value.FileName);
        }

        /// <summary>
        /// Mengajukan batch formasi ke workflow persetujuan T10 (kriteria 4 dan 5).
        /// </summary>
        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit Batch Formasi Awal", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("WorkforceInitialFormationBatch", "Update")]
        [ProducesResponseType(typeof(ApiResponse<InitialFormationBatchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Submit(
            [FromRoute] Guid id,
            [FromBody] SubmitInitialFormationBatchRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.SubmitAsync(id, request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Membatalkan draf atau dokumen yang dikembalikan (kriteria 7).
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Batch Formasi Awal", AccessType = AccessTypes.Delete, SortOrder = 10)]
        [AccessPermission("WorkforceInitialFormationBatch", "Delete")]
        [ProducesResponseType(typeof(ApiResponse<InitialFormationBatchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Cancel(
            [FromRoute] Guid id,
            [FromBody] CancelInitialFormationBatchRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(WorkforcePlanningResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
