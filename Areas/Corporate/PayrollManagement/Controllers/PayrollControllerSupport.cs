using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Pembantu kecil bersama controller Payroll: identitas pengguna dari klaim dan pemetaan eksepsi domain ke kode status HTTP.
/// Pemetaan mengikuti standar transaksi: 400 aksi tidak sah untuk status saat ini, 403 aktor tidak berwenang, 404 tidak ada,
/// 409 bentrok versi/kebijakan; 422 dipertahankan untuk kegagalan validasi data (kontrak yang sudah dipakai layar Payroll Run).
/// Kegagalan yang tidak dikenali TIDAK ditelan: dilempar ulang agar tercatat sebagai kesalahan server.
/// </summary>
internal static class PayrollControllerSupport
{
    public static Guid CurrentUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }

    public static bool IsHandled(Exception exception) =>
        exception is KeyNotFoundException
            or PayrollInvalidTransitionException
            or PayrollForbiddenActionException
            or PayrollRunConflictException
            or PayrollVersionConflictException
            or PayrollPolicyNotConfirmedException
            or PayrollRunValidationException
            or PayrollResultValidationException
            or ArgumentException;

    public static IActionResult Failure(Exception exception)
    {
        var code = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            PayrollInvalidTransitionException or ArgumentException => StatusCodes.Status400BadRequest,
            PayrollForbiddenActionException => StatusCodes.Status403Forbidden,
            PayrollRunConflictException or PayrollVersionConflictException or PayrollPolicyNotConfirmedException => StatusCodes.Status409Conflict,
            PayrollRunValidationException or PayrollResultValidationException => StatusCodes.Status422UnprocessableEntity,
            _ => throw exception
        };
        return new ObjectResult(ApiResponse<object>.Fail(code, exception.Message)) { StatusCode = code };
    }

    /// <summary>Menjalankan aksi layanan; hasil dibungkus <c>ApiResponse</c>, eksepsi domain dipetakan ke kode status.</summary>
    public static async Task<IActionResult> RunAsync<T>(Func<Task<T>> action, string message)
    {
        try
        {
            return new OkObjectResult(ApiResponse<T>.Ok(await action(), message));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }
}
