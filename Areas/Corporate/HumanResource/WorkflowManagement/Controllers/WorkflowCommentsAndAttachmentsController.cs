using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/workflow/instances/{id}")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFLOW",
        moduleName: "Human Resource Workflow",
        displayName: "Komentar & Lampiran Workflow",
        AreaName = "Corporate",
        ControllerName = "WorkflowCommentAttachment",
        Description = "Utas komentar klarifikasi dan lampiran pendukung permohonan",
        SortOrder = 22)]
    [Tags("Corporate - HRD Workflow Comments & Attachments")]
    public class WorkflowCommentsAndAttachmentsController : ControllerBase
    {
        private readonly IWorkflowCommentAndAttachmentService _commentAndAttachmentService;

        public WorkflowCommentsAndAttachmentsController(IWorkflowCommentAndAttachmentService commentAndAttachmentService)
        {
            _commentAndAttachmentService = commentAndAttachmentService;
        }

        private Guid GetCurrentEmployeeId()
        {
            var claim = User.FindFirst("EmployeeId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var empId)) return empId;
            return Guid.Empty;
        }

        /// <summary>
        /// Mengambil riwayat percakapan thread dan catatan revisi permohonan (FR-WF-20)
        /// </summary>
        [HttpGet("comments")]
        [AccessAction("ViewInstanceDetail", "Melihat Komentar & Lampiran", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowCommentAttachment", "ViewInstanceDetail")]
        [ProducesResponseType(typeof(ApiResponse<List<WorkflowCommentV2ResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetComments([FromRoute] Guid id)
        {
            var comments = await _commentAndAttachmentService.GetCommentsAsync(id);
            return Ok(ApiResponse<List<WorkflowCommentV2ResponseDto>>.Ok(comments));
        }

        /// <summary>
        /// Menambahkan komentar klarifikasi baru pada tiket permohonan (FR-WF-20)
        /// </summary>
        [HttpPost("comments")]
        [AccessAction("AddComment", "Menambahkan Komentar", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkflowCommentAttachment", "AddComment")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowCommentV2ResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddComment([FromRoute] Guid id, [FromBody] AddWorkflowCommentRequestDto request)
        {
            try
            {
                var empId = GetCurrentEmployeeId();
                var result = await _commentAndAttachmentService.AddCommentAsync(id, request, empId);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<WorkflowCommentV2ResponseDto>.Ok(result, "Komentar berhasil ditambahkan"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<WorkflowCommentV2ResponseDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WorkflowCommentV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Mengambil daftar berkas lampiran pendukung permohonan (FR-WF-20)
        /// </summary>
        [HttpGet("attachments")]
        [AccessAction("ViewInstanceDetail", "Melihat Komentar & Lampiran", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowCommentAttachment", "ViewInstanceDetail")]
        [ProducesResponseType(typeof(ApiResponse<List<WorkflowAttachmentV2ResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAttachments([FromRoute] Guid id)
        {
            var attachments = await _commentAndAttachmentService.GetAttachmentsAsync(id);
            return Ok(ApiResponse<List<WorkflowAttachmentV2ResponseDto>>.Ok(attachments));
        }

        /// <summary>
        /// Mengunggah berkas lampiran pendukung baru (PDF/JPG maksimal 10MB) (FR-WF-20)
        /// </summary>
        [HttpPost("attachments")]
        [AccessAction("UploadAttachment", "Mengunggah Lampiran", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("WorkflowCommentAttachment", "UploadAttachment")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowAttachmentV2ResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAttachment([FromRoute] Guid id, IFormFile file, [FromForm] string? description)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<WorkflowAttachmentV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, "Berkas lampiran tidak boleh kosong"));
            }

            try
            {
                var empId = GetCurrentEmployeeId();
                var result = await _commentAndAttachmentService.UploadAttachmentAsync(id, file, description, empId);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<WorkflowAttachmentV2ResponseDto>.Ok(result, "Berkas lampiran berhasil diunggah"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<WorkflowAttachmentV2ResponseDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WorkflowAttachmentV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }
    }
}
