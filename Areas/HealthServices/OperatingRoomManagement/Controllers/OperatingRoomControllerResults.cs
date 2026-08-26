using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

/// <summary>
/// Pemetaan tunggal kegagalan domain modul operasi ke kode HTTP sesuai `opr-api-v1`:
/// `403` tidak berwenang, `409` benturan/transisi ilegal, `422` prasyarat klinis.
/// </summary>
internal static class OperatingRoomControllerResults
{
    public static ObjectResult OperatingRoomForbidden(this ControllerBase controller,
        OperatingRoomForbiddenException exception) =>
        controller.StatusCode(StatusCodes.Status403Forbidden,
            ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));

    public static ObjectResult OperatingRoomConflict(this ControllerBase controller,
        OperatingRoomConflictException exception) =>
        controller.StatusCode(StatusCodes.Status409Conflict,
            ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message, new { exception.Code }));

    public static ObjectResult OperatingRoomUnprocessable(this ControllerBase controller,
        OperatingRoomUnprocessableException exception) =>
        controller.StatusCode(StatusCodes.Status422UnprocessableEntity,
            ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message, new { exception.Code }));
}
