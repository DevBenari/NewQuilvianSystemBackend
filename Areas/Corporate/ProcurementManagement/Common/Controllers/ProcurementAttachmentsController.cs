using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Controllers;

/// <summary>
/// Lampiran dokumen Pengadaan (BE-PBJ-004, PBJ-DEC-023). Sub-resource yang dicakup dokumen
/// induknya, bukan master data. Setiap aksi juga memeriksa hak atas dokumen induk lewat
/// <see cref="IProcurementDocumentAccessProvider"/> di service; dokumen di luar cakupan → 404.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/attachments")]
[AccessController(ProcurementPermissions.CommonModuleCode, ProcurementPermissions.CommonModuleName, "Procurement Attachment",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.AttachmentResource,
    Description = "Lampiran dokumen Pengadaan", SortOrder = 1)]
[Tags("Corporate / Procurement Management / Common / Attachment")]
public sealed class ProcurementAttachmentsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.AttachmentResource;
    private readonly ProcurementAttachmentService _service;

    public ProcurementAttachmentsController(ProcurementAttachmentService service) => _service = service;

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Attachment", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<ProcurementAttachmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Get([FromQuery] ProcurementAttachmentQuery query, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetAsync(User, query, cancellationToken), "Daftar lampiran berhasil diambil.");

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(25L * 1024L * 1024L)]
    [AccessAction(ProcurementPermissions.UploadAction, "Upload Procurement Attachment", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.UploadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementAttachmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Upload([FromForm] UploadProcurementAttachmentRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UploadAsync(User, request, CurrentUserId(), cancellationToken), "Lampiran berhasil diunggah.");

    [HttpGet("{id:guid}/download")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Attachment", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _service.GetDownloadAsync(User, id, cancellationToken);
            // Nama unduhan membuat Content-Disposition: attachment, sehingga peramban tidak merender isinya.
            return PhysicalFile(file.PhysicalPath, file.ContentType, file.FileName);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(404, exception.Message));
        }
    }

    [HttpPost("{id:guid}/deactivate")]
    [AccessAction(ProcurementPermissions.UploadAction, "Upload Procurement Attachment", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.UploadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementAttachmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeactivateAsync(User, id, CurrentUserId(), cancellationToken), "Lampiran berhasil ditarik.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message)
    {
        try { return Ok(ApiResponse<T>.Ok(await command(), message)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementAttachmentConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
        catch (ProcurementAttachmentValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
