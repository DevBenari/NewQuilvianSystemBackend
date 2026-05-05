using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Filters
{
    public class AccessPermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly AccessPermissionService _accessPermissionService;
        private readonly LoggerService _loggerService;
        private readonly string _controllerName;
        private readonly string _actionName;

        public AccessPermissionFilter(
            AccessPermissionService accessPermissionService,
            LoggerService loggerService,
            string controllerName,
            string actionName)
        {
            _accessPermissionService = accessPermissionService;
            _loggerService = loggerService;
            _controllerName = controllerName;
            _actionName = actionName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var isAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated == true;

            if (!isAuthenticated)
            {
                context.Result = new UnauthorizedObjectResult(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Anda belum login atau session sudah berakhir."
                ));

                return;
            }

            var hasAccess = await _accessPermissionService.HasAccessAsync(
                context.HttpContext.User,
                _controllerName,
                _actionName
            );

            if (hasAccess)
            {
                return;
            }

            await _loggerService.WarningAsync(
                "Security",
                "AccessDenied",
                "Akses ditolak.",
                new
                {
                    ControllerName = _controllerName,
                    ActionName = _actionName
                }
            );

            context.Result = new ObjectResult(ApiResponse<object>.Fail(
                StatusCodes.Status403Forbidden,
                "Anda tidak memiliki akses ke fitur ini."
            ))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}