using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-069</c> — tempat tidur yang ditolak ikut menyebutkan alasannya.
/// </summary>
/// <remarks>
/// <b>Keadaan yang ditiru.</b> Bukti runtime pemilik 9 September 2026: papan pemilihan tempat
/// tidur menuliskan "Tidak lolos kelayakan" pada bed isolasi dan pada seluruh bed sekamar
/// dengan pasien berjenis kelamin berbeda, tanpa satu pun keterangan. Dunia uji di bawah
/// menyusun keadaan yang sama — satu kamar berisi pasien perempuan, satu tempat tidur
/// isolasi, dan satu tempat tidur yang benar-benar layak — lalu membuktikan bahwa alasannya
/// kini ikut terkirim.
///
/// <para>
/// <b>Yang tidak dibuktikan di sini.</b> Jumlah query ke database. Provider InMemory tidak
/// menjalankan satu pun perintah SQL, sehingga menghitungnya di sini tidak membuktikan apa
/// pun. Kriteria 6 dibuktikan terpisah pada project uji SQLite, yang providernya relasional
/// dan perintahnya benar-benar terhitung.
/// </para>
/// </remarks>
public sealed class InpIneligibleBedReasonTests
{
    /// <summary>
    /// Kriteria 1. Tanpa <c>includeIneligible</c>, jawabannya sama persis seperti sebelum
    /// perubahan dan daftar penolakan terkirim kosong — bukan hilang, bukan null.
    /// </summary>
    [Fact]
    public async Task Kriteria1_TanpaIncludeIneligible_JawabanTidakBerubahDanDaftarKosong()
    {
        var dunia = await BuatDuniaPemilihanBedAsync();

        var hasil = await dunia.World.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery { EpisodeId = dunia.EpisodeLakiLaki.Id });

        Assert.Empty(hasil.Ineligible);

        // Hanya tempat tidur di kamar kosong yang lolos; ketiga tempat tidur lain ditolak.
        var lolos = Assert.Single(hasil.Items);
        Assert.Equal(dunia.BedLayak.Id, lolos.BedId);
        Assert.Equal(1, hasil.TotalData);
    }

    /// <summary>
    /// Kriteria 2. Dengan <c>includeIneligible=true</c> dan <c>episodeId</c> terisi, setiap
    /// tempat tidur yang ditolak muncul beserta <b>seluruh</b> aturan yang menolaknya.
    /// </summary>
    [Fact]
    public async Task Kriteria2_DenganIncludeIneligible_SetiapBedDitolakMembawaSeluruhAturannya()
    {
        var dunia = await BuatDuniaPemilihanBedAsync();

        var hasil = await dunia.World.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery
            {
                EpisodeId = dunia.EpisodeLakiLaki.Id,
                IncludeIneligible = true
            });

        Assert.Equal(3, hasil.Ineligible.Count);

        // Tempat tidur sekamar dengan pasien perempuan: aturan 6.
        var sekamar = hasil.Ineligible.Single(x => x.BedId == dunia.BedSekamar.Id);
        Assert.Contains(sekamar.Failures, f => f.RuleNumber == 6 && f.Code == "ROOM_GENDER_MIXED");

        // Tempat tidur isolasi bagi pasien yang tidak membutuhkan isolasi: aturan 8.
        var isolasi = hasil.Ineligible.Single(x => x.BedId == dunia.BedIsolasi.Id);
        Assert.Contains(
            isolasi.Failures,
            f => f.RuleNumber == 8 && f.Code == "ISOLATION_BED_RESERVED");

        // Kode dan nama tempat tidurnya ikut, supaya layar tidak perlu mencarinya lagi.
        Assert.Equal(dunia.BedIsolasi.BedCode, isolasi.BedCode);
        Assert.Equal(dunia.BedIsolasi.BedName, isolasi.BedName);
        Assert.False(string.IsNullOrWhiteSpace(isolasi.RoomName));

        // Tempat tidur yang sedang dihuni pasien perempuan ditolak lebih dari satu aturan
        // sekaligus, dan seluruhnya terkirim — bukan hanya yang pertama.
        var dihuni = hasil.Ineligible.Single(x => x.BedId == dunia.BedDihuni.Id);
        Assert.True(
            dihuni.Failures.Count > 1,
            $"Diharapkan lebih dari satu aturan menolak, terkirim {dihuni.Failures.Count}.");
    }

    /// <summary>
    /// Kriteria 3. Kalimat pada <c>failures[].message</c> identik dengan kalimat yang
    /// dikembalikan penolakan <c>POST /placements</c> untuk tempat tidur dan episode yang
    /// sama.
    /// </summary>
    /// <remarks>
    /// Inilah test yang menangkap kesalahan yang paling mudah terjadi: menyusun kalimat
    /// alasan tersendiri di dalam pencarian, sehingga papan pemilihan dan penolakan
    /// penempatan lama-lama menyimpang. Kalimatnya dibandingkan <b>utuh</b>, bukan hanya
    /// kode aturannya.
    /// </remarks>
    [Fact]
    public async Task Kriteria3_KalimatAlasanIdentikDenganPenolakanPenempatan()
    {
        var dunia = await BuatDuniaPemilihanBedAsync();

        var hasil = await dunia.World.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery
            {
                EpisodeId = dunia.EpisodeLakiLaki.Id,
                IncludeIneligible = true
            });

        foreach (var bedId in new[] { dunia.BedSekamar.Id, dunia.BedIsolasi.Id })
        {
            var dariPencarian = hasil.Ineligible.Single(x => x.BedId == bedId);

            var penempatan = await dunia.World.BedOccupancyService.PlacePatientAsync(
                new PlacePatientRequest
                {
                    EpisodeId = dunia.EpisodeLakiLaki.Id,
                    BedId = bedId
                },
                InpatientEpisodeTestWorld.ActorUserId);

            Assert.NotEqual(InpEpisodeOperationStatus.Success, penempatan.Status);

            Assert.Equal(
                penempatan.Failures.Select(f => f.Message).OrderBy(x => x, StringComparer.Ordinal),
                dariPencarian.Failures.Select(f => f.Message).OrderBy(x => x, StringComparer.Ordinal));

            Assert.Equal(
                penempatan.Failures.Select(f => f.RuleNumber).OrderBy(x => x),
                dariPencarian.Failures.Select(f => f.RuleNumber).OrderBy(x => x));
        }
    }

    /// <summary>
    /// Kriteria 4. Tempat tidur yang lolos tidak pernah muncul pada kedua daftar sekaligus.
    /// </summary>
    [Fact]
    public async Task Kriteria4_BedYangLolosTidakPernahMunculDiKeduaDaftar()
    {
        var dunia = await BuatDuniaPemilihanBedAsync();

        var hasil = await dunia.World.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery
            {
                EpisodeId = dunia.EpisodeLakiLaki.Id,
                IncludeIneligible = true
            });

        var lolos = hasil.Items.Select(x => x.BedId).ToHashSet();
        var ditolak = hasil.Ineligible.Select(x => x.BedId).ToHashSet();

        Assert.Empty(lolos.Intersect(ditolak));

        // Keduanya berasal dari satu pemeriksaan yang sama, sehingga jumlahnya menutup
        // seluruh tempat tidur kandidat tanpa sisa dan tanpa rangkap.
        Assert.Equal(4, lolos.Count + ditolak.Count);
    }

    /// <summary>
    /// Kriteria 5. <c>includeIneligible=true</c> tanpa <c>episodeId</c> menjawab daftar
    /// kosong, bukan alasan sebagian.
    /// </summary>
    /// <remarks>
    /// Tanpa episode, aturan jenis kelamin dan aturan isolasi tidak dapat dinilai sama
    /// sekali. Mengirim alasan seadanya akan membuat petugas membaca sebab yang salah, dan
    /// itu lebih buruk daripada tidak membaca sebab apa pun.
    /// </remarks>
    [Fact]
    public async Task Kriteria5_TanpaEpisodeId_DaftarPenolakanKosongBukanAlasanSebagian()
    {
        var dunia = await BuatDuniaPemilihanBedAsync();

        var hasil = await dunia.World.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery { IncludeIneligible = true });

        Assert.Empty(hasil.Ineligible);
    }

    // =========================================================================
    // Dunia uji
    // =========================================================================

    private sealed record DuniaPemilihanBed(
        InpatientEpisodeTestWorld World,
        InpEpisode EpisodeLakiLaki,
        MstBed BedDihuni,
        MstBed BedSekamar,
        MstBed BedIsolasi,
        MstBed BedLayak);

    /// <summary>
    /// Menyusun keadaan runtime 9 September 2026: satu kamar dihuni pasien perempuan, satu
    /// tempat tidur isolasi, dan satu tempat tidur yang benar-benar layak. Pasien yang sedang
    /// dicarikan tempat tidur adalah laki-laki dan tidak membutuhkan isolasi.
    /// </summary>
    private static async Task<DuniaPemilihanBed> BuatDuniaPemilihanBedAsync()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var kamarPerempuan = await world.AddRoomAsync("Melati 3");
        var bedDihuni = await world.AddBedAsync(kamarPerempuan, "3A");
        var bedSekamar = await world.AddBedAsync(kamarPerempuan, "3B");

        // Pasien bawaan dunia uji perempuan. Ia ditempatkan lebih dulu, sehingga kamarnya
        // menjadi kamar perempuan.
        await world.OpenAndPlaceAsync(bedDihuni);

        var kamarIsolasi = await world.AddRoomAsync("Isolasi 1");
        var bedIsolasi = await world.AddBedAsync(kamarIsolasi, "IS-1", isIsolationBed: true);

        var kamarKosong = await world.AddRoomAsync("Melati 4");
        var bedLayak = await world.AddBedAsync(kamarKosong, "4A");

        var pasienLakiLaki = await world.AddPatientAsync("Bapak Budi", Gender.Male);
        var episode = await world.OpenDraftEpisodeAsync(pasienLakiLaki.Id);

        return new DuniaPemilihanBed(
            world,
            episode,
            bedDihuni,
            bedSekamar,
            bedIsolasi,
            bedLayak);
    }
}
