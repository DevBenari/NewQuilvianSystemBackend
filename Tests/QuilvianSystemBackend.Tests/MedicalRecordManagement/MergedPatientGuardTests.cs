using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-16` — pengaman pasien bernomor rekam medis ganda.
    ///
    /// Menutup uji penerimaan `AT-RM-22` beserta kedua acceptance criteria `BE-16`:
    /// <list type="number">
    /// <item>pasien dengan `MergedToPatientId` terisi dijawab `409` disertai nomor rekam medis
    /// pengganti;</item>
    /// <item>riwayat sebagian **tidak** ditampilkan.</item>
    /// </list>
    ///
    /// LATAR YANG PERLU DIPAHAMI. Penggabungan pasien di sistem ini hanya berupa **penandaan**
    /// (`RM-FACT-007`). Menyetel `MergedToPatientId` tidak memindahkan satu pun data klinis, dan
    /// tidak ada query di modul mana pun yang mengikutinya. Akibatnya pasti, bukan kemungkinan:
    /// riwayat pasien bernomor ganda **selalu** terpecah.
    ///
    /// `RM-DEC-026` memilih menolak membuka berkasnya, bukan menyatukan saat dibaca maupun
    /// memindahkan data klinis. Alasannya: riwayat tidak lengkap yang tampil tanpa peringatan
    /// akan dibaca sebagai riwayat lengkap — dan itu lebih berbahaya daripada tidak dapat
    /// membuka sama sekali.
    ///
    /// Seluruh data di sini adalah data karangan. Tidak ada data pasien sungguhan.
    /// </summary>
    public class MergedPatientGuardTests
    {
        private static MedicalRecordController BuatController(
            ApplicationDbContext context,
            Guid userId)
        {
            var controller = new MedicalRecordController(
                context,
                ControllerTestHarness.BuatLoggerService(userId),
                new MedicalRecordAccessAuditService(context),
                new MedicalRecordTimelineService(context));

            return controller.DenganPengguna(userId);
        }

        /// <summary>
        /// Menandai satu pasien sebagai hasil penggabungan ke pasien lain.
        /// </summary>
        private static void Gabungkan(ApplicationDbContext context, Guid dariPatientId, Guid kePatientId)
        {
            var pasien = context.Set<MstPatient>().First(x => x.Id == dariPatientId);
            pasien.MergedToPatientId = kePatientId;
            pasien.MergeReason = "Pasien terdaftar dua kali pada hari yang sama.";
            context.SaveChanges();
        }

        private static string NomorRekamMedis(ApplicationDbContext context, Guid patientId)
            => context.Set<MstPatient>().AsNoTracking().First(x => x.Id == patientId).MedicalRecordNumber;

        private static TrxPatientIntegratedProgressNote BuatCppt(
            ApplicationDbContext context,
            Guid patientId,
            Guid encounterId)
        {
            var cppt = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{Guid.NewGuid():N}"[..20],
                PatientId = patientId,
                EncounterId = encounterId,
                ProfessionType = "Doctor",
                SubjectiveSummary = "Catatan pada nomor rekam medis lama.",
                NoteDateTime = DateTime.UtcNow.AddDays(-3)
            };

            context.Set<TrxPatientIntegratedProgressNote>().Add(cppt);
            context.SaveChanges();
            return cppt;
        }

        private static void PastikanTanpaIsi(IActionResult hasil)
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var body = Assert.IsType<ApiResponse<object>>(objek.Value);

            Assert.False(body.Success);
            Assert.Null(body.Data);
        }

        // =====================================================================
        // AT-RM-22 — seluruh pintu masuk berkas rekam medis
        // =====================================================================

        /// <summary>
        /// `AT-RM-22`: membuka rekam medis pasien yang `MergedToPatientId`-nya terisi ditolak
        /// `409` disertai nomor rekam medis pengganti, pada **seluruh** pintu masuk berkas.
        ///
        /// Diperiksa keempat-empatnya, bukan hanya riwayat. Satu pintu yang lupa dijaga sudah
        /// cukup untuk menampilkan riwayat terpecah — dan pengaman yang berlubang di satu tempat
        /// bukan pengaman.
        /// </summary>
        [Fact]
        public async Task PasienHasilPenggabungan_DitolakPadaSeluruhPintuMasukBerkas()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var lama = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var baru = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            var cppt = BuatCppt(context, lama.PatientId, lama.EncounterId);
            Gabungkan(context, lama.PatientId, baru.PatientId);

            var nomorPengganti = NomorRekamMedis(context, baru.PatientId);
            var controller = BuatController(context, dokter.Id);

            var hasil = new (string Nama, IActionResult Balasan)[]
            {
                ("ringkasan", await controller.GetSummary(lama.PatientId)),
                ("riwayat", await controller.GetTimeline(lama.PatientId)),
                ("detail dokumen", await controller.GetDocumentDetail(
                    lama.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id)),
                ("catatan pribadi", await controller.GetPrivateNote(
                    lama.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id))
            };

            foreach (var (nama, balasan) in hasil)
            {
                Assert.Equal(StatusCodes.Status409Conflict, ControllerTestHarness.KodeStatus(balasan));

                var pesan = ControllerTestHarness.Pesan(balasan);
                Assert.NotNull(pesan);

                // Acceptance criteria 1: nomor penggantinya benar-benar disebut.
                Assert.Contains(nomorPengganti, pesan!);

                // Acceptance criteria 2: tidak ada isi rekam medis yang ikut terbawa.
                PastikanTanpaIsi(balasan);
            }
        }

        /// <summary>
        /// Riwayat sebagian tidak ditampilkan — walaupun datanya benar-benar ada.
        ///
        /// Ini penegasan acceptance criteria nomor 2. Pasien lama pada uji ini memang punya
        /// CPPT, dan tanpa pengaman ini CPPT itu akan tampil sebagai "riwayat lengkap" padahal
        /// hanya sebagian.
        /// </summary>
        [Fact]
        public async Task RiwayatSebagian_TidakDitampilkanWalaupunDatanyaAda()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var lama = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var baru = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            BuatCppt(context, lama.PatientId, lama.EncounterId);
            BuatCppt(context, lama.PatientId, lama.EncounterId);

            // Sebelum digabungkan, riwayatnya memang terbaca.
            var controller = BuatController(context, dokter.Id);
            var sebelum = await controller.GetTimeline(lama.PatientId);
            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(sebelum));

            Gabungkan(context, lama.PatientId, baru.PatientId);

            using var konteksSetelah = database.CreateContext();
            var controllerSetelah = BuatController(konteksSetelah, dokter.Id);
            var sesudah = await controllerSetelah.GetTimeline(lama.PatientId);

            Assert.Equal(StatusCodes.Status409Conflict, ControllerTestHarness.KodeStatus(sesudah));
            PastikanTanpaIsi(sesudah);
        }

        /// <summary>
        /// Penolakan `409` tidak menghasilkan jejak akses.
        ///
        /// Tidak ada isi rekam medis yang dibaca, jadi tidak ada pembacaan yang perlu dicatat.
        /// Mencatatnya akan mengotori angka tinjauan dengan pembukaan yang tidak pernah terjadi.
        /// </summary>
        [Fact]
        public async Task Penolakan409_TidakMenghasilkanJejakAkses()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var lama = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var baru = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            Gabungkan(context, lama.PatientId, baru.PatientId);

            var controller = BuatController(context, dokter.Id);
            await controller.GetSummary(lama.PatientId);
            await controller.GetTimeline(lama.PatientId);

            using var konteksBaca = database.CreateContext();
            Assert.Empty(await konteksBaca.Set<TrxMedicalRecordAccessLog>().AsNoTracking().ToListAsync());
        }

        /// <summary>
        /// Nomor rekam medis pengganti yang disebut adalah nomor yang **benar-benar dapat
        /// dibuka**, bukan sekadar tujuan penggabungan satu langkah.
        ///
        /// Penggabungan berantai mungkin terjadi: pemeriksaan saat menggabungkan hanya memastikan
        /// pasien tujuan ada dan aktif, tanpa memeriksa apakah tujuan itu kelak ikut digabungkan.
        /// Bila A digabung ke B dan B digabung ke C, menyebut nomor B berarti menyuruh pengguna
        /// membuka berkas yang juga akan ditolak — petunjuk menyesatkan yang membuat sistem
        /// terlihat rusak.
        /// </summary>
        [Fact]
        public async Task PenggabunganBerantai_MenyebutNomorUjungRantai()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var a = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var b = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var c = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            Gabungkan(context, a.PatientId, b.PatientId);
            Gabungkan(context, b.PatientId, c.PatientId);

            var nomorB = NomorRekamMedis(context, b.PatientId);
            var nomorC = NomorRekamMedis(context, c.PatientId);

            var controller = BuatController(context, dokter.Id);
            var hasil = await controller.GetTimeline(a.PatientId);

            Assert.Equal(StatusCodes.Status409Conflict, ControllerTestHarness.KodeStatus(hasil));

            var pesan = ControllerTestHarness.Pesan(hasil)!;

            // Yang disebut adalah C — nomor yang benar-benar dapat dibuka.
            Assert.Contains(nomorC, pesan);
            Assert.DoesNotContain(nomorB, pesan);
        }

        /// <summary>
        /// Rantai penggabungan yang melingkar tidak membuat permintaan berjalan tanpa akhir.
        ///
        /// Keadaan ini seharusnya tidak pernah terjadi, tetapi tidak ada satu pun aturan di
        /// sistem yang mencegahnya. Pengaman yang menggantung selamanya lebih berbahaya daripada
        /// pengaman yang menyerah dengan jawaban seadanya.
        /// </summary>
        [Fact]
        public async Task RantaiPenggabunganMelingkar_TetapDijawabTanpaMenggantung()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var a = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var b = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            // A ke B, lalu B kembali ke A.
            Gabungkan(context, a.PatientId, b.PatientId);
            Gabungkan(context, b.PatientId, a.PatientId);

            var controller = BuatController(context, dokter.Id);
            var hasil = await controller.GetTimeline(a.PatientId);

            Assert.Equal(StatusCodes.Status409Conflict, ControllerTestHarness.KodeStatus(hasil));
            PastikanTanpaIsi(hasil);

            // Tetap memberi nomor seadanya, bukan mengembalikan pesan kosong.
            Assert.Contains("rekam medis", ControllerTestHarness.Pesan(hasil)!);
        }

        /// <summary>
        /// Nomor pengganti selalu menunjuk pasien yang benar-benar ada — dijamin basis data.
        ///
        /// Diperiksa karena menentukan sifat sebuah cabang kode: `MergedToPatientId` memiliki
        /// foreign key sungguhan ke tabel pasien, sehingga menunjuk pasien yang tidak ada
        /// **ditolak basis data**. Cabang "nomor pengganti tidak diketahui" pada service karena
        /// itu bersifat pengaman terakhir, bukan jalur yang dapat dicapai pemakaian normal.
        ///
        /// Uji ini menjaga agar cabang itu tidak dibuang seseorang dengan anggapan mubazir,
        /// dan sekaligus mencatat jaminan integritas yang sedang diandalkan.
        /// </summary>
        [Fact]
        public void NomorPengganti_SelaluMenunjukPasienYangAda()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var lama = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var pasien = context.Set<MstPatient>().First(x => x.Id == lama.PatientId);
            pasien.MergedToPatientId = Guid.NewGuid();

            // Basis data menolak penggabungan ke pasien yang tidak ada.
            Assert.ThrowsAny<Exception>(() => context.SaveChanges());
        }

        /// <summary>
        /// Pasien tujuan penggabungan tetap dapat dibuka seperti biasa.
        ///
        /// Pengaman ini hanya berlaku pada nomor yang ditinggalkan. Bila nomor penggantinya ikut
        /// tertutup, pasiennya justru kehilangan seluruh berkas — kebalikan dari maksud
        /// `RM-DEC-026`.
        /// </summary>
        [Fact]
        public async Task PasienTujuanPenggabungan_TetapDapatDibuka()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var lama = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var baru = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            BuatCppt(context, baru.PatientId, baru.EncounterId);
            Gabungkan(context, lama.PatientId, baru.PatientId);

            var controller = BuatController(context, dokter.Id);
            var hasil = await controller.GetTimeline(baru.PatientId);

            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(hasil));
        }
    }
}
