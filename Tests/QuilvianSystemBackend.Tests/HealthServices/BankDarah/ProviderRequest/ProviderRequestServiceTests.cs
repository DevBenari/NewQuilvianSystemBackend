using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Reflection;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.ProviderRequest;

/// <summary>
/// Bukti untuk <c>BE-BD-004</c> — permintaan PMI dibuat, penerimaan dicatat, kantong lahir
/// <c>Received</c> — blueprint <c>BD-BP-001</c>, kontrak <c>v4</c>.
/// </summary>
/// <remarks>
/// <para>
/// Yang dibuktikan di sini, sesuai acceptance criteria roadmap: <c>AC-BD-005</c>,
/// <c>AC-BD-006</c>, <c>AC-BD-009</c>, <c>AC-BD-022</c>, <c>AC-BD-023</c>, <c>AC-BD-031</c>,
/// <c>AC-BD-032</c>, dan <c>AC-BD-059</c> — beserta aturan yang menopangnya: kelebihan dihitung
/// per komponen, nomor kantong PMI unik, dan pembatalan beralasan terkendali.
/// </para>
/// <para>
/// <b>Batas yang jujur.</b> Provider InMemory tidak mengenal <c>pg_advisory_xact_lock</c>
/// maupun index unik fisik, sehingga perilaku berebut antar-koneksi dan penolakan index
/// dibuktikan terpisah terhadap PostgreSQL (<c>ProviderRequestPostgresTests</c>).
/// </para>
/// </remarks>
public partial class ProviderRequestServiceTests
{
    private static readonly Guid PetugasUnit = Guid.Parse("51515151-5151-5151-5151-515151515151");
    private static readonly Guid PetugasBdrs = Guid.Parse("52525252-5252-5252-5252-525252525252");

    private const string AlasanBatalPmi = "BTL-PMI";
    private const string AlasanNonaktif = "BTL-PMI-LAMA";
    private const string AlasanBatalOrder = "BTL-OPS";
    private const string TeksAlasanBatalPmi = "Kebutuhan dibatalkan sebelum kiriman PMI datang";

    private const string PesanVal003 = "Komponen darah harus dipilih dari katalog, tidak boleh diketik bebas.";
    private const string PesanVal006 =
        "Sudah ada permintaan darah yang masih berjalan untuk kebutuhan ini. Tidak boleh dibuat permintaan baru.";
    private const string PesanVal014 =
        "Kiriman melebihi permintaan. Kantong tetap dicatat diterima dan masuk daftar menunggu keputusan.";
    private const string PesanVal016 = "Alasan wajib dipilih dari daftar, tidak boleh diketik bebas.";

    private const string DeretPermintaan = "BBK_PROVIDER_REQUEST";

    // =====================================================================
    // 1. Pembuatan permintaan dan nomornya
    // =====================================================================

    /// <summary>
    /// Nomor permintaan datang dari provider bersama pada deret milik Bank Darah, dan pasien
    /// disalin dari order asal — bukan dari isian permintaan.
    /// </summary>
    [Fact]
    public async Task PermintaanDibuat_BernomorDariDeretBankDarah_AtasNamaPasienOrder()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 3));

        Assert.Equal("PMI-00000001", permintaan.RequestNumber);
        Assert.Equal(BbkProviderRequestStatus.Requested, permintaan.RequestStatus);
        Assert.Equal(l.D.PasienA, permintaan.PatientId);
        Assert.Equal(PetugasBdrs, permintaan.CreateBy);
        Assert.Equal(0, permintaan.Version);

        await using var baca = l.CreateContext();

        var deret = await baca.NumNumberSeries.SingleAsync(x => x.SequenceKey == DeretPermintaan);
        Assert.Equal(1L, deret.CurrentValue);

        var riwayat = await baca.BbkTransitionHistories.SingleAsync(x => x.EntityId == permintaan.Id);
        Assert.Equal(BbkTransitionScopes.ProviderRequest, riwayat.Scope);
        Assert.Equal("Create", riwayat.Action);
        Assert.Null(riwayat.FromStatus);
        Assert.Equal("Requested", riwayat.ToStatus);
        Assert.Equal(PetugasBdrs, riwayat.ActorUserId);
    }

    /// <summary>
    /// <c>AC-BD-006</c> / <c>VAL-BD-006</c> — permintaan kedua untuk kebutuhan yang sama
    /// ditolak, dan nomor tidak terbit untuk permintaan yang ditolak.
    /// </summary>
    [Fact]
    public async Task AC_BD_006_PermintaanBelumDikirim_DibuatLagiUntukKebutuhanSama_Ditolak()
    {
        await using var l = await Lingkungan.BuatAsync();

        var pertama = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 3));

        var kedua = await l.Service().CreateAsync(
            new CreateProviderRequestRequest { BloodOrderId = pertama.BloodOrderId },
            PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.DuplicateRequest, kedua.Outcome);
        Assert.Equal(PesanVal006, kedua.Message);

        await using var baca = l.CreateContext();

        Assert.Equal(1, await baca.BbkProviderRequests.CountAsync());
        Assert.Equal(1L, (await baca.NumNumberSeries.SingleAsync(x => x.SequenceKey == DeretPermintaan)).CurrentValue);
    }

    /// <summary>
    /// Permintaan yang dibatalkan tidak lagi berjalan, sehingga order yang sama boleh dimintakan
    /// lagi — penahanan <c>VAL-BD-006</c> hanya berlaku bagi permintaan yang masih berjalan.
    /// </summary>
    [Fact]
    public async Task PermintaanDibatalkan_OrderYangSamaBolehDimintakanLagi()
    {
        await using var l = await Lingkungan.BuatAsync();

        var pertama = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var batal = await l.Service().CancelAsync(pertama.Id, Batal(AlasanBatalPmi), PetugasBdrs);
        Assert.Equal(ProviderRequestOutcome.Success, batal.Outcome);

        var lagi = await l.Service().CreateAsync(
            new CreateProviderRequestRequest { BloodOrderId = pertama.BloodOrderId },
            PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Success, lagi.Outcome);
        Assert.Equal("PMI-00000002", lagi.Entity!.RequestNumber);
    }

    /// <summary>Dua order berbeda adalah dua kebutuhan berbeda, sehingga keduanya boleh dimintakan.</summary>
    [Fact]
    public async Task DuaOrderBerbeda_MasingMasingBolehPunyaPermintaan()
    {
        await using var l = await Lingkungan.BuatAsync();

        await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));
        var kedua = await l.BuatPermintaanAsync(l.D.KunjunganRi002, (l.D.Prc, 2));

        Assert.Equal("PMI-00000002", kedua.RequestNumber);
    }

    [Fact]
    public async Task OrderTidakAktif_TidakDapatDibuatkanPermintaan()
    {
        await using var l = await Lingkungan.BuatAsync();

        var orderId = await l.BuatOrderAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var batalOrder = await l.OrderService().CancelAsync(
            orderId,
            new CancelBloodOrderRequest { ReasonCode = AlasanBatalOrder },
            PetugasBdrs);
        Assert.Equal(BloodOrderOutcome.Success, batalOrder.Outcome);

        var hasil = await l.Service().CreateAsync(new CreateProviderRequestRequest { BloodOrderId = orderId }, PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Equal(0, await HitungPermintaanAsync(l));
    }

    /// <summary>
    /// Order yang kunjungannya sudah berakhir akan kedaluwarsa (<c>DEC-BD-014</c>); memesan darah
    /// untuknya berarti memesan kantong tanpa pasien untuk diberikan.
    /// </summary>
    [Fact]
    public async Task OrderPadaKunjunganYangSudahBerakhir_TidakDapatDibuatkanPermintaan()
    {
        await using var l = await Lingkungan.BuatAsync();

        var orderId = await l.BuatOrderAsync(l.D.KunjunganRj001, (l.D.Prc, 2));
        await UbahStatusKunjunganAsync(l, l.D.KunjunganRj001, EncounterStatus.Completed);

        var hasil = await l.Service().CreateAsync(new CreateProviderRequestRequest { BloodOrderId = orderId }, PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Equal(0, await HitungPermintaanAsync(l));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PermintaanTanpaPelakuAtauTanpaOrder_Ditolak(bool tanpaPelaku)
    {
        await using var l = await Lingkungan.BuatAsync();

        var orderId = await l.BuatOrderAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await l.Service().CreateAsync(
            new CreateProviderRequestRequest { BloodOrderId = tanpaPelaku ? orderId : Guid.Empty },
            tanpaPelaku ? Guid.Empty : PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Invalid, hasil.Outcome);
        Assert.Equal(0, await HitungPermintaanAsync(l));
    }

    /// <summary><c>AC-BD-009</c> — permintaan yang sudah dikirim tidak menambah stok.</summary>
    [Fact]
    public async Task AC_BD_009_PermintaanDikirim_DarahBelumDiterimaFisik_StokTidakBertambah()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 3));

        await using var baca = l.CreateContext();
        Assert.Equal(0, await baca.BbkBloodUnits.CountAsync());

        var ringkasan = await l.UnitService().GetSummaryAsync();
        Assert.Equal(0, ringkasan.TotalUnit);

        var detail = await l.Service().GetDetailAsync(permintaan.Id);
        Assert.Equal(3, detail!.TotalOutstandingQuantity);
        Assert.Equal(0, detail.TotalReceivedQuantity);
    }

    // =====================================================================
    // 2. Penerimaan dan sisa permintaan
    // =====================================================================

    /// <summary><c>AC-BD-005</c> — minta 3 PRC, diterima 2 pada hari pertama.</summary>
    [Fact]
    public async Task AC_BD_005_Minta3Prc_Diterima2HariPertama_PartiallyFulfilledSisa1()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 3));

        var hasil = await l.Service().RecordReceiptAsync(
            permintaan.Id,
            Terima((l.D.Prc, "PMI-A-001"), (l.D.Prc, "PMI-A-002")),
            PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Success, hasil.Outcome);
        Assert.Equal(0, hasil.ExcessUnitCount);
        Assert.Equal("Penerimaan kantong berhasil dicatat.", hasil.Message);

        var detail = (await l.Service().GetDetailAsync(permintaan.Id))!;

        Assert.Equal(BbkProviderRequestStatus.PartiallyFulfilled, detail.RequestStatus);
        Assert.Equal(3, detail.TotalRequestedQuantity);
        Assert.Equal(2, detail.TotalReceivedQuantity);
        Assert.Equal(0, detail.TotalExcessQuantity);
        Assert.Equal(1, detail.TotalOutstandingQuantity);
        Assert.Equal(1, detail.Version);

        var penerimaan = Assert.Single(detail.Receipts);
        Assert.Equal(1, penerimaan.Sequence);
        Assert.Equal(2, penerimaan.ReceivedQuantity);
        Assert.Equal(PetugasBdrs, penerimaan.ReceivedByUserId);
        Assert.Equal(new[] { "Receive", "Cancel" }, detail.AvailableActions);
    }

    /// <summary>
    /// <c>AC-BD-031</c> — minta 2 PRC, datang 3: <c>Fulfilled</c> dengan sisa <b>0, bukan −1</b>,
    /// dan ketiga kantong tercatat membawa rujukan permintaan asalnya.
    /// </summary>
    [Fact]
    public async Task AC_BD_031_Minta2Prc_Datang3_FulfilledSisaNol_TigaKantongTercatat()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await l.Service().RecordReceiptAsync(
            permintaan.Id,
            Terima((l.D.Prc, "PMI-B-001"), (l.D.Prc, "PMI-B-002"), (l.D.Prc, "PMI-B-003")),
            PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Success, hasil.Outcome);

        var detail = (await l.Service().GetDetailAsync(permintaan.Id))!;

        Assert.Equal(BbkProviderRequestStatus.Fulfilled, detail.RequestStatus);
        Assert.Equal(0, detail.TotalOutstandingQuantity);
        Assert.Equal(3, detail.TotalReceivedQuantity);

        await using var baca = l.CreateContext();
        var kantong = await baca.BbkBloodUnits.Where(x => x.ProviderRequestId == permintaan.Id).ToListAsync();

        Assert.Equal(3, kantong.Count);
        Assert.All(kantong, x => Assert.Equal(permintaan.Id, x.ProviderRequestId));
        Assert.All(kantong, x => Assert.Equal(BbkBloodUnitStatus.Received, x.UnitStatus));
    }

    /// <summary>
    /// <c>AC-BD-032</c> pada tingkat <c>BE-BD-004</c> — kantong ke-3 ditandai berlebih dan membawa
    /// alasan "kiriman melebihi permintaan"; balasannya peringatan <c>VAL-BD-014</c>, bukan
    /// penolakan. Perpindahannya ke <c>PendingReview</c> terjadi setelah disimpan
    /// (<c>BE-BD-015</c>), karena matriks perpindahan status menuntut kantong berlebih pun disimpan
    /// lebih dulu.
    /// </summary>
    [Fact]
    public async Task AC_BD_032_KantongKetiga_DitandaiBerlebih_DenganAlasanKirimanMelebihiPermintaan()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await l.Service().RecordReceiptAsync(
            permintaan.Id,
            Terima((l.D.Prc, "PMI-C-001"), (l.D.Prc, "PMI-C-002"), (l.D.Prc, "PMI-C-003")),
            PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Success, hasil.Outcome);
        Assert.Equal(1, hasil.ExcessUnitCount);
        Assert.Equal(PesanVal014, hasil.Message);

        await using var baca = l.CreateContext();

        var ketiga = await baca.BbkBloodUnits.SingleAsync(x => x.PmiBagNumber == "PMI-C-003");
        Assert.True(ketiga.IsExcess);
        Assert.Equal(BbkBloodUnitStatus.Received, ketiga.UnitStatus);

        Assert.False((await baca.BbkBloodUnits.SingleAsync(x => x.PmiBagNumber == "PMI-C-001")).IsExcess);
        Assert.False((await baca.BbkBloodUnits.SingleAsync(x => x.PmiBagNumber == "PMI-C-002")).IsExcess);

        var riwayat = await baca.BbkTransitionHistories.SingleAsync(x => x.EntityId == ketiga.Id);
        Assert.Equal(BbkTransitionScopes.BloodUnit, riwayat.Scope);
        Assert.Equal("Receive", riwayat.Action);
        Assert.Equal("Received", riwayat.ToStatus);
        Assert.Equal("Kiriman melebihi permintaan.", riwayat.ReasonNote);

        // Daftar kantong berlebih dapat disaring — bahan daftar kerja #2 sesudah penyimpanan.
        var berlebih = await l.UnitService().GetPagedAsync(null, null, true, null, null, null, null, 1, 25);
        Assert.Equal(1, berlebih.TotalData);
        Assert.Equal("PMI-C-003", Assert.Single(berlebih.Items).PmiBagNumber);
    }

    /// <summary>
    /// Kelebihan dihitung <b>per komponen</b>, sesuai baris order asalnya. PRC yang lebih tidak
    /// menutupi trombosit yang kurang.
    /// </summary>
    [Fact]
    public async Task KelebihanDihitungPerKomponen_PrcBerlebihTidakMenutupiTrombositYangKurang()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2), (l.D.Trombosit, 1));

        var hasil = await l.Service().RecordReceiptAsync(
            permintaan.Id,
            Terima((l.D.Prc, "PMI-D-001"), (l.D.Prc, "PMI-D-002"), (l.D.Prc, "PMI-D-003")),
            PetugasBdrs);

        Assert.Equal(1, hasil.ExcessUnitCount);

        var detail = (await l.Service().GetDetailAsync(permintaan.Id))!;

        Assert.Equal(BbkProviderRequestStatus.PartiallyFulfilled, detail.RequestStatus);
        Assert.Equal(1, detail.TotalOutstandingQuantity);

        var prc = detail.Components.Single(x => x.BloodComponentId == l.D.Prc);
        Assert.Equal((2, 2, 1, 0), (prc.RequestedQuantity, prc.ReceivedQuantity, prc.ExcessQuantity, prc.OutstandingQuantity));

        var trombosit = detail.Components.Single(x => x.BloodComponentId == l.D.Trombosit);
        Assert.Equal((1, 0, 0, 1), (trombosit.RequestedQuantity, trombosit.ReceivedQuantity, trombosit.ExcessQuantity, trombosit.OutstandingQuantity));

        var susulan = await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Trombosit, "PMI-D-004")), PetugasBdrs);

        Assert.Equal(0, susulan.ExcessUnitCount);
        Assert.Equal(BbkProviderRequestStatus.Fulfilled, (await l.Service().GetDetailAsync(permintaan.Id))!.RequestStatus);
    }

    /// <summary>
    /// Kantong komponen yang tidak diminta sama sekali tetap dicatat — penerimaan tidak pernah
    /// ditolak karena kelebihan (<c>DEC-BD-025</c>) — dan tidak mengubah sisa permintaan.
    /// </summary>
    [Fact]
    public async Task KomponenYangTidakDiminta_TetapDicatatSebagaiBerlebih_SisaTidakBerubah()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 1));

        var hasil = await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Trombosit, "PMI-E-001")), PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Success, hasil.Outcome);
        Assert.Equal(1, hasil.ExcessUnitCount);

        var detail = (await l.Service().GetDetailAsync(permintaan.Id))!;
        Assert.Equal(BbkProviderRequestStatus.Requested, detail.RequestStatus);
        Assert.Equal(1, detail.TotalOutstandingQuantity);
        Assert.Equal(1, detail.TotalExcessQuantity);
    }

    /// <summary>
    /// Penerimaan yang dicatat dalam beberapa kedatangan bernomor urut, dan kantong yang
    /// datang sesudah permintaan terpenuhi penuh tidak dapat dicatat — <c>Fulfilled</c> terminal
    /// menurut matriks perpindahan status §2.
    /// </summary>
    [Fact]
    public async Task PenerimaanBertahap_BernomorUrut_DanPermintaanTerpenuhiTidakMenerimaLagi()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-F-001")), PetugasBdrs);
        await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-F-002")), PetugasBdrs);

        var detail = (await l.Service().GetDetailAsync(permintaan.Id))!;
        Assert.Equal(new[] { 1, 2 }, detail.Receipts.Select(x => x.Sequence));
        Assert.Equal(BbkProviderRequestStatus.Fulfilled, detail.RequestStatus);
        Assert.Empty(detail.AvailableActions);

        var lagi = await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-F-003")), PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.NotAllowedByState, lagi.Outcome);

        await using var baca = l.CreateContext();
        Assert.Equal(2, await baca.BbkBloodUnits.CountAsync());
    }

    /// <summary>
    /// <c>AC-BD-059</c> — kantong baru berstatus <c>Received</c>, belum punya lokasi, dan belum
    /// dapat dialokasikan: tidak ada satu aksi pun yang ditawarkan sampai penyimpanan lahir
    /// (<c>BE-BD-015</c>).
    /// </summary>
    [Fact]
    public async Task AC_BD_059_KantongBaruDiterima_StatusReceived_BelumDapatDialokasikan()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 1));
        var waktuTerima = DateTime.UtcNow.AddHours(-2);

        await l.Service().RecordReceiptAsync(
            permintaan.Id,
            new RecordReceiptRequest
            {
                ReceivedAt = waktuTerima,
                Units = new List<ReceivedBloodUnitRequest> { new() { PmiBagNumber = "PMI-G-001", BloodComponentId = l.D.Prc } }
            },
            PetugasBdrs);

        await using var baca = l.CreateContext();
        var kantong = await baca.BbkBloodUnits.SingleAsync();

        var detail = (await l.UnitService().GetDetailAsync(kantong.Id))!;

        Assert.Equal(BbkBloodUnitStatus.Received, detail.UnitStatus);
        Assert.Equal("Diterima", detail.UnitStatusLabel);
        Assert.Empty(detail.AvailableActions);
        Assert.Equal(permintaan.Id, detail.ProviderRequestId);
        Assert.Equal(permintaan.RequestNumber, detail.RequestNumber);
        Assert.Equal(waktuTerima, detail.ReceivedAt);
        Assert.Equal(PetugasBdrs, detail.ReceivedByUserId);
        Assert.Null(detail.IssuedAt);
        Assert.False(detail.IssuedViaEmergency);

        var riwayat = Assert.Single(detail.Transitions);
        Assert.Equal("Receive", riwayat.Action);
        Assert.Equal(waktuTerima, riwayat.OccurredAt);
    }

    // =====================================================================
    // 3. Penerimaan yang ditolak
    // =====================================================================

    [Fact]
    public async Task NomorKantongPmiGanda_PadaSatuPenerimaanAtauSudahTercatat_Ditolak()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 3));

        var dalamSatu = await l.Service().RecordReceiptAsync(
            permintaan.Id,
            Terima((l.D.Prc, "PMI-H-001"), (l.D.Prc, "PMI-H-001")),
            PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.DuplicateBagNumber, dalamSatu.Outcome);

        await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-H-002")), PetugasBdrs);

        var sudahAda = await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-H-002")), PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.DuplicateBagNumber, sudahAda.Outcome);

        await using var baca = l.CreateContext();
        Assert.Equal(1, await baca.BbkBloodUnits.CountAsync());
        Assert.Equal(1, await baca.BbkBloodUnitReceipts.CountAsync());
    }

    [Theory]
    [InlineData("tanpa-pelaku")]
    [InlineData("tanpa-kantong")]
    [InlineData("nomor-kosong")]
    [InlineData("komponen-bukan-katalog")]
    [InlineData("waktu-masa-depan")]
    public async Task PenerimaanTidakSah_Ditolak_TanpaSatuKantongPunLahir(string kasus)
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var request = kasus switch
        {
            "tanpa-kantong" => new RecordReceiptRequest { Units = new List<ReceivedBloodUnitRequest>() },
            "nomor-kosong" => Terima((l.D.Prc, "   ")),
            "komponen-bukan-katalog" => Terima((Guid.NewGuid(), "PMI-I-001")),
            "waktu-masa-depan" => new RecordReceiptRequest
            {
                ReceivedAt = DateTime.UtcNow.AddHours(1),
                Units = new List<ReceivedBloodUnitRequest> { new() { PmiBagNumber = "PMI-I-002", BloodComponentId = l.D.Prc } }
            },
            _ => Terima((l.D.Prc, "PMI-I-003"))
        };

        var hasil = await l.Service().RecordReceiptAsync(
            permintaan.Id,
            request,
            kasus == "tanpa-pelaku" ? Guid.Empty : PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Invalid, hasil.Outcome);

        if (kasus == "komponen-bukan-katalog")
            Assert.Equal(PesanVal003, hasil.Message);

        await using var baca = l.CreateContext();
        Assert.Equal(0, await baca.BbkBloodUnits.CountAsync());
        Assert.Equal(BbkProviderRequestStatus.Requested, (await baca.BbkProviderRequests.SingleAsync()).RequestStatus);
    }

    [Fact]
    public async Task PenerimaanPadaPermintaanYangTidakAda_Menjadi404()
    {
        await using var l = await Lingkungan.BuatAsync();

        var hasil = await l.Service().RecordReceiptAsync(Guid.NewGuid(), Terima((l.D.Prc, "PMI-J-001")), PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.NotFound, hasil.Outcome);
    }

    // =====================================================================
    // 4. Penutupan karena kunjungan berakhir — AC-BD-022, AC-BD-023
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-022</c> — sisa 1 kantong saat kunjungan berakhir: permintaan menjadi
    /// <c>ClosedEncounter</c> dan riwayatnya tetap utuh.
    /// </summary>
    [Fact]
    public async Task AC_BD_022_Sisa1KantongSaatKunjunganBerakhir_ClosedEncounter_RiwayatUtuh()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRj001, (l.D.Prc, 3));
        await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-K-001"), (l.D.Prc, "PMI-K-002")), PetugasBdrs);

        await UbahStatusKunjunganAsync(l, l.D.KunjunganRj001, EncounterStatus.Completed);

        var hasil = await l.Service().CloseForEncounterEndAsync(permintaan.Id, PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Success, hasil.Outcome);

        var detail = (await l.Service().GetDetailAsync(permintaan.Id))!;

        Assert.Equal(BbkProviderRequestStatus.ClosedEncounter, detail.RequestStatus);
        Assert.Equal(1, detail.TotalOutstandingQuantity);
        Assert.Equal(2, detail.TotalReceivedQuantity);
        Assert.Single(detail.Receipts);
        Assert.NotNull(detail.ClosedEncounterAt);
        Assert.Equal(new[] { "Create", "Receive", "CloseEncounter" }, detail.Transitions.Select(x => x.Action));
        Assert.Equal(new[] { "Receive" }, detail.AvailableActions);
    }

    /// <summary>
    /// <c>AC-BD-023</c> pada tingkat <c>BE-BD-004</c> — kantong yang tetap datang sesudah
    /// <c>ClosedEncounter</c> dicatat, membawa rujukan permintaan asal, dan permintaannya tidak
    /// kembali aktif. Perpindahan kantong itu ke <c>PendingReview</c> terjadi setelah disimpan
    /// (<c>BE-BD-015</c>), dengan status permintaan asal sebagai sebabnya.
    /// </summary>
    [Fact]
    public async Task AC_BD_023_KantongDatangSetelahClosedEncounter_TetapDicatat_PermintaanTidakAktifLagi()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRj001, (l.D.Prc, 3));
        await UbahStatusKunjunganAsync(l, l.D.KunjunganRj001, EncounterStatus.Completed);
        await l.Service().CloseForEncounterEndAsync(permintaan.Id, PetugasBdrs);

        var hasil = await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-L-001")), PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Success, hasil.Outcome);

        await using var baca = l.CreateContext();

        var kantong = await baca.BbkBloodUnits.SingleAsync();
        Assert.Equal(permintaan.Id, kantong.ProviderRequestId);
        Assert.Equal(BbkBloodUnitStatus.Received, kantong.UnitStatus);

        var tersimpan = await baca.BbkProviderRequests.SingleAsync(x => x.Id == permintaan.Id);
        Assert.Equal(BbkProviderRequestStatus.ClosedEncounter, tersimpan.RequestStatus);
    }

    [Fact]
    public async Task PenutupanSaatKunjunganMasihBerjalan_Ditolak()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRj001, (l.D.Prc, 1));

        var hasil = await l.Service().CloseForEncounterEndAsync(permintaan.Id, PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Equal(BbkProviderRequestStatus.Requested, (await l.Service().GetDetailAsync(permintaan.Id))!.RequestStatus);
    }

    // =====================================================================
    // 5. Pembatalan permintaan
    // =====================================================================

    [Fact]
    public async Task PembatalanDenganAlasanTerkendali_Berhasil_TeksAlasanDisalinKeRiwayat()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await l.Service().CancelAsync(permintaan.Id, Batal(" btl-pmi "), PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Success, hasil.Outcome);
        Assert.Equal(BbkProviderRequestStatus.Cancelled, hasil.Entity!.RequestStatus);

        await using var baca = l.CreateContext();

        var riwayat = await baca.BbkTransitionHistories.SingleAsync(x => x.EntityId == permintaan.Id && x.Action == "Cancel");
        Assert.Equal("Requested", riwayat.FromStatus);
        Assert.Equal("Cancelled", riwayat.ToStatus);
        Assert.Equal(AlasanBatalPmi, riwayat.ReasonCode);
        Assert.Equal(TeksAlasanBatalPmi, riwayat.ReasonNote);
        Assert.Equal(PetugasBdrs, riwayat.ActorUserId);
    }

    /// <summary><c>VAL-BD-016</c> — kosong, ketikan bebas, dan alasan nonaktif ditolak.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("Batal saja")]
    [InlineData(AlasanNonaktif)]
    public async Task PembatalanTanpaAlasanTerkendali_Ditolak(string kode)
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));

        var hasil = await l.Service().CancelAsync(permintaan.Id, Batal(kode), PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.Invalid, hasil.Outcome);
        Assert.Equal(PesanVal016, hasil.Message);
        Assert.Equal(BbkProviderRequestStatus.Requested, (await l.Service().GetDetailAsync(permintaan.Id))!.RequestStatus);
    }

    [Fact]
    public async Task PembatalanDariLayarUsang_Ditolak409()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 3));
        await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-M-001")), PetugasBdrs);

        var request = Batal(AlasanBatalPmi);
        request.Version = 0;

        var hasil = await l.Service().CancelAsync(permintaan.Id, request, PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.VersionConflict, hasil.Outcome);
    }

    [Fact]
    public async Task PermintaanTerpenuhi_TidakDapatDibatalkan()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 1));
        await l.Service().RecordReceiptAsync(permintaan.Id, Terima((l.D.Prc, "PMI-N-001")), PetugasBdrs);

        var hasil = await l.Service().CancelAsync(permintaan.Id, Batal(AlasanBatalPmi), PetugasBdrs);

        Assert.Equal(ProviderRequestOutcome.NotAllowedByState, hasil.Outcome);
    }

    // =====================================================================
    // 6. Daftar, ringkasan, dan kolom tersimpan
    // =====================================================================

    [Fact]
    public async Task DaftarDanRingkasan_MenghitungSisaTanpaPernahNegatif()
    {
        await using var l = await Lingkungan.BuatAsync();

        var permintaan = await l.BuatPermintaanAsync(l.D.KunjunganRi001, (l.D.Prc, 2));
        await l.Service().RecordReceiptAsync(
            permintaan.Id,
            Terima((l.D.Prc, "PMI-O-001"), (l.D.Prc, "PMI-O-002"), (l.D.Prc, "PMI-O-003")),
            PetugasBdrs);

        var daftar = await l.Service().GetPagedAsync("PMI-0000", null, null, null, null, null, 1, 25);

        var baris = Assert.Single(daftar.Items);
        Assert.Equal((2, 3, 1, 0), (baris.TotalRequestedQuantity, baris.TotalReceivedQuantity, baris.TotalExcessQuantity, baris.TotalOutstandingQuantity));
        Assert.Equal("Terpenuhi", baris.RequestStatusLabel);

        var ringkasan = await l.Service().GetSummaryAsync();
        Assert.Equal(1, ringkasan.FulfilledRequest);
        Assert.Equal(1, ringkasan.RequestWithExcess);

        var ringkasanKantong = await l.UnitService().GetSummaryAsync();
        Assert.Equal((3, 3, 1), (ringkasanKantong.TotalUnit, ringkasanKantong.ReceivedUnit, ringkasanKantong.ExcessUnit));
    }

    [Fact]
    public async Task RiwayatStatus_PermintaanTidakAda_Null()
    {
        await using var l = await Lingkungan.BuatAsync();

        Assert.Null(await l.Service().GetStatusHistoryAsync(Guid.NewGuid()));
        Assert.Null(await l.UnitService().GetStatusHistoryAsync(Guid.NewGuid()));
        Assert.Null(await l.Service().GetDetailAsync(Guid.NewGuid()));
        Assert.Null(await l.UnitService().GetDetailAsync(Guid.NewGuid()));
    }

    /// <summary>
    /// Kolom tersimpan ketiga entity sama persis dengan kamus data kontrak <c>v4</c> — dikurangi
    /// <c>CurrentPlacementId</c> (<c>BE-BD-015</c>) dan <c>CompatibilityEvidenceIdUsed</c>
    /// (<c>BE-BD-007</c>) — ditambah kolom audit <c>IdentityModel</c>. Penjaga terhadap field
    /// yang dikarang, termasuk kolom jumlah atau sisa pada permintaan (<c>QBE-ENT-003</c>).
    /// </summary>
    [Theory]
    [InlineData(typeof(BbkProviderRequest), "Id,RequestNumber,BloodOrderId,PatientId,RequestStatus,Version")]
    [InlineData(typeof(BbkBloodUnitReceipt), "Id,ProviderRequestId,ReceivedQuantity,ReceivedAt,ReceivedByUserId,Sequence")]
    [InlineData(typeof(BbkBloodUnit), "Id,PmiBagNumber,ProviderRequestId,ReceiptId,BloodComponentId,IsExcess,UnitStatus,IssuedToPatientId,IssuedAt,IssuedByUserId,IssuedViaEmergency,Version")]
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

    private static RecordReceiptRequest Terima(params (Guid Komponen, string Nomor)[] kantong) => new()
    {
        Units = kantong
            .Select(x => new ReceivedBloodUnitRequest { PmiBagNumber = x.Nomor, BloodComponentId = x.Komponen })
            .ToList()
    };

    private static CancelProviderRequestRequest Batal(string kode) => new() { ReasonCode = kode };

    private static bool IsScalar(Type type)
    {
        var dasar = Nullable.GetUnderlyingType(type) ?? type;

        return dasar.IsPrimitive || dasar.IsEnum || dasar == typeof(string) ||
               dasar == typeof(Guid) || dasar == typeof(DateTime) || dasar == typeof(decimal);
    }

    private static async Task<int> HitungPermintaanAsync(Lingkungan l)
    {
        await using var baca = l.CreateContext();

        return await baca.BbkProviderRequests.CountAsync();
    }

    private static async Task UbahStatusKunjunganAsync(Lingkungan l, Guid kunjungan, EncounterStatus status)
    {
        await using var tulis = l.CreateContext();
        var encounter = await tulis.Set<TrxPatientEncounter>().SingleAsync(x => x.Id == kunjungan);
        encounter.EncounterStatus = status;
        await tulis.SaveChangesAsync();
    }

    private sealed record Dunia(
        Guid PasienA,
        Guid KunjunganRi001,
        Guid KunjunganRi002,
        Guid KunjunganRj001,
        Guid UnitBerwenang,
        Guid Dokter,
        Guid Prc,
        Guid Trombosit);

    /// <summary>
    /// Satu database InMemory yang dipakai bersama service <b>dan</b> alokator nomor, dengan
    /// isolasi antar-test lewat <b>nama</b> database yang unik — pola yang sama dengan
    /// <c>BloodOrderServiceTests</c>. Setiap service mendapat konteks baru supaya pembacaan
    /// sesudah penulisan benar-benar datang dari penyimpanan, bukan dari cache pelacak.
    /// </summary>
    private sealed class Lingkungan : IAsyncDisposable
    {
        private readonly string _nama = $"provider-request-{Guid.NewGuid():N}";
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

        public BbkProviderRequestService Service()
        {
            var context = Baru();

            return new BbkProviderRequestService(
                context,
                new NumberSeriesAllocator(new ContextFactory(CreateContext)),
                new BbkEncounterStatusReader(context));
        }

        public BbkBloodOrderService OrderService()
        {
            var context = Baru();

            return new BbkBloodOrderService(
                context,
                new NumberSeriesAllocator(new ContextFactory(CreateContext)),
                new BbkEncounterStatusReader(context));
        }

        public BbkBloodUnitService UnitService() => new(Baru());

        public async Task<Guid> BuatOrderAsync(Guid kunjungan, params (Guid Komponen, int Jumlah)[] baris)
        {
            var hasil = await OrderService().CreateAsync(
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

            Assert.True(hasil.Outcome == BloodOrderOutcome.Success, hasil.Message);

            return hasil.Entity!.Id;
        }

        public async Task<BbkProviderRequest> BuatPermintaanAsync(Guid kunjungan, params (Guid Komponen, int Jumlah)[] baris)
        {
            var orderId = await BuatOrderAsync(kunjungan, baris);

            var hasil = await Service().CreateAsync(new CreateProviderRequestRequest { BloodOrderId = orderId }, PetugasBdrs);

            Assert.True(hasil.Outcome == ProviderRequestOutcome.Success, hasil.Message);

            return hasil.Entity!;
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
        var ri002 = Kunjungan(db, pasien.Id, unit.Id, "RI-002", EncounterType.Inpatient, EncounterStatus.Registered);
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

        var prc = Komponen(db, "PRC", "Packed Red Cell");
        var trombosit = Komponen(db, "TC", "Trombosit Konsentrat");

        Alasan(db, AlasanBatalPmi, TeksAlasanBatalPmi, BloodBankReasonCategories.OrderCancellationOperational, aktif: true);
        Alasan(db, AlasanNonaktif, "Alasan lama yang sudah dicabut", BloodBankReasonCategories.OrderCancellationOperational, aktif: false);
        Alasan(db, AlasanBatalOrder, "Order ganda karena kekeliruan input", BloodBankReasonCategories.OrderCancellationOperational, aktif: true);

        await db.SaveChangesAsync();

        return new Dunia(pasien.Id, ri001, ri002, rj001, unit.Id, dokter.Id, prc, trombosit);
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

    private static Guid Komponen(ApplicationDbContext db, string kode, string nama)
    {
        var komponen = new MstBloodComponent
        {
            Id = Guid.NewGuid(),
            ComponentCode = kode,
            ComponentName = nama,
            IsActive = true
        };
        db.Set<MstBloodComponent>().Add(komponen);

        return komponen.Id;
    }

    private static void Alasan(ApplicationDbContext db, string kode, string teks, string kategori, bool aktif)
        => db.Set<MstBloodBankReason>().Add(new MstBloodBankReason
        {
            Id = Guid.NewGuid(),
            ReasonCode = kode,
            ReasonText = teks,
            ReasonCategory = kategori,
            IsActive = aktif
        });
}
