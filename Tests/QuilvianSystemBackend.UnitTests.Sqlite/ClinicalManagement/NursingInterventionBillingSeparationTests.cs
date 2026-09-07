using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-062</c> — tagihan yang gagal tidak menghapus catatan
    /// klinis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kesalahan yang paling mahal pada slice ini</b> adalah menggabungkan keadaan klinis
    /// dengan keadaan tagihan: catatan klinis bisa hilang karena masalah keuangan. Ns. Sari
    /// memasang infus pukul 02.00 saat sistem tagihan mati; pukul 08.00 harus tetap ada bukti
    /// infus pernah dipasang. Berkas ini menguji tepat hal itu.
    /// </para>
    /// <para>
    /// Sekaligus diuji di sini: finalisasi mendaftarkan catatan sebagai dokumen
    /// <c>Procedure</c> tertanda tangan dalam satu <c>SaveChanges</c>, dan koreksinya tersimpan
    /// sebagai addendum bernomor tanpa mengubah isi aslinya — <c>RWI-DEC-091</c>,
    /// <c>RWI-AC-176</c>.
    /// </para>
    /// </remarks>
    public class NursingInterventionBillingSeparationTests
    {
        private static T Isi<T>(IActionResult hasil) where T : class
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);
            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        private static async Task<(RawatInapTestData.Konteks Konteks, Guid PerawatUserId, NursingInterventionResponse Tindakan)>
            SiapkanTindakanAsync(ApplicationDbContext context, bool dapatDitagih = true)
        {
            var konteks = RawatInapTestData.SiapkanPerawatan(context);
            var (_, akun) = RawatInapTestData.BuatPerawat(context);

            var hasil = await NursingInterventionTests.BuatController(context, akun.Id)
                .CreateNursingIntervention(new CreateNursingInterventionRequest
                {
                    EncounterId = konteks.EncounterId,
                    InterventionName = "Pemasangan infus",
                    ResultNote = "Infus terpasang di tangan kiri pukul 02.00",
                    IsBillable = dapatDitagih
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            return (konteks, akun.Id, Isi<NursingInterventionResponse>(hasil));
        }

        // =====================================================================
        // Kriteria 1 dan 2 - dua mesin status yang tidak saling mengunci
        // =====================================================================

        /// <summary>
        /// `BE-RWI-062 AC 1`, `AC-CAP014-02` — ketika pengiriman ke Billing gagal, catatan
        /// klinisnya <b>tetap tersimpan</b> dan keadaan pengirimannya menjadi gagal.
        /// </summary>
        [Fact]
        public async Task BillingGagal_CatatanKlinisTetapTersimpan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, tindakan) = await SiapkanTindakanAsync(context);

            var service = NursingInterventionTests.BuatService(context);

            var hasil = await service.RecordBillingDispatchOutcomeAsync(
                tindakan.Id,
                berhasil: false,
                failureReason: "Layanan Billing tidak dapat dihubungi",
                actorUserId: perawatUserId);

            Assert.True(hasil.IsSuccess);

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingIntervention>().SingleAsync(x => x.Id == tindakan.Id);

            // Catatan klinisnya utuh: masih ada, isinya tidak berubah, keadaannya tetap tercatat.
            Assert.Equal("Pemasangan infus", tersimpan.InterventionName);
            Assert.Equal("Infus terpasang di tangan kiri pukul 02.00", tersimpan.ResultNote);
            Assert.Equal(NursingInterventionStatus.Recorded, tersimpan.RecordStatus);

            // Kegagalannya terlihat sebagai keadaan integrasi tersendiri yang dapat dicoba ulang.
            Assert.Equal(NursingBillingDispatchStatus.Failed, tersimpan.BillingDispatchStatus);
            Assert.Equal("Layanan Billing tidak dapat dihubungi", tersimpan.BillingDispatchFailureReason);
            Assert.Equal(1, tersimpan.BillingDispatchAttemptCount);
        }

        /// <summary>
        /// `BE-RWI-062 AC 2` — kedua mesin status tidak saling mengunci: catatan yang sudah final
        /// tetap dapat berdampingan dengan pengiriman tagihan yang gagal, dan percobaan ulang
        /// yang berhasil tidak mengubah keadaan klinisnya.
        /// </summary>
        [Fact]
        public async Task DuaMesinStatus_TidakSalingMengunci()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, tindakan) = await SiapkanTindakanAsync(context);

            var difinalkan = await NursingInterventionTests.BuatController(context, perawatUserId)
                .FinalizeNursingIntervention(tindakan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(difinalkan));

            var service = NursingInterventionTests.BuatService(context);

            await service.RecordBillingDispatchOutcomeAsync(
                tindakan.Id, berhasil: false, failureReason: "Billing sedang mati", actorUserId: perawatUserId);

            var keadaanGagal = Isi<BillingDispatchResponse>(
                await NursingInterventionTests.BuatController(context, perawatUserId)
                    .GetBillingDispatch(tindakan.Id));

            Assert.Equal(NursingInterventionStatus.Finalized, keadaanGagal.RecordStatus);
            Assert.Equal(NursingBillingDispatchStatus.Failed, keadaanGagal.BillingDispatchStatus);

            // Percobaan ulang berhasil: keadaan tagihan berubah, keadaan klinis tidak.
            await service.RecordBillingDispatchOutcomeAsync(
                tindakan.Id, berhasil: true, failureReason: null, actorUserId: perawatUserId);

            var keadaanBerhasil = Isi<BillingDispatchResponse>(
                await NursingInterventionTests.BuatController(context, perawatUserId)
                    .GetBillingDispatch(tindakan.Id));

            Assert.Equal(NursingInterventionStatus.Finalized, keadaanBerhasil.RecordStatus);
            Assert.Equal(NursingBillingDispatchStatus.Dispatched, keadaanBerhasil.BillingDispatchStatus);
            Assert.Null(keadaanBerhasil.BillingDispatchFailureReason);
            Assert.Equal(2, keadaanBerhasil.BillingDispatchAttemptCount);
        }

        // =====================================================================
        // Kriteria 3 - finalisasi mendaftarkan dokumen Procedure tertanda tangan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-062 AC 3` — finalisasi mendaftarkan catatan sebagai dokumen <c>Procedure</c>
        /// tertanda tangan, dengan penulis catatan sebagai penanda tangannya.
        /// </summary>
        [Fact]
        public async Task Finalisasi_MendaftarkanDokumenProcedureTertandaTangan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, tindakan) = await SiapkanTindakanAsync(context);

            var hasil = await NursingInterventionTests.BuatController(context, perawatUserId)
                .FinalizeNursingIntervention(tindakan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();

            var keutuhan = await pembaca.Set<MrcClinicalDocumentIntegrity>()
                .SingleAsync(x => x.DocumentKind == ClinicalDocumentKind.Procedure &&
                                  x.DocumentId == tindakan.Id);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, keutuhan.IntegrityStatus);
            Assert.Equal(perawatUserId, keutuhan.SignedByUserId);
            Assert.NotNull(keutuhan.SignedAt);
            Assert.NotNull(keutuhan.LockedAt);

            var tersimpan = await pembaca.Set<CliNursingIntervention>().SingleAsync(x => x.Id == tindakan.Id);

            Assert.Equal(NursingInterventionStatus.Finalized, tersimpan.RecordStatus);
            Assert.NotNull(tersimpan.FinalizedAt);
        }

        /// <summary>
        /// `BE-RWI-062 AC 3` — bila pendaftaran keutuhan gagal, <b>finalisasi ikut batal</b>:
        /// catatannya tetap berkeadaan tercatat dan tidak ada baris keutuhan yang terbentuk.
        /// </summary>
        /// <remarks>
        /// Kegagalan dipaksa dengan menghapus penulis catatan. Mesin keutuhan menolak
        /// menandatangani dokumen yang penulisnya tidak dapat ditentukan, karena dokumen
        /// bertanda tangan tanpa penanda tangan bukan bukti apa pun.
        /// </remarks>
        [Fact]
        public async Task PendaftaranKeutuhanGagal_FinalisasiIkutBatal()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, _, tindakan) = await SiapkanTindakanAsync(context);

            var baris = await context.Set<CliNursingIntervention>().SingleAsync(x => x.Id == tindakan.Id);
            baris.CreateBy = Guid.Empty;
            await context.SaveChangesAsync();

            var service = NursingInterventionTests.BuatService(context);

            var hasil = await service.FinalizeAsync(
                tindakan.Id, Guid.Empty, deviceInfo: null, ipAddress: null);

            Assert.False(hasil.IsSuccess);
            Assert.Equal(400, hasil.StatusCode);

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingIntervention>().SingleAsync(x => x.Id == tindakan.Id);

            // Tidak ada keadaan setengah jadi: catatannya tetap Recorded.
            Assert.Equal(NursingInterventionStatus.Recorded, tersimpan.RecordStatus);
            Assert.Null(tersimpan.FinalizedAt);

            Assert.Equal(0, await pembaca.Set<MrcClinicalDocumentIntegrity>()
                .CountAsync(x => x.DocumentKind == ClinicalDocumentKind.Procedure &&
                                 x.DocumentId == tindakan.Id));
        }

        // =====================================================================
        // Kriteria 4, 5, dan 6 - koreksi dan penyuntingan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-062 AC 5` — percobaan menyunting langsung isi catatan yang sudah final
        /// ditolak, dan isinya tidak berubah sedikit pun.
        /// </summary>
        [Fact]
        public async Task MenyuntingCatatanFinal_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, tindakan) = await SiapkanTindakanAsync(context);

            await NursingInterventionTests.BuatController(context, perawatUserId)
                .FinalizeNursingIntervention(tindakan.Id);

            var hasil = await NursingInterventionTests.BuatController(context, perawatUserId)
                .UpdateNursingIntervention(tindakan.Id, new UpdateNursingInterventionRequest
                {
                    InterventionName = "Pemasangan kateter",
                    ResultNote = "Isi yang menimpa catatan asli"
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingIntervention>().SingleAsync(x => x.Id == tindakan.Id);

            Assert.Equal("Pemasangan infus", tersimpan.InterventionName);
            Assert.Equal("Infus terpasang di tangan kiri pukul 02.00", tersimpan.ResultNote);
        }

        /// <summary>
        /// `BE-RWI-062 AC 6`, `RWI-AC-176` — koreksi tersimpan sebagai addendum bernomor beserta
        /// alasannya, isi aslinya tidak berubah, dan status catatan <b>tetap</b> final.
        /// </summary>
        [Fact]
        public async Task Koreksi_TersimpanSebagaiAddendumBernomorTanpaMengubahIsiAsli()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, tindakan) = await SiapkanTindakanAsync(context);

            await NursingInterventionTests.BuatController(context, perawatUserId)
                .FinalizeNursingIntervention(tindakan.Id);

            foreach (var (isi, alasan) in new[]
                     {
                         ("Lokasi infus seharusnya tangan kanan", "Salah tulis lokasi"),
                         ("Ukuran kateter IV 20G", "Melengkapi keterangan")
                     })
            {
                var hasil = await NursingInterventionTests.BuatController(context, perawatUserId)
                    .CreateAddendum(tindakan.Id, new CreateInterventionAddendumRequest
                    {
                        Content = isi,
                        Reason = alasan
                    });

                Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));
            }

            var daftar = Isi<List<Areas.HealthServices.MedicalRecordManagement.DTOs.ClinicalNoteAddendumResponse>>(
                await NursingInterventionTests.BuatController(context, perawatUserId)
                    .GetAddendums(tindakan.Id));

            Assert.Equal(2, daftar.Count);
            Assert.Equal(new[] { 1, 2 }, daftar.Select(x => x.Sequence));
            Assert.Equal("Salah tulis lokasi", daftar[0].CorrectionReason);
            Assert.Equal("Melengkapi keterangan", daftar[1].CorrectionReason);

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingIntervention>().SingleAsync(x => x.Id == tindakan.Id);

            // Isi asli tidak berubah, dan statusnya tetap final sesudah dua kali dikoreksi.
            Assert.Equal("Pemasangan infus", tersimpan.InterventionName);
            Assert.Equal("Infus terpasang di tangan kiri pukul 02.00", tersimpan.ResultNote);
            Assert.Equal(NursingInterventionStatus.Finalized, tersimpan.RecordStatus);
        }

        /// <summary>
        /// `BE-RWI-062 AC 4`, `VAL-KEP-07`, `AC-CAP014-03` — catatan final yang dikoreksi bukan
        /// penulisnya ditolak <c>403</c>, dan tidak ada satu pun addendum yang terbentuk.
        /// </summary>
        /// <remarks>
        /// Jalur kepala ruangan tidak lewat sini: ia memakai endpoint pengganti milik
        /// <c>MedicalRecordManagement</c>, karena aturan pengganti beserta penetapan
        /// berhalangannya dimiliki modul itu. Batas yang sama sudah dipakai <c>BE-RWI-057</c>.
        /// </remarks>
        [Fact]
        public async Task KoreksiOlehBukanPenulis_Ditolak403()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, tindakan) = await SiapkanTindakanAsync(context);

            await NursingInterventionTests.BuatController(context, perawatUserId)
                .FinalizeNursingIntervention(tindakan.Id);

            var perawatLain = RekamMedisTestData.BuatPengguna(context, "perawat.lain");

            var hasil = await NursingInterventionTests.BuatController(context, perawatLain.Id)
                .CreateAddendum(tindakan.Id, new CreateInterventionAddendumRequest
                {
                    Content = "Koreksi dari petugas lain",
                    Reason = "Mencoba mengubah catatan orang lain"
                });

            Assert.Equal(403, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();

            Assert.Equal(0, await pembaca.Set<MrcClinicalNoteAddendum>().CountAsync());
        }

        /// <summary>
        /// `VAL-KEP-06` — catatan yang belum final hanya dapat disunting penulisnya.
        /// </summary>
        [Fact]
        public async Task MenyuntingCatatanBelumFinal_OlehBukanPenulis_Ditolak403()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, _, tindakan) = await SiapkanTindakanAsync(context);

            var perawatLain = RekamMedisTestData.BuatPengguna(context, "perawat.lain");

            var hasil = await NursingInterventionTests.BuatController(context, perawatLain.Id)
                .UpdateNursingIntervention(tindakan.Id, new UpdateNursingInterventionRequest
                {
                    InterventionName = "Diubah petugas lain"
                });

            Assert.Equal(403, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(NursingInterventionService.PenolakanBukanPenulis, ControllerTestHarness.Pesan(hasil));
        }

        /// <summary>
        /// Catatan yang belum final masih dapat disunting penulisnya sendiri —
        /// <c>state-transition-matrix.md</c> bagian 3.
        /// </summary>
        [Fact]
        public async Task MenyuntingCatatanBelumFinal_OlehPenulisnya_Diterima()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, tindakan) = await SiapkanTindakanAsync(context);

            var hasil = await NursingInterventionTests.BuatController(context, perawatUserId)
                .UpdateNursingIntervention(tindakan.Id, new UpdateNursingInterventionRequest
                {
                    InterventionName = "Pemasangan infus ulang",
                    ResultNote = "Infus dipasang ulang di tangan kanan"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();

            var tersimpan = await pembaca.Set<CliNursingIntervention>().SingleAsync(x => x.Id == tindakan.Id);

            Assert.Equal("Pemasangan infus ulang", tersimpan.InterventionName);
            Assert.Equal(NursingInterventionStatus.Recorded, tersimpan.RecordStatus);
        }

        /// <summary>
        /// Tindakan yang tidak ditagihkan tetap dapat dibaca keadaan pengirimannya, dan
        /// kalimatnya menyebut bahwa memang tidak ada tagihan untuknya.
        /// </summary>
        [Fact]
        public async Task TindakanTidakDitagihkan_KeadaanPengirimanTidakBerlaku()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, perawatUserId, tindakan) = await SiapkanTindakanAsync(context, dapatDitagih: false);

            var keadaan = Isi<BillingDispatchResponse>(
                await NursingInterventionTests.BuatController(context, perawatUserId)
                    .GetBillingDispatch(tindakan.Id));

            Assert.False(keadaan.IsBillable);
            Assert.Equal(NursingBillingDispatchStatus.NotApplicable, keadaan.BillingDispatchStatus);
            Assert.Equal("Tindakan ini tidak ditagihkan.", keadaan.Message);
        }
    }
}
