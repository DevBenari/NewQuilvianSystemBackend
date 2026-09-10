using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-058</c> dan <c>BE-RWI-064</c> — perkembangan pasien
    /// terbaca sebagai satu garis waktu, dan kepala ruangan menemukan pengkajian yang tertinggal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kedua task diuji berdampingan karena keduanya membaca angka yang sama: tenggat yang sudah
    /// tersimpan pada pengkajian. Yang dijaga paling ketat adalah <b>pembedaan tiga keadaan
    /// kosong</b> — belum ada kebijakan, sudah tepat waktu, dan tidak ada data — karena ketiganya
    /// terlihat sama di layar tetapi menuntut tindakan yang berbeda dari kepala ruangan.
    /// </para>
    /// </remarks>
    public class NursingAssessmentMonitoringTests
    {
        private static NursingAssessmentMonitoringService BuatService(ApplicationDbContext c) =>
            new(c, new ClinicalAssessmentPolicyService(c));

        private static MstClinicalAssessmentPolicy BuatKebijakan(
            ApplicationDbContext context,
            int menit,
            ServiceUnitType? jenisPelayanan = ServiceUnitType.Inpatient,
            DateTime? berlakuMulai = null,
            DateTime? berlakuSampai = null,
            string? kode = null)
        {
            var kebijakan = new MstClinicalAssessmentPolicy
            {
                PolicyCode = kode ?? $"KEP-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
                PolicyName = "Kebijakan uji",
                AssessmentType = PatientAssessmentType.Initial,
                ServiceUnitType = jenisPelayanan,
                DueWithinMinutes = menit,
                EffectiveFrom = berlakuMulai ?? DateTime.UtcNow.AddDays(-30),
                EffectiveTo = berlakuSampai,
                IsActive = true
            };

            context.Set<MstClinicalAssessmentPolicy>().Add(kebijakan);
            context.SaveChanges();

            return kebijakan;
        }

        private static TrxPatientAssessment BuatPengkajian(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k,
            PatientAssessmentType jenis,
            DateTime waktu,
            int? skalaNyeri = null,
            FallRiskStatus risikoJatuh = FallRiskStatus.NoRisk,
            NutritionRiskStatus risikoGizi = NutritionRiskStatus.LowRisk,
            DateTime? tenggat = null,
            DateTime? selesai = null,
            Guid? policyId = null,
            Guid? penulisId = null)
        {
            var pengkajian = new TrxPatientAssessment
            {
                AssessmentNumber = $"ASM-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                InpEpisodeId = k.EpisodeId,
                AssessmentType = jenis,
                AssessmentDateTime = waktu,
                AssessmentStatus = selesai.HasValue
                    ? PatientAssessmentStatus.Completed
                    : PatientAssessmentStatus.InProgress,
                CompletedAt = selesai,
                DueAt = tenggat,
                PolicyId = policyId,
                AssessmentByUserId = penulisId,
                HasPain = skalaNyeri.HasValue,
                PainScale = skalaNyeri,
                FallRiskStatus = risikoJatuh,
                NutritionRiskStatus = risikoGizi
            };

            context.Set<TrxPatientAssessment>().Add(pengkajian);
            context.SaveChanges();

            return pengkajian;
        }

        // =====================================================================
        // BE-RWI-058 kriteria 1 dan 2 — seluruh pengukuran, nilai lama tidak ditimpa
        // =====================================================================

        /// <summary>
        /// `BE-RWI-058 AC 1 dan 2` — lini masa menampilkan <b>seluruh</b> pengukuran nyeri terurut
        /// waktu, bukan hanya yang terakhir, dan nilai lama tetap utuh.
        /// </summary>
        /// <remarks>
        /// <b>Contoh yang diuji.</b> Nyeri Tn. Budi tercatat 8 pada hari pertama, 5 pada hari
        /// kedua, dan 2 pada hari ketiga. Yang perlu dibaca perawat bukan angka 2, melainkan
        /// bahwa nyerinya <i>membaik</i> — dan itu hanya terlihat bila ketiganya tampil.
        /// </remarks>
        [Fact]
        public async Task LiniMasa_MenampilkanSeluruhPengukuranTerurutWaktu()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var dasar = DateTime.UtcNow.AddDays(-3);

            BuatPengkajian(context, k, PatientAssessmentType.Initial, dasar, skalaNyeri: 8);
            BuatPengkajian(context, k, PatientAssessmentType.DailyReassessment, dasar.AddDays(1), skalaNyeri: 5);
            BuatPengkajian(context, k, PatientAssessmentType.DailyReassessment, dasar.AddDays(2), skalaNyeri: 2);

            var hasil = await BuatService(context).GetTimelineAsync(k.EpisodeId);

            Assert.NotNull(hasil);
            Assert.Equal(3, hasil!.TotalAssessment);
            Assert.Equal(3, hasil.Entries.Count);

            var nyeri = hasil.Series.Single(x => x.Measurement == "pain");

            Assert.Equal(3, nyeri.Points.Count);
            Assert.Equal([8, 5, 2], nyeri.Points.Select(x => x.Score));
            Assert.True(nyeri.Points[0].MeasuredAt < nyeri.Points[1].MeasuredAt);
            Assert.True(nyeri.Points[1].MeasuredAt < nyeri.Points[2].MeasuredAt);
        }

        /// <summary>
        /// Ketiga deret pengukuran yang dituntut <c>FR-KEP-007</c> tersedia: nyeri, risiko jatuh,
        /// dan skrining gizi.
        /// </summary>
        [Fact]
        public async Task LiniMasa_MemuatTigaDeretPengukuran()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            BuatPengkajian(context, k, PatientAssessmentType.Initial, DateTime.UtcNow.AddHours(-2),
                skalaNyeri: 4, risikoJatuh: FallRiskStatus.HighRisk,
                risikoGizi: NutritionRiskStatus.MediumRisk);

            var hasil = await BuatService(context).GetTimelineAsync(k.EpisodeId);

            Assert.NotNull(hasil);
            Assert.Equal(["pain", "fallRisk", "nutrition"],
                hasil!.Series.Select(x => x.Measurement));
            Assert.All(hasil.Series, x => Assert.Single(x.Points));
        }

        /// <summary>Lini masa perawatan yang tidak ada dijawab kosong, bukan galat.</summary>
        [Fact]
        public async Task LiniMasaPerawatanTidakDikenal_Kosong()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            Assert.Null(await BuatService(context).GetTimelineAsync(Guid.NewGuid()));
        }

        // =====================================================================
        // BE-RWI-058 kriteria 3 — tenggat memakai kebijakan saat pengkajian dibuat
        // =====================================================================

        /// <summary>
        /// `BE-RWI-058 AC 3` — mengubah kebijakan <b>tidak</b> mengubah penilaian pengkajian yang
        /// lalu.
        /// </summary>
        /// <remarks>
        /// <b>Contoh berangka.</b> Batas semula 1440 menit. Pengkajian Tn. Budi selesai pada menit
        /// ke-1200, jadi tepat waktu. Kebijakan lalu diperketat menjadi 480 menit. Pengkajian Tn.
        /// Budi <b>tetap</b> tepat waktu, karena tenggat miliknya sudah tersimpan sejak ia dibuat
        /// — <c>AC-CAP012-04</c>.
        /// </remarks>
        [Fact]
        public async Task PenilaianPengkajianLama_TidakBerubahSaatKebijakanDiperketat()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var kebijakanLama = BuatKebijakan(context, menit: 1440, kode: "KEP-LAMA");

            var dibuatPada = DateTime.UtcNow.AddDays(-2);

            var pengkajian = BuatPengkajian(context, k, PatientAssessmentType.Initial, dibuatPada,
                tenggat: dibuatPada.AddMinutes(1440),
                selesai: dibuatPada.AddMinutes(1200),
                policyId: kebijakanLama.Id);

            var sebelum = await BuatService(context).GetDueStatusAsync(k.EpisodeId);

            Assert.Equal(AssessmentDueState.OnTime, sebelum!.Items.Single().DueState);

            // Kebijakan diperketat hari ini.
            BuatKebijakan(context, menit: 480, kode: "KEP-BARU",
                berlakuMulai: DateTime.UtcNow.AddMinutes(-1));

            using var pembaca = database.CreateContext();
            var sesudah = await BuatService(pembaca).GetDueStatusAsync(k.EpisodeId);

            var baris = sesudah!.Items.Single();

            Assert.Equal(AssessmentDueState.OnTime, baris.DueState);
            Assert.Equal(pengkajian.DueAt, baris.DueAt);
            Assert.Equal("KEP-LAMA", baris.PolicyCode);
            Assert.Null(baris.LateByMinutes);
        }

        /// <summary>
        /// Pengkajian yang selesai melewati tenggat terbaca <c>Late</c> beserta selisih menitnya.
        /// </summary>
        [Fact]
        public async Task PengkajianSelesaiMelewatiTenggat_TerbacaTerlambatBesertaSelisihnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var kebijakan = BuatKebijakan(context, menit: 1440);

            var dibuatPada = DateTime.UtcNow.AddDays(-3);

            BuatPengkajian(context, k, PatientAssessmentType.Initial, dibuatPada,
                tenggat: dibuatPada.AddMinutes(1440),
                selesai: dibuatPada.AddMinutes(1560),
                policyId: kebijakan.Id);

            var hasil = await BuatService(context).GetDueStatusAsync(k.EpisodeId);

            var baris = hasil!.Items.Single();

            Assert.Equal(AssessmentDueState.Late, baris.DueState);
            Assert.Equal(120, baris.LateByMinutes);
            Assert.Equal(1, hasil.LateCount);
            Assert.Contains("melewati tenggat", hasil.Explanation);
        }

        // =====================================================================
        // BE-RWI-058 kriteria 4 — master kosong bukan "terlambat"
        // =====================================================================

        /// <summary>
        /// `BE-RWI-058 AC 4` — master kebijakan kosong menghasilkan keadaan "tidak dipantau",
        /// <b>bukan</b> "terlambat" (<c>VAL-KEP-17</c>).
        /// </summary>
        [Fact]
        public async Task MasterKebijakanKosong_MenghasilkanTidakDipantau()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var lama = DateTime.UtcNow.AddDays(-10);

            BuatPengkajian(context, k, PatientAssessmentType.Initial, lama, selesai: lama.AddDays(5));

            var hasil = await BuatService(context).GetDueStatusAsync(k.EpisodeId);

            Assert.True(hasil!.IsPolicyMasterEmpty);
            Assert.Equal("Batas waktu pengkajian belum ditetapkan.", hasil.Explanation);
            Assert.Equal(AssessmentDueState.NotMonitored, hasil.Items.Single().DueState);
            Assert.Equal(0, hasil.LateCount);
            Assert.Equal(1, hasil.NotMonitoredCount);
        }

        // =====================================================================
        // BE-RWI-058 kriteria 5 — baris yang pernah dikoreksi
        // =====================================================================

        /// <summary>
        /// `BE-RWI-058 AC 5` — baris yang pernah dikoreksi membawa nomor urut addendum-nya, dan
        /// isi aslinya tetap tampil apa adanya.
        /// </summary>
        [Fact]
        public async Task BarisYangPernahDikoreksi_MembawaNomorUrutDanIsiAslinyaTetapTampil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    InpEpisodeId = k.EpisodeId,
                    AssessmentType = PatientAssessmentType.Initial,
                    HasPain = true,
                    PainScale = 3,
                    FallRiskStatus = FallRiskStatus.NoRisk,
                    NutritionRiskStatus = NutritionRiskStatus.LowRisk
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>().SingleAsync();

            Assert.Equal(200, ControllerTestHarness.KodeStatus(
                await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                    .CompleteAssessment(pengkajian.Id, new CompletePatientAssessmentRequest())));

            Assert.Equal(201, ControllerTestHarness.KodeStatus(
                await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                    .CreateAddendum(pengkajian.Id, new CreateAssessmentAddendumRequest
                    {
                        Content = "Skala nyeri seharusnya 7.",
                        Reason = "Salah ketik"
                    })));

            using var pembaca = database.CreateContext();
            var hasil = await BuatService(pembaca).GetTimelineAsync(k.EpisodeId);

            var baris = hasil!.Entries.Single();

            Assert.Equal(1, baris.AddendumCount);
            Assert.Equal(1, baris.LastAddendumSequence);

            // Isi aslinya tetap 3, bukan tergantikan isi koreksinya.
            Assert.Equal(3, baris.PainScale);
            Assert.Equal(3, hasil.Series.Single(x => x.Measurement == "pain").Points.Single().Score);
        }

        // =====================================================================
        // BE-RWI-064 — daftar pantau kepatuhan pengkajian awal
        // =====================================================================

        /// <summary>
        /// `BE-RWI-064 AC 1` — daftar memuat perawatan yang pengkajian awalnya belum ada.
        /// </summary>
        [Fact]
        public async Task DaftarPantau_MemuatPerawatanTanpaPengkajianAwal()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            BuatKebijakan(context, menit: 1440);

            var hasil = await BuatService(context)
                .GetInitialAssessmentComplianceAsync(null, onlyLate: false, 1, 25);

            Assert.Equal(1, hasil.TotalData);

            var baris = hasil.Items.Single();

            Assert.Equal(k.EpisodeId, baris.EpisodeId);
            Assert.False(baris.HasInitialAssessment);
            Assert.Null(baris.AssessmentId);
            Assert.Equal("Pasien Uji", baris.PatientName);
            Assert.Equal("Rawat Inap Uji", baris.ServiceUnitName);
        }

        /// <summary>
        /// `BE-RWI-064 AC 1` — perawatan yang pengkajian awalnya sudah lewat tenggat terbaca
        /// terlambat, dan yang tepat waktu tidak ikut muncul.
        /// </summary>
        [Fact]
        public async Task DaftarPantau_MemuatYangTerlambatDanMenyembunyikanYangTepatWaktu()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var kebijakan = BuatKebijakan(context, menit: 1440);

            var terlambat = RawatInapTestData.SiapkanPerawatan(context);
            var tepatWaktu = RawatInapTestData.SiapkanPerawatan(context);

            var lama = DateTime.UtcNow.AddDays(-3);

            BuatPengkajian(context, terlambat, PatientAssessmentType.Initial, lama,
                tenggat: lama.AddMinutes(1440),
                selesai: lama.AddMinutes(2000),
                policyId: kebijakan.Id);

            BuatPengkajian(context, tepatWaktu, PatientAssessmentType.Initial, lama,
                tenggat: lama.AddMinutes(1440),
                selesai: lama.AddMinutes(600),
                policyId: kebijakan.Id);

            var hasil = await BuatService(context)
                .GetInitialAssessmentComplianceAsync(null, onlyLate: false, 1, 25);

            var baris = Assert.Single(hasil.Items);

            Assert.Equal(terlambat.EpisodeId, baris.EpisodeId);
            Assert.Equal(AssessmentDueState.Late, baris.DueState);
            Assert.Equal(560, baris.LateByMinutes);
            Assert.True(baris.HasInitialAssessment);
        }

        /// <summary>
        /// `BE-RWI-064 AC 2` — daftar kosong karena semua tepat waktu dibedakan dari daftar
        /// kosong karena kebijakan belum ditetapkan.
        /// </summary>
        /// <remarks>
        /// Keduanya menghasilkan nol baris, tetapi menuntut tindakan yang berbeda: yang pertama
        /// berarti tidak ada pekerjaan tertinggal, yang kedua berarti pemantauannya sendiri belum
        /// menyala. Menyamakan keduanya menjadi "tidak ada data" menyembunyikan hal itu —
        /// <c>FR-KEP-025</c>.
        /// </remarks>
        [Fact]
        public async Task DaftarPantauKosong_DibedakanMenurutSebabnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var kebijakan = BuatKebijakan(context, menit: 1440);
            var k = RawatInapTestData.SiapkanPerawatan(context);

            var lama = DateTime.UtcNow.AddDays(-3);

            BuatPengkajian(context, k, PatientAssessmentType.Initial, lama,
                tenggat: lama.AddMinutes(1440),
                selesai: lama.AddMinutes(600),
                policyId: kebijakan.Id);

            var service = BuatService(context);

            var tepatWaktu = await service.GetInitialAssessmentComplianceAsync(null, false, 1, 25);
            Assert.Equal(0, tepatWaktu.TotalData);
            Assert.Equal(
                "Seluruh pengkajian awal sudah tepat waktu.",
                await service.JelaskanDaftarPantauAsync(tepatWaktu.TotalData));

            // Keadaan kedua: master kebijakan dikosongkan.
            using var tanpaKebijakan = database.CreateContext();
            var barisKebijakan = await tanpaKebijakan.Set<MstClinicalAssessmentPolicy>().SingleAsync();
            barisKebijakan.IsActive = false;
            await tanpaKebijakan.SaveChangesAsync();

            var serviceKedua = BuatService(tanpaKebijakan);
            var kosong = await serviceKedua.GetInitialAssessmentComplianceAsync(null, false, 1, 25);

            Assert.Equal(
                "Batas waktu pengkajian belum ditetapkan.",
                await serviceKedua.JelaskanDaftarPantauAsync(kosong.TotalData));
        }

        /// <summary>
        /// `BE-RWI-064 AC 3` — keterlambatan pengkajian <b>tidak menahan</b> pencatatan lain.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>INV-KEP-03</c>, <c>VAL-KEP-18</c>. Daftar pantau yang memblokir pekerjaan klinis
        /// hanya akan mendorong perawat mengakali sistem, dan itu justru menurunkan keselamatan.
        /// </para>
        /// <para>
        /// Tabel tindakan keperawatan baru lahir pada <c>BE-RWI-061</c>. Yang dapat dibuktikan
        /// hari ini adalah pencatatan dokumen keperawatan berikutnya pada perawatan yang sedang
        /// terlambat tetap diterima, dan perawatan itu tetap muncul pada daftar pantau — daftar
        /// memantau, bukan menggerbang.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task Keterlambatan_TidakMenahanPencatatanBerikutnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var kebijakan = BuatKebijakan(context, menit: 1440);
            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var lama = DateTime.UtcNow.AddDays(-5);

            BuatPengkajian(context, k, PatientAssessmentType.Initial, lama,
                tenggat: lama.AddMinutes(1440),
                policyId: kebijakan.Id);

            var sebelum = await BuatService(context)
                .GetInitialAssessmentComplianceAsync(null, onlyLate: true, 1, 25);

            Assert.Equal(1, sebelum.TotalData);

            // Pencatatan berikutnya tetap diterima walaupun perawatan ini sedang terlambat.
            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    InpEpisodeId = k.EpisodeId,
                    AssessmentType = PatientAssessmentType.DailyReassessment,
                    FallRiskStatus = FallRiskStatus.NoRisk,
                    NutritionRiskStatus = NutritionRiskStatus.LowRisk
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();
            var sesudah = await BuatService(pembaca)
                .GetInitialAssessmentComplianceAsync(null, onlyLate: true, 1, 25);

            Assert.Equal(1, sesudah.TotalData);
        }

        /// <summary>
        /// `BE-RWI-064 AC 4` — daftar tidak menampilkan satu pun isi klinis.
        /// </summary>
        /// <remarks>
        /// Kepala ruangan memakainya untuk menemukan pekerjaan yang tertinggal, bukan untuk
        /// membaca rekam medis pasien. Karena itu bentuk balasannya sengaja tidak memiliki
        /// properti keluhan, nyeri, gizi, maupun catatan apa pun.
        /// </remarks>
        [Fact]
        public void DaftarPantau_TidakMemuatIsiKlinis()
        {
            var properti = typeof(InitialAssessmentComplianceItemResponse)
                .GetProperties()
                .Select(x => x.Name)
                .ToList();

            foreach (var terlarang in new[]
                     {
                         "ChiefComplaint", "PainScale", "HasPain", "NutritionRiskStatus",
                         "FallRiskStatus", "NurseNote", "PsychosocialNote", "EducationNote"
                     })
            {
                Assert.DoesNotContain(terlarang, properti);
            }

            Assert.Contains("PatientName", properti);
            Assert.Contains("ServiceUnitName", properti);
            Assert.Contains("LateByMinutes", properti);
        }

        /// <summary>Penyaring unit perawatan mempersempit daftar sebagaimana dijanjikan.</summary>
        [Fact]
        public async Task DaftarPantau_MenyaringMenurutUnitPerawatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            BuatKebijakan(context, menit: 1440);

            var pertama = RawatInapTestData.SiapkanPerawatan(context);
            RawatInapTestData.SiapkanPerawatan(context);

            var service = BuatService(context);

            var seluruh = await service.GetInitialAssessmentComplianceAsync(null, false, 1, 25);
            var tersaring = await service.GetInitialAssessmentComplianceAsync(
                pertama.ServiceUnitId, false, 1, 25);

            Assert.Equal(2, seluruh.TotalData);
            Assert.Equal(1, tersaring.TotalData);
            Assert.Equal(pertama.EpisodeId, tersaring.Items.Single().EpisodeId);
        }

        /// <summary>
        /// Perhitungan keadaan tenggat mengikuti urutan yang mengikat: tanpa tenggat tidak ada
        /// penilaian sama sekali.
        /// </summary>
        [Theory]
        [InlineData(false, false, AssessmentDueState.NotMonitored)]
        [InlineData(true, true, AssessmentDueState.OnTime)]
        [InlineData(true, false, AssessmentDueState.Pending)]
        public void HitungKeadaan_MengikutiUrutanYangMengikat(
            bool punyaTenggat, bool sudahSelesai, AssessmentDueState diharapkan)
        {
            var sekarang = new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc);
            var tenggat = punyaTenggat ? sekarang.AddHours(4) : (DateTime?)null;
            var selesai = sudahSelesai ? sekarang.AddHours(1) : (DateTime?)null;
            var status = sudahSelesai
                ? PatientAssessmentStatus.Completed
                : PatientAssessmentStatus.InProgress;

            var (keadaan, terlambat) = NursingAssessmentMonitoringService.HitungKeadaan(
                tenggat, selesai, status, sekarang);

            Assert.Equal(diharapkan, keadaan);
            Assert.Null(terlambat);
        }
    }
}
