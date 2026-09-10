using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.HealthServices
{
    /// <summary>
    /// <c>BE-RWI-069</c> kriteria 6 — meminta alasan penolakan tidak menambah satu pun query
    /// ke basis data.
    /// </summary>
    /// <remarks>
    /// <b>Kenapa uji ini di sini, bukan di project InMemory.</b> Provider InMemory tidak
    /// menjalankan satu pun perintah SQL, sehingga menghitung query di sana tidak
    /// membuktikan apa pun. SQLite adalah provider relasional, sehingga
    /// <c>RelationalEventId.CommandExecuted</c> benar-benar menyala sekali untuk setiap
    /// perintah yang dijalankan, dan jumlahnya dapat dibandingkan.
    ///
    /// <para>
    /// <b>Godaan yang ditangkap uji ini.</b> Cara termudah membuat daftar penolakan terlihat
    /// rapi adalah menambahkan satu pembacaan tersendiri di dalam pencarian — misalnya
    /// mengambil ulang kamar beserta penghuninya untuk menyusun kalimat alasan. Itu akan
    /// membuat dua sumber kebenaran, persis kesalahan yang dulu ditemukan
    /// <c>BE-RWI-034</c>. Bila itu terjadi, hitungan di bawah tidak lagi sama.
    /// </para>
    /// </remarks>
    public class InpatientIneligibleBedQueryCountTests
    {
        [Fact]
        public async Task Kriteria6_MemintaAlasanPenolakanTidakMenambahQuery()
        {
            using var database = TestDatabase.Create();

            Guid episodeId;

            using (var penyiapan = database.CreateContext())
            {
                episodeId = SiapkanPemilihanBed(penyiapan);
            }

            // Pemanasan. Pembacaan pertama pada sebuah koneksi menjalankan perintah yang tidak
            // berulang pada pembacaan berikutnya, dan menghitungnya akan membuat kedua angka
            // di bawah berbeda karena alasan yang tidak ada hubungannya dengan perubahan ini.
            using (var pemanasan = database.CreateContext())
            {
                await BuatService(pemanasan).SearchAvailableBedsAsync(
                    new AvailableBedQuery { EpisodeId = episodeId });
            }

            var tanpaAlasan = 0;

            using (var context = database.CreateContext(_ => tanpaAlasan++))
            {
                var hasil = await BuatService(context).SearchAvailableBedsAsync(
                    new AvailableBedQuery { EpisodeId = episodeId });

                Assert.Empty(hasil.Ineligible);
                Assert.Single(hasil.Items);
            }

            var denganAlasan = 0;

            using (var context = database.CreateContext(_ => denganAlasan++))
            {
                var hasil = await BuatService(context).SearchAvailableBedsAsync(
                    new AvailableBedQuery
                    {
                        EpisodeId = episodeId,
                        IncludeIneligible = true
                    });

                // Tempat tidur isolasi ditolak aturan 8, dan alasannya benar-benar terkirim.
                var ditolak = Assert.Single(hasil.Ineligible);
                Assert.Contains(
                    ditolak.Failures,
                    f => f.RuleNumber == 8 && f.Code == "ISOLATION_BED_RESERVED");

                Assert.Single(hasil.Items);
            }

            Assert.True(
                tanpaAlasan > 0,
                "Uji ini tidak berarti apa-apa bila tidak ada satu pun perintah yang terhitung.");

            Assert.Equal(tanpaAlasan, denganAlasan);
        }

        private static InpBedOccupancyService BuatService(ApplicationDbContext context)
        {
            var settingService = new InpSettingService(
                context,
                NullLogger<InpSettingService>.Instance);

            var episodeService = new InpEpisodeService(
                context,
                settingService,
                new InpEpisodeNumberService(settingService));

            return new InpBedOccupancyService(context, settingService, episodeService);
        }

        /// <summary>
        /// Menyusun satu episode laki-laki yang tidak membutuhkan isolasi, satu tempat tidur
        /// biasa yang layak, dan satu tempat tidur isolasi yang pasti ditolak aturan 8.
        /// </summary>
        private static Guid SiapkanPemilihanBed(ApplicationDbContext context)
        {
            var konteks = RawatInapTestData.SiapkanPerawatan(
                context,
                episodeStatus: InpEpisodeStatus.Draft);

            // Jenis kelamin wajib tercatat, jika tidak aturan 5 ikut menolak dan uji ini
            // membuktikan penolakan yang salah.
            var pasien = context.Set<MstPatient>().Single(x => x.Id == konteks.PatientId);
            pasien.Gender = Gender.Male;

            var patientClassId = context.Set<MstPatientClass>()
                .OrderBy(x => x.PatientClassCode)
                .Select(x => x.Id)
                .First();

            var pembeda = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

            var kamar = new MstRoom
            {
                ServiceUnitId = konteks.ServiceUnitId,
                PatientClassId = patientClassId,
                RoomCode = $"RM-{pembeda}",
                RoomName = "Melati 3",
                Capacity = 4,
                IsActive = true
            };
            context.Set<MstRoom>().Add(kamar);
            context.SaveChanges();

            context.Set<MstBed>().AddRange(
                new MstBed
                {
                    RoomId = kamar.Id,
                    BedCode = $"BD-{pembeda}-1",
                    BedName = "3A",
                    BedStatus = BedStatus.Available,
                    IsForMale = true,
                    IsForFemale = true,
                    IsReservable = true,
                    IsActive = true
                },
                new MstBed
                {
                    RoomId = kamar.Id,
                    BedCode = $"BD-{pembeda}-2",
                    BedName = "IS-1",
                    BedStatus = BedStatus.Available,
                    IsForMale = true,
                    IsForFemale = true,
                    IsIsolationBed = true,
                    IsReservable = true,
                    IsActive = true
                });

            context.SaveChanges();

            return konteks.EpisodeId;
        }
    }
}
