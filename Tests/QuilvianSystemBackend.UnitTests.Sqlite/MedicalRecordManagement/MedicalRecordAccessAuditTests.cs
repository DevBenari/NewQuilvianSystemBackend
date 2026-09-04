using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-10` dan `BE-11` — jejak akses dan kewenangan tingkat
    /// pasien.
    ///
    /// Menutup uji penerimaan `AT-RM-06`, `AT-RM-07`, `AT-RM-12`, `AT-RM-16`, `AT-RM-22`,
    /// dan `AT-RM-25`.
    /// </summary>
    public class MedicalRecordAccessAuditTests
    {
        private static readonly DateTime Sekarang = new(2026, 8, 26, 13, 0, 0, DateTimeKind.Utc);

        private static MedicalRecordAccessAuditService Service(ApplicationDbContext c) => new(c);

        private static MstMedicalRecordAccessPurpose BuatKeperluan(
            ApplicationDbContext context,
            string kode,
            bool wajibAlasanBebas = false,
            bool perluDitinjau = true,
            bool aktif = true)
        {
            var keperluan = new MstMedicalRecordAccessPurpose
            {
                PurposeCode = $"{kode}-{Guid.NewGuid().ToString("N")[..6]}",
                PurposeName = kode,
                IsFreeTextRequired = wajibAlasanBebas,
                RequiresReview = perluDitinjau,
                IsActive = aktif
            };

            context.Set<MstMedicalRecordAccessPurpose>().Add(keperluan);
            context.SaveChanges();
            return keperluan;
        }

        private static MedicalRecordAccessRequest Permintaan(
            Guid patientId,
            Guid userId,
            MedicalRecordAccessScope scope = MedicalRecordAccessScope.Timeline,
            Guid? purposeId = null,
            string? reason = null)
            => new(patientId, userId, scope, purposeId, reason,
                   "10.20.30.40", "Chrome 140", "/medical-records/timeline");

        // =====================================================================
        // AT-RM-06 — pasien rawatan
        // =====================================================================

        /// <summary>
        /// `AT-RM-06`: pasien dengan kunjungan aktif dibuka tanpa diminta alasan, dan tetap
        /// tercatat.
        /// </summary>
        [Fact]
        public async Task PasienDenganKunjunganAktif_DibukaTanpaAlasanDanTetapTercatat()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(konteks.PatientId, dokter.Id), Sekarang);

            Assert.True(hasil.IsAllowed);
            Assert.Equal(MedicalRecordAccessType.RoutineCare, hasil.AccessType);
            Assert.False(hasil.IsFlaggedForReview);

            var jejak = context.Set<MrcAccessLog>().AsNoTracking().Single();
            Assert.Equal(konteks.PatientId, jejak.PatientId);
            Assert.Equal(dokter.Id, jejak.UserId);
            Assert.True(jejak.HasActiveEncounter);
            Assert.Equal(Sekarang, jejak.AccessedAt);
            Assert.Null(jejak.AccessPurposeId);

            // Nama pengguna disalin supaya jejak tetap terbaca puluhan tahun kemudian.
            Assert.False(string.IsNullOrWhiteSpace(jejak.UserDisplayNameSnapshot));
        }

        // =====================================================================
        // AT-RM-07 — pasien tanpa kunjungan aktif
        // =====================================================================

        /// <summary>
        /// `AT-RM-07`: membuka pasien tanpa kunjungan aktif dan tanpa keperluan ditolak, dan
        /// **tidak ada jejak yang tercatat** karena isinya memang tidak dikembalikan.
        /// </summary>
        [Fact]
        public async Task PasienTanpaKunjunganAktif_TanpaKeperluan_DitolakDanIsinyaTidakDikembalikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(konteks.PatientId, dokter.Id), Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            Assert.Contains("Pilih keperluan akses", hasil.ErrorMessage);
            Assert.Null(hasil.AccessLogId);
        }

        /// <summary>
        /// Dengan keperluan yang sah, akses diizinkan dan ditandai untuk ditinjau.
        /// </summary>
        [Fact]
        public async Task PasienTanpaKunjunganAktif_DenganKeperluan_DiizinkanDanDitandaiUntukDitinjau()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var keperluan = BuatKeperluan(context, "Konsultasi lintas unit");

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(konteks.PatientId, dokter.Id, purposeId: keperluan.Id), Sekarang);

            Assert.True(hasil.IsAllowed);
            Assert.Equal(MedicalRecordAccessType.ReasonedAccess, hasil.AccessType);
            Assert.True(hasil.IsFlaggedForReview);

            var jejak = context.Set<MrcAccessLog>().AsNoTracking().Single();
            Assert.Equal(keperluan.Id, jejak.AccessPurposeId);
            Assert.True(jejak.IsFlaggedForReview);
            Assert.False(jejak.HasActiveEncounter);
        }

        /// <summary>
        /// Keperluan yang menuntut penjelasan tambahan menolak permintaan tanpa penjelasan.
        /// </summary>
        [Fact]
        public async Task KeperluanYangMenuntutPenjelasan_MenolakBilaPenjelasannyaKosong()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var lainnya = BuatKeperluan(context, "Lainnya", wajibAlasanBebas: true);

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(konteks.PatientId, dokter.Id, purposeId: lainnya.Id), Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Contains("Tuliskan alasannya", hasil.ErrorMessage);
        }

        [Fact]
        public async Task KeperluanYangSudahTidakAktif_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var nonaktif = BuatKeperluan(context, "Sudah tidak dipakai", aktif: false);

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(konteks.PatientId, dokter.Id, purposeId: nonaktif.Id), Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Contains("sudah tidak berlaku", hasil.ErrorMessage);
        }

        // =====================================================================
        // AT-RM-16 — catatan pribadi
        // =====================================================================

        /// <summary>
        /// `AT-RM-16`: membuka catatan pribadi SELALU menuntut keperluan, bahkan untuk pasien
        /// yang sedang dirawat pengguna.
        ///
        /// Ini yang membedakan catatan pribadi dari isi rekam medis lainnya (RM-DEC-022).
        /// </summary>
        [Fact]
        public async Task CatatanPribadi_SelaluMenuntutKeperluanWalauPasienSedangDirawat()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(konteks.PatientId, dokter.Id,
                           scope: MedicalRecordAccessScope.PrivateNote),
                Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Contains("catatan pribadi selalu memerlukan keperluan", hasil.ErrorMessage);
        }

        /// <summary>
        /// Pembukaan catatan pribadi tercatat dengan cakupan tersendiri, supaya dapat dihitung
        /// terpisah saat ditinjau.
        /// </summary>
        [Fact]
        public async Task CatatanPribadi_TercatatDenganCakupanTersendiri()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var keperluan = BuatKeperluan(context, "Penelusuran kelengkapan");

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(konteks.PatientId, dokter.Id,
                           scope: MedicalRecordAccessScope.PrivateNote,
                           purposeId: keperluan.Id),
                Sekarang);

            Assert.True(hasil.IsAllowed);

            var jejak = context.Set<MrcAccessLog>().AsNoTracking().Single();
            Assert.Equal(MedicalRecordAccessScope.PrivateNote, jejak.AccessScope);
            Assert.Equal(MedicalRecordAccessType.ReasonedAccess, jejak.AccessType);
        }

        // =====================================================================
        // AT-RM-12 — setiap pembukaan tercatat
        // =====================================================================

        /// <summary>
        /// `AT-RM-12`: sepuluh pembukaan menghasilkan tepat sepuluh baris jejak.
        /// </summary>
        [Fact]
        public async Task SepuluhPembukaan_MenghasilkanTepatSepuluhBarisJejak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var service = Service(context);

            for (var i = 0; i < 10; i++)
            {
                var hasil = await service.EvaluateAndRecordAsync(
                    Permintaan(konteks.PatientId, dokter.Id), Sekarang.AddMinutes(i));
                Assert.True(hasil.IsAllowed);
            }

            Assert.Equal(10, context.Set<MrcAccessLog>().Count());
        }

        // =====================================================================
        // AT-RM-22 — pasien hasil penggabungan
        // =====================================================================

        /// <summary>
        /// `AT-RM-22`: pasien yang ditandai digabung ditolak, disertai nomor rekam medis
        /// penggantinya.
        ///
        /// Menampilkan riwayat sebagian tanpa peringatan lebih berbahaya daripada menolak:
        /// riwayat tidak lengkap akan dibaca sebagai riwayat lengkap, dan keputusan klinis
        /// dapat diambil di atasnya.
        /// </summary>
        [Fact]
        public async Task PasienHasilPenggabungan_DitolakDisertaiNomorPengganti()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var lama = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);
            var baru = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.InConsultation);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            var pasienLama = context.Set<MstPatient>().Single(x => x.Id == lama.PatientId);
            var pasienBaru = context.Set<MstPatient>().AsNoTracking()
                .Single(x => x.Id == baru.PatientId);
            pasienLama.MergedToPatientId = baru.PatientId;
            context.SaveChanges();

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(lama.PatientId, dokter.Id), Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
            Assert.Contains("sudah digabungkan", hasil.ErrorMessage);
            Assert.Contains(pasienBaru.MedicalRecordNumber, hasil.ErrorMessage);
        }

        // =====================================================================
        // AT-RM-25 — kegagalan tidak melonggarkan kewenangan
        // =====================================================================

        /// <summary>
        /// `AT-RM-25`: bila penilaian kunjungan gagal, akses diperlakukan sebagai beralasan,
        /// bukan sebagai rawatan.
        ///
        /// Kegagalan teknis tidak boleh berubah menjadi pelonggaran kewenangan. Dibuktikan
        /// dengan membuang konteks basis datanya sehingga seluruh pembacaan gagal.
        /// </summary>
        [Fact]
        public async Task PenilaianKunjunganGagal_DiperlakukanSebagaiAksesBeralasan()
        {
            using var database = TestDatabase.Create();

            Guid patientId;
            using (var penyiap = database.CreateContext())
            {
                var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                    penyiap, EncounterStatus.InConsultation);
                patientId = konteks.PatientId;
            }

            var context = database.CreateContext();
            var service = Service(context);

            // Konteks dibuang, sehingga seluruh pembacaan basis data akan gagal.
            await context.DisposeAsync();

            var punyaKunjungan = await service.PunyaKunjunganAktifAsync(patientId);

            // Kegagalan tidak berubah menjadi "punya kunjungan aktif".
            Assert.False(punyaKunjungan);
        }

        // =====================================================================
        // Pasien tidak ditemukan
        // =====================================================================

        [Fact]
        public async Task PasienYangTidakAda_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(Guid.NewGuid(), dokter.Id), Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status404NotFound, hasil.StatusCode);
            Assert.Equal(0, context.Set<MrcAccessLog>().Count());
        }

        /// <summary>
        /// Keperluan yang tidak menuntut tinjauan tidak menandai jejaknya untuk ditinjau.
        ///
        /// Diperlukan supaya antrean tinjauan tidak penuh oleh akses yang memang wajar.
        /// </summary>
        [Fact]
        public async Task KeperluanYangTidakMenuntutTinjauan_TidakMenandaiJejaknya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var rutin = BuatKeperluan(context, "Penanganan gawat darurat", perluDitinjau: false);

            var hasil = await Service(context).EvaluateAndRecordAsync(
                Permintaan(konteks.PatientId, dokter.Id, purposeId: rutin.Id), Sekarang);

            Assert.True(hasil.IsAllowed);
            Assert.False(hasil.IsFlaggedForReview);
        }
    }
}
