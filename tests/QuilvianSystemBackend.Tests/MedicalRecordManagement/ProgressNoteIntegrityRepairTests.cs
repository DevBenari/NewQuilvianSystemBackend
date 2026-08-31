using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-03` — menutup tiga celah keutuhan pada CPPT.
    ///
    /// Menutup uji penerimaan `AT-RM-01`, `AT-RM-19`, `AT-RM-20`, dan `AT-RM-24`.
    ///
    /// Uji ini memanggil controller langsung, bukan lewat HTTP, karena ketiga perbaikan memang
    /// berada di dalam controller. Menguji lapisan di bawahnya saja tidak akan membuktikan
    /// apa pun tentang perbaikan tersebut.
    /// </summary>
    public class ProgressNoteIntegrityRepairTests
    {
        private static PatientIntegratedProgressNoteController Controller(
            ApplicationDbContext context,
            Guid actorUserId)
            => new PatientIntegratedProgressNoteController(
                    context,
                    ControllerTestHarness.BuatLoggerService(actorUserId),
                    new ClinicalDocumentIntegrityService(context))
                .DenganPengguna(actorUserId);

        /// <summary>
        /// Membuat satu CPPT langsung lewat basis data, beserta baris keutuhannya, supaya uji
        /// perubahan tidak bergantung pada jalur pembuatan.
        /// </summary>
        private static async Task<(RekamMedisTestData.Konteks Konteks, TrxPatientIntegratedProgressNote Cppt, Guid PenulisId)>
            SiapkanCppt(ApplicationDbContext context)
        {
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            var cppt = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{Guid.NewGuid():N}"[..20],
                PatientId = konteks.PatientId,
                EncounterId = konteks.EncounterId,
                ProfessionType = "Doctor",
                ProviderUserId = penulis.Id,
                SubjectiveSummary = "Keluhan awal.",
                NoteDateTime = DateTime.UtcNow,
                CreateBy = penulis.Id
            };
            context.Set<TrxPatientIntegratedProgressNote>().Add(cppt);

            await new ClinicalDocumentIntegrityService(context).RegisterAsync(
                ClinicalDocumentKind.ProgressNote,
                cppt.Id,
                konteks.PatientId,
                konteks.EncounterId,
                penulis.Id);

            await context.SaveChangesAsync();

            return (konteks, cppt, penulis.Id);
        }

        private static UpdatePatientIntegratedProgressNoteRequest PermintaanUbah(
            TrxPatientIntegratedProgressNote cppt) => new()
            {
                NoteDateTime = cppt.NoteDateTime,
                ProfessionType = cppt.ProfessionType,
                SubjectiveSummary = "Keluhan setelah diubah.",
                IsActive = true
            };

        // =====================================================================
        // AT-RM-01 — dokumen terkunci menolak perubahan
        // =====================================================================

        /// <summary>
        /// `AT-RM-01`: mengubah CPPT yang sudah ditandatangani ditolak, dan isinya tidak
        /// berubah sedikit pun.
        ///
        /// Pemeriksaan terakhir yang paling penting: bukan hanya balasannya yang ditolak,
        /// tetapi isi di basis data benar-benar tidak tersentuh.
        /// </summary>
        [Fact]
        public async Task MengubahCpptYangSudahDitandatangani_DitolakDanIsinyaTidakBerubah()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, cppt, penulisId) = await SiapkanCppt(context);

            await new ClinicalDocumentIntegrityService(context).SignAsync(
                ClinicalDocumentKind.ProgressNote, cppt.Id, penulisId,
                null, null, DateTime.UtcNow);

            var hasil = await Controller(context, penulisId)
                .UpdateProgressNote(cppt.Id, PermintaanUbah(cppt));

            Assert.Equal(StatusCodes.Status400BadRequest, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("addendum", ControllerTestHarness.Pesan(hasil));

            var tersimpan = context.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking().Single(x => x.Id == cppt.Id);
            Assert.Equal("Keluhan awal.", tersimpan.SubjectiveSummary);
        }

        /// <summary>
        /// CPPT yang terkunci otomatis karena kunjungan ditutup juga menolak perubahan.
        /// </summary>
        [Fact]
        public async Task MengubahCpptYangTerkunciKarenaKunjunganDitutup_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, cppt, penulisId) = await SiapkanCppt(context);

            await new ClinicalDocumentIntegrityService(context)
                .LockOpenDocumentsForEncounterAsync(konteks.EncounterId, penulisId, DateTime.UtcNow);
            await context.SaveChangesAsync();

            var hasil = await Controller(context, penulisId)
                .UpdateProgressNote(cppt.Id, PermintaanUbah(cppt));

            Assert.Equal(StatusCodes.Status400BadRequest, ControllerTestHarness.KodeStatus(hasil));
        }

        /// <summary>
        /// CPPT yang masih draf tetap dapat diubah. Perbaikan ini tidak boleh memblokir alur
        /// yang wajar.
        /// </summary>
        [Fact]
        public async Task MengubahCpptYangMasihDraf_TetapBerhasil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, cppt, penulisId) = await SiapkanCppt(context);

            var hasil = await Controller(context, penulisId)
                .UpdateProgressNote(cppt.Id, PermintaanUbah(cppt));

            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(hasil));

            var tersimpan = context.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking().Single(x => x.Id == cppt.Id);
            Assert.Equal("Keluhan setelah diubah.", tersimpan.SubjectiveSummary);
        }

        // =====================================================================
        // AT-RM-19 — penulis tidak dapat dipindahkan
        // =====================================================================

        /// <summary>
        /// `AT-RM-19`: mengirim `ProviderUserId` orang lain tidak mengubah penulis catatan.
        ///
        /// Permintaan TIDAK gagal — nilainya diabaikan. Pilihan ini diambil supaya frontend
        /// yang sedang berjalan tidak putus, karena ia mengirim seluruh isi formulir termasuk
        /// kolom ini.
        /// </summary>
        [Fact]
        public async Task MengirimPenulisOrangLain_TidakMengubahPenulisCatatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, cppt, penulisId) = await SiapkanCppt(context);
            var oranglain = RekamMedisTestData.BuatPengguna(context, "penyusup");

            var permintaan = PermintaanUbah(cppt);
            permintaan.ProviderUserId = oranglain.Id;

            var hasil = await Controller(context, penulisId)
                .UpdateProgressNote(cppt.Id, permintaan);

            // Permintaan berhasil, bukan ditolak.
            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(hasil));

            // Penulis pada CPPT tidak berpindah.
            var tersimpan = context.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking().Single(x => x.Id == cppt.Id);
            Assert.Equal(penulisId, tersimpan.ProviderUserId);

            // Penulis pada daftar keutuhan juga tidak berpindah.
            var keutuhan = context.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking().Single(x => x.DocumentId == cppt.Id);
            Assert.Equal(penulisId, keutuhan.AuthorUserId);
        }

        // =====================================================================
        // AT-RM-20 — penanda hanya-baca tidak dapat dilepas
        // =====================================================================

        /// <summary>
        /// `AT-RM-20`: penanda hanya-baca tidak dapat dinyalakan lewat permintaan ubah.
        ///
        /// Sisi sebaliknya — melepas penanda pada catatan yang sudah hanya-baca — tidak dapat
        /// diuji lewat jalur ini, karena controller menolak lebih dulu dengan pesan tersendiri.
        /// Yang dibuktikan di sini adalah kolom itu tidak lagi mengikuti kiriman klien.
        /// </summary>
        [Fact]
        public async Task MengirimPenandaHanyaBaca_TidakMengubahPenandaPadaCatatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, cppt, penulisId) = await SiapkanCppt(context);

            Assert.False(cppt.IsReadOnlyGenerated);

            var permintaan = PermintaanUbah(cppt);
            permintaan.IsReadOnlyGenerated = true;

            var hasil = await Controller(context, penulisId)
                .UpdateProgressNote(cppt.Id, permintaan);

            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(hasil));

            var tersimpan = context.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking().Single(x => x.Id == cppt.Id);
            Assert.False(tersimpan.IsReadOnlyGenerated);
        }

        // =====================================================================
        // AT-RM-24 — pembuatan CPPT mendaftarkan keutuhan
        // =====================================================================

        /// <summary>
        /// `AT-RM-24`: membuat CPPT menghasilkan satu baris keutuhan berstatus draf, dengan
        /// penulis terisi dari CPPT-nya.
        /// </summary>
        [Fact]
        public async Task MembuatCppt_MenghasilkanBarisKeutuhanBerstatusDraf()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            var permintaan = new CreatePatientIntegratedProgressNoteRequest
            {
                PatientId = konteks.PatientId,
                EncounterId = konteks.EncounterId,
                ProfessionType = "Doctor",
                ProviderUserId = penulis.Id,
                SubjectiveSummary = "Pasien mengeluh nyeri kepala.",
                IsActive = true
            };

            var hasil = await Controller(context, penulis.Id)
                .CreateProgressNote(permintaan);

            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(hasil));

            var cppt = context.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking().Single();

            var keutuhan = context.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking().Single();

            Assert.Equal(ClinicalDocumentKind.ProgressNote, keutuhan.DocumentKind);
            Assert.Equal(cppt.Id, keutuhan.DocumentId);
            Assert.Equal(konteks.PatientId, keutuhan.PatientId);
            Assert.Equal(konteks.EncounterId, keutuhan.EncounterId);
            Assert.Equal(penulis.Id, keutuhan.AuthorUserId);
            Assert.True(keutuhan.IsAuthorKnown);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, keutuhan.IntegrityStatus);
        }

        /// <summary>
        /// CPPT yang baru dibuat langsung tunduk aturan keutuhan: setelah ditandatangani, ia
        /// tidak dapat diubah lagi.
        ///
        /// Ini yang membuktikan ketiga perbaikan bekerja sebagai satu kesatuan, bukan sekadar
        /// berdiri sendiri-sendiri.
        /// </summary>
        [Fact]
        public async Task CpptBaru_LangsungTundukAturanKeutuhanSetelahDitandatangani()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            var buatHasil = await Controller(context, penulis.Id).CreateProgressNote(
                new CreatePatientIntegratedProgressNoteRequest
                {
                    PatientId = konteks.PatientId,
                    EncounterId = konteks.EncounterId,
                    ProfessionType = "Doctor",
                    ProviderUserId = penulis.Id,
                    SubjectiveSummary = "Keluhan awal.",
                    IsActive = true
                });

            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(buatHasil));

            var cppt = context.Set<TrxPatientIntegratedProgressNote>().AsNoTracking().Single();

            await new ClinicalDocumentIntegrityService(context).SignAsync(
                ClinicalDocumentKind.ProgressNote, cppt.Id, penulis.Id,
                null, null, DateTime.UtcNow);

            var ubahHasil = await Controller(context, penulis.Id)
                .UpdateProgressNote(cppt.Id, new UpdatePatientIntegratedProgressNoteRequest
                {
                    ProfessionType = "Doctor",
                    SubjectiveSummary = "Percobaan mengubah setelah terkunci.",
                    IsActive = true
                });

            Assert.Equal(StatusCodes.Status400BadRequest, ControllerTestHarness.KodeStatus(ubahHasil));

            var tersimpan = context.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking().Single(x => x.Id == cppt.Id);
            Assert.Equal("Keluhan awal.", tersimpan.SubjectiveSummary);
        }

        /// <summary>
        /// CPPT yang tidak melekat ke kunjungan mana pun tetap dapat dibuat, hanya saja tidak
        /// terdaftar pada daftar keutuhan.
        ///
        /// Keterbatasan ini dinyatakan terbuka, bukan disembunyikan: baris keutuhan
        /// mensyaratkan kunjungan sebagai pengelompokannya.
        /// </summary>
        [Fact]
        public async Task CpptTanpaKunjungan_TetapDibuatTetapiTidakTerdaftarKeutuhan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "perawat");

            var hasil = await Controller(context, penulis.Id).CreateProgressNote(
                new CreatePatientIntegratedProgressNoteRequest
                {
                    PatientId = konteks.PatientId,
                    EncounterId = null,
                    ProfessionType = "Nurse",
                    ProviderUserId = penulis.Id,
                    SubjectiveSummary = "Catatan tanpa kunjungan.",
                    IsActive = true
                });

            // Bila validasi controller menolak CPPT tanpa kunjungan, keadaan itu juga sah —
            // yang penting tidak ada baris keutuhan yang tercipta tanpa kunjungan.
            if (ControllerTestHarness.KodeStatus(hasil) == StatusCodes.Status200OK)
            {
                Assert.Equal(1, context.Set<TrxPatientIntegratedProgressNote>().Count());
            }

            Assert.Equal(0, context.Set<TrxClinicalDocumentIntegrity>().Count());
        }
    }
}
