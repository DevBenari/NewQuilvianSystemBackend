using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers;
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

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.BloodBankProcedure;

/// <summary>
/// Bukti untuk <c>BE-BD-012</c> — tindakan Bank Darah dicatat tanpa penyaluran biaya — blueprint
/// <c>BD-BP-001</c>, roadmap revisi 8, <c>DEC-BD-048</c> dan <c>DEC-BD-049</c>.
/// </summary>
/// <remarks>
/// Kelima kriteria <c>AC-BD-098</c> sampai <c>AC-BD-102</c>, beserta setiap cabang resolusi tarif:
/// kelas pasien, cadangan tarif umum, tarif kelas lain, tarif nonaktif, kedaluwarsa, belum berlaku,
/// terikat unit lain, dan tanpa kandidat.
/// </remarks>
public partial class BloodBankProcedureServiceTests
{
    private static readonly Guid PetugasUnit = Guid.Parse("61616161-6161-6161-6161-616161616161");
    private static readonly Guid PetugasBdrs = Guid.Parse("62626262-6262-6262-6262-626262626262");
    private static readonly Guid PetugasBdrsLain = Guid.Parse("63636363-6363-6363-6363-636363636363");

    private const string PesanVal026 = "Tindakan Bank Darah wajib menunjuk satu order yang sah.";
    private const string PesanVal084 =
        "Tarif tindakan ini belum diatur untuk kelas pasien dan unit kunjungan ini. Hubungi bagian data induk tarif.";

    private const string DeretTindakan = "BBK_PROCEDURE";

    // =====================================================================
    // 1. AC-BD-098 — pencatatan tindakan
    // =====================================================================

    [Fact]
    public async Task AC_BD_098_TindakanDicatat_BernomorDariProvider_KonteksDariKunjungan_StatusRecorded()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs);

        Assert.Equal(BloodBankProcedureOutcome.Success, hasil.Outcome);

        var tindakan = hasil.Entity!;
        Assert.Equal("TND-00000001", tindakan.ProcedureNumber);
        Assert.Equal(d.OrderVip, tindakan.BloodOrderId);
        Assert.Equal(d.UjiSilang, tindakan.ProcedureRefId);
        Assert.Equal(d.DokterBdrs, tindakan.BdrsDoctorId);
        Assert.Equal(PetugasBdrs, tindakan.PerformedByUserId);
        Assert.Equal(d.UnitIcu, tindakan.ServiceUnitId);
        Assert.Equal(d.KelasVip, tindakan.PatientClassId);
        Assert.Equal(d.TarifSilangVip, tindakan.TariffId);
        Assert.Equal(BbkProcedureStatus.Recorded, tindakan.ProcedureStatus);
        Assert.Equal(PetugasBdrs, tindakan.CreateBy);

        await using var baca = l.CreateContext();

        Assert.Equal(1L, (await baca.NumNumberSeries.SingleAsync(x => x.SequenceKey == DeretTindakan)).CurrentValue);

        var riwayat = await baca.BbkTransitionHistories.SingleAsync(x => x.EntityId == tindakan.Id);
        Assert.Equal(BbkTransitionScopes.BloodBankProcedure, riwayat.Scope);
        Assert.Equal("Create", riwayat.Action);
        Assert.Null(riwayat.FromStatus);
        Assert.Equal("Recorded", riwayat.ToStatus);
        Assert.Equal(PetugasBdrs, riwayat.ActorUserId);
    }

    [Fact]
    public async Task NomorTindakan_BerurutanDanTidakPernahSama()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var nomor = new List<string>();

        for (var i = 0; i < 3; i++)
            nomor.Add((await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!.ProcedureNumber);

        Assert.Equal(new[] { "TND-00000001", "TND-00000002", "TND-00000003" }, nomor);
    }

    /// <summary><c>VAL-BD-026</c> — order kosong, tidak ada, atau dihapus ditolak; nomor tidak terbit.</summary>
    [Theory]
    [InlineData("kosong")]
    [InlineData("acak")]
    [InlineData("dihapus")]
    public async Task AC_BD_098_OrderTidakSah_Ditolak_VAL_BD_026(string kasus)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        if (kasus == "dihapus")
        {
            await using var tulis = l.CreateContext();
            var order = await tulis.BbkBloodOrders.SingleAsync(x => x.Id == d.OrderVip);
            order.IsDelete = true;
            await tulis.SaveChangesAsync();
        }

        var orderId = kasus switch
        {
            "kosong" => Guid.Empty,
            "acak" => Guid.NewGuid(),
            _ => d.OrderVip
        };

        var hasil = await l.Service().CreateAsync(Catat(orderId, d.UjiSilang), PetugasBdrs);

        Assert.Equal(BloodBankProcedureOutcome.Invalid, hasil.Outcome);
        Assert.Equal(PesanVal026, hasil.Message);
        await AssertTanpaTindakanDanTanpaNomorAsync(l);
    }

    [Theory]
    [InlineData("kosong")]
    [InlineData("acak")]
    [InlineData("nonaktif")]
    public async Task TindakanBertarifTidakSah_Ditolak(string kasus)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var procedureId = kasus switch
        {
            "kosong" => Guid.Empty,
            "acak" => Guid.NewGuid(),
            _ => d.TindakanNonaktif
        };

        var hasil = await l.Service().CreateAsync(Catat(d.OrderVip, procedureId), PetugasBdrs);

        Assert.Equal(BloodBankProcedureOutcome.Invalid, hasil.Outcome);
        await AssertTanpaTindakanDanTanpaNomorAsync(l);
    }

    [Theory]
    [InlineData("tanpa-pelaku")]
    [InlineData("dokter-kosong")]
    [InlineData("dokter-acak")]
    public async Task PelakuAtauDokterTidakSah_Ditolak(string kasus)
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var request = Catat(d.OrderVip, d.UjiSilang);
        if (kasus == "dokter-kosong") request.BdrsDoctorId = Guid.Empty;
        if (kasus == "dokter-acak") request.BdrsDoctorId = Guid.NewGuid();

        var hasil = await l.Service().CreateAsync(request, kasus == "tanpa-pelaku" ? Guid.Empty : PetugasBdrs);

        Assert.Equal(BloodBankProcedureOutcome.Invalid, hasil.Outcome);
        await AssertTanpaTindakanDanTanpaNomorAsync(l);
    }

    // =====================================================================
    // 2. AC-BD-099 — resolusi tarif menurut tindakan dan kelas pasien
    // =====================================================================

    /// <summary>
    /// Pasien VIP mendapat tarif VIP Rp250.000 — bukan tarif umum, bukan tarif kelas 1, bukan tarif
    /// nonaktif, kedaluwarsa, belum berlaku, maupun tarif VIP milik unit lain yang juga ada.
    /// </summary>
    [Fact]
    public async Task AC_BD_099_TarifKelasPasienTerpilih_BukanKelasLainNonaktifKedaluwarsaAtauUnitLain()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var tindakan = (await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!;

        Assert.Equal(d.TarifSilangVip, tindakan.TariffId);
        Assert.Equal(250_000m, tindakan.TariffAmountSnapshot);
    }

    /// <summary>Tidak ada tarif kelas 3 → tarif umum (<c>PatientClassId</c> kosong) menjadi cadangan.</summary>
    [Fact]
    public async Task AC_BD_099_TarifUmumMenjadiCadangan_BilaTarifKelasPasienTidakAda()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var tindakan = (await l.Service().CreateAsync(Catat(d.OrderKelas3, d.UjiSilang), PetugasBdrs)).Entity!;

        Assert.Equal(d.TarifSilangUmum, tindakan.TariffId);
        Assert.Equal(150_000m, tindakan.TariffAmountSnapshot);
        Assert.Equal(d.KelasTiga, tindakan.PatientClassId);
    }

    /// <summary>
    /// Tarif kelas pasien lebih diutamakan daripada tarif umum yang terikat unit kunjungan; di antara
    /// tarif umum, yang terikat unit kunjungan lebih spesifik daripada yang berlaku di mana saja.
    /// </summary>
    [Fact]
    public async Task AC_BD_099_KelasPasienDiutamakan_LaluTarifUmumYangPalingSpesifik()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var vip = (await l.Service().CreateAsync(Catat(d.OrderVip, d.Penyiapan), PetugasBdrs)).Entity!;
        var kelas3 = (await l.Service().CreateAsync(Catat(d.OrderKelas3, d.Penyiapan), PetugasBdrs)).Entity!;

        Assert.Equal(d.TarifPenyiapanVip, vip.TariffId);
        Assert.Equal(260_000m, vip.TariffAmountSnapshot);

        Assert.Equal(d.TarifPenyiapanUmumUnitIcu, kelas3.TariffId);
        Assert.Equal(175_000m, kelas3.TariffAmountSnapshot);
    }

    /// <summary>
    /// Tindakan yang hanya punya tarif kelas 1 ditolak untuk pasien VIP — tidak pernah jatuh ke
    /// tarif milik kelas lain.
    /// </summary>
    [Fact]
    public async Task AC_BD_099_TarifMilikKelasLainTidakPernahDipakai_Ditolak422()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().CreateAsync(Catat(d.OrderVip, d.TindakanHanyaKelas1), PetugasBdrs);

        Assert.Equal(BloodBankProcedureOutcome.TariffUnavailable, hasil.Outcome);
        Assert.Equal(PesanVal084, hasil.Message);
        await AssertTanpaTindakanDanTanpaNomorAsync(l);
    }

    [Fact]
    public async Task AC_BD_099_TindakanTanpaTarifSamaSekali_Ditolak422_NomorTidakTerbit()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().CreateAsync(Catat(d.OrderVip, d.TindakanTanpaTarif), PetugasBdrs);

        Assert.Equal(BloodBankProcedureOutcome.TariffUnavailable, hasil.Outcome);
        await AssertTanpaTindakanDanTanpaNomorAsync(l);
    }

    /// <summary>
    /// Tarif yang hanya nonaktif, kedaluwarsa, atau belum berlaku tidak menjadi kandidat, walau
    /// kelasnya cocok.
    /// </summary>
    [Fact]
    public async Task AC_BD_099_HanyaTarifNonaktifKedaluwarsaAtauBelumBerlaku_Ditolak422()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().CreateAsync(Catat(d.OrderVip, d.TindakanTarifTakBerlaku), PetugasBdrs);

        Assert.Equal(BloodBankProcedureOutcome.TariffUnavailable, hasil.Outcome);
    }

    /// <summary>Kamus data mewajibkan kelas pasien; kunjungan tanpa kelas tidak ditebak kelasnya.</summary>
    [Fact]
    public async Task KunjunganTanpaKelasPasien_Ditolak422()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var hasil = await l.Service().CreateAsync(Catat(d.OrderTanpaKelas, d.UjiSilang), PetugasBdrs);

        Assert.Equal(BloodBankProcedureOutcome.NotAllowedByState, hasil.Outcome);
        await AssertTanpaTindakanDanTanpaNomorAsync(l);
    }

    /// <summary>
    /// Client tidak punya satu isian pun untuk menentukan harga, tarif, unit, maupun kelas — nominal
    /// dari client tidak dapat menjadi authority karena memang tidak dapat dikirim.
    /// </summary>
    [Fact]
    public void AC_BD_099_PermintaanPencatatan_TidakMemuatNominalTarifUnitMaupunKelas()
    {
        var isian = typeof(CreateBloodBankProcedureRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(new[] { "BdrsDoctorId", "BloodOrderId", "ProcedureRefId" }, isian);
    }

    // =====================================================================
    // 3. AC-BD-100 — salinan tarif beku
    // =====================================================================

    [Fact]
    public async Task AC_BD_100_DataIndukTindakanDanTarifBerubah_SalinanPadaTindakanLamaTidakBerubah()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var tindakan = (await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!;

        await using (var tulis = l.CreateContext())
        {
            var procedure = await tulis.MstProcedures.SingleAsync(x => x.Id == d.UjiSilang);
            procedure.ProcedureCode = "BDR-BARU";
            procedure.ProcedureName = "Nama tindakan yang sudah diganti";

            var tariff = await tulis.MstTariffs.SingleAsync(x => x.Id == d.TarifSilangVip);
            tariff.NormalPrice = 999_999m;

            await tulis.SaveChangesAsync();
        }

        var detail = (await l.Service().GetDetailAsync(tindakan.Id))!;

        Assert.Equal("BDR-SILANG", detail.ProcedureCodeSnapshot);
        Assert.Equal("Uji Silang Serasi", detail.ProcedureNameSnapshot);
        Assert.Equal(250_000m, detail.TariffAmountSnapshot);

        // Tindakan baru sesudah perubahan memakai nilai baru — salinan lama tetap.
        var baru = (await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!;
        Assert.Equal("BDR-BARU", baru.ProcedureCodeSnapshot);
        Assert.Equal(999_999m, baru.TariffAmountSnapshot);
    }

    // =====================================================================
    // 4. AC-BD-101 — penyelesaian
    // =====================================================================

    [Fact]
    public async Task AC_BD_101_RecordedMenjadiCompleted_TransisiDanAuditTersimpan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var tindakan = (await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!;

        var hasil = await l.Service().CompleteAsync(tindakan.Id, PetugasBdrsLain);

        Assert.Equal(BloodBankProcedureOutcome.Success, hasil.Outcome);

        var detail = (await l.Service().GetDetailAsync(tindakan.Id))!;

        Assert.Equal(BbkProcedureStatus.Completed, detail.ProcedureStatus);
        Assert.Equal("Selesai", detail.ProcedureStatusLabel);
        Assert.Equal(PetugasBdrsLain, detail.UpdateBy);
        Assert.NotNull(detail.UpdateDateTime);
        Assert.Equal(PetugasBdrsLain, detail.CompletedByUserId);
        Assert.NotNull(detail.CompletedAt);
        Assert.Empty(detail.AvailableActions);
        Assert.Equal(new[] { "Create", "Complete" }, detail.Transitions.Select(x => x.Action));
        Assert.Equal(new string?[] { null, "Recorded" }, detail.Transitions.Select(x => x.FromStatus));
        Assert.Equal(new[] { "Recorded", "Completed" }, detail.Transitions.Select(x => x.ToStatus));
    }

    [Fact]
    public async Task AC_BD_101_MenyelesaikanDuaKali_DitolakTerkendali_StatusDanAuditTidakBergerak()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var tindakan = (await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!;
        await l.Service().CompleteAsync(tindakan.Id, PetugasBdrs);

        var kedua = await l.Service().CompleteAsync(tindakan.Id, PetugasBdrsLain);

        Assert.Equal(BloodBankProcedureOutcome.NotAllowedByState, kedua.Outcome);

        await using var baca = l.CreateContext();
        Assert.Equal(1, await baca.BbkTransitionHistories.CountAsync(x => x.EntityId == tindakan.Id && x.Action == "Complete"));
        Assert.Equal(PetugasBdrs, (await baca.BbkBloodBankProcedures.SingleAsync(x => x.Id == tindakan.Id)).UpdateBy);
    }

    /// <summary>
    /// Dua petugas membuka tindakan yang sama lalu keduanya menyelesaikan. <c>ProcedureStatus</c>
    /// sebagai concurrency token menolak yang kedua — satu baris riwayat penyelesaian saja.
    /// </summary>
    [Fact]
    public async Task AC_BD_101_PenyelesaianBersamaanDuaPetugas_YangKeduaDitolak409()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var id = (await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!.Id;

        await using var konteksA = l.CreateContext();
        await using var konteksB = l.CreateContext();

        await konteksA.BbkBloodBankProcedures.SingleAsync(x => x.Id == id);
        await konteksB.BbkBloodBankProcedures.SingleAsync(x => x.Id == id);

        var pertama = await l.Service(konteksA).CompleteAsync(id, PetugasBdrs);
        var kedua = await l.Service(konteksB).CompleteAsync(id, PetugasBdrsLain);

        Assert.Equal(BloodBankProcedureOutcome.Success, pertama.Outcome);
        Assert.Equal(BloodBankProcedureOutcome.VersionConflict, kedua.Outcome);

        // Provider InMemory tidak bertransaksi: baris riwayat milik penyelesaian yang ditolak dapat
        // tertulis sebelum pemeriksaan token. "Tepat satu baris riwayat" karena itu dibuktikan pada
        // BloodBankProcedurePostgresTests, tempat SaveChanges berjalan dalam satu transaksi.
        await using var baca = l.CreateContext();
        var tersimpan = await baca.BbkBloodBankProcedures.SingleAsync(x => x.Id == id);
        Assert.Equal(BbkProcedureStatus.Completed, tersimpan.ProcedureStatus);
        Assert.Equal(PetugasBdrs, tersimpan.UpdateBy);
    }

    [Fact]
    public async Task AC_BD_101_TindakanTidakAdaAtauTanpaPelaku_Ditolak()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        Assert.Equal(BloodBankProcedureOutcome.NotFound, (await l.Service().CompleteAsync(Guid.NewGuid(), PetugasBdrs)).Outcome);

        var tindakan = (await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!;

        Assert.Equal(BloodBankProcedureOutcome.Invalid, (await l.Service().CompleteAsync(tindakan.Id, Guid.Empty)).Outcome);
        Assert.Equal(BbkProcedureStatus.Recorded, (await l.Service().GetDetailAsync(tindakan.Id))!.ProcedureStatus);
    }

    // =====================================================================
    // 5. AC-BD-102 — tanpa side-effect Billing
    // =====================================================================

    /// <summary>Dibuat lalu diselesaikan: nol baris pada tabel Billing yang menampung tagihan dan biaya.</summary>
    [Fact]
    public async Task AC_BD_102_DibuatDanDiselesaikan_NolBarisBilling()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var tindakan = (await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!;
        await l.Service().CompleteAsync(tindakan.Id, PetugasBdrs);

        await using var baca = l.CreateContext();

        Assert.Equal(0, await baca.BilInvoices.CountAsync());
        Assert.Equal(0, await baca.BilInvoiceItems.CountAsync());
        Assert.Equal(0, await baca.BilFolios.CountAsync());
        Assert.Equal(0, await baca.BilChargeLines.CountAsync());
        Assert.Equal(0, await baca.BilChargeComponents.CountAsync());
        Assert.Equal(0, await baca.BilProcessingEffects.CountAsync());
    }

    /// <summary>
    /// Struktural: tidak satu pun tipe Bank Darah bergantung pada Billing atau producer fakta biaya,
    /// dan entity tindakan tidak punya kolom penagihan. Tidak ada jalur penyaluran yang dapat terpanggil.
    /// </summary>
    [Fact]
    public void AC_BD_102_TidakAdaTipeBankDarahYangBergantungPadaBillingAtauProducerBiaya()
    {
        var bankDarah = typeof(BbkBloodBankProcedureService).Assembly
            .GetTypes()
            .Where(x => x.Namespace != null &&
                        x.Namespace.StartsWith("QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement", StringComparison.Ordinal))
            .ToList();

        Assert.Contains(typeof(BbkBloodBankProcedureService), bankDarah);
        Assert.Contains(typeof(BbkBloodBankProcedureController), bankDarah);

        var pelanggar = new List<string>();

        foreach (var type in bankDarah)
        {
            var dependensi = type
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .SelectMany(x => x.GetParameters().Select(p => p.ParameterType))
                .Concat(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(x => x.FieldType));

            foreach (var dep in dependensi)
            {
                var nama = dep.FullName ?? dep.Name;

                if (nama.Contains("BillingManagement", StringComparison.Ordinal) ||
                    nama.Contains("ChargeEligib", StringComparison.Ordinal) ||
                    nama.Contains("FactProducer", StringComparison.Ordinal))
                {
                    pelanggar.Add($"{type.Name} → {nama}");
                }
            }
        }

        Assert.True(pelanggar.Count == 0, "Bank Darah bergantung pada Billing: " + string.Join(", ", pelanggar));

        var kolomPenagihan = typeof(BbkBloodBankProcedure)
            .GetProperties()
            .Select(x => x.Name)
            .Where(x => x.Contains("Invoice") || x.Contains("Charge") || x.Contains("Billing") || x.Contains("Posted"))
            .ToList();

        Assert.Empty(kolomPenagihan);
    }

    // =====================================================================
    // 6. Pembacaan dan kolom tersimpan
    // =====================================================================

    [Fact]
    public async Task DaftarDanDetail_MemuatSalinanDanKonteksKunjungan()
    {
        await using var l = await Lingkungan.BuatAsync();
        var d = l.D;

        var tindakan = (await l.Service().CreateAsync(Catat(d.OrderVip, d.UjiSilang), PetugasBdrs)).Entity!;

        var daftar = await l.Service().GetPagedAsync("TND-0000", d.OrderVip, null, BbkProcedureStatus.Recorded, null, null, 1, 25);

        var baris = Assert.Single(daftar.Items);
        Assert.Equal(tindakan.ProcedureNumber, baris.ProcedureNumber);
        Assert.Equal("Uji Silang Serasi", baris.ProcedureNameSnapshot);
        Assert.Equal(250_000m, baris.TariffAmountSnapshot);
        Assert.Equal("Pasien A", baris.PatientName);
        Assert.Equal("dr. BDRS", baris.BdrsDoctorName);
        Assert.Equal("Dicatat", baris.ProcedureStatusLabel);

        var detail = (await l.Service().GetDetailAsync(tindakan.Id))!;
        Assert.Equal("ICU", detail.ServiceUnitName);
        Assert.Equal("VIP", detail.PatientClassName);
        Assert.Equal("RI-VIP", detail.EncounterNumber);
        Assert.Equal(new[] { "Complete" }, detail.AvailableActions);

        Assert.Null(await l.Service().GetDetailAsync(Guid.NewGuid()));
    }

    /// <summary>Kolom tersimpan sama persis dengan kamus data kontrak <c>v4</c> — penjaga field karangan.</summary>
    [Fact]
    public void KolomTersimpan_SamaPersisDenganKamusData()
    {
        Assert.True(typeof(IdentityModel).IsAssignableFrom(typeof(BbkBloodBankProcedure)));

        var tersimpan = typeof(BbkBloodBankProcedure)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => IsScalar(x.PropertyType))
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToList();

        var kamus = "Id,ProcedureNumber,BloodOrderId,ServiceUnitId,BdrsDoctorId,PerformedByUserId,PatientClassId," +
                    "ProcedureRefId,TariffId,ProcedureCodeSnapshot,ProcedureNameSnapshot,TariffAmountSnapshot,ProcedureStatus";

        Assert.Equal(kamus.Split(',').OrderBy(x => x), tersimpan);
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static CreateBloodBankProcedureRequest Catat(Guid orderId, Guid procedureId) => new()
    {
        BloodOrderId = orderId,
        ProcedureRefId = procedureId,
        BdrsDoctorId = DokterBdrsTetap
    };

    private static readonly Guid DokterBdrsTetap = Guid.Parse("64646464-6464-6464-6464-646464646464");

    private static bool IsScalar(Type type)
    {
        var dasar = Nullable.GetUnderlyingType(type) ?? type;

        return dasar.IsPrimitive || dasar.IsEnum || dasar == typeof(string) ||
               dasar == typeof(Guid) || dasar == typeof(DateTime) || dasar == typeof(decimal);
    }

    private static async Task AssertTanpaTindakanDanTanpaNomorAsync(Lingkungan l)
    {
        await using var baca = l.CreateContext();

        Assert.Equal(0, await baca.BbkBloodBankProcedures.CountAsync());
        Assert.False(await baca.NumNumberSeries.AnyAsync(x => x.SequenceKey == DeretTindakan));
    }

    private sealed record Dunia(
        Guid UnitIcu,
        Guid KelasVip,
        Guid KelasTiga,
        Guid DokterBdrs,
        Guid OrderVip,
        Guid OrderKelas3,
        Guid OrderTanpaKelas,
        Guid UjiSilang,
        Guid Penyiapan,
        Guid TindakanHanyaKelas1,
        Guid TindakanTanpaTarif,
        Guid TindakanTarifTakBerlaku,
        Guid TindakanNonaktif,
        Guid TarifSilangVip,
        Guid TarifSilangUmum,
        Guid TarifPenyiapanVip,
        Guid TarifPenyiapanUmumUnitIcu);

    /// <summary>
    /// Satu database InMemory yang dipakai bersama service dan alokator nomor, dengan isolasi lewat
    /// nama database yang unik — pola yang sama dengan <c>BloodOrderServiceTests</c>.
    /// </summary>
    private sealed class Lingkungan : IAsyncDisposable
    {
        private readonly string _nama = $"blood-bank-procedure-{Guid.NewGuid():N}";
        private readonly List<ApplicationDbContext> _konteks = new();

        public Dunia D { get; private set; } = null!;

        public static async Task<Lingkungan> BuatAsync()
        {
            var lingkungan = new Lingkungan();
            lingkungan.D = await SemaiAsync(lingkungan);

            return lingkungan;
        }

        public ApplicationDbContext CreateContext()
            => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_nama)
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

        public BbkBloodBankProcedureService Service(ApplicationDbContext? context = null)
            => new(context ?? Baru(), new NumberSeriesAllocator(new ContextFactory(CreateContext)));

        public BbkBloodOrderService OrderService()
        {
            var context = Baru();

            return new BbkBloodOrderService(
                context,
                new NumberSeriesAllocator(new ContextFactory(CreateContext)),
                new BbkEncounterStatusReader(context));
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

    private static async Task<Dunia> SemaiAsync(Lingkungan l)
    {
        Guid unitIcu, unitLain, kelasVip, kelasTiga, kelasSatu, encVip, encKelas3, encTanpaKelas, prc, pasien;
        Guid ujiSilang, penyiapan, hanyaKelas1, tanpaTarif, tarifTakBerlaku, nonaktif;
        Guid tarifSilangVip, tarifSilangUmum, tarifPenyiapanVip, tarifPenyiapanUmumUnitIcu;

        await using (var db = l.CreateContext())
        {
            pasien = Tambah(db, new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = "PSN-A",
                MedicalRecordNumber = "RM-A",
                FullName = "Pasien A"
            }).Id;

            unitIcu = Tambah(db, new MstServiceUnit { Id = Guid.NewGuid(), ServiceUnitCode = "ICU", ServiceUnitName = "ICU", IsAvailableForBloodOrder = true }).Id;
            unitLain = Tambah(db, new MstServiceUnit { Id = Guid.NewGuid(), ServiceUnitCode = "HCU", ServiceUnitName = "HCU", IsAvailableForBloodOrder = true }).Id;

            kelasVip = Tambah(db, new MstPatientClass { Id = Guid.NewGuid(), PatientClassCode = "VIP", PatientClassName = "VIP" }).Id;
            kelasTiga = Tambah(db, new MstPatientClass { Id = Guid.NewGuid(), PatientClassCode = "K3", PatientClassName = "Kelas 3" }).Id;
            kelasSatu = Tambah(db, new MstPatientClass { Id = Guid.NewGuid(), PatientClassCode = "K1", PatientClassName = "Kelas 1" }).Id;

            encVip = Kunjungan(db, pasien, unitIcu, "RI-VIP", kelasVip);
            encKelas3 = Kunjungan(db, pasien, unitIcu, "RI-K3", kelasTiga);
            encTanpaKelas = Kunjungan(db, pasien, unitIcu, "RI-TANPA", null);

            Tambah(db, new MstDoctor { Id = DokterBdrsTetap, DoctorCode = "D-BDRS", DoctorNumber = "DN-BDRS", FullName = "dr. BDRS", IsActive = true });
            Tambah(db, new MstDoctor { Id = Guid.NewGuid(), DoctorCode = "D-PMT", DoctorNumber = "DN-PMT", FullName = "dr. Peminta", IsActive = true });

            db.Users.Add(new ApplicationUser { Id = PetugasBdrs, UserName = "petugas.bdrs" });
            db.Users.Add(new ApplicationUser { Id = PetugasBdrsLain, UserName = "petugas.bdrs.lain" });

            prc = Tambah(db, new MstBloodComponent { Id = Guid.NewGuid(), ComponentCode = "PRC", ComponentName = "Packed Red Cell", IsActive = true }).Id;

            var kategori = Tambah(db, new MstTariffCategory { Id = Guid.NewGuid(), TariffCategoryCode = "TND", TariffCategoryName = "Tindakan", IsProcedure = true }).Id;

            ujiSilang = Tindakan(db, "BDR-SILANG", "Uji Silang Serasi", aktif: true);
            penyiapan = Tindakan(db, "BDR-SIAP", "Penyiapan Komponen", aktif: true);
            hanyaKelas1 = Tindakan(db, "BDR-K1", "Tindakan Khusus Kelas 1", aktif: true);
            tanpaTarif = Tindakan(db, "BDR-KOSONG", "Tindakan Tanpa Tarif", aktif: true);
            tarifTakBerlaku = Tindakan(db, "BDR-TAKBERLAKU", "Tindakan Tarif Tak Berlaku", aktif: true);
            nonaktif = Tindakan(db, "BDR-NONAKTIF", "Tindakan Nonaktif", aktif: false);

            var kemarin = DateTime.UtcNow.Date.AddDays(-1);
            var besok = DateTime.UtcNow.Date.AddDays(1);

            // Uji silang: tarif umum, VIP, kelas 1, dan empat tarif VIP yang tidak boleh terpilih.
            tarifSilangUmum = Tarif(db, kategori, ujiSilang, "T-SILANG-UMUM", 150_000m, kelas: null);
            tarifSilangVip = Tarif(db, kategori, ujiSilang, "T-SILANG-VIP", 250_000m, kelas: kelasVip);
            Tarif(db, kategori, ujiSilang, "T-SILANG-K1", 300_000m, kelas: kelasSatu);
            Tarif(db, kategori, ujiSilang, "T-SILANG-VIP-NONAKTIF", 999_000m, kelas: kelasVip, aktif: false);
            Tarif(db, kategori, ujiSilang, "T-SILANG-VIP-KEDALUWARSA", 888_000m, kelas: kelasVip, berakhir: kemarin);
            Tarif(db, kategori, ujiSilang, "T-SILANG-VIP-BELUM", 777_000m, kelas: kelasVip, mulai: besok);
            Tarif(db, kategori, ujiSilang, "T-SILANG-VIP-HCU", 555_000m, kelas: kelasVip, unit: unitLain);

            // Penyiapan: kelas VIP tanpa unit, umum terikat ICU, umum di mana saja.
            tarifPenyiapanVip = Tarif(db, kategori, penyiapan, "T-SIAP-VIP", 260_000m, kelas: kelasVip);
            tarifPenyiapanUmumUnitIcu = Tarif(db, kategori, penyiapan, "T-SIAP-UMUM-ICU", 175_000m, kelas: null, unit: unitIcu);
            Tarif(db, kategori, penyiapan, "T-SIAP-UMUM", 140_000m, kelas: null);

            // Hanya kelas 1.
            Tarif(db, kategori, hanyaKelas1, "T-K1-SAJA", 320_000m, kelas: kelasSatu);

            // Kelas cocok tetapi tidak satu pun berlaku.
            Tarif(db, kategori, tarifTakBerlaku, "T-TB-NONAKTIF", 100_000m, kelas: kelasVip, aktif: false);
            Tarif(db, kategori, tarifTakBerlaku, "T-TB-KEDALUWARSA", 110_000m, kelas: null, berakhir: kemarin);
            Tarif(db, kategori, tarifTakBerlaku, "T-TB-BELUM", 120_000m, kelas: kelasVip, mulai: besok);

            await db.SaveChangesAsync();
        }

        var orderVip = await BuatOrderAsync(l, pasien, encVip, unitIcu, prc);
        var orderKelas3 = await BuatOrderAsync(l, pasien, encKelas3, unitIcu, prc);
        var orderTanpaKelas = await BuatOrderAsync(l, pasien, encTanpaKelas, unitIcu, prc);

        return new Dunia(
            unitIcu, kelasVip, kelasTiga, DokterBdrsTetap,
            orderVip, orderKelas3, orderTanpaKelas,
            ujiSilang, penyiapan, hanyaKelas1, tanpaTarif, tarifTakBerlaku, nonaktif,
            tarifSilangVip, tarifSilangUmum, tarifPenyiapanVip, tarifPenyiapanUmumUnitIcu);
    }

    private static async Task<Guid> BuatOrderAsync(Lingkungan l, Guid pasien, Guid kunjungan, Guid unit, Guid komponen)
    {
        var hasil = await l.OrderService().CreateAsync(
            new CreateBloodOrderRequest
            {
                PatientId = pasien,
                EncounterId = kunjungan,
                ServiceUnitId = unit,
                RequestingDoctorId = DokterBdrsTetap,
                Lines = new List<BloodOrderLineRequest> { new() { BloodComponentId = komponen, RequestedQuantity = 1 } }
            },
            PetugasUnit);

        Assert.True(hasil.Outcome == BloodOrderOutcome.Success, hasil.Message);

        return hasil.Entity!.Id;
    }

    private static T Tambah<T>(ApplicationDbContext db, T entity) where T : class
    {
        db.Set<T>().Add(entity);

        return entity;
    }

    private static Guid Kunjungan(ApplicationDbContext db, Guid pasien, Guid unit, string nomor, Guid? kelas)
        => Tambah(db, new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = nomor,
            PatientId = pasien,
            ServiceUnitId = unit,
            PatientClassId = kelas,
            EncounterType = EncounterType.Inpatient,
            EncounterStatus = EncounterStatus.Registered
        }).Id;

    private static Guid Tindakan(ApplicationDbContext db, string kode, string nama, bool aktif)
        => Tambah(db, new MstProcedure { Id = Guid.NewGuid(), ProcedureCode = kode, ProcedureName = nama, IsActive = aktif }).Id;

    private static Guid Tarif(
        ApplicationDbContext db,
        Guid kategori,
        Guid tindakan,
        string kode,
        decimal harga,
        Guid? kelas,
        Guid? unit = null,
        bool aktif = true,
        DateTime? mulai = null,
        DateTime? berakhir = null)
        => Tambah(db, new MstTariff
        {
            Id = Guid.NewGuid(),
            TariffCode = kode,
            TariffName = kode,
            TariffCategoryId = kategori,
            ProcedureId = tindakan,
            PatientClassId = kelas,
            ServiceUnitId = unit,
            NormalPrice = harga,
            IsActive = aktif,
            EffectiveStartDate = mulai,
            EffectiveEndDate = berakhir
        }).Id;
}
