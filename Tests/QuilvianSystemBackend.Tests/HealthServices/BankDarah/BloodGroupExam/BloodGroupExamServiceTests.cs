using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.BloodGroupExam;

/// <summary>
/// Bukti untuk <c>BE-BD-005</c> (pemeriksaan golongan darah) dan <c>BE-BD-011</c>
/// (penyelesaian perbedaan hasil), blueprint <c>BD-BP-001</c>, kontrak <c>v4</c>.
/// </summary>
/// <remarks>
/// <para>
/// Yang dibuktikan di sini, berurutan sesuai acceptance criteria:
/// </para>
/// <list type="bullet">
/// <item><c>AC-BD-030</c> — hasil tanpa pemeriksa atau waktu pemeriksaan ditolak.</item>
/// <item><c>AC-BD-034</c> — hasil tervalidasi baru yang berbeda menahan keduanya; pasien
/// kehilangan golongan darah sah dan kedua hasil tetap tersimpan.</item>
/// <item><c>AC-BD-035</c> — hasil tervalidasi baru yang sama berlaku tanpa penahanan.</item>
/// <item><c>AC-BD-036</c> — konflik diselesaikan validator; tepat satu hasil sah kembali;
/// pelaku, alasan, dan waktu tersimpan; riwayat kedua hasil tetap terbaca.</item>
/// <item><c>AC-BD-051</c> / <c>AC-BD-080</c> — penyelesaian tanpa pemeriksaan ulang
/// tervalidasi ditolak.</item>
/// <item><c>AC-BD-053</c> — hasil ulang bernilai ketiga tetap boleh dinyatakan berlaku.</item>
/// <item><c>AC-BD-054</c> — sistem tidak pernah menghitung mayoritas.</item>
/// <item><c>AC-BD-077</c> / <c>AC-BD-078</c> — pemisahan wewenang validasi rutin dari
/// penyelesaian konflik.</item>
/// </list>
/// <para>
/// <b>Batas yang jujur.</b> <c>AC-BD-037</c>, <c>AC-BD-078</c>, dan <c>AC-BD-079</c> menyangkut
/// <b>kewenangan</b>, yang ditegakkan <c>AccessPermissionFilter</c> lewat dua butir hak akses
/// berbeda — bukan oleh service ini, yang memang tidak boleh memeriksa peran sama sekali. Bukti
/// pemisahannya karena itu ada pada <c>BloodBankRoleAccessContractTests</c>; yang diuji di sini
/// adalah sisi service-nya, yaitu bahwa prasyarat tetap berlaku bahkan bagi pelaku yang
/// berwenang penuh (<c>AC-BD-080</c>).
/// </para>
/// <para>
/// Provider InMemory dipakai supaya bukti ini dapat dijalankan tanpa database mana pun.
/// Konsekuensinya index unik fisik pada <c>SampleIdentifier</c> tidak ikut teruji; penolakan
/// identifier kembar dibuktikan lewat jalur pemeriksaan service, dan index uniknya menjadi
/// bagian verifikasi migration.
/// </para>
/// </remarks>
public class BloodGroupExamServiceTests
{
    private static readonly Guid PetugasPengambil = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Pemeriksa = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PetugasBerwenangValidasi = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ValidatorKlinis = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private const string KodeAlasan = "SEED-PENDING-SELESAI";

    // =====================================================================
    // 1. Pengambilan sampel
    // =====================================================================

    [Fact]
    public async Task Sampel_Dicatat_MembukaPemeriksaanBerstatusSampleTaken()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        var hasil = await service.RecordSampleAsync(
            new RecordSampleRequest { PatientId = pasien, SampleIdentifier = " bd-0001 " },
            PetugasPengambil);

        Assert.Equal(BloodGroupExamOutcome.Success, hasil.Outcome);
        Assert.Equal(BbkBloodGroupExamStatus.SampleTaken, hasil.Entity!.ExamStatus);
        Assert.False(hasil.Entity.IsValidResult);
        Assert.False(hasil.Entity.IsConflictHeld);
        Assert.Null(hasil.Entity.AboRhesusResult);

        var sampel = Assert.Single(hasil.Entity.Samples);
        Assert.Equal("BD-0001", sampel.SampleIdentifier);
        Assert.Equal(PetugasPengambil, sampel.TakenByUserId);
    }

    /// <summary>
    /// Identifier sampel ditulis petugas, jadi keunikannya benar-benar dapat dilanggar dua
    /// tabung yang diberi label sama.
    /// </summary>
    [Fact]
    public async Task Sampel_IdentifierYangSudahDipakai_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        await service.RecordSampleAsync(Permintaan(pasien, "BD-0001"), PetugasPengambil);

        var kedua = await service.RecordSampleAsync(Permintaan(pasien, "bd-0001"), PetugasPengambil);

        Assert.Equal(BloodGroupExamOutcome.DuplicateIdentity, kedua.Outcome);
        Assert.Equal(1, await db.Set<BbkBloodGroupSample>().CountAsync());
    }

    [Fact]
    public async Task Sampel_PasienTidakDikenal_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);

        var hasil = await service.RecordSampleAsync(
            Permintaan(Guid.NewGuid(), "BD-0001"),
            PetugasPengambil);

        Assert.Equal(BloodGroupExamOutcome.Invalid, hasil.Outcome);
        Assert.Equal(0, await db.Set<BbkBloodGroupExam>().CountAsync());
    }

    [Fact]
    public async Task Sampel_WaktuPengambilanDiMasaDepan_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        var permintaan = Permintaan(pasien, "BD-0001");
        permintaan.TakenAt = DateTime.UtcNow.AddHours(2);

        var hasil = await service.RecordSampleAsync(permintaan, PetugasPengambil);

        Assert.Equal(BloodGroupExamOutcome.Invalid, hasil.Outcome);
    }

    // =====================================================================
    // 2. Pencatatan hasil — AC-BD-030
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-030</c> sisi positif: pemeriksa dan waktu pemeriksaan <b>selalu</b> tersimpan,
    /// karena keduanya diturunkan dari pelaku terautentikasi dan jam server — bukan dari isian
    /// yang dapat dikosongkan pemanggil.
    /// </summary>
    [Fact]
    public async Task Hasil_Dicatat_SelaluMenyimpanPemeriksaDanWaktuPemeriksaan()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var exam = await SampaiHasilTercatatAsync(db, service, BloodType.OPositive);

        Assert.Equal(BbkBloodGroupExamStatus.ResultRecorded, exam.ExamStatus);
        Assert.Equal(Pemeriksa, exam.ExaminedByUserId);
        Assert.NotNull(exam.ExaminedAt);
        Assert.Equal(BloodType.OPositive, exam.AboRhesusResult);
    }

    /// <summary>
    /// <c>AC-BD-030</c> — <c>VAL-BD-030</c>. Pelaku yang tidak dikenali tidak dapat meninggalkan
    /// hasil tanpa pemeriksa.
    /// </summary>
    [Fact]
    public async Task Hasil_PelakuTidakDikenali_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        var dibuat = await service.RecordSampleAsync(Permintaan(pasien, "BD-0001"), PetugasPengambil);

        var hasil = await service.RecordResultAsync(
            dibuat.Entity!.Id,
            new RecordResultRequest { AboRhesusResult = BloodType.OPositive },
            Guid.Empty);

        Assert.Equal(BloodGroupExamOutcome.Invalid, hasil.Outcome);

        var tersimpan = await db.Set<BbkBloodGroupExam>().SingleAsync();
        Assert.Equal(BbkBloodGroupExamStatus.SampleTaken, tersimpan.ExamStatus);
        Assert.Null(tersimpan.ExaminedByUserId);
    }

    /// <summary>
    /// <c>Unknown</c> dan <c>NotDisclosed</c> menyatakan ketiadaan hasil. Menerimanya akan
    /// membuat pasien punya golongan darah sah yang isinya "tidak diketahui".
    /// </summary>
    [Theory]
    [InlineData(BloodType.Unknown)]
    [InlineData(BloodType.NotDisclosed)]
    public async Task Hasil_NilaiYangMenyatakanKetiadaanHasil_Ditolak(BloodType nilai)
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        var dibuat = await service.RecordSampleAsync(Permintaan(pasien, "BD-0001"), PetugasPengambil);

        var hasil = await service.RecordResultAsync(
            dibuat.Entity!.Id,
            new RecordResultRequest { AboRhesusResult = nilai },
            Pemeriksa);

        Assert.Equal(BloodGroupExamOutcome.Invalid, hasil.Outcome);
    }

    /// <summary>Hasil tervalidasi tidak pernah ditimpa (<c>DEC-BD-026</c>).</summary>
    [Fact]
    public async Task Hasil_PemeriksaanYangSudahTervalidasi_TidakDapatDitimpa()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        var exam = await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);

        var hasil = await service.RecordResultAsync(
            exam.Id,
            new RecordResultRequest { AboRhesusResult = BloodType.APositive },
            Pemeriksa);

        Assert.Equal(BloodGroupExamOutcome.NotAllowedByState, hasil.Outcome);

        var tersimpan = await db.Set<BbkBloodGroupExam>().SingleAsync(x => x.Id == exam.Id);
        Assert.Equal(BloodType.OPositive, tersimpan.AboRhesusResult);
    }

    // =====================================================================
    // 3. Validasi rutin dan deteksi perbedaan — AC-BD-034, AC-BD-035
    // =====================================================================

    [Fact]
    public async Task Validasi_HasilPertamaPasien_LangsungBerlakuSebagaiHasilSah()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        var exam = await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);

        Assert.Equal(BbkBloodGroupExamStatus.Validated, exam.ExamStatus);
        Assert.True(exam.IsValidResult);
        Assert.False(exam.IsConflictHeld);

        var golongan = await service.GetValidBloodGroupAsync(pasien);
        Assert.Equal(BloodType.OPositive, golongan.BloodType);
        Assert.True(golongan.IsUsableForClinicalDecision);
    }

    /// <summary>
    /// <c>AC-BD-035</c> — hasil tervalidasi baru bernilai sama dengan hasil sah sebelumnya:
    /// hasil terbaru berlaku, <b>tanpa penahanan apa pun</b>.
    /// </summary>
    [Fact]
    public async Task Validasi_HasilBaruSamaDenganHasilSah_BerlakuTanpaPenahanan()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        var lama = await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        var baru = await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.OPositive);

        await ReloadAsync(db, lama);

        Assert.False(lama.IsValidResult);
        Assert.False(lama.IsConflictHeld);
        Assert.True(baru.IsValidResult);
        Assert.False(baru.IsConflictHeld);

        var golongan = await service.GetValidBloodGroupAsync(pasien);
        Assert.False(golongan.IsConflictHeld);
        Assert.Equal(BloodType.OPositive, golongan.BloodType);
        Assert.Equal(baru.Id, golongan.SourceExamId);
        Assert.True(golongan.IsUsableForClinicalDecision);
    }

    /// <summary>
    /// <c>AC-BD-034</c> dan <c>BD-XINV-04</c> — hasil sah O Positif lalu muncul hasil
    /// tervalidasi baru A Positif: pasien tidak punya hasil sah, gerbang tertahan, dan
    /// <b>kedua hasil tetap tersimpan</b>.
    /// </summary>
    [Fact]
    public async Task Validasi_HasilBaruBerbeda_MenahanKeduanyaDanPasienKehilanganHasilSah()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        var lama = await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        var baru = await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);

        await ReloadAsync(db, lama);

        Assert.True(lama.IsConflictHeld);
        Assert.False(lama.IsValidResult);
        Assert.True(baru.IsConflictHeld);
        Assert.False(baru.IsValidResult);

        // Kedua hasil tetap tersimpan apa adanya — tidak satu pun ditimpa atau dihapus.
        Assert.Equal(BloodType.OPositive, lama.AboRhesusResult);
        Assert.Equal(BloodType.APositive, baru.AboRhesusResult);
        Assert.Equal(2, await db.Set<BbkBloodGroupExam>().CountAsync());

        var golongan = await service.GetValidBloodGroupAsync(pasien);
        Assert.True(golongan.IsConflictHeld);
        Assert.Null(golongan.BloodType);
        Assert.False(golongan.IsUsableForClinicalDecision);
        Assert.Equal(2, golongan.ConflictingExamIds.Count);
    }

    /// <summary>
    /// Memvalidasi pemeriksaan ulang saat perbedaan sedang tertahan <b>tidak</b> menutup
    /// perbedaan itu sendiri. Penutupannya menuntut pernyataan validator klinis
    /// (<c>DEC-BD-031</c>).
    /// </summary>
    [Fact]
    public async Task Validasi_PemeriksaanUlangSaatKonflikTertahan_TidakMenutupKonflik()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);

        var ulang = await SampaiTervalidasiAsync(db, service, pasien, "BD-0003", BloodType.OPositive);

        Assert.Equal(BbkBloodGroupExamStatus.Validated, ulang.ExamStatus);
        Assert.False(ulang.IsValidResult);
        Assert.False(ulang.IsConflictHeld);

        var golongan = await service.GetValidBloodGroupAsync(pasien);
        Assert.True(golongan.IsConflictHeld);
        Assert.Null(golongan.BloodType);
    }

    [Fact]
    public async Task Validasi_HasilYangBelumDicatat_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        var dibuat = await service.RecordSampleAsync(Permintaan(pasien, "BD-0001"), PetugasPengambil);

        var hasil = await service.ValidateAsync(dibuat.Entity!.Id, PetugasBerwenangValidasi);

        Assert.Equal(BloodGroupExamOutcome.NotAllowedByState, hasil.Outcome);
    }

    [Fact]
    public async Task GolonganDarahSah_PasienTanpaPemeriksaan_KosongDanTidakDapatDipakaiKlinis()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);

        var golongan = await service.GetValidBloodGroupAsync(Guid.NewGuid());

        Assert.Null(golongan.BloodType);
        Assert.False(golongan.IsConflictHeld);
        Assert.False(golongan.IsUsableForClinicalDecision);
    }

    /// <summary>
    /// Hasil yang sudah dicatat tetapi <b>belum divalidasi</b> tidak pernah menjadi golongan
    /// darah sah (<c>BD-DOM-09</c>).
    /// </summary>
    [Fact]
    public async Task GolonganDarahSah_HasilBelumTervalidasi_TidakPernahDipakaiKlinis()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        await SampaiHasilTercatatAsync(db, service, BloodType.OPositive, pasien);

        var golongan = await service.GetValidBloodGroupAsync(pasien);

        Assert.Null(golongan.BloodType);
        Assert.False(golongan.IsUsableForClinicalDecision);
    }

    // =====================================================================
    // 4. Penyelesaian perbedaan — BE-BD-011
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-036</c> dan <c>AC-BD-079</c> — perbedaan diselesaikan lewat pemeriksaan ulang
    /// tervalidasi: tepat satu hasil sah kembali berlaku; pelaku, alasan, dan waktu tersimpan;
    /// riwayat kedua hasil lama tetap terbaca.
    /// </summary>
    [Fact]
    public async Task Penyelesaian_MenunjukPemeriksaanUlangTervalidasi_MengembalikanTepatSatuHasilSah()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        await TambahAlasanAsync(db);

        var lama = await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        var baru = await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);
        var ulang = await SampaiTervalidasiAsync(db, service, pasien, "BD-0003", BloodType.APositive);

        var hasil = await service.ResolveConflictAsync(
            new ResolveConflictRequest
            {
                PatientId = pasien,
                ResolvingExamId = ulang.Id,
                ReasonCode = KodeAlasan
            },
            ValidatorKlinis);

        Assert.Equal(BloodGroupExamOutcome.Success, hasil.Outcome);
        Assert.Equal(BloodType.APositive, hasil.ValidBloodGroup!.BloodType);
        Assert.False(hasil.ValidBloodGroup.IsConflictHeld);

        var seluruhnya = await db.Set<BbkBloodGroupExam>()
            .Where(x => x.PatientId == pasien)
            .ToListAsync();

        // Tepat satu hasil sah — INV-BD-018.
        var satuSatunyaHasilSah = Assert.Single(seluruhnya, x => x.IsValidResult);
        Assert.Equal(ulang.Id, satuSatunyaHasilSah.Id);
        Assert.DoesNotContain(seluruhnya, x => x.IsConflictHeld);

        // Riwayat kedua hasil lama tetap terbaca, nilainya tidak disentuh.
        Assert.Equal(3, seluruhnya.Count);
        Assert.Equal(BloodType.OPositive, seluruhnya.Single(x => x.Id == lama.Id).AboRhesusResult);
        Assert.Equal(BloodType.APositive, seluruhnya.Single(x => x.Id == baru.Id).AboRhesusResult);

        // Pelaku, alasan, dan waktu tersimpan pada catatan append-only.
        var catatan = await db.Set<BbkBloodGroupConflictResolution>().SingleAsync();
        Assert.Equal(pasien, catatan.PatientId);
        Assert.Equal(ulang.Id, catatan.ResolvingExamId);
        Assert.Equal(ValidatorKlinis, catatan.ResolvedByUserId);
        Assert.Equal(KodeAlasan, catatan.ReasonCode);
        Assert.NotEqual(default, catatan.ResolvedAt);

        var golongan = await service.GetValidBloodGroupAsync(pasien);
        Assert.True(golongan.IsUsableForClinicalDecision);
        Assert.Equal(BloodType.APositive, golongan.BloodType);
    }

    /// <summary>
    /// <c>AC-BD-053</c> — pemeriksaan ulang menghasilkan nilai <b>ketiga</b> yang berbeda dari
    /// kedua hasil bentrok, dan validator menyatakannya berlaku. Diterima: sistem tidak memaksa
    /// hasil baru cocok dengan salah satu hasil lama.
    /// </summary>
    [Fact]
    public async Task Penyelesaian_HasilUlangBernilaiKetiga_TetapDiterima()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        await TambahAlasanAsync(db);

        await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);
        var ulang = await SampaiTervalidasiAsync(db, service, pasien, "BD-0003", BloodType.BNegative);

        var hasil = await service.ResolveConflictAsync(
            new ResolveConflictRequest
            {
                PatientId = pasien,
                ResolvingExamId = ulang.Id,
                ReasonCode = KodeAlasan
            },
            ValidatorKlinis);

        Assert.Equal(BloodGroupExamOutcome.Success, hasil.Outcome);
        Assert.Equal(BloodType.BNegative, hasil.ValidBloodGroup!.BloodType);
    }

    /// <summary>
    /// <c>AC-BD-051</c> dan <c>AC-BD-080</c> — <c>VAL-BD-051</c>. Penyelesaian tanpa menunjuk
    /// pemeriksaan ulang ditolak, <b>walaupun pelakunya validator klinis</b>: wewenang tidak
    /// menggantikan prasyarat.
    /// </summary>
    [Fact]
    public async Task Penyelesaian_TanpaMenunjukPemeriksaanUlang_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        await TambahAlasanAsync(db);

        await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);

        var hasil = await service.ResolveConflictAsync(
            new ResolveConflictRequest
            {
                PatientId = pasien,
                ResolvingExamId = Guid.Empty,
                ReasonCode = KodeAlasan
            },
            ValidatorKlinis);

        Assert.Equal(BloodGroupExamOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Contains("pemeriksaan ulang", hasil.Message);
        Assert.Equal(0, await db.Set<BbkBloodGroupConflictResolution>().CountAsync());

        var golongan = await service.GetValidBloodGroupAsync(pasien);
        Assert.True(golongan.IsConflictHeld);
    }

    /// <summary>
    /// <c>VAL-BD-051</c> — menunjuk salah satu pihak konflik sama saja dengan memilih salah satu
    /// hasil lama, dan itu persis yang ditolak <c>DEC-BD-031</c>.
    /// </summary>
    [Fact]
    public async Task Penyelesaian_MenunjukSalahSatuPihakKonflik_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        await TambahAlasanAsync(db);

        var lama = await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);

        var hasil = await service.ResolveConflictAsync(
            new ResolveConflictRequest
            {
                PatientId = pasien,
                ResolvingExamId = lama.Id,
                ReasonCode = KodeAlasan
            },
            ValidatorKlinis);

        Assert.Equal(BloodGroupExamOutcome.NotAllowedByState, hasil.Outcome);
        Assert.Equal(0, await db.Set<BbkBloodGroupConflictResolution>().CountAsync());
    }

    /// <summary>
    /// <c>VAL-BD-051</c> — pemeriksaan ulang yang belum divalidasi belum dapat memutus apa pun.
    /// </summary>
    [Fact]
    public async Task Penyelesaian_PemeriksaanUlangBelumTervalidasi_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        await TambahAlasanAsync(db);

        await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);
        var ulang = await SampaiHasilTercatatAsync(db, service, BloodType.APositive, pasien, "BD-0003");

        var hasil = await service.ResolveConflictAsync(
            new ResolveConflictRequest
            {
                PatientId = pasien,
                ResolvingExamId = ulang.Id,
                ReasonCode = KodeAlasan
            },
            ValidatorKlinis);

        Assert.Equal(BloodGroupExamOutcome.NotAllowedByState, hasil.Outcome);
    }

    /// <summary>
    /// <c>AC-BD-054</c> — sistem tidak menghitung mayoritas. Dibuktikan pada keadaan yang
    /// <b>punya</b> mayoritas jelas: dua hasil A Positif berbanding satu O Positif. Sistem tetap
    /// menolak menutup konflik tanpa validator menyebut satu pemeriksaan, dan tetap tidak
    /// memilih A Positif sendiri.
    /// </summary>
    [Fact]
    public async Task Penyelesaian_SistemTidakPernahMenghitungMayoritas()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        await TambahAlasanAsync(db);

        await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0003", BloodType.APositive);

        // Mayoritas A Positif tersedia, dan sistem tetap tidak memakainya.
        var golongan = await service.GetValidBloodGroupAsync(pasien);
        Assert.True(golongan.IsConflictHeld);
        Assert.Null(golongan.BloodType);

        var hasil = await service.ResolveConflictAsync(
            new ResolveConflictRequest
            {
                PatientId = pasien,
                ResolvingExamId = Guid.Empty,
                ReasonCode = KodeAlasan
            },
            ValidatorKlinis);

        Assert.Equal(BloodGroupExamOutcome.NotAllowedByState, hasil.Outcome);
    }

    /// <summary>
    /// Permintaan penyelesaian <b>tidak punya</b> field yang meminta sistem memilih sendiri.
    /// Ketiadaan itu bagian dari kontrak (<c>INV-BD-022</c>), sehingga dijaga sebagai bukti,
    /// bukan diserahkan pada ingatan.
    /// </summary>
    [Fact]
    public void Penyelesaian_PermintaanTidakPunyaModePemilihanOtomatis()
    {
        var properti = typeof(ResolveConflictRequest)
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        Assert.Equal(
            new[] { "PatientId", "ReasonCode", "ResolvingExamId" },
            properti.OrderBy(x => x, StringComparer.Ordinal).ToArray());
    }

    /// <summary>Alasan tidak boleh teks bebas (<c>INV-BD-016</c>).</summary>
    [Fact]
    public async Task Penyelesaian_AlasanDiLuarDaftarTerkendali_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        await TambahAlasanAsync(db);

        await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);
        var ulang = await SampaiTervalidasiAsync(db, service, pasien, "BD-0003", BloodType.APositive);

        var hasil = await service.ResolveConflictAsync(
            new ResolveConflictRequest
            {
                PatientId = pasien,
                ResolvingExamId = ulang.Id,
                ReasonCode = "ALASAN-KETIKAN-BEBAS"
            },
            ValidatorKlinis);

        Assert.Equal(BloodGroupExamOutcome.Invalid, hasil.Outcome);
        Assert.Equal(0, await db.Set<BbkBloodGroupConflictResolution>().CountAsync());
    }

    [Fact]
    public async Task Penyelesaian_PasienTidakSedangMenahanPerbedaan_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);
        await TambahAlasanAsync(db);

        var exam = await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);

        var hasil = await service.ResolveConflictAsync(
            new ResolveConflictRequest
            {
                PatientId = pasien,
                ResolvingExamId = exam.Id,
                ReasonCode = KodeAlasan
            },
            ValidatorKlinis);

        Assert.Equal(BloodGroupExamOutcome.NotAllowedByState, hasil.Outcome);
    }

    // =====================================================================
    // 5. Pembacaan daftar dan ringkasan
    // =====================================================================

    [Fact]
    public async Task Ringkasan_MenghitungPerbedaanYangTertahanPerPasien()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);

        var ringkasan = await service.GetSummaryAsync();

        Assert.Equal(2, ringkasan.TotalExam);
        Assert.Equal(2, ringkasan.ValidatedExam);
        Assert.Equal(2, ringkasan.ConflictHeldExam);
        Assert.Equal(1, ringkasan.PatientWithHeldConflict);
    }

    [Fact]
    public async Task Daftar_DapatDisaringKeadaanPerbedaanYangTertahan()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        await SampaiTervalidasiAsync(db, service, pasien, "BD-0001", BloodType.OPositive);
        await SampaiTervalidasiAsync(db, service, pasien, "BD-0002", BloodType.APositive);
        await service.RecordSampleAsync(Permintaan(pasien, "BD-0003"), PetugasPengambil);

        var tertahan = await service.GetPagedAsync(
            null, null, null, isConflictHeld: true, null, null, null, 1, 25);

        Assert.Equal(2, tertahan.TotalData);
        Assert.All(tertahan.Items, x => Assert.True(x.IsConflictHeld));
    }

    [Fact]
    public async Task Daftar_DicariLewatIdentifierSampel()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        await service.RecordSampleAsync(Permintaan(pasien, "BD-0001"), PetugasPengambil);
        await service.RecordSampleAsync(Permintaan(pasien, "BD-0002"), PetugasPengambil);

        var hasil = await service.GetPagedAsync(
            "bd-0002", null, null, null, null, null, null, 1, 25);

        Assert.Equal(1, hasil.TotalData);
        Assert.Equal("BD-0002", hasil.Items[0].SampleIdentifier);
    }

    /// <summary>
    /// Aksi yang tersedia menjawab kelayakan status, dan berpindah mengikuti keadaan
    /// pemeriksaan.
    /// </summary>
    [Fact]
    public async Task Detail_AksiTersediaMengikutiKeadaanPemeriksaan()
    {
        await using var db = CreateContext();
        var service = new BbkBloodGroupExamService(db);
        var pasien = await TambahPasienAsync(db);

        var dibuat = await service.RecordSampleAsync(Permintaan(pasien, "BD-0001"), PetugasPengambil);
        var sampelDiambil = await service.GetDetailAsync(dibuat.Entity!.Id);
        Assert.Equal(new[] { "RecordResult" }, sampelDiambil!.AvailableActions);

        await service.RecordResultAsync(
            dibuat.Entity.Id,
            new RecordResultRequest { AboRhesusResult = BloodType.OPositive },
            Pemeriksa);

        var hasilTercatat = await service.GetDetailAsync(dibuat.Entity.Id);
        Assert.Equal(new[] { "Validate" }, hasilTercatat!.AvailableActions);
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static RecordSampleRequest Permintaan(Guid pasien, string identifier) => new()
    {
        PatientId = pasien,
        SampleIdentifier = identifier
    };

    private static async Task<Guid> TambahPasienAsync(ApplicationDbContext db)
    {
        var pasien = new MstPatient
        {
            Id = Guid.NewGuid(),
            PatientCode = "PSN-" + Guid.NewGuid().ToString("N")[..8],
            MedicalRecordNumber = "RM-" + Guid.NewGuid().ToString("N")[..8],
            FullName = "Pasien Uji Bank Darah"
        };

        db.Set<MstPatient>().Add(pasien);
        await db.SaveChangesAsync();

        return pasien.Id;
    }

    private static async Task TambahAlasanAsync(ApplicationDbContext db)
    {
        db.Set<MstBloodBankReason>().Add(new MstBloodBankReason
        {
            Id = Guid.NewGuid(),
            ReasonCode = KodeAlasan,
            ReasonText = "Perbedaan hasil golongan darah diselesaikan validator klinis",
            ReasonCategory = BloodBankReasonCategories.PendingReviewResolution,
            IsActive = true
        });

        await db.SaveChangesAsync();
    }

    private static async Task<BbkBloodGroupExam> SampaiHasilTercatatAsync(
        ApplicationDbContext db,
        BbkBloodGroupExamService service,
        BloodType hasil,
        Guid? pasienId = null,
        string identifier = "BD-0001")
    {
        var pasien = pasienId ?? await TambahPasienAsync(db);

        var dibuat = await service.RecordSampleAsync(Permintaan(pasien, identifier), PetugasPengambil);

        var dicatat = await service.RecordResultAsync(
            dibuat.Entity!.Id,
            new RecordResultRequest { AboRhesusResult = hasil },
            Pemeriksa);

        Assert.Equal(BloodGroupExamOutcome.Success, dicatat.Outcome);

        return dicatat.Entity!;
    }

    private static async Task<BbkBloodGroupExam> SampaiTervalidasiAsync(
        ApplicationDbContext db,
        BbkBloodGroupExamService service,
        Guid pasienId,
        string identifier,
        BloodType hasil)
    {
        var exam = await SampaiHasilTercatatAsync(db, service, hasil, pasienId, identifier);

        var divalidasi = await service.ValidateAsync(exam.Id, PetugasBerwenangValidasi);

        Assert.Equal(BloodGroupExamOutcome.Success, divalidasi.Outcome);

        return divalidasi.Entity!;
    }

    private static Task ReloadAsync(ApplicationDbContext db, BbkBloodGroupExam entity)
        => db.Entry(entity).ReloadAsync();

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"blood-group-exam-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
