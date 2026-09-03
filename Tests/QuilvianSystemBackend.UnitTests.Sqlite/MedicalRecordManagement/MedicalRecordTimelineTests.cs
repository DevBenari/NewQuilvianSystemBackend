using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-13` — service penggabungan riwayat rekam medis.
    ///
    /// Menutup uji penerimaan `AT-RM-09` dan `AT-RM-31`, beserta seluruh acceptance criteria
    /// `BE-13`:
    /// <list type="number">
    /// <item>dokumen dari beberapa kunjungan tampil dalam satu daftar berurut waktu;</item>
    /// <item>jumlah baris dibatasi dan penyaringan tanggal berfungsi;</item>
    /// <item>hanya jenis dokumen yang diminta yang diambil;</item>
    /// <item>bila satu sumber gagal, sumber lain tetap tampil dan yang gagal ditandai;</item>
    /// <item>seluruh pembacaan memakai `AsNoTracking`.</item>
    /// </list>
    ///
    /// Seluruh data di sini adalah data karangan. Tidak ada data pasien sungguhan.
    /// </summary>
    public class MedicalRecordTimelineTests
    {
        private static readonly DateTime Sekarang = new(2026, 8, 26, 9, 0, 0, DateTimeKind.Utc);

        private static MedicalRecordTimelineService Service(ApplicationDbContext c) => new(c);

        // =====================================================================
        // Penyiapan data
        // =====================================================================

        /// <summary>
        /// Membuat kunjungan tambahan untuk pasien yang sama.
        ///
        /// Diperlukan karena inti `AT-RM-09` justru pada riwayat LINTAS kunjungan, sedangkan
        /// penyiapan bawaan hanya membuat satu kunjungan per pasien.
        /// </summary>
        private static Guid BuatKunjunganLain(
            ApplicationDbContext context,
            RekamMedisTestData.Konteks konteks)
        {
            var kunjungan = new TrxPatientEncounter
            {
                EncounterNumber = $"KJG-{Guid.NewGuid():N}"[..20],
                PatientId = konteks.PatientId,
                ServiceUnitId = konteks.ServiceUnitId,
                EncounterType = EncounterType.Outpatient,
                VisitType = VisitType.FollowUp,
                EncounterStatus = EncounterStatus.Completed,
                RegisteredByUserId = konteks.UserId
            };

            context.Set<TrxPatientEncounter>().Add(kunjungan);
            context.SaveChanges();

            return kunjungan.Id;
        }

        private static TrxPatientIntegratedProgressNote BuatCppt(
            ApplicationDbContext context,
            Guid patientId,
            Guid? encounterId,
            DateTime waktu,
            bool dibatalkan = false)
        {
            var cppt = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{Guid.NewGuid():N}"[..20],
                PatientId = patientId,
                EncounterId = encounterId,
                ProfessionType = "Doctor",
                ProfessionName = "Dokter",
                ProviderDisplayNameSnapshot = "dr. Uji",
                NoteDateTime = waktu,
                IsCancel = dibatalkan,
                CreateDateTime = waktu
            };

            context.Set<TrxPatientIntegratedProgressNote>().Add(cppt);
            context.SaveChanges();
            return cppt;
        }

        private static TrxPatientVitalSign BuatTandaVital(
            ApplicationDbContext context,
            Guid patientId,
            Guid? encounterId,
            DateTime waktu)
        {
            var tandaVital = new TrxPatientVitalSign
            {
                VitalSignRecordNumber = $"TTV-{Guid.NewGuid():N}"[..20],
                PatientId = patientId,
                EncounterId = encounterId,
                ObservationDateTime = waktu,
                ObservationLocation = "Poliklinik Uji",
                CreateDateTime = waktu
            };

            context.Set<TrxPatientVitalSign>().Add(tandaVital);
            context.SaveChanges();
            return tandaVital;
        }

        private static TrxPatientAllergy BuatAlergi(
            ApplicationDbContext context,
            Guid patientId,
            Guid? encounterId,
            DateTime waktu)
        {
            var alergi = new TrxPatientAllergy
            {
                AllergyRecordNumber = $"ALG-{Guid.NewGuid():N}"[..20],
                PatientId = patientId,
                EncounterId = encounterId,
                AllergenName = "Amoksisilin",
                AllergenGroupName = "Antibiotik",
                ReportedDateTime = waktu,
                CreateDateTime = waktu
            };

            context.Set<TrxPatientAllergy>().Add(alergi);
            context.SaveChanges();
            return alergi;
        }

        private static TrxPatientConsent BuatPersetujuan(
            ApplicationDbContext context,
            Guid patientId,
            Guid? encounterId,
            DateTime waktu)
        {
            var persetujuan = new TrxPatientConsent
            {
                ConsentNumber = $"CNS-{Guid.NewGuid():N}"[..20],
                PatientId = patientId,
                EncounterId = encounterId,
                ConsentTitle = "Persetujuan Tindakan Uji",
                ConsentCategoryName = "Tindakan",
                SignerName = "Wali Uji",
                ConsentDateTime = waktu,
                CreateDateTime = waktu
            };

            context.Set<TrxPatientConsent>().Add(persetujuan);
            context.SaveChanges();
            return persetujuan;
        }

        private static MedicalRecordTimelineQuery Permintaan(
            Guid patientId,
            params ClinicalDocumentKind[] jenis) => new()
            {
                PatientId = patientId,
                DocumentKinds = jenis.Length == 0 ? null : jenis,
                PageSize = 50,
                NewestFirst = false
            };

        // =====================================================================
        // AT-RM-09 — riwayat lintas kunjungan dalam satu daftar
        // =====================================================================

        /// <summary>
        /// `AT-RM-09`: pasien dengan tiga kunjungan berbeda. Dokumen dari ketiganya tampil dalam
        /// satu daftar berurut waktu, tanpa perlu membuka kunjungan satu per satu.
        ///
        /// Inilah gap `RM-CAP-004`. Sebelum service ini ada, keenam dokumen di bawah tersebar di
        /// empat endpoint berbeda dengan penomoran halaman masing-masing.
        /// </summary>
        [Fact]
        public async Task RiwayatTigaKunjungan_TampilSebagaiSatuDaftarBerurutWaktu()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var kunjunganKedua = BuatKunjunganLain(context, konteks);
            var kunjunganKetiga = BuatKunjunganLain(context, konteks);

            // Sengaja dimasukkan dengan urutan waktu yang berantakan, supaya yang diuji benar
            // benar pengurutannya, bukan urutan penyimpanan.
            BuatPersetujuan(context, konteks.PatientId, kunjunganKetiga, Sekarang.AddDays(-2));
            BuatCppt(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-90));
            BuatAlergi(context, konteks.PatientId, kunjunganKedua, Sekarang.AddDays(-30));
            BuatTandaVital(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-91));
            BuatCppt(context, konteks.PatientId, kunjunganKedua, Sekarang.AddDays(-29));
            BuatCppt(context, konteks.PatientId, kunjunganKetiga, Sekarang.AddDays(-1));

            using var konteksBaca = database.CreateContext();
            var hasil = await Service(konteksBaca).GetTimelineAsync(Permintaan(konteks.PatientId));

            Assert.Equal(6, hasil.Page.TotalData);
            Assert.Equal(6, hasil.Page.Items.Count);

            // Satu daftar, berurut waktu naik.
            var waktu = hasil.Page.Items.Select(x => x.OccurredAt).ToList();
            Assert.Equal(waktu.OrderBy(x => x).ToList(), waktu);

            // Ketiga kunjungan benar-benar terwakili dalam satu daftar yang sama.
            var kunjunganTampil = hasil.Page.Items
                .Select(x => x.EncounterId)
                .Distinct()
                .ToList();
            Assert.Equal(3, kunjunganTampil.Count);
            Assert.Contains(konteks.EncounterId, kunjunganTampil);
            Assert.Contains(kunjunganKedua, kunjunganTampil);
            Assert.Contains(kunjunganKetiga, kunjunganTampil);

            // Empat jenis dokumen berbeda, masing-masing dengan nama yang siap ditampilkan.
            var jenisTampil = hasil.Page.Items.Select(x => x.DocumentKind).Distinct().ToList();
            Assert.Equal(4, jenisTampil.Count);
            Assert.All(hasil.Page.Items, x => Assert.False(string.IsNullOrWhiteSpace(x.DocumentKindName)));

            Assert.True(hasil.IsComplete);
        }

        /// <summary>
        /// Penyaring kunjungan mempersempit daftar ke satu kunjungan saja, dan tetap bekerja
        /// pada tabel yang kolom kunjungannya boleh kosong maupun yang wajib terisi.
        /// </summary>
        [Fact]
        public async Task PenyaringKunjungan_HanyaMengambilDokumenKunjunganItu()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var kunjunganKedua = BuatKunjunganLain(context, konteks);

            BuatCppt(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-5));
            BuatTandaVital(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-5));
            BuatCppt(context, konteks.PatientId, kunjunganKedua, Sekarang.AddDays(-1));

            using var konteksBaca = database.CreateContext();

            var permintaan = Permintaan(konteks.PatientId);
            permintaan.EncounterId = kunjunganKedua;

            var hasil = await Service(konteksBaca).GetTimelineAsync(permintaan);

            Assert.Equal(1, hasil.Page.TotalData);
            Assert.Equal(kunjunganKedua, hasil.Page.Items.Single().EncounterId);
        }

        // =====================================================================
        // AT-RM-31 — pembatasan jumlah baris dan penyaringan tanggal
        // =====================================================================

        /// <summary>
        /// `AT-RM-31`: pasien dengan sangat banyak dokumen. Jumlah baris dibatasi, penyaringan
        /// rentang tanggal berfungsi, dan tidak ada permintaan yang berjalan tanpa batas.
        /// </summary>
        [Fact]
        public async Task PasienDenganBanyakDokumen_JumlahBarisDibatasiDanTanggalTersaring()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            // Tiga puluh catatan, satu per hari mundur dari hari ini.
            for (var i = 0; i < 30; i++)
                BuatCppt(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-i));

            using var konteksBaca = database.CreateContext();
            var service = Service(konteksBaca);

            // 1) Jumlah baris dibatasi ukuran halaman, tetapi jumlah totalnya tetap benar.
            var halamanPertama = Permintaan(konteks.PatientId);
            halamanPertama.PageSize = 10;
            halamanPertama.NewestFirst = true;

            var hasilPertama = await service.GetTimelineAsync(halamanPertama);

            Assert.Equal(10, hasilPertama.Page.Items.Count);
            Assert.Equal(30, hasilPertama.Page.TotalData);
            Assert.Equal(3, hasilPertama.Page.TotalPage);
            Assert.Equal(Sekarang, hasilPertama.Page.Items.First().OccurredAt);

            // 2) Halaman berikutnya melanjutkan urutan, tidak mengulang.
            var halamanKedua = Permintaan(konteks.PatientId);
            halamanKedua.PageSize = 10;
            halamanKedua.NewestFirst = true;
            halamanKedua.Page = 2;

            var hasilKedua = await service.GetTimelineAsync(halamanKedua);

            Assert.Equal(10, hasilKedua.Page.Items.Count);
            Assert.Equal(Sekarang.AddDays(-10), hasilKedua.Page.Items.First().OccurredAt);
            Assert.Empty(hasilPertama.Page.Items
                .Select(x => x.DocumentId)
                .Intersect(hasilKedua.Page.Items.Select(x => x.DocumentId)));

            // 3) Permintaan berukuran raksasa dipotong ke batas atas, bukan dilayani apa adanya.
            var halamanRaksasa = Permintaan(konteks.PatientId);
            halamanRaksasa.PageSize = 100_000;

            var hasilRaksasa = await service.GetTimelineAsync(halamanRaksasa);

            Assert.Equal(
                MedicalRecordTimelineService.UkuranHalamanMaksimal,
                hasilRaksasa.Page.PageSize);

            // 4) Penyaringan rentang tanggal benar-benar mempersempit hasil.
            var rentang = Permintaan(konteks.PatientId);
            rentang.StartDate = Sekarang.AddDays(-4);
            rentang.EndDate = Sekarang.AddDays(-2);

            var hasilRentang = await service.GetTimelineAsync(rentang);

            Assert.Equal(3, hasilRentang.Page.TotalData);
            Assert.All(hasilRentang.Page.Items, x =>
            {
                Assert.True(x.OccurredAt >= rentang.StartDate);
                Assert.True(x.OccurredAt <= rentang.EndDate);
            });
        }

        // =====================================================================
        // Acceptance criteria 3 — hanya jenis yang diminta
        // =====================================================================

        /// <summary>
        /// Hanya jenis dokumen yang diminta yang ditanyakan ke basis data.
        ///
        /// Ini pembatas paling ampuh terhadap risiko "tiga belas query sekali jalan" yang
        /// disebut pada arsitektur bagian 5.8.
        /// </summary>
        [Fact]
        public async Task HanyaJenisYangDiminta_YangDiambil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            BuatCppt(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-3));
            BuatAlergi(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-2));
            BuatPersetujuan(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-1));

            using var konteksBaca = database.CreateContext();
            var service = Service(konteksBaca);

            var hanyaCppt = await service.GetTimelineAsync(
                Permintaan(konteks.PatientId, ClinicalDocumentKind.ProgressNote));

            Assert.Single(hanyaCppt.RequestedKinds);
            Assert.Equal(1, hanyaCppt.Page.TotalData);
            Assert.All(hanyaCppt.Page.Items,
                x => Assert.Equal(ClinicalDocumentKind.ProgressNote, x.DocumentKind));

            var duaJenis = await service.GetTimelineAsync(Permintaan(
                konteks.PatientId,
                ClinicalDocumentKind.ProgressNote,
                ClinicalDocumentKind.Allergy));

            Assert.Equal(2, duaJenis.RequestedKinds.Count);
            Assert.Equal(2, duaJenis.Page.TotalData);
            Assert.DoesNotContain(duaJenis.Page.Items,
                x => x.DocumentKind == ClinicalDocumentKind.Consent);

            // Daftar jenis yang kosong berarti seluruh tiga belas sumber, bukan nol sumber.
            var seluruhnya = await service.GetTimelineAsync(Permintaan(konteks.PatientId));
            Assert.Equal(13, seluruhnya.RequestedKinds.Count);
            Assert.Equal(3, seluruhnya.Page.TotalData);
        }

        // =====================================================================
        // Acceptance criteria 4 — satu sumber gagal
        // =====================================================================

        /// <summary>
        /// Bila satu sumber gagal dibaca, sumber lain tetap tampil dan yang gagal ditandai.
        ///
        /// Kegagalannya ditirukan dengan menghapus satu tabel dari basis data uji, sehingga
        /// query ke tabel itu benar-benar gagal — bukan disimulasikan lewat penanda.
        ///
        /// Pilihan perilaku ini disengaja: kehilangan seluruh riwayat pasien karena satu tabel
        /// bermasalah lebih berbahaya bagi pelayanan daripada kehilangan satu jenis dokumen,
        /// ASALKAN kekurangannya dinyatakan. Karena itu <c>IsComplete</c> ikut diperiksa.
        /// </summary>
        [Fact]
        public async Task SatuSumberGagal_SumberLainTetapTampilDanYangGagalDitandai()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            BuatCppt(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-2));
            BuatAlergi(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-1));
            BuatPersetujuan(context, konteks.PatientId, konteks.EncounterId, Sekarang);

            using var konteksBaca = database.CreateContext();

            // Tabel persetujuan dihilangkan supaya pembacaannya gagal sungguhan.
            var namaTabel = konteksBaca.Model
                .FindEntityType(typeof(TrxPatientConsent))!
                .GetTableName();

            // Nama tabel diambil dari model EF, bukan dari masukan siapa pun, dan dirangkai
            // tanpa interpolasi supaya tidak terbaca sebagai SQL yang dapat disusupi.
            await konteksBaca.Database.ExecuteSqlRawAsync("DROP TABLE \"" + namaTabel + "\"");

            var hasil = await Service(konteksBaca).GetTimelineAsync(Permintaan(konteks.PatientId));

            // Sumber yang sehat tetap terbaca.
            Assert.Equal(2, hasil.Page.TotalData);
            Assert.Contains(hasil.Page.Items, x => x.DocumentKind == ClinicalDocumentKind.ProgressNote);
            Assert.Contains(hasil.Page.Items, x => x.DocumentKind == ClinicalDocumentKind.Allergy);

            // Sumber yang gagal disebut namanya, bukan didiamkan.
            var gagal = Assert.Single(hasil.FailedSources);
            Assert.Equal(ClinicalDocumentKind.Consent, gagal.DocumentKind);
            Assert.Equal("Persetujuan Tindakan", gagal.DocumentKindName);
            Assert.False(string.IsNullOrWhiteSpace(gagal.Message));

            Assert.False(hasil.IsComplete);
        }

        // =====================================================================
        // Acceptance criteria 5 — AsNoTracking
        // =====================================================================

        /// <summary>
        /// Seluruh pembacaan memakai `AsNoTracking`.
        ///
        /// Dibuktikan dari akibatnya: setelah service dipanggil, tidak ada satu pun entity yang
        /// tertinggal terlacak di konteks. Ini penting karena service membaca tabel milik modul
        /// lain; entity yang terlacak berisiko ikut tersimpan pada penyimpanan berikutnya.
        /// </summary>
        [Fact]
        public async Task SeluruhPembacaan_TidakMeninggalkanEntityTerlacak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            BuatCppt(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-1));
            BuatAlergi(context, konteks.PatientId, konteks.EncounterId, Sekarang);

            using var konteksBaca = database.CreateContext();

            var hasil = await Service(konteksBaca).GetTimelineAsync(Permintaan(konteks.PatientId));

            Assert.Equal(2, hasil.Page.Items.Count);
            Assert.Empty(konteksBaca.ChangeTracker.Entries());
        }

        // =====================================================================
        // Status keutuhan dan dokumen yang dibatalkan
        // =====================================================================

        /// <summary>
        /// Status keutuhan ikut dikembalikan untuk jenis dokumen yang sudah tunduk aturan, dan
        /// jenis yang belum tunduk ditandai terbuka.
        ///
        /// Rilis pertama hanya menegakkan CPPT (`RM-DEC-019`). Keadaan itu WAJIB dapat dibaca
        /// dari data, karena layar harus menyatakannya sesuai `RM-FE-009` — bukan menampilkan
        /// alergi seolah-olah sudah terlindungi aturan keutuhan.
        /// </summary>
        [Fact]
        public async Task StatusKeutuhan_DitempelkanUntukJenisYangSudahDitegakkan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-1));
            BuatAlergi(context, konteks.PatientId, konteks.EncounterId, Sekarang);

            await new ClinicalDocumentIntegrityService(context).RegisterAsync(
                ClinicalDocumentKind.ProgressNote,
                cppt.Id,
                konteks.PatientId,
                konteks.EncounterId,
                penulis.Id);

            await context.SaveChangesAsync();

            using var konteksBaca = database.CreateContext();
            var hasil = await Service(konteksBaca).GetTimelineAsync(Permintaan(konteks.PatientId));

            var barisCppt = hasil.Page.Items.Single(
                x => x.DocumentKind == ClinicalDocumentKind.ProgressNote);
            Assert.True(barisCppt.IsIntegrityEnforced);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, barisCppt.IntegrityStatus);
            Assert.Equal("Draf", barisCppt.IntegrityStatusName);

            var barisAlergi = hasil.Page.Items.Single(
                x => x.DocumentKind == ClinicalDocumentKind.Allergy);
            Assert.False(barisAlergi.IsIntegrityEnforced);
            Assert.Null(barisAlergi.IntegrityStatus);
        }

        /// <summary>
        /// Dokumen yang dibatalkan tidak ikut tampil kecuali memang diminta.
        ///
        /// Mengikuti perilaku endpoint riwayat CPPT yang sudah berjalan, supaya dua layar tidak
        /// menampilkan jumlah dokumen yang berbeda untuk pasien yang sama.
        /// </summary>
        [Fact]
        public async Task DokumenDibatalkan_TidakTampilKecualiDiminta()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            BuatCppt(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-2));
            BuatCppt(context, konteks.PatientId, konteks.EncounterId, Sekarang.AddDays(-1),
                dibatalkan: true);

            using var konteksBaca = database.CreateContext();
            var service = Service(konteksBaca);

            var bawaan = await service.GetTimelineAsync(
                Permintaan(konteks.PatientId, ClinicalDocumentKind.ProgressNote));

            Assert.Equal(1, bawaan.Page.TotalData);
            Assert.All(bawaan.Page.Items, x => Assert.False(x.IsCancelled));

            var termasukDibatalkan = Permintaan(konteks.PatientId, ClinicalDocumentKind.ProgressNote);
            termasukDibatalkan.IncludeCancelled = true;

            var lengkap = await service.GetTimelineAsync(termasukDibatalkan);

            Assert.Equal(2, lengkap.Page.TotalData);
            Assert.Contains(lengkap.Page.Items, x => x.IsCancelled);
        }

        /// <summary>
        /// Riwayat pasien lain tidak pernah ikut terbawa.
        ///
        /// Diperiksa tersendiri karena penyaring pasien pada service ini dibangun secara umum
        /// untuk tiga belas tabel sekaligus. Kekeliruan di satu tempat itu akan bocor ke seluruh
        /// sumber, dan bocornya berupa rekam medis orang lain.
        /// </summary>
        [Fact]
        public async Task RiwayatPasienLain_TidakIkutTerbawa()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var pasienA = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var pasienB = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            BuatCppt(context, pasienA.PatientId, pasienA.EncounterId, Sekarang.AddDays(-1));
            BuatCppt(context, pasienB.PatientId, pasienB.EncounterId, Sekarang.AddDays(-1));
            BuatAlergi(context, pasienB.PatientId, pasienB.EncounterId, Sekarang);

            using var konteksBaca = database.CreateContext();
            var hasil = await Service(konteksBaca).GetTimelineAsync(Permintaan(pasienA.PatientId));

            Assert.Equal(1, hasil.Page.TotalData);
            Assert.Equal(pasienA.EncounterId, hasil.Page.Items.Single().EncounterId);
        }

        /// <summary>
        /// Permintaan tanpa id pasien ditolak, bukan dijawab daftar kosong.
        ///
        /// Daftar kosong akan terbaca sebagai "pasien ini memang tidak punya riwayat", dan itu
        /// keterangan yang menyesatkan pada berkas rekam medis.
        /// </summary>
        [Fact]
        public async Task TanpaIdPasien_PermintaanDitolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => Service(context).GetTimelineAsync(Permintaan(Guid.Empty)));
        }
    }
}
