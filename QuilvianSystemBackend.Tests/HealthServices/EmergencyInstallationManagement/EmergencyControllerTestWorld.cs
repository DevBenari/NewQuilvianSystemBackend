using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.HealthServices.EmergencyInstallationManagement;

/// <summary>
/// Dunia uji bersama untuk controller IGD: satu <c>ApplicationDbContext</c> InMemory, satu
/// <c>LoggerService</c> yang benar-benar dapat dipanggil, dan controller yang sudah punya
/// <c>ControllerContext</c> sehingga <c>GetCurrentUserId</c> tidak melempar.
/// </summary>
/// <remarks>
/// Dibuat oleh <c>BE-IGD-021</c>. Sebelumnya seluruh test IGD berhenti di lapisan service,
/// sehingga kode balik <c>409</c> hanya dapat dibuktikan lewat penelusuran kode. Percabangan
/// yang diperbaiki <c>BE-IGD-021</c> justru berada di controller, jadi pembuktiannya harus
/// ikut naik ke sana.
///
/// <para>
/// Provider InMemory tetap tidak menjalankan pipeline MVC — model binding, filter otorisasi,
/// dan atribut <c>[AccessPermission]</c> tidak ikut berjalan. Yang dibuktikan di sini adalah
/// <b>isi metode aksinya</b>: status apa yang tersimpan, dan <c>IActionResult</c> apa yang
/// dikembalikan.
/// </para>
/// </remarks>
internal static class EmergencyControllerTestWorld
{
    internal static ApplicationDbContext BuatContext(string prefix)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"{prefix}-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    internal static LoggerService BuatLoggerService()
        => new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

    internal static EmergencyVisitService BuatVisitService(ApplicationDbContext context)
        => new(context, new EmergencyDocumentNumberService());

    /// <summary>
    /// Memasang <c>HttpContext</c> berisi satu klaim identitas, supaya
    /// <c>GetCurrentUserId</c> mengembalikan pelaku yang dapat diperiksa test.
    /// </summary>
    internal static T DenganPelaku<T>(this T controller, Guid actorUserId)
        where T : ControllerBase
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return controller;
    }

    internal static EmergencyObservationController BuatObservationController(
        ApplicationDbContext context,
        Guid actorUserId)
        => new EmergencyObservationController(
                context,
                BuatLoggerService(),
                new EmergencyObservationService(context, new EmergencyDocumentNumberService()),
                BuatVisitService(context))
            .DenganPelaku(actorUserId);

    internal static EmergencyResuscitationController BuatResuscitationController(
        ApplicationDbContext context,
        Guid actorUserId)
        => new EmergencyResuscitationController(
                context,
                BuatLoggerService(),
                new EmergencyResuscitationService(context, new EmergencyDocumentNumberService()),
                BuatVisitService(context))
            .DenganPelaku(actorUserId);

    internal static EmergencyDispositionController BuatDispositionController(
        ApplicationDbContext context,
        Guid actorUserId)
        => new EmergencyDispositionController(
                context,
                BuatLoggerService(),
                new EmergencyDispositionService(context),
                BuatVisitService(context))
            .DenganPelaku(actorUserId);

    internal static EmergencyVisitController BuatVisitController(
        ApplicationDbContext context,
        Guid actorUserId)
        => new EmergencyVisitController(
                context,
                BuatLoggerService(),
                BuatVisitService(context),
                new EmergencyDispositionService(context))
            .DenganPelaku(actorUserId);

    internal static async Task<TrxEmergencyVisit> SimpanKunjunganAsync(
        ApplicationDbContext context,
        EmergencyVisitStatus status)
    {
        var visit = new TrxEmergencyVisit
        {
            Id = Guid.NewGuid(),
            EmergencyVisitNumber = $"IGD{Guid.NewGuid():N}"[..12],
            VisitStatus = status,
            IsDelete = false,
        };

        context.Set<TrxEmergencyVisit>().Add(visit);
        await context.SaveChangesAsync();
        return visit;
    }

    /// <summary>
    /// Membaca kode status HTTP dari <c>IActionResult</c> yang dikembalikan aksi controller.
    /// </summary>
    internal static int KodeStatus(IActionResult result) => result switch
    {
        ObjectResult objectResult => objectResult.StatusCode ?? 0,
        StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
        _ => 0
    };

    /// <summary>
    /// Membaca pesan pada <c>ApiResponse</c> apa pun bentuk generiknya.
    /// </summary>
    internal static string? Pesan(IActionResult result)
    {
        if (result is not ObjectResult { Value: not null } objectResult)
            return null;

        var property = objectResult.Value.GetType().GetProperty(nameof(ApiResponse<object>.Message));
        return property?.GetValue(objectResult.Value) as string;
    }
}
