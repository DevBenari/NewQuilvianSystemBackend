using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Gerbang satu jalur kalkulasi payroll: menjaga jalur lama (modul "Modern") agar tidak menjalankan perhitungan paralel terhadap Payroll
/// Run resmi. Berlaku hanya untuk aksi tulis (POST, PUT, PATCH, DELETE); pembacaan riwayat tidak disentuh dan tidak ada data historis
/// yang dihapus.
///
/// 1. Kalkulasi PPh jalur lama SELALU ditolak: tarifnya tertanam atau berasal dari nilai bawaan yang belum diverifikasi.
///    Tarif resmi hanya dari kebijakan berversi yang disahkan dan dibaca mesin Payroll Run. (Endpoint kalkulasi BPJS jalur lama sudah
///    dihapus bersama layanannya pada PAY-BE-FIX-007.)
/// 2. Run yang sudah dihitung mesin Payroll Run resmi tidak dapat diubah jalur lama (mencegah komponen dan potongan ganda).
/// 3. Bila saklar <c>Payroll:Policy:LegacyModernMutationsEnabled</c> dimatikan, seluruh aksi tulis jalur lama ditolak.
/// </summary>
public sealed class LegacyModernPayrollGateFilter : IAsyncActionFilter
{
    /// <summary>Aksi kalkulasi pajak jalur lama: selalu diblokir.</summary>
    private static readonly HashSet<string> AlwaysBlockedActions = new(StringComparer.Ordinal)
    {
        "CalculatePph21", "CalculateEmployeePph21"
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly PayrollPolicyOptions _options;

    public LegacyModernPayrollGateFilter(ApplicationDbContext dbContext, IOptions<PayrollPolicyOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    private static IActionResult Reject(string message) =>
        new ObjectResult(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, message))
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity
        };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        var isWrite = HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);
        if (!isWrite)
        {
            await next();
            return;
        }

        var actionName = (context.ActionDescriptor as ControllerActionDescriptor)?.ActionName ?? string.Empty;
        if (AlwaysBlockedActions.Contains(actionName))
        {
            context.Result = Reject(PayrollTaxInsuranceRules.LegacyTaxInsuranceBlockedMessage);
            return;
        }

        if (!_options.LegacyModernMutationsEnabled)
        {
            context.Result = Reject(PayrollTaxInsuranceRules.LegacyPathDisabledMessage);
            return;
        }

        if (context.RouteData.Values.TryGetValue("id", out var rawId) && Guid.TryParse(rawId?.ToString(), out var runId))
        {
            var policySnapshot = await _dbContext.PrlPayrollRuns.AsNoTracking()
                .Where(x => x.Id == runId)
                .Select(x => x.PolicySnapshotJson)
                .FirstOrDefaultAsync(context.HttpContext.RequestAborted);
            if (PayrollTaxInsuranceRules.IsCalculatedByOfficialEngine(policySnapshot))
            {
                context.Result = Reject(PayrollTaxInsuranceRules.LegacyEngineBlockedMessage);
                return;
            }
        }

        await next();
    }
}
