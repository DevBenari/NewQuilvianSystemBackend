using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/register/attachments")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Register",
    AreaName = "Corporate", ControllerName = "AssetRegister", Description = "Pencatatan, persetujuan, dan penomoran aset", SortOrder = 1)]
[Tags("Corporate / Asset Management / Register / Asset Attachment")]
public sealed class AssetAttachmentsController : ControllerBase
{
    private readonly AssetAttachmentAccessService _service;

    public AssetAttachmentsController(AssetAttachmentAccessService service) => _service = service;

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Register", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetRegister", "Read")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var content = await _service.OpenAsync(id, cancellationToken);
            return File(content.Stream, content.ContentType, content.FileName);
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }
}
