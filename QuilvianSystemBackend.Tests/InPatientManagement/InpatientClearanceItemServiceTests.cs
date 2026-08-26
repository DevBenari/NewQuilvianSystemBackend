using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Aturan butir administrasi yang diminta <c>BE-RWI-005</c>: kode kembar ditolak, dan
/// menonaktifkan butir wajib tidak menghapus penandaan pada episode lama.
/// </summary>
public sealed class InpatientClearanceItemServiceTests
{
    private static readonly Guid ActorUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task ButirBaru_TersimpanDenganKodeHurufBesar()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        var result = await service.CreateAsync(
            new CreateInpatientClearanceItemRequest
            {
                ItemCode = " adm-doc ",
                ItemName = "Berkas administrasi pasien lengkap",
                IsMandatory = true,
                SortOrder = 10
            },
            ActorUserId);

        Assert.Equal(InpatientClearanceItemStatus.Success, result.Status);
        Assert.Equal("ADM-DOC", result.Entity!.ItemCode);
    }

    [Fact]
    public async Task ButirDenganKodeKembar_Ditolak()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        await service.CreateAsync(BuildCreateRequest("ADM-DOC"), ActorUserId);

        // Huruf kecil sengaja dipakai: kode butir dibandingkan tanpa peduli besar kecil
        // huruf, supaya "adm-doc" tidak lolos sebagai butir kedua yang artinya sama.
        var kedua = await service.CreateAsync(BuildCreateRequest("adm-doc"), ActorUserId);

        Assert.Equal(InpatientClearanceItemStatus.DuplicateCode, kedua.Status);
        Assert.Contains("sudah dipakai", kedua.Message);
        Assert.Equal(1, await db.Set<MstInpatientClearanceItem>().CountAsync());
    }

    [Fact]
    public async Task MengubahButirMenjadiKodeMilikButirLain_Ditolak()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        await service.CreateAsync(BuildCreateRequest("ADM-DOC"), ActorUserId);
        var kedua = await service.CreateAsync(BuildCreateRequest("RETURN-ITEM"), ActorUserId);

        var hasil = await service.UpdateAsync(
            kedua.Entity!.Id,
            new UpdateInpatientClearanceItemRequest
            {
                ItemCode = "ADM-DOC",
                ItemName = "Coba pakai kode milik butir lain",
                IsMandatory = true,
                SortOrder = 20
            },
            ActorUserId);

        Assert.Equal(InpatientClearanceItemStatus.DuplicateCode, hasil.Status);
    }

    [Fact]
    public async Task MengubahButirDenganKodenyaSendiri_Diterima()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        var dibuat = await service.CreateAsync(BuildCreateRequest("ADM-DOC"), ActorUserId);

        var hasil = await service.UpdateAsync(
            dibuat.Entity!.Id,
            new UpdateInpatientClearanceItemRequest
            {
                ItemCode = "ADM-DOC",
                ItemName = "Berkas administrasi pasien lengkap dan sudah diverifikasi",
                IsMandatory = true,
                SortOrder = 15
            },
            ActorUserId);

        Assert.Equal(InpatientClearanceItemStatus.Success, hasil.Status);
        Assert.Equal(15, hasil.Entity!.SortOrder);
    }

    [Fact]
    public async Task MenonaktifkanButirWajib_TidakMenghapusPenandaanPadaEpisodeLama()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        var butir = (await service.CreateAsync(BuildCreateRequest("ADM-DOC"), ActorUserId)).Entity!;

        // Penandaan milik episode Ny. Sari yang sudah ditutup bulan lalu.
        var episodeId = Guid.NewGuid();
        db.Set<InpClearanceMark>().Add(new InpClearanceMark
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            ClearanceItemId = butir.Id,
            MarkedAt = new DateTime(2026, 7, 15, 9, 0, 0, DateTimeKind.Utc),
            MarkedByUserId = ActorUserId,
            IsActive = true,
            CreateDateTime = new DateTime(2026, 7, 15, 9, 0, 0, DateTimeKind.Utc),
            CreateBy = ActorUserId
        });
        await db.SaveChangesAsync();

        var hasil = await service.UpdateStatusAsync(butir.Id, isActive: false, ActorUserId);

        Assert.Equal(InpatientClearanceItemStatus.Success, hasil.Status);
        Assert.False(hasil.Entity!.IsActive);

        // RWI-DEC-032: butir yang tidak berlaku lagi dinonaktifkan, bukan dihapus, dan
        // penandaan yang sudah ada tetap utuh. Menghapusnya akan membuat riwayat
        // penutupan episode lama berbohong kepada auditor.
        var penandaan = await db.Set<InpClearanceMark>().SingleAsync();
        Assert.Equal(episodeId, penandaan.EpisodeId);
        Assert.True(penandaan.IsActive);
        Assert.False(penandaan.IsDelete);
    }

    [Fact]
    public async Task MenghapusButir_BersifatLunakDanTidakMenyentuhPenandaan()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        var butir = (await service.CreateAsync(BuildCreateRequest("RETURN-ITEM"), ActorUserId)).Entity!;

        db.Set<InpClearanceMark>().Add(new InpClearanceMark
        {
            Id = Guid.NewGuid(),
            EpisodeId = Guid.NewGuid(),
            ClearanceItemId = butir.Id,
            MarkedByUserId = ActorUserId,
            CreateBy = ActorUserId
        });
        await db.SaveChangesAsync();

        var hasil = await service.DeleteAsync(butir.Id, ActorUserId);

        Assert.Equal(InpatientClearanceItemStatus.Success, hasil.Status);
        Assert.True(hasil.Entity!.IsDelete);
        Assert.False(hasil.Entity.IsActive);

        // Barisnya tetap ada di tabel, hanya ditandai terhapus.
        Assert.Equal(1, await db.Set<MstInpatientClearanceItem>().CountAsync());
        Assert.Equal(1, await db.Set<InpClearanceMark>().CountAsync());
    }

    [Fact]
    public async Task ButirTerhapus_TidakMunculPadaDaftarDanTidakMenahanKodenya()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        var butir = (await service.CreateAsync(BuildCreateRequest("ADM-DOC"), ActorUserId)).Entity!;
        await service.DeleteAsync(butir.Id, ActorUserId);

        var daftar = await service.GetPagedAsync(
            search: null,
            isMandatory: null,
            isActive: null,
            sortBy: null,
            sortDirection: "asc",
            pageNumber: 1,
            pageSize: 25);

        Assert.Equal(0, daftar.TotalData);
        Assert.Null(await service.GetByIdAsync(butir.Id));
    }

    [Fact]
    public async Task DaftarButir_DisaringDanDiurutkanSesuaiPermintaan()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        await service.CreateAsync(BuildCreateRequest("ADM-DOC", isMandatory: true, sortOrder: 10), ActorUserId);
        await service.CreateAsync(BuildCreateRequest("RETURN-ITEM", isMandatory: true, sortOrder: 20), ActorUserId);
        await service.CreateAsync(BuildCreateRequest("DISCHARGE-MED", isMandatory: false, sortOrder: 30), ActorUserId);

        var wajib = await service.GetPagedAsync(
            search: null,
            isMandatory: true,
            isActive: null,
            sortBy: null,
            sortDirection: "asc",
            pageNumber: 1,
            pageSize: 25);

        Assert.Equal(2, wajib.TotalData);
        Assert.Equal(new[] { "ADM-DOC", "RETURN-ITEM" }, wajib.Items.Select(x => x.ItemCode));

        var pencarian = await service.GetPagedAsync(
            search: "discharge",
            isMandatory: null,
            isActive: null,
            sortBy: null,
            sortDirection: "asc",
            pageNumber: 1,
            pageSize: 25);

        Assert.Equal(1, pencarian.TotalData);
        Assert.Equal("DISCHARGE-MED", pencarian.Items[0].ItemCode);
    }

    [Fact]
    public async Task ButirTanpaKode_Ditolak()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        var hasil = await service.CreateAsync(BuildCreateRequest("   "), ActorUserId);

        Assert.Equal(InpatientClearanceItemStatus.Invalid, hasil.Status);
        Assert.Equal(0, await db.Set<MstInpatientClearanceItem>().CountAsync());
    }

    [Fact]
    public async Task ButirYangTidakAda_MengembalikanNotFoundBukanGalat()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientClearanceItemService(db);

        var acak = Guid.NewGuid();

        Assert.Equal(
            InpatientClearanceItemStatus.NotFound,
            (await service.UpdateStatusAsync(acak, true, ActorUserId)).Status);

        Assert.Equal(
            InpatientClearanceItemStatus.NotFound,
            (await service.DeleteAsync(acak, ActorUserId)).Status);
    }

    private static CreateInpatientClearanceItemRequest BuildCreateRequest(
        string itemCode,
        bool isMandatory = true,
        int sortOrder = 10)
        => new()
        {
            ItemCode = itemCode,
            ItemName = $"Butir {itemCode}",
            IsMandatory = isMandatory,
            SortOrder = sortOrder,
            IsActive = true
        };
}
