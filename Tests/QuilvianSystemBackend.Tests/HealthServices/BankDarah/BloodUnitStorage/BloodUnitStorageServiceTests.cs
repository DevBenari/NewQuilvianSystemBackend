using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Reflection;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.BloodUnitStorage;

/// <summary>
/// Bukti untuk <c>BE-BD-015</c> — kantong disimpan, dipindahkan, riwayatnya tak pernah ditimpa —
/// blueprint <c>BD-BP-001</c>, roadmap revisi 9, kontrak <c>v4</c>.
/// </summary>
/// <remarks>
/// <para>
/// Acceptance criteria: <c>AC-BD-060</c>..<c>AC-BD-063</c>, <c>AC-BD-065</c>..<c>AC-BD-070</c>, serta
/// <c>AC-BD-023</c> dan <c>AC-BD-032</c> yang diteruskan dari <c>BE-BD-004</c>. Ditambah skenario
/// risiko rancangan pada matriks acceptance §7: master kosong, pengaktifan kembali, perpindahan kantong
/// <c>Allocated</c>, dan tidak ada jalur menyunting riwayat.
/// </para>
/// <para>
/// Kantong di sini lahir lewat jalur sebenarnya — order, permintaan PMI, penerimaan — bukan disisipkan
/// langsung, supaya catatan penerimaan awal yang dijaga <c>AC-BD-063</c> memang ada.
/// </para>
/// <para>
/// <b>Batas yang jujur.</b> Provider InMemory tidak bertransaksi dan tidak mengenal index unik
/// terfilter. Perebutan antar-koneksi dan penolakan index fisik dibuktikan pada
/// <c>BloodUnitStoragePostgresTests</c>.
/// </para>
/// </remarks>
public partial class BloodUnitStorageServiceTests
{
    private static readonly Guid PetugasUnit = Guid.Parse("81818181-8181-8181-8181-818181818181");
    private static readonly Guid PetugasBdrs = Guid.Parse("82828282-8282-8282-8282-828282828282");
    private static readonly Guid PetugasBdrsLain = Guid.Parse("83838383-8383-8383-8383-838383838383");

    // Pesan kanonis validation-matrix §4b, persis.
    private const string PesanVal060 =
        "Lokasi penyimpanan itu sudah tidak aktif dan tidak dapat dipilih. Pilih lokasi lain yang masih aktif.";
    private const string PesanVal061 =
        "Kantong ini sudah punya lokasi penyimpanan. Gunakan perpindahan lokasi bila ingin memindahkannya.";
    private const string PesanVal062 =
        "Kantong ini belum punya lokasi penyimpanan. Tetapkan lokasinya lebih dulu.";
    private const string PesanVal063 =
        "Kantong belum disimpan pada lokasi penyimpanan, sehingga belum dapat dialokasikan. Tetapkan lokasi penyimpanannya lebih dulu.";
    private const string PesanVal064 =
        "Kantong ini berada di lokasi penyimpanan yang sudah tidak aktif. Pindahkan dulu ke lokasi yang aktif sebelum dialokasikan.";

    private static string PesanVal068(int n)
        => $"Lokasi dinonaktifkan. Ada {n} kantong yang masih tercatat di sana dan belum dapat dialokasikan sampai dipindahkan ke lokasi aktif.";

    // =====================================================================
    // 1. Penetapan lokasi pertama — AC-BD-061, AC-BD-062, AC-BD-065, VAL-BD-061
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-061</c> — kantong <c>Received</c> ditaruh di lokasi aktif: <c>Stored</c> lalu
    /// <c>Available</c>, dan tepat satu baris riwayat penempatan bertambah.
    /// </summary>
    [Fact]
    public async Task AC_BD_061_KantongReceivedDisimpan_StoredLaluAvailable_SatuRiwayatPenempatan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        var hasil = await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar, "Rak atas"), PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.Success, hasil.Outcome);
        Assert.Equal(BbkBloodUnitStatus.Available, hasil.Entity!.UnitStatus);

        await using var baca = l.CreateContext();

        var penempatan = Assert.Single(await baca.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id).ToListAsync());
        Assert.Equal(l.D.KulkasBesar, penempatan.StorageLocationId);
        Assert.Null(penempatan.PreviousPlacementId);
        Assert.True(penempatan.IsCurrent);
        Assert.Equal(PetugasBdrs, penempatan.PlacedByUserId);
        Assert.Equal("Rak atas", penempatan.Note);

        var kantong = await baca.BbkBloodUnits.SingleAsync(x => x.Id == id);
        Assert.Equal(penempatan.Id, kantong.CurrentPlacementId);
        Assert.Equal(1, kantong.Version);

        var riwayat = (await l.Service().GetDetailAsync(id))!.Transitions;
        Assert.Equal(new[] { "Receive", "Store", "MakeAvailable" }, riwayat.Select(x => x.Action));
        Assert.Equal(new string?[] { null, "Received", "Stored" }, riwayat.Select(x => x.FromStatus));
        Assert.Equal(new[] { "Received", "Stored", "Available" }, riwayat.Select(x => x.ToStatus));
        Assert.All(riwayat.Skip(1), x => Assert.Equal(PetugasBdrs, x.ActorUserId));
        Assert.All(riwayat.Skip(1), x => Assert.Equal(penempatan.Id, x.CorrelationId));
    }

    /// <summary>
    /// <c>AC-BD-062</c> dan <c>AC-BD-065</c> — dua kriteria dengan skenario yang sama: lokasi nonaktif
    /// dipilih untuk penyimpanan kantong baru.
    /// </summary>
    [Fact]
    public async Task AC_BD_062_AC_BD_065_LokasiNonaktifUntukPenyimpananBaru_Ditolak_VAL_BD_060()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        var hasil = await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasRusak), PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Equal(PesanVal060, hasil.Message);

        await AssertBelumPernahDisimpanAsync(l, id);
    }

    [Fact]
    public async Task PenetapanPertamaDuaKali_Ditolak_VAL_BD_061_TetapSatuPenempatan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);
        var kedua = await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasKecil), PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.NotAllowedByState, kedua.Outcome);
        Assert.Equal(PesanVal061, kedua.Message);

        await using var baca = l.CreateContext();
        var penempatan = Assert.Single(await baca.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id).ToListAsync());
        Assert.Equal(l.D.KulkasBesar, penempatan.StorageLocationId);
    }

    // =====================================================================
    // 2. Perpindahan — AC-BD-063, AC-BD-066, VAL-BD-062
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-063</c> — kantong yang sudah tersimpan dipindahkan: riwayat bertambah, penempatan lama
    /// tetap ada, status tidak berubah, dan catatan penerimaan awal tidak tersentuh.
    /// </summary>
    /// <remarks>
    /// Kantong "tersimpan" dibaca sebagai kantong yang sudah melewati <c>Stored</c>: matriks §3
    /// menjadikan <c>Stored</c> → <c>Available</c> akibat langsung penempatan, sehingga status yang
    /// tersimpan sesudah penempatan pertama selalu <c>Available</c> atau <c>PendingReview</c>.
    /// </remarks>
    [Fact]
    public async Task AC_BD_063_KantongTersimpanDipindahkan_RiwayatBertambah_StatusDanPenerimaanAwalTidakTersentuh()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);

        BbkBloodUnitReceipt penerimaanSebelum;
        BbkBloodUnitPlacement lamaSebelum;
        List<BbkTransitionHistory> riwayatSebelum;
        BbkBloodUnit kantongSebelum;

        await using (var baca = l.CreateContext())
        {
            kantongSebelum = await baca.BbkBloodUnits.AsNoTracking().SingleAsync(x => x.Id == id);
            penerimaanSebelum = await baca.BbkBloodUnitReceipts.AsNoTracking().SingleAsync(x => x.Id == kantongSebelum.ReceiptId);
            lamaSebelum = await baca.BbkBloodUnitPlacements.AsNoTracking().SingleAsync(x => x.BloodUnitId == id);
            riwayatSebelum = await baca.BbkTransitionHistories.AsNoTracking().Where(x => x.EntityId == id).ToListAsync();
        }

        var hasil = await l.Service().MoveStorageLocationAsync(id, Pindah(l.D.KulkasKecil, "Kulkas Besar akan diservis"), PetugasBdrsLain);

        Assert.Equal(BloodUnitOutcome.Success, hasil.Outcome);

        await using var sesudah = l.CreateContext();

        var penempatan = await sesudah.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id).ToListAsync();
        Assert.Equal(2, penempatan.Count);

        var lama = penempatan.Single(x => x.Id == lamaSebelum.Id);
        Assert.False(lama.IsCurrent);
        Assert.Equal(lamaSebelum.StorageLocationId, lama.StorageLocationId);
        Assert.Equal(lamaSebelum.PlacedAt, lama.PlacedAt);
        Assert.Equal(lamaSebelum.PlacedByUserId, lama.PlacedByUserId);
        Assert.Null(lama.PreviousPlacementId);

        var baru = penempatan.Single(x => x.Id != lamaSebelum.Id);
        Assert.True(baru.IsCurrent);
        Assert.Equal(lama.Id, baru.PreviousPlacementId);
        Assert.Equal(l.D.KulkasKecil, baru.StorageLocationId);
        Assert.Equal(PetugasBdrsLain, baru.PlacedByUserId);
        Assert.Equal("Kulkas Besar akan diservis", baru.Note);

        var kantong = await sesudah.BbkBloodUnits.SingleAsync(x => x.Id == id);
        Assert.Equal(BbkBloodUnitStatus.Available, kantong.UnitStatus);
        Assert.Equal(baru.Id, kantong.CurrentPlacementId);
        Assert.Equal(kantongSebelum.ProviderRequestId, kantong.ProviderRequestId);
        Assert.Equal(kantongSebelum.ReceiptId, kantong.ReceiptId);
        Assert.Equal(kantongSebelum.PmiBagNumber, kantong.PmiBagNumber);
        Assert.Equal(kantongSebelum.Version + 1, kantong.Version);

        // Catatan penerimaan awal dan riwayat status tidak tersentuh sama sekali.
        var penerimaan = await sesudah.BbkBloodUnitReceipts.SingleAsync(x => x.Id == kantong.ReceiptId);
        Assert.Equal(penerimaanSebelum.ReceivedAt, penerimaan.ReceivedAt);
        Assert.Equal(penerimaanSebelum.ReceivedByUserId, penerimaan.ReceivedByUserId);
        Assert.Equal(penerimaanSebelum.Sequence, penerimaan.Sequence);

        var riwayat = await sesudah.BbkTransitionHistories.Where(x => x.EntityId == id).ToListAsync();
        Assert.Equal(riwayatSebelum.Select(x => x.Id).OrderBy(x => x), riwayat.Select(x => x.Id).OrderBy(x => x));
    }

    /// <summary><c>AC-BD-066</c> — aturan lokasi aktif berlaku juga untuk tujuan perpindahan.</summary>
    [Fact]
    public async Task AC_BD_066_LokasiNonaktifSebagaiTujuanPerpindahan_Ditolak_VAL_BD_060()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);

        var hasil = await l.Service().MoveStorageLocationAsync(id, Pindah(l.D.KulkasRusak), PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Equal(PesanVal060, hasil.Message);

        await using var baca = l.CreateContext();
        var penempatan = Assert.Single(await baca.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id).ToListAsync());
        Assert.Equal(l.D.KulkasBesar, penempatan.StorageLocationId);
        Assert.True(penempatan.IsCurrent);
        Assert.Equal(1, (await baca.BbkBloodUnits.SingleAsync(x => x.Id == id)).Version);
    }

    [Fact]
    public async Task PerpindahanKantongYangBelumPernahDisimpan_Ditolak_VAL_BD_062()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        var hasil = await l.Service().MoveStorageLocationAsync(id, Pindah(l.D.KulkasBesar), PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Equal(PesanVal062, hasil.Message);

        await AssertBelumPernahDisimpanAsync(l, id);
    }

    // =====================================================================
    // 3. Gerbang alokasi dari sisi penyimpanan — AC-BD-060, AC-BD-068, AC-BD-070
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-060</c> — kantong <c>Received</c> tertahan gerbang alokasi dengan <c>VAL-BD-063</c>.
    /// Endpoint alokasi yang memanggil gerbang ini lahir pada <c>BE-BD-006</c>.
    /// </summary>
    [Fact]
    public async Task AC_BD_060_KantongReceived_GerbangAlokasiTertutup_VAL_BD_063()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        var gerbang = (await l.Service().EvaluateAllocationGateAsync(id))!;

        Assert.False(gerbang.IsOpen);
        Assert.Equal("VAL-BD-063", gerbang.RuleCode);
        Assert.Equal(PesanVal063, gerbang.Message);

        // Tidak ada aksi alokasi yang ditawarkan; satu-satunya aksi adalah menyimpan.
        Assert.Equal(new[] { "AssignStorageLocation" }, (await l.Service().GetDetailAsync(id))!.AvailableActions);
    }

    /// <summary><c>AC-BD-068</c> — kantong di lokasi nonaktif tertahan gerbang alokasi dengan <c>VAL-BD-064</c>.</summary>
    [Fact]
    public async Task AC_BD_068_KantongDiLokasiNonaktif_GerbangAlokasiTertutup_VAL_BD_064()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);
        Assert.True((await l.Service().EvaluateAllocationGateAsync(id))!.IsOpen);

        await l.LocationService().UpdateStatusAsync(l.D.KulkasBesar, isActive: false, PetugasBdrs);

        var gerbang = (await l.Service().EvaluateAllocationGateAsync(id))!;

        Assert.False(gerbang.IsOpen);
        Assert.Equal("VAL-BD-064", gerbang.RuleCode);
        Assert.Equal(PesanVal064, gerbang.Message);
    }

    /// <summary>
    /// <c>AC-BD-070</c> — petugas memindahkan kantong dari lokasi nonaktif ke lokasi aktif: berhasil,
    /// riwayat mencatat pelaku dan waktu, dan gerbang alokasi terbuka kembali.
    /// </summary>
    [Fact]
    public async Task AC_BD_070_DipindahkanDariLokasiNonaktifKeAktif_PelakuDanWaktuTercatat_GerbangTerbukaKembali()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);
        await l.LocationService().UpdateStatusAsync(l.D.KulkasBesar, isActive: false, PetugasBdrs);
        Assert.False((await l.Service().EvaluateAllocationGateAsync(id))!.IsOpen);

        var sebelum = DateTime.UtcNow;
        var hasil = await l.Service().MoveStorageLocationAsync(id, Pindah(l.D.KulkasKecil), PetugasBdrsLain);
        var sesudah = DateTime.UtcNow;

        Assert.Equal(BloodUnitOutcome.Success, hasil.Outcome);
        Assert.True((await l.Service().EvaluateAllocationGateAsync(id))!.IsOpen);

        var riwayat = (await l.Service().GetPlacementsAsync(id))!;
        Assert.Equal(2, riwayat.Count);

        var asal = riwayat[0];
        Assert.Equal(l.D.KulkasBesar, asal.StorageLocationId);
        Assert.False(asal.IsStorageLocationActive);
        Assert.False(asal.IsCurrent);

        var tujuan = riwayat[1];
        Assert.Equal(l.D.KulkasKecil, tujuan.StorageLocationId);
        Assert.Equal(asal.Id, tujuan.PreviousPlacementId);
        Assert.Equal(l.D.KulkasBesar, tujuan.PreviousStorageLocationId);
        Assert.Equal("KLK-BSR", tujuan.PreviousStorageLocationCode);
        Assert.Equal(PetugasBdrsLain, tujuan.PlacedByUserId);
        Assert.InRange(tujuan.PlacedAt, sebelum, sesudah);
        Assert.True(tujuan.IsCurrent);
    }

    // =====================================================================
    // 4. Penonaktifan lokasi — AC-BD-067, AC-BD-069
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-067</c> — lokasi yang masih berisi kantong dinonaktifkan: berhasil, kantong tetap
    /// tercatat di sana dengan status yang sama, dan peringatan <c>VAL-BD-068</c> menyebut jumlahnya.
    /// Kantong yang sudah keluar dari stok tidak ikut terhitung.
    /// </summary>
    [Fact]
    public async Task AC_BD_067_LokasiBerisiKantongDinonaktifkan_Berhasil_KantongTetapDiSana_PeringatanMenyebutJumlah()
    {
        await using var l = await Lingkungan.BuatAsync();
        var kantong = await l.SiapkanKantongAsync(4);

        foreach (var id in kantong.Take(3))
            await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);

        await l.Service().AssignStorageLocationAsync(kantong[3], Simpan(l.D.KulkasKecil), PetugasBdrs);

        // Kantong ketiga sudah keluar dari stok. Statusnya disetel langsung karena pengembalian ke
        // PMI lahir pada BE-BD-009; yang diuji di sini hanya bahwa ia tidak lagi terhitung.
        await l.SetelStatusAsync(kantong[2], BbkBloodUnitStatus.ReturnedToProvider);

        var sebelum = await l.PotretKantongAsync();

        var hasil = await l.LocationService().UpdateStatusAsync(l.D.KulkasBesar, isActive: false, PetugasBdrs);

        Assert.Equal(BloodStorageLocationStatus.Success, hasil.Status);
        Assert.False(hasil.Entity!.IsActive);
        Assert.Equal(2, hasil.HeldUnitCount);
        Assert.Equal(PesanVal068(2), hasil.Message);

        Assert.Equal(sebelum, await l.PotretKantongAsync());
    }

    /// <summary>
    /// <c>AC-BD-069</c> — sistem tidak memindahkan kantong sendiri saat lokasinya dinonaktifkan: nol
    /// penempatan baru, nol perubahan status, nol riwayat baru, dan nol job — lewat PATCH status
    /// maupun lewat PUT yang ikut menonaktifkan.
    /// </summary>
    [Fact]
    public async Task AC_BD_069_PenonaktifanLokasi_TidakMemindahkanKantong_TidakMengubahStatus_TanpaJob()
    {
        await using var l = await Lingkungan.BuatAsync();
        var kantong = await l.SiapkanKantongAsync(2);

        await l.Service().AssignStorageLocationAsync(kantong[0], Simpan(l.D.KulkasBesar), PetugasBdrs);
        await l.Service().AssignStorageLocationAsync(kantong[1], Simpan(l.D.KulkasKecil), PetugasBdrs);

        var kantongSebelum = await l.PotretKantongAsync();
        var penempatanSebelum = await l.PotretPenempatanAsync();
        var riwayatSebelum = await l.HitungRiwayatAsync();

        await l.LocationService().UpdateStatusAsync(l.D.KulkasBesar, isActive: false, PetugasBdrs);

        var lewatPut = await l.LocationService().UpdateAsync(
            l.D.KulkasKecil,
            new UpdateBloodStorageLocationRequest
            {
                StorageLocationCode = "KLK-KCL",
                StorageLocationName = "Kulkas Kecil",
                IsActive = false
            },
            PetugasBdrs);

        Assert.Equal(PesanVal068(1), lewatPut.Message);

        Assert.Equal(kantongSebelum, await l.PotretKantongAsync());
        Assert.Equal(penempatanSebelum, await l.PotretPenempatanAsync());
        Assert.Equal(riwayatSebelum, await l.HitungRiwayatAsync());

        // Tidak ada background job pada modul ini: tidak satu tipe pun di Bank Darah maupun master
        // lokasi yang dapat didaftarkan sebagai hosted service.
        var hosted = typeof(BbkBloodUnitService).Assembly
            .GetTypes()
            .Where(x => typeof(IHostedService).IsAssignableFrom(x) &&
                        x.Namespace != null &&
                        (x.Namespace.StartsWith("QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement", StringComparison.Ordinal) ||
                         x.Name.Contains("BloodStorageLocation", StringComparison.Ordinal)))
            .ToList();

        Assert.Empty(hosted);
    }

    [Fact]
    public async Task LokasiDinonaktifkanLaluDiaktifkanKembali_GerbangTerbukaTanpaSatuKantongPunDisunting()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);
        var sebelum = await l.PotretKantongAsync();

        await l.LocationService().UpdateStatusAsync(l.D.KulkasBesar, isActive: false, PetugasBdrs);
        Assert.False((await l.Service().EvaluateAllocationGateAsync(id))!.IsOpen);

        await l.LocationService().UpdateStatusAsync(l.D.KulkasBesar, isActive: true, PetugasBdrs);
        Assert.True((await l.Service().EvaluateAllocationGateAsync(id))!.IsOpen);

        Assert.Equal(sebelum, await l.PotretKantongAsync());
    }

    /// <summary>
    /// Master kosong: tidak ada satu pun lokasi aktif. Kantong berhenti di <c>Received</c>, modul
    /// <i>fail-closed</i>, dan pesannya mengarahkan ke Setup (<c>INV-BD-025</c>).
    /// </summary>
    [Fact]
    public async Task TanpaSatuPunLokasiAktif_KantongBerhentiDiReceived_PesanMengarahkanKeSetup()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.LocationService().UpdateStatusAsync(l.D.KulkasBesar, isActive: false, PetugasBdrs);
        await l.LocationService().UpdateStatusAsync(l.D.KulkasKecil, isActive: false, PetugasBdrs);
        await l.LocationService().UpdateStatusAsync(l.D.KulkasCadangan, isActive: false, PetugasBdrs);

        var hasil = await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Contains("Belum ada lokasi penyimpanan darah yang aktif", hasil.Message);
        Assert.Contains("Setup", hasil.Message);

        await AssertBelumPernahDisimpanAsync(l, id);
        Assert.Equal("VAL-BD-063", (await l.Service().EvaluateAllocationGateAsync(id))!.RuleCode);
    }

    // =====================================================================
    // 5. PendingReview — AC-BD-023 dan AC-BD-032, diteruskan dari BE-BD-004
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-023</c> penuh — kantong yang datang sesudah permintaannya <c>ClosedEncounter</c>
    /// disimpan lebih dulu, lalu masuk <c>PendingReview</c>, tetap membawa asal dan riwayatnya.
    /// </summary>
    /// <remarks>
    /// Bagian penerimaannya sudah dibuktikan <c>BE-BD-004</c>; test ini mengulang jalurnya dari awal
    /// supaya perpindahan <c>Received</c> → <c>Stored</c> → <c>PendingReview</c> terbukti pada kantong
    /// yang benar-benar lahir dari penerimaan susulan.
    /// </remarks>
    [Fact]
    public async Task AC_BD_023_KantongSesudahClosedEncounter_DisimpanLaluPendingReview_AsalDanRiwayatUtuh()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRj001, (l.D.Prc, 3));
        await l.UbahStatusKunjunganAsync(l.D.KunjunganRj001, EncounterStatus.Completed);
        await l.RequestService().CloseForEncounterEndAsync(permintaan.Id, PetugasBdrs);

        var id = (await l.TerimaAsync(permintaan.Id, (l.D.Prc, "PMI-L-001")))[0];

        Assert.Equal(BbkBloodUnitStatus.Received, (await l.Service().GetDetailAsync(id))!.UnitStatus);
        Assert.Equal(0, (await DaftarPendingReviewAsync(l)).TotalData);

        var hasil = await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.Success, hasil.Outcome);

        var detail = (await l.Service().GetDetailAsync(id))!;
        Assert.Equal(BbkBloodUnitStatus.PendingReview, detail.UnitStatus);
        Assert.Equal(permintaan.Id, detail.ProviderRequestId);
        Assert.Equal(BbkProviderRequestStatus.ClosedEncounter, detail.RequestStatus);
        Assert.Equal(l.D.KulkasBesar, detail.CurrentStorageLocationId);

        Assert.Equal(new[] { "Receive", "Store", "HoldForReview" }, detail.Transitions.Select(x => x.Action));
        Assert.Equal("PendingReview", detail.Transitions[2].ToStatus);
        Assert.Equal("Permintaan asal sudah ditutup karena kunjungan pasien berakhir.", detail.Transitions[2].ReasonNote);

        var daftar = await DaftarPendingReviewAsync(l);
        Assert.Equal(id, Assert.Single(daftar.Items).Id);
    }

    /// <summary>
    /// <c>AC-BD-032</c> penuh — kantong ke-3 pada permintaan 2 kantong disimpan, lalu masuk
    /// <c>PendingReview</c> dengan alasan "kiriman melebihi permintaan", dan muncul di daftar kerja #2
    /// (<c>unitStatus=PendingReview</c>). Dua kantong lainnya menjadi <c>Available</c>.
    /// </summary>
    [Fact]
    public async Task AC_BD_032_KantongKetigaDisimpan_PendingReviewDenganAlasanKelebihan_MunculDiDaftarKerjaDua()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));
        var kantong = await l.TerimaAsync(
            permintaan.Id,
            (l.D.Prc, "PMI-C-001"),
            (l.D.Prc, "PMI-C-002"),
            (l.D.Prc, "PMI-C-003"));

        foreach (var id in kantong)
            await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);

        var detail = new List<BloodUnitDetailDto>();
        foreach (var id in kantong)
            detail.Add((await l.Service().GetDetailAsync(id))!);

        Assert.Equal(
            new[] { BbkBloodUnitStatus.Available, BbkBloodUnitStatus.Available, BbkBloodUnitStatus.PendingReview },
            detail.Select(x => x.UnitStatus));

        var berlebih = detail[2];
        Assert.True(berlebih.IsExcess);
        Assert.Equal("HoldForReview", berlebih.Transitions.Last().Action);
        Assert.Equal("Kiriman melebihi permintaan.", berlebih.Transitions.Last().ReasonNote);

        var daftar = await DaftarPendingReviewAsync(l);
        var baris = Assert.Single(daftar.Items);
        Assert.Equal(kantong[2], baris.Id);
        Assert.True(baris.IsExcess);
        Assert.Equal("Menunggu keputusan", baris.UnitStatusLabel);
        Assert.Equal("Kulkas Besar", baris.CurrentStorageLocationName);
    }

    // =====================================================================
    // 6. Keadaan kantong lain dan konkurensi
    // =====================================================================

    /// <summary>
    /// Perpindahan pada kantong <c>Allocated</c>: status, pasien, dan seluruh data pemberiannya tetap.
    /// Status disetel langsung karena alokasi lahir pada <c>BE-BD-006</c>; yang dibuktikan adalah bahwa
    /// perpindahan hanya menyentuh penunjuk penempatan dan token kantong.
    /// </summary>
    [Fact]
    public async Task PerpindahanKantongAllocated_HanyaPenunjukPenempatanDanTokenYangBerubah()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);
        await l.SetelStatusAsync(id, BbkBloodUnitStatus.Allocated);

        BbkBloodUnit sebelum;
        await using (var baca = l.CreateContext())
            sebelum = await baca.BbkBloodUnits.AsNoTracking().SingleAsync(x => x.Id == id);

        var hasil = await l.Service().MoveStorageLocationAsync(id, Pindah(l.D.KulkasKecil), PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.Success, hasil.Outcome);

        await using var sesudah = l.CreateContext();
        var kantong = await sesudah.BbkBloodUnits.SingleAsync(x => x.Id == id);

        Assert.Equal(BbkBloodUnitStatus.Allocated, kantong.UnitStatus);
        Assert.Equal(sebelum.IssuedToPatientId, kantong.IssuedToPatientId);
        Assert.Equal(sebelum.IssuedAt, kantong.IssuedAt);
        Assert.Equal(sebelum.IssuedByUserId, kantong.IssuedByUserId);
        Assert.Equal(sebelum.IssuedViaEmergency, kantong.IssuedViaEmergency);
        Assert.Equal(sebelum.IsExcess, kantong.IsExcess);
        Assert.Equal(sebelum.ProviderRequestId, kantong.ProviderRequestId);
        Assert.Equal(sebelum.ReceiptId, kantong.ReceiptId);
        Assert.NotEqual(sebelum.CurrentPlacementId, kantong.CurrentPlacementId);
        Assert.Equal(sebelum.Version + 1, kantong.Version);
    }

    [Theory]
    [InlineData(BbkBloodUnitStatus.Issued)]
    [InlineData(BbkBloodUnitStatus.ReturnedToProvider)]
    [InlineData(BbkBloodUnitStatus.NotUsable)]
    public async Task KantongBerstatusAkhir_TidakDapatDipindahkan(BbkBloodUnitStatus statusAkhir)
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);
        await l.SetelStatusAsync(id, statusAkhir);

        var hasil = await l.Service().MoveStorageLocationAsync(id, Pindah(l.D.KulkasKecil), PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Contains("sudah keluar dari stok", hasil.Message);

        await using var baca = l.CreateContext();
        Assert.Single(await baca.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id).ToListAsync());
        Assert.Empty((await l.Service().GetDetailAsync(id))!.AvailableActions);
    }

    [Fact]
    public async Task TokenVersionDariLayarUsang_Ditolak409_TanpaPenempatanBaru()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        var usang = await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar, versi: 7), PetugasBdrs);
        Assert.Equal(BloodUnitOutcome.VersionConflict, usang.Outcome);
        await AssertBelumPernahDisimpanAsync(l, id);

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar, versi: 0), PetugasBdrs);

        var pindahUsang = await l.Service().MoveStorageLocationAsync(id, Pindah(l.D.KulkasKecil, versi: 0), PetugasBdrs);
        Assert.Equal(BloodUnitOutcome.VersionConflict, pindahUsang.Outcome);

        await using var baca = l.CreateContext();
        Assert.Single(await baca.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id).ToListAsync());
    }

    /// <summary>
    /// Dua petugas membuka kantong yang sama lalu keduanya memindahkan ke lokasi berbeda. Token
    /// <c>Version</c> menolak yang kedua; kantong tetap punya tepat satu penempatan berlaku.
    /// </summary>
    [Fact]
    public async Task DuaPetugasMemindahkanBersamaan_YangKeduaDitolak409_TetapSatuPenempatanBerlaku()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);

        await using var konteksA = l.CreateContext();
        await using var konteksB = l.CreateContext();

        await konteksA.BbkBloodUnits.SingleAsync(x => x.Id == id);
        await konteksB.BbkBloodUnits.SingleAsync(x => x.Id == id);

        var pertama = await l.Service(konteksA).MoveStorageLocationAsync(id, Pindah(l.D.KulkasKecil), PetugasBdrs);
        var kedua = await l.Service(konteksB).MoveStorageLocationAsync(id, Pindah(l.D.KulkasCadangan), PetugasBdrsLain);

        Assert.Equal(BloodUnitOutcome.Success, pertama.Outcome);
        Assert.Equal(BloodUnitOutcome.VersionConflict, kedua.Outcome);

        await using var baca = l.CreateContext();
        var berlaku = Assert.Single(await baca.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id && x.IsCurrent).ToListAsync());
        Assert.Equal(l.D.KulkasKecil, berlaku.StorageLocationId);
        Assert.Equal(berlaku.Id, (await baca.BbkBloodUnits.SingleAsync(x => x.Id == id)).CurrentPlacementId);
    }

    // =====================================================================
    // 7. Isian tidak sah dan pembacaan
    // =====================================================================

    [Theory]
    [InlineData("tanpa-pelaku")]
    [InlineData("lokasi-kosong")]
    [InlineData("lokasi-acak")]
    [InlineData("lokasi-dihapus")]
    [InlineData("keterangan-panjang")]
    public async Task IsianTidakSah_Ditolak400_KantongTetapReceived(string kasus)
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        if (kasus == "lokasi-dihapus")
            Assert.Equal(BloodStorageLocationStatus.Success, (await l.LocationService().DeleteAsync(l.D.KulkasCadangan, PetugasBdrs)).Status);

        var permintaan = kasus switch
        {
            "lokasi-kosong" => Simpan(Guid.Empty),
            "lokasi-acak" => Simpan(Guid.NewGuid()),
            "lokasi-dihapus" => Simpan(l.D.KulkasCadangan),
            "keterangan-panjang" => Simpan(l.D.KulkasBesar, new string('x', 501)),
            _ => Simpan(l.D.KulkasBesar)
        };

        var hasil = await l.Service().AssignStorageLocationAsync(id, permintaan, kasus == "tanpa-pelaku" ? Guid.Empty : PetugasBdrs);

        Assert.Equal(BloodUnitOutcome.Invalid, hasil.Outcome);
        await AssertBelumPernahDisimpanAsync(l, id);
    }

    [Fact]
    public async Task KantongTidakAda_NotFound_UntukSeluruhTindakanDanPembacaan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var acak = Guid.NewGuid();

        Assert.Equal(BloodUnitOutcome.NotFound, (await l.Service().AssignStorageLocationAsync(acak, Simpan(l.D.KulkasBesar), PetugasBdrs)).Outcome);
        Assert.Equal(BloodUnitOutcome.NotFound, (await l.Service().MoveStorageLocationAsync(acak, Pindah(l.D.KulkasBesar), PetugasBdrs)).Outcome);
        Assert.Null(await l.Service().GetPlacementsAsync(acak));
        Assert.Null(await l.Service().EvaluateAllocationGateAsync(acak));
    }

    [Fact]
    public async Task DaftarDanDetail_MembawaLokasiSaatIniDanPenandaNonaktif()
    {
        await using var l = await Lingkungan.BuatAsync();
        var kantong = await l.SiapkanKantongAsync(2);

        await l.Service().AssignStorageLocationAsync(kantong[0], Simpan(l.D.KulkasBesar), PetugasBdrs);

        Assert.Empty((await l.Service().GetPlacementsAsync(kantong[1]))!);

        var belumDisimpan = (await l.Service().GetDetailAsync(kantong[1]))!;
        Assert.Null(belumDisimpan.CurrentPlacementId);
        Assert.Null(belumDisimpan.IsCurrentStorageLocationActive);

        var tersimpan = (await l.Service().GetDetailAsync(kantong[0]))!;
        Assert.Equal("KLK-BSR", tersimpan.CurrentStorageLocationCode);
        Assert.Equal("Kulkas Besar", tersimpan.CurrentStorageLocationName);
        Assert.True(tersimpan.IsCurrentStorageLocationActive);
        Assert.NotNull(tersimpan.CurrentPlacedAt);
        Assert.Equal(new[] { "MoveStorageLocation" }, tersimpan.AvailableActions);

        await l.LocationService().UpdateStatusAsync(l.D.KulkasBesar, isActive: false, PetugasBdrs);

        var daftar = await l.Service().GetPagedAsync(null, null, null, null, null, null, null, 1, 25);
        var baris = daftar.Items.Single(x => x.Id == kantong[0]);
        Assert.False(baris.IsCurrentStorageLocationActive);
        Assert.Equal("Kulkas Besar", baris.CurrentStorageLocationName);
        Assert.Null(daftar.Items.Single(x => x.Id == kantong[1]).IsCurrentStorageLocationActive);

        // Jalan keluar dari lokasi nonaktif tetap ditawarkan.
        Assert.Equal(new[] { "MoveStorageLocation" }, (await l.Service().GetDetailAsync(kantong[0]))!.AvailableActions);
    }

    // =====================================================================
    // 8. Hanya-tambah dan kolom tersimpan
    // =====================================================================

    /// <summary>
    /// Tidak ada jalur bisnis yang mengubah atau menghapus baris riwayat penempatan (<c>INV-BD-026</c>):
    /// service tidak punya method semacam itu, dan controller tidak punya endpoint <c>DELETE</c>,
    /// <c>PATCH</c>, maupun endpoint pemindahan massal.
    /// </summary>
    [Fact]
    public void RiwayatPenempatan_TidakPunyaJalurUbahAtauHapus()
    {
        var method = typeof(BbkBloodUnitService)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(x => x.Name)
            .ToList();

        Assert.DoesNotContain(method, x =>
            x.Contains("Delete", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("Remove", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("UpdatePlacement", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("Bulk", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("Batch", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task LokasiYangPernahDipakai_TidakDapatDihapus_HanyaDinonaktifkan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var id = (await l.SiapkanKantongAsync(1))[0];

        await l.Service().AssignStorageLocationAsync(id, Simpan(l.D.KulkasBesar), PetugasBdrs);

        var hasil = await l.LocationService().DeleteAsync(l.D.KulkasBesar, PetugasBdrs);

        Assert.Equal(BloodStorageLocationStatus.InUse, hasil.Status);

        await using var baca = l.CreateContext();
        var lokasi = await baca.MstBloodStorageLocations.SingleAsync(x => x.Id == l.D.KulkasBesar);
        Assert.False(lokasi.IsDelete);
        Assert.True(lokasi.IsActive);

        // Lokasi yang belum pernah dipakai tetap dapat dihapus.
        Assert.Equal(BloodStorageLocationStatus.Success, (await l.LocationService().DeleteAsync(l.D.KulkasCadangan, PetugasBdrs)).Status);
    }

    [Theory]
    [InlineData(typeof(BbkBloodUnitPlacement), "Id,BloodUnitId,StorageLocationId,PreviousPlacementId,PlacedAt,PlacedByUserId,IsCurrent,Note")]
    [InlineData(typeof(BbkBloodUnit), "Id,PmiBagNumber,ProviderRequestId,ReceiptId,BloodComponentId,IsExcess,UnitStatus,CurrentPlacementId,IssuedToPatientId,IssuedAt,IssuedByUserId,IssuedViaEmergency,Version")]
    public void KolomTersimpan_SamaPersisDenganKamusData(Type entity, string kolom)
    {
        Assert.True(typeof(IdentityModel).IsAssignableFrom(entity));

        var tersimpan = entity
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => IsScalar(x.PropertyType))
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(kolom.Split(',').OrderBy(x => x), tersimpan);
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static AssignStorageLocationRequest Simpan(Guid lokasi, string? catatan = null, int? versi = null)
        => new() { StorageLocationId = lokasi, Note = catatan, Version = versi };

    private static AssignStorageLocationRequest Simpan(Guid lokasi, int versi)
        => new() { StorageLocationId = lokasi, Version = versi };

    private static MoveStorageLocationRequest Pindah(Guid lokasi, string? catatan = null, int? versi = null)
        => new() { StorageLocationId = lokasi, Note = catatan, Version = versi };

    private static MoveStorageLocationRequest Pindah(Guid lokasi, int versi)
        => new() { StorageLocationId = lokasi, Version = versi };

    private static Task<QuilvianSystemBackend.Responses.PagedResult<BloodUnitListDto>> DaftarPendingReviewAsync(Lingkungan l)
        => l.Service().GetPagedAsync(null, BbkBloodUnitStatus.PendingReview, null, null, null, null, null, 1, 25);

    private static async Task AssertBelumPernahDisimpanAsync(Lingkungan l, Guid id)
    {
        await using var baca = l.CreateContext();

        var kantong = await baca.BbkBloodUnits.SingleAsync(x => x.Id == id);
        Assert.Equal(BbkBloodUnitStatus.Received, kantong.UnitStatus);
        Assert.Null(kantong.CurrentPlacementId);
        Assert.Equal(0, kantong.Version);
        Assert.False(await baca.BbkBloodUnitPlacements.AnyAsync(x => x.BloodUnitId == id));
    }

    private static bool IsScalar(Type type)
    {
        var dasar = Nullable.GetUnderlyingType(type) ?? type;

        return dasar.IsPrimitive || dasar.IsEnum || dasar == typeof(string) ||
               dasar == typeof(Guid) || dasar == typeof(DateTime) || dasar == typeof(decimal);
    }

    private sealed record Dunia(
        Guid PasienA,
        Guid KunjunganRi001,
        Guid KunjunganRj001,
        Guid UnitBerwenang,
        Guid Dokter,
        Guid Prc,
        Guid KulkasBesar,
        Guid KulkasKecil,
        Guid KulkasCadangan,
        Guid KulkasRusak);

    /// <summary>
    /// Satu database InMemory yang dipakai bersama seluruh service, dengan isolasi antar-test lewat
    /// nama database yang unik — pola yang sama dengan <c>ProviderRequestServiceTests</c>. Setiap
    /// service mendapat konteks baru supaya pembacaan sesudah penulisan datang dari penyimpanan.
    /// </summary>
    private sealed class Lingkungan : IAsyncDisposable
    {
        private readonly string _nama = $"blood-unit-storage-{Guid.NewGuid():N}";
        private readonly List<ApplicationDbContext> _konteks = new();

        private Lingkungan()
        {
        }

        public Dunia D { get; private set; } = null!;

        public static async Task<Lingkungan> BuatAsync()
        {
            var lingkungan = new Lingkungan();

            await using var db = lingkungan.CreateContext();
            lingkungan.D = await SemaiAsync(db);

            return lingkungan;
        }

        public ApplicationDbContext CreateContext()
            => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_nama)
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

        public BbkBloodUnitService Service(ApplicationDbContext? context = null) => new(context ?? Baru());

        public BloodStorageLocationService LocationService() => new(Baru());

        public BbkProviderRequestService RequestService()
        {
            var context = Baru();

            return new BbkProviderRequestService(
                context,
                new NumberSeriesAllocator(new ContextFactory(CreateContext)),
                new BbkEncounterStatusReader(context));
        }

        public async Task<BbkProviderRequest> BuatPermintaanAsync(Guid kunjungan, params (Guid Komponen, int Jumlah)[] baris)
        {
            var orderContext = Baru();

            var order = await new BbkBloodOrderService(
                    orderContext,
                    new NumberSeriesAllocator(new ContextFactory(CreateContext)),
                    new BbkEncounterStatusReader(orderContext))
                .CreateAsync(
                    new CreateBloodOrderRequest
                    {
                        PatientId = D.PasienA,
                        EncounterId = kunjungan,
                        ServiceUnitId = D.UnitBerwenang,
                        RequestingDoctorId = D.Dokter,
                        Lines = baris
                            .Select(x => new BloodOrderLineRequest { BloodComponentId = x.Komponen, RequestedQuantity = x.Jumlah })
                            .ToList()
                    },
                    PetugasUnit);

            Assert.True(order.Outcome == BloodOrderOutcome.Success, order.Message);

            var permintaan = await RequestService().CreateAsync(
                new CreateProviderRequestRequest { BloodOrderId = order.Entity!.Id },
                PetugasBdrs);

            Assert.True(permintaan.Outcome == ProviderRequestOutcome.Success, permintaan.Message);

            return permintaan.Entity!;
        }

        /// <summary>Mencatat penerimaan lalu memulangkan id kantong menurut urutan nomor yang diberikan.</summary>
        public async Task<List<Guid>> TerimaAsync(Guid permintaanId, params (Guid Komponen, string Nomor)[] kantong)
        {
            var hasil = await RequestService().RecordReceiptAsync(
                permintaanId,
                new RecordReceiptRequest
                {
                    Units = kantong
                        .Select(x => new ReceivedBloodUnitRequest { PmiBagNumber = x.Nomor, BloodComponentId = x.Komponen })
                        .ToList()
                },
                PetugasBdrs);

            Assert.True(hasil.Outcome == ProviderRequestOutcome.Success, hasil.Message);

            await using var baca = CreateContext();

            var nomor = kantong.Select(x => x.Nomor).ToList();
            var tersimpan = await baca.BbkBloodUnits
                .Where(x => nomor.Contains(x.PmiBagNumber))
                .Select(x => new { x.Id, x.PmiBagNumber })
                .ToListAsync();

            return nomor.Select(n => tersimpan.Single(x => x.PmiBagNumber == n).Id).ToList();
        }

        /// <summary>Kantong PRC sejumlah itu, seluruhnya <c>Received</c> dan tidak berlebih.</summary>
        public async Task<List<Guid>> SiapkanKantongAsync(int jumlah)
        {
            var permintaan = await BuatPermintaanAsync(D.KunjunganRi001, (D.Prc, jumlah));

            return await TerimaAsync(
                permintaan.Id,
                Enumerable.Range(1, jumlah).Select(i => (D.Prc, $"PMI-S-{i:000}")).ToArray());
        }

        public async Task UbahStatusKunjunganAsync(Guid kunjungan, EncounterStatus status)
        {
            await using var tulis = CreateContext();
            var encounter = await tulis.Set<TrxPatientEncounter>().SingleAsync(x => x.Id == kunjungan);
            encounter.EncounterStatus = status;
            await tulis.SaveChangesAsync();
        }

        /// <summary>Menyetel status kantong langsung, untuk keadaan yang tindakannya lahir pada task lain.</summary>
        public async Task SetelStatusAsync(Guid kantong, BbkBloodUnitStatus status)
        {
            await using var tulis = CreateContext();
            var unit = await tulis.BbkBloodUnits.SingleAsync(x => x.Id == kantong);
            unit.UnitStatus = status;
            await tulis.SaveChangesAsync();
        }

        /// <summary>Potret seluruh kolom kantong yang dapat berubah, untuk membuktikan tidak ada yang disentuh.</summary>
        public async Task<string> PotretKantongAsync()
        {
            await using var baca = CreateContext();

            var baris = await baca.BbkBloodUnits
                .OrderBy(x => x.PmiBagNumber)
                .Select(x => $"{x.Id}|{x.UnitStatus}|{x.CurrentPlacementId}|{x.Version}|{x.UpdateDateTime:O}|{x.UpdateBy}")
                .ToListAsync();

            return string.Join(";", baris);
        }

        public async Task<string> PotretPenempatanAsync()
        {
            await using var baca = CreateContext();

            var baris = await baca.BbkBloodUnitPlacements
                .OrderBy(x => x.Id)
                .Select(x => $"{x.Id}|{x.BloodUnitId}|{x.StorageLocationId}|{x.IsCurrent}|{x.PlacedAt:O}")
                .ToListAsync();

            return string.Join(";", baris);
        }

        public async Task<int> HitungRiwayatAsync()
        {
            await using var baca = CreateContext();

            return await baca.BbkTransitionHistories.CountAsync();
        }

        private ApplicationDbContext Baru()
        {
            var context = CreateContext();
            _konteks.Add(context);

            return context;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var context in _konteks)
                await context.DisposeAsync();
        }
    }

    private sealed class ContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly Func<ApplicationDbContext> _create;

        public ContextFactory(Func<ApplicationDbContext> create)
        {
            _create = create;
        }

        public ApplicationDbContext CreateDbContext() => _create();
    }

    private static async Task<Dunia> SemaiAsync(ApplicationDbContext db)
    {
        var pasien = new MstPatient
        {
            Id = Guid.NewGuid(),
            PatientCode = "PSN-" + Guid.NewGuid().ToString("N")[..8],
            MedicalRecordNumber = "RM-" + Guid.NewGuid().ToString("N")[..8],
            FullName = "Pasien A"
        };
        db.Set<MstPatient>().Add(pasien);

        var unit = new MstServiceUnit
        {
            Id = Guid.NewGuid(),
            ServiceUnitCode = "UNIT-ICU",
            ServiceUnitName = "ICU",
            IsAvailableForBloodOrder = true
        };
        db.Set<MstServiceUnit>().Add(unit);

        var ri001 = Kunjungan(db, pasien.Id, unit.Id, "RI-001", EncounterType.Inpatient, EncounterStatus.Registered);
        var rj001 = Kunjungan(db, pasien.Id, unit.Id, "RJ-001", EncounterType.Outpatient, EncounterStatus.InConsultation);

        var dokter = new MstDoctor
        {
            Id = Guid.NewGuid(),
            DoctorCode = "D-001",
            DoctorNumber = "DN-D-001",
            FullName = "dr. Peminta",
            IsActive = true
        };
        db.Set<MstDoctor>().Add(dokter);

        db.Users.Add(new ApplicationUser { Id = PetugasBdrs, UserName = "petugas.bdrs" });

        var prc = new MstBloodComponent
        {
            Id = Guid.NewGuid(),
            ComponentCode = "PRC",
            ComponentName = "Packed Red Cell",
            IsActive = true
        };
        db.Set<MstBloodComponent>().Add(prc);

        var besar = Lokasi(db, "KLK-BSR", "Kulkas Besar", aktif: true);
        var kecil = Lokasi(db, "KLK-KCL", "Kulkas Kecil", aktif: true);
        var cadangan = Lokasi(db, "KLK-CDG", "Kulkas Cadangan", aktif: true);
        var rusak = Lokasi(db, "KLK-RSK", "Kulkas Rusak", aktif: false);

        await db.SaveChangesAsync();

        return new Dunia(pasien.Id, ri001, rj001, unit.Id, dokter.Id, prc.Id, besar, kecil, cadangan, rusak);
    }

    private static Guid Kunjungan(
        ApplicationDbContext db,
        Guid pasien,
        Guid unit,
        string nomor,
        EncounterType jenis,
        EncounterStatus status)
    {
        var kunjungan = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = nomor,
            PatientId = pasien,
            ServiceUnitId = unit,
            EncounterType = jenis,
            EncounterStatus = status
        };
        db.Set<TrxPatientEncounter>().Add(kunjungan);

        return kunjungan.Id;
    }

    private static Guid Lokasi(ApplicationDbContext db, string kode, string nama, bool aktif)
    {
        var lokasi = new MstBloodStorageLocation
        {
            Id = Guid.NewGuid(),
            StorageLocationCode = kode,
            StorageLocationName = nama,
            IsActive = aktif
        };
        db.Set<MstBloodStorageLocation>().Add(lokasi);

        return lokasi.Id;
    }
}
