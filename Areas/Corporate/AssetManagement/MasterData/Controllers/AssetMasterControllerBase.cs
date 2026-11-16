using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;

public abstract class AssetMasterControllerBase : ControllerBase
{
    protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action, string successMessage)
    {
        try
        {
            return Ok(ApiResponse<T>.Ok(await action(), successMessage));
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }

    protected Guid CurrentUserId()
    {
        var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
