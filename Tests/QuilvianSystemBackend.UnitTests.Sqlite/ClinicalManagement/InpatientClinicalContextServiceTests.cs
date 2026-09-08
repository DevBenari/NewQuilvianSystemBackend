using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-039</c> — satu tempat menjawab "dokumen ini milik
    /// perawatan yang mana", beserta kewenangan dokternya.
    /// </summary>
    /// <remarks>
    /// Ketujuh acceptance criteria task diuji satu per satu, termasuk yang paling mudah
    /// terlupakan: <b>nol baris antrean dibuat</b> pada seluruh jalur.
    /// </remarks>
    public class InpatientClinicalContextServiceTests
    {
        private static InpatientClinicalContextService Service(ApplicationDbContext c) => new(c);

        private static Task<int> JumlahAntreanAsync(ApplicationDbContext c) =>
            c.Set<TrxQueue>().CountAsync();

        // =====================================================================
        // AC 1 - konteks terbentuk untuk perawatan yang berjalan
        // =====================================================================

        /// <summary>`AC 1` — perawatan berjalan mengembalikan pasien, kunjungan, dan kewenangan.</summary>
        [Fact]
        public async Task PerawatanBerjalan_MengembalikanKonteksLengkap()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await Service(context).ResolveAsync(
                k.EncounterId,
                expectedPatientId: k.PatientId,
                doctorId: k.DoctorMasterId);

            Assert.True(hasil.IsResolved);
            Assert.Equal(InpatientClinicalContextOutcome.Resolved, hasil.Outcome);
            Assert.NotNull(hasil.Context);
            Assert.Equal(k.EpisodeId, hasil.Context!.EpisodeId);
            Assert.Equal(k.EncounterId, hasil.Context.EncounterId);
            Assert.Equal(k.PatientId, hasil.Context.PatientId);
            Assert.Equal(InpEpisodeStatus.Admitted, hasil.Context.EpisodeStatus);
            Assert.True(hasil.Context.IsEpisodeOpen);
            Assert.Equal(k.DoctorMasterId, hasil.Context.AttendingDoctorId);
            Assert.True(hasil.Context.IsDoctorAuthorized);
        }

        // =====================================================================
        // AC 2 s.d. 4 - penolakan menurut keadaan perawatan
        // =====================================================================

        /// <summary>`AC 2` — kunjungan tanpa perawatan rawat inap ditolak `422`.</summary>
        [Fact]
        public async Task KunjunganTanpaPerawatan_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var hasil = await Service(context).ResolveAsync(konteks.EncounterId);

            Assert.False(hasil.IsResolved);
            Assert.Equal(InpatientClinicalContextOutcome.NoInpatientEpisode, hasil.Outcome);
            Assert.Equal(422, hasil.StatusCode);
        }

        /// <summary>`AC 3` — perawatan `Draft` ditolak `422`.</summary>
        [Fact]
        public async Task PerawatanDraft_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Draft);

            var hasil = await Service(context).ResolveAsync(k.EncounterId);

            Assert.False(hasil.IsResolved);
            Assert.Equal(InpatientClinicalContextOutcome.EpisodeNotAdmitted, hasil.Outcome);
            Assert.Equal(422, hasil.StatusCode);
        }

        /// <summary>`AC 4` — perawatan `Closed` menolak dokumen baru, tetapi menerima koreksi.</summary>
        [Theory]
        [InlineData(InpEpisodeStatus.Closed)]
        [InlineData(InpEpisodeStatus.Cancelled)]
        public async Task PerawatanTertutup_MenolakDokumenBaru_TetapiMenerimaKoreksi(
            InpEpisodeStatus status)
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, status);

            var dokumenBaru = await Service(context).ResolveAsync(
                k.EncounterId, forNewDocument: true);

            Assert.False(dokumenBaru.IsResolved);
            Assert.Equal(InpatientClinicalContextOutcome.EpisodeClosed, dokumenBaru.Outcome);
            Assert.Equal(422, dokumenBaru.StatusCode);

            // Koreksi atas dokumen lama tetap boleh; perawatan tertutup tidak menutup jalur
            // pembetulan.
            var koreksi = await Service(context).ResolveAsync(
                k.EncounterId, forNewDocument: false);

            Assert.True(koreksi.IsResolved);
            Assert.False(koreksi.Context!.IsEpisodeOpen);
        }

        // =====================================================================
        // AC 5 dan 6 - penjaga salah pasien dan kewenangan dokter
        // =====================================================================

        /// <summary>`AC 5` — pasien dokumen yang tidak cocok ditolak `400`.</summary>
        [Fact]
        public async Task PasienTidakCocok_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await Service(context).ResolveAsync(
                k.EncounterId,
                expectedPatientId: Guid.NewGuid());

            Assert.False(hasil.IsResolved);
            Assert.Equal(InpatientClinicalContextOutcome.PatientMismatch, hasil.Outcome);
            Assert.Equal(400, hasil.StatusCode);
        }

        /// <summary>`AC 6` — dokter tanpa penugasan berlaku ditolak `403`, dua dokter berbeda.</summary>
        [Fact]
        public async Task DokterTanpaKewenangan_Ditolak403()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var dokterLain = RawatInapTestData.BuatDokterMaster(context);

            var berwenang = await Service(context).ResolveAsync(
                k.EncounterId, doctorId: k.DoctorMasterId);
            var tidakBerwenang = await Service(context).ResolveAsync(
                k.EncounterId, doctorId: dokterLain.Id);

            Assert.True(berwenang.IsResolved);

            Assert.False(tidakBerwenang.IsResolved);
            Assert.Equal(InpatientClinicalContextOutcome.DoctorNotAuthorized, tidakBerwenang.Outcome);
            Assert.Equal(403, tidakBerwenang.StatusCode);
        }

        /// <summary>
        /// Penugasan bersifat berperiode: dokter yang penugasannya sudah berakhir ditolak,
        /// walaupun barisnya masih ada.
        /// </summary>
        [Fact]
        public async Task PenugasanYangSudahBerakhir_TidakLagiBerwenang()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var penugasan = await context.Set<InpDoctorAssignment>()
                .FirstAsync(x => x.EpisodeId == k.EpisodeId);
            penugasan.EndDateTime = DateTime.UtcNow.AddDays(-1);
            await context.SaveChangesAsync();

            var hasil = await Service(context).ResolveAsync(
                k.EncounterId, doctorId: k.DoctorMasterId);

            Assert.False(hasil.IsResolved);
            Assert.Equal(403, hasil.StatusCode);

            // Namun pada saat penugasannya masih berlaku, dokter yang sama tetap berwenang.
            var kemarinDulu = await Service(context).ResolveAsync(
                k.EncounterId,
                doctorId: k.DoctorMasterId,
                atUtc: DateTime.UtcNow.AddDays(-2).AddHours(1));

            Assert.True(kemarinDulu.IsResolved);
        }

        /// <summary>Penanda perawatan yang tidak cocok ditolak `400` — `VAL-DOK-26`.</summary>
        [Fact]
        public async Task PenandaPerawatanTidakCocok_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await Service(context).ResolveAsync(
                k.EncounterId, expectedEpisodeId: Guid.NewGuid());

            Assert.False(hasil.IsResolved);
            Assert.Equal(InpatientClinicalContextOutcome.EpisodeMismatch, hasil.Outcome);
            Assert.Equal(400, hasil.StatusCode);
        }

        // =====================================================================
        // AC 7 - nol baris antrean pada seluruh jalur
        // =====================================================================

        /// <summary>
        /// `AC 7` — tidak satu pun jalur service membuat baris antrean.
        /// </summary>
        /// <remarks>
        /// Jalan pintas "membuatkan antrean semu supaya jalur lama terpakai" akan terlihat di
        /// sini sebagai selisih hitungan, bukan sebagai perilaku yang lolos diam-diam.
        /// </remarks>
        [Fact]
        public async Task SeluruhJalur_TidakMembuatBarisAntrean()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var berjalan = RawatInapTestData.SiapkanPerawatan(context);
            var draft = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Draft);
            var tertutup = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Closed);
            var tanpaPerawatan = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var sebelum = await JumlahAntreanAsync(context);

            var service = Service(context);

            await service.ResolveAsync(berjalan.EncounterId, doctorId: berjalan.DoctorMasterId);
            await service.ResolveAsync(berjalan.EncounterId, expectedPatientId: Guid.NewGuid());
            await service.ResolveAsync(draft.EncounterId);
            await service.ResolveAsync(tertutup.EncounterId);
            await service.ResolveAsync(tanpaPerawatan.EncounterId);
            await service.ResolveAsync(Guid.NewGuid());
            await service.FindOpenEpisodeIdAsync(berjalan.EncounterId);

            var sesudah = await JumlahAntreanAsync(context);

            Assert.Equal(0, sebelum);
            Assert.Equal(sebelum, sesudah);
        }

        /// <summary>Kunjungan yang tidak ada ditolak `404`, bukan kegagalan sistem.</summary>
        [Fact]
        public async Task KunjunganTidakAda_Ditolak404()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var hasil = await Service(context).ResolveAsync(Guid.NewGuid());

            Assert.False(hasil.IsResolved);
            Assert.Equal(InpatientClinicalContextOutcome.EncounterNotFound, hasil.Outcome);
            Assert.Equal(404, hasil.StatusCode);
        }

        /// <summary>
        /// <c>FindOpenEpisodeIdAsync</c> hanya mengakui perawatan yang benar-benar berjalan.
        /// </summary>
        [Theory]
        [InlineData(InpEpisodeStatus.Admitted, true)]
        [InlineData(InpEpisodeStatus.DischargePending, true)]
        [InlineData(InpEpisodeStatus.Draft, false)]
        [InlineData(InpEpisodeStatus.Closed, false)]
        [InlineData(InpEpisodeStatus.Cancelled, false)]
        public async Task PerawatanBerjalan_HanyaAdmittedDanDischargePending(
            InpEpisodeStatus status, bool diharapkanBerjalan)
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, status);

            var episodeId = await Service(context).FindOpenEpisodeIdAsync(k.EncounterId);

            Assert.Equal(diharapkanBerjalan, episodeId.HasValue);
        }
    }
}
