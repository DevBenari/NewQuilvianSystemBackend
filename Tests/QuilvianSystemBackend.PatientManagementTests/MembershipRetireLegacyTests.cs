using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using static QuilvianSystemBackend.Tests.PatientManagement.Infrastructure.MembershipHarness;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Penutupan rute kiosk dan pensiun endpoint lama — crm-membership `BE-CRM-015`,
/// `FR-CRM-041`, `FR-CRM-050` s.d. `FR-CRM-052`, `AT-22`, `AT-25`.
/// </summary>
public class MembershipRetireLegacyTests : IDisposable
{
    private readonly MembershipHarness _h = new(Wib(2026, 12, 1));

    public void Dispose() => _h.Dispose();

    private static IReadOnlyList<(string Template, string[] Methods)> RuteMembership()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiagnosticListener("uji"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());
        services.AddControllers().AddApplicationPart(typeof(PatientMembershipController).Assembly);
        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items
            .Where(x => x.AttributeRouteInfo?.Template?.Contains("patient-memberships", StringComparison.Ordinal) == true)
            .Select(x => (
                x.AttributeRouteInfo!.Template!,
                x.ActionConstraints?.OfType<HttpMethodActionConstraint>().SelectMany(c => c.HttpMethods).ToArray() ?? []))
            .ToList();
    }

    [Fact]
    public void RutePembuatanLewatKiosk_SudahTidakAda()
    {
        var rute = RuteMembership();
        var post = rute.Where(x => x.Methods.Contains("POST")).Select(x => x.Template).ToList();

        Assert.DoesNotContain(post, x => x.EndsWith("patient-memberships", StringComparison.Ordinal));
        Assert.DoesNotContain(post, x => x.EndsWith("patient-memberships/kiosk", StringComparison.Ordinal));
        Assert.Contains(post, x => x.EndsWith("patient-memberships/admin", StringComparison.Ordinal));

        // Akses baca kiosk tetap ada.
        Assert.Contains(rute, x => x.Template.EndsWith("patient-memberships/kiosk/options", StringComparison.Ordinal) && x.Methods.Contains("GET"));
    }

    [Fact]
    public void EndpointLama_Menjawab410_DenganPetunjukPengganti()
    {
        using var k = _h.Konteks();
        var controller = _h.Controller(k);
        var id = Guid.NewGuid();

        AssertGone(controller.UpdatePatientMembership(id), "Fitur ubah membership sudah tidak tersedia. Gunakan ubah catatan, upgrade, atau pembatalan.");
        AssertGone(controller.UpdatePatientMembershipStatus(id), "Ubah status membership sudah tidak tersedia. Gunakan pembatalan atau verifikasi.");
        AssertGone(controller.DeletePatientMembership(id), "Hapus membership sudah tidak tersedia. Gunakan pembatalan dengan alasan.");
    }

    [Fact]
    public void PasienBaru_TanpaMengisiIsMember_TersimpanBukanMember()
    {
        // Sebelum BE-CRM-015 default kolom true selalu menang saat insert (penyebab member palsu).
        var id = Guid.NewGuid();
        using (var k = _h.Konteks())
        {
            k.Set<MstPatient>().Add(new MstPatient { Id = id, PatientCode = "P-BARU", MedicalRecordNumber = "00-55-01", FullName = "Pasien Baru" });
            k.SaveChanges();
        }

        using var baca = _h.Konteks();
        Assert.False(baca.Set<MstPatient>().AsNoTracking().Single(x => x.Id == id).IsMember);
    }

    [Fact]
    public void MembershipControllerTidakLagiPunyaAksiPembuatanLama()
    {
        Assert.Null(typeof(PatientMembershipController).GetMethod("CreatePatientMembership"));
    }

    private static void AssertGone(IActionResult hasil, string pesan)
    {
        var obj = Assert.IsType<ObjectResult>(hasil);
        Assert.Equal(StatusCodes.Status410Gone, obj.StatusCode);
        Assert.Equal(pesan, Assert.IsType<ApiResponse<object>>(obj.Value).Message);
    }
}
