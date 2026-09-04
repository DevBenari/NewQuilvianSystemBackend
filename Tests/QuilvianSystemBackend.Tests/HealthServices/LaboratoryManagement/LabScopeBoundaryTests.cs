using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Reflection;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Batas modul Laboratorium — <c>AC-42</c> dan <c>AC-19</c>.
///
/// Dua hal yang <b>sengaja tidak</b> dilayani modul ini:
///   1. <b>Bank Darah</b> (<c>AC-42</c>). Bank Darah punya alur, regulasi, dan penelusuran
///      kantongnya sendiri. Menaruhnya di bawah Laboratorium karena "sama-sama darah" akan
///      menyembunyikan alur yang sebenarnya jauh berbeda;
///   2. <b>Stok, pembelian, dan pemakaian reagen</b> (<c>AC-19</c>, <c>LAB-DEC-014</c>).
///      Persediaan adalah urusan logistik, bukan pemeriksaan.
///
/// Keduanya adalah batas yang mudah dilanggar tanpa disadari — biasanya lewat satu kolom yang
/// "sekalian dibuat". Uji ini menelusuri seluruh tipe, anggota, entity, dan route modul
/// Laboratorium, dan gagal begitu istilah keduanya muncul.
/// </summary>
public class LabScopeBoundaryTests
{
    private const string NamespaceLaboratorium =
        "QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement";

    /// <summary>Istilah Bank Darah yang tidak boleh muncul di mana pun pada modul ini.</summary>
    private static readonly string[] IstilahBankDarah =
    {
        "BloodBank",
        "BloodBag",
        "BloodComponent",
        "BloodProduct",
        "BloodGroup",
        "Donor",
        "Transfusion",
        "Crossmatch"
    };

    /// <summary>Istilah persediaan reagen yang tidak boleh muncul di mana pun pada modul ini.</summary>
    private static readonly string[] IstilahReagen =
    {
        "Reagent",
        "Reagen",
        "Inventory",
        "StockOpname",
        "StockBalance",
        "Restock",
        "Purchase",
        "Procurement",
        "Supplier",
        "Consumable"
    };

    [Fact]
    public void AC42_TidakSatuPunTipeAtauAnggotaLaboratorium_MelayaniBankDarah()
    {
        var temuan = TelusuriIstilah(IstilahBankDarah);

        Assert.Empty(temuan);
    }

    [Fact]
    public void AC19_TidakSatuPunTipeAtauAnggotaLaboratorium_MenyimpanStokPembelianAtauPemakaianReagen()
    {
        var temuan = TelusuriIstilah(IstilahReagen);

        Assert.Empty(temuan);
    }

    [Fact]
    public void AC42_TidakSatuPunEntityTersimpan_MelayaniBankDarahMaupunReagen()
    {
        using var context = CreateContext();

        var entityLaboratorium = context.Model
            .GetEntityTypes()
            .Where(x => x.ClrType.Namespace != null &&
                        x.ClrType.Namespace.StartsWith(NamespaceLaboratorium, StringComparison.Ordinal))
            .ToList();

        // Tanpa ini, uji lulus hanya karena tidak menemukan entity Laboratorium sama sekali.
        Assert.NotEmpty(entityLaboratorium);

        var terlarang = IstilahBankDarah.Concat(IstilahReagen).ToList();

        var temuan = entityLaboratorium
            .SelectMany(entity => entity.GetProperties()
                .Select(p => $"{entity.ClrType.Name}.{p.Name}")
                .Append(entity.ClrType.Name))
            .Where(nama => terlarang.Any(x => nama.Contains(x, StringComparison.OrdinalIgnoreCase)))
            .Distinct()
            .ToList();

        Assert.Empty(temuan);
    }

    [Fact]
    public void AC42_TidakSatuPunRouteLaboratorium_MelayaniBankDarahMaupunReagen()
    {
        var controller = typeof(LabSpecimen).Assembly
            .GetTypes()
            .Where(x =>
                x.Namespace != null &&
                x.Namespace.StartsWith($"{NamespaceLaboratorium}.Controllers", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(controller);

        var terlarang = IstilahBankDarah.Concat(IstilahReagen).ToList();

        var routes = controller
            .SelectMany(x => x.GetCustomAttributes<RouteAttribute>()
                .Select(r => r.Template ?? string.Empty))
            .Concat(controller
                .SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
                .Select(x => x.Template ?? string.Empty))
            .ToList();

        Assert.NotEmpty(routes);

        var temuan = routes
            .Where(route => terlarang.Any(x => route.Contains(x, StringComparison.OrdinalIgnoreCase)))
            .Distinct()
            .ToList();

        Assert.Empty(temuan);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    /// <summary>
    /// Menelusuri nama tipe beserta nama properti dan method publiknya pada seluruh berkas
    /// modul Laboratorium — model, DTO, service, controller, dan enum sekaligus.
    /// </summary>
    private static List<string> TelusuriIstilah(IReadOnlyList<string> terlarang)
    {
        var tipe = typeof(LabSpecimen).Assembly
            .GetTypes()
            .Where(x => x.Namespace != null &&
                        x.Namespace.StartsWith(NamespaceLaboratorium, StringComparison.Ordinal))
            .ToList();

        // Tanpa ini, uji lulus hanya karena tidak menemukan satu pun tipe Laboratorium.
        Assert.NotEmpty(tipe);

        var nama = new List<string>();

        foreach (var t in tipe)
        {
            nama.Add(t.Name);

            nama.AddRange(t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Select(x => $"{t.Name}.{x.Name}"));

            nama.AddRange(t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(x => $"{t.Name}.{x.Name}"));

            if (t.IsEnum)
                nama.AddRange(Enum.GetNames(t).Select(x => $"{t.Name}.{x}"));
        }

        return nama
            .Where(x => terlarang.Any(k => x.Contains(k, StringComparison.OrdinalIgnoreCase)))
            .Distinct()
            .ToList();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-scope-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
