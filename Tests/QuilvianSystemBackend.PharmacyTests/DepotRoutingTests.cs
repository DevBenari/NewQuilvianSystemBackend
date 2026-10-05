using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy
{
    /// <summary>
    /// `PHA-BE-002` — bukti perilaku otomatis untuk `PharmacyDepotRoutingService`.
    ///
    /// Resolver ini menjawab satu pertanyaan: diberi satu kunjungan pasien, depo mana yang
    /// melayaninya. Yang dijaga di sini adalah sembilan acceptance criteria `PHA-BE-001`
    /// beserta satu error code tambahan yang ada di source tetapi tidak disebut kriterianya.
    ///
    /// Uji ini memakai fixture sendiri, bukan <see cref="PharmacyHarness"/>, karena routing
    /// menuntut depo dengan `ServiceUnitId`, `ClinicId`, dan `StorageLocationType` yang
    /// dikarang per skenario — sementara harness menyeragamkan depo untuk uji penyerahan.
    /// Memaksakan keduanya berbagi fixture akan membuat satu uji mengubah arti uji lain.
    /// </summary>
    public sealed class DepotRoutingTests : IDisposable
    {
        private readonly TestDatabase _db = TestDatabase.Create();

        private static readonly Guid UnitRawatJalan = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid UnitLain = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid KlinikAnak = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public void Dispose() => _db.Dispose();

        private PharmacyDepotRoutingService Layanan(ApplicationDbContext k) => new(k);

        /// <summary>
        /// Menyiapkan satu kunjungan. Nilai bawaannya sah dan aktif, sehingga setiap uji hanya
        /// perlu menyebut hal yang memang ingin dibuat berbeda.
        /// </summary>
        private Guid Kunjungan(
            ApplicationDbContext k,
            EncounterType jenis = EncounterType.Outpatient,
            Guid? unitLayanan = null,
            Guid? klinik = null,
            bool aktif = true,
            bool terhapus = false,
            bool dibatalkan = false)
        {
            var id = Guid.NewGuid();

            k.Set<RegPatientEncounter>().Add(new RegPatientEncounter
            {
                Id = id,
                PatientId = Guid.NewGuid(),
                EncounterNumber = $"UJI-RUTE-{Guid.NewGuid():N}"[..20],
                EncounterType = jenis,
                ServiceUnitId = unitLayanan ?? UnitRawatJalan,
                ClinicId = klinik,
                IsActive = aktif,
                IsDelete = terhapus,
                IsCancel = dibatalkan,
                CreateDateTime = DateTime.UtcNow
            });

            k.SaveChanges();
            return id;
        }

        /// <summary>
        /// Menyiapkan satu lokasi penyimpanan obat. Nilai bawaannya adalah depo farmasi yang
        /// sah dan boleh menyerahkan obat.
        /// </summary>
        private Guid Depo(
            ApplicationDbContext k,
            Guid? unitLayanan = null,
            Guid? klinik = null,
            string jenis = "Pharmacy",
            bool aktif = true,
            bool terhapus = false,
            bool dibatalkan = false,
            bool lokasiFarmasi = true,
            bool bolehSerah = true,
            bool gudangUtama = false,
            bool karantina = false)
        {
            var id = Guid.NewGuid();

            k.Set<MstDrugStorageLocation>().Add(new MstDrugStorageLocation
            {
                Id = id,
                StorageLocationCode = $"UJI-RUTE-{Guid.NewGuid():N}"[..20],
                StorageLocationName = "Depo Uji Routing",
                ServiceUnitId = unitLayanan ?? UnitRawatJalan,
                ClinicId = klinik,
                StorageLocationType = jenis,
                IsActive = aktif,
                IsDelete = terhapus,
                IsCancel = dibatalkan,
                IsPharmacyLocation = lokasiFarmasi,
                IsAllowDispensing = bolehSerah,
                IsMainWarehouse = gudangUtama,
                IsQuarantineLocation = karantina,
                CreateDateTime = DateTime.UtcNow
            });

            k.SaveChanges();
            return id;
        }

        // ── Kriteria 1 — Rawat Jalan: klinik lebih dulu, baru unit layanan ──────────────────

        [Fact]
        public async Task Rawat_jalan_memilih_depo_kliniknya_lebih_dulu()
        {
            using var k = _db.CreateContext();
            var depoKlinik = Depo(k, klinik: KlinikAnak);
            Depo(k, klinik: null);

            var kunjungan = Kunjungan(k, EncounterType.Outpatient, klinik: KlinikAnak);

            var hasil = await Layanan(k).ResolveAsync(kunjungan);

            Assert.True(hasil.IsSuccess);
            Assert.Equal(depoKlinik, hasil.StorageLocationId);
            Assert.Equal("PHA_ROUTE_RESOLVED", hasil.Code);
        }

        [Fact]
        public async Task Rawat_jalan_jatuh_ke_unit_layanan_bila_kliniknya_tidak_punya_depo()
        {
            using var k = _db.CreateContext();
            var depoUnit = Depo(k, klinik: null);

            // Kliniknya ada pada kunjungan, tetapi tidak ada depo yang memegang klinik itu.
            var kunjungan = Kunjungan(k, EncounterType.Outpatient, klinik: KlinikAnak);

            var hasil = await Layanan(k).ResolveAsync(kunjungan);

            Assert.True(hasil.IsSuccess);
            Assert.Equal(depoUnit, hasil.StorageLocationId);
        }

        [Fact]
        public async Task Rawat_jalan_tanpa_klinik_langsung_memakai_unit_layanan()
        {
            using var k = _db.CreateContext();
            var depoUnit = Depo(k, klinik: null);

            var hasil = await Layanan(k).ResolveAsync(
                Kunjungan(k, EncounterType.Outpatient, klinik: null));

            Assert.True(hasil.IsSuccess);
            Assert.Equal(depoUnit, hasil.StorageLocationId);
        }

        [Fact]
        public async Task Rawat_jalan_dengan_klinik_kosong_diperlakukan_sebagai_tanpa_klinik()
        {
            using var k = _db.CreateContext();
            var depoUnit = Depo(k, klinik: null);

            var hasil = await Layanan(k).ResolveAsync(
                Kunjungan(k, EncounterType.Outpatient, klinik: Guid.Empty));

            Assert.True(hasil.IsSuccess);
            Assert.Equal(depoUnit, hasil.StorageLocationId);
        }

        /// <summary>
        /// Penting: fallback hanya berlaku ketika kliniknya **tidak punya** depo. Ketika
        /// kliniknya punya dua, itu salah konfigurasi dan harus terlihat — bukan disembunyikan
        /// dengan diam-diam pindah ke prioritas berikutnya.
        /// </summary>
        [Fact]
        public async Task Depo_klinik_ganda_tidak_jatuh_ke_unit_layanan_melainkan_ditolak()
        {
            using var k = _db.CreateContext();
            Depo(k, klinik: KlinikAnak);
            Depo(k, klinik: KlinikAnak);
            Depo(k, klinik: null);

            var hasil = await Layanan(k).ResolveAsync(
                Kunjungan(k, EncounterType.Outpatient, klinik: KlinikAnak));

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_AMBIGUOUS", hasil.Code);
            Assert.Null(hasil.StorageLocationId);
        }

        [Fact]
        public async Task Rawat_jalan_tidak_mengambil_depo_unit_layanan_lain()
        {
            using var k = _db.CreateContext();
            Depo(k, unitLayanan: UnitLain, klinik: null);

            var hasil = await Layanan(k).ResolveAsync(
                Kunjungan(k, EncounterType.Outpatient, unitLayanan: UnitRawatJalan));

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_NOT_FOUND", hasil.Code);
        }

        // ── Kriteria 2 — IGD: unit sama, bertipe Emergency ─────────────────────────────────

        [Fact]
        public async Task Igd_memilih_depo_bertipe_darurat_pada_unit_yang_sama()
        {
            using var k = _db.CreateContext();
            var depoDarurat = Depo(k, jenis: "Emergency");
            Depo(k, jenis: "Pharmacy");

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Emergency));

            Assert.True(hasil.IsSuccess);
            Assert.Equal(depoDarurat, hasil.StorageLocationId);
        }

        [Fact]
        public async Task Igd_tidak_menerima_depo_bertipe_farmasi()
        {
            using var k = _db.CreateContext();
            Depo(k, jenis: "Pharmacy");

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Emergency));

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_NOT_FOUND", hasil.Code);
        }

        [Fact]
        public async Task Igd_tidak_menerima_depo_darurat_pada_unit_lain()
        {
            using var k = _db.CreateContext();
            Depo(k, unitLayanan: UnitLain, jenis: "Emergency");

            var hasil = await Layanan(k).ResolveAsync(
                Kunjungan(k, EncounterType.Emergency, unitLayanan: UnitRawatJalan));

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_NOT_FOUND", hasil.Code);
        }

        // ── Kriteria 3 — Rawat Inap: unit sama, bertipe Pharmacy ───────────────────────────

        [Fact]
        public async Task Rawat_inap_memilih_depo_bertipe_farmasi_pada_unit_yang_sama()
        {
            using var k = _db.CreateContext();
            var depoFarmasi = Depo(k, jenis: "Pharmacy");
            Depo(k, jenis: "Emergency");

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Inpatient));

            Assert.True(hasil.IsSuccess);
            Assert.Equal(depoFarmasi, hasil.StorageLocationId);
        }

        [Fact]
        public async Task Rawat_inap_tidak_menerima_depo_bertipe_darurat()
        {
            using var k = _db.CreateContext();
            Depo(k, jenis: "Emergency");

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Inpatient));

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_NOT_FOUND", hasil.Code);
        }

        /// <summary>
        /// Source membandingkan `StorageLocationType.ToLower()`, sehingga ejaan huruf besar
        /// pada data master tidak boleh menggagalkan routing.
        /// </summary>
        [Theory]
        [InlineData("Pharmacy")]
        [InlineData("PHARMACY")]
        [InlineData("pharmacy")]
        [InlineData("PhArMaCy")]
        public async Task Jenis_lokasi_dibandingkan_tanpa_memandang_huruf(string ejaan)
        {
            using var k = _db.CreateContext();
            var depo = Depo(k, jenis: ejaan);

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Inpatient));

            Assert.True(hasil.IsSuccess);
            Assert.Equal(depo, hasil.StorageLocationId);
        }

        // ── Kriteria 4 — kandidat yang harus dikeluarkan ───────────────────────────────────

        public static TheoryData<string> AlasanPengecualian() => new()
        {
            "nonaktif", "terhapus", "dibatalkan", "gudang utama",
            "karantina", "bukan lokasi farmasi", "tidak boleh menyerahkan"
        };

        [Theory]
        [MemberData(nameof(AlasanPengecualian))]
        public async Task Kandidat_yang_tidak_layak_dikeluarkan_lebih_dulu(string alasan)
        {
            using var k = _db.CreateContext();

            Depo(
                k,
                klinik: null,
                aktif: alasan != "nonaktif",
                terhapus: alasan == "terhapus",
                dibatalkan: alasan == "dibatalkan",
                lokasiFarmasi: alasan != "bukan lokasi farmasi",
                bolehSerah: alasan != "tidak boleh menyerahkan",
                gudangUtama: alasan == "gudang utama",
                karantina: alasan == "karantina");

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Outpatient));

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_NOT_FOUND", hasil.Code);
        }

        [Fact]
        public async Task Satu_kandidat_layak_tetap_terpilih_walau_dikelilingi_kandidat_tidak_layak()
        {
            using var k = _db.CreateContext();
            var layak = Depo(k, klinik: null);
            Depo(k, klinik: null, aktif: false);
            Depo(k, klinik: null, gudangUtama: true);
            Depo(k, klinik: null, karantina: true);
            Depo(k, klinik: null, bolehSerah: false);
            Depo(k, klinik: null, lokasiFarmasi: false);

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Outpatient));

            Assert.True(hasil.IsSuccess);
            Assert.Equal(layak, hasil.StorageLocationId);
        }

        // ── Kriteria 5 dan 6 — nol kandidat, dan lebih dari satu ───────────────────────────

        [Fact]
        public async Task Nol_kandidat_menghasilkan_not_found()
        {
            using var k = _db.CreateContext();

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Outpatient));

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_NOT_FOUND", hasil.Code);
            Assert.Null(hasil.StorageLocationId);
        }

        /// <summary>
        /// Inti kriteria 6: ketika kandidatnya lebih dari satu, resolver **tidak** boleh
        /// memilih salah satunya. Dibuktikan dengan memeriksa bahwa `StorageLocationId` kosong,
        /// bukan sekadar bahwa codenya `AMBIGUOUS`.
        /// </summary>
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task Kandidat_ganda_ditolak_dan_tidak_memilih_baris_pertama(int jumlah)
        {
            using var k = _db.CreateContext();
            var dibuat = new List<Guid>();
            for (var i = 0; i < jumlah; i++) dibuat.Add(Depo(k, klinik: null));

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Outpatient));

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_AMBIGUOUS", hasil.Code);
            Assert.Null(hasil.StorageLocationId);
            Assert.DoesNotContain(hasil.StorageLocationId ?? Guid.Empty, dibuat);
        }

        // ── Kriteria 7 — jenis layanan yang belum punya aturan ─────────────────────────────

        [Theory]
        [InlineData(EncounterType.MedicalCheckup)]
        [InlineData(EncounterType.Telemedicine)]
        public async Task Jenis_layanan_di_luar_tiga_aturan_ditolak(EncounterType jenis)
        {
            using var k = _db.CreateContext();
            Depo(k, klinik: null);

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, jenis));

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_SERVICE_UNSUPPORTED", hasil.Code);
            Assert.Null(hasil.StorageLocationId);
        }

        /// <summary>
        /// `EncounterType.Unknown` sengaja TIDAK diuji lewat kunjungan tersimpan, karena nilai
        /// itu tidak dapat bertahan di basis data.
        ///
        /// `RegPatientEncounterConfiguration` baris 95–97 memberi kolomnya
        /// `HasDefaultValue(EncounterType.Outpatient)`. Karena `Unknown = 0` kebetulan sama
        /// dengan nilai bawaan CLR, EF Core memperlakukannya sebagai "belum diisi", menghilangkan
        /// kolomnya dari perintah `INSERT`, dan store menuliskan `Outpatient`.
        ///
        /// Jadi arm `_` pada `switch` resolver memang tidak dapat dijangkau oleh `Unknown` —
        /// bukan karena resolvernya salah, melainkan karena nilai itu tidak pernah sampai ke
        /// sana. Uji ini mengunci kenyataan tersebut supaya tidak ada yang menyimpulkan
        /// resolver menerima kunjungan berjenis tidak diketahui.
        /// </summary>
        [Fact]
        public async Task Jenis_layanan_tidak_diketahui_tidak_dapat_tersimpan_dan_menjadi_rawat_jalan()
        {
            using var k = _db.CreateContext();
            var depo = Depo(k, klinik: null);

            var kunjungan = Kunjungan(k, EncounterType.Unknown);

            var tersimpan = await k.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == kunjungan)
                .Select(x => x.EncounterType)
                .SingleAsync();

            Assert.Equal(EncounterType.Outpatient, tersimpan);

            // Karena tersimpan sebagai rawat jalan, resolver menjawabnya dengan aturan rawat
            // jalan — bukan dengan `PHA_ROUTE_SERVICE_UNSUPPORTED`.
            var hasil = await Layanan(k).ResolveAsync(kunjungan);

            Assert.True(hasil.IsSuccess);
            Assert.Equal(depo, hasil.StorageLocationId);
        }

        // ── Error code di luar sembilan kriteria, tercatat pada laporan PHA-BE-001 ─────────

        [Fact]
        public async Task Encounter_kosong_ditolak_sebagai_encounter_tidak_sah()
        {
            using var k = _db.CreateContext();

            var hasil = await Layanan(k).ResolveAsync(Guid.Empty);

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_ENCOUNTER_INVALID", hasil.Code);
        }

        [Fact]
        public async Task Encounter_yang_tidak_ada_ditolak_sebagai_encounter_tidak_sah()
        {
            using var k = _db.CreateContext();

            var hasil = await Layanan(k).ResolveAsync(Guid.NewGuid());

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_ENCOUNTER_INVALID", hasil.Code);
        }

        public static TheoryData<string> KeadaanEncounterTidakSah() => new()
        {
            "nonaktif", "terhapus", "dibatalkan"
        };

        /// <summary>
        /// Kunjungan yang sudah tidak berlaku tidak boleh menghasilkan depo. Yang dijaga di
        /// sini bukan hanya penolakannya, melainkan juga **sebabnya**: ditolak karena
        /// kunjungannya, bukan karena jenis layanannya.
        /// </summary>
        [Theory]
        [MemberData(nameof(KeadaanEncounterTidakSah))]
        public async Task Encounter_yang_sudah_tidak_berlaku_ditolak(string keadaan)
        {
            using var k = _db.CreateContext();
            Depo(k, klinik: null);

            var kunjungan = Kunjungan(
                k,
                EncounterType.Outpatient,
                aktif: keadaan != "nonaktif",
                terhapus: keadaan == "terhapus",
                dibatalkan: keadaan == "dibatalkan");

            var hasil = await Layanan(k).ResolveAsync(kunjungan);

            Assert.False(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_ENCOUNTER_INVALID", hasil.Code);
        }

        // ── Kriteria 8 — baca saja ─────────────────────────────────────────────────────────

        /// <summary>
        /// Resolver tidak boleh mengubah apa pun. Dibuktikan dua arah: `ChangeTracker` tidak
        /// memegang satu entitas pun sesudahnya — bukti `AsNoTracking` benar-benar dipakai —
        /// dan jumlah baris kedua tabel tidak berubah.
        /// </summary>
        [Fact]
        public async Task Resolver_tidak_mengubah_apa_pun()
        {
            using var k = _db.CreateContext();
            Depo(k, klinik: null);
            var kunjungan = Kunjungan(k, EncounterType.Outpatient);

            var depoSebelum = await k.Set<MstDrugStorageLocation>().CountAsync();
            var kunjunganSebelum = await k.Set<RegPatientEncounter>().CountAsync();

            k.ChangeTracker.Clear();

            var hasil = await Layanan(k).ResolveAsync(kunjungan);

            Assert.True(hasil.IsSuccess);
            Assert.Empty(k.ChangeTracker.Entries());
            Assert.Equal(depoSebelum, await k.Set<MstDrugStorageLocation>().CountAsync());
            Assert.Equal(kunjunganSebelum, await k.Set<RegPatientEncounter>().CountAsync());
        }

        [Fact]
        public async Task Memanggil_resolver_berulang_kali_memberi_jawaban_yang_sama()
        {
            using var k = _db.CreateContext();
            var depo = Depo(k, klinik: null);
            var kunjungan = Kunjungan(k, EncounterType.Outpatient);

            var layanan = Layanan(k);

            for (var i = 0; i < 3; i++)
            {
                var hasil = await layanan.ResolveAsync(kunjungan);
                Assert.True(hasil.IsSuccess);
                Assert.Equal(depo, hasil.StorageLocationId);
            }
        }

        [Fact]
        public async Task Token_pembatalan_dihormati()
        {
            using var k = _db.CreateContext();
            Depo(k, klinik: null);
            var kunjungan = Kunjungan(k, EncounterType.Outpatient);

            using var pembatalan = new CancellationTokenSource();
            await pembatalan.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => Layanan(k).ResolveAsync(kunjungan, pembatalan.Token));
        }

        // ── Bentuk balasan ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Balasan_gagal_selalu_memuat_code_dan_pesan_dan_tanpa_depo()
        {
            using var k = _db.CreateContext();

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Outpatient));

            Assert.False(hasil.IsSuccess);
            Assert.NotEmpty(hasil.Code);
            Assert.NotEmpty(hasil.Message);
            Assert.Null(hasil.StorageLocationId);
        }

        [Fact]
        public async Task Balasan_berhasil_memuat_code_resolved_dan_depo_yang_terpilih()
        {
            using var k = _db.CreateContext();
            var depo = Depo(k, klinik: null);

            var hasil = await Layanan(k).ResolveAsync(Kunjungan(k, EncounterType.Outpatient));

            Assert.True(hasil.IsSuccess);
            Assert.Equal("PHA_ROUTE_RESOLVED", hasil.Code);
            Assert.NotEmpty(hasil.Message);
            Assert.Equal(depo, hasil.StorageLocationId);
        }
    }
}
