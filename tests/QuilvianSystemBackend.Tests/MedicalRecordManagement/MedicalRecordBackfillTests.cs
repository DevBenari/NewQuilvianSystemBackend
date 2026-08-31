using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-08` — pengisian status keutuhan catatan lama.
    ///
    /// Menutup uji penerimaan `AT-RM-21` dan `AT-RM-33`.
    ///
    /// Uji ini yang membuat `BE-08` aman dijalankan pada data sungguhan. Aturan penentuan
    /// statusnya dibuktikan lebih dulu di sini, dengan seluruh keadaan yang mungkin ditemui,
    /// sebelum menyentuh satu baris pun data pasien.
    /// </summary>
    public class MedicalRecordBackfillTests
    {
        private static readonly DateTime Sekarang = new(2026, 8, 26, 12, 0, 0, DateTimeKind.Utc);

        private static MedicalRecordBackfillService Service(ApplicationDbContext c) => new(c);

        /// <summary>
        /// Membuat satu CPPT lama yang belum terdaftar pada daftar keutuhan.
        /// </summary>
        private static TrxPatientIntegratedProgressNote BuatCpptLama(
            ApplicationDbContext context,
            Guid patientId,
            Guid? encounterId,
            Guid? providerUserId,
            bool isCancel = false)
        {
            var cppt = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{Guid.NewGuid():N}"[..20],
                PatientId = patientId,
                EncounterId = encounterId,
                ProfessionType = "Doctor",
                ProviderUserId = providerUserId,
                SubjectiveSummary = "Catatan lama.",
                NoteDateTime = Sekarang.AddYears(-1),
                IsCancel = isCancel,
                CreateDateTime = Sekarang.AddYears(-1),
                CreateBy = providerUserId ?? Guid.NewGuid()
            };

            context.Set<TrxPatientIntegratedProgressNote>().Add(cppt);
            return cppt;
        }

        // =====================================================================
        // AT-RM-21 — penentuan status berdasarkan keadaan kunjungan
        // =====================================================================

        /// <summary>
        /// `AT-RM-21`: tiga keadaan berbeda menghasilkan tiga status berbeda, persis seperti
        /// RM-DEC-014.
        /// </summary>
        [Fact]
        public async Task PengisianDataLama_MemberiStatusSesuaiKeadaanKunjungannya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var selesai = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var berjalan = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);
            var dibatalkan = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);

            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            var cpptSelesai = BuatCpptLama(context, selesai.PatientId, selesai.EncounterId, penulis.Id);
            var cpptBerjalan = BuatCpptLama(context, berjalan.PatientId, berjalan.EncounterId, penulis.Id);
            var cpptDibatalkan = BuatCpptLama(context, dibatalkan.PatientId, dibatalkan.EncounterId,
                penulis.Id, isCancel: true);
            await context.SaveChangesAsync();

            var hasil = await Service(context).ExecuteBatchAsync(
                penulis.Id, Sekarang, batchSize: 100, isDryRun: false);

            Assert.Equal(3, hasil.JumlahDiproses);
            Assert.Equal(1, hasil.JumlahTerkunciTanpaTandaTangan);
            Assert.Equal(1, hasil.JumlahTetapDraf);
            Assert.Equal(1, hasil.JumlahDitandaiDibatalkan);

            var keutuhan = context.Set<TrxClinicalDocumentIntegrity>().AsNoTracking().ToList();

            var a = keutuhan.Single(x => x.DocumentId == cpptSelesai.Id);
            Assert.Equal(ClinicalDocumentIntegrityStatus.LockedUnsigned, a.IntegrityStatus);
            Assert.Equal(ClinicalDocumentLockTrigger.BackfillEncounterClosed, a.LockTrigger);

            var b = keutuhan.Single(x => x.DocumentId == cpptBerjalan.Id);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, b.IntegrityStatus);
            Assert.Null(b.LockTrigger);
            Assert.Null(b.LockedAt);

            var c = keutuhan.Single(x => x.DocumentId == cpptDibatalkan.Id);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Cancelled, c.IntegrityStatus);
        }

        /// <summary>
        /// Kunjungan yang berstatus batal atau tidak hadir juga dianggap sudah tidak berjalan,
        /// sehingga catatannya ikut terkunci.
        /// </summary>
        [Theory]
        [InlineData(EncounterStatus.Cancelled)]
        [InlineData(EncounterStatus.NoShow)]
        public async Task KunjunganBatalAtauTidakHadir_CatatannyaIkutTerkunci(EncounterStatus status)
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context, status);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            var cppt = BuatCpptLama(context, konteks.PatientId, konteks.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            await Service(context).ExecuteBatchAsync(penulis.Id, Sekarang, 100, isDryRun: false);

            var keutuhan = context.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking().Single(x => x.DocumentId == cppt.Id);

            Assert.Equal(ClinicalDocumentIntegrityStatus.LockedUnsigned, keutuhan.IntegrityStatus);
        }

        // =====================================================================
        // AT-RM-33 — catatan tanpa penulis
        // =====================================================================

        /// <summary>
        /// `AT-RM-33`: catatan yang penulisnya tidak tercatat tetap dibuat barisnya, dengan
        /// penanda penulis tidak diketahui.
        ///
        /// Melewatinya diam-diam akan menyembunyikan keadaan sebenarnya, dan catatan itu akan
        /// luput dari seluruh aturan penguncian tanpa ada yang tahu.
        /// </summary>
        [Fact]
        public async Task CatatanTanpaPenulis_TetapDibuatDenganPenandaPenulisTidakDiketahui()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var pelaksana = RekamMedisTestData.BuatPengguna(context, "petugas");

            var cppt = BuatCpptLama(context, konteks.PatientId, konteks.EncounterId,
                providerUserId: null);
            await context.SaveChangesAsync();

            var hasil = await Service(context).ExecuteBatchAsync(
                pelaksana.Id, Sekarang, 100, isDryRun: false);

            Assert.Equal(1, hasil.JumlahPenulisTidakDiketahui);

            var keutuhan = context.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking().Single(x => x.DocumentId == cppt.Id);

            Assert.False(keutuhan.IsAuthorKnown);
            Assert.Equal(ClinicalDocumentIntegrityStatus.LockedUnsigned, keutuhan.IntegrityStatus);
        }

        /// <summary>
        /// Catatan tanpa kunjungan dilewati, bukan digagalkan, dan jumlahnya dihitung terbuka.
        /// </summary>
        [Fact]
        public async Task CatatanTanpaKunjungan_DilewatiDanDihitungTerbuka()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "perawat");

            BuatCpptLama(context, konteks.PatientId, encounterId: null, penulis.Id);
            await context.SaveChangesAsync();

            var hasil = await Service(context).ExecuteBatchAsync(
                penulis.Id, Sekarang, 100, isDryRun: false);

            Assert.Equal(1, hasil.JumlahDiproses);
            Assert.Equal(1, hasil.JumlahDilewatiTanpaKunjungan);
            Assert.Equal(0, context.Set<TrxClinicalDocumentIntegrity>().Count());
        }

        // =====================================================================
        // Sifat yang membuat pengisian ini aman dijalankan
        // =====================================================================

        /// <summary>
        /// Percobaan tidak menyimpan apa pun, tetapi tetap melaporkan angka yang sama seperti
        /// bila dijalankan sungguhan.
        ///
        /// Inilah sifat yang membuat pengisian ini aman: hasilnya dapat dibuktikan lebih dulu
        /// tanpa menyentuh data.
        /// </summary>
        [Fact]
        public async Task Percobaan_MelaporkanAngkaYangSamaTanpaMenyimpanApaPun()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var selesai = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var berjalan = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            BuatCpptLama(context, selesai.PatientId, selesai.EncounterId, penulis.Id);
            BuatCpptLama(context, berjalan.PatientId, berjalan.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            var percobaan = await Service(context).ExecuteBatchAsync(
                penulis.Id, Sekarang, 100, isDryRun: true);

            Assert.True(percobaan.IsDryRun);
            Assert.Equal(2, percobaan.JumlahDiproses);
            Assert.Equal(1, percobaan.JumlahTerkunciTanpaTandaTangan);
            Assert.Equal(1, percobaan.JumlahTetapDraf);

            // Tidak ada satu baris pun tersimpan.
            Assert.Equal(0, context.Set<TrxClinicalDocumentIntegrity>().Count());

            var sungguhan = await Service(context).ExecuteBatchAsync(
                penulis.Id, Sekarang, 100, isDryRun: false);

            Assert.Equal(percobaan.JumlahDiproses, sungguhan.JumlahDiproses);
            Assert.Equal(percobaan.JumlahTerkunciTanpaTandaTangan,
                         sungguhan.JumlahTerkunciTanpaTandaTangan);
            Assert.Equal(2, context.Set<TrxClinicalDocumentIntegrity>().Count());
        }

        /// <summary>
        /// Catatan yang sudah terdaftar tidak diproses ulang.
        ///
        /// Diperlukan karena pengisian dijalankan bertahap dan dapat diulang bila terhenti.
        /// </summary>
        [Fact]
        public async Task CatatanYangSudahTerdaftar_TidakDiprosesUlang()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            BuatCpptLama(context, konteks.PatientId, konteks.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            var pertama = await Service(context).ExecuteBatchAsync(
                penulis.Id, Sekarang, 100, isDryRun: false);
            var kedua = await Service(context).ExecuteBatchAsync(
                penulis.Id, Sekarang.AddHours(1), 100, isDryRun: false);

            Assert.Equal(1, pertama.JumlahDiproses);
            Assert.Equal(0, kedua.JumlahDiproses);
            Assert.Equal(1, context.Set<TrxClinicalDocumentIntegrity>().Count());
        }

        /// <summary>
        /// Pengisian bertahap menyelesaikan seluruhnya bila dijalankan berulang.
        /// </summary>
        [Fact]
        public async Task PengisianBertahap_MenyelesaikanSeluruhnyaBilaDijalankanBerulang()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            for (var i = 0; i < 7; i++)
                BuatCpptLama(context, konteks.PatientId, konteks.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            var service = Service(context);
            var total = 0;
            var putaran = 0;

            while (putaran++ < 10)
            {
                var hasil = await service.ExecuteBatchAsync(
                    penulis.Id, Sekarang, batchSize: 2, isDryRun: false);

                total += hasil.JumlahDiproses;

                if (hasil.JumlahDiproses == 0)
                    break;
            }

            Assert.Equal(7, total);
            Assert.Equal(7, context.Set<TrxClinicalDocumentIntegrity>().Count());
        }

        // =====================================================================
        // Penelaahan
        // =====================================================================

        /// <summary>
        /// Penelaahan melaporkan keadaan apa adanya tanpa mengubah apa pun.
        ///
        /// Inilah yang menjawab pertanyaan yang tidak dapat dijawab dari source code: berapa
        /// banyak catatan lama yang ada, dan akan menjadi apa masing-masing.
        /// </summary>
        [Fact]
        public async Task Penelaahan_MelaporkanKeadaanTanpaMengubahApaPun()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var selesai = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var berjalan = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            BuatCpptLama(context, selesai.PatientId, selesai.EncounterId, penulis.Id);
            BuatCpptLama(context, selesai.PatientId, selesai.EncounterId, providerUserId: null);
            BuatCpptLama(context, berjalan.PatientId, berjalan.EncounterId, penulis.Id);
            BuatCpptLama(context, berjalan.PatientId, encounterId: null, penulis.Id);
            await context.SaveChangesAsync();

            var telaah = await Service(context).SurveyAsync(batchSize: 2);

            Assert.Equal(4, telaah.TotalProgressNote);
            Assert.Equal(4, telaah.BelumTerdaftar);
            Assert.Equal(0, telaah.SudahTerdaftar);
            Assert.Equal(2, telaah.AkanTerkunciTanpaTandaTangan);
            Assert.Equal(1, telaah.AkanTetapDraf);
            Assert.Equal(1, telaah.TanpaKunjungan);
            Assert.Equal(1, telaah.PenulisTidakDiketahui);

            // Tidak ada yang berubah.
            Assert.Equal(0, context.Set<TrxClinicalDocumentIntegrity>().Count());
        }

        /// <summary>
        /// Penelaahan menyertakan peringatan yang perlu dibaca sebelum pengisian dijalankan.
        ///
        /// Peringatan tentang angka besar pada laporan kelengkapan adalah yang paling penting:
        /// tanpa itu, unit rekam medis akan membaca angka tersebut sebagai kegagalan sistem
        /// baru, padahal itu gambaran keadaan sekarang.
        /// </summary>
        [Fact]
        public async Task Penelaahan_MenyertakanPeringatanYangPerluDibaca()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var selesai = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            BuatCpptLama(context, selesai.PatientId, selesai.EncounterId, penulis.Id);
            BuatCpptLama(context, selesai.PatientId, selesai.EncounterId, providerUserId: null);
            await context.SaveChangesAsync();

            var telaah = await Service(context).SurveyAsync();

            Assert.NotEmpty(telaah.Peringatan);
            Assert.Contains(telaah.Peringatan, x => x.Contains("laporan kelengkapan"));
            Assert.Contains(telaah.Peringatan, x => x.Contains("tidak mencantumkan penulisnya"));
        }

        [Fact]
        public async Task PenelaahanPadaDataKosong_MenyatakanTidakPerluDijalankan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var telaah = await Service(context).SurveyAsync();

            Assert.Equal(0, telaah.BelumTerdaftar);
            Assert.Contains(telaah.Peringatan, x => x.Contains("tidak perlu dijalankan"));
        }
    }
}
