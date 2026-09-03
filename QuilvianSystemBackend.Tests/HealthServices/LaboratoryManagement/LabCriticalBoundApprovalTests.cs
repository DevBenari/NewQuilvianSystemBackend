using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-05</c> — pengajuan dan persetujuan perubahan batas kritis
/// (<c>FR-03.4</c>, <c>LAB-DEC-023</c>, <c>AC-33</c> seluruh jalur).
///
/// Yang paling penting di berkas ini adalah <c>VAL-33</c>: pengaju tidak boleh menyetujui
/// pengajuannya sendiri. <c>CAP-16</c> sudah membuktikan sistem permission tidak dapat
/// menegakkannya, jadi yang diuji di sini adalah kodenya — bukan konfigurasinya.
/// </summary>
public class LabCriticalBoundApprovalTests
{
    private static readonly Guid KepalaInstalasi = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid PenyetujuKlinis = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // =====================================================================
    // 1. Mengajukan
    // =====================================================================

    [Fact]
    public async Task Mengajukan_MenghasilkanPengajuanSubmittedDanBatasLamaTidakBergerak()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var hasil = await service.SubmitAsync(boundId, new SubmitCriticalBoundChangeRequest
        {
            ProposedCriticalHigh = 8.0m,
            RequestReason = "Peringatan nilai kritis dinilai terlalu sering muncul."
        });

        Assert.Equal(nameof(LabBoundChangeStatus.Submitted), hasil.RequestStatus);
        Assert.Equal(8.0m, hasil.ProposedCriticalHigh);
        Assert.Equal(KepalaInstalasi, hasil.RequestedByUserId);
        Assert.Null(hasil.DecidedByUserId);

        // Inti AC-33: yang berlaku tidak bergerak sedikit pun.
        Assert.Equal(6.0m, hasil.CurrentCriticalHigh);

        var bound = await context.LabValueBounds.AsNoTracking().SingleAsync(x => x.Id == boundId);

        Assert.Equal(6.0m, bound.CriticalHigh);
        Assert.Empty(await context.LabValueBoundHistories.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task VAL31_MengajukanTanpaAlasan_Ditolak()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var galat = await Assert.ThrowsAsync<LabCriticalBoundValidationException>(() =>
            service.SubmitAsync(boundId, new SubmitCriticalBoundChangeRequest
            {
                ProposedCriticalHigh = 8.0m,
                RequestReason = "   "
            }));

        Assert.Equal("Jelaskan alasan perubahan batas kritis ini.", galat.Message);
        Assert.Empty(await context.LabValueBoundChangeRequests.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task VAL32_PengajuanKeduaSaatYangPertamaBelumDiputuskan_Ditolak()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        await service.SubmitAsync(boundId, Usulan(8.0m));

        var galat = await Assert.ThrowsAsync<LabCriticalBoundConflictException>(() =>
            service.SubmitAsync(boundId, Usulan(9.0m)));

        Assert.Equal("Masih ada pengajuan yang belum diputuskan untuk batas nilai ini.", galat.Message);
        Assert.Single(await context.LabValueBoundChangeRequests.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task MengajukanUsulanYangMustahilDisetujui_DitolakSejakAwal()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        // Kalium normal 3,5–5,1. Batas kritis atas 4,0 akan membuat angka 4,5 yang masih normal
        // ikut terhitung kritis.
        var galat = await Assert.ThrowsAsync<LabCriticalBoundValidationException>(() =>
            service.SubmitAsync(boundId, Usulan(4.0m)));

        Assert.Equal("Batas kritis atas harus lebih tinggi daripada batas normal atas.", galat.Message);
    }

    // =====================================================================
    // 2. VAL-33 — invariant keselamatan utama
    // =====================================================================

    [Fact]
    public async Task VAL33_PengajuMenyetujuiPengajuannyaSendiri_Ditolak()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var pengaju = CreateService(context, KepalaInstalasi);

        var pengajuan = await pengaju.SubmitAsync(boundId, Usulan(8.0m));

        // Orang yang sama, walaupun ia memegang hak akses Approve.
        var galat = await Assert.ThrowsAsync<LabCriticalBoundForbiddenException>(() =>
            pengaju.ApproveAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest()));

        Assert.Equal("Pengaju tidak boleh menyetujui pengajuannya sendiri.", galat.Message);

        // Tidak satu pun akibatnya terjadi: batas lama tetap, status tetap, riwayat kosong.
        var bound = await context.LabValueBounds.AsNoTracking().SingleAsync(x => x.Id == boundId);

        Assert.Equal(6.0m, bound.CriticalHigh);
        Assert.Empty(await context.LabValueBoundHistories.AsNoTracking().ToListAsync());

        var tersimpan = await context.LabValueBoundChangeRequests.AsNoTracking().SingleAsync();

        Assert.Equal(LabBoundChangeStatus.Submitted, tersimpan.RequestStatus);
        Assert.Null(tersimpan.DecidedByUserId);
    }

    [Fact]
    public async Task VAL33_PengajuMenolakPengajuannyaSendiri_JugaDitolak()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var pengaju = CreateService(context, KepalaInstalasi);

        var pengajuan = await pengaju.SubmitAsync(boundId, Usulan(8.0m));

        // Menolak pengajuan sendiri terlihat tidak berbahaya, tetapi ia tetap keputusan atas
        // pengajuan sendiri — dan membiarkannya membuat aturannya tidak konsisten.
        var galat = await Assert.ThrowsAsync<LabCriticalBoundForbiddenException>(() =>
            pengaju.RejectAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest()));

        Assert.Equal("Pengaju tidak boleh menyetujui pengajuannya sendiri.", galat.Message);
    }

    [Fact]
    public async Task PelakuTanpaIdentitas_DitolakSaatMengajukanMaupunMemutuskan()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);

        var tanpaIdentitas = CreateService(context, Guid.Empty);

        // Tanpa penolakan ini, dua pelaku yang sama-sama tidak dikenali terbaca sebagai orang
        // yang sama — dan yang lebih berbahaya, pengajuan milik pengguna sungguhan dapat
        // disetujui pemanggil tanpa identitas, karena Guid.Empty tidak sama dengan id siapa pun.
        await Assert.ThrowsAsync<LabCriticalBoundForbiddenException>(() =>
            tanpaIdentitas.SubmitAsync(boundId, Usulan(8.0m)));

        var pengajuan = await CreateService(context, KepalaInstalasi).SubmitAsync(boundId, Usulan(8.0m));

        var galat = await Assert.ThrowsAsync<LabCriticalBoundForbiddenException>(() =>
            tanpaIdentitas.ApproveAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest()));

        Assert.Equal(
            "Identitas pengguna tidak dikenali, sehingga tindakan ini tidak dapat dipertanggungjawabkan.",
            galat.Message);

        var bound = await context.LabValueBounds.AsNoTracking().SingleAsync(x => x.Id == boundId);

        Assert.Equal(6.0m, bound.CriticalHigh);
    }

    // =====================================================================
    // 3. Menyetujui dan menolak
    // =====================================================================

    [Fact]
    public async Task Menyetujui_MengubahBatasKritisDanMengisiPenyetujuPadaRiwayat()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);

        var pengajuan = await CreateService(context, KepalaInstalasi)
            .SubmitAsync(boundId, Usulan(8.0m));

        var hasil = await CreateService(context, PenyetujuKlinis)
            .ApproveAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest
            {
                DecisionNote = "Disetujui setelah tinjauan komite."
            });

        Assert.Equal(nameof(LabBoundChangeStatus.Approved), hasil.RequestStatus);
        Assert.Equal(PenyetujuKlinis, hasil.DecidedByUserId);
        Assert.NotNull(hasil.DecidedAt);

        // Batas kritis yang baru mulai berlaku tepat di sini.
        var bound = await context.LabValueBounds.AsNoTracking().SingleAsync(x => x.Id == boundId);

        Assert.Equal(8.0m, bound.CriticalHigh);
        Assert.Equal(2.5m, bound.CriticalLow);

        // AC-34 untuk jalur yang memerlukan persetujuan: pelaku dan penyetuju dua orang berbeda.
        var riwayat = await context.LabValueBoundHistories.AsNoTracking().SingleAsync();

        Assert.Equal(nameof(LabValueBound.CriticalHigh), riwayat.ChangedField);
        Assert.Equal("6.0", riwayat.OldValue);
        Assert.Equal("8.0", riwayat.NewValue);
        Assert.Equal(KepalaInstalasi, riwayat.ActorUserId);
        Assert.Equal(PenyetujuKlinis, riwayat.ApprovedByUserId);
        Assert.NotEqual(riwayat.ActorUserId, riwayat.ApprovedByUserId!.Value);
    }

    [Fact]
    public async Task Menolak_TidakMengubahBatasKritisSamaSekali()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);

        var pengajuan = await CreateService(context, KepalaInstalasi)
            .SubmitAsync(boundId, Usulan(8.0m));

        var hasil = await CreateService(context, PenyetujuKlinis)
            .RejectAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest
            {
                DecisionNote = "Tidak ada dasar klinis."
            });

        Assert.Equal(nameof(LabBoundChangeStatus.Rejected), hasil.RequestStatus);
        Assert.Equal(PenyetujuKlinis, hasil.DecidedByUserId);

        var bound = await context.LabValueBounds.AsNoTracking().SingleAsync(x => x.Id == boundId);

        Assert.Equal(6.0m, bound.CriticalHigh);
        Assert.Empty(await context.LabValueBoundHistories.AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VAL34_MemutuskanUlangPengajuanYangSudahTerminal_Ditolak(bool disetujuiLebihDulu)
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);

        var pengajuan = await CreateService(context, KepalaInstalasi).SubmitAsync(boundId, Usulan(8.0m));
        var pemutus = CreateService(context, PenyetujuKlinis);

        if (disetujuiLebihDulu)
            await pemutus.ApproveAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest());
        else
            await pemutus.RejectAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest());

        var galat = await Assert.ThrowsAsync<LabCriticalBoundConflictException>(() =>
            pemutus.ApproveAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest()));

        Assert.Equal("Pengajuan ini sudah diputuskan.", galat.Message);
    }

    // =====================================================================
    // 4. Menarik
    // =====================================================================

    [Fact]
    public async Task VAL35_YangMenarikBukanPengaju_Ditolak()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);

        var pengajuan = await CreateService(context, KepalaInstalasi).SubmitAsync(boundId, Usulan(8.0m));

        var galat = await Assert.ThrowsAsync<LabCriticalBoundForbiddenException>(() =>
            CreateService(context, PenyetujuKlinis).WithdrawAsync(boundId, pengajuan.Id));

        Assert.Equal("Hanya pengaju yang boleh menarik pengajuannya.", galat.Message);
    }

    [Fact]
    public async Task MenarikPengajuanSendiri_MembebaskanBatasNilaiUntukPengajuanBaru()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var pengaju = CreateService(context, KepalaInstalasi);

        var pertama = await pengaju.SubmitAsync(boundId, Usulan(8.0m));
        var ditarik = await pengaju.WithdrawAsync(boundId, pertama.Id);

        Assert.Equal(nameof(LabBoundChangeStatus.Withdrawn), ditarik.RequestStatus);

        // VAL-32 hanya menahan pengajuan yang masih berjalan; yang sudah ditarik tidak menahan.
        var kedua = await pengaju.SubmitAsync(boundId, Usulan(7.5m));

        Assert.Equal(nameof(LabBoundChangeStatus.Submitted), kedua.RequestStatus);
        Assert.Equal(2, (await context.LabValueBoundChangeRequests.AsNoTracking().ToListAsync()).Count);

        // Menarik tidak pernah menyentuh batas yang berlaku.
        var bound = await context.LabValueBounds.AsNoTracking().SingleAsync(x => x.Id == boundId);

        Assert.Equal(6.0m, bound.CriticalHigh);
    }

    [Fact]
    public async Task MenarikPengajuanYangSudahDiputuskan_Ditolak()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var pengaju = CreateService(context, KepalaInstalasi);

        var pengajuan = await pengaju.SubmitAsync(boundId, Usulan(8.0m));

        await CreateService(context, PenyetujuKlinis)
            .ApproveAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest());

        var galat = await Assert.ThrowsAsync<LabCriticalBoundConflictException>(() =>
            pengaju.WithdrawAsync(boundId, pengajuan.Id));

        Assert.Equal("Pengajuan ini sudah diputuskan.", galat.Message);
    }

    // =====================================================================
    // 5. Daftar dan jalur tidak ditemukan
    // =====================================================================

    [Fact]
    public async Task DaftarPengajuan_MenampilkanSeluruhnyaTerbaruLebihDulu()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var pengaju = CreateService(context, KepalaInstalasi);

        var pertama = await pengaju.SubmitAsync(boundId, Usulan(8.0m));
        await pengaju.WithdrawAsync(boundId, pertama.Id);
        await pengaju.SubmitAsync(boundId, Usulan(7.5m));

        var daftar = await pengaju.GetListAsync(boundId);

        Assert.Equal(2, daftar.Count);
        Assert.Contains(daftar, x => x.RequestStatus == nameof(LabBoundChangeStatus.Withdrawn));
        Assert.Contains(daftar, x => x.RequestStatus == nameof(LabBoundChangeStatus.Submitted));
    }

    [Fact]
    public async Task BatasNilaiAtauPengajuanYangTidakAda_MenghasilkanTidakDitemukan()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetListAsync(Guid.NewGuid()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.SubmitAsync(Guid.NewGuid(), Usulan(8.0m)));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.WithdrawAsync(boundId, Guid.NewGuid()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService(context, PenyetujuKlinis)
                .ApproveAsync(boundId, Guid.NewGuid(), new DecideCriticalBoundChangeRequest()));
    }

    // =====================================================================
    // 6. Temuan audit adversarial — dijaga agar tidak kembali
    // =====================================================================

    [Fact]
    public async Task TokenKonkurensi_NaikPadaSetiapKeputusan()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);

        var pengajuan = await CreateService(context, KepalaInstalasi).SubmitAsync(boundId, Usulan(8.0m));

        var sebelum = (await context.LabValueBoundChangeRequests.AsNoTracking()
            .SingleAsync(x => x.Id == pengajuan.Id)).Version;

        await CreateService(context, PenyetujuKlinis)
            .ApproveAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest());

        var sesudah = (await context.LabValueBoundChangeRequests.AsNoTracking()
            .SingleAsync(x => x.Id == pengajuan.Id)).Version;

        // Tanpa kenaikan ini token konkurensi tidak pernah berubah, sehingga klausa WHERE milik
        // EF tetap cocok bagi penulis kedua dan CAP-17 tidak pernah menyala.
        Assert.Equal(sebelum + 1, sesudah);
    }

    [Fact]
    public async Task MenyetujuiSetelahBatasNormalBergeser_DiperiksaUlangDanDitolak()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);

        // Usulan 8,0 masuk akal terhadap batas normal atas 5,1 saat diajukan.
        var pengajuan = await CreateService(context, KepalaInstalasi).SubmitAsync(boundId, Usulan(8.0m));

        // Kepala instalasi kemudian menaikkan batas normal atas menjadi 9,0 lewat jalur biasa.
        var bound = await context.LabValueBounds.SingleAsync(x => x.Id == boundId);
        bound.NormalHigh = 9.0m;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Usulan yang tadinya masuk akal kini mustahil: batas kritis atas 8,0 berada DI BAWAH
        // batas normal atas 9,0, sehingga angka 8,5 yang masih normal akan terhitung kritis.
        var galat = await Assert.ThrowsAsync<LabCriticalBoundValidationException>(() =>
            CreateService(context, PenyetujuKlinis)
                .ApproveAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest()));

        Assert.Equal("Batas kritis atas harus lebih tinggi daripada batas normal atas.", galat.Message);

        var sesudah = await context.LabValueBounds.AsNoTracking().SingleAsync(x => x.Id == boundId);

        Assert.Equal(6.0m, sesudah.CriticalHigh);
    }

    [Fact]
    public async Task MengusulkanKodePilihanYangTidakDikenal_DitolakBukanDiabaikan()
    {
        await using var context = CreateContext();
        var boundId = await SeedProteinUrinAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        // Penerapan bekerja dengan memadamkan penanda kritis pada pilihan yang tidak disebut.
        // Satu kode keliru karena itu akan mencabut seluruh penanda kritis diam-diam.
        var galat = await Assert.ThrowsAsync<LabCriticalBoundValidationException>(() =>
            service.SubmitAsync(boundId, new SubmitCriticalBoundChangeRequest
            {
                ProposedCriticalOptionCodes = "P5",
                RequestReason = "Salah ketik kode pilihan."
            }));

        Assert.Contains("P5", galat.Message);
        Assert.Empty(await context.LabValueBoundChangeRequests.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task MengusulkanBatasAngkaPadaPemeriksaanBerhasilPilihan_Ditolak()
    {
        await using var context = CreateContext();
        var boundId = await SeedProteinUrinAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var galat = await Assert.ThrowsAsync<LabCriticalBoundValidationException>(() =>
            service.SubmitAsync(boundId, Usulan(8.0m)));

        Assert.Equal("Pemeriksaan berhasil pilihan tidak memakai batas kritis berupa angka.", galat.Message);
    }

    [Fact]
    public async Task MengusulkanPilihanKritisPadaPemeriksaanBerhasilAngka_Ditolak()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var galat = await Assert.ThrowsAsync<LabCriticalBoundValidationException>(() =>
            service.SubmitAsync(boundId, new SubmitCriticalBoundChangeRequest
            {
                ProposedCriticalOptionCodes = "P3,P4",
                RequestReason = "Salah bentuk hasil."
            }));

        Assert.Equal(
            "Pemeriksaan berhasil angka tidak punya daftar pilihan, sehingga pilihan kritis tidak dapat diusulkan.",
            galat.Message);
    }

    [Fact]
    public async Task MenyetujuiPerubahanPilihanKritis_MenerapkanPenandanyaDanMenerbitkanRiwayat()
    {
        await using var context = CreateContext();
        var boundId = await SeedProteinUrinAsync(context);

        // Semula P3 dan P4 kritis. Usulan: hanya P4 yang kritis.
        var pengajuan = await CreateService(context, KepalaInstalasi)
            .SubmitAsync(boundId, new SubmitCriticalBoundChangeRequest
            {
                ProposedCriticalOptionCodes = "P4",
                RequestReason = "P3 dinilai belum mengancam."
            });

        await CreateService(context, PenyetujuKlinis)
            .ApproveAsync(boundId, pengajuan.Id, new DecideCriticalBoundChangeRequest());

        var pilihan = await context.LabValueOptions.AsNoTracking()
            .Where(x => x.ValueBoundId == boundId).ToListAsync();

        Assert.False(pilihan.Single(x => x.OptionCode == "P3").IsCritical);
        Assert.True(pilihan.Single(x => x.OptionCode == "P4").IsCritical);

        var riwayat = await context.LabValueBoundHistories.AsNoTracking().SingleAsync();

        Assert.Equal("CriticalOptions", riwayat.ChangedField);
        Assert.Equal("P3,P4", riwayat.OldValue);
        Assert.Equal("P4", riwayat.NewValue);
        Assert.Equal(PenyetujuKlinis, riwayat.ApprovedByUserId);
    }

    [Fact]
    public async Task MenarikPengajuan_TidakMengisiPemutus()
    {
        await using var context = CreateContext();
        var boundId = await SeedKaliumAsync(context);
        var pengaju = CreateService(context, KepalaInstalasi);

        var pengajuan = await pengaju.SubmitAsync(boundId, Usulan(8.0m));
        await pengaju.WithdrawAsync(boundId, pengajuan.Id);

        var tersimpan = await context.LabValueBoundChangeRequests.AsNoTracking().SingleAsync();

        // Menarik pengajuan sendiri bukan keputusan pihak berwenang. Mengisi DecidedByUserId
        // dengan pengaju akan membuat baris ini terbaca seolah pengaju dan pemutusnya orang
        // yang sama — persis keadaan yang VAL-33 ada untuk mencegahnya.
        Assert.Null(tersimpan.DecidedByUserId);
        Assert.NotNull(tersimpan.DecidedAt);
        Assert.Equal(KepalaInstalasi, tersimpan.RequestedByUserId);
    }

    // =====================================================================
    // 7. Kontrak endpoint — DoD BE-LAB-05
    // =====================================================================

    [Theory]
    [InlineData(nameof(LabCriticalBoundApprovalController.GetList), "LabCriticalBound", "Read", typeof(HttpGetAttribute), null)]
    [InlineData(nameof(LabCriticalBoundApprovalController.Submit), "LabValueBound", "Update", typeof(HttpPostAttribute), null)]
    [InlineData(nameof(LabCriticalBoundApprovalController.Approve), "LabCriticalBound", "Approve", typeof(HttpPostAttribute), "{requestId:guid}/approve")]
    [InlineData(nameof(LabCriticalBoundApprovalController.Reject), "LabCriticalBound", "Approve", typeof(HttpPostAttribute), "{requestId:guid}/reject")]
    [InlineData(nameof(LabCriticalBoundApprovalController.Withdraw), "LabValueBound", "Update", typeof(HttpPostAttribute), "{requestId:guid}/withdraw")]
    public void KelimaEndpoint_MemakaiRouteDanPermissionYangDikunciKontrak(
        string methodName,
        string resource,
        string action,
        Type verbAttribute,
        string? template)
    {
        var method = typeof(LabCriticalBoundApprovalController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal(resource, arguments[0]);
        Assert.Equal(action, arguments[1]);

        var verb = method.GetCustomAttributes(verbAttribute, inherit: false).SingleOrDefault();

        Assert.NotNull(verb);
        Assert.Equal(template, ((IRouteTemplateProvider)verb!).Template);
    }

    [Fact]
    public void ControllerPersetujuan_MemakaiBaseRouteYangDikunciKontrak()
    {
        var route = typeof(LabCriticalBoundApprovalController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(route);
        Assert.Equal(
            "api/v1/health-services/laboratory-management/lab-value-bounds/{valueBoundId:guid}/critical-change-requests",
            route!.Template);

        var endpoints = typeof(LabCriticalBoundApprovalController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<AccessPermissionAttribute>().Any())
            .ToList();

        Assert.Equal(5, endpoints.Count);

        // Menyetujui dan menolak sama-sama memakai LabCriticalBound : Approve; mengajukan dan
        // menarik memakai LabValueBound : Update. Pemisahan itulah yang membuat dua peran
        // berbeda dapat diberikan kepada dua orang berbeda.
        var approveCount = endpoints.Count(x =>
            ((object[])x.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments!)[1] as string == "Approve");

        Assert.Equal(2, approveCount);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static SubmitCriticalBoundChangeRequest Usulan(decimal criticalHigh) =>
        new()
        {
            ProposedCriticalHigh = criticalHigh,
            RequestReason = "Peninjauan ulang batas kritis."
        };

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-critical-approval-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Satu database, dua identitas. Inilah yang membuat <c>VAL-33</c> dapat diuji sungguhan:
    /// pengaju dan pemutus adalah dua service dengan pelaku berbeda di atas data yang sama.
    /// </summary>
    private static LabCriticalBoundApprovalService CreateService(ApplicationDbContext context, Guid actorUserId)
    {
        var accessor = CreateHttpContextAccessor(actorUserId);

        return new LabCriticalBoundApprovalService(
            context,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(Guid actorUserId)
    {
        var principal = actorUserId == Guid.Empty
            ? new ClaimsPrincipal(new ClaimsIdentity())
            : new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
                authenticationType: "LabCriticalBoundApprovalTest"));

        return new PerInstanceHttpContextAccessor(new DefaultHttpContext { User = principal });
    }

    /// <summary>
    /// Accessor yang menyimpan <see cref="HttpContext"/> miliknya sendiri.
    ///
    /// <see cref="HttpContextAccessor"/> bawaan framework menyimpan nilainya pada
    /// <c>AsyncLocal</c> <b>statis</b>, sehingga membuat instance kedua akan menimpa nilai
    /// instance pertama di dalam alur async yang sama. Pada pengujian yang memerlukan dua
    /// identitas berbeda di atas data yang sama — dan justru itulah yang dibutuhkan untuk
    /// menguji <c>VAL-33</c> — accessor bawaan membuat kedua service membaca pelaku yang sama.
    ///
    /// Akibatnya pengujian bisa lulus karena kebetulan urutan pemanggilan, bukan karena
    /// aturannya benar-benar ditegakkan. Itu jenis kelulusan yang lebih berbahaya daripada
    /// kegagalan.
    /// </summary>
    private sealed class PerInstanceHttpContextAccessor(HttpContext context) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = context;
    }

    /// <summary>Kalium yang berlaku: normal 3,5–5,1 mmol/L, kritis 2,5–6,0.</summary>
    private static async Task<Guid> SeedKaliumAsync(ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"LB-{suffix}",
            ProcedureName = "Kalium",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

        var bound = new LabValueBound
        {
            Id = Guid.NewGuid(),
            ProcedureId = procedure.Id,
            ResultForm = LabResultForm.Numeric,
            Unit = "mmol/L",
            GenderScope = LabGenderScope.All,
            NormalLow = 3.5m,
            NormalHigh = 5.1m,
            CriticalLow = 2.5m,
            CriticalHigh = 6.0m
        };

        context.Set<MstProcedure>().Add(procedure);
        context.LabValueBounds.Add(bound);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return bound.Id;
    }

    /// <summary>
    /// Protein urin berbentuk pilihan: Negatif, +1, +2, +3, +4 — dengan +3 dan +4 kritis.
    /// </summary>
    private static async Task<Guid> SeedProteinUrinAsync(ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"LB-{suffix}",
            ProcedureName = "Protein Urin",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

        var bound = new LabValueBound
        {
            Id = Guid.NewGuid(),
            ProcedureId = procedure.Id,
            ResultForm = LabResultForm.Choice,
            GenderScope = LabGenderScope.All
        };

        bound.Options.Add(new LabValueOption { OptionCode = "NEG", OptionName = "Negatif", SortOrder = 0 });
        bound.Options.Add(new LabValueOption { OptionCode = "P1", OptionName = "+1", IsOutOfReference = true, SortOrder = 1 });
        bound.Options.Add(new LabValueOption { OptionCode = "P2", OptionName = "+2", IsOutOfReference = true, SortOrder = 2 });
        bound.Options.Add(new LabValueOption { OptionCode = "P3", OptionName = "+3", IsOutOfReference = true, IsCritical = true, SortOrder = 3 });
        bound.Options.Add(new LabValueOption { OptionCode = "P4", OptionName = "+4", IsOutOfReference = true, IsCritical = true, SortOrder = 4 });

        context.Set<MstProcedure>().Add(procedure);
        context.LabValueBounds.Add(bound);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return bound.Id;
    }
}
