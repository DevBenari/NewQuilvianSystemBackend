using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-19` — penanda kunjungan aktif pada daftar pasien.
    /// </summary>
    /// <remarks>
    /// Inti task ini bukan "ada field baru", melainkan <b>satu aturan, satu tempat</b>. Penanda
    /// pada daftar pasien harus selalu sepadan dengan keputusan yang nanti diambil server saat
    /// berkas benar-benar dibuka. Bila keduanya berbeda tipis, layar akan menjanjikan sesuatu
    /// yang ditolak server — atau lebih buruk, melewatkan permintaan keperluan akses yang
    /// seharusnya diminta.
    ///
    /// Karena itu seluruh uji di bawah membandingkan hasil penilaian sekelompok pasien terhadap
    /// hasil penilaian satu per satu, memakai method yang sama yang dipakai penilaian kewenangan.
    /// </remarks>
    public class ActiveEncounterFlagTests
    {
        private static MedicalRecordAccessAuditService Service(ApplicationDbContext c) => new(c);

        [Theory]
        [InlineData(EncounterStatus.Registered, true)]
        [InlineData(EncounterStatus.WaitingForNurse, true)]
        [InlineData(EncounterStatus.WaitingForDoctor, true)]
        [InlineData(EncounterStatus.Completed, false)]
        [InlineData(EncounterStatus.Cancelled, false)]
        [InlineData(EncounterStatus.NoShow, false)]
        public async Task PenilaianSekelompok_SamaDenganPenilaianSatuPerSatu(
            EncounterStatus status,
            bool diharapkanAktif)
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context, status);
            var service = Service(context);

            var satuPerSatu = await service.PunyaKunjunganAktifAsync(konteks.PatientId);
            var sekelompok = await service.PasienDenganKunjunganAktifAsync([konteks.PatientId]);

            Assert.Equal(diharapkanAktif, satuPerSatu);
            Assert.NotNull(sekelompok);
            Assert.Equal(satuPerSatu, sekelompok!.Contains(konteks.PatientId));
        }

        [Fact]
        public async Task KunjunganYangSudahDitutupWaktunya_TidakDianggapBerjalan()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            // Statusnya masih Registered, tetapi kunjungannya sudah punya waktu selesai.
            // Aturannya menuntut KEDUANYA, bukan salah satu.
            var kunjungan = await context.Set<TrxPatientEncounter>()
                .SingleAsync(x => x.Id == konteks.EncounterId);

            kunjungan.CompletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            var service = Service(context);

            Assert.False(await service.PunyaKunjunganAktifAsync(konteks.PatientId));

            var sekelompok = await service.PasienDenganKunjunganAktifAsync([konteks.PatientId]);

            Assert.NotNull(sekelompok);
            Assert.DoesNotContain(konteks.PatientId, sekelompok!);
        }

        [Fact]
        public async Task KunjunganYangDibatalkanPenandanya_TidakDianggapBerjalan()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var kunjungan = await context.Set<TrxPatientEncounter>()
                .SingleAsync(x => x.Id == konteks.EncounterId);

            kunjungan.IsCancel = true;
            await context.SaveChangesAsync();

            var service = Service(context);

            Assert.False(await service.PunyaKunjunganAktifAsync(konteks.PatientId));
            Assert.DoesNotContain(
                konteks.PatientId,
                (await service.PasienDenganKunjunganAktifAsync([konteks.PatientId]))!);
        }

        [Fact]
        public async Task SatuQueryMenilaiSeluruhHalaman_DanMemisahkanPasienDenganBenar()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var dirawat = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.WaitingForDoctor);
            var sudahPulang = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var tidakDatang = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.NoShow);

            var service = Service(context);

            var hasil = await service.PasienDenganKunjunganAktifAsync(
            [
                dirawat.PatientId,
                sudahPulang.PatientId,
                tidakDatang.PatientId
            ]);

            Assert.NotNull(hasil);
            Assert.Contains(dirawat.PatientId, hasil!);
            Assert.DoesNotContain(sudahPulang.PatientId, hasil);
            Assert.DoesNotContain(tidakDatang.PatientId, hasil);

            // Tiap pasien menghasilkan jawaban yang sama bila dinilai sendiri-sendiri.
            foreach (var patientId in new[]
                     {
                         dirawat.PatientId, sudahPulang.PatientId, tidakDatang.PatientId
                     })
            {
                Assert.Equal(
                    await service.PunyaKunjunganAktifAsync(patientId),
                    hasil.Contains(patientId));
            }
        }

        [Fact]
        public async Task DaftarKosongAtauIdKosong_MenghasilkanHimpunanKosongBukanNull()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var service = Service(context);

            var kosong = await service.PasienDenganKunjunganAktifAsync([]);
            var idKosong = await service.PasienDenganKunjunganAktifAsync([Guid.Empty]);

            // Himpunan kosong berarti "tidak ada yang dirawat" — bukan "tidak diketahui".
            // Pembedaan itu menentukan apakah layar menampilkan "Tidak ada" atau
            // "Tidak diketahui".
            Assert.NotNull(kosong);
            Assert.Empty(kosong!);
            Assert.NotNull(idKosong);
            Assert.Empty(idKosong!);
        }

        [Fact]
        public async Task PasienTanpaKunjunganSamaSekali_TidakDianggapBerjalan()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            context.Set<TrxPatientEncounter>().Remove(
                await context.Set<TrxPatientEncounter>()
                    .SingleAsync(x => x.Id == konteks.EncounterId));

            await context.SaveChangesAsync();

            var service = Service(context);
            var hasil = await service.PasienDenganKunjunganAktifAsync([konteks.PatientId]);

            Assert.False(await service.PunyaKunjunganAktifAsync(konteks.PatientId));
            Assert.NotNull(hasil);
            Assert.Empty(hasil!);
        }
    }
}
