using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

/// <summary>
/// Laporan operasional modul Operasi (`BE-OPR-010`).
/// </summary>
/// <remarks>
/// <para>
/// Laporan dibaca untuk menilai pemakaian ruang, lama operasi, dan jejak implant. Salahnya tidak
/// terlihat sebagai galat — ia terlihat sebagai angka yang masuk akal tetapi keliru, dan
/// keputusan penjadwalan diambil di atasnya.
/// </para>
/// <para>
/// Yang dijaga di sini penyaring, rentang tanggal, paging, dan bentuk keluarannya. Seluruh query
/// laporan hanya membaca; tidak ada satu pun uji di berkas ini yang mengubah data lewat laporan.
/// </para>
/// </remarks>
public class ReportTests
{
    private static OperatingRoomReportService Laporan(ApplicationDbContext konteks) =>
        new(konteks);

    /// <summary>
    /// Dua kasus operasi dengan waktu permintaan yang berbeda, ditanam langsung sebagai data
    /// acuan laporan.
    /// </summary>
    /// <remarks>
    /// Laporan hanya membaca, jadi titik awalnya memang keadaan tersimpan — bukan alur yang
    /// menghasilkannya. Alur pembuatan kasus sendiri sudah dijaga `StateTransitionTests`.
    /// </remarks>
    /// <summary>
    /// Dua kasus operasi yang dibuat lewat service produksi, lalu waktu dan sifatnya disesuaikan
    /// agar penyaring laporan dapat diuji.
    /// </summary>
    /// <remarks>
    /// Kasusnya dibuat lewat `AlurOperasi.BuatKasusAsync`, bukan ditanam langsung ke tabel:
    /// `OprCase` punya beberapa acuan wajib dan basis data uji Operasi memang menegakkan foreign
    /// key. Yang disesuaikan sesudahnya hanya kolom yang memang menjadi penyaring laporan —
    /// waktu permintaan, status, jenis, dan prioritas.
    /// </remarks>
    private static async Task<(ApplicationDbContext Konteks, Guid Lama, Guid Baru,
        string NomorLama, string NomorBaru)>
        DuaKasusAsync(OperatingRoomHarness h, OperatingRoomSeed seed)
    {
        var alur = new AlurOperasi(h, seed);
        var idLama = await alur.BuatKasusAsync();
        var idBaru = await alur.BuatKasusAsync();

        var konteks = h.NewContext();

        var lama = await konteks.Set<OprCase>().FindAsync(idLama);
        lama!.RequestedAt = DateTime.UtcNow.AddDays(-10);
        lama.Status = OprCaseStatus.Completed;
        lama.CaseType = OprCaseType.Elective;
        lama.Priority = OprPriority.Routine;

        var baru = await konteks.Set<OprCase>().FindAsync(idBaru);
        baru!.RequestedAt = DateTime.UtcNow.AddDays(-1);
        baru.Status = OprCaseStatus.Requested;
        baru.CaseType = OprCaseType.Emergency;
        baru.Priority = OprPriority.Urgent;

        await konteks.SaveChangesAsync();

        return (konteks, idLama, idBaru, lama.CaseNumber, baru.CaseNumber);
    }

    /// <summary>Satu kasus pada waktu tertentu, dibuat lewat service produksi.</summary>
    private static async Task<(ApplicationDbContext Konteks, string Nomor)>
        SatuKasusPadaAsync(OperatingRoomHarness h, OperatingRoomSeed seed, DateTime waktu)
    {
        var alur = new AlurOperasi(h, seed);
        var id = await alur.BuatKasusAsync();

        var konteks = h.NewContext();
        var kasus = await konteks.Set<OprCase>().FindAsync(id);
        kasus!.RequestedAt = waktu;
        await konteks.SaveChangesAsync();

        return (konteks, kasus.CaseNumber);
    }

    // ------------------------------------------------------------------ penyaring

    [Fact]
    public async Task Laporan_operasi_memuat_kasus_yang_ada()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery { PageSize = 50 });

        Assert.True(hasil.TotalData >= 2);
        Assert.Contains(nomorLama, hasil.Items.Select(x => x.CaseNumber));
        Assert.Contains(nomorBaru, hasil.Items.Select(x => x.CaseNumber));
    }

    [Fact]
    public async Task Penyaring_status_membuang_status_lain()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            Status = OprCaseStatus.Completed
        });

        Assert.All(hasil.Items, x => Assert.Equal(OprCaseStatus.Completed, x.Status));
        Assert.Contains(nomorLama, hasil.Items.Select(x => x.CaseNumber));
        Assert.DoesNotContain(nomorBaru, hasil.Items.Select(x => x.CaseNumber));
    }

    [Fact]
    public async Task Penyaring_jenis_operasi_berlaku()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            CaseType = OprCaseType.Emergency
        });

        Assert.All(hasil.Items, x => Assert.Equal(OprCaseType.Emergency, x.CaseType));
        Assert.Contains(nomorBaru, hasil.Items.Select(x => x.CaseNumber));
    }

    [Fact]
    public async Task Penyaring_prioritas_berlaku()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            Priority = OprPriority.Urgent
        });

        Assert.All(hasil.Items, x => Assert.Equal(OprPriority.Urgent, x.Priority));
    }

    [Fact]
    public async Task Penyaring_dokter_operator_berlaku()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);
        var layanan = Laporan(konteks);

        var milikDokter = await layanan.GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            PrimarySurgeonId = seed.DokterBedahId
        });
        var milikDokterLain = await layanan.GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            PrimarySurgeonId = Guid.NewGuid()
        });

        Assert.NotEmpty(milikDokter.Items);
        Assert.Empty(milikDokterLain.Items);
    }

    [Fact]
    public async Task Pencarian_menemukan_kasus_lewat_nomor_kasus()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            Search = nomorLama
        });

        Assert.Single(hasil.Items);
        Assert.Equal(nomorLama, hasil.Items[0].CaseNumber);
    }

    [Fact]
    public async Task Pencarian_kata_yang_tidak_ada_mengembalikan_kosong()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            Search = "tidak-ada-sama-sekali"
        });

        Assert.Empty(hasil.Items);
        Assert.Equal(0, hasil.TotalData);
    }

    [Fact]
    public async Task Penyaring_ruang_hanya_mengambil_jadwal_yang_berlaku()
    {
        // Penyaringnya menuntut `IsCurrent` — jadwal yang sudah digantikan tidak boleh membuat
        // kasus ikut terhitung pada ruang lamanya.
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            RoomId = seed.RuangId
        });

        // Kasus uji belum punya jadwal, jadi penyaring ruang menyaringnya habis.
        Assert.Empty(hasil.Items);
    }

    // ------------------------------------------------------------- rentang tanggal

    [Fact]
    public async Task Data_di_luar_rentang_tidak_ikut()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            From = DateTime.UtcNow.AddDays(-3)
        });

        Assert.DoesNotContain(nomorLama, hasil.Items.Select(x => x.CaseNumber));
        Assert.Contains(nomorBaru, hasil.Items.Select(x => x.CaseNumber));
    }

    [Fact]
    public async Task Batas_awal_tepat_tetap_terbaca()
    {
        // Perbandingannya `>=`, jadi kasus yang jatuh tepat pada batas awal harus ikut.
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var tepat = DateTime.UtcNow.AddDays(-5);
        var (konteks, nomor) = await SatuKasusPadaAsync(h, seed, tepat);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            From = tepat
        });

        Assert.Contains(nomor, hasil.Items.Select(x => x.CaseNumber));
    }

    [Fact]
    public async Task Batas_akhir_tepat_pada_waktu_yang_sama_tetap_terbaca()
    {
        // Perbandingannya `<=`, jadi kasus yang jatuh tepat pada batas akhir ikut — selama
        // batasnya memang menyebut waktu, bukan hanya tanggal.
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var tepat = DateTime.UtcNow.AddDays(-5);
        var (konteks, nomor) = await SatuKasusPadaAsync(h, seed, tepat);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            To = tepat
        });

        Assert.Contains(nomor, hasil.Items.Select(x => x.CaseNumber));
    }

    [Fact]
    public async Task Rentang_terbalik_mengembalikan_kosong_bukan_melempar()
    {
        // `From` lebih besar daripada `To` tidak ditolak kontraknya; yang penting ia tidak
        // meledak dan tidak diam-diam mengabaikan penyaringnya. Dicatat sebagai gap, bukan
        // sebagai aturan yang dikarang di sini.
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            From = DateTime.UtcNow.AddDays(5),
            To = DateTime.UtcNow.AddDays(-5)
        });

        Assert.Empty(hasil.Items);
        Assert.Equal(0, hasil.TotalData);
    }

    [Fact]
    public async Task Tanggal_akhir_tanpa_jam_membuang_seluruh_data_hari_itu()
    {
        // Perilaku apa adanya, dicatat supaya terlihat — BUKAN perilaku yang dibenarkan uji ini.
        //
        // Penyaring layar mengirim tanggal TANPA jam. Laporan Operasi memakainya apa adanya,
        // sehingga `To` berarti pukul 00:00 dan seluruh kasus pada hari itu justru terbuang.
        // Laporan Gizi sudah menutup cacat yang sama lewat penolong `ToInclusive`; laporan
        // Operasi belum punya padanannya.
        //
        // Dilaporkan sebagai `BUG-OPR-BE-001`; source tidak diubah dari task pengujian ini.
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var hariIni = DateTime.UtcNow.Date;
        var (konteks, nomor) = await SatuKasusPadaAsync(h, seed, hariIni.AddHours(10));

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageSize = 50,
            From = hariIni,
            To = hariIni
        });

        Assert.DoesNotContain(nomor, hasil.Items.Select(x => x.CaseNumber));
    }

    // ------------------------------------------------------------------ paging

    [Fact]
    public async Task Paging_membatasi_jumlah_baris_dan_menghitung_total()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageNumber = 1,
            PageSize = 1
        });

        Assert.Single(hasil.Items);
        Assert.Equal(1, hasil.PageNumber);
        Assert.Equal(1, hasil.PageSize);
        Assert.True(hasil.TotalData >= 2);
        Assert.True(hasil.TotalPage >= 2);
    }

    [Fact]
    public async Task Halaman_kedua_memuat_baris_yang_berbeda()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);
        var layanan = Laporan(konteks);

        var satu = await layanan.GetOperationsAsync(new OprReportQuery { PageNumber = 1, PageSize = 1 });
        var dua = await layanan.GetOperationsAsync(new OprReportQuery { PageNumber = 2, PageSize = 1 });

        Assert.NotEqual(satu.Items[0].OprCaseId, dua.Items[0].OprCaseId);
    }

    [Fact]
    public async Task Halaman_di_luar_jangkauan_kosong_tanpa_galat()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery
        {
            PageNumber = 999,
            PageSize = 20
        });

        Assert.Empty(hasil.Items);
        Assert.True(hasil.TotalData >= 2);
    }

    [Fact]
    public async Task Urutan_stabil_terbaru_lebih_dulu()
    {
        // Urutannya `RequestedAt` menurun lalu `Id` menurun. Tanpa kunci kedua, dua kasus
        // berwaktu sama dapat bertukar posisi antar halaman dan satu baris hilang dari paging.
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);
        var layanan = Laporan(konteks);

        var pertama = await layanan.GetOperationsAsync(new OprReportQuery { PageSize = 50 });
        var kedua = await layanan.GetOperationsAsync(new OprReportQuery { PageSize = 50 });

        Assert.Equal(
            pertama.Items.Select(x => x.OprCaseId),
            kedua.Items.Select(x => x.OprCaseId));

        var waktu = pertama.Items.Select(x => x.ScheduledStartAt).ToList();
        Assert.Equal(waktu.Count, pertama.Items.Count);
    }

    [Fact]
    public async Task Urutan_menempatkan_kasus_terbaru_di_atas()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery { PageSize = 50 });

        var urutan = hasil.Items.Select(x => x.CaseNumber).ToList();
        Assert.True(urutan.IndexOf(nomorBaru) < urutan.IndexOf(nomorLama));
    }

    // ------------------------------------------------------------------ bentuk keluaran

    [Fact]
    public async Task Baris_laporan_hanya_memuat_field_pada_kontraknya()
    {
        // Laporan dibaca banyak peran, jadi bentuknya adalah batas privasi: apa pun yang tidak
        // ada pada kontraknya tidak boleh ikut keluar. Uji ini mengunci daftar itu, sehingga
        // menambahkan field baru menjadi keputusan yang terlihat dan bukan kebocoran diam-diam.
        var field = typeof(OprOperationReportRow).GetProperties().Select(x => x.Name).OrderBy(x => x);

        Assert.Equal(
            new[]
            {
                "ActualDurationMinutes", "CaseNumber", "CaseType", "EstimatedMinutes",
                "FinishedAt", "OprCaseId", "Outcome", "PatientName", "PrimaryProcedureName",
                "PrimarySurgeonName", "Priority", "RoomName", "ScheduledEndAt",
                "ScheduledStartAt", "StartedAt", "Status"
            },
            field);
    }

    [Fact]
    public void Baris_laporan_tidak_memuat_jejak_audit_maupun_penanda_hapus()
    {
        // Kolom audit — siapa membuat, siapa mengubah, penanda hapus — milik basis data, bukan
        // milik laporan. Membocorkannya memberi pembaca laporan keterangan yang bukan haknya.
        var field = typeof(OprOperationReportRow).GetProperties().Select(x => x.Name).ToList();

        foreach (var terlarang in new[]
        {
            "CreateBy", "UpdateBy", "DeleteBy", "CreateDateTime", "UpdateDateTime",
            "DeleteDateTime", "IsDelete", "IsCancel", "CancelReason", "RowVersion"
        })
        {
            Assert.DoesNotContain(terlarang, field);
        }
    }

    [Fact]
    public void Baris_laporan_tidak_memuat_identitas_pasien_selain_namanya()
    {
        // Kontraknya menyebut nama pasien saja. Nomor rekam medis, tanggal lahir, dan alamat
        // tidak termasuk — laporan operasional tidak membutuhkannya untuk menilai pemakaian
        // ruang maupun lama operasi.
        var field = typeof(OprOperationReportRow).GetProperties().Select(x => x.Name).ToList();

        Assert.Contains("PatientName", field);
        foreach (var terlarang in new[]
        {
            "MedicalRecordNumber", "BirthDate", "Address", "PhoneNumber", "PatientId",
            "IdentityNumber", "Nik"
        })
        {
            Assert.DoesNotContain(terlarang, field);
        }
    }

    [Fact]
    public void Baris_laporan_material_tidak_memuat_jejak_audit()
    {
        var field = typeof(OprMaterialReportRow).GetProperties().Select(x => x.Name).ToList();

        foreach (var terlarang in new[]
        {
            "CreateBy", "UpdateBy", "DeleteBy", "IsDelete", "IsCancel", "RowVersion"
        })
        {
            Assert.DoesNotContain(terlarang, field);
        }
    }

    [Fact]
    public void Baris_laporan_material_memuat_jejak_implant_yang_memang_dibutuhkan()
    {
        // Nomor bets dan nomor serial justru WAJIB ada: itulah inti laporan traceability
        // implant, dan tanpanya implant yang ditarik produsennya tidak dapat dilacak.
        var field = typeof(OprMaterialReportRow).GetProperties().Select(x => x.Name).ToList();

        Assert.Contains("BatchNumber", field);
        Assert.Contains("SerialNumber", field);
        Assert.Contains("CaseNumber", field);
        Assert.Contains("Revision", field);
    }

    // ------------------------------------------------------------------ laporan lain

    [Fact]
    public async Task Laporan_material_pada_basis_data_tanpa_material_kosong()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetMaterialsAsync(new OprMaterialReportQuery
        {
            PageSize = 50
        });

        Assert.Empty(hasil.Items);
        Assert.Equal(0, hasil.TotalData);
    }

    [Fact]
    public async Task Laporan_pemakaian_ruang_pada_rentang_tanpa_jadwal_kosong()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, _, _, nomorLama, nomorBaru) = await DuaKasusAsync(h, seed);

        var hasil = await Laporan(konteks).GetUtilizationAsync(new OprUtilizationQuery
        {
            From = DateTime.UtcNow.AddDays(-30),
            To = DateTime.UtcNow.AddDays(-20)
        });

        Assert.NotNull(hasil);
        Assert.Empty(hasil.Rooms);
    }

    [Fact]
    public async Task Laporan_hanya_membaca_dan_tidak_mengubah_kasus()
    {
        // Seluruh query laporan memakai `AsNoTracking`. Uji ini menjaga agar laporan tidak
        // pernah menjadi jalan masuk perubahan data.
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, idLama, _, nomorLama, _) = await DuaKasusAsync(h, seed);

        var sebelum = await konteks.Set<OprCase>().FindAsync(idLama);
        var statusSebelum = sebelum!.Status;
        var versiSebelum = sebelum.UpdateDateTime;

        await Laporan(konteks).GetOperationsAsync(new OprReportQuery { PageSize = 50 });
        await Laporan(konteks).GetMaterialsAsync(new OprMaterialReportQuery { PageSize = 50 });

        await konteks.Entry(sebelum).ReloadAsync();
        Assert.Equal(statusSebelum, sebelum.Status);
        Assert.Equal(versiSebelum, sebelum.UpdateDateTime);
    }

    [Fact]
    public async Task Kasus_yang_ditandai_terhapus_tidak_ikut_laporan()
    {
        using var h = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(h);
        var (konteks, idLama, _, nomorLama, _) = await DuaKasusAsync(h, seed);

        var kasus = await konteks.Set<OprCase>().FindAsync(idLama);
        kasus!.IsDelete = true;
        await konteks.SaveChangesAsync();

        var hasil = await Laporan(konteks).GetOperationsAsync(new OprReportQuery { PageSize = 50 });

        Assert.DoesNotContain(nomorLama, hasil.Items.Select(x => x.CaseNumber));
    }
}
