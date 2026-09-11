using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Reflection;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.BloodOrder;

/// <summary>
/// Bukti untuk <c>BE-BD-003</c> — order darah dibuat, ganda tertahan, dibatalkan dua peran —
/// blueprint <c>BD-BP-001</c>, kontrak <c>v4</c>.
/// </summary>
/// <remarks>
/// <para>
/// Yang dibuktikan di sini, sesuai acceptance criteria roadmap:
/// </para>
/// <list type="bullet">
/// <item><c>AC-BD-001</c> — order PRC kedua pada pasien dan kunjungan yang sama tertahan.</item>
/// <item><c>AC-BD-002</c> — order trombosit tidak tertahan oleh order PRC yang aktif.</item>
/// <item><c>AC-BD-003</c> — kunjungan berbeda tidak saling menahan.</item>
/// <item><c>AC-BD-004</c> — order pada kunjungan rawat jalan yang selesai berhenti menahan.</item>
/// <item><c>AC-BD-010</c> — order manual tanpa rujukan wajib ditolak.</item>
/// <item><c>AC-BD-011</c> — setiap order menyimpan pelaku input.</item>
/// <item><c>AC-BD-013</c> — unit tanpa kewenangan memesan darah ditolak.</item>
/// <item><c>AC-BD-017</c> — rawat inap berakhir saat pasien benar-benar pulang, bukan saat
/// episode ditutup administratif.</item>
/// <item><c>AC-BD-095</c> / <c>AC-BD-096</c> / <c>AC-BD-097</c> — pembatalan dua peran dan
/// larangan pembatalan tanpa alasan terkendali.</item>
/// </list>
/// <para>
/// <b>Batas yang jujur.</b> Provider InMemory tidak mengenal <c>pg_advisory_xact_lock</c>
/// maupun index unik fisik, sehingga perilaku berebut antar-koneksi dan penolakan index
/// dibuktikan terpisah terhadap PostgreSQL. Yang dibuktikan di sini adalah aturan bisnisnya,
/// alokasi nomor lewat <c>NumberSeriesAllocator</c> yang asli, dan concurrency token
/// <c>Version</c> — yang memang ditegakkan provider InMemory.
/// </para>
/// <para>
/// <b>Kewenangan tidak diuji di sini.</b> Pemisahan <c>BloodOrder : Cancel</c> dari
/// <c>BloodOrder : Update</c> ditegakkan <c>AccessPermissionFilter</c> lewat butir hak akses,
/// dan buktinya ada pada <c>BloodBankRoleAccessContractTests</c>. Yang diuji di sini adalah
/// kewenangan yang melekat pada <b>data</b>: dokter peminta order ini, atau bukan.
/// </para>
/// </remarks>
public partial class BloodOrderServiceTests
{
    private static readonly Guid PetugasUnit = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PetugasBdrs = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PetugasBdrsLain = Guid.Parse("23232323-2323-2323-2323-232323232323");
    private static readonly Guid AkunDokterPeminta = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid AkunDokterLain = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private const string AlasanKlinis = "BTL-KLINIS";
    private const string AlasanOperasional = "BTL-OPERASIONAL";
    private const string AlasanDarurat = "DARURAT-01";
    private const string AlasanNonaktif = "BTL-LAMA";

    private const string TeksAlasanKlinis = "Kebutuhan klinis pasien berubah";
    private const string TeksAlasanOperasional = "Order ganda karena kekeliruan input";

    private const string PesanVal001 =
        "Sudah ada order darah aktif untuk pasien dan komponen ini pada kunjungan yang sama. " +
        "Lanjutkan hanya dengan alasan tertulis.";
    private const string PesanVal002 = "Jumlah kantong yang diminta harus lebih dari nol.";
    private const string PesanVal003 = "Komponen darah harus dipilih dari katalog, tidak boleh diketik bebas.";
    private const string PesanVal004 =
        "Order yang sudah kedaluwarsa tidak dapat dibuka kembali. Buat order baru pada kunjungan yang berjalan.";
    private const string PesanVal010 =
        "Order manual wajib mengisi pasien, kunjungan, dokter peminta, unit asal, dan petugas yang menginput.";
    private const string PesanVal013 = "Unit pelayanan ini belum diberi kewenangan memesan darah.";
    private const string PesanVal016 = "Alasan wajib dipilih dari daftar, tidak boleh diketik bebas.";
    private const string PesanVal083 =
        "Pilih alasan pembatalan yang sesuai: pembatalan klinis oleh dokter peminta, " +
        "atau pembatalan operasional oleh petugas Bank Darah.";

    private const string DeretOrder = "BBK_BLOOD_ORDER";

    // =====================================================================
    // 1. Pembuatan order elektronik dan nomornya
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-011</c> — order yang tersimpan menyimpan siapa yang membuatnya, dan nomornya
    /// datang dari provider bersama.
    /// </summary>
    [Fact]
    public async Task AC_BD_011_OrderElektronik_BernomorDariProvider_DanMenyimpanPelaku()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().CreateAsync(
            Order(d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, (d.Prc, 2), (d.Trombosit, 4)),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.Success, hasil.Outcome);

        var order = hasil.Entity!;
        Assert.Equal("ORD-00000001", order.OrderNumber);
        Assert.Equal(BbkOrderSource.Electronic, order.OrderSource);
        Assert.Equal(BbkBloodOrderStatus.Active, order.OrderStatus);
        Assert.Equal(PetugasUnit, order.InputByUserId);
        Assert.Equal(PetugasUnit, order.CreateBy);
        Assert.Equal(0, order.Version);

        var baris = order.Lines.OrderBy(x => x.Sequence).ToList();
        Assert.Equal(new[] { 1, 2 }, baris.Select(x => x.Sequence));
        Assert.Equal(new[] { 2, 4 }, baris.Select(x => x.RequestedQuantity));

        await using var baca = l.CreateContext();

        var riwayat = await baca.Set<BbkTransitionHistory>().SingleAsync(x => x.EntityId == order.Id);
        Assert.Equal(BbkTransitionScopes.BloodOrder, riwayat.Scope);
        Assert.Equal("Create", riwayat.Action);
        Assert.Null(riwayat.FromStatus);
        Assert.Equal("Active", riwayat.ToStatus);
        Assert.Equal(PetugasUnit, riwayat.ActorUserId);
    }

    /// <summary>
    /// Nomor berurutan dan tidak pernah sama, dan pencacahnya hidup pada deret milik Bank Darah
    /// di tabel provider — bukan dihitung dari jumlah order (<c>QBE-CODE-003</c>).
    /// </summary>
    [Fact]
    public async Task NomorOrder_DariDeretBankDarah_BerurutanDanTidakPernahSama()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var satu = await service.CreateAsync(OrderPrc(d), PetugasUnit);
        var dua = await service.CreateAsync(
            Order(d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, (d.Trombosit, 4)),
            PetugasUnit);

        Assert.Equal("ORD-00000001", satu.Entity!.OrderNumber);
        Assert.Equal("ORD-00000002", dua.Entity!.OrderNumber);

        await using var baca = l.CreateContext();
        var deret = await baca.NumNumberSeries.SingleAsync(x => x.SequenceKey == DeretOrder);

        Assert.Equal("GLOBAL", deret.ScopeKey);
        Assert.Equal(2L, deret.CurrentValue);
    }

    /// <summary>
    /// Seluruh penolakan terjadi <b>sebelum</b> nomor diminta, sehingga permintaan yang memang
    /// tidak sah tidak melubangi deret (<c>INV-PLT-002</c>).
    /// </summary>
    [Fact]
    public async Task PenolakanValidasi_TerjadiSebelumNomorDiminta_DeretTidakBerlubang()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var ditolak = await service.CreateAsync(
            Order(d.PasienA, d.KunjunganRi001, d.UnitTakBerwenang, d.DokterPeminta, (d.Prc, 2)),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.UnitNotAuthorized, ditolak.Outcome);

        await using (var baca = l.CreateContext())
            Assert.False(await baca.NumNumberSeries.AnyAsync(x => x.SequenceKey == DeretOrder));

        var sah = await service.CreateAsync(OrderPrc(d), PetugasUnit);

        Assert.Equal("ORD-00000001", sah.Entity!.OrderNumber);
    }

    // =====================================================================
    // 2. Order manual — AC-BD-010, AC-BD-011
    // =====================================================================

    [Fact]
    public async Task OrderManual_Dibuat_BersumberManual_DanMenyimpanPetugasPenginput()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().CreateManualAsync(
            Isi(new CreateManualBloodOrderRequest(), d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, new[] { (d.Prc, 2) }),
            PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.Success, hasil.Outcome);
        Assert.Equal(BbkOrderSource.Manual, hasil.Entity!.OrderSource);
        Assert.Equal(PetugasBdrs, hasil.Entity.InputByUserId);
        Assert.Equal(BbkBloodOrderStatus.Active, hasil.Entity.OrderStatus);
    }

    /// <summary><c>AC-BD-010</c> / <c>VAL-BD-010</c>.</summary>
    [Theory]
    [InlineData("pasien")]
    [InlineData("kunjungan")]
    [InlineData("dokter")]
    [InlineData("unit")]
    public async Task AC_BD_010_OrderManualTanpaRujukanWajib_Ditolak(string yangKosong)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var request = Isi(
            new CreateManualBloodOrderRequest(),
            yangKosong == "pasien" ? Guid.Empty : d.PasienA,
            yangKosong == "kunjungan" ? Guid.Empty : d.KunjunganRi001,
            yangKosong == "unit" ? Guid.Empty : d.UnitBerwenang,
            yangKosong == "dokter" ? Guid.Empty : d.DokterPeminta,
            new[] { (d.Prc, 2) });

        var hasil = await l.Service().CreateManualAsync(request, PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.Invalid, hasil.Outcome);
        Assert.Equal(PesanVal010, hasil.Message);
        Assert.Equal(0, await HitungOrderAsync(l));
    }

    /// <summary>
    /// <c>AC-BD-011</c> — tanpa pelaku yang dikenali, tidak ada order yang tersimpan. Pelaku
    /// tidak pernah diterima dari isian permintaan; ia diturunkan dari pengguna terautentikasi.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AC_BD_011_PelakuTidakDikenali_OrderTidakTersimpan(bool manual)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var hasil = manual
            ? await service.CreateManualAsync(
                Isi(new CreateManualBloodOrderRequest(), d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, new[] { (d.Prc, 2) }),
                Guid.Empty)
            : await service.CreateAsync(OrderPrc(d), Guid.Empty);

        Assert.Equal(BloodOrderOutcome.Invalid, hasil.Outcome);
        Assert.Equal(0, await HitungOrderAsync(l));
    }

    /// <summary>
    /// Kolom tersimpan ketiga entity sama persis dengan kamus data kontrak <c>v4</c>, ditambah
    /// kolom audit bawaan <c>IdentityModel</c> (<c>QBE-ENT-001</c>). Penjaga terhadap field
    /// yang dikarang.
    /// </summary>
    [Theory]
    [InlineData(typeof(BbkBloodOrder), "Id,OrderNumber,PatientId,EncounterId,ServiceUnitId,RequestingDoctorId,OrderSource,InputByUserId,OrderStatus,Version")]
    [InlineData(typeof(BbkBloodOrderLine), "Id,BloodOrderId,BloodComponentId,RequestedQuantity,Sequence")]
    [InlineData(typeof(BbkTransitionHistory), "Id,Scope,EntityId,Action,FromStatus,ToStatus,ReasonCode,ReasonNote,ActorUserId,OccurredAt,CorrelationId")]
    public void KolomTersimpan_SamaPersisDenganKamusData(Type entity, string kolom)
    {
        var dideklarasikan = entity
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => IsScalar(x.PropertyType))
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(kolom.Split(',').OrderBy(x => x), dideklarasikan);
        Assert.True(typeof(IdentityModel).IsAssignableFrom(entity));
    }

    [Fact]
    public void PermintaanPembuatan_TidakMenerimaPelakuInputDariPemanggil()
    {
        var properti = typeof(CreateBloodOrderRequest)
            .GetProperties()
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(
            new[] { "EncounterId", "Lines", "PatientId", "RequestingDoctorId", "ServiceUnitId" },
            properti);
    }

    // =====================================================================
    // 3. Kewenangan unit — AC-BD-013
    // =====================================================================

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AC_BD_013_UnitTanpaKewenanganMemesanDarah_Ditolak(bool manual)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var hasil = manual
            ? await service.CreateManualAsync(
                Isi(new CreateManualBloodOrderRequest(), d.PasienA, d.KunjunganRi001, d.UnitTakBerwenang, d.DokterPeminta, new[] { (d.Prc, 2) }),
                PetugasBdrs)
            : await service.CreateAsync(
                Order(d.PasienA, d.KunjunganRi001, d.UnitTakBerwenang, d.DokterPeminta, (d.Prc, 2)),
                PetugasUnit);

        Assert.Equal(BloodOrderOutcome.UnitNotAuthorized, hasil.Outcome);
        Assert.Equal(PesanVal013, hasil.Message);
        Assert.Equal(0, await HitungOrderAsync(l));
    }

    // =====================================================================
    // 4. Validasi baris dan rujukan
    // =====================================================================

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task VAL_BD_002_JumlahTidakLebihDariNol_Ditolak(int jumlah)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().CreateAsync(
            Order(d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, (d.Prc, jumlah)),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.Invalid, hasil.Outcome);
        Assert.Equal(PesanVal002, hasil.Message);
    }

    [Theory]
    [InlineData("tidak-ada")]
    [InlineData("nonaktif")]
    [InlineData("kosong")]
    public async Task VAL_BD_003_KomponenDiLuarKatalogAktif_Ditolak(string jenis)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var komponen = jenis switch
        {
            "tidak-ada" => Guid.NewGuid(),
            "nonaktif" => d.KomponenNonaktif,
            _ => Guid.Empty
        };

        var hasil = await l.Service().CreateAsync(
            Order(d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, (komponen, 2)),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.Invalid, hasil.Outcome);
        Assert.Equal(PesanVal003, hasil.Message);
    }

    [Fact]
    public async Task KunjunganMilikPasienLain_Ditolak()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().CreateAsync(
            Order(d.PasienA, d.KunjunganPasienB, d.UnitBerwenang, d.DokterPeminta, (d.Prc, 2)),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.Invalid, hasil.Outcome);
        Assert.Equal(0, await HitungOrderAsync(l));
    }

    /// <summary>
    /// Kunjungan yang sudah berakhir tidak menerima order baru: order yang lahir di sana akan
    /// langsung berakhir menurut <c>DEC-BD-006</c>, dan pasien yang masih butuh darah dibuatkan
    /// order pada kunjungan yang berjalan (<c>ASM-BD-002</c>, pesan <c>VAL-BD-004</c>).
    /// </summary>
    [Theory]
    [InlineData(EncounterStatus.Completed)]
    [InlineData(EncounterStatus.Cancelled)]
    [InlineData(EncounterStatus.NoShow)]
    public async Task KunjunganYangSudahBerakhir_TidakMenerimaOrderBaru(EncounterStatus statusAkhir)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        await UbahStatusKunjunganAsync(l, d.KunjunganRj001, statusAkhir);

        var hasil = await l.Service().CreateAsync(
            Order(d.PasienA, d.KunjunganRj001, d.UnitBerwenang, d.DokterPeminta, (d.Prc, 2)),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Equal(0, await HitungOrderAsync(l));
    }

    // =====================================================================
    // 5. Deteksi order ganda — AC-BD-001/002/003, ASM-BD-001
    // =====================================================================

    [Fact]
    public async Task AC_BD_001_OrderPrcKeduaPadaPasienDanKunjunganSama_Tertahan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        await service.CreateAsync(OrderPrc(d), PetugasUnit);

        var kedua = await service.CreateAsync(OrderPrc(d), PetugasUnit);

        Assert.Equal(BloodOrderOutcome.DuplicateOrder, kedua.Outcome);
        Assert.Equal(PesanVal001, kedua.Message);
        Assert.Equal(new[] { d.Prc }, kedua.DuplicateComponentIds);
        Assert.Null(kedua.Entity);
        Assert.Equal(1, await HitungOrderAsync(l));

        // Order yang tertahan tidak menghabiskan nomor.
        await using var baca = l.CreateContext();
        var deret = await baca.NumNumberSeries.SingleAsync(x => x.SequenceKey == DeretOrder);
        Assert.Equal(1L, deret.CurrentValue);
    }

    /// <summary>
    /// Order yang memuat komponen campuran tertahan bila salah satu komponennya bentrok, dan
    /// balasannya menyebut komponen mana yang bentrok — bukan seluruh isi order.
    /// </summary>
    [Fact]
    public async Task OrderCampuran_TertahanPadaKomponenYangBentrokSaja()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        await service.CreateAsync(OrderPrc(d), PetugasUnit);

        var campuran = await service.CreateAsync(
            Order(d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, (d.Prc, 1), (d.Trombosit, 4)),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.DuplicateOrder, campuran.Outcome);
        Assert.Equal(new[] { d.Prc }, campuran.DuplicateComponentIds);
    }

    [Fact]
    public async Task AC_BD_002_OrderTrombositSaatPrcAktif_BolehDibuat()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        await service.CreateAsync(OrderPrc(d), PetugasUnit);

        var trombosit = await service.CreateAsync(
            Order(d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, (d.Trombosit, 4)),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.Success, trombosit.Outcome);
        Assert.Equal(2, await HitungOrderAsync(l));
    }

    [Fact]
    public async Task AC_BD_003_KunjunganBerbeda_BolehDibuat()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        await service.CreateAsync(OrderPrc(d), PetugasUnit);

        var kunjunganLain = await service.CreateAsync(OrderPrc(d, d.KunjunganRi002), PetugasUnit);

        Assert.Equal(BloodOrderOutcome.Success, kunjunganLain.Outcome);
        Assert.Equal(2, await HitungOrderAsync(l));
    }

    [Fact]
    public async Task OrderYangSudahDibatalkan_TidakLagiMenahan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var pertama = await service.CreateAsync(OrderPrc(d), PetugasUnit);
        await service.CancelAsync(pertama.Entity!.Id, Batal(AlasanOperasional), PetugasBdrs);

        var baru = await service.CreateAsync(OrderPrc(d), PetugasUnit);

        Assert.Equal(BloodOrderOutcome.Success, baru.Outcome);
    }

    /// <summary>
    /// <c>ASM-BD-001</c> — melanjutkan order ganda menuntut alasan tertulis yang tersimpan
    /// permanen beserta pelakunya. Penahanannya tidak dimatikan; ia dilewati dengan jejak.
    /// </summary>
    [Fact]
    public async Task ASM_BD_001_OrderGandaDilanjutkanDenganAlasanTertulis_TersimpanBersamaPelaku()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        await service.CreateAsync(OrderPrc(d), PetugasUnit);

        var lanjutan = await service.ConfirmDuplicateAsync(
            Isi(
                new ConfirmDuplicateOrderRequest
                {
                    DuplicateOverrideReason = "  Perdarahan ulang, kebutuhan PRC bertambah  "
                },
                d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, new[] { (d.Prc, 2) }),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.Success, lanjutan.Outcome);

        var order = lanjutan.Entity!;
        Assert.Equal("ORD-00000002", order.OrderNumber);

        // Alasan dan pelakunya terbaca pada detail, diturunkan dari riwayat — bukan kolom order.
        var detail = (await service.GetDetailAsync(order.Id))!;
        Assert.Equal("Perdarahan ulang, kebutuhan PRC bertambah", detail.DuplicateOverrideReason);
        Assert.Equal(PetugasUnit, detail.DuplicateOverrideByUserId);
        Assert.NotNull(detail.DuplicateOverrideAt);

        await using var baca = l.CreateContext();
        var riwayat = await baca.Set<BbkTransitionHistory>().SingleAsync(x => x.EntityId == order.Id);
        Assert.Equal("CreateConfirmedDuplicate", riwayat.Action);
        Assert.Equal("Perdarahan ulang, kebutuhan PRC bertambah", riwayat.ReasonNote);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ASM_BD_001_LanjutanTanpaAlasanTertulis_Ditolak(string alasan)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        await service.CreateAsync(OrderPrc(d), PetugasUnit);

        var lanjutan = await service.ConfirmDuplicateAsync(
            Isi(new ConfirmDuplicateOrderRequest { DuplicateOverrideReason = alasan },
                d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, new[] { (d.Prc, 2) }),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.Invalid, lanjutan.Outcome);
        Assert.Equal(1, await HitungOrderAsync(l));
    }

    /// <summary>
    /// Alasan tertulis melewati penahanan ganda, <b>bukan</b> kewenangan unit. Tanpa penjaga
    /// ini, endpoint lanjutan menjadi pintu belakang bagi unit yang tidak berwenang.
    /// </summary>
    [Fact]
    public async Task LanjutanGanda_TetapMenjagaKewenanganUnit()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().ConfirmDuplicateAsync(
            Isi(new ConfirmDuplicateOrderRequest { DuplicateOverrideReason = "Alasan apa pun" },
                d.PasienA, d.KunjunganRi001, d.UnitTakBerwenang, d.DokterPeminta, new[] { (d.Prc, 2) }),
            PetugasUnit);

        Assert.Equal(BloodOrderOutcome.UnitNotAuthorized, hasil.Outcome);
        Assert.Equal(0, await HitungOrderAsync(l));
    }

    // =====================================================================
    // 6. Kunjungan berakhir — AC-BD-004, AC-BD-017
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-004</c> — kunjungan rawat jalan mencapai <c>Completed</c> sementara order PRC
    /// belum terpenuhi. Order itu berakhir, tidak lagi dihitung aktif, dan tidak menahan order
    /// PRC baru pasien yang sama pada kunjungan yang berjalan.
    /// </summary>
    [Fact]
    public async Task AC_BD_004_KunjunganRawatJalanSelesai_OrderBerhentiMenahan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(OrderPrc(d, d.KunjunganRj001), PetugasUnit)).Entity!;

        await UbahStatusKunjunganAsync(l, d.KunjunganRj001, EncounterStatus.Completed);

        var keadaan = await l.Reader().ReadAsync(d.KunjunganRj001);
        Assert.True(keadaan.IsClosed);
        Assert.Equal(EncounterClosureSignal.EncounterStatusClosed, keadaan.Signal);

        var kedaluwarsa = await service.ExpireAsync(order.Id, PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.Success, kedaluwarsa.Outcome);
        Assert.Equal(BbkBloodOrderStatus.Expired, kedaluwarsa.Entity!.OrderStatus);

        var ringkasan = await service.GetSummaryAsync();
        Assert.Equal(0, ringkasan.ActiveOrder);
        Assert.Equal(1, ringkasan.ExpiredOrder);

        var baru = await service.CreateAsync(OrderPrc(d, d.KunjunganRi001), PetugasUnit);
        Assert.Equal(BloodOrderOutcome.Success, baru.Outcome);
    }

    /// <summary>
    /// <c>AC-BD-017</c> — keputusan pulang Senin pagi, pasien benar-benar pulang Senin siang,
    /// episode baru ditutup Rabu. Order tidak aktif sejak <b>Senin siang</b>, bukan Rabu.
    /// </summary>
    [Fact]
    public async Task AC_BD_017_RawatInap_BerakhirSaatPasienPulangFisik_BukanSaatEpisodeDitutup()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var seninPagi = new DateTime(2026, 9, 7, 1, 0, 0, DateTimeKind.Utc);
        var seninSiang = new DateTime(2026, 9, 7, 5, 0, 0, DateTimeKind.Utc);
        var rabu = new DateTime(2026, 9, 9, 3, 0, 0, DateTimeKind.Utc);

        var episode = await TambahEpisodeAsync(l, d.KunjunganRi001, d.PasienA, d.UnitBerwenang);
        var order = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        // Senin pagi: keputusan pulang saja belum mengakhiri order.
        await UbahEpisodeAsync(l, episode, x => x.DischargeDecidedAt = seninPagi);

        var belum = await service.ExpireAsync(order.Id, PetugasBdrs);
        Assert.Equal(BloodOrderOutcome.NotAllowedByState, belum.Outcome);

        // Senin siang: pasien benar-benar pulang; episode belum ditutup.
        await UbahEpisodeAsync(l, episode, x => x.PhysicallyLeftAt = seninSiang);

        var keadaan = await l.Reader().ReadAsync(d.KunjunganRi001);
        Assert.True(keadaan.IsClosed);
        Assert.Equal(EncounterClosureSignal.InpatientPhysicallyLeft, keadaan.Signal);
        Assert.Equal((DateTime?)seninSiang, keadaan.ClosedAt);

        // Sejak Senin siang, kunjungan ini tidak lagi menerima order baru.
        var orderBaru = await service.CreateAsync(
            Order(d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, (d.Trombosit, 2)),
            PetugasUnit);
        Assert.Equal(BloodOrderOutcome.NotAllowedByState, orderBaru.Outcome);

        // Rabu: episode ditutup administratif. Waktu berakhir order tetap Senin siang.
        await UbahEpisodeAsync(l, episode, x =>
        {
            x.ClosedAt = rabu;
            x.EpisodeStatus = InpEpisodeStatus.Closed;
        });

        var kedaluwarsa = await service.ExpireAsync(order.Id, PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.Success, kedaluwarsa.Outcome);
        Assert.Equal(BbkBloodOrderStatus.Expired, kedaluwarsa.Entity!.OrderStatus);

        await using var baca = l.CreateContext();
        var riwayat = await baca.Set<BbkTransitionHistory>()
            .SingleAsync(x => x.EntityId == order.Id && x.Action == "Expire");
        Assert.Equal(seninSiang, riwayat.OccurredAt);

        var detail = (await service.GetDetailAsync(order.Id))!;
        Assert.Equal((DateTime?)seninSiang, detail.ExpiredAt);
    }

    /// <summary><c>VAL-BD-004</c> / <c>ASM-BD-002</c> — order kedaluwarsa tidak dibuka kembali.</summary>
    [Fact]
    public async Task VAL_BD_004_OrderKedaluwarsa_TidakDapatDibatalkan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(OrderPrc(d, d.KunjunganRj001), PetugasUnit)).Entity!;
        await UbahStatusKunjunganAsync(l, d.KunjunganRj001, EncounterStatus.Completed);
        await service.ExpireAsync(order.Id, PetugasBdrs);

        var batal = await service.CancelAsync(order.Id, Batal(AlasanOperasional), PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.NotAllowedByState, batal.Outcome);
        Assert.Equal(PesanVal004, batal.Message);
    }

    // =====================================================================
    // 7. Pembatalan dua peran — AC-BD-095/096/097, VAL-BD-083
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-095</c> — dokter peminta membatalkan ordernya karena kebutuhan klinis berubah.
    /// Alasan terkendali, pelaku, waktu, dan riwayat tersimpan (<c>INV-BD-035</c>).
    /// </summary>
    [Fact]
    public async Task AC_BD_095_DokterPemintaMembatalkanDenganAlasanKlinis_BerhasilDanTerekam()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        var batal = await service.CancelAsync(order.Id, Batal(" btl-klinis "), AkunDokterPeminta);

        Assert.Equal(BloodOrderOutcome.Success, batal.Outcome);
        Assert.Equal(BbkBloodOrderStatus.Cancelled, batal.Entity!.OrderStatus);
        Assert.Equal(1, batal.Entity.Version);

        await using var baca = l.CreateContext();
        var riwayat = await baca.Set<BbkTransitionHistory>()
            .SingleAsync(x => x.EntityId == order.Id && x.Action == "Cancel");

        Assert.Equal("Active", riwayat.FromStatus);
        Assert.Equal("Cancelled", riwayat.ToStatus);
        Assert.Equal(AlasanKlinis, riwayat.ReasonCode);
        Assert.Equal(TeksAlasanKlinis, riwayat.ReasonNote);
        Assert.Equal(AkunDokterPeminta, riwayat.ActorUserId);
        Assert.NotEqual(default(DateTime), riwayat.OccurredAt);
    }

    /// <summary>
    /// <c>AC-BD-096</c> — petugas BDRS membatalkan order ganda dengan alasan operasional, dan
    /// kategori alasannya membedakannya dari pembatalan klinis pada rekam.
    /// </summary>
    [Fact]
    public async Task AC_BD_096_PetugasBdrsMembatalkanOrderGandaDenganAlasanOperasional_Berhasil()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        await service.CreateAsync(OrderPrc(d), PetugasUnit);
        var ganda = (await service.ConfirmDuplicateAsync(
            Isi(new ConfirmDuplicateOrderRequest { DuplicateOverrideReason = "Terkirim dua kali" },
                d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, new[] { (d.Prc, 2) }),
            PetugasUnit)).Entity!;

        var batal = await service.CancelAsync(ganda.Id, Batal(AlasanOperasional), PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.Success, batal.Outcome);

        await using var baca = l.CreateContext();
        var riwayat = await baca.Set<BbkTransitionHistory>()
            .SingleAsync(x => x.EntityId == ganda.Id && x.Action == "Cancel");
        var kategori = await baca.Set<MstBloodBankReason>()
            .Where(x => x.ReasonCode == riwayat.ReasonCode)
            .Select(x => x.ReasonCategory)
            .SingleAsync();

        Assert.Equal(AlasanOperasional, riwayat.ReasonCode);
        Assert.Equal(TeksAlasanOperasional, riwayat.ReasonNote);
        Assert.Equal(PetugasBdrs, riwayat.ActorUserId);
        Assert.Equal(BloodBankReasonCategories.OrderCancellationOperational, kategori);
    }

    /// <summary>
    /// <c>AC-BD-097</c> / <c>VAL-BD-016</c> — tidak ada pembatalan order tanpa alasan terkendali.
    /// Kosong, diketik bebas, maupun alasan yang sudah dinonaktifkan sama-sama ditolak, dan
    /// ordernya tetap aktif tanpa satu baris riwayat pembatalan pun.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ALASAN-KETIKAN-BEBAS")]
    [InlineData(AlasanNonaktif)]
    public async Task AC_BD_097_PembatalanTanpaAlasanTerkendali_Ditolak(string kode)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        var batal = await service.CancelAsync(order.Id, Batal(kode), PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.Invalid, batal.Outcome);
        Assert.Equal(PesanVal016, batal.Message);

        await using var baca = l.CreateContext();
        Assert.Equal(
            BbkBloodOrderStatus.Active,
            (await baca.BbkBloodOrders.SingleAsync(x => x.Id == order.Id)).OrderStatus);
        Assert.False(await baca.Set<BbkTransitionHistory>()
            .AnyAsync(x => x.EntityId == order.Id && x.Action == "Cancel"));
    }

    /// <summary>
    /// <c>VAL-BD-083</c>, kedua arahnya: alasan klinis dipakai selain dokter peminta, alasan
    /// operasional dipakai dokter peminta, dan alasan berkategori lain.
    /// </summary>
    [Theory]
    [InlineData("bdrs", AlasanKlinis)]
    [InlineData("dokter-lain", AlasanKlinis)]
    [InlineData("dokter-peminta", AlasanOperasional)]
    [InlineData("bdrs", AlasanDarurat)]
    [InlineData("dokter-peminta", AlasanDarurat)]
    public async Task VAL_BD_083_KategoriAlasanTidakSesuaiPelaku_Ditolak(string pelaku, string kode)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        var aktor = pelaku switch
        {
            "bdrs" => PetugasBdrs,
            "dokter-lain" => AkunDokterLain,
            _ => AkunDokterPeminta
        };

        var batal = await service.CancelAsync(order.Id, Batal(kode), aktor);

        Assert.Equal(BloodOrderOutcome.NotAllowedByState, batal.Outcome);
        Assert.Equal(PesanVal083, batal.Message);

        await using var baca = l.CreateContext();
        Assert.Equal(
            BbkBloodOrderStatus.Active,
            (await baca.BbkBloodOrders.SingleAsync(x => x.Id == order.Id)).OrderStatus);
    }

    [Fact]
    public async Task PembatalanDenganTokenVersiUsang_Ditolak409()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        var batal = await service.CancelAsync(
            order.Id,
            new CancelBloodOrderRequest { ReasonCode = AlasanOperasional, Version = 7 },
            PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.VersionConflict, batal.Outcome);
    }

    /// <summary>
    /// Dua petugas membatalkan order yang sama pada saat bersamaan. Petugas kedua sudah membaca
    /// order sebelum petugas pertama menyimpan, sehingga pemeriksaan statusnya lolos — yang
    /// menolaknya adalah concurrency token <c>Version</c>.
    /// </summary>
    /// <remarks>
    /// <b>Batas provider yang jujur.</b> InMemory tidak punya transaksi: dalam satu
    /// <c>SaveChanges</c>, baris riwayat yang ditambahkan ikut tersimpan lebih dulu sebelum
    /// <c>UPDATE</c> order ditolak token, dan tidak dibatalkan. Karena itu uji ini hanya
    /// menegaskan yang memang ditegakkan InMemory — petugas kedua ditolak dan order tercatat
    /// dibatalkan satu kali. Kepastian <b>tepat satu baris riwayat pembatalan</b> dibuktikan
    /// PostgreSQL, yang membungkus <c>SaveChanges</c> dalam satu transaksi:
    /// <c>BloodOrderPostgresTests.PembatalanBersamaanDuaPetugas_HanyaSatuYangTercatat</c>.
    /// </remarks>
    [Fact]
    public async Task PembatalanBersamaanDuaPetugas_YangKeduaDitolakTokenVersi()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var order = (await l.Service().CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        await using var konteksKedua = l.CreateContext();
        var bacaanBasi = await konteksKedua.BbkBloodOrders
            .Include(x => x.Lines)
            .SingleAsync(x => x.Id == order.Id);
        Assert.Equal(BbkBloodOrderStatus.Active, bacaanBasi.OrderStatus);

        var pertama = await l.Service().CancelAsync(order.Id, Batal(AlasanOperasional), PetugasBdrs);
        var kedua = await l.Service(konteksKedua).CancelAsync(order.Id, Batal(AlasanOperasional), PetugasBdrsLain);

        Assert.Equal(BloodOrderOutcome.Success, pertama.Outcome);
        Assert.Equal(BloodOrderOutcome.VersionConflict, kedua.Outcome);

        await using var baca = l.CreateContext();
        var tersimpan = await baca.BbkBloodOrders.SingleAsync(x => x.Id == order.Id);

        Assert.Equal(BbkBloodOrderStatus.Cancelled, tersimpan.OrderStatus);
        Assert.Equal(1, tersimpan.Version);
    }

    [Fact]
    public async Task OrderYangSudahDibatalkan_TidakDapatDibatalkanLagi()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;
        await service.CancelAsync(order.Id, Batal(AlasanOperasional), PetugasBdrs);

        var lagi = await service.CancelAsync(order.Id, Batal(AlasanOperasional), PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.NotAllowedByState, lagi.Outcome);
    }

    [Fact]
    public async Task PembatalanOrderYangTidakAda_NotFound()
    {
        await using var l = await Lingkungan.BuatAsync();

        var batal = await l.Service().CancelAsync(Guid.NewGuid(), Batal(AlasanOperasional), PetugasBdrs);

        Assert.Equal(BloodOrderOutcome.NotFound, batal.Outcome);
    }

    [Fact]
    public void PermintaanPembatalan_TidakPunyaRuasTeksAlasanBebas()
    {
        var properti = typeof(CancelBloodOrderRequest)
            .GetProperties()
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(new[] { "ReasonCode", "Version" }, properti);
    }

    // =====================================================================
    // 8. Pembacaan, pemenuhan, dan riwayat
    // =====================================================================

    [Fact]
    public async Task Detail_MemuatRujukanBarisPemenuhanRiwayatDanAksi()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(
            Order(d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, (d.Prc, 2), (d.Trombosit, 4)),
            PetugasUnit)).Entity!;

        var detail = await service.GetDetailAsync(order.Id);

        Assert.NotNull(detail);
        Assert.Equal("Pasien A", detail!.PatientName);
        Assert.Equal("RI-001", detail.EncounterNumber);
        Assert.Equal("ICU", detail.ServiceUnitName);
        Assert.Equal("dr. Peminta", detail.RequestingDoctorName);
        Assert.Equal("Aktif", detail.OrderStatusLabel);
        Assert.Equal("Elektronik", detail.OrderSourceLabel);
        Assert.Equal(new[] { "PRC", "TC" }, detail.Lines.Select(x => x.BloodComponentCode));
        Assert.Equal(new[] { "Cancel" }, detail.AvailableActions);

        Assert.NotNull(detail.Fulfillment);
        Assert.Equal(6, detail.Fulfillment!.TotalRequestedQuantity);
        Assert.Equal(0, detail.Fulfillment.TotalIssuedQuantity);
        Assert.Equal(6, detail.Fulfillment.TotalOutstandingQuantity);

        var riwayat = Assert.Single(detail.Transitions);
        Assert.Equal("Create", riwayat.Action);

        await service.CancelAsync(order.Id, Batal(AlasanOperasional), PetugasBdrs);

        var setelahBatal = await service.GetDetailAsync(order.Id);
        Assert.Empty(setelahBatal!.AvailableActions);
        Assert.Equal(new[] { "Create", "Cancel" }, setelahBatal.Transitions.Select(x => x.Action));
    }

    /// <summary>
    /// <c>BD-DOM-17</c> — angka pemenuhan dihitung, tidak disimpan. Tidak ada satu kolom pun
    /// pada order maupun barisnya yang dapat disunting untuk mengubah angka pemenuhan.
    /// </summary>
    [Fact]
    public async Task RingkasanPemenuhan_DihitungDariTransaksi_BukanKolomTersimpan()
    {
        var kolomPemenuhan = typeof(BbkBloodOrder).GetProperties()
            .Concat(typeof(BbkBloodOrderLine).GetProperties())
            .Where(x =>
                x.Name.Contains("Fulfill", StringComparison.OrdinalIgnoreCase) ||
                x.Name.Contains("Issued", StringComparison.OrdinalIgnoreCase) ||
                x.Name.Contains("Outstanding", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Name)
            .ToList();

        Assert.Empty(kolomPemenuhan);

        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;

        var pemenuhan = await service.GetFulfillmentAsync(order.Id);

        Assert.NotNull(pemenuhan);
        var baris = Assert.Single(pemenuhan!.Lines);
        Assert.Equal(2, baris.RequestedQuantity);
        Assert.Equal(0, baris.IssuedQuantity);
        Assert.Equal(2, baris.OutstandingQuantity);

        Assert.Null(await service.GetFulfillmentAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Daftar_MencariDanMenyaring()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var orderA = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;
        await service.CreateAsync(
            Order(d.PasienB, d.KunjunganPasienB, d.UnitBerwenang, d.DokterLain, (d.Trombosit, 3)),
            PetugasUnit);
        await service.CancelAsync(orderA.Id, Batal(AlasanOperasional), PetugasBdrs);

        var semua = await service.GetPagedAsync(null, null, null, null, null, null, null, null, 1, 25);
        Assert.Equal(2, semua.TotalData);

        var cariNomor = await service.GetPagedAsync("ord-00000001", null, null, null, null, null, null, null, 1, 25);
        Assert.Equal(orderA.Id, Assert.Single(cariNomor.Items).Id);

        var cariNama = await service.GetPagedAsync("pasien b", null, null, null, null, null, null, null, 1, 25);
        var milikB = Assert.Single(cariNama.Items);
        Assert.Equal(3, milikB.TotalRequestedQuantity);
        Assert.Equal("Aktif", milikB.OrderStatusLabel);

        var dibatalkan = await service.GetPagedAsync(
            null, null, null, null, BbkBloodOrderStatus.Cancelled, null, null, null, 1, 25);
        Assert.Equal(orderA.Id, Assert.Single(dibatalkan.Items).Id);

        var perPasien = await service.GetPagedAsync(null, d.PasienA, null, null, null, null, null, null, 1, 25);
        Assert.Equal(orderA.Id, Assert.Single(perPasien.Items).Id);
    }

    [Fact]
    public async Task Ringkasan_MenghitungPerStatusDanOrderGanda()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var pertama = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;
        await service.ConfirmDuplicateAsync(
            Isi(new ConfirmDuplicateOrderRequest { DuplicateOverrideReason = "Kebutuhan bertambah" },
                d.PasienA, d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, new[] { (d.Prc, 2) }),
            PetugasUnit);
        await service.CancelAsync(pertama.Id, Batal(AlasanOperasional), PetugasBdrs);

        var ringkasan = await service.GetSummaryAsync();

        Assert.Equal(2, ringkasan.TotalOrder);
        Assert.Equal(1, ringkasan.ActiveOrder);
        Assert.Equal(1, ringkasan.CancelledOrder);
        Assert.Equal(1, ringkasan.DuplicateOverriddenOrder);
    }

    [Fact]
    public async Task RiwayatStatus_UrutTerlamaLebihDulu_DanNullBilaOrderTidakAda()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;
        var service = l.Service();

        var order = (await service.CreateAsync(OrderPrc(d), PetugasUnit)).Entity!;
        await service.CancelAsync(order.Id, Batal(AlasanOperasional), PetugasBdrs);

        var riwayat = await service.GetStatusHistoryAsync(order.Id);

        Assert.NotNull(riwayat);
        Assert.Equal(new[] { "Create", "Cancel" }, riwayat!.Select(x => x.Action));
        Assert.Equal(new string?[] { null, "Active" }, riwayat.Select(x => x.FromStatus));
        Assert.Null(await service.GetStatusHistoryAsync(Guid.NewGuid()));
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static CreateBloodOrderRequest OrderPrc(Dunia d, Guid? kunjungan = null)
        => Order(d.PasienA, kunjungan ?? d.KunjunganRi001, d.UnitBerwenang, d.DokterPeminta, (d.Prc, 2));

    private static CreateBloodOrderRequest Order(
        Guid pasien,
        Guid kunjungan,
        Guid unit,
        Guid dokter,
        params (Guid Komponen, int Jumlah)[] baris)
        => Isi(new CreateBloodOrderRequest(), pasien, kunjungan, unit, dokter, baris);

    private static T Isi<T>(
        T request,
        Guid pasien,
        Guid kunjungan,
        Guid unit,
        Guid dokter,
        (Guid Komponen, int Jumlah)[] baris)
        where T : CreateBloodOrderRequest
    {
        request.PatientId = pasien;
        request.EncounterId = kunjungan;
        request.ServiceUnitId = unit;
        request.RequestingDoctorId = dokter;
        request.Lines = baris
            .Select(x => new BloodOrderLineRequest { BloodComponentId = x.Komponen, RequestedQuantity = x.Jumlah })
            .ToList();

        return request;
    }

    private static CancelBloodOrderRequest Batal(string kode) => new() { ReasonCode = kode };

    private static bool IsScalar(Type type)
    {
        var dasar = Nullable.GetUnderlyingType(type) ?? type;

        return dasar.IsPrimitive || dasar.IsEnum || dasar == typeof(string) ||
               dasar == typeof(Guid) || dasar == typeof(DateTime) || dasar == typeof(decimal);
    }

    private static async Task<int> HitungOrderAsync(Lingkungan l)
    {
        await using var baca = l.CreateContext();

        return await baca.BbkBloodOrders.CountAsync();
    }

    private static async Task UbahStatusKunjunganAsync(Lingkungan l, Guid kunjungan, EncounterStatus status)
    {
        await using var tulis = l.CreateContext();
        var encounter = await tulis.Set<TrxPatientEncounter>().SingleAsync(x => x.Id == kunjungan);
        encounter.EncounterStatus = status;
        await tulis.SaveChangesAsync();
    }

    private static async Task<Guid> TambahEpisodeAsync(Lingkungan l, Guid kunjungan, Guid pasien, Guid unit)
    {
        await using var tulis = l.CreateContext();
        var episode = new InpEpisode
        {
            Id = Guid.NewGuid(),
            EpisodeNumber = "EP-" + Guid.NewGuid().ToString("N")[..8],
            EncounterId = kunjungan,
            PatientId = pasien,
            ServiceUnitId = unit,
            PatientClassId = Guid.NewGuid(),
            EpisodeStatus = InpEpisodeStatus.Admitted,
            IsActive = true
        };
        tulis.Set<InpEpisode>().Add(episode);
        await tulis.SaveChangesAsync();

        return episode.Id;
    }

    private static async Task UbahEpisodeAsync(Lingkungan l, Guid episodeId, Action<InpEpisode> ubah)
    {
        await using var tulis = l.CreateContext();
        var episode = await tulis.Set<InpEpisode>().SingleAsync(x => x.Id == episodeId);
        ubah(episode);
        await tulis.SaveChangesAsync();
    }

    private sealed record Dunia(
        Guid PasienA,
        Guid PasienB,
        Guid KunjunganRi001,
        Guid KunjunganRi002,
        Guid KunjunganRj001,
        Guid KunjunganPasienB,
        Guid UnitBerwenang,
        Guid UnitTakBerwenang,
        Guid DokterPeminta,
        Guid DokterLain,
        Guid Prc,
        Guid Trombosit,
        Guid KomponenNonaktif);

    /// <summary>
    /// Satu database InMemory yang dipakai bersama service <b>dan</b> alokator nomor. Alokator
    /// sengaja membuka konteksnya sendiri lewat factory — persis seperti di aplikasi — sehingga
    /// keduanya wajib menunjuk penyimpanan yang sama.
    /// </summary>
    /// <remarks>
    /// Isolasi antar-test datang dari <b>nama</b> database yang unik, bukan dari
    /// <c>InMemoryDatabaseRoot</c> tersendiri. Root baru per test membuat EF membangun service
    /// provider internal baru setiap kali, dan setelah dua puluh provider EF menolak dengan
    /// <c>ManyServiceProvidersCreatedWarning</c>. Nama yang sama pada provider yang sama tetap
    /// berbagi satu penyimpanan — itulah yang dibutuhkan service dan alokator.
    /// </remarks>
    private sealed class Lingkungan : IAsyncDisposable
    {
        private readonly string _nama = $"blood-order-{Guid.NewGuid():N}";

        private Lingkungan()
        {
            Db = CreateContext();
        }

        public ApplicationDbContext Db { get; }

        public Dunia D { get; private set; } = null!;

        public static async Task<Lingkungan> BuatAsync()
        {
            var lingkungan = new Lingkungan();
            lingkungan.D = await SemaiAsync(lingkungan.Db);

            return lingkungan;
        }

        public ApplicationDbContext CreateContext()
            => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_nama)
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

        public BbkBloodOrderService Service(ApplicationDbContext? db = null)
        {
            var context = db ?? Db;

            return new BbkBloodOrderService(
                context,
                new NumberSeriesAllocator(new ContextFactory(CreateContext)),
                new BbkEncounterStatusReader(context));
        }

        public BbkEncounterStatusReader Reader() => new(CreateContext());

        public ValueTask DisposeAsync() => Db.DisposeAsync();
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
        var pasienA = Pasien(db, "Pasien A");
        var pasienB = Pasien(db, "Pasien B");

        var unitBerwenang = Unit(db, "UNIT-ICU", "ICU", bolehMemesanDarah: true);
        var unitTakBerwenang = Unit(db, "UNIT-FIS", "Fisioterapi", bolehMemesanDarah: false);

        var ri001 = Kunjungan(db, pasienA, unitBerwenang, "RI-001", EncounterType.Inpatient, EncounterStatus.Registered);
        var ri002 = Kunjungan(db, pasienA, unitBerwenang, "RI-002", EncounterType.Inpatient, EncounterStatus.Registered);
        var rj001 = Kunjungan(db, pasienA, unitBerwenang, "RJ-001", EncounterType.Outpatient, EncounterStatus.InConsultation);
        var kunjunganB = Kunjungan(db, pasienB, unitBerwenang, "RJ-900", EncounterType.Outpatient, EncounterStatus.InConsultation);

        var dokterPeminta = Dokter(db, "D-001", "dr. Peminta");
        var dokterLain = Dokter(db, "D-002", "dr. Lain");

        db.Users.Add(new ApplicationUser { Id = AkunDokterPeminta, UserName = "dokter.peminta", DoctorId = dokterPeminta });
        db.Users.Add(new ApplicationUser { Id = AkunDokterLain, UserName = "dokter.lain", DoctorId = dokterLain });
        db.Users.Add(new ApplicationUser { Id = PetugasBdrs, UserName = "petugas.bdrs" });

        var prc = Komponen(db, "PRC", "Packed Red Cell", aktif: true);
        var trombosit = Komponen(db, "TC", "Trombosit Konsentrat", aktif: true);
        var nonaktif = Komponen(db, "WB", "Whole Blood", aktif: false);

        Alasan(db, AlasanKlinis, TeksAlasanKlinis, BloodBankReasonCategories.OrderCancellationClinical, aktif: true);
        Alasan(db, AlasanOperasional, TeksAlasanOperasional, BloodBankReasonCategories.OrderCancellationOperational, aktif: true);
        Alasan(db, AlasanDarurat, "Kondisi darurat", BloodBankReasonCategories.Emergency, aktif: true);
        Alasan(db, AlasanNonaktif, "Alasan lama yang sudah dicabut", BloodBankReasonCategories.OrderCancellationOperational, aktif: false);

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        return new Dunia(
            pasienA, pasienB,
            ri001, ri002, rj001, kunjunganB,
            unitBerwenang, unitTakBerwenang,
            dokterPeminta, dokterLain,
            prc, trombosit, nonaktif);
    }

    private static Guid Pasien(ApplicationDbContext db, string nama)
    {
        var pasien = new MstPatient
        {
            Id = Guid.NewGuid(),
            PatientCode = "PSN-" + Guid.NewGuid().ToString("N")[..8],
            MedicalRecordNumber = "RM-" + Guid.NewGuid().ToString("N")[..8],
            FullName = nama
        };
        db.Set<MstPatient>().Add(pasien);

        return pasien.Id;
    }

    private static Guid Unit(ApplicationDbContext db, string kode, string nama, bool bolehMemesanDarah)
    {
        var unit = new MstServiceUnit
        {
            Id = Guid.NewGuid(),
            ServiceUnitCode = kode,
            ServiceUnitName = nama,
            IsAvailableForBloodOrder = bolehMemesanDarah
        };
        db.Set<MstServiceUnit>().Add(unit);

        return unit.Id;
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

    private static Guid Dokter(ApplicationDbContext db, string kode, string nama)
    {
        var dokter = new MstDoctor
        {
            Id = Guid.NewGuid(),
            DoctorCode = kode,
            DoctorNumber = "DN-" + kode,
            FullName = nama,
            IsActive = true
        };
        db.Set<MstDoctor>().Add(dokter);

        return dokter.Id;
    }

    private static Guid Komponen(ApplicationDbContext db, string kode, string nama, bool aktif)
    {
        var komponen = new MstBloodComponent
        {
            Id = Guid.NewGuid(),
            ComponentCode = kode,
            ComponentName = nama,
            IsActive = aktif
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
