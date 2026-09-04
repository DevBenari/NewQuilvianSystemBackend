using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-12</c> — endpoint wadah berperilaku baru
/// (<c>FR-02.1</c> .. <c>FR-02.3</c>, <c>FR-02.5</c>; <c>LAB-DEC-024</c>).
///
/// Yang dibuktikan di sini:
///   1. satu wadah direncanakan beserta beberapa pemeriksaan sekaligus, dengan satu barcode;
///   2. <c>AC-36</c> — menolak wadah menggugurkan <b>seluruh</b> pemeriksaan yang ditopangnya;
///   3. <c>VAL-13</c> — tidak ada jalur yang menolak sebagian pemeriksaan saja;
///   4. <c>VAL-05</c> sampai <c>VAL-15</c> yang menjadi milik task ini, masing-masing beserta
///      kode statusnya;
///   5. <c>VAL-09</c>, aturan empat mata pada tingkat wadah, ditegakkan **di dalam service** —
///      bukan diserahkan ke konfigurasi permission yang menurut <c>CAP-16</c> memang tidak
///      dapat menegakkannya.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini berjalan tanpa database mana pun. Jalur penetapan
/// layak yang menerbitkan fakta ke Billing tidak diuji di sini — penerbitannya adalah cakupan
/// <c>BE-LAB-13</c>, dan buktinya ada pada suite integrasi Postgres. Yang diuji adalah penjaga
/// yang berjalan <b>sebelum</b> penerbitan, beserta perpindahan status pemeriksaan.
/// </remarks>
public class LabSpecimenDecisionTests
{
    private static readonly Guid Pengambil = Guid.Parse("A1111111-1111-1111-1111-111111111111");
    private static readonly Guid Penilai = Guid.Parse("B2222222-2222-2222-2222-222222222222");

    // =====================================================================
    // 1. Merencanakan wadah beserta pemeriksaannya
    // =====================================================================

    [Fact]
    public async Task MerencanakanWadah_DenganDuaPemeriksaan_MenghasilkanSatuWadahDanDuaBaris()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var hasil = await CreateService(context, Penilai).PlanAsync(dunia.OrderId, new PlanLabSpecimenRequest
        {
            Examinations = new List<Guid> { dunia.Hemoglobin, dunia.Leukosit },
            SpecimenDescription = "Tabung ungu"
        });

        context.ChangeTracker.Clear();

        var wadah = await context.LabSpecimens.AsNoTracking()
            .Where(x => x.LabOrderId == dunia.OrderId).ToListAsync();

        var pemeriksaan = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == hasil.Specimen.Id).ToListAsync();

        // Satu wadah, satu barcode, dua pemeriksaan.
        Assert.Single(wadah);
        Assert.Equal(2, pemeriksaan.Count);
        Assert.All(pemeriksaan, x => Assert.Equal(LabExaminationStatus.Ordered, x.ExaminationStatus));

        // Masing-masing membawa salinan tarifnya sendiri — hemoglobin dan leukosit berbeda harga
        // walaupun berasal dari tabung yang sama.
        Assert.Equal(
            new decimal?[] { 35_000m, 30_000m },
            pemeriksaan.OrderBy(x => x.CreateDateTime).ThenBy(x => x.ProcedureCodeSnapshot == "WBC")
                .Select(x => x.UnitPriceSnapshot).OrderByDescending(x => x).ToArray());
    }

    [Fact]
    public async Task MerencanakanWadah_JalurRingkasSatuPemeriksaanTetapBerlaku()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var hasil = await CreateService(context, Penilai).PlanAsync(dunia.OrderId, new PlanLabSpecimenRequest
        {
            ProcedureId = dunia.Hemoglobin
        });

        context.ChangeTracker.Clear();

        var pemeriksaan = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == hasil.Specimen.Id).ToListAsync();

        Assert.Single(pemeriksaan);
        Assert.Equal(dunia.Hemoglobin, pemeriksaan[0].ProcedureId);
    }

    [Fact]
    public async Task VAL05_WadahTanpaSatuPunPemeriksaan_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context, procedureIdPesanan: Guid.Empty);

        var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
            CreateService(context, Penilai).PlanAsync(dunia.OrderId, new PlanLabSpecimenRequest()));

        Assert.Equal("Satu wadah harus memuat sekurang-kurangnya satu pemeriksaan.", galat.Message);
    }

    [Fact]
    public async Task VAL06_PesananYangSudahDibatalkan_TidakDapatMenerimaWadahBaru()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var pesanan = await context.LabOrders.SingleAsync(x => x.Id == dunia.OrderId);
        pesanan.OrderStatus = LabOrderStatus.Cancelled;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var galat = await Assert.ThrowsAsync<LabSpecimenConflictException>(() =>
            CreateService(context, Penilai).PlanAsync(dunia.OrderId, new PlanLabSpecimenRequest
            {
                Examinations = new List<Guid> { dunia.Hemoglobin }
            }));

        Assert.Equal("Pesanan ini sudah dibatalkan, wadah baru tidak dapat ditambahkan.", galat.Message);
    }

    [Fact]
    public async Task VAL07_JenisPemeriksaanYangSamaDuaKali_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
            CreateService(context, Penilai).PlanAsync(dunia.OrderId, new PlanLabSpecimenRequest
            {
                Examinations = new List<Guid> { dunia.Hemoglobin, dunia.Hemoglobin }
            }));

        Assert.Equal(
            "Pemeriksaan yang sama tidak boleh dimasukkan dua kali dalam satu wadah.",
            galat.Message);

        Assert.Equal(0, await context.LabSpecimens.CountAsync());
    }

    // =====================================================================
    // 2. AC-36 dan VAL-13 — keputusan atas wadah berlaku untuk seluruh isinya
    // =====================================================================

    [Fact]
    public async Task AC36_MenolakWadah_MenggugurkanSeluruhPemeriksaanYangDitopangnya()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, Penilai);

        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, service);

        await service.RejectAsync(wadah, new RejectLabSpecimenRequest
        {
            ReasonCode = "CLOTTED"
        });

        context.ChangeTracker.Clear();

        var pemeriksaan = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == wadah).ToListAsync();

        // Kedua pemeriksaan gugur bersama wadahnya — bukan satu saja.
        Assert.Equal(2, pemeriksaan.Count);
        Assert.All(pemeriksaan, x => Assert.Equal(LabExaminationStatus.Voided, x.ExaminationStatus));
        Assert.All(pemeriksaan, x => Assert.Null(x.ChargeEligibleAt));

        var tersimpan = await context.LabSpecimens.AsNoTracking().SingleAsync(x => x.Id == wadah);
        Assert.Equal(LabSpecimenStatus.Rejected, tersimpan.SpecimenStatus);
    }

    /// <summary>
    /// <c>VAL-13</c> ditegakkan secara struktural: tidak ada satu pun jalur pada grup wadah yang
    /// menerima daftar pemeriksaan untuk ditolak sebagian. Penolakan selalu menyasar wadah, dan
    /// wadah selalu membawa seluruh isinya.
    /// </summary>
    [Fact]
    public void VAL13_TidakAdaJalurYangMenolakSebagianPemeriksaan()
    {
        // Permintaan penolakan tidak punya ruas daftar pemeriksaan.
        var properties = typeof(RejectLabSpecimenRequest)
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        Assert.DoesNotContain("Examinations", properties);
        Assert.DoesNotContain("ExaminationIds", properties);
        Assert.DoesNotContain("ProcedureIds", properties);

        // Dan controller wadah hanya punya SATU jalur pengubah yang menolak — yang menyasar
        // wadah. `GetRejectionReasons` tidak ikut terhitung karena ia hanya membaca katalog.
        var rejectEndpoints = typeof(LabSpecimenController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x =>
                x.Name.Contains("Reject", StringComparison.OrdinalIgnoreCase) &&
                x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.HttpPostAttribute>().Any())
            .ToList();

        Assert.Single(rejectEndpoints);
        Assert.Equal("Reject", rejectEndpoints[0].Name);
    }

    [Fact]
    public async Task PemeriksaanYangSudahDibatalkanSendiri_TidakTertimpaKeputusanWadah()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, Penilai);

        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, service);

        // Satu pemeriksaan dibatalkan tersendiri lebih dulu — keputusan klinis atas satu jenis.
        var dibatalkan = await context.LabExaminations
            .Where(x => x.SpecimenId == wadah).OrderBy(x => x.ProcedureCodeSnapshot).FirstAsync();
        dibatalkan.ExaminationStatus = LabExaminationStatus.Cancelled;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await CreateService(context, Penilai).RejectAsync(wadah, new RejectLabSpecimenRequest
        {
            ReasonCode = "CLOTTED"
        });

        context.ChangeTracker.Clear();

        var sesudah = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == wadah).ToListAsync();

        // Yang dibatalkan tetap Cancelled; pembatalannya tidak boleh tertimpa menjadi Voided.
        Assert.Equal(
            LabExaminationStatus.Cancelled,
            sesudah.Single(x => x.Id == dibatalkan.Id).ExaminationStatus);

        Assert.Equal(
            LabExaminationStatus.Voided,
            sesudah.Single(x => x.Id != dibatalkan.Id).ExaminationStatus);
    }

    // =====================================================================
    // 3. VAL-08 sampai VAL-12
    // =====================================================================

    [Fact]
    public async Task VAL08_WadahYangBelumDiterima_TidakDapatDinyatakanLayak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, Penilai);

        var hasil = await service.PlanAsync(dunia.OrderId, new PlanLabSpecimenRequest
        {
            Examinations = new List<Guid> { dunia.Hemoglobin }
        });

        context.ChangeTracker.Clear();

        var galat = await Assert.ThrowsAsync<LabSpecimenConflictException>(() =>
            CreateService(context, Penilai).AcceptAsync(
                hasil.Specimen.Id, new AcceptLabSpecimenRequest()));

        Assert.Equal(
            "Wadah ini belum tercatat tiba di laboratorium, jadi belum bisa dinyatakan layak.",
            galat.Message);
    }

    /// <summary>
    /// Inti <c>VAL-09</c>. Orang yang mengambil sampel sudah punya kepentingan pada hasilnya
    /// dinyatakan layak; bila ia juga yang menilai, tidak ada mata kedua yang memeriksa
    /// pekerjaannya.
    /// </summary>
    [Fact]
    public async Task VAL09_PetugasYangMengambilSampel_TidakBolehMenyatakanKelayakannya()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, CreateService(context, Penilai));

        // Pengambilnya adalah Pengambil; kini ia sendiri yang mencoba menyatakan layak.
        var galat = await Assert.ThrowsAsync<LabSpecimenForbiddenException>(() =>
            CreateService(context, Pengambil).AcceptAsync(wadah, new AcceptLabSpecimenRequest()));

        Assert.Equal(
            "Petugas yang mengambil sampel tidak boleh menyatakan kelayakannya.",
            galat.Message);

        context.ChangeTracker.Clear();

        // Keadaan tidak bergeser sedikit pun.
        var tersimpan = await context.LabSpecimens.AsNoTracking().SingleAsync(x => x.Id == wadah);
        Assert.Equal(LabSpecimenStatus.Received, tersimpan.SpecimenStatus);

        var pemeriksaan = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == wadah).ToListAsync();
        Assert.All(pemeriksaan, x => Assert.Equal(LabExaminationStatus.Ordered, x.ExaminationStatus));
    }

    [Fact]
    public async Task VAL10_AlasanPenolakanKosong_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, CreateService(context, Penilai));

        var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
            CreateService(context, Penilai).RejectAsync(wadah, new RejectLabSpecimenRequest()));

        Assert.Equal("Pilih alasan penolakan lebih dulu.", galat.Message);
    }

    [Fact]
    public async Task VAL11_AlasanPenolakanTidakDikenal_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, CreateService(context, Penilai));

        var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
            CreateService(context, Penilai).RejectAsync(wadah, new RejectLabSpecimenRequest
            {
                ReasonCode = "TIDAK_ADA"
            }));

        Assert.Equal("Alasan penolakan yang dipilih tidak berlaku.", galat.Message);
    }

    [Fact]
    public async Task VAL12_AlasanYangMenuntutCatatanTanpaCatatan_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, CreateService(context, Penilai));

        var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
            CreateService(context, Penilai).RejectAsync(wadah, new RejectLabSpecimenRequest
            {
                ReasonCode = "OTHER"
            }));

        Assert.Equal(
            "Alasan ini membutuhkan keterangan tambahan. Mohon isi catatannya.",
            galat.Message);
    }

    // =====================================================================
    // 4. VAL-14 dan VAL-15 — pengambilan ulang
    // =====================================================================

    [Fact]
    public async Task VAL14_SebabAmbilUlangKosong_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadah = await SiapkanWadahDitolakAsync(context, dunia);

        var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
            CreateService(context, Penilai).RequestRecollectionAsync(
                wadah, new RequestLabRecollectionRequest()));

        Assert.Equal("Pilih sebab pengambilan ulang lebih dulu.", galat.Message);
    }

    [Fact]
    public async Task VAL15_SebabSelainKesalahanInternalTanpaAlasan_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadah = await SiapkanWadahDitolakAsync(context, dunia);

        var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
            CreateService(context, Penilai).RequestRecollectionAsync(wadah, new RequestLabRecollectionRequest
            {
                Cause = LabRecollectionCause.PatientOrSpecimenCondition
            }));

        Assert.Equal(
            "Pengambilan ulang dengan sebab ini membutuhkan alasan tertulis.",
            galat.Message);
    }

    /// <summary>
    /// <c>AC-38</c>. Bahannya diambil ulang karena yang lama tidak layak — bukan karena
    /// permintaan dokternya berubah. Menyalin hanya satu pemeriksaan akan diam-diam membatalkan
    /// sisanya.
    /// </summary>
    [Fact]
    public async Task AC38_WadahPengganti_MenampungSeluruhPemeriksaanWadahLama()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadahLama = await SiapkanWadahDitolakAsync(context, dunia);

        var hasil = await CreateService(context, Penilai).RequestRecollectionAsync(
            wadahLama,
            new RequestLabRecollectionRequest { Cause = LabRecollectionCause.InternalHospitalError });

        context.ChangeTracker.Clear();

        var pengganti = hasil.Specimen.Id;

        Assert.NotEqual(wadahLama, pengganti);

        var pemeriksaanPengganti = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == pengganti).ToListAsync();

        // Kedua pemeriksaan ikut pindah ke bahan pengganti, bukan satu saja.
        Assert.Equal(2, pemeriksaanPengganti.Count);
        Assert.All(pemeriksaanPengganti, x => Assert.Equal(LabExaminationStatus.Ordered, x.ExaminationStatus));
        Assert.Equal(
            new[] { dunia.Hemoglobin, dunia.Leukosit }.OrderBy(x => x),
            pemeriksaanPengganti.Select(x => x.ProcedureId).OrderBy(x => x));

        // Pemeriksaan wadah lama tetap gugur — keduanya tidak dihidupkan kembali.
        var pemeriksaanLama = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == wadahLama).ToListAsync();

        Assert.Equal(2, pemeriksaanLama.Count);
        Assert.All(pemeriksaanLama, x => Assert.Equal(LabExaminationStatus.Voided, x.ExaminationStatus));

        // Wadah pengganti membawa barcode sendiri dan menunjuk asal-usulnya.
        var penggantiTersimpan = await context.LabSpecimens.AsNoTracking()
            .SingleAsync(x => x.Id == pengganti);

        Assert.Equal(wadahLama, penggantiTersimpan.SupersededSpecimenId);
    }

    [Fact]
    public async Task AC38_PemeriksaanYangSudahDibatalkanSendiri_TidakIkutPindahKeWadahPengganti()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadahLama = await SiapkanWadahDiterimaAsync(context, dunia, CreateService(context, Penilai));

        // Satu pemeriksaan dibatalkan tersendiri sebelum wadahnya ditolak.
        var dibatalkan = await context.LabExaminations
            .Where(x => x.SpecimenId == wadahLama).OrderBy(x => x.ProcedureCodeSnapshot).FirstAsync();
        dibatalkan.ExaminationStatus = LabExaminationStatus.Cancelled;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await CreateService(context, Penilai).RejectAsync(
            wadahLama, new RejectLabSpecimenRequest { ReasonCode = "CLOTTED" });
        context.ChangeTracker.Clear();

        var hasil = await CreateService(context, Penilai).RequestRecollectionAsync(
            wadahLama,
            new RequestLabRecollectionRequest { Cause = LabRecollectionCause.InternalHospitalError });

        context.ChangeTracker.Clear();

        var pemeriksaanPengganti = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == hasil.Specimen.Id).ToListAsync();

        // Hanya satu yang pindah — pembatalan klinis tetap berlaku pada bahan pengganti.
        Assert.Single(pemeriksaanPengganti);
        Assert.NotEqual(dibatalkan.ProcedureId, pemeriksaanPengganti[0].ProcedureId);
    }

    // =====================================================================
    // 5. FR-05.1 — fakta kelayakan tagih per pemeriksaan
    // =====================================================================

    /// <summary>
    /// Inti <c>FR-05.1</c> dan <c>AC-37</c>: wadah adalah bahan, yang ditagihkan adalah
    /// pemeriksaan yang dikerjakan darinya. Satu tabung yang menopang dua pemeriksaan
    /// menerbitkan <b>dua</b> fakta dengan salinan tarif masing-masing — bukan satu fakta
    /// berharga satu tabung.
    /// </summary>
    [Fact]
    public async Task FR0501_WadahDuaPemeriksaan_MenerbitkanDuaFaktaDenganTarifMasingMasing()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, CreateService(context, Penilai));

        var hasil = await CreateService(context, Penilai).AcceptAsync(
            wadah, new AcceptLabSpecimenRequest());

        Assert.NotNull(hasil.Handoff);

        // Dua fakta, bukan satu.
        Assert.Equal(2, hasil.Handoff!.Count);
        Assert.Equal(2, hasil.Handoff.FactIds.Distinct().Count());

        context.ChangeTracker.Clear();

        // Masing-masing menunjuk identitas pemeriksaan, dan setiap pemeriksaan menjadi layak
        // tagih pada waktu keputusan yang sama.
        var pemeriksaan = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == wadah).ToListAsync();

        Assert.Equal(2, pemeriksaan.Count);
        Assert.All(pemeriksaan, x => Assert.Equal(LabExaminationStatus.ChargeEligible, x.ExaminationStatus));
        Assert.All(pemeriksaan, x => Assert.NotNull(x.ChargeEligibleAt));
        Assert.Single(pemeriksaan.Select(x => x.ChargeEligibleAt).Distinct());

        // Salinan tarifnya berbeda — 35.000 dan 30.000, bukan satu angka untuk seluruh tabung.
        Assert.Equal(
            new decimal?[] { 30_000m, 35_000m },
            pemeriksaan.Select(x => x.UnitPriceSnapshot).OrderBy(x => x).ToArray());
    }

    /// <summary>
    /// Idempotensi. Menekan tombol layak dua kali menghasilkan <b>dua</b> fakta, bukan empat —
    /// karena identitas faktanya menunjuk pemeriksaan yang sama.
    /// </summary>
    [Fact]
    public async Task FR0501_MenekanLayakDuaKali_TetapMenghasilkanDuaFakta()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, CreateService(context, Penilai));

        var pertama = await CreateService(context, Penilai).AcceptAsync(
            wadah, new AcceptLabSpecimenRequest());
        context.ChangeTracker.Clear();

        var kedua = await CreateService(context, Penilai).AcceptAsync(
            wadah, new AcceptLabSpecimenRequest());

        Assert.Equal(2, pertama.Handoff!.Count);
        Assert.Equal(2, kedua.Handoff!.Count);

        // Identitas faktanya sama persis — pengiriman ulang, bukan fakta baru.
        Assert.Equal(
            pertama.Handoff.FactIds.OrderBy(x => x),
            kedua.Handoff.FactIds.OrderBy(x => x));
    }

    [Fact]
    public async Task WadahDitolak_TidakMenerbitkanFaktaApaPun()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, CreateService(context, Penilai));

        var hasil = await CreateService(context, Penilai).RejectAsync(
            wadah, new RejectLabSpecimenRequest { ReasonCode = "CLOTTED" });

        Assert.Null(hasil.Handoff);
    }

    // =====================================================================
    // 6. Pemetaan kode status pada controller
    // =====================================================================

    /// <summary>
    /// Matriks validasi menetapkan kode yang berbeda untuk aturan yang berbeda. Sebelum task
    /// ini, seluruhnya menjadi <c>400</c>, sehingga layar tidak dapat membedakan permintaan
    /// yang cacat bentuk dari permintaan yang melanggar aturan bisnis.
    /// </summary>
    [Fact]
    public void ControllerWadah_MemetakanKetigaTipeGalatKeKodeYangBenar()
    {
        var source = File.ReadAllText(LokasiControllerWadah());

        Assert.Contains("catch (LabSpecimenForbiddenException", source);
        Assert.Contains("Status403Forbidden", source);

        Assert.Contains("catch (LabSpecimenConflictException", source);
        Assert.Contains("catch (LabSpecimenValidationException", source);
        Assert.Contains("Status422UnprocessableEntity", source);

        // Urutannya penting: ketiganya harus ditangkap sebelum ArgumentException yang lebih umum.
        Assert.True(
            source.IndexOf("catch (LabSpecimenValidationException", StringComparison.Ordinal) <
            source.IndexOf("catch (ArgumentException", StringComparison.Ordinal),
            "Tipe galat spesifik harus ditangkap sebelum ArgumentException.");
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private sealed record Dunia(Guid OrderId, Guid Hemoglobin, Guid Leukosit);

    /// <summary>
    /// Wadah berisi dua pemeriksaan, sudah diambil oleh <see cref="Pengambil"/> dan tercatat
    /// tiba di laboratorium.
    /// </summary>
    private static async Task<Guid> SiapkanWadahDiterimaAsync(
        ApplicationDbContext context, Dunia dunia, LabSpecimenService service)
    {
        var hasil = await service.PlanAsync(dunia.OrderId, new PlanLabSpecimenRequest
        {
            Examinations = new List<Guid> { dunia.Hemoglobin, dunia.Leukosit }
        });

        context.ChangeTracker.Clear();

        var wadah = await context.LabSpecimens.SingleAsync(x => x.Id == hasil.Specimen.Id);
        wadah.SpecimenStatus = LabSpecimenStatus.Received;
        wadah.CollectedByUserId = Pengambil;
        wadah.CollectedAt = DateTime.UtcNow;
        wadah.ReceivedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return wadah.Id;
    }

    private static async Task<Guid> SiapkanWadahDitolakAsync(ApplicationDbContext context, Dunia dunia)
    {
        var wadah = await SiapkanWadahDiterimaAsync(context, dunia, CreateService(context, Penilai));

        await CreateService(context, Penilai).RejectAsync(wadah, new RejectLabSpecimenRequest
        {
            ReasonCode = "CLOTTED"
        });

        context.ChangeTracker.Clear();

        return wadah;
    }

    private static string LokasiControllerWadah()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "QuilvianSystemBackend.csproj")))
            dir = dir.Parent;

        Assert.NotNull(dir);

        return Path.Combine(
            dir!.FullName,
            "Areas", "HealthServices", "LaboratoryManagement", "Controllers",
            "LabSpecimenController.cs");
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-specimen-decision-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LabSpecimenService CreateService(ApplicationDbContext context, Guid actorUserId)
    {
        var accessor = CreateHttpContextAccessor(actorUserId);
        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, accessor);

        return new LabSpecimenService(
            context,
            new ClinicalMilestoneFactProducer(context, new BillingFolioService(context), loggerService),
            accessor,
            loggerService);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(Guid actorUserId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            authenticationType: "LabSpecimenDecisionTest");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static async Task<Dunia> SeedAsync(
        ApplicationDbContext context, Guid? procedureIdPesanan = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var hemoglobin = Procedure($"HB-{suffix}", "Hemoglobin");
        var leukosit = Procedure($"WBC-{suffix}", "Leukosit");

        context.Set<MstProcedure>().AddRange(hemoglobin, leukosit);

        context.Set<MstTariff>().AddRange(
            Tarif(hemoglobin.Id, "TRF-HB", 35_000m),
            Tarif(leukosit.Id, "TRF-WBC", 30_000m));

        context.MstLabRejectionReasons.AddRange(
            new MstLabRejectionReason
            {
                Id = Guid.NewGuid(), ReasonCode = "CLOTTED", ReasonName = "Sampel menggumpal",
                IsActive = true, RequiresNote = false
            },
            new MstLabRejectionReason
            {
                Id = Guid.NewGuid(), ReasonCode = "OTHER", ReasonName = "Lainnya",
                IsActive = true, RequiresNote = true
            });

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = procedureIdPesanan ?? hemoglobin.Id,
            Discipline = LabDiscipline.ClinicalPathology,
            OrderStatus = LabOrderStatus.Requested
        };

        context.LabOrders.Add(order);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return new Dunia(order.Id, hemoglobin.Id, leukosit.Id);
    }

    private static MstProcedure Procedure(string kode, string nama) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProcedureCode = kode,
            ProcedureName = nama,
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

    private static MstTariff Tarif(Guid procedureId, string kode, decimal harga) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProcedureId = procedureId,
            TariffCode = kode,
            NormalPrice = harga
        };
}
