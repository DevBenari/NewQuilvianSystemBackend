using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public sealed class InpBedOperationFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is QuilvianSystemBackend.Areas.HealthServices.MasterData.Services.MasterWriteRejectedException rejected)
        {
            context.Result = rejected.Result; context.ExceptionHandled = true; return Task.CompletedTask;
        }
        if (context.Exception is InpBedOperationException error)
        {
            context.Result = new ObjectResult(ApiResponse<object>.Fail(error.StatusCode, error.Message, new { code = error.Code })) { StatusCode = error.StatusCode };
            context.ExceptionHandled = true;
        }
        return Task.CompletedTask;
    }
}

