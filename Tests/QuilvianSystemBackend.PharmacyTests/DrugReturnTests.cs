using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Retur obat: <c>Draft → Submitted → Verified / Rejected</c>, dengan <c>Cancelled</c> dari dua
/// keadaan awal.
/// </summary>
/// <remarks>
/// <para>
/// Retur adalah satu-satunya jalan obat kembali ke stok setelah diserahkan, jadi ia menyentuh
/// dua hal yang tidak boleh keliru sekaligus: saldo stok dan catatan apa yang terjadi pada obat
/// pasien. Barang yang masuk kembali tanpa diperiksa akan diserahkan lagi ke pasien lain; barang
/// yang ditolak tetapi tetap menambah saldo membuat stok berbohong.
/// </para>
/// <para>
/// Retur di source ini mengacu pada <b>batch obat</b>, bukan pada baris penyerahan. Satu akibatnya
/// dicatat terpisah pada uji di bawah.
/// </para>
/// </remarks>
public class DrugReturnTests
{
    private static CreateDrugReturnRequest Pembuatan(PharmacyHarness h, Guid betsId,
        decimal jumlah = 4, Guid? depo = null, Guid? petugas = null, string? kunci = null) => new()
        {
            EncounterId = h.KunjunganId,
            StorageLocationId = depo ?? h.DepoId,
            ReturnedByWorkforceId = petugas ?? h.PetugasId,
            ReturnedAt = DateTime.UtcNow,
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
            IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N")
        };

    private static DrugReturnCommandRequest Perintah(int versi, string? kunci = null) => new()
    {
        ExpectedVersion = versi,
        IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N")
    };

    private static DrugReturnReasonRequest Beralasan(int versi, string alasan = "Tidak layak",
        string? kunci = null) => new()
    {
        Reason = alasan,
        ExpectedVersion = versi,
        IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N")
    };

    /// <summary>Satu bets bersaldo di depo, beserta service retur dan konteksnya.</summary>
    private static async Task<(DrugReturnService Layanan, ApplicationDbContext Konteks, Guid BetsId)>
        SiapAsync(PharmacyHarness h)
    {
        await using var pembuat = h.CreateContext();
        await h.SediakanStokAsync(pembuat);

        var konteks = h.CreateContext();
        var betsId = konteks.Set<PhmDrugBatch>().First().Id;
        return (h.ReturService(konteks), konteks, betsId);
    }

    private static async Task<DrugReturnDetailResponse> DiajukanAsync(DrugReturnService layanan,
        PharmacyHarness h, Guid betsId, decimal jumlah = 4)
    {
        var retur = await layanan.CreateAsync(Pembuatan(h, betsId, jumlah));
        return await layanan.SubmitAsync(retur.Id, Perintah(retur.Version));
    }

    // ------------------------------------------------------------------ pembuatan

    [Fact]
    public async Task Retur_valid_lahir_pada_status_draft()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));

        Assert.Equal(DrugReturnStatus.Draft, retur.Status);
        Assert.StartsWith("RTN-", retur.ReturnNumber);
        Assert.Equal(1, retur.ItemCount);
        Assert.Equal(0, retur.Version);
    }

    [Fact]
    public async Task Retur_belum_menyentuh_stok_saat_masih_draft()
    {
        // Draft adalah niat, bukan kejadian. Saldo baru berubah setelah diperiksa.
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        await layanan.CreateAsync(Pembuatan(h, betsId));

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(100, saldo.QuantityOnHand);
    }

    [Fact]
    public async Task Kunjungan_yang_tidak_ada_ditolak()
    {
        // `PHM074`.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var permintaan = Pembuatan(h, betsId);
        permintaan.EncounterId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.CreateAsync(permintaan));

        Assert.Equal("PHM074", galat.Code);
    }

    [Fact]
    public async Task Petugas_yang_tidak_aktif_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.CreateAsync(Pembuatan(h, betsId, petugas: h.PetugasNonaktifId)));

        Assert.Equal("PHM074", galat.Code);
    }

    [Fact]
    public async Task Lokasi_yang_tidak_boleh_menerima_barang_ditolak()
    {
        // `PHM076`. Depo yang boleh menyerahkan belum tentu boleh menerima kembali; membedakan
        // keduanya mencegah obat retur menumpuk di tempat yang tidak memeriksanya.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.CreateAsync(Pembuatan(h, betsId, depo: h.DepoTanpaTerimaId)));

        Assert.Equal("PHM076", galat.Code);
    }

    [Fact]
    public async Task Lokasi_yang_tidak_aktif_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.CreateAsync(Pembuatan(h, betsId, depo: h.DepoNonaktifId)));

        Assert.Equal("PHM076", galat.Code);
    }

    [Fact]
    public async Task Retur_tanpa_item_ditolak()
    {
        // `PHM073`.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var permintaan = Pembuatan(h, betsId);
        permintaan.Items.Clear();

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.CreateAsync(permintaan));

        Assert.Equal("PHM073", galat.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Jumlah_item_harus_lebih_dari_nol(int jumlah)
    {
        // `PHM077`.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.CreateAsync(Pembuatan(h, betsId, jumlah: jumlah)));

        Assert.Equal("PHM077", galat.Code);
    }

    [Fact]
    public async Task Satu_batch_hanya_boleh_disebut_satu_kali()
    {
        // `PHM078`. Dua baris untuk satu batch membuat jumlah yang dikembalikan bergantung pada
        // cara pembacanya menjumlahkan.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var permintaan = Pembuatan(h, betsId);
        permintaan.Items.Add(new DrugReturnItemInput
        {
            DrugId = h.ObatId,
            DrugBatchId = betsId,
            MeasurementId = h.SatuanId,
            Quantity = 2
        });

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.CreateAsync(permintaan));

        Assert.Equal("PHM078", galat.Code);
    }

    [Fact]
    public async Task Pembuatan_ulang_dengan_kunci_sama_tidak_menambah_retur()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        var kunci = Guid.NewGuid().ToString("N");
        var pertama = await layanan.CreateAsync(Pembuatan(h, betsId, kunci: kunci));
        var kedua = await layanan.CreateAsync(Pembuatan(h, betsId, kunci: kunci));

        Assert.Equal(pertama.Id, kedua.Id);
        Assert.Single(konteks.PhmDrugReturns.Where(x => !x.IsDelete));
    }

    [Fact]
    public async Task Kunci_idempotensi_wajib_diisi()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            layanan.CreateAsync(Pembuatan(h, betsId, kunci: "   ")));
    }

    // ------------------------------------------------------------------ pengajuan

    [Fact]
    public async Task Retur_draft_dapat_diajukan()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        var diajukan = await layanan.SubmitAsync(retur.Id, Perintah(retur.Version));

        Assert.Equal(DrugReturnStatus.Submitted, diajukan.Status);
        Assert.Equal(retur.Version + 1, diajukan.Version);
    }

    [Fact]
    public async Task Retur_yang_sudah_diajukan_tidak_dapat_diajukan_lagi()
    {
        // `PHM070`.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId);

        var galat = await Assert.ThrowsAsync<DrugReturnConflictException>(() =>
            layanan.SubmitAsync(diajukan.Id, Perintah(diajukan.Version)));

        Assert.Equal("PHM070", galat.Code);
    }

    [Fact]
    public async Task Versi_yang_tidak_cocok_saat_mengajukan_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));

        await Assert.ThrowsAsync<DrugReturnConflictException>(() =>
            layanan.SubmitAsync(retur.Id, Perintah(retur.Version + 5)));
    }

    [Fact]
    public async Task Pengajuan_yang_diulang_dengan_kunci_sama_tidak_menaikkan_versi_dua_kali()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        var kunci = Guid.NewGuid().ToString("N");

        var pertama = await layanan.SubmitAsync(retur.Id, Perintah(retur.Version, kunci));
        var kedua = await layanan.SubmitAsync(retur.Id, Perintah(retur.Version, kunci));

        Assert.Equal(pertama.Version, kedua.Version);
    }

    // ------------------------------------------------------------------ pemeriksaan

    [Fact]
    public async Task Verifikasi_penuh_mengembalikan_seluruh_jumlah_ke_stok()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId, jumlah: 4);
        var baris = diajukan.Items.Single();

        var diperiksa = await layanan.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = h.PetugasId,
            Items = [Keputusan(baris.Id, 4)],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        Assert.Equal(DrugReturnStatus.Verified, diperiksa.Status);

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(104, saldo.QuantityOnHand);
    }

    [Fact]
    public async Task Verifikasi_sebagian_hanya_mengembalikan_yang_layak()
    {
        // Selisih antara yang dikembalikan dan yang diterima adalah barang yang tidak layak
        // kembali — ia tidak boleh ikut menambah saldo.
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId, jumlah: 4);
        var baris = diajukan.Items.Single();

        await layanan.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = h.PetugasId,
            Items = [Keputusan(baris.Id, 1)],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(101, saldo.QuantityOnHand);
    }

    [Fact]
    public async Task Verifikasi_nol_tidak_menambah_stok_sama_sekali()
    {
        // Nol berarti seluruh baris ditolak. Returnya tetap `Verified` — ia sudah diperiksa —
        // tetapi tidak satu pun barang masuk kembali.
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId, jumlah: 4);
        var baris = diajukan.Items.Single();

        var diperiksa = await layanan.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = h.PetugasId,
            Items = [Keputusan(baris.Id, 0)],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        Assert.Equal(DrugReturnStatus.Verified, diperiksa.Status);

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(100, saldo.QuantityOnHand);
    }

    [Fact]
    public async Task Jumlah_diterima_melebihi_jumlah_yang_dikembalikan_ditolak()
    {
        // `PHM071`. Menerima lebih banyak daripada yang dikembalikan berarti barang muncul entah
        // dari mana; itu selisih yang harus ditelusuri sendiri, bukan retur.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId, jumlah: 4);
        var baris = diajukan.Items.Single();

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
            {
                VerifiedByWorkforceId = h.PetugasId,
                Items = [Keputusan(baris.Id, 5)],
                ExpectedVersion = diajukan.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("PHM071", galat.Code);
    }

    [Fact]
    public async Task Setiap_baris_wajib_diputuskan()
    {
        // `PHM079`. Baris yang tidak disebut tidak dianggap ditolak: diam bisa berarti "tidak
        // layak" atau "lupa diperiksa", dan keduanya berbeda akibatnya bagi yang mengembalikan.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId);

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
            {
                VerifiedByWorkforceId = h.PetugasId,
                Items = [],
                ExpectedVersion = diajukan.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("PHM079", galat.Code);
    }

    [Fact]
    public async Task Baris_pemeriksaan_yang_bukan_bagian_retur_ini_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId);

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
            {
                VerifiedByWorkforceId = h.PetugasId,
                Items = [Keputusan(Guid.NewGuid(), 1)],
                ExpectedVersion = diajukan.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("PHM079", galat.Code);
    }

    [Fact]
    public async Task Retur_draft_tidak_dapat_langsung_diperiksa()
    {
        // `PHM070`. Pemeriksaan hanya sah atas retur yang sudah diajukan.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        var baris = retur.Items.Single();

        var galat = await Assert.ThrowsAsync<DrugReturnConflictException>(() =>
            layanan.VerifyAsync(retur.Id, new VerifyDrugReturnRequest
            {
                VerifiedByWorkforceId = h.PetugasId,
                Items = [Keputusan(baris.Id, 1)],
                ExpectedVersion = retur.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("PHM070", galat.Code);
    }

    [Fact]
    public async Task Pemeriksaan_yang_diulang_tidak_menambah_stok_dua_kali()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId, jumlah: 4);
        var baris = diajukan.Items.Single();
        var kunci = Guid.NewGuid().ToString("N");

        var permintaan = () => new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = h.PetugasId,
            Items = [Keputusan(baris.Id, 4)],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = kunci
        };

        await layanan.VerifyAsync(diajukan.Id, permintaan());
        await layanan.VerifyAsync(diajukan.Id, permintaan());

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(104, saldo.QuantityOnHand);
    }

    // ------------------------------------------------------------------ penolakan

    [Fact]
    public async Task Penolakan_wajib_beralasan()
    {
        // `PHM072`.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId);

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.RejectAsync(diajukan.Id, Beralasan(diajukan.Version, "   ")));

        Assert.Equal("PHM072", galat.Code);
    }

    [Fact]
    public async Task Penolakan_tidak_menambah_stok()
    {
        // Retur yang ditolak berarti barangnya tidak diterima kembali. Saldo yang bertambah di
        // sini akan membuat obat yang tidak pernah masuk tercatat sebagai ada.
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId, jumlah: 4);

        var ditolak = await layanan.RejectAsync(diajukan.Id,
            Beralasan(diajukan.Version, "Kemasan sudah terbuka"));

        Assert.Equal(DrugReturnStatus.Rejected, ditolak.Status);

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(100, saldo.QuantityOnHand);
    }

    [Fact]
    public async Task Retur_draft_tidak_dapat_ditolak()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));

        var galat = await Assert.ThrowsAsync<DrugReturnConflictException>(() =>
            layanan.RejectAsync(retur.Id, Beralasan(retur.Version)));

        Assert.Equal("PHM070", galat.Code);
    }

    // ------------------------------------------------------------------ pembatalan

    [Fact]
    public async Task Pembatalan_wajib_beralasan()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));

        var galat = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(() =>
            layanan.CancelAsync(retur.Id, Beralasan(retur.Version, "")));

        Assert.Equal("PHM072", galat.Code);
    }

    [Theory]
    [InlineData(false)]  // dari draft
    [InlineData(true)]   // dari submitted
    public async Task Pembatalan_sah_dari_draft_maupun_sudah_diajukan(bool sudahDiajukan)
    {
        // Pembatalan sah dari DUA keadaan, berbeda dari perintah lain yang hanya dari satu.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        if (sudahDiajukan) retur = await layanan.SubmitAsync(retur.Id, Perintah(retur.Version));

        var dibatalkan = await layanan.CancelAsync(retur.Id,
            Beralasan(retur.Version, "Salah input"));

        Assert.Equal(DrugReturnStatus.Cancelled, dibatalkan.Status);
    }

    [Fact]
    public async Task Pembatalan_tidak_menambah_stok()
    {
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId, jumlah: 4));
        await layanan.CancelAsync(retur.Id, Beralasan(retur.Version, "Salah input"));

        var saldo = konteks.Set<PhmDrugStockBalance>().Single(x => x.StorageLocationId == h.DepoId);
        await konteks.Entry(saldo).ReloadAsync();
        Assert.Equal(100, saldo.QuantityOnHand);
    }

    [Fact]
    public async Task Retur_yang_sudah_diperiksa_tidak_dapat_dibatalkan()
    {
        // Keadaan terminal tidak mundur: stok sudah bertambah, dan membatalkannya di sini tidak
        // akan menariknya kembali.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId);
        var baris = diajukan.Items.Single();

        var diperiksa = await layanan.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = h.PetugasId,
            Items = [Keputusan(baris.Id, 1)],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var galat = await Assert.ThrowsAsync<DrugReturnConflictException>(() =>
            layanan.CancelAsync(diperiksa.Id, Beralasan(diperiksa.Version, "Mau dibatalkan")));

        Assert.Equal("PHM070", galat.Code);
    }

    [Fact]
    public async Task Retur_yang_sudah_ditolak_tidak_dapat_dibatalkan()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId);
        var ditolak = await layanan.RejectAsync(diajukan.Id, Beralasan(diajukan.Version));

        var galat = await Assert.ThrowsAsync<DrugReturnConflictException>(() =>
            layanan.CancelAsync(ditolak.Id, Beralasan(ditolak.Version, "Mau dibatalkan")));

        Assert.Equal("PHM070", galat.Code);
    }

    [Fact]
    public async Task Retur_yang_sudah_dibatalkan_tidak_dapat_diajukan()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        var dibatalkan = await layanan.CancelAsync(retur.Id, Beralasan(retur.Version, "Salah"));

        var galat = await Assert.ThrowsAsync<DrugReturnConflictException>(() =>
            layanan.SubmitAsync(dibatalkan.Id, Perintah(dibatalkan.Version)));

        Assert.Equal("PHM070", galat.Code);
    }

    // ------------------------------------------------------------------ histori

    [Fact]
    public async Task Setiap_perpindahan_status_meninggalkan_satu_baris_histori()
    {
        // Histori inilah yang menjawab "siapa menolak retur ini dan kapan" ketika dipersoalkan.
        using var h = new PharmacyHarness();
        var (layanan, konteks, betsId) = await SiapAsync(h);

        var diajukan = await DiajukanAsync(layanan, h, betsId);
        await layanan.RejectAsync(diajukan.Id, Beralasan(diajukan.Version, "Tidak layak"));

        var histori = konteks.PhmDrugReturnHistories
            .Where(x => x.DrugReturnId == diajukan.Id && !x.IsDelete)
            .ToList();

        // Pembuatan, pengajuan, penolakan.
        Assert.Equal(3, histori.Count);
    }

    [Fact]
    public async Task Versi_naik_satu_per_perpindahan_status()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId));
        Assert.Equal(0, retur.Version);

        var diajukan = await layanan.SubmitAsync(retur.Id, Perintah(retur.Version));
        Assert.Equal(1, diajukan.Version);

        var ditolak = await layanan.RejectAsync(diajukan.Id, Beralasan(diajukan.Version));
        Assert.Equal(2, ditolak.Version);
    }

    [Fact]
    public async Task Detail_retur_yang_tidak_ada_mengembalikan_null()
    {
        using var h = new PharmacyHarness();
        var (layanan, _, _) = await SiapAsync(h);

        Assert.Null(await layanan.GetDetailAsync(Guid.NewGuid()));
    }

    // ------------------------------------------------------- catatan atas lingkup source

    [Fact]
    public async Task Retur_tidak_dibatasi_jumlah_yang_pernah_diserahkan()
    {
        // Perilaku apa adanya, dicatat supaya terlihat — BUKAN perilaku yang dibenarkan uji ini.
        //
        // Retur pada source ini mengacu pada BATCH obat, bukan pada baris penyerahan, dan tidak
        // ada satu pun pemeriksaan yang membandingkan jumlah retur dengan jumlah yang pernah
        // benar-benar diserahkan kepada pasien itu. `SourceDrugUsageId` pun opsional dan hanya
        // disimpan sebagai rujukan.
        //
        // Akibatnya retur 1.000 tablet atas resep berisi 10 tetap diterima, dan setelah
        // diperiksa jumlah itu benar-benar masuk ke saldo. Dilaporkan sebagai temuan.
        using var h = new PharmacyHarness();
        var (layanan, _, betsId) = await SiapAsync(h);

        var retur = await layanan.CreateAsync(Pembuatan(h, betsId, jumlah: 1000));

        Assert.Equal(DrugReturnStatus.Draft, retur.Status);
        Assert.Equal(1000, retur.Items.Single().Quantity);
    }

    private static VerifyDrugReturnItemInput Keputusan(Guid barisId, decimal diterima) => new()
    {
        DrugReturnItemId = barisId,
        AcceptedQuantity = diterima,
        AcceptedStatus = DrugStockStatus.Available
    };
}
