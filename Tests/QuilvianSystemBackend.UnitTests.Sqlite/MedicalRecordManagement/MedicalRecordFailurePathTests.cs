using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-17` — jalur gagal yang wajib diuji.
    ///
    /// Acceptance test matrix bagian 3 mendaftar **empat belas** jalur gagal. Sepuluh di
    /// antaranya sudah terbukti pada task pendahulunya. Berkas ini menutup empat sisanya:
    ///
    /// <list type="table">
    /// <item><term>`AT-RM-13`</term><description>`SuperAdmin` tanpa alasan</description></item>
    /// <item><term>`AT-RM-30`</term><description>pencatatan jejak gagal</description></item>
    /// <item><term>`AT-RM-35`</term><description>pendaftaran keutuhan gagal saat CPPT dibuat</description></item>
    /// <item><term>`AT-RM-36`</term><description>penguncian gagal saat kunjungan ditutup</description></item>
    /// </list>
    ///
    /// KENAPA JALUR GAGAL DIJADIKAN TASK TERSENDIRI. Jalur gagal sering dianggap pelengkap lalu
    /// dilewati saat waktu menipis. Padahal justru di sinilah aturan keselamatan modul ini
    /// berada: gagal mencatat jejak berarti gagal membaca, dan dokumen tanpa baris keutuhan
    /// akan luput dari seluruh aturan penguncian.
    ///
    /// CARA MENIRUKAN KEGAGALAN. Tabel yang bersangkutan dihapus dari basis data uji, sehingga
    /// query ke tabel itu benar-benar gagal. Bukan disimulasikan lewat penanda maupun tiruan
    /// objek — yang diuji adalah perilaku sistem ketika basis datanya sungguh-sungguh bermasalah.
    ///
    /// Seluruh data di sini adalah data karangan. Tidak ada data pasien sungguhan.
    /// </summary>
    public class MedicalRecordFailurePathTests
    {
        // =====================================================================
        // Penyiapan
        // =====================================================================

        private static void HapusTabel<TEntity>(ApplicationDbContext context) where TEntity : class
        {
            var namaTabel = context.Model.FindEntityType(typeof(TEntity))!.GetTableName();

            // Nama tabel berasal dari model EF, bukan dari masukan siapa pun, dan dirangkai
            // tanpa interpolasi supaya tidak terbaca sebagai SQL yang dapat disusupi.
            context.Database.ExecuteSqlRaw("DROP TABLE \"" + namaTabel + "\"");
        }

        private static MedicalRecordController ControllerRekamMedis(
            ApplicationDbContext context,
            Guid userId)
            => new MedicalRecordController(
                    context,
                    ControllerTestHarness.BuatLoggerService(userId),
                    new MedicalRecordAccessAuditService(context),
                    new MedicalRecordTimelineService(context))
                .DenganPengguna(userId);

        private static PatientIntegratedProgressNoteController ControllerCppt(
            ApplicationDbContext context,
            Guid userId)
            => new PatientIntegratedProgressNoteController(
                    context,
                    ControllerTestHarness.BuatLoggerService(userId),
                    new ClinicalDocumentIntegrityService(context))
                .DenganPengguna(userId);

        private static PatientEncounterController ControllerKunjungan(
            ApplicationDbContext context,
            Guid userId)
        {
            var logger = ControllerTestHarness.BuatLoggerService(userId);

            var realtime = new QueueRealtimeService(
                context, new HubContextKosong<QueueHub>(), logger);

            return new PatientEncounterController(
                    context, logger, realtime, new ClinicalDocumentIntegrityService(context))
                .DenganPengguna(userId);
        }

        private static MstMedicalRecordAccessPurpose BuatKeperluan(ApplicationDbContext context)
        {
            var keperluan = new MstMedicalRecordAccessPurpose
            {
                PurposeCode = $"AUDIT-{Guid.NewGuid().ToString("N")[..6]}",
                PurposeName = "Penelaahan internal",
                IsFreeTextRequired = false,
                RequiresReview = true,
                IsActive = true
            };

            context.Set<MstMedicalRecordAccessPurpose>().Add(keperluan);
            context.SaveChanges();
            return keperluan;
        }

        /// <summary>
        /// Memberi seorang pengguna peran `SuperAdmin` sungguhan.
        ///
        /// Diperlukan agar `AT-RM-13` benar-benar menguji pengguna ber-peran itu, bukan sekadar
        /// pengguna biasa yang dinamai demikian.
        /// </summary>
        private static void JadikanSuperAdmin(ApplicationDbContext context, Guid userId)
        {
            var peran = new ApplicationRole
            {
                Name = "SuperAdmin",
                NormalizedName = "SUPERADMIN"
            };

            context.Set<ApplicationRole>().Add(peran);
            context.SaveChanges();

            context.Set<IdentityUserRole<Guid>>().Add(new IdentityUserRole<Guid>
            {
                UserId = userId,
                RoleId = peran.Id
            });
            context.SaveChanges();
        }

        // =====================================================================
        // AT-RM-13 — SuperAdmin tanpa alasan
        // =====================================================================

        /// <summary>
        /// `AT-RM-13`: pengguna ber-peran `SuperAdmin` yang membuka rekam medis pasien tanpa
        /// kunjungan aktif **tetap** diminta alasan; aksesnya tercatat dan ditandai perlu
        /// ditinjau.
        ///
        /// Ini penerapan `RM-DEC-017`, dan merupakan keputusan yang paling mudah dilanggar tanpa
        /// sengaja. Pola umum pada banyak sistem adalah memberi peran tertinggi jalan pintas
        /// melewati pemeriksaan. Di modul ini jalan pintas itu **tidak ada**, dan ketiadaannya
        /// perlu dibuktikan — bukan sekadar dipercaya karena tidak ada kode yang menuliskannya.
        /// </summary>
        [Fact]
        public async Task SuperAdmin_TetapDimintaAlasanDanTetapDitandaiPerluDitinjau()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var superAdmin = RekamMedisTestData.BuatPengguna(context, "super.admin");
            JadikanSuperAdmin(context, superAdmin.Id);

            var controller = ControllerRekamMedis(context, superAdmin.Id);

            // 1) Tanpa keperluan akses: ditolak, sama seperti pengguna mana pun.
            var ditolak = await controller.GetTimeline(konteks.PatientId);

            Assert.Equal(StatusCodes.Status400BadRequest, ControllerTestHarness.KodeStatus(ditolak));

            using (var konteksPeriksa = database.CreateContext())
            {
                Assert.Empty(await konteksPeriksa.Set<MrcAccessLog>()
                    .AsNoTracking().ToListAsync());
            }

            // 2) Dengan keperluan akses: dilayani, tetapi tetap ditandai perlu ditinjau.
            var keperluan = BuatKeperluan(context);

            var diizinkan = await controller.GetTimeline(
                konteks.PatientId, accessPurposeId: keperluan.Id);

            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(diizinkan));

            using var konteksBaca = database.CreateContext();
            var jejak = Assert.Single(
                await konteksBaca.Set<MrcAccessLog>().AsNoTracking().ToListAsync());

            Assert.Equal(superAdmin.Id, jejak.UserId);
            Assert.Equal(MedicalRecordAccessType.ReasonedAccess, jejak.AccessType);
            Assert.True(jejak.IsFlaggedForReview);

            // Peran tertinggi benar-benar melekat pada penggunanya.
            Assert.True(await konteksBaca.Set<IdentityUserRole<Guid>>()
                .AsNoTracking().AnyAsync(x => x.UserId == superAdmin.Id));
        }

        // =====================================================================
        // AT-RM-30 — pencatatan jejak gagal
        // =====================================================================

        /// <summary>
        /// `AT-RM-30`: bila penulisan jejak akses gagal, permintaan dijawab `503` dan isi rekam
        /// medis **tidak** dikembalikan.
        ///
        /// Inilah aturan "gagal mencatat jejak berarti gagal membaca" (`RM-DEC-015`). Pilihan
        /// ini menutup rapat dan konsekuensinya diterima sadar: gangguan pada tabel jejak akan
        /// menghambat pembacaan rekam medis. Itu dinilai lebih baik daripada ada pembacaan yang
        /// tidak tercatat.
        ///
        /// Diperiksa pada seluruh pintu masuk berkas, karena satu pintu yang lolos berarti ada
        /// jalur membaca tanpa jejak.
        /// </summary>
        [Fact]
        public async Task PencatatanJejakGagal_Dijawab503DanIsiTidakDikembalikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            var cppt = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{Guid.NewGuid():N}"[..20],
                PatientId = konteks.PatientId,
                EncounterId = konteks.EncounterId,
                ProfessionType = "Doctor",
                SubjectiveSummary = "Isi yang tidak boleh keluar tanpa jejak.",
                NoteDateTime = DateTime.UtcNow.AddDays(-1)
            };
            context.Set<TrxPatientIntegratedProgressNote>().Add(cppt);
            context.SaveChanges();

            // Tabel jejak dihilangkan supaya pencatatannya gagal sungguhan.
            HapusTabel<MrcAccessLog>(context);

            var controller = ControllerRekamMedis(context, dokter.Id);

            var balasan = new[]
            {
                await controller.GetSummary(konteks.PatientId),
                await controller.GetTimeline(konteks.PatientId),
                await controller.GetDocumentDetail(
                    konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id)
            };

            foreach (var hasil in balasan)
            {
                Assert.Equal(
                    StatusCodes.Status503ServiceUnavailable,
                    ControllerTestHarness.KodeStatus(hasil));

                var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
                var body = Assert.IsType<ApiResponse<object>>(objek.Value);

                Assert.False(body.Success);
                Assert.Null(body.Data);

                // Isi catatan tidak boleh ikut terbawa lewat pesan galat.
                Assert.DoesNotContain("tidak boleh keluar", ControllerTestHarness.Pesan(hasil)!);
            }
        }

        // =====================================================================
        // AT-RM-35 — pendaftaran keutuhan gagal saat CPPT dibuat
        // =====================================================================

        /// <summary>
        /// `AT-RM-35`: bila pendaftaran keutuhan gagal saat CPPT dibuat, pembuatan CPPT ikut
        /// dibatalkan. **Tidak ada CPPT tanpa baris keutuhan.**
        ///
        /// Ini yang membuat aturan penguncian dapat diandalkan. Sebuah CPPT yang tersimpan tanpa
        /// baris keutuhan akan luput dari seluruh pemeriksaan `EnsureMutableAsync` selamanya —
        /// bukan karena aturannya salah, melainkan karena dokumen itu tidak pernah terdaftar
        /// untuk diperiksa.
        ///
        /// Integration contract bagian 2.1 karena itu mewajibkan keduanya berada dalam satu
        /// penyimpanan: `RegisterAsync` tidak menyimpan sendiri, melainkan ikut `SaveChanges`
        /// milik pembuatan CPPT.
        /// </summary>
        [Fact]
        public async Task PendaftaranKeutuhanGagal_PembuatanCpptIkutDibatalkan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            // Tabel keutuhan dihilangkan supaya pendaftarannya gagal sungguhan.
            HapusTabel<MrcClinicalDocumentIntegrity>(context);

            var permintaan = new CreatePatientIntegratedProgressNoteRequest
            {
                PatientId = konteks.PatientId,
                EncounterId = konteks.EncounterId,
                ProfessionType = "Doctor",
                ProviderUserId = penulis.Id,
                SubjectiveSummary = "CPPT yang seharusnya tidak jadi tersimpan.",
                IsActive = true
            };

            await Assert.ThrowsAnyAsync<Exception>(
                () => ControllerCppt(context, penulis.Id).CreateProgressNote(permintaan));

            // Yang paling penting: CPPT-nya benar-benar tidak tersimpan.
            using var konteksBaca = database.CreateContext();
            Assert.Empty(await konteksBaca.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking().ToListAsync());
        }

        // =====================================================================
        // AT-RM-36 — penguncian gagal saat kunjungan ditutup
        // =====================================================================

        /// <summary>
        /// `AT-RM-36`: bila penguncian gagal saat kunjungan ditutup, penutupan kunjungan ikut
        /// dibatalkan dan kunjungan **tetap terbuka**.
        ///
        /// Alasannya sama tegasnya dengan `AT-RM-35`, hanya dari arah sebaliknya: kunjungan yang
        /// tertutup sementara dokumennya masih terbuka adalah keadaan yang dilarang
        /// `RM-DEC-003`. Catatan yang tertinggal terbuka selamanya tidak akan pernah terkunci
        /// lagi, karena pemicunya — penutupan kunjungan — sudah lewat.
        ///
        /// Lebih baik kunjungan gagal ditutup dan petugas mencoba lagi, daripada tertutup dengan
        /// catatan menggantung.
        /// </summary>
        [Fact]
        public async Task PenguncianGagalSaatKunjunganDitutup_KunjunganTetapTerbuka()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas");

            // Tabel keutuhan dihilangkan supaya penguncian gagal sungguhan.
            HapusTabel<MrcClinicalDocumentIntegrity>(context);

            var permintaan = new PatientEncounterStatusRequest
            {
                EncounterStatus = EncounterStatus.Completed
            };

            await Assert.ThrowsAnyAsync<Exception>(
                () => ControllerKunjungan(context, petugas.Id)
                    .UpdateEncounterStatus(konteks.EncounterId, permintaan));

            using var konteksBaca = database.CreateContext();
            var kunjungan = await konteksBaca.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == konteks.EncounterId);

            Assert.NotEqual(EncounterStatus.Completed, kunjungan.EncounterStatus);
            Assert.Null(kunjungan.CompletedAt);
        }

        // =====================================================================
        // AT-RM-26 — penetapan tanpa batas waktu
        // =====================================================================

        /// <summary>
        /// `AT-RM-26`: penetapan berhalangan yang dibuat **tanpa mengisi batas waktu** ditolak
        /// `400`, dan tidak tersimpan.
        ///
        /// Uji ini melengkapi `PenetapanDenganBatasWaktuYangSudahLewat_Ditolak` pada `BE-05`,
        /// yang menguji batas waktu yang diisi tetapi sudah lewat. Yang diuji di sini adalah
        /// kolomnya memang tidak diisi sama sekali — keadaan yang sampai ke service sebagai
        /// nilai tanggal bawaan, bukan sebagai kosong.
        ///
        /// Bedanya penting karena `[Required]` pada tanggal yang tidak boleh kosong **tidak**
        /// menangkap keadaan ini: nilai bawaan bukan nilai kosong, sehingga lolos pemeriksaan
        /// atribut. Yang benar-benar menahannya adalah aturan "batas waktu harus setelah hari
        /// ini" di dalam service.
        ///
        /// Kenapa aturan ini ada: penetapan tanpa batas waktu adalah pintu belakang permanen —
        /// kewenangan mengoreksi catatan orang lain yang tidak pernah tertutup dengan
        /// sendirinya (`RM-DEC-020`).
        /// </summary>
        [Fact]
        public async Task PenetapanTanpaBatasWaktu_DitolakDanTidakTersimpan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");

            var sekarang = DateTime.UtcNow;

            var (hasil, penetapan) = await new ClinicalNoteAuthorDelegationService(context)
                .CreateAsync(
                    penulis.Id,
                    kepalaUnit.Id,
                    "Penulis sedang cuti panjang.",
                    // Batas waktu yang tidak diisi sampai ke service sebagai nilai bawaan.
                    validUntilUtc: default,
                    nowUtc: sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            Assert.Null(penetapan);

            using var konteksBaca = database.CreateContext();
            Assert.Empty(await konteksBaca.Set<MrcClinicalNoteAuthorDelegation>()
                .AsNoTracking().ToListAsync());
        }

        /// <summary>
        /// Pembanding: bila tabel keutuhan sehat, penutupan kunjungan berhasil.
        ///
        /// Diperiksa supaya uji di atas benar-benar membuktikan **penguncian yang gagal**
        /// membatalkan penutupan, bukan sekadar membuktikan bahwa penutupan memang tidak pernah
        /// bekerja.
        /// </summary>
        [Fact]
        public async Task PenguncianSehat_PenutupanKunjunganBerhasil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas");

            var permintaan = new PatientEncounterStatusRequest
            {
                EncounterStatus = EncounterStatus.Completed
            };

            var hasil = await ControllerKunjungan(context, petugas.Id)
                .UpdateEncounterStatus(konteks.EncounterId, permintaan);

            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(hasil));

            using var konteksBaca = database.CreateContext();
            var kunjungan = await konteksBaca.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == konteks.EncounterId);

            Assert.Equal(EncounterStatus.Completed, kunjungan.EncounterStatus);
        }
    }
}
