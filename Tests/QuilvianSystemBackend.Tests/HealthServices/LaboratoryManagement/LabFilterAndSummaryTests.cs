using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-17</c> — endpoint <c>filters/metadata</c> dan <c>summary</c> pada
/// kelima grup Laboratorium, amandemen <c>LAB-API-v1</c> atas instruksi pemilik modul.
///
/// Yang dibuktikan di sini:
///   1. kesepuluh endpoint ada dengan route, verb, dan hak akses yang benar;
///   2. metadata memuat <b>seluruh</b> nilai enum yang berlaku, bukan sebagian;
///   3. metadata menyatakan kemampuan penyaringan <b>apa adanya</b> — grup yang daftarnya belum
///      menyaring di sisi server tidak boleh mengaku bisa;
///   4. metadata membawa penanda keselamatan yang sudah ditegakkan service, sehingga layar
///      mengunci jalannya sejak awal, bukan setelah gagal menyimpan;
///   5. setiap rekap menghitung angka yang benar dari data yang belum ditandai terhapus.
/// </summary>
/// <remarks>
/// Metadata murni keterangan bentuk dan tidak menyentuh database, sehingga diuji langsung dari
/// factory-nya. Rekap menyentuh database dan diuji lewat provider InMemory.
/// </remarks>
public class LabFilterAndSummaryTests
{
    private static readonly Guid Aktor = Guid.Parse("77777777-7777-7777-7777-777777777777");

    // =====================================================================
    // 1. Bentuk kontrak — kesepuluh endpoint
    // =====================================================================

    [Theory]
    [InlineData(typeof(LabOrderController), "GetFilterMetadata", "LabOrder", "Read", "filters/metadata")]
    [InlineData(typeof(LabOrderController), "GetSummary", "LabOrder", "Read", "summary")]
    [InlineData(typeof(LabSpecimenController), "GetFilterMetadata", "LabSpecimen", "Read", "filters/metadata")]
    [InlineData(typeof(LabSpecimenController), "GetSummary", "LabSpecimen", "Read", "summary")]
    [InlineData(typeof(LabValueBoundController), "GetFilterMetadata", "LabValueBound", "Read", "filters/metadata")]
    [InlineData(typeof(LabValueBoundController), "GetSummary", "LabValueBound", "Read", "summary")]
    [InlineData(typeof(LabCriticalBoundApprovalController), "GetFilterMetadata", "LabCriticalBound", "Read", "filters/metadata")]
    [InlineData(typeof(LabCriticalBoundApprovalController), "GetSummary", "LabCriticalBound", "Read", "summary")]
    [InlineData(typeof(LabRejectionReasonController), "GetFilterMetadata", "LabRejectionReason", "Read", "filters/metadata")]
    [InlineData(typeof(LabRejectionReasonController), "GetSummary", "LabRejectionReason", "Read", "summary")]
    public void KesepuluhEndpoint_MemakaiRouteVerbDanHakAksesYangBenar(
        Type controller,
        string methodName,
        string resource,
        string action,
        string template)
    {
        var method = controller.GetMethod(methodName);

        Assert.True(method != null, $"{controller.Name}.{methodName} tidak ditemukan.");

        // Keduanya membaca, sehingga wajib GET.
        var verb = method!.GetCustomAttributes<HttpGetAttribute>(inherit: false).SingleOrDefault();

        Assert.True(verb != null, $"{controller.Name}.{methodName} bukan endpoint GET.");
        Assert.Equal(template, ((IRouteTemplateProvider)verb!).Template);

        var permission = method.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.True(permission != null, $"{controller.Name}.{methodName} tanpa AccessPermission.");

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal(resource, arguments[0]);
        Assert.Equal(action, arguments[1]);
    }

    /// <summary>
    /// Keduanya hanya membaca. Bila kelak ada yang menambahkan verb pengubah pada salah satunya,
    /// uji ini gagal lebih dulu.
    /// </summary>
    [Theory]
    [InlineData(typeof(LabOrderController))]
    [InlineData(typeof(LabSpecimenController))]
    [InlineData(typeof(LabValueBoundController))]
    [InlineData(typeof(LabCriticalBoundApprovalController))]
    [InlineData(typeof(LabRejectionReasonController))]
    public void EndpointMetadataDanSummary_SelaluBacaSaja(Type controller)
    {
        foreach (var methodName in new[] { "GetFilterMetadata", "GetSummary" })
        {
            var method = controller.GetMethod(methodName);

            Assert.NotNull(method);
            Assert.Empty(method!.GetCustomAttributes<HttpPostAttribute>(inherit: false));
            Assert.Empty(method.GetCustomAttributes<HttpPutAttribute>(inherit: false));
            Assert.Empty(method.GetCustomAttributes<HttpPatchAttribute>(inherit: false));
            Assert.Empty(method.GetCustomAttributes<HttpDeleteAttribute>(inherit: false));
        }
    }

    // =====================================================================
    // 2. Metadata memuat seluruh nilai enum
    // =====================================================================

    [Fact]
    public void MetadataPesanan_MemuatSeluruhStatusDanSeluruhDisiplin()
    {
        var metadata = LabFilterMetadataFactory.LabOrder();

        Assert.Equal(
            Enum.GetValues<LabOrderStatus>().Length,
            metadata.OrderStatuses.Count);
        Assert.Equal(
            Enum.GetValues<LabDiscipline>().Length,
            metadata.Disciplines.Count);

        // Nilai yang dikirim balik ke API adalah angka enumnya, bukan urutan daftar.
        Assert.Equal((int)LabOrderStatus.Cancelled, metadata.OrderStatuses.Single(x => x.Name == "Cancelled").Value);
        Assert.Equal("Dibatalkan", metadata.OrderStatuses.Single(x => x.Name == "Cancelled").Label);
        Assert.Equal("Patologi Klinik", metadata.Disciplines.Single(x => x.Name == "ClinicalPathology").Label);
    }

    [Fact]
    public void MetadataWadah_MemuatSeluruhStatusDanSeluruhSebabAmbilUlang()
    {
        var metadata = LabFilterMetadataFactory.LabSpecimen();

        Assert.Equal(Enum.GetValues<LabSpecimenStatus>().Length, metadata.SpecimenStatuses.Count);
        Assert.Equal(Enum.GetValues<LabRecollectionCause>().Length, metadata.RecollectionCauses.Count);

        Assert.Equal("Dinyatakan layak", metadata.SpecimenStatuses.Single(x => x.Name == "Accepted").Label);
        Assert.Equal(
            "Kesalahan internal rumah sakit",
            metadata.RecollectionCauses.Single(x => x.Name == "InternalHospitalError").Label);
    }

    [Fact]
    public void MetadataBatasNilai_MemuatSeluruhBentukHasilDanJenisKelamin()
    {
        var metadata = LabFilterMetadataFactory.LabValueBound();

        Assert.Equal(Enum.GetValues<LabResultForm>().Length, metadata.ResultForms.Count);
        Assert.Equal(Enum.GetValues<LabGenderScope>().Length, metadata.GenderScopes.Count);

        Assert.Equal("Angka", metadata.ResultForms.Single(x => x.Name == "Numeric").Label);
        Assert.Equal("Laki-laki", metadata.GenderScopes.Single(x => x.Name == "Male").Label);
    }

    [Fact]
    public void MetadataPengajuan_MemuatSeluruhStatusPengajuan()
    {
        var metadata = LabFilterMetadataFactory.LabCriticalBoundApproval();

        Assert.Equal(Enum.GetValues<LabBoundChangeStatus>().Length, metadata.RequestStatuses.Count);
        Assert.Equal("Diajukan", metadata.RequestStatuses.Single(x => x.Name == "Submitted").Label);
    }

    [Fact]
    public void SeluruhMetadata_MemakaiUkuranHalamanDanArahUrutYangSeragam()
    {
        var seluruhArah = new[]
        {
            LabFilterMetadataFactory.LabOrder().SortDirections,
            LabFilterMetadataFactory.LabSpecimen().SortDirections,
            LabFilterMetadataFactory.LabValueBound().SortDirections,
            LabFilterMetadataFactory.LabCriticalBoundApproval().SortDirections,
            LabFilterMetadataFactory.LabRejectionReason().SortDirections
        };

        Assert.All(seluruhArah, x => Assert.Equal(new[] { "asc", "desc" }, x));

        var seluruhUkuran = new[]
        {
            LabFilterMetadataFactory.LabOrder().PageSizeOptions,
            LabFilterMetadataFactory.LabSpecimen().PageSizeOptions,
            LabFilterMetadataFactory.LabValueBound().PageSizeOptions,
            LabFilterMetadataFactory.LabCriticalBoundApproval().PageSizeOptions,
            LabFilterMetadataFactory.LabRejectionReason().PageSizeOptions
        };

        Assert.All(seluruhUkuran, x => Assert.Equal(new[] { 10, 25, 50, 100 }, x));
    }

    // =====================================================================
    // 3. Metadata jujur soal kemampuan penyaringan
    // =====================================================================

    /// <summary>
    /// Aturan yang paling mudah dilanggar: menjanjikan penyaring yang tidak diproses daftar.
    /// Sejak <c>BE-LAB-18</c>, <c>GET /lab-orders</c> benar-benar menyaring, sehingga
    /// metadata-nya wajib mengaku demikian <b>dan</b> setiap parameter yang diumumkannya wajib
    /// ada pada penyaringnya.
    /// </summary>
    [Fact]
    public void MetadataPesanan_MengakuMenyaringDanSeluruhParameternyaNyata()
    {
        var metadata = LabFilterMetadataFactory.LabOrder();

        Assert.True(metadata.SupportsServerSideFiltering);
        Assert.True(metadata.SupportsServerSidePaging);
        Assert.NotEmpty(metadata.QueryParameters);

        var getList = typeof(LabOrderController).GetMethod("GetList");

        Assert.NotNull(getList);
        Assert.Contains(
            getList!.GetParameters(),
            x => x.GetCustomAttribute<FromQueryAttribute>() != null);

        var ruasPenyaring = typeof(QuilvianSystemBackend.Areas.HealthServices
                .LaboratoryManagement.DTOs.LabOrderPagedQuery)
            .GetProperties()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.All(
            metadata.QueryParameters,
            x => Assert.True(
                ruasPenyaring.Contains(x.Name),
                $"Metadata mengumumkan parameter {x.Name} yang tidak ada pada LabOrderPagedQuery."));
    }

    [Fact]
    public void MetadataBatasNilaiDanAlasanPenolakan_MengakuMenyaringDiSisiServer()
    {
        var batasNilai = LabFilterMetadataFactory.LabValueBound();
        var alasan = LabFilterMetadataFactory.LabRejectionReason();

        Assert.True(batasNilai.SupportsServerSideFiltering);
        Assert.True(batasNilai.SupportsServerSidePaging);
        Assert.True(alasan.SupportsServerSideFiltering);
        Assert.True(alasan.SupportsServerSidePaging);

        // Keduanya benar-benar menerima parameter query pada daftarnya.
        foreach (var (controller, metode) in new[]
        {
            (typeof(LabValueBoundController), "GetList"),
            (typeof(LabRejectionReasonController), "GetList")
        })
        {
            var method = controller.GetMethod(metode);

            Assert.NotNull(method);
            Assert.Contains(
                method!.GetParameters(),
                x => x.GetCustomAttribute<FromQueryAttribute>() != null);
        }
    }

    /// <summary>
    /// Setiap parameter yang diumumkan metadata batas nilai wajib benar-benar ada pada
    /// penyaring daftarnya. Metadata yang menjanjikan ruas yang tidak diproses adalah cacat
    /// kontrak, bukan dokumentasi yang usang.
    /// </summary>
    [Fact]
    public void ParameterYangDiumumkanMetadataBatasNilai_AdaPadaPenyaringDaftarnya()
    {
        var metadata = LabFilterMetadataFactory.LabValueBound();

        var ruasPenyaring = typeof(QuilvianSystemBackend.Areas.HealthServices
                .LaboratoryManagement.DTOs.LabValueBoundPagedQuery)
            .GetProperties()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.All(
            metadata.QueryParameters,
            x => Assert.True(
                ruasPenyaring.Contains(x.Name),
                $"Metadata mengumumkan parameter {x.Name} yang tidak ada pada LabValueBoundPagedQuery."));
    }

    [Fact]
    public void ParameterYangDiumumkanMetadataAlasanPenolakan_AdaPadaPenyaringDaftarnya()
    {
        var metadata = LabFilterMetadataFactory.LabRejectionReason();

        var ruasPenyaring = typeof(QuilvianSystemBackend.Areas.HealthServices
                .LaboratoryManagement.DTOs.LabRejectionReasonPagedQuery)
            .GetProperties()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.All(
            metadata.QueryParameters,
            x => Assert.True(
                ruasPenyaring.Contains(x.Name),
                $"Metadata mengumumkan parameter {x.Name} yang tidak ada pada LabRejectionReasonPagedQuery."));
    }

    // =====================================================================
    // 4. Penanda keselamatan ikut diumumkan
    // =====================================================================

    [Fact]
    public void MetadataMembawaPenandaKeselamatanYangSudahDitegakkanService()
    {
        // VAL-28: batas kritis hanya berubah lewat pengajuan yang disetujui.
        Assert.True(LabFilterMetadataFactory.LabValueBound().CriticalBoundRequiresApproval);

        // VAL-33 dan VAL-32.
        var pengajuan = LabFilterMetadataFactory.LabCriticalBoundApproval();
        Assert.True(pengajuan.SelfApprovalForbidden);
        Assert.True(pengajuan.SinglePendingRequestOnly);
        Assert.True(pengajuan.IsScopedToSingleValueBound);

        // VAL-37: kedua ruas terkunci disebut namanya supaya layar menggemboknya sejak awal.
        var alasan = LabFilterMetadataFactory.LabRejectionReason();
        Assert.Equal(
            new[] { "isInternalHospitalError", "requiresNote" },
            alasan.SystemFlagFields);

        // Tidak satu pun grup yang menyediakan jalur hapus.
        Assert.False(LabFilterMetadataFactory.LabValueBound().IsDeletable);
        Assert.False(LabFilterMetadataFactory.LabRejectionReason().IsDeletable);
        Assert.False(LabFilterMetadataFactory.LabSpecimen().IsDeletable);
    }

    // =====================================================================
    // 5. Rekap menghitung angka yang benar
    // =====================================================================

    [Fact]
    public async Task RekapAlasanPenolakan_MenghitungAktifNonaktifDanKeduaPenanda()
    {
        await using var context = CreateContext();

        context.MstLabRejectionReasons.AddRange(
            Alasan("CLOTTED", aktif: true, internalError: true, wajibCatatan: false),
            Alasan("INSUFFICIENT", aktif: true, internalError: false, wajibCatatan: false),
            Alasan("OTHER", aktif: true, internalError: false, wajibCatatan: true),
            Alasan("ARCHIVED", aktif: false, internalError: false, wajibCatatan: false));

        var terhapus = Alasan("DELETED", aktif: true, internalError: true, wajibCatatan: true);
        terhapus.IsDelete = true;
        context.MstLabRejectionReasons.Add(terhapus);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rekap = await CreateRejectionReasonService(context).GetSummaryAsync();

        // Baris yang sudah ditandai terhapus tidak ikut dihitung sama sekali.
        Assert.Equal(4, rekap.TotalAlasan);
        Assert.Equal(3, rekap.Aktif);
        Assert.Equal(1, rekap.Nonaktif);
        Assert.Equal(1, rekap.KesalahanInternalRumahSakit);
        Assert.Equal(1, rekap.WajibDisertaiCatatan);
    }

    [Fact]
    public async Task RekapAlasanPenolakan_TabelKosongMenghasilkanNolBukanGalat()
    {
        await using var context = CreateContext();

        var rekap = await CreateRejectionReasonService(context).GetSummaryAsync();

        Assert.Equal(0, rekap.TotalAlasan);
        Assert.Equal(0, rekap.Aktif);
        Assert.Equal(0, rekap.KesalahanInternalRumahSakit);
    }

    [Fact]
    public async Task RekapBatasNilai_MenghitungAktifBentukHasilDanPengajuanTertunda()
    {
        await using var context = CreateContext();
        var procedureId = Guid.NewGuid();
        var procedureLain = Guid.NewGuid();

        var angka = BatasNilai(procedureId, LabResultForm.Numeric, aktif: true, LabGenderScope.Male);
        var angkaKedua = BatasNilai(procedureId, LabResultForm.Numeric, aktif: false, LabGenderScope.Female);
        var pilihan = BatasNilai(procedureLain, LabResultForm.Choice, aktif: true, LabGenderScope.All);

        context.LabValueBounds.AddRange(angka, angkaKedua, pilihan);

        context.LabValueOptions.AddRange(
            new LabValueOption { ValueBoundId = pilihan.Id, OptionCode = "NEG", OptionName = "Negatif", SortOrder = 0 },
            new LabValueOption { ValueBoundId = pilihan.Id, OptionCode = "P1", OptionName = "+1", SortOrder = 1 });

        context.LabValueBoundChangeRequests.Add(new LabValueBoundChangeRequest
        {
            ValueBoundId = angka.Id,
            RequestStatus = LabBoundChangeStatus.Submitted
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rekap = await CreateValueBoundService(context).GetSummaryAsync();

        Assert.Equal(3, rekap.TotalBatasNilai);
        Assert.Equal(2, rekap.Aktif);
        Assert.Equal(1, rekap.Nonaktif);
        Assert.Equal(2, rekap.BentukAngka);
        Assert.Equal(1, rekap.BentukPilihan);
        Assert.Equal(2, rekap.TotalPilihanHasil);
        Assert.Equal(1, rekap.MenungguPersetujuanBatasKritis);

        // Dua jenis pemeriksaan berbeda, walaupun barisnya tiga.
        Assert.Equal(2, rekap.JumlahPemeriksaanBerbeda);
    }

    [Fact]
    public async Task RekapPesanan_MenghitungPerStatusDanPerDisiplinDalamRentangWaktu()
    {
        await using var context = CreateContext();

        var didalam = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var diluar = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        context.LabOrders.AddRange(
            Pesanan(LabOrderStatus.Requested, LabDiscipline.ClinicalPathology, didalam),
            Pesanan(LabOrderStatus.Completed, LabDiscipline.ClinicalPathology, didalam),
            Pesanan(LabOrderStatus.Cancelled, LabDiscipline.Microbiology, didalam),
            Pesanan(LabOrderStatus.Requested, null, didalam),
            Pesanan(LabOrderStatus.Requested, LabDiscipline.AnatomicalPathology, diluar));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rekap = await CreateOrderService(context).GetSummaryAsync(
            new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc));

        // Pesanan Januari berada di luar rentang, jadi tidak ikut terhitung.
        Assert.Equal(4, rekap.TotalPesanan);
        Assert.Equal(2, rekap.Diminta);
        Assert.Equal(1, rekap.Selesai);
        Assert.Equal(1, rekap.Dibatalkan);
        Assert.Equal(2, rekap.PatologiKlinik);
        Assert.Equal(1, rekap.Mikrobiologi);
        Assert.Equal(0, rekap.PatologiAnatomi);
        Assert.Equal(1, rekap.TanpaDisiplin);
    }

    [Fact]
    public async Task RekapPengajuan_BerScopeSatuBatasNilaiSaja()
    {
        await using var context = CreateContext();
        var procedureId = await SeedProcedureAsync(context);

        var pertama = BatasNilai(procedureId, LabResultForm.Numeric, aktif: true, LabGenderScope.Male);
        var kedua = BatasNilai(procedureId, LabResultForm.Numeric, aktif: true, LabGenderScope.Female);

        context.LabValueBounds.AddRange(pertama, kedua);

        context.LabValueBoundChangeRequests.AddRange(
            new LabValueBoundChangeRequest { ValueBoundId = pertama.Id, RequestStatus = LabBoundChangeStatus.Submitted },
            new LabValueBoundChangeRequest { ValueBoundId = pertama.Id, RequestStatus = LabBoundChangeStatus.Approved },
            new LabValueBoundChangeRequest { ValueBoundId = pertama.Id, RequestStatus = LabBoundChangeStatus.Rejected },
            new LabValueBoundChangeRequest { ValueBoundId = kedua.Id, RequestStatus = LabBoundChangeStatus.Withdrawn });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rekap = await CreateApprovalService(context).GetSummaryAsync(pertama.Id);

        // Pengajuan milik batas nilai kedua tidak ikut terhitung.
        Assert.Equal(pertama.Id, rekap.ValueBoundId);
        Assert.Equal(3, rekap.TotalPengajuan);
        Assert.Equal(1, rekap.Diajukan);
        Assert.Equal(1, rekap.Disetujui);
        Assert.Equal(1, rekap.Ditolak);
        Assert.Equal(0, rekap.Ditarik);
        Assert.True(rekap.AdaPengajuanBelumDiputuskan);
    }

    [Fact]
    public async Task RekapPengajuan_BatasNilaiYangTidakAdaDitolak()
    {
        await using var context = CreateContext();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateApprovalService(context).GetSummaryAsync(Guid.NewGuid()));
    }

    // =====================================================================
    // 6. Daftar pesanan menyaring, mengurutkan, dan mem-paging di sisi server
    // =====================================================================

    /// <summary>
    /// Inti perbaikan <c>IGD-DEC-105</c>: layar yang hanya butuh pesanan satu pasien tidak lagi
    /// menerima pesanan pasien lain. Sebelumnya penyaringan itu dikerjakan di dalam browser,
    /// setelah seluruh tabel terlanjur terkirim.
    /// </summary>
    [Fact]
    public async Task DaftarPesanan_DisaringPerKunjunganPasien()
    {
        await using var context = CreateContext();
        var procedureId = await SeedProcedureAsync(context);

        var pasienIni = Guid.NewGuid();
        var pasienLain = Guid.NewGuid();

        context.LabOrders.AddRange(
            PesananPada(pasienIni, procedureId, LabOrderStatus.Requested),
            PesananPada(pasienIni, procedureId, LabOrderStatus.Completed),
            PesananPada(pasienLain, procedureId, LabOrderStatus.Requested));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var hasil = await CreateOrderService(context).GetListAsync(
            new QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
                .LabOrderPagedQuery { EncounterId = pasienIni });

        Assert.Equal(2, hasil.TotalData);
        Assert.All(hasil.Items, x => Assert.Equal(pasienIni, x.EncounterId));
    }

    [Fact]
    public async Task DaftarPesanan_DisaringPerStatusDanPerDisiplin()
    {
        await using var context = CreateContext();
        var procedureId = await SeedProcedureAsync(context);

        context.LabOrders.AddRange(
            PesananDenganProcedure(procedureId, LabOrderStatus.Requested, LabDiscipline.ClinicalPathology, DateTime.UtcNow),
            PesananDenganProcedure(procedureId, LabOrderStatus.Completed, LabDiscipline.ClinicalPathology, DateTime.UtcNow),
            PesananDenganProcedure(procedureId, LabOrderStatus.Requested, LabDiscipline.Microbiology, DateTime.UtcNow));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = CreateOrderService(context);

        var perStatus = await service.GetListAsync(
            new QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
                .LabOrderPagedQuery { OrderStatus = LabOrderStatus.Requested });

        Assert.Equal(2, perStatus.TotalData);

        var perDisiplin = await service.GetListAsync(
            new QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
                .LabOrderPagedQuery { Discipline = LabDiscipline.Microbiology });

        Assert.Equal(1, perDisiplin.TotalData);
    }

    [Fact]
    public async Task DaftarPesanan_MemakaiBentukPagingYangSudahMapan()
    {
        await using var context = CreateContext();
        var procedureId = await SeedProcedureAsync(context);

        for (var i = 0; i < 5; i++)
            context.LabOrders.Add(PesananDenganProcedure(
                procedureId, LabOrderStatus.Requested, LabDiscipline.ClinicalPathology, DateTime.UtcNow));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var hasil = await CreateOrderService(context).GetListAsync(
            new QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
                .LabOrderPagedQuery { PageNumber = 2, PageSize = 2 });

        Assert.Equal(2, hasil.PageNumber);
        Assert.Equal(2, hasil.PageSize);
        Assert.Equal(5, hasil.TotalData);
        Assert.Equal(3, hasil.TotalPage);
        Assert.Equal(2, hasil.Items.Count);
    }

    /// <summary>
    /// Ukuran halaman dibatasi supaya satu permintaan tidak dapat menarik seluruh tabel dengan
    /// menuliskan angka yang besar sekali.
    /// </summary>
    [Fact]
    public async Task DaftarPesanan_UkuranHalamanDibatasiSeratus()
    {
        await using var context = CreateContext();
        var procedureId = await SeedProcedureAsync(context);
        context.LabOrders.Add(PesananDenganProcedure(procedureId, LabOrderStatus.Requested, null, DateTime.UtcNow));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var hasil = await CreateOrderService(context).GetListAsync(
            new QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
                .LabOrderPagedQuery { PageSize = 5000, PageNumber = 0 });

        Assert.Equal(100, hasil.PageSize);
        Assert.Equal(1, hasil.PageNumber);
    }

    [Fact]
    public async Task DaftarPesanan_DiurutkanTerbaruLebihDuluSecaraBawaan()
    {
        await using var context = CreateContext();
        var procedureId = await SeedProcedureAsync(context);

        var lama = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var baru = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

        context.LabOrders.AddRange(
            PesananDenganProcedure(procedureId, LabOrderStatus.Requested, null, lama),
            PesananDenganProcedure(procedureId, LabOrderStatus.Requested, null, baru));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = CreateOrderService(context);

        var bawaan = await service.GetListAsync(
            new QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
                .LabOrderPagedQuery());

        Assert.Equal(baru, bawaan.Items[0].CreateDateTime);

        var menaik = await service.GetListAsync(
            new QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
                .LabOrderPagedQuery { SortDirection = "asc" });

        Assert.Equal(lama, menaik.Items[0].CreateDateTime);
    }

    /// <summary>
    /// Nama kolom yang tidak dikenal dikembalikan ke bawaan, bukan ditolak. Layar lama yang
    /// mengirim kolom yang sudah tidak ada tetap memperoleh daftar yang masuk akal.
    /// </summary>
    [Fact]
    public async Task DaftarPesanan_KolomUrutanTidakDikenalKembaliKeBawaan()
    {
        await using var context = CreateContext();
        var procedureId = await SeedProcedureAsync(context);
        context.LabOrders.Add(PesananDenganProcedure(procedureId, LabOrderStatus.Requested, null, DateTime.UtcNow));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var hasil = await CreateOrderService(context).GetListAsync(
            new QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
                .LabOrderPagedQuery { SortBy = "kolomYangTidakAda" });

        Assert.Single(hasil.Items);
    }

    [Fact]
    public async Task DaftarPesanan_TidakMenampilkanBarisYangSudahDitandaiTerhapus()
    {
        await using var context = CreateContext();
        var procedureId = await SeedProcedureAsync(context);

        var terlihat = PesananDenganProcedure(procedureId, LabOrderStatus.Requested, null, DateTime.UtcNow);
        var terhapus = PesananDenganProcedure(procedureId, LabOrderStatus.Requested, null, DateTime.UtcNow);
        terhapus.IsDelete = true;

        context.LabOrders.AddRange(terlihat, terhapus);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var hasil = await CreateOrderService(context).GetListAsync(
            new QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
                .LabOrderPagedQuery());

        Assert.Equal(1, hasil.TotalData);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static LabOrder PesananPada(Guid encounterId, Guid procedureId, LabOrderStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            ProcedureId = procedureId,
            OrderStatus = status,
            CreateDateTime = DateTime.UtcNow
        };

    /// <summary>
    /// Pesanan yang menunjuk jenis pemeriksaan sungguhan.
    ///
    /// Wajib memakai <see cref="SeedProcedureAsync"/>, bukan <c>Guid.NewGuid()</c> karangan:
    /// proyeksi daftar pesanan menyentuh navigasi <c>Procedure</c>, dan relasi wajib yang
    /// principal-nya tidak ada membuat barisnya tidak ikut terbawa sama sekali. Pada database
    /// sungguhan keadaan itu tidak mungkin terjadi karena <c>ProcedureId</c> wajib dan
    /// relasinya <c>Restrict</c>.
    /// </summary>
    private static LabOrder PesananDenganProcedure(
        Guid procedureId, LabOrderStatus status, LabDiscipline? disiplin, DateTime dibuat) =>
        new()
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = procedureId,
            OrderStatus = status,
            Discipline = disiplin,
            CreateDateTime = dibuat
        };

    /// <summary>
    /// Menyimpan satu jenis pemeriksaan sungguhan. Diperlukan karena pemuatan batas nilai
    /// menyertakan navigasi ke <c>MstProcedure</c>.
    /// </summary>
    private static async Task<Guid> SeedProcedureAsync(ApplicationDbContext context)
    {
        var procedure = new QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"LB-{Guid.NewGuid():N}"[..12],
            ProcedureName = "Kalium",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

        context.Set<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstProcedure>()
            .Add(procedure);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return procedure.Id;
    }

    private static MstLabRejectionReason Alasan(
        string kode, bool aktif, bool internalError, bool wajibCatatan) =>
        new()
        {
            Id = Guid.NewGuid(),
            ReasonCode = kode,
            ReasonName = kode,
            IsActive = aktif,
            IsInternalHospitalError = internalError,
            RequiresNote = wajibCatatan
        };

    private static LabValueBound BatasNilai(
        Guid procedureId, LabResultForm bentuk, bool aktif, LabGenderScope jenisKelamin) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProcedureId = procedureId,
            ResultForm = bentuk,
            GenderScope = jenisKelamin,
            Unit = bentuk == LabResultForm.Numeric ? "g/dL" : null,
            IsActive = aktif
        };

    private static LabOrder Pesanan(
        LabOrderStatus status, LabDiscipline? disiplin, DateTime dibuat) =>
        new()
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = Guid.NewGuid(),
            OrderStatus = status,
            Discipline = disiplin,
            CreateDateTime = dibuat
        };

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-filter-summary-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, Aktor.ToString()) },
            authenticationType: "LabFilterAndSummaryTest");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static LoggerService CreateLoggerService(IHttpContextAccessor accessor) =>
        new(NullLogger<LoggerService>.Instance, accessor);

    private static LabRejectionReasonService CreateRejectionReasonService(ApplicationDbContext context)
    {
        var accessor = CreateHttpContextAccessor();

        return new LabRejectionReasonService(context, accessor, CreateLoggerService(accessor));
    }

    private static LabValueBoundService CreateValueBoundService(ApplicationDbContext context)
    {
        var accessor = CreateHttpContextAccessor();

        return new LabValueBoundService(context, accessor, CreateLoggerService(accessor));
    }

    private static LabCriticalBoundApprovalService CreateApprovalService(ApplicationDbContext context)
    {
        var accessor = CreateHttpContextAccessor();

        return new LabCriticalBoundApprovalService(context, accessor, CreateLoggerService(accessor));
    }

    private static LabOrderService CreateOrderService(ApplicationDbContext context)
    {
        var accessor = CreateHttpContextAccessor();
        var loggerService = CreateLoggerService(accessor);

        var specimenService = new LabSpecimenService(
            context,
            new ClinicalMilestoneFactProducer(
                context,
                new BillingFolioService(context),
                loggerService),
            accessor,
            loggerService);

        return new LabOrderService(context, specimenService, accessor, loggerService);
    }
}
