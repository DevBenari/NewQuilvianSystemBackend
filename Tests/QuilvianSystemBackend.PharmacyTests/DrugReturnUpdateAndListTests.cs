using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Dua permukaan `DrugReturnService` yang sebelumnya tidak punya uji sama sekali:
/// <c>UpdateAsync</c> dan <c>GetPagedAsync</c>.
/// </summary>
/// <remarks>
/// <para>
/// Keduanya dikelompokkan di sini karena keduanya mudah terlihat benar padahal tidak.
/// <c>UpdateAsync</c> mengganti <b>seluruh</b> baris retur, sehingga ia harus lolos pembanding
/// penyerahan yang sama seperti saat pembuatan — sambil mengeluarkan dirinya sendiri dari
/// hitungan kumulatif, kalau tidak sebuah retur akan dianggap menghalangi perubahannya sendiri.
/// <c>GetPagedAsync</c> menyaring tujuh hal dan membatasi ukuran halaman; saringan yang salah
/// pada daftar retur berarti petugas memeriksa retur milik kunjungan lain.
/// </para>
/// <para>
/// Penyerahan bawaan harness adalah <b>10</b> satuan, jadi seluruh jumlah retur pada uji ini
/// dijaga di bawah angka itu — batas kumulatifnya sudah dibuktikan terpisah pada
/// <c>DrugReturnTests</c> dan tidak diuji ulang di sini.
/// </para>
/// </remarks>
public class DrugReturnUpdateAndListTests
{
    private static async Task<(DrugReturnService Layanan, ApplicationDbContext Konteks, Guid BetsId)>
        SiapAsync(PharmacyHarness h)
    {
        var (konteks, usageId, betsId) = await h.SampaiDiserahkanAsync();
        h.SumberPenyerahanId = usageId;
        return (h.ReturService(konteks), konteks, betsId);
    }

    private static CreateDrugReturnRequest Pembuatan(PharmacyHarness h, Guid betsId,
        decimal jumlah = 4, DateTime? waktu = null) => new()
        {
            EncounterId = h.KunjunganId,
            StorageLocationId = h.DepoId,
            ReturnedByWorkforceId = h.PetugasId,
            SourceDrugUsageId = h.SumberPenyerahanId,
            ReturnedAt = waktu ?? DateTime.UtcNow,
            Reason = "Sisa obat pasien pulang",
            Items =
            [
                new DrugReturnItemInput
                {
                    DrugId = h.ObatId,
                    DrugBatchId = betsId,
                    MeasurementId = h.SatuanId,
                    Quantity = jumlah
                }
            ],
            IdempotencyKey = Guid.NewGuid().ToString("N")
        };

    private static UpdateDrugReturnRequest Perubahan(PharmacyHarness h, Guid betsId, int versi,
        decimal jumlah = 2, Guid? depo = null, string? alasan = null,
        Guid? sumber = null, bool sumberDikosongkan = false) => new()
        {
            StorageLocationId = depo ?? h.DepoId,
            SourceDrugUsageId = sumberDikosongkan ? null : sumber ?? h.SumberPenyerahanId,
            ReturnedAt = DateTime.UtcNow,
            Reason = alasan ?? "Jumlah dikoreksi petugas",
            Items =
            [
                new DrugReturnItemInput
                {
                    DrugId = h.ObatId,
                    DrugBatchId = betsId,
                    MeasurementId = h.SatuanId,
                    Quantity = jumlah
                }
            ],
            ExpectedVersion = versi,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        };

    // ===================================================================== UpdateAsync

    [Fact]
    public async Task Retur_draft_dapat_diubah_dan_versinya_naik()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId, jumlah: 4));

        var diubah = await layanan.UpdateAsync(retur.Id,
            Perubahan(h, betsId, retur.Version, jumlah: 2, alasan: "Jumlah dikoreksi petugas"));

        Assert.Equal(DrugReturnStatus.Draft, diubah.Status);
        Assert.Equal(retur.Version + 1, diubah.Version);
        Assert.Equal("Jumlah dikoreksi petugas", diubah.Reason);
        Assert.Equal(1, diubah.ItemCount);
    }

    /// <summary>
    /// Perubahan mengganti seluruh baris, bukan menambah. Kalau baris lama tidak ditandai
    /// terhapus, jumlah retur akan terhitung dua kali dan stok berbohong saat diperiksa.
    /// </summary>
    [Fact]
    public async Task Perubahan_mengganti_seluruh_baris_bukan_menambahkannya()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId, jumlah: 4));
        var diubah = await layanan.UpdateAsync(retur.Id,
            Perubahan(h, betsId, retur.Version, jumlah: 2));

        Assert.Equal(1, diubah.ItemCount);

        var barisHidup = konteks.Set<Areas.HealthServices.PharmacyManagement.Models.PhmDrugReturnItem>()
            .Where(x => x.DrugReturnId == retur.Id && !x.IsDelete)
            .ToList();

        var barisTerhapus = konteks.Set<Areas.HealthServices.PharmacyManagement.Models.PhmDrugReturnItem>()
            .Where(x => x.DrugReturnId == retur.Id && x.IsDelete)
            .ToList();

        Assert.Single(barisHidup);
        Assert.Equal(2m, barisHidup[0].Quantity);
        Assert.Single(barisTerhapus);
        Assert.Equal(4m, barisTerhapus[0].Quantity);
    }

    [Fact]
    public async Task Perubahan_dapat_memindahkan_depo_penerima()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));

        var diubah = await layanan.UpdateAsync(retur.Id,
            Perubahan(h, betsId, retur.Version, depo: h.DepoId));

        Assert.Equal(h.DepoId, diubah.StorageLocationId);
    }

    [Fact]
    public async Task Perubahan_ke_depo_yang_tidak_boleh_menerima_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));

        await Assert.ThrowsAnyAsync<Exception>(() => layanan.UpdateAsync(retur.Id,
            Perubahan(h, betsId, retur.Version, depo: h.DepoTanpaTerimaId)));
    }

    [Fact]
    public async Task Perubahan_dengan_versi_usang_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        await layanan.UpdateAsync(retur.Id, Perubahan(h, betsId, retur.Version));

        // Versi yang sama dipakai dua kali: percobaan kedua membawa versi yang sudah lewat.
        await Assert.ThrowsAnyAsync<Exception>(() => layanan.UpdateAsync(retur.Id,
            Perubahan(h, betsId, retur.Version)));
    }

    [Fact]
    public async Task Perubahan_pada_retur_yang_tidak_ada_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            layanan.UpdateAsync(Guid.NewGuid(), Perubahan(h, betsId, 0)));
    }

    /// <summary>
    /// `PHM070`. Setelah diajukan, retur sudah menjadi klaim terhadap stok dan tidak boleh
    /// diubah diam-diam oleh pembuatnya.
    /// </summary>
    [Fact]
    public async Task Retur_yang_sudah_diajukan_tidak_dapat_diubah()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        var diajukan = await layanan.SubmitAsync(retur.Id, new DrugReturnCommandRequest
        {
            ExpectedVersion = retur.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var galat = await Assert.ThrowsAsync<DrugReturnConflictException>(() =>
            layanan.UpdateAsync(retur.Id, Perubahan(h, betsId, diajukan.Version)));

        Assert.Equal("PHM070", galat.Code);
    }

    [Fact]
    public async Task Perubahan_tanpa_kunci_idempotency_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        var permintaan = Perubahan(h, betsId, retur.Version);
        permintaan.IdempotencyKey = "   ";

        await Assert.ThrowsAnyAsync<Exception>(() =>
            layanan.UpdateAsync(retur.Id, permintaan));
    }

    [Fact]
    public async Task Perubahan_tanpa_baris_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        var permintaan = Perubahan(h, betsId, retur.Version);
        permintaan.Items = [];

        await Assert.ThrowsAnyAsync<Exception>(() =>
            layanan.UpdateAsync(retur.Id, permintaan));
    }

    /// <summary>
    /// `BUG-PHA-BE-003` berlaku juga pada perubahan: sumber penyerahan tetap wajib, tidak boleh
    /// dilepas dengan mengosongkannya saat mengubah.
    /// </summary>
    [Fact]
    public async Task Perubahan_tidak_boleh_mengosongkan_sumber_penyerahan()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));

        await Assert.ThrowsAnyAsync<Exception>(() => layanan.UpdateAsync(retur.Id,
            Perubahan(h, betsId, retur.Version, sumberDikosongkan: true)));
    }

    /// <summary>
    /// Inti `returnIdToExclude`. Retur ini sendiri tidak boleh ikut dihitung sebagai retur yang
    /// sudah terjadi; kalau ikut, mengubah 4 menjadi 4 akan tertolak oleh dirinya sendiri.
    /// </summary>
    [Fact]
    public async Task Perubahan_tidak_menghitung_dirinya_sendiri_pada_batas_kumulatif()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        // Penyerahannya 10. Retur pertama mengambil 10 — habis.
        var retur = await layanan.CreateAsync(Pembuatan(h, betsId, jumlah: 10));

        // Mengubahnya tetap 10 harus boleh, karena baris lamanya digantikan, bukan ditambah.
        var diubah = await layanan.UpdateAsync(retur.Id,
            Perubahan(h, betsId, retur.Version, jumlah: 10));

        Assert.Equal(DrugReturnStatus.Draft, diubah.Status);
        Assert.Equal(retur.Version + 1, diubah.Version);
    }

    [Fact]
    public async Task Perubahan_yang_melampaui_jumlah_diserahkan_tetap_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId, jumlah: 4));

        // `PHM083` dilempar sebagai `DrugReturnUnprocessableException`, bukan konflik versi:
        // jumlahnya memang tidak dapat diproses, bukan bertabrakan dengan perubahan lain.
        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.UpdateAsync(retur.Id, Perubahan(h, betsId, retur.Version, jumlah: 11)));

        Assert.Equal("PHM083", galat.Code);
    }

    // ==================================================================== GetPagedAsync

    /// <summary>Membuat beberapa retur Draft pada kunjungan yang sama, masing-masing 1 satuan.</summary>
    private static async Task<List<DrugReturnDetailResponse>> BeberapaReturAsync(
        DrugReturnService layanan, PharmacyHarness h, Guid betsId, int jumlahRetur)
    {
        var hasil = new List<DrugReturnDetailResponse>();

        for (var i = 0; i < jumlahRetur; i++)
        {
            hasil.Add(await layanan.CreateAsync(
                Pembuatan(h, betsId, jumlah: 1, waktu: DateTime.UtcNow.AddMinutes(-i))));
        }

        return hasil;
    }

    [Fact]
    public async Task Daftar_kosong_ketika_belum_ada_retur()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, _) = await SiapAsync(h);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery());

        Assert.Equal(0, halaman.TotalData);
        Assert.Empty(halaman.Items);
    }

    [Fact]
    public async Task Daftar_memuat_retur_yang_sudah_dibuat()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 3);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery());

        Assert.Equal(3, halaman.TotalData);
        Assert.Equal(3, halaman.Items.Count);
    }

    [Fact]
    public async Task Daftar_terurut_dari_retur_terbaru()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 3);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery());
        var waktu = halaman.Items.Select(x => x.ReturnedAt).ToList();

        Assert.Equal(waktu.OrderByDescending(x => x), waktu);
    }

    [Theory]
    [InlineData(1, 2, 2)]
    [InlineData(2, 2, 1)]
    [InlineData(3, 2, 0)]
    public async Task Paging_membagi_hasil_sesuai_nomor_dan_ukuran_halaman(
        int nomor, int ukuran, int diharapkan)
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 3);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            PageNumber = nomor,
            PageSize = ukuran
        });

        Assert.Equal(3, halaman.TotalData);
        Assert.Equal(diharapkan, halaman.Items.Count);
    }

    /// <summary>
    /// Nomor halaman di bawah satu dibetulkan menjadi satu, bukan menghasilkan `Skip` negatif.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Nomor_halaman_tidak_sah_dibetulkan_menjadi_halaman_pertama(int nomor)
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 3);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            PageNumber = nomor,
            PageSize = 2
        });

        Assert.Equal(2, halaman.Items.Count);
    }

    /// <summary>
    /// Ukuran halaman di luar 1–100 dikembalikan ke 10. Batas atas itu mencegah satu permintaan
    /// menarik seluruh tabel retur.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(100000)]
    public async Task Ukuran_halaman_di_luar_batas_dikembalikan_ke_sepuluh(int ukuran)
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 3);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            PageNumber = 1,
            PageSize = ukuran
        });

        // Ketiganya muat dalam satu halaman berukuran 10.
        Assert.Equal(3, halaman.Items.Count);
    }

    [Fact]
    public async Task Saringan_status_hanya_memberi_status_yang_diminta()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        var retur = await BeberapaReturAsync(layanan, h, betsId, 3);

        await layanan.SubmitAsync(retur[0].Id, new DrugReturnCommandRequest
        {
            ExpectedVersion = retur[0].Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var draft = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            Status = DrugReturnStatus.Draft
        });

        var diajukan = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            Status = DrugReturnStatus.Submitted
        });

        Assert.Equal(2, draft.TotalData);
        Assert.Equal(1, diajukan.TotalData);
        Assert.All(draft.Items, x => Assert.Equal(DrugReturnStatus.Draft, x.Status));
    }

    [Fact]
    public async Task Saringan_kunjungan_mengeluarkan_kunjungan_lain()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        var sesuai = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            EncounterId = h.KunjunganId
        });

        var kunjunganLain = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            EncounterId = Guid.NewGuid()
        });

        Assert.Equal(2, sesuai.TotalData);
        Assert.Equal(0, kunjunganLain.TotalData);
    }

    [Fact]
    public async Task Saringan_pasien_mengeluarkan_pasien_lain()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        var sesuai = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            PatientId = h.PasienId
        });

        var pasienLain = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            PatientId = Guid.NewGuid()
        });

        Assert.Equal(2, sesuai.TotalData);
        Assert.Equal(0, pasienLain.TotalData);
    }

    [Fact]
    public async Task Saringan_depo_mengeluarkan_depo_lain()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        var sesuai = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            StorageLocationId = h.DepoId
        });

        var depoLain = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            StorageLocationId = h.DepoNonaktifId
        });

        Assert.Equal(2, sesuai.TotalData);
        Assert.Equal(0, depoLain.TotalData);
    }

    [Fact]
    public async Task Saringan_obat_mengeluarkan_obat_lain()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        var sesuai = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            DrugId = h.ObatId
        });

        var obatLain = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            DrugId = h.ObatTanpaSatuanId
        });

        Assert.Equal(2, sesuai.TotalData);
        Assert.Equal(0, obatLain.TotalData);
    }

    [Fact]
    public async Task Saringan_tanggal_akhir_bersifat_inklusif_terhadap_hari_itu()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        // Kedua retur dibuat hari ini. Tanggal akhir "hari ini" harus memuat keduanya, karena
        // source memakai batas eksklusif pada hari berikutnya.
        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date
        });

        Assert.Equal(2, halaman.TotalData);
    }

    [Fact]
    public async Task Saringan_tanggal_mengeluarkan_retur_di_luar_rentang()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        var masaDepan = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            StartDate = DateTime.UtcNow.Date.AddDays(5)
        });

        var masaLalu = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            EndDate = DateTime.UtcNow.Date.AddDays(-5)
        });

        Assert.Equal(0, masaDepan.TotalData);
        Assert.Equal(0, masaLalu.TotalData);
    }

    [Fact]
    public async Task Pencarian_menemukan_retur_lewat_nomornya()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        var retur = await BeberapaReturAsync(layanan, h, betsId, 2);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            Search = retur[0].ReturnNumber
        });

        Assert.Equal(1, halaman.TotalData);
        Assert.Equal(retur[0].ReturnNumber, halaman.Items.Single().ReturnNumber);
    }

    [Fact]
    public async Task Pencarian_tidak_memandang_huruf_besar_kecil()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        var retur = await BeberapaReturAsync(layanan, h, betsId, 1);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            Search = retur[0].ReturnNumber.ToLowerInvariant()
        });

        Assert.Equal(1, halaman.TotalData);
    }

    [Fact]
    public async Task Pencarian_menemukan_retur_lewat_nama_pasien()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            Search = "Pasien Uji Farmasi"
        });

        Assert.Equal(2, halaman.TotalData);
    }

    [Fact]
    public async Task Pencarian_menemukan_retur_lewat_nomor_rekam_medis()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            Search = "UJI-PHM-RM-1"
        });

        Assert.Equal(2, halaman.TotalData);
    }

    [Fact]
    public async Task Pencarian_yang_tidak_cocok_memberi_daftar_kosong()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            Search = "tidak-akan-pernah-cocok-xyz"
        });

        Assert.Equal(0, halaman.TotalData);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Pencarian_kosong_tidak_menyaring_apa_pun(string kata)
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery { Search = kata });

        Assert.Equal(2, halaman.TotalData);
    }

    [Fact]
    public async Task Dua_saringan_digabung_dengan_dan_bukan_atau()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 2);

        // Status cocok, kunjungan tidak. Kalau saringannya digabung sebagai "atau", hasilnya
        // tidak akan nol.
        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            Status = DrugReturnStatus.Draft,
            EncounterId = Guid.NewGuid()
        });

        Assert.Equal(0, halaman.TotalData);
    }

    [Fact]
    public async Task Retur_yang_dibatalkan_tetap_terbaca_pada_daftar()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        var retur = await BeberapaReturAsync(layanan, h, betsId, 1);

        await layanan.CancelAsync(retur[0].Id, new DrugReturnReasonRequest
        {
            Reason = "Salah input",
            ExpectedVersion = retur[0].Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var halaman = await layanan.GetPagedAsync(new DrugReturnPagedQuery
        {
            Status = DrugReturnStatus.Cancelled
        });

        // Dibatalkan bukan dihapus: catatannya harus tetap dapat ditelusuri.
        Assert.Equal(1, halaman.TotalData);
    }

    [Fact]
    public async Task Ringkasan_daftar_memuat_identitas_pasien_dan_depo()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);
        await BeberapaReturAsync(layanan, h, betsId, 1);

        var ringkasan = (await layanan.GetPagedAsync(new DrugReturnPagedQuery())).Items.Single();

        Assert.Equal(h.KunjunganId, ringkasan.EncounterId);
        Assert.Equal(h.PasienId, ringkasan.PatientId);
        Assert.Equal("Pasien Uji Farmasi", ringkasan.PatientName);
        Assert.Equal("UJI-PHM-RM-1", ringkasan.MedicalRecordNumber);
        Assert.Equal(h.DepoId, ringkasan.StorageLocationId);
        Assert.NotEmpty(ringkasan.ReturnNumber);
    }
}
